using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Internal helper for catalog providers that follow the load, normalize, validate and return payload flow.
/// </summary>
internal static class CatalogProviderPipeline
{
   public static async Task<Response<TPayload>> LoadNormalizeValidateAsync<TDocument, TPayload>(
       string filePath,
       CancellationToken cancellationToken,
       Func<string, CancellationToken, Task<Response<TDocument>>> loadAsync,
       Func<TDocument, TDocument> normalize,
       Func<TDocument, ErrorCatalogValidationResult> validate,
       Func<TDocument, ErrorCatalogValidationResult, TPayload> createPayload,
       string loadFailedCode,
       string loadFailedMessage,
       string loadedDocumentIsNullCode,
       string loadedDocumentIsNullMessage,
       string validationFailedCode,
       string validationFailedMessage)
       where TDocument : class
   {
      ArgumentNullException.ThrowIfNull(loadAsync);
      ArgumentNullException.ThrowIfNull(normalize);
      ArgumentNullException.ThrowIfNull(validate);
      ArgumentNullException.ThrowIfNull(createPayload);

      cancellationToken.ThrowIfCancellationRequested();

      Response<TDocument>? loadResponse =
          await loadAsync(filePath, cancellationToken);

      if(loadResponse is null)
      {
         return Response<TPayload>.Invalid(
             code: "WIF_CATALOG_PIPELINE_LOADER_RESPONSE_NULL",
             message: "The catalog provider pipeline loader returned a null response.");
      }

      if(!loadResponse.IsSuccess)
      {
         return Response<TPayload>.WithStatus(
             Response<TPayload>.Fail(
                 code: GetFirstIssueCode(loadResponse, loadFailedCode),
                 message: GetResponseMessage(loadResponse, loadFailedMessage)),
             loadResponse.Status);
      }

      if(loadResponse.Data is null)
      {
         return Response<TPayload>.Invalid(
             code: loadedDocumentIsNullCode,
             message: loadedDocumentIsNullMessage);
      }

      TDocument? normalizedDocument = normalize(loadResponse.Data);

      if(normalizedDocument is null)
      {
         return Response<TPayload>.Invalid(
             code: "WIF_CATALOG_PIPELINE_NORMALIZER_RESULT_NULL",
             message: "The catalog provider pipeline normalizer returned a null result.");
      }

      ErrorCatalogValidationResult? validationResult = validate(normalizedDocument);

      if(validationResult is null)
      {
         return Response<TPayload>.Invalid(
             code: "WIF_CATALOG_PIPELINE_VALIDATOR_RESULT_NULL",
             message: "The catalog provider pipeline validator returned a null result.");
      }

      if(!validationResult.IsValid)
      {
         return Response<TPayload>.Invalid(
             code: validationFailedCode,
             message: validationFailedMessage);
      }

      TPayload? payload = createPayload(normalizedDocument, validationResult);

      if(payload is null)
      {
         return Response<TPayload>.Invalid(
             code: "WIF_CATALOG_PIPELINE_PAYLOAD_NULL",
             message: "The catalog provider pipeline payload factory returned a null result.");
      }

      return Response<TPayload>.Ok(payload);
   }

   private static string GetFirstIssueCode<TDocument>(
       Response<TDocument> response,
       string fallbackCode)
   {
      return response.Issues.Count > 0
          ? response.Issues[0].Code
          : fallbackCode;
   }

   private static string GetResponseMessage<TDocument>(
       Response<TDocument> response,
       string fallbackMessage)
   {
      return string.IsNullOrWhiteSpace(response.Message)
          ? fallbackMessage
          : response.Message;
   }
}
