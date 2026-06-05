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
   public async Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default)
   {
      cancellationToken.ThrowIfCancellationRequested();

      Response<ErrorCodeGroupCatalogDocument> loadResponse =
          await _loader.LoadFromFileAsync(filePath, cancellationToken);

      if(!loadResponse.IsSuccess)
      {
         return Response<ErrorCodeGroupCatalogProviderPayload>.WithStatus(
             Response<ErrorCodeGroupCatalogProviderPayload>.Fail(
                 code: GetFirstIssueCode(loadResponse, "CodeGroupCatalogLoadFailed"),
                 message: GetResponseMessage(loadResponse, "Error code group catalog loading failed.")),
             loadResponse.Status);
      }

      if(loadResponse.Data is null)
      {
         return Response<ErrorCodeGroupCatalogProviderPayload>.Invalid(
             code: "LoadedCodeGroupCatalogDocumentIsNull",
             message: "Error code group catalog loader returned success, but document is null.");
      }

      ErrorCodeGroupCatalogDocument normalizedDocument =
          _normalizer.Normalize(loadResponse.Data);

      ErrorCatalogValidationResult validationResult =
          _validator.Validate(normalizedDocument);

      if(!validationResult.IsValid)
      {
         return Response<ErrorCodeGroupCatalogProviderPayload>.Invalid(
             code: "CodeGroupCatalogValidationFailed",
             message: "Error code group catalog validation failed.");
      }

      ErrorCodeGroupCatalogProviderPayload payload = new()
      {
         Document = normalizedDocument,
         ValidationResult = validationResult
      };

      return Response<ErrorCodeGroupCatalogProviderPayload>.Ok(payload);
   }

   private static string GetFirstIssueCode(
       Response<ErrorCodeGroupCatalogDocument> response,
       string fallbackCode)
   {
      return response.Issues.Count > 0
          ? response.Issues[0].Code
          : fallbackCode;
   }

   private static string GetResponseMessage(
       Response<ErrorCodeGroupCatalogDocument> response,
       string fallbackMessage)
   {
      return string.IsNullOrWhiteSpace(response.Message)
          ? fallbackMessage
          : response.Message;
   }
}