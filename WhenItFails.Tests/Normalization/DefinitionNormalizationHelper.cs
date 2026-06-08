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

    public static List<string> NormalizeStringList(IEnumerable<string>? values)
    {
        List<string> normalizedValues = new();

        if (values is null)
        {
            return normalizedValues;
        }

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
        Dictionary<string, string>? values)
    {
        Dictionary<string, string> normalizedValues = new(StringComparer.OrdinalIgnoreCase);

        if (values is null)
        {
            return normalizedValues;
        }

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
public sealed class DefinitionNormalizationHelperNullSafetyTests
{
   [Fact]
   public void NormalizeStringList_ShouldReturnEmptyList_WhenValuesAreNull()
   {
      List<string> result = DefinitionNormalizationHelper.NormalizeStringList(null);

      Assert.Empty(result);
   }

   [Fact]
   public void NormalizeDictionary_ShouldReturnEmptyDictionary_WhenValuesAreNull()
   {
      Dictionary<string, string> result = DefinitionNormalizationHelper.NormalizeDictionary(null);

      Assert.Empty(result);
   }
}
