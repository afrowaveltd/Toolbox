using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Default implementation that loads, normalizes and validates error owner catalog documents.
/// </summary>
public sealed class ErrorOwnerCatalogProvider : IErrorOwnerCatalogProvider
{
   private readonly IErrorOwnerCatalogLoader _loader;
   private readonly ErrorOwnerCatalogDocumentNormalizer _normalizer;
   private readonly IErrorOwnerCatalogValidator _validator;

   /// <summary>
   /// Initializes a new instance of the <see cref="ErrorOwnerCatalogProvider"/> class.
   /// </summary>
   public ErrorOwnerCatalogProvider(
       IErrorOwnerCatalogLoader loader,
       ErrorOwnerCatalogDocumentNormalizer normalizer,
       IErrorOwnerCatalogValidator validator)
   {
      _loader = loader ?? throw new ArgumentNullException(nameof(loader));
      _normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
      _validator = validator ?? throw new ArgumentNullException(nameof(validator));
   }

   /// <inheritdoc />
   public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default)
   {
      return CatalogProviderPipeline.LoadNormalizeValidateAsync(
          filePath,
          cancellationToken,
          _loader.LoadFromFileAsync,
          _normalizer.Normalize,
          _validator.Validate,
          static (document, validationResult) => new ErrorOwnerCatalogProviderPayload
          {
             Document = document,
             ValidationResult = validationResult
          },
          loadFailedCode: "OwnerCatalogLoadFailed",
          loadFailedMessage: "Error owner catalog loading failed.",
          loadedDocumentIsNullCode: "LoadedOwnerCatalogDocumentIsNull",
          loadedDocumentIsNullMessage: "Error owner catalog loader returned success, but document is null.",
          validationFailedCode: "OwnerCatalogValidationFailed",
          validationFailedMessage: "Error owner catalog validation failed.");
   }
}
