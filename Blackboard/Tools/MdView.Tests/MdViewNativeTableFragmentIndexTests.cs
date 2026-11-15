using System.Text;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>
/// Disk record, binary-search jump, source continuation and table-only
/// renderer parity tests using validated local Markdown snapshots.
/// </summary>
public sealed class MdViewNativeTableFragmentIndexTests
{
    [Fact]
    public async Task DiskIndexResumesFourteenRowsAndFindsSourceLinesByBinarySearch()
    {
        using var fixture = new Fixture(
            "Introduction\n| Name | Value |\n| :--- | ---: |\n" +
            string.Join("\n", Enumerable.Range(1, 14)
                .Select(i => "| Item " + i + " | " + i + " |")) +
            "\nAfter table");
        var original = await File.ReadAllBytesAsync(fixture.Path);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 1);
        Assert.NotNull(header);
        var console = MakeConsole();
        await using var index = await MdViewNativeTableFragmentIndex.CreateAsync(
            cursor, header, console, contentRows: 60);

        Assert.True(index.IsNative);
        Assert.Equal(MdViewNativeTableFallbackReason.None, index.FallbackReason);
        Assert.Equal(3, index.FragmentCount);
        Assert.Equal(14, index.SourceDataRows);
        Assert.Equal(17, index.NextDataLine);
        Assert.Equal(60, index.ContentRows);
        Assert.Equal(96, index.TerminalWidth);
        Assert.Equal(100, index.TerminalHeight);

        var first = await index.GetFragmentAsync(0);
        var second = await index.GetFragmentAsync(1);
        var third = await index.GetFragmentAsync(2);
        Assert.Equal((3L, 9L, 6, true),
            (first.FirstDataLine, first.NextDataLine, first.DataRows, first.HasMore));
        Assert.Equal((9L, 15L, 6, true),
            (second.FirstDataLine, second.NextDataLine, second.DataRows, second.HasMore));
        Assert.Equal((15L, 17L, 2, false),
            (third.FirstDataLine, third.NextDataLine, third.DataRows, third.HasMore));
        Assert.All(new[] { first, second, third }, fragment =>
            Assert.InRange(fragment.VisualRows, 1, index.ContentRows));

        Assert.Null(await index.FindFragmentAsync(0));
        Assert.Equal(0, await index.FindFragmentAsync(1));
        Assert.Equal(0, await index.FindFragmentAsync(2));
        Assert.Equal(0, await index.FindFragmentAsync(8));
        Assert.Equal(1, await index.FindFragmentAsync(9));
        Assert.Equal(1, await index.FindFragmentAsync(14));
        Assert.Equal(2, await index.FindFragmentAsync(15));
        Assert.Equal(2, await index.FindFragmentAsync(16));
        Assert.Null(await index.FindFragmentAsync(17));
        Assert.Equal(string.Empty, console.Output);
        Assert.Equal(original, await File.ReadAllBytesAsync(fixture.Path));
    }

    [Fact]
    public async Task IndexedRendererReconstructsOnlyChosenFragment()
    {
        using var fixture = new Fixture(
            "| Product | Count |\n| --- | ---: |\n" +
            string.Join("\n", Enumerable.Range(1, 14)
                .Select(i => "| Item " + i + " | " + i + " |")));
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);
        var console = MakeConsole();
        await using var index = await MdViewNativeTableFragmentIndex.CreateAsync(
            cursor, header, console, contentRows: 80);
        Assert.Equal(string.Empty, console.Output);

        var expected = await index.GetFragmentAsync(1);
        var actual = await MdViewNativeTableFragmentRenderer.RenderAsync(
            index, cursor, header, console, fragmentNumber: 1);

        Assert.Equal(expected, actual);
        Assert.Contains("╭", console.Output);
        Assert.Contains("Item 7", console.Output);
        Assert.Contains("Item 12", console.Output);
        Assert.DoesNotContain("Item 6", console.Output);
        Assert.DoesNotContain("Item 13", console.Output);
        Assert.Equal(14, actual.NextDataLine);
    }

    [Fact]
    public async Task HeaderOnlyTableUsesOneNativeFragmentWithoutData()
    {
        using var fixture = new Fixture("| Name | Value |\n| --- | --- |");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);
        var console = MakeConsole();
        await using var index = await MdViewNativeTableFragmentIndex.CreateAsync(
            cursor, header, console, 30);

        Assert.True(index.IsNative);
        Assert.Equal(1, index.FragmentCount);
        Assert.Equal(0, index.SourceDataRows);
        Assert.Equal(2, index.NextDataLine);
        var fragment = await index.GetFragmentAsync(0);
        Assert.Equal(0, fragment.DataRows);
        Assert.True(fragment.VisualRows > 0);
        Assert.False(fragment.HasMore);
        Assert.Equal(0, await index.FindFragmentAsync(0));
        Assert.Equal(0, await index.FindFragmentAsync(1));
        Assert.Null(await index.FindFragmentAsync(2));

        await MdViewNativeTableFragmentRenderer.RenderAsync(
            index, cursor, header, console, 0);
        Assert.Contains("Name", console.Output);
        Assert.Contains("Value", console.Output);
    }

    [Fact]
    public async Task NarrowTerminalFallsBackForEntireTableWithoutWrites()
    {
        using var fixture = new Fixture(
            "| Name | Value |\n| --- | --- |\n| First | One |");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);
        var console = MakeConsole(width: 36);
        await using var index = await MdViewNativeTableFragmentIndex.CreateAsync(
            cursor, header, console, 30);

        Assert.False(index.IsNative);
        Assert.Equal(MdViewNativeTableFallbackReason.NarrowTerminal,
            index.FallbackReason);
        Assert.Equal(0, index.FragmentCount);
        Assert.Equal(0, index.SourceDataRows);
        Assert.Equal(2, index.NextDataLine);
        Assert.Null(await index.FindFragmentAsync(2));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            MdViewNativeTableFragmentRenderer.RenderAsync(
                index, cursor, header, console, 0));
        Assert.Equal(string.Empty, console.Output);
    }

    [Fact]
    public async Task TooLittleHeightRequestsHeaderOrWholeTableRowFallback()
    {
        using var fixture = new Fixture(
            "| Name | Value |\n| --- | --- |\n| First | One |");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);
        var console = MakeConsole();

        await using var tooShort = await MdViewNativeTableFragmentIndex.CreateAsync(
            cursor, header, console, 1);
        Assert.False(tooShort.IsNative);
        Assert.Equal(MdViewNativeTableFallbackReason.HeaderTooTall,
            tooShort.FallbackReason);
        Assert.Equal(0, tooShort.FragmentCount);

        using var emptyFixture = new Fixture(
            "| Name | Value |\n| --- | --- |");
        await using var emptySnapshot = await Utf8ValidatedSnapshot.CreateAsync(
            emptyFixture.Path);
        await using var emptyCursor = new MdViewContainerPageCursor(emptySnapshot);
        var emptyHeader = await MdViewSpectreTableBlockReader.ReadHeaderAsync(
            emptyCursor, 0);
        Assert.NotNull(emptyHeader);
        await using var emptyIndex = await MdViewNativeTableFragmentIndex.CreateAsync(
            emptyCursor, emptyHeader, console, 80);
        var onlyHeader = await emptyIndex.GetFragmentAsync(0);

        await using var firstRowCannotFit =
            await MdViewNativeTableFragmentIndex.CreateAsync(
                cursor, header, console, onlyHeader.VisualRows);
        Assert.False(firstRowCannotFit.IsNative);
        Assert.Equal(MdViewNativeTableFallbackReason.FirstRowTooTall,
            firstRowCannotFit.FallbackReason);
        Assert.Equal(0, firstRowCannotFit.FragmentCount);
        Assert.Equal(2, firstRowCannotFit.NextDataLine);
    }

    [Fact]
    public async Task ExactlyMeasuredOneRowPagesRepeatHeaderAndKeepAllRows()
    {
        var description = string.Join(" ",
            Enumerable.Repeat("long descriptive value", 6));
        using var fixture = new Fixture(
            "| Name | Description |\n| --- | --- |\n" +
            string.Join("\n", Enumerable.Range(1, 5)
                .Select(i => "| Item " + i + " | " + description + " |")));
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);
        var console = MakeConsole();
        var oneRow = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 2, console, 100, maximumSourceRows: 1);
        Assert.True(oneRow.IsNative);

        await using var index = await MdViewNativeTableFragmentIndex.CreateAsync(
            cursor, header, console, oneRow.VisualRows);

        Assert.True(index.IsNative);
        Assert.Equal(5, index.FragmentCount);
        Assert.Equal(5, index.SourceDataRows);
        Assert.Equal(7, index.NextDataLine);
        for (var i = 0; i < 5; i++)
        {
            var fragment = await index.GetFragmentAsync(i);
            Assert.Equal(2L + i, fragment.FirstDataLine);
            Assert.Equal(3L + i, fragment.NextDataLine);
            Assert.Equal(1, fragment.DataRows);
            Assert.Equal(oneRow.VisualRows, fragment.VisualRows);
            Assert.Equal(i != 4, fragment.HasMore);
        }
        Assert.Equal(string.Empty, console.Output);
    }

    [Fact]
    public async Task LateOverHeightDataRowRollsBackTheEntireNativeIndex()
    {
        var longCell = string.Join(" ",
            Enumerable.Repeat("long descriptive text", 12));
        using var fixture = new Fixture(
            "| Name | Description |\n| --- | --- |\n" +
            "| A | Short |\n| B | Short |\n| C | Short |\n" +
            "| D | " + longCell + " |");
        var before = await File.ReadAllBytesAsync(fixture.Path);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);
        var console = MakeConsole(width: 80);

        var first = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 2, console, 100, maximumSourceRows: 1);
        var last = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 5, console, 100, maximumSourceRows: 1);
        Assert.True(first.IsNative);
        Assert.True(last.IsNative);
        Assert.True(last.VisualRows > first.VisualRows,
            $"Short row: {first.VisualRows}, long row: {last.VisualRows}");

        await using var index = await MdViewNativeTableFragmentIndex.CreateAsync(
            cursor, header, console, first.VisualRows);

        Assert.False(index.IsNative);
        Assert.Equal(MdViewNativeTableFallbackReason.FirstRowTooTall,
            index.FallbackReason);
        Assert.Equal(0, index.FragmentCount);
        Assert.Equal(0, index.SourceDataRows);
        Assert.Equal(2, index.NextDataLine);
        Assert.Null(await index.FindFragmentAsync(3));
        Assert.Equal(before, await File.ReadAllBytesAsync(fixture.Path));
        Assert.Equal(string.Empty, console.Output);
    }

    [Fact]
    public async Task DiskBudgetFailureDoesNotChangeValidatedMarkdown()
    {
        using var fixture = new Fixture(
            "| Name | Value |\n| --- | --- |\n" +
            string.Join("\n", Enumerable.Range(1, 19)
                .Select(i => "| Item " + i + " | " + i + " |")));
        var before = await File.ReadAllBytesAsync(fixture.Path);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);
        var console = MakeConsole();

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            MdViewNativeTableFragmentIndex.CreateAsync(cursor, header, console,
                80, maximumIndexBytes: 2 * MdViewNativeTableFragmentIndex.RecordBytes));
        Assert.Equal(before, await File.ReadAllBytesAsync(fixture.Path));
        Assert.Equal(string.Empty, console.Output);
    }

    [Fact]
    public async Task ForeignHeaderAndInvalidIndexArgumentsAreRejected()
    {
        using var fixture = new Fixture(
            "| Name | Value |\n| --- | --- |\n| First | One |");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);
        var console = MakeConsole();
        var foreign = new MdViewSpectreTableHeader(0,
            ["Another", "Value"], header.Separators);

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            MdViewNativeTableFragmentIndex.CreateAsync(cursor, foreign, console, 30));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewNativeTableFragmentIndex.CreateAsync(cursor, header, console, 0));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewNativeTableFragmentIndex.CreateAsync(cursor, header, console, 30,
                maximumIndexBytes: MdViewNativeTableFragmentIndex.RecordBytes - 1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewNativeTableFragmentIndex.CreateAsync(cursor, header, console, 101));

        await using var index = await MdViewNativeTableFragmentIndex.CreateAsync(
            cursor, header, console, 30);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            index.GetFragmentAsync(-1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            index.GetFragmentAsync(index.FragmentCount));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            index.FindFragmentAsync(-1));
    }

    [Fact]
    public async Task ResizedTerminalRefusesStaleIndexBeforeAnyOutput()
    {
        using var fixture = new Fixture(
            "| Name | Value |\n| --- | --- |\n| First | One |");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);
        var console = MakeConsole();
        await using var index = await MdViewNativeTableFragmentIndex.CreateAsync(
            cursor, header, console, 30);
        console.Profile.Width = 100;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            MdViewNativeTableFragmentRenderer.RenderAsync(
                index, cursor, header, console, 0));
        Assert.Equal(string.Empty, console.Output);
    }

    [Fact]
    public async Task NativeRendererRejectsSubstitutedHeaderAndCursorBeforeWriting()
    {
        using var fixture = new Fixture(
            "| Name | Value |\n| --- | --- |\n| First | One |");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        await using var otherCursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);
        var console = MakeConsole();
        await using var index = await MdViewNativeTableFragmentIndex.CreateAsync(
            cursor, header, console, 30);

        var swapped = new MdViewSpectreTableHeader(0,
            ["Fake", "Value"], header.Separators);
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            MdViewNativeTableFragmentRenderer.RenderAsync(
                index, cursor, swapped, console, 0));
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            MdViewNativeTableFragmentRenderer.RenderAsync(
                index, otherCursor, header, console, 0));
        Assert.Equal(string.Empty, console.Output);
    }

    [Fact]
    public async Task CancellationAndDisposalPreventStaleReadsOrRendering()
    {
        using var fixture = new Fixture(
            "| Name | Value |\n| --- | --- |\n| First | One |");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);
        var console = MakeConsole();
        using var cancel = new CancellationTokenSource();
        cancel.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            MdViewNativeTableFragmentIndex.CreateAsync(
                cursor, header, console, 30, cancellationToken: cancel.Token));

        var index = await MdViewNativeTableFragmentIndex.CreateAsync(
            cursor, header, console, 30);
        try
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                MdViewNativeTableFragmentRenderer.RenderAsync(
                    index, cursor, header, console, 0, cancel.Token));
        }
        finally
        {
            await index.DisposeAsync();
        }

        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            index.GetFragmentAsync(0));
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            index.FindFragmentAsync(2));
        Assert.Equal(string.Empty, console.Output);
    }

    private static TestConsole MakeConsole(int width = 96)
    {
        var console = new TestConsole();
        console.Profile.Width = width;
        console.Profile.Height = 100;
        console.Profile.Capabilities.Unicode = true;
        console.Profile.Capabilities.Interactive = true;
        return console;
    }

    private sealed class Fixture : IDisposable
    {
        private readonly string _folder = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "mdview-native-index-" + Guid.NewGuid().ToString("N"));

        public Fixture(string markdown)
        {
            Directory.CreateDirectory(_folder);
            Path = System.IO.Path.Combine(_folder, "README.md");
            File.WriteAllText(Path, markdown, new UTF8Encoding(false));
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(_folder))
                Directory.Delete(_folder, recursive: true);
        }
    }
}
