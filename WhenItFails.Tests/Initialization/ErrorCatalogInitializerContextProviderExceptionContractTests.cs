using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Initialization;

public sealed class ErrorCatalogInitializerContextProviderExceptionContractTests
{
    [Fact]
    public async Task InitializeAsync_WhenContextProviderThrows_ReturnsStableFailure()
    {
        ErrorCatalogContext previousContext = new();
        TrackingContextStore contextStore = new(previousContext);

        ErrorCatalogInitializer initializer = new(
            new SuccessfulBootstrapper(),
            new ThrowingContextProvider(),
            contextStore);

        Response<ErrorCatalogInitializationPayload> response =
            await initializer.InitializeAsync(new JsonsOptions());

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The error catalog context provider failed during initialization.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal(
                    "WIF_INITIALIZER_CONTEXT_PROVIDER_FAILED",
                    issue.Code);
                Assert.Equal(
                    "The error catalog context provider failed during initialization.",
                    issue.Message);
            });

        Assert.Same(previousContext, contextStore.StoredContext);
    }

    private sealed class SuccessfulBootstrapper : IJsonsBootstrapper
    {
        public Task<Response<JsonsBootstrapPayload>> EnsureWorkspaceAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Response<JsonsBootstrapPayload>.Ok(
                    new JsonsBootstrapPayload()));
        }
    }

    private sealed class ThrowingContextProvider
        : IErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromException<Response<ErrorCatalogContext>>(
                new InvalidOperationException(
                    "Sensitive initializer context provider detail must not escape."));
        }
    }

    private sealed class TrackingContextStore : IErrorCatalogContextStore
    {
        public TrackingContextStore(
            ErrorCatalogContext initialContext)
        {
            StoredContext = initialContext;
        }

        public bool IsInitialized => StoredContext is not null;

        public ErrorCatalogContext? Current => StoredContext;

        public ErrorCatalogContext? StoredContext { get; private set; }

        public Response<ErrorCatalogContext> GetCurrent()
        {
            return StoredContext is null
                ? Response<ErrorCatalogContext>.Invalid(
                    code: "ErrorCatalogContextNotInitialized",
                    message: "Error catalog context has not been initialized.")
                : Response<ErrorCatalogContext>.Ok(StoredContext);
        }

        public void Set(ErrorCatalogContext context)
        {
            throw new InvalidOperationException("Unexpected Set call.");
        }
    }
}
