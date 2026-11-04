using System.Text;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

public sealed class MdViewSourcePagerTests
{
    [Fact]
    public async Task EscapeQuitsWithoutChangingInputOrEvaluatingMarkup()
    {
        using var fixture = new Fixture();
        var path = await fixture.WriteAsync("# README\n[red]literally text[/]");
        var before = await File.ReadAllBytesAsync(path);

        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Escape);
        await new MdViewSourcePager(console).RunAsync(path, 2);

        Assert.Contains("raw source pager", console.Output);
        Assert.Contains("1: # README", console.Output);
        Assert.Contains("[red]literally text[/]", console.Output);
        Assert.Equal(before, await File.ReadAllBytesAsync(path));
    }

    [Fact]
    public async Task NextAndPreviousPageRecoverTheOriginalLineRange()
    {
        using var fixture = new Fixture();
        var path = await fixture.WriteAsync("First\nSecond\nThird\nFourth\nFifth");
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Enter); // Next page.
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter); // Previous page.
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewSourcePager(console).RunAsync(path, 2);

        Assert.Contains("3: Third", console.Output);
        Assert.Equal(2, console.Output.Split("1: First", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public async Task JumpCanNavigateDirectlyToAnotherPartOfTheFile()
    {
        using var fixture = new Fixture();
        var path = await fixture.WriteAsync(
            string.Join("\n", Enumerable.Range(1, 30).Select(i => "Line " + i)));
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.DownArrow); // Go to line.
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushTextWithEnter("25");
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewSourcePager(console).RunAsync(path, 4);

        Assert.Contains("25: Line 25", console.Output);
    }

    [Fact]
    public async Task InvalidJumpDoesNotChangeCurrentPage()
    {
        using var fixture = new Fixture();
        var path = await fixture.WriteAsync("One\nTwo\nThree");
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.DownArrow); // Go to line.
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushTextWithEnter("999");
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewSourcePager(console).RunAsync(path, 2);

        Assert.Contains("Line number is outside this document.", console.Output);
        Assert.Equal(2, console.Output.Split("1: One", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public async Task NoninteractiveUseFailsBeforeOpeningSource()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = false;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new MdViewSourcePager(console).RunAsync("nonexistent.md"));
    }

    [Fact]
    public async Task UnsafeSourceNeverReachesTheTerminal()
    {
        using var fixture = new Fixture();
        var path = await fixture.WriteBytesAsync(Encoding.UTF8.GetBytes("First\n\u001b[31m"));
        var console = Interactive();
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            new MdViewSourcePager(console).RunAsync(path));
        Assert.DoesNotContain("1: First", console.Output);
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
        public string Root { get; } =
            Path.Combine(Path.GetTempPath(), "mdview-paging-" + Guid.NewGuid().ToString("N"));

        public Fixture() => Directory.CreateDirectory(Root);

        public Task<string> WriteAsync(string content) =>
            WriteBytesAsync(Encoding.UTF8.GetBytes(content));

        public async Task<string> WriteBytesAsync(byte[] content)
        {
            var path = Path.Combine(Root, "README.md");
            await File.WriteAllBytesAsync(path, content);
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
