using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Resolution;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Resolution;

public sealed class ErrorProfileSelectionServiceIntegrationTests
{
    [Fact]
    public async Task ResolveByProfileName_ShouldResolveWebProfile_FromDefaultBootstrappedJsons()
    {
        string rootDirectory = CreateTemporaryRootDirectory();

        try
        {
            JsonsOptions options =
                await BootstrapWorkspaceAsync(rootDirectory);

            ErrorCatalogContextProvider contextProvider =
                CreateContextProvider();

            Response<ErrorCatalogContext> contextResponse =
                await contextProvider.LoadFromJsonsAsync(options);

            Assert.True(contextResponse.IsSuccess);
            Assert.NotNull(contextResponse.Data);

            ErrorProfileSelectionService selectionService = new(
                new ErrorProfileResolver());

            Response<IReadOnlyList<ErrorDefinition>> selectionResponse =
                selectionService.ResolveByProfileName(
                    contextResponse.Data,
                    "web");

            Assert.True(selectionResponse.IsSuccess);
            Assert.NotNull(selectionResponse.Data);
            Assert.NotEmpty(selectionResponse.Data);

            Assert.Contains(
                selectionResponse.Data,
                error => error.Id == "AFW_NET_0001");

            Assert.DoesNotContain(
                selectionResponse.Data,
                error => error.Id == "AFW_DB_0002");

            Assert.All(
                selectionResponse.Data,
                error => Assert.DoesNotContain(
                    error.Tags,
                    tag => string.Equals(
                        tag,
                        "INTERNAL_ONLY",
                        StringComparison.OrdinalIgnoreCase)));
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    private static ErrorCatalogContextProvider CreateContextProvider()
    {
        return new ErrorCatalogContextProvider(
            new ErrorCatalogProvider(
                new JsonErrorCatalogLoader(),
                new ErrorCatalogDocumentNormalizer(),
                new ErrorCatalogValidator(),
                new ErrorCatalogFactory()),
            new ErrorCategoryCatalogProvider(
                new JsonErrorCategoryCatalogLoader(),
                new ErrorCategoryCatalogDocumentNormalizer(),
                new ErrorCategoryCatalogValidator()),
            new ErrorCodeGroupCatalogProvider(
                new JsonErrorCodeGroupCatalogLoader(),
                new ErrorCodeGroupCatalogDocumentNormalizer(),
                new ErrorCodeGroupCatalogValidator()),
            new ErrorOwnerCatalogProvider(
                new JsonErrorOwnerCatalogLoader(),
                new ErrorOwnerCatalogDocumentNormalizer(),
                new ErrorOwnerCatalogValidator()),
            new ErrorProfileCatalogProvider(
                new JsonErrorProfileCatalogLoader(),
                new ErrorProfileCatalogDocumentNormalizer(),
                new ErrorProfileCatalogValidator()));
    }

    private static async Task<JsonsOptions> BootstrapWorkspaceAsync(
        string rootDirectory)
    {
        JsonsOptions options = new()
        {
            RootDirectory = rootDirectory,
            PackageDirectoryName = "WhenItFails"
        };

        JsonsBootstrapper bootstrapper = new(
            new DefaultJsonsTemplateProvider());

        Response<JsonsBootstrapPayload> bootstrapResponse =
            await bootstrapper.EnsureWorkspaceAsync(options);

        Assert.True(bootstrapResponse.IsSuccess);

        return options;
    }

    private static string CreateTemporaryRootDirectory()
    {
        return Path.Combine(
            Path.GetTempPath(),
            $"when-it-fails-profile-selection-integration-{Guid.NewGuid():N}");
    }

    private static void DeleteDirectoryIfExists(
        string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(
                directoryPath,
                recursive: true);
        }
    }
}