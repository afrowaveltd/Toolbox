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

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class ContextPublicationStatusLifecycleContractTests
{
    [Fact]
    public async Task SuccessfulProjectInitialization_PublishesContextBeforeRecordingStatus()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = new();
        SequencedInitializer initializer = new(store, _ => SuccessfulProject(context));
        ErrorCatalogRuntime runtime = CreateRuntime(store, initializer);

        Response<ErrorCatalogInitializationPayload> result =
            await runtime.InitializeAsync();

        Assert.True(result.IsSuccess);
        Assert.Same(context, CurrentPublication(store).Context);
        Assert.Equal(1L, CurrentPublication(store).Generation);
        Assert.Equal(ErrorCatalogRuntimeState.ProjectCatalog, CurrentStatus(runtime).State);
        Assert.Same(context, runtime.GetCurrentContext().Data);
    }

    [Fact]
    public async Task StrictFailure_RetainsBothPublicationAndPreviouslyRecordedStatus()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = new();
        SequencedInitializer initializer = new(
            store,
            call => call == 1
                ? SuccessfulProject(context)
                : InitializationFailure());
        ErrorCatalogRuntime runtime = CreateRuntime(
            store,
            initializer,
            new WhenItFailsOptions
            {
                InitializationMode = ErrorCatalogInitializationMode.Strict
            });

        Assert.True((await runtime.InitializeAsync()).IsSuccess);
        ErrorCatalogContextPublication before = CurrentPublication(store);
        ErrorCatalogRuntimeStatus statusBefore = CurrentStatus(runtime);

        Response<ErrorCatalogInitializationPayload> failed =
            await runtime.InitializeAsync();

        Assert.False(failed.IsSuccess);
        Assert.Same(before, CurrentPublication(store));
        Assert.Same(statusBefore, CurrentStatus(runtime));
        Assert.Same(context, runtime.GetCurrentContext().Data);
    }

    [Fact]
    public async Task FlexibleRecovery_ChangesStatusWithoutPublishingPreviousContextAgain()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = new();
        SequencedInitializer initializer = new(
            store,
            call => call == 1
                ? SuccessfulProject(context)
                : InitializationFailure());
        ErrorCatalogRuntime runtime = CreateRuntime(
            store,
            initializer,
            new WhenItFailsOptions
            {
                InitializationMode = ErrorCatalogInitializationMode.Flexible
            });

        Assert.True((await runtime.InitializeAsync()).IsSuccess);
        ErrorCatalogContextPublication before = CurrentPublication(store);
        ErrorCatalogRuntimeStatus statusBefore = CurrentStatus(runtime);

        Response<ErrorCatalogInitializationPayload> recovered =
            await runtime.InitializeAsync();

        Assert.True(recovered.IsSuccess);
        Assert.True(recovered.Data!.KeptPreviousContext);
        Assert.Same(context, recovered.Data.Context);
        Assert.Same(before, CurrentPublication(store));
        ErrorCatalogRuntimeStatus statusAfter = CurrentStatus(runtime);
        Assert.NotSame(statusBefore, statusAfter);
        Assert.Equal(ErrorCatalogRuntimeState.PreviousContextRecovery, statusAfter.State);
        Assert.True(statusAfter.KeptPreviousContext);
        Assert.False(statusAfter.UsedFallback);
        Assert.Same(context, runtime.GetCurrentContext().Data);
    }

    [Fact]
    public async Task FlexibleFallbackAndExplicitReset_EachPublishesAContextWithDifferentStatus()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext fallbackContext = new();
        ErrorCatalogContext resetContext = new();
        SequencedInitializer initializer = new(store, _ => InitializationFailure());
        SequencedBuiltInProvider builtIn = new(
            call => Response<ErrorCatalogContext>.Ok(
                call == 1 ? fallbackContext : resetContext));
        ErrorCatalogRuntime runtime = CreateRuntime(
            store,
            initializer,
            new WhenItFailsOptions
            {
                InitializationMode = ErrorCatalogInitializationMode.Flexible
            },
            builtIn);

        Response<ErrorCatalogInitializationPayload> fallback =
            await runtime.InitializeAsync();

        Assert.True(fallback.IsSuccess);
        Assert.True(fallback.Data!.UsedFallback);
        ErrorCatalogContextPublication first = CurrentPublication(store);
        Assert.Equal(1L, first.Generation);
        Assert.Same(fallbackContext, first.Context);
        Assert.Equal(ErrorCatalogRuntimeState.BuiltInFallback, CurrentStatus(runtime).State);

        Response<ErrorCatalogInitializationPayload> reset =
            await runtime.ResetToDefaultsAsync();

        Assert.True(reset.IsSuccess);
        Assert.False(reset.Data!.UsedFallback);
        ErrorCatalogContextPublication second = CurrentPublication(store);
        Assert.Equal(first.StoreId, second.StoreId);
        Assert.Equal(2L, second.Generation);
        Assert.Same(resetContext, second.Context);
        Assert.Equal(ErrorCatalogRuntimeState.BuiltInDefaults, CurrentStatus(runtime).State);
        Assert.Equal(2, builtIn.CallCount);
    }

    [Fact]
    public async Task FailedExplicitReset_DoesNotReplaceContextPublicationOrStatus()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext active = new();
        SequencedInitializer initializer = new(store, _ => SuccessfulProject(active));
        SequencedBuiltInProvider builtIn = new(
            _ => Response<ErrorCatalogContext>.Invalid(
                code: "BuiltInCatalogInvalid",
                message: "The built-in catalog is invalid."));
        ErrorCatalogRuntime runtime = CreateRuntime(store, initializer, builtIn: builtIn);

        Assert.True((await runtime.InitializeAsync()).IsSuccess);
        ErrorCatalogContextPublication before = CurrentPublication(store);
        ErrorCatalogRuntimeStatus statusBefore = CurrentStatus(runtime);

        Response<ErrorCatalogInitializationPayload> failed =
            await runtime.ResetToDefaultsAsync();

        Assert.False(failed.IsSuccess);
        Assert.Same(before, CurrentPublication(store));
        Assert.Same(statusBefore, CurrentStatus(runtime));
        Assert.Same(active, runtime.GetCurrentContext().Data);
    }

    [Fact]
    public async Task PublicationBeforeStatusWindow_CanExposeNewGenerationWithPreviousStatus()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext firstContext = new();
        ErrorCatalogContext secondContext = new();
        SequencedInitializer initializer = new(
            store,
            call => SuccessfulProject(call == 1 ? firstContext : secondContext));
        ErrorCatalogRuntime runtime = CreateRuntime(store, initializer);

        Assert.True((await runtime.InitializeAsync()).IsSuccess);
        ErrorCatalogContextPublication first = CurrentPublication(store);
        ErrorCatalogRuntimeStatus statusBefore = CurrentStatus(runtime);
        ErrorCatalogContextPublication? observedPublication = null;
        ErrorCatalogRuntimeStatus? observedStatus = null;

        // Called by the initializer after Set but before it returns to the
        // runtime, which has not yet executed RecordStatus for this activation.
        initializer.OnPublished = () =>
        {
            observedPublication = CurrentPublication(store);
            observedStatus = CurrentStatus(runtime);
        };

        Assert.True((await runtime.InitializeAsync()).IsSuccess);

        Assert.NotNull(observedPublication);
        Assert.NotNull(observedStatus);
        Assert.Equal(first.Generation + 1L, observedPublication.Generation);
        Assert.Same(secondContext, observedPublication.Context);
        Assert.Same(statusBefore, observedStatus);
        Assert.Same(observedPublication, CurrentPublication(store));
        Assert.NotSame(statusBefore, CurrentStatus(runtime));
        Assert.Equal(ErrorCatalogRuntimeState.ProjectCatalog, CurrentStatus(runtime).State);
    }

    private static Response<ErrorCatalogInitializationPayload> SuccessfulProject(
        ErrorCatalogContext context) =>
        Response<ErrorCatalogInitializationPayload>.Ok(
            new ErrorCatalogInitializationPayload
            {
                Bootstrap = new JsonsBootstrapPayload
                {
                    PackageDirectoryPath = "Jsons/WhenItFails"
                },
                Context = context,
                ContextSource = ErrorCatalogContextSource.ProjectCatalog
            });

    private static Response<ErrorCatalogInitializationPayload> InitializationFailure() =>
        Response<ErrorCatalogInitializationPayload>.Invalid(
            code: "CatalogDocumentsInvalid",
            message: "The project catalog is invalid.");

    private static ErrorCatalogContextPublication CurrentPublication(
        ErrorCatalogContextStore store) =>
        Assert.IsType<ErrorCatalogContextPublication>(
            store.GetCurrentPublication().Data);

    private static ErrorCatalogRuntimeStatus CurrentStatus(ErrorCatalogRuntime runtime) =>
        Assert.IsType<ErrorCatalogRuntimeStatus>(runtime.GetStatus().Data);

    private static ErrorCatalogRuntime CreateRuntime(
        ErrorCatalogContextStore store,
        IErrorCatalogInitializer initializer,
        WhenItFailsOptions? options = null,
        IBuiltInErrorCatalogContextProvider? builtIn = null) =>
        new(
            initializer,
            options ?? new WhenItFailsOptions(),
            store,
            builtIn ?? new SequencedBuiltInProvider(_ =>
                throw new InvalidOperationException("Unexpected built-in fallback.")),
            new UnusedDescriptorService(),
            new UnusedProfileSelectionService());

    private sealed class SequencedInitializer(
        ErrorCatalogContextStore store,
        Func<int, Response<ErrorCatalogInitializationPayload>> sequence)
        : IErrorCatalogInitializer
    {
        private int _calls;

        public Action? OnPublished { get; set; }

        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Response<ErrorCatalogInitializationPayload> result =
                sequence(++_calls);

            if (result.IsSuccess && result.Data is { } payload)
            {
                store.Set(payload.Context);
                OnPublished?.Invoke();
            }

            return Task.FromResult(result);
        }
    }

    private sealed class SequencedBuiltInProvider(
        Func<int, Response<ErrorCatalogContext>> sequence)
        : IBuiltInErrorCatalogContextProvider
    {
        public int CallCount { get; private set; }

        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(sequence(++CallCount));
        }
    }

    private sealed class UnusedDescriptorService : IErrorDescriptorService
    {
        public Response<ErrorDescriptor> FromId(
            ErrorCatalogContext? context, string errorId) =>
            throw new NotSupportedException();

        public Response<ErrorDescriptor> FromName(
            ErrorCatalogContext? context, string errorName) =>
            throw new NotSupportedException();

        public Response<ErrorDescriptor> FromCode(
            ErrorCatalogContext? context, int code) =>
            throw new NotSupportedException();
    }

    private sealed class UnusedProfileSelectionService
        : IErrorProfileSelectionService
    {
        public Response<IReadOnlyList<ErrorDefinition>> ResolveByProfileName(
            ErrorCatalogContext? context, string profileName) =>
            throw new NotSupportedException();
    }
}
