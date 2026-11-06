using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Xunit;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown.Tests;

public sealed class MarkdownIncrementalContainerScannerTests
{
    [Fact]
    public async Task EmptyInputProducesNoEvents()
    {
        Assert.Empty(await ScanAsync(""));
    }

    [Fact]
    public async Task ExplicitNestedQuotesRetainDepthAndRawSource()
    {
        const string source = "> One\r\n>> Two\n> > Three\r> Four\nOutside";
        var lines = await ScanAsync(source);

        Assert.Equal(source, string.Concat(lines.Select(x => x.Source.Raw)));
        Assert.Equal(new[] { 1, 2, 2, 1, 0 }, lines.Select(x => x.QuoteDepth));
        Assert.Equal(new[]
        {
            MarkdownContainerLineKind.Quote,
            MarkdownContainerLineKind.Quote,
            MarkdownContainerLineKind.Quote,
            MarkdownContainerLineKind.Quote,
            MarkdownContainerLineKind.Plain
        }, lines.Select(x => x.Kind));
        Assert.Equal(new long[] { 0, 7, 14, 25, 32 },
            lines.Select(x => x.Source.Utf16Offset));
    }

    [Fact]
    public async Task NestedListMarkersAndExplicitContinuationsTrackCorrectOwners()
    {
        const string source =
            "- parent\n  - child\n    child detail\n  parent detail\n- sibling\n";
        var lines = await ScanAsync(source);

        Assert.Equal(new[]
        {
            MarkdownContainerLineKind.ListItem,
            MarkdownContainerLineKind.ListItem,
            MarkdownContainerLineKind.ListContinuation,
            MarkdownContainerLineKind.ListContinuation,
            MarkdownContainerLineKind.ListItem
        }, lines.Select(x => x.Kind));
        Assert.Equal(new[] { 0, 1, 1, 0, 0 }, lines.Select(x => x.ListDepth));
        Assert.Equal(new[] { "-", "-", null, null, "-" },
            lines.Select(x => x.ListMarker));
        Assert.Equal(lines[1].Source.Utf16Offset, lines[2].ListOwnerUtf16Offset);
        Assert.Equal(lines[0].Source.Utf16Offset, lines[3].ListOwnerUtf16Offset);
        Assert.Equal(lines[4].Source.Utf16Offset, lines[4].ListOwnerUtf16Offset);
    }

    [Fact]
    public async Task ChangingQuoteDepthIsolatesListNesting()
    {
        const string source =
            "> - one\n>   - nested\n>> - different depth\n>>   continuation\n> - fresh\n";
        var lines = await ScanAsync(source);

        Assert.Equal(new[] { 1, 1, 2, 2, 1 }, lines.Select(x => x.QuoteDepth));
        Assert.Equal(new[] { 0, 1, 0, 0, 0 }, lines.Select(x => x.ListDepth));
        Assert.Equal(MarkdownContainerLineKind.ListContinuation, lines[3].Kind);
        Assert.Equal(lines[2].Source.Utf16Offset, lines[3].ListOwnerUtf16Offset);
        Assert.Null(lines[4].FenceStartUtf16Offset);
    }

    [Fact]
    public async Task QuotedFenceKeepsCodeAndMarkersLiteralUntilMatchingCloser()
    {
        const string source =
            "> ~~~~javascript\r\n> # not a heading\n> - not a list\n> ~~~\n> ~~~~  \n# real heading";
        var lines = await ScanAsync(source);

        Assert.Equal(new[]
        {
            MarkdownContainerLineKind.FenceOpen,
            MarkdownContainerLineKind.CodeLine,
            MarkdownContainerLineKind.CodeLine,
            MarkdownContainerLineKind.CodeLine,
            MarkdownContainerLineKind.FenceClose,
            MarkdownContainerLineKind.Plain
        }, lines.Select(x => x.Kind));
        Assert.Equal("javascript", lines[0].CodeLanguage);
        Assert.All(lines.Take(5), x => Assert.Equal(lines[0].Source.Utf16Offset,
            x.FenceStartUtf16Offset));
        Assert.All(lines.Take(5), x => Assert.Equal(1, x.QuoteDepth));
        Assert.All(lines.Skip(1).Take(4), x => Assert.Equal("javascript", x.CodeLanguage));
        Assert.Equal(source, string.Concat(lines.Select(x => x.Source.Raw)));
    }

    [Fact]
    public async Task QuotedFenceEndsOnDepthMismatchAndReclassifiesCurrentLine()
    {
        const string source = "> ```cs\n> literal\n>> - nested quote list\n- outside";
        var lines = await ScanAsync(source);

        Assert.Equal(new[]
        {
            MarkdownContainerLineKind.FenceOpen,
            MarkdownContainerLineKind.CodeLine,
            MarkdownContainerLineKind.ListItem,
            MarkdownContainerLineKind.ListItem
        }, lines.Select(x => x.Kind));
        Assert.Equal(new[] { 1, 1, 2, 0 }, lines.Select(x => x.QuoteDepth));
        Assert.Null(lines[2].FenceStartUtf16Offset);
        Assert.Null(lines[3].FenceStartUtf16Offset);
    }

    [Fact]
    public async Task TopLevelFenceProtectsEveryMarkerInsideIt()
    {
        const string source =
            "```csharp\n> quote\n  - list\n# heading\n```\n- actual";
        var lines = await ScanAsync(source);
        Assert.Equal(new[]
        {
            MarkdownContainerLineKind.FenceOpen,
            MarkdownContainerLineKind.CodeLine,
            MarkdownContainerLineKind.CodeLine,
            MarkdownContainerLineKind.CodeLine,
            MarkdownContainerLineKind.FenceClose,
            MarkdownContainerLineKind.ListItem
        }, lines.Select(x => x.Kind));
        Assert.All(lines.Take(5), x => Assert.Equal(0, x.QuoteDepth));
    }

    [Fact]
    public async Task QuotedListOwnedFenceProvidesExplicitOwnerMetadata()
    {
        const string source =
            "> - intro\n>   ```json\n>   {\"ok\":true}\n>   ```\n";
        var lines = await ScanAsync(source);
        Assert.Equal(MarkdownContainerLineKind.ListItem, lines[0].Kind);
        Assert.Equal(MarkdownContainerLineKind.FenceOpen, lines[1].Kind);
        Assert.Equal(MarkdownContainerLineKind.CodeLine, lines[2].Kind);
        Assert.Equal(MarkdownContainerLineKind.FenceClose, lines[3].Kind);
        Assert.Equal(0, lines[1].ListDepth);
        Assert.All(lines.Skip(1), x =>
            Assert.Equal(lines[0].Source.Utf16Offset, x.ListOwnerUtf16Offset));
    }

    [Fact]
    public async Task StaleListOwnerCannotSurviveAnUnrelatedParagraph()
    {
        const string source = "- item\n\nplain unrelated\n  should not be attached";
        var lines = await ScanAsync(source);
        Assert.Equal(MarkdownContainerLineKind.ListItem, lines[0].Kind);
        Assert.Equal(MarkdownContainerLineKind.Blank, lines[1].Kind);
        Assert.Equal(MarkdownContainerLineKind.Plain, lines[2].Kind);
        Assert.Equal(MarkdownContainerLineKind.Plain, lines[3].Kind);
        Assert.Null(lines[3].ListOwnerUtf16Offset);
    }

    [Fact]
    public async Task OrderedMarkersWithNineDigitsAreAcceptedButTenAreNot()
    {
        const string source = "123456789. valid\n1234567890. invalid\n1) again";
        var lines = await ScanAsync(source);
        Assert.Equal(MarkdownContainerLineKind.ListItem, lines[0].Kind);
        Assert.Equal("123456789.", lines[0].ListMarker);
        Assert.Equal(MarkdownContainerLineKind.Plain, lines[1].Kind);
        Assert.Equal(MarkdownContainerLineKind.ListItem, lines[2].Kind);
        Assert.Equal("1)", lines[2].ListMarker);
    }

    [Fact]
    public async Task TabsPreserveSourceAndUseFourColumnIndentStops()
    {
        const string source = "- item\n\tchild\n- after";
        var lines = await ScanAsync(source);
        Assert.Equal(MarkdownContainerLineKind.ListContinuation, lines[1].Kind);
        Assert.Equal(lines[0].Source.Utf16Offset, lines[1].ListOwnerUtf16Offset);
        Assert.Equal(source, string.Concat(lines.Select(x => x.Source.Raw)));
    }

    [Fact]
    public async Task ExplicitMarkerParityWithCurrentFullParserForSupportedCases()
    {
        const string source =
            "- root\n  - child\n    detail\n- second\n" +
            "> - quoted root\n>   - quoted child\n> ```js\n> const x = 1;\n> ```\n";
        var scanned = await ScanAsync(source);
        var parsed = MarkdownParser.Parse(source);

        Assert.Equal(scanned.Count, parsed.Blocks.Count);
        Assert.Equal(source, MarkdownWriter.Write(parsed));
        for (var i = 0; i < scanned.Count; i++)
        {
            Assert.Equal(parsed.Blocks[i].Start, scanned[i].Source.Utf16Offset);
            if (scanned[i].Kind == MarkdownContainerLineKind.ListItem)
            {
                Assert.Equal(MarkdownBlockKind.ListItem, parsed.Blocks[i].Kind);
                Assert.Equal(scanned[i].ListMarker, parsed.Blocks[i].ListMarker?.OriginalMarker);
                Assert.Equal(scanned[i].ListDepth, parsed.Blocks[i].ListMarker?.Depth);
            }
            else if (scanned[i].Kind == MarkdownContainerLineKind.FenceOpen ||
                     scanned[i].Kind == MarkdownContainerLineKind.FenceClose)
                Assert.Equal(MarkdownBlockKind.CodeFence, parsed.Blocks[i].Kind);
            else if (scanned[i].Kind == MarkdownContainerLineKind.CodeLine)
                Assert.Equal(MarkdownBlockKind.CodeLine, parsed.Blocks[i].Kind);
        }
    }

    [Fact]
    public async Task EventsRemainIncrementalOnLargeInput()
    {
        using var reader = new OneCharReader("> - first\n" + new string('a', 60000));
        await using var iterator = MarkdownIncrementalContainerScanner.ScanAsync(reader)
            .GetAsyncEnumerator();

        Assert.True(await iterator.MoveNextAsync());
        Assert.Equal(MarkdownContainerLineKind.ListItem, iterator.Current.Kind);
        Assert.InRange(reader.CharactersConsumed, 10, 11);
    }

    [Fact]
    public async Task InvalidUnsafeControlStillThrowsAfterPriorCompletedLine()
    {
        using var reader = new StringReader("- first\n\u001b[31m");
        await using var iterator = MarkdownIncrementalContainerScanner.ScanAsync(reader)
            .GetAsyncEnumerator();
        Assert.True(await iterator.MoveNextAsync());
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            await iterator.MoveNextAsync();
        });
    }

    [Fact]
    public async Task PreCancelledInputIsNotRead()
    {
        using var reader = new OneCharReader("- item");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await CollectAsync(MarkdownIncrementalContainerScanner.ScanAsync(
                reader, cancellationToken: cancellation.Token));
        });
        Assert.Equal(0, reader.CharactersConsumed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(4 * 1024 * 1024 + 1)]
    public async Task BadLineBudgetsAreRejected(int limit)
    {
        using var reader = new OneCharReader("Nothing");
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await CollectAsync(MarkdownIncrementalContainerScanner.ScanAsync(
                reader, maxLineCharacters: limit));
        });
        Assert.Equal(0, reader.CharactersConsumed);
    }

    private static async Task<List<MarkdownContainerLine>> ScanAsync(string text)
    {
        using var reader = new StringReader(text);
        return await CollectAsync(MarkdownIncrementalContainerScanner.ScanAsync(reader));
    }

    private static async Task<List<MarkdownContainerLine>> CollectAsync(
        IAsyncEnumerable<MarkdownContainerLine> input)
    {
        var lines = new List<MarkdownContainerLine>();
        await foreach (var line in input)
            lines.Add(line);
        return lines;
    }

    private sealed class OneCharReader(string text) : TextReader
    {
        public int CharactersConsumed { get; private set; }

        public override ValueTask<int> ReadAsync(Memory<char> buffer,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (CharactersConsumed == text.Length)
                return ValueTask.FromResult(0);
            buffer.Span[0] = text[CharactersConsumed++];
            return ValueTask.FromResult(1);
        }
    }
}
