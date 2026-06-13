using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Validation;

/// <summary>
/// Default validator for error profile catalog documents.
/// </summary>
public sealed class ErrorProfileCatalogValidator : IErrorProfileCatalogValidator
{
    /// <inheritdoc />
    public ErrorCatalogValidationResult Validate(ErrorProfileCatalogDocument? document)
    {
        ErrorCatalogValidationResult result = new();

        if (document is null)
        {
            result.AddError(
                code: "ProfileCatalogDocumentIsNull",
                message: "Error profile catalog document is null.",
                path: "$");

            return result;
        }

        ValidateDocumentHeader(document, result);
        ValidateProfiles(document, result);

        return result;
    }

    private static void ValidateDocumentHeader(
        ErrorProfileCatalogDocument document,
        ErrorCatalogValidationResult result)
    {
        CatalogValidationHelper.ValidateDocumentHeader(
            catalogKind: "Profile",
            schemaVersion: document.SchemaVersion,
            catalogId: document.CatalogId,
            catalogName: document.CatalogName,
            language: document.Language,
            result: result);
    }

    private static void ValidateProfiles(
        ErrorProfileCatalogDocument document,
        ErrorCatalogValidationResult result)
    {
        if (document.Profiles.Count == 0)
        {
            result.AddWarning(
                code: "ProfileCatalogContainsNoProfiles",
                message: "Profile catalog does not contain any profile definitions.",
                path: "profiles");

            return;
        }

        HashSet<string> usedNames = new(StringComparer.OrdinalIgnoreCase);

        for (int profileIndex = 0; profileIndex < document.Profiles.Count; profileIndex++)
        {
            ErrorProfileDefinition profile = document.Profiles[profileIndex];
            string profilePath = $"profiles[{profileIndex}]";

            ValidateSingleProfile(profile, profilePath, result);

            CatalogValidationHelper.AddDuplicateKeyCheck(
                usedNames,
                profile.Name,
                profile.Name,
                "DuplicateProfileName",
                $"Duplicate profile name '{profile.Name}'.",
                profilePath + ".name",
                result);

            CatalogValidationHelper.ValidateStringCollection(
                profile.IncludeOwners,
                profile.Name,
                profilePath + ".includeOwners",
                "IncludeOwner",
                result);

            CatalogValidationHelper.ValidateStringCollection(
                profile.IncludeCodeGroups,
                profile.Name,
                profilePath + ".includeCodeGroups",
                "IncludeCodeGroup",
                result);

            CatalogValidationHelper.ValidateStringCollection(
                profile.IncludeCategories,
                profile.Name,
                profilePath + ".includeCategories",
                "IncludeCategory",
                result);

            CatalogValidationHelper.ValidateStringCollection(
                profile.IncludeSubcategories,
                profile.Name,
                profilePath + ".includeSubcategories",
                "IncludeSubcategory",
                result);

            CatalogValidationHelper.ValidateStringCollection(
                profile.IncludeTags,
                profile.Name,
                profilePath + ".includeTags",
                "IncludeTag",
                result);

            CatalogValidationHelper.ValidateStringCollection(
                profile.ExcludeTags,
                profile.Name,
                profilePath + ".excludeTags",
                "ExcludeTag",
                result);

            CatalogValidationHelper.ValidateStringCollection(
                profile.IncludeErrors,
                profile.Name,
                profilePath + ".includeErrors",
                "IncludeError",
                result);

            CatalogValidationHelper.ValidateStringCollection(
                profile.ExcludeErrors,
                profile.Name,
                profilePath + ".excludeErrors",
                "ExcludeError",
                result);
            ValidateIncludeExcludeErrorConflicts(
                profile,
                profilePath,
                result);

        }
    }

    private static void ValidateSingleProfile(
        ErrorProfileDefinition profile,
        string profilePath,
        ErrorCatalogValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(profile.Name))
        {
            result.AddError(
                code: "MissingProfileName",
                message: "Profile name is missing.",
                path: profilePath + ".name");
        }

        if (string.IsNullOrWhiteSpace(profile.DisplayName))
        {
            result.AddWarning(
                code: "MissingProfileDisplayName",
                message: "Profile display name is missing.",
                errorId: profile.Name,
                path: profilePath + ".displayName");
        }
    }

    private static void ValidateIncludeExcludeErrorConflicts(
    ErrorProfileDefinition profile,
    string profilePath,
    ErrorCatalogValidationResult result)
    {
        HashSet<string> includedErrors = profile.IncludeErrors
            .Where(errorId => !string.IsNullOrWhiteSpace(errorId))
            .Select(TextKeyNormalizer.NormalizeKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (int errorIndex = 0;
             errorIndex < profile.ExcludeErrors.Count;
             errorIndex++)
        {
            string excludedErrorId = profile.ExcludeErrors[errorIndex];

            if (string.IsNullOrWhiteSpace(excludedErrorId))
            {
                continue;
            }

            string normalizedExcludedErrorId =
                TextKeyNormalizer.NormalizeKey(excludedErrorId);

            if (!includedErrors.Contains(normalizedExcludedErrorId))
            {
                continue;
            }

            result.AddWarning(
                code: "ProfileErrorIncludedAndExcluded",
                message:
                    $"Profile '{profile.Name}' includes and excludes error " +
                    $"'{excludedErrorId}' at the same time.",
                errorId: excludedErrorId,
                errorName: profile.Name,
                path: $"{profilePath}.excludeErrors[{errorIndex}]");
        }
    }
}