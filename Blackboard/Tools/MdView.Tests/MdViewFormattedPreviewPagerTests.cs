using System.Text;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

public sealed class MdViewFormattedPreviewPagerTests
{
    [Fact]
    public async Task RendersHeadingFromValidatedSnapshotAndExitsOnEscape()
    {
        using var fixture = new Fixture();
        var path = await fixture.WriteAsync("# Preview heading\nPlain text");
        var before = await File.ReadAllBytesAsync(path);
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewFormattedPreviewPager(console).RunAsync(path, 2);

        Assert.Contains("experimental Markdown page preview", console.Output);
        Assert.Contains("Preview heading", console.Output);
        Assert.Contains("Plain text", console.Output);
        Assert.Equal(before, await File.ReadAllBytesAsync(path));
    }

    [Fact]
    public async Task NextAndPreviousPreviewPagesKeepSourceNavigation()
    {
        using var fixture = new Fixture();
        var path = await fixture.WriteAsync("# One\nfirst\n# Two\nsecond");
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Enter); // Next on the first page.
        // On the last page there is no Next choice; Previous is first.
        console.Input.PushKey(ConsoleKey.Enter); // Previous
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewFormattedPreviewPager(console).RunAsync(path, 2);

        Assert.Contains("Two", console.Output);
        Assert.Equal(2,
            console.Output.Split("Source lines 1–2", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public async Task GoToSourceLineReadsRequestedPage()
    {
        using var fixture = new Fixture();
        var path = await fixture.WriteAsync("# One\nfirst\n# Two\nsecond");
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.DownArrow); // Go to source line.
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushTextWithEnter("3");
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewFormattedPreviewPager(console).RunAsync(path, 2);

        Assert.Contains("Source lines 3–4", console.Output);
        Assert.Contains("Two", console.Output);
    }

    [Fact]
    public async Task FenceCrossingPageBoundaryKeepsHeadingLookingCodeLiteral()
    {
        using var fixture = new Fixture();
        var path = await fixture.WriteAsync(
            "intro\n```text\n# literal inside code\n- not a list\n```\n# actual heading");
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Enter); // Next page: inside the fence.
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewFormattedPreviewPager(console).RunAsync(path, 2);

        Assert.Contains("# literal inside code", console.Output);
        Assert.Contains("- not a list", console.Output);
        Assert.Contains("Source lines 3–4", console.Output);
    }

    [Fact]
    public async Task ReturningToEarlierPageReplaysScannerWithoutLosingFenceContext()
    {
        using var fixture = new Fixture();
        var path = await fixture.WriteAsync(
            "intro\n```text\n# code\nsecond code\n```\nending");
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Enter); // Next.
        console.Input.PushKey(ConsoleKey.Enter); // Next again, to the final page.
        console.Input.PushKey(ConsoleKey.Enter); // Previous is first on the final page.
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewFormattedPreviewPager(console).RunAsync(path, 2);

        Assert.Contains("# code", console.Output);
        Assert.Contains("Source lines 5–6", console.Output);
        Assert.Equal(2, console.Output.Split(
            "Source lines 3–4", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public async Task DiskIndexedHeadingSelectionJumpsToDistantSourceLine()
    {
        using var fixture = new Fixture();
        var lines = Enumerable.Range(0, 55).Select(i => "ordinary " + i).ToArray();
        lines[0] = "# Introduction";
        lines[37] = "## Target section";
        var path = await fixture.WriteAsync(string.Join("\n", lines));
        var console = Interactive();

        console.Input.PushKey(ConsoleKey.DownArrow); // Go to source line
        console.Input.PushKey(ConsoleKey.DownArrow); // Find heading
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.DownArrow); // Target section
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewFormattedPreviewPager(console).RunAsync(path);

        Assert.Contains("Indexed headings", console.Output);
        Assert.Contains("Jumped to source line 38", console.Output);
        Assert.Contains("Source lines 38–", console.Output);
        Assert.Contains("Target section", console.Output);
    }

    [Fact]
    public async Task DiskIndexedLinkSelectionJumpsToItsSourceLocation()
    {
        using var fixture = new Fixture();
        var lines = Enumerable.Range(0, 50).Select(i => "ordinary " + i).ToArray();
        lines[19] = "[Installation](Docs/Install/en.md)";
        var path = await fixture.WriteAsync(string.Join("\n", lines));
        var console = Interactive();

        console.Input.PushKey(ConsoleKey.DownArrow); // Go to source line
        console.Input.PushKey(ConsoleKey.DownArrow); // Find heading
        console.Input.PushKey(ConsoleKey.DownArrow); // Find link
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Enter); // First link
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewFormattedPreviewPager(console).RunAsync(path);

        Assert.Contains("Indexed links", console.Output);
        Assert.Contains("Jumped to source line 20", console.Output);
        Assert.Contains("Docs/Install/en.md", console.Output);
    }

    [Fact]
    public async Task EmptyHeadingIndexReturnsToPreviewWithoutReplacingSource()
    {
        using var fixture = new Fixture();
        var path = await fixture.WriteAsync("ordinary text\n[One](Doc.md)");
        var console = Interactive();

        // No Next on the last page: Go to line, Find heading, Find link, Exit.
        console.Input.PushKey(ConsoleKey.DownArrow); // Find heading
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewFormattedPreviewPager(console).RunAsync(path, 24);

        Assert.Contains("No indexed headings were found.", console.Output);
        Assert.Contains("ordinary text", console.Output);
    }

    [Fact]
    public async Task ResponsiveFooterShowsTotalPagesAndVisualSourceRange()
    {
        using var fixture = new Fixture();
        var source = string.Join("\n", Enumerable.Range(1, 48).Select(number =>
            "Chapter " + number + ". This sentence should wrap, naturally, in a narrow terminal."));
        var path = await fixture.WriteAsync(source);
        var console = Interactive();
        console.Profile.Width = 42;
        console.Profile.Height = 28;
        console.Input.PushKey(ConsoleKey.Enter); // Next page
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewFormattedPreviewPager(console).RunAsync(path);

        Assert.Contains("Page 1 of ", console.Output);
        Assert.Contains("Page 2 of ", console.Output);
        Assert.Contains("Source lines 1–", console.Output);
        Assert.Contains("Smart prose wrapping", console.Output);
        // The 42-column viewport intentionally shortens header text.
        Assert.Contains("mdview — experimental Markdown page", console.Output);
    }

    [Fact]
    public async Task CodeOverflowIsMarkedButCodeSourceRemainsUnchanged()
    {
        using var fixture = new Fixture();
        var content = "```c\n    return really_long_function_name(12345);\n```\n# End";
        var path = await fixture.WriteAsync(content);
        var original = await File.ReadAllBytesAsync(path);
        var console = Interactive();
        console.Profile.Width = 31;
        console.Profile.Height = 40;
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewFormattedPreviewPager(console).RunAsync(path);

        // The 31-column terminal receives a single-row compact notice.
        Assert.Contains("→ Code clipped (no wrap).", console.Output);
        Assert.DoesNotContain("Code continues beyond the visible width", console.Output);
        Assert.Contains("Page 1 of 1", console.Output);
        Assert.Equal(original, await File.ReadAllBytesAsync(path));
    }

    [Fact]
    public async Task UnsafeInputIsNeverRendered()
    {
        using var fixture = new Fixture();
        var path = await fixture.WriteAsync("# Safe\n\u001b[31m");
        var console = Interactive();

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            new MdViewFormattedPreviewPager(console).RunAsync(path));

        Assert.DoesNotContain("Safe", console.Output);
    }

    [Fact]
    public async Task NoninteractivePreviewDoesNotOpenSource()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = false;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new MdViewFormattedPreviewPager(console).RunAsync("missing.md"));
    }

    private static TestConsole Interactive()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Profile.Capabilities.Unicode = true;
        console.Profile.Width = 120;
        console.Profile.Height = 40;
        return console;
    }

    private sealed class Fixture : IDisposable
    {
        private readonly string _directory = Path.Combine(
            Path.GetTempPath(), "afrowave-md-preview-" + Guid.NewGuid().ToString("N"));

        public Fixture() => Directory.CreateDirectory(_directory);

        public async Task<string> WriteAsync(string source)
        {
            var path = Path.Combine(_directory, "README.md");
            await File.WriteAllTextAsync(path, source, new UTF8Encoding(false));
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, recursive: true);
        }
    }
}
