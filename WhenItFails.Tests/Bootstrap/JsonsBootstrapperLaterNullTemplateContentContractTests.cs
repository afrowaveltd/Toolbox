using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperLaterNullTemplateContentContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenLaterTemplateContentIsNull_ReturnsInvalidWithoutPartialWrites()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string packageDirectoryPath = Path.Combine(
            rootDirectory,
            "WhenItFails");

        string firstTargetPath = Path.Combine(
            packageDirectoryPath,
            "first.json");

        try
        {
            JsonsBootstrapper bootstrapper = new(
                new LaterNullContentTemplateProvider());

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
                "WIF_JSONS_TEMPLATE_CONTENT_NULL",
                issue.Code);
            Assert.Equal(
                "The JSON template provider returned a template with null content.",
                response.Message);
            Assert.Equal(
                "The JSON template provider returned a template with null content.",
                issue.Message);

            Assert.True(Directory.Exists(packageDirectoryPath));
            Assert.False(File.Exists(firstTargetPath));
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    private sealed class LaterNullContentTemplateProvider
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
                    Name = "Second",
                    TargetFileName = "second.json",
                    Content = null!
                }
            ];
        }
    }
}
