using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.Initialization;

public sealed class ErrorCatalogInitializerLateCancellationPublicationContractTests
{
    [Fact]
    public async Task InitializeAsync_WhenBootstrapReturnsSuccessAfterCancelling_DoesNotStartContextLoadOrPublish()
    {
        using CancellationTokenSource source = new();
        ErrorCatalogContext previousContext = new();

        ErrorCatalogContextStore store = new();
        store.Set(previousContext);

        var previousPublication =
            store.GetCurrentPublication().Data;

        Assert.NotNull(previousPublication);

        TrackingContextProvider provider = new();

        ErrorCatalogInitializer initializer = new(
            new CancellingSuccessfulBootstrapper(source),
            provider,
            store);

        OperationCanceledException exception =
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => initializer.InitializeAsync(
                    new JsonsOptions(),
                    source.Token));

        Assert.True(source.IsCancellationRequested);
        Assert.Equal(source.Token, exception.CancellationToken);
        Assert.Equal(0, provider.LoadCount);
        Assert.Same(previousContext, store.Current);
        Assert.Same(
            previousPublication,
            store.GetCurrentPublication().Data);
    }

    [Fact]
    public async Task InitializeAsync_WhenContextProviderReturnsSuccessAfterCancelling_DoesNotPublishNewContext()
    {
        using CancellationTokenSource source = new();
        ErrorCatalogContext previousContext = new();
        ErrorCatalogContext candidateContext = new();

        ErrorCatalogContextStore store = new();
        store.Set(previousContext);

        var previousPublication =
            store.GetCurrentPublication().Data;

        Assert.NotNull(previousPublication);

        CancellingSuccessfulContextProvider provider = new(
            source,
            candidateContext);

        ErrorCatalogInitializer initializer = new(
            new SuccessfulBootstrapper(),
            provider,
            store);

        OperationCanceledException exception =
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => initializer.InitializeAsync(
                    new JsonsOptions(),
                    source.Token));

        Assert.True(source.IsCancellationRequested);
        Assert.Equal(source.Token, exception.CancellationToken);
        Assert.Equal(1, provider.LoadCount);
        Assert.Same(previousContext, store.Current);
        Assert.NotSame(candidateContext, store.Current);
        Assert.Same(
            previousPublication,
            store.GetCurrentPublication().Data);
    }

    private sealed class CancellingSuccessfulBootstrapper
        : IJsonsBootstrapper
    {
        private readonly CancellationTokenSource _source;

        public CancellingSuccessfulBootstrapper(
            CancellationTokenSource source)
        {
            _source = source;
        }

        public Task<Response<JsonsBootstrapPayload>> EnsureWorkspaceAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            Assert.Equal(_source.Token, cancellationToken);
            _source.Cancel();

            return Task.FromResult(
                Response<JsonsBootstrapPayload>.Ok(
                    new JsonsBootstrapPayload()));
        }
    }

    private sealed class SuccessfulBootstrapper : IJsonsBootstrapper
    {
        public Task<Response<JsonsBootstrapPayload>> EnsureWorkspaceAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                Response<JsonsBootstrapPayload>.Ok(
                    new JsonsBootstrapPayload()));
        }
    }

    private sealed class TrackingContextProvider
        : IErrorCatalogContextProvider
    {
        public int LoadCount { get; private set; }

        public Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            LoadCount++;

            return Task.FromResult(
                Response<ErrorCatalogContext>.Ok(
                    new ErrorCatalogContext()));
        }
    }

    private sealed class CancellingSuccessfulContextProvider
        : IErrorCatalogContextProvider
    {
        private readonly CancellationTokenSource _source;
        private readonly ErrorCatalogContext _candidateContext;

        public CancellingSuccessfulContextProvider(
            CancellationTokenSource source,
            ErrorCatalogContext candidateContext)
        {
            _source = source;
            _candidateContext = candidateContext;
        }

        public int LoadCount { get; private set; }

        public Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            Assert.Equal(_source.Token, cancellationToken);
            LoadCount++;
            _source.Cancel();

            return Task.FromResult(
                Response<ErrorCatalogContext>.Ok(
                    _candidateContext));
        }
    }
}
