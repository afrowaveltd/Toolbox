using System.Text;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>Streaming Markdown navigation index and disk-budget regressions.</summary>
public sealed class MdViewDiskNavigationIndexTests
{
    [Fact]
    public async Task IndexesHeadingsAndLinksButNeverFencedOrInlineCode()
    {
        using var sample = new Document("""
            # Start
            [Install](Docs/Install/en.md) and **[Top](#start)**
            `[not a link](inline.md)` ![picture](image.png)
            ```markdown
            # False heading
            [Hidden](secret.md)
            ```
            ## Installation
            > [Quoted docs](Docs/Quoted.md)
            """);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(sample.Path);
        await using var index = await MdViewDiskNavigationIndex.CreateAsync(snapshot);

        Assert.Equal(2, index.HeadingCount);
        Assert.Equal(3, index.LinkCount);
        Assert.Equal(snapshot.Lines.LineCount, index.SourceLineCount);
        Assert.True(index.IndexBytes > 0);

        var headings = await index.ReadAsync(MdViewIndexedItemKind.Heading, 0);
        Assert.Equal(new[] { "Start", "Installation" },
            headings.Select(item => item.Label).ToArray());
        Assert.Equal(new long[] { 0, 7 },
            headings.Select(item => item.LineNumber).ToArray());

        var links = await index.ReadAsync(MdViewIndexedItemKind.Link, 0);
        Assert.Equal(
            new[] { "Docs/Install/en.md", "#start", "Docs/Quoted.md" },
            links.Select(item => item.Destination).ToArray());
        Assert.Equal(new long[] { 1, 1, 8 },
            links.Select(item => item.LineNumber).ToArray());
        Assert.Equal((long)sample.Text.IndexOf("[Install]", StringComparison.Ordinal),
            links[0].Utf16Offset);
        Assert.All(links, item => Assert.Equal(MdViewIndexedItemKind.Link, item.Kind));
    }

    [Fact]
    public async Task IndentedCodeDoesNotProduceClickableSourceLinkLocations()
    {
        using var sample = new Document(
            "# Real heading\n    [Not a link](hidden.md)\n\t[Also code](tab.md)\n[Real](Docs/en.md)");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(sample.Path);
        await using var index = await MdViewDiskNavigationIndex.CreateAsync(snapshot);

        Assert.Equal(1, index.HeadingCount);
        Assert.Equal(1, index.LinkCount);
        var links = await index.ReadAsync(MdViewIndexedItemKind.Link, 0);
        Assert.Equal("Docs/en.md", Assert.Single(links).Destination);
        Assert.Equal(3, links[0].LineNumber);
    }

    [Fact]
    public async Task ReadsDistantResultPagesWithoutMaterializingAllEntries()
    {
        var text = string.Join("\n", Enumerable.Range(0, 3000).Select(i =>
            i % 3 == 0 ? "# Heading " + i :
            i % 3 == 1 ? "[Read](Docs/" + i + ".md)" : "ordinary paragraph"));
        using var sample = new Document(text);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(sample.Path);
        await using var index = await MdViewDiskNavigationIndex.CreateAsync(snapshot);

        Assert.Equal(1000, index.HeadingCount);
        Assert.Equal(1000, index.LinkCount);
        var last = await index.ReadAsync(MdViewIndexedItemKind.Heading, 995, 5);
        Assert.Equal(5, last.Count);
        Assert.Equal(2985, last[0].LineNumber);
        Assert.Equal(2997, last[^1].LineNumber);
        Assert.Empty(await index.ReadAsync(MdViewIndexedItemKind.Link, 1000));
    }

    [Fact]
    public async Task MixedLineEndingsPreserveUtf16AndPhysicalCoordinates()
    {
        const string source = "# One\r\nčlánek\r[Two](Docs/two.md)\n# Three";
        using var sample = new Document(source);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(sample.Path);
        await using var index = await MdViewDiskNavigationIndex.CreateAsync(snapshot);
        var headings = await index.ReadAsync(MdViewIndexedItemKind.Heading, 0);
        var links = await index.ReadAsync(MdViewIndexedItemKind.Link, 0);

        Assert.Equal(2, headings.Count);
        Assert.Equal(1, links.Count);
        Assert.Equal(3, headings[1].LineNumber);
        Assert.Equal(2, links[0].LineNumber);
        Assert.Equal((long)source.IndexOf("[Two]", StringComparison.Ordinal),
            links[0].Utf16Offset);
        Assert.Equal((long)source.IndexOf("# Three", StringComparison.Ordinal),
            headings[1].Utf16Offset);
    }

    [Fact]
    public async Task RejectsTooSmallDiskBudgetWithoutPublishingPartialIndex()
    {
        using var sample = new Document("# One\n[link](Docs/en.md)\n# Two");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(sample.Path);
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            MdViewDiskNavigationIndex.CreateAsync(snapshot, maximumIndexBytes: 64));

        // The validated copy is still owned and readable after index failure.
        var page = await snapshot.Lines.ReadPageAsync(0, 1);
        Assert.Contains("# One", page.Lines[0]);
    }

    [Fact]
    public async Task ExcludesOverlongLinkTargetsAndBoundsUnicodeLabels()
    {
        var longLabel = new string('a', MdViewDiskNavigationIndex.MaximumLabelCharacters - 1)
            + "😀" + new string('b', 40);
        var longTarget = new string('c',
            MdViewDiskNavigationIndex.MaximumDestinationCharacters + 1);
        using var sample = new Document(
            "# " + longLabel + "\n[valid](" + "docs.md" + ")" +
            "\n[too long](" + longTarget + ")");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(sample.Path);
        await using var index = await MdViewDiskNavigationIndex.CreateAsync(snapshot);
        var headings = await index.ReadAsync(MdViewIndexedItemKind.Heading, 0);
        var links = await index.ReadAsync(MdViewIndexedItemKind.Link, 0);

        Assert.Single(headings);
        Assert.Equal(MdViewDiskNavigationIndex.MaximumLabelCharacters - 1,
            headings[0].Label.Length);
        Assert.False(char.IsHighSurrogate(headings[0].Label[^1]));
        Assert.Single(links);
        Assert.Equal("docs.md", links[0].Destination);
    }

    [Fact]
    public async Task DisposedIndexRejectsFurtherReadsAndValidatesBounds()
    {
        using var sample = new Document("# Hello");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(sample.Path);
        var index = await MdViewDiskNavigationIndex.CreateAsync(snapshot);
        try
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                index.ReadAsync(MdViewIndexedItemKind.Heading, -1));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                index.ReadAsync(MdViewIndexedItemKind.Link, 1));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                index.ReadAsync(MdViewIndexedItemKind.Heading, 0, 129));
        }
        finally
        {
            await index.DisposeAsync();
        }
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            index.ReadAsync(MdViewIndexedItemKind.Heading, 0));
    }

    private sealed class Document : IDisposable
    {
        private readonly string _directory = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "mdview-disk-nav-test-" + Guid.NewGuid().ToString("N"));

        public Document(string text)
        {
            Text = text;
            Directory.CreateDirectory(_directory);
            Path = System.IO.Path.Combine(_directory, "README.md");
            File.WriteAllText(Path, text, new UTF8Encoding(false));
        }

        public string Text { get; }
        public string Path { get; }

        public void Dispose() => Directory.Delete(_directory, recursive: true);
    }
}
