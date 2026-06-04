using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Normalization;

/// <summary>
/// Normalizes flexible key-like fields in error owner definitions.
/// </summary>
public sealed class ErrorOwnerDefinitionNormalizer
{
   /// <summary>
   /// Creates a normalized copy of the specified owner definition.
   /// </summary>
   /// <param name="definition">Source owner definition.</param>
   /// <returns>Normalized owner definition copy.</returns>
   public ErrorOwnerDefinition Normalize(ErrorOwnerDefinition definition)
   {
      ArgumentNullException.ThrowIfNull(definition);

      return new ErrorOwnerDefinition
      {
         Name = TextKeyNormalizer.NormalizeKey(definition.Name),
         DisplayName = CreateDisplayName(definition.DisplayName, definition.Name),
         Description = NormalizeNullableDisplayText(definition.Description),

         CodeFrom = definition.CodeFrom,
         CodeTo = definition.CodeTo,
         IsBuiltIn = definition.IsBuiltIn,

         Aliases = NormalizeStringList(definition.Aliases),

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