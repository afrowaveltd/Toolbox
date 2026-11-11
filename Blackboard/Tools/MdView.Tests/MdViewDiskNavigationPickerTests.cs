using System.Text;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>Bounded terminal browsing over disk-only heading/link result pages.</summary>
public sealed class MdViewDiskNavigationPickerTests
{
    [Fact]
    public async Task SelectsHeadingFromNextDiskResultPage()
    {
        using var document = new Document(string.Join("\n",
            Enumerable.Range(1, 30).Select(i => "# Heading " + i)));
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(document.Path);
        await using var index = await MdViewDiskNavigationIndex.CreateAsync(snapshot);
        var console = Interactive();
        for (var i = 0; i < MdViewDiskNavigationPicker.ResultsPerPage; i++)
            console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter); // Next results
        console.Input.PushKey(ConsoleKey.Enter); // Heading 13

        var chosen = await new MdViewDiskNavigationPicker(console)
            .PickAsync(index, MdViewIndexedItemKind.Heading);

        Assert.Equal(12, chosen?.LineNumber);
        Assert.Equal("Heading 13", chosen?.Label);
        Assert.Contains("13–24 of 30", console.Output);
    }

    [Fact]
    public async Task EscapeLeavesDiskIndexAvailableForLaterSelection()
    {
        using var document = new Document("# One\n# Two");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(document.Path);
        await using var index = await MdViewDiskNavigationIndex.CreateAsync(snapshot);
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Escape);
        Assert.Null(await new MdViewDiskNavigationPicker(console)
            .PickAsync(index, MdViewIndexedItemKind.Heading));

        var entries = await index.ReadAsync(MdViewIndexedItemKind.Heading, 0);
        Assert.Equal(2, entries.Count);
    }

    [Fact]
    public async Task EmptyIndexAndNoninteractiveTerminalsAreHandled()
    {
        using var document = new Document("plain text");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(document.Path);
        await using var index = await MdViewDiskNavigationIndex.CreateAsync(snapshot);
        var console = Interactive();
        Assert.Null(await new MdViewDiskNavigationPicker(console)
            .PickAsync(index, MdViewIndexedItemKind.Link));
        Assert.Contains("No indexed links were found.", console.Output);

        var noninteractive = new TestConsole();
        noninteractive.Profile.Capabilities.Interactive = false;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new MdViewDiskNavigationPicker(noninteractive)
                .PickAsync(index, MdViewIndexedItemKind.Link));
    }

    private static TestConsole Interactive()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Profile.Capabilities.Unicode = true;
        console.Profile.Width = 120;
        return console;
    }

    private sealed class Document : IDisposable
    {
        private readonly string _directory = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "mdview-index-picker-" + Guid.NewGuid().ToString("N"));

        public Document(string source)
        {
            Directory.CreateDirectory(_directory);
            Path = System.IO.Path.Combine(_directory, "README.md");
            File.WriteAllText(Path, source, new UTF8Encoding(false));
        }

        public string Path { get; }

        public void Dispose() => Directory.Delete(_directory, recursive: true);
    }
}
