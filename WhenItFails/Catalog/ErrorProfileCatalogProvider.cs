using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Default implementation that loads, normalizes and validates error profile catalog documents.
/// </summary>
public sealed class ErrorProfileCatalogProvider : IErrorProfileCatalogProvider
{
   private readonly IErrorProfileCatalogLoader _loader;
   private readonly ErrorProfileCatalogDocumentNormalizer _normalizer;
   private readonly IErrorProfileCatalogValidator _validator;

   /// <summary>
   /// Initializes a new instance of the <see cref="ErrorProfileCatalogProvider"/> class.
   /// </summary>
   public ErrorProfileCatalogProvider(
       IErrorProfileCatalogLoader loader,
       ErrorProfileCatalogDocumentNormalizer normalizer,
       IErrorProfileCatalogValidator validator)
   {
      _loader = loader ?? throw new ArgumentNullException(nameof(loader));
      _normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
      _validator = validator ?? throw new ArgumentNullException(nameof(validator));
   }

   /// <inheritdoc />
   public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default)
   {
      return CatalogProviderPipeline.LoadNormalizeValidateAsync(
          filePath,
          cancellationToken,
          _loader.LoadFromFileAsync,
          _normalizer.Normalize,
          _validator.Validate,
          static (document, validationResult) => new ErrorProfileCatalogProviderPayload
          {
             Document = document,
             ValidationResult = validationResult
          },
          loadFailedCode: "ProfileCatalogLoadFailed",
          loadFailedMessage: "Error profile catalog loading failed.",
          loadedDocumentIsNullCode: "LoadedProfileCatalogDocumentIsNull",
          loadedDocumentIsNullMessage: "Error profile catalog loader returned success, but document is null.",
          validationFailedCode: "ProfileCatalogValidationFailed",
          validationFailedMessage: "Error profile catalog validation failed.");
   }
}
