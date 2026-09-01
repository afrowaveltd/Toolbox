using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonCatalogDocumentWriterInvalidFilePathContractTests
{
    [Fact]
    public async Task SaveToFileAsync_ShouldReturnInvalid_WhenFilePathContainsNullCharacter()
    {
        ErrorCatalogDocument document = new()
        {
            SchemaVersion = "1.0",
            CatalogId = "test.catalog",
            CatalogName = "Test catalog",
            Language = "en",
            Errors = []
        };

        JsonCatalogDocumentWriter writer = new();

        Response response = await writer.SaveToFileAsync(
            document,
            "invalid\0path/errors.en.json");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("FilePathIsInvalid", issue.Code);
        Assert.Equal(
            "JSON catalog file path is invalid.",
            response.Message);
    }
}
