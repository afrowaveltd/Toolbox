using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonErrorCatalogLoaderTests
{
   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnFailure_WhenFilePathIsEmpty()
   {
      JsonErrorCatalogLoader loader = new();

      ErrorCatalogLoadResult result = await loader.LoadFromFileAsync(string.Empty);

      Assert.False(result.Success);
      Assert.Equal("FilePathIsEmpty", result.ErrorCode);
      Assert.Null(result.Document);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnFailure_WhenFileDoesNotExist()
   {
      JsonErrorCatalogLoader loader = new();

      string filePath = Path.Combine(
          Path.GetTempPath(),
          Guid.NewGuid().ToString("N"),
          "missing-catalog.json");

      ErrorCatalogLoadResult result = await loader.LoadFromFileAsync(filePath);

      Assert.False(result.Success);
      Assert.Equal("FileNotFound", result.ErrorCode);
      Assert.Null(result.Document);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnFailure_WhenJsonIsInvalid()
   {
      JsonErrorCatalogLoader loader = new();

      string filePath = CreateTemporaryFile("{ invalid json");

      try
      {
         ErrorCatalogLoadResult result = await loader.LoadFromFileAsync(filePath);

         Assert.False(result.Success);
         Assert.Equal("InvalidJson", result.ErrorCode);
         Assert.Null(result.Document);
         Assert.NotNull(result.Exception);
      }
      finally
      {
         DeleteTemporaryFile(filePath);
      }
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldLoadValidCatalogDocument()
   {
      JsonErrorCatalogLoader loader = new();

      string json = """
        {
          "schemaVersion": "1.0",
          "catalogId": "test.catalog",
          "catalogName": "Test Catalog",
          "language": "en",
          "errors": [
            {
              "id": "CFG-0001",
              "code": 1001,
              "name": "MissingConfigurationValue",
              "category": "Configuration",
              "categoryPrefix": "CFG",
              "title": "Missing configuration value",
              "message": "A required configuration value is missing.",
              "defaultSeverity": "Error",
              "tags": [ "configuration", "startup" ]
            }
          ]
        }
        """;

      string filePath = CreateTemporaryFile(json);

      try
      {
         ErrorCatalogLoadResult result = await loader.LoadFromFileAsync(filePath);

         Assert.True(result.Success);
         Assert.NotNull(result.Document);
         Assert.Equal("test.catalog", result.Document.CatalogId);
         Assert.Single(result.Document.Errors);
         Assert.Equal("CFG-0001", result.Document.Errors[0].Id);
      }
      finally
      {
         DeleteTemporaryFile(filePath);
      }
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldSupportCommentsAndTrailingCommas()
   {
      JsonErrorCatalogLoader loader = new();

      string json = """
        {
          // catalog header
          "schemaVersion": "1.0",
          "catalogId": "test.catalog",
          "catalogName": "Test Catalog",
          "language": "en",
          "errors": [
            {
              "id": "CFG-0001",
              "code": 1001,
              "name": "MissingConfigurationValue",
              "category": "Configuration",
              "categoryPrefix": "CFG",
              "title": "Missing configuration value",
              "message": "A required configuration value is missing.",
              "defaultSeverity": "Error",
            },
          ],
        }
        """;

      string filePath = CreateTemporaryFile(json);

      try
      {
         ErrorCatalogLoadResult result = await loader.LoadFromFileAsync(filePath);

         Assert.True(result.Success);
         Assert.NotNull(result.Document);
         Assert.Single(result.Document.Errors);
      }
      finally
      {
         DeleteTemporaryFile(filePath);
      }
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldThrowOperationCanceledException_WhenCancelled()
   {
      JsonErrorCatalogLoader loader = new();

      string json = """
        {
          "schemaVersion": "1.0",
          "catalogId": "test.catalog",
          "catalogName": "Test Catalog",
          "language": "en",
          "errors": []
        }
        """;

      string filePath = CreateTemporaryFile(json);

      using CancellationTokenSource cancellationTokenSource = new();
      await cancellationTokenSource.CancelAsync();

      try
      {
         await Assert.ThrowsAsync<OperationCanceledException>(
             () => loader.LoadFromFileAsync(filePath, cancellationTokenSource.Token));
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