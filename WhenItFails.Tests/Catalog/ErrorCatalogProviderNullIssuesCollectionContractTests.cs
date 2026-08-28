using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogProviderNullIssuesCollectionContractTests
{
    [Fact]
    public async Task LoadFromFileAsync_ShouldUseFallbackCode_WhenLoaderIssuesCollectionIsNull()
    {
        ErrorCatalogProvider provider = new(
            new NullIssuesFailedLoader(),
            new ErrorCatalogDocumentNormalizer(),
            new ErrorCatalogValidator(),
            new ErrorCatalogFactory());

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

    private sealed class NullIssuesFailedLoader : IErrorCatalogLoader
    {
        public Task<Response<ErrorCatalogDocument>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Response<ErrorCatalogDocument> response = new()
            {
                Status = ResultStatus.Failed,
                Data = null,
                Message = string.Empty,
                Issues = null!
            };

            return Task.FromResult(response);
        }
    }
}
