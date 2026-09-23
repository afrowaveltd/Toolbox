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
    public async Task SaveToFileAsync_WhenTargetDoesNotExist_LeavesOnlyTargetWithoutBackupOrTemporaryFile()
    {
        string temporaryDirectoryPath = CreateTemporaryDirectoryPath();
        string targetFilePath = Path.Combine(
            temporaryDirectoryPath,
            "errors.en.json");

        try
        {
            JsonCatalogDocumentWriter writer = new();

            Essentials.Results.Response response =
                await writer.SaveToFileAsync(
                    CreateDocument("First catalog"),
                    targetFilePath);

            Assert.True(response.IsSuccess);
            Assert.True(File.Exists(targetFilePath));

            Assert.Empty(
                Directory.GetFiles(
                    temporaryDirectoryPath,
                    ".errors.en.json.*.tmp"));

            Assert.Empty(
                Directory.GetFiles(
                    temporaryDirectoryPath,
                    "*.bak.json"));

            string[] files = Directory.GetFiles(temporaryDirectoryPath);
            Assert.Single(files);
            Assert.Equal(targetFilePath, files[0]);
        }
        finally
        {
            DeleteDirectoryIfExists(temporaryDirectoryPath);
        }
    }

    [Fact]
    public async Task SaveToFileAsync_WhenTargetDoesNotExist_ResponseMessageContainsOnlyTargetPath()
    {
        string temporaryDirectoryPath = CreateTemporaryDirectoryPath();
        string targetFilePath = Path.Combine(
            temporaryDirectoryPath,
            "errors.en.json");

        try
        {
            JsonCatalogDocumentWriter writer = new();

            Essentials.Results.Response response =
                await writer.SaveToFileAsync(
                    CreateDocument("First catalog"),
                    targetFilePath);

            Assert.True(response.IsSuccess);
            Assert.Equal(
                $"JSON catalog file was saved: {targetFilePath}",
                response.Message);
            Assert.DoesNotContain(
                "Backup:",
                response.Message,
                StringComparison.Ordinal);
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
    public async Task SaveToFileAsync_WhenTargetAlreadyExists_ResponseMessageIncludesActualBackupPath()
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
                "{\"catalogId\":\"original\"}");

            JsonCatalogDocumentWriter writer = new();

            Essentials.Results.Response response =
                await writer.SaveToFileAsync(
                    CreateDocument("Replacement catalog"),
                    targetFilePath);

            Assert.True(response.IsSuccess);

            string backupFilePath = Assert.Single(
                Directory.GetFiles(
                    temporaryDirectoryPath,
                    "*.bak.json"));

            Assert.Equal(
                $"JSON catalog file was saved: {targetFilePath}. Backup: {backupFilePath}",
                response.Message);
        }
        finally
        {
            DeleteDirectoryIfExists(temporaryDirectoryPath);
        }
    }

    [Fact]
    public async Task SaveToFileAsync_WhenTargetAlreadyExists_BackupPreservesOriginalBytesExactly()
    {
        string temporaryDirectoryPath = CreateTemporaryDirectoryPath();
        string targetFilePath = Path.Combine(
            temporaryDirectoryPath,
            "errors.en.json");

        byte[] originalBytes =
        [
            0xEF, 0xBB, 0xBF,
            0x7B, 0x0D, 0x0A,
            0x20, 0x20, 0x22, 0x63, 0x61, 0x74, 0x61, 0x6C, 0x6F, 0x67, 0x49, 0x64, 0x22, 0x3A, 0x20,
            0x22, 0x6F, 0x72, 0x69, 0x67, 0x69, 0x6E, 0x61, 0x6C, 0x22,
            0x0D, 0x0A, 0x7D
        ];

        try
        {
            Directory.CreateDirectory(temporaryDirectoryPath);
            await File.WriteAllBytesAsync(
                targetFilePath,
                originalBytes);

            JsonCatalogDocumentWriter writer = new();

            Essentials.Results.Response response =
                await writer.SaveToFileAsync(
                    CreateDocument("Replacement catalog"),
                    targetFilePath);

            Assert.True(response.IsSuccess);

            string[] backupFilePaths = Directory.GetFiles(
                temporaryDirectoryPath,
                "*.bak.json");

            string backupFilePath = Assert.Single(backupFilePaths);

            Assert.Equal(
                originalBytes,
                await File.ReadAllBytesAsync(backupFilePath));

            Assert.NotEqual(
                originalBytes,
                await File.ReadAllBytesAsync(targetFilePath));
        }
        finally
        {
            DeleteDirectoryIfExists(temporaryDirectoryPath);
        }
    }

    [Fact]
    public async Task SaveToFileAsync_WhenExistingTargetIsReplaced_DoesNotLeaveTemporaryFile()
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
                "{\"catalogId\":\"original\"}");

            JsonCatalogDocumentWriter writer = new();

            Essentials.Results.Response response =
                await writer.SaveToFileAsync(
                    CreateDocument("Replacement catalog"),
                    targetFilePath);

            Assert.True(response.IsSuccess);
            Assert.True(File.Exists(targetFilePath));

            Assert.Empty(
                Directory.GetFiles(
                    temporaryDirectoryPath,
                    ".errors.en.json.*.tmp"));

            Assert.Single(
                Directory.GetFiles(
                    temporaryDirectoryPath,
                    "*.bak.json"));

            Assert.Equal(
                2,
                Directory.GetFiles(temporaryDirectoryPath).Length);
        }
        finally
        {
            DeleteDirectoryIfExists(temporaryDirectoryPath);
        }
    }

    [Fact]
    public async Task SaveToFileAsync_ShouldCreateDistinctBackups_ForRapidConsecutiveWrites()
    {
        string temporaryDirectoryPath = CreateTemporaryDirectoryPath();
        string targetFilePath = Path.Combine(
            temporaryDirectoryPath,
            "errors.en.json");

        try
        {
            Directory.CreateDirectory(temporaryDirectoryPath);

            JsonCatalogDocumentWriter writer = new();

            Essentials.Results.Response firstResponse =
                await writer.SaveToFileAsync(
                    CreateDocument("First catalog"),
                    targetFilePath);

            Essentials.Results.Response secondResponse =
                await writer.SaveToFileAsync(
                    CreateDocument("Second catalog"),
                    targetFilePath);

            Essentials.Results.Response thirdResponse =
                await writer.SaveToFileAsync(
                    CreateDocument("Third catalog"),
                    targetFilePath);

            Assert.True(firstResponse.IsSuccess);
            Assert.True(secondResponse.IsSuccess);
            Assert.True(thirdResponse.IsSuccess);

            string[] backupFilePaths = Directory.GetFiles(
                temporaryDirectoryPath,
                "*.bak.json");

            Assert.Equal(2, backupFilePaths.Length);
            Assert.Equal(2, backupFilePaths.Distinct(StringComparer.Ordinal).Count());
        }
        finally
        {
            DeleteDirectoryIfExists(temporaryDirectoryPath);
        }
    }

    [Fact]
    public async Task SaveToFileAsync_WhenFilePathHasSurroundingWhitespace_WritesToTrimmedPath()
    {
        string temporaryDirectoryPath = CreateTemporaryDirectoryPath();
        string targetFilePath = Path.Combine(
            temporaryDirectoryPath,
            "errors.en.json");

        string paddedTargetFilePath = $"  {targetFilePath}  ";

        try
        {
            JsonCatalogDocumentWriter writer = new();

            Essentials.Results.Response response =
                await writer.SaveToFileAsync(
                    CreateDocument("Trimmed path catalog"),
                    paddedTargetFilePath);

            Assert.True(response.IsSuccess);
            Assert.True(File.Exists(targetFilePath));
            Assert.False(File.Exists(paddedTargetFilePath));

            string fileText = await File.ReadAllTextAsync(targetFilePath);
            Assert.Contains(
                "\"catalogName\": \"Trimmed path catalog\"",
                fileText);
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

    [Fact]
    public async Task SaveToFileAsync_WhenFilePathIsWhitespace_ReturnsInvalid()
    {
        ErrorCatalogDocument document = CreateDocument("Test catalog");

        JsonCatalogDocumentWriter writer = new();

        Essentials.Results.Response response =
            await writer.SaveToFileAsync(
                document,
                " 	 ");

        Assert.False(response.IsSuccess);
        Assert.Equal("FilePathIsEmpty", response.Issues[0].Code);
        Assert.Equal(
            "JSON catalog file path is empty.",
            response.Message);
    }

    [Fact]
    public async Task SaveToFileAsync_WhenFilePathIsNull_ReturnsInvalid()
    {
        ErrorCatalogDocument document = CreateDocument("Test catalog");

        JsonCatalogDocumentWriter writer = new();

        Essentials.Results.Response response =
            await writer.SaveToFileAsync(
                document,
                null!);

        Assert.False(response.IsSuccess);
        Assert.Equal("FilePathIsEmpty", response.Issues[0].Code);
        Assert.Equal(
            "JSON catalog file path is empty.",
            response.Message);
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
