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

      Response<TDocument> loadResponse =
          await loadAsync(filePath, cancellationToken);

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

      TDocument normalizedDocument = normalize(loadResponse.Data);

      ErrorCatalogValidationResult validationResult = validate(normalizedDocument);

      if(!validationResult.IsValid)
      {
         return Response<TPayload>.Invalid(
             code: validationFailedCode,
             message: validationFailedMessage);
      }

      TPayload payload = createPayload(normalizedDocument, validationResult);

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
