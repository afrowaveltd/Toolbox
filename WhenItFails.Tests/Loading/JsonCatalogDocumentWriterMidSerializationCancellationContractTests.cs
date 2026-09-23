using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonCatalogDocumentWriterMidSerializationCancellationContractTests
{
    [Fact]
    public async Task SaveToFileAsync_WhenCancelledDuringSerialization_PreservesExistingTargetWithoutBackupOrTemporaryFile()
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

        using CancellationTokenSource cancellationSource = new();

        try
        {
            byte[] originalContent =
                "{\"catalogId\":\"original\",\"catalogName\":\"Keep this catalog\"}"u8.ToArray();

            await File.WriteAllBytesAsync(targetFilePath, originalContent);

            JsonCatalogDocumentWriter writer = new();

            Assert.False(cancellationSource.IsCancellationRequested);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => writer.SaveToFileAsync(
                    new CancelDuringSerializationDocument(cancellationSource),
                    targetFilePath,
                    cancellationSource.Token));

            Assert.True(cancellationSource.IsCancellationRequested);
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
                _cancellationSource.Token.ThrowIfCancellationRequested();
                return "unreachable";
            }
        }
    }
}
