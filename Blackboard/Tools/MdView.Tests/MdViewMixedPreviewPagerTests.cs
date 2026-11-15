using System.Text;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>
/// Opt-in mixed Markdown/native-table CLI navigation over validated local
/// sources, including canonical page jumps, compact fallback and guards.
/// </summary>
public sealed class MdViewMixedPreviewPagerTests
{
    [Fact]
    public async Task DisplaysInlineNativeTableBetweenProseAndExitsOnEscape()
    {
        using var fixture = new Fixture(
            "Before table\n| Name | Value |\n| --- | ---: |\n" +
            "| CPU | 16 cores |\nAfter table");
        var original = await File.ReadAllBytesAsync(fixture.Path);
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(fixture.Path);

        Assert.Contains("mixed Markdown / native Spectre preview", console.Output);
        Assert.Contains("Page 1 of 1", console.Output);
        Assert.Contains("╭", console.Output);
        Assert.Contains("CPU", console.Output);
        Assert.Contains("16 cores", console.Output);
        Assert.Contains("After table", console.Output);
        Assert.Equal(original, await File.ReadAllBytesAsync(fixture.Path));
    }

    [Fact]
    public async Task NextAndPreviousPagesFollowCanonicalDiskIndex()
    {
        using var fixture = new Fixture(string.Join("\n",
            Enumerable.Range(1, 14).Select(i => "Line " + i)));
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Enter); // Next page.
        console.Input.PushKey(ConsoleKey.DownArrow); // Previous page.
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(
            fixture.Path, linesPerPage: 2);

        Assert.Contains("Page 1 of 7", console.Output);
        Assert.Contains("Page 2 of 7", console.Output);
        Assert.Equal(2, console.Output.Split(
            "Page 1 of 7", StringSplitOptions.None).Length - 1);
        Assert.Contains("Previous page", console.Output);
    }

    [Fact]
    public async Task SourceLineJumpInsideNativeTableOpensCanonicalPage()
    {
        var markdown = string.Join("\n",
            Enumerable.Range(1, 15).Select(i => "Introduction " + i)) +
            "\n| Name | Value |\n| --- | --- |\n| Apple | 10 |\nAfterwards";
        using var fixture = new Fixture(markdown);
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.DownArrow); // Go to source line.
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushTextWithEnter("18"); // Native data row.
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(
            fixture.Path, linesPerPage: 10);

        Assert.Contains("Source line 18 is on this canonical page.", console.Output);
        Assert.Contains("Apple", console.Output);
        Assert.Contains("╭", console.Output);
        Assert.Contains("Page 3 of ", console.Output);
        Assert.DoesNotContain("Source line is outside this document.",
            console.Output);
    }

    [Fact]
    public async Task DiskIndexedHeadingPickerFindsDistantSourcePage()
    {
        var lines = Enumerable.Range(0, 40)
            .Select(i => "ordinary line " + (i + 1)).ToArray();
        lines[0] = "# Start";
        lines[32] = "## Distant heading";
        using var fixture = new Fixture(string.Join("\n", lines));
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.DownArrow); // Go to source line.
        console.Input.PushKey(ConsoleKey.DownArrow); // Find heading.
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.DownArrow); // Distant heading.
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(
            fixture.Path, linesPerPage: 8);

        Assert.Contains("Indexed headings", console.Output);
        Assert.Contains("Source line 33 is on this canonical page.", console.Output);
        Assert.Contains("Distant heading", console.Output);
        Assert.Contains("Page 5 of 5", console.Output);
    }

    [Fact]
    public async Task LongTableStaysCompactThroughMixedPreview()
    {
        var source = "| Name | Value |\n| --- | --- |\n" +
            string.Join("\n", Enumerable.Range(1, 14)
                .Select(i => "| Item " + i + " | " + i + " |"));
        using var fixture = new Fixture(source);
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Enter); // Next page.
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(
            fixture.Path, linesPerPage: 8);

        Assert.Contains("Page 1 of ", console.Output);
        Assert.Contains("Page 2 of ", console.Output);
        Assert.Contains("Item 1", console.Output);
        Assert.DoesNotContain("╭", console.Output);
    }

    [Fact]
    public async Task NarrowTerminalKeepsSmallTableInCompactMode()
    {
        using var fixture = new Fixture(
            "| Name | Value |\n| --- | --- |\n| CPU | OK |");
        var console = Interactive();
        console.Profile.Width = 36;
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(fixture.Path);

        Assert.Contains("Page 1 of 1", console.Output);
        Assert.Contains("CPU", console.Output);
        Assert.DoesNotContain("╭", console.Output);
    }

    [Fact]
    public async Task EmptySourceShowsSingleEmptyPage()
    {
        using var fixture = new Fixture("");
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewMixedPreviewPager(console).RunAsync(fixture.Path);

        Assert.Contains("(Empty page)", console.Output);
        Assert.Contains("Page 1 of 1", console.Output);
        Assert.DoesNotContain("Next page", console.Output);
    }

    [Fact]
    public async Task TinyTerminalReturnsBeforeOpeningMissingSource()
    {
        var console = Interactive();
        console.Profile.Height = 12;

        await new MdViewMixedPreviewPager(console).RunAsync("missing.md");

        Assert.Contains("requires at least 28 columns and 18 rows",
            console.Output);
        Assert.DoesNotContain("Mixed preview navigation", console.Output);
    }

    [Fact]
    public async Task NoninteractiveOrInvalidArgumentsFailBeforeOpeningSource()
    {
        var console = Interactive();
        console.Profile.Capabilities.Interactive = false;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new MdViewMixedPreviewPager(console).RunAsync("missing.md"));

        console.Profile.Capabilities.Interactive = true;
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            new MdViewMixedPreviewPager(console).RunAsync("missing.md", 129));
    }

    [Fact]
    public async Task UnsafeSourceRejectedWithoutRenderingAnyPage()
    {
        using var fixture = new Fixture("# Safe\n\u001b[31m");
        var console = Interactive();

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            new MdViewMixedPreviewPager(console).RunAsync(fixture.Path));
        Assert.DoesNotContain("Page 1 of", console.Output);
        Assert.DoesNotContain("# Safe", console.Output);
    }

    private static TestConsole Interactive()
    {
        var console = new TestConsole();
        console.Profile.Width = 96;
        console.Profile.Height = 40;
        console.Profile.Capabilities.Unicode = true;
        console.Profile.Capabilities.Interactive = true;
        return console;
    }

    private sealed class Fixture : IDisposable
    {
        private readonly string _folder = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "mdview-mixed-ui-" + Guid.NewGuid().ToString("N"));

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
