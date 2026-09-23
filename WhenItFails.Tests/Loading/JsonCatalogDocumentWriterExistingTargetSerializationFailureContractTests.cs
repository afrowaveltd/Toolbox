using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonCatalogDocumentWriterExistingTargetSerializationFailureContractTests
{
    [Fact]
    public async Task SaveToFileAsync_WhenSerializationFailsForExistingTarget_PreservesOriginalWithoutBackupOrTemporaryFile()
    {
        string directoryPath = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string targetFilePath = Path.Combine(
            directoryPath,
            "errors.en.json");

        Directory.CreateDirectory(directoryPath);

        try
        {
            byte[] originalContent =
                "{\"catalogId\":\"original\",\"catalogName\":\"Keep this catalog\"}"u8.ToArray();

            await File.WriteAllBytesAsync(targetFilePath, originalContent);

            SelfReferencingDocument document = new();
            document.Self = document;

            JsonCatalogDocumentWriter writer = new();

            Response response =
                await writer.SaveToFileAsync(document, targetFilePath);

            Assert.Equal(ResultStatus.Invalid, response.Status);
            Assert.False(response.IsSuccess);

            var issue = Assert.Single(response.Issues);
            Assert.Equal("JsonSerializationFailed", issue.Code);

            Assert.True(File.Exists(targetFilePath));
            Assert.Equal(
                originalContent,
                await File.ReadAllBytesAsync(targetFilePath));

            string[] remainingFiles = Directory.GetFiles(directoryPath);
            Assert.Single(remainingFiles);
            Assert.Equal(targetFilePath, remainingFiles[0]);

            Assert.Empty(Directory.GetFiles(
                directoryPath,
                ".errors.en.json.*.tmp"));
            Assert.Empty(Directory.GetFiles(
                directoryPath,
                "*.bak*"));
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
