using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Default implementation that loads, normalizes and validates error category catalog documents.
/// </summary>
public sealed class ErrorCategoryCatalogProvider : IErrorCategoryCatalogProvider
{
   private readonly IErrorCategoryCatalogLoader _loader;
   private readonly ErrorCategoryCatalogDocumentNormalizer _normalizer;
   private readonly IErrorCategoryCatalogValidator _validator;

   /// <summary>
   /// Initializes a new instance of the <see cref="ErrorCategoryCatalogProvider"/> class.
   /// </summary>
   public ErrorCategoryCatalogProvider(
       IErrorCategoryCatalogLoader loader,
       ErrorCategoryCatalogDocumentNormalizer normalizer,
       IErrorCategoryCatalogValidator validator)
   {
      _loader = loader ?? throw new ArgumentNullException(nameof(loader));
      _normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
      _validator = validator ?? throw new ArgumentNullException(nameof(validator));
   }

   /// <inheritdoc />
   public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default)
   {
      return CatalogProviderPipeline.LoadNormalizeValidateAsync(
          filePath,
          cancellationToken,
          _loader.LoadFromFileAsync,
          _normalizer.Normalize,
          _validator.Validate,
          static (document, validationResult) => new ErrorCategoryCatalogProviderPayload
          {
             Document = document,
             ValidationResult = validationResult
          },
          loadFailedCode: "CategoryCatalogLoadFailed",
          loadFailedMessage: "Error category catalog loading failed.",
          loadedDocumentIsNullCode: "LoadedCategoryCatalogDocumentIsNull",
          loadedDocumentIsNullMessage: "Error category catalog loader returned success, but document is null.",
          validationFailedCode: "CategoryCatalogValidationFailed",
          validationFailedMessage: "Error category catalog validation failed.");
   }
}
