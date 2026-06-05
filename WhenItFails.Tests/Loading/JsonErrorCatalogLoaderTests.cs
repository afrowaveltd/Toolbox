using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonErrorCatalogLoaderTests
{
   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenDocumentLoaderIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new JsonErrorCatalogLoader(null!));
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnInvalidResponse_WhenFilePathIsEmpty()
   {
      JsonErrorCatalogLoader loader = new();

      Response<ErrorCatalogDocument> response =
          await loader.LoadFromFileAsync(string.Empty);

      Assert.False(response.IsSuccess);
      Assert.Equal(ResultStatus.Invalid, response.Status);
      Assert.NotEmpty(response.Issues);
      Assert.Equal("FilePathIsEmpty", response.Issues[0].Code);
      Assert.Null(response.Data);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnNotFoundResponse_WhenFileDoesNotExist()
   {
      JsonErrorCatalogLoader loader = new();

      string filePath = Path.Combine(
          Path.GetTempPath(),
          Guid.NewGuid().ToString("N"),
          "missing-catalog.json");

      Response<ErrorCatalogDocument> response =
          await loader.LoadFromFileAsync(filePath);

      Assert.False(response.IsSuccess);
      Assert.Equal(ResultStatus.NotFound, response.Status);
      Assert.NotEmpty(response.Issues);
      Assert.Equal("FileNotFound", response.Issues[0].Code);
      Assert.Null(response.Data);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnInvalidResponse_WhenJsonIsInvalid()
   {
      JsonErrorCatalogLoader loader = new();

      string filePath = CreateTemporaryFile("{ invalid json");

      try
      {
         Response<ErrorCatalogDocument> response =
             await loader.LoadFromFileAsync(filePath);

         Assert.False(response.IsSuccess);
         Assert.Equal(ResultStatus.Invalid, response.Status);
         Assert.NotEmpty(response.Issues);
         Assert.Equal("InvalidJson", response.Issues[0].Code);
         Assert.Null(response.Data);
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
              "id": "AFW-CFG-0001",
              "code": 200001,
              "name": "MissingConfigurationValue",
              "owner": "AFW",
              "codePrefix": "CFG",
              "codeGroup": "Configuration",
              "primaryCategory": "Configuration",
              "categories": [ "Configuration", "Startup", "Validation" ],
              "subcategories": [ "RequiredValue", "AppSettings" ],
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
         Response<ErrorCatalogDocument> response =
             await loader.LoadFromFileAsync(filePath);

         Assert.True(response.IsSuccess);
         Assert.Equal(ResultStatus.Success, response.Status);
         Assert.NotNull(response.Data);
         Assert.Equal("test.catalog", response.Data.CatalogId);
         Assert.Single(response.Data.Errors);
         Assert.Equal("AFW-CFG-0001", response.Data.Errors[0].Id);
         Assert.Equal("MissingConfigurationValue", response.Data.Errors[0].Name);
         Assert.Equal("AFW", response.Data.Errors[0].Owner);
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
              "id": "AFW-CFG-0001",
              "code": 200001,
              "name": "MissingConfigurationValue",
              "owner": "AFW",
              "codePrefix": "CFG",
              "codeGroup": "Configuration",
              "primaryCategory": "Configuration",
              "categories": [ "Configuration", "Startup" ],
              "subcategories": [ "RequiredValue" ],
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
         Response<ErrorCatalogDocument> response =
             await loader.LoadFromFileAsync(filePath);

         Assert.True(response.IsSuccess);
         Assert.Equal(ResultStatus.Success, response.Status);
         Assert.NotNull(response.Data);
         Assert.Single(response.Data.Errors);
         Assert.Equal("AFW-CFG-0001", response.Data.Errors[0].Id);
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
         await Assert.ThrowsAnyAsync<OperationCanceledException>(
             () => loader.LoadFromFileAsync(
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
          $"when-it-fails-error-loader-test-{Guid.NewGuid():N}.json");

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