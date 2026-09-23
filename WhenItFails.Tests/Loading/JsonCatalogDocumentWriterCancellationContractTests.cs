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

    [Fact]
    public async Task SaveToFileAsync_WhenCancellationOccursDuringSerialization_ThrowsAndPreservesExistingFileWithoutTemporaryOrBackupFile()
    {
        string directoryPath = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string targetFilePath = Path.Combine(
            directoryPath,
            "errors.en.json");

        byte[] originalBytes =
        [
            0x7B,
            0x0A,
            0x20,
            0x20,
            0x22,
            0x6F,
            0x72,
            0x69,
            0x67,
            0x69,
            0x6E,
            0x61,
            0x6C,
            0x22,
            0x3A,
            0x20,
            0x74,
            0x72,
            0x75,
            0x65,
            0x0A,
            0x7D
        ];

        Directory.CreateDirectory(directoryPath);
        await File.WriteAllBytesAsync(
            targetFilePath,
            originalBytes);

        using CancellationTokenSource cancellationSource = new();

        try
        {
            JsonCatalogDocumentWriter writer = new();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => writer.SaveToFileAsync(
                    new CancelDuringSerializationDocument(cancellationSource),
                    targetFilePath,
                    cancellationSource.Token));

            Assert.True(File.Exists(targetFilePath));
            Assert.Equal(
                originalBytes,
                await File.ReadAllBytesAsync(targetFilePath));

            string[] files = Directory.GetFiles(directoryPath);

            Assert.Single(files);
            Assert.Equal(
                targetFilePath,
                files[0]);
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

    private sealed class CancelDuringSerializationDocument
    {
        private readonly CancellationTokenSource _cancellationSource;

        public CancelDuringSerializationDocument(
            CancellationTokenSource cancellationSource)
        {
            _cancellationSource = cancellationSource;
        }

        public string Payload
        {
            get
            {
                _cancellationSource.Cancel();
                return new string(
                    'x',
                    1024 * 1024);
            }
        }
    }
}
