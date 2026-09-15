using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonCatalogDocumentWriterTemporaryFileCleanupContractTests
{
    [Fact]
    public async Task SaveToFileAsync_WhenSerializationFails_DoesNotLeaveTemporaryFile()
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
            SelfReferencingDocument document = new();
            document.Self = document;

            JsonCatalogDocumentWriter writer = new();

            Response response =
                await writer.SaveToFileAsync(
                    document,
                    targetFilePath);

            Assert.NotNull(response);
            Assert.False(response.IsSuccess);
            Assert.Equal(ResultStatus.Invalid, response.Status);
            Assert.False(File.Exists(targetFilePath));

            Assert.Collection(
                response.Issues,
                issue => Assert.Equal(
                    "JsonSerializationFailed",
                    issue.Code));

            string[] temporaryFiles = Directory.GetFiles(
                directoryPath,
                ".errors.en.json.*.tmp");

            Assert.Empty(temporaryFiles);
        }
        finally
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, recursive: true);
            }
        }
    }

    private sealed class SelfReferencingDocument
    {
        public SelfReferencingDocument? Self { get; set; }
    }
}
