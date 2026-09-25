using System.Reflection;
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

public sealed class CompletedActivationStatusContractTests
{
    [Fact]
    public void BeforeInitialization_IsUnavailableAndLegacyStoreIsUnsupported()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogRuntime runtime = CreateRuntime(
            store, new SequenceInitializer(store, _ => InitFailure()));

        Response<ErrorCatalogActivationStatusSnapshot> before =
            runtime.GetCompletedActivation();

        Assert.Equal(ResultStatus.Invalid, before.Status);
        Assert.Null(before.Data);
        Assert.Contains(before.Issues,
            issue => issue.Code == "WIF_ACTIVATION_STATUS_UNAVAILABLE");

        ErrorCatalogRuntime legacy = CreateRuntime(
            new LegacyStore(),
            new SequenceInitializer(store, _ => InitFailure()));

        Response<ErrorCatalogActivationStatusSnapshot> unsupported =
            legacy.GetCompletedActivation();

        Assert.Equal(ResultStatus.NotSupported, unsupported.Status);
        Assert.Null(unsupported.Data);
        Assert.Contains(unsupported.Issues,
            issue => issue.Code == "WIF_ACTIVATION_STATUS_NOT_SUPPORTED");
    }

    [Fact]
    public async Task TwoSuccessfulInitializations_AdvanceBothGenerationAndActivationSequence()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext firstContext = new();
        ErrorCatalogContext secondContext = new();
        ErrorCatalogRuntime runtime = CreateRuntime(
            store,
            new SequenceInitializer(store,
                call => InitSuccess(call == 1 ? firstContext : secondContext)));

        Assert.True((await runtime.InitializeAsync()).IsSuccess);
        ErrorCatalogActivationStatusSnapshot first = CurrentActivation(runtime);
        Assert.Equal(1L, first.Generation);
        Assert.Equal(1L, first.ActivationSequence);
        Assert.Equal(ErrorCatalogRuntimeState.ProjectCatalog, first.Status.State);
        Assert.Same(runtime.GetStatus().Data, first.Status);

        Assert.True((await runtime.InitializeAsync()).IsSuccess);
        ErrorCatalogActivationStatusSnapshot second = CurrentActivation(runtime);
        Assert.Equal(first.StoreId, second.StoreId);
        Assert.Equal(2L, second.Generation);
        Assert.Equal(2L, second.ActivationSequence);
        Assert.Equal(ErrorCatalogRuntimeState.ProjectCatalog, second.Status.State);
        Assert.NotSame(first.Status, second.Status);
        Assert.Same(runtime.GetStatus().Data, second.Status);
        Assert.Equal(1L, first.Generation);
        Assert.Equal(1L, first.ActivationSequence);
    }

    [Fact]
    public async Task FlexiblePreviousContextRecovery_AdvancesStatusSequenceButRetainsGeneration()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = new();
        ErrorCatalogRuntime runtime = CreateRuntime(
            store,
            new SequenceInitializer(store,
                call => call == 1 ? InitSuccess(context) : InitFailure()),
            new WhenItFailsOptions
            {
                InitializationMode = ErrorCatalogInitializationMode.Flexible
            });

        Assert.True((await runtime.InitializeAsync()).IsSuccess);
        ErrorCatalogActivationStatusSnapshot initial = CurrentActivation(runtime);
        ErrorCatalogContextPublication published =
            Assert.IsType<ErrorCatalogContextPublication>(
                store.GetCurrentPublication().Data);

        Response<ErrorCatalogInitializationPayload> recovered =
            await runtime.InitializeAsync();

        Assert.True(recovered.IsSuccess);
        Assert.True(recovered.Data!.KeptPreviousContext);
        ErrorCatalogActivationStatusSnapshot after = CurrentActivation(runtime);

        Assert.Equal(initial.StoreId, after.StoreId);
        Assert.Equal(initial.Generation, after.Generation);
        Assert.Equal(initial.ActivationSequence + 1L, after.ActivationSequence);
        Assert.Same(published, store.GetCurrentPublication().Data);
        Assert.Equal(ErrorCatalogRuntimeState.PreviousContextRecovery, after.Status.State);
        Assert.Same(runtime.GetStatus().Data, after.Status);
        Assert.Equal(ErrorCatalogRuntimeState.ProjectCatalog, initial.Status.State);
    }

    [Fact]
    public async Task ExternalSetIncludingSameContext_InvalidatesCompletedObservation()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = new();
        ErrorCatalogRuntime runtime = CreateRuntime(
            store,
            new SequenceInitializer(store, _ => InitSuccess(context)));

        Assert.True((await runtime.InitializeAsync()).IsSuccess);
        ErrorCatalogActivationStatusSnapshot completed = CurrentActivation(runtime);

        // Even publishing the identical mutable object creates a distinct
        // publication that this runtime has not associated with a new status.
        store.Set(context);
        Response<ErrorCatalogActivationStatusSnapshot> result =
            runtime.GetCompletedActivation();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_ACTIVATION_PUBLICATION_CHANGED");
        Assert.Equal(completed.Generation + 1L,
            store.GetCurrentPublication().Data!.Generation);
    }

    [Fact]
    public async Task BetweenStorePublicationAndStatusUpdate_RejectsUnpairedGeneration()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext first = new();
        ErrorCatalogContext second = new();
        SequenceInitializer initializer = new(store,
            call => InitSuccess(call == 1 ? first : second));
        ErrorCatalogRuntime runtime = CreateRuntime(store, initializer);

        Assert.True((await runtime.InitializeAsync()).IsSuccess);
        ErrorCatalogActivationStatusSnapshot previous = CurrentActivation(runtime);
        Response<ErrorCatalogActivationStatusSnapshot>? during = null;

        initializer.OnPublished = () =>
        {
            during = runtime.GetCompletedActivation();
        };

        Assert.True((await runtime.InitializeAsync()).IsSuccess);
        Assert.NotNull(during);
        Assert.Equal(ResultStatus.Invalid, during.Status);
        Assert.Null(during.Data);
        Assert.Contains(during.Issues,
            issue => issue.Code == "WIF_ACTIVATION_PUBLICATION_CHANGED");

        ErrorCatalogActivationStatusSnapshot completed = CurrentActivation(runtime);
        Assert.Equal(previous.Generation + 1L, completed.Generation);
        Assert.Equal(previous.ActivationSequence + 1L, completed.ActivationSequence);
    }

    [Fact]
    public async Task FailedStrictReinitializationAndReset_PreserveCompletedObservation()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext original = new();
        ErrorCatalogRuntime runtime = CreateRuntime(
            store,
            new SequenceInitializer(store,
                call => call == 1 ? InitSuccess(original) : InitFailure()),
            new WhenItFailsOptions
            {
                InitializationMode = ErrorCatalogInitializationMode.Strict
            },
            new SequenceBuiltInProvider(_ =>
                Response<ErrorCatalogContext>.Invalid(
                    code: "BuiltInCatalogInvalid",
                    message: "No defaults could be activated.")));

        Assert.True((await runtime.InitializeAsync()).IsSuccess);
        ErrorCatalogActivationStatusSnapshot before = CurrentActivation(runtime);

        Assert.False((await runtime.InitializeAsync()).IsSuccess);
        Assert.False((await runtime.ResetToDefaultsAsync()).IsSuccess);

        ErrorCatalogActivationStatusSnapshot after = CurrentActivation(runtime);
        Assert.Equal(before.StoreId, after.StoreId);
        Assert.Equal(before.Generation, after.Generation);
        Assert.Equal(before.ActivationSequence, after.ActivationSequence);
        Assert.Same(before.Status, after.Status);
        Assert.Same(store.GetCurrentPublication().Data!.Context, original);
    }

    [Fact]
    public async Task FallbackThenExplicitReset_AdvanceBothSequencesWithDistinctStatus()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext fallback = new();
        ErrorCatalogContext reset = new();
        ErrorCatalogRuntime runtime = CreateRuntime(
            store,
            new SequenceInitializer(store, _ => InitFailure()),
            new WhenItFailsOptions
            {
                InitializationMode = ErrorCatalogInitializationMode.Flexible
            },
            new SequenceBuiltInProvider(call =>
                Response<ErrorCatalogContext>.Ok(call == 1 ? fallback : reset)));

        Assert.True((await runtime.InitializeAsync()).IsSuccess);
        ErrorCatalogActivationStatusSnapshot first = CurrentActivation(runtime);
        Assert.Equal(1L, first.Generation);
        Assert.Equal(1L, first.ActivationSequence);
        Assert.Equal(ErrorCatalogRuntimeState.BuiltInFallback, first.Status.State);

        Assert.True((await runtime.ResetToDefaultsAsync()).IsSuccess);
        ErrorCatalogActivationStatusSnapshot second = CurrentActivation(runtime);
        Assert.Equal(first.StoreId, second.StoreId);
        Assert.Equal(first.Generation + 1L, second.Generation);
        Assert.Equal(first.ActivationSequence + 1L, second.ActivationSequence);
        Assert.Equal(ErrorCatalogRuntimeState.BuiltInDefaults, second.Status.State);
    }

    [Fact]
    public void PublicShape_IsAdditiveAndContainsNoLiveContext()
    {
        Type type = typeof(ErrorCatalogActivationStatusSnapshot);
        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.Empty(type.GetConstructors(
            BindingFlags.Public | BindingFlags.Instance));

        PropertyInfo[] properties = type.GetProperties(
            BindingFlags.Public | BindingFlags.Instance |
            BindingFlags.DeclaredOnly);

        Assert.Equal(4, properties.Length);
        Assert.All(properties, property => Assert.Null(property.SetMethod));
        Assert.Equal(typeof(Guid), type.GetProperty("StoreId")!.PropertyType);
        Assert.Equal(typeof(long), type.GetProperty("Generation")!.PropertyType);
        Assert.Equal(typeof(long), type.GetProperty("ActivationSequence")!.PropertyType);
        Assert.Equal(typeof(ErrorCatalogRuntimeStatus),
            type.GetProperty("Status")!.PropertyType);
        Assert.DoesNotContain(properties,
            property => property.PropertyType == typeof(ErrorCatalogContext));
        Assert.DoesNotContain(typeof(IErrorCatalogRuntime).GetMethods(),
            method => method.Name == "GetCompletedActivation");
        Assert.Contains(typeof(IErrorCatalogRuntimeActivationReader),
            typeof(ErrorCatalogRuntime).GetInterfaces());
    }

    private static Response<ErrorCatalogInitializationPayload> InitSuccess(
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

    private static Response<ErrorCatalogInitializationPayload> InitFailure() =>
        Response<ErrorCatalogInitializationPayload>.Invalid(
            code: "CatalogDocumentsInvalid",
            message: "Project catalog is invalid.");

    private static ErrorCatalogActivationStatusSnapshot CurrentActivation(
        ErrorCatalogRuntime runtime) =>
        Assert.IsType<ErrorCatalogActivationStatusSnapshot>(
            runtime.GetCompletedActivation().Data);

    private static ErrorCatalogRuntime CreateRuntime(
        IErrorCatalogContextStore store,
        IErrorCatalogInitializer initializer,
        WhenItFailsOptions? options = null,
        IBuiltInErrorCatalogContextProvider? builtIn = null) =>
        new(initializer,
            options ?? new WhenItFailsOptions(),
            store,
            builtIn ?? new SequenceBuiltInProvider(_ =>
                throw new InvalidOperationException("Unexpected built-in activation.")),
            new UnusedDescriptorService(),
            new UnusedProfileSelectionService());

    private sealed class SequenceInitializer(
        ErrorCatalogContextStore store,
        Func<int, Response<ErrorCatalogInitializationPayload>> sequence)
        : IErrorCatalogInitializer
    {
        private int _calls;
        public Action? OnPublished { get; set; }

        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options, CancellationToken cancellationToken = default)
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

    private sealed class SequenceBuiltInProvider(
        Func<int, Response<ErrorCatalogContext>> sequence)
        : IBuiltInErrorCatalogContextProvider
    {
        private int _calls;

        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(sequence(++_calls));
        }
    }

    private sealed class LegacyStore : IErrorCatalogContextStore
    {
        public bool IsInitialized => false;
        public ErrorCatalogContext? Current => null;
        public Response<ErrorCatalogContext> GetCurrent() =>
            throw new NotSupportedException();
        public void Set(ErrorCatalogContext context) =>
            throw new NotSupportedException();
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
