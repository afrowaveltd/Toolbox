using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderCategorySourceMessageFallbackTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldPreserveCategorySourceMessageAndUseFallbackCode_WhenFailureHasNoIssues()
    {
        const string sourceMessage = "The source category catalog provider rejected the catalog.";

        ErrorCatalogContextProvider provider = new(
            new SuccessfulErrorCatalogProvider(),
            new MessageOnlyNotSupportedCategoryCatalogProvider(sourceMessage),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(CreateOptions());

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.NotSupported, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(sourceMessage, response.Message);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("ErrorCatalogContextCategoryCatalogLoadFailed", issue.Code);
        Assert.Equal(sourceMessage, issue.Message);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldPreserveFirstCategorySourceIssueCodeAndSourceMessage()
    {
        const string firstSourceCode = "SourceCategoryCatalogRejected";
        const string secondSourceCode = "SourceCategoryCatalogSecondary";
        const string sourceMessage = "The category catalog source supplied a detailed failure.";

        ErrorCatalogContextProvider provider = new(
            new SuccessfulErrorCatalogProvider(),
            new FullSourceNotSupportedCategoryCatalogProvider(
                firstSourceCode,
                secondSourceCode,
                sourceMessage),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(CreateOptions());

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.NotSupported, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(sourceMessage, response.Message);

        var issue = Assert.Single(response.Issues);
        Assert.Equal(firstSourceCode, issue.Code);
        Assert.Equal(sourceMessage, issue.Message);
        Assert.DoesNotContain(response.Issues, candidate => candidate.Code == secondSourceCode);
    }

    private static JsonsOptions CreateOptions()
    {
        return new JsonsOptions
        {
            RootDirectory = "Jsons",
            PackageDirectoryName = "WhenItFails"
        };
    }

    private sealed class SuccessfulErrorCatalogProvider : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ErrorCatalogDocument document = new();

            return Task.FromResult(Response<ErrorCatalogProviderPayload>.Ok(
                new ErrorCatalogProviderPayload
                {
                    Catalog = new ErrorCatalog(document.Errors),
                    Document = document,
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }

    private sealed class MessageOnlyNotSupportedCategoryCatalogProvider(string message)
        : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new Response<ErrorCategoryCatalogProviderPayload>
            {
                Status = ResultStatus.NotSupported,
                Message = message
            });
        }
    }

    private sealed class FullSourceNotSupportedCategoryCatalogProvider(
        string firstCode,
        string secondCode,
        string message) : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new Response<ErrorCategoryCatalogProviderPayload>
            {
                Status = ResultStatus.NotSupported,
                Message = message,
                Issues =
                [
                    new IssueInfo
                    {
                        Code = firstCode,
                        Message = "The first source issue message must not replace the response message."
                    },
                    new IssueInfo
                    {
                        Code = secondCode,
                        Message = "The second source issue must not escape the context boundary."
                    }
                ]
            });
        }
    }

    private sealed class UnexpectedCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The code-group provider must not run after the category provider fails.");
        }
    }

    private sealed class UnexpectedOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The owner provider must not run after the category provider fails.");
        }
    }

    private sealed class UnexpectedProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The profile provider must not run after the category provider fails.");
        }
    }
}
