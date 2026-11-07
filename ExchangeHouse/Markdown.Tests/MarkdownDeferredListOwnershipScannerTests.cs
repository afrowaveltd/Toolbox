using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Xunit;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown.Tests;

public sealed class MarkdownDeferredListOwnershipScannerTests
{
    [Fact]
    public async Task LooseOrdinaryListBlanksReceiveOwnerFromExplicitFollowingContinuation()
    {
        const string source = "- parent\r\n\n\r  continued\n";
        var lines = await ReadAsync(source);
        var legacy = MarkdownParser.Parse(source);

        Assert.Equal(source, string.Concat(lines.Select(x => x.Source.Raw)));
        Assert.Equal(4, lines.Count);
        Assert.Equal(MarkdownContainerLineKind.ListContinuation, lines[3].Kind);
        Assert.Equal(lines[0].Source.Utf16Offset, lines[3].ListOwnerUtf16Offset);
        Assert.All(lines.Skip(1).Take(2), line =>
        {
            Assert.Equal(lines[0].Source.Utf16Offset, line.ListBlankOwnerUtf16Offset);
            Assert.Equal(legacy.Blocks[(int)line.Source.LineNumber].ListBlankOwnerStart,
                line.ListBlankOwnerUtf16Offset);
        });
        Assert.Equal(legacy.Blocks[3].ListContinuationOwnerStart,
            lines[3].ListOwnerUtf16Offset);
    }

    [Fact]
    public async Task ConsecutiveQuotedBlanksAreAssignedOnlyWithinTheSameQuoteScope()
    {
        const string source = "> - quoted\n>\n> \n>   continuation\n";
        var lines = await ReadAsync(source);
        var legacy = MarkdownParser.Parse(source);

        Assert.Equal(new[] { 1, 1, 1, 1 }, lines.Select(x => x.QuoteDepth));
        Assert.All(lines.Skip(1).Take(2), line =>
        {
            Assert.Equal(lines[0].Source.Utf16Offset, line.ListBlankOwnerUtf16Offset);
            Assert.Equal(legacy.Blocks[(int)line.Source.LineNumber].ListBlankOwnerStart,
                line.ListBlankOwnerUtf16Offset);
        });
        Assert.Equal(lines[0].Source.Utf16Offset, lines[3].ListOwnerUtf16Offset);
        Assert.Equal(source, string.Concat(lines.Select(x => x.Source.Raw)));
    }

    [Fact]
    public async Task QuoteBlankKeepsOuterOwnerSeparateFromInnerBlankOwner()
    {
        const string source = "- outer\n  > - inner\n  >\n  >   inner detail";
        var lines = await ReadAsync(source);
        var legacy = MarkdownParser.Parse(source);

        Assert.Equal(lines[0].Source.Utf16Offset,
            lines[2].QuoteContainerListOwnerUtf16Offset);
        Assert.Equal(lines[1].Source.Utf16Offset,
            lines[2].ListBlankOwnerUtf16Offset);
        Assert.Equal(lines[0].Source.Utf16Offset,
            lines[3].QuoteContainerListOwnerUtf16Offset);
        Assert.Equal(lines[1].Source.Utf16Offset, lines[3].ListOwnerUtf16Offset);
        Assert.Equal(legacy.Blocks[2].ListBlankOwnerStart,
            lines[2].ListBlankOwnerUtf16Offset);
    }

    [Fact]
    public async Task BlankBeforeNewListItemDoesNotInheritPreviousItem()
    {
        const string source = "- one\n\n- two\n";
        var lines = await ReadAsync(source);
        Assert.Null(lines[1].ListBlankOwnerUtf16Offset);
        Assert.Equal(MarkdownContainerLineKind.ListItem, lines[2].Kind);
        Assert.Equal(0, lines[2].ListDepth);
    }

    [Fact]
    public async Task BlankBeforeUnindentedProseDoesNotCreateLazyOwner()
    {
        const string source = "- one\n\nlazy must stop\n  unrelated";
        var lines = await ReadAsync(source);
        Assert.Null(lines[1].ListBlankOwnerUtf16Offset);
        Assert.Equal(MarkdownContainerLineKind.Plain, lines[2].Kind);
        Assert.Equal(MarkdownContainerLineKind.Plain, lines[3].Kind);
        Assert.False(lines[2].IsLazyListContinuation);
    }

    [Fact]
    public async Task OrdinaryLazyContinuationWorksOnlyWithoutInterveningBlank()
    {
        const string source = "- item\ncontinued prose\n  explicit detail\n# Heading";
        var lines = await ReadAsync(source);
        var parsed = MarkdownParser.Parse(source);

        Assert.Equal(MarkdownContainerLineKind.ListContinuation, lines[1].Kind);
        Assert.True(lines[1].IsLazyListContinuation);
        Assert.Equal(lines[0].Source.Utf16Offset, lines[1].ListOwnerUtf16Offset);
        Assert.Equal(parsed.Blocks[1].ListContinuationOwnerStart,
            lines[1].ListOwnerUtf16Offset);
        Assert.Equal(parsed.Blocks[1].IsLazyListContinuation, lines[1].IsLazyListContinuation);
        Assert.False(lines[2].IsLazyListContinuation);
        Assert.Equal(MarkdownContainerLineKind.Plain, lines[3].Kind);
    }

    [Fact]
    public async Task QuotedLazyContinuationRetainsLocalOwnerAndOuterOwner()
    {
        const string source = "- outer\n  > - inner\n  > continued prose\n";
        var lines = await ReadAsync(source);
        var parsed = MarkdownParser.Parse(source);

        Assert.Equal(MarkdownContainerLineKind.ListContinuation, lines[2].Kind);
        Assert.True(lines[2].IsLazyListContinuation);
        Assert.Equal(lines[1].Source.Utf16Offset, lines[2].ListOwnerUtf16Offset);
        Assert.Equal(lines[0].Source.Utf16Offset,
            lines[2].QuoteContainerListOwnerUtf16Offset);
        Assert.Equal(parsed.Blocks[2].IsLazyListContinuation, lines[2].IsLazyListContinuation);
    }

    [Fact]
    public async Task NewHeadingAndFenceCannotBeLazyParagraphs()
    {
        const string source = "- item\n# Heading\n- next\n```text\nhello\n```";
        var lines = await ReadAsync(source);
        Assert.False(lines[1].IsLazyListContinuation);
        Assert.False(lines[3].IsLazyListContinuation);
        Assert.Equal(MarkdownContainerLineKind.FenceOpen, lines[3].Kind);
    }

    [Fact]
    public async Task ThematicBreakCannotBeLazyParagraph()
    {
        const string source = "- item\n* * *\n  not owned";
        var lines = await ReadAsync(source);
        Assert.False(lines[1].IsLazyListContinuation);
        Assert.Equal(MarkdownContainerLineKind.Plain, lines[1].Kind);
        Assert.Null(lines[2].ListOwnerUtf16Offset);
    }

    [Fact]
    public async Task QuoteDepthChangeRejectsPreviousPendingBlankOwner()
    {
        const string source = "> - item\n>\n>>   other";
        var lines = await ReadAsync(source);
        Assert.Null(lines[1].ListBlankOwnerUtf16Offset);
        Assert.Equal(2, lines[2].QuoteDepth);
        Assert.Null(lines[2].ListOwnerUtf16Offset);
    }

    [Fact]
    public async Task ChangedOuterQuoteOwnerRejectsPendingBlankOwner()
    {
        const string source = "- first\n  > - a\n  >\n- second\n  >   prose";
        var lines = await ReadAsync(source);
        Assert.Null(lines[2].ListBlankOwnerUtf16Offset);
        Assert.Equal(lines[3].Source.Utf16Offset,
            lines[4].QuoteContainerListOwnerUtf16Offset);
        Assert.Null(lines[4].ListOwnerUtf16Offset);
    }

    [Fact]
    public async Task TrailingBlankRunDoesNotAcquireAnInventedOwner()
    {
        const string source = "- one\n\n\n";
        var lines = await ReadAsync(source);
        Assert.Equal(source, string.Concat(lines.Select(x => x.Source.Raw)));
        Assert.All(lines.Skip(1), x => Assert.Null(x.ListBlankOwnerUtf16Offset));
    }

    [Fact]
    public async Task FirstNonBlankEventIsAvailableWithoutReadingTheWholeSource()
    {
        using var reader = new OneCharReader("- first\n" + new string('z', 60000));
        await using var iterator = MarkdownDeferredListOwnershipScanner.ScanAsync(reader)
            .GetAsyncEnumerator();
        Assert.True(await iterator.MoveNextAsync());
        Assert.Equal(MarkdownContainerLineKind.ListItem, iterator.Current.Kind);
        Assert.InRange(reader.CharactersConsumed, 8, 9);
    }

    [Fact]
    public async Task PendingRunEmitsOnlyAfterFollowingLineDecidesOwnership()
    {
        using var reader = new OneCharReader("- first\n\n  continued\n" +
            new string('z', 60000));
        await using var iterator = MarkdownDeferredListOwnershipScanner.ScanAsync(reader)
            .GetAsyncEnumerator();

        Assert.True(await iterator.MoveNextAsync());
        Assert.Equal(MarkdownContainerLineKind.ListItem, iterator.Current.Kind);
        Assert.True(await iterator.MoveNextAsync());
        Assert.Equal(MarkdownContainerLineKind.Blank, iterator.Current.Kind);
        Assert.Equal(0L, iterator.Current.ListBlankOwnerUtf16Offset);
        Assert.InRange(reader.CharactersConsumed, 21, 22);
    }

    [Fact]
    public async Task TooManyPendingBlanksFailByConfiguredCountBudget()
    {
        using var reader = new StringReader("- item\n\n\n");
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            await ReadAsync(reader, maxPendingBlankLines: 1);
        });
    }

    [Fact]
    public async Task LargeBlankLineFailsByAggregateCharacterBudget()
    {
        using var reader = new StringReader("- item\n" + new string(' ', 64) + "\n");
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            await ReadAsync(reader, maxPendingBlankCharacters: 16);
        });
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public async Task InvalidPendingBudgetsRejectedBeforeReading(int lines, int chars)
    {
        using var reader = new OneCharReader("# Title");
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await ReadAsync(reader, maxPendingBlankLines: lines,
                maxPendingBlankCharacters: chars);
        });
        Assert.Equal(0, reader.CharactersConsumed);
    }

    private static async Task<List<MarkdownContainerLine>> ReadAsync(string source)
    {
        using var reader = new StringReader(source);
        return await ReadAsync(reader);
    }

    private static async Task<List<MarkdownContainerLine>> ReadAsync(
        TextReader reader,
        int maxPendingBlankLines =
            MarkdownDeferredListOwnershipScanner.DefaultMaxPendingBlankLines,
        int maxPendingBlankCharacters =
            MarkdownDeferredListOwnershipScanner.DefaultMaxPendingBlankCharacters)
    {
        var result = new List<MarkdownContainerLine>();
        await foreach (var line in MarkdownDeferredListOwnershipScanner.ScanAsync(
            reader, maxPendingBlankLines: maxPendingBlankLines,
            maxPendingBlankCharacters: maxPendingBlankCharacters))
            result.Add(line);
        return result;
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
