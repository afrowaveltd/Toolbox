using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperCurrentDirectoryTemplateTargetContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenLaterTemplateTargetResolvesToPackageDirectory_ReturnsInvalidWithoutPartialWrites()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string packageDirectoryPath = Path.Combine(
            rootDirectory,
            "WhenItFails");

        string firstTargetFilePath = Path.Combine(
            packageDirectoryPath,
            "first.json");

        try
        {
            JsonsBootstrapper bootstrapper = new(
                new CurrentDirectoryTargetTemplateProvider());

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

            Assert.True(Directory.Exists(packageDirectoryPath));
            Assert.False(File.Exists(firstTargetFilePath));
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    private sealed class CurrentDirectoryTargetTemplateProvider
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
                    TargetFileName = "first.json",
                    Content = "{}"
                },
                new JsonsTemplateFile
                {
                    Name = "Package directory",
                    TargetFileName = ".",
                    Content = "{}"
                }
            ];
        }
    }
}
