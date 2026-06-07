using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderIntegrationTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldCreateContext_FromDefaultBootstrappedJsons()
    {
        string rootDirectory = CreateTemporaryRootDirectory();

        try
        {
            JsonsOptions options = await BootstrapWorkspaceAsync(rootDirectory);

            ErrorCatalogContextProvider contextProvider = CreateContextProvider();

            Response<ErrorCatalogContext> response =
                await contextProvider.LoadFromJsonsAsync(options);

            Assert.True(response.IsSuccess);
            Assert.Equal(ResultStatus.Success, response.Status);
            Assert.NotNull(response.Data);

            Assert.NotNull(response.Data.ErrorCatalog);
            Assert.NotNull(response.Data.CategoryCatalog);
            Assert.NotNull(response.Data.CodeGroupCatalog);
            Assert.NotNull(response.Data.OwnerCatalog);
            Assert.NotNull(response.Data.ProfileCatalog);

            Assert.NotEmpty(response.Data.ErrorCatalog.GetAll());
            Assert.NotEmpty(response.Data.CategoryCatalog.Categories);
            Assert.NotEmpty(response.Data.CodeGroupCatalog.CodeGroups);
            Assert.NotEmpty(response.Data.OwnerCatalog.Owners);
            Assert.NotEmpty(response.Data.ProfileCatalog.Profiles);

            Assert.NotNull(response.Data.ErrorCatalog.FindById("AFW-GEN-0001"));
            Assert.NotNull(response.Data.ErrorCatalog.FindById("AFW-CFG-0001"));

            Assert.Contains(
                response.Data.CategoryCatalog.Categories,
                category => category.Name == "GENERAL");

            Assert.Contains(
                response.Data.CodeGroupCatalog.CodeGroups,
                codeGroup => codeGroup.Name == "GENERAL"
                    && codeGroup.CodePrefix == "GEN");

            Assert.Contains(
                response.Data.OwnerCatalog.Owners,
                owner => owner.Name == "AFW"
                    && owner.IsBuiltIn);

            Assert.Contains(
                response.Data.ProfileCatalog.Profiles,
                profile => profile.Name == "WEB");

            Assert.Contains(
                response.Data.ProfileCatalog.Profiles,
                profile => profile.Name == "PRODUCTION");
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReturnNotFound_WhenOneBootstrappedFileIsMissing()
    {
        string rootDirectory = CreateTemporaryRootDirectory();

        try
        {
            JsonsOptions options = await BootstrapWorkspaceAsync(rootDirectory);

            File.Delete(options.OwnerCatalogFilePath);

            ErrorCatalogContextProvider contextProvider = CreateContextProvider();

            Response<ErrorCatalogContext> response =
                await contextProvider.LoadFromJsonsAsync(options);

            Assert.False(response.IsSuccess);
            Assert.Equal(ResultStatus.NotFound, response.Status);
            Assert.NotEmpty(response.Issues);
            Assert.Equal("FileNotFound", response.Issues[0].Code);
            Assert.Null(response.Data);
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    private static ErrorCatalogContextProvider CreateContextProvider()
    {
        return new ErrorCatalogContextProvider(
            new ErrorCatalogProvider(
                new JsonErrorCatalogLoader(),
                new ErrorCatalogDocumentNormalizer(),
                new ErrorCatalogValidator(),
                new ErrorCatalogFactory()),
            new ErrorCategoryCatalogProvider(
                new JsonErrorCategoryCatalogLoader(),
                new ErrorCategoryCatalogDocumentNormalizer(),
                new ErrorCategoryCatalogValidator()),
            new ErrorCodeGroupCatalogProvider(
                new JsonErrorCodeGroupCatalogLoader(),
                new ErrorCodeGroupCatalogDocumentNormalizer(),
                new ErrorCodeGroupCatalogValidator()),
            new ErrorOwnerCatalogProvider(
                new JsonErrorOwnerCatalogLoader(),
                new ErrorOwnerCatalogDocumentNormalizer(),
                new ErrorOwnerCatalogValidator()),
            new ErrorProfileCatalogProvider(
                new JsonErrorProfileCatalogLoader(),
                new ErrorProfileCatalogDocumentNormalizer(),
                new ErrorProfileCatalogValidator()));
    }

    private static async Task<JsonsOptions> BootstrapWorkspaceAsync(string rootDirectory)
    {
        JsonsOptions options = new()
        {
            RootDirectory = rootDirectory,
            PackageDirectoryName = "WhenItFails"
        };

        JsonsBootstrapper bootstrapper = new(
            new DefaultJsonsTemplateProvider());

        Response<JsonsBootstrapPayload> response =
            await bootstrapper.EnsureWorkspaceAsync(options);

        Assert.True(response.IsSuccess);

        return options;
    }

    private static string CreateTemporaryRootDirectory()
    {
        return Path.Combine(
            Path.GetTempPath(),
            $"when-it-fails-context-integration-test-{Guid.NewGuid():N}");
    }

    private static void DeleteDirectoryIfExists(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }
}