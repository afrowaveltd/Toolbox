using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Normalization;

/// <summary>
/// Normalizes flexible key-like fields in complete error catalog documents.
/// </summary>
/// <remarks>
/// This normalizer creates a normalized copy of the input document.
/// It does not modify the original instance.
/// </remarks>
public sealed class ErrorCatalogDocumentNormalizer : IErrorCatalogDocumentNormalizer
{
   private readonly ErrorDefinitionNormalizer _errorDefinitionNormalizer;

   /// <summary>
   /// Initializes a new instance of the <see cref="ErrorCatalogDocumentNormalizer"/> class.
   /// </summary>
   public ErrorCatalogDocumentNormalizer()
       : this(new ErrorDefinitionNormalizer())
   {
   }

   /// <summary>
   /// Initializes a new instance of the <see cref="ErrorCatalogDocumentNormalizer"/> class.
   /// </summary>
   /// <param name="errorDefinitionNormalizer">Error definition normalizer.</param>
   public ErrorCatalogDocumentNormalizer(ErrorDefinitionNormalizer errorDefinitionNormalizer)
   {
      _errorDefinitionNormalizer = errorDefinitionNormalizer
          ?? throw new ArgumentNullException(nameof(errorDefinitionNormalizer));
   }

   /// <summary>
   /// Creates a normalized copy of the specified catalog document.
   /// </summary>
   /// <param name="document">Source catalog document.</param>
   /// <returns>Normalized catalog document copy.</returns>
   public ErrorCatalogDocument Normalize(ErrorCatalogDocument document)
   {
      ArgumentNullException.ThrowIfNull(document);

      return new ErrorCatalogDocument
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

         Errors = document.Errors
              .Select(_errorDefinitionNormalizer.Normalize)
              .ToList()
      };
   }
}