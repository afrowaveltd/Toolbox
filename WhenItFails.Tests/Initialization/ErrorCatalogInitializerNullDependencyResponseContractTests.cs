using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Initialization;

public sealed class ErrorCatalogInitializerNullDependencyResponseContractTests
{
    [Fact]
    public async Task InitializeAsync_ShouldReturnInvalidResponse_WhenBootstrapperReturnsNull()
    {
        TrackingContextProvider contextProvider = new();
        TrackingContextStore contextStore = new();

        ErrorCatalogInitializer initializer = new(
            new NullResponseBootstrapper(),
            contextProvider,
            contextStore);

        Response<ErrorCatalogInitializationPayload> response =
            await initializer.InitializeAsync(new JsonsOptions());

        AssertInvalidResponse(
            response,
            "WIF_INITIALIZER_BOOTSTRAPPER_RESPONSE_NULL",
            "The JSON workspace bootstrapper returned a null response.");

        Assert.False(contextProvider.WasCalled);
        Assert.Null(contextStore.StoredContext);
    }

    [Fact]
    public async Task InitializeAsync_ShouldReturnInvalidResponse_WhenContextProviderReturnsNull()
    {
        ErrorCatalogContext previousContext = new();
        TrackingContextStore contextStore = new(previousContext);

        ErrorCatalogInitializer initializer = new(
            new ValidBootstrapper(),
            new NullResponseContextProvider(),
            contextStore);

        Response<ErrorCatalogInitializationPayload> response =
            await initializer.InitializeAsync(new JsonsOptions());

        AssertInvalidResponse(
            response,
            "WIF_INITIALIZER_CONTEXT_PROVIDER_RESPONSE_NULL",
            "The error catalog context provider returned a null response during initialization.");

        Assert.Same(previousContext, contextStore.StoredContext);
    }

    private static void AssertInvalidResponse(
        Response<ErrorCatalogInitializationPayload> response,
        string code,
        string message)
    {
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal(code, issue.Code);
        Assert.Equal(message, response.Message);
    }

    private sealed class NullResponseBootstrapper : IJsonsBootstrapper
    {
        public Task<Response<JsonsBootstrapPayload>> EnsureWorkspaceAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Response<JsonsBootstrapPayload>>(null!);
        }
    }

    private sealed class ValidBootstrapper : IJsonsBootstrapper
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

    private sealed class TrackingContextProvider : IErrorCatalogContextProvider
    {
        public bool WasCalled { get; private set; }

        public Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;

            return Task.FromResult(
                Response<ErrorCatalogContext>.Ok(
                    new ErrorCatalogContext()));
        }
    }

    private sealed class NullResponseContextProvider : IErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Response<ErrorCatalogContext>>(null!);
        }
    }

    private sealed class TrackingContextStore : IErrorCatalogContextStore
    {
        public TrackingContextStore(
            ErrorCatalogContext? initialContext = null)
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
            StoredContext = context;
        }
    }
}
