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

public sealed class CompletedFullSnapshotContractTests
{
    [Fact]
    public async Task CompletedCapture_UsesTheSelectedActivationStatusAndDetachesAllFourCatalogs()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext source = Context("FIRST");
        ErrorCatalogRuntime runtime = CreateRuntime(
            store, new UnusedInitializer(), new FixedBuiltInProvider(source));

        Assert.True((await runtime.ResetToDefaultsAsync()).IsSuccess);
        ErrorCatalogContextPublication publication =
            Assert.IsType<ErrorCatalogContextPublication>(
                store.GetCurrentPublication().Data);

        Response<ErrorCatalogCompletedFullSnapshot> response =
            runtime.GetCompletedFullSnapshot();
        Assert.True(response.IsSuccess);
        ErrorCatalogCompletedFullSnapshot observation =
            Assert.IsType<ErrorCatalogCompletedFullSnapshot>(response.Data);

        Assert.Equal(publication.StoreId, observation.StoreId);
        Assert.Equal(publication.Generation, observation.Generation);
        Assert.Equal(1L, observation.ActivationSequence);
        Assert.Same(runtime.GetStatus().Data, observation.Status);
        Assert.Equal(ErrorCatalogRuntimeState.BuiltInDefaults, observation.Status.State);

        Assert.Equal("FIRST-ERROR",
            Assert.Single(observation.Snapshot.Definitions).Id);
        Assert.Equal("FIRST-WARNING",
            Assert.Single(observation.Snapshot.Validation.Issues).Code);
        Assert.True(observation.Snapshot.Validation.IsValid);
        Assert.Equal("FIRST-CATEGORY",
            Assert.Single(observation.Snapshot.CategoryCatalog.Categories).Name);
        Assert.Equal("FIRST-OWNER",
            Assert.Single(observation.Snapshot.OwnerCatalog.Owners).Name);
        Assert.Equal("FIRST-GROUP",
            Assert.Single(observation.Snapshot.CodeGroupCatalog.CodeGroups).Name);
        Assert.Equal("FIRST-PROFILE",
            Assert.Single(observation.Snapshot.ProfileCatalog.Profiles).Name);

        source.ErrorCatalog.GetAll()[0].Id = "CHANGED";
        source.CrossValidationResult.Issues[0].Code = "CHANGED";
        source.CategoryCatalog.Categories[0].Name = "CHANGED";
        source.OwnerCatalog.Owners[0].Aliases.Clear();
        source.CodeGroupCatalog.CodeGroups[0].DefaultCategories.Clear();
        source.ProfileCatalog.Profiles[0].IncludeOwners.Clear();

        Assert.Equal("FIRST-ERROR",
            Assert.Single(observation.Snapshot.Definitions).Id);
        Assert.Equal("FIRST-WARNING",
            Assert.Single(observation.Snapshot.Validation.Issues).Code);
        Assert.Equal("FIRST-CATEGORY",
            Assert.Single(observation.Snapshot.CategoryCatalog.Categories).Name);
        Assert.Equal("FIRST-ALIAS",
            Assert.Single(Assert.Single(observation.Snapshot.OwnerCatalog.Owners).Aliases));
        Assert.Equal("FIRST-CATEGORY",
            Assert.Single(Assert.Single(observation.Snapshot.CodeGroupCatalog.CodeGroups).DefaultCategories));
        Assert.Equal("FIRST-OWNER",
            Assert.Single(Assert.Single(observation.Snapshot.ProfileCatalog.Profiles).IncludeOwners));
    }

    [Fact]
    public async Task PreviousContextRecovery_AdvancesActivationSequenceWithoutRepublishingCatalogs()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = Context("PROJECT");
        ErrorCatalogRuntime runtime = CreateRuntime(
            store,
            new SequencedInitializer(store, call => call == 1
                ? ProjectSuccess(context)
                : Response<ErrorCatalogInitializationPayload>.Invalid(
                    code: "ProjectCatalogInvalid",
                    message: "Project configuration is invalid.")),
            new FixedBuiltInProvider(Context("UNUSED")));

        Assert.True((await runtime.InitializeAsync()).IsSuccess);
        ErrorCatalogCompletedFullSnapshot first =
            Assert.IsType<ErrorCatalogCompletedFullSnapshot>(
                runtime.GetCompletedFullSnapshot().Data);

        Assert.True((await runtime.InitializeAsync()).IsSuccess);
        ErrorCatalogCompletedFullSnapshot recovered =
            Assert.IsType<ErrorCatalogCompletedFullSnapshot>(
                runtime.GetCompletedFullSnapshot().Data);

        Assert.Equal(first.StoreId, recovered.StoreId);
        Assert.Equal(first.Generation, recovered.Generation);
        Assert.Equal(first.ActivationSequence + 1L, recovered.ActivationSequence);
        Assert.Equal(ErrorCatalogRuntimeState.ProjectCatalog, first.Status.State);
        Assert.Equal(ErrorCatalogRuntimeState.PreviousContextRecovery, recovered.Status.State);
        Assert.Equal("PROJECT-PROFILE",
            Assert.Single(recovered.Snapshot.ProfileCatalog.Profiles).Name);
    }

    [Fact]
    public async Task RepublishingSameContextReference_RejectsStaleCompletedStatus()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext source = Context("SAME");
        ErrorCatalogRuntime runtime = CreateRuntime(
            store, new UnusedInitializer(), new FixedBuiltInProvider(source));

        Assert.True((await runtime.ResetToDefaultsAsync()).IsSuccess);
        store.Set(source);

        Response<ErrorCatalogCompletedFullSnapshot> result =
            runtime.GetCompletedFullSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_COMPLETED_FULL_PUBLICATION_CHANGED");
    }

    [Fact]
    public async Task ExternalPublicationDuringCopy_IsDetectedBySecondPublicationRead()
    {
        InterferingStore store = new();
        ErrorCatalogRuntime runtime = CreateRuntime(
            store, new UnusedInitializer(), new FixedBuiltInProvider(Context("FIRST")));
        Assert.True((await runtime.ResetToDefaultsAsync()).IsSuccess);
        store.OnSecondRead = () => store.Set(Context("SECOND"));

        Response<ErrorCatalogCompletedFullSnapshot> result =
            runtime.GetCompletedFullSnapshot();

        Assert.Equal(2, store.PublicationReads);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_COMPLETED_FULL_PUBLICATION_CHANGED");
    }

    [Fact]
    public async Task RecoveryDuringCopy_WithSameGeneration_RejectsChangedStatus()
    {
        InterferingStore store = new();
        ErrorCatalogRuntime runtime = CreateRuntime(
            store, new UnusedInitializer(), new FixedBuiltInProvider(Context("FIRST")));
        Assert.True((await runtime.ResetToDefaultsAsync()).IsSuccess);

        // A flexible recovery changes the completed status without writing a
        // new context generation. Neither two matching store reads nor the
        // same context reference alone can justify returning stale status.
        store.OnSecondRead = () =>
            Assert.True(runtime.InitializeAsync().GetAwaiter().GetResult().IsSuccess);

        Response<ErrorCatalogCompletedFullSnapshot> result =
            runtime.GetCompletedFullSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_COMPLETED_FULL_STATUS_CHANGED");
        Assert.Equal(1L, store.CurrentPublication.Generation);
        Assert.Equal(ErrorCatalogRuntimeState.PreviousContextRecovery,
            runtime.GetStatus().Data!.State);
    }

    [Fact]
    public void BeforeAnyCompletedActivation_ReturnsUnavailableWithoutInventingIdentity()
    {
        ErrorCatalogRuntime runtime = CreateRuntime(
            new ErrorCatalogContextStore(), new UnusedInitializer(),
            new FixedBuiltInProvider(Context("UNUSED")));

        Response<ErrorCatalogCompletedFullSnapshot> result =
            runtime.GetCompletedFullSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_COMPLETED_FULL_UNAVAILABLE");
    }

    [Fact]
    public void LegacyStore_ReturnsNotSupportedWithoutFallbackGeneration()
    {
        ErrorCatalogRuntime runtime = CreateRuntime(
            new LegacyStore(), new UnusedInitializer(),
            new FixedBuiltInProvider(Context("UNUSED")));

        Response<ErrorCatalogCompletedFullSnapshot> result =
            runtime.GetCompletedFullSnapshot();

        Assert.Equal(ResultStatus.NotSupported, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_COMPLETED_FULL_NOT_SUPPORTED");
    }

    [Theory]
    [InlineData(0, "WIF_COMBINED_SNAPSHOT_CATEGORY_CATALOG_NULL")]
    [InlineData(1, "WIF_SUPPORTING_SNAPSHOT_OWNER_CATALOG_NULL")]
    [InlineData(2, "WIF_SUPPORTING_SNAPSHOT_CODE_GROUP_CATALOG_NULL")]
    [InlineData(3, "WIF_SUPPORTING_SNAPSHOT_PROFILE_CATALOG_NULL")]
    public async Task MissingSupportingCatalog_RejectsIncompleteObservation(
        int missing, string expectedCode)
    {
        ErrorCatalogContext context = Context("FIRST");
        switch (missing)
        {
            case 0: context.CategoryCatalog = null!; break;
            case 1: context.OwnerCatalog = null!; break;
            case 2: context.CodeGroupCatalog = null!; break;
            case 3: context.ProfileCatalog = null!; break;
        }
        ErrorCatalogRuntime runtime = CreateRuntime(
            new ErrorCatalogContextStore(), new UnusedInitializer(),
            new FixedBuiltInProvider(context));
        Assert.True((await runtime.ResetToDefaultsAsync()).IsSuccess);

        Response<ErrorCatalogCompletedFullSnapshot> result =
            runtime.GetCompletedFullSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues, issue => issue.Code == expectedCode);
    }

    [Fact]
    public async Task MalformedNestedProfile_ReturnsStableFailureWithoutPartialSnapshot()
    {
        ErrorCatalogContext source = Context("FIRST");
        source.ProfileCatalog.Profiles[0].IncludeErrors = null!;
        ErrorCatalogRuntime runtime = CreateRuntime(
            new ErrorCatalogContextStore(), new UnusedInitializer(),
            new FixedBuiltInProvider(source));
        Assert.True((await runtime.ResetToDefaultsAsync()).IsSuccess);

        Response<ErrorCatalogCompletedFullSnapshot> result =
            runtime.GetCompletedFullSnapshot();

        Assert.Equal(ResultStatus.Failed, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_SUPPORTING_SNAPSHOT_FAILED");
        Assert.DoesNotContain("NullReferenceException", result.Message);
    }

    [Fact]
    public async Task SecondPublicationReadOrdinaryException_IsNormalizedWithoutDetails()
    {
        InterferingStore store = new();
        ErrorCatalogRuntime runtime = CreateRuntime(
            store, new UnusedInitializer(), new FixedBuiltInProvider(Context("FIRST")));
        Assert.True((await runtime.ResetToDefaultsAsync()).IsSuccess);
        store.OnSecondRead = () => throw new InvalidOperationException("Sensitive store details.");

        Response<ErrorCatalogCompletedFullSnapshot> result =
            runtime.GetCompletedFullSnapshot();

        Assert.Equal(ResultStatus.Failed, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_COMPLETED_FULL_FAILED");
        Assert.DoesNotContain("Sensitive store details.", result.Message);
    }

    [Fact]
    public async Task PublicationReaderCancellation_PropagatesExactException()
    {
        InterferingStore store = new();
        ErrorCatalogRuntime runtime = CreateRuntime(
            store, new UnusedInitializer(), new FixedBuiltInProvider(Context("FIRST")));
        Assert.True((await runtime.ResetToDefaultsAsync()).IsSuccess);
        OperationCanceledException cancellation = new("Cancellation from publication reader.");
        store.OnSecondRead = () => throw cancellation;

        OperationCanceledException actual = Assert.Throws<OperationCanceledException>(
            () => runtime.GetCompletedFullSnapshot());

        Assert.Same(cancellation, actual);
    }

    [Fact]
    public void OptionalInterfaceAndGetterOnlyModel_LeaveLegacyContractsUnchanged()
    {
        Assert.Contains(typeof(IErrorCatalogRuntimeFullObservationReader),
            typeof(ErrorCatalogRuntime).GetInterfaces());
        Assert.DoesNotContain(typeof(IErrorCatalogRuntime).GetMethods(),
            method => method.Name == "GetCompletedFullSnapshot");
        Assert.DoesNotContain(typeof(IErrorCatalogRuntimeCombinedObservationReader)
            .GetMethods(), method => method.Name == "GetCompletedFullSnapshot");
        Assert.DoesNotContain(typeof(IErrorCatalogRuntimeSupportingObservationReader)
            .GetMethods(), method => method.Name == "GetCompletedFullSnapshot");

        Type type = typeof(ErrorCatalogCompletedFullSnapshot);
        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        PropertyInfo[] properties = type.GetProperties(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Equal(5, properties.Length);
        Assert.All(properties, property => Assert.Null(property.SetMethod));
        Assert.Equal(typeof(Guid), type.GetProperty("StoreId")!.PropertyType);
        Assert.Equal(typeof(long), type.GetProperty("Generation")!.PropertyType);
        Assert.Equal(typeof(long), type.GetProperty("ActivationSequence")!.PropertyType);
        Assert.Equal(typeof(ErrorCatalogRuntimeStatus),
            type.GetProperty("Status")!.PropertyType);
        Assert.Equal(typeof(ErrorCatalogFullSnapshot),
            type.GetProperty("Snapshot")!.PropertyType);
        Assert.DoesNotContain(properties,
            property => property.PropertyType == typeof(ErrorCatalogContext));
    }

    [Theory]
    [InlineData(0, "WIF_COMBINED_SNAPSHOT_ERROR_CATALOG_NULL")]
    [InlineData(1, "WIF_COMBINED_SNAPSHOT_VALIDATION_RESULT_NULL")]
    public async Task MissingMainCatalogOrValidation_ReturnsInvalidWithoutPartialData(
        int missing, string expectedCode)
    {
        ErrorCatalogContext context = Context("FIRST");
        if (missing == 0)
        {
            context.ErrorCatalog = null!;
        }
        else
        {
            context.CrossValidationResult = null!;
        }

        ErrorCatalogRuntime runtime = CreateRuntime(
            new ErrorCatalogContextStore(), new UnusedInitializer(),
            new FixedBuiltInProvider(context));
        Assert.True((await runtime.ResetToDefaultsAsync()).IsSuccess);

        Response<ErrorCatalogCompletedFullSnapshot> result =
            runtime.GetCompletedFullSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues, issue => issue.Code == expectedCode);
    }

    [Fact]
    public async Task MainDefinitionCaptureFailure_ReturnsFailedWithoutPartialData()
    {
        ErrorCatalogContext context = Context("FIRST");
        context.ErrorCatalog.GetAll()[0].Categories = null!;
        ErrorCatalogRuntime runtime = CreateRuntime(
            new ErrorCatalogContextStore(), new UnusedInitializer(),
            new FixedBuiltInProvider(context));
        Assert.True((await runtime.ResetToDefaultsAsync()).IsSuccess);

        Response<ErrorCatalogCompletedFullSnapshot> result =
            runtime.GetCompletedFullSnapshot();

        Assert.Equal(ResultStatus.Failed, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_COMBINED_SNAPSHOT_FAILED");
        Assert.DoesNotContain("NullReferenceException", result.Message);
    }

    [Fact]
    public void FullSnapshotType_IsGetterOnlyWithSixDetachedDataFields()
    {
        Type type = typeof(ErrorCatalogFullSnapshot);
        Assert.True(type.IsSealed);
        Assert.True(type.IsPublic);
        Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        PropertyInfo[] properties = type.GetProperties(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Equal(6, properties.Length);
        Assert.All(properties, property => Assert.Null(property.SetMethod));
        Assert.Equal(typeof(IReadOnlyList<ErrorDefinitionSnapshot>),
            type.GetProperty("Definitions")!.PropertyType);
        Assert.Equal(typeof(ErrorCategoryCatalogSnapshot),
            type.GetProperty("CategoryCatalog")!.PropertyType);
        Assert.Equal(typeof(ErrorOwnerCatalogSnapshot),
            type.GetProperty("OwnerCatalog")!.PropertyType);
        Assert.Equal(typeof(ErrorCodeGroupCatalogSnapshot),
            type.GetProperty("CodeGroupCatalog")!.PropertyType);
        Assert.Equal(typeof(ErrorProfileCatalogSnapshot),
            type.GetProperty("ProfileCatalog")!.PropertyType);
        Assert.Equal(typeof(ErrorCatalogValidationSnapshot),
            type.GetProperty("Validation")!.PropertyType);
        Assert.DoesNotContain(properties,
            property => property.PropertyType == typeof(ErrorCatalogContext));
    }

    private static ErrorCatalogContext Context(string prefix) =>
        new()
        {
            ErrorCatalog = new ErrorCatalog([new ErrorDefinition
            {
                Id = prefix + "-ERROR",
                Name = prefix + "_ERROR"
            }]),
            CrossValidationResult = RecordedValidation(prefix),
            CategoryCatalog = new ErrorCategoryCatalogDocument
            {
                Categories = [new ErrorCategoryDefinition
                {
                    Name = prefix + "-CATEGORY"
                }]
            },
            OwnerCatalog = new ErrorOwnerCatalogDocument
            {
                Owners = [new ErrorOwnerDefinition
                {
                    Name = prefix + "-OWNER",
                    Aliases = [prefix + "-ALIAS"]
                }]
            },
            CodeGroupCatalog = new ErrorCodeGroupCatalogDocument
            {
                CodeGroups = [new ErrorCodeGroupDefinition
                {
                    Name = prefix + "-GROUP",
                    DefaultCategories = [prefix + "-CATEGORY"]
                }]
            },
            ProfileCatalog = new ErrorProfileCatalogDocument
            {
                Profiles = [new ErrorProfileDefinition
                {
                    Name = prefix + "-PROFILE",
                    IncludeOwners = [prefix + "-OWNER"],
                    IncludeErrors = [prefix + "-ERROR"]
                }]
            }
        };

    private static Afrowave.Toolbox.WhenItFails.Validation.ErrorCatalogValidationResult
        RecordedValidation(string prefix)
    {
        Afrowave.Toolbox.WhenItFails.Validation.ErrorCatalogValidationResult result = new();
        result.AddWarning(prefix + "-WARNING", "Recorded validation warning.");
        return result;
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

    private static ErrorCatalogRuntime CreateRuntime(
        IErrorCatalogContextStore store,
        IErrorCatalogInitializer initializer,
        IBuiltInErrorCatalogContextProvider builtIn) =>
        new(
            initializer,
            new WhenItFailsOptions
            {
                InitializationMode = ErrorCatalogInitializationMode.Flexible,
                HideRecoverableFailures = true
            },
            store, builtIn,
            new UnusedDescriptorService(), new UnusedProfileSelectionService());

    private sealed class SequencedInitializer(
        ErrorCatalogContextStore store,
        Func<int, Response<ErrorCatalogInitializationPayload>> step)
        : IErrorCatalogInitializer
    {
        private int _calls;
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Response<ErrorCatalogInitializationPayload> result = step(++_calls);
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
            Task.FromResult(Response<ErrorCatalogInitializationPayload>.Invalid(
                code: "ProjectCatalogInvalid",
                message: "Project configuration is invalid."));
    }

    private sealed class InterferingStore :
        IErrorCatalogContextStore,
        IErrorCatalogContextPublicationReader,
        IErrorCatalogContextPublisher
    {
        private readonly ErrorCatalogContextStore _inner = new();
        private int _reads;

        public Action? OnSecondRead { get; set; }
        public int PublicationReads => _reads;
        public ErrorCatalogContextPublication CurrentPublication =>
            Assert.IsType<ErrorCatalogContextPublication>(
                _inner.GetCurrentPublication().Data);
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

    private sealed class UnusedProfileSelectionService : IErrorProfileSelectionService
    {
        public Response<IReadOnlyList<ErrorDefinition>> ResolveByProfileName(
            ErrorCatalogContext? context, string profileName) =>
            throw new NotSupportedException();
    }
}
