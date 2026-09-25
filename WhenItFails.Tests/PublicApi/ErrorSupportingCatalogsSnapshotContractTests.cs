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

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class ErrorSupportingCatalogsSnapshotContractTests
{
    [Fact]
    public void SupportingCapture_DetachesAllFourCatalogsAndTheirNestedData()
    {
        ErrorCatalogContext context = CreateContext("FIRST");
        StubRuntime runtime = new(() => Response<ErrorCatalogContext>.Ok(context));

        Response<ErrorSupportingCatalogsSnapshot> result =
            runtime.GetSupportingCatalogsSnapshot();

        Assert.True(result.IsSuccess);
        Assert.Equal(1, runtime.ContextReads);
        ErrorSupportingCatalogsSnapshot snapshot =
            Assert.IsType<ErrorSupportingCatalogsSnapshot>(result.Data);

        ErrorCategoryDefinitionSnapshot category =
            Assert.Single(snapshot.CategoryCatalog.Categories);
        ErrorOwnerDefinitionSnapshot owner =
            Assert.Single(snapshot.OwnerCatalog.Owners);
        ErrorCodeGroupDefinitionSnapshot group =
            Assert.Single(snapshot.CodeGroupCatalog.CodeGroups);
        ErrorProfileDefinitionSnapshot profile =
            Assert.Single(snapshot.ProfileCatalog.Profiles);

        Assert.Equal("FIRST-CATEGORY", category.Name);
        Assert.Equal("FIRST-OWNER", owner.Name);
        Assert.Equal("FIRST-GROUP", group.Name);
        Assert.Equal("FIRST-PROFILE", profile.Name);
        Assert.Equal("before", snapshot.CategoryCatalog.Metadata["note"]);
        Assert.Equal("before", snapshot.OwnerCatalog.Metadata["note"]);
        Assert.Equal("before", snapshot.CodeGroupCatalog.Metadata["note"]);
        Assert.Equal("before", snapshot.ProfileCatalog.Metadata["note"]);

        context.CategoryCatalog.Categories[0].Name = "CHANGED";
        context.OwnerCatalog.Owners[0].Aliases.Clear();
        context.CodeGroupCatalog.CodeGroups[0].DefaultCategories.Clear();
        context.ProfileCatalog.Profiles[0].IncludeOwners.Clear();
        context.CategoryCatalog.Tags.Add("later");
        context.OwnerCatalog.Metadata.Set("note", "after");
        context.CodeGroupCatalog.CodeGroups.Add(
            new ErrorCodeGroupDefinition { Name = "ADDED" });
        context.ProfileCatalog.Profiles.Add(
            new ErrorProfileDefinition { Name = "ADDED" });

        Assert.Equal("FIRST-CATEGORY", category.Name);
        Assert.Equal("FIRST-ALIAS", Assert.Single(owner.Aliases));
        Assert.Equal("FIRST-CATEGORY", Assert.Single(group.DefaultCategories));
        Assert.Equal("FIRST-OWNER", Assert.Single(profile.IncludeOwners));
        Assert.Single(snapshot.CategoryCatalog.Tags);
        Assert.Equal("before", snapshot.OwnerCatalog.Metadata["note"]);
        Assert.Single(snapshot.CodeGroupCatalog.CodeGroups);
        Assert.Single(snapshot.ProfileCatalog.Profiles);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<ErrorCategoryDefinitionSnapshot>)
                snapshot.CategoryCatalog.Categories).Clear());
        Assert.Throws<NotSupportedException>(() =>
            ((IList<string>)profile.IncludeOwners).Add("DENIED"));
    }

    [Fact]
    public void SupportingCapture_SelectsOneContextPerCallAcrossReplacement()
    {
        ErrorCatalogContext first = CreateContext("FIRST");
        ErrorCatalogContext second = CreateContext("SECOND");
        ErrorCatalogContext current = first;
        StubRuntime runtime = new(() =>
        {
            ErrorCatalogContext selected = current;
            current = second;
            return Response<ErrorCatalogContext>.Ok(selected);
        });

        ErrorSupportingCatalogsSnapshot oldSnapshot =
            Assert.IsType<ErrorSupportingCatalogsSnapshot>(
                runtime.GetSupportingCatalogsSnapshot().Data);
        Assert.Equal(1, runtime.ContextReads);

        ErrorSupportingCatalogsSnapshot newSnapshot =
            Assert.IsType<ErrorSupportingCatalogsSnapshot>(
                runtime.GetSupportingCatalogsSnapshot().Data);
        Assert.Equal(2, runtime.ContextReads);

        Assert.Equal("FIRST-CATEGORY",
            Assert.Single(oldSnapshot.CategoryCatalog.Categories).Name);
        Assert.Equal("FIRST-OWNER",
            Assert.Single(oldSnapshot.OwnerCatalog.Owners).Name);
        Assert.Equal("FIRST-GROUP",
            Assert.Single(oldSnapshot.CodeGroupCatalog.CodeGroups).Name);
        Assert.Equal("FIRST-PROFILE",
            Assert.Single(oldSnapshot.ProfileCatalog.Profiles).Name);
        Assert.Equal("SECOND-CATEGORY",
            Assert.Single(newSnapshot.CategoryCatalog.Categories).Name);
        Assert.Equal("SECOND-OWNER",
            Assert.Single(newSnapshot.OwnerCatalog.Owners).Name);
        Assert.Equal("SECOND-GROUP",
            Assert.Single(newSnapshot.CodeGroupCatalog.CodeGroups).Name);
        Assert.Equal("SECOND-PROFILE",
            Assert.Single(newSnapshot.ProfileCatalog.Profiles).Name);
        Assert.NotSame(oldSnapshot, newSnapshot);
    }

    [Fact]
    public void SupportingCapture_ForwardsUninitializedResponseWithoutData()
    {
        StubRuntime runtime = new(() => Response<ErrorCatalogContext>.Invalid(
            code: "ErrorCatalogContextNotInitialized",
            message: "No active context."));

        Response<ErrorSupportingCatalogsSnapshot> result =
            runtime.GetSupportingCatalogsSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "ErrorCatalogContextNotInitialized");
        Assert.Equal(1, runtime.ContextReads);
    }

    [Fact]
    public void SupportingCapture_NullContextResponseReturnsInvalid()
    {
        StubRuntime runtime = new(() => null!);

        Response<ErrorSupportingCatalogsSnapshot> result =
            runtime.GetSupportingCatalogsSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_SUPPORTING_SNAPSHOT_CONTEXT_RESPONSE_NULL");
        Assert.Equal(1, runtime.ContextReads);
    }

    [Theory]
    [InlineData(0, "WIF_SUPPORTING_SNAPSHOT_CATEGORY_CATALOG_NULL")]
    [InlineData(1, "WIF_SUPPORTING_SNAPSHOT_OWNER_CATALOG_NULL")]
    [InlineData(2, "WIF_SUPPORTING_SNAPSHOT_CODE_GROUP_CATALOG_NULL")]
    [InlineData(3, "WIF_SUPPORTING_SNAPSHOT_PROFILE_CATALOG_NULL")]
    public void SupportingCapture_RejectsMissingCatalogWithoutPartialData(
        int missing, string code)
    {
        ErrorCatalogContext context = CreateContext("FIRST");
        switch (missing)
        {
            case 0: context.CategoryCatalog = null!; break;
            case 1: context.OwnerCatalog = null!; break;
            case 2: context.CodeGroupCatalog = null!; break;
            case 3: context.ProfileCatalog = null!; break;
        }

        StubRuntime runtime = new(() => Response<ErrorCatalogContext>.Ok(context));
        Response<ErrorSupportingCatalogsSnapshot> result =
            runtime.GetSupportingCatalogsSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues, issue => issue.Code == code);
        Assert.Equal(1, runtime.ContextReads);
    }

    [Fact]
    public void SupportingCapture_MalformedNestedProfileFailsWithoutPartialData()
    {
        ErrorCatalogContext context = CreateContext("FIRST");
        context.ProfileCatalog.Profiles[0].IncludeErrors = null!;
        StubRuntime runtime = new(() => Response<ErrorCatalogContext>.Ok(context));

        Response<ErrorSupportingCatalogsSnapshot> result =
            runtime.GetSupportingCatalogsSnapshot();

        Assert.Equal(ResultStatus.Failed, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_SUPPORTING_SNAPSHOT_FAILED");
        Assert.DoesNotContain("NullReferenceException", result.Message);
        Assert.Equal(1, runtime.ContextReads);
    }

    [Fact]
    public void SupportingCapture_OrdinaryGetterExceptionReturnsStableFailure()
    {
        StubRuntime runtime = new(() =>
            throw new InvalidOperationException("Sensitive failure details."));

        Response<ErrorSupportingCatalogsSnapshot> result =
            runtime.GetSupportingCatalogsSnapshot();

        Assert.Equal(ResultStatus.Failed, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_SUPPORTING_SNAPSHOT_FAILED");
        Assert.DoesNotContain("Sensitive failure details", result.Message);
        Assert.Equal(1, runtime.ContextReads);
    }

    [Fact]
    public void SupportingCapture_PropagatesTheExactCancellationException()
    {
        OperationCanceledException cancellation = new("Cancelled.");
        StubRuntime runtime = new(() => throw cancellation);

        OperationCanceledException actual = Assert.Throws<OperationCanceledException>(
            () => runtime.GetSupportingCatalogsSnapshot());

        Assert.Same(cancellation, actual);
        Assert.Equal(1, runtime.ContextReads);
    }

    [Fact]
    public void SupportingCapture_GetterOnlyTypeAndOriginalRuntimeInterfaceUnchanged()
    {
        Type type = typeof(ErrorSupportingCatalogsSnapshot);
        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.Empty(type.GetConstructors(
            BindingFlags.Public | BindingFlags.Instance));
        PropertyInfo[] properties = type.GetProperties(
            BindingFlags.Public | BindingFlags.Instance |
            BindingFlags.DeclaredOnly);
        Assert.Equal(4, properties.Length);
        Assert.All(properties, property => Assert.Null(property.SetMethod));
        Assert.Equal(typeof(ErrorCategoryCatalogSnapshot),
            type.GetProperty("CategoryCatalog")!.PropertyType);
        Assert.Equal(typeof(ErrorOwnerCatalogSnapshot),
            type.GetProperty("OwnerCatalog")!.PropertyType);
        Assert.Equal(typeof(ErrorCodeGroupCatalogSnapshot),
            type.GetProperty("CodeGroupCatalog")!.PropertyType);
        Assert.Equal(typeof(ErrorProfileCatalogSnapshot),
            type.GetProperty("ProfileCatalog")!.PropertyType);
        Assert.DoesNotContain(typeof(IErrorCatalogRuntime).GetMethods(),
            method => method.Name == "GetSupportingCatalogsSnapshot");

        MethodInfo extension = Assert.Single(
            typeof(ErrorSupportingCatalogsSnapshotExtensions).GetMethods(
                BindingFlags.Public | BindingFlags.Static |
                BindingFlags.DeclaredOnly));
        Assert.Equal("GetSupportingCatalogsSnapshot", extension.Name);
        Assert.Equal(typeof(Response<ErrorSupportingCatalogsSnapshot>),
            extension.ReturnType);
        Assert.Equal(typeof(IErrorCatalogRuntime),
            Assert.Single(extension.GetParameters()).ParameterType);
    }

    private static ErrorCatalogContext CreateContext(string prefix)
    {
        ErrorCategoryCatalogDocument categories = new()
        {
            CatalogName = prefix + "-CATEGORIES",
            Tags = ["initial"],
            Categories = [new ErrorCategoryDefinition
            {
                Name = prefix + "-CATEGORY",
                Aliases = [prefix + "-ALIAS"]
            }]
        };
        categories.Metadata.Set("note", "before");

        ErrorOwnerCatalogDocument owners = new()
        {
            CatalogName = prefix + "-OWNERS",
            Owners = [new ErrorOwnerDefinition
            {
                Name = prefix + "-OWNER",
                Aliases = [prefix + "-ALIAS"]
            }]
        };
        owners.Metadata.Set("note", "before");

        ErrorCodeGroupCatalogDocument codeGroups = new()
        {
            CatalogName = prefix + "-GROUPS",
            CodeGroups = [new ErrorCodeGroupDefinition
            {
                Name = prefix + "-GROUP",
                DefaultCategories = [prefix + "-CATEGORY"]
            }]
        };
        codeGroups.Metadata.Set("note", "before");

        ErrorProfileCatalogDocument profiles = new()
        {
            CatalogName = prefix + "-PROFILES",
            Profiles = [new ErrorProfileDefinition
            {
                Name = prefix + "-PROFILE",
                IncludeOwners = [prefix + "-OWNER"],
                IncludeErrors = [prefix + "-ERROR"]
            }]
        };
        profiles.Metadata.Set("note", "before");

        return new ErrorCatalogContext
        {
            CategoryCatalog = categories,
            OwnerCatalog = owners,
            CodeGroupCatalog = codeGroups,
            ProfileCatalog = profiles
        };
    }

    private sealed class StubRuntime(
        Func<Response<ErrorCatalogContext>> current) : IErrorCatalogRuntime
    {
        public int ContextReads { get; private set; }

        public Response<ErrorCatalogContext> GetCurrentContext()
        {
            ContextReads++;
            return current();
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
}
