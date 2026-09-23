using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonCatalogDocumentLoaderNullJsonDocumentContractTests
{
    [Fact]
    public async Task LoadFromFileAsync_WhenJsonDocumentIsNull_ReturnsEmptyCatalogDocumentInsteadOfInvalidJson()
    {
        string filePath = Path.Combine(
            Path.GetTempPath(),
            $"when-it-fails-null-document-{Guid.NewGuid():N}.json");

        File.WriteAllText(filePath, "null");

        try
        {
            JsonCatalogDocumentLoader loader = new();

            Response<ErrorCategoryCatalogDocument> response =
                await loader.LoadFromFileAsync<ErrorCategoryCatalogDocument>(
                    filePath);

            Assert.Equal(ResultStatus.Invalid, response.Status);
            Assert.False(response.IsSuccess);
            Assert.Null(response.Data);

            var issue = Assert.Single(response.Issues);
            Assert.Equal("EmptyCatalogDocument", issue.Code);
            Assert.Equal(
                "JSON catalog file was loaded, but the catalog document is empty.",
                response.Message);
            Assert.Equal(
                "JSON catalog file was loaded, but the catalog document is empty.",
                issue.Message);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
