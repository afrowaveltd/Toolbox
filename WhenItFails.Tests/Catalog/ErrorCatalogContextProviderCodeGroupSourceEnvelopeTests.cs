using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderCodeGroupSourceEnvelopeTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldPreserveFullCodeGroupSourceEnvelopeAndUseOnlyFirstIssue()
    {
        const string sourceMessage = "The source code-group catalog provider rejected the catalog.";
        const string firstSourceCode = "SourceCodeGroupCatalogRejected";

        ErrorCatalogContextProvider provider = new(
            new SuccessfulErrorCatalogProvider(),
            new SuccessfulCategoryCatalogProvider(),
            new FullSourceCancelledCodeGroupCatalogProvider(
                sourceMessage,
                firstSourceCode,
                "SecondaryCodeGroupDiagnostic"),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(CreateOptions());

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Cancelled, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(sourceMessage, response.Message);

        var issue = Assert.Single(response.Issues);
        Assert.Equal(firstSourceCode, issue.Code);
        Assert.Equal(sourceMessage, issue.Message);
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

    private sealed class SuccessfulCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorCategoryCatalogProviderPayload>.Ok(
                new ErrorCategoryCatalogProviderPayload
                {
                    Document = new ErrorCategoryCatalogDocument(),
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }

    private sealed class FullSourceCancelledCodeGroupCatalogProvider(
        string message,
        string firstCode,
        string secondCode)
        : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new Response<ErrorCodeGroupCatalogProviderPayload>
            {
                Status = ResultStatus.Cancelled,
                Message = message,
                Issues =
                [
                    new IssueInfo
                    {
                        Code = firstCode,
                        Message = "This issue message must be replaced by the response message."
                    },
                    new IssueInfo
                    {
                        Code = secondCode,
                        Message = "This later issue must not be copied."
                    }
                ]
            });
        }
    }

    private sealed class UnexpectedOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The owner provider must not run after the code-group provider fails.");
        }
    }

    private sealed class UnexpectedProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The profile provider must not run after the code-group provider fails.");
        }
    }
}
