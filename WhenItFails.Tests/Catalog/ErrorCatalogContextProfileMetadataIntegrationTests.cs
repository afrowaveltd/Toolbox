using System.Text.Json;
using System.Text.Json.Nodes;
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

            Response<ErrorCatalogContext> response =
                await CreateContextProvider().LoadFromJsonsAsync(options);

            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Data);

            ErrorProfileDefinition profile = Assert.Single(
                response.Data.ProfileCatalog.Profiles,
                candidate => candidate.Name == "WEB");

            Assert.Equal("true", profile.DefaultMappings["WEB_PROBLEMDETAILS"]);
            Assert.Equal(2, profile.Metadata.Count);
            Assert.True(profile.Metadata.TryGet("consumer", out string? consumer));
            Assert.Equal("SeeMe", consumer);
            Assert.True(profile.Metadata.TryGet("AUDIT_NOTE", out string? auditNote));
            Assert.Equal("preserved through context", auditNote);
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
        JsonNode root = JsonNode.Parse(await File.ReadAllTextAsync(profileCatalogFilePath))
            ?? throw new InvalidOperationException("Profile catalog JSON could not be parsed.");
        JsonArray profiles = root["profiles"]?.AsArray()
            ?? throw new InvalidOperationException("Profile catalog does not contain profiles.");
        JsonObject webProfile = profiles
            .Select(node => node?.AsObject())
            .First(profile => string.Equals(
                profile?["name"]?.GetValue<string>(),
                "WEB",
                StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("WEB profile was not found.");

        webProfile["metadata"] = new JsonObject
        {
            ["consumer"] = "SeeMe",
            ["auditNote"] = "preserved through context"
        };

        await File.WriteAllTextAsync(
            profileCatalogFilePath,
            root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
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
