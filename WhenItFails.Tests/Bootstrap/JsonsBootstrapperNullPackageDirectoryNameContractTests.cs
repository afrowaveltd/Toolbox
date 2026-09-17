using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperNullPackageDirectoryNameContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenPackageDirectoryNameIsNull_ReturnsInvalidWithoutCreatingWorkspace()
    {
        string rootDirectory = Path.Combine(
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
                        RootDirectory = rootDirectory,
                        PackageDirectoryName = null!
                    });

            Assert.Equal(ResultStatus.Invalid, response.Status);
            Assert.False(response.IsSuccess);
            Assert.Null(response.Data);

            var issue = Assert.Single(response.Issues);
            Assert.Equal(
                "WIF_JSONS_PACKAGE_DIRECTORY_NAME_NULL",
                issue.Code);
            Assert.Equal(
                "The package directory name cannot be null.",
                response.Message);
            Assert.Equal(
                "The package directory name cannot be null.",
                issue.Message);

            Assert.False(Directory.Exists(rootDirectory));
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
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
