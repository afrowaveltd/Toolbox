using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperNullRootDirectoryContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenRootDirectoryIsNull_ReturnsInvalidWithoutCreatingWorkspace()
    {
        string packageDirectoryPath = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        try
        {
            JsonsBootstrapper bootstrapper = new(
                new EmptyTemplateProvider());

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(
                    new JsonsOptions
                    {
                        RootDirectory = null!,
                        PackageDirectoryName = packageDirectoryPath
                    });

            Assert.Equal(ResultStatus.Invalid, response.Status);
            Assert.False(response.IsSuccess);
            Assert.Null(response.Data);

            var issue = Assert.Single(response.Issues);
            Assert.Equal(
                "WIF_JSONS_ROOT_DIRECTORY_NULL",
                issue.Code);
            Assert.Equal(
                "The JSON root directory cannot be null.",
                response.Message);
            Assert.Equal(
                "The JSON root directory cannot be null.",
                issue.Message);

            Assert.False(Directory.Exists(packageDirectoryPath));
        }
        finally
        {
            if (Directory.Exists(packageDirectoryPath))
            {
                Directory.Delete(packageDirectoryPath, recursive: true);
            }
        }
    }

    private sealed class EmptyTemplateProvider : IJsonsTemplateProvider
    {
        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            return [];
        }
    }
}
