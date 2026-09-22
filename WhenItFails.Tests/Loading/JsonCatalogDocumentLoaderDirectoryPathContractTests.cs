using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonCatalogDocumentLoaderDirectoryPathContractTests
{
    [Fact]
    public async Task LoadFromFileAsync_WhenFilePathPointsToDirectory_ReturnsInvalidDirectoryPath()
    {
        string directoryPath = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directoryPath);

        try
        {
            JsonCatalogDocumentLoader loader = new();

            Response<ErrorCategoryCatalogDocument> response =
                await loader.LoadFromFileAsync<ErrorCategoryCatalogDocument>(
                    directoryPath);

            Assert.Equal(ResultStatus.Invalid, response.Status);
            Assert.False(response.IsSuccess);
            Assert.Null(response.Data);

            var issue = Assert.Single(response.Issues);
            Assert.Equal("FilePathIsDirectory", issue.Code);
            Assert.Equal(
                "JSON catalog file path points to a directory.",
                response.Message);
            Assert.Equal(
                "JSON catalog file path points to a directory.",
                issue.Message);
        }
        finally
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, recursive: true);
            }
        }
    }
}
