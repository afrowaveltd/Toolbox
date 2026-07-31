using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderNullFailureIssueElementTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldSkipNullFailureIssueAndPreserveFirstValidSourceCode()
    {
        const string sourceCode = "SourceErrorCatalogRejected";
        const string sourceMessage = "The source error catalog provider rejected the catalog.";

        ErrorCatalogContextProvider provider = new(
            new NullFirstIssueErrorCatalogProvider(sourceCode, sourceMessage),
            new UnexpectedCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(new JsonsOptions
            {
                RootDirectory = "Jsons",
                PackageDirectoryName = "WhenItFails"
            });

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(sourceMessage, response.Message);

        IssueInfo outputIssue = Assert.Single(response.Issues);
        Assert.Equal(sourceCode, outputIssue.Code);
        Assert.Equal(sourceMessage, outputIssue.Message);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldUseFallbackCode_WhenFirstNonNullFailureIssueCodeIsBlank()
    {
        const string sourceMessage = "The source error catalog provider rejected the catalog.";

        ErrorCatalogContextProvider provider = new(
            new NullThenBlankThenValidIssueErrorCatalogProvider(sourceMessage),
            new UnexpectedCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(new JsonsOptions
            {
                RootDirectory = "Jsons",
                PackageDirectoryName = "WhenItFails"
            });

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(sourceMessage, response.Message);

        IssueInfo outputIssue = Assert.Single(response.Issues);
        Assert.Equal("ErrorCatalogContextErrorCatalogLoadFailed", outputIssue.Code);
        Assert.Equal(sourceMessage, outputIssue.Message);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldUseFallbackEnvelope_WhenAllFailureIssueElementsAreNull()
    {
        ErrorCatalogContextProvider provider = new(
            new AllNullIssuesErrorCatalogProvider(),
            new UnexpectedCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(new JsonsOptions
            {
                RootDirectory = "Jsons",
                PackageDirectoryName = "WhenItFails"
            });

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "Error catalog loading failed while creating catalog context.",
            response.Message);

        IssueInfo outputIssue = Assert.Single(response.Issues);
        Assert.Equal("ErrorCatalogContextErrorCatalogLoadFailed", outputIssue.Code);
        Assert.Equal(response.Message, outputIssue.Message);
    }

    private sealed class NullFirstIssueErrorCatalogProvider(
        string sourceCode,
        string sourceMessage)
        : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new Response<ErrorCatalogProviderPayload>
            {
                Status = ResultStatus.Invalid,
                Message = sourceMessage,
                Issues =
                [
                    null!,
                    new IssueInfo
                    {
                        Code = sourceCode,
                        Message = sourceMessage,
                        Severity = IssueSeverity.Error
                    }
                ]
            });
        }
    }

    private sealed class NullThenBlankThenValidIssueErrorCatalogProvider(string sourceMessage)
        : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new Response<ErrorCatalogProviderPayload>
            {
                Status = ResultStatus.Invalid,
                Message = sourceMessage,
                Issues =
                [
                    null!,
                    new IssueInfo
                    {
                        Code = "   ",
                        Message = "The first actual issue has no usable code.",
                        Severity = IssueSeverity.Error
                    },
                    new IssueInfo
                    {
                        Code = "LaterSourceCodeMustRemainSuppressed",
                        Message = "This later issue must not replace the first actual issue.",
                        Severity = IssueSeverity.Error
                    }
                ]
            });
        }
    }

    private sealed class AllNullIssuesErrorCatalogProvider : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new Response<ErrorCatalogProviderPayload>
            {
                Status = ResultStatus.NotFound,
                Issues = [null!, null!]
            });
        }
    }

    private sealed class UnexpectedCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Category provider must not run after failure.");
        }
    }

    private sealed class UnexpectedCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Code-group provider must not run after failure.");
        }
    }

    private sealed class UnexpectedOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Owner provider must not run after failure.");
        }
    }

    private sealed class UnexpectedProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Profile provider must not run after failure.");
        }
    }
}
