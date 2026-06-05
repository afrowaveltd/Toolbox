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
   public async Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default)
   {
      cancellationToken.ThrowIfCancellationRequested();

      Response<ErrorCategoryCatalogDocument> loadResponse =
          await _loader.LoadFromFileAsync(filePath, cancellationToken);

      if(!loadResponse.IsSuccess)
      {
         return Response<ErrorCategoryCatalogProviderPayload>.WithStatus(
             Response<ErrorCategoryCatalogProviderPayload>.Fail(
                 code: GetFirstIssueCode(loadResponse, "CategoryCatalogLoadFailed"),
                 message: GetResponseMessage(loadResponse, "Error category catalog loading failed.")),
             loadResponse.Status);
      }

      if(loadResponse.Data is null)
      {
         return Response<ErrorCategoryCatalogProviderPayload>.Invalid(
             code: "LoadedCategoryCatalogDocumentIsNull",
             message: "Error category catalog loader returned success, but document is null.");
      }

      ErrorCategoryCatalogDocument normalizedDocument =
          _normalizer.Normalize(loadResponse.Data);

      ErrorCatalogValidationResult validationResult =
          _validator.Validate(normalizedDocument);

      if(!validationResult.IsValid)
      {
         return Response<ErrorCategoryCatalogProviderPayload>.Invalid(
             code: "CategoryCatalogValidationFailed",
             message: "Error category catalog validation failed.");
      }

      ErrorCategoryCatalogProviderPayload payload = new()
      {
         Document = normalizedDocument,
         ValidationResult = validationResult
      };

      return Response<ErrorCategoryCatalogProviderPayload>.Ok(payload);
   }

   private static string GetFirstIssueCode(
       Response<ErrorCategoryCatalogDocument> response,
       string fallbackCode)
   {
      return response.Issues.Count > 0
          ? response.Issues[0].Code
          : fallbackCode;
   }

   private static string GetResponseMessage(
       Response<ErrorCategoryCatalogDocument> response,
       string fallbackMessage)
   {
      return string.IsNullOrWhiteSpace(response.Message)
          ? fallbackMessage
          : response.Message;
   }
}