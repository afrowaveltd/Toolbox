using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperFileParentErrorCatalogFileNameContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenErrorCatalogFileNameParentIsExistingFile_ReturnsInvalidBeforeProvider()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string packageDirectoryPath = Path.Combine(
            rootDirectory,
            "WhenItFails");

        string existingParentFilePath = Path.Combine(
            packageDirectoryPath,
            "Nested");

        string targetFilePath = Path.Combine(
            packageDirectoryPath,
            "Nested",
            "errors.json");

        TrackingTemplateProvider templateProvider = new();

        try
        {
            Directory.CreateDirectory(packageDirectoryPath);
            File.WriteAllText(existingParentFilePath, "Keep me.");

            JsonsBootstrapper bootstrapper = new(templateProvider);

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(
                    new JsonsOptions
                    {
                        RootDirectory = rootDirectory,
                        PackageDirectoryName = "WhenItFails",
                        ErrorCatalogFileName = Path.Combine(
                            "Nested",
                            "errors.json")
                    });

            Assert.Equal(ResultStatus.Invalid, response.Status);
            Assert.False(response.IsSuccess);
            Assert.Null(response.Data);

            var issue = Assert.Single(response.Issues);
            Assert.Equal(
                "WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID",
                issue.Code);
            Assert.Equal(
                "The error catalog file name is invalid.",
                response.Message);
            Assert.Equal(
                "The error catalog file name is invalid.",
                issue.Message);

            Assert.False(templateProvider.WasCalled);
            Assert.False(File.Exists(targetFilePath));
            Assert.True(File.Exists(existingParentFilePath));
            Assert.False(Directory.Exists(existingParentFilePath));
            Assert.Equal("Keep me.", File.ReadAllText(existingParentFilePath));
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    private sealed class TrackingTemplateProvider : IJsonsTemplateProvider
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
