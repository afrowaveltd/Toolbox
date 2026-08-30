using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperNullTemplateTargetFileNameContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_ShouldReturnInvalid_WhenTemplateTargetFileNameIsNull()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        try
        {
            JsonsBootstrapper bootstrapper = new(
                new NullTargetFileNameTemplateProvider());

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
            Assert.Equal("WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_NULL", issue.Code);
            Assert.Equal(
                "The JSON template provider returned a template with a null target file name.",
                response.Message);
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    private sealed class NullTargetFileNameTemplateProvider : IJsonsTemplateProvider
    {
        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            return
            [
                new JsonsTemplateFile
                {
                    Name = "Errors",
                    TargetFileName = null!,
                    Content = "{}"
                }
            ];
        }
    }
}
