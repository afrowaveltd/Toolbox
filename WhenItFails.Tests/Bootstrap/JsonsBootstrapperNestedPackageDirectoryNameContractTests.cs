using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperNestedPackageDirectoryNameContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenPackageDirectoryNameIsNestedInsideRoot_CreatesWorkspace()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string packageDirectoryName = Path.Combine("Packages", "WhenItFails");
        string packageDirectoryPath = Path.Combine(rootDirectory, packageDirectoryName);
        TrackingTemplateProvider templateProvider = new();

        try
        {
            JsonsBootstrapper bootstrapper = new(templateProvider);

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(
                    new JsonsOptions
                    {
                        RootDirectory = rootDirectory,
                        PackageDirectoryName = packageDirectoryName
                    });

            Assert.Equal(ResultStatus.Success, response.Status);
            Assert.True(response.IsSuccess);

            JsonsBootstrapPayload payload = Assert.IsType<JsonsBootstrapPayload>(
                response.Data);
            Assert.Equal(rootDirectory, payload.RootDirectory);
            Assert.Equal(packageDirectoryPath, payload.PackageDirectoryPath);
            Assert.False(payload.PackageDirectoryAlreadyExisted);
            Assert.True(payload.PackageDirectoryCreated);
            Assert.Empty(payload.Files);

            Assert.True(templateProvider.WasCalled);
            Assert.True(Directory.Exists(packageDirectoryPath));
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
