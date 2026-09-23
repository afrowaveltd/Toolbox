using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonCatalogDocumentLoaderFileParentContractTests
{
    [Fact]
    public async Task LoadFromFileAsync_WhenFilePathParentIsExistingFile_ReturnsInvalidBeforeFileOpen()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string blockingFilePath = Path.Combine(
            rootDirectory,
            "blocked");

        string catalogFilePath = Path.Combine(
            blockingFilePath,
            "catalog.json");

        Directory.CreateDirectory(rootDirectory);
        File.WriteAllText(blockingFilePath, "keep-this-file");

        try
        {
            JsonCatalogDocumentLoader loader = new();

            Response<ErrorCategoryCatalogDocument> response =
                await loader.LoadFromFileAsync<ErrorCategoryCatalogDocument>(
                    catalogFilePath);

            Assert.Equal(ResultStatus.Invalid, response.Status);
            Assert.False(response.IsSuccess);
            Assert.Null(response.Data);

            var issue = Assert.Single(response.Issues);
            Assert.Equal("FilePathParentIsFile", issue.Code);
            Assert.Equal(
                "JSON catalog file path has an existing file as a parent.",
                response.Message);
            Assert.Equal(
                "JSON catalog file path has an existing file as a parent.",
                issue.Message);

            Assert.True(File.Exists(blockingFilePath));
            Assert.Equal("keep-this-file", File.ReadAllText(blockingFilePath));
            Assert.False(File.Exists(catalogFilePath));
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
