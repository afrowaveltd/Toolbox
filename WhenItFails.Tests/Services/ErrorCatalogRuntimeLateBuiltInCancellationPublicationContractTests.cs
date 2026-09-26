using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.Services;

public sealed class ErrorCatalogRuntimeLateBuiltInCancellationPublicationContractTests
{
    [Fact]
    public async Task ResetToDefaultsAsync_WhenProviderReturnsSuccessAfterCancelling_PreservesPreviousPublicationAndStatus()
    {
        using CancellationTokenSource source = new();

        ErrorCatalogContext previousContext = new();
        ErrorCatalogContext candidateContext = new();

        ErrorCatalogContextStore store = new();
        store.Set(previousContext);

        var previousPublication =
            store.GetCurrentPublication().Data;

        Assert.NotNull(previousPublication);

        ErrorCatalogRuntime runtime = new(
            new UnusedInitializer(),
            new WhenItFailsOptions(),
            store,
            new CancellingSuccessfulBuiltInProvider(
                source,
                candidateContext),
            new UnusedDescriptorService(),
            new UnusedProfileSelectionService());

        OperationCanceledException exception =
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => runtime.ResetToDefaultsAsync(
                    source.Token));

        Assert.True(source.IsCancellationRequested);
        Assert.Equal(source.Token, exception.CancellationToken);

        Assert.Same(previousContext, store.Current);
        Assert.NotSame(candidateContext, store.Current);
        Assert.Same(
            previousPublication,
            store.GetCurrentPublication().Data);

        Assert.False(runtime.GetStatus().IsSuccess);
    }

    [Fact]
    public async Task InitializeAsync_WhenFlexibleFallbackProviderReturnsSuccessAfterCancelling_DoesNotPublishFallbackOrStatus()
    {
        using CancellationTokenSource source = new();

        ErrorCatalogContext candidateContext = new();
        ErrorCatalogContextStore store = new();

        ErrorCatalogRuntime runtime = new(
            new FailingInitializer(),
            new WhenItFailsOptions
            {
                InitializationMode =
                    ErrorCatalogInitializationMode.Flexible
            },
            store,
            new CancellingSuccessfulBuiltInProvider(
                source,
                candidateContext),
            new UnusedDescriptorService(),
            new UnusedProfileSelectionService());

        OperationCanceledException exception =
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => runtime.InitializeAsync(
                    new JsonsOptions(),
                    source.Token));

        Assert.True(source.IsCancellationRequested);
        Assert.Equal(source.Token, exception.CancellationToken);

        Assert.False(store.IsInitialized);
        Assert.Null(store.Current);
        Assert.False(store.GetCurrent().IsSuccess);
        Assert.False(runtime.GetCurrentContext().IsSuccess);
        Assert.False(runtime.GetStatus().IsSuccess);
    }

    private sealed class CancellingSuccessfulBuiltInProvider
        : IBuiltInErrorCatalogContextProvider
    {
        private readonly CancellationTokenSource _source;
        private readonly ErrorCatalogContext _candidateContext;

        public CancellingSuccessfulBuiltInProvider(
            CancellationTokenSource source,
            ErrorCatalogContext candidateContext)
        {
            _source = source;
            _candidateContext = candidateContext;
        }

        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            Assert.Equal(
                _source.Token,
                cancellationToken);

            _source.Cancel();

            return Task.FromResult(
                Response<ErrorCatalogContext>.Ok(
                    _candidateContext));
        }
    }

    private sealed class FailingInitializer
        : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                Response<ErrorCatalogInitializationPayload>.Invalid(
                    code: "CatalogDocumentsInvalid",
                    message: "Catalog documents are invalid."));
        }
    }

    private sealed class UnusedInitializer
        : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Unexpected initializer call.");
        }
    }

    private sealed class UnusedDescriptorService
        : IErrorDescriptorService
    {
        public Response<ErrorDescriptor> FromId(
            ErrorCatalogContext? context,
            string errorId)
        {
            throw new InvalidOperationException(
                "Unexpected descriptor call.");
        }

        public Response<ErrorDescriptor> FromName(
            ErrorCatalogContext? context,
            string errorName)
        {
            throw new InvalidOperationException(
                "Unexpected descriptor call.");
        }

        public Response<ErrorDescriptor> FromCode(
            ErrorCatalogContext? context,
            int code)
        {
            throw new InvalidOperationException(
                "Unexpected descriptor call.");
        }
    }

    private sealed class UnusedProfileSelectionService
        : IErrorProfileSelectionService
    {
        public Response<IReadOnlyList<ErrorDefinition>> ResolveByProfileName(
            ErrorCatalogContext? context,
            string profileName)
        {
            throw new InvalidOperationException(
                "Unexpected profile selection call.");
        }
    }
}
