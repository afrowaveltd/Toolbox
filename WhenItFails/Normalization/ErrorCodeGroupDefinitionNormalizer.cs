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
         DisplayName = DefinitionNormalizationHelper.CreateDisplayName(
              definition.DisplayName,
              definition.Name),
         CodePrefix = TextKeyNormalizer.NormalizeKey(definition.CodePrefix),
         CodeFrom = definition.CodeFrom,
         CodeTo = definition.CodeTo,
         Description = DefinitionNormalizationHelper.NormalizeNullableDisplayText(
              definition.Description),

         DefaultCategories = DefinitionNormalizationHelper.NormalizeStringList(
              definition.DefaultCategories),
         DefaultTags = DefinitionNormalizationHelper.NormalizeStringList(
              definition.DefaultTags),

         DefaultMappings = DefinitionNormalizationHelper.NormalizeDictionary(
              definition.DefaultMappings),
         Metadata = definition.Metadata
      };
   }
}