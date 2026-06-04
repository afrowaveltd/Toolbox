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
         DisplayName = DefinitionNormalizationHelper.CreateDisplayName(
              definition.DisplayName,
              definition.Name),
         Description = DefinitionNormalizationHelper.NormalizeNullableDisplayText(
              definition.Description),

         CodeFrom = definition.CodeFrom,
         CodeTo = definition.CodeTo,
         IsBuiltIn = definition.IsBuiltIn,

         Aliases = DefinitionNormalizationHelper.NormalizeStringList(
              definition.Aliases),

         DefaultMappings = DefinitionNormalizationHelper.NormalizeDictionary(
              definition.DefaultMappings),
         Metadata = definition.Metadata
      };
   }
}
