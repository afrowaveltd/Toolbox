using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides additional-category editing for error definitions.
/// </summary>
internal static class WhenItFailsWorkspaceEditorCategoryExtensions
{
    /// <summary>
    /// Adds one existing workspace category to an error definition.
    /// </summary>
    public static async Task<Response<ErrorDefinition>> ErrorAddCategoryAsync(
        this WhenItFailsWorkspaceEditor editor,
        string inputPath,
        string lookupValue,
        string categoryName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ErrorCategoryEditContext> contextResponse = await LoadContextAsync(
            inputPath,
            lookupValue,
            categoryName,
            cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure(contextResponse);
        }

        ErrorCategoryEditContext context = contextResponse.Data;

        if (context.ErrorDefinition.Categories.Any(category => string.Equals(
            category,
            context.CategoryDefinition.Name,
            StringComparison.OrdinalIgnoreCase)))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorCategoryAlreadyIncluded",
                message: $"Error '{context.ErrorDefinition.Id}' already includes category '{context.CategoryDefinition.Name}'.");
        }

        context.ErrorDefinition.Categories.Add(context.CategoryDefinition.Name);

        Response<ErrorDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ErrorDefinition.Categories.Remove(context.CategoryDefinition.Name),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorDefinition>.Ok(
            context.ErrorDefinition,
            $"Category '{context.CategoryDefinition.Name}' was added to error '{context.ErrorDefinition.Id}'.");
    }

    /// <summary>
    /// Removes one additional category from an error definition.
    /// </summary>
    public static async Task<Response<ErrorDefinition>> ErrorRemoveCategoryAsync(
        this WhenItFailsWorkspaceEditor editor,
        string inputPath,
        string lookupValue,
        string categoryName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ErrorCategoryEditContext> contextResponse = await LoadContextAsync(
            inputPath,
            lookupValue,
            categoryName,
            cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure(contextResponse);
        }

        ErrorCategoryEditContext context = contextResponse.Data;
        int categoryIndex = context.ErrorDefinition.Categories.FindIndex(category => string.Equals(
            category,
            context.CategoryDefinition.Name,
            StringComparison.OrdinalIgnoreCase));

        if (categoryIndex < 0)
        {
            return Response<ErrorDefinition>.NotFound(
                code: "ErrorCategoryNotIncluded",
                message: $"Error '{context.ErrorDefinition.Id}' does not include category '{context.CategoryDefinition.Name}'.");
        }

        string removedCategory = context.ErrorDefinition.Categories[categoryIndex];
        context.ErrorDefinition.Categories.RemoveAt(categoryIndex);

        Response<ErrorDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ErrorDefinition.Categories.Insert(categoryIndex, removedCategory),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorDefinition>.Ok(
            context.ErrorDefinition,
            $"Category '{context.CategoryDefinition.Name}' was removed from error '{context.ErrorDefinition.Id}'.");
    }

    private static async Task<Response<ErrorCategoryEditContext>> LoadContextAsync(
        string inputPath,
        string lookupValue,
        string categoryName,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(lookupValue))
        {
            return Response<ErrorCategoryEditContext>.Invalid(
                code: "ErrorLookupIsEmpty",
                message: "Error id, code, or name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return Response<ErrorCategoryEditContext>.Invalid(
                code: "CategoryNameIsEmpty",
                message: "Category name cannot be empty.");
        }

        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);
        Response<ErrorCatalogDocument> errorLoadResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                options.ErrorCatalogFilePath,
                cancellationToken);

        if (!errorLoadResponse.IsSuccess || errorLoadResponse.Data is null)
        {
            return Response<ErrorCategoryEditContext>.Fail(
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
            return Response<ErrorCategoryEditContext>.NotFound(
                code: "ErrorDefinitionNotFound",
                message: $"No error definition was found for '{lookupValue}'. Search by Id, Code or Name.");
        }

        Response<ErrorCategoryCatalogDocument> categoryLoadResponse =
            await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(
                options.CategoryCatalogFilePath,
                cancellationToken);

        if (!categoryLoadResponse.IsSuccess || categoryLoadResponse.Data is null)
        {
            return Response<ErrorCategoryEditContext>.Fail(
                code: "CategoryCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(categoryLoadResponse.Message)
                    ? $"Category catalog could not be loaded: {options.CategoryCatalogFilePath}"
                    : categoryLoadResponse.Message);
        }

        ErrorCategoryCatalogDocument categoryCatalog =
            new ErrorCategoryCatalogDocumentNormalizer().Normalize(categoryLoadResponse.Data);
        string normalizedCategoryName = TextKeyNormalizer.NormalizeKey(categoryName);
        ErrorCategoryDefinition? categoryDefinition =
            categoryCatalog.Categories.FirstOrDefault(category =>
                string.Equals(
                    category.Name,
                    normalizedCategoryName,
                    StringComparison.OrdinalIgnoreCase)
                || category.Aliases.Any(alias => string.Equals(
                    alias,
                    normalizedCategoryName,
                    StringComparison.OrdinalIgnoreCase)));

        if (categoryDefinition is null)
        {
            return Response<ErrorCategoryEditContext>.NotFound(
                code: "CategoryNotFound",
                message: $"Category '{normalizedCategoryName}' was not found.");
        }

        return Response<ErrorCategoryEditContext>.Ok(
            new ErrorCategoryEditContext(
                options.ErrorCatalogFilePath,
                errorCatalog,
                errorDefinition,
                categoryDefinition));
    }

    private static async Task<Response<ErrorDefinition>?> ValidateAndSaveAsync(
        ErrorCategoryEditContext context,
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
        Response<ErrorCategoryEditContext> response)
    {
        string code = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ErrorCategoryEditFailed";

        return response.Issues.Count > 0
            && response.Issues[0].Code.EndsWith("NotFound", StringComparison.Ordinal)
            ? Response<ErrorDefinition>.NotFound(code, response.Message)
            : Response<ErrorDefinition>.Fail(code, response.Message);
    }

    private sealed record ErrorCategoryEditContext(
        string ErrorCatalogFilePath,
        ErrorCatalogDocument ErrorCatalog,
        ErrorDefinition ErrorDefinition,
        ErrorCategoryDefinition CategoryDefinition);
}
