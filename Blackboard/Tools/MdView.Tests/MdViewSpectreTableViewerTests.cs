using System.Text;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>Real Spectre.Console table rendering and bounded source navigation.</summary>
public sealed class MdViewSpectreTableViewerTests
{
    [Fact]
    public async Task FindsOnlyHeaderFollowedByMatchingDelimiter()
    {
        using var fixture = new SourceFixture(
            "ordinary | words\n" +
            "| Name | Price |\n" +
            "| :--- | ---: |\n" +
            "| Apple | 10 |\n" +
            "After table");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);

        Assert.Equal(1, await MdViewSpectreTableViewer.FindFirstTableAsync(
            cursor, 0, 5));
        Assert.Null(await MdViewSpectreTableViewer.FindFirstTableAsync(
            cursor, 0, 1));
        Assert.Null(await MdViewSpectreTableViewer.FindFirstTableAsync(
            cursor, 3, 5));
    }

    [Fact]
    public async Task NativeWideTableHasRoundedBorderAndRealColumns()
    {
        using var fixture = new SourceFixture(
            "| Name | Price |\n| :--- | ---: |\n" +
            "| Apple | 10 |\n| Pear | 20 |\nAfter");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = Interactive(width: 96, height: 34);
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewSpectreTableViewer(console).RunAsync(cursor, 0);

        Assert.Contains("╭", console.Output);
        Assert.Contains("╯", console.Output);
        Assert.Contains("Name", console.Output);
        Assert.Contains("Price", console.Output);
        Assert.Contains("Apple", console.Output);
        Assert.Contains("Pear", console.Output);
        Assert.Contains("Back to Markdown preview", console.Output);
        Assert.DoesNotContain("Next table chunk", console.Output);
    }

    [Fact]
    public async Task NativeNarrowTableUsesFieldValueLayoutWithNextFieldChunk()
    {
        using var fixture = new SourceFixture(
            "| A | B | C | D | E |\n| --- | --- | --- | --- | --- |\n" +
            "| Alpha | Beta | Gamma | Delta | Epsilon |");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = Interactive(width: 36, height: 15);
        console.Input.PushKey(ConsoleKey.Enter); // Next field chunk
        console.Input.PushKey(ConsoleKey.Escape); // Back

        await new MdViewSpectreTableViewer(console).RunAsync(cursor, 0);

        Assert.Contains("Column", console.Output);
        Assert.Contains("Value", console.Output);
        Assert.Contains("Alpha", console.Output);
        Assert.Contains("Epsilon", console.Output);
        Assert.Contains("Next table chunk", console.Output);
        Assert.Contains("╭", console.Output);
    }

    [Fact]
    public async Task MultiPageGridKeepsHeaderAndDoesNotLoadEntireTable()
    {
        var source = "| Product | Amount |\n| --- | ---: |\n" +
            string.Join("\n", Enumerable.Range(1, 19)
                .Select(i => "| Item " + i + " | " + i + " |"));
        using var fixture = new SourceFixture(source);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = Interactive(width: 96, height: 24);
        console.Input.PushKey(ConsoleKey.Enter); // Next four rows
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewSpectreTableViewer(console).RunAsync(cursor, 0);

        Assert.Contains("Item 1", console.Output);
        Assert.Contains("Item 5", console.Output);
        Assert.Contains("Rows visited: 8", console.Output);
        Assert.DoesNotContain("Item 19", console.Output);
    }

    [Fact]
    public async Task UntrustedMarkupIsRenderedAsLiteralTextAndSourceDoesNotChange()
    {
        var text = "| [red]Name[/] | Value |\n| --- | --- |\n" +
            "| [green]literal[/] | [red]not-an-ansi-command[/] |";
        using var fixture = new SourceFixture(text);
        var before = await File.ReadAllBytesAsync(fixture.Path);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = Interactive(width: 96, height: 30);
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewSpectreTableViewer(console).RunAsync(cursor, 0);

        Assert.Contains("[red]Name[/]", console.Output);
        Assert.Contains("[green]literal[/]", console.Output);
        Assert.Equal(before, await File.ReadAllBytesAsync(fixture.Path));
    }

    [Fact]
    public async Task LongCellIsExplicitlyShortenedWithoutLosingValidatedSource()
    {
        var source = "| ID | Value |\n| --- | --- |\n| 1 | " +
            new string('x', 2048) + "TAILMARKER |";
        using var fixture = new SourceFixture(source);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = Interactive(width: 96, height: 24);
        console.Input.PushKey(ConsoleKey.Escape);

        await new MdViewSpectreTableViewer(console).RunAsync(cursor, 0);

        Assert.Contains("…", console.Output);
        Assert.DoesNotContain("TAILMARKER", console.Output);
        Assert.Contains("TAILMARKER", await File.ReadAllTextAsync(fixture.Path));
    }

    [Fact]
    public async Task FencedPipeTableDoesNotExposeNativePreview()
    {
        using var fixture = new SourceFixture(
            "\u0060\u0060\u0060md\n| Name | Value |\n" +
            "| --- | --- |\n\u0060\u0060\u0060\nordinary");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);

        Assert.Null(await MdViewSpectreTableViewer.FindFirstTableAsync(
            cursor, 0, 5));
    }

    [Fact]
    public async Task IncorrectHeaderOrNoninteractiveModeFailsClosed()
    {
        using var fixture = new SourceFixture(
            "| Name | Value |\n| wrong | data |");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var console = Interactive();
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            new MdViewSpectreTableViewer(console).RunAsync(cursor, 0));

        console.Profile.Capabilities.Interactive = false;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new MdViewSpectreTableViewer(console).RunAsync(cursor, 0));
    }

    private static TestConsole Interactive(int width = 120, int height = 40)
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Profile.Capabilities.Unicode = true;
        console.Profile.Width = width;
        console.Profile.Height = height;
        return console;
    }

    private sealed class SourceFixture : IDisposable
    {
        private readonly string _folder = Path.Combine(Path.GetTempPath(),
            "mdview-spectre-table-" + Guid.NewGuid().ToString("N"));

        public SourceFixture(string source)
        {
            Directory.CreateDirectory(_folder);
            Path = System.IO.Path.Combine(_folder, "README.md");
            File.WriteAllText(Path, source, new UTF8Encoding(false));
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(_folder))
                Directory.Delete(_folder, true);
        }
    }
}
