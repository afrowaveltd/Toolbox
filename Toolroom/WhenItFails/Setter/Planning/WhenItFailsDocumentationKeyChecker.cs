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
        if (errors.Any(error => error is null))
        {
            throw new ArgumentException(
                "Error catalog entries cannot be null.",
                nameof(catalog));
        }

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
internal sealed record DocumentationKeyCheckReport
{
    public DocumentationKeyCheckReport(
        int TotalErrors,
        IReadOnlyList<DocumentationKeyIssue> MissingKeys,
        IReadOnlyList<DuplicateDocumentationKey> DuplicateKeys)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(TotalErrors);
        ArgumentNullException.ThrowIfNull(MissingKeys);
        ArgumentNullException.ThrowIfNull(DuplicateKeys);

        if (MissingKeys.Any(issue => issue is null))
        {
            throw new ArgumentException(
                "Missing documentation key entries cannot be null.",
                nameof(MissingKeys));
        }

        if (DuplicateKeys.Any(issue => issue is null))
        {
            throw new ArgumentException(
                "Duplicate documentation key entries cannot be null.",
                nameof(DuplicateKeys));
        }

        if (MissingKeys.Count > TotalErrors)
        {
            throw new ArgumentException(
                "The number of missing documentation keys cannot exceed the total number of errors.",
                nameof(MissingKeys));
        }

        if (DuplicateKeys.Count > TotalErrors)
        {
            throw new ArgumentException(
                "The number of duplicate documentation keys cannot exceed the total number of errors.",
                nameof(DuplicateKeys));
        }

        this.TotalErrors = TotalErrors;
        this.MissingKeys = MissingKeys;
        this.DuplicateKeys = DuplicateKeys;
    }

    public int TotalErrors { get; }

    public IReadOnlyList<DocumentationKeyIssue> MissingKeys { get; }

    public IReadOnlyList<DuplicateDocumentationKey> DuplicateKeys { get; }

    public bool IsValid => MissingKeys.Count == 0 && DuplicateKeys.Count == 0;
}

/// <summary>
/// Identifies one error definition involved in a documentation-key issue.
/// </summary>
internal sealed record DocumentationKeyIssue
{
    public DocumentationKeyIssue(
        string ErrorId,
        int ErrorCode,
        string ErrorName,
        string? DocumentationKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ErrorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(ErrorName);

        this.ErrorId = ErrorId;
        this.ErrorCode = ErrorCode;
        this.ErrorName = ErrorName;
        this.DocumentationKey = DocumentationKey;
    }

    public string ErrorId { get; }

    public int ErrorCode { get; }

    public string ErrorName { get; }

    public string? DocumentationKey { get; }
}

/// <summary>
/// Describes one documentation key used by multiple error definitions.
/// </summary>
internal sealed record DuplicateDocumentationKey
{
    public DuplicateDocumentationKey(
        string DocumentationKey,
        IReadOnlyList<DocumentationKeyIssue> Errors)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(DocumentationKey);
        ArgumentNullException.ThrowIfNull(Errors);

        if (Errors.Count < 2)
        {
            throw new ArgumentException(
                "A duplicate documentation key must reference at least two errors.",
                nameof(Errors));
        }

        string normalizedDocumentationKey = DocumentationKey.Trim();
        if (Errors.Any(error =>
                error is null ||
                !string.Equals(
                    error.DocumentationKey?.Trim(),
                    normalizedDocumentationKey,
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException(
                "Every error in a duplicate documentation key group must use the group's documentation key.",
                nameof(Errors));
        }

        this.DocumentationKey = DocumentationKey;
        this.Errors = Errors;
    }

    public string DocumentationKey { get; }

    public IReadOnlyList<DocumentationKeyIssue> Errors { get; }
}
