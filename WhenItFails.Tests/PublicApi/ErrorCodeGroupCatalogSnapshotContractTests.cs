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

public sealed class ErrorCodeGroupCatalogSnapshotContractTests
{
    [Fact]
    public void CodeGroupSnapshot_DetachesDocumentDefinitionsListsMappingsAndMetadata()
    {
        ErrorCodeGroupCatalogDocument document = CreateCatalog();
        ErrorCodeGroupDefinition group = Assert.Single(document.CodeGroups);
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext { CodeGroupCatalog = document }));

        Response<ErrorCodeGroupCatalogSnapshot> response =
            runtime.GetCodeGroupCatalogSnapshot();

        Assert.True(response.IsSuccess);
        Assert.Equal(1, runtime.ContextReads);
        ErrorCodeGroupCatalogSnapshot captured =
            Assert.IsType<ErrorCodeGroupCatalogSnapshot>(response.Data);
        ErrorCodeGroupDefinitionSnapshot item = Assert.Single(captured.CodeGroups);

        Assert.Equal("code-groups.example", captured.CatalogId);
        Assert.Equal("Initial groups", captured.CatalogName);
        Assert.Equal("en", captured.Language);
        Assert.Equal("CONFIGURATION", item.Name);
        Assert.Equal("Configuration", item.DisplayName);
        Assert.Equal("CFG", item.CodePrefix);
        Assert.Equal(200000, item.CodeFrom);
        Assert.Equal(299999, item.CodeTo);
        Assert.Equal("CONFIGURATION", Assert.Single(item.DefaultCategories));
        Assert.Equal("RETRY", Assert.Single(item.DefaultTags));
        Assert.Equal("503", item.DefaultMappings["web.httpStatus"]);
        Assert.Equal("before", item.Metadata["note"]);
        Assert.Equal("before", captured.Metadata["note"]);
        Assert.Equal("catalog-tag", Assert.Single(captured.Tags));

        document.CatalogName = "Changed";
        document.Tags.Add("later");
        document.Metadata.Set("note", "after");
        group.Name = "CHANGED";
        group.CodeFrom = 0;
        group.DefaultCategories.Add("LATER");
        group.DefaultTags.Clear();
        group.DefaultMappings["web.httpStatus"] = "200";
        group.Metadata.Set("note", "after");
        document.CodeGroups.Add(new ErrorCodeGroupDefinition { Name = "ADDED" });

        Assert.Equal("Initial groups", captured.CatalogName);
        Assert.Equal("before", captured.Metadata["note"]);
        Assert.Single(captured.Tags);
        Assert.Single(captured.CodeGroups);
        Assert.Equal("CONFIGURATION", item.Name);
        Assert.Equal(200000, item.CodeFrom);
        Assert.Equal("CONFIGURATION", Assert.Single(item.DefaultCategories));
        Assert.Equal("RETRY", Assert.Single(item.DefaultTags));
        Assert.Equal("503", item.DefaultMappings["web.httpStatus"]);
        Assert.Equal("before", item.Metadata["note"]);
        Assert.True(item.DefaultMappings.ContainsKey("WEB.HTTPSTATUS"));
        Assert.True(item.Metadata.ContainsKey("NOTE"));

        Assert.Throws<NotSupportedException>(
            () => ((IList<ErrorCodeGroupDefinitionSnapshot>)captured.CodeGroups).Clear());
        Assert.Throws<NotSupportedException>(
            () => ((IList<string>)item.DefaultCategories).Add("DENIED"));
        Assert.Throws<NotSupportedException>(
            () => ((IList<string>)item.DefaultTags).Add("DENIED"));
        Assert.Throws<NotSupportedException>(
            () => ((IDictionary<string, string>)item.DefaultMappings).Add("DENIED", "value"));
        Assert.Throws<NotSupportedException>(
            () => ((IDictionary<string, string>)captured.Metadata).Add("DENIED", "value"));
    }

    [Fact]
    public void CodeGroupSnapshot_SelectsOneContextAndKeepsEarlierCapturedValues()
    {
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext { CodeGroupCatalog = CreateCatalog() }));
        ErrorCodeGroupCatalogSnapshot first =
            Assert.IsType<ErrorCodeGroupCatalogSnapshot>(
                runtime.GetCodeGroupCatalogSnapshot().Data);
        Assert.Equal(1, runtime.ContextReads);

        ErrorCodeGroupCatalogDocument replacement = CreateCatalog();
        replacement.CodeGroups[0].Name = "OTHER";
        runtime.Current = Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext { CodeGroupCatalog = replacement });

        ErrorCodeGroupCatalogSnapshot second =
            Assert.IsType<ErrorCodeGroupCatalogSnapshot>(
                runtime.GetCodeGroupCatalogSnapshot().Data);

        Assert.Equal(2, runtime.ContextReads);
        Assert.Equal("CONFIGURATION", Assert.Single(first.CodeGroups).Name);
        Assert.Equal("OTHER", Assert.Single(second.CodeGroups).Name);
        Assert.NotSame(first, second);
    }

    [Fact]
    public void CodeGroupSnapshot_ForwardsUninitializedContextResponseWithoutData()
    {
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Invalid(
            code: "ErrorCatalogContextNotInitialized",
            message: "No active catalog."));

        Response<ErrorCodeGroupCatalogSnapshot> result =
            runtime.GetCodeGroupCatalogSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "ErrorCatalogContextNotInitialized");
        Assert.Equal(1, runtime.ContextReads);
    }

    [Fact]
    public void CodeGroupSnapshot_MissingCodeGroupCatalogReturnsStableInvalid()
    {
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext()));

        Response<ErrorCodeGroupCatalogSnapshot> result =
            runtime.GetCodeGroupCatalogSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_CODE_GROUP_SNAPSHOT_CATALOG_NULL");
    }

    [Fact]
    public void CodeGroupSnapshot_MalformedNestedDataReturnsStableFailure()
    {
        ErrorCodeGroupCatalogDocument document = CreateCatalog();
        document.CodeGroups[0].DefaultCategories = null!;
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext { CodeGroupCatalog = document }));

        Response<ErrorCodeGroupCatalogSnapshot> result =
            runtime.GetCodeGroupCatalogSnapshot();

        Assert.Equal(ResultStatus.Failed, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_CODE_GROUP_SNAPSHOT_FAILED");
        Assert.DoesNotContain("NullReferenceException", result.Message);
    }

    [Fact]
    public void CodeGroupSnapshot_SealedGetterOnlyShapeDoesNotChangeRuntimeInterface()
    {
        foreach (Type type in new[]
                 {
                     typeof(ErrorCodeGroupCatalogSnapshot),
                     typeof(ErrorCodeGroupDefinitionSnapshot)
                 })
        {
            Assert.True(type.IsPublic);
            Assert.True(type.IsSealed);
            Assert.Empty(type.GetConstructors(
                BindingFlags.Public | BindingFlags.Instance));
            Assert.All(type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                property => Assert.Null(property.SetMethod));
        }

        Assert.Equal(11, typeof(ErrorCodeGroupCatalogSnapshot).GetProperties().Length);
        Assert.Equal(10, typeof(ErrorCodeGroupDefinitionSnapshot).GetProperties().Length);
        Assert.DoesNotContain(typeof(IErrorCatalogRuntime).GetMethods(),
            method => method.Name == "GetCodeGroupCatalogSnapshot");
    }

    private static ErrorCodeGroupCatalogDocument CreateCatalog()
    {
        ErrorCodeGroupDefinition group = new()
        {
            Name = "CONFIGURATION",
            DisplayName = "Configuration",
            CodePrefix = "CFG",
            CodeFrom = 200000,
            CodeTo = 299999,
            DefaultCategories = ["CONFIGURATION"],
            DefaultTags = ["RETRY"],
            DefaultMappings = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["web.httpStatus"] = "503"
            }
        };
        group.Metadata.Set("note", "before");

        ErrorCodeGroupCatalogDocument document = new()
        {
            CatalogId = "code-groups.example",
            CatalogName = "Initial groups",
            Tags = ["catalog-tag"],
            CodeGroups = [group]
        };
        document.Metadata.Set("note", "before");
        return document;
    }

    private sealed class StubRuntime(
        Response<ErrorCatalogContext> initial) : IErrorCatalogRuntime
    {
        public Response<ErrorCatalogContext> Current { get; set; } = initial;
        public int ContextReads { get; private set; }

        public Response<ErrorCatalogContext> GetCurrentContext()
        {
            ContextReads++;
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
