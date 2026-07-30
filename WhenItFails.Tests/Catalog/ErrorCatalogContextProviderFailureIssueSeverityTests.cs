using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderFailureIssueSeverityTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldCreateErrorIssueWithoutMutatingSourceIssue()
    {
        const string sourceCode = "SourceErrorCatalogRejected";
        const string sourceMessage = "The source error catalog provider rejected the catalog.";

        IssueInfo sourceIssue = new()
        {
            Code = sourceCode,
            Message = "The source issue message must not be reused.",
            Severity = IssueSeverity.Warning
        };

        ErrorCatalogContextProvider provider = new(
            new WarningIssueFailedErrorCatalogProvider(sourceMessage, sourceIssue),
            new UnexpectedCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(CreateOptions());

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(sourceMessage, response.Message);

        IssueInfo outputIssue = Assert.Single(response.Issues);
        Assert.NotSame(sourceIssue, outputIssue);
        Assert.Equal(sourceCode, outputIssue.Code);
        Assert.Equal(sourceMessage, outputIssue.Message);
        Assert.Equal(IssueSeverity.Error, outputIssue.Severity);

        Assert.Equal(IssueSeverity.Warning, sourceIssue.Severity);
        Assert.Equal("The source issue message must not be reused.", sourceIssue.Message);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldSuppressSourceOnlyIssueFieldsWithoutMutatingSourceIssue()
    {
        const string sourceCode = "SourceErrorCatalogRejected";
        const string sourceMessage = "The source error catalog provider rejected the catalog.";
        const string sourceDetails = "These provider-specific details must not cross the context boundary.";

        MetadataBag sourceMetadata = new();
        sourceMetadata["provider"] = "source-error-catalog";

        IssueInfo sourceIssue = new()
        {
            Code = sourceCode,
            Number = 417,
            Message = "The source issue message must not be reused.",
            Details = sourceDetails,
            Severity = IssueSeverity.Warning,
            Metadata = sourceMetadata
        };

        ErrorCatalogContextProvider provider = new(
            new WarningIssueFailedErrorCatalogProvider(sourceMessage, sourceIssue),
            new UnexpectedCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(CreateOptions());

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(sourceMessage, response.Message);

        IssueInfo outputIssue = Assert.Single(response.Issues);
        Assert.NotSame(sourceIssue, outputIssue);
        Assert.Equal(sourceCode, outputIssue.Code);
        Assert.Equal(sourceMessage, outputIssue.Message);
        Assert.Equal(IssueSeverity.Error, outputIssue.Severity);
        Assert.Null(outputIssue.Number);
        Assert.Null(outputIssue.Details);
        Assert.True(outputIssue.Metadata.IsEmpty);

        Assert.Equal(417, sourceIssue.Number);
        Assert.Equal(sourceDetails, sourceIssue.Details);
        Assert.Equal(IssueSeverity.Warning, sourceIssue.Severity);
        Assert.Equal("source-error-catalog", sourceIssue.Metadata["provider"]);
        Assert.Same(sourceMetadata, sourceIssue.Metadata);
    }

    private static JsonsOptions CreateOptions()
    {
        return new JsonsOptions
        {
            RootDirectory = "Jsons",
            PackageDirectoryName = "WhenItFails"
        };
    }

    private sealed class WarningIssueFailedErrorCatalogProvider(
        string message,
        IssueInfo sourceIssue)
        : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new Response<ErrorCatalogProviderPayload>
            {
                Status = ResultStatus.Failed,
                Message = message,
                Issues = [sourceIssue]
            });
        }
    }

    private sealed class UnexpectedCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The category provider must not run after the error provider fails.");
        }
    }

    private sealed class UnexpectedCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The code-group provider must not run after the error provider fails.");
        }
    }

    private sealed class UnexpectedOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The owner provider must not run after the error provider fails.");
        }
    }

    private sealed class UnexpectedProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The profile provider must not run after the error provider fails.");
        }
    }
}
