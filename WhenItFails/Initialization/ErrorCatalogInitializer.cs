using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Initialization;

/// <summary>
/// Default coordinator for the complete WhenItFails catalog initialization flow.
/// </summary>
public sealed class ErrorCatalogInitializer : IErrorCatalogInitializer
{
   private readonly IJsonsBootstrapper _bootstrapper;
   private readonly IErrorCatalogContextProvider _contextProvider;
   private readonly IErrorCatalogContextStore _contextStore;

   /// <summary>
   /// Initializes a new instance of the
   /// <see cref="ErrorCatalogInitializer"/> class.
   /// </summary>
   public ErrorCatalogInitializer(
       IJsonsBootstrapper bootstrapper,
       IErrorCatalogContextProvider contextProvider,
       IErrorCatalogContextStore contextStore)
   {
      _bootstrapper = bootstrapper
          ?? throw new ArgumentNullException(nameof(bootstrapper));

      _contextProvider = contextProvider
          ?? throw new ArgumentNullException(nameof(contextProvider));

      _contextStore = contextStore
          ?? throw new ArgumentNullException(nameof(contextStore));
   }

   /// <inheritdoc />
   public async Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
       JsonsOptions options,
       CancellationToken cancellationToken = default)
   {
      cancellationToken.ThrowIfCancellationRequested();
      ArgumentNullException.ThrowIfNull(options);

      Response<JsonsBootstrapPayload> bootstrapResponse =
          await _bootstrapper.EnsureWorkspaceAsync(
              options,
              cancellationToken);

      if(!bootstrapResponse.IsSuccess)
      {
         return CreateFailedResponse(
             bootstrapResponse,
             fallbackCode: "ErrorCatalogBootstrapFailed",
             fallbackMessage:
                 "The WhenItFails JSON workspace could not be prepared.");
      }

      if(bootstrapResponse.Data is null)
      {
         return Response<ErrorCatalogInitializationPayload>.Invalid(
             code: "ErrorCatalogBootstrapPayloadIsNull",
             message:
                 "JSON workspace bootstrap succeeded without payload data.");
      }

      Response<ErrorCatalogContext> contextResponse =
          await _contextProvider.LoadFromJsonsAsync(
              options,
              cancellationToken);

      if(!contextResponse.IsSuccess)
      {
         return CreateFailedResponse(
             contextResponse,
             fallbackCode: "ErrorCatalogContextLoadFailed",
             fallbackMessage:
                 "The WhenItFails catalog context could not be loaded.");
      }

      if(contextResponse.Data is null)
      {
         return Response<ErrorCatalogInitializationPayload>.Invalid(
             code: "ErrorCatalogContextPayloadIsNull",
             message:
                 "Catalog context loading succeeded without payload data.");
      }

      _contextStore.Set(contextResponse.Data);

        ErrorCatalogInitializationPayload payload = new()
        {
            Bootstrap = bootstrapResponse.Data,
            Context = contextResponse.Data,
            ContextSource =
            ErrorCatalogContextSource.ProjectCatalog,
            KeptPreviousContext = false,
            UsedFallback = false
        };

        return Response<ErrorCatalogInitializationPayload>.Ok(payload);
   }

   private static Response<ErrorCatalogInitializationPayload>
       CreateFailedResponse<TPayload>(
           Response<TPayload> sourceResponse,
           string fallbackCode,
           string fallbackMessage)
   {
      string issueCode = sourceResponse.Issues.Count > 0
          ? sourceResponse.Issues[0].Code
          : fallbackCode;

      string message = string.IsNullOrWhiteSpace(sourceResponse.Message)
          ? fallbackMessage
          : sourceResponse.Message;

      return Response<ErrorCatalogInitializationPayload>.WithStatus(
          Response<ErrorCatalogInitializationPayload>.Fail(
              code: issueCode,
              message: message),
          sourceResponse.Status);
   }
}