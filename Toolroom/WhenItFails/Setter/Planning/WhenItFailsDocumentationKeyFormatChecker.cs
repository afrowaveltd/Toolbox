using Afrowave.Toolbox.WhenItFails.Definitions;

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
            .Where(error => !IsCanonical(error.DocumentationKey!))
            .OrderBy(error => error.Code)
            .ThenBy(error => error.Id, StringComparer.OrdinalIgnoreCase)
            .Select(error => new InvalidDocumentationKeyFormat(
                ErrorId: error.Id,
                ErrorCode: error.Code,
                ErrorName: error.Name,
                DocumentationKey: error.DocumentationKey!))
            .ToArray();

        return new DocumentationKeyFormatCheckReport(
            TotalErrors: errors.Length,
            InvalidKeys: invalidKeys);
    }

    /// <summary>
    /// Determines whether one non-empty documentation key uses canonical slash-separated kebab-case.
    /// </summary>
    public static bool IsCanonical(string documentationKey)
    {
        ArgumentNullException.ThrowIfNull(documentationKey);

        if (!string.Equals(
                documentationKey,
                documentationKey.Trim(),
                StringComparison.Ordinal))
        {
            return false;
        }

        string[] segments = documentationKey.Split('/');
        if (segments.Length < 2)
        {
            return false;
        }

        return segments.All(IsCanonicalSegment);
    }

    private static bool IsCanonicalSegment(string segment)
    {
        if (segment.Length == 0 || segment[0] == '-' || segment[^1] == '-')
        {
            return false;
        }

        bool previousWasHyphen = false;

        foreach (char character in segment)
        {
            if (character == '-')
            {
                if (previousWasHyphen)
                {
                    return false;
                }

                previousWasHyphen = true;
                continue;
            }

            if (character is not (>= 'a' and <= 'z') &&
                character is not (>= '0' and <= '9'))
            {
                return false;
            }

            previousWasHyphen = false;
        }

        return true;
    }
}

/// <summary>
/// Result of checking canonical documentation-key formatting.
/// </summary>
internal sealed record DocumentationKeyFormatCheckReport(
    int TotalErrors,
    IReadOnlyList<InvalidDocumentationKeyFormat> InvalidKeys)
{
    public bool IsValid => InvalidKeys.Count == 0;
}

/// <summary>
/// Identifies one error whose documentation key is not canonical.
/// </summary>
internal sealed record InvalidDocumentationKeyFormat(
    string ErrorId,
    int ErrorCode,
    string ErrorName,
    string DocumentationKey);