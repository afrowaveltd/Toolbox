using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Normalization;

/// <summary>
/// Normalizes complete error category catalog documents.
/// </summary>
/// <remarks>
/// This normalizer creates a normalized copy of the input document.
/// It does not modify the original instance.
/// </remarks>
public sealed class ErrorCategoryCatalogDocumentNormalizer
{
    private readonly ErrorCategoryDefinitionNormalizer _categoryDefinitionNormalizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorCategoryCatalogDocumentNormalizer"/> class.
    /// </summary>
    public ErrorCategoryCatalogDocumentNormalizer()
        : this(new ErrorCategoryDefinitionNormalizer())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorCategoryCatalogDocumentNormalizer"/> class.
    /// </summary>
    /// <param name="categoryDefinitionNormalizer">Category definition normalizer.</param>
    public ErrorCategoryCatalogDocumentNormalizer(
        ErrorCategoryDefinitionNormalizer categoryDefinitionNormalizer)
    {
        _categoryDefinitionNormalizer = categoryDefinitionNormalizer
            ?? throw new ArgumentNullException(nameof(categoryDefinitionNormalizer));
    }

    /// <summary>
    /// Creates a normalized copy of the specified category catalog document.
    /// </summary>
    /// <param name="document">Source category catalog document.</param>
    /// <returns>Normalized category catalog document copy.</returns>
    public ErrorCategoryCatalogDocument Normalize(ErrorCategoryCatalogDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new ErrorCategoryCatalogDocument
        {
            SchemaVersion = TextKeyNormalizer.NormalizeDisplayName(document.SchemaVersion),
            CatalogId = TextKeyNormalizer.NormalizeDisplayName(document.CatalogId),
            CatalogName = TextKeyNormalizer.NormalizeDisplayName(document.CatalogName),
            Description = DefinitionNormalizationHelper.NormalizeNullableDisplayText(
                document.Description),
            Language = TextKeyNormalizer.NormalizeDisplayName(document.Language),

            SourceCatalogId = DefinitionNormalizationHelper.NormalizeNullableDisplayText(
                document.SourceCatalogId),
            SourceCatalogVersion = DefinitionNormalizationHelper.NormalizeNullableDisplayText(
                document.SourceCatalogVersion),
            IsShadowCopy = document.IsShadowCopy,

            Tags = DefinitionNormalizationHelper.NormalizeStringList(document.Tags),
            Metadata = document.Metadata,

            Categories = document.Categories
                .Select(_categoryDefinitionNormalizer.Normalize)
                .ToList()
        };
    }
}