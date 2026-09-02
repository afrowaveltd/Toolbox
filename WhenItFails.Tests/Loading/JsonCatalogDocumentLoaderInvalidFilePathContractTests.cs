using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonCatalogDocumentLoaderInvalidFilePathContractTests
{
    [Fact]
    public async Task LoadFromFileAsync_ShouldReturnInvalid_WhenFilePathContainsNullCharacter()
    {
        JsonCatalogDocumentLoader loader = new();

        Response<ErrorCatalogDocument> response =
            await loader.LoadFromFileAsync<ErrorCatalogDocument>(
                "invalid\0errors.en.json");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("FilePathIsInvalid", issue.Code);
        Assert.Equal(
            "JSON catalog file path is invalid.",
            response.Message);
    }
}
