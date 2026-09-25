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

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class PublishedSupportingCatalogsSnapshotContractTests
{
    [Fact]
    public void DefaultRuntime_ReturnsExactStorePublicationWithoutReadingStatus()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = Context("FIRST");
        store.Set(context);
        ErrorCatalogContextPublication record = Assert.IsType<ErrorCatalogContextPublication>(
            store.GetCurrentPublication().Data);
        ErrorCatalogRuntime runtime = CreateRuntime(store);

        // No activation status has been recorded by this runtime.
        Assert.False(runtime.GetStatus().IsSuccess);
        Response<ErrorCatalogPublishedSupportingCatalogsSnapshot> result =
            runtime.GetPublishedSupportingCatalogsSnapshot();

        Assert.True(result.IsSuccess);
        ErrorCatalogPublishedSupportingCatalogsSnapshot snapshot =
            Assert.IsType<ErrorCatalogPublishedSupportingCatalogsSnapshot>(result.Data);
        Assert.Equal(record.StoreId, snapshot.StoreId);
        Assert.Equal(record.Generation, snapshot.Generation);
        Assert.Equal("FIRST-CATEGORY",
            Assert.Single(snapshot.Snapshot.CategoryCatalog.Categories).Name);
        Assert.Equal("FIRST-OWNER",
            Assert.Single(snapshot.Snapshot.OwnerCatalog.Owners).Name);
        Assert.Equal("FIRST-GROUP",
            Assert.Single(snapshot.Snapshot.CodeGroupCatalog.CodeGroups).Name);
        Assert.Equal("FIRST-PROFILE",
            Assert.Single(snapshot.Snapshot.ProfileCatalog.Profiles).Name);

        context.ProfileCatalog.Profiles[0].Name = "CHANGED";
        context.OwnerCatalog.Owners[0].Aliases.Clear();
        Assert.Equal("FIRST-PROFILE",
            Assert.Single(snapshot.Snapshot.ProfileCatalog.Profiles).Name);
        Assert.Equal("FIRST-ALIAS",
            Assert.Single(snapshot.Snapshot.OwnerCatalog.Owners).Aliases.Single());
        Assert.False(runtime.GetStatus().IsSuccess);
    }

    [Fact]
    public void PublicationSelection_HappensOnceEvenWhenContextIsReplaced()
    {
        ErrorCatalogContextStore store = new();
        store.Set(Context("FIRST"));
        ErrorCatalogContextPublication first = Assert.IsType<ErrorCatalogContextPublication>(
            store.GetCurrentPublication().Data);

        PublicationRuntimeStub runtime = new(() =>
        {
            Response<ErrorCatalogContextPublication> result =
                store.GetCurrentPublication();
            store.Set(Context("SECOND"));
            return result;
        });

        ErrorCatalogPublishedSupportingCatalogsSnapshot snapshot =
            Assert.IsType<ErrorCatalogPublishedSupportingCatalogsSnapshot>(
                runtime.GetPublishedSupportingCatalogsSnapshot().Data);

        Assert.Equal(1, runtime.PublicationReads);
        Assert.Equal(0, runtime.LegacyContextReads);
        Assert.Equal(first.StoreId, snapshot.StoreId);
        Assert.Equal(first.Generation, snapshot.Generation);
        Assert.Equal("FIRST-CATEGORY",
            Assert.Single(snapshot.Snapshot.CategoryCatalog.Categories).Name);
        Assert.Equal("FIRST-OWNER",
            Assert.Single(snapshot.Snapshot.OwnerCatalog.Owners).Name);
        Assert.Equal("FIRST-GROUP",
            Assert.Single(snapshot.Snapshot.CodeGroupCatalog.CodeGroups).Name);
        Assert.Equal("FIRST-PROFILE",
            Assert.Single(snapshot.Snapshot.ProfileCatalog.Profiles).Name);
        Assert.Equal(first.Generation + 1L, store.GetCurrentPublication().Data!.Generation);
    }

    [Fact]
    public void RuntimeWithoutPublicationCapability_ReturnsNotSupported()
    {
        Response<ErrorCatalogPublishedSupportingCatalogsSnapshot> result =
            new RuntimeStub().GetPublishedSupportingCatalogsSnapshot();

        Assert.Equal(ResultStatus.NotSupported, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_PUBLISHED_SUPPORTING_SNAPSHOT_NOT_SUPPORTED");
    }

    [Fact]
    public void DefaultRuntimeWithLegacyStore_ReturnsItsNotSupportedResponse()
    {
        Response<ErrorCatalogPublishedSupportingCatalogsSnapshot> result =
            CreateRuntime(new LegacyStore()).GetPublishedSupportingCatalogsSnapshot();

        Assert.Equal(ResultStatus.NotSupported, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_CONTEXT_PUBLICATION_NOT_SUPPORTED");
    }

    [Fact]
    public void UninitializedPublication_IsForwardedWithoutData()
    {
        ErrorCatalogContextStore store = new();
        Response<ErrorCatalogPublishedSupportingCatalogsSnapshot> result =
            CreateRuntime(store).GetPublishedSupportingCatalogsSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "ErrorCatalogContextNotInitialized");
    }

    [Fact]
    public void NullPublicationResponse_ReturnsInvalidWithoutContextFallback()
    {
        PublicationRuntimeStub runtime = new(() => null!);
        Response<ErrorCatalogPublishedSupportingCatalogsSnapshot> result =
            runtime.GetPublishedSupportingCatalogsSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_PUBLISHED_SUPPORTING_SNAPSHOT_PUBLICATION_RESPONSE_NULL");
        Assert.Equal(1, runtime.PublicationReads);
        Assert.Equal(0, runtime.LegacyContextReads);
    }

    [Fact]
    public void SuccessWithoutPublication_ReturnsInvalidWithoutInventedIdentity()
    {
        PublicationRuntimeStub runtime = new(() =>
            Response<ErrorCatalogContextPublication>.Ok(null));

        Response<ErrorCatalogPublishedSupportingCatalogsSnapshot> result =
            runtime.GetPublishedSupportingCatalogsSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_PUBLISHED_SUPPORTING_SNAPSHOT_PUBLICATION_NULL");
        Assert.Equal(1, runtime.PublicationReads);
    }

    [Theory]
    [InlineData(0, "WIF_SUPPORTING_SNAPSHOT_CATEGORY_CATALOG_NULL")]
    [InlineData(1, "WIF_SUPPORTING_SNAPSHOT_OWNER_CATALOG_NULL")]
    [InlineData(2, "WIF_SUPPORTING_SNAPSHOT_CODE_GROUP_CATALOG_NULL")]
    [InlineData(3, "WIF_SUPPORTING_SNAPSHOT_PROFILE_CATALOG_NULL")]
    public void MissingSupportingCatalog_ReturnsInvalidWithoutPartialData(
        int missing, string code)
    {
        ErrorCatalogContext context = Context("FIRST");
        switch (missing)
        {
            case 0: context.CategoryCatalog = null!; break;
            case 1: context.OwnerCatalog = null!; break;
            case 2: context.CodeGroupCatalog = null!; break;
            case 3: context.ProfileCatalog = null!; break;
        }

        ErrorCatalogContextStore store = new();
        store.Set(context);
        Response<ErrorCatalogPublishedSupportingCatalogsSnapshot> result =
            CreateRuntime(store).GetPublishedSupportingCatalogsSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues, issue => issue.Code == code);
    }

    [Fact]
    public void MalformedNestedDocument_ReturnsStableFailureWithoutPartialData()
    {
        ErrorCatalogContext context = Context("FIRST");
        context.CodeGroupCatalog.CodeGroups[0].DefaultCategories = null!;
        ErrorCatalogContextStore store = new();
        store.Set(context);

        Response<ErrorCatalogPublishedSupportingCatalogsSnapshot> result =
            CreateRuntime(store).GetPublishedSupportingCatalogsSnapshot();

        Assert.Equal(ResultStatus.Failed, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_SUPPORTING_SNAPSHOT_FAILED");
        Assert.DoesNotContain("NullReferenceException", result.Message);
    }

    [Fact]
    public void PublicationReaderException_IsNormalizedWithoutExposingDetails()
    {
        PublicationRuntimeStub runtime = new(() =>
            throw new InvalidOperationException("Secret reader details."));

        Response<ErrorCatalogPublishedSupportingCatalogsSnapshot> result =
            runtime.GetPublishedSupportingCatalogsSnapshot();

        Assert.Equal(ResultStatus.Failed, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_PUBLISHED_SUPPORTING_SNAPSHOT_FAILED");
        Assert.DoesNotContain("Secret reader details", result.Message);
        Assert.Equal(1, runtime.PublicationReads);
    }

    [Fact]
    public void PublicationReaderCancellation_PropagatesExactInstance()
    {
        OperationCanceledException cancellation = new("Cancelled.");
        PublicationRuntimeStub runtime = new(() => throw cancellation);

        OperationCanceledException actual = Assert.Throws<OperationCanceledException>(
            () => runtime.GetPublishedSupportingCatalogsSnapshot());

        Assert.Same(cancellation, actual);
        Assert.Equal(1, runtime.PublicationReads);
    }

    [Fact]
    public void PublishedProjection_IsGetterOnlyAndOriginalInterfacesRemainUnchanged()
    {
        Type type = typeof(ErrorCatalogPublishedSupportingCatalogsSnapshot);
        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        PropertyInfo[] properties = type.GetProperties(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Equal(3, properties.Length);
        Assert.All(properties, property => Assert.Null(property.SetMethod));
        Assert.Equal(typeof(Guid), type.GetProperty("StoreId")!.PropertyType);
        Assert.Equal(typeof(long), type.GetProperty("Generation")!.PropertyType);
        Assert.Equal(typeof(ErrorSupportingCatalogsSnapshot),
            type.GetProperty("Snapshot")!.PropertyType);

        Assert.DoesNotContain(typeof(IErrorCatalogRuntime).GetMethods(),
            method => method.Name == "GetPublishedSupportingCatalogsSnapshot");
        Assert.DoesNotContain(typeof(IErrorCatalogContextStore).GetMethods(),
            method => method.Name == "GetCurrentPublication");
        MethodInfo extension = Assert.Single(
            typeof(ErrorCatalogPublishedSupportingCatalogsSnapshotExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static |
                            BindingFlags.DeclaredOnly));
        Assert.Equal("GetPublishedSupportingCatalogsSnapshot", extension.Name);
        Assert.Equal(typeof(Response<ErrorCatalogPublishedSupportingCatalogsSnapshot>),
            extension.ReturnType);
        Assert.Equal(typeof(IErrorCatalogRuntime),
            Assert.Single(extension.GetParameters()).ParameterType);
    }

    private static ErrorCatalogContext Context(string prefix)
    {
        ErrorCategoryCatalogDocument categories = new()
        {
            Categories = [new ErrorCategoryDefinition { Name = prefix + "-CATEGORY" }]
        };
        ErrorOwnerCatalogDocument owners = new()
        {
            Owners = [new ErrorOwnerDefinition
            {
                Name = prefix + "-OWNER", Aliases = [prefix + "-ALIAS"]
            }]
        };
        ErrorCodeGroupCatalogDocument groups = new()
        {
            CodeGroups = [new ErrorCodeGroupDefinition
            {
                Name = prefix + "-GROUP",
                DefaultCategories = [prefix + "-CATEGORY"]
            }]
        };
        ErrorProfileCatalogDocument profiles = new()
        {
            Profiles = [new ErrorProfileDefinition
            {
                Name = prefix + "-PROFILE",
                IncludeOwners = [prefix + "-OWNER"]
            }]
        };
        return new ErrorCatalogContext
        {
            CategoryCatalog = categories,
            OwnerCatalog = owners,
            CodeGroupCatalog = groups,
            ProfileCatalog = profiles
        };
    }

    private static ErrorCatalogRuntime CreateRuntime(IErrorCatalogContextStore store) =>
        new(
            new UnusedInitializer(),
            new WhenItFailsOptions(),
            store,
            new UnusedBuiltInProvider(),
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
            throw new InvalidOperationException("Legacy context must not be read.");
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
            throw new InvalidOperationException("Status must not be read.");

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
            throw new InvalidOperationException("Legacy context must not be read.");
        public void Set(ErrorCatalogContext context) =>
            throw new InvalidOperationException("Legacy store must not be written.");
    }

    private sealed class UnusedInitializer : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class UnusedBuiltInProvider : IBuiltInErrorCatalogContextProvider
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
