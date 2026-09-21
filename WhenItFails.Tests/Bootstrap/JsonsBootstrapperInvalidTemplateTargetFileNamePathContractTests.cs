using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperInvalidTemplateTargetFileNamePathContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenTemplateTargetFileNameContainsNullCharacter_ReturnsInvalidBeforeWritingTemplateFiles()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string packageDirectoryPath = Path.Combine(
            rootDirectory,
            "WhenItFails");

        string sentinelFilePath = Path.Combine(
            packageDirectoryPath,
            "should-not-be-created.json");

        try
        {
            JsonsBootstrapper bootstrapper = new(
                new InvalidTargetFileNameTemplateProvider());

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

            Assert.False(File.Exists(sentinelFilePath));
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    private sealed class InvalidTargetFileNameTemplateProvider
        : IJsonsTemplateProvider
    {
        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            return
            [
                new JsonsTemplateFile
                {
                    Name = "Invalid",
                    TargetFileName = "errors\0.en.json",
                    Content = "{}"
                },
                new JsonsTemplateFile
                {
                    Name = "Sentinel",
                    TargetFileName = "should-not-be-created.json",
                    Content = "{}"
                }
            ];
        }
    }
}
