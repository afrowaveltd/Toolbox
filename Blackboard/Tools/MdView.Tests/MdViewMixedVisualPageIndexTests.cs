using System.Text;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>
/// Fixed-record mixed-document page indexing, source jumps and
/// planner/index/renderer parity over validated local Markdown snapshots.
/// </summary>
public sealed class MdViewMixedVisualPageIndexTests
{
    [Fact]
    public async Task OneMixedPageIndexesNativeTableAndEveryPhysicalSourceLine()
    {
        const string markdown =
            "Before\n| Name | Amount |\n| :--- | ---: |\n" +
            "| Apple | 10 |\nAfter";
        using var fixture = new Fixture(markdown);
        var before = await File.ReadAllBytesAsync(fixture.Path);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = Console();
        await using var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, contentRows: 20);

        Assert.Equal(1, index.PageCount);
        Assert.Equal(snapshot.Lines.LineCount, index.SourceLineCount);
        Assert.Equal(96, index.TerminalWidth);
        Assert.Equal(80, index.TerminalHeight);
        Assert.Equal(56, MdViewMixedVisualPageIndex.RecordBytes);

        var record = await index.GetPageAsync(0);
        Assert.Equal(MdViewMixedVisualPosition.Beginning, record.Start);
        Assert.Equal(5, record.Next.Line);
        Assert.True(record.AtEnd);
        Assert.True(record.VisualRows > 5);
        for (var line = 0L; line < index.SourceLineCount; line++)
            Assert.Equal(0, await index.FindPageAsync(line));
        Assert.Equal(string.Empty, console.Output);

        var rendered = await MdViewMixedVisualIndexedRenderer.RenderPageAsync(
            index, cursor, console, 0, new MarkdownSpectreTheme());
        Assert.Equal(record, rendered);
        Assert.Contains("Before", console.Output);
        Assert.Contains("╭", console.Output);
        Assert.Contains("Apple", console.Output);
        Assert.Contains("After", console.Output);
        Assert.Equal(before, await File.ReadAllBytesAsync(fixture.Path));
    }

    [Fact]
    public async Task LongCompactTableCrossesPagesWithPersistentSourceState()
    {
        var markdown = "Introduction\n| Name | Value |\n| --- | --- |\n" +
            string.Join("\n", Enumerable.Range(1, 14)
                .Select(i => "| Item " + i + " | " + i + " |")) +
            "\nEnding";
        using var fixture = new Fixture(markdown);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = Console();
        await using var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, contentRows: 4);

        Assert.True(index.PageCount >= 7);
        Assert.Equal(snapshot.Lines.LineCount, index.SourceLineCount);
        var previousNext = MdViewMixedVisualPosition.Beginning;
        var sawCompact = false;
        for (var pageNumber = 0L; pageNumber < index.PageCount; pageNumber++)
        {
            var record = await index.GetPageAsync(pageNumber);
            Assert.Equal(previousNext, record.Start);
            Assert.InRange(record.VisualRows, 1, 4);
            Assert.Equal(pageNumber == index.PageCount - 1, record.AtEnd);
            sawCompact |= record.Start.CompactHeaderLine == 1 ||
                          record.Next.CompactHeaderLine == 1;

            var verified = await MdViewMixedVisualIndexedRenderer.RenderPageAsync(
                index, cursor, console, pageNumber, new MarkdownSpectreTheme());
            Assert.Equal(record, verified);
            previousNext = record.Next;
        }

        Assert.True(sawCompact);
        Assert.Equal(index.SourceLineCount, previousNext.Line);
        Assert.Equal(-1, previousNext.CompactHeaderLine);
        Assert.Equal(0, await index.FindPageAsync(0));
        Assert.Equal(0, await index.FindPageAsync(1));
        Assert.Equal(0, await index.FindPageAsync(2));
        var distant = await index.FindPageAsync(16);
        Assert.InRange(distant, 1L, index.PageCount - 1);
        Assert.Contains("Item 1", console.Output);
        Assert.Contains("Item 14", console.Output);
        Assert.Contains("Ending", console.Output);
        Assert.DoesNotContain("╭", console.Output);
    }

    [Fact]
    public async Task SourceRowLookupUsesDiskBoundariesWithinOneLongWrappedLine()
    {
        using var fixture = new Fixture(new string('a', 210) + "\nEnding");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = Console(width: 20);
        await using var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, contentRows: 2);

        Assert.True(index.PageCount > 4);
        Assert.Equal(0, await index.FindPageAsync(0));
        Assert.Equal(1, await index.FindPageAsync(new MdViewVisualPosition(0, 2)));
        Assert.Equal(2, await index.FindPageAsync(new MdViewVisualPosition(0, 4)));
        Assert.Equal(new MdViewMixedVisualPosition(0, 2),
            (await index.GetPageAsync(1)).Start);
        Assert.Equal(new MdViewMixedVisualPosition(0, 4),
            (await index.GetPageAsync(2)).Start);
        var finalPage = await index.FindPageAsync(1);
        Assert.True(finalPage >= 3);
        Assert.Equal(1, (await index.GetPageAsync(finalPage)).Start.Line);
        Assert.Equal(string.Empty, console.Output);
    }

    [Fact]
    public async Task CompactHeaderAndDelimiterStayInSameTableAcrossDiskPages()
    {
        var markdown = "Intro\n| Name | Value |\n| --- | --- |\n" +
            string.Join("\n", Enumerable.Range(1, 9)
                .Select(i => "| Item " + i + " | " + i + " |"));
        using var fixture = new Fixture(markdown);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = Console();
        await using var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, 3);

        var first = await index.GetPageAsync(0);
        var second = await index.GetPageAsync(1);
        Assert.Equal(2, first.Next.Line);
        Assert.Equal(1, first.Next.CompactHeaderLine);
        Assert.Equal(2, second.Start.Line);
        Assert.Equal(1, second.Start.CompactHeaderLine);
        Assert.Equal(0, await index.FindPageAsync(1));
        Assert.Equal(1, await index.FindPageAsync(2));
    }

    [Fact]
    public async Task TwoCompleteNativeTablesCanOccupyDifferentMixedPages()
    {
        const string markdown =
            "Overview\n| Name | Value |\n| --- | --- |\n| A | 1 |\n" +
            "Between\n| Name | Value |\n| --- | --- |\n| B | 2 |\nEnding";
        using var fixture = new Fixture(markdown);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = Console();
        await using var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, 9);

        Assert.True(index.PageCount >= 2);
        for (var i = 0L; i < index.PageCount; i++)
            await MdViewMixedVisualIndexedRenderer.RenderPageAsync(
                index, cursor, console, i, new MarkdownSpectreTheme());

        Assert.Equal(2, console.Output.Count(ch => ch == '╭'));
        Assert.Contains("Overview", console.Output);
        Assert.Contains("Between", console.Output);
        Assert.Contains("Ending", console.Output);
        Assert.Contains("A", console.Output);
        Assert.Contains("B", console.Output);
    }

    [Fact]
    public async Task EmptyMarkdownDocumentProducesExactlyOneEmptyPage()
    {
        using var fixture = new Fixture("");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = Console();
        await using var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, 5);

        Assert.Equal(1, index.PageCount);
        var record = await index.GetPageAsync(0);
        Assert.True(record.AtEnd);
        Assert.Equal(0, record.VisualRows);
        Assert.Equal(record.Start, record.Next);
        await MdViewMixedVisualIndexedRenderer.RenderPageAsync(
            index, cursor, console, 0, new MarkdownSpectreTheme());
        Assert.Equal(string.Empty, console.Output);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            index.FindPageAsync(0));
    }

    [Fact]
    public async Task ExactlyFilledTextPageDoesNotAddPhantomTrailingPage()
    {
        using var fixture = new Fixture("First\nSecond");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = Console();
        await using var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, 2);

        Assert.Equal(1, index.PageCount);
        var page = await index.GetPageAsync(0);
        Assert.Equal(2, page.VisualRows);
        Assert.True(page.AtEnd);
        Assert.Equal(2, page.Next.Line);
    }

    [Fact]
    public async Task StaleTerminalAndSubstitutedSourceAreRejectedBeforeOutput()
    {
        using var fixture = new Fixture("| Name | Value |\n| --- | --- |\n| A | B |");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        await using var foreignCursor = new MdViewContainerPageCursor(snapshot);
        var console = Console();
        await using var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, 20);

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            MdViewMixedVisualIndexedRenderer.RenderPageAsync(
                index, foreignCursor, console, 0, new MarkdownSpectreTheme()));
        console.Profile.Width++;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            MdViewMixedVisualIndexedRenderer.RenderPageAsync(
                index, cursor, console, 0, new MarkdownSpectreTheme()));
        Assert.Equal(string.Empty, console.Output);
    }

    [Fact]
    public async Task DiskBudgetFailureLeavesValidatedSourceUntouched()
    {
        using var fixture = new Fixture(string.Join("\n",
            Enumerable.Range(1, 20).Select(i => "Line " + i)));
        var before = await File.ReadAllBytesAsync(fixture.Path);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = Console();

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            MdViewMixedVisualPageIndex.CreateAsync(
                cursor, console, 1,
                maximumIndexBytes: 2 * MdViewMixedVisualPageIndex.RecordBytes));
        Assert.Equal(before, await File.ReadAllBytesAsync(fixture.Path));
        Assert.Equal(string.Empty, console.Output);
    }

    [Fact]
    public async Task InvalidArgumentsAndReleasedIndexFailPredictably()
    {
        using var fixture = new Fixture("One\nTwo\nThree");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = Console();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewMixedVisualPageIndex.CreateAsync(cursor, console, 0));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewMixedVisualPageIndex.CreateAsync(cursor, console, 81));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewMixedVisualPageIndex.CreateAsync(cursor, console, 10,
                maximumIndexBytes: MdViewMixedVisualPageIndex.RecordBytes - 1));

        var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, 2);
        try
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                index.GetPageAsync(-1));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                index.GetPageAsync(index.PageCount));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                index.FindPageAsync(-1));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                index.FindPageAsync(index.SourceLineCount));
        }
        finally
        {
            await index.DisposeAsync();
        }

        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            index.GetPageAsync(0));
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            index.FindPageAsync(0));
    }

    [Fact]
    public async Task CancellationPreventsIndexConstructionAndOutput()
    {
        using var fixture = new Fixture("Line one\nLine two");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = Console();
        using var cancel = new CancellationTokenSource();
        cancel.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            MdViewMixedVisualPageIndex.CreateAsync(
                cursor, console, 5, cancellationToken: cancel.Token));

        await using var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, 5);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            MdViewMixedVisualIndexedRenderer.RenderPageAsync(
                index, cursor, console, 0, new MarkdownSpectreTheme(), cancel.Token));
        Assert.Equal(string.Empty, console.Output);
    }

    private static TestConsole Console(int width = 96)
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
        private readonly string _folder = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "mdview-mixed-index-" + Guid.NewGuid().ToString("N"));

        public Fixture(string text)
        {
            Directory.CreateDirectory(_folder);
            Path = System.IO.Path.Combine(_folder, "README.md");
            File.WriteAllText(Path, text, new UTF8Encoding(false));
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(_folder))
                Directory.Delete(_folder, recursive: true);
        }
    }
}
