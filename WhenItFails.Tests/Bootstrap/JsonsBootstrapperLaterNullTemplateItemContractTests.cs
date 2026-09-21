using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperLaterNullTemplateItemContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenLaterTemplateItemIsNull_ReturnsInvalidBeforeWritingAnyTemplateFiles()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string firstTargetFilePath = Path.Combine(
            rootDirectory,
            "WhenItFails",
            "first.json");

        try
        {
            JsonsBootstrapper bootstrapper = new(
                new LaterNullItemTemplateProvider());

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(
                    new JsonsOptions
                    {
                        RootDirectory = rootDirectory,
                        PackageDirectoryName = "WhenItFails"
                    });

            Assert.False(response.IsSuccess);
            Assert.Equal(ResultStatus.Invalid, response.Status);
            Assert.Null(response.Data);

            var issue = Assert.Single(response.Issues);
            Assert.Equal(
                "WIF_JSONS_TEMPLATE_ITEM_NULL",
                issue.Code);
            Assert.Equal(
                "The JSON template provider returned a null template item.",
                response.Message);
            Assert.Equal(
                "The JSON template provider returned a null template item.",
                issue.Message);

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

    private sealed class LaterNullItemTemplateProvider
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
                null!
            ];
        }
    }
}
