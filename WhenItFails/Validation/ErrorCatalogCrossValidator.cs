using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Validation;

/// <summary>
/// Validates relationships between the main error catalog and supporting catalogs.
/// </summary>
/// <remarks>
/// This validator does not replace <see cref="ErrorCatalogValidator"/>.
/// It checks cross-document rules such as:
/// owner exists, code group exists, code belongs to owner and group ranges,
/// code prefix matches the selected code group, and categories are known.
/// </remarks>
public sealed class ErrorCatalogCrossValidator
{
    /// <summary>
    /// Validates the error catalog against owner, code group and category catalogs.
    /// </summary>
    /// <param name="errorCatalog">Main error catalog document.</param>
    /// <param name="ownerCatalog">Owner catalog document.</param>
    /// <param name="codeGroupCatalog">Code group catalog document.</param>
    /// <param name="categoryCatalog">Category catalog document.</param>
    /// <returns>Validation result containing all discovered issues.</returns>
    public ErrorCatalogValidationResult Validate(
        ErrorCatalogDocument? errorCatalog,
        ErrorOwnerCatalogDocument? ownerCatalog,
        ErrorCodeGroupCatalogDocument? codeGroupCatalog,
        ErrorCategoryCatalogDocument? categoryCatalog)
    {
        ErrorCatalogValidationResult result = new();

        if (errorCatalog is null)
        {
            result.AddError(
                code: "CatalogDocumentIsNull",
                message: "Error catalog document is null.",
                path: "$");

            return result;
        }

        if (ownerCatalog is null)
        {
            result.AddError(
                code: "OwnerCatalogDocumentIsNull",
                message: "Owner catalog document is null.",
                path: "ownerCatalog");
        }

        if (codeGroupCatalog is null)
        {
            result.AddError(
                code: "CodeGroupCatalogDocumentIsNull",
                message: "Code group catalog document is null.",
                path: "codeGroupCatalog");
        }

        if (categoryCatalog is null)
        {
            result.AddWarning(
                code: "CategoryCatalogDocumentIsNull",
                message: "Category catalog document is null. Category cross-validation will be skipped.",
                path: "categoryCatalog");
        }

        Dictionary<string, ErrorOwnerDefinition> ownersByName = BuildOwnerIndex(ownerCatalog);
        Dictionary<string, ErrorCodeGroupDefinition> codeGroupsByName = BuildCodeGroupIndex(codeGroupCatalog);
        Dictionary<string, ErrorCategoryDefinition> categoriesByName = BuildCategoryIndex(categoryCatalog);

        for (int errorIndex = 0; errorIndex < errorCatalog.Errors.Count; errorIndex++)
        {
            ErrorDefinition error = errorCatalog.Errors[errorIndex];

            ValidateErrorAgainstSupportingCatalogs(
                error,
                errorIndex,
                ownersByName,
                codeGroupsByName,
                categoriesByName,
                result);
        }

        return result;
    }

    private static void ValidateErrorAgainstSupportingCatalogs(
        ErrorDefinition error,
        int errorIndex,
        IReadOnlyDictionary<string, ErrorOwnerDefinition> ownersByName,
        IReadOnlyDictionary<string, ErrorCodeGroupDefinition> codeGroupsByName,
        IReadOnlyDictionary<string, ErrorCategoryDefinition> categoriesByName,
        ErrorCatalogValidationResult result)
    {
        string errorPath = $"errors[{errorIndex}]";

        ErrorOwnerDefinition? owner = ResolveOwner(error.Owner, ownersByName);
        ErrorCodeGroupDefinition? codeGroup = ResolveCodeGroup(error.CodeGroup, codeGroupsByName);

        if (!string.IsNullOrWhiteSpace(error.Owner) && owner is null)
        {
            result.AddError(
                code: "UnknownErrorOwner",
                message: $"Error owner '{error.Owner}' is not defined in the owner catalog.",
                errorId: error.Id,
                errorName: error.Name,
                path: $"{errorPath}.owner");
        }

        if (!string.IsNullOrWhiteSpace(error.CodeGroup) && codeGroup is null)
        {
            result.AddError(
                code: "UnknownErrorCodeGroup",
                message: $"Error code group '{error.CodeGroup}' is not defined in the code group catalog.",
                errorId: error.Id,
                errorName: error.Name,
                path: $"{errorPath}.codeGroup");
        }

        if (error.Code > 0 && owner is not null && !IsCodeInsideRange(error.Code, owner.CodeFrom, owner.CodeTo))
        {
            result.AddError(
                code: "ErrorCodeOutsideOwnerRange",
                message: $"Error code '{error.Code}' is outside owner '{owner.Name}' reserved range {owner.CodeFrom}-{owner.CodeTo}.",
                errorId: error.Id,
                errorName: error.Name,
                path: $"{errorPath}.code");
        }

        if (error.Code > 0 && codeGroup is not null && !IsCodeInsideRange(error.Code, codeGroup.CodeFrom, codeGroup.CodeTo))
        {
            result.AddError(
                code: "ErrorCodeOutsideCodeGroupRange",
                message: $"Error code '{error.Code}' is outside code group '{codeGroup.Name}' reserved range {codeGroup.CodeFrom}-{codeGroup.CodeTo}.",
                errorId: error.Id,
                errorName: error.Name,
                path: $"{errorPath}.code");
        }

        if (codeGroup is not null && !string.IsNullOrWhiteSpace(error.CodePrefix))
        {
            string normalizedErrorCodePrefix = TextKeyNormalizer.NormalizeKey(error.CodePrefix);
            string normalizedExpectedCodePrefix = TextKeyNormalizer.NormalizeKey(codeGroup.CodePrefix);

            if (!string.Equals(normalizedErrorCodePrefix, normalizedExpectedCodePrefix, StringComparison.OrdinalIgnoreCase))
            {
                result.AddError(
                    code: "ErrorCodePrefixDoesNotMatchCodeGroup",
                    message: $"Error code prefix '{error.CodePrefix}' does not match code group '{codeGroup.Name}' prefix '{codeGroup.CodePrefix}'.",
                    errorId: error.Id,
                    errorName: error.Name,
                    path: $"{errorPath}.codePrefix");
            }
        }

        ValidatePrimaryCategory(
            error,
            errorPath,
            categoriesByName,
            result);

        ValidateAdditionalCategories(
            error,
            errorPath,
            categoriesByName,
            result);
    }

    private static void ValidatePrimaryCategory(
        ErrorDefinition error,
        string errorPath,
        IReadOnlyDictionary<string, ErrorCategoryDefinition> categoriesByName,
        ErrorCatalogValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(error.PrimaryCategory))
        {
            return;
        }

        if (categoriesByName.Count == 0)
        {
            return;
        }

        string normalizedPrimaryCategory = TextKeyNormalizer.NormalizeKey(error.PrimaryCategory);

        if (!categoriesByName.ContainsKey(normalizedPrimaryCategory))
        {
            result.AddError(
                code: "UnknownPrimaryCategory",
                message: $"Primary category '{error.PrimaryCategory}' is not defined in the category catalog.",
                errorId: error.Id,
                errorName: error.Name,
                path: $"{errorPath}.primaryCategory");

            return;
        }

        bool primaryCategoryIsListedInCategories = error.Categories
            .Select(TextKeyNormalizer.NormalizeKey)
            .Any(category => string.Equals(category, normalizedPrimaryCategory, StringComparison.OrdinalIgnoreCase));

        if (error.Categories.Count > 0 && !primaryCategoryIsListedInCategories)
        {
            result.AddInformation(
                code: "PrimaryCategoryNotListedInCategories",
                message: $"Primary category '{error.PrimaryCategory}' is not listed in the additional categories collection.",
                errorId: error.Id,
                errorName: error.Name,
                path: $"{errorPath}.categories");
        }
    }

    private static void ValidateAdditionalCategories(
        ErrorDefinition error,
        string errorPath,
        IReadOnlyDictionary<string, ErrorCategoryDefinition> categoriesByName,
        ErrorCatalogValidationResult result)
    {
        if (categoriesByName.Count == 0)
        {
            return;
        }

        for (int categoryIndex = 0; categoryIndex < error.Categories.Count; categoryIndex++)
        {
            string category = error.Categories[categoryIndex];

            if (string.IsNullOrWhiteSpace(category))
            {
                continue;
            }

            string normalizedCategory = TextKeyNormalizer.NormalizeKey(category);

            if (!categoriesByName.ContainsKey(normalizedCategory))
            {
                result.AddWarning(
                    code: "UnknownAdditionalCategory",
                    message: $"Additional category '{category}' is not defined in the category catalog.",
                    errorId: error.Id,
                    errorName: error.Name,
                    path: $"{errorPath}.categories[{categoryIndex}]");
            }
        }
    }

    private static Dictionary<string, ErrorOwnerDefinition> BuildOwnerIndex(ErrorOwnerCatalogDocument? ownerCatalog)
    {
        Dictionary<string, ErrorOwnerDefinition> ownersByName = new(StringComparer.OrdinalIgnoreCase);

        if (ownerCatalog is null)
        {
            return ownersByName;
        }

        foreach (ErrorOwnerDefinition owner in ownerCatalog.Owners)
        {
            AddOwnerKey(ownersByName, owner.Name, owner);

            foreach (string alias in owner.Aliases)
            {
                AddOwnerKey(ownersByName, alias, owner);
            }
        }

        return ownersByName;
    }

    private static Dictionary<string, ErrorCodeGroupDefinition> BuildCodeGroupIndex(ErrorCodeGroupCatalogDocument? codeGroupCatalog)
    {
        Dictionary<string, ErrorCodeGroupDefinition> codeGroupsByName = new(StringComparer.OrdinalIgnoreCase);

        if (codeGroupCatalog is null)
        {
            return codeGroupsByName;
        }

        foreach (ErrorCodeGroupDefinition codeGroup in codeGroupCatalog.CodeGroups)
        {
            if (string.IsNullOrWhiteSpace(codeGroup.Name))
            {
                continue;
            }

            string normalizedName = TextKeyNormalizer.NormalizeKey(codeGroup.Name);

            if (!codeGroupsByName.ContainsKey(normalizedName))
            {
                codeGroupsByName.Add(normalizedName, codeGroup);
            }
        }

        return codeGroupsByName;
    }

    private static Dictionary<string, ErrorCategoryDefinition> BuildCategoryIndex(ErrorCategoryCatalogDocument? categoryCatalog)
    {
        Dictionary<string, ErrorCategoryDefinition> categoriesByName = new(StringComparer.OrdinalIgnoreCase);

        if (categoryCatalog is null)
        {
            return categoriesByName;
        }

        foreach (ErrorCategoryDefinition category in categoryCatalog.Categories)
        {
            AddCategoryKey(categoriesByName, category.Name, category);

            foreach (string alias in category.Aliases)
            {
                AddCategoryKey(categoriesByName, alias, category);
            }
        }

        return categoriesByName;
    }

    private static ErrorOwnerDefinition? ResolveOwner(
        string ownerName,
        IReadOnlyDictionary<string, ErrorOwnerDefinition> ownersByName)
    {
        if (string.IsNullOrWhiteSpace(ownerName))
        {
            return null;
        }

        string normalizedOwnerName = TextKeyNormalizer.NormalizeKey(ownerName);

        return ownersByName.TryGetValue(normalizedOwnerName, out ErrorOwnerDefinition? owner)
            ? owner
            : null;
    }

    private static ErrorCodeGroupDefinition? ResolveCodeGroup(
        string codeGroupName,
        IReadOnlyDictionary<string, ErrorCodeGroupDefinition> codeGroupsByName)
    {
        if (string.IsNullOrWhiteSpace(codeGroupName))
        {
            return null;
        }

        string normalizedCodeGroupName = TextKeyNormalizer.NormalizeKey(codeGroupName);

        return codeGroupsByName.TryGetValue(normalizedCodeGroupName, out ErrorCodeGroupDefinition? codeGroup)
            ? codeGroup
            : null;
    }

    private static void AddOwnerKey(
        IDictionary<string, ErrorOwnerDefinition> ownersByName,
        string key,
        ErrorOwnerDefinition owner)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        string normalizedKey = TextKeyNormalizer.NormalizeKey(key);

        if (!ownersByName.ContainsKey(normalizedKey))
        {
            ownersByName.Add(normalizedKey, owner);
        }
    }

    private static void AddCategoryKey(
        IDictionary<string, ErrorCategoryDefinition> categoriesByName,
        string key,
        ErrorCategoryDefinition category)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        string normalizedKey = TextKeyNormalizer.NormalizeKey(key);

        if (!categoriesByName.ContainsKey(normalizedKey))
        {
            categoriesByName.Add(normalizedKey, category);
        }
    }

    private static bool IsCodeInsideRange(int code, int codeFrom, int codeTo)
    {
        return code >= codeFrom && code <= codeTo;
    }
}