using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperDuplicateCanonicalTemplateTargetContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenTemplateTargetsResolveToSameFile_ReturnsInvalidWithoutPartialWrites()
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

        string targetFilePath = Path.Combine(
            nestedDirectoryPath,
            "item.json");

        try
        {
            JsonsBootstrapper bootstrapper = new(
                new DuplicateCanonicalTargetTemplateProvider());

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
                "WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_DUPLICATE",
                issue.Code);
            Assert.Equal(
                "The JSON template provider returned multiple templates for the same target file.",
                response.Message);
            Assert.Equal(
                "The JSON template provider returned multiple templates for the same target file.",
                issue.Message);

            Assert.True(Directory.Exists(packageDirectoryPath));
            Assert.False(Directory.Exists(nestedDirectoryPath));
            Assert.False(File.Exists(targetFilePath));
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    private sealed class DuplicateCanonicalTargetTemplateProvider
        : IJsonsTemplateProvider
    {
        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            return
            [
                new JsonsTemplateFile
                {
                    Name = "First",
                    TargetFileName = Path.Combine(
                        "Nested",
                        "item.json"),
                    Content = "{\"source\":1}"
                },
                new JsonsTemplateFile
                {
                    Name = "Second",
                    TargetFileName = Path.Combine(
                        "Nested",
                        ".",
                        "item.json"),
                    Content = "{\"source\":2}"
                }
            ];
        }
    }
}
