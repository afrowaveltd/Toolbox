using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

/// <summary>
/// Performs read-only checks of documentation keys in an error catalog.
/// </summary>
internal sealed class WhenItFailsDocumentationKeyChecker
{
    /// <summary>
    /// Finds error definitions with missing documentation keys and keys used by more than one error.
    /// </summary>
    public DocumentationKeyCheckReport Check(ErrorCatalogDocument catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        ErrorDefinition[] errors = catalog.Errors?.ToArray() ?? [];

        DocumentationKeyIssue[] missingKeys = errors
            .Where(error => string.IsNullOrWhiteSpace(error.DocumentationKey))
            .OrderBy(error => error.Code)
            .ThenBy(error => error.Id, StringComparer.OrdinalIgnoreCase)
            .Select(error => new DocumentationKeyIssue(
                ErrorId: error.Id,
                ErrorCode: error.Code,
                ErrorName: error.Name,
                DocumentationKey: error.DocumentationKey))
            .ToArray();

        DuplicateDocumentationKey[] duplicateKeys = errors
            .Where(error => !string.IsNullOrWhiteSpace(error.DocumentationKey))
            .GroupBy(
                error => error.DocumentationKey!.Trim(),
                StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => new DuplicateDocumentationKey(
                DocumentationKey: group.Key,
                Errors: group
                    .OrderBy(error => error.Code)
                    .ThenBy(error => error.Id, StringComparer.OrdinalIgnoreCase)
                    .Select(error => new DocumentationKeyIssue(
                        ErrorId: error.Id,
                        ErrorCode: error.Code,
                        ErrorName: error.Name,
                        DocumentationKey: error.DocumentationKey))
                    .ToArray()))
            .ToArray();

        return new DocumentationKeyCheckReport(
            TotalErrors: errors.Length,
            MissingKeys: missingKeys,
            DuplicateKeys: duplicateKeys);
    }
}

/// <summary>
/// Result of checking documentation keys in an error catalog.
/// </summary>
internal sealed record DocumentationKeyCheckReport(
    int TotalErrors,
    IReadOnlyList<DocumentationKeyIssue> MissingKeys,
    IReadOnlyList<DuplicateDocumentationKey> DuplicateKeys)
{
    public bool IsValid => MissingKeys.Count == 0 && DuplicateKeys.Count == 0;
}

/// <summary>
/// Identifies one error definition involved in a documentation-key issue.
/// </summary>
internal sealed record DocumentationKeyIssue(
    string ErrorId,
    int ErrorCode,
    string ErrorName,
    string? DocumentationKey);

/// <summary>
/// Describes one documentation key used by multiple error definitions.
/// </summary>
internal sealed record DuplicateDocumentationKey(
    string DocumentationKey,
    IReadOnlyList<DocumentationKeyIssue> Errors);