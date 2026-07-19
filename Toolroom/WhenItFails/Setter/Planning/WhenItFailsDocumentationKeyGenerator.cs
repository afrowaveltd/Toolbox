using System.Globalization;
using System.Text;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

/// <summary>
/// Generates canonical, unique documentation keys for error definitions.
/// </summary>
internal sealed class WhenItFailsDocumentationKeyGenerator
{
    /// <summary>
    /// Generates the first available documentation key for one category and title.
    /// </summary>
    /// <param name="categoryName">Canonical category name or alias-like input.</param>
    /// <param name="title">Human-readable error title.</param>
    /// <param name="existingKeys">Documentation keys that are already in use.</param>
    /// <returns>A canonical, unique documentation key.</returns>
    public string Generate(
        string categoryName,
        string title,
        IEnumerable<string?> existingKeys)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(categoryName);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(existingKeys);

        string categorySegment = ToSegment(categoryName);
        string titleSegment = ToSegment(title);

        if (categorySegment.Length == 0)
        {
            throw new ArgumentException(
                "Category name must contain at least one ASCII letter or digit.",
                nameof(categoryName));
        }

        if (titleSegment.Length == 0)
        {
            throw new ArgumentException(
                "Title must contain at least one ASCII letter or digit.",
                nameof(title));
        }

        string baseKey = $"when-it-fails/errors/{categorySegment}/{titleSegment}";
        HashSet<string> usedKeys = existingKeys
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Select(key => key!.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!usedKeys.Contains(baseKey))
        {
            return baseKey;
        }

        for (int suffix = 2; ; suffix++)
        {
            string candidate = $"{baseKey}-{suffix}";
            if (!usedKeys.Contains(candidate))
            {
                return candidate;
            }
        }
    }

    /// <summary>
    /// Converts one human-readable value to a canonical documentation-key segment.
    /// </summary>
    public static string ToSegment(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        string decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        StringBuilder builder = new();
        bool pendingSeparator = false;

        foreach (char character in decomposed)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsAsciiLetterOrDigit(character))
            {
                if (pendingSeparator && builder.Length > 0)
                {
                    builder.Append('-');
                }

                builder.Append(char.ToLowerInvariant(character));
                pendingSeparator = false;
                continue;
            }

            pendingSeparator = builder.Length > 0;
        }

        return builder.ToString();
    }
}
