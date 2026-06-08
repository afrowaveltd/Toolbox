using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Default implementation that loads, normalizes and validates error code group catalog documents.
/// </summary>
public sealed class ErrorCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
{
   private readonly IErrorCodeGroupCatalogLoader _loader;
   private readonly ErrorCodeGroupCatalogDocumentNormalizer _normalizer;
   private readonly IErrorCodeGroupCatalogValidator _validator;

   /// <summary>
   /// Initializes a new instance of the <see cref="ErrorCodeGroupCatalogProvider"/> class.
   /// </summary>
   public ErrorCodeGroupCatalogProvider(
       IErrorCodeGroupCatalogLoader loader,
       ErrorCodeGroupCatalogDocumentNormalizer normalizer,
       IErrorCodeGroupCatalogValidator validator)
   {
      _loader = loader ?? throw new ArgumentNullException(nameof(loader));
      _normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
      _validator = validator ?? throw new ArgumentNullException(nameof(validator));
   }

   /// <inheritdoc />
   public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default)
   {
      return CatalogProviderPipeline.LoadNormalizeValidateAsync(
          filePath,
          cancellationToken,
          _loader.LoadFromFileAsync,
          _normalizer.Normalize,
          _validator.Validate,
          static (document, validationResult) => new ErrorCodeGroupCatalogProviderPayload
          {
             Document = document,
             ValidationResult = validationResult
          },
          loadFailedCode: "CodeGroupCatalogLoadFailed",
          loadFailedMessage: "Error code group catalog loading failed.",
          loadedDocumentIsNullCode: "LoadedCodeGroupCatalogDocumentIsNull",
          loadedDocumentIsNullMessage: "Error code group catalog loader returned success, but document is null.",
          validationFailedCode: "CodeGroupCatalogValidationFailed",
          validationFailedMessage: "Error code group catalog validation failed.");
   }
}
