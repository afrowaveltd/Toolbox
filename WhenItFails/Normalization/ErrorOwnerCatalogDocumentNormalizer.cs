using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Normalization;

/// <summary>
/// Normalizes complete error owner catalog documents.
/// </summary>
/// <remarks>
/// This normalizer creates a normalized copy of the input document.
/// It does not modify the original instance.
/// </remarks>
public sealed class ErrorOwnerCatalogDocumentNormalizer
{
    private readonly ErrorOwnerDefinitionNormalizer _ownerDefinitionNormalizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorOwnerCatalogDocumentNormalizer"/> class.
    /// </summary>
    public ErrorOwnerCatalogDocumentNormalizer()
        : this(new ErrorOwnerDefinitionNormalizer())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorOwnerCatalogDocumentNormalizer"/> class.
    /// </summary>
    /// <param name="ownerDefinitionNormalizer">Owner definition normalizer.</param>
    public ErrorOwnerCatalogDocumentNormalizer(
        ErrorOwnerDefinitionNormalizer ownerDefinitionNormalizer)
    {
        _ownerDefinitionNormalizer = ownerDefinitionNormalizer
            ?? throw new ArgumentNullException(nameof(ownerDefinitionNormalizer));
    }

    /// <summary>
    /// Creates a normalized copy of the specified owner catalog document.
    /// </summary>
    /// <param name="document">Source owner catalog document.</param>
    /// <returns>Normalized owner catalog document copy.</returns>
    public ErrorOwnerCatalogDocument Normalize(ErrorOwnerCatalogDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new ErrorOwnerCatalogDocument
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

            Owners = document.Owners
                .Select(_ownerDefinitionNormalizer.Normalize)
                .ToList()
        };
    }
}