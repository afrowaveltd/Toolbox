using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Initialization;

public sealed class ErrorCatalogInitializerBootstrapperExceptionContractTests
{
    [Fact]
    public async Task InitializeAsync_WhenBootstrapperThrows_ReturnsStableFailure()
    {
        ErrorCatalogContext previousContext = new();
        TrackingContextProvider contextProvider = new();
        TrackingContextStore contextStore = new(previousContext);

        ErrorCatalogInitializer initializer = new(
            new ThrowingBootstrapper(),
            contextProvider,
            contextStore);

        Response<ErrorCatalogInitializationPayload> response =
            await initializer.InitializeAsync(new JsonsOptions());

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The JSON workspace bootstrapper failed.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal(
                    "WIF_INITIALIZER_BOOTSTRAPPER_FAILED",
                    issue.Code);
                Assert.Equal(
                    "The JSON workspace bootstrapper failed.",
                    issue.Message);
            });

        Assert.False(contextProvider.WasCalled);
        Assert.Same(previousContext, contextStore.StoredContext);
    }

    [Fact]
    public async Task InitializeAsync_WhenBootstrapperCancels_RethrowsSameOperationCanceledException()
    {
        OperationCanceledException cancellation = new(
            "Initializer bootstrapper cancellation must propagate unchanged.");

        ErrorCatalogContext previousContext = new();
        TrackingContextProvider contextProvider = new();
        TrackingContextStore contextStore = new(previousContext);

        ErrorCatalogInitializer initializer = new(
            new CancelingBootstrapper(cancellation),
            contextProvider,
            contextStore);

        OperationCanceledException thrown =
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => initializer.InitializeAsync(new JsonsOptions()));

        Assert.Same(cancellation, thrown);
        Assert.False(contextProvider.WasCalled);
        Assert.Same(previousContext, contextStore.StoredContext);
    }

    private sealed class ThrowingBootstrapper : IJsonsBootstrapper
    {
        public Task<Response<JsonsBootstrapPayload>> EnsureWorkspaceAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromException<Response<JsonsBootstrapPayload>>(
                new InvalidOperationException(
                    "Sensitive initializer bootstrapper detail must not escape."));
        }
    }

    private sealed class CancelingBootstrapper : IJsonsBootstrapper
    {
        private readonly OperationCanceledException _cancellation;

        public CancelingBootstrapper(
            OperationCanceledException cancellation)
        {
            _cancellation = cancellation;
        }

        public Task<Response<JsonsBootstrapPayload>> EnsureWorkspaceAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromException<Response<JsonsBootstrapPayload>>(
                _cancellation);
        }
    }

    private sealed class TrackingContextProvider
        : IErrorCatalogContextProvider
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
