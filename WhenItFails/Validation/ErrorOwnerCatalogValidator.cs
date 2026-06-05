using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Validation;

/// <summary>
/// Default validator for error owner catalog documents.
/// </summary>
public sealed class ErrorOwnerCatalogValidator : IErrorOwnerCatalogValidator
{
    /// <inheritdoc />
    public ErrorCatalogValidationResult Validate(ErrorOwnerCatalogDocument? document)
    {
        ErrorCatalogValidationResult result = new();

        if (document is null)
        {
            result.AddError(
                code: "OwnerCatalogDocumentIsNull",
                message: "Error owner catalog document is null.",
                path: "$");

            return result;
        }

        ValidateDocumentHeader(document, result);
        ValidateOwners(document, result);

        return result;
    }

    private static void ValidateDocumentHeader(
        ErrorOwnerCatalogDocument document,
        ErrorCatalogValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(document.SchemaVersion))
        {
            result.AddError(
                code: "MissingSchemaVersion",
                message: "Owner catalog schema version is missing.",
                path: "schemaVersion");
        }

        if (string.IsNullOrWhiteSpace(document.CatalogId))
        {
            result.AddError(
                code: "MissingCatalogId",
                message: "Owner catalog id is missing.",
                path: "catalogId");
        }

        if (string.IsNullOrWhiteSpace(document.CatalogName))
        {
            result.AddWarning(
                code: "MissingCatalogName",
                message: "Owner catalog name is missing.",
                path: "catalogName");
        }

        if (string.IsNullOrWhiteSpace(document.Language))
        {
            result.AddWarning(
                code: "MissingCatalogLanguage",
                message: "Owner catalog language is missing. Default language should normally be specified.",
                path: "language");
        }
    }

    private static void ValidateOwners(
        ErrorOwnerCatalogDocument document,
        ErrorCatalogValidationResult result)
    {
        if (document.Owners.Count == 0)
        {
            result.AddWarning(
                code: "OwnerCatalogContainsNoOwners",
                message: "Owner catalog does not contain any owner definitions.",
                path: "owners");

            return;
        }

        HashSet<string> usedNames = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> allOwnerNames = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> usedAliases = new(StringComparer.OrdinalIgnoreCase);

        foreach (ErrorOwnerDefinition owner in document.Owners)
        {
            if (string.IsNullOrWhiteSpace(owner.Name))
            {
                continue;
            }

            string normalizedOwnerName = TextKeyNormalizer.NormalizeKey(owner.Name);

            if (!string.IsNullOrWhiteSpace(normalizedOwnerName))
            {
                allOwnerNames.Add(normalizedOwnerName);
            }
        }

        for (int ownerIndex = 0; ownerIndex < document.Owners.Count; ownerIndex++)
        {
            ErrorOwnerDefinition owner = document.Owners[ownerIndex];
            string ownerPath = $"owners[{ownerIndex}]";

            ValidateSingleOwner(owner, ownerPath, result);

            AddDuplicateKeyCheck(
                usedNames,
                owner.Name,
                owner.Name,
                "DuplicateOwnerName",
                $"Duplicate owner name '{owner.Name}'.",
                ownerPath + ".name",
                result);

            ValidateStringCollection(
                owner.Aliases,
                owner.Name,
                ownerPath + ".aliases",
                "OwnerAlias",
                result);

            ValidateOwnerAliases(
                owner,
                ownerPath,
                allOwnerNames,
                usedAliases,
                result);
        }

        ValidateRangeOverlaps(document, result);
    }

    private static void ValidateSingleOwner(
        ErrorOwnerDefinition owner,
        string ownerPath,
        ErrorCatalogValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(owner.Name))
        {
            result.AddError(
                code: "MissingOwnerName",
                message: "Owner name is missing.",
                path: ownerPath + ".name");
        }

        if (string.IsNullOrWhiteSpace(owner.DisplayName))
        {
            result.AddWarning(
                code: "MissingOwnerDisplayName",
                message: "Owner display name is missing.",
                errorId: owner.Name,
                path: ownerPath + ".displayName");
        }

        if (owner.CodeFrom < 0)
        {
            result.AddError(
                code: "InvalidOwnerCodeFrom",
                message: "Owner codeFrom must be greater than or equal to zero.",
                errorId: owner.Name,
                path: ownerPath + ".codeFrom");
        }

        if (owner.CodeTo < 0)
        {
            result.AddError(
                code: "InvalidOwnerCodeTo",
                message: "Owner codeTo must be greater than or equal to zero.",
                errorId: owner.Name,
                path: ownerPath + ".codeTo");
        }

        if (owner.CodeFrom >= 0
            && owner.CodeTo >= 0
            && owner.CodeFrom > owner.CodeTo)
        {
            result.AddError(
                code: "InvalidOwnerCodeRange",
                message: $"Owner code range is invalid. codeFrom '{owner.CodeFrom}' is greater than codeTo '{owner.CodeTo}'.",
                errorId: owner.Name,
                path: ownerPath);
        }
    }

    private static void ValidateOwnerAliases(
        ErrorOwnerDefinition owner,
        string ownerPath,
        HashSet<string> allOwnerNames,
        HashSet<string> usedAliases,
        ErrorCatalogValidationResult result)
    {
        foreach (string alias in owner.Aliases)
        {
            string normalizedAlias = TextKeyNormalizer.NormalizeKey(alias);

            if (string.IsNullOrWhiteSpace(normalizedAlias))
            {
                continue;
            }

            if (!usedAliases.Add(normalizedAlias))
            {
                result.AddWarning(
                    code: "DuplicateOwnerAliasAcrossCatalog",
                    message: $"Owner alias '{alias}' is used more than once in the owner catalog.",
                    errorId: owner.Name,
                    errorName: owner.DisplayName,
                    path: ownerPath + ".aliases");
            }

            if (allOwnerNames.Contains(normalizedAlias))
            {
                result.AddWarning(
                    code: "OwnerAliasMatchesExistingOwnerName",
                    message: $"Owner alias '{alias}' matches an existing owner name.",
                    errorId: owner.Name,
                    errorName: owner.DisplayName,
                    path: ownerPath + ".aliases");
            }
        }
    }

    private static void ValidateRangeOverlaps(
        ErrorOwnerCatalogDocument document,
        ErrorCatalogValidationResult result)
    {
        for (int firstIndex = 0; firstIndex < document.Owners.Count; firstIndex++)
        {
            ErrorOwnerDefinition first = document.Owners[firstIndex];

            if (!IsValidRange(first))
            {
                continue;
            }

            for (int secondIndex = firstIndex + 1; secondIndex < document.Owners.Count; secondIndex++)
            {
                ErrorOwnerDefinition second = document.Owners[secondIndex];

                if (!IsValidRange(second))
                {
                    continue;
                }

                bool overlaps =
                    first.CodeFrom <= second.CodeTo
                    && second.CodeFrom <= first.CodeTo;

                if (overlaps)
                {
                    result.AddError(
                        code: "OwnerCodeRangeOverlap",
                        message: $"Owner '{first.Name}' range {first.CodeFrom}-{first.CodeTo} overlaps with owner '{second.Name}' range {second.CodeFrom}-{second.CodeTo}.",
                        errorId: first.Name,
                        errorName: second.Name,
                        path: $"owners[{firstIndex}]");
                }
            }
        }
    }

    private static bool IsValidRange(ErrorOwnerDefinition owner)
    {
        return owner.CodeFrom >= 0
            && owner.CodeTo >= 0
            && owner.CodeFrom <= owner.CodeTo;
    }

    private static void ValidateStringCollection(
        List<string> values,
        string ownerName,
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
                    errorId: ownerName,
                    path: $"{path}[{valueIndex}]");

                continue;
            }

            string normalizedValue = TextKeyNormalizer.NormalizeKey(value);

            if (!usedValues.Add(normalizedValue))
            {
                result.AddWarning(
                    code: $"Duplicate{valueName}",
                    message: $"{valueName} '{value}' is duplicated using normalized comparison.",
                    errorId: ownerName,
                    path: $"{path}[{valueIndex}]");
            }
        }
    }

    private static void AddDuplicateKeyCheck(
        HashSet<string> usedValues,
        string value,
        string ownerName,
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
                errorId: ownerName,
                path: path);
        }
    }
}