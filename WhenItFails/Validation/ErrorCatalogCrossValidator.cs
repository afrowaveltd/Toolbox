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
/// code prefix matches the selected code group, categories are known,
/// and profiles reference known errors, owners, code groups and categories.
/// </remarks>
public sealed class ErrorCatalogCrossValidator
{
    /// <summary>
    /// Validates the error catalog against owner, code group, category and profile catalogs.
    /// </summary>
    /// <param name="errorCatalog">Main error catalog document.</param>
    /// <param name="ownerCatalog">Owner catalog document.</param>
    /// <param name="codeGroupCatalog">Code group catalog document.</param>
    /// <param name="categoryCatalog">Category catalog document.</param>
    /// <param name="profileCatalog">Profile catalog document.</param>
    /// <returns>Validation result containing all discovered issues.</returns>
    public ErrorCatalogValidationResult Validate(
        ErrorCatalogDocument? errorCatalog,
        ErrorOwnerCatalogDocument? ownerCatalog,
        ErrorCodeGroupCatalogDocument? codeGroupCatalog,
        ErrorCategoryCatalogDocument? categoryCatalog,
        ErrorProfileCatalogDocument? profileCatalog = null)
    {
        ErrorCatalogValidationResult result = new();

        if (errorCatalog is null)
        {
            result.AddError("CatalogDocumentIsNull", "Error catalog document is null.", path: "$");
            return result;
        }

        if (ownerCatalog is null)
            result.AddError("OwnerCatalogDocumentIsNull", "Owner catalog document is null.", path: "ownerCatalog");

        if (codeGroupCatalog is null)
            result.AddError("CodeGroupCatalogDocumentIsNull", "Code group catalog document is null.", path: "codeGroupCatalog");

        if (categoryCatalog is null)
            result.AddWarning("CategoryCatalogDocumentIsNull", "Category catalog document is null. Category cross-validation will be skipped.", path: "categoryCatalog");

        if (profileCatalog is null)
            result.AddWarning("ProfileCatalogDocumentIsNull", "Profile catalog document is null. Profile cross-validation will be skipped.", path: "profileCatalog");

        if (errorCatalog.Errors is null)
        {
            result.AddError("CatalogErrorsCollectionIsNull", "Error catalog errors collection is null.", path: "errors");
            return result;
        }

        for (int errorIndex = 0; errorIndex < errorCatalog.Errors.Count; errorIndex++)
        {
            if (errorCatalog.Errors[errorIndex] is null)
                result.AddError("ErrorDefinitionIsNull", "Error catalog contains a null error definition.", path: $"errors[{errorIndex}]");
        }

        if (ownerCatalog is not null)
        {
            for (int ownerIndex = 0; ownerIndex < ownerCatalog.Owners.Count; ownerIndex++)
            {
                if (ownerCatalog.Owners[ownerIndex] is null)
                    result.AddError("OwnerDefinitionIsNull", "Owner catalog contains a null owner definition.", path: $"ownerCatalog.owners[{ownerIndex}]");
            }
        }

        if (codeGroupCatalog is not null)
        {
            for (int codeGroupIndex = 0; codeGroupIndex < codeGroupCatalog.CodeGroups.Count; codeGroupIndex++)
            {
                if (codeGroupCatalog.CodeGroups[codeGroupIndex] is null)
                    result.AddError("CodeGroupDefinitionIsNull", "Code group catalog contains a null code group definition.", path: $"codeGroupCatalog.codeGroups[{codeGroupIndex}]");
            }
        }

        if (categoryCatalog is not null)
        {
            for (int categoryIndex = 0; categoryIndex < categoryCatalog.Categories.Count; categoryIndex++)
            {
                if (categoryCatalog.Categories[categoryIndex] is null)
                    result.AddError("CategoryDefinitionIsNull", "Category catalog contains a null category definition.", path: $"categoryCatalog.categories[{categoryIndex}]");
            }
        }

        if (profileCatalog is not null)
        {
            for (int profileIndex = 0; profileIndex < profileCatalog.Profiles.Count; profileIndex++)
            {
                if (profileCatalog.Profiles[profileIndex] is null)
                    result.AddError("ProfileDefinitionIsNull", "Profile catalog contains a null profile definition.", path: $"profileCatalog.profiles[{profileIndex}]");
            }
        }

        Dictionary<string, ErrorDefinition> errorsById = BuildErrorIndex(errorCatalog);
        Dictionary<string, ErrorOwnerDefinition> ownersByName = BuildOwnerIndex(ownerCatalog);
        Dictionary<string, ErrorCodeGroupDefinition> codeGroupsByName = BuildCodeGroupIndex(codeGroupCatalog);
        Dictionary<string, ErrorCategoryDefinition> categoriesByName = BuildCategoryIndex(categoryCatalog);

        for (int errorIndex = 0; errorIndex < errorCatalog.Errors.Count; errorIndex++)
        {
            ErrorDefinition? error = errorCatalog.Errors[errorIndex];
            if (error is null)
                continue;

            ValidateErrorAgainstSupportingCatalogs(error, errorIndex, ownersByName, codeGroupsByName, categoriesByName, result);
        }

        ValidateProfilesAgainstSupportingCatalogs(profileCatalog, errorsById, ownersByName, codeGroupsByName, categoriesByName, result);
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
            result.AddError("UnknownErrorOwner", $"Error owner '{error.Owner}' is not defined in the owner catalog.", error.Id, error.Name, $"{errorPath}.owner");

        if (!string.IsNullOrWhiteSpace(error.CodeGroup) && codeGroup is null)
            result.AddError("UnknownErrorCodeGroup", $"Error code group '{error.CodeGroup}' is not defined in the code group catalog.", error.Id, error.Name, $"{errorPath}.codeGroup");

        if (error.Code > 0 && owner is not null && !IsCodeInsideRange(error.Code, owner.CodeFrom, owner.CodeTo))
            result.AddError("ErrorCodeOutsideOwnerRange", $"Error code '{error.Code}' is outside owner '{owner.Name}' reserved range {owner.CodeFrom}-{owner.CodeTo}.", error.Id, error.Name, $"{errorPath}.code");

        if (error.Code > 0 && codeGroup is not null && !IsCodeInsideRange(error.Code, codeGroup.CodeFrom, codeGroup.CodeTo))
            result.AddError("ErrorCodeOutsideCodeGroupRange", $"Error code '{error.Code}' is outside code group '{codeGroup.Name}' reserved range {codeGroup.CodeFrom}-{codeGroup.CodeTo}.", error.Id, error.Name, $"{errorPath}.code");

        if (codeGroup is not null && !string.IsNullOrWhiteSpace(error.CodePrefix))
        {
            string normalizedErrorCodePrefix = TextKeyNormalizer.NormalizeKey(error.CodePrefix);
            string normalizedExpectedCodePrefix = TextKeyNormalizer.NormalizeKey(codeGroup.CodePrefix);
            if (!string.Equals(normalizedErrorCodePrefix, normalizedExpectedCodePrefix, StringComparison.OrdinalIgnoreCase))
                result.AddError("ErrorCodePrefixDoesNotMatchCodeGroup", $"Error code prefix '{error.CodePrefix}' does not match code group '{codeGroup.Name}' prefix '{codeGroup.CodePrefix}'.", error.Id, error.Name, $"{errorPath}.codePrefix");
        }

        ValidatePrimaryCategory(error, errorPath, categoriesByName, result);
        ValidateAdditionalCategories(error, errorPath, categoriesByName, result);
    }

    private static void ValidatePrimaryCategory(ErrorDefinition error, string errorPath, IReadOnlyDictionary<string, ErrorCategoryDefinition> categoriesByName, ErrorCatalogValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(error.PrimaryCategory) || categoriesByName.Count == 0)
            return;

        string normalizedPrimaryCategory = TextKeyNormalizer.NormalizeKey(error.PrimaryCategory);
        if (!categoriesByName.ContainsKey(normalizedPrimaryCategory))
        {
            result.AddError("UnknownPrimaryCategory", $"Primary category '{error.PrimaryCategory}' is not defined in the category catalog.", error.Id, error.Name, $"{errorPath}.primaryCategory");
            return;
        }

        bool listed = error.Categories.Select(TextKeyNormalizer.NormalizeKey).Any(category => string.Equals(category, normalizedPrimaryCategory, StringComparison.OrdinalIgnoreCase));
        if (error.Categories.Count > 0 && !listed)
            result.AddInformation("PrimaryCategoryNotListedInCategories", $"Primary category '{error.PrimaryCategory}' is not listed in the additional categories collection.", error.Id, error.Name, $"{errorPath}.categories");
    }

    private static void ValidateAdditionalCategories(ErrorDefinition error, string errorPath, IReadOnlyDictionary<string, ErrorCategoryDefinition> categoriesByName, ErrorCatalogValidationResult result)
    {
        if (categoriesByName.Count == 0)
            return;

        for (int categoryIndex = 0; categoryIndex < error.Categories.Count; categoryIndex++)
        {
            string category = error.Categories[categoryIndex];
            if (string.IsNullOrWhiteSpace(category))
                continue;

            string normalizedCategory = TextKeyNormalizer.NormalizeKey(category);
            if (!categoriesByName.ContainsKey(normalizedCategory))
                result.AddWarning("UnknownAdditionalCategory", $"Additional category '{category}' is not defined in the category catalog.", error.Id, error.Name, $"{errorPath}.categories[{categoryIndex}]");
        }
    }

    private static void ValidateProfilesAgainstSupportingCatalogs(
        ErrorProfileCatalogDocument? profileCatalog,
        IReadOnlyDictionary<string, ErrorDefinition> errorsById,
        IReadOnlyDictionary<string, ErrorOwnerDefinition> ownersByName,
        IReadOnlyDictionary<string, ErrorCodeGroupDefinition> codeGroupsByName,
        IReadOnlyDictionary<string, ErrorCategoryDefinition> categoriesByName,
        ErrorCatalogValidationResult result)
    {
        if (profileCatalog is null)
            return;

        for (int profileIndex = 0; profileIndex < profileCatalog.Profiles.Count; profileIndex++)
        {
            ErrorProfileDefinition? profile = profileCatalog.Profiles[profileIndex];
            if (profile is null)
                continue;

            string profilePath = $"profiles[{profileIndex}]";
            ValidateProfileIncludeErrors(profile, profilePath, errorsById, result);
            ValidateProfileExcludeErrors(profile, profilePath, errorsById, result);
            ValidateProfileIncludeOwners(profile, profilePath, ownersByName, result);
            ValidateProfileIncludeCodeGroups(profile, profilePath, codeGroupsByName, result);
            ValidateProfileIncludeCategories(profile, profilePath, categoriesByName, result);
        }
    }

    private static void ValidateProfileIncludeErrors(ErrorProfileDefinition profile, string profilePath, IReadOnlyDictionary<string, ErrorDefinition> errorsById, ErrorCatalogValidationResult result)
    {
        for (int errorIndex = 0; errorIndex < profile.IncludeErrors.Count; errorIndex++)
        {
            string errorId = profile.IncludeErrors[errorIndex];
            if (string.IsNullOrWhiteSpace(errorId)) continue;
            string normalizedErrorId = TextKeyNormalizer.NormalizeKey(errorId);
            if (!errorsById.ContainsKey(normalizedErrorId))
                result.AddWarning("UnknownProfileIncludeError", $"Profile '{profile.Name}' includes error '{errorId}', but this error is not defined in the error catalog.", errorId, profile.Name, $"{profilePath}.includeErrors[{errorIndex}]");
        }
    }

    private static void ValidateProfileExcludeErrors(ErrorProfileDefinition profile, string profilePath, IReadOnlyDictionary<string, ErrorDefinition> errorsById, ErrorCatalogValidationResult result)
    {
        for (int errorIndex = 0; errorIndex < profile.ExcludeErrors.Count; errorIndex++)
        {
            string errorId = profile.ExcludeErrors[errorIndex];
            if (string.IsNullOrWhiteSpace(errorId)) continue;
            string normalizedErrorId = TextKeyNormalizer.NormalizeKey(errorId);
            if (!errorsById.ContainsKey(normalizedErrorId))
                result.AddWarning("UnknownProfileExcludeError", $"Profile '{profile.Name}' excludes error '{errorId}', but this error is not defined in the error catalog.", errorId, profile.Name, $"{profilePath}.excludeErrors[{errorIndex}]");
        }
    }

    private static void ValidateProfileIncludeOwners(ErrorProfileDefinition profile, string profilePath, IReadOnlyDictionary<string, ErrorOwnerDefinition> ownersByName, ErrorCatalogValidationResult result)
    {
        if (ownersByName.Count == 0) return;
        for (int ownerIndex = 0; ownerIndex < profile.IncludeOwners.Count; ownerIndex++)
        {
            string owner = profile.IncludeOwners[ownerIndex];
            if (string.IsNullOrWhiteSpace(owner)) continue;
            string normalizedOwner = TextKeyNormalizer.NormalizeKey(owner);
            if (!ownersByName.ContainsKey(normalizedOwner))
                result.AddWarning("UnknownProfileIncludeOwner", $"Profile '{profile.Name}' includes owner '{owner}', but this owner is not defined in the owner catalog.", errorName: profile.Name, path: $"{profilePath}.includeOwners[{ownerIndex}]");
        }
    }

    private static void ValidateProfileIncludeCodeGroups(ErrorProfileDefinition profile, string profilePath, IReadOnlyDictionary<string, ErrorCodeGroupDefinition> codeGroupsByName, ErrorCatalogValidationResult result)
    {
        if (codeGroupsByName.Count == 0) return;
        for (int codeGroupIndex = 0; codeGroupIndex < profile.IncludeCodeGroups.Count; codeGroupIndex++)
        {
            string codeGroup = profile.IncludeCodeGroups[codeGroupIndex];
            if (string.IsNullOrWhiteSpace(codeGroup)) continue;
            string normalizedCodeGroup = TextKeyNormalizer.NormalizeKey(codeGroup);
            if (!codeGroupsByName.ContainsKey(normalizedCodeGroup))
                result.AddWarning("UnknownProfileIncludeCodeGroup", $"Profile '{profile.Name}' includes code group '{codeGroup}', but this code group is not defined in the code group catalog.", errorName: profile.Name, path: $"{profilePath}.includeCodeGroups[{codeGroupIndex}]");
        }
    }

    private static void ValidateProfileIncludeCategories(ErrorProfileDefinition profile, string profilePath, IReadOnlyDictionary<string, ErrorCategoryDefinition> categoriesByName, ErrorCatalogValidationResult result)
    {
        if (categoriesByName.Count == 0) return;
        for (int categoryIndex = 0; categoryIndex < profile.IncludeCategories.Count; categoryIndex++)
        {
            string category = profile.IncludeCategories[categoryIndex];
            if (string.IsNullOrWhiteSpace(category)) continue;
            string normalizedCategory = TextKeyNormalizer.NormalizeKey(category);
            if (!categoriesByName.ContainsKey(normalizedCategory))
                result.AddWarning("UnknownProfileIncludeCategory", $"Profile '{profile.Name}' includes category '{category}', but this category is not defined in the category catalog.", errorName: profile.Name, path: $"{profilePath}.includeCategories[{categoryIndex}]");
        }
    }

    private static Dictionary<string, ErrorDefinition> BuildErrorIndex(ErrorCatalogDocument errorCatalog)
    {
        Dictionary<string, ErrorDefinition> errorsById = new(StringComparer.OrdinalIgnoreCase);
        foreach (ErrorDefinition? error in errorCatalog.Errors)
        {
            if (error is null || string.IsNullOrWhiteSpace(error.Id)) continue;
            string normalizedErrorId = TextKeyNormalizer.NormalizeKey(error.Id);
            if (!errorsById.ContainsKey(normalizedErrorId)) errorsById.Add(normalizedErrorId, error);
        }
        return errorsById;
    }

    private static Dictionary<string, ErrorOwnerDefinition> BuildOwnerIndex(ErrorOwnerCatalogDocument? ownerCatalog)
    {
        Dictionary<string, ErrorOwnerDefinition> ownersByName = new(StringComparer.OrdinalIgnoreCase);
        if (ownerCatalog is null) return ownersByName;
        foreach (ErrorOwnerDefinition? owner in ownerCatalog.Owners)
        {
            if (owner is null) continue;
            AddOwnerKey(ownersByName, owner.Name, owner);
            foreach (string alias in owner.Aliases) AddOwnerKey(ownersByName, alias, owner);
        }
        return ownersByName;
    }

    private static Dictionary<string, ErrorCodeGroupDefinition> BuildCodeGroupIndex(ErrorCodeGroupCatalogDocument? codeGroupCatalog)
    {
        Dictionary<string, ErrorCodeGroupDefinition> codeGroupsByName = new(StringComparer.OrdinalIgnoreCase);
        if (codeGroupCatalog is null) return codeGroupsByName;
        foreach (ErrorCodeGroupDefinition? codeGroup in codeGroupCatalog.CodeGroups)
        {
            if (codeGroup is null || string.IsNullOrWhiteSpace(codeGroup.Name)) continue;
            string normalizedName = TextKeyNormalizer.NormalizeKey(codeGroup.Name);
            if (!codeGroupsByName.ContainsKey(normalizedName)) codeGroupsByName.Add(normalizedName, codeGroup);
        }
        return codeGroupsByName;
    }

    private static Dictionary<string, ErrorCategoryDefinition> BuildCategoryIndex(ErrorCategoryCatalogDocument? categoryCatalog)
    {
        Dictionary<string, ErrorCategoryDefinition> categoriesByName = new(StringComparer.OrdinalIgnoreCase);
        if (categoryCatalog is null) return categoriesByName;
        foreach (ErrorCategoryDefinition? category in categoryCatalog.Categories)
        {
            if (category is null) continue;
            AddCategoryKey(categoriesByName, category.Name, category);
            foreach (string alias in category.Aliases) AddCategoryKey(categoriesByName, alias, category);
        }
        return categoriesByName;
    }

    private static ErrorOwnerDefinition? ResolveOwner(string ownerName, IReadOnlyDictionary<string, ErrorOwnerDefinition> ownersByName)
    {
        if (string.IsNullOrWhiteSpace(ownerName)) return null;
        string normalizedOwnerName = TextKeyNormalizer.NormalizeKey(ownerName);
        return ownersByName.TryGetValue(normalizedOwnerName, out ErrorOwnerDefinition? owner) ? owner : null;
    }

    private static ErrorCodeGroupDefinition? ResolveCodeGroup(string codeGroupName, IReadOnlyDictionary<string, ErrorCodeGroupDefinition> codeGroupsByName)
    {
        if (string.IsNullOrWhiteSpace(codeGroupName)) return null;
        string normalizedCodeGroupName = TextKeyNormalizer.NormalizeKey(codeGroupName);
        return codeGroupsByName.TryGetValue(normalizedCodeGroupName, out ErrorCodeGroupDefinition? codeGroup) ? codeGroup : null;
    }

    private static void AddOwnerKey(IDictionary<string, ErrorOwnerDefinition> ownersByName, string key, ErrorOwnerDefinition owner)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        string normalizedKey = TextKeyNormalizer.NormalizeKey(key);
        if (!ownersByName.ContainsKey(normalizedKey)) ownersByName.Add(normalizedKey, owner);
    }

    private static void AddCategoryKey(IDictionary<string, ErrorCategoryDefinition> categoriesByName, string key, ErrorCategoryDefinition category)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        string normalizedKey = TextKeyNormalizer.NormalizeKey(key);
        if (!categoriesByName.ContainsKey(normalizedKey)) categoriesByName.Add(normalizedKey, category);
    }

    private static bool IsCodeInsideRange(int code, int codeFrom, int codeTo) => code >= codeFrom && code <= codeTo;
}
