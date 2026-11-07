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
        console.Input.PushKey(ConsoleKey.Enter); // Previous (first choice on last? No: line 3–4 has Next).
        // Second Enter selects Next again because this is the middle page.
        console.Input.PushKey(ConsoleKey.Enter); // Previous on final page.
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewFormattedPreviewPager(console).RunAsync(path, 2);

        Assert.Contains("# code", console.Output);
        Assert.Contains("Source lines 5–6", console.Output);
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
