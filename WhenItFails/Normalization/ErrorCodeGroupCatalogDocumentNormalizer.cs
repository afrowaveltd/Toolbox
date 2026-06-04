using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Normalization;

/// <summary>
/// Normalizes complete error code group catalog documents.
/// </summary>
/// <remarks>
/// This normalizer creates a normalized copy of the input document.
/// It does not modify the original instance.
/// </remarks>
public sealed class ErrorCodeGroupCatalogDocumentNormalizer
{
    private readonly ErrorCodeGroupDefinitionNormalizer _codeGroupDefinitionNormalizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorCodeGroupCatalogDocumentNormalizer"/> class.
    /// </summary>
    public ErrorCodeGroupCatalogDocumentNormalizer()
        : this(new ErrorCodeGroupDefinitionNormalizer())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorCodeGroupCatalogDocumentNormalizer"/> class.
    /// </summary>
    /// <param name="codeGroupDefinitionNormalizer">Code group definition normalizer.</param>
    public ErrorCodeGroupCatalogDocumentNormalizer(
        ErrorCodeGroupDefinitionNormalizer codeGroupDefinitionNormalizer)
    {
        _codeGroupDefinitionNormalizer = codeGroupDefinitionNormalizer
            ?? throw new ArgumentNullException(nameof(codeGroupDefinitionNormalizer));
    }

    /// <summary>
    /// Creates a normalized copy of the specified code group catalog document.
    /// </summary>
    /// <param name="document">Source code group catalog document.</param>
    /// <returns>Normalized code group catalog document copy.</returns>
    public ErrorCodeGroupCatalogDocument Normalize(ErrorCodeGroupCatalogDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new ErrorCodeGroupCatalogDocument
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

            CodeGroups = document.CodeGroups
                .Select(_codeGroupDefinitionNormalizer.Normalize)
                .ToList()
        };
    }
}