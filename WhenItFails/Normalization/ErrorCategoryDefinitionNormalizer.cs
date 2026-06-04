using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Normalization;

/// <summary>
/// Normalizes flexible key-like fields in error category definitions.
/// </summary>
public sealed class ErrorCategoryDefinitionNormalizer
{
   /// <summary>
   /// Creates a normalized copy of the specified category definition.
   /// </summary>
   /// <param name="definition">Source category definition.</param>
   /// <returns>Normalized category definition copy.</returns>
   public ErrorCategoryDefinition Normalize(ErrorCategoryDefinition definition)
   {
      ArgumentNullException.ThrowIfNull(definition);

      string normalizedName = TextKeyNormalizer.NormalizeKey(definition.Name);

      return new ErrorCategoryDefinition
      {
         Name = normalizedName,
         DisplayName = DefinitionNormalizationHelper.CreateDisplayName(
              definition.DisplayName,
              definition.Name),
         Description = DefinitionNormalizationHelper.NormalizeNullableDisplayText(
              definition.Description),

         Aliases = DefinitionNormalizationHelper.NormalizeStringList(
              definition.Aliases),
         ParentCategories = DefinitionNormalizationHelper.NormalizeStringList(
              definition.ParentCategories),
         DefaultTags = DefinitionNormalizationHelper.NormalizeStringList(
              definition.DefaultTags),

         DefaultMappings = DefinitionNormalizationHelper.NormalizeDictionary(
              definition.DefaultMappings),
         Metadata = definition.Metadata
      };
   }
}