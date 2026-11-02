using System.Text;
using Xunit;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown.Tests;

public sealed class MarkdownListSpacingIndexTests
{
    [Fact]
    public async Task Later_blank_separated_item_marks_entire_previous_group_as_loose_candidate()
    {
        const string markdown =
            "- First\n" +
            "  first continuation\n" +
            "- Second\n" +
            "\n" +
            "- Third\n" +
            "  third continuation\n" +
            "\n" +
            "Outside\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 2 }, tempDirectory: temp);
            await using var index = await MarkdownListSpacingIndex.BuildAsync(
                pages, tempDirectory: temp);
            Assert.Equal(1, index.GroupCount);
            Assert.Equal(3, index.ItemCount);

            // Earlier items acquire the result retroactively after a later
            // item is separated by a blank source line.
            var first = await index.FindForItemAsync(0);
            var second = await index.FindForItemAsync(
                markdown.IndexOf("- Second", StringComparison.Ordinal));
            var third = await index.FindForItemAsync(
                markdown.IndexOf("- Third", StringComparison.Ordinal));
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.NotNull(third);
            Assert.True(first.IsLooseCandidate);
            Assert.Equal(first, second);
            Assert.Equal(second, third);
            Assert.Equal(1, first.Depth);
            Assert.Equal(0, first.QuoteDepth);
            Assert.Null(await index.FindForItemAsync(12_345));
        });
    }

    [Fact]
    public async Task Tight_group_and_blank_separated_continuation_are_distinct()
    {
        const string markdown =
            "- A\n" +
            "- B\n" +
            "  continuation\n" +
            "\n" +
            "  new paragraph in B\n" +
            "- C\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 1 }, tempDirectory: temp);
            await using var index = await MarkdownListSpacingIndex.BuildAsync(
                pages, tempDirectory: temp);
            var first = await index.FindForItemAsync(0);
            Assert.NotNull(first);
            Assert.True(first.IsLooseCandidate);
            Assert.Equal(3, index.ItemCount);
            Assert.Equal(1, index.GroupCount);
        });

        await WithMarkdownAsync("- A\n- B\n- C\n", async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(
                path, tempDirectory: temp);
            await using var index = await MarkdownListSpacingIndex.BuildAsync(
                pages, tempDirectory: temp);
            var first = await index.FindForItemAsync(0);
            Assert.NotNull(first);
            Assert.False(first.IsLooseCandidate);
            Assert.Equal(3, index.ItemCount);
        });
    }

    [Fact]
    public async Task Nested_list_has_independent_group_decision_and_grouped_siblings()
    {
        const string markdown =
            "- Parent\n" +
            "  - Child A\n" +
            "\n" +
            "  - Child B\n" +
            "- Parent sibling\n" +
            "Outside\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 1 }, tempDirectory: temp);
            await using var index = await MarkdownListSpacingIndex.BuildAsync(
                pages, tempDirectory: temp);
            var root = await index.FindForItemAsync(0);
            var childA = await index.FindForItemAsync(
                markdown.IndexOf("  - Child A", StringComparison.Ordinal));
            var childB = await index.FindForItemAsync(
                markdown.IndexOf("  - Child B", StringComparison.Ordinal));
            var sibling = await index.FindForItemAsync(
                markdown.IndexOf("- Parent sibling", StringComparison.Ordinal));
            Assert.NotNull(root);
            Assert.NotNull(childA);
            Assert.NotNull(childB);
            Assert.NotNull(sibling);
            Assert.Equal(2, index.GroupCount);
            Assert.Equal(4, index.ItemCount);
            Assert.False(root.IsLooseCandidate);
            Assert.True(childA.IsLooseCandidate);
            Assert.Equal(childA, childB);
            Assert.Equal(root, sibling);
            Assert.Equal(2, childA.Depth);
        });
    }

    [Fact]
    public async Task Quote_boundary_and_list_marker_type_start_separate_groups()
    {
        const string markdown =
            "> - Quoted A\n" +
            "> - Quoted B\n" +
            "- Plain\n" +
            "1. Numbered\n" +
            "2. Numbered sibling\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                new MarkdownDiskPageOptions { LinesPerPage = 1 }, tempDirectory: temp);
            await using var index = await MarkdownListSpacingIndex.BuildAsync(
                pages, tempDirectory: temp);
            Assert.Equal(3, index.GroupCount);
            Assert.Equal(5, index.ItemCount);
            Assert.Equal(1, (await index.FindForItemAsync(0))!.QuoteDepth);
            var numbered = await index.FindForItemAsync(
                markdown.IndexOf("1. Numbered", StringComparison.Ordinal));
            Assert.Equal(numbered, await index.FindForItemAsync(
                markdown.IndexOf("2. Numbered sibling", StringComparison.Ordinal)));
            Assert.False(numbered!.IsLooseCandidate);
        });
    }

    [Fact]
    public async Task Quota_rejection_cleans_both_files_and_disposal_removes_records()
    {
        const string markdown = "- A\n- B\n- C\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                tempDirectory: temp);
            await Assert.ThrowsAsync<InvalidDataException>(() =>
                MarkdownListSpacingIndex.BuildAsync(pages,
                    tempDirectory: temp, maxIndexBytes: 24 + 2 * 16));
            Assert.Empty(Directory.GetFiles(temp, "afrowave-markdown-list-*.tmp"));

            var index = await MarkdownListSpacingIndex.BuildAsync(
                pages, tempDirectory: temp);
            Assert.Equal(2, Directory.GetFiles(temp, "afrowave-markdown-list-*.tmp").Length);
            await index.DisposeAsync();
            Assert.Empty(Directory.GetFiles(temp, "afrowave-markdown-list-*.tmp"));
            await Assert.ThrowsAsync<ObjectDisposedException>(
                () => index.FindForItemAsync(0));
        });
    }

    [Fact]
    public async Task No_list_creates_no_entries_and_does_not_change_source()
    {
        const string markdown = "# Heading\nPlain prose\n";
        await WithMarkdownAsync(markdown, async (path, temp) =>
        {
            await using var pages = await MarkdownDiskPageIndex.BuildAsync(path,
                tempDirectory: temp);
            await using var index = await MarkdownListSpacingIndex.BuildAsync(
                pages, tempDirectory: temp);
            Assert.Equal(0, index.GroupCount);
            Assert.Equal(0, index.ItemCount);
            Assert.Null(await index.FindForItemAsync(0));
            Assert.Equal(markdown, await File.ReadAllTextAsync(path));
        });
    }

    private static async Task WithMarkdownAsync(
        string source, Func<string, string, Task> assertion)
    {
        var root = Path.Combine(Path.GetTempPath(),
            "afrowave-list-summary-test-" + Guid.NewGuid().ToString("N"));
        var temp = Path.Combine(root, "temp");
        Directory.CreateDirectory(temp);
        var path = Path.Combine(root, "source.md");
        await File.WriteAllTextAsync(path, source, new UTF8Encoding(false));
        try { await assertion(path, temp); }
        finally { Directory.Delete(root, recursive: true); }
    }
}
