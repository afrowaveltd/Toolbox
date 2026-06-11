using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonCatalogDocumentWriterTests
{
    [Fact]
    public async Task SaveToFileAsync_ShouldCreateJsonFile_WhenTargetDoesNotExist()
    {
        string temporaryDirectoryPath = CreateTemporaryDirectoryPath();
        string targetFilePath = Path.Combine(
           temporaryDirectoryPath,
           "errors.en.json");

        try
        {
            ErrorCatalogDocument document = CreateDocument("Test catalog");

            JsonCatalogDocumentWriter writer = new();

            Essentials.Results.Response response =
               await writer.SaveToFileAsync(
                  document,
                  targetFilePath);

            Assert.True(response.IsSuccess);
            Assert.True(File.Exists(targetFilePath));

            string fileText = await File.ReadAllTextAsync(targetFilePath);

            Assert.Contains("\"catalogName\": \"Test catalog\"", fileText);
            Assert.Contains("\"errors\":", fileText);
        }
        finally
        {
            DeleteDirectoryIfExists(temporaryDirectoryPath);
        }
    }

    [Fact]
    public async Task SaveToFileAsync_ShouldCreateBackup_WhenTargetAlreadyExists()
    {
        string temporaryDirectoryPath = CreateTemporaryDirectoryPath();
        string targetFilePath = Path.Combine(
           temporaryDirectoryPath,
           "errors.en.json");

        try
        {
            Directory.CreateDirectory(temporaryDirectoryPath);

            await File.WriteAllTextAsync(
               targetFilePath,
               """
            {
              "schemaVersion": "1.0",
              "catalogId": "old",
              "catalogName": "Old catalog",
              "language": "en",
              "errors": []
            }
            """);

            ErrorCatalogDocument document = CreateDocument("New catalog");

            JsonCatalogDocumentWriter writer = new();

            Essentials.Results.Response response =
               await writer.SaveToFileAsync(
                  document,
                  targetFilePath);

            Assert.True(response.IsSuccess);

            string fileText = await File.ReadAllTextAsync(targetFilePath);

            Assert.Contains("\"catalogName\": \"New catalog\"", fileText);

            string[] backupFilePaths = Directory.GetFiles(
               temporaryDirectoryPath,
               "*.bak.json");

            Assert.Single(backupFilePaths);

            string backupFileText = await File.ReadAllTextAsync(backupFilePaths[0]);

            Assert.Contains("\"catalogName\": \"Old catalog\"", backupFileText);
        }
        finally
        {
            DeleteDirectoryIfExists(temporaryDirectoryPath);
        }
    }

    [Fact]
    public async Task SaveToFileAsync_ShouldReturnInvalid_WhenFilePathIsEmpty()
    {
        ErrorCatalogDocument document = CreateDocument("Test catalog");

        JsonCatalogDocumentWriter writer = new();

        Essentials.Results.Response response =
           await writer.SaveToFileAsync(
              document,
              string.Empty);

        Assert.False(response.IsSuccess);
        Assert.Equal("FilePathIsEmpty", response.Issues[0].Code);
    }

    private static ErrorCatalogDocument CreateDocument(string catalogName)
    {
        return new ErrorCatalogDocument
        {
            SchemaVersion = "1.0",
            CatalogId = "test.catalog",
            CatalogName = catalogName,
            Language = "en",
            Errors =
           [
              new ErrorDefinition
            {
               Id = "AFW_TEST_0001",
               Code = 100001,
               Name = "TESTERROR",
               Owner = "AFW",
               CodePrefix = "TEST",
               CodeGroup = "GENERAL",
               PrimaryCategory = "GENERAL",
               Categories = ["GENERAL"],
               Title = "Test error",
               Message = "This is a test error.",
               DefaultSeverity = "Error"
            }
           ]
        };
    }

    private static string CreateTemporaryDirectoryPath()
    {
        return Path.Combine(
           Path.GetTempPath(),
           "afrowave-when-it-fails-writer-tests",
           Guid.NewGuid().ToString("N"));
    }

    private static void DeleteDirectoryIfExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        Directory.Delete(
           directoryPath,
           recursive: true);
    }
}