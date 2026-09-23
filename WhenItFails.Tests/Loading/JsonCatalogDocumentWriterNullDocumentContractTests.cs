using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.Loading;

public sealed class JsonCatalogDocumentWriterNullDocumentContractTests
{
    [Fact]
    public async Task SaveToFileAsync_WhenDocumentIsNull_ThrowsBeforeFilesystemSideEffects()
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

            ArgumentNullException exception =
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => writer.SaveToFileAsync<ErrorCatalogDocument>(
                        null!,
                        targetFilePath));

            Assert.Equal(
                "document",
                exception.ParamName);

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
}
