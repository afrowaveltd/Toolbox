using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperWhitespaceTemplateNameContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenTemplateNameIsWhitespace_ReturnsInvalidBeforeWritingTemplateFile()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string targetFilePath = Path.Combine(
            rootDirectory,
            "WhenItFails",
            "errors.en.json");

        try
        {
            JsonsBootstrapper bootstrapper = new(
                new WhitespaceNameTemplateProvider());

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
                "WIF_JSONS_TEMPLATE_NAME_EMPTY",
                issue.Code);
            Assert.Equal(
                "The JSON template provider returned a template with an empty name.",
                response.Message);
            Assert.Equal(
                "The JSON template provider returned a template with an empty name.",
                issue.Message);

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

    private sealed class WhitespaceNameTemplateProvider
        : IJsonsTemplateProvider
    {
        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            return
            [
                new JsonsTemplateFile
                {
                    Name = "   ",
                    TargetFileName = "errors.en.json",
                    Content = "{}"
                }
            ];
        }
    }
}
