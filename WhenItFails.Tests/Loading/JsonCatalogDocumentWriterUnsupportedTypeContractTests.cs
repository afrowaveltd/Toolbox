using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonCatalogDocumentWriterUnsupportedTypeContractTests
{
    [Fact]
    public async Task SaveToFileAsync_WhenDocumentContainsUnsupportedType_ReturnsSerializationFailureWithoutTemporaryFile()
    {
        string directoryPath = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string targetFilePath = Path.Combine(
            directoryPath,
            "errors.en.json");

        try
        {
            JsonCatalogDocumentWriter writer = new();

            Response response = await writer.SaveToFileAsync(
                new UnsupportedTypeDocument
                {
                    Unsupported = typeof(string)
                },
                targetFilePath);

            Assert.Equal(ResultStatus.Invalid, response.Status);
            Assert.False(response.IsSuccess);

            var issue = Assert.Single(response.Issues);
            Assert.Equal("JsonSerializationFailed", issue.Code);
            Assert.StartsWith(
                "JSON catalog document serialization failed.",
                response.Message);
            Assert.StartsWith(
                "JSON catalog document serialization failed.",
                issue.Message);

            Assert.False(File.Exists(targetFilePath));
            Assert.True(Directory.Exists(directoryPath));
            Assert.Empty(Directory.GetFileSystemEntries(directoryPath));
        }
        finally
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, recursive: true);
            }
        }
    }

    private sealed class UnsupportedTypeDocument
    {
        public Type Unsupported { get; init; } = typeof(string);
    }
}
