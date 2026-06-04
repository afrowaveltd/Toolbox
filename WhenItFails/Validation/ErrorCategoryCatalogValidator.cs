using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Validation;

/// <summary>
/// Default validator for error category catalog documents.
/// </summary>
public sealed class ErrorCategoryCatalogValidator : IErrorCategoryCatalogValidator
{
    /// <inheritdoc />
    public ErrorCatalogValidationResult Validate(ErrorCategoryCatalogDocument? document)
    {
        ErrorCatalogValidationResult result = new();

        if (document is null)
        {
            result.AddError(
                code: "CategoryCatalogDocumentIsNull",
                message: "Error category catalog document is null.",
                path: "$");

            return result;
        }

        ValidateDocumentHeader(document, result);
        ValidateCategories(document, result);

        return result;
    }

    private static void ValidateDocumentHeader(
        ErrorCategoryCatalogDocument document,
        ErrorCatalogValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(document.SchemaVersion))
        {
            result.AddError(
                code: "MissingSchemaVersion",
                message: "Category catalog schema version is missing.",
                path: "schemaVersion");
        }

        if (string.IsNullOrWhiteSpace(document.CatalogId))
        {
            result.AddError(
                code: "MissingCatalogId",
                message: "Category catalog id is missing.",
                path: "catalogId");
        }

        if (string.IsNullOrWhiteSpace(document.CatalogName))
        {
            result.AddWarning(
                code: "MissingCatalogName",
                message: "Category catalog name is missing.",
                path: "catalogName");
        }

        if (string.IsNullOrWhiteSpace(document.Language))
        {
            result.AddWarning(
                code: "MissingCatalogLanguage",
                message: "Category catalog language is missing. Default language should normally be specified.",
                path: "language");
        }
    }

    private static void ValidateCategories(
        ErrorCategoryCatalogDocument document,
        ErrorCatalogValidationResult result)
    {
        if (document.Categories.Count == 0)
        {
            result.AddWarning(
                code: "CategoryCatalogContainsNoCategories",
                message: "Category catalog does not contain any category definitions.",
                path: "categories");

            return;
        }

        HashSet<string> categoryNames = CreateNormalizedCategoryNameSet(document.Categories);
        HashSet<string> usedNames = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> usedAliases = new(StringComparer.OrdinalIgnoreCase);

        for (int categoryIndex = 0; categoryIndex < document.Categories.Count; categoryIndex++)
        {
            ErrorCategoryDefinition category = document.Categories[categoryIndex];
            string categoryPath = $"categories[{categoryIndex}]";

            ValidateSingleCategory(
                category,
                categoryPath,
                result);

            AddDuplicateNameCheck(
                usedNames,
                category.Name,
                category.Name,
                "DuplicateCategoryName",
                $"Duplicate category name '{category.Name}'.",
                categoryPath + ".name",
                result);

            ValidateStringCollection(
                category.Aliases,
                category.Name,
                categoryPath + ".aliases",
                "CategoryAlias",
                result);

            foreach (string alias in category.Aliases)
            {
                string normalizedAlias = TextKeyNormalizer.NormalizeKey(alias);

                if (string.IsNullOrWhiteSpace(normalizedAlias))
                {
                    continue;
                }

                if (!usedAliases.Add(normalizedAlias))
                {
                    result.AddWarning(
                        code: "DuplicateCategoryAliasAcrossCatalog",
                        message: $"Category alias '{alias}' is used more than once in the category catalog.",
                        errorId: category.Name,
                        errorName: category.DisplayName,
                        path: categoryPath + ".aliases");
                }

                if (categoryNames.Contains(normalizedAlias))
                {
                    result.AddWarning(
                        code: "CategoryAliasMatchesExistingCategoryName",
                        message: $"Category alias '{alias}' matches an existing category name.",
                        errorId: category.Name,
                        errorName: category.DisplayName,
                        path: categoryPath + ".aliases");
                }
            }

            ValidateStringCollection(
                category.ParentCategories,
                category.Name,
                categoryPath + ".parentCategories",
                "ParentCategory",
                result);

            ValidateStringCollection(
                category.DefaultTags,
                category.Name,
                categoryPath + ".defaultTags",
                "DefaultTag",
                result);
        }
    }

    private static HashSet<string> CreateNormalizedCategoryNameSet(
        IEnumerable<ErrorCategoryDefinition> categories)
    {
        HashSet<string> categoryNames = new(StringComparer.OrdinalIgnoreCase);

        foreach (ErrorCategoryDefinition category in categories)
        {
            string normalizedName = TextKeyNormalizer.NormalizeKey(category.Name);

            if (!string.IsNullOrWhiteSpace(normalizedName))
            {
                categoryNames.Add(normalizedName);
            }
        }

        return categoryNames;
    }

    private static void ValidateSingleCategory(
        ErrorCategoryDefinition category,
        string categoryPath,
        ErrorCatalogValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            result.AddError(
                code: "MissingCategoryName",
                message: "Category name is missing.",
                path: categoryPath + ".name");
        }

        if (string.IsNullOrWhiteSpace(category.DisplayName))
        {
            result.AddWarning(
                code: "MissingCategoryDisplayName",
                message: "Category display name is missing.",
                errorId: category.Name,
                path: categoryPath + ".displayName");
        }
    }

    private static void ValidateStringCollection(
        List<string> values,
        string categoryName,
        string path,
        string valueName,
        ErrorCatalogValidationResult result)
    {
        HashSet<string> usedValues = new(StringComparer.OrdinalIgnoreCase);

        for (int valueIndex = 0; valueIndex < values.Count; valueIndex++)
        {
            string value = values[valueIndex];

            if (string.IsNullOrWhiteSpace(value))
            {
                result.AddWarning(
                    code: $"Empty{valueName}",
                    message: $"{valueName} value is empty.",
                    errorId: categoryName,
                    path: $"{path}[{valueIndex}]");

                continue;
            }

            string normalizedValue = TextKeyNormalizer.NormalizeKey(value);

            if (!usedValues.Add(normalizedValue))
            {
                result.AddWarning(
                    code: $"Duplicate{valueName}",
                    message: $"{valueName} '{value}' is duplicated using normalized comparison.",
                    errorId: categoryName,
                    path: $"{path}[{valueIndex}]");
            }
        }
    }

    private static void AddDuplicateNameCheck(
        HashSet<string> usedValues,
        string value,
        string categoryName,
        string issueCode,
        string issueMessage,
        string path,
        ErrorCatalogValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        string normalizedValue = TextKeyNormalizer.NormalizeKey(value);

        if (!usedValues.Add(normalizedValue))
        {
            result.AddError(
                code: issueCode,
                message: issueMessage,
                errorId: categoryName,
                path: path);
        }
    }
}
