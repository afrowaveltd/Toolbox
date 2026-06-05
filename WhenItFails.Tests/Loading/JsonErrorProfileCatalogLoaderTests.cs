using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonErrorProfileCatalogLoaderTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDocumentLoaderIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new JsonErrorProfileCatalogLoader(null!));
    }

    [Fact]
    public async Task LoadFromFileAsync_ShouldLoadProfileCatalogDocument()
    {
        JsonErrorProfileCatalogLoader loader = new();

        string filePath = CreateTemporaryFile("""
        {
          "schemaVersion": "1.0",
          "catalogId": "test.profiles",
          "catalogName": "Test Profiles",
          "language": "en",
          "profiles": [
            {
              "name": "WEB_API",
              "displayName": "Web API",
              "description": "Profile for web APIs.",
              "includeOwners": [ "AFW", "APP" ],
              "includeCodeGroups": [ "CONFIGURATION", "VALIDATION" ],
              "includeCategories": [ "WEB", "SERVER", "VALIDATION" ],
              "includeSubcategories": [ "REQUIRED_VALUE" ],
              "includeTags": [ "USER_VISIBLE" ],
              "excludeTags": [ "INTERNAL_ONLY" ],
              "defaultMappings": {
                "web.problemDetails": "true"
              }
            }
          ]
        }
        """);

        try
        {
            Response<ErrorProfileCatalogDocument> response =
                await loader.LoadFromFileAsync(filePath);

            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Data);
            Assert.Equal("test.profiles", response.Data.CatalogId);

            ErrorProfileDefinition profile =
                Assert.Single(response.Data.Profiles);

            Assert.Equal("WEB_API", profile.Name);
            Assert.Equal("Web API", profile.DisplayName);
            Assert.Equal(["AFW", "APP"], profile.IncludeOwners);
            Assert.Equal(["CONFIGURATION", "VALIDATION"], profile.IncludeCodeGroups);
            Assert.Equal(["WEB", "SERVER", "VALIDATION"], profile.IncludeCategories);
            Assert.Equal(["REQUIRED_VALUE"], profile.IncludeSubcategories);
            Assert.Equal(["USER_VISIBLE"], profile.IncludeTags);
            Assert.Equal(["INTERNAL_ONLY"], profile.ExcludeTags);
            Assert.Equal("true", profile.DefaultMappings["web.problemDetails"]);
        }
        finally
        {
            DeleteTemporaryFile(filePath);
        }
    }

    private static string CreateTemporaryFile(string content)
    {
        string filePath = Path.Combine(
            Path.GetTempPath(),
            $"when-it-fails-profile-loader-test-{Guid.NewGuid():N}.json");

        File.WriteAllText(filePath, content);

        return filePath;
    }

    private static void DeleteTemporaryFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}