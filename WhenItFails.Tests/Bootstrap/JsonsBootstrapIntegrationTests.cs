using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapIntegrationTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_ShouldCreateDefaultJsonFiles()
    {
        string rootDirectory = CreateTemporaryRootDirectory();

        try
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
            Assert.Equal(ResultStatus.Success, response.Status);
            Assert.NotNull(response.Data);

            Assert.True(Directory.Exists(options.PackageDirectoryPath));

            Assert.True(File.Exists(options.ErrorCatalogFilePath));
            Assert.True(File.Exists(options.CategoryCatalogFilePath));
            Assert.True(File.Exists(options.CodeGroupCatalogFilePath));
            Assert.True(File.Exists(options.OwnerCatalogFilePath));
            Assert.True(File.Exists(options.ProfilesFilePath));

            Assert.Equal(5, response.Data.Files.Count);
            Assert.All(response.Data.Files, file => Assert.True(file.Created));
            Assert.All(response.Data.Files, file => Assert.False(file.Skipped));
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task EnsureWorkspaceAsync_ShouldNotOverwriteExistingDefaultJsonFiles()
    {
        string rootDirectory = CreateTemporaryRootDirectory();

        try
        {
            JsonsOptions options = new()
            {
                RootDirectory = rootDirectory,
                PackageDirectoryName = "WhenItFails"
            };

            JsonsBootstrapper bootstrapper = new(
                new DefaultJsonsTemplateProvider());

            Response<JsonsBootstrapPayload> firstResponse =
                await bootstrapper.EnsureWorkspaceAsync(options);

            Assert.True(firstResponse.IsSuccess);

            string customErrorCatalogContent = "{ \"custom\": true }";
            File.WriteAllText(
                options.ErrorCatalogFilePath,
                customErrorCatalogContent);

            Response<JsonsBootstrapPayload> secondResponse =
                await bootstrapper.EnsureWorkspaceAsync(options);

            Assert.True(secondResponse.IsSuccess);
            Assert.NotNull(secondResponse.Data);

            Assert.Equal(
                customErrorCatalogContent,
                File.ReadAllText(options.ErrorCatalogFilePath));

            JsonsBootstrapFileResult errorCatalogResult =
                Assert.Single(
                    secondResponse.Data.Files,
                    file => file.TargetFilePath == options.ErrorCatalogFilePath);

            Assert.True(errorCatalogResult.AlreadyExisted);
            Assert.False(errorCatalogResult.Created);
            Assert.True(errorCatalogResult.Skipped);
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task DefaultBootstrappedErrorCatalog_ShouldLoadNormalizeAndValidate()
    {
        string rootDirectory = CreateTemporaryRootDirectory();

        try
        {
            JsonsOptions options = await BootstrapWorkspaceAsync(rootDirectory);

            ErrorCatalogProvider provider = new(
                new JsonErrorCatalogLoader(),
                new ErrorCatalogDocumentNormalizer(),
                new ErrorCatalogValidator(),
                new ErrorCatalogFactory());

            Response<ErrorCatalogProviderPayload> response =
                await provider.LoadFromFileAsync(options.ErrorCatalogFilePath);

            Assert.True(response.IsSuccess);
            Assert.Equal(ResultStatus.Success, response.Status);
            Assert.NotNull(response.Data);
            Assert.True(response.Data.ValidationResult.IsValid);

            Assert.NotEmpty(response.Data.Catalog.GetAll());
            Assert.NotNull(response.Data.Catalog.FindById("AFW-GEN-0001"));
            Assert.NotNull(response.Data.Catalog.FindById("AFW-CFG-0001"));
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task DefaultBootstrappedCategoryCatalog_ShouldLoadNormalizeAndValidate()
    {
        string rootDirectory = CreateTemporaryRootDirectory();

        try
        {
            JsonsOptions options = await BootstrapWorkspaceAsync(rootDirectory);

            ErrorCategoryCatalogProvider provider = new(
                new JsonErrorCategoryCatalogLoader(),
                new ErrorCategoryCatalogDocumentNormalizer(),
                new ErrorCategoryCatalogValidator());

            Response<ErrorCategoryCatalogProviderPayload> response =
                await provider.LoadFromFileAsync(options.CategoryCatalogFilePath);

            Assert.True(response.IsSuccess);
            Assert.Equal(ResultStatus.Success, response.Status);
            Assert.NotNull(response.Data);
            Assert.True(response.Data.ValidationResult.IsValid);

            Assert.NotEmpty(response.Data.Document.Categories);
            Assert.Contains(
                response.Data.Document.Categories,
                category => category.Name == "GENERAL");

            Assert.Contains(
                response.Data.Document.Categories,
                category => category.Name == "CONFIGURATION");
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task DefaultBootstrappedCodeGroupCatalog_ShouldLoadNormalizeAndValidate()
    {
        string rootDirectory = CreateTemporaryRootDirectory();

        try
        {
            JsonsOptions options = await BootstrapWorkspaceAsync(rootDirectory);

            ErrorCodeGroupCatalogProvider provider = new(
                new JsonErrorCodeGroupCatalogLoader(),
                new ErrorCodeGroupCatalogDocumentNormalizer(),
                new ErrorCodeGroupCatalogValidator());

            Response<ErrorCodeGroupCatalogProviderPayload> response =
                await provider.LoadFromFileAsync(options.CodeGroupCatalogFilePath);

            Assert.True(response.IsSuccess);
            Assert.Equal(ResultStatus.Success, response.Status);
            Assert.NotNull(response.Data);
            Assert.True(response.Data.ValidationResult.IsValid);

            Assert.NotEmpty(response.Data.Document.CodeGroups);
            Assert.Contains(
                response.Data.Document.CodeGroups,
                codeGroup => codeGroup.Name == "GENERAL"
                    && codeGroup.CodePrefix == "GEN");

            Assert.Contains(
                response.Data.Document.CodeGroups,
                codeGroup => codeGroup.Name == "CONFIGURATION"
                    && codeGroup.CodePrefix == "CFG");
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task DefaultBootstrappedOwnerCatalog_ShouldLoadNormalizeAndValidate()
    {
        string rootDirectory = CreateTemporaryRootDirectory();

        try
        {
            JsonsOptions options = await BootstrapWorkspaceAsync(rootDirectory);

            ErrorOwnerCatalogProvider provider = new(
                new JsonErrorOwnerCatalogLoader(),
                new ErrorOwnerCatalogDocumentNormalizer(),
                new ErrorOwnerCatalogValidator());

            Response<ErrorOwnerCatalogProviderPayload> response =
                await provider.LoadFromFileAsync(options.OwnerCatalogFilePath);

            Assert.True(response.IsSuccess);
            Assert.Equal(ResultStatus.Success, response.Status);
            Assert.NotNull(response.Data);
            Assert.True(response.Data.ValidationResult.IsValid);

            Assert.NotEmpty(response.Data.Document.Owners);
            Assert.Contains(
                response.Data.Document.Owners,
                owner => owner.Name == "AFW"
                    && owner.IsBuiltIn);

            Assert.Contains(
                response.Data.Document.Owners,
                owner => owner.Name == "APP"
                    && !owner.IsBuiltIn);
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task DefaultBootstrappedProfileCatalog_ShouldLoadNormalizeAndValidate()
    {
        string rootDirectory = CreateTemporaryRootDirectory();

        try
        {
            JsonsOptions options = await BootstrapWorkspaceAsync(rootDirectory);

            ErrorProfileCatalogProvider provider = new(
                new JsonErrorProfileCatalogLoader(),
                new ErrorProfileCatalogDocumentNormalizer(),
                new ErrorProfileCatalogValidator());

            Response<ErrorProfileCatalogProviderPayload> response =
                await provider.LoadFromFileAsync(options.ProfilesFilePath);

            Assert.True(response.IsSuccess);
            Assert.Equal(ResultStatus.Success, response.Status);
            Assert.NotNull(response.Data);
            Assert.True(response.Data.ValidationResult.IsValid);

            Assert.NotNull(response.Data.Document.Profiles);
            Assert.NotEmpty(response.Data.Document.Profiles);

            Assert.Contains(
                response.Data.Document.Profiles,
                profile => profile.Name == "WEB");

            Assert.Contains(
                response.Data.Document.Profiles,
                profile => profile.Name == "API");

            Assert.Contains(
                response.Data.Document.Profiles,
                profile => profile.Name == "CLI");

            Assert.Contains(
                response.Data.Document.Profiles,
                profile => profile.Name == "DESKTOP");

            Assert.Contains(
                response.Data.Document.Profiles,
                profile => profile.Name == "DEVELOPMENT");

            Assert.Contains(
                response.Data.Document.Profiles,
                profile => profile.Name == "PRODUCTION");
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
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
            $"when-it-fails-bootstrap-integration-test-{Guid.NewGuid():N}");
    }

    private static void DeleteDirectoryIfExists(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }
}