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

public sealed class ErrorCategoryCatalogSnapshotContractTests
{
    [Fact]
    public void Snapshot_DetachesDocumentDefinitionsListsMappingsAndMetadata()
    {
        ErrorCategoryCatalogDocument source = CreateCatalog();
        ErrorCategoryDefinition original = Assert.Single(source.Categories);
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext { CategoryCatalog = source }));

        Response<ErrorCategoryCatalogSnapshot> response =
            runtime.GetCategoryCatalogSnapshot();

        Assert.True(response.IsSuccess);
        ErrorCategoryCatalogSnapshot captured =
            Assert.IsType<ErrorCategoryCatalogSnapshot>(response.Data);
        ErrorCategoryDefinitionSnapshot category =
            Assert.Single(captured.Categories);

        Assert.Equal("1.0", captured.SchemaVersion);
        Assert.Equal("categories.example", captured.CatalogId);
        Assert.Equal("en", captured.Language);
        Assert.Equal("NETWORK", category.Name);
        Assert.Equal("Network", category.DisplayName);
        Assert.Equal("NETWORKING", Assert.Single(category.Aliases));
        Assert.Equal("GENERAL", Assert.Single(category.ParentCategories));
        Assert.Equal("RETRY", Assert.Single(category.DefaultTags));
        Assert.Equal("503", category.DefaultMappings["web.httpStatus"]);
        Assert.Equal("before", category.Metadata["note"]);
        Assert.Equal("before", captured.Metadata["owner"]);
        Assert.Equal("catalog-tag", Assert.Single(captured.Tags));

        source.CatalogName = "Updated source";
        source.Tags.Add("later");
        source.Metadata.Set("owner", "after");
        original.Name = "CHANGED";
        original.Aliases.Clear();
        original.ParentCategories.Add("LATER");
        original.DefaultTags.Add("LATER");
        original.DefaultMappings["web.httpStatus"] = "200";
        original.Metadata.Set("note", "after");
        source.Categories.Add(new ErrorCategoryDefinition { Name = "NEW" });

        Assert.Equal("Initial catalog", captured.CatalogName);
        Assert.Single(captured.Tags);
        Assert.Single(captured.Categories);
        Assert.Equal("before", captured.Metadata["owner"]);
        Assert.Equal("NETWORK", category.Name);
        Assert.Equal("NETWORKING", Assert.Single(category.Aliases));
        Assert.Equal("GENERAL", Assert.Single(category.ParentCategories));
        Assert.Equal("RETRY", Assert.Single(category.DefaultTags));
        Assert.Equal("503", category.DefaultMappings["web.httpStatus"]);
        Assert.Equal("before", category.Metadata["note"]);

        Assert.Throws<NotSupportedException>(
            () => ((IList<ErrorCategoryDefinitionSnapshot>)captured.Categories)
                .Clear());
        Assert.Throws<NotSupportedException>(
            () => ((IList<string>)category.Aliases).Add("DENIED"));
        Assert.Throws<NotSupportedException>(
            () => ((IDictionary<string, string>)category.DefaultMappings)
                .Add("DENIED", "value"));
        Assert.Throws<NotSupportedException>(
            () => ((IDictionary<string, string>)captured.Metadata)
                .Add("DENIED", "value"));
    }

    [Fact]
    public void Snapshot_UsesOneContextResponsePerCaptureAndRetainsOldValuesAfterReplacement()
    {
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext { CategoryCatalog = CreateCatalog() }));

        ErrorCategoryCatalogSnapshot first = Assert.IsType<ErrorCategoryCatalogSnapshot>(
            runtime.GetCategoryCatalogSnapshot().Data);
        Assert.Equal(1, runtime.ContextReadCount);

        ErrorCategoryCatalogDocument replacement = CreateCatalog();
        replacement.CatalogName = "Replacement";
        replacement.Categories[0].Name = "OTHER";
        runtime.Current = Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext { CategoryCatalog = replacement });

        ErrorCategoryCatalogSnapshot second = Assert.IsType<ErrorCategoryCatalogSnapshot>(
            runtime.GetCategoryCatalogSnapshot().Data);

        Assert.Equal(2, runtime.ContextReadCount);
        Assert.Equal("Initial catalog", first.CatalogName);
        Assert.Equal("NETWORK", Assert.Single(first.Categories).Name);
        Assert.Equal("Replacement", second.CatalogName);
        Assert.Equal("OTHER", Assert.Single(second.Categories).Name);
        Assert.NotSame(first, second);
    }

    [Fact]
    public void Snapshot_PropagatesUninitializedContextFailure()
    {
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Invalid(
            code: "ErrorCatalogContextNotInitialized",
            message: "Error catalog context has not been initialized."));

        Response<ErrorCategoryCatalogSnapshot> response =
            runtime.GetCategoryCatalogSnapshot();

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.Contains(response.Issues,
            issue => issue.Code == "ErrorCatalogContextNotInitialized");
    }

    [Fact]
    public void Snapshot_ReturnsStableInvalidWhenCategoryCatalogIsMissing()
    {
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext()));

        Response<ErrorCategoryCatalogSnapshot> response =
            runtime.GetCategoryCatalogSnapshot();

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.Contains(response.Issues,
            issue => issue.Code == "WIF_CATEGORY_SNAPSHOT_CATALOG_NULL");
    }

    [Fact]
    public void Snapshot_MalformedSourceReturnsStableFailureWithoutExceptionText()
    {
        ErrorCategoryCatalogDocument source = CreateCatalog();
        source.Categories[0].Aliases = null!;
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext { CategoryCatalog = source }));

        Response<ErrorCategoryCatalogSnapshot> response =
            runtime.GetCategoryCatalogSnapshot();

        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Contains(response.Issues,
            issue => issue.Code == "WIF_CATEGORY_SNAPSHOT_FAILED");
        Assert.DoesNotContain("NullReferenceException", response.Message);
    }

    [Fact]
    public void Snapshot_UsesSealedGetterOnlyTypesAndLeavesRuntimeInterfaceUnchanged()
    {
        foreach (Type type in new[]
                 {
                     typeof(ErrorCategoryCatalogSnapshot),
                     typeof(ErrorCategoryDefinitionSnapshot)
                 })
        {
            Assert.True(type.IsPublic);
            Assert.True(type.IsSealed);
            Assert.Empty(type.GetConstructors(
                BindingFlags.Public | BindingFlags.Instance));
            Assert.All(type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance |
                BindingFlags.DeclaredOnly),
                property => Assert.Null(property.SetMethod));
        }

        Assert.Equal(11, typeof(ErrorCategoryCatalogSnapshot).GetProperties().Length);
        Assert.Equal(8, typeof(ErrorCategoryDefinitionSnapshot).GetProperties().Length);
        Assert.DoesNotContain(typeof(IErrorCatalogRuntime).GetMethods(),
            method => method.Name == "GetCategoryCatalogSnapshot");
    }

    private static ErrorCategoryCatalogDocument CreateCatalog()
    {
        ErrorCategoryDefinition category = new()
        {
            Name = "NETWORK",
            DisplayName = "Network",
            Aliases = ["NETWORKING"],
            ParentCategories = ["GENERAL"],
            DefaultTags = ["RETRY"],
            DefaultMappings = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["web.httpStatus"] = "503"
            }
        };
        category.Metadata.Set("note", "before");

        ErrorCategoryCatalogDocument document = new()
        {
            CatalogId = "categories.example",
            CatalogName = "Initial catalog",
            Tags = ["catalog-tag"],
            Categories = [category]
        };
        document.Metadata.Set("owner", "before");
        return document;
    }

    private sealed class StubRuntime(
        Response<ErrorCatalogContext> current) : IErrorCatalogRuntime
    {
        public Response<ErrorCatalogContext> Current { get; set; } = current;
        public int ContextReadCount { get; private set; }

        public Response<ErrorCatalogContext> GetCurrentContext()
        {
            ContextReadCount++;
            return Current;
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
