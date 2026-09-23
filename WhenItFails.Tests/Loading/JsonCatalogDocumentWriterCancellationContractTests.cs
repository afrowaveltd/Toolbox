using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonCatalogDocumentWriterCancellationContractTests
{
    [Fact]
    public async Task SaveToFileAsync_WhenTokenAlreadyCancelled_ThrowsWithoutFilesystemSideEffects()
    {
        string directoryPath = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string targetFilePath = Path.Combine(
            directoryPath,
            "errors.en.json");

        using CancellationTokenSource cancellationSource = new();
        cancellationSource.Cancel();

        try
        {
            JsonCatalogDocumentWriter writer = new();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => writer.SaveToFileAsync(
                    CreateDocument(),
                    targetFilePath,
                    cancellationSource.Token));

            Assert.False(Directory.Exists(directoryPath));
            Assert.False(File.Exists(targetFilePath));
        }
        finally
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, recursive: true);
            }
        }
    }

    private static ErrorCatalogDocument CreateDocument()
    {
        return new ErrorCatalogDocument
        {
            SchemaVersion = "1.0",
            CatalogId = "test.catalog",
            CatalogName = "Test catalog",
            Language = "en",
            Errors = []
        };
    }
}
