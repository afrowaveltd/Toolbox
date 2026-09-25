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

public sealed class ErrorDefinitionSnapshotContractTests
{
    [Fact]
    public void Snapshot_CopiesDefinitionListsAndMetadataWithoutExposingMutableSources()
    {
        ErrorDefinition definition = CreateDefinition();
        definition.Categories.Add("INITIAL");
        definition.Subcategories.Add("DETAIL");
        definition.Tags.Add("TAG_BEFORE");
        definition.Metadata.Set("ownerNote", "before");

        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            CreateContext(definition)));

        Response<IReadOnlyList<ErrorDefinitionSnapshot>> response =
            runtime.GetErrorDefinitionSnapshots();

        Assert.True(response.IsSuccess);
        IReadOnlyList<ErrorDefinitionSnapshot> snapshots =
            Assert.IsAssignableFrom<IReadOnlyList<ErrorDefinitionSnapshot>>(
                response.Data);
        ErrorDefinitionSnapshot item = Assert.Single(snapshots);

        Assert.Equal(definition.Id, item.Id);
        Assert.Equal(definition.Code, item.Code);
        Assert.Equal(definition.Name, item.Name);
        Assert.Equal("INITIAL", Assert.Single(item.Categories));
        Assert.Equal("DETAIL", Assert.Single(item.Subcategories));
        Assert.Equal("TAG_BEFORE", Assert.Single(item.Tags));
        Assert.Equal("before", item.Metadata["ownerNote"]);

        definition.Id = "AFW-CFG-CHANGED";
        definition.Title = "Changed title";
        definition.Categories.Add("LATER");
        definition.Subcategories.Clear();
        definition.Tags.Add("TAG_AFTER");
        definition.Metadata.Set("ownerNote", "after");

        Assert.Equal("AFW-CFG-0001", item.Id);
        Assert.Equal("Original title", item.Title);
        Assert.Single(item.Categories);
        Assert.Single(item.Subcategories);
        Assert.Single(item.Tags);
        Assert.Equal("before", item.Metadata["ownerNote"]);

        Assert.Throws<NotSupportedException>(
            () => ((IList<ErrorDefinitionSnapshot>)snapshots).Clear());
        Assert.Throws<NotSupportedException>(
            () => ((IList<string>)item.Tags).Add("DENIED"));
        Assert.Throws<NotSupportedException>(
            () => ((IDictionary<string, string>)item.Metadata)
                .Add("DENIED", "value"));
        Assert.All(
            typeof(ErrorDefinitionSnapshot).GetProperties(),
            property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public void Snapshot_RemainsDetachedAfterActiveContextIsReplaced()
    {
        ErrorDefinition oldDefinition = CreateDefinition();
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            CreateContext(oldDefinition)));

        ErrorDefinitionSnapshot oldSnapshot = Assert.Single(
            runtime.GetErrorDefinitionSnapshots().Data!);

        ErrorDefinition newDefinition = CreateDefinition();
        newDefinition.Title = "New active title";
        runtime.CurrentResponse = Response<ErrorCatalogContext>.Ok(
            CreateContext(newDefinition));

        ErrorDefinitionSnapshot newSnapshot = Assert.Single(
            runtime.GetErrorDefinitionSnapshots().Data!);

        Assert.Equal("Original title", oldSnapshot.Title);
        Assert.Equal("New active title", newSnapshot.Title);
        Assert.NotSame(oldSnapshot, newSnapshot);
    }

    [Fact]
    public void Snapshot_PropagatesUninitializedContextFailureWithoutData()
    {
        StubRuntime runtime = new(
            Response<ErrorCatalogContext>.Invalid(
                code: "ErrorCatalogContextNotInitialized",
                message: "Error catalog context has not been initialized."));

        Response<IReadOnlyList<ErrorDefinitionSnapshot>> result =
            runtime.GetErrorDefinitionSnapshots();

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(
            result.Issues,
            issue => issue.Code == "ErrorCatalogContextNotInitialized");
    }

    [Fact]
    public void Snapshot_RejectsContextWithoutCatalogWithStableInvalidResult()
    {
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext()));

        Response<IReadOnlyList<ErrorDefinitionSnapshot>> result =
            runtime.GetErrorDefinitionSnapshots();

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(
            result.Issues,
            issue => issue.Code == "WIF_ERROR_DEFINITION_SNAPSHOT_CATALOG_NULL");
    }

    private static ErrorDefinition CreateDefinition() => new()
    {
        Id = "AFW-CFG-0001",
        Code = 200001,
        Name = "KNOWN_CONFIGURATION_ERROR",
        Owner = "AFW",
        CodePrefix = "CFG",
        CodeGroup = "CONFIGURATION",
        PrimaryCategory = "CONFIGURATION",
        Title = "Original title",
        Message = "Original message"
    };

    private static ErrorCatalogContext CreateContext(
        ErrorDefinition definition) => new()
    {
        ErrorCatalog = new ErrorCatalog([definition]),
        ErrorCatalogDocument = new ErrorCatalogDocument
        {
            Errors = [definition]
        }
    };

    private sealed class StubRuntime(
        Response<ErrorCatalogContext> currentResponse) : IErrorCatalogRuntime
    {
        public Response<ErrorCatalogContext> CurrentResponse { get; set; } =
            currentResponse;

        public Response<ErrorCatalogContext> GetCurrentContext() =>
            CurrentResponse;

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
