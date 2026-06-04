using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Default implementation that loads, normalizes, validates and creates runtime error catalogs.
/// </summary>
public sealed class ErrorCatalogProvider : IErrorCatalogProvider
{
   private readonly IErrorCatalogLoader _loader;
   private readonly IErrorCatalogDocumentNormalizer _normalizer;
   private readonly IErrorCatalogValidator _validator;
   private readonly IErrorCatalogFactory _factory;

   /// <summary>
   /// Initializes a new instance of the <see cref="ErrorCatalogProvider"/> class.
   /// </summary>
   public ErrorCatalogProvider(
       IErrorCatalogLoader loader,
       IErrorCatalogDocumentNormalizer normalizer,
       IErrorCatalogValidator validator,
       IErrorCatalogFactory factory)
   {
      _loader = loader ?? throw new ArgumentNullException(nameof(loader));
      _normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
      _validator = validator ?? throw new ArgumentNullException(nameof(validator));
      _factory = factory ?? throw new ArgumentNullException(nameof(factory));
   }

   /// <inheritdoc />
   public async Task<ErrorCatalogProviderResult> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default)
   {
      cancellationToken.ThrowIfCancellationRequested();

      ErrorCatalogLoadResult loadResult =
          await _loader.LoadFromFileAsync(filePath, cancellationToken);

      if(!loadResult.Success)
      {
         return ErrorCatalogProviderResult.Fail(
             errorCode: loadResult.ErrorCode ?? "CatalogLoadFailed",
             errorMessage: loadResult.ErrorMessage ?? "Error catalog loading failed.",
             loadResult: loadResult);
      }

      if(loadResult.Document is null)
      {
         return ErrorCatalogProviderResult.Fail(
             errorCode: "LoadedCatalogDocumentIsNull",
             errorMessage: "Error catalog loader returned success, but document is null.",
             loadResult: loadResult);
      }

      ErrorCatalogDocument normalizedDocument =
          _normalizer.Normalize(loadResult.Document);

      ErrorCatalogValidationResult validationResult =
          _validator.Validate(normalizedDocument);

      if(!validationResult.IsValid)
      {
         return ErrorCatalogProviderResult.Fail(
             errorCode: "CatalogValidationFailed",
             errorMessage: "Error catalog validation failed.",
             loadResult: loadResult,
             validationResult: validationResult);
      }

      IErrorCatalog catalog = _factory.Create(normalizedDocument);

      return ErrorCatalogProviderResult.Ok(
          catalog,
          loadResult,
          validationResult);
   }
}