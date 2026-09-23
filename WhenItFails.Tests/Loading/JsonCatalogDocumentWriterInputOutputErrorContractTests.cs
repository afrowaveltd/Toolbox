using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonCatalogDocumentWriterInputOutputErrorContractTests
{
    [Fact]
    public async Task SaveToFileAsync_WhenExistingTargetIsLocked_ReturnsInputOutputErrorAndPreservesTarget()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string targetFilePath = Path.Combine(
            rootDirectory,
            "errors.en.json");

        byte[] originalBytes = "{\"catalogId\":\"original\"}"u8.ToArray();

        Directory.CreateDirectory(rootDirectory);
        await File.WriteAllBytesAsync(
            targetFilePath,
            originalBytes);

        try
        {
            Response response;

            await using (FileStream lockStream = new(
                targetFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None))
            {
                JsonCatalogDocumentWriter writer = new();

                response = await writer.SaveToFileAsync(
                    new ErrorCatalogDocument
                    {
                        SchemaVersion = "1.0",
                        CatalogId = "test.catalog",
                        CatalogName = "Replacement catalog",
                        Language = "en",
                        Errors = []
                    },
                    targetFilePath);

                Assert.False(response.IsSuccess);
                Assert.Equal(ResultStatus.Failed, response.Status);

                var issue = Assert.Single(response.Issues);
                Assert.Equal("InputOutputError", issue.Code);
            }

            Assert.Equal(
                originalBytes,
                await File.ReadAllBytesAsync(targetFilePath));

            Assert.Empty(
                Directory.GetFiles(
                    rootDirectory,
                    "*.bak.json"));

            Assert.Empty(
                Directory.GetFiles(
                    rootDirectory,
                    ".errors.en.json.*.tmp"));
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
