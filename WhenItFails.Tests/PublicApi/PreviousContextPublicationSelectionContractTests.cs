using System.Reflection;
using System.Text.Json;
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

public sealed class PreviousContextPublicationSelectionContractTests
{
    [Fact]
    public async Task Recovery_SelectsExistingPublicationAndAdvancesStatusWithoutAnotherWrite()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = new();
        ErrorCatalogRuntime runtime = CreateRuntime(
            store,
            new SequencedInitializer(store, call =>
                call == 1 ? ProjectSuccess(context) : InitializationFailure()));

        Assert.True((await runtime.InitializeAsync()).IsSuccess);
        ErrorCatalogContextPublication before =
            Assert.IsType<ErrorCatalogContextPublication>(
                store.GetCurrentPublication().Data);
        ErrorCatalogActivationStatusSnapshot initial =
            Assert.IsType<ErrorCatalogActivationStatusSnapshot>(
                runtime.GetCompletedActivation().Data);

        Response<ErrorCatalogInitializationPayload> recovered =
            await runtime.InitializeAsync();

        Assert.True(recovered.IsSuccess);
        Assert.True(recovered.Data!.KeptPreviousContext);
        Assert.Same(context, recovered.Data.Context);
        Assert.Same(before, store.GetCurrentPublication().Data);

        ErrorCatalogActivationStatusSnapshot after =
            Assert.IsType<ErrorCatalogActivationStatusSnapshot>(
                runtime.GetCompletedActivation().Data);
        Assert.Equal(before.StoreId, after.StoreId);
        Assert.Equal(before.Generation, after.Generation);
        Assert.Equal(initial.ActivationSequence + 1L, after.ActivationSequence);
        Assert.Equal(ErrorCatalogRuntimeState.PreviousContextRecovery, after.Status.State);
        Assert.NotSame(initial.Status, after.Status);

        PropertyInfo selected = Assert.IsAssignableFrom<PropertyInfo>(
            typeof(ErrorCatalogInitializationPayload).GetProperty(
                "SelectedPublication",
                BindingFlags.Instance | BindingFlags.NonPublic));
        Assert.Same(before, selected.GetValue(recovered.Data));
        Assert.False(selected.GetMethod!.IsPublic);
        Assert.DoesNotContain("SelectedPublication",
            JsonSerializer.Serialize(new ErrorCatalogInitializationPayload()));
    }

    [Fact]
    public async Task ExternalSameReferenceRepublishAfterSelection_CannotStealRecoveryIdentity()
    {
        InterferingPublicationStore store = new();
        ErrorCatalogContext context = new();
        store.Set(context);
        ErrorCatalogRuntime runtime = CreateRuntime(store, new AlwaysFailingInitializer());

        store.OnSelected = selected =>
        {
            // The same object reference now belongs to a NEW publication.
            Assert.Same(context, selected.Context);
            store.Set(context);
        };

        Response<ErrorCatalogInitializationPayload> recovered =
            await runtime.InitializeAsync();

        Assert.True(recovered.IsSuccess);
        Assert.True(recovered.Data!.KeptPreviousContext);
        Assert.Same(context, recovered.Data.Context);
        Assert.Equal(2L, store.GetCurrentPublication().Data!.Generation);

        Response<ErrorCatalogActivationStatusSnapshot> completed =
            runtime.GetCompletedActivation();

        Assert.False(completed.IsSuccess);
        Assert.Null(completed.Data);
        Assert.Contains(completed.Issues,
            issue => issue.Code == "WIF_ACTIVATION_PUBLICATION_CHANGED");
        Assert.Equal(ErrorCatalogRuntimeState.PreviousContextRecovery,
            runtime.GetStatus().Data!.State);
        Assert.Equal(0, store.LegacyReadCount);
    }

    [Fact]
    public async Task ExternalDifferentContextRepublish_DoesNotReplaceTheAlreadySelectedRecoveryContext()
    {
        InterferingPublicationStore store = new();
        ErrorCatalogContext original = new();
        ErrorCatalogContext external = new();
        store.Set(original);
        ErrorCatalogRuntime runtime = CreateRuntime(store, new AlwaysFailingInitializer());

        store.OnSelected = _ => store.Set(external);

        Response<ErrorCatalogInitializationPayload> recovered =
            await runtime.InitializeAsync();

        Assert.True(recovered.IsSuccess);
        Assert.True(recovered.Data!.KeptPreviousContext);
        Assert.Same(original, recovered.Data.Context);
        Assert.Same(external, store.Current);
        Assert.Equal(2L, store.GetCurrentPublication().Data!.Generation);
        Assert.Equal(0, store.LegacyReadCount);
        Assert.Contains(runtime.GetCompletedActivation().Issues,
            issue => issue.Code == "WIF_ACTIVATION_PUBLICATION_CHANGED");
    }

    [Fact]
    public async Task LegacyStoreWithoutReader_RetainsPreviousContextAndOriginalStatusContract()
    {
        LegacyStore store = new();
        ErrorCatalogContext context = new();
        store.Set(context);
        ErrorCatalogRuntime runtime = CreateRuntime(store, new AlwaysFailingInitializer());

        Response<ErrorCatalogInitializationPayload> result =
            await runtime.InitializeAsync();

        Assert.True(result.IsSuccess);
        Assert.True(result.Data!.KeptPreviousContext);
        Assert.Same(context, result.Data.Context);
        Assert.Same(context, store.Current);
        Assert.Equal(1, store.LegacyReadCount);
        Assert.Equal(ErrorCatalogRuntimeState.PreviousContextRecovery,
            runtime.GetStatus().Data!.State);
        Assert.False(runtime.GetCompletedActivation().IsSuccess);
    }

    [Fact]
    public async Task FailingOptionalPublicationReader_PreservesLegacyRecoveryWithoutInventingSelection()
    {
        InterferingPublicationStore store = new() { ThrowOnRead = true };
        ErrorCatalogContext context = new();
        store.Set(context);
        ErrorCatalogRuntime runtime = CreateRuntime(store, new AlwaysFailingInitializer());

        Response<ErrorCatalogInitializationPayload> result =
            await runtime.InitializeAsync();

        Assert.True(result.IsSuccess);
        Assert.Same(context, result.Data!.Context);
        Assert.True(result.Data.KeptPreviousContext);
        Assert.Equal(1, store.LegacyReadCount);
        Assert.Equal(ErrorCatalogRuntimeState.PreviousContextRecovery,
            runtime.GetStatus().Data!.State);

        PropertyInfo selected = Assert.IsAssignableFrom<PropertyInfo>(
            typeof(ErrorCatalogInitializationPayload).GetProperty(
                "SelectedPublication",
                BindingFlags.Instance | BindingFlags.NonPublic));
        Assert.Null(selected.GetValue(result.Data));
        Assert.False(runtime.GetCompletedActivation().IsSuccess);
    }

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

    private static Response<ErrorCatalogInitializationPayload> InitializationFailure() =>
        Response<ErrorCatalogInitializationPayload>.Invalid(
            code: "ProjectCatalogInvalid",
            message: "The project catalog is invalid.");

    private static ErrorCatalogRuntime CreateRuntime(
        IErrorCatalogContextStore store, IErrorCatalogInitializer initializer) =>
        new(initializer,
            new WhenItFailsOptions
            {
                InitializationMode = ErrorCatalogInitializationMode.Flexible
            },
            store,
            new UnusedBuiltInProvider(),
            new UnusedDescriptorService(),
            new UnusedProfileSelectionService());

    private sealed class SequencedInitializer(
        IErrorCatalogContextStore store,
        Func<int, Response<ErrorCatalogInitializationPayload>> sequence)
        : IErrorCatalogInitializer
    {
        private int _calls;

        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Response<ErrorCatalogInitializationPayload> result =
                sequence(++_calls);
            if (result.IsSuccess && result.Data is { } payload)
            {
                store.Set(payload.Context);
            }
            return Task.FromResult(result);
        }
    }

    private sealed class AlwaysFailingInitializer : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options, CancellationToken cancellationToken = default) =>
            Task.FromResult(InitializationFailure());
    }

    private sealed class InterferingPublicationStore
        : IErrorCatalogContextStore, IErrorCatalogContextPublicationReader
    {
        private readonly ErrorCatalogContextStore _inner = new();
        private Action<ErrorCatalogContextPublication>? _onSelected;

        public Action<ErrorCatalogContextPublication>? OnSelected
        {
            get => _onSelected;
            set => _onSelected = value;
        }

        public bool ThrowOnRead { get; set; }
        public int LegacyReadCount { get; private set; }
        public bool IsInitialized => _inner.IsInitialized;
        public ErrorCatalogContext? Current => _inner.Current;

        public void Set(ErrorCatalogContext context) => _inner.Set(context);

        public Response<ErrorCatalogContext> GetCurrent()
        {
            LegacyReadCount++;
            return _inner.GetCurrent();
        }

        public Response<ErrorCatalogContextPublication> GetCurrentPublication()
        {
            if (ThrowOnRead)
            {
                throw new InvalidOperationException("Optional reader is unavailable.");
            }

            Response<ErrorCatalogContextPublication> selected =
                _inner.GetCurrentPublication();

            Action<ErrorCatalogContextPublication>? afterRead = _onSelected;
            _onSelected = null;

            if (selected.Data is { } publication)
            {
                afterRead?.Invoke(publication);
            }

            return selected;
        }
    }

    private sealed class LegacyStore : IErrorCatalogContextStore
    {
        public ErrorCatalogContext? Current { get; private set; }
        public int LegacyReadCount { get; private set; }
        public bool IsInitialized => Current is not null;

        public void Set(ErrorCatalogContext context) =>
            Current = context ?? throw new ArgumentNullException(nameof(context));

        public Response<ErrorCatalogContext> GetCurrent()
        {
            LegacyReadCount++;
            return Current is { } context
                ? Response<ErrorCatalogContext>.Ok(context)
                : Response<ErrorCatalogContext>.Invalid(
                    code: "ErrorCatalogContextNotInitialized",
                    message: "No active context.");
        }
    }

    private sealed class UnusedBuiltInProvider : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Unexpected built-in activation.");
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
