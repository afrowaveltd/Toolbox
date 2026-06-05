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
        if (string.IsNullOrWhiteSpace(document.SchemaVersion))
        {
            result.AddError(
                code: "MissingSchemaVersion",
                message: "Profile catalog schema version is missing.",
                path: "schemaVersion");
        }

        if (string.IsNullOrWhiteSpace(document.CatalogId))
        {
            result.AddError(
                code: "MissingCatalogId",
                message: "Profile catalog id is missing.",
                path: "catalogId");
        }

        if (string.IsNullOrWhiteSpace(document.CatalogName))
        {
            result.AddWarning(
                code: "MissingCatalogName",
                message: "Profile catalog name is missing.",
                path: "catalogName");
        }

        if (string.IsNullOrWhiteSpace(document.Language))
        {
            result.AddWarning(
                code: "MissingCatalogLanguage",
                message: "Profile catalog language is missing. Default language should normally be specified.",
                path: "language");
        }
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

            AddDuplicateKeyCheck(
                usedNames,
                profile.Name,
                profile.Name,
                "DuplicateProfileName",
                $"Duplicate profile name '{profile.Name}'.",
                profilePath + ".name",
                result);

            ValidateStringCollection(
                profile.IncludeOwners,
                profile.Name,
                profilePath + ".includeOwners",
                "IncludeOwner",
                result);

            ValidateStringCollection(
                profile.IncludeCodeGroups,
                profile.Name,
                profilePath + ".includeCodeGroups",
                "IncludeCodeGroup",
                result);

            ValidateStringCollection(
                profile.IncludeCategories,
                profile.Name,
                profilePath + ".includeCategories",
                "IncludeCategory",
                result);

            ValidateStringCollection(
                profile.IncludeSubcategories,
                profile.Name,
                profilePath + ".includeSubcategories",
                "IncludeSubcategory",
                result);

            ValidateStringCollection(
                profile.IncludeTags,
                profile.Name,
                profilePath + ".includeTags",
                "IncludeTag",
                result);

            ValidateStringCollection(
                profile.ExcludeTags,
                profile.Name,
                profilePath + ".excludeTags",
                "ExcludeTag",
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

    private static void ValidateStringCollection(
        List<string> values,
        string profileName,
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
                    errorId: profileName,
                    path: $"{path}[{valueIndex}]");

                continue;
            }

            string normalizedValue = TextKeyNormalizer.NormalizeKey(value);

            if (!usedValues.Add(normalizedValue))
            {
                result.AddWarning(
                    code: $"Duplicate{valueName}",
                    message: $"{valueName} '{value}' is duplicated using normalized comparison.",
                    errorId: profileName,
                    path: $"{path}[{valueIndex}]");
            }
        }
    }

    private static void AddDuplicateKeyCheck(
        HashSet<string> usedValues,
        string value,
        string profileName,
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
                errorId: profileName,
                path: path);
        }
    }
}