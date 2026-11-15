using System.Text;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>
/// Full-screen mixed preview alignment: document, chrome and interactive
/// Spectre navigation share the same column without altering page geometry.
/// </summary>
public sealed class MdViewMixedScreenAlignmentTests
{
    [Fact]
    public async Task CenteredHeaderFooterMenuAndMarkdownShareOneLeftEdge()
    {
        using var fixture = new Fixture(string.Join("\n",
            Enumerable.Range(1, 15).Select(i => "Readable paragraph " + i)));
        var console = Interactive(180);
        const int maximum = 80;
        var gutter = new string(' ', 49);
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(
            fixture.Path, linesPerPage: 6,
            maximumContentColumns: maximum, centerContent: true);

        Assert.Contains(gutter + "mdview — mixed Markdown", console.Output);
        Assert.Contains(gutter + "README.md |", console.Output);
        Assert.Contains(gutter + "Readable paragraph 1", console.Output);
        Assert.Contains(gutter + "Page 1 of ", console.Output);
        Assert.Contains(gutter + "Mixed preview navigation", console.Output);
        Assert.Contains(gutter + "> Next page", console.Output);
        Assert.DoesNotContain("\nmdview — mixed Markdown", console.Output);
        Assert.Equal(180, console.Profile.Width);
    }

    [Fact]
    public async Task CenteredSelectionKeepsNextAndPreviousKeyboardNavigation()
    {
        using var fixture = new Fixture(string.Join("\n",
            Enumerable.Range(1, 14).Select(i => "Item " + i)));
        var console = Interactive(180);
        var gutter = new string(' ', 54);
        console.Input.PushKey(ConsoleKey.Enter);     // Next.
        console.Input.PushKey(ConsoleKey.DownArrow); // Previous.
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(
            fixture.Path, linesPerPage: 2,
            maximumContentColumns: 70, centerContent: true);

        Assert.Contains(gutter + "Page 1 of 7", console.Output);
        Assert.Contains(gutter + "Page 2 of 7", console.Output);
        Assert.Contains(gutter + "Previous page", console.Output);
        Assert.Equal(2, console.Output.Split(
            "Page 1 of 7", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public async Task CenteredSourceLinePromptIndentsTitleButNotEachTypedCharacter()
    {
        using var fixture = new Fixture(string.Join("\n",
            Enumerable.Range(1, 20).Select(i => "Physical item " + i)));
        var console = Interactive(180);
        var gutter = new string(' ', 49);
        console.Input.PushKey(ConsoleKey.DownArrow); // Source line.
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushTextWithEnter("12");
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(
            fixture.Path, linesPerPage: 5,
            maximumContentColumns: 80, centerContent: true);

        Assert.Contains(gutter + "Physical source line (1-based", console.Output);
        Assert.Contains(gutter + "Source line 12 is on this canonical page.",
            console.Output);
        Assert.Contains(gutter + "Page 3 of 4", console.Output);
        Assert.DoesNotContain("Source line is outside this document.",
            console.Output);
    }

    [Fact]
    public async Task CenteredIndexedHeadingPickerUsesSameSelectionArrowGutter()
    {
        var source = Enumerable.Range(0, 40)
            .Select(i => "ordinary line " + (i + 1)).ToArray();
        source[0] = "# Start";
        source[32] = "## Distant heading";
        using var fixture = new Fixture(string.Join("\n", source));
        var console = Interactive(180);
        var gutter = new string(' ', 49);
        console.Input.PushKey(ConsoleKey.DownArrow); // Source line.
        console.Input.PushKey(ConsoleKey.DownArrow); // Heading picker.
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.DownArrow); // Second heading.
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(
            fixture.Path, linesPerPage: 8,
            maximumContentColumns: 80, centerContent: true);

        Assert.Contains(gutter + "Mixed preview navigation", console.Output);
        Assert.Contains(gutter + "Indexed headings", console.Output);
        Assert.Contains(gutter + "Source line 33 is on this canonical page.",
            console.Output);
        Assert.Contains(gutter + "Distant heading", console.Output);
        Assert.Contains(gutter + "Page 5 of 5", console.Output);
    }

    [Fact]
    public async Task CenteredIndexedLinkPickerKeepsSelectedLocationOnCanonicalPage()
    {
        var lines = Enumerable.Range(0, 32)
            .Select(i => "ordinary line " + (i + 1)).ToArray();
        lines[23] = "[Installation](Docs/Installation/en.md)";
        using var fixture = new Fixture(string.Join("\n", lines));
        var console = Interactive(180);
        var gutter = new string(' ', 49);
        console.Input.PushKey(ConsoleKey.DownArrow); // Source line.
        console.Input.PushKey(ConsoleKey.DownArrow); // Heading picker.
        console.Input.PushKey(ConsoleKey.DownArrow); // Link picker.
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Enter);     // Indexed link.
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(
            fixture.Path, linesPerPage: 8,
            maximumContentColumns: 80, centerContent: true);

        Assert.Contains(gutter + "Indexed links", console.Output);
        Assert.Contains(gutter + "Source line 24 is on this canonical page.",
            console.Output);
        Assert.Contains(gutter + "Page 3 of 4", console.Output);
        Assert.Contains(gutter + "Installation", console.Output);
    }

    [Fact]
    public async Task CenteredEmptyHeadingSearchMessageUsesReadingColumn()
    {
        using var fixture = new Fixture("Text without any headings.");
        var console = Interactive(180);
        var gutter = new string(' ', 49);
        console.Input.PushKey(ConsoleKey.DownArrow); // Heading picker.
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(
            fixture.Path, maximumContentColumns: 80, centerContent: true);

        Assert.Contains(gutter + "No indexed headings were found.",
            console.Output);
        Assert.Contains(gutter + "Page 1 of 1", console.Output);
    }

    [Fact]
    public async Task CenteredEmptyDocumentAndExitAreInsideReadingColumn()
    {
        using var fixture = new Fixture("");
        var console = Interactive(180);
        var gutter = new string(' ', 49);
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(
            fixture.Path, maximumContentColumns: 80, centerContent: true);

        Assert.Contains(gutter + "(Empty page)", console.Output);
        Assert.Contains(gutter + "Page 1 of 1", console.Output);
        Assert.Contains(gutter + "Mixed preview navigation", console.Output);
        Assert.Contains(gutter + "Exit mixed preview", console.Output);
    }

    [Fact]
    public async Task CenterFlagDoesNotIndentChromeWithoutSpareColumns()
    {
        using var fixture = new Fixture("Simple document.");
        var console = Interactive(72);
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(
            fixture.Path, maximumContentColumns: 100, centerContent: true);

        Assert.True(AtColumnZero(console.Output, "mdview — mixed Markdown"));
        Assert.True(AtColumnZero(console.Output, "Page 1 of 1"));
        Assert.True(AtColumnZero(console.Output, "Mixed preview navigation"));
        Assert.Equal(72, console.Profile.Width);
    }

    [Fact]
    public async Task UncenteredDefaultKeepsOriginalLeftAlignedChrome()
    {
        using var fixture = new Fixture("Simple document.");
        var console = Interactive(180);
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(
            fixture.Path, maximumContentColumns: 80);

        Assert.True(AtColumnZero(console.Output, "mdview — mixed Markdown"));
        Assert.True(AtColumnZero(console.Output, "Page 1 of 1"));
        Assert.True(AtColumnZero(console.Output, "Mixed preview navigation"));
        Assert.DoesNotContain(new string(' ', 49) + "Mixed preview navigation",
            console.Output);
    }

    private static bool AtColumnZero(string output, string prefix) =>
        output.Split('\n').Any(line =>
            line.TrimEnd('\r').StartsWith(
                prefix, StringComparison.Ordinal));

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
            "mdview-screen-" + Guid.NewGuid().ToString("N"));

        public Fixture(string text)
        {
            Directory.CreateDirectory(_directory);
            Path = System.IO.Path.Combine(_directory, "README.md");
            File.WriteAllText(Path, text, new UTF8Encoding(false));
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, recursive: true);
        }
    }
}
