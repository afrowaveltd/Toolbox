using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Xunit;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown.Tests;

public sealed class MarkdownIncrementalBlockScannerTests
{
    [Fact]
    public async Task EmptyDocumentDoesNotEmitAPhantomBlock()
    {
        Assert.Empty(await ScanAsync(""));
    }

    [Fact]
    public async Task SourceSpansCoverEveryCharacterExactlyOnceWithMixedEndings()
    {
        const string source =
            "# Installation\r\nFirst line\nsecond line\r\n\r- list\n> quoted\r\n" +
            "```javascript\r\n# literal heading\nconsole.log(42);\r\n```\n## Next";
        var blocks = await ScanAsync(source);

        Assert.Equal(new[]
        {
            MarkdownStreamBlockKind.Heading,
            MarkdownStreamBlockKind.ParagraphRun,
            MarkdownStreamBlockKind.Blank,
            MarkdownStreamBlockKind.OpaqueContainer,
            MarkdownStreamBlockKind.OpaqueContainer,
            MarkdownStreamBlockKind.FencedCode,
            MarkdownStreamBlockKind.Heading
        }, blocks.Select(block => block.Kind));

        Assert.Equal(source, string.Concat(blocks.Select(block => Extract(source, block))));
        Assert.Equal(source.Length, blocks[^1].EndUtf16Offset);
        Assert.Equal(1, blocks[0].HeadingLevel);
        Assert.Equal(2, blocks[^1].HeadingLevel);
        Assert.Equal("javascript", blocks[5].CodeLanguage);
        Assert.True(blocks[5].FenceClosed);

        for (var i = 1; i < blocks.Count; i++)
        {
            Assert.Equal(blocks[i - 1].EndUtf16Offset, blocks[i].StartUtf16Offset);
            Assert.Equal(blocks[i - 1].EndLineExclusive, blocks[i].StartLine);
        }
    }

    [Fact]
    public async Task ConsecutiveParagraphLinesAreGroupedWithoutRetainingTheirText()
    {
        var source = string.Join("\n", Enumerable.Repeat("small line", 20000));
        var blocks = await ScanAsync(source);
        var paragraph = Assert.Single(blocks);

        Assert.Equal(MarkdownStreamBlockKind.ParagraphRun, paragraph.Kind);
        Assert.Equal(20000, paragraph.LineCount);
        Assert.Equal(source.Length, paragraph.Length);
        Assert.Null(paragraph.CodeLanguage);
        Assert.Equal(source, Extract(source, paragraph));
    }

    [Fact]
    public async Task HugeFenceKeepsOneRangeIncludingBothDelimiters()
    {
        var source = "```python\n" +
            string.Concat(Enumerable.Repeat("print('hello')\n", 8000)) + "```\n# After";
        var blocks = await ScanAsync(source);

        Assert.Equal(2, blocks.Count);
        var fence = blocks[0];
        Assert.Equal(MarkdownStreamBlockKind.FencedCode, fence.Kind);
        Assert.True(fence.FenceClosed);
        Assert.Equal(8002, fence.LineCount);
        Assert.Equal("python", fence.CodeLanguage);
        Assert.Equal(MarkdownStreamBlockKind.Heading, blocks[1].Kind);
        Assert.Equal(source, string.Concat(blocks.Select(block => Extract(source, block))));
    }

    [Fact]
    public async Task UnterminatedFenceIsExplicitAndDoesNotTurnCodeIntoHeadings()
    {
        const string source = "Intro\n~~~sql\nSELECT 1;\n# still code";
        var blocks = await ScanAsync(source);

        Assert.Equal(2, blocks.Count);
        Assert.Equal(MarkdownStreamBlockKind.ParagraphRun, blocks[0].Kind);
        var fence = blocks[1];
        Assert.Equal(MarkdownStreamBlockKind.FencedCode, fence.Kind);
        Assert.Equal("sql", fence.CodeLanguage);
        Assert.False(fence.FenceClosed);
        Assert.Equal(3, fence.LineCount);
        Assert.Equal(source, string.Concat(blocks.Select(block => Extract(source, block))));
    }

    [Fact]
    public async Task QuoteAndListMarkersRemainOpaqueAndUnmerged()
    {
        const string source = "> # not top level\n- list\n1. ordered\n" +
            "> ```cs\n> literal\n# Actual heading";
        var blocks = await ScanAsync(source);

        Assert.Equal(6, blocks.Count);
        Assert.Equal(5, blocks.Count(block => block.Kind == MarkdownStreamBlockKind.OpaqueContainer));
        Assert.Equal(MarkdownStreamBlockKind.Heading, blocks[^1].Kind);
        Assert.Equal(source, string.Concat(blocks.Select(block => Extract(source, block))));
    }

    [Fact]
    public async Task ExactUtf16OffsetsRetainUnicodeAndCrLf()
    {
        const string source = "🌍\r\n## Build\n";
        var blocks = await ScanAsync(source);

        Assert.Equal(2, blocks.Count);
        Assert.Equal(0, blocks[0].StartUtf16Offset);
        Assert.Equal(4, blocks[0].EndUtf16Offset); // two UTF-16 units + CRLF
        Assert.Equal(4, blocks[1].StartUtf16Offset);
        Assert.Equal(source.Length, blocks[1].EndUtf16Offset);
        Assert.Equal(new long[] { 0, 1 }, blocks.Select(block => block.StartLine));
    }

    [Fact]
    public async Task SpanKindsAgreeWithExistingParserOnSupportedFlatSubset()
    {
        const string source =
            "# Installation\nFirst paragraph\nsecond paragraph\n\n" +
            "```csharp\nvar x = 1;\n# code not a title\n```\n## Usage\n";
        var streamBlocks = await ScanAsync(source);
        var original = MarkdownParser.Parse(source);

        Assert.Equal(source, string.Concat(streamBlocks.Select(block => Extract(source, block))));
        Assert.Equal(source, MarkdownWriter.Write(original));

        foreach (var span in streamBlocks)
        {
            var lines = original.Blocks.Where(line =>
                line.Start >= span.StartUtf16Offset &&
                line.Start < span.EndUtf16Offset).ToArray();
            Assert.Equal(span.LineCount, lines.Length);

            switch (span.Kind)
            {
                case MarkdownStreamBlockKind.Heading:
                    Assert.Equal(MarkdownBlockKind.Heading, Assert.Single(lines).Kind);
                    Assert.Equal(span.HeadingLevel, lines[0].HeadingLevel);
                    break;
                case MarkdownStreamBlockKind.Blank:
                    Assert.Equal(MarkdownBlockKind.Blank, Assert.Single(lines).Kind);
                    break;
                case MarkdownStreamBlockKind.ParagraphRun:
                    Assert.All(lines, line => Assert.Equal(MarkdownBlockKind.Paragraph, line.Kind));
                    break;
                case MarkdownStreamBlockKind.FencedCode:
                    Assert.Equal(MarkdownBlockKind.CodeFence, lines[0].Kind);
                    Assert.Equal(MarkdownBlockKind.CodeFence, lines[^1].Kind);
                    Assert.All(lines.Skip(1).SkipLast(1),
                        line => Assert.Equal(MarkdownBlockKind.CodeLine, line.Kind));
                    break;
                default:
                    throw new InvalidOperationException("Only the supported flat subset is compared.");
            }
        }
    }

    [Fact]
    public async Task FirstCompletedHeadingCanBeConsumedWithoutReadingTheWholeSource()
    {
        using var reader = new OneCharReader("# Ready\n" + new string('x', 50000));
        await using var iterator = MarkdownIncrementalBlockScanner.ScanAsync(reader)
            .GetAsyncEnumerator();

        Assert.True(await iterator.MoveNextAsync());
        Assert.Equal(MarkdownStreamBlockKind.Heading, iterator.Current.Kind);
        Assert.InRange(reader.CharactersConsumed, 8, 9);
    }

    [Fact]
    public async Task ParagraphNeedsAtMostOneAdditionalLineToFinish()
    {
        using var reader = new OneCharReader("first\nsecond\n# Title\n" +
            new string('x', 50000));
        await using var iterator = MarkdownIncrementalBlockScanner.ScanAsync(reader)
            .GetAsyncEnumerator();

        Assert.True(await iterator.MoveNextAsync());
        Assert.Equal(MarkdownStreamBlockKind.ParagraphRun, iterator.Current.Kind);
        Assert.Equal(2, iterator.Current.LineCount);
        Assert.InRange(reader.CharactersConsumed, 20, 21);
    }

    [Fact]
    public async Task AClosedFenceCanBeConsumedBeforeFollowingContent()
    {
        using var reader = new OneCharReader("```js\nconst x = 2;\n```\n" +
            new string('x', 50000));
        await using var iterator = MarkdownIncrementalBlockScanner.ScanAsync(reader)
            .GetAsyncEnumerator();

        Assert.True(await iterator.MoveNextAsync());
        Assert.Equal(MarkdownStreamBlockKind.FencedCode, iterator.Current.Kind);
        Assert.True(iterator.Current.FenceClosed);
        Assert.InRange(reader.CharactersConsumed, 22, 23);
    }

    [Fact]
    public async Task InvalidLateControlIsNotSilentlyAccepted()
    {
        using var reader = new StringReader("# Ready\n\u001b[31m");
        await using var iterator = MarkdownIncrementalBlockScanner.ScanAsync(reader)
            .GetAsyncEnumerator();

        Assert.True(await iterator.MoveNextAsync());
        Assert.Equal(MarkdownStreamBlockKind.Heading, iterator.Current.Kind);
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            await iterator.MoveNextAsync();
        });
    }

    [Fact]
    public async Task CancellationBeforeScanPreventsReading()
    {
        using var reader = new OneCharReader("# Title");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await CollectAsync(MarkdownIncrementalBlockScanner.ScanAsync(
                reader, cancellationToken: cancellation.Token));
        });
        Assert.Equal(0, reader.CharactersConsumed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(4 * 1024 * 1024 + 1)]
    public async Task InvalidLineBudgetFailsWithoutReading(int limit)
    {
        using var reader = new OneCharReader("Text");
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await CollectAsync(MarkdownIncrementalBlockScanner.ScanAsync(
                reader, maxLineCharacters: limit));
        });
        Assert.Equal(0, reader.CharactersConsumed);
    }

    private static string Extract(string source, MarkdownStreamBlock span) =>
        source.Substring(checked((int)span.StartUtf16Offset), checked((int)span.Length));

    private static async Task<List<MarkdownStreamBlock>> ScanAsync(string source)
    {
        using var reader = new StringReader(source);
        return await CollectAsync(MarkdownIncrementalBlockScanner.ScanAsync(reader));
    }

    private static async Task<List<MarkdownStreamBlock>> CollectAsync(
        IAsyncEnumerable<MarkdownStreamBlock> source)
    {
        var blocks = new List<MarkdownStreamBlock>();
        await foreach (var block in source)
            blocks.Add(block);
        return blocks;
    }

    private sealed class OneCharReader(string text) : TextReader
    {
        /// <summary>Number of decoded UTF-16 characters consumed by the scanner.</summary>
        public int CharactersConsumed { get; private set; }

        public override ValueTask<int> ReadAsync(Memory<char> buffer,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (CharactersConsumed >= text.Length)
                return ValueTask.FromResult(0);
            buffer.Span[0] = text[CharactersConsumed++];
            return ValueTask.FromResult(1);
        }
    }
}
