using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperFileParentRootDirectoryContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenRootDirectoryParentIsExistingFile_ReturnsInvalidBeforeProvider()
    {
        string testDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string existingParentFilePath = Path.Combine(
            testDirectory,
            "Blocked");

        string rootDirectory = Path.Combine(
            existingParentFilePath,
            "Jsons");

        string packageDirectoryPath = Path.Combine(
            rootDirectory,
            "WhenItFails");

        TrackingTemplateProvider templateProvider = new();

        try
        {
            Directory.CreateDirectory(testDirectory);
            File.WriteAllText(existingParentFilePath, "Keep me.");

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
                "WIF_JSONS_ROOT_DIRECTORY_INVALID",
                issue.Code);
            Assert.Equal(
                "The JSON root directory path is invalid.",
                response.Message);
            Assert.Equal(
                "The JSON root directory path is invalid.",
                issue.Message);

            Assert.False(templateProvider.WasCalled);
            Assert.True(File.Exists(existingParentFilePath));
            Assert.False(Directory.Exists(existingParentFilePath));
            Assert.False(Directory.Exists(rootDirectory));
            Assert.False(Directory.Exists(packageDirectoryPath));
            Assert.Equal(
                "Keep me.",
                File.ReadAllText(existingParentFilePath));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, recursive: true);
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
