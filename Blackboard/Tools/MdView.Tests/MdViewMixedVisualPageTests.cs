using System.Text;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>
/// Mixed Markdown/native Spectre layout parity across source page boundaries,
/// preserving all compact-table, code and terminal resize safeguards.
/// </summary>
public sealed class MdViewMixedVisualPageTests
{
    [Fact]
    public async Task NativeTableIsEmbeddedBetweenOrdinaryMarkdownRows()
    {
        const string markdown =
            "Introduction\n| Name | Count |\n| :--- | ---: |\n" +
            "| [red]literal[/] | 1 |\nFollowing paragraph.";
        using var fixture = new Fixture(markdown);
        var original = await File.ReadAllBytesAsync(fixture.Path);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole();

        var page = await MdViewMixedVisualPagePlanner.PlanAsync(
            cursor, MdViewMixedVisualPosition.Beginning, console, 40);

        Assert.True(page.AtEnd);
        Assert.Equal(5, page.Next.Line);
        Assert.Equal(0, page.Next.WrapRow);
        Assert.Equal(3, page.Blocks.Count);
        var native = Assert.IsType<MdViewMixedVisualTableBlock>(page.Blocks[1]);
        Assert.Equal(1, native.HeaderLine);
        Assert.Equal(4, native.NextSourceLine);
        Assert.Equal(1, native.DataRows);
        Assert.Equal(native.Height,
            MdViewSpectreTableHeight.CountRows(native.Widget, console));
        Assert.Equal(page.Blocks.Sum(block => block.VisualRows), page.Rows);
        Assert.Equal(string.Empty, console.Output);

        var resume = MdViewMixedVisualPageRenderer.Render(
            console, page, new MarkdownSpectreTheme());

        Assert.Equal(page.Next, resume);
        Assert.Contains("Introduction", console.Output);
        Assert.Contains("╭", console.Output);
        Assert.Contains("Name", console.Output);
        Assert.Contains("Count", console.Output);
        Assert.Contains("[red]literal[/]", console.Output);
        Assert.Contains("Following paragraph.", console.Output);
        Assert.Equal(original, await File.ReadAllBytesAsync(fixture.Path));
    }

    [Fact]
    public async Task TableMovesToNextPageBeforeItsHeaderWhenRemainingHeightIsSmall()
    {
        const string table =
            "| Name | Value |\n| --- | ---: |\n| Apple | 10 |\nAfter";
        using var probe = new Fixture(table);
        await using var probeSnapshot = await Utf8ValidatedSnapshot.CreateAsync(probe.Path);
        await using var probeCursor = new MdViewContainerPageCursor(probeSnapshot);
        var console = MakeConsole();
        var probeHeader = await MdViewSpectreTableBlockReader.ReadHeaderAsync(
            probeCursor, 0);
        Assert.NotNull(probeHeader);
        var measured = await MdViewNativeTablePagePlanner.PlanAsync(
            probeCursor, probeHeader, 2, console, 50);
        Assert.True(measured.IsNative);

        // Leave exactly one visual row before the table, even though
        // the complete same table fits an otherwise empty page.
        var introCount = measured.VisualRows;
        var source = string.Join("\n", Enumerable.Repeat("Introduction", introCount)) +
            "\n" + table;
        using var fixture = new Fixture(source);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);

        var first = await MdViewMixedVisualPagePlanner.PlanAsync(
            cursor, MdViewMixedVisualPosition.Beginning, console,
            measured.VisualRows + 1);
        Assert.Equal(introCount, first.Next.Line);
        Assert.Equal(0, first.Next.WrapRow);
        Assert.False(first.AtEnd);
        Assert.Empty(first.Blocks.OfType<MdViewMixedVisualTableBlock>());

        var second = await MdViewMixedVisualPagePlanner.PlanAsync(
            cursor, first.Next, console, measured.VisualRows + 1);
        Assert.Equal(introCount, second.Start.Line);
        Assert.Single(second.Blocks.OfType<MdViewMixedVisualTableBlock>());
        Assert.Contains(second.Blocks,
            block => block is MdViewMixedVisualTableBlock native &&
                native.HeaderLine == introCount);
        Assert.True(second.Next.Line > first.Next.Line);
        Assert.Equal(string.Empty, console.Output);
    }

    [Fact]
    public async Task LongTableRemainsCompactAcrossAllPageBreaks()
    {
        const string header = "| Name | Value |\n| --- | --- |\n";
        var source = "Before\n" + header +
            string.Join("\n", Enumerable.Range(1, 14)
                .Select(i => "| Item " + i + " | " + i + " |")) +
            "\nAfter";
        using var fixture = new Fixture(source);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole(width: 80);
        var position = MdViewMixedVisualPosition.Beginning;
        var sawCompactState = false;
        var pageCount = 0;

        while (true)
        {
            var page = await MdViewMixedVisualPagePlanner.PlanAsync(
                cursor, position, console, contentRows: 4);
            Assert.Empty(page.Blocks.OfType<MdViewMixedVisualTableBlock>());
            Assert.InRange(page.Rows, 1, 4);
            Assert.True(page.Next.Line > position.Line ||
                page.Next.WrapRow > position.WrapRow);
            sawCompactState |= page.Next.CompactHeaderLine == 1;

            MdViewMixedVisualPageRenderer.Render(
                console, page, new MarkdownSpectreTheme());
            position = page.Next;
            pageCount++;
            Assert.InRange(pageCount, 1, 60);
            if (page.AtEnd)
                break;
        }

        Assert.True(sawCompactState);
        Assert.True(pageCount > 3);
        Assert.Contains("Item 1", console.Output);
        Assert.Contains("Item 14", console.Output);
        Assert.Contains("After", console.Output);
        Assert.DoesNotContain("╭", console.Output);
        Assert.Equal(-1, position.CompactHeaderLine);
    }

    [Fact]
    public async Task OverheightSingleRowTableUsesCompactFallbackOnEveryPage()
    {
        var source = "| Key | Description |\n| --- | --- |\n" +
            "| Alpha | " +
            string.Join(" ", Enumerable.Repeat("very long description", 30)) + " |" +
            "\nDone";
        using var fixture = new Fixture(source);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole(width: 80);
        var next = MdViewMixedVisualPosition.Beginning;
        var sawCompact = false;

        for (var pageNo = 0; pageNo < 100; pageNo++)
        {
            var page = await MdViewMixedVisualPagePlanner.PlanAsync(
                cursor, next, console, 3);
            Assert.Empty(page.Blocks.OfType<MdViewMixedVisualTableBlock>());
            Assert.True(page.Rows is > 0 and <= 3);
            sawCompact |= page.Next.CompactHeaderLine == 0;
            next = page.Next;
            if (page.AtEnd)
            {
                Assert.True(sawCompact);
                Assert.Equal(-1, next.CompactHeaderLine);
                return;
            }
        }

        Assert.Fail("Compact fallback did not reach the end of the document.");
    }

    [Fact]
    public async Task OrdinaryMarkdownHasExactlyTheExistingCompactPageStarts()
    {
        var source = "# Heading\n" +
            string.Join(" ", Enumerable.Repeat("This is ordinary wrapped prose.", 30)) +
            "\n> Quoted line with visible prefix\n- A list item\n" +
            "\u0060\u0060\u0060c\n    int x = 7;\n\u0060\u0060\u0060\nEnd";
        using var fixture = new Fixture(source);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole(width: 30);
        await using var compactIndex = await MdViewTerminalPageIndex.CreateAsync(
            snapshot, width: 30, height: 80, maximumSourceLinesPerPage: 2);

        var next = MdViewMixedVisualPosition.Beginning;
        for (long pageNumber = 0; pageNumber < compactIndex.PageCount; pageNumber++)
        {
            var oldStart = await compactIndex.GetStartAsync(pageNumber);
            Assert.Equal(oldStart.Line, next.Line);
            Assert.Equal(oldStart.WrapRow, next.WrapRow);
            Assert.Equal(-1, next.CompactHeaderLine);

            var page = await MdViewMixedVisualPagePlanner.PlanAsync(
                cursor, next, console, 2);
            Assert.Empty(page.Blocks.OfType<MdViewMixedVisualTableBlock>());
            Assert.InRange(page.Rows, 1, 2);
            next = page.Next;
        }

        Assert.Equal(snapshot.Lines.LineCount, next.Line);
        Assert.Equal(0, next.WrapRow);
    }

    [Fact]
    public async Task FencedPipeTextDoesNotBecomeNativeTable()
    {
        const string source =
            "\u0060\u0060\u0060markdown\n" +
            "| Name | Value |\n| --- | --- |\n| A | B |\n" +
            "\u0060\u0060\u0060\nOrdinary";
        using var fixture = new Fixture(source);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole();

        var page = await MdViewMixedVisualPagePlanner.PlanAsync(
            cursor, MdViewMixedVisualPosition.Beginning, console, 40);
        Assert.Empty(page.Blocks.OfType<MdViewMixedVisualTableBlock>());
        Assert.True(page.AtEnd);
        MdViewMixedVisualPageRenderer.Render(
            console, page, new MarkdownSpectreTheme());
        Assert.Contains("| Name | Value |", console.Output);
        Assert.DoesNotContain("╭", console.Output);
    }

    [Fact]
    public async Task ResizeIsRejectedBeforeAnyMixedPageOutput()
    {
        using var fixture = new Fixture(
            "Before\n| Name | Value |\n| --- | --- |\n| A | B |");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole();

        var page = await MdViewMixedVisualPagePlanner.PlanAsync(
            cursor, MdViewMixedVisualPosition.Beginning, console, 40);
        console.Profile.Width++;

        Assert.Throws<InvalidOperationException>(() =>
            MdViewMixedVisualPageRenderer.Render(
                console, page, new MarkdownSpectreTheme()));
        Assert.Equal(string.Empty, console.Output);
    }

    [Fact]
    public async Task InvalidPositionsAndCancelledPlanningAreRejected()
    {
        using var fixture = new Fixture("First\nSecond");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewMixedVisualPagePlanner.PlanAsync(
                cursor, new MdViewMixedVisualPosition(-1, 0), console, 20));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewMixedVisualPagePlanner.PlanAsync(
                cursor, new MdViewMixedVisualPosition(0, -1), console, 20));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewMixedVisualPagePlanner.PlanAsync(
                cursor, new MdViewMixedVisualPosition(0, 0, 0, 0), console, 20));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewMixedVisualPagePlanner.PlanAsync(
                cursor, MdViewMixedVisualPosition.Beginning, console, 0));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewMixedVisualPagePlanner.PlanAsync(
                cursor, MdViewMixedVisualPosition.Beginning, console, 81));

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            MdViewMixedVisualPagePlanner.PlanAsync(
                cursor, MdViewMixedVisualPosition.Beginning, console, 20,
                cancellation.Token));
        Assert.Equal(string.Empty, console.Output);
    }

    private static TestConsole MakeConsole(int width = 96)
    {
        var console = new TestConsole();
        console.Profile.Width = width;
        console.Profile.Height = 80;
        console.Profile.Capabilities.Unicode = true;
        console.Profile.Capabilities.Interactive = true;
        return console;
    }

    private sealed class Fixture : IDisposable
    {
        private readonly string _directory = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "mdview-mixed-page-" + Guid.NewGuid().ToString("N"));

        public Fixture(string markdown)
        {
            Directory.CreateDirectory(_directory);
            Path = System.IO.Path.Combine(_directory, "README.md");
            File.WriteAllText(Path, markdown, new UTF8Encoding(false));
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, recursive: true);
        }
    }
}
