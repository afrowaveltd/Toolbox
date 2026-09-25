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
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class CompletedCombinedSnapshotContractTests
{
    [Fact]
    public async Task SuccessfulActivation_CombinesOnePublicationWithItsStatusAndDetachedData()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext source = Context("FIRST");
        ErrorCatalogRuntime runtime = CreateRuntime(
            store, new UnusedInitializer(), new FixedBuiltInProvider(source));

        Assert.True((await runtime.ResetToDefaultsAsync()).IsSuccess);
        ErrorCatalogContextPublication publication =
            Assert.IsType<ErrorCatalogContextPublication>(
                store.GetCurrentPublication().Data);

        Response<ErrorCatalogCompletedCombinedSnapshot> response =
            runtime.GetCompletedCombinedSnapshot();

        Assert.True(response.IsSuccess);
        ErrorCatalogCompletedCombinedSnapshot result =
            Assert.IsType<ErrorCatalogCompletedCombinedSnapshot>(response.Data);
        Assert.Equal(publication.StoreId, result.StoreId);
        Assert.Equal(publication.Generation, result.Generation);
        Assert.Equal(1L, result.ActivationSequence);
        Assert.Same(runtime.GetStatus().Data, result.Status);
        Assert.Equal(ErrorCatalogRuntimeState.BuiltInDefaults, result.Status.State);
        Assert.Equal("FIRST-ERROR", Assert.Single(result.Snapshot.Definitions).Id);
        Assert.Equal("FIRST-CATEGORY",
            Assert.Single(result.Snapshot.CategoryCatalog.Categories).Name);
        Assert.Equal("FIRST-WARNING",
            Assert.Single(result.Snapshot.Validation.Issues).Code);

        // Live source modifications cannot retarget the already captured data.
        source.ErrorCatalog.GetAll()[0].Id = "CHANGED";
        source.CategoryCatalog.Categories[0].Name = "CHANGED";
        source.CrossValidationResult.Issues[0].Code = "CHANGED";
        Assert.Equal("FIRST-ERROR", Assert.Single(result.Snapshot.Definitions).Id);
        Assert.Equal("FIRST-CATEGORY",
            Assert.Single(result.Snapshot.CategoryCatalog.Categories).Name);
        Assert.Equal("FIRST-WARNING",
            Assert.Single(result.Snapshot.Validation.Issues).Code);
    }

    [Fact]
    public async Task PreviousContextRecovery_AdvancesStatusSequenceWithoutChangingGeneration()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = Context("PROJECT");
        ErrorCatalogRuntime runtime = CreateRuntime(
            store,
            new SequencedInitializer(store, call => call == 1
                ? ProjectSuccess(context)
                : InitializationFailure()));

        Assert.True((await runtime.InitializeAsync()).IsSuccess);
        ErrorCatalogCompletedCombinedSnapshot initial =
            Assert.IsType<ErrorCatalogCompletedCombinedSnapshot>(
                runtime.GetCompletedCombinedSnapshot().Data);

        Assert.True((await runtime.InitializeAsync()).IsSuccess);
        ErrorCatalogCompletedCombinedSnapshot recovered =
            Assert.IsType<ErrorCatalogCompletedCombinedSnapshot>(
                runtime.GetCompletedCombinedSnapshot().Data);

        Assert.Equal(initial.StoreId, recovered.StoreId);
        Assert.Equal(initial.Generation, recovered.Generation);
        Assert.Equal(initial.ActivationSequence + 1L, recovered.ActivationSequence);
        Assert.Equal(ErrorCatalogRuntimeState.PreviousContextRecovery,
            recovered.Status.State);
        Assert.Equal("PROJECT-ERROR", Assert.Single(recovered.Snapshot.Definitions).Id);
        Assert.Equal(ErrorCatalogRuntimeState.ProjectCatalog, initial.Status.State);
    }

    [Fact]
    public async Task LaterSameReferencePublication_RejectsStaleStatusInsteadOfMixingGenerations()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = Context("SAME");
        ErrorCatalogRuntime runtime = CreateRuntime(
            store, new UnusedInitializer(), new FixedBuiltInProvider(context));

        Assert.True((await runtime.ResetToDefaultsAsync()).IsSuccess);
        store.Set(context);

        Response<ErrorCatalogCompletedCombinedSnapshot> result =
            runtime.GetCompletedCombinedSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_COMPLETED_COMBINED_PUBLICATION_CHANGED");
    }

    [Fact]
    public async Task ExternalPublicationDuringCopy_IsDetectedBySecondRead()
    {
        InterferingStore store = new();
        ErrorCatalogContext context = Context("BEFORE");
        ErrorCatalogRuntime runtime = CreateRuntime(
            store, new UnusedInitializer(), new FixedBuiltInProvider(context));

        Assert.True((await runtime.ResetToDefaultsAsync()).IsSuccess);
        store.OnSecondRead = () => store.Set(Context("AFTER"));

        Response<ErrorCatalogCompletedCombinedSnapshot> result =
            runtime.GetCompletedCombinedSnapshot();

        Assert.Equal(2, store.PublicationReads);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_COMPLETED_COMBINED_PUBLICATION_CHANGED");
        Assert.Equal("AFTER-ERROR", store.Current!.ErrorCatalog.GetAll()[0].Id);
    }

    [Fact]
    public async Task MissingRequiredCatalog_ReturnsStructuredFailureWithoutPartialData()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = Context("MISSING");
        context.CategoryCatalog = null!;
        ErrorCatalogRuntime runtime = CreateRuntime(
            store, new UnusedInitializer(), new FixedBuiltInProvider(context));

        Assert.True((await runtime.ResetToDefaultsAsync()).IsSuccess);
        Response<ErrorCatalogCompletedCombinedSnapshot> result =
            runtime.GetCompletedCombinedSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_COMBINED_SNAPSHOT_CATEGORY_CATALOG_NULL");
    }

    [Fact]
    public void OptionalPublicContract_LeavesLegacyRuntimeUnchanged()
    {
        ErrorCatalogRuntime legacy = CreateRuntime(
            new LegacyStore(), new UnusedInitializer(),
            new FixedBuiltInProvider(Context("LEGACY")));

        Response<ErrorCatalogCompletedCombinedSnapshot> unsupported =
            legacy.GetCompletedCombinedSnapshot();

        Assert.Equal(ResultStatus.NotSupported, unsupported.Status);
        Assert.Null(unsupported.Data);
        Assert.Contains(unsupported.Issues,
            issue => issue.Code == "WIF_COMPLETED_COMBINED_NOT_SUPPORTED");

        Type type = typeof(ErrorCatalogCompletedCombinedSnapshot);
        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        PropertyInfo[] properties = type.GetProperties(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Equal(5, properties.Length);
        Assert.All(properties, property => Assert.Null(property.SetMethod));
        Assert.DoesNotContain(properties,
            property => property.PropertyType == typeof(ErrorCatalogContext));
        Assert.DoesNotContain(typeof(IErrorCatalogRuntime).GetMethods(),
            method => method.Name == "GetCompletedCombinedSnapshot");
        Assert.Contains(typeof(IErrorCatalogRuntimeCombinedObservationReader),
            typeof(ErrorCatalogRuntime).GetInterfaces());
    }

    private static ErrorCatalogContext Context(string prefix)
    {
        ErrorDefinition definition = new()
        {
            Id = prefix + "-ERROR",
            Name = prefix + "_ERROR"
        };
        ErrorCategoryDefinition category = new() { Name = prefix + "-CATEGORY" };
        ErrorCatalogValidationResult validation = new();
        validation.AddWarning(prefix + "-WARNING", "Recorded warning");
        return new ErrorCatalogContext
        {
            ErrorCatalog = new ErrorCatalog([definition]),
            CategoryCatalog = new ErrorCategoryCatalogDocument
            {
                Categories = [category]
            },
            CrossValidationResult = validation
        };
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
        IErrorCatalogContextStore store,
        IErrorCatalogInitializer initializer,
        IBuiltInErrorCatalogContextProvider builtIn) =>
        new(initializer,
            new WhenItFailsOptions
            {
                InitializationMode = ErrorCatalogInitializationMode.Flexible
            },
            store, builtIn,
            new UnusedDescriptorService(), new UnusedProfileSelectionService());

    private sealed class SequencedInitializer(
        ErrorCatalogContextStore store,
        Func<int, Response<ErrorCatalogInitializationPayload>> run)
        : IErrorCatalogInitializer
    {
        private int _calls;

        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Response<ErrorCatalogInitializationPayload> result = run(++_calls);
            if (result.IsSuccess && result.Data is { } payload)
            {
                store.Set(payload.Context);
            }
            return Task.FromResult(result);
        }
    }

    private sealed class FixedBuiltInProvider(ErrorCatalogContext context)
        : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Response<ErrorCatalogContext>.Ok(context));
    }

    private sealed class UnusedInitializer : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class InterferingStore
        : IErrorCatalogContextStore, IErrorCatalogContextPublicationReader,
          IErrorCatalogContextPublisher
    {
        private readonly ErrorCatalogContextStore _inner = new();
        private int _reads;

        public Action? OnSecondRead { get; set; }
        public int PublicationReads => _reads;
        public bool IsInitialized => _inner.IsInitialized;
        public ErrorCatalogContext? Current => _inner.Current;
        public Response<ErrorCatalogContext> GetCurrent() => _inner.GetCurrent();
        public void Set(ErrorCatalogContext context) => _inner.Set(context);
        public ErrorCatalogContextPublication Publish(ErrorCatalogContext context) =>
            _inner.Publish(context);

        public Response<ErrorCatalogContextPublication> GetCurrentPublication()
        {
            if (++_reads == 2)
            {
                OnSecondRead?.Invoke();
            }
            return _inner.GetCurrentPublication();
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
