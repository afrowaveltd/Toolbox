using System.Text;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.TextFiles.Tests;

public sealed class ReaderExtractionTests
{
    [Fact]
    public async Task CompleteReadPreservesUnicodeAndNewlines()
    {
        await WithFileAsync(Encoding.UTF8.GetBytes("# Nadpis\nPříliš žluťoučký kůň 🌍\n"),
            async path =>
            {
                var result = await TextFilePreviewReader.ReadCompleteAsync(path);
                Assert.Equal(TextPreviewKind.Text, result.Kind);
                Assert.Equal("# Nadpis\nPříliš žluťoučký kůň 🌍\n", result.Content);
                Assert.False(result.Truncated);
            });
    }

    [Fact]
    public async Task BoundedPreviewDoesNotReadEntireFile()
    {
        await WithFileAsync(Encoding.UTF8.GetBytes(new string('A', 64 * 1024)),
            async path =>
            {
                var result = await TextFilePreviewReader.ReadAsync(path, 1024);
                Assert.Equal(TextPreviewKind.Text, result.Kind);
                Assert.Equal(1024, result.Content.Length);
                Assert.True(result.Truncated);
            });
    }

    [Fact]
    public async Task CompleteReadRejectsUnsafeControlsBeyondPreviewBoundary()
    {
        var payload = Encoding.UTF8.GetBytes(new string('a', 32 * 1024) + "\u001b[31m");
        await WithFileAsync(payload, async path =>
        {
            var result = await TextFilePreviewReader.ReadCompleteAsync(path);
            Assert.Equal(TextPreviewKind.BinaryOrUnsupportedEncoding, result.Kind);
            Assert.Empty(result.Content);
        });
    }

    [Fact]
    public async Task CompleteReadDecodesUtf16Bom()
    {
        var bytes = Encoding.Unicode.GetPreamble()
            .Concat(Encoding.Unicode.GetBytes("Překlad")).ToArray();
        await WithFileAsync(bytes, async path =>
        {
            var result = await TextFilePreviewReader.ReadCompleteAsync(path);
            Assert.Equal(TextPreviewKind.Text, result.Kind);
            Assert.Equal("Překlad", result.Content);
        });
    }

    [Fact]
    public async Task InvalidUtf8IsRejected()
    {
        await WithFileAsync([0xC3, 0x28], async path =>
        {
            var result = await TextFilePreviewReader.ReadCompleteAsync(path);
            Assert.Equal(TextPreviewKind.BinaryOrUnsupportedEncoding, result.Kind);
        });
    }

    private static async Task WithFileAsync(byte[] data, Func<string, Task> assertion)
    {
        var path = Path.Combine(Path.GetTempPath(), "afrowave-textfiles-" + Guid.NewGuid().ToString("N"));
        try
        {
            await File.WriteAllBytesAsync(path, data);
            await assertion(path);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
