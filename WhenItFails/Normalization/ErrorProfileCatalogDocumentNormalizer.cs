using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Normalization;

/// <summary>
/// Normalizes complete error profile catalog documents.
/// </summary>
/// <remarks>
/// This normalizer creates a normalized copy of the input document.
/// It does not modify the original instance.
/// </remarks>
public sealed class ErrorProfileCatalogDocumentNormalizer
{
    private readonly ErrorProfileDefinitionNormalizer _profileDefinitionNormalizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorProfileCatalogDocumentNormalizer"/> class.
    /// </summary>
    public ErrorProfileCatalogDocumentNormalizer()
        : this(new ErrorProfileDefinitionNormalizer())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorProfileCatalogDocumentNormalizer"/> class.
    /// </summary>
    /// <param name="profileDefinitionNormalizer">Profile definition normalizer.</param>
    public ErrorProfileCatalogDocumentNormalizer(
        ErrorProfileDefinitionNormalizer profileDefinitionNormalizer)
    {
        _profileDefinitionNormalizer = profileDefinitionNormalizer
            ?? throw new ArgumentNullException(nameof(profileDefinitionNormalizer));
    }

    /// <summary>
    /// Creates a normalized copy of the specified profile catalog document.
    /// </summary>
    /// <param name="document">Source profile catalog document.</param>
    /// <returns>Normalized profile catalog document copy.</returns>
    public ErrorProfileCatalogDocument Normalize(ErrorProfileCatalogDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new ErrorProfileCatalogDocument
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

            Profiles = document.Profiles
                .Select(_profileDefinitionNormalizer.Normalize)
                .ToList()
        };
    }
}