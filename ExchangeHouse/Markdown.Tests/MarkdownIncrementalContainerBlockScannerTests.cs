using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Xunit;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown.Tests;

public sealed class MarkdownIncrementalContainerBlockScannerTests
{
    [Fact]
    public async Task EmptySourceYieldsNoSpans()
    {
        Assert.Empty(await ScanAsync(""));
    }

    [Fact]
    public async Task MixedLinesCoverEveryUtf16CharacterExactlyOnce()
    {
        const string source =
            "# Installation\r\nNormal text\nsecond line\r\n\n" +
            "- first\r  explicit detail\n- next\n" +
            "> quoted\r\n> other\n" +
            "~~~js\r\n# code literal\n~~~\n# End";
        var spans = await ScanAsync(source);
        Assert.Equal(new[]
        {
            MarkdownContainerSpanKind.Heading,
            MarkdownContainerSpanKind.PlainRun,
            MarkdownContainerSpanKind.BlankRun,
            MarkdownContainerSpanKind.ListItem,
            MarkdownContainerSpanKind.ListContentRun,
            MarkdownContainerSpanKind.ListItem,
            MarkdownContainerSpanKind.QuoteRun,
            MarkdownContainerSpanKind.FencedCode,
            MarkdownContainerSpanKind.Heading
        }, spans.Select(x => x.Kind));
        Assert.Equal(source, string.Concat(spans.Select(x => Extract(source, x))));
        Assert.Equal(source.Length, spans[^1].EndUtf16Offset);

        for (var i = 0; i < spans.Count; i++)
        {
            Assert.True(spans[i].Length > 0);
            Assert.True(spans[i].LineCount > 0);
            if (i == 0)
            {
                Assert.Equal(0, spans[i].StartUtf16Offset);
                Assert.Equal(0, spans[i].StartLine);
            }
            else
            {
                Assert.Equal(spans[i - 1].EndUtf16Offset, spans[i].StartUtf16Offset);
                Assert.Equal(spans[i - 1].EndLineExclusive, spans[i].StartLine);
            }
        }
        Assert.Equal(1, spans[0].HeadingLevel);
        Assert.Equal(1, spans[^1].HeadingLevel);
        Assert.Equal("js", spans[^2].CodeLanguage);
        Assert.Equal(MarkdownFenceSpanEnd.Closed, spans[^2].FenceEnd);
    }

    [Fact]
    public async Task ListItemMarkersAreSeparateFromOwnedContentAndSiblings()
    {
        const string source =
            "- parent\n  first detail\n  second detail\n" +
            "  - child\n    child detail\n- sibling\n";
        var spans = await ScanAsync(source);
        Assert.Equal(new[]
        {
            MarkdownContainerSpanKind.ListItem,
            MarkdownContainerSpanKind.ListContentRun,
            MarkdownContainerSpanKind.ListItem,
            MarkdownContainerSpanKind.ListContentRun,
            MarkdownContainerSpanKind.ListItem
        }, spans.Select(x => x.Kind));
        Assert.Equal(2, spans[1].LineCount);
        Assert.Equal(spans[0].StartUtf16Offset, spans[1].ListOwnerUtf16Offset);
        Assert.Equal(0, spans[1].ListDepth);
        Assert.Equal(1, spans[2].ListDepth);
        Assert.Equal(spans[2].StartUtf16Offset, spans[3].ListOwnerUtf16Offset);
        Assert.Equal(0, spans[4].ListDepth);
        Assert.Equal(source, string.Concat(spans.Select(x => Extract(source, x))));
    }

    [Fact]
    public async Task LazyAndExplicitContinuationsStaySeparate()
    {
        const string source =
            "- item\nunindented paragraph\n  indented paragraph\n";
        var spans = await ScanAsync(source);

        Assert.Equal(3, spans.Count);
        Assert.True(spans[1].IsLazyListContinuation);
        Assert.False(spans[2].IsLazyListContinuation);
        Assert.Equal(MarkdownContainerSpanKind.ListContentRun, spans[1].Kind);
        Assert.Equal(MarkdownContainerSpanKind.ListContentRun, spans[2].Kind);
        Assert.Equal(spans[0].StartUtf16Offset, spans[1].ListOwnerUtf16Offset);
        Assert.Equal(spans[0].StartUtf16Offset, spans[2].ListOwnerUtf16Offset);
    }

    [Fact]
    public async Task DeferredLooseListBlanksMergeOnlyWhenOwnerIsTheSame()
    {
        const string source = "- item\n\n\r\n  detail\n- next";
        var spans = await ScanAsync(source);
        var parsed = MarkdownParser.Parse(source);

        Assert.Equal(new[]
        {
            MarkdownContainerSpanKind.ListItem,
            MarkdownContainerSpanKind.BlankRun,
            MarkdownContainerSpanKind.ListContentRun,
            MarkdownContainerSpanKind.ListItem
        }, spans.Select(x => x.Kind));
        Assert.Equal(2, spans[1].LineCount);
        Assert.Equal(spans[0].StartUtf16Offset, spans[1].ListBlankOwnerUtf16Offset);
        Assert.Equal(spans[0].StartUtf16Offset, spans[2].ListOwnerUtf16Offset);
        Assert.Equal(parsed.Blocks[1].ListBlankOwnerStart,
            spans[1].ListBlankOwnerUtf16Offset);
        Assert.Equal(source, string.Concat(spans.Select(x => Extract(source, x))));
    }

    [Fact]
    public async Task UnresolvedBlanksNeverInventAListOwner()
    {
        const string source = "- first\n\n- second\n\n";
        var spans = await ScanAsync(source);
        Assert.Equal(new[]
        {
            MarkdownContainerSpanKind.ListItem,
            MarkdownContainerSpanKind.BlankRun,
            MarkdownContainerSpanKind.ListItem,
            MarkdownContainerSpanKind.BlankRun
        }, spans.Select(x => x.Kind));
        Assert.Null(spans[1].ListBlankOwnerUtf16Offset);
        Assert.Null(spans[3].ListBlankOwnerUtf16Offset);
        Assert.Equal(MarkdownFenceSpanEnd.NotApplicable, spans[3].FenceEnd);
    }

    [Fact]
    public async Task QuotedInnerListOwnerAndOuterOrdinaryListOwnerStayIndependent()
    {
        const string source =
            "- outer\n" +
            "  > - inner\n" +
            "  >   line one\n" +
            "  >   line two\n" +
            "  > - sibling\n";
        var spans = await ScanAsync(source);
        Assert.Equal(4, spans.Count);
        Assert.Equal(new[]
        {
            MarkdownContainerSpanKind.ListItem,
            MarkdownContainerSpanKind.ListItem,
            MarkdownContainerSpanKind.ListContentRun,
            MarkdownContainerSpanKind.ListItem
        }, spans.Select(x => x.Kind));
        Assert.All(spans.Skip(1), span =>
        {
            Assert.Equal(1, span.QuoteDepth);
            Assert.Equal(spans[0].StartUtf16Offset,
                span.QuoteContainerListOwnerUtf16Offset);
        });
        Assert.Equal(2, spans[2].LineCount);
        Assert.Equal(spans[1].StartUtf16Offset, spans[2].ListOwnerUtf16Offset);
        Assert.Equal(spans[3].StartUtf16Offset, spans[3].ListOwnerUtf16Offset);
    }

    [Fact]
    public async Task ExplicitQuoteDepthChangesSplitRunsEvenWhenOtherMetadataMatches()
    {
        const string source = "> first\n> second\n>> nested\n> again\n";
        var spans = await ScanAsync(source);
        Assert.Equal(3, spans.Count);
        Assert.All(spans, x => Assert.Equal(MarkdownContainerSpanKind.QuoteRun, x.Kind));
        Assert.Equal(new[] { 1, 2, 1 }, spans.Select(x => x.QuoteDepth));
        Assert.Equal(2, spans[0].LineCount);
        Assert.Equal(source, string.Concat(spans.Select(x => Extract(source, x))));
    }

    [Fact]
    public async Task MatchedQuotedFenceIsOneSpanIncludingBothDelimiters()
    {
        const string source =
            "- outer\n  > - inner\n  >   ```csharp\n" +
            "  >   # not a heading\n  >   - not an item\n  >   ```\n" +
            "  > - next";
        var spans = await ScanAsync(source);
        var fence = Assert.Single(spans.Where(x =>
            x.Kind == MarkdownContainerSpanKind.FencedCode));

        Assert.Equal(4, fence.LineCount);
        Assert.Equal(MarkdownFenceSpanEnd.Closed, fence.FenceEnd);
        Assert.Equal(fence.StartUtf16Offset, fence.FenceStartUtf16Offset);
        Assert.Equal("csharp", fence.CodeLanguage);
        Assert.Equal(1, fence.QuoteDepth);
        Assert.Equal(spans[0].StartUtf16Offset, fence.QuoteContainerListOwnerUtf16Offset);
        Assert.Equal(spans[1].StartUtf16Offset, fence.ListOwnerUtf16Offset);
        Assert.Equal(source, string.Concat(spans.Select(x => Extract(source, x))));
        Assert.Equal(source, MarkdownWriter.Write(MarkdownParser.Parse(source)));
    }

    [Fact]
    public async Task QuoteDepthMismatchInterruptsFenceBeforeTheFirstDifferentLine()
    {
        const string source =
            "> ```js\n> inside\n>> - a separate list\n> normal";
        var spans = await ScanAsync(source);
        Assert.Equal(MarkdownContainerSpanKind.FencedCode, spans[0].Kind);
        Assert.Equal(MarkdownFenceSpanEnd.Interrupted, spans[0].FenceEnd);
        Assert.Equal(2, spans[0].LineCount);
        Assert.Equal(MarkdownContainerSpanKind.ListItem, spans[1].Kind);
        Assert.Null(spans[1].FenceStartUtf16Offset);
        Assert.Equal(source, string.Concat(spans.Select(x => Extract(source, x))));
    }

    [Fact]
    public async Task ChangingOuterItemInterruptsQuoteOwnedFence()
    {
        const string source =
            "- first\n  > ~~~text\n  > literal\n" +
            "- second\n  > ordinary quote";
        var spans = await ScanAsync(source);
        var fence = Assert.Single(spans.Where(x =>
            x.Kind == MarkdownContainerSpanKind.FencedCode));
        Assert.Equal(MarkdownFenceSpanEnd.Interrupted, fence.FenceEnd);
        Assert.Equal(2, fence.LineCount);
        var marker = spans.Single(x =>
            x.Kind == MarkdownContainerSpanKind.ListItem &&
            x.StartUtf16Offset > fence.StartUtf16Offset);
        Assert.Equal("- second\n", Extract(source, marker));
        Assert.Equal(source, string.Concat(spans.Select(x => Extract(source, x))));
    }

    [Fact]
    public async Task UnterminatedFenceAtEofHasExplicitTerminationState()
    {
        const string source = "Some text\n~~~python\nprint(42)";
        var spans = await ScanAsync(source);
        Assert.Equal(2, spans.Count);
        Assert.Equal(MarkdownContainerSpanKind.FencedCode, spans[1].Kind);
        Assert.Equal(MarkdownFenceSpanEnd.EndOfFile, spans[1].FenceEnd);
        Assert.Equal("python", spans[1].CodeLanguage);
        Assert.Equal(source, string.Concat(spans.Select(x => Extract(source, x))));
    }

    [Fact]
    public async Task TopLevelFenceTreatsAllQuoteAndListCharactersAsLiteral()
    {
        const string source = "```text\n> quote\n- list\n# heading\n```\n# Actual";
        var spans = await ScanAsync(source);
        Assert.Equal(2, spans.Count);
        Assert.Equal(MarkdownContainerSpanKind.FencedCode, spans[0].Kind);
        Assert.Equal(5, spans[0].LineCount);
        Assert.Equal(MarkdownFenceSpanEnd.Closed, spans[0].FenceEnd);
        Assert.Equal(MarkdownContainerSpanKind.Heading, spans[1].Kind);
    }

    [Fact]
    public async Task LargeParagraphAndCodeRangesStoreOnlyMetadata()
    {
        var source = "# Head\n" + string.Concat(
            Enumerable.Repeat("one line\n", 12000)) +
            "```txt\n" + string.Concat(Enumerable.Repeat("long code line\n", 9000)) +
            "```";
        var spans = await ScanAsync(source);
        Assert.Equal(3, spans.Count);
        Assert.Equal(12000, spans[1].LineCount);
        Assert.Equal(9002, spans[2].LineCount);
        Assert.Equal(MarkdownFenceSpanEnd.Closed, spans[2].FenceEnd);
        Assert.Equal(source.Length, spans[^1].EndUtf16Offset);
        Assert.Equal(source, string.Concat(spans.Select(x => Extract(source, x))));
    }

    [Fact]
    public async Task UnicodeAndMixedTerminatorsPreserveExactDecodedOffsets()
    {
        const string source = "🌍\r\n> žluťoučký\r> další\n- item";
        var spans = await ScanAsync(source);
        Assert.Equal(source, string.Concat(spans.Select(x => Extract(source, x))));
        var sourceLineNumber = 0L;
        foreach (var span in spans)
        {
            Assert.Equal(sourceLineNumber, span.StartLine);
            sourceLineNumber = span.EndLineExclusive;
        }
        Assert.Equal(source.Length, spans[^1].EndUtf16Offset);
    }

    [Fact]
    public async Task FirstHeadingIsEmittedWithoutConsumingTheEntireDocument()
    {
        using var reader = new OneCharReader("# First\n# Second\n" +
            new string('x', 60000));
        await using var enumerator = MarkdownIncrementalContainerBlockScanner
            .ScanAsync(reader).GetAsyncEnumerator();
        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal(MarkdownContainerSpanKind.Heading, enumerator.Current.Kind);
        Assert.Equal("# First\n", "# First\n");
        Assert.InRange(reader.CharactersConsumed, 16, 18);
    }

    [Fact]
    public async Task DeferredBlankBudgetsStillApplyToBlockScanner()
    {
        using var reader = new StringReader("- item\n\n\n");
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            await CollectAsync(MarkdownIncrementalContainerBlockScanner.ScanAsync(
                reader, maxPendingBlankLines: 1));
        });
    }

    [Fact]
    public async Task CancellationBeforeReadLeavesReaderUntouched()
    {
        using var reader = new OneCharReader("# Title");
        using var source = new CancellationTokenSource();
        source.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await CollectAsync(MarkdownIncrementalContainerBlockScanner.ScanAsync(
                reader, cancellationToken: source.Token));
        });
        Assert.Equal(0, reader.CharactersConsumed);
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 1, 0)]
    [InlineData(4 * 1024 * 1024 + 1, 1, 1)]
    public async Task InvalidResourceBudgetsFailBeforeReading(
        int lineCharacters, int blankLines, int blankCharacters)
    {
        using var reader = new OneCharReader("Not read");
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await CollectAsync(MarkdownIncrementalContainerBlockScanner.ScanAsync(
                reader, maxLineCharacters: lineCharacters,
                maxPendingBlankLines: blankLines,
                maxPendingBlankCharacters: blankCharacters));
        });
        Assert.Equal(0, reader.CharactersConsumed);
    }

    private static string Extract(string source, MarkdownContainerBlockSpan span) =>
        source.Substring(checked((int)span.StartUtf16Offset), checked((int)span.Length));

    private static async Task<List<MarkdownContainerBlockSpan>> ScanAsync(string source)
    {
        using var reader = new StringReader(source);
        return await CollectAsync(MarkdownIncrementalContainerBlockScanner.ScanAsync(reader));
    }

    private static async Task<List<MarkdownContainerBlockSpan>> CollectAsync(
        IAsyncEnumerable<MarkdownContainerBlockSpan> sequence)
    {
        var spans = new List<MarkdownContainerBlockSpan>();
        await foreach (var span in sequence)
            spans.Add(span);
        return spans;
    }

    private sealed class OneCharReader(string source) : TextReader
    {
        public int CharactersConsumed { get; private set; }

        public override ValueTask<int> ReadAsync(
            Memory<char> buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (CharactersConsumed == source.Length)
                return ValueTask.FromResult(0);
            buffer.Span[0] = source[CharactersConsumed++];
            return ValueTask.FromResult(1);
        }
    }
}
