using System.Text;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>
/// Deterministic journal request-count regressions for native Spectre
/// fragment planning over bounded disk-backed source windows.
/// </summary>
public sealed class MdViewNativeTableBatchReadTests
{
    [Fact]
    public async Task SixRowsAndLookaheadRequireOneJournalReadRequestPerFragment()
    {
        using var fixture = new Fixture(Table(14) + "\nAfter the table");
        var bytes = await File.ReadAllBytesAsync(fixture.Path);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole();
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);

        var before = cursor.ReadRequestCount;
        var first = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 2, console, 60);
        var second = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, first.NextDataLine, console, 60);
        var third = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, second.NextDataLine, console, 60);

        Assert.Equal(3, cursor.ReadRequestCount - before);
        Assert.All(new[] { first, second, third }, x => Assert.True(x.IsNative));
        Assert.Equal(new[] { 6, 6, 2 },
            new[] { first.DataRows, second.DataRows, third.DataRows });
        Assert.Equal(new[] { 8L, 14L, 16L },
            new[] { first.NextDataLine, second.NextDataLine, third.NextDataLine });
        Assert.True(first.HasMore);
        Assert.True(second.HasMore);
        Assert.False(third.HasMore);
        Assert.Equal("After the table",
            (await cursor.ReadAsync(16, 1)).Single().Source.Text);
        Assert.Equal(bytes, await File.ReadAllBytesAsync(fixture.Path));
        Assert.Equal(string.Empty, console.Output);
    }

    [Fact]
    public async Task HeightLimitedFragmentKeepsFirstRejectedSourceRowForNextPage()
    {
        var longCell = string.Join(" ",
            Enumerable.Repeat("description with several wrapped words", 5));
        using var fixture = new Fixture(
            "| Name | Description |\n| --- | --- |\n" +
            string.Join("\n", Enumerable.Range(1, 5)
                .Select(i => $"| Item {i} | {longCell} |")));
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole();
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);

        var single = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 2, console, 80, maximumSourceRows: 1);
        Assert.True(single.IsNative);
        var before = cursor.ReadRequestCount;
        var first = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 2, console, single.VisualRows);
        Assert.True(first.IsNative);
        Assert.Equal(1, cursor.ReadRequestCount - before);
        Assert.Equal(1, first.DataRows);
        Assert.Equal(3, first.NextDataLine);
        Assert.True(first.HasMore);

        before = cursor.ReadRequestCount;
        var continuation = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, first.NextDataLine, console, single.VisualRows);
        Assert.Equal(1, cursor.ReadRequestCount - before);
        Assert.True(continuation.IsNative);
        Assert.Equal(3L, continuation.FirstDataLine);
        Assert.Equal(4L, continuation.NextDataLine);
        Assert.Equal(1, continuation.DataRows);
        Assert.Equal(string.Empty, console.Output);
    }

    [Fact]
    public async Task EndOfTableAndMissingContinuationUseOneBoundedRequestEach()
    {
        using var fixture = new Fixture(Table(1) +
            "\nFollowing paragraph\nAnother paragraph");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole();
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);

        var before = cursor.ReadRequestCount;
        var last = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 2, console, 30);
        Assert.Equal(1, cursor.ReadRequestCount - before);
        Assert.True(last.IsNative);
        Assert.Equal(1, last.DataRows);
        Assert.Equal(3L, last.NextDataLine);
        Assert.False(last.HasMore);

        before = cursor.ReadRequestCount;
        var missing = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 3, console, 30);
        Assert.Equal(1, cursor.ReadRequestCount - before);
        Assert.False(missing.IsNative);
        Assert.Equal(0, missing.DataRows);
        Assert.Equal(3L, missing.NextDataLine);
        Assert.Equal(MdViewNativeTableFallbackReason.NoContinuationRow,
            missing.FallbackReason);
        Assert.Equal("Following paragraph",
            (await cursor.ReadAsync(3, 1)).Single().Source.Text);
    }

    [Fact]
    public async Task LongTablePreflightUsesOneBatchPerMeasuredFragment()
    {
        using var fixture = new Fixture(Table(14) + "\nAfter");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);
        var console = MakeConsole();

        var before = cursor.ReadRequestCount;
        var eligible = await MdViewLongNativeTableEligibility
            .CanRenderEntireTableAsync(cursor, header, console, 40);
        Assert.True(eligible);
        // Six + six + two physical data rows require exactly three
        // native fragments, each with one journal window read.
        Assert.Equal(3, cursor.ReadRequestCount - before);
        Assert.Equal(string.Empty, console.Output);
    }

    [Fact]
    public async Task NativeFallbackAndPrecancelledRequestsDoNotAccessJournal()
    {
        using var fixture = new Fixture(Table(6));
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);

        var narrow = MakeConsole(width: 36);
        var before = cursor.ReadRequestCount;
        var fallback = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 2, narrow, 30);
        Assert.Equal(MdViewNativeTableFallbackReason.NarrowTerminal,
            fallback.FallbackReason);
        Assert.Equal(before, cursor.ReadRequestCount);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            MdViewNativeTablePagePlanner.PlanAsync(
                cursor, header, 2, MakeConsole(), 30,
                cancellationToken: cancellation.Token));
        Assert.Equal(before, cursor.ReadRequestCount);

        // Pre-cancelled attempts do not poison the source journal.
        var valid = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 2, MakeConsole(), 30);
        Assert.True(valid.IsNative);
        Assert.Equal(before + 1, cursor.ReadRequestCount);
    }

    private static string Table(int dataRows) =>
        "| Name | Value |\n| --- | --- |\n" +
        string.Join("\n", Enumerable.Range(1, dataRows)
            .Select(i => $"| Row-{i:000} | Value-{i:000} |"));

    private static TestConsole MakeConsole(int width = 96)
    {
        var console = new TestConsole();
        console.Profile.Width = width;
        console.Profile.Height = 100;
        console.Profile.Capabilities.Interactive = true;
        console.Profile.Capabilities.Unicode = true;
        return console;
    }

    private sealed class Fixture : IDisposable
    {
        private readonly string _directory = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "mdview-native-batch-" + Guid.NewGuid().ToString("N"));

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
