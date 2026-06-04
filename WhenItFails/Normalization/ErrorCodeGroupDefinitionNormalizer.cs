using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Normalization;

/// <summary>
/// Normalizes flexible key-like fields in error code group definitions.
/// </summary>
public sealed class ErrorCodeGroupDefinitionNormalizer
{
   /// <summary>
   /// Creates a normalized copy of the specified code group definition.
   /// </summary>
   /// <param name="definition">Source code group definition.</param>
   /// <returns>Normalized code group definition copy.</returns>
   public ErrorCodeGroupDefinition Normalize(ErrorCodeGroupDefinition definition)
   {
      ArgumentNullException.ThrowIfNull(definition);

      return new ErrorCodeGroupDefinition
      {
         Name = TextKeyNormalizer.NormalizeKey(definition.Name),
         DisplayName = CreateDisplayName(definition.DisplayName, definition.Name),
         CodePrefix = TextKeyNormalizer.NormalizeKey(definition.CodePrefix),
         CodeFrom = definition.CodeFrom,
         CodeTo = definition.CodeTo,
         Description = NormalizeNullableDisplayText(definition.Description),

         DefaultCategories = NormalizeStringList(definition.DefaultCategories),
         DefaultTags = NormalizeStringList(definition.DefaultTags),

         DefaultMappings = NormalizeDictionary(definition.DefaultMappings),
         Metadata = definition.Metadata
      };
   }

   private static string CreateDisplayName(
       string? displayName,
       string? fallbackName)
   {
      string normalizedDisplayName = TextKeyNormalizer.NormalizeDisplayName(displayName);

      if(!string.IsNullOrWhiteSpace(normalizedDisplayName))
      {
         return normalizedDisplayName;
      }

      return TextKeyNormalizer.NormalizeDisplayName(fallbackName);
   }

   private static List<string> NormalizeStringList(IEnumerable<string> values)
   {
      List<string> normalizedValues = new();
      HashSet<string> usedKeys = new(StringComparer.OrdinalIgnoreCase);

      foreach(string value in values)
      {
         string normalizedValue = TextKeyNormalizer.NormalizeKey(value);

         if(string.IsNullOrWhiteSpace(normalizedValue))
         {
            continue;
         }

         if(usedKeys.Add(normalizedValue))
         {
            normalizedValues.Add(normalizedValue);
         }
      }

      return normalizedValues;
   }

   private static Dictionary<string, string> NormalizeDictionary(
       Dictionary<string, string> values)
   {
      Dictionary<string, string> normalizedValues = new(StringComparer.OrdinalIgnoreCase);

      foreach(KeyValuePair<string, string> pair in values)
      {
         string normalizedKey = TextKeyNormalizer.NormalizeKey(pair.Key);
         string normalizedValue = TextKeyNormalizer.NormalizeDisplayName(pair.Value);

         if(string.IsNullOrWhiteSpace(normalizedKey))
         {
            continue;
         }

         normalizedValues.TryAdd(normalizedKey, normalizedValue);
      }

      return normalizedValues;
   }

   private static string? NormalizeNullableDisplayText(string? value)
   {
      string normalizedValue = TextKeyNormalizer.NormalizeDisplayName(value);

      return string.IsNullOrWhiteSpace(normalizedValue)
          ? null
          : normalizedValue;
   }
}