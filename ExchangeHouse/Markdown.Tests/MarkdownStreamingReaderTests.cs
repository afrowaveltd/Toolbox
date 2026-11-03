using Xunit;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown.Tests;

public sealed class MarkdownStreamingReaderTests
{
    [Fact]
    public async Task Preserves_all_source_characters_and_global_offsets_across_one_character_reads()
    {
        const string source = "# Heading\r\nParagraph\n\n- List\r- Next\r\n\n" +
            "~~~csharp\n# literal\n\n~~~\r\n\n## Last";
        var lines = await CollectAsync(source, new MarkdownStreamOptions { ReadBufferChars = 1 });

        Assert.Equal(source, string.Concat(lines.Select(line => line.RawText)));
        long expected = 0;
        foreach (var line in lines)
        {
            Assert.Equal(expected, line.Start);
            expected += line.Length;
        }
        Assert.Equal(source.Length, expected);
        Assert.Contains(lines, line => line.Kind == MarkdownStreamLineKind.CodeContent &&
            line.Content == "# literal");
        Assert.Contains(lines, line => line.Kind == MarkdownStreamLineKind.CodeContent &&
            line.Content == "");
    }

    [Fact]
    public async Task Keeps_open_code_fence_across_arbitrarily_many_lines_without_accumulating_it()
    {
        const int count = 50_000;
        using var reader = new RepeatingFenceReader(count);
        var open = 0;
        var contentLines = 0;
        var close = 0;
        long offset = 0;

        await foreach (var line in MarkdownStreamingReader.ScanAsync(reader,
            new MarkdownStreamOptions { MaxLineChars = 64, ReadBufferChars = 3 }))
        {
            Assert.Equal(offset, line.Start);
            offset += line.Length;
            switch (line.Kind)
            {
                case MarkdownStreamLineKind.CodeFenceOpen:
                    open++;
                    Assert.Equal("bash", line.CodeLanguage);
                    break;
                case MarkdownStreamLineKind.CodeContent:
                    contentLines++;
                    Assert.Equal("bash", line.CodeLanguage);
                    Assert.Equal(0L, line.OpenFenceStart);
                    break;
                case MarkdownStreamLineKind.CodeFenceClose:
                    close++;
                    break;
                default:
                    throw new Xunit.Sdk.XunitException("An in-fence line was misclassified.");
            }
        }

        Assert.Equal(1, open);
        Assert.Equal(count, contentLines);
        Assert.Equal(1, close);
        Assert.Equal(reader.TotalChars, offset);
    }

    [Fact]
    public async Task Quote_owned_fence_does_not_consume_unrelated_document()
    {
        const string source = "> ~~~csharp\n> code\n> \n# Heading\n" +
            "~~~\nnot quoted\n~~~\n";
        var lines = await CollectAsync(source, new MarkdownStreamOptions
        {
            ReadBufferChars = 2
        });

        Assert.Equal(MarkdownStreamLineKind.CodeFenceOpen, lines[0].Kind);
        Assert.Equal(1, lines[1].QuoteDepth);
        Assert.Equal(MarkdownStreamLineKind.CodeContent, lines[2].Kind);
        Assert.Equal(MarkdownStreamLineKind.Markdown, lines[3].Kind);
        Assert.Equal(MarkdownStreamLineKind.CodeFenceOpen, lines[4].Kind);
        Assert.Equal(MarkdownStreamLineKind.CodeContent, lines[5].Kind);
        Assert.Equal(MarkdownStreamLineKind.CodeFenceClose, lines[6].Kind);
        Assert.Equal(source, string.Concat(lines.Select(line => line.RawText)));
    }

    [Fact]
    public async Task Tracks_bounded_list_depth_and_does_not_parse_fence_content_as_items()
    {
        const string source = "- Parent\n  - Child\n~~~md\n- not a list\n~~~\n";
        var lines = await CollectAsync(source, new MarkdownStreamOptions());
        Assert.Equal(1, lines[0].ListDepth);
        Assert.Equal(2, lines[1].ListDepth);
        Assert.Equal(MarkdownStreamLineKind.CodeContent, lines[3].Kind);
        Assert.Equal(2, lines[3].ListDepth);
    }

    [Fact]
    public async Task Rejects_a_single_unbounded_physical_line_not_a_long_code_block()
    {
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            using var reader = new StringReader(new string('z', 65));
            await foreach (var _ in MarkdownStreamingReader.ScanAsync(reader,
                new MarkdownStreamOptions { MaxLineChars = 64, ReadBufferChars = 7 }))
            {
            }
        });
    }

    [Fact]
    public async Task Honors_cancellation_even_with_virtual_reader()
    {
        using var reader = new StringReader("# Heading\n");
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in MarkdownStreamingReader.ScanAsync(
                reader, cancellationToken: cts.Token))
            {
            }
        });
    }

    [Fact]
    public async Task Empty_reader_does_not_emit_an_artificial_line()
    {
        var lines = await CollectAsync("", new MarkdownStreamOptions());
        Assert.Empty(lines);
    }

    private static async Task<List<MarkdownStreamLine>> CollectAsync(
        string content, MarkdownStreamOptions options)
    {
        var lines = new List<MarkdownStreamLine>();
        using var reader = new StringReader(content);
        await foreach (var line in MarkdownStreamingReader.ScanAsync(reader, options))
            lines.Add(line);
        return lines;
    }

    /// <summary>Virtual reader for one long fenced block, without the block in RAM.</summary>
    private sealed class RepeatingFenceReader : TextReader
    {
        private const string Open = "~~~bash\n";
        private const string Body = "code line\n";
        private const string Close = "~~~\n";
        private readonly int _bodyCount;
        private long _position;

        public RepeatingFenceReader(int bodyCount) => _bodyCount = bodyCount;

        public long TotalChars => Open.Length + (long)_bodyCount * Body.Length + Close.Length;

        public override ValueTask<int> ReadAsync(
            Memory<char> buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var written = 0;
            while (written < buffer.Length && _position < TotalChars)
            {
                string fragment;
                int index;
                if (_position < Open.Length)
                {
                    fragment = Open;
                    index = (int)_position;
                }
                else if (_position < Open.Length + (long)_bodyCount * Body.Length)
                {
                    fragment = Body;
                    index = (int)((_position - Open.Length) % Body.Length);
                }
                else
                {
                    fragment = Close;
                    index = (int)(_position - Open.Length - (long)_bodyCount * Body.Length);
                }
                var copy = Math.Min(buffer.Length - written, fragment.Length - index);
                fragment.AsMemory(index, copy).CopyTo(buffer[written..]);
                written += copy;
                _position += copy;
            }
            return ValueTask.FromResult(written);
        }
    }
}
