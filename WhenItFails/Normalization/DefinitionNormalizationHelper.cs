namespace Afrowave.Toolbox.WhenItFails.Normalization;

/// <summary>
/// Provides shared helper methods for definition normalizers.
/// </summary>
internal static class DefinitionNormalizationHelper
{
    public static string CreateDisplayName(
        string? displayName,
        string? fallbackName)
    {
        string normalizedDisplayName = TextKeyNormalizer.NormalizeDisplayName(displayName);

        if (!string.IsNullOrWhiteSpace(normalizedDisplayName))
        {
            return normalizedDisplayName;
        }

        return TextKeyNormalizer.NormalizeDisplayName(fallbackName);
    }

    public static List<string> NormalizeStringList(IEnumerable<string> values)
    {
        List<string> normalizedValues = new();
        HashSet<string> usedKeys = new(StringComparer.OrdinalIgnoreCase);

        foreach (string value in values)
        {
            string normalizedValue = TextKeyNormalizer.NormalizeKey(value);

            if (string.IsNullOrWhiteSpace(normalizedValue))
            {
                continue;
            }

            if (usedKeys.Add(normalizedValue))
            {
                normalizedValues.Add(normalizedValue);
            }
        }

        return normalizedValues;
    }

    public static Dictionary<string, string> NormalizeDictionary(
        Dictionary<string, string> values)
    {
        Dictionary<string, string> normalizedValues = new(StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<string, string> pair in values)
        {
            string normalizedKey = TextKeyNormalizer.NormalizeKey(pair.Key);
            string normalizedValue = TextKeyNormalizer.NormalizeDisplayName(pair.Value);

            if (string.IsNullOrWhiteSpace(normalizedKey))
            {
                continue;
            }

            normalizedValues.TryAdd(normalizedKey, normalizedValue);
        }

        return normalizedValues;
    }

    public static string? NormalizeNullableDisplayText(string? value)
    {
        string normalizedValue = TextKeyNormalizer.NormalizeDisplayName(value);

        return string.IsNullOrWhiteSpace(normalizedValue)
            ? null
            : normalizedValue;
    }
}