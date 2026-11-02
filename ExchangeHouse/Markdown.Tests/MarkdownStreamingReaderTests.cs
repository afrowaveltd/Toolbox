using Xunit;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown.Tests;

public sealed class MarkdownStreamingReaderTests
{
    [Fact]
    public async Task Preserves_mixed_line_endings_across_one_character_reads()
    {
        const string source = "# Heading\r\nParagraph\n\n- List\r- Next\r\n\n~~~csharp\n# literal\n\n~~~\r\n\n## Last";
        var documents = await CollectAsync(source, new MarkdownStreamOptions
        {
            ReadBufferChars = 1,
            PreferredChunkChars = 12,
            MaxChunkChars = 1024,
            MaxLineChars = 128
        });

        Assert.True(documents.Count >= 2);
        Assert.Equal(source, string.Concat(documents.Select(MarkdownWriter.Write)));
        Assert.Contains(documents.SelectMany(d => d.Blocks),
            b => b.Kind == MarkdownBlockKind.CodeLine && b.Text == "# literal");
    }

    [Fact]
    public async Task Does_not_split_at_blank_lines_inside_an_open_fence()
    {
        const string source = "~~~csharp\nFirst line\n\nSecond line\n~~~\n\n# After\n";
        var documents = await CollectAsync(source, new MarkdownStreamOptions
        {
            PreferredChunkChars = 1,
            MaxChunkChars = 1024,
            MaxLineChars = 128,
            ReadBufferChars = 3
        });

        Assert.Equal(2, documents.Count);
        Assert.Equal(source, string.Concat(documents.Select(MarkdownWriter.Write)));
        Assert.Contains(documents[0].Blocks, b =>
            b.Kind == MarkdownBlockKind.CodeLine && b.Text == "Second line");
    }

    [Fact]
    public async Task Streams_many_independent_blocks_without_retaining_source()
    {
        const string block = "# Title\n- first\n- second\n\n";
        var source = string.Concat(Enumerable.Repeat(block, 5000));
        long observed = 0;
        var count = 0;
        using var reader = new StringReader(source);
        await foreach (var document in MarkdownStreamingReader.ParseAsync(reader,
            new MarkdownStreamOptions
            {
                PreferredChunkChars = 400,
                MaxChunkChars = 4096,
                MaxLineChars = 256,
                ReadBufferChars = 5
            }))
        {
            observed += MarkdownWriter.Write(document).Length;
            count++;
        }
        Assert.Equal(source.Length, observed);
        Assert.True(count > 100);
    }

    [Fact]
    public async Task Rejects_a_single_unbounded_line_instead_of_oom()
    {
        var options = new MarkdownStreamOptions
        {
            PreferredChunkChars = 32,
            MaxChunkChars = 256,
            MaxLineChars = 64,
            ReadBufferChars = 7
        };

        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            using var reader = new StringReader(new string('z', 65));
            await foreach (var _ in MarkdownStreamingReader.ParseAsync(reader, options))
            {
            }
        });
    }

    [Fact]
    public async Task Rejects_a_huge_unclosed_fence_with_explicit_limit()
    {
        var options = new MarkdownStreamOptions
        {
            PreferredChunkChars = 16,
            MaxChunkChars = 128,
            MaxLineChars = 64,
            ReadBufferChars = 9
        };

        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            using var reader = new StringReader("~~~\n" +
                string.Concat(Enumerable.Repeat("more code\n", 25)));
            await foreach (var _ in MarkdownStreamingReader.ParseAsync(reader, options))
            {
            }
        });
    }

    [Fact]
    public async Task Honors_cancellation()
    {
        using var source = new StringReader("# Heading\n");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in MarkdownStreamingReader.ParseAsync(
                source, cancellationToken: cts.Token))
            {
            }
        });
    }

    private static async Task<List<MarkdownDocument>> CollectAsync(
        string text, MarkdownStreamOptions options)
    {
        var documents = new List<MarkdownDocument>();
        using var reader = new StringReader(text);
        await foreach (var document in MarkdownStreamingReader.ParseAsync(reader, options))
            documents.Add(document);
        return documents;
    }
}
