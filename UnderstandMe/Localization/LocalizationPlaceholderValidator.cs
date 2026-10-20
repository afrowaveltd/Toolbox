namespace Afrowave.Toolbox.UnderstandMe.Localization;

/// <summary>
/// Validates that a translated candidate preserves every named placeholder
/// in the source text, including repeated occurrences.
/// </summary>
/// <remarks>
/// This is an opt-in candidate-validation helper, not a required dictionary
/// store or translation provider. It does not change normal offline lookup.
/// Double braces are escaped literal braces; unescaped malformed brace syntax
/// is rejected conservatively.
/// </remarks>
public static class LocalizationPlaceholderValidator
{
    /// <summary>
    /// Checks exact ordinal placeholder names and occurrence counts,
    /// regardless of their order in the translated text.
    /// </summary>
    /// <param name="source">Source-language template.</param>
    /// <param name="candidate">Proposed translated template.</param>
    /// <returns>True when both templates have valid brace syntax and identical named placeholders.</returns>
    public static bool HasMatchingPlaceholders(string source, string candidate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!TryReadPlaceholders(source, out var original) ||
            !TryReadPlaceholders(candidate, out var translated) ||
            original.Count != translated.Count)
            return false;

        foreach (var entry in original)
        {
            if (!translated.TryGetValue(entry.Key, out int actualCount) ||
                actualCount != entry.Value)
                return false;
        }

        return true;
    }

    /// <summary>Checks that a template has well-formed named placeholder syntax.</summary>
    /// <param name="template">Template to check.</param>
    /// <returns>True when its brace syntax is valid, even if no placeholders exist.</returns>
    public static bool HasValidPlaceholderSyntax(string template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return TryReadPlaceholders(template, out _);
    }

    private static bool TryReadPlaceholders(string template, out Dictionary<string, int> occurrences)
    {
        occurrences = new Dictionary<string, int>(StringComparer.Ordinal);

        for (int i = 0; i < template.Length;)
        {
            if (template[i] == '{')
            {
                if (i + 1 < template.Length && template[i + 1] == '{')
                {
                    i += 2;
                    continue;
                }

                int end = template.IndexOf('}', i + 1);
                if (end <= i + 1 || !IsValidName(template.AsSpan(i + 1, end - i - 1)))
                    return false;

                string name = template.Substring(i + 1, end - i - 1);
                occurrences.TryGetValue(name, out int count);
                occurrences[name] = count + 1;
                i = end + 1;
                continue;
            }

            if (template[i] == '}')
            {
                if (i + 1 >= template.Length || template[i + 1] != '}')
                    return false;
                i += 2;
                continue;
            }

            i++;
        }

        return true;
    }

    private static bool IsValidName(ReadOnlySpan<char> name)
    {
        if (name.IsEmpty || !(char.IsLetter(name[0]) || name[0] == '_'))
            return false;

        foreach (char value in name[1..])
        {
            if (!(char.IsLetterOrDigit(value) || value == '_'))
                return false;
        }

        return true;
    }
}
