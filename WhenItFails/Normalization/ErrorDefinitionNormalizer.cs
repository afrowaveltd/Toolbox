using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Normalization;

/// <summary>
/// Normalizes flexible key-like fields in error definitions.
/// </summary>
/// <remarks>
/// This normalizer does not translate text and does not change human-facing
/// message fields such as Title, Message or DeveloperHint.
///
/// It only normalizes identity and classification fields that are intended
/// to behave as stable keys.
/// </remarks>
public sealed class ErrorDefinitionNormalizer
{
   /// <summary>
   /// Creates a normalized copy of the specified error definition.
   /// </summary>
   /// <param name="definition">Source error definition.</param>
   /// <returns>Normalized error definition copy.</returns>
   public ErrorDefinition Normalize(ErrorDefinition definition)
   {
      ArgumentNullException.ThrowIfNull(definition);

      return new ErrorDefinition
      {
         Id = TextKeyNormalizer.NormalizeKey(definition.Id),
         Code = definition.Code,
         Name = TextKeyNormalizer.NormalizeKey(definition.Name),

         Owner = TextKeyNormalizer.NormalizeKey(definition.Owner),
         CodePrefix = TextKeyNormalizer.NormalizeKey(definition.CodePrefix),
         CodeGroup = TextKeyNormalizer.NormalizeKey(definition.CodeGroup),

         PrimaryCategory = TextKeyNormalizer.NormalizeKey(definition.PrimaryCategory),
         Categories = NormalizeStringList(definition.Categories),
         Subcategories = NormalizeStringList(definition.Subcategories),

         Title = TextKeyNormalizer.NormalizeDisplayName(definition.Title),
         Message = definition.Message?.Trim() ?? string.Empty,
         DefaultSeverity = TextKeyNormalizer.NormalizeDisplayName(definition.DefaultSeverity),

         DeveloperHint = NormalizeNullableDisplayText(definition.DeveloperHint),
         DocumentationKey = NormalizeNullableDisplayText(definition.DocumentationKey),

         Tags = NormalizeStringList(definition.Tags),
         Metadata = definition.Metadata
      };
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

   private static string? NormalizeNullableDisplayText(string? value)
   {
      string normalizedValue = TextKeyNormalizer.NormalizeDisplayName(value);

      return string.IsNullOrWhiteSpace(normalizedValue)
          ? null
          : normalizedValue;
   }
}