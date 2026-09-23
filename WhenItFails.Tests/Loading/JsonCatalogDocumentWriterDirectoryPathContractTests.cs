using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonCatalogDocumentWriterDirectoryPathContractTests
{
    [Fact]
    public async Task SaveToFileAsync_WhenTargetPathIsExistingDirectory_ReturnsInvalidWithoutTemporaryFiles()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string targetDirectoryPath = Path.Combine(
            rootDirectory,
            "catalog.json");

        Directory.CreateDirectory(targetDirectoryPath);

        try
        {
            JsonCatalogDocumentWriter writer = new();

            Response response = await writer.SaveToFileAsync(
                new ErrorCatalogDocument
                {
                    SchemaVersion = "1.0",
                    CatalogId = "test.catalog",
                    CatalogName = "Test catalog",
                    Language = "en",
                    Errors = []
                },
                targetDirectoryPath);

            Assert.Equal(ResultStatus.Invalid, response.Status);
            Assert.False(response.IsSuccess);

            var issue = Assert.Single(response.Issues);
            Assert.Equal("FilePathIsDirectory", issue.Code);
            Assert.Equal(
                "JSON catalog file path points to a directory.",
                response.Message);
            Assert.Equal(
                "JSON catalog file path points to a directory.",
                issue.Message);

            Assert.True(Directory.Exists(targetDirectoryPath));
            Assert.Empty(Directory.GetFileSystemEntries(targetDirectoryPath));
            Assert.Empty(Directory.GetFiles(rootDirectory, "*.tmp"));
            Assert.Empty(Directory.GetFiles(rootDirectory, "*.bak*"));
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }
}
