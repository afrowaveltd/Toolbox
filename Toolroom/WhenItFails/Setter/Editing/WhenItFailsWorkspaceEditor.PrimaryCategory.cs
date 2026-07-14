using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides primary-category editing for error definitions.
/// </summary>
internal static class WhenItFailsWorkspaceEditorPrimaryCategoryExtensions
{
    /// <summary>
    /// Sets the primary category of one existing error definition.
    /// </summary>
    public static async Task<Response<ErrorDefinition>> SetPrimaryCategoryAsync(
        this WhenItFailsWorkspaceEditor editor,
        string inputPath,
        string lookupValue,
        string categoryName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(lookupValue))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorLookupIsEmpty",
                message: "Error id, code, or name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "PrimaryCategoryNameIsEmpty",
                message: "Primary category name cannot be empty.");
        }

        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);

        Response<ErrorCatalogDocument> errorLoadResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                options.ErrorCatalogFilePath,
                cancellationToken);

        if (!errorLoadResponse.IsSuccess || errorLoadResponse.Data is null)
        {
            return Response<ErrorDefinition>.Fail(
                code: "ErrorCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(errorLoadResponse.Message)
                    ? $"Error catalog could not be loaded: {options.ErrorCatalogFilePath}"
                    : errorLoadResponse.Message);
        }

        ErrorCatalogDocument errorCatalog =
            new ErrorCatalogDocumentNormalizer().Normalize(errorLoadResponse.Data);

        ErrorDefinition? errorDefinition = FindErrorDefinition(
            errorCatalog,
            lookupValue.Trim());

        if (errorDefinition is null)
        {
            return Response<ErrorDefinition>.NotFound(
                code: "ErrorDefinitionNotFound",
                message: $"No error definition was found for '{lookupValue}'. Search by Id, Code or Name.");
        }

        Response<ErrorCategoryCatalogDocument> categoryLoadResponse =
            await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(
                options.CategoryCatalogFilePath,
                cancellationToken);

        if (!categoryLoadResponse.IsSuccess || categoryLoadResponse.Data is null)
        {
            return Response<ErrorDefinition>.Fail(
                code: "CategoryCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(categoryLoadResponse.Message)
                    ? $"Category catalog could not be loaded: {options.CategoryCatalogFilePath}"
                    : categoryLoadResponse.Message);
        }

        ErrorCategoryCatalogDocument categoryCatalog =
            new ErrorCategoryCatalogDocumentNormalizer().Normalize(categoryLoadResponse.Data);

        string normalizedCategoryName = TextKeyNormalizer.NormalizeKey(categoryName);
        ErrorCategoryDefinition? categoryDefinition = categoryCatalog.Categories.FirstOrDefault(category =>
            string.Equals(category.Name, normalizedCategoryName, StringComparison.OrdinalIgnoreCase)
            || category.Aliases.Any(alias => string.Equals(
                alias,
                normalizedCategoryName,
                StringComparison.OrdinalIgnoreCase)));

        if (categoryDefinition is null)
        {
            return Response<ErrorDefinition>.NotFound(
                code: "CategoryNotFound",
                message: $"Category '{normalizedCategoryName}' was not found.");
        }

        string oldPrimaryCategory = errorDefinition.PrimaryCategory;

        if (string.Equals(
            oldPrimaryCategory,
            categoryDefinition.Name,
            StringComparison.OrdinalIgnoreCase))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "PrimaryCategoryAlreadySet",
                message: $"Error '{errorDefinition.Id}' already has primary category '{categoryDefinition.Name}'.");
        }

        errorDefinition.PrimaryCategory = categoryDefinition.Name;

        ErrorCatalogValidationResult validationResult =
            new ErrorCatalogValidator().Validate(errorCatalog);

        if (!validationResult.IsValid)
        {
            errorDefinition.PrimaryCategory = oldPrimaryCategory;

            return Response<ErrorDefinition>.Invalid(
                code: "EditedErrorCatalogIsInvalid",
                message: "The edited error catalog is invalid and was not saved.");
        }

        Response saveResponse = await new JsonCatalogDocumentWriter().SaveToFileAsync(
            errorCatalog,
            options.ErrorCatalogFilePath,
            cancellationToken);

        if (!saveResponse.IsSuccess)
        {
            errorDefinition.PrimaryCategory = oldPrimaryCategory;

            string code = saveResponse.Issues.Count > 0
                ? saveResponse.Issues[0].Code
                : "ErrorCatalogSaveFailed";

            return Response<ErrorDefinition>.Fail(
                code: code,
                message: string.IsNullOrWhiteSpace(saveResponse.Message)
                    ? "Error catalog could not be saved."
                    : saveResponse.Message);
        }

        return Response<ErrorDefinition>.Ok(
            errorDefinition,
            $"Primary category changed from '{oldPrimaryCategory}' to '{categoryDefinition.Name}' for error '{errorDefinition.Id}'.");
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
}
