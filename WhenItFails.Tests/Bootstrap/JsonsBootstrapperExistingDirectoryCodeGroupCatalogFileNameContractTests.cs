using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperExistingDirectoryCodeGroupCatalogFileNameContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameResolvesToExistingDirectory_ReturnsInvalidBeforeProvider()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string packageDirectoryPath = Path.Combine(
            rootDirectory,
            "WhenItFails");

        string existingDirectoryPath = Path.Combine(
            packageDirectoryPath,
            "Nested");

        string existingFilePath = Path.Combine(
            existingDirectoryPath,
            "preserve.txt");

        TrackingTemplateProvider templateProvider = new();

        try
        {
            Directory.CreateDirectory(existingDirectoryPath);
            File.WriteAllText(existingFilePath, "Keep me.");

            JsonsBootstrapper bootstrapper = new(templateProvider);

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(
                    new JsonsOptions
                    {
                        RootDirectory = rootDirectory,
                        PackageDirectoryName = "WhenItFails",
                        CodeGroupCatalogFileName = "Nested"
                    });

            Assert.Equal(ResultStatus.Invalid, response.Status);
            Assert.False(response.IsSuccess);
            Assert.Null(response.Data);

            var issue = Assert.Single(response.Issues);
            Assert.Equal(
                "WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID",
                issue.Code);
            Assert.Equal(
                "The code group catalog file name is invalid.",
                response.Message);
            Assert.Equal(
                "The code group catalog file name is invalid.",
                issue.Message);

            Assert.False(templateProvider.WasCalled);
            Assert.True(Directory.Exists(existingDirectoryPath));
            Assert.Equal("Keep me.", File.ReadAllText(existingFilePath));
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    private sealed class TrackingTemplateProvider
        : IJsonsTemplateProvider
    {
        public bool WasCalled { get; private set; }

        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            WasCalled = true;
            return [];
        }
    }
}
