using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides metadata editing for error definitions.
/// </summary>
internal static class WhenItFailsWorkspaceEditorMetadataExtensions
{
    /// <summary>
    /// Adds or updates one metadata value on an existing error definition.
    /// </summary>
    public static async Task<Response<ErrorDefinition>> ErrorSetMetadataAsync(
        this WhenItFailsWorkspaceEditor editor,
        string inputPath,
        string lookupValue,
        string metadataKey,
        string metadataValue,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ErrorMetadataEditContext> contextResponse = await LoadContextAsync(
            inputPath,
            lookupValue,
            metadataKey,
            metadataValue,
            requireValue: true,
            cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure(contextResponse);
        }

        ErrorMetadataEditContext context = contextResponse.Data;
        bool metadataExisted = context.ErrorDefinition.Metadata.TryGet(
            context.MetadataKey,
            out string? oldValue);

        if (metadataExisted
            && string.Equals(oldValue, context.MetadataValue, StringComparison.Ordinal))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorMetadataAlreadySet",
                message: $"Error '{context.ErrorDefinition.Id}' already has metadata '{context.MetadataKey}' set to '{context.MetadataValue}'.");
        }

        context.ErrorDefinition.Metadata.Set(context.MetadataKey, context.MetadataValue!);

        Response<ErrorDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => RollbackMetadata(context, oldValue, metadataExisted),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        string action = metadataExisted ? "updated" : "added";
        return Response<ErrorDefinition>.Ok(
            context.ErrorDefinition,
            $"Metadata '{context.MetadataKey}' was {action} for error '{context.ErrorDefinition.Id}'.");
    }

    /// <summary>
    /// Removes one metadata value from an existing error definition.
    /// </summary>
    public static async Task<Response<ErrorDefinition>> ErrorRemoveMetadataAsync(
        this WhenItFailsWorkspaceEditor editor,
        string inputPath,
        string lookupValue,
        string metadataKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ErrorMetadataEditContext> contextResponse = await LoadContextAsync(
            inputPath,
            lookupValue,
            metadataKey,
            metadataValue: null,
            requireValue: false,
            cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure(contextResponse);
        }

        ErrorMetadataEditContext context = contextResponse.Data;
        if (!context.ErrorDefinition.Metadata.TryGet(context.MetadataKey, out string? removedValue))
        {
            return Response<ErrorDefinition>.NotFound(
                code: "ErrorMetadataNotFound",
                message: $"Error '{context.ErrorDefinition.Id}' does not contain metadata '{context.MetadataKey}'.");
        }

        context.ErrorDefinition.Metadata.Remove(context.MetadataKey);

        Response<ErrorDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ErrorDefinition.Metadata.Set(
                context.MetadataKey,
                removedValue ?? string.Empty),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorDefinition>.Ok(
            context.ErrorDefinition,
            $"Metadata '{context.MetadataKey}' was removed from error '{context.ErrorDefinition.Id}'.");
    }

    private static async Task<Response<ErrorMetadataEditContext>> LoadContextAsync(
        string inputPath,
        string lookupValue,
        string metadataKey,
        string? metadataValue,
        bool requireValue,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(lookupValue))
        {
            return Response<ErrorMetadataEditContext>.Invalid(
                code: "ErrorLookupIsEmpty",
                message: "Error id, code, or name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(metadataKey))
        {
            return Response<ErrorMetadataEditContext>.Invalid(
                code: "ErrorMetadataKeyIsEmpty",
                message: "Error metadata key cannot be empty.");
        }

        if (requireValue && string.IsNullOrWhiteSpace(metadataValue))
        {
            return Response<ErrorMetadataEditContext>.Invalid(
                code: "ErrorMetadataValueIsEmpty",
                message: "Error metadata value cannot be empty.");
        }

        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);
        Response<ErrorCatalogDocument> loadResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                options.ErrorCatalogFilePath,
                cancellationToken);

        if (!loadResponse.IsSuccess || loadResponse.Data is null)
        {
            return Response<ErrorMetadataEditContext>.Fail(
                code: "ErrorCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(loadResponse.Message)
                    ? $"Error catalog could not be loaded: {options.ErrorCatalogFilePath}"
                    : loadResponse.Message);
        }

        ErrorCatalogDocument catalog =
            new ErrorCatalogDocumentNormalizer().Normalize(loadResponse.Data);
        ErrorCatalogValidationResult validationResult =
            new ErrorCatalogValidator().Validate(catalog);

        if (!validationResult.IsValid)
        {
            return Response<ErrorMetadataEditContext>.Invalid(
                code: "ErrorCatalogIsInvalid",
                message: "The error catalog is invalid and cannot be edited safely.");
        }

        ErrorDefinition? errorDefinition = FindErrorDefinition(catalog, lookupValue.Trim());
        if (errorDefinition is null)
        {
            return Response<ErrorMetadataEditContext>.NotFound(
                code: "ErrorDefinitionNotFound",
                message: $"No error definition was found for '{lookupValue}'. Search by Id, Code or Name.");
        }

        return Response<ErrorMetadataEditContext>.Ok(
            new ErrorMetadataEditContext(
                options.ErrorCatalogFilePath,
                catalog,
                errorDefinition,
                TextKeyNormalizer.NormalizeKey(metadataKey),
                metadataValue is null
                    ? null
                    : TextKeyNormalizer.NormalizeDisplayName(metadataValue)));
    }

    private static async Task<Response<ErrorDefinition>?> ValidateAndSaveAsync(
        ErrorMetadataEditContext context,
        Action rollback,
        CancellationToken cancellationToken)
    {
        ErrorCatalogValidationResult validationResult =
            new ErrorCatalogValidator().Validate(context.ErrorCatalog);

        if (!validationResult.IsValid)
        {
            rollback();
            return Response<ErrorDefinition>.Invalid(
                code: "EditedErrorCatalogIsInvalid",
                message: "The edited error catalog is invalid and was not saved.");
        }

        Response saveResponse = await new JsonCatalogDocumentWriter().SaveToFileAsync(
            context.ErrorCatalog,
            context.ErrorCatalogFilePath,
            cancellationToken);

        if (saveResponse.IsSuccess)
        {
            return null;
        }

        rollback();
        return Response<ErrorDefinition>.Fail(
            code: saveResponse.Issues.Count > 0
                ? saveResponse.Issues[0].Code
                : "ErrorCatalogSaveFailed",
            message: string.IsNullOrWhiteSpace(saveResponse.Message)
                ? "Error catalog could not be saved."
                : saveResponse.Message);
    }

    private static void RollbackMetadata(
        ErrorMetadataEditContext context,
        string? oldValue,
        bool metadataExisted)
    {
        if (metadataExisted)
        {
            context.ErrorDefinition.Metadata.Set(context.MetadataKey, oldValue ?? string.Empty);
            return;
        }

        context.ErrorDefinition.Metadata.Remove(context.MetadataKey);
    }

    private static ErrorDefinition? FindErrorDefinition(
        ErrorCatalogDocument catalog,
        string lookupValue)
    {
        if (int.TryParse(lookupValue, out int numericCode))
        {
            ErrorDefinition? byCode = catalog.Errors.FirstOrDefault(error => error.Code == numericCode);
            if (byCode is not null)
            {
                return byCode;
            }
        }

        return catalog.Errors.FirstOrDefault(error =>
            string.Equals(error.Id, lookupValue, StringComparison.OrdinalIgnoreCase)
            || string.Equals(error.Name, lookupValue, StringComparison.OrdinalIgnoreCase));
    }

    private static Response<ErrorDefinition> CopyFailure(
        Response<ErrorMetadataEditContext> response)
    {
        string code = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ErrorMetadataEditFailed";

        return code.EndsWith("NotFound", StringComparison.Ordinal)
            ? Response<ErrorDefinition>.NotFound(code, response.Message)
            : Response<ErrorDefinition>.Fail(code, response.Message);
    }

    private sealed record ErrorMetadataEditContext(
        string ErrorCatalogFilePath,
        ErrorCatalogDocument ErrorCatalog,
        ErrorDefinition ErrorDefinition,
        string MetadataKey,
        string? MetadataValue);
}
