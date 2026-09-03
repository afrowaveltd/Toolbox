using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogProviderNullFirstIssueContractTests
{
    [Fact]
    public async Task LoadFromFileAsync_ShouldUseFallbackCode_WhenFirstLoaderIssueIsNull()
    {
        Response<ErrorCatalogDocument> loaderResponse = new()
        {
            Status = ResultStatus.Failed,
            Message = string.Empty,
            Data = null,
            Issues = [null!]
        };

        ErrorCatalogProvider provider = new(
            new FakeLoader(loaderResponse),
            new UnexpectedNormalizer(),
            new UnexpectedValidator(),
            new UnexpectedFactory());

        Response<ErrorCatalogProviderPayload> response =
            await provider.LoadFromFileAsync("catalog.json");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal("Error catalog loading failed.", response.Message);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("CatalogLoadFailed", issue.Code);
        Assert.Equal("Error catalog loading failed.", issue.Message);
    }

    private sealed class FakeLoader(Response<ErrorCatalogDocument> response)
        : IErrorCatalogLoader
    {
        public Task<Response<ErrorCatalogDocument>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(response);
        }
    }

    private sealed class UnexpectedNormalizer : IErrorCatalogDocumentNormalizer
    {
        public ErrorCatalogDocument Normalize(ErrorCatalogDocument document)
        {
            throw new InvalidOperationException(
                "The normalizer must not be called after a failed loader response.");
        }
    }

    private sealed class UnexpectedValidator : IErrorCatalogValidator
    {
        public ErrorCatalogValidationResult Validate(ErrorCatalogDocument document)
        {
            throw new InvalidOperationException(
                "The validator must not be called after a failed loader response.");
        }
    }

    private sealed class UnexpectedFactory : IErrorCatalogFactory
    {
        public IErrorCatalog Create(ErrorCatalogDocument document)
        {
            throw new InvalidOperationException(
                "The factory must not be called after a failed loader response.");
        }
    }
}
