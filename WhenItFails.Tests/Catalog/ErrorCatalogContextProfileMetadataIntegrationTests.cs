using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProfileMetadataIntegrationTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldPreserveProfileMetadataAndMappingsInContext()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            $"when-it-fails-profile-context-test-{Guid.NewGuid():N}");

        try
        {
            JsonsOptions options = await BootstrapWorkspaceAsync(rootDirectory);
            await AddProfileMetadataAsync(options.ProfilesFilePath);
            await AssertPersistedProfileMetadataAsync(options.ProfilesFilePath);

            Response<ErrorCatalogContext> response =
                await CreateContextProvider().LoadFromJsonsAsync(options);

            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Data);

            ErrorProfileDefinition profile = Assert.Single(
                response.Data.ProfileCatalog.Profiles,
                candidate => candidate.Name == "WEB");

            Assert.Equal("true", profile.DefaultMappings["WEB_PROBLEMDETAILS"]);
            AssertProfileMetadata(profile);
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    private static async Task<JsonsOptions> BootstrapWorkspaceAsync(string rootDirectory)
    {
        JsonsOptions options = new()
        {
            RootDirectory = rootDirectory,
            PackageDirectoryName = "WhenItFails"
        };

        Response<JsonsBootstrapPayload> response = await new JsonsBootstrapper(
            new DefaultJsonsTemplateProvider()).EnsureWorkspaceAsync(options);

        Assert.True(response.IsSuccess);
        return options;
    }

    private static async Task AddProfileMetadataAsync(string profileCatalogFilePath)
    {
        Response<ErrorProfileCatalogDocument> loadResponse =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(profileCatalogFilePath);

        Assert.True(loadResponse.IsSuccess);
        Assert.NotNull(loadResponse.Data);

        ErrorProfileDefinition webProfile = Assert.Single(
            loadResponse.Data.Profiles,
            profile => string.Equals(
                profile.Name,
                "WEB",
                StringComparison.OrdinalIgnoreCase));

        webProfile.Metadata = new MetadataBag(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["consumer"] = "SeeMe",
                ["auditNote"] = "preserved through context"
            });

        Response saveResponse = await new JsonCatalogDocumentWriter().SaveToFileAsync(
            loadResponse.Data,
            profileCatalogFilePath);

        Assert.True(saveResponse.IsSuccess);
    }

    private static async Task AssertPersistedProfileMetadataAsync(string profileCatalogFilePath)
    {
        Response<ErrorProfileCatalogDocument> response =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(profileCatalogFilePath);

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorProfileDefinition profile = Assert.Single(
            response.Data.Profiles,
            candidate => string.Equals(
                candidate.Name,
                "WEB",
                StringComparison.OrdinalIgnoreCase));

        AssertProfileMetadata(profile);
    }

    private static void AssertProfileMetadata(ErrorProfileDefinition profile)
    {
        Assert.Equal(2, profile.Metadata.Count);
        Assert.Equal(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["consumer"] = "SeeMe",
                ["auditNote"] = "preserved through context"
            },
            profile.Metadata.Items);
        Assert.True(profile.Metadata.TryGet("consumer", out string? consumer));
        Assert.Equal("SeeMe", consumer);
        Assert.True(profile.Metadata.TryGet("AUDIT_NOTE", out string? auditNote));
        Assert.Equal("preserved through context", auditNote);
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
}
