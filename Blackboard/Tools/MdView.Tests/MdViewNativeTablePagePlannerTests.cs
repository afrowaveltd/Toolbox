using System.Text;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>
/// Exact Spectre row geometry, source-bound continuation and compact
/// fallback tests for the future in-page native table layout.
/// </summary>
public sealed class MdViewNativeTablePagePlannerTests
{
    [Fact]
    public async Task SixRowFragmentsResumeWithMeasuredHeightAndNoMissingRows()
    {
        var markdown = "| Name | Value |\n| :--- | ---: |\n" +
            string.Join("\n", Enumerable.Range(1, 14)
                .Select(i => "| Item " + i + " | " + i + " |"));
        using var fixture = new Fixture(markdown);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole(96);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);

        var first = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 2, console, 80);
        var second = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, first.NextDataLine, console, 80);
        var third = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, second.NextDataLine, console, 80);

        Assert.All(new[] { first, second, third }, fragment =>
        {
            Assert.True(fragment.IsNative);
            Assert.NotNull(fragment.Widget);
            Assert.InRange(fragment.VisualRows, 1, 80);
            Assert.Equal(fragment.VisualRows,
                MdViewSpectreTableHeight.CountRows(fragment.Widget!, console));
        });

        Assert.Equal(6, first.DataRows);
        Assert.Equal(6, second.DataRows);
        Assert.Equal(2, third.DataRows);
        Assert.Equal(8, first.NextDataLine);
        Assert.Equal(14, second.NextDataLine);
        Assert.Equal(16, third.NextDataLine);
        Assert.True(first.HasMore);
        Assert.True(second.HasMore);
        Assert.False(third.HasMore);
        Assert.Equal(string.Empty, console.Output);

        console.Write(second.Widget!);
        Assert.Contains("Item 7", console.Output);
        Assert.Contains("Item 12", console.Output);
        Assert.DoesNotContain("Item 6", console.Output);
        Assert.DoesNotContain("Item 13", console.Output);
    }

    [Fact]
    public async Task ActualRenderedHeightSplitsTableAtAnExactSourceRow()
    {
        var longValue = string.Join(" ",
            Enumerable.Repeat("some long descriptive text", 5));
        using var fixture = new Fixture(
            "| Name | Description |\n| --- | --- |\n" +
            string.Join("\n", Enumerable.Range(1, 5)
                .Select(i => "| Item " + i + " | " + longValue + " |")));
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole(96);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);

        var one = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 2, console, 80, maximumSourceRows: 1);
        Assert.True(one.IsNative);
        var first = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 2, console, one.VisualRows);
        Assert.True(first.IsNative);
        Assert.Equal(1, first.DataRows);
        Assert.Equal(3, first.NextDataLine);
        Assert.True(first.HasMore);
        Assert.Equal(one.VisualRows, first.VisualRows);

        var second = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, first.NextDataLine, console, one.VisualRows);
        Assert.True(second.IsNative);
        Assert.Equal(1, second.DataRows);
        Assert.Equal(4, second.NextDataLine);
        Assert.True(second.HasMore);
        Assert.Equal(string.Empty, console.Output);
    }

    [Fact]
    public async Task NarrowTerminalsRequestCompactFallbackWithoutConsumingSource()
    {
        using var fixture = new Fixture(
            "| Name | Value |\n| --- | --- |\n| A | B |");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);
        var console = MakeConsole(36);

        var planned = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 2, console, 30);

        Assert.False(planned.IsNative);
        Assert.Null(planned.Widget);
        Assert.Equal(MdViewNativeTableFallbackReason.NarrowTerminal,
            planned.FallbackReason);
        Assert.Equal(0, planned.DataRows);
        Assert.Equal(2, planned.NextDataLine);
        Assert.Equal(0, planned.VisualRows);
        Assert.Equal(string.Empty, console.Output);
        Assert.False(MdViewNativeTablePagePlanner.SupportsNativeGrid(36, 2));
        Assert.False(MdViewNativeTablePagePlanner.SupportsNativeGrid(96, 7));
        Assert.True(MdViewNativeTablePagePlanner.SupportsNativeGrid(96, 6));
    }

    [Fact]
    public async Task InsufficientRowsForHeaderOrFirstRowPreserveResumePoint()
    {
        var markdown = "| Name | Value |\n| --- | --- |\n| First | Data |";
        using var fixture = new Fixture(markdown);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);
        var console = MakeConsole(96);

        var tooSmall = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 2, console, 1);
        Assert.Equal(MdViewNativeTableFallbackReason.HeaderTooTall,
            tooSmall.FallbackReason);
        Assert.Equal(2, tooSmall.NextDataLine);
        Assert.Null(tooSmall.Widget);

        // A header-only table establishes the exact native base height,
        // independently of Spectre's current border implementation.
        using var headerFixture = new Fixture(
            "| Name | Value |\n| --- | --- |");
        await using var headerSnapshot = await Utf8ValidatedSnapshot.CreateAsync(
            headerFixture.Path);
        await using var headerCursor = new MdViewContainerPageCursor(headerSnapshot);
        var emptyHeader = await MdViewSpectreTableBlockReader.ReadHeaderAsync(
            headerCursor, 0);
        Assert.NotNull(emptyHeader);
        var headerOnly = await MdViewNativeTablePagePlanner.PlanAsync(
            headerCursor, emptyHeader, 2, console, 80);
        Assert.True(headerOnly.IsNative);
        Assert.Equal(0, headerOnly.DataRows);

        var noRowSpace = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 2, console, headerOnly.VisualRows);
        Assert.False(noRowSpace.IsNative);
        Assert.Equal(MdViewNativeTableFallbackReason.FirstRowTooTall,
            noRowSpace.FallbackReason);
        Assert.Equal(2, noRowSpace.NextDataLine);
        Assert.Equal(0, noRowSpace.DataRows);
    }

    [Fact]
    public async Task UntrustedMarkupRemainsLiteralAndSourcesStayReadOnly()
    {
        const string markdown =
            "| [red]Title[/] | Status |\n| --- | --- |\n" +
            "| [green]not a command[/] | OK |";
        using var fixture = new Fixture(markdown);
        var before = await File.ReadAllBytesAsync(fixture.Path);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);
        var console = MakeConsole(96);

        var planned = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 2, console, 30);
        Assert.True(planned.IsNative);
        Assert.Equal(string.Empty, console.Output);

        console.Write(planned.Widget!);
        Assert.Contains("[red]Title[/]", console.Output);
        Assert.Contains("[green]not a command[/]", console.Output);
        Assert.Equal(before, await File.ReadAllBytesAsync(fixture.Path));
    }

    [Fact]
    public async Task InvalidBudgetsMissingContinuationAndCancellationAreSafe()
    {
        using var fixture = new Fixture(
            "| Name | Value |\n| --- | --- |\n| First | One |\nAfter table");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);
        var console = MakeConsole(96);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewNativeTablePagePlanner.PlanAsync(cursor, header, 1, console, 20));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewNativeTablePagePlanner.PlanAsync(cursor, header, 2, console, 0));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewNativeTablePagePlanner.PlanAsync(cursor, header, 2, console, 20, 7));

        var notAContinuation = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, 3, console, 20);
        Assert.Equal(MdViewNativeTableFallbackReason.NoContinuationRow,
            notAContinuation.FallbackReason);
        Assert.Equal(3, notAContinuation.NextDataLine);
        Assert.Null(notAContinuation.Widget);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            MdViewNativeTablePagePlanner.PlanAsync(
                cursor, header, 2, console, 20,
                cancellationToken: cancellation.Token));
    }

    private static TestConsole MakeConsole(int width)
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
            "mdview-native-page-" + Guid.NewGuid().ToString("N"));

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
