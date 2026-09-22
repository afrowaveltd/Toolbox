using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperExistingFilePackageDirectoryContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenPackageDirectoryPathIsExistingFile_ReturnsInvalidBeforeProvider()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string packageDirectoryPath = Path.Combine(
            rootDirectory,
            "WhenItFails");

        TrackingTemplateProvider templateProvider = new();

        try
        {
            Directory.CreateDirectory(rootDirectory);
            File.WriteAllText(packageDirectoryPath, "Keep me.");

            JsonsBootstrapper bootstrapper = new(templateProvider);

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(
                    new JsonsOptions
                    {
                        RootDirectory = rootDirectory,
                        PackageDirectoryName = "WhenItFails"
                    });

            Assert.Equal(ResultStatus.Invalid, response.Status);
            Assert.False(response.IsSuccess);
            Assert.Null(response.Data);

            var issue = Assert.Single(response.Issues);
            Assert.Equal(
                "WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID",
                issue.Code);
            Assert.Equal(
                "The package directory name is invalid.",
                response.Message);
            Assert.Equal(
                "The package directory name is invalid.",
                issue.Message);

            Assert.False(templateProvider.WasCalled);
            Assert.True(File.Exists(packageDirectoryPath));
            Assert.False(Directory.Exists(packageDirectoryPath));
            Assert.Equal(
                "Keep me.",
                File.ReadAllText(packageDirectoryPath));
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
