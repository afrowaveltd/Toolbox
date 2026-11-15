using System.Text;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>
/// Optional centered reading-column geometry, literal source rendering and
/// parity between measured native tables and indexed mixed pages.
/// </summary>
public sealed class MdViewMixedContentAlignmentTests
{
    [Theory]
    [InlineData(200, 100, true, 49)]
    [InlineData(200, 100, false, 0)]
    [InlineData(200, 0, true, 0)]
    [InlineData(72, 100, true, 0)]
    [InlineData(200, 40, true, 79)]
    [InlineData(101, 40, true, 29)]
    [InlineData(40, 4096, true, 0)]
    public void LeftGutterCentersReadingColumnWithoutResizingTerminal(
        int physical, int maximum, bool centered, int expected)
    {
        Assert.Equal(expected,
            MdViewMixedContentAlignment.LeftGutter(
                physical, maximum, centered));
    }

    [Fact]
    public void NativeIndentRejectsNegativeGuttersAndPreservesZeroIdentity()
    {
        // The argument validation does not depend on table columns or
        // data rows; use the smallest native table to avoid extensions.
        var table = new Spectre.Console.Table();
        Assert.Same(table, MdViewMixedContentAlignment.IndentTable(table, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = MdViewMixedContentAlignment.IndentTable(table, -1);
        });
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = MdViewMixedContentAlignment.IndentTable(null!, 5);
        });
    }

    [Fact]
    public async Task CenteredNativeTableKeepsExactMeasuredHeightAndOriginalSource()
    {
        const string markdown =
            "Introduction\n| Name | Value |\n| --- | --- |\n" +
            "| [red]literal[/] | 42 |\nFollowing text";
        using var fixture = new Fixture(markdown);
        var before = await File.ReadAllBytesAsync(fixture.Path);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var left = Interactive(width: 180);
        var centered = Interactive(width: 180);
        const int maximum = 80;

        var page = await MdViewMixedVisualPagePlanner.PlanAsync(
            cursor, MdViewMixedVisualPosition.Beginning, left,
            contentRows: 40, maximumContentColumns: maximum);
        var native = Assert.Single(page.Blocks.OfType<MdViewMixedVisualTableBlock>());
        var gutter = MdViewMixedContentAlignment.LeftGutter(
            left.Profile.Width, maximum, centerContent: true);
        Assert.Equal(49, gutter);
        Assert.Equal(native.Height,
            MdViewSpectreTableHeight.CountRows(
                MdViewMixedContentAlignment.IndentTable(native.Widget, gutter),
                centered, 128));
        Assert.Equal(string.Empty, centered.Output);

        var leftResume = MdViewMixedVisualPageRenderer.Render(
            left, page, new MarkdownSpectreTheme());
        var centeredResume = MdViewMixedVisualPageRenderer.Render(
            centered, page, new MarkdownSpectreTheme(), centerContent: true);

        Assert.Equal(leftResume, centeredResume);
        Assert.Equal(page.Next, centeredResume);
        Assert.Equal(180, centered.Profile.Width);
        Assert.Contains("╭", centered.Output);
        Assert.Contains(new string(' ', gutter) + "╭", centered.Output);
        Assert.Contains(new string(' ', gutter) + "Introduction",
            centered.Output);
        Assert.Contains("[red]literal[/]", centered.Output);
        Assert.Contains("Following text", centered.Output);
        Assert.Equal(before, await File.ReadAllBytesAsync(fixture.Path));
    }

    [Fact]
    public async Task IndexedCenteredOutputUsesTheSameCanonicalPageCountAndJumps()
    {
        var markdown = string.Join("\n", Enumerable.Range(1, 36)
            .Select(i => i == 22
                ? "| Key | Value |"
                : i == 23
                    ? "| --- | --- |"
                    : i == 24
                        ? "| CPU | 16 |"
                        : "Section " + i + ". " +
                          string.Join(" ", Enumerable.Repeat(
                              "Natural Markdown reading text.", 5))));
        using var fixture = new Fixture(markdown);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var left = Interactive(width: 180);
        var center = Interactive(width: 180);

        await using var index = await MdViewMixedVisualPageIndex.CreateAsync(
            cursor, left, contentRows: 12, maximumContentColumns: 70);
        Assert.True(index.PageCount > 2);
        var pageNumber = await index.FindPageAsync(23);
        var expected = await index.GetPageAsync(pageNumber);

        var first = await MdViewMixedVisualIndexedRenderer.RenderPageAsync(
            index, cursor, left, pageNumber, new MarkdownSpectreTheme());
        var second = await MdViewMixedVisualIndexedRenderer.RenderPageAsync(
            index, cursor, center, pageNumber, new MarkdownSpectreTheme(),
            centerContent: true);

        Assert.Equal(expected, first);
        Assert.Equal(expected, second);
        Assert.Equal(first.VisualRows, second.VisualRows);
        Assert.Equal(180, center.Profile.Width);
        Assert.Equal(70, index.MaximumContentColumns);
        Assert.Equal(54, MdViewMixedContentAlignment.LeftGutter(
            center.Profile.Width, 70, centerContent: true));
        Assert.Contains("CPU", center.Output);
    }

    [Fact]
    public async Task CenteringWithoutAWidthCapHasIdenticalOutput()
    {
        using var fixture = new Fixture(
            "# Heading\nSimple source-aware Markdown sentence.");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var left = Interactive(width: 120);
        var center = Interactive(width: 120);
        var page = await MdViewMixedVisualPagePlanner.PlanAsync(
            cursor, MdViewMixedVisualPosition.Beginning, left, 20);

        MdViewMixedVisualPageRenderer.Render(
            left, page, new MarkdownSpectreTheme());
        MdViewMixedVisualPageRenderer.Render(
            center, page, new MarkdownSpectreTheme(), centerContent: true);

        Assert.Equal(left.Output, center.Output);
        Assert.Equal(0, MdViewMixedContentAlignment.LeftGutter(
            120, 0, centerContent: true));
    }

    [Fact]
    public void ExplicitCenterCliCanCombineWithMaxWidthInEitherOrder()
    {
        var first = MdViewCommand.Parse(
            "PREVIEW-NATIVE", "README.md",
            "--max-width", "100", "--center");
        var second = MdViewCommand.Parse(
            "preview-native", "README.md",
            "--CENTER", "--MAX-WIDTH", "100");
        Assert.Equal(first, second);
        Assert.Equal(MdViewSourceKind.MixedPreview, first.Kind);
        Assert.Equal(100, first.MaximumContentColumns);
        Assert.True(first.CenterContent);
        Assert.Equal("README.md", first.Value);
        Assert.False(MdViewCommand.Parse(
            "preview-native", "README.md").CenterContent);
        Assert.True(MdViewCommand.Parse(
            "preview-native", "README.md", "--center").CenterContent);
        Assert.False(MdViewCommand.Parse(
            "preview", "README.md").CenterContent);
    }

    [Theory]
    [InlineData("--center --center")]
    [InlineData("--max-width 100 --max-width 80")]
    [InlineData("--unknown 100")]
    [InlineData("--center --unknown")]
    public void UnknownAndDuplicateCenterOptionsAreRejected(string options)
    {
        var args = new[] { "preview-native", "README.md" }
            .Concat(options.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToArray();
        Assert.Throws<ArgumentException>(() => MdViewCommand.Parse(args));
    }

    [Fact]
    public async Task OptInPagerIdentifiesCenteredWidthWithoutChangingPhysicalProfile()
    {
        using var fixture = new Fixture(
            "Introduction\n| Name | Value |\n| --- | --- |\n| CPU | OK |");
        var console = Interactive(width: 180);
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(
            fixture.Path, maximumContentColumns: 80, centerContent: true);

        Assert.Contains("max 80 columns", console.Output);
        Assert.Contains("centered", console.Output);
        Assert.Contains("Page 1 of 1", console.Output);
        Assert.Contains("CPU", console.Output);
        Assert.Equal(180, console.Profile.Width);
    }

    private static TestConsole Interactive(int width)
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
            "mdview-center-" + Guid.NewGuid().ToString("N"));

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
