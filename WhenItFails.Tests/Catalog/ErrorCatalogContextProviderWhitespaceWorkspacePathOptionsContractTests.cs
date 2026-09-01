using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderWhitespaceWorkspacePathOptionsContractTests
{
    [Theory]
    [InlineData(
        WorkspacePathKind.RootDirectory,
        "WIF_JSONS_ROOT_DIRECTORY_EMPTY",
        "The JSON root directory cannot be empty.")]
    [InlineData(
        WorkspacePathKind.PackageDirectoryName,
        "WIF_JSONS_PACKAGE_DIRECTORY_NAME_EMPTY",
        "The package directory name cannot be empty.")]
    public async Task LoadFromJsonsAsync_ShouldReturnInvalid_WhenWorkspacePathOptionIsWhitespace(
        WorkspacePathKind workspacePathKind,
        string expectedCode,
        string expectedMessage)
    {
        JsonsOptions options = new();

        switch (workspacePathKind)
        {
            case WorkspacePathKind.RootDirectory:
                options.RootDirectory = "   ";
                break;
            case WorkspacePathKind.PackageDirectoryName:
                options.PackageDirectoryName = "   ";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(workspacePathKind));
        }

        ErrorCatalogContextProvider provider = new(
            new ThrowingErrorCatalogProvider(),
            new ThrowingCategoryCatalogProvider(),
            new ThrowingCodeGroupCatalogProvider(),
            new ThrowingOwnerCatalogProvider(),
            new ThrowingProfileCatalogProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(options);

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal(expectedCode, issue.Code);
        Assert.Equal(expectedMessage, response.Message);
    }

    public enum WorkspacePathKind
    {
        RootDirectory,
        PackageDirectoryName
    }

    private sealed class ThrowingErrorCatalogProvider : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "No provider should be called for invalid JSON options.");
        }
    }

    private sealed class ThrowingCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "No provider should be called for invalid JSON options.");
        }
    }

    private sealed class ThrowingCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "No provider should be called for invalid JSON options.");
        }
    }

    private sealed class ThrowingOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "No provider should be called for invalid JSON options.");
        }
    }

    private sealed class ThrowingProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "No provider should be called for invalid JSON options.");
        }
    }
}
