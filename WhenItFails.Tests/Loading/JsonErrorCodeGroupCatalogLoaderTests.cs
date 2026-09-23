using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonErrorCodeGroupCatalogLoaderTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDocumentLoaderIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new JsonErrorCodeGroupCatalogLoader(null!));
    }

    [Fact]
    public async Task LoadFromFileAsync_ShouldLoadCodeGroupCatalogDocument()
    {
        JsonErrorCodeGroupCatalogLoader loader = new();

        string filePath = CreateTemporaryFile("""
        {
          "schemaVersion": "1.0",
          "catalogId": "test.code-groups",
          "catalogName": "Test Code Groups",
          "language": "en",
          "codeGroups": [
            {
              "name": "CONFIGURATION",
              "displayName": "Configuration",
              "codePrefix": "CFG",
              "codeFrom": 200000,
              "codeTo": 299999,
              "defaultCategories": [ "CONFIGURATION" ],
              "defaultTags": [ "SYSTEM" ],
              "defaultMappings": {
                "http.status": "500"
              }
            }
          ]
        }
        """);

        try
        {
            Response<ErrorCodeGroupCatalogDocument> response =
                await loader.LoadFromFileAsync(filePath);

            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Data);
            Assert.Equal("test.code-groups", response.Data.CatalogId);

            ErrorCodeGroupDefinition codeGroup =
                Assert.Single(response.Data.CodeGroups);

            Assert.Equal("CONFIGURATION", codeGroup.Name);
            Assert.Equal("Configuration", codeGroup.DisplayName);
            Assert.Equal("CFG", codeGroup.CodePrefix);
            Assert.Equal(200000, codeGroup.CodeFrom);
            Assert.Equal(299999, codeGroup.CodeTo);
            Assert.Equal(["CONFIGURATION"], codeGroup.DefaultCategories);
            Assert.Equal(["SYSTEM"], codeGroup.DefaultTags);
            Assert.Equal("500", codeGroup.DefaultMappings["http.status"]);
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
            $"when-it-fails-code-group-loader-test-{Guid.NewGuid():N}.json");

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
