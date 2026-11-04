using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Xunit;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown.Tests;

public sealed class MarkdownIncrementalLineScannerTests
{
    [Fact]
    public async Task EmptyInputHasNoPhantomLine()
    {
        Assert.Empty(await ScanAsync(""));
    }

    [Fact]
    public async Task PhysicalEndingsAndSourceOffsetsRoundTripExactly()
    {
        const string original = "# Head\r\n\rParagraph\nFinal";
        var lines = await ScanAsync(original);
        Assert.Equal(4, lines.Count);
        Assert.Equal(original, string.Concat(lines.Select(line => line.Raw)));
        Assert.Equal(new[] { "\r\n", "\r", "\n", "" },
            lines.Select(line => line.Terminator).ToArray());
        Assert.Equal(new long[] { 0, 8, 9, 19 },
            lines.Select(line => line.Utf16Offset).ToArray());
        Assert.Equal(new[]
        {
            MarkdownStreamLineKind.Heading,
            MarkdownStreamLineKind.Blank,
            MarkdownStreamLineKind.ParagraphLine,
            MarkdownStreamLineKind.ParagraphLine
        }, lines.Select(line => line.Kind).ToArray());
    }

    [Fact]
    public async Task CrLfAcrossReaderChunksStaysOneTerminator()
    {
        const string source = "One\r\nTwo\nThree\rFour";
        using var reader = new OneCharReader(source);
        var lines = await CollectAsync(
            MarkdownIncrementalLineScanner.ScanAsync(reader));
        Assert.Equal(source, string.Concat(lines.Select(line => line.Raw)));
        Assert.Equal(new[] { "One", "Two", "Three", "Four" },
            lines.Select(line => line.Text).ToArray());
        Assert.Equal(new[] { "\r\n", "\n", "\r", "" },
            lines.Select(line => line.Terminator).ToArray());
    }

    [Fact]
    public async Task TopLevelFencesStayOpenUntilMatchingLongEnoughCloser()
    {
        const string source =
            "````javascript extra\n# not heading\n~~~\n```\n````  \n## Heading\n";
        var lines = await ScanAsync(source);
        Assert.Equal(new[]
        {
            MarkdownStreamLineKind.FenceOpen,
            MarkdownStreamLineKind.CodeLine,
            MarkdownStreamLineKind.CodeLine,
            MarkdownStreamLineKind.CodeLine,
            MarkdownStreamLineKind.FenceClose,
            MarkdownStreamLineKind.Heading
        }, lines.Select(line => line.Kind).ToArray());
        Assert.Equal("javascript", lines[0].CodeLanguage);
        Assert.Equal("javascript", lines[3].CodeLanguage);
        Assert.Equal("javascript", lines[4].CodeLanguage);
        Assert.Null(lines[5].CodeLanguage);
        Assert.Equal(2, lines[5].HeadingLevel);
        Assert.Equal(source, string.Concat(lines.Select(line => line.Raw)));
    }

    [Fact]
    public async Task UnterminatedFenceDoesNotDropAnyTrailingLine()
    {
        const string source = "~~~python\nprint('Hello')\n# still inside";
        var lines = await ScanAsync(source);
        Assert.Equal(new[]
        {
            MarkdownStreamLineKind.FenceOpen,
            MarkdownStreamLineKind.CodeLine,
            MarkdownStreamLineKind.CodeLine
        }, lines.Select(line => line.Kind));
        Assert.Equal(source, string.Concat(lines.Select(line => line.Raw)));
        Assert.Equal("python", lines[^1].CodeLanguage);
    }

    [Fact]
    public async Task QuoteAndListPrefixesRemainOpaqueNotIncorrectlyParsed()
    {
        const string source = "> # quotation\n- list item\n1. numbered\n> ```cs\n> code\n";
        var lines = await ScanAsync(source);
        Assert.All(lines, line => Assert.Equal(MarkdownStreamLineKind.OpaqueContainer,
            line.Kind));
        Assert.Equal(source, string.Concat(lines.Select(line => line.Raw)));
    }

    [Fact]
    public async Task ValidHeadingsMatchConservativeExistingParserForFlatSubset()
    {
        const string source = "# Installation\n## Build\n#### Details\n####### not heading\n#no space\n";
        var lines = await ScanAsync(source);
        var whole = MarkdownParser.Parse(source);
        Assert.Equal(whole.Blocks.Count, lines.Count);
        for (var i = 0; i < lines.Count; i++)
        {
            var isHeading = whole.Blocks[i].Kind == MarkdownBlockKind.Heading;
            Assert.Equal(isHeading, lines[i].Kind == MarkdownStreamLineKind.Heading);
        }
        Assert.Equal(new[] { 1, 2, 4, 0, 0 }, lines.Select(x => x.HeadingLevel));
    }

    [Fact]
    public async Task UnicodeOffsetsAreUtf16PositionsNotUtf8ByteOffsets()
    {
        const string source = "🌍\n# Head";
        var lines = await ScanAsync(source);
        Assert.Equal(new long[] { 0, 3 }, lines.Select(line => line.Utf16Offset));
        Assert.Equal("🌍\n", lines[0].Raw);
        Assert.Equal(MarkdownStreamLineKind.Heading, lines[1].Kind);
    }

    [Fact]
    public async Task StrictLineBudgetFailsExplicitlyInsteadOfTruncating()
    {
        using var reader = new StringReader("abcd\nefghij");
        var exception = await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            await foreach (var _ in MarkdownIncrementalLineScanner.ScanAsync(
                reader, maxLineCharacters: 4)) { }
        });
        Assert.Contains("line", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnsafeTerminalControlsAreRejected()
    {
        using var reader = new StringReader("Valid first\nSecond \u001b[31m");
        await using var enumerator = MarkdownIncrementalLineScanner.ScanAsync(reader)
            .GetAsyncEnumerator();
        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal("Valid first", enumerator.Current.Text);
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            await enumerator.MoveNextAsync();
        });
    }

    [Fact]
    public async Task YieldsFirstCompletedLineBeforeConsumingEntireSource()
    {
        var source = "A\n" + new string('x', 16000);
        using var reader = new OneCharReader(source);
        await using var enumerator = MarkdownIncrementalLineScanner.ScanAsync(reader)
            .GetAsyncEnumerator();

        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal("A", enumerator.Current.Text);
        Assert.InRange(reader.CharactersConsumed, 2, 3);
    }

    [Fact]
    public async Task PreservesTabsAndDoesNotInterpretSpectreTags()
    {
        const string text = "[red]literal[/]\n\t```not top-level fence\n";
        var lines = await ScanAsync(text);
        Assert.Equal(text, string.Concat(lines.Select(x => x.Raw)));
        Assert.Equal(MarkdownStreamLineKind.ParagraphLine, lines[0].Kind);
        Assert.Equal(MarkdownStreamLineKind.ParagraphLine, lines[1].Kind);
    }

    [Fact]
    public async Task PreCancelledTokenNeverReadsACharacter()
    {
        using var reader = new OneCharReader("# Never read");
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await CollectAsync(MarkdownIncrementalLineScanner.ScanAsync(
                reader, cancellationToken: cts.Token)));
        Assert.Equal(0, reader.CharactersConsumed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(4 * 1024 * 1024 + 1)]
    public async Task InvalidLineBudgetIsRejected(int limit)
    {
        using var reader = new StringReader("A");
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await CollectAsync(MarkdownIncrementalLineScanner.ScanAsync(
                reader, maxLineCharacters: limit)));
    }

    private static async Task<List<MarkdownStreamLine>> ScanAsync(string source)
    {
        using var reader = new StringReader(source);
        return await CollectAsync(MarkdownIncrementalLineScanner.ScanAsync(reader));
    }

    private static async Task<List<MarkdownStreamLine>> CollectAsync(
        IAsyncEnumerable<MarkdownStreamLine> source)
    {
        var result = new List<MarkdownStreamLine>();
        await foreach (var line in source)
            result.Add(line);
        return result;
    }

    private sealed class OneCharReader(string input) : TextReader
    {
        public int CharactersConsumed { get; private set; }

        public override ValueTask<int> ReadAsync(Memory<char> buffer,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (CharactersConsumed == input.Length)
                return ValueTask.FromResult(0);
            buffer.Span[0] = input[CharactersConsumed++];
            return ValueTask.FromResult(1);
        }
    }
}
