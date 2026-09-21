using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperEscapingPackageDirectoryNameContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenPackageDirectoryNameEscapesRoot_ReturnsInvalidBeforeProviderOrFilesystem()
    {
        string temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string rootDirectory = Path.Combine(temporaryDirectory, "Jsons");
        string escapedDirectory = Path.Combine(temporaryDirectory, "escaped");
        TrackingTemplateProvider templateProvider = new();

        try
        {
            JsonsBootstrapper bootstrapper = new(templateProvider);

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(
                    new JsonsOptions
                    {
                        RootDirectory = rootDirectory,
                        PackageDirectoryName = "../escaped"
                    });

            Assert.Equal(ResultStatus.Invalid, response.Status);
            Assert.False(response.IsSuccess);
            Assert.Null(response.Data);

            var issue = Assert.Single(response.Issues);
            Assert.Equal(
                "WIF_JSONS_PACKAGE_DIRECTORY_NAME_OUTSIDE_ROOT",
                issue.Code);
            Assert.Equal(
                "The package directory name must stay inside the JSON root directory.",
                response.Message);
            Assert.Equal(
                "The package directory name must stay inside the JSON root directory.",
                issue.Message);

            Assert.False(templateProvider.WasCalled);
            Assert.False(Directory.Exists(rootDirectory));
            Assert.False(Directory.Exists(escapedDirectory));
            Assert.False(Directory.Exists(temporaryDirectory));
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }
    }

    private sealed class TrackingTemplateProvider : IJsonsTemplateProvider
    {
        public bool WasCalled { get; private set; }

        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            WasCalled = true;
            return [];
        }
    }
}
