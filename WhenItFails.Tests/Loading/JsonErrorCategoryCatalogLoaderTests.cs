using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonErrorCategoryCatalogLoaderTests
{
   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenDocumentLoaderIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new JsonErrorCategoryCatalogLoader(null!));
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldLoadCategoryCatalogDocument()
   {
      JsonErrorCategoryCatalogLoader loader = new();

      string filePath = CreateTemporaryFile("""
        {
          "schemaVersion": "1.0",
          "catalogId": "test.categories",
          "catalogName": "Test Categories",
          "language": "en",
          "categories": [
            {
              "name": "NETWORK",
              "displayName": "Network"
            }
          ]
        }
        """);

      try
      {
         Response<ErrorCategoryCatalogDocument> response =
             await loader.LoadFromFileAsync(filePath);

         Assert.True(response.IsSuccess);
         Assert.NotNull(response.Data);
         Assert.Equal("test.categories", response.Data.CatalogId);
         Assert.Single(response.Data.Categories);
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
          $"when-it-fails-category-loader-test-{Guid.NewGuid():N}.json");

      File.WriteAllText(filePath, content);

      return filePath;
   }

   private static void DeleteTemporaryFile(string filePath)
   {
      if(File.Exists(filePath))
      {
         File.Delete(filePath);
      }
   }
}