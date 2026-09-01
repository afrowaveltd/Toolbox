using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderNullErrorCatalogFileNameContractTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReturnInvalid_WhenErrorCatalogFileNameIsNull()
    {
        ErrorCatalogContextProvider provider = new(
            new ThrowingErrorCatalogProvider(),
            new ThrowingCategoryCatalogProvider(),
            new ThrowingCodeGroupCatalogProvider(),
            new ThrowingOwnerCatalogProvider(),
            new ThrowingProfileCatalogProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(
                new JsonsOptions
                {
                    RootDirectory = "Jsons",
                    PackageDirectoryName = "WhenItFails",
                    ErrorCatalogFileName = null!
                });

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal(
            "WIF_JSONS_ERROR_CATALOG_FILE_NAME_NULL",
            issue.Code);
        Assert.Equal(
            "The error catalog file name cannot be null.",
            response.Message);
    }

    private sealed class ThrowingErrorCatalogProvider : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The error catalog provider must not be called for invalid JSON options.");
        }
    }

    private sealed class ThrowingCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The category catalog provider must not be called for invalid JSON options.");
        }
    }

    private sealed class ThrowingCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The code-group catalog provider must not be called for invalid JSON options.");
        }
    }

    private sealed class ThrowingOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The owner catalog provider must not be called for invalid JSON options.");
        }
    }

    private sealed class ThrowingProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The profile catalog provider must not be called for invalid JSON options.");
        }
    }
}
