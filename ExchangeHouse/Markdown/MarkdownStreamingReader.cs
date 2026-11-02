using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Limits for the forward-only Markdown scanner. Open block state is stored
/// separately from the content, so an arbitrarily long fenced block is legal.
/// </summary>
public sealed class MarkdownStreamOptions
{
    public int MaxLineChars { get; init; } = 64 * 1024;
    public int ReadBufferChars { get; init; } = 8 * 1024;

    internal void Validate()
    {
        if (MaxLineChars < 1 || ReadBufferChars is < 1 or > 1024 * 1024)
            throw new ArgumentOutOfRangeException(nameof(MarkdownStreamOptions),
                "Both limits must be positive; read buffer must not exceed 1 MiB.");
    }
}

public enum MarkdownStreamLineKind
{
    Blank,
    Markdown,
    CodeFenceOpen,
    CodeContent,
    CodeFenceClose
}

/// <summary>
/// One completed physical line and its active structural index. RawText
/// preserves the exact characters and newline. Start is a 64-bit UTF-16
/// character offset; it is not a byte offset. No previous content is retained.
/// </summary>
public sealed record MarkdownStreamLine(
    MarkdownStreamLineKind Kind,
    long Start,
    string RawText,
    string Content,
    int QuoteDepth,
    int ListDepth,
    long? OpenFenceStart,
    string? CodeLanguage)
{
    /// <summary>The full UTF-16 character length including line ending.</summary>
    public int Length => RawText.Length;
}

/// <summary>
/// Memory-bounded Markdown input scanner. Unlike Parse(string), this scanner
/// does not build a document tree. It stores only unfinished structural state
/// (fence delimiter and language, quote depth, and bounded list indent stack).
/// Each finished line can immediately be rendered or discarded.
/// </summary>
/// <remarks>
/// This is a conservative streaming classification API, not a CommonMark-complete
/// incremental AST. Cross-line inline references, full list continuation
/// ownership and absolute inline offsets require later dedicated work.
/// A long unfinished fence is processed line by line; only a single excessively
/// long physical line is rejected according to MaxLineChars.
/// </remarks>
public static class MarkdownStreamingReader
{
    private readonly record struct FenceState(
        char Marker, int Length, long Start, int QuoteDepth, string? Language);

    public static async IAsyncEnumerable<MarkdownStreamLine> ScanAsync(
        TextReader reader,
        MarkdownStreamOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        options ??= new MarkdownStreamOptions();
        options.Validate();

        long offset = 0;
        FenceState? activeFence = null;
        var openLists = new List<int>(32);

        await foreach (var raw in ReadLinesAsync(reader, options, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var content = raw.AsSpan().TrimEnd("\r\n").ToString();
            var quoteDepth = TryQuotePrefix(content.AsSpan(), out var quoteBody)
                ? CountQuoteDepth(content.AsSpan(), out quoteBody)
                : 0;
            var body = quoteDepth > 0 ? quoteBody.ToString() : content;
            MarkdownStreamLineKind kind;
            long? fenceStart = null;
            string? language = null;

            if (activeFence is { } fence)
            {
                fenceStart = fence.Start;
                language = fence.Language;

                if (quoteDepth != fence.QuoteDepth)
                {
                    // An explicitly quote-owned fence never consumes a line
                    // from a different quote container. Reclassify it below.
                    activeFence = null;
                }
                else if (TryFence(body.AsSpan(), out var closingMarker,
                    out var closingCount, out _, out var trailingOnly) &&
                    closingMarker == fence.Marker && closingCount >= fence.Length &&
                    trailingOnly)
                {
                    kind = MarkdownStreamLineKind.CodeFenceClose;
                    activeFence = null;
                    yield return new MarkdownStreamLine(kind, offset, raw, body, quoteDepth,
                        openLists.Count, fenceStart, language);
                    offset = checked(offset + raw.Length);
                    continue;
                }
                else
                {
                    kind = MarkdownStreamLineKind.CodeContent;
                    yield return new MarkdownStreamLine(kind, offset, raw, body, quoteDepth,
                        openLists.Count, fenceStart, language);
                    offset = checked(offset + raw.Length);
                    continue;
                }
            }

            if (TryFence(body.AsSpan(), out var opener, out var openerCount,
                out var info, out _))
            {
                kind = MarkdownStreamLineKind.CodeFenceOpen;
                language = FirstWord(info);
                fenceStart = offset;
                activeFence = new FenceState(opener, openerCount, offset, quoteDepth, language);
            }
            else if (string.IsNullOrWhiteSpace(body))
            {
                kind = MarkdownStreamLineKind.Blank;
            }
            else
            {
                kind = MarkdownStreamLineKind.Markdown;
                // Only list *openers* update the index. This does not claim
                // complete CommonMark lazy/loose continuation semantics.
                if (TryListIndent(body.AsSpan(), out var indent))
                {
                    while (openLists.Count > 0 && openLists[^1] >= indent)
                        openLists.RemoveAt(openLists.Count - 1);
                    if (openLists.Count < 32)
                        openLists.Add(indent);
                }
                else if (!char.IsWhiteSpace(body[0]))
                {
                    openLists.Clear();
                }
            }

            yield return new MarkdownStreamLine(kind, offset, raw, body, quoteDepth,
                openLists.Count, fenceStart, language);
            offset = checked(offset + raw.Length);
        }
    }

    private static bool TryQuotePrefix(ReadOnlySpan<char> value, out ReadOnlySpan<char> body)
    {
        body = value;
        var spaces = 0;
        while (spaces < body.Length && spaces < 4 && body[spaces] == ' ')
            spaces++;
        if (spaces > 3)
            return false;
        body = body[spaces..];
        return !body.IsEmpty && body[0] == '>';
    }

    private static int CountQuoteDepth(ReadOnlySpan<char> line, out ReadOnlySpan<char> body)
    {
        body = line.TrimStart(' ');
        var depth = 0;
        while (depth < 32 && !body.IsEmpty && body[0] == '>')
        {
            depth++;
            body = body[1..];
            if (!body.IsEmpty && body[0] == ' ')
                body = body[1..];
        }
        return depth;
    }

    private static bool TryFence(ReadOnlySpan<char> content,
        out char marker, out int length, out ReadOnlySpan<char> info,
        out bool trailingOnly)
    {
        marker = '\0';
        length = 0;
        info = default;
        trailingOnly = false;
        var indentation = 0;
        while (indentation < content.Length && indentation < 4 &&
               content[indentation] == ' ')
            indentation++;
        if (indentation > 3)
            return false;
        content = content[indentation..];

        if (content.Length < 3 || (content[0] != '~' && content[0] != '\u0060'))
            return false;
        marker = content[0];
        while (length < content.Length && content[length] == marker)
            length++;
        if (length < 3)
            return false;
        info = content[length..].Trim();
        trailingOnly = info.IsEmpty;
        // Backticks inside a backtick opener's information string are invalid.
        if (marker == '\u0060' && info.Contains('\u0060'))
            return false;
        return true;
    }

    private static string? FirstWord(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty)
            return null;
        var i = 0;
        while (i < value.Length && !char.IsWhiteSpace(value[i]))
            i++;
        return value[..i].ToString();
    }

    private static bool TryListIndent(ReadOnlySpan<char> value, out int indent)
    {
        indent = 0;
        var i = 0;
        while (i < value.Length && indent <= 32)
        {
            if (value[i] == ' ')
                indent++;
            else if (value[i] == '\t')
                indent += 4 - indent % 4;
            else
                break;
            i++;
        }
        if (i >= value.Length)
            return false;
        if ((value[i] is '-' or '*' or '+') &&
            i + 1 < value.Length && char.IsWhiteSpace(value[i + 1]))
            return true;
        var start = i;
        while (i < value.Length && i - start < 9 && char.IsDigit(value[i]))
            i++;
        return i > start && i + 1 < value.Length &&
            (value[i] == '.' || value[i] == ')') && char.IsWhiteSpace(value[i + 1]);
    }

    private static async IAsyncEnumerable<string> ReadLinesAsync(
        TextReader reader, MarkdownStreamOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<char>.Shared.Rent(options.ReadBufferChars);
        var line = new StringBuilder();
        var afterCr = false;

        try
        {
            int read;
            while ((read = await reader.ReadAsync(
                       buffer.AsMemory(0, options.ReadBufferChars), cancellationToken)) != 0)
            {
                for (var i = 0; i < read; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var c = buffer[i];
                    if (afterCr)
                    {
                        if (c == '\n')
                        {
                            if (line.Length >= options.MaxLineChars + 1)
                                throw new InvalidDataException("Markdown physical line exceeds MaxLineChars.");
                            line.Append('\n');
                            yield return line.ToString();
                            line.Clear();
                            afterCr = false;
                            continue;
                        }
                        yield return line.ToString();
                        line.Clear();
                        afterCr = false;
                    }

                    if (line.Length >= options.MaxLineChars)
                        throw new InvalidDataException("Markdown physical line exceeds MaxLineChars.");
                    line.Append(c);
                    if (c == '\n')
                    {
                        yield return line.ToString();
                        line.Clear();
                    }
                    else if (c == '\r')
                    {
                        afterCr = true;
                    }
                }
            }

            if (line.Length > 0)
                yield return line.ToString();
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }
}
