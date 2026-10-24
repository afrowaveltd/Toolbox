using System.Text;
using Afrowave.Toolbox.Blackboard.Showcase;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Showcase.Tests;

public sealed class TextAndJsonPreviewTests
{
    [Fact]
    public async Task Utf8_text_with_czech_characters_can_be_previewed()
    {
        await WithFileAsync(Encoding.UTF8.GetBytes("Příliš žluťoučký kůň\nHello"), async path =>
        {
            var result = await TextFilePreviewReader.ReadAsync(path);
            Assert.Equal(TextPreviewKind.Text, result.Kind);
            Assert.Contains("žluťoučký", result.Content);
            Assert.Equal("UTF-8", result.EncodingName);
            Assert.False(result.Truncated);
        });
    }

    [Fact]
    public async Task Utf16_bom_is_supported()
    {
        var bytes = Encoding.Unicode.GetPreamble().Concat(
            Encoding.Unicode.GetBytes("Překlad: ahoj")).ToArray();
        await WithFileAsync(bytes, async path =>
        {
            var result = await TextFilePreviewReader.ReadAsync(path);
            Assert.Equal(TextPreviewKind.Text, result.Kind);
            Assert.Equal("Překlad: ahoj", result.Content);
            Assert.Equal("UTF-16 LE", result.EncodingName);
        });
    }

    [Fact]
    public async Task Binary_nul_bytes_do_not_reach_the_terminal()
    {
        await WithFileAsync([0x01, 0x00, 0x02, 0x03], async path =>
        {
            var result = await TextFilePreviewReader.ReadAsync(path);
            Assert.Equal(TextPreviewKind.BinaryOrUnsupportedEncoding, result.Kind);
            Assert.Empty(result.Content);
        });
    }

    [Fact]
    public async Task Escape_sequences_are_not_printable_text()
    {
        await WithFileAsync(Encoding.UTF8.GetBytes("hello\u001B[31mRED"), async path =>
        {
            var result = await TextFilePreviewReader.ReadAsync(path);
            Assert.Equal(TextPreviewKind.BinaryOrUnsupportedEncoding, result.Kind);
            Assert.Empty(result.Content);
        });
    }

    [Fact]
    public async Task Invalid_utf8_does_not_create_replacement_glyphs()
    {
        await WithFileAsync([0xC3, 0x28], async path =>
        {
            var result = await TextFilePreviewReader.ReadAsync(path);
            Assert.Equal(TextPreviewKind.BinaryOrUnsupportedEncoding, result.Kind);
            Assert.Empty(result.Content);
        });
    }

    [Fact]
    public async Task Preview_reads_only_a_bounded_prefix()
    {
        await WithFileAsync(Encoding.UTF8.GetBytes(new string('a', 128 * 1024)), async path =>
        {
            var result = await TextFilePreviewReader.ReadAsync(path, 1024);
            Assert.Equal(TextPreviewKind.Text, result.Kind);
            Assert.True(result.Truncated);
            Assert.Equal(1024, result.Content.Length);
            Assert.Equal(128 * 1024, result.TotalBytes);
        });
    }

    [Fact]
    public async Task Empty_file_does_not_produce_fake_content()
    {
        await WithFileAsync([], async path =>
        {
            var result = await TextFilePreviewReader.ReadAsync(path);
            Assert.Equal(TextPreviewKind.Empty, result.Kind);
            Assert.Empty(result.Content);
        });
    }

    [Fact]
    public async Task Complete_valid_json_is_prepared_for_syntax_highlighting()
    {
        await WithFileAsync(Encoding.UTF8.GetBytes(
            """{"language":"cs","translation":"Překlad","count":2,"active":true}"""),
            async path =>
            {
                var result = await JsonFilePreviewReader.ReadAsync(path);
                Assert.Equal(JsonPreviewKind.Valid, result.Kind);
                Assert.Contains("\"language\"", result.Json);
                Assert.Contains("Překlad", result.Json);
            });
    }

    [Fact]
    public async Task Malformed_json_is_rejected_without_rendering()
    {
        await WithFileAsync(Encoding.UTF8.GetBytes("""{"broken":}"""), async path =>
        {
            var result = await JsonFilePreviewReader.ReadAsync(path);
            Assert.Equal(JsonPreviewKind.InvalidJson, result.Kind);
            Assert.Empty(result.Json);
            Assert.Contains("Invalid JSON", result.Details);
        });
    }

    [Fact]
    public async Task Oversized_json_is_not_partially_parsed_or_rendered()
    {
        var json = """{"content":"""" + new string('x', 70 * 1024) + "\"}";
        await WithFileAsync(Encoding.UTF8.GetBytes(json), async path =>
        {
            var result = await JsonFilePreviewReader.ReadAsync(path);
            Assert.Equal(JsonPreviewKind.TooLarge, result.Kind);
            Assert.Empty(result.Json);
        });
    }

    [Fact]
    public async Task JSON_with_unescaped_terminal_control_char_is_rejected()
    {
        await WithFileAsync(Encoding.UTF8.GetBytes("{\"text\":\"hello\u001B[31m\"}"), async path =>
        {
            var result = await JsonFilePreviewReader.ReadAsync(path);
            Assert.Equal(JsonPreviewKind.BinaryOrUnsupportedEncoding, result.Kind);
            Assert.Empty(result.Json);
        });
    }

    [Fact]
    public async Task Binary_file_disguised_as_json_is_rejected()
    {
        await WithFileAsync([0x00, 0x01, 0x02], async path =>
        {
            var result = await JsonFilePreviewReader.ReadAsync(path);
            Assert.Equal(JsonPreviewKind.BinaryOrUnsupportedEncoding, result.Kind);
        });
    }

    [Fact]
    public async Task Empty_json_file_is_invalid_not_a_valid_empty_object()
    {
        await WithFileAsync([], async path =>
        {
            var result = await JsonFilePreviewReader.ReadAsync(path);
            Assert.Equal(JsonPreviewKind.InvalidJson, result.Kind);
        });
    }

    private static async Task WithFileAsync(byte[] content, Func<string, Task> assertion)
    {
        var path = Path.Combine(Path.GetTempPath(), "AfrowaveShowcase-" + Guid.NewGuid().ToString("N"));
        try
        {
            await File.WriteAllBytesAsync(path, content);
            await assertion(path);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
