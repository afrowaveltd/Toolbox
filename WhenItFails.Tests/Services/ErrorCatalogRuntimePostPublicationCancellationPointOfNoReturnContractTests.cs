using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.Services;

public sealed class ErrorCatalogRuntimePostPublicationCancellationPointOfNoReturnContractTests
{
    [Fact]
    public async Task InitializeAsync_WhenStoreCancelsAfterSuccessfulProjectPublication_CompletesProjectActivation()
    {
        using CancellationTokenSource source = new();

        ErrorCatalogContext projectContext = new();
        CancellingPublicationStore store = new(source);

        ErrorCatalogInitializer initializer = new(
            new FixedBootstrapper(),
            new FixedContextProvider(projectContext),
            store);

        ErrorCatalogRuntime runtime = CreateRuntime(
            store,
            initializer,
            new UnusedBuiltInProvider());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(
                new JsonsOptions(),
                source.Token);

        Assert.True(source.IsCancellationRequested);
        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal(
            ErrorCatalogContextSource.ProjectCatalog,
            response.Data.ContextSource);
        Assert.False(response.Data.IsDegraded);

        Assert.Same(projectContext, store.Current);
        Assert.Equal(1, store.PublishCount);

        Response<ErrorCatalogRuntimeStatus> statusResponse =
            runtime.GetStatus();

        Assert.True(statusResponse.IsSuccess);
        Assert.NotNull(statusResponse.Data);
        Assert.Equal(
            ErrorCatalogRuntimeState.ProjectCatalog,
            statusResponse.Data.State);
        Assert.True(statusResponse.Data.IsConsistent);

        Response<ErrorCatalogActivationStatusSnapshot> completed =
            runtime.GetCompletedActivation();

        Assert.True(completed.IsSuccess);
        Assert.NotNull(completed.Data);
        Assert.Equal(1L, completed.Data.Generation);
        Assert.Equal(1L, completed.Data.ActivationSequence);
        Assert.Equal(
            ErrorCatalogRuntimeState.ProjectCatalog,
            completed.Data.Status.State);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_WhenStoreCancelsAfterSuccessfulPublication_CompletesBuiltInActivation()
    {
        using CancellationTokenSource source = new();

        ErrorCatalogContext builtInContext = new();
        CancellingPublicationStore store = new(source);

        ErrorCatalogRuntime runtime = CreateRuntime(
            store,
            new UnusedInitializer(),
            new FixedBuiltInProvider(builtInContext));

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.ResetToDefaultsAsync(
                source.Token);

        Assert.True(source.IsCancellationRequested);
        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal(
            ErrorCatalogContextSource.BuiltInDefaults,
            response.Data.ContextSource);
        Assert.False(response.Data.IsDegraded);
        Assert.False(response.Data.UsedFallback);

        Assert.Same(builtInContext, store.Current);
        Assert.Equal(1, store.PublishCount);

        Response<ErrorCatalogRuntimeStatus> statusResponse =
            runtime.GetStatus();

        Assert.True(statusResponse.IsSuccess);
        Assert.NotNull(statusResponse.Data);
        Assert.Equal(
            ErrorCatalogRuntimeState.BuiltInDefaults,
            statusResponse.Data.State);
        Assert.True(statusResponse.Data.IsConsistent);

        Response<ErrorCatalogActivationStatusSnapshot> completed =
            runtime.GetCompletedActivation();

        Assert.True(completed.IsSuccess);
        Assert.NotNull(completed.Data);
        Assert.Equal(1L, completed.Data.Generation);
        Assert.Equal(1L, completed.Data.ActivationSequence);
        Assert.Equal(
            ErrorCatalogRuntimeState.BuiltInDefaults,
            completed.Data.Status.State);
    }

    [Fact]
    public async Task InitializeAsync_WhenStoreCancelsAfterSuccessfulFallbackPublication_CompletesFallbackRecovery()
    {
        using CancellationTokenSource source = new();

        ErrorCatalogContext fallbackContext = new();
        CancellingPublicationStore store = new(source);

        ErrorCatalogRuntime runtime = CreateRuntime(
            store,
            new FailingInitializer(),
            new FixedBuiltInProvider(fallbackContext),
            new WhenItFailsOptions
            {
                InitializationMode =
                    ErrorCatalogInitializationMode.Flexible
            });

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(
                new JsonsOptions(),
                source.Token);

        Assert.True(source.IsCancellationRequested);
        Assert.True(response.IsSuccess);
        Assert.Equal(
            ResultStatus.SuccessWithWarnings,
            response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal(
            ErrorCatalogContextSource.BuiltInDefaults,
            response.Data.ContextSource);
        Assert.True(response.Data.IsDegraded);
        Assert.True(response.Data.UsedFallback);
        Assert.False(response.Data.KeptPreviousContext);

        Assert.Same(fallbackContext, store.Current);
        Assert.Equal(1, store.PublishCount);

        Response<ErrorCatalogRuntimeStatus> statusResponse =
            runtime.GetStatus();

        Assert.True(statusResponse.IsSuccess);
        Assert.NotNull(statusResponse.Data);
        Assert.Equal(
            ErrorCatalogRuntimeState.BuiltInFallback,
            statusResponse.Data.State);
        Assert.True(statusResponse.Data.IsConsistent);
        Assert.Equal(
            "CatalogDocumentsInvalid",
            statusResponse.Data.RecoveryReasonCode);
        Assert.Equal(
            ResultStatus.Invalid,
            statusResponse.Data.RecoveryStatus);

        Response<ErrorCatalogActivationStatusSnapshot> completed =
            runtime.GetCompletedActivation();

        Assert.True(completed.IsSuccess);
        Assert.NotNull(completed.Data);
        Assert.Equal(1L, completed.Data.Generation);
        Assert.Equal(1L, completed.Data.ActivationSequence);
        Assert.Equal(
            ErrorCatalogRuntimeState.BuiltInFallback,
            completed.Data.Status.State);
    }

    private static ErrorCatalogRuntime CreateRuntime(
        IErrorCatalogContextStore store,
        IErrorCatalogInitializer initializer,
        IBuiltInErrorCatalogContextProvider builtInProvider,
        WhenItFailsOptions? options = null)
    {
        return new ErrorCatalogRuntime(
            initializer,
            options ?? new WhenItFailsOptions(),
            store,
            builtInProvider,
            new UnusedDescriptorService(),
            new UnusedProfileSelectionService());
    }

    private sealed class CancellingPublicationStore
        : IErrorCatalogContextStore,
          IErrorCatalogContextPublisher,
          IErrorCatalogContextPublicationReader
    {
        private readonly ErrorCatalogContextStore _inner = new();
        private readonly CancellationTokenSource _source;

        public CancellingPublicationStore(
            CancellationTokenSource source)
        {
            _source = source;
        }

        public int PublishCount { get; private set; }

        public bool IsInitialized => _inner.IsInitialized;

        public ErrorCatalogContext? Current => _inner.Current;

        public Response<ErrorCatalogContext> GetCurrent()
        {
            return _inner.GetCurrent();
        }

        public Response<ErrorCatalogContextPublication>
            GetCurrentPublication()
        {
            return _inner.GetCurrentPublication();
        }

        public void Set(ErrorCatalogContext context)
        {
            _ = Publish(context);
        }

        public ErrorCatalogContextPublication Publish(
            ErrorCatalogContext context)
        {
            ErrorCatalogContextPublication publication =
                _inner.Publish(context);

            PublishCount++;
            _source.Cancel();

            return publication;
        }
    }

    private sealed class FixedBootstrapper
        : IJsonsBootstrapper
    {
        public Task<Response<JsonsBootstrapPayload>> EnsureWorkspaceAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                Response<JsonsBootstrapPayload>.Ok(
                    new JsonsBootstrapPayload
                    {
                        PackageDirectoryPath =
                            options.PackageDirectoryPath
                    }));
        }
    }

    private sealed class FixedContextProvider(
        ErrorCatalogContext context)
        : IErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                Response<ErrorCatalogContext>.Ok(
                    context));
        }
    }

    private sealed class FixedBuiltInProvider(
        ErrorCatalogContext context)
        : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                Response<ErrorCatalogContext>.Ok(
                    context));
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

    private sealed class UnusedBuiltInProvider
        : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Unexpected built-in provider call.");
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
