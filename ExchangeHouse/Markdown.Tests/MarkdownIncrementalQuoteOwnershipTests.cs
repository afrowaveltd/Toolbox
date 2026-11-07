using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Xunit;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown.Tests;

public sealed class MarkdownIncrementalQuoteOwnershipTests
{
    [Fact]
    public async Task QuotedSubtreeRetainsOuterListOwnerAndDistinctInnerListOwner()
    {
        const string source =
            "- outer\r\n" +
            "  > description\n" +
            "  > - inner\n" +
            "  >   - nested\r\n" +
            "  >     detail\n" +
            "  > - sibling\n";
        var stream = await ScanAsync(source);
        var parser = MarkdownParser.Parse(source);

        Assert.Equal(source, string.Concat(stream.Select(x => x.Source.Raw)));
        Assert.Equal(6, stream.Count);
        Assert.Equal(MarkdownContainerLineKind.ListItem, stream[0].Kind);
        Assert.Equal(MarkdownContainerLineKind.Quote, stream[1].Kind);
        Assert.Equal(new[] { -1, -1, 0, 1, 1, 0 }, stream.Select(x => x.ListDepth));
        Assert.All(stream.Skip(1), x =>
        {
            Assert.Equal(1, x.QuoteDepth);
            Assert.Equal(stream[0].Source.Utf16Offset, x.QuoteContainerListOwnerUtf16Offset);
            Assert.Equal(0, x.QuoteContainerListDepth);
            Assert.Equal("-", x.QuoteContainerListMarker);
        });
        Assert.Null(stream[1].ListOwnerUtf16Offset);
        Assert.Equal(stream[2].Source.Utf16Offset, stream[2].ListOwnerUtf16Offset);
        Assert.Equal(stream[3].Source.Utf16Offset, stream[4].ListOwnerUtf16Offset);
        Assert.Equal(stream[5].Source.Utf16Offset, stream[5].ListOwnerUtf16Offset);

        Assert.Equal(source, MarkdownWriter.Write(parser));
        for (var i = 1; i < stream.Count; i++)
        {
            Assert.Equal(parser.Blocks[i].QuoteContainerListOwnerStart,
                stream[i].QuoteContainerListOwnerUtf16Offset);
            Assert.Equal(parser.Blocks[i].ListMarker?.Depth, stream[i].Kind ==
                MarkdownContainerLineKind.ListItem ? stream[i].ListDepth : null);
        }
    }

    [Fact]
    public async Task ChangingOuterItemResetsQuoteLocalNestingAtTheSameQuoteDepth()
    {
        const string source =
            "- first\n  > - alpha\n  >   - deep\n" +
            "- second\n  > - beta\n  >   content";
        var scanned = await ScanAsync(source);
        var parsed = MarkdownParser.Parse(source);

        Assert.Equal(new[] { 0, 1, 1, 0, 1, 1 },
            scanned.Select(x => x.QuoteDepth));
        Assert.Equal(new[] { 0, 0, 1, 0, 0, 0 },
            scanned.Select(x => x.ListDepth));
        Assert.All(scanned.Skip(1).Take(2), x =>
            Assert.Equal(scanned[0].Source.Utf16Offset,
                x.QuoteContainerListOwnerUtf16Offset));
        Assert.All(scanned.Skip(4), x =>
            Assert.Equal(scanned[3].Source.Utf16Offset,
                x.QuoteContainerListOwnerUtf16Offset));
        Assert.Equal(scanned[4].Source.Utf16Offset, scanned[5].ListOwnerUtf16Offset);
        Assert.Equal(source, MarkdownWriter.Write(parsed));
        Assert.Equal(parsed.Blocks[4].QuoteContainerListOwnerStart,
            scanned[4].QuoteContainerListOwnerUtf16Offset);
    }

    [Fact]
    public async Task NestedOrdinaryListMayOwnQuoteWhileQuoteListDepthStartsAtZero()
    {
        const string source =
            "1. parent\n" +
            "   - child\n" +
            "     > - quote item\n" +
            "     >   detail\n" +
            "   - sibling\n";
        var lines = await ScanAsync(source);
        var parsed = MarkdownParser.Parse(source);

        Assert.Equal(new[] { 0, 1, 0, 0, 1 }, lines.Select(x => x.ListDepth));
        Assert.Equal(new[] { 0, 0, 1, 1, 0 }, lines.Select(x => x.QuoteDepth));
        Assert.All(lines.Skip(2).Take(2), x =>
        {
            Assert.Equal(lines[1].Source.Utf16Offset, x.QuoteContainerListOwnerUtf16Offset);
            Assert.Equal(1, x.QuoteContainerListDepth);
            Assert.Equal("-", x.QuoteContainerListMarker);
        });
        Assert.Equal(lines[2].Source.Utf16Offset, lines[3].ListOwnerUtf16Offset);
        Assert.Equal(parsed.Blocks[2].QuoteContainerListOwnerStart,
            lines[2].QuoteContainerListOwnerUtf16Offset);
        Assert.Equal(source, string.Concat(lines.Select(x => x.Source.Raw)));
    }

    [Fact]
    public async Task DirectQuoteDoesNotInventAnOuterOwner()
    {
        const string source =
            "> - standalone\n>   detail\n>> - nested depth\n";
        var lines = await ScanAsync(source);
        Assert.Equal(new[] { 1, 1, 2 }, lines.Select(x => x.QuoteDepth));
        Assert.Equal(new[] { 0, 0, 0 }, lines.Select(x => x.ListDepth));
        Assert.All(lines, line =>
        {
            Assert.Null(line.QuoteContainerListOwnerUtf16Offset);
            Assert.Equal(-1, line.QuoteContainerListDepth);
            Assert.Null(line.QuoteContainerListMarker);
        });
        Assert.Equal(lines[0].Source.Utf16Offset, lines[1].ListOwnerUtf16Offset);
    }

    [Fact]
    public async Task StandaloneQuoteCannotRevivePreviousOuterOwner()
    {
        const string source =
            "- parent\n" +
            "> - standalone\n" +
            "  > cannot belong to parent\n";
        var lines = await ScanAsync(source);
        Assert.All(lines.Skip(1), x =>
            Assert.Null(x.QuoteContainerListOwnerUtf16Offset));
        Assert.Equal(new[] { 0, 1, 1 }, lines.Select(x => x.QuoteDepth));
        Assert.Equal(MarkdownContainerLineKind.Quote, lines[2].Kind);
    }

    [Fact]
    public async Task ExplicitlyIndentedQuoteWithoutInnerListPreservesOnlyOuterOwner()
    {
        const string source =
            "- outer\n  > plain prose\n  > more prose\n- next";
        var lines = await ScanAsync(source);

        Assert.All(lines.Skip(1).Take(2), x =>
        {
            Assert.Equal(MarkdownContainerLineKind.Quote, x.Kind);
            Assert.Equal(lines[0].Source.Utf16Offset, x.QuoteContainerListOwnerUtf16Offset);
            Assert.Null(x.ListOwnerUtf16Offset);
            Assert.Equal(-1, x.ListDepth);
        });
        Assert.Equal(MarkdownContainerLineKind.ListItem, lines[^1].Kind);
        Assert.Null(lines[^1].QuoteContainerListOwnerUtf16Offset);
    }

    [Fact]
    public async Task QuoteOwnedFenceRetainsOuterOwnerAcrossLiteralCode()
    {
        const string source =
            "- outer\n" +
            "  > ```csharp\n" +
            "  > # literal heading\n" +
            "  > - literal list\n" +
            "  > ```\n" +
            "- after";
        var lines = await ScanAsync(source);
        var parsed = MarkdownParser.Parse(source);

        Assert.Equal(new[]
        {
            MarkdownContainerLineKind.ListItem,
            MarkdownContainerLineKind.FenceOpen,
            MarkdownContainerLineKind.CodeLine,
            MarkdownContainerLineKind.CodeLine,
            MarkdownContainerLineKind.FenceClose,
            MarkdownContainerLineKind.ListItem
        }, lines.Select(x => x.Kind));
        Assert.All(lines.Skip(1).Take(4), x =>
        {
            Assert.Equal(lines[0].Source.Utf16Offset, x.QuoteContainerListOwnerUtf16Offset);
            Assert.Equal(lines[1].Source.Utf16Offset, x.FenceStartUtf16Offset);
            Assert.Equal("csharp", x.CodeLanguage);
            Assert.Null(x.ListOwnerUtf16Offset);
        });
        for (var i = 1; i < 5; i++)
        {
            Assert.Equal(parsed.Blocks[i].QuoteContainerListOwnerStart,
                lines[i].QuoteContainerListOwnerUtf16Offset);
            Assert.Equal(parsed.Blocks[i].CodeFenceStart, lines[i].FenceStartUtf16Offset);
        }
    }

    [Fact]
    public async Task QuoteListOwnedFenceRetainsBothOwnersAndProtectsMarkers()
    {
        const string source =
            "- outer\n" +
            "  > - inner\n" +
            "  >   ```json\n" +
            "  >   {\"ok\":true}\n" +
            "  >   # still code\n" +
            "  >   ```\n" +
            "  > - sibling";
        var lines = await ScanAsync(source);
        var parsed = MarkdownParser.Parse(source);

        Assert.Equal(MarkdownContainerLineKind.FenceOpen, lines[2].Kind);
        Assert.Equal(MarkdownContainerLineKind.CodeLine, lines[3].Kind);
        Assert.Equal(MarkdownContainerLineKind.CodeLine, lines[4].Kind);
        Assert.Equal(MarkdownContainerLineKind.FenceClose, lines[5].Kind);
        Assert.All(lines.Skip(2).Take(4), x =>
        {
            Assert.Equal(lines[0].Source.Utf16Offset, x.QuoteContainerListOwnerUtf16Offset);
            Assert.Equal(lines[1].Source.Utf16Offset, x.ListOwnerUtf16Offset);
            Assert.Equal(lines[2].Source.Utf16Offset, x.FenceStartUtf16Offset);
            Assert.Equal(0, x.ListDepth);
            Assert.Equal(1, x.QuoteDepth);
        });
        Assert.Equal(0, lines[6].ListDepth);
        Assert.Equal(source, MarkdownWriter.Write(parsed));
        for (var i = 2; i < 6; i++)
        {
            Assert.Equal(parsed.Blocks[i].CodeFenceStart, lines[i].FenceStartUtf16Offset);
            Assert.Equal(parsed.Blocks[i].QuoteContainerListOwnerStart,
                lines[i].QuoteContainerListOwnerUtf16Offset);
        }
    }

    [Fact]
    public async Task DifferentQuoteDepthInterruptsFenceWithoutInventingCloser()
    {
        const string source =
            "- outer\n" +
            "  > ```js\n" +
            "  > inside\n" +
            "  >> - deeper list\n" +
            "  > resumed but not code";
        var lines = await ScanAsync(source);
        Assert.Equal(new[]
        {
            MarkdownContainerLineKind.ListItem,
            MarkdownContainerLineKind.FenceOpen,
            MarkdownContainerLineKind.CodeLine,
            MarkdownContainerLineKind.ListItem,
            MarkdownContainerLineKind.Quote
        }, lines.Select(x => x.Kind));
        Assert.Null(lines[3].FenceStartUtf16Offset);
        Assert.Null(lines[4].FenceStartUtf16Offset);
        Assert.Equal(lines[0].Source.Utf16Offset,
            lines[3].QuoteContainerListOwnerUtf16Offset);
    }

    [Fact]
    public async Task UnquotedNewItemEndsQuotedFenceAndChangesOwner()
    {
        const string source =
            "- first\n  > ```txt\n" +
            "- second\n  > not a code line";
        var lines = await ScanAsync(source);

        Assert.Equal(new[]
        {
            MarkdownContainerLineKind.ListItem,
            MarkdownContainerLineKind.FenceOpen,
            MarkdownContainerLineKind.ListItem,
            MarkdownContainerLineKind.Quote
        }, lines.Select(x => x.Kind));
        Assert.Null(lines[3].FenceStartUtf16Offset);
        Assert.Equal(lines[2].Source.Utf16Offset, lines[3].QuoteContainerListOwnerUtf16Offset);
    }

    [Fact]
    public async Task QuoteScopeAndInnerListStateAreSeparatedAcrossTwoOuterItems()
    {
        const string source =
            "- a\n  > - x\n  >   detail\n" +
            "- b\n  > - y\n  >   detail";
        var lines = await ScanAsync(source);
        Assert.Equal(0, lines[1].ListDepth);
        Assert.Equal(0, lines[4].ListDepth);
        Assert.Equal(lines[1].Source.Utf16Offset, lines[2].ListOwnerUtf16Offset);
        Assert.Equal(lines[4].Source.Utf16Offset, lines[5].ListOwnerUtf16Offset);
        Assert.Equal(lines[0].Source.Utf16Offset, lines[2].QuoteContainerListOwnerUtf16Offset);
        Assert.Equal(lines[3].Source.Utf16Offset, lines[5].QuoteContainerListOwnerUtf16Offset);
    }

    [Fact]
    public async Task QuotedBlankCanKeepProvenOuterOwnerWithoutInventingInnerOwner()
    {
        const string source =
            "- outer\n  > - item\n  >\n  >   continuation";
        var lines = await ScanAsync(source);
        Assert.Equal(MarkdownContainerLineKind.Blank, lines[2].Kind);
        Assert.Equal(lines[0].Source.Utf16Offset, lines[2].QuoteContainerListOwnerUtf16Offset);
        Assert.Null(lines[2].ListOwnerUtf16Offset);
        Assert.Equal(lines[1].Source.Utf16Offset, lines[3].ListOwnerUtf16Offset);
    }

    [Fact]
    public async Task UnicodeAndMixedLineEndingsPreserveExactIndependentOwnerOffsets()
    {
        const string source =
            "- 🌍\r\n  > - žlutý\r  >   text\n- other";
        var lines = await ScanAsync(source);
        var parsed = MarkdownParser.Parse(source);
        Assert.Equal(source, string.Concat(lines.Select(x => x.Source.Raw)));
        Assert.Equal(parsed.Blocks[1].Start, lines[1].Source.Utf16Offset);
        Assert.Equal(parsed.Blocks[2].Start, lines[2].Source.Utf16Offset);
        Assert.Equal(parsed.Blocks[2].QuoteContainerListOwnerStart,
            lines[2].QuoteContainerListOwnerUtf16Offset);
        Assert.Equal(parsed.Blocks[2].ListContinuationOwnerStart,
            lines[2].ListOwnerUtf16Offset);
    }

    [Fact]
    public async Task CompleteReaderDoesNotNeedToReadWholeFileBeforeFirstQuoteEvent()
    {
        using var reader = new OneCharReader("- outer\n  > quote\n" + new string('x', 50000));
        await using var iterator = MarkdownIncrementalContainerScanner.ScanAsync(reader)
            .GetAsyncEnumerator();

        Assert.True(await iterator.MoveNextAsync());
        Assert.True(await iterator.MoveNextAsync());
        Assert.Equal(MarkdownContainerLineKind.Quote, iterator.Current.Kind);
        Assert.Equal(0L, iterator.Current.QuoteContainerListOwnerUtf16Offset);
        Assert.InRange(reader.CharactersConsumed, 18, 20);
    }

    private static async Task<List<MarkdownContainerLine>> ScanAsync(string source)
    {
        using var reader = new StringReader(source);
        var lines = new List<MarkdownContainerLine>();
        await foreach (var line in MarkdownIncrementalContainerScanner.ScanAsync(reader))
            lines.Add(line);
        return lines;
    }

    private sealed class OneCharReader(string source) : TextReader
    {
        public int CharactersConsumed { get; private set; }

        public override ValueTask<int> ReadAsync(Memory<char> buffer,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (CharactersConsumed == source.Length)
                return ValueTask.FromResult(0);
            buffer.Span[0] = source[CharactersConsumed++];
            return ValueTask.FromResult(1);
        }
    }
}
