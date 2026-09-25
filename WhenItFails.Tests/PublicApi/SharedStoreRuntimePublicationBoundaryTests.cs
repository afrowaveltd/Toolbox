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

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class SharedStoreRuntimePublicationBoundaryTests
{
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task SecondRuntimePublishesDifferentContext_FirstRuntimeRejectsStaleCompletion()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext firstContext = new();
        ErrorCatalogContext secondContext = new();
        ErrorCatalogRuntime first = CreateRuntime(
            store, new PublishingInitializer(store, _ => ProjectSuccess(firstContext)));
        ErrorCatalogRuntime second = CreateRuntime(
            store, new PublishingInitializer(store, _ => ProjectSuccess(secondContext)));

        Assert.True((await first.InitializeAsync()).IsSuccess);
        ErrorCatalogActivationStatusSnapshot original = Activation(first);
        Assert.True((await second.InitializeAsync()).IsSuccess);

        ErrorCatalogActivationStatusSnapshot newest = Activation(second);
        Assert.Equal(original.StoreId, newest.StoreId);
        Assert.Equal(original.Generation + 1L, newest.Generation);
        Assert.Equal(1L, newest.ActivationSequence);
        Assert.Same(secondContext, store.Current);

        Response<ErrorCatalogActivationStatusSnapshot> stale =
            first.GetCompletedActivation();

        Assert.Equal(ResultStatus.Invalid, stale.Status);
        Assert.Null(stale.Data);
        Assert.Contains(stale.Issues,
            issue => issue.Code == "WIF_ACTIVATION_PUBLICATION_CHANGED");
        Assert.Equal(ErrorCatalogRuntimeState.ProjectCatalog,
            first.GetStatus().Data!.State);
        Assert.Same(original.Status, first.GetStatus().Data);
    }

    [Fact]
    public async Task DifferentRuntimesCanOverlap_FirstStatusMayFinishAfterSecondPublication()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext firstContext = new();
        ErrorCatalogContext secondContext = new();
        TaskCompletionSource<bool> publishedFirst = Signal();
        TaskCompletionSource<bool> releaseFirst = Signal();

        PublishingInitializer firstInitializer = new(
            store, _ => ProjectSuccess(firstContext))
        {
            AfterPublication = async token =>
            {
                publishedFirst.TrySetResult(true);
                await releaseFirst.Task.WaitAsync(token);
            }
        };
        ErrorCatalogRuntime first = CreateRuntime(store, firstInitializer);
        ErrorCatalogRuntime second = CreateRuntime(
            store, new PublishingInitializer(store, _ => ProjectSuccess(secondContext)));

        Task<Response<ErrorCatalogInitializationPayload>> firstRun =
            first.InitializeAsync();

        try
        {
            await publishedFirst.Task.WaitAsync(TestTimeout);
            Assert.Equal(1L, Publication(store).Generation);
            Assert.False(first.GetStatus().IsSuccess);

            // This is another runtime instance, so its instance-local
            // activation gate does not wait for the first runtime.
            Assert.True((await second.InitializeAsync().WaitAsync(TestTimeout)).IsSuccess);
            ErrorCatalogContextPublication latest = Publication(store);
            Assert.Equal(2L, latest.Generation);
            Assert.Same(secondContext, latest.Context);

            releaseFirst.TrySetResult(true);
            Assert.True((await firstRun.WaitAsync(TestTimeout)).IsSuccess);

            Assert.Same(latest, Publication(store));
            Assert.Equal(ErrorCatalogRuntimeState.ProjectCatalog,
                first.GetStatus().Data!.State);
            Assert.Equal(ResultStatus.Invalid, first.GetCompletedActivation().Status);
            Assert.Equal("WIF_ACTIVATION_STATUS_UNAVAILABLE",
                Assert.Single(first.GetCompletedActivation().Issues).Code);
            Assert.Equal(latest.Generation, Activation(second).Generation);
            Assert.Equal(1L, Activation(second).ActivationSequence);
        }
        finally
        {
            releaseFirst.TrySetResult(true);
            await firstRun.WaitAsync(TestTimeout);
        }
    }

    [Fact]
    public async Task DirectStoreWriteBetweenPublicationAndStatus_PreventsCompletedMatch()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext runtimeContext = new();
        ErrorCatalogContext externalContext = new();
        PublishingInitializer initializer = new(
            store, _ => ProjectSuccess(runtimeContext))
        {
            AfterPublication = _ =>
            {
                store.Set(externalContext);
                return Task.CompletedTask;
            }
        };
        ErrorCatalogRuntime runtime = CreateRuntime(store, initializer);

        Assert.True((await runtime.InitializeAsync()).IsSuccess);

        ErrorCatalogContextPublication external = Publication(store);
        Assert.Equal(2L, external.Generation);
        Assert.Same(externalContext, external.Context);
        Assert.Equal(ErrorCatalogRuntimeState.ProjectCatalog,
            runtime.GetStatus().Data!.State);
        Assert.Equal(ResultStatus.Invalid, runtime.GetCompletedActivation().Status);
        Assert.Equal("WIF_ACTIVATION_STATUS_UNAVAILABLE",
            Assert.Single(runtime.GetCompletedActivation().Issues).Code);
    }

    [Fact]
    public async Task AnotherRuntimeStrictFailure_DoesNotReplacePublicationOrFirstCompletion()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext active = new();
        ErrorCatalogRuntime first = CreateRuntime(
            store, new PublishingInitializer(store, _ => ProjectSuccess(active)));
        ErrorCatalogRuntime failing = CreateRuntime(
            store, new PublishingInitializer(store, _ =>
                Response<ErrorCatalogInitializationPayload>.Invalid(
                    code: "ProjectCatalogInvalid",
                    message: "The requested project catalog is invalid.")),
            new WhenItFailsOptions
            {
                InitializationMode = ErrorCatalogInitializationMode.Strict
            });

        Assert.True((await first.InitializeAsync()).IsSuccess);
        ErrorCatalogContextPublication before = Publication(store);
        ErrorCatalogActivationStatusSnapshot beforeStatus = Activation(first);

        Assert.False((await failing.InitializeAsync()).IsSuccess);

        Assert.Same(before, Publication(store));
        ErrorCatalogActivationStatusSnapshot afterStatus = Activation(first);
        Assert.Equal(beforeStatus.Generation, afterStatus.Generation);
        Assert.Equal(beforeStatus.ActivationSequence, afterStatus.ActivationSequence);
        Assert.Same(beforeStatus.Status, afterStatus.Status);
        Assert.False(failing.GetStatus().IsSuccess);
    }

    [Fact]
    public async Task TwoRuntimesCanHoldDifferentStatusesForSameStoreGeneration()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = new();
        ErrorCatalogRuntime project = CreateRuntime(
            store, new PublishingInitializer(store, _ => ProjectSuccess(context)));
        ErrorCatalogRuntime recovering = CreateRuntime(
            store, new PublishingInitializer(store, _ =>
                Response<ErrorCatalogInitializationPayload>.Invalid(
                    code: "ProjectCatalogInvalid",
                    message: "The requested project catalog is invalid.")));

        Assert.True((await project.InitializeAsync()).IsSuccess);
        ErrorCatalogContextPublication published = Publication(store);
        Assert.True((await recovering.InitializeAsync()).IsSuccess);

        ErrorCatalogActivationStatusSnapshot projectStatus = Activation(project);
        ErrorCatalogActivationStatusSnapshot recoveryStatus = Activation(recovering);

        Assert.Same(published, Publication(store));
        Assert.Equal(projectStatus.StoreId, recoveryStatus.StoreId);
        Assert.Equal(projectStatus.Generation, recoveryStatus.Generation);
        Assert.Equal(1L, projectStatus.ActivationSequence);
        Assert.Equal(1L, recoveryStatus.ActivationSequence);
        Assert.Equal(ErrorCatalogRuntimeState.ProjectCatalog,
            projectStatus.Status.State);
        Assert.Equal(ErrorCatalogRuntimeState.PreviousContextRecovery,
            recoveryStatus.Status.State);
        Assert.True(recoveryStatus.Status.KeptPreviousContext);
        Assert.Same(context, published.Context);
    }

    private static TaskCompletionSource<bool> Signal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static ErrorCatalogContextPublication Publication(
        ErrorCatalogContextStore store) =>
        Assert.IsType<ErrorCatalogContextPublication>(
            store.GetCurrentPublication().Data);

    private static ErrorCatalogActivationStatusSnapshot Activation(
        ErrorCatalogRuntime runtime) =>
        Assert.IsType<ErrorCatalogActivationStatusSnapshot>(
            runtime.GetCompletedActivation().Data);

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

    private static ErrorCatalogRuntime CreateRuntime(
        ErrorCatalogContextStore store,
        IErrorCatalogInitializer initializer,
        WhenItFailsOptions? options = null) =>
        new(initializer,
            options ?? new WhenItFailsOptions(),
            store,
            new UnusedBuiltInProvider(),
            new UnusedDescriptorService(),
            new UnusedProfileSelectionService());

    private sealed class PublishingInitializer(
        ErrorCatalogContextStore store,
        Func<int, Response<ErrorCatalogInitializationPayload>> create)
        : IErrorCatalogInitializer
    {
        private int _calls;
        public Func<CancellationToken, Task>? AfterPublication { get; init; }

        public async Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Response<ErrorCatalogInitializationPayload> response =
                create(Interlocked.Increment(ref _calls));

            if (response.IsSuccess && response.Data is { } payload)
            {
                store.Set(payload.Context);
                if (AfterPublication is not null)
                {
                    await AfterPublication(cancellationToken);
                }
            }

            return response;
        }
    }

    private sealed class UnusedBuiltInProvider : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Unexpected fallback invocation.");
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
