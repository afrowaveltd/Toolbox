using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperFileParentTemplateTargetContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenLaterTemplateTargetParentIsExistingFile_ReturnsInvalidWithoutPartialWrites()
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

        string firstTargetFilePath = Path.Combine(
            packageDirectoryPath,
            "first.json");

        string nestedTargetFilePath = Path.Combine(
            packageDirectoryPath,
            "Nested",
            "child.json");

        try
        {
            Directory.CreateDirectory(packageDirectoryPath);
            File.WriteAllText(existingParentFilePath, "Keep me.");

            JsonsBootstrapper bootstrapper = new(
                new FileParentTargetTemplateProvider());

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
                "WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID",
                issue.Code);
            Assert.Equal(
                "The JSON template provider returned a template with an invalid target file name.",
                response.Message);
            Assert.Equal(
                "The JSON template provider returned a template with an invalid target file name.",
                issue.Message);

            Assert.False(File.Exists(firstTargetFilePath));
            Assert.False(File.Exists(nestedTargetFilePath));
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

    private sealed class FileParentTargetTemplateProvider : IJsonsTemplateProvider
    {
        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            return
            [
                new JsonsTemplateFile
                {
                    Name = "First",
                    TargetFileName = "first.json",
                    Content = "{}"
                },
                new JsonsTemplateFile
                {
                    Name = "Nested child",
                    TargetFileName = Path.Combine("Nested", "child.json"),
                    Content = "{}"
                }
            ];
        }
    }
}
