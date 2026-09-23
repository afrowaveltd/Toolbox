using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonErrorOwnerCatalogLoaderTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDocumentLoaderIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new JsonErrorOwnerCatalogLoader(null!));
    }

    [Fact]
    public async Task LoadFromFileAsync_ShouldLoadOwnerCatalogDocument()
    {
        JsonErrorOwnerCatalogLoader loader = new();

        string filePath = CreateTemporaryFile("""
        {
          "schemaVersion": "1.0",
          "catalogId": "test.owners",
          "catalogName": "Test Owners",
          "language": "en",
          "owners": [
            {
              "name": "AFW",
              "displayName": "Afrowave",
              "description": "Built-in Afrowave owner.",
              "codeFrom": 0,
              "codeTo": 999999,
              "isBuiltIn": true,
              "aliases": [ "AFROWAVE" ],
              "defaultMappings": {
                "support.owner": "platform"
              }
            }
          ]
        }
        """);

        try
        {
            Response<ErrorOwnerCatalogDocument> response =
                await loader.LoadFromFileAsync(filePath);

            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Data);
            Assert.Equal("test.owners", response.Data.CatalogId);

            ErrorOwnerDefinition owner =
                Assert.Single(response.Data.Owners);

            Assert.Equal("AFW", owner.Name);
            Assert.Equal("Afrowave", owner.DisplayName);
            Assert.Equal("Built-in Afrowave owner.", owner.Description);
            Assert.Equal(0, owner.CodeFrom);
            Assert.Equal(999999, owner.CodeTo);
            Assert.True(owner.IsBuiltIn);
            Assert.Equal(["AFROWAVE"], owner.Aliases);
            Assert.Equal("platform", owner.DefaultMappings["support.owner"]);
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
            $"when-it-fails-owner-loader-test-{Guid.NewGuid():N}.json");

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
