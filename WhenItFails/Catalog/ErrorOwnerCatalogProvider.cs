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
   public async Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default)
   {
      cancellationToken.ThrowIfCancellationRequested();

      Response<ErrorOwnerCatalogDocument> loadResponse =
          await _loader.LoadFromFileAsync(filePath, cancellationToken);

      if(!loadResponse.IsSuccess)
      {
         return Response<ErrorOwnerCatalogProviderPayload>.WithStatus(
             Response<ErrorOwnerCatalogProviderPayload>.Fail(
                 code: GetFirstIssueCode(loadResponse, "OwnerCatalogLoadFailed"),
                 message: GetResponseMessage(loadResponse, "Error owner catalog loading failed.")),
             loadResponse.Status);
      }

      if(loadResponse.Data is null)
      {
         return Response<ErrorOwnerCatalogProviderPayload>.Invalid(
             code: "LoadedOwnerCatalogDocumentIsNull",
             message: "Error owner catalog loader returned success, but document is null.");
      }

      ErrorOwnerCatalogDocument normalizedDocument =
          _normalizer.Normalize(loadResponse.Data);

      ErrorCatalogValidationResult validationResult =
          _validator.Validate(normalizedDocument);

      if(!validationResult.IsValid)
      {
         return Response<ErrorOwnerCatalogProviderPayload>.Invalid(
             code: "OwnerCatalogValidationFailed",
             message: "Error owner catalog validation failed.");
      }

      ErrorOwnerCatalogProviderPayload payload = new()
      {
         Document = normalizedDocument,
         ValidationResult = validationResult
      };

      return Response<ErrorOwnerCatalogProviderPayload>.Ok(payload);
   }

   private static string GetFirstIssueCode(
       Response<ErrorOwnerCatalogDocument> response,
       string fallbackCode)
   {
      return response.Issues.Count > 0
          ? response.Issues[0].Code
          : fallbackCode;
   }

   private static string GetResponseMessage(
       Response<ErrorOwnerCatalogDocument> response,
       string fallbackMessage)
   {
      return string.IsNullOrWhiteSpace(response.Message)
          ? fallbackMessage
          : response.Message;
   }
}