using System.Reflection;
using System.Text.Json;
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

public sealed class OwnedPublicationActivationBridgeContractTests
{
    [Fact]
    public async Task DefaultInitializer_ReturnsItsExactPublicationWithoutExposingItInPublicPayloadJson()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = new();
        ErrorCatalogInitializer initializer = DefaultInitializer(store, context);

        Response<ErrorCatalogInitializationPayload> response =
            await initializer.InitializeAsync(new JsonsOptions());

        Assert.True(response.IsSuccess);
        ErrorCatalogInitializationPayload payload =
            Assert.IsType<ErrorCatalogInitializationPayload>(response.Data);
        ErrorCatalogContextPublication owned =
            Assert.IsType<ErrorCatalogContextPublication>(OwnedPublication(payload));

        Assert.Same(owned, store.GetCurrentPublication().Data);
        Assert.Same(context, owned.Context);
        Assert.Equal(1L, owned.Generation);

        PropertyInfo property = Assert.IsType<PropertyInfo>(
            typeof(ErrorCatalogInitializationPayload).GetProperty(
                "OwnedPublication",
                BindingFlags.Instance | BindingFlags.NonPublic));
        Assert.False(property.GetMethod!.IsPublic);

        string json = JsonSerializer.Serialize(payload);
        Assert.DoesNotContain("OwnedPublication", json);
        Assert.DoesNotContain("StoreId", json);
        Assert.DoesNotContain("Generation", json);
    }

    [Fact]
    public async Task DefaultInitializerAndRuntime_CompleteAgainstTheirOwnPublication()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = new();
        ErrorCatalogRuntime runtime = CreateRuntime(
            store,
            DefaultInitializer(store, context));

        Assert.True((await runtime.InitializeAsync()).IsSuccess);

        ErrorCatalogActivationStatusSnapshot completed =
            Assert.IsType<ErrorCatalogActivationStatusSnapshot>(
                runtime.GetCompletedActivation().Data);
        Assert.Equal(1L, completed.Generation);
        Assert.Equal(1L, completed.ActivationSequence);
        Assert.Equal(store.GetCurrentPublication().Data!.StoreId, completed.StoreId);
        Assert.Equal(ErrorCatalogRuntimeState.ProjectCatalog, completed.Status.State);
    }

    [Fact]
    public async Task DefaultInitializer_SameReferenceExternalRepublishCannotStealOwnedActivation()
    {
        RepublishSameReferenceStore store = new() { RepublishAfterOwnedWrite = true };
        ErrorCatalogContext context = new();
        ErrorCatalogRuntime runtime = CreateRuntime(
            store,
            DefaultInitializer(store, context));

        Assert.True((await runtime.InitializeAsync()).IsSuccess);
        Assert.Equal(2L, store.GetCurrentPublication().Data!.Generation);
        Assert.Same(context, store.GetCurrentPublication().Data!.Context);
        Assert.Equal(ErrorCatalogRuntimeState.ProjectCatalog,
            runtime.GetStatus().Data!.State);

        // Without carrying the exact Publish return through the initializer,
        // a later read would incorrectly attribute generation 2 to this run.
        Response<ErrorCatalogActivationStatusSnapshot> observed =
            runtime.GetCompletedActivation();

        Assert.Equal(ResultStatus.Invalid, observed.Status);
        Assert.Null(observed.Data);
        Assert.Contains(observed.Issues,
            issue => issue.Code == "WIF_ACTIVATION_PUBLICATION_CHANGED");
    }

    [Fact]
    public async Task ExplicitReset_SameReferenceRepublishDoesNotAcquireRuntimeStatusOwnership()
    {
        RepublishSameReferenceStore store = new() { RepublishAfterOwnedWrite = true };
        ErrorCatalogContext context = new();
        ErrorCatalogRuntime runtime = CreateRuntime(
            store,
            new FailingInitializer(),
            new FixedBuiltInProvider(context));

        Assert.True((await runtime.ResetToDefaultsAsync()).IsSuccess);
        Assert.Equal(2L, store.GetCurrentPublication().Data!.Generation);
        Assert.Equal(ErrorCatalogRuntimeState.BuiltInDefaults,
            runtime.GetStatus().Data!.State);
        Assert.Equal(ResultStatus.Invalid, runtime.GetCompletedActivation().Status);
        Assert.Contains(runtime.GetCompletedActivation().Issues,
            issue => issue.Code == "WIF_ACTIVATION_PUBLICATION_CHANGED");
    }

    [Fact]
    public async Task AutomaticFallback_SameReferenceRepublishDoesNotAcquireRuntimeStatusOwnership()
    {
        RepublishSameReferenceStore store = new() { RepublishAfterOwnedWrite = true };
        ErrorCatalogContext context = new();
        ErrorCatalogRuntime runtime = CreateRuntime(
            store,
            new FailingInitializer(),
            new FixedBuiltInProvider(context),
            new WhenItFailsOptions
            {
                InitializationMode = ErrorCatalogInitializationMode.Flexible
            });

        Response<ErrorCatalogInitializationPayload> result =
            await runtime.InitializeAsync();

        Assert.True(result.IsSuccess);
        Assert.True(result.Data!.UsedFallback);
        Assert.Equal(2L, store.GetCurrentPublication().Data!.Generation);
        Assert.Equal(ErrorCatalogRuntimeState.BuiltInFallback,
            runtime.GetStatus().Data!.State);
        Assert.Equal(ResultStatus.Invalid, runtime.GetCompletedActivation().Status);
        Assert.Contains(runtime.GetCompletedActivation().Issues,
            issue => issue.Code == "WIF_ACTIVATION_PUBLICATION_CHANGED");
    }

    [Fact]
    public async Task LegacyStore_KeepsOriginalInitializeAndResetPathsWithoutOwnedPublication()
    {
        LegacyStore store = new();
        ErrorCatalogContext context = new();
        ErrorCatalogInitializer initializer = DefaultInitializer(store, context);
        ErrorCatalogRuntime runtime = CreateRuntime(
            store, initializer, new FixedBuiltInProvider(new ErrorCatalogContext()));

        Response<ErrorCatalogInitializationPayload> initialized =
            await runtime.InitializeAsync();
        Assert.True(initialized.IsSuccess);
        Assert.Null(OwnedPublication(initialized.Data!));
        Assert.Same(context, store.Current);
        Assert.Equal(ErrorCatalogRuntimeState.ProjectCatalog,
            runtime.GetStatus().Data!.State);
        Assert.Equal(ResultStatus.NotSupported,
            runtime.GetCompletedActivation().Status);

        Assert.True((await runtime.ResetToDefaultsAsync()).IsSuccess);
        Assert.Equal(ErrorCatalogRuntimeState.BuiltInDefaults,
            runtime.GetStatus().Data!.State);
    }

    private static object? OwnedPublication(
        ErrorCatalogInitializationPayload payload) =>
        typeof(ErrorCatalogInitializationPayload)
            .GetProperty("OwnedPublication",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(payload);

    private static ErrorCatalogInitializer DefaultInitializer(
        IErrorCatalogContextStore store, ErrorCatalogContext context) =>
        new(new FixedBootstrapper(), new FixedContextProvider(context), store);

    private static ErrorCatalogRuntime CreateRuntime(
        IErrorCatalogContextStore store,
        IErrorCatalogInitializer initializer,
        IBuiltInErrorCatalogContextProvider? builtIn = null,
        WhenItFailsOptions? options = null) =>
        new(initializer, options ?? new WhenItFailsOptions(), store,
            builtIn ?? new FixedBuiltInProvider(new ErrorCatalogContext()),
            new UnusedDescriptorService(), new UnusedProfileSelectionService());

    private sealed class RepublishSameReferenceStore
        : IErrorCatalogContextStore, IErrorCatalogContextPublisher,
          IErrorCatalogContextPublicationReader
    {
        private readonly ErrorCatalogContextStore _inner = new();

        public bool RepublishAfterOwnedWrite { get; set; }
        public bool IsInitialized => _inner.IsInitialized;
        public ErrorCatalogContext? Current => _inner.Current;

        public Response<ErrorCatalogContext> GetCurrent() => _inner.GetCurrent();

        public Response<ErrorCatalogContextPublication> GetCurrentPublication() =>
            _inner.GetCurrentPublication();

        public void Set(ErrorCatalogContext context) => _ = Publish(context);

        public ErrorCatalogContextPublication Publish(ErrorCatalogContext context)
        {
            ErrorCatalogContextPublication owned = _inner.Publish(context);
            if (RepublishAfterOwnedWrite)
            {
                // Simulates a competing direct writer publishing the SAME
                // context after the successful write but before status recording.
                _inner.Publish(context);
            }
            return owned;
        }
    }

    private sealed class LegacyStore : IErrorCatalogContextStore
    {
        public ErrorCatalogContext? Current { get; private set; }
        public bool IsInitialized => Current is not null;
        public Response<ErrorCatalogContext> GetCurrent() =>
            Current is { } context
                ? Response<ErrorCatalogContext>.Ok(context)
                : Response<ErrorCatalogContext>.Invalid(
                    code: "ErrorCatalogContextNotInitialized",
                    message: "No active context.");
        public void Set(ErrorCatalogContext context) =>
            Current = context ?? throw new ArgumentNullException(nameof(context));
    }

    private sealed class FixedBootstrapper : IJsonsBootstrapper
    {
        public Task<Response<JsonsBootstrapPayload>> EnsureWorkspaceAsync(
            JsonsOptions options, CancellationToken cancellationToken = default) =>
            Task.FromResult(Response<JsonsBootstrapPayload>.Ok(
                new JsonsBootstrapPayload
                {
                    PackageDirectoryPath = "Jsons/WhenItFails"
                }));
    }

    private sealed class FixedContextProvider(ErrorCatalogContext context)
        : IErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
            JsonsOptions options, CancellationToken cancellationToken = default) =>
            Task.FromResult(Response<ErrorCatalogContext>.Ok(context));
    }

    private sealed class FailingInitializer : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options, CancellationToken cancellationToken = default) =>
            Task.FromResult(Response<ErrorCatalogInitializationPayload>.Invalid(
                code: "CatalogInvalid",
                message: "Invalid project catalog."));
    }

    private sealed class FixedBuiltInProvider(ErrorCatalogContext context)
        : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Response<ErrorCatalogContext>.Ok(context));
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
