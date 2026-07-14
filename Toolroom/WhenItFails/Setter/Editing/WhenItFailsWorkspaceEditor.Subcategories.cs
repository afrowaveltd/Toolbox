using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides subcategory editing for error definitions.
/// </summary>
internal static class WhenItFailsWorkspaceEditorSubcategoryExtensions
{
    /// <summary>
    /// Adds one existing workspace subcategory to an error definition.
    /// </summary>
    public static async Task<Response<ErrorDefinition>> ErrorAddSubcategoryAsync(
        this WhenItFailsWorkspaceEditor editor,
        string inputPath,
        string lookupValue,
        string subcategoryName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ErrorSubcategoryEditContext> contextResponse = await LoadContextAsync(
            inputPath,
            lookupValue,
            subcategoryName,
            cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure(contextResponse);
        }

        ErrorSubcategoryEditContext context = contextResponse.Data;

        if (context.ErrorDefinition.Subcategories.Any(subcategory => string.Equals(
            subcategory,
            context.CanonicalSubcategoryName,
            StringComparison.OrdinalIgnoreCase)))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorSubcategoryAlreadyIncluded",
                message: $"Error '{context.ErrorDefinition.Id}' already includes subcategory '{context.CanonicalSubcategoryName}'.");
        }

        context.ErrorDefinition.Subcategories.Add(context.CanonicalSubcategoryName);

        Response<ErrorDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ErrorDefinition.Subcategories.Remove(context.CanonicalSubcategoryName),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorDefinition>.Ok(
            context.ErrorDefinition,
            $"Subcategory '{context.CanonicalSubcategoryName}' was added to error '{context.ErrorDefinition.Id}'.");
    }

    /// <summary>
    /// Removes one subcategory from an error definition.
    /// </summary>
    public static async Task<Response<ErrorDefinition>> ErrorRemoveSubcategoryAsync(
        this WhenItFailsWorkspaceEditor editor,
        string inputPath,
        string lookupValue,
        string subcategoryName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ErrorSubcategoryEditContext> contextResponse = await LoadContextAsync(
            inputPath,
            lookupValue,
            subcategoryName,
            cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure(contextResponse);
        }

        ErrorSubcategoryEditContext context = contextResponse.Data;
        int subcategoryIndex = context.ErrorDefinition.Subcategories.FindIndex(subcategory => string.Equals(
            subcategory,
            context.CanonicalSubcategoryName,
            StringComparison.OrdinalIgnoreCase));

        if (subcategoryIndex < 0)
        {
            return Response<ErrorDefinition>.NotFound(
                code: "ErrorSubcategoryNotIncluded",
                message: $"Error '{context.ErrorDefinition.Id}' does not include subcategory '{context.CanonicalSubcategoryName}'.");
        }

        string removedSubcategory = context.ErrorDefinition.Subcategories[subcategoryIndex];
        context.ErrorDefinition.Subcategories.RemoveAt(subcategoryIndex);

        Response<ErrorDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ErrorDefinition.Subcategories.Insert(subcategoryIndex, removedSubcategory),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorDefinition>.Ok(
            context.ErrorDefinition,
            $"Subcategory '{context.CanonicalSubcategoryName}' was removed from error '{context.ErrorDefinition.Id}'.");
    }

    private static async Task<Response<ErrorSubcategoryEditContext>> LoadContextAsync(
        string inputPath,
        string lookupValue,
        string subcategoryName,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(lookupValue))
        {
            return Response<ErrorSubcategoryEditContext>.Invalid(
                code: "ErrorLookupIsEmpty",
                message: "Error id, code, or name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(subcategoryName))
        {
            return Response<ErrorSubcategoryEditContext>.Invalid(
                code: "SubcategoryNameIsEmpty",
                message: "Subcategory name cannot be empty.");
        }

        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);
        Response<ErrorCatalogDocument> loadResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                options.ErrorCatalogFilePath,
                cancellationToken);

        if (!loadResponse.IsSuccess || loadResponse.Data is null)
        {
            return Response<ErrorSubcategoryEditContext>.Fail(
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
            return Response<ErrorSubcategoryEditContext>.NotFound(
                code: "ErrorDefinitionNotFound",
                message: $"No error definition was found for '{lookupValue}'. Search by Id, Code or Name.");
        }

        string normalizedSubcategoryName = TextKeyNormalizer.NormalizeKey(subcategoryName);
        string? canonicalSubcategoryName = catalog.Errors
            .SelectMany(error => error.Subcategories)
            .Select(TextKeyNormalizer.NormalizeKey)
            .FirstOrDefault(subcategory => string.Equals(
                subcategory,
                normalizedSubcategoryName,
                StringComparison.OrdinalIgnoreCase));

        if (canonicalSubcategoryName is null)
        {
            return Response<ErrorSubcategoryEditContext>.NotFound(
                code: "SubcategoryNotFound",
                message: $"Subcategory '{normalizedSubcategoryName}' was not found in the error catalog.");
        }

        return Response<ErrorSubcategoryEditContext>.Ok(
            new ErrorSubcategoryEditContext(
                options.ErrorCatalogFilePath,
                catalog,
                errorDefinition,
                canonicalSubcategoryName));
    }

    private static async Task<Response<ErrorDefinition>?> ValidateAndSaveAsync(
        ErrorSubcategoryEditContext context,
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
        Response<ErrorSubcategoryEditContext> response)
    {
        string code = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ErrorSubcategoryEditFailed";

        return response.Issues.Count > 0
            && response.Issues[0].Code.EndsWith("NotFound", StringComparison.Ordinal)
            ? Response<ErrorDefinition>.NotFound(code, response.Message)
            : Response<ErrorDefinition>.Fail(code, response.Message);
    }

    private sealed record ErrorSubcategoryEditContext(
        string ErrorCatalogFilePath,
        ErrorCatalogDocument ErrorCatalog,
        ErrorDefinition ErrorDefinition,
        string CanonicalSubcategoryName);
}
