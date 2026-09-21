using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperInvalidRootDirectoryPathContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenRootDirectoryContainsNullCharacter_ReturnsInvalidBeforeProviderOrFilesystem()
    {
        string temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string invalidRootDirectory = temporaryDirectory + "\0invalid";
        TrackingTemplateProvider templateProvider = new();

        try
        {
            JsonsBootstrapper bootstrapper = new(templateProvider);

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(
                    new JsonsOptions
                    {
                        RootDirectory = invalidRootDirectory,
                        PackageDirectoryName = "WhenItFails"
                    });

            Assert.Equal(ResultStatus.Invalid, response.Status);
            Assert.False(response.IsSuccess);
            Assert.Null(response.Data);

            var issue = Assert.Single(response.Issues);
            Assert.Equal(
                "WIF_JSONS_ROOT_DIRECTORY_INVALID",
                issue.Code);
            Assert.Equal(
                "The JSON root directory path is invalid.",
                response.Message);
            Assert.Equal(
                "The JSON root directory path is invalid.",
                issue.Message);

            Assert.False(templateProvider.WasCalled);
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
