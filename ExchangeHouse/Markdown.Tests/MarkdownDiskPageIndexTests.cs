using System.Text;
using Xunit;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown.Tests;

public sealed class MarkdownDiskPageIndexTests
{
    [Fact]
    public async Task Seeks_backward_and_forward_with_exact_UTF8_content_and_byte_offsets()
    {
        const string source = "# Úvod\r\nŘádek 🐈\n\n~~~csharp\r\n" +
            "var x = 1;\r\n~~~\nZávěr";
        await WithFileAsync(Encoding.UTF8.GetBytes(source), async (path, temp) =>
        {
            await using var index = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 2 }, tempDirectory: temp);

            Assert.Equal(7, index.LineCount);
            Assert.Equal(4, index.PageCount);
            var last = await index.ReadPageAsync(3);
            var first = await index.ReadPageAsync(0);
            var middle = await index.ReadPageAsync(2);
            var second = await index.ReadPageAsync(1);

            Assert.Equal(7, last.FirstLine);
            Assert.Equal("Závěr", last.Content);
            Assert.Equal("# Úvod\r\nŘádek 🐈\n", first.Content);
            Assert.Equal("\n~~~csharp\r\n", second.Content);
            Assert.Equal("var x = 1;\r\n~~~\n", middle.Content);
            Assert.Equal(source, first.Content + second.Content + middle.Content + last.Content);
            Assert.Equal(0L, first.StartByte);
            Assert.Equal(Encoding.UTF8.GetByteCount(source), last.EndByte);
            Assert.Equal(first.EndByte, second.StartByte);
            Assert.Equal(second.EndByte, middle.StartByte);
        });
    }

    [Theory]
    [InlineData("", 0, 0)]
    [InlineData("first\n", 1, 1)]
    [InlineData("first\nsecond\n", 2, 1)]
    [InlineData("first\nsecond\nthird", 3, 2)]
    [InlineData("first\rsecond\r\nthird\n", 3, 2)]
    [InlineData("\r\n\n", 2, 1)]
    public async Task Counts_physical_lines_without_creating_an_empty_trailing_page(
        string source, long expectedLines, long expectedPages)
    {
        await WithFileAsync(Encoding.UTF8.GetBytes(source), async (path, temp) =>
        {
            await using var index = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 2 }, tempDirectory: temp);
            Assert.Equal(expectedLines, index.LineCount);
            Assert.Equal(expectedPages, index.PageCount);

            var output = new StringBuilder();
            for (long i = 0; i < index.PageCount; i++)
                output.Append((await index.ReadPageAsync(i)).Content);
            Assert.Equal(source, output.ToString());
        });
    }

    [Fact]
    public async Task Accepts_UTF8_BOM_only_as_a_document_prefix()
    {
        var bytes = new byte[] { 0xEF, 0xBB, 0xBF }
            .Concat(Encoding.UTF8.GetBytes("# nadpis\nline two\n")).ToArray();
        await WithFileAsync(bytes, async (path, temp) =>
        {
            await using var index = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 1 }, tempDirectory: temp);
            Assert.Equal(2, index.PageCount);
            var first = await index.ReadPageAsync(0);
            Assert.Equal("# nadpis\n", first.Content);
            Assert.Equal(0, first.StartByte);
            Assert.Equal(bytes.Length - Encoding.UTF8.GetByteCount("line two\n"),
                first.EndByte);
        });
    }

    [Theory]
    [InlineData(new byte[] { 0x61, 0xC3 })]
    [InlineData(new byte[] { 0xC3, 0x28 })]
    [InlineData(new byte[] { 0x61, 0x1B, 0x62 })]
    [InlineData(new byte[] { 0x61, 0x00, 0x62 })]
    [InlineData(new byte[] { 0x61, 0xC2, 0x85, 0x62 })]
    public async Task Rejects_bad_utf8_and_terminal_controls_without_leaving_an_index(byte[] bytes)
    {
        await WithFileAsync(bytes, async (path, temp) =>
        {
            var failure = await Record.ExceptionAsync(() =>
                MarkdownDiskPageIndex.BuildAsync(path, tempDirectory: temp));
            Assert.True(failure is DecoderFallbackException or InvalidDataException,
                "Expected a strict UTF-8 or terminal-control validation failure; got " +
                failure?.GetType().Name);
            Assert.Empty(Directory.GetFiles(temp, "afrowave-markdown-index-*.tmp"));
        });
    }

    [Fact]
    public async Task Enforces_one_line_limit_even_when_file_contains_no_newlines()
    {
        await WithFileAsync(Encoding.UTF8.GetBytes(new string('x', 1025)),
            async (path, temp) =>
            {
                await Assert.ThrowsAsync<InvalidDataException>(() =>
                    MarkdownDiskPageIndex.BuildAsync(path,
                        new MarkdownDiskPageOptions { MaxLineBytes = 1024 },
                        tempDirectory: temp));
                Assert.Empty(Directory.GetFiles(temp, "afrowave-markdown-index-*.tmp"));
            });
    }

    [Fact]
    public async Task Disposes_index_without_touching_source_and_rejects_later_reads()
    {
        await WithFileAsync(Encoding.UTF8.GetBytes("one\ntwo\nthree"),
            async (path, temp) =>
            {
                var index = await MarkdownDiskPageIndex.BuildAsync(path,
                    new MarkdownDiskPageOptions { LinesPerPage = 1 },
                    tempDirectory: temp);
                Assert.Single(Directory.GetFiles(temp, "afrowave-markdown-index-*.tmp"));
                await index.DisposeAsync();
                index.Dispose();
                Assert.Empty(Directory.GetFiles(temp, "afrowave-markdown-index-*.tmp"));
                Assert.True(File.Exists(path));
                await Assert.ThrowsAsync<ObjectDisposedException>(
                    () => index.ReadPageAsync(0));
            });
    }

    [Fact]
    public async Task Detects_source_size_changes_before_page_reads()
    {
        await WithFileAsync(Encoding.UTF8.GetBytes("one\ntwo\n"),
            async (path, temp) =>
            {
                await using var index = await MarkdownDiskPageIndex.BuildAsync(path,
                    tempDirectory: temp);
                await File.AppendAllTextAsync(path, "three\n");
                await Assert.ThrowsAsync<IOException>(() => index.ReadPageAsync(0));
            });
    }

    [Fact]
    public async Task Rejects_invalid_options_and_invalid_page_numbers()
    {
        await WithFileAsync(Encoding.UTF8.GetBytes("line"),
            async (path, temp) =>
            {
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                    MarkdownDiskPageIndex.BuildAsync(path,
                        new MarkdownDiskPageOptions { LinesPerPage = 0 },
                        tempDirectory: temp));
                await using var index = await MarkdownDiskPageIndex.BuildAsync(path,
                    tempDirectory: temp);
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                    () => index.ReadPageAsync(-1));
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                    () => index.ReadPageAsync(1));
            });
    }

    [Fact]
    public async Task A_large_number_of_lines_creates_a_compact_disk_index()
    {
        const int lines = 25_000;
        await WithFileAsync(Encoding.UTF8.GetBytes(string.Concat(
            Enumerable.Repeat("test line\n", lines))), async (path, temp) =>
        {
            await using var index = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 50 }, tempDirectory: temp);
            Assert.Equal(lines, index.LineCount);
            Assert.Equal(500, index.PageCount);
            var indexFile = Assert.Single(Directory.GetFiles(temp,
                "afrowave-markdown-index-*.tmp"));
            Assert.Equal(500 * sizeof(long), new FileInfo(indexFile).Length);
            Assert.Equal("test line\n", (await index.ReadPageAsync(499)).Content[..10]);
            Assert.Equal(50 * "test line\n".Length,
                (await index.ReadPageAsync(499)).Content.Length);
        });
    }

    private static async Task WithFileAsync(
        byte[] bytes, Func<string, string, Task> assertion)
    {
        var folder = Path.Combine(Path.GetTempPath(),
            "afrowave-markdown-page-test-" + Guid.NewGuid().ToString("N"));
        var temp = Path.Combine(folder, "indexes");
        Directory.CreateDirectory(temp);
        var path = Path.Combine(folder, "document.md");
        await File.WriteAllBytesAsync(path, bytes);
        try
        {
            await assertion(path, temp);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}
