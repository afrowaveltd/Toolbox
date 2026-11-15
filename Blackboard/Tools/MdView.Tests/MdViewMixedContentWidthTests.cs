using System.Text;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>
/// Source/index/render parity with optional content columns on wide and
/// narrow terminals; the physical console profile remains untouched.
/// </summary>
public sealed class MdViewMixedContentWidthTests
{
    [Theory]
    [InlineData(220, 0, 220, 218)]
    [InlineData(220, 100, 102, 100)]
    [InlineData(72, 100, 72, 70)]
    [InlineData(220, 40, 42, 40)]
    [InlineData(40, 4096, 40, 38)]
    public void SharedPolicyNeverChangesPhysicalTerminalWidth(
        int physical, int maximum, int expectedEffective, int expectedContent)
    {
        Assert.Equal(expectedEffective,
            MdViewMixedContentWidth.EffectiveTerminalWidth(physical, maximum));
        Assert.Equal(expectedContent,
            MdViewMixedContentWidth.ContentColumns(physical, maximum));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(39)]
    [InlineData(4097)]
    public void InvalidContentWidthIsRejected(int maximum)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MdViewMixedContentWidth.Validate(maximum));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MdViewMixedContentWidth.ContentColumns(180, maximum));
    }

    [Fact]
    public async Task WidthLimitChangesWrappedPageCountButPreservesExactSourceJumps()
    {
        var paragraph = string.Join(" ", Enumerable.Repeat(
            "natural wrapped Markdown prose, and another sentence.", 9));
        using var fixture = new Fixture(string.Join("\n",
            Enumerable.Range(1, 36).Select(i => $"Line {i}: {paragraph}")));
        var original = await File.ReadAllBytesAsync(fixture.Path);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole(220);
        await using var wide = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, contentRows: 16);
        await using var limited = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, contentRows: 16, maximumContentColumns: 60);

        Assert.Equal(220, wide.TerminalWidth);
        Assert.Equal(220, limited.TerminalWidth);
        Assert.Equal(0, wide.MaximumContentColumns);
        Assert.Equal(60, limited.MaximumContentColumns);
        Assert.True(limited.PageCount > wide.PageCount,
            $"Wide pages: {wide.PageCount}; 60-column pages: {limited.PageCount}");

        var jump = await limited.FindPageAsync(25);
        var record = await limited.GetPageAsync(jump);
        Assert.True(record.Start.Line <= 25);
        Assert.True(record.Next.Line >= 25);

        Assert.Equal(string.Empty, console.Output);
        await MdViewMixedVisualIndexedRenderer.RenderPageAsync(
            limited, cursor, console, jump, new MarkdownSpectreTheme());
        Assert.Contains("Line ", console.Output);
        Assert.Equal(220, console.Profile.Width);
        Assert.Equal(original, await File.ReadAllBytesAsync(fixture.Path));
    }

    [Fact]
    public async Task NativeTableUsesSameWidthLimitAsProseAndDiskIndexedRenderer()
    {
        var description = string.Join(" ",
            Enumerable.Repeat("a descriptive text value", 7));
        using var fixture = new Fixture(
            "Introduction\n| Name | Description |\n| --- | --- |\n" +
            "| First | " + description + " |\nFollowing text");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole(200);
        const int maximum = 80;
        const int contentRows = 35;

        var planned = await MdViewMixedVisualPagePlanner.PlanAsync(
            cursor, MdViewMixedVisualPosition.Beginning, console, contentRows,
            maximumContentColumns: maximum);
        var table = Assert.Single(planned.Blocks.OfType<MdViewMixedVisualTableBlock>());
        Assert.Equal(maximum, planned.MaximumContentColumns);
        Assert.InRange(table.Height, 1, contentRows);
        Assert.Equal(table.Height,
            MdViewSpectreTableHeight.CountRows(table.Widget, console));
        Assert.True(planned.AtEnd);
        Assert.Equal(5, planned.Next.Line);

        await using var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, contentRows, maximumContentColumns: maximum);
        Assert.Equal(1, index.PageCount);
        Assert.Equal(maximum, index.MaximumContentColumns);
        var record = await index.GetPageAsync(0);
        Assert.Equal(planned.Rows, record.VisualRows);
        Assert.Equal(planned.Next, record.Next);

        await MdViewMixedVisualIndexedRenderer.RenderPageAsync(
            index, cursor, console, 0, new MarkdownSpectreTheme());
        Assert.Contains("╭", console.Output);
        Assert.Contains("First", console.Output);
        Assert.Contains("Following text", console.Output);
        Assert.Equal(200, console.Profile.Width);
    }

    [Fact]
    public async Task NarrowContentLimitFallsBackForNativeGridWithoutLosingRows()
    {
        const string source =
            "| One | Two |\n| --- | --- |\n| Alpha | Beta |\nEnd";
        using var fixture = new Fixture(source);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole(200);

        var wide = await MdViewMixedVisualPagePlanner.PlanAsync(
            cursor, MdViewMixedVisualPosition.Beginning, console, 30);
        var limited = await MdViewMixedVisualPagePlanner.PlanAsync(
            cursor, MdViewMixedVisualPosition.Beginning, console, 30,
            maximumContentColumns: 40);

        Assert.Single(wide.Blocks.OfType<MdViewMixedVisualTableBlock>());
        Assert.Empty(limited.Blocks.OfType<MdViewMixedVisualTableBlock>());
        Assert.Equal(4, limited.Next.Line);
        Assert.True(limited.AtEnd);
        Assert.Equal(200, console.Profile.Width);
        Assert.Equal(string.Empty, console.Output);

        MdViewMixedVisualPageRenderer.Render(
            console, limited, new MarkdownSpectreTheme());
        Assert.Contains("Alpha", console.Output);
        Assert.Contains("Beta", console.Output);
        Assert.DoesNotContain("╭", console.Output);
    }

    [Fact]
    public async Task PhysicalResizeIsStillRejectedWhenContentWidthCapIsUnchanged()
    {
        using var fixture = new Fixture(
            "| Name | Value |\n| --- | --- |\n| CPU | OK |");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole(180);
        await using var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, console, 30, maximumContentColumns: 80);
        Assert.Equal(80, index.MaximumContentColumns);
        console.Profile.Width = 200;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            MdViewMixedVisualIndexedRenderer.RenderPageAsync(
                index, cursor, console, 0, new MarkdownSpectreTheme()));
        Assert.Equal(string.Empty, console.Output);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(39)]
    [InlineData(4097)]
    public async Task PlannerAndDiskIndexRejectInvalidContentWidth(int maximum)
    {
        using var fixture = new Fixture("Simple Markdown paragraph.");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = MakeConsole(180);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewMixedVisualPagePlanner.PlanAsync(
                cursor, MdViewMixedVisualPosition.Beginning, console, 30,
                maximumContentColumns: maximum));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewMixedVisualPageIndex.CreateAsync(
                cursor, console, 30, maximumContentColumns: maximum));
        Assert.Equal(string.Empty, console.Output);
    }

    [Fact]
    public void CliOptionIsExplicitAndDoesNotChangeOtherCommands()
    {
        var selected = MdViewCommand.Parse(
            "PrEvIeW-NaTiVe", "README.md", "--MAX-WIDTH", "100");
        Assert.Equal(MdViewSourceKind.MixedPreview, selected.Kind);
        Assert.Equal(100, selected.MaximumContentColumns);
        Assert.Equal("README.md", selected.Value);
        Assert.Equal(0, MdViewCommand.Parse(
            "preview-native", "README.md").MaximumContentColumns);
        Assert.Equal(MdViewSourceKind.FormattedPreview,
            MdViewCommand.Parse("preview", "README.md").Kind);
        Assert.Equal(0, MdViewCommand.Parse(
            "preview", "README.md").MaximumContentColumns);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("39")]
    [InlineData("4097")]
    [InlineData("wide")]
    [InlineData("-100")]
    public void InvalidCliWidthIsRejectedBeforeSourceSelection(string value) =>
        Assert.Throws<ArgumentException>(() =>
            MdViewCommand.Parse("preview-native", "README.md",
                "--max-width", value));

    [Fact]
    public async Task OptInPagerDisplaysConfiguredWidthWithoutChangingDefault()
    {
        using var fixture = new Fixture("# Heading\n" +
            string.Join(" ", Enumerable.Repeat(
                "a readable long sentence with clear wrapping.", 18)));
        var console = MakeConsole(180);
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(
            fixture.Path, maximumContentColumns: 70);

        Assert.Contains("max 70 columns", console.Output);
        Assert.Contains("Heading", console.Output);
        Assert.Contains("Page 1 of ", console.Output);
        Assert.Equal(180, console.Profile.Width);
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
            "mdview-width-" + Guid.NewGuid().ToString("N"));

        public Fixture(string source)
        {
            Directory.CreateDirectory(_directory);
            Path = System.IO.Path.Combine(_directory, "README.md");
            File.WriteAllText(Path, source, new UTF8Encoding(false));
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, recursive: true);
        }
    }
}
