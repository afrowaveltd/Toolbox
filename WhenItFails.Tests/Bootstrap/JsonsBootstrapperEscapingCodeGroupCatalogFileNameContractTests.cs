using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperEscapingCodeGroupCatalogFileNameContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameEscapesPackage_ReturnsInvalidBeforeProviderOrFilesystem()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string escapedFilePath = Path.Combine(rootDirectory, "escaped.json");
        TrackingTemplateProvider templateProvider = new();

        try
        {
            JsonsBootstrapper bootstrapper = new(templateProvider);

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(
                    new JsonsOptions
                    {
                        RootDirectory = rootDirectory,
                        PackageDirectoryName = "WhenItFails",
                        CodeGroupCatalogFileName = "../escaped.json"
                    });

            Assert.Equal(ResultStatus.Invalid, response.Status);
            Assert.False(response.IsSuccess);
            Assert.Null(response.Data);

            var issue = Assert.Single(response.Issues);
            Assert.Equal(
                "WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_OUTSIDE_PACKAGE",
                issue.Code);
            Assert.Equal(
                "The code group catalog file name must stay inside the package directory.",
                response.Message);
            Assert.Equal(
                "The code group catalog file name must stay inside the package directory.",
                issue.Message);

            Assert.False(templateProvider.WasCalled);
            Assert.False(Directory.Exists(rootDirectory));
            Assert.False(File.Exists(escapedFilePath));
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
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
