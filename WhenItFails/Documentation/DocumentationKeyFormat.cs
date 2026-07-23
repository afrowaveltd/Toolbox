namespace Afrowave.Toolbox.WhenItFails.Documentation;

/// <summary>
/// Validates canonical documentation-key formatting.
/// </summary>
public static class DocumentationKeyFormat
{
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
