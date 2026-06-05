using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonCatalogDocumentLoaderTests
{
   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnInvalidResponse_WhenFilePathIsEmpty()
   {
      JsonCatalogDocumentLoader loader = new();

      Response<ErrorCategoryCatalogDocument> response =
          await loader.LoadFromFileAsync<ErrorCategoryCatalogDocument>(string.Empty);

      Assert.True(response.IsFailure);
      Assert.Equal("FilePathIsEmpty", response.Issues[0].Code);
      Assert.Null(response.Data);
   }


   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnNotFoundResponse_WhenFileDoesNotExist()
   {
      JsonCatalogDocumentLoader loader = new();

      string filePath = Path.Combine(
          Path.GetTempPath(),
          Guid.NewGuid().ToString("N"),
          "missing-catalog.json");

      Response<ErrorCategoryCatalogDocument> response =
          await loader.LoadFromFileAsync<ErrorCategoryCatalogDocument>(filePath);

      Assert.False(response.IsSuccess);
      Assert.Equal(ResultStatus.NotFound, response.Status);
      Assert.Equal("FileNotFound", response.Issues[0].Code);
      Assert.Null(response.Data);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnInvalidResponse_WhenJsonIsInvalid()
   {
      JsonCatalogDocumentLoader loader = new();

      string filePath = CreateTemporaryFile("{ invalid json");

      try
      {
         Response<ErrorCategoryCatalogDocument> response =
             await loader.LoadFromFileAsync<ErrorCategoryCatalogDocument>(filePath);

         Assert.True(response.IsFailure);
         Assert.Equal("InvalidJson", response.Issues[0].Code);
         Assert.Null(response.Data);
      }
      finally
      {
         DeleteTemporaryFile(filePath);
      }
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldLoadValidCategoryCatalogDocument()
   {
      JsonCatalogDocumentLoader loader = new();

      string json = """
        {
          "schemaVersion": "1.0",
          "catalogId": "test.categories",
          "catalogName": "Test Categories",
          "language": "en",
          "categories": [
            {
              "name": "NETWORK",
              "displayName": "Network",
              "description": "Errors related to network communication.",
              "aliases": [ "NETWORKING" ],
              "parentCategories": [ "EXTERNAL_COMMUNICATION" ],
              "defaultTags": [ "RETRYABLE_CANDIDATE" ]
            }
          ]
        }
        """;

      string filePath = CreateTemporaryFile(json);

      try
      {
         Response<ErrorCategoryCatalogDocument> response =
             await loader.LoadFromFileAsync<ErrorCategoryCatalogDocument>(filePath);

         Assert.True(response.IsSuccess);
         Assert.NotNull(response.Data);
         Assert.Equal("test.categories", response.Data.CatalogId);
         Assert.Single(response.Data.Categories);
         Assert.Equal("NETWORK", response.Data.Categories[0].Name);
      }
      finally
      {
         DeleteTemporaryFile(filePath);
      }
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldSupportCommentsAndTrailingCommas()
   {
      JsonCatalogDocumentLoader loader = new();

      string json = """
        {
          // catalog header
          "schemaVersion": "1.0",
          "catalogId": "test.categories",
          "catalogName": "Test Categories",
          "language": "en",
          "categories": [
            {
              "name": "NETWORK",
              "displayName": "Network",
            },
          ],
        }
        """;

      string filePath = CreateTemporaryFile(json);

      try
      {
         Response<ErrorCategoryCatalogDocument> response =
             await loader.LoadFromFileAsync<ErrorCategoryCatalogDocument>(filePath);

         Assert.True(response.IsSuccess);
         Assert.NotNull(response.Data);
         Assert.Single(response.Data.Categories);
      }
      finally
      {
         DeleteTemporaryFile(filePath);
      }
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldThrowOperationCanceledException_WhenCancelled()
   {
      JsonCatalogDocumentLoader loader = new();

      string json = """
        {
          "schemaVersion": "1.0",
          "catalogId": "test.categories",
          "catalogName": "Test Categories",
          "language": "en",
          "categories": []
        }
        """;

      string filePath = CreateTemporaryFile(json);

      using CancellationTokenSource cancellationTokenSource = new();
      await cancellationTokenSource.CancelAsync();

      try
      {
         await Assert.ThrowsAnyAsync<OperationCanceledException>(
             () => loader.LoadFromFileAsync<ErrorCategoryCatalogDocument>(
                 filePath,
                 cancellationTokenSource.Token));
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
          $"when-it-fails-test-{Guid.NewGuid():N}.json");

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