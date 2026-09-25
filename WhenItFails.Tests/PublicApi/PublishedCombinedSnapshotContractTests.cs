using System.Reflection;
using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;
using Afrowave.Toolbox.WhenItFails.Services;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class PublishedCombinedSnapshotContractTests
{
    [Fact]
    public void DefaultRuntime_UsesActualStorePublicationAndDoesNotReadSeparateStatus()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = CreateContext("FIRST");
        store.Set(context);
        ErrorCatalogContextPublication selected = Assert.IsType<ErrorCatalogContextPublication>(
            store.GetCurrentPublication().Data);

        IErrorCatalogRuntime runtime = CreateRuntime(store);
        Assert.IsAssignableFrom<IErrorCatalogRuntimePublicationReader>(runtime);

        // The test runtime has not recorded status; publication identity must
        // come from the store and must not be synthesized from GetStatus().
        Assert.False(runtime.GetStatus().IsSuccess);

        Response<ErrorCatalogPublishedCombinedSnapshot> response =
            runtime.GetPublishedCombinedSnapshot();

        Assert.True(response.IsSuccess);
        ErrorCatalogPublishedCombinedSnapshot published =
            Assert.IsType<ErrorCatalogPublishedCombinedSnapshot>(response.Data);
        Assert.Equal(selected.StoreId, published.StoreId);
        Assert.Equal(selected.Generation, published.Generation);
        Assert.Equal("FIRST-ERROR", Assert.Single(published.Snapshot.Definitions).Id);
        Assert.Equal("FIRST-CATEGORY",
            Assert.Single(published.Snapshot.CategoryCatalog.Categories).Name);
        Assert.Equal("FIRST-WARNING",
            Assert.Single(published.Snapshot.Validation.Issues).Code);
        Assert.False(runtime.GetStatus().IsSuccess);

        // The published data is detached even though the infrastructure record
        // still contains the live source context.
        context.ErrorCatalog.GetAll()[0].Id = "CHANGED";
        Assert.Equal("FIRST-ERROR", Assert.Single(published.Snapshot.Definitions).Id);
    }

    [Fact]
    public void Extension_SelectsPublicationOnceEvenWhenStoreReplacesContextDuringRead()
    {
        ErrorCatalogContextStore store = new();
        store.Set(CreateContext("FIRST"));
        ErrorCatalogContextPublication first = Assert.IsType<ErrorCatalogContextPublication>(
            store.GetCurrentPublication().Data);

        PublicationRuntimeStub runtime = new(() =>
        {
            Response<ErrorCatalogContextPublication> response =
                store.GetCurrentPublication();
            store.Set(CreateContext("SECOND"));
            return response;
        });

        ErrorCatalogPublishedCombinedSnapshot captured =
            Assert.IsType<ErrorCatalogPublishedCombinedSnapshot>(
                runtime.GetPublishedCombinedSnapshot().Data);

        Assert.Equal(1, runtime.PublicationReads);
        Assert.Equal(0, runtime.LegacyContextReads);
        Assert.Equal(first.StoreId, captured.StoreId);
        Assert.Equal(first.Generation, captured.Generation);
        Assert.Equal("FIRST-ERROR", Assert.Single(captured.Snapshot.Definitions).Id);
        Assert.Equal("FIRST-CATEGORY",
            Assert.Single(captured.Snapshot.CategoryCatalog.Categories).Name);
        Assert.Equal("FIRST-WARNING",
            Assert.Single(captured.Snapshot.Validation.Issues).Code);
        Assert.Equal(2L, store.GetCurrentPublication().Data!.Generation);
    }

    [Fact]
    public void RuntimeWithoutOptionalCapability_ReturnsNotSupportedWithoutGuessingGeneration()
    {
        IErrorCatalogRuntime runtime = new RuntimeStub();

        Response<ErrorCatalogPublishedCombinedSnapshot> response =
            runtime.GetPublishedCombinedSnapshot();

        Assert.Equal(ResultStatus.NotSupported, response.Status);
        Assert.Null(response.Data);
        Assert.Contains(response.Issues,
            issue => issue.Code == "WIF_PUBLISHED_SNAPSHOT_NOT_SUPPORTED");
    }

    [Fact]
    public void DefaultRuntime_WithLegacyStore_ReturnsNotSupportedWithoutFallbackIdentity()
    {
        IErrorCatalogRuntime runtime = CreateRuntime(new LegacyStore());

        Response<ErrorCatalogPublishedCombinedSnapshot> response =
            runtime.GetPublishedCombinedSnapshot();

        Assert.Equal(ResultStatus.NotSupported, response.Status);
        Assert.Null(response.Data);
        Assert.Contains(response.Issues,
            issue => issue.Code == "WIF_CONTEXT_PUBLICATION_NOT_SUPPORTED");
    }

    [Fact]
    public void UninitializedStore_FailureIsForwardedWithoutSnapshot()
    {
        ErrorCatalogContextStore store = new();
        IErrorCatalogRuntime runtime = CreateRuntime(store);

        Response<ErrorCatalogPublishedCombinedSnapshot> response =
            runtime.GetPublishedCombinedSnapshot();

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.Contains(response.Issues,
            issue => issue.Code == "ErrorCatalogContextNotInitialized");
    }

    [Fact]
    public void MissingRequiredCategory_ReturnsInvalidWithoutPartialSnapshot()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = CreateContext("FIRST");
        context.CategoryCatalog = null!;
        store.Set(context);

        Response<ErrorCatalogPublishedCombinedSnapshot> response =
            CreateRuntime(store).GetPublishedCombinedSnapshot();

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.Contains(response.Issues,
            issue => issue.Code == "WIF_COMBINED_SNAPSHOT_CATEGORY_CATALOG_NULL");
    }

    [Fact]
    public void PublishedProjection_IsGetterOnlyAndLeavesOldInterfacesUnchanged()
    {
        Type type = typeof(ErrorCatalogPublishedCombinedSnapshot);
        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        PropertyInfo[] properties = type.GetProperties(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Equal(3, properties.Length);
        Assert.All(properties, property => Assert.Null(property.SetMethod));
        Assert.Equal(typeof(Guid), type.GetProperty("StoreId")!.PropertyType);
        Assert.Equal(typeof(long), type.GetProperty("Generation")!.PropertyType);
        Assert.Equal(typeof(ErrorCatalogCombinedSnapshot),
            type.GetProperty("Snapshot")!.PropertyType);

        Assert.DoesNotContain(typeof(IErrorCatalogRuntime).GetMethods(),
            method => method.Name == "GetCurrentPublication");
        Assert.DoesNotContain(typeof(IErrorCatalogContextStore).GetMethods(),
            method => method.Name == "GetCurrentPublication");
        Assert.Contains(typeof(IErrorCatalogRuntimePublicationReader),
            typeof(ErrorCatalogRuntime).GetInterfaces());
    }

    private static ErrorCatalogContext CreateContext(string prefix)
    {
        ErrorDefinition definition = new()
        {
            Id = prefix + "-ERROR",
            Name = prefix + "_ERROR"
        };
        ErrorCategoryDefinition category = new()
        {
            Name = prefix + "-CATEGORY"
        };
        ErrorCatalogValidationResult validation = new();
        validation.AddWarning(prefix + "-WARNING", "Existing validation warning");

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

    private static ErrorCatalogRuntime CreateRuntime(IErrorCatalogContextStore store) =>
        new(
            new UnusedInitializer(),
            new WhenItFailsOptions(),
            store,
            new UnusedBuiltInContextProvider(),
            new UnusedDescriptorService(),
            new UnusedProfileSelectionService());

    private sealed class PublicationRuntimeStub(
        Func<Response<ErrorCatalogContextPublication>> read)
        : RuntimeStub, IErrorCatalogRuntimePublicationReader
    {
        public int PublicationReads { get; private set; }

        public Response<ErrorCatalogContextPublication> GetCurrentPublication()
        {
            PublicationReads++;
            return read();
        }
    }

    private class RuntimeStub : IErrorCatalogRuntime
    {
        public int LegacyContextReads { get; private set; }

        public Response<ErrorCatalogContext> GetCurrentContext()
        {
            LegacyContextReads++;
            throw new InvalidOperationException("Must not read legacy context.");
        }

        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<Response<ErrorCatalogInitializationPayload>> ResetToDefaultsAsync(
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Response<ErrorCatalogRuntimeStatus> GetStatus() =>
            throw new NotSupportedException();

        public Response<ErrorDescriptor> FromId(string errorId) =>
            throw new NotSupportedException();

        public Response<ErrorDescriptor> FromName(string errorName) =>
            throw new NotSupportedException();

        public Response<ErrorDescriptor> FromCode(int code) =>
            throw new NotSupportedException();

        public Response<IReadOnlyList<ErrorDefinition>> ResolveProfile(
            string profileName) =>
            throw new NotSupportedException();
    }

    private sealed class LegacyStore : IErrorCatalogContextStore
    {
        public bool IsInitialized => false;
        public ErrorCatalogContext? Current => null;
        public Response<ErrorCatalogContext> GetCurrent() =>
            throw new InvalidOperationException("Legacy read should not be called.");
        public void Set(ErrorCatalogContext context) =>
            throw new InvalidOperationException("Legacy write should not be called.");
    }

    private sealed class UnusedInitializer : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class UnusedBuiltInContextProvider
        : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default) =>
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
