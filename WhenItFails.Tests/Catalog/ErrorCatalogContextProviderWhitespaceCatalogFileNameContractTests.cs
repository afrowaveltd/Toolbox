using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderWhitespaceCatalogFileNameContractTests
{
    [Theory]
    [InlineData(FileNameKind.Error, "WIF_JSONS_ERROR_CATALOG_FILE_NAME_EMPTY", "The error catalog file name cannot be empty.")]
    [InlineData(FileNameKind.Category, "WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_EMPTY", "The category catalog file name cannot be empty.")]
    [InlineData(FileNameKind.CodeGroup, "WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_EMPTY", "The code group catalog file name cannot be empty.")]
    [InlineData(FileNameKind.Owner, "WIF_JSONS_OWNER_CATALOG_FILE_NAME_EMPTY", "The owner catalog file name cannot be empty.")]
    [InlineData(FileNameKind.Profile, "WIF_JSONS_PROFILE_CATALOG_FILE_NAME_EMPTY", "The profile catalog file name cannot be empty.")]
    public async Task LoadFromJsonsAsync_ShouldReturnInvalid_WhenCatalogFileNameIsWhitespace(
        FileNameKind fileNameKind,
        string expectedCode,
        string expectedMessage)
    {
        JsonsOptions options = new()
        {
            RootDirectory = "Jsons",
            PackageDirectoryName = "WhenItFails"
        };

        switch (fileNameKind)
        {
            case FileNameKind.Error:
                options.ErrorCatalogFileName = "   ";
                break;
            case FileNameKind.Category:
                options.CategoryCatalogFileName = "   ";
                break;
            case FileNameKind.CodeGroup:
                options.CodeGroupCatalogFileName = "   ";
                break;
            case FileNameKind.Owner:
                options.OwnerCatalogFileName = "   ";
                break;
            case FileNameKind.Profile:
                options.ProfilesFileName = "   ";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileNameKind));
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

    public enum FileNameKind
    {
        Error,
        Category,
        CodeGroup,
        Owner,
        Profile
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
