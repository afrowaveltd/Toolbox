using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Documentation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

/// <summary>
/// Performs read-only checks of the canonical documentation-key format.
/// </summary>
internal sealed class WhenItFailsDocumentationKeyFormatChecker
{
    /// <summary>
    /// Finds non-empty documentation keys that do not use canonical slash-separated kebab-case.
    /// </summary>
    public DocumentationKeyFormatCheckReport Check(ErrorCatalogDocument catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        ErrorDefinition[] errors = catalog.Errors?.ToArray() ?? [];

        InvalidDocumentationKeyFormat[] invalidKeys = errors
            .Where(error => !string.IsNullOrWhiteSpace(error.DocumentationKey))
            .Where(error => !DocumentationKeyFormat.IsCanonical(error.DocumentationKey!))
            .OrderBy(error => error.Code)
            .ThenBy(error => error.Id, StringComparer.OrdinalIgnoreCase)
            .Select(error => new InvalidDocumentationKeyFormat(
                ErrorId: error.Id,
                ErrorCode: error.Code,
                ErrorName: error.Name,
                DocumentationKey: error.DocumentationKey!))
            .ToArray();

        return new DocumentationKeyFormatCheckReport(
            totalErrors: errors.Length,
            invalidKeys: invalidKeys);
    }
}

/// <summary>
/// Result of checking canonical documentation-key formatting.
/// </summary>
internal sealed record DocumentationKeyFormatCheckReport
{
    public DocumentationKeyFormatCheckReport(
        int totalErrors,
        IReadOnlyList<InvalidDocumentationKeyFormat> invalidKeys)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalErrors);
        ArgumentNullException.ThrowIfNull(invalidKeys);

        if (invalidKeys.Count > totalErrors)
        {
            throw new ArgumentException(
                "The number of invalid documentation keys cannot exceed the total number of errors.",
                nameof(invalidKeys));
        }

        TotalErrors = totalErrors;
        InvalidKeys = invalidKeys;
    }

    public int TotalErrors { get; }

    public IReadOnlyList<InvalidDocumentationKeyFormat> InvalidKeys { get; }

    public bool IsValid => InvalidKeys.Count == 0;
}

/// <summary>
/// Identifies one error whose documentation key is not canonical.
/// </summary>
internal sealed record InvalidDocumentationKeyFormat
{
    public InvalidDocumentationKeyFormat(
        string ErrorId,
        int ErrorCode,
        string ErrorName,
        string DocumentationKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ErrorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(ErrorName);
        ArgumentException.ThrowIfNullOrWhiteSpace(DocumentationKey);

        this.ErrorId = ErrorId;
        this.ErrorCode = ErrorCode;
        this.ErrorName = ErrorName;
        this.DocumentationKey = DocumentationKey;
    }

    public string ErrorId { get; }

    public int ErrorCode { get; }

    public string ErrorName { get; }

    public string DocumentationKey { get; }
}
