using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class ErrorCatalogValidationSnapshotContractTests
{
    [Fact]
    public void Snapshot_DetachesIssueInstancesAndCapturesValidity()
    {
        ErrorCatalogValidationResult validation = new();
        ErrorCatalogValidationIssue sourceIssue = new()
        {
            Severity = ErrorCatalogValidationSeverity.Warning,
            Code = "MissingTag",
            Message = "Initial warning",
            ErrorId = "AFW-CFG-0001",
            ErrorName = "KnownError",
            Path = "errors[0].tags"
        };
        validation.AddIssue(sourceIssue);
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext { CrossValidationResult = validation }));

        Response<ErrorCatalogValidationSnapshot> response =
            runtime.GetCrossValidationSnapshot();

        Assert.True(response.IsSuccess);
        ErrorCatalogValidationSnapshot snapshot =
            Assert.IsType<ErrorCatalogValidationSnapshot>(response.Data);
        Assert.True(snapshot.IsValid);
        ErrorCatalogValidationIssueSnapshot captured =
            Assert.Single(snapshot.Issues);

        Assert.Equal(ErrorCatalogValidationSeverity.Warning, captured.Severity);
        Assert.Equal("MissingTag", captured.Code);
        Assert.Equal("Initial warning", captured.Message);
        Assert.Equal("AFW-CFG-0001", captured.ErrorId);
        Assert.Equal("KnownError", captured.ErrorName);
        Assert.Equal("errors[0].tags", captured.Path);
        Assert.Throws<NotSupportedException>(
            () => ((IList<ErrorCatalogValidationIssueSnapshot>)snapshot.Issues)
                .Clear());

        sourceIssue.Severity = ErrorCatalogValidationSeverity.Error;
        sourceIssue.Code = "Changed";
        sourceIssue.Message = "Changed after capture";
        sourceIssue.ErrorId = "DIFFERENT";
        validation.AddError("LaterError", "Not in the old snapshot");

        Assert.False(validation.IsValid);
        Assert.True(snapshot.IsValid);
        Assert.Single(snapshot.Issues);
        Assert.Equal(ErrorCatalogValidationSeverity.Warning, captured.Severity);
        Assert.Equal("MissingTag", captured.Code);
        Assert.Equal("Initial warning", captured.Message);
        Assert.Equal("AFW-CFG-0001", captured.ErrorId);
    }

    [Fact]
    public void Snapshot_EmptyAndErrorResultsKeepCapturedValidity()
    {
        ErrorCatalogValidationResult validation = new();
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext { CrossValidationResult = validation }));

        ErrorCatalogValidationSnapshot empty = Assert.IsType<ErrorCatalogValidationSnapshot>(
            runtime.GetCrossValidationSnapshot().Data);
        Assert.True(empty.IsValid);
        Assert.Empty(empty.Issues);

        validation.AddError("Blocked", "Validation failed");

        ErrorCatalogValidationSnapshot failed = Assert.IsType<ErrorCatalogValidationSnapshot>(
            runtime.GetCrossValidationSnapshot().Data);
        Assert.True(empty.IsValid);
        Assert.False(failed.IsValid);
        Assert.Equal("Blocked", Assert.Single(failed.Issues).Code);
        Assert.NotSame(empty, failed);
    }

    [Fact]
    public void Snapshot_PropagatesUninitializedContextFailureWithoutData()
    {
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Invalid(
            code: "ErrorCatalogContextNotInitialized",
            message: "Error catalog context has not been initialized."));

        Response<ErrorCatalogValidationSnapshot> response =
            runtime.GetCrossValidationSnapshot();

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.Contains(response.Issues,
            issue => issue.Code == "ErrorCatalogContextNotInitialized");
    }

    [Fact]
    public void Snapshot_RejectsMissingCrossValidationResult()
    {
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext()));

        Response<ErrorCatalogValidationSnapshot> response =
            runtime.GetCrossValidationSnapshot();

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.Contains(response.Issues,
            issue => issue.Code == "WIF_VALIDATION_SNAPSHOT_RESULT_NULL");
    }

    [Fact]
    public void Snapshot_PublicShapeDoesNotExposeMutableValidationModels()
    {
        Assert.True(typeof(ErrorCatalogValidationSnapshot).IsPublic);
        Assert.True(typeof(ErrorCatalogValidationIssueSnapshot).IsPublic);
        Assert.Empty(typeof(ErrorCatalogValidationSnapshot).GetConstructors());
        Assert.Empty(typeof(ErrorCatalogValidationIssueSnapshot).GetConstructors());
        Assert.All(typeof(ErrorCatalogValidationSnapshot).GetProperties(),
            property => Assert.Null(property.SetMethod));
        Assert.All(typeof(ErrorCatalogValidationIssueSnapshot).GetProperties(),
            property => Assert.Null(property.SetMethod));
        Assert.Equal(2, typeof(ErrorCatalogValidationSnapshot).GetProperties().Length);
        Assert.Equal(6, typeof(ErrorCatalogValidationIssueSnapshot).GetProperties().Length);
    }

    private sealed class StubRuntime(
        Response<ErrorCatalogContext> currentResponse) : IErrorCatalogRuntime
    {
        public Response<ErrorCatalogContext> GetCurrentContext() =>
            currentResponse;

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
