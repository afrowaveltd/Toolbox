using System.Text;
using Xunit;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown.Tests;

public sealed class MarkdownFenceCheckpointIndexTests
{
    [Fact]
    public async Task Code_fence_opened_on_previous_page_is_preserved_after_random_seek()
    {
        const string markdown =
            "# Introduction\n\n~~~~csharp\n" +
            "# inside code\n- also code\n~~~\n" +
            "~~~~\n# Outside\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 3 }, tempDirectory: temp);
            await using var fences = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);
            Assert.Equal(3, fences.PageCount);

            var last = await fences.ReadPageAsync(2);    // jump to end first
            var middle = await fences.ReadPageAsync(1);
            var first = await fences.ReadPageAsync(0);
            Assert.Equal('~', middle.Checkpoint.FenceMarker);
            Assert.Equal(4, middle.Checkpoint.FenceLength);
            Assert.Equal("csharp", middle.Checkpoint.CodeLanguage);
            Assert.Equal(0, middle.Checkpoint.FenceQuoteDepth);
            Assert.All(middle.Lines, line =>
                Assert.Equal(MarkdownStreamLineKind.CodeContent, line.Kind));
            Assert.Equal(MarkdownStreamLineKind.CodeFenceClose, last.Lines[0].Kind);
            Assert.Equal(MarkdownStreamLineKind.Markdown, last.Lines[1].Kind);
            Assert.Equal(MarkdownStreamLineKind.CodeFenceOpen, first.Lines[2].Kind);
            Assert.Equal(markdown,
                first.Source.Content + middle.Source.Content + last.Source.Content);

            var ordered = first.Lines.Concat(middle.Lines).Concat(last.Lines).ToArray();
            long offset = 0;
            foreach (var line in ordered)
            {
                Assert.Equal(offset, line.Start);
                offset += line.Length;
            }
            Assert.Equal(markdown.Length, offset);
        });
    }

    [Fact]
    public async Task Backtick_fence_requires_matching_length_across_page_boundaries()
    {
        const string markdown = "````js\n" +
            "```\n" +
            "# still code\n" +
            "````\n" +
            "# heading\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 1 }, tempDirectory: temp);
            await using var fences = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);
            Assert.Equal(MarkdownStreamLineKind.CodeContent,
                Assert.Single((await fences.ReadPageAsync(1)).Lines).Kind);
            Assert.Equal(MarkdownStreamLineKind.CodeContent,
                Assert.Single((await fences.ReadPageAsync(2)).Lines).Kind);
            Assert.Equal(MarkdownStreamLineKind.CodeFenceClose,
                Assert.Single((await fences.ReadPageAsync(3)).Lines).Kind);
            Assert.Equal(MarkdownStreamLineKind.Markdown,
                Assert.Single((await fences.ReadPageAsync(4)).Lines).Kind);
            Assert.Equal('\0', (await fences.ReadCheckpointAsync(4)).FenceMarker);
        });
    }

    [Fact]
    public async Task Quoted_fence_is_interrupted_by_unquoted_line_across_page_boundary()
    {
        const string markdown =
            "> ~~~bash\n" +
            "> echo safe\n" +
            "## Outside\n" +
            "> not code\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 2 }, tempDirectory: temp);
            await using var fences = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);
            Assert.Equal(1, (await fences.ReadCheckpointAsync(1)).FenceQuoteDepth);
            var page = await fences.ReadPageAsync(1);
            Assert.Equal(MarkdownStreamLineKind.Markdown, page.Lines[0].Kind);
            Assert.Equal(MarkdownStreamLineKind.Markdown, page.Lines[1].Kind);
            Assert.Equal("## Outside", page.Lines[0].Content);
        });
    }

    [Fact]
    public async Task Utf8_bom_and_multibyte_characters_preserve_global_UTF16_offsets()
    {
        const string markdown = "# 🐈\n~~~cs\nřádek 🦋\n~~~\nEnd\n";
        var bytes = new byte[] { 0xEF, 0xBB, 0xBF }
            .Concat(Encoding.UTF8.GetBytes(markdown)).ToArray();
        await WithBytesAsync(bytes, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 2 }, tempDirectory: temp);
            await using var fences = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);
            var middle = await fences.ReadPageAsync(1);
            Assert.Equal(markdown.IndexOf("řádek", StringComparison.Ordinal),
                middle.Checkpoint.Start);
            Assert.Equal(MarkdownStreamLineKind.CodeContent, middle.Lines[0].Kind);
            Assert.Equal(MarkdownStreamLineKind.CodeFenceClose, middle.Lines[1].Kind);
            Assert.Equal(markdown, (await fences.ReadPageAsync(0)).Source.Content +
                middle.Source.Content + (await fences.ReadPageAsync(2)).Source.Content);
        });
    }

    [Fact]
    public async Task Index_file_is_constant_size_per_page_and_is_deleted_on_dispose()
    {
        var markdown = "~~~bash\n" +
            string.Concat(Enumerable.Repeat("some code\n", 2_000)) + "~~~\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 10 }, tempDirectory: temp);
            var fences = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);
            var checkpoint = Assert.Single(Directory.GetFiles(temp,
                "afrowave-markdown-fences-*.tmp"));
            Assert.Equal(pages.PageCount * 96, new FileInfo(checkpoint).Length);
            Assert.Equal(MarkdownStreamLineKind.CodeContent,
                (await fences.ReadPageAsync(150)).Lines[0].Kind);
            await fences.DisposeAsync();
            Assert.False(File.Exists(checkpoint));
            Assert.True(File.Exists(path));
            await Assert.ThrowsAsync<ObjectDisposedException>(
                () => fences.ReadCheckpointAsync(0));
        });
    }

    [Fact]
    public async Task Rejects_too_small_checkpoint_quota_before_creating_file()
    {
        await WithMarkdownAsync("one\ntwo\n", async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 1 }, tempDirectory: temp);
            await Assert.ThrowsAsync<InvalidDataException>(() =>
                MarkdownFenceCheckpointIndex.BuildAsync(
                    pages, tempDirectory: temp, maxIndexBytes: 96));
            Assert.Empty(Directory.GetFiles(temp, "afrowave-markdown-fences-*.tmp"));
        });
    }

    [Fact]
    public async Task Utf8_bom_only_document_has_no_phantom_line_or_checkpoint()
    {
        await WithBytesAsync([0xEF, 0xBB, 0xBF], async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                tempDirectory: temp);
            Assert.Equal(0, pages.LineCount);
            Assert.Equal(0, pages.PageCount);
            await using var fences = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);
            Assert.Equal(0, fences.PageCount);
        });
    }

    [Fact]
    public async Task An_empty_document_has_no_checkpoint_records()
    {
        await WithMarkdownAsync("", async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                tempDirectory: temp);
            await using var fences = await MarkdownFenceCheckpointIndex.BuildAsync(
                pages, tempDirectory: temp);
            Assert.Equal(0, fences.PageCount);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => fences.ReadCheckpointAsync(0));
        });
    }

    private static Task WithMarkdownAsync(
        string markdown, Func<string, string, Task> assertion) =>
        WithBytesAsync(new UTF8Encoding(false).GetBytes(markdown), assertion);

    private static async Task WithBytesAsync(
        byte[] bytes, Func<string, string, Task> assertion)
    {
        var directory = Path.Combine(Path.GetTempPath(), "afrowave-fence-index-test-" +
            Guid.NewGuid().ToString("N"));
        var temp = Path.Combine(directory, "index");
        Directory.CreateDirectory(temp);
        var file = Path.Combine(directory, "source.md");
        await File.WriteAllBytesAsync(file, bytes);
        try { await assertion(file, temp); }
        finally { Directory.Delete(directory, true); }
    }
}
