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

public sealed class RuntimeActivationSerializationContractTests
{
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task ConcurrentInitializations_PublishAndRecordStatusInAdmissionOrder()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext firstContext = new();
        ErrorCatalogContext secondContext = new();
        TaskCompletionSource<bool> entered = NewSignal();
        TaskCompletionSource<bool> release = NewSignal();

        ControlledInitializer initializer = new(store, async (call, token) =>
        {
            if (call == 1)
            {
                entered.TrySetResult(true);
                await release.Task.WaitAsync(token);
            }

            return ProjectSuccess(call == 1 ? firstContext : secondContext);
        });
        ErrorCatalogRuntime runtime = CreateRuntime(store, initializer);

        Task<Response<ErrorCatalogInitializationPayload>> first =
            runtime.InitializeAsync();

        try
        {
            await entered.Task.WaitAsync(TestTimeout);

            Task<Response<ErrorCatalogInitializationPayload>> second =
                runtime.InitializeAsync();

            // The second initializer cannot begin before the first releases
            // its runtime-wide activation gate.
            Assert.Equal(1, initializer.CallCount);

            release.TrySetResult(true);

            Assert.True((await first.WaitAsync(TestTimeout)).IsSuccess);
            Assert.True((await second.WaitAsync(TestTimeout)).IsSuccess);

            Assert.Equal(2, initializer.CallCount);
            ErrorCatalogContextPublication current = Publication(store);
            Assert.Equal(2L, current.Generation);
            Assert.Same(secondContext, current.Context);
            Assert.Same(secondContext, runtime.GetCurrentContext().Data);

            ErrorCatalogActivationStatusSnapshot status = Activation(runtime);
            Assert.Equal(2L, status.Generation);
            Assert.Equal(2L, status.ActivationSequence);
            Assert.Same(runtime.GetStatus().Data, status.Status);
            Assert.Equal(ErrorCatalogRuntimeState.ProjectCatalog, status.Status.State);
        }
        finally
        {
            release.TrySetResult(true);
            await first.WaitAsync(TestTimeout);
        }
    }

    [Fact]
    public async Task ResetInProgress_BlocksInitializationUntilItsStatusIsRecorded()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext builtIn = new();
        ErrorCatalogContext project = new();
        TaskCompletionSource<bool> entered = NewSignal();
        TaskCompletionSource<bool> release = NewSignal();

        ControlledInitializer initializer = new(store,
            (_, _) => Task.FromResult(ProjectSuccess(project)));

        ControlledBuiltInProvider provider = new(async (_, token) =>
        {
            entered.TrySetResult(true);
            await release.Task.WaitAsync(token);
            return Response<ErrorCatalogContext>.Ok(builtIn);
        });
        ErrorCatalogRuntime runtime = CreateRuntime(store, initializer, provider);

        Task<Response<ErrorCatalogInitializationPayload>> reset =
            runtime.ResetToDefaultsAsync();

        try
        {
            await entered.Task.WaitAsync(TestTimeout);

            Task<Response<ErrorCatalogInitializationPayload>> initialize =
                runtime.InitializeAsync();

            Assert.Equal(0, initializer.CallCount);
            release.TrySetResult(true);

            Assert.True((await reset.WaitAsync(TestTimeout)).IsSuccess);
            Assert.True((await initialize.WaitAsync(TestTimeout)).IsSuccess);

            Assert.Equal(1, initializer.CallCount);
            Assert.Equal(2L, Publication(store).Generation);
            Assert.Same(project, Publication(store).Context);
            Assert.Equal(ErrorCatalogRuntimeState.ProjectCatalog, Activation(runtime).Status.State);
            Assert.Equal(2L, Activation(runtime).ActivationSequence);
        }
        finally
        {
            release.TrySetResult(true);
            await reset.WaitAsync(TestTimeout);
        }
    }

    [Fact]
    public async Task InitializationInProgress_BlocksResetUntilItsStatusIsRecorded()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext project = new();
        ErrorCatalogContext builtIn = new();
        TaskCompletionSource<bool> entered = NewSignal();
        TaskCompletionSource<bool> release = NewSignal();

        ControlledInitializer initializer = new(store, async (_, token) =>
        {
            entered.TrySetResult(true);
            await release.Task.WaitAsync(token);
            return ProjectSuccess(project);
        });

        ControlledBuiltInProvider provider = new((_, _) =>
            Task.FromResult(Response<ErrorCatalogContext>.Ok(builtIn)));
        ErrorCatalogRuntime runtime = CreateRuntime(store, initializer, provider);

        Task<Response<ErrorCatalogInitializationPayload>> initialize =
            runtime.InitializeAsync();

        try
        {
            await entered.Task.WaitAsync(TestTimeout);

            Task<Response<ErrorCatalogInitializationPayload>> reset =
                runtime.ResetToDefaultsAsync();

            Assert.Equal(0, provider.CallCount);
            release.TrySetResult(true);

            Assert.True((await initialize.WaitAsync(TestTimeout)).IsSuccess);
            Assert.True((await reset.WaitAsync(TestTimeout)).IsSuccess);

            Assert.Equal(1, provider.CallCount);
            Assert.Equal(2L, Publication(store).Generation);
            Assert.Same(builtIn, Publication(store).Context);
            Assert.Equal(ErrorCatalogRuntimeState.BuiltInDefaults, Activation(runtime).Status.State);
            Assert.Equal(2L, Activation(runtime).ActivationSequence);
        }
        finally
        {
            release.TrySetResult(true);
            await initialize.WaitAsync(TestTimeout);
        }
    }

    [Fact]
    public async Task CancellingQueuedInitialization_DoesNotEnterInitializerOrPublishContext()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = new();
        TaskCompletionSource<bool> entered = NewSignal();
        TaskCompletionSource<bool> release = NewSignal();

        ControlledInitializer initializer = new(store, async (_, token) =>
        {
            entered.TrySetResult(true);
            await release.Task.WaitAsync(token);
            return ProjectSuccess(context);
        });
        ErrorCatalogRuntime runtime = CreateRuntime(store, initializer);

        Task<Response<ErrorCatalogInitializationPayload>> first =
            runtime.InitializeAsync();

        try
        {
            await entered.Task.WaitAsync(TestTimeout);

            using CancellationTokenSource cancellation = new();
            Task<Response<ErrorCatalogInitializationPayload>> queued =
                runtime.InitializeAsync(cancellation.Token);

            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => { await queued.WaitAsync(TestTimeout); });

            Assert.Equal(1, initializer.CallCount);
            release.TrySetResult(true);
            Assert.True((await first.WaitAsync(TestTimeout)).IsSuccess);

            Assert.Equal(1L, Publication(store).Generation);
            Assert.Equal(1L, Activation(runtime).ActivationSequence);
        }
        finally
        {
            release.TrySetResult(true);
            await first.WaitAsync(TestTimeout);
        }
    }

    [Fact]
    public async Task CancellingQueuedReset_DoesNotCallProviderOrReplaceActiveProject()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext active = new();
        ErrorCatalogContext replacement = new();
        TaskCompletionSource<bool> entered = NewSignal();
        TaskCompletionSource<bool> release = NewSignal();

        ControlledInitializer initializer = new(store, async (call, token) =>
        {
            if (call == 2)
            {
                entered.TrySetResult(true);
                await release.Task.WaitAsync(token);
            }

            return ProjectSuccess(call == 1 ? active : replacement);
        });

        ControlledBuiltInProvider provider = new((_, _) =>
            throw new InvalidOperationException("Cancelled reset reached the provider."));
        ErrorCatalogRuntime runtime = CreateRuntime(store, initializer, provider);

        Assert.True((await runtime.InitializeAsync().WaitAsync(TestTimeout)).IsSuccess);
        ErrorCatalogRuntimeStatus recordedStatus =
            Assert.IsType<ErrorCatalogRuntimeStatus>(runtime.GetStatus().Data);
        ErrorCatalogContextPublication initial = Publication(store);
        ErrorCatalogActivationStatusSnapshot initialActivation = Activation(runtime);

        Task<Response<ErrorCatalogInitializationPayload>> inProgress =
            runtime.InitializeAsync();

        try
        {
            await entered.Task.WaitAsync(TestTimeout);

            using CancellationTokenSource cancellation = new();
            Task<Response<ErrorCatalogInitializationPayload>> queuedReset =
                runtime.ResetToDefaultsAsync(cancellation.Token);

            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => { await queuedReset.WaitAsync(TestTimeout); });

            Assert.Equal(0, provider.CallCount);
            Assert.Equal(2, initializer.CallCount);
            Assert.Same(active, runtime.GetCurrentContext().Data);
            Assert.Same(recordedStatus, runtime.GetStatus().Data);
            Assert.Same(initial.Context, Publication(store).Context);
            Assert.Equal(initial.Generation, Publication(store).Generation);
            Assert.Equal(initialActivation.ActivationSequence,
                Activation(runtime).ActivationSequence);

            release.TrySetResult(true);
            Assert.True((await inProgress.WaitAsync(TestTimeout)).IsSuccess);
            Assert.Equal(0, provider.CallCount);
            Assert.Equal(initial.Generation + 1L, Publication(store).Generation);
            Assert.Same(replacement, runtime.GetCurrentContext().Data);
            Assert.Equal(initialActivation.ActivationSequence + 1L,
                Activation(runtime).ActivationSequence);
            Assert.Equal(ErrorCatalogRuntimeState.ProjectCatalog,
                Activation(runtime).Status.State);
        }
        finally
        {
            release.TrySetResult(true);
            await inProgress.WaitAsync(TestTimeout);
        }
    }

    [Fact]
    public async Task CancellingQueuedInitializationBehindReset_DoesNotCallInitializer()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext builtIn = new();
        TaskCompletionSource<bool> entered = NewSignal();
        TaskCompletionSource<bool> release = NewSignal();

        ControlledInitializer initializer = new(store, (_, _) =>
            throw new InvalidOperationException("Cancelled initialization entered the initializer."));

        ControlledBuiltInProvider provider = new(async (_, token) =>
        {
            entered.TrySetResult(true);
            await release.Task.WaitAsync(token);
            return Response<ErrorCatalogContext>.Ok(builtIn);
        });

        ErrorCatalogRuntime runtime = CreateRuntime(store, initializer, provider);
        Task<Response<ErrorCatalogInitializationPayload>> reset =
            runtime.ResetToDefaultsAsync();

        try
        {
            await entered.Task.WaitAsync(TestTimeout);

            using CancellationTokenSource cancellation = new();
            Task<Response<ErrorCatalogInitializationPayload>> queuedInitialization =
                runtime.InitializeAsync(new JsonsOptions(), cancellation.Token);

            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => { await queuedInitialization.WaitAsync(TestTimeout); });

            Assert.Equal(0, initializer.CallCount);
            Assert.Equal(1, provider.CallCount);
            Assert.False(runtime.GetStatus().IsSuccess);
            Assert.False(store.GetCurrentPublication().IsSuccess);

            release.TrySetResult(true);
            Assert.True((await reset.WaitAsync(TestTimeout)).IsSuccess);
            Assert.Equal(0, initializer.CallCount);
            Assert.Equal(1, provider.CallCount);
            Assert.Same(builtIn, runtime.GetCurrentContext().Data);
            Assert.Equal(1L, Publication(store).Generation);
            Assert.Equal(1L, Activation(runtime).ActivationSequence);
            Assert.Equal(ErrorCatalogRuntimeState.BuiltInDefaults,
                Activation(runtime).Status.State);
        }
        finally
        {
            release.TrySetResult(true);
            await reset.WaitAsync(TestTimeout);
        }
    }

    [Fact]
    public async Task FailedFirstInitialization_ReleasesGateForSubsequentSuccessfulAttempt()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = new();
        TaskCompletionSource<bool> entered = NewSignal();
        TaskCompletionSource<bool> release = NewSignal();

        ControlledInitializer initializer = new(store, async (call, token) =>
        {
            if (call == 1)
            {
                entered.TrySetResult(true);
                await release.Task.WaitAsync(token);
                return Response<ErrorCatalogInitializationPayload>.Invalid(
                    code: "ProjectCatalogInvalid",
                    message: "The project catalog is invalid.");
            }

            return ProjectSuccess(context);
        });

        ErrorCatalogRuntime runtime = CreateRuntime(store, initializer,
            options: new WhenItFailsOptions
            {
                InitializationMode = ErrorCatalogInitializationMode.Strict
            });

        Task<Response<ErrorCatalogInitializationPayload>> first =
            runtime.InitializeAsync();

        try
        {
            await entered.Task.WaitAsync(TestTimeout);

            Task<Response<ErrorCatalogInitializationPayload>> second =
                runtime.InitializeAsync();

            Assert.Equal(1, initializer.CallCount);
            release.TrySetResult(true);

            Assert.False((await first.WaitAsync(TestTimeout)).IsSuccess);
            Assert.True((await second.WaitAsync(TestTimeout)).IsSuccess);

            Assert.Equal(2, initializer.CallCount);
            Assert.Equal(1L, Publication(store).Generation);
            Assert.Same(context, Publication(store).Context);
            Assert.Equal(1L, Activation(runtime).ActivationSequence);
        }
        finally
        {
            release.TrySetResult(true);
            await first.WaitAsync(TestTimeout);
        }
    }

    private static TaskCompletionSource<bool> NewSignal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static Response<ErrorCatalogInitializationPayload> ProjectSuccess(
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

    private static ErrorCatalogContextPublication Publication(
        ErrorCatalogContextStore store) =>
        Assert.IsType<ErrorCatalogContextPublication>(
            store.GetCurrentPublication().Data);

    private static ErrorCatalogActivationStatusSnapshot Activation(
        ErrorCatalogRuntime runtime) =>
        Assert.IsType<ErrorCatalogActivationStatusSnapshot>(
            runtime.GetCompletedActivation().Data);

    private static ErrorCatalogRuntime CreateRuntime(
        ErrorCatalogContextStore store,
        IErrorCatalogInitializer initializer,
        IBuiltInErrorCatalogContextProvider? builtIn = null,
        WhenItFailsOptions? options = null) =>
        new(initializer,
            options ?? new WhenItFailsOptions(),
            store,
            builtIn ?? new ControlledBuiltInProvider((_, _) =>
                throw new InvalidOperationException("Unexpected fallback.")),
            new UnusedDescriptorService(),
            new UnusedProfileSelectionService());

    private sealed class ControlledInitializer(
        ErrorCatalogContextStore store,
        Func<int, CancellationToken, Task<Response<ErrorCatalogInitializationPayload>>> run)
        : IErrorCatalogInitializer
    {
        private int _calls;

        public int CallCount => Volatile.Read(ref _calls);

        public async Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options, CancellationToken cancellationToken = default)
        {
            int call = Interlocked.Increment(ref _calls);
            Response<ErrorCatalogInitializationPayload> response =
                await run(call, cancellationToken);

            if (response.IsSuccess && response.Data is { } payload)
            {
                store.Set(payload.Context);
            }

            return response;
        }
    }

    private sealed class ControlledBuiltInProvider(
        Func<int, CancellationToken, Task<Response<ErrorCatalogContext>>> run)
        : IBuiltInErrorCatalogContextProvider
    {
        private int _calls;
        public int CallCount => Volatile.Read(ref _calls);

        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default) =>
            run(Interlocked.Increment(ref _calls), cancellationToken);
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

    private sealed class UnusedProfileSelectionService : IErrorProfileSelectionService
    {
        public Response<IReadOnlyList<ErrorDefinition>> ResolveByProfileName(
            ErrorCatalogContext? context, string profileName) =>
            throw new NotSupportedException();
    }
}
