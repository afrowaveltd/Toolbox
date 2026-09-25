using System.Text.Json;
using System.Text.Json.Serialization;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonCatalogDocumentLoaderMidDeserializationCancellationContractTests
{
    private static readonly AsyncLocal<CancellationTokenSource?> CurrentCancellationSource = new();

    [Fact]
    public async Task LoadFromFileAsync_WhenCancelledDuringDeserialization_RethrowsCancellationAndReleasesFileHandle()
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

        byte[] originalContent =
            "{\"payload\":\"cancel-during-read\"}"u8.ToArray();

        await File.WriteAllBytesAsync(
            targetFilePath,
            originalContent);

        using CancellationTokenSource cancellationSource = new();

        CurrentCancellationSource.Value = cancellationSource;

        try
        {
            JsonCatalogDocumentLoader loader = new();

            OperationCanceledException exception =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    () => loader.LoadFromFileAsync<CancelDuringDeserializationDocument>(
                        targetFilePath,
                        cancellationSource.Token));

            Assert.True(cancellationSource.IsCancellationRequested);
            Assert.Equal(
                cancellationSource.Token,
                exception.CancellationToken);

            Assert.True(File.Exists(targetFilePath));
            Assert.Equal(
                originalContent,
                await File.ReadAllBytesAsync(targetFilePath));

            using FileStream exclusiveStream = new(
                targetFilePath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None);

            Assert.True(exclusiveStream.CanRead);
            Assert.True(exclusiveStream.CanWrite);

            string[] remainingFiles =
                Directory.GetFiles(directoryPath);

            Assert.Single(remainingFiles);
            Assert.Equal(
                targetFilePath,
                remainingFiles[0]);
        }
        finally
        {
            CurrentCancellationSource.Value = null;

            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(
                    directoryPath,
                    recursive: true);
            }
        }
    }

    private sealed class CancelDuringDeserializationDocument
    {
        [JsonConverter(typeof(CancelOnReadStringConverter))]
        public string? Payload { get; set; }
    }

    public sealed class CancelOnReadStringConverter
        : JsonConverter<string>
    {
        public override string? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            string? value = reader.GetString();

            CancellationTokenSource cancellationSource =
                CurrentCancellationSource.Value
                ?? throw new InvalidOperationException(
                    "The test cancellation source was not installed.");

            cancellationSource.Cancel();
            cancellationSource.Token.ThrowIfCancellationRequested();

            return value;
        }

        public override void Write(
            Utf8JsonWriter writer,
            string value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
