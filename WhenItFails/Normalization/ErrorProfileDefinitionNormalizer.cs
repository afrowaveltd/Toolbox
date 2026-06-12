using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Normalization;

/// <summary>
/// Normalizes flexible key-like fields in error profile definitions.
/// </summary>
public sealed class ErrorProfileDefinitionNormalizer
{
    /// <summary>
    /// Creates a normalized copy of the specified profile definition.
    /// </summary>
    /// <param name="definition">Source profile definition.</param>
    /// <returns>Normalized profile definition copy.</returns>
    public ErrorProfileDefinition Normalize(ErrorProfileDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return new ErrorProfileDefinition
        {
            Name = TextKeyNormalizer.NormalizeKey(definition.Name),
            DisplayName = DefinitionNormalizationHelper.CreateDisplayName(
                definition.DisplayName,
                definition.Name),
            Description = DefinitionNormalizationHelper.NormalizeNullableDisplayText(
                definition.Description),

            Source = TextKeyNormalizer.NormalizeDisplayName(definition.Source),

            IncludeOwners = DefinitionNormalizationHelper.NormalizeStringList(
                definition.IncludeOwners),
            IncludeCodeGroups = DefinitionNormalizationHelper.NormalizeStringList(
                definition.IncludeCodeGroups),
            IncludeCategories = DefinitionNormalizationHelper.NormalizeStringList(
                definition.IncludeCategories),
            IncludeSubcategories = DefinitionNormalizationHelper.NormalizeStringList(
                definition.IncludeSubcategories),
            IncludeTags = DefinitionNormalizationHelper.NormalizeStringList(
                definition.IncludeTags),
            IncludeErrors = DefinitionNormalizationHelper.NormalizeStringList(
                definition.IncludeErrors),
            ExcludeTags = DefinitionNormalizationHelper.NormalizeStringList(
                definition.ExcludeTags),
            ExcludeErrors = DefinitionNormalizationHelper.NormalizeStringList(
                definition.ExcludeErrors),

            DefaultMappings = DefinitionNormalizationHelper.NormalizeDictionary(
                definition.DefaultMappings),
            Metadata = definition.Metadata
        };
    }
}