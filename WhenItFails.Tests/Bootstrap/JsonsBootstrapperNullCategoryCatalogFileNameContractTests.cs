using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperNullCategoryCatalogFileNameContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenCategoryCatalogFileNameIsNull_ReturnsInvalidBeforeProviderOrFilesystem()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

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
                        CategoryCatalogFileName = null!
                    });

            Assert.Equal(ResultStatus.Invalid, response.Status);
            Assert.False(response.IsSuccess);
            Assert.Null(response.Data);

            var issue = Assert.Single(response.Issues);
            Assert.Equal(
                "WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_NULL",
                issue.Code);
            Assert.Equal(
                "The category catalog file name cannot be null.",
                response.Message);
            Assert.Equal(
                "The category catalog file name cannot be null.",
                issue.Message);

            Assert.False(templateProvider.WasCalled);
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
