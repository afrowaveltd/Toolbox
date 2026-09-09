using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderErrorCatalogProviderExceptionContractTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_WhenErrorCatalogProviderThrows_ReturnsStableFailure()
    {
        ErrorCatalogContextProvider provider = new(
            new ThrowingErrorCatalogProvider(),
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

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The error catalog provider failed.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal(
                    "WIF_ERROR_CATALOG_PROVIDER_FAILED",
                    issue.Code);
                Assert.Equal(
                    "The error catalog provider failed.",
                    issue.Message);
            });
    }

    private sealed class ThrowingErrorCatalogProvider : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            return Task.FromException<Response<ErrorCatalogProviderPayload>>(
                new InvalidOperationException(
                    "Sensitive error catalog provider detail must not escape."));
        }
    }

    private sealed class UnexpectedCategoryCatalogProvider
        : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Unexpected category catalog provider call.");
        }
    }

    private sealed class UnexpectedCodeGroupCatalogProvider
        : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Unexpected code group catalog provider call.");
        }
    }

    private sealed class UnexpectedOwnerCatalogProvider
        : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Unexpected owner catalog provider call.");
        }
    }

    private sealed class UnexpectedProfileCatalogProvider
        : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Unexpected profile catalog provider call.");
        }
    }
}
