using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides tag editing for error definitions.
/// </summary>
internal static class WhenItFailsWorkspaceEditorTagExtensions
{
    /// <summary>
    /// Adds one normalized tag to an existing error definition.
    /// </summary>
    public static async Task<Response<ErrorDefinition>> ErrorAddTagAsync(
        this WhenItFailsWorkspaceEditor editor,
        string inputPath,
        string lookupValue,
        string tagName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ErrorTagEditContext> contextResponse = await LoadContextAsync(
            inputPath,
            lookupValue,
            tagName,
            cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure(contextResponse);
        }

        ErrorTagEditContext context = contextResponse.Data;

        if (context.ErrorDefinition.Tags.Any(tag => string.Equals(
            tag,
            context.CanonicalTagName,
            StringComparison.OrdinalIgnoreCase)))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorTagAlreadyIncluded",
                message: $"Error '{context.ErrorDefinition.Id}' already includes tag '{context.CanonicalTagName}'.");
        }

        context.ErrorDefinition.Tags.Add(context.CanonicalTagName);

        Response<ErrorDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ErrorDefinition.Tags.Remove(context.CanonicalTagName),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorDefinition>.Ok(
            context.ErrorDefinition,
            $"Tag '{context.CanonicalTagName}' was added to error '{context.ErrorDefinition.Id}'.");
    }

    /// <summary>
    /// Removes one tag from an existing error definition.
    /// </summary>
    public static async Task<Response<ErrorDefinition>> ErrorRemoveTagAsync(
        this WhenItFailsWorkspaceEditor editor,
        string inputPath,
        string lookupValue,
        string tagName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ErrorTagEditContext> contextResponse = await LoadContextAsync(
            inputPath,
            lookupValue,
            tagName,
            cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure(contextResponse);
        }

        ErrorTagEditContext context = contextResponse.Data;
        int tagIndex = context.ErrorDefinition.Tags.FindIndex(tag => string.Equals(
            tag,
            context.CanonicalTagName,
            StringComparison.OrdinalIgnoreCase));

        if (tagIndex < 0)
        {
            return Response<ErrorDefinition>.NotFound(
                code: "ErrorTagNotIncluded",
                message: $"Error '{context.ErrorDefinition.Id}' does not include tag '{context.CanonicalTagName}'.");
        }

        string removedTag = context.ErrorDefinition.Tags[tagIndex];
        context.ErrorDefinition.Tags.RemoveAt(tagIndex);

        Response<ErrorDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ErrorDefinition.Tags.Insert(tagIndex, removedTag),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorDefinition>.Ok(
            context.ErrorDefinition,
            $"Tag '{context.CanonicalTagName}' was removed from error '{context.ErrorDefinition.Id}'.");
    }

    private static async Task<Response<ErrorTagEditContext>> LoadContextAsync(
        string inputPath,
        string lookupValue,
        string tagName,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(lookupValue))
        {
            return Response<ErrorTagEditContext>.Invalid(
                code: "ErrorLookupIsEmpty",
                message: "Error id, code, or name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(tagName))
        {
            return Response<ErrorTagEditContext>.Invalid(
                code: "TagNameIsEmpty",
                message: "Tag name cannot be empty.");
        }

        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);
        Response<ErrorCatalogDocument> loadResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                options.ErrorCatalogFilePath,
                cancellationToken);

        if (!loadResponse.IsSuccess || loadResponse.Data is null)
        {
            return Response<ErrorTagEditContext>.Fail(
                code: "ErrorCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(loadResponse.Message)
                    ? $"Error catalog could not be loaded: {options.ErrorCatalogFilePath}"
                    : loadResponse.Message);
        }

        ErrorCatalogDocument catalog =
            new ErrorCatalogDocumentNormalizer().Normalize(loadResponse.Data);
        ErrorDefinition? errorDefinition = FindErrorDefinition(catalog, lookupValue.Trim());

        if (errorDefinition is null)
        {
            return Response<ErrorTagEditContext>.NotFound(
                code: "ErrorDefinitionNotFound",
                message: $"No error definition was found for '{lookupValue}'. Search by Id, Code or Name.");
        }

        return Response<ErrorTagEditContext>.Ok(
            new ErrorTagEditContext(
                options.ErrorCatalogFilePath,
                catalog,
                errorDefinition,
                TextKeyNormalizer.NormalizeKey(tagName)));
    }

    private static async Task<Response<ErrorDefinition>?> ValidateAndSaveAsync(
        ErrorTagEditContext context,
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
        string code = saveResponse.Issues.Count > 0
            ? saveResponse.Issues[0].Code
            : "ErrorCatalogSaveFailed";

        return Response<ErrorDefinition>.Fail(
            code: code,
            message: string.IsNullOrWhiteSpace(saveResponse.Message)
                ? "Error catalog could not be saved."
                : saveResponse.Message);
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
        Response<ErrorTagEditContext> response)
    {
        string code = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ErrorTagEditFailed";

        return response.Issues.Count > 0
            && response.Issues[0].Code.EndsWith("NotFound", StringComparison.Ordinal)
            ? Response<ErrorDefinition>.NotFound(code, response.Message)
            : Response<ErrorDefinition>.Fail(code, response.Message);
    }

    private sealed record ErrorTagEditContext(
        string ErrorCatalogFilePath,
        ErrorCatalogDocument ErrorCatalog,
        ErrorDefinition ErrorDefinition,
        string CanonicalTagName);
}
