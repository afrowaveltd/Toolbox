using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperNestedTemplateTargetFileNameContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenTemplateTargetIsNestedInsidePackage_CreatesParentDirectoryAndFile()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string packageDirectoryPath = Path.Combine(
            rootDirectory,
            "WhenItFails");

        string nestedDirectoryPath = Path.Combine(
            packageDirectoryPath,
            "Nested");

        string nestedFilePath = Path.Combine(
            nestedDirectoryPath,
            "errors.en.json");

        try
        {
            JsonsBootstrapper bootstrapper = new(
                new NestedTargetTemplateProvider());

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(
                    new JsonsOptions
                    {
                        RootDirectory = rootDirectory,
                        PackageDirectoryName = "WhenItFails"
                    });

            Assert.True(response.IsSuccess);
            Assert.Equal(ResultStatus.Success, response.Status);
            Assert.NotNull(response.Data);

            Assert.True(Directory.Exists(nestedDirectoryPath));
            Assert.True(File.Exists(nestedFilePath));
            Assert.Equal("{}", File.ReadAllText(nestedFilePath));

            JsonsBootstrapFileResult fileResult =
                Assert.Single(response.Data.Files);

            Assert.Equal("Nested errors", fileResult.Name);
            Assert.Equal(nestedFilePath, fileResult.TargetFilePath);
            Assert.False(fileResult.AlreadyExisted);
            Assert.True(fileResult.Created);
            Assert.False(fileResult.Skipped);
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    private sealed class NestedTargetTemplateProvider
        : IJsonsTemplateProvider
    {
        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            return
            [
                new JsonsTemplateFile
                {
                    Name = "Nested errors",
                    TargetFileName = Path.Combine(
                        "Nested",
                        "errors.en.json"),
                    Content = "{}"
                }
            ];
        }
    }
}
