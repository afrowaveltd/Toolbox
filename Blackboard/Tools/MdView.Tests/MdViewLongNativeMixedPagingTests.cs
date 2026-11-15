using System.Text;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>
/// Experimental, explicitly selected repeated-header native table fragments
/// across canonical mixed Markdown pages, including source lookup and the
/// disk-backed renderer's exact re-planning contract.
/// </summary>
public sealed class MdViewLongNativeMixedPagingTests
{
    [Fact]
    public async Task LongNativeFragmentsRepeatHeaderAndPreserveEverySourceRow()
    {
        using var fixture = new Fixture("Before table\n" + LongTable(17) +
            "\nAfter table");
        var original = await File.ReadAllBytesAsync(fixture.Path);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole();
        await using var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, contentRows: 8, longNativeTables: true);

        Assert.True(index.LongNativeTables);
        Assert.Equal(56, MdViewMixedVisualPageIndex.RecordBytes);
        Assert.True(index.PageCount >= 3);
        Assert.Equal(snapshot.Lines.LineCount, index.SourceLineCount);

        var headerCount = 0;
        var preceding = MdViewMixedVisualPosition.Beginning;
        var continuations = 0;
        for (var pageNumber = 0L; pageNumber < index.PageCount; pageNumber++)
        {
            var record = await index.GetPageAsync(pageNumber);
            Assert.Equal(preceding, record.Start);
            Assert.InRange(record.VisualRows, 1, 8);
            if (record.Start.IsNativeContinuation)
            {
                continuations++;
                Assert.Equal(1, record.Start.CompactHeaderLine);
                Assert.Equal(-1, record.Start.CompactColumns);
                Assert.Equal(0, record.Start.WrapRow);
            }

            var rendered = await MdViewMixedVisualIndexedRenderer.RenderPageAsync(
                index, cursor, console, pageNumber, new MarkdownSpectreTheme());
            Assert.Equal(record, rendered);
            preceding = record.Next;
        }

        Assert.True(continuations >= 2);
        Assert.Equal(snapshot.Lines.LineCount, preceding.Line);
        Assert.Equal(0, preceding.WrapRow);
        Assert.Equal(-1, preceding.CompactHeaderLine);
        Assert.Contains("Before table", console.Output);
        Assert.Contains("After table", console.Output);
        Assert.Contains("╭", console.Output);

        // The native Spectre heading must be repeated on each widget,
        // but every source data row is displayed exactly once.
        headerCount = Count(console.Output, "Description");
        Assert.True(headerCount >= continuations + 1);
        for (var number = 1; number <= 17; number++)
            Assert.Equal(1, Count(console.Output, $"Row-{number:000}"));

        Assert.Equal(original, await File.ReadAllBytesAsync(fixture.Path));
    }

    [Fact]
    public async Task SourceLineJumpsChooseCanonicalContinuingNativePage()
    {
        using var fixture = new Fixture("Introduction\n" + LongTable(14) +
            "\nEnding");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole();
        await using var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, contentRows: 8, longNativeTables: true);

        var firstTablePage = await index.FindPageAsync(1);
        Assert.Equal(firstTablePage, await index.FindPageAsync(2));
        var lastDataLine = 16L;
        var finalNativePageNumber = await index.FindPageAsync(lastDataLine);
        Assert.True(finalNativePageNumber > firstTablePage);

        var fragment = await index.GetPageAsync(finalNativePageNumber);
        Assert.True(fragment.Start.IsNativeContinuation);
        Assert.True(fragment.Start.Line <= lastDataLine);
        Assert.True(fragment.Next.Line > lastDataLine);
        Assert.Equal(1L, fragment.Start.CompactHeaderLine);

        await MdViewMixedVisualIndexedRenderer.RenderPageAsync(
            index, cursor, console, finalNativePageNumber,
            new MarkdownSpectreTheme());
        Assert.Contains("Row-014", console.Output);
        Assert.Contains("Description", console.Output);

        var afterPage = await index.FindPageAsync(17);
        Assert.True(afterPage >= finalNativePageNumber);
    }

    [Fact]
    public async Task ExactlyFullNativeFragmentDoesNotCreatePhantomDocumentPage()
    {
        using var fixture = new Fixture(LongTable(1));
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole();
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);
        var first = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 2, console, 80);
        Assert.True(first.IsNative);
        Assert.False(first.HasMore);

        await using var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, contentRows: first.VisualRows,
            longNativeTables: true);
        Assert.Equal(1, index.PageCount);
        var record = await index.GetPageAsync(0);
        Assert.True(record.AtEnd);
        Assert.Equal(first.VisualRows, record.VisualRows);
        Assert.Equal(3, record.Next.Line);
    }

    [Fact]
    public async Task LongTableIsStillEntirelyCompactWithoutExplicitOptIn()
    {
        using var fixture = new Fixture(LongTable(14) + "\nEnding");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole();
        await using var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, contentRows: 8);

        Assert.False(index.LongNativeTables);
        for (var pageNumber = 0L; pageNumber < index.PageCount; pageNumber++)
        {
            var page = await index.GetPageAsync(pageNumber);
            Assert.False(page.Start.IsNativeContinuation);
            Assert.False(page.Next.IsNativeContinuation);
            await MdViewMixedVisualIndexedRenderer.RenderPageAsync(
                index, cursor, console, pageNumber, new MarkdownSpectreTheme());
        }

        Assert.DoesNotContain("╭", console.Output);
        Assert.Contains("Row-001", console.Output);
        Assert.Contains("Row-014", console.Output);
        Assert.Contains("Ending", console.Output);
    }

    [Fact]
    public async Task NarrowCappedReadingAreaKeepsWholeTableCompactEvenWhenOptedIn()
    {
        using var fixture = new Fixture(LongTable(14) + "\nEnding");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole(width: 180);
        await using var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, 8, maximumContentColumns: 40,
            longNativeTables: true);

        Assert.Equal(40, index.MaximumContentColumns);
        Assert.True(index.LongNativeTables);
        for (var pageNumber = 0L; pageNumber < index.PageCount; pageNumber++)
        {
            var record = await index.GetPageAsync(pageNumber);
            Assert.False(record.Start.IsNativeContinuation);
            Assert.False(record.Next.IsNativeContinuation);
            await MdViewMixedVisualIndexedRenderer.RenderPageAsync(
                index, cursor, console, pageNumber, new MarkdownSpectreTheme(),
                centerContent: true);
        }

        Assert.Contains("Row-014", console.Output);
        Assert.DoesNotContain("╭", console.Output);
    }

    [Fact]
    public async Task RepeatedNativeHeadersRespectCenteredWidthAndPhysicalResize()
    {
        using var fixture = new Fixture(LongTable(14));
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole(width: 180);
        await using var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, contentRows: 8,
            maximumContentColumns: 80, longNativeTables: true);
        Assert.True(index.PageCount > 1);
        var start = await index.GetPageAsync(1);
        Assert.True(start.Start.IsNativeContinuation);

        await MdViewMixedVisualIndexedRenderer.RenderPageAsync(
            index, cursor, console, 1, new MarkdownSpectreTheme(),
            centerContent: true);
        Assert.Contains(new string(' ', 49) + "╭", console.Output);
        Assert.Equal(180, console.Profile.Width);

        console.Profile.Width = 190;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            MdViewMixedVisualIndexedRenderer.RenderPageAsync(
                index, cursor, console, 1, new MarkdownSpectreTheme()));
    }

    [Fact]
    public async Task InvalidNativeContinuationStatesAreRejectedWithoutWriting()
    {
        using var fixture = new Fixture(LongTable(10));
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole();
        var invalidStart = MdViewMixedVisualPosition.NativeContinuation(0, 2);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewMixedVisualPagePlanner.PlanAsync(
                cursor, invalidStart, console, 8));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewMixedVisualPagePlanner.PlanAsync(
                cursor, invalidStart with { WrapRow = 1 }, console, 8,
                longNativeTables: true));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = MdViewMixedVisualPosition.NativeContinuation(0, 1);
        });
        Assert.Equal(string.Empty, console.Output);
    }

    [Fact]
    public async Task CancelledPreflightDoesNotWriteOrConsumeOriginalSource()
    {
        using var fixture = new Fixture(LongTable(14));
        var before = await File.ReadAllBytesAsync(fixture.Path);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole();
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            MdViewLongNativeTableEligibility.CanRenderEntireTableAsync(
                cursor, header, console, 8, cancellationToken: cancellation.Token));
        Assert.Equal(string.Empty, console.Output);
        Assert.Equal(before, await File.ReadAllBytesAsync(fixture.Path));
    }

    [Fact]
    public void CliLongTableOptionIsExplicitAndCombinesWithCenteringAndWidth()
    {
        var first = MdViewCommand.Parse("PREVIEW-NATIVE", "README.md",
            "--long-tables", "--max-width", "100", "--center");
        var second = MdViewCommand.Parse("preview-native", "README.md",
            "--CENTER", "--LONG-TABLES", "--MAX-WIDTH", "100");
        Assert.Equal(first, second);
        Assert.True(first.LongNativeTables);
        Assert.Equal(100, first.MaximumContentColumns);
        Assert.True(first.CenterContent);
        Assert.False(MdViewCommand.Parse("preview-native",
            "README.md").LongNativeTables);
        Assert.False(MdViewCommand.Parse("preview",
            "README.md").LongNativeTables);
    }

    [Theory]
    [InlineData("--long-tables --long-tables")]
    [InlineData("--long-tables --unknown")]
    public void CliRejectsDuplicateOrUnknownLongTableFlags(string extra)
    {
        var args = new[] { "preview-native", "README.md" }
            .Concat(extra.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToArray();
        Assert.Throws<ArgumentException>(() => MdViewCommand.Parse(args));
    }

    [Fact]
    public async Task OptInPagerRendersLongNativeFragmentsAndPreservesSourceBytes()
    {
        using var fixture = new Fixture(LongTable(14));
        var before = await File.ReadAllBytesAsync(fixture.Path);
        var console = MakeConsole();
        console.Input.PushKey(ConsoleKey.Enter); // Next page.
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(
            fixture.Path, linesPerPage: 8, longNativeTables: true);

        Assert.Contains("Page 1 of ", console.Output);
        Assert.Contains("Page 2 of ", console.Output);
        Assert.Contains("long tables", console.Output);
        Assert.Contains("╭", console.Output);
        Assert.Contains("Row-001", console.Output);
        Assert.Equal(before, await File.ReadAllBytesAsync(fixture.Path));
    }

    private static int Count(string text, string token) =>
        text.Split(token, StringSplitOptions.None).Length - 1;

    private static string LongTable(int dataRows) =>
        "| Name | Description |\n| --- | --- |\n" +
        string.Join("\n", Enumerable.Range(1, dataRows)
            .Select(i => $"| Row-{i:000} | Value-{i:000} |"));

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
            "mdview-long-native-" + Guid.NewGuid().ToString("N"));

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
