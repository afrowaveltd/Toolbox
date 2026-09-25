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

public sealed class ErrorOwnerCatalogSnapshotContractTests
{
    [Fact]
    public void OwnerSnapshot_DetachesDocumentDefinitionAndEveryNestedCollection()
    {
        ErrorOwnerCatalogDocument document = CreateOwnerCatalog();
        ErrorOwnerDefinition owner = Assert.Single(document.Owners);
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext { OwnerCatalog = document }));

        Response<ErrorOwnerCatalogSnapshot> response =
            runtime.GetOwnerCatalogSnapshot();

        Assert.True(response.IsSuccess);
        Assert.Equal(1, runtime.ContextReads);
        ErrorOwnerCatalogSnapshot captured =
            Assert.IsType<ErrorOwnerCatalogSnapshot>(response.Data);
        ErrorOwnerDefinitionSnapshot item = Assert.Single(captured.Owners);

        Assert.Equal("owners.example", captured.CatalogId);
        Assert.Equal("Initial owners", captured.CatalogName);
        Assert.Equal("en", captured.Language);
        Assert.Equal("AFW", item.Name);
        Assert.Equal("Afrowave", item.DisplayName);
        Assert.Equal(100, item.CodeFrom);
        Assert.Equal(199, item.CodeTo);
        Assert.True(item.IsBuiltIn);
        Assert.Equal("LEGACY", Assert.Single(item.Aliases));
        Assert.Equal("503", item.DefaultMappings["web.httpStatus"]);
        Assert.Equal("before", item.Metadata["note"]);
        Assert.Equal("before", captured.Metadata["note"]);
        Assert.Equal("catalog-tag", Assert.Single(captured.Tags));

        document.CatalogName = "Changed";
        document.Tags.Add("added");
        document.Metadata.Set("note", "after");
        owner.Name = "CHANGED";
        owner.CodeFrom = 500;
        owner.Aliases.Add("NEW");
        owner.DefaultMappings["web.httpStatus"] = "200";
        owner.Metadata.Set("note", "after");
        document.Owners.Add(new ErrorOwnerDefinition { Name = "ADDED" });

        Assert.Equal("Initial owners", captured.CatalogName);
        Assert.Equal("before", captured.Metadata["note"]);
        Assert.Single(captured.Tags);
        Assert.Single(captured.Owners);
        Assert.Equal("AFW", item.Name);
        Assert.Equal(100, item.CodeFrom);
        Assert.Equal("LEGACY", Assert.Single(item.Aliases));
        Assert.Equal("503", item.DefaultMappings["web.httpStatus"]);
        Assert.Equal("before", item.Metadata["note"]);
        Assert.True(item.DefaultMappings.ContainsKey("WEB.HTTPSTATUS"));
        Assert.True(item.Metadata.ContainsKey("NOTE"));

        Assert.Throws<NotSupportedException>(
            () => ((IList<ErrorOwnerDefinitionSnapshot>)captured.Owners).Clear());
        Assert.Throws<NotSupportedException>(
            () => ((IList<string>)item.Aliases).Add("DENIED"));
        Assert.Throws<NotSupportedException>(
            () => ((IDictionary<string, string>)item.DefaultMappings)
                .Add("DENIED", "value"));
        Assert.Throws<NotSupportedException>(
            () => ((IDictionary<string, string>)captured.Metadata)
                .Add("DENIED", "value"));
    }

    [Fact]
    public void OwnerSnapshot_OneContextReadAndOldCaptureSurvivesReplacement()
    {
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext { OwnerCatalog = CreateOwnerCatalog() }));

        ErrorOwnerCatalogSnapshot before = Assert.IsType<ErrorOwnerCatalogSnapshot>(
            runtime.GetOwnerCatalogSnapshot().Data);
        Assert.Equal(1, runtime.ContextReads);

        ErrorOwnerCatalogDocument replacement = CreateOwnerCatalog();
        replacement.Owners[0].Name = "NEW";
        runtime.Current = Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext { OwnerCatalog = replacement });

        ErrorOwnerCatalogSnapshot after = Assert.IsType<ErrorOwnerCatalogSnapshot>(
            runtime.GetOwnerCatalogSnapshot().Data);

        Assert.Equal(2, runtime.ContextReads);
        Assert.Equal("AFW", Assert.Single(before.Owners).Name);
        Assert.Equal("NEW", Assert.Single(after.Owners).Name);
        Assert.NotSame(before, after);
    }

    [Fact]
    public void OwnerSnapshot_ForwardsUninitializedRuntimeWithoutData()
    {
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Invalid(
            code: "ErrorCatalogContextNotInitialized",
            message: "The context is not initialized."));

        Response<ErrorOwnerCatalogSnapshot> result =
            runtime.GetOwnerCatalogSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "ErrorCatalogContextNotInitialized");
        Assert.Equal(1, runtime.ContextReads);
    }

    [Fact]
    public void OwnerSnapshot_RejectsMissingOwnerCatalog()
    {
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext()));

        Response<ErrorOwnerCatalogSnapshot> result =
            runtime.GetOwnerCatalogSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_OWNER_SNAPSHOT_CATALOG_NULL");
    }

    [Fact]
    public void OwnerSnapshot_MalformedNestedAliasesFailWithoutLeakingExceptionDetails()
    {
        ErrorOwnerCatalogDocument document = CreateOwnerCatalog();
        document.Owners[0].Aliases = null!;
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext { OwnerCatalog = document }));

        Response<ErrorOwnerCatalogSnapshot> result =
            runtime.GetOwnerCatalogSnapshot();

        Assert.Equal(ResultStatus.Failed, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_OWNER_SNAPSHOT_FAILED");
        Assert.DoesNotContain("NullReferenceException", result.Message);
    }

    [Fact]
    public void OwnerSnapshot_PublicTypesAreGetterOnlyAndRuntimeInterfaceIsUnchanged()
    {
        foreach (Type type in new[]
                 {
                     typeof(ErrorOwnerCatalogSnapshot),
                     typeof(ErrorOwnerDefinitionSnapshot)
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

        Assert.Equal(11, typeof(ErrorOwnerCatalogSnapshot).GetProperties().Length);
        Assert.Equal(9, typeof(ErrorOwnerDefinitionSnapshot).GetProperties().Length);
        Assert.DoesNotContain(typeof(IErrorCatalogRuntime).GetMethods(),
            method => method.Name == "GetOwnerCatalogSnapshot");
    }

    private static ErrorOwnerCatalogDocument CreateOwnerCatalog()
    {
        ErrorOwnerDefinition owner = new()
        {
            Name = "AFW",
            DisplayName = "Afrowave",
            CodeFrom = 100,
            CodeTo = 199,
            IsBuiltIn = true,
            Aliases = ["LEGACY"],
            DefaultMappings = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["web.httpStatus"] = "503"
            }
        };
        owner.Metadata.Set("note", "before");

        ErrorOwnerCatalogDocument document = new()
        {
            CatalogId = "owners.example",
            CatalogName = "Initial owners",
            Tags = ["catalog-tag"],
            Owners = [owner]
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
