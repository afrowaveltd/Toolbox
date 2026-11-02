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

    /// <summary>
    /// True when this ordinary line may continue a prose paragraph across a
    /// line/page boundary. This is not full CommonMark paragraph ownership.
    /// </summary>
    public bool IsParagraphContinuationCandidate { get; init; }

    /// <summary>The absolute UTF-16 start of the owning list marker, if recognized.</summary>
    public long? ListOwnerStart { get; init; }

    /// <summary>Whether this physical prose line is conservatively owned by an earlier list item.</summary>
    public bool IsListContinuationCandidate { get; init; }
}

/// <summary>A bounded active list-item candidate, without its source text.</summary>
public sealed record MarkdownListOwner(long Start, int Indent, int ContentIndent);


/// <summary>
/// Structural state immediately before one physical Markdown line. The start
/// offset is a UTF-16 character position in the logical document, not a byte
/// position. A zero marker means there is no open fenced code block.
/// </summary>
/// <remarks>
/// This experimental checkpoint covers only code fences. Full list ownership,
/// reference links, paragraph continuation and CommonMark container state
/// cannot yet be resumed from this structure.
/// </remarks>
public sealed record MarkdownStreamCheckpoint(
    long Start, char FenceMarker = '\0', int FenceLength = 0,
    long FenceStart = -1, int FenceQuoteDepth = 0,
    string? CodeLanguage = null)
{
    /// <summary>Bounded list indentation candidates before the next line (max 32).</summary>
    public IReadOnlyList<int> ListIndents { get; init; } = Array.Empty<int>();

    /// <summary>Quote depth of the preceding physical line, 0..32.</summary>
    public int PreviousQuoteDepth { get; init; }

    /// <summary>Whether the previous line may be an open prose paragraph.</summary>
    public bool PreviousParagraphCandidate { get; init; }

    /// <summary>Recognized list-item owners, outermost first, at most 32.</summary>
    public IReadOnlyList<MarkdownListOwner> ListOwners { get; init; } =
        Array.Empty<MarkdownListOwner>();

    /// <summary>Whether the previous line was a list item or owned prose.</summary>
    public bool PreviousOwnedText { get; init; }

    /// <summary>Whether the preceding physical line was blank.</summary>
    public bool PreviousWasBlank { get; init; }
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

    /// <summary>Scan a new stream from character offset zero.</summary>
    public static IAsyncEnumerable<MarkdownStreamLine> ScanAsync(
        TextReader reader,
        MarkdownStreamOptions? options = null,
        CancellationToken cancellationToken = default) =>
        ScanCoreAsync(reader, null, options, cancellationToken);

    /// <summary>
    /// Continue a physical-line scan from a previously indexed fence
    /// checkpoint. Raw input must begin at the checkpoint's line boundary.
    /// The bounded list-candidate stack and previous quote/paragraph hints
    /// are also restored. This is not CommonMark-complete ownership.
    /// </summary>
    public static IAsyncEnumerable<MarkdownStreamLine> ScanAsync(
        TextReader reader,
        MarkdownStreamCheckpoint checkpoint,
        MarkdownStreamOptions? options = null,
        CancellationToken cancellationToken = default) =>
        ScanCoreAsync(reader, checkpoint, options, cancellationToken);

    private static async IAsyncEnumerable<MarkdownStreamLine> ScanCoreAsync(
        TextReader reader,
        MarkdownStreamCheckpoint? checkpoint,
        MarkdownStreamOptions? options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(reader);
        options ??= new MarkdownStreamOptions();
        options.Validate();
        if (checkpoint is not null && !IsValidCheckpoint(checkpoint))
            throw new ArgumentException("Invalid Markdown checkpoint.", nameof(checkpoint));

        long offset = checkpoint?.Start ?? 0;
        FenceState? activeFence = checkpoint?.FenceMarker is '~' or '`'
            ? new FenceState(checkpoint.FenceMarker, checkpoint.FenceLength,
                checkpoint.FenceStart, checkpoint.FenceQuoteDepth,
                checkpoint.CodeLanguage)
            : null;
        var openLists = checkpoint?.ListIndents.ToList() ?? new List<int>(32);
        var priorQuoteDepth = checkpoint?.PreviousQuoteDepth ?? 0;
        var priorParagraphCandidate = checkpoint?.PreviousParagraphCandidate ?? false;
        var ownershipState = checkpoint ?? new MarkdownStreamCheckpoint(0);

        await foreach (var raw in ReadLinesAsync(reader, options, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var content = raw.AsSpan().TrimEnd("\r\n").ToString();
            var quoteDepth = TryQuotePrefix(content.AsSpan(), out var quoteBody)
                ? CountQuoteDepth(content.AsSpan(), out quoteBody)
                : 0;
            var body = quoteDepth > 0 ? quoteBody.ToString() : content;
            if (quoteDepth != priorQuoteDepth)
                openLists.Clear();
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
                    var ownedLine = AttachOwnership(ownershipState,
                        new MarkdownStreamLine(kind, offset, raw, body, quoteDepth,
                            openLists.Count, fenceStart, language));
                    ownershipState = AdvanceCheckpoint(ownershipState, ownedLine);
                    yield return ownedLine;
                    priorQuoteDepth = quoteDepth;
                    priorParagraphCandidate = false;
                    offset = checked(offset + raw.Length);
                    continue;
                }
                else
                {
                    kind = MarkdownStreamLineKind.CodeContent;
                    var ownedLine = AttachOwnership(ownershipState,
                        new MarkdownStreamLine(kind, offset, raw, body, quoteDepth,
                            openLists.Count, fenceStart, language));
                    ownershipState = AdvanceCheckpoint(ownershipState, ownedLine);
                    yield return ownedLine;
                    priorQuoteDepth = quoteDepth;
                    priorParagraphCandidate = false;
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
            }

            var continuingParagraph = kind == MarkdownStreamLineKind.Markdown &&
                priorParagraphCandidate && quoteDepth == priorQuoteDepth &&
                LooksLikeParagraph(body.AsSpan());
            UpdateListStack(openLists, kind, body.AsSpan());
            priorParagraphCandidate = kind == MarkdownStreamLineKind.Markdown &&
                LooksLikeParagraph(body.AsSpan());
            priorQuoteDepth = quoteDepth;

            var classified = AttachOwnership(ownershipState,
                new MarkdownStreamLine(kind, offset, raw, body, quoteDepth,
                    openLists.Count, fenceStart, language)
                {
                    IsParagraphContinuationCandidate = continuingParagraph
                });
            ownershipState = AdvanceCheckpoint(ownershipState, classified);
            yield return classified;
            offset = checked(offset + raw.Length);
        }
    }

    private static bool IsValidCheckpoint(MarkdownStreamCheckpoint c)
    {
        if (c.Start < 0 || c.PreviousQuoteDepth is < 0 or > 32 ||
            c.ListIndents is null || c.ListIndents.Count > 32 ||
            c.ListIndents.Any(n => n is < 0 or > 32) ||
            c.ListOwners is null || c.ListOwners.Count > 32 ||
            c.ListOwners.Any(n => n.Start < 0 || n.Start > c.Start ||
                n.Indent is < 0 or > 32 || n.ContentIndent < n.Indent ||
                n.ContentIndent > 128))
            return false;
        if (c.FenceMarker == '\0')
            return c.FenceLength == 0 && c.FenceStart == -1 &&
                c.FenceQuoteDepth == 0 && c.CodeLanguage is null;
        return c.FenceMarker is '~' or '`' && c.FenceLength >= 3 &&
            c.FenceStart >= 0 && c.FenceStart <= c.Start &&
            c.FenceQuoteDepth is >= 0 and <= 32;
    }

    /// <summary>
    /// Advance only bounded scanner state from a classified line. This is the
    /// shared state transition used when persisting page checkpoints.
    /// </summary>
    public static MarkdownStreamCheckpoint AdvanceCheckpoint(
        MarkdownStreamCheckpoint current, MarkdownStreamLine line)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(line);
        if (line.Start != current.Start)
            throw new ArgumentException("Checkpoint and line offsets disagree.", nameof(line));

        var owners = UpdateOwners(current, line);
        var lists = owners.Select(x => x.Indent).ToArray();

        var next = line.Kind switch
        {
            MarkdownStreamLineKind.CodeFenceOpen => new MarkdownStreamCheckpoint(
                checked(line.Start + line.Length),
                ExtractMarker(line.Content), ExtractRunLength(line.Content),
                line.Start, line.QuoteDepth, line.CodeLanguage),
            MarkdownStreamLineKind.CodeContent => current with
            {
                Start = checked(line.Start + line.Length)
            },
            _ => new MarkdownStreamCheckpoint(checked(line.Start + line.Length))
        };
        return next with
        {
            ListIndents = lists,
            ListOwners = owners,
            PreviousOwnedText = line.Kind == MarkdownStreamLineKind.Markdown &&
                line.ListOwnerStart.HasValue,
            PreviousWasBlank = line.Kind == MarkdownStreamLineKind.Blank,
            PreviousQuoteDepth = line.QuoteDepth,
            PreviousParagraphCandidate = line.Kind == MarkdownStreamLineKind.Markdown &&
                LooksLikeParagraph(line.Content.AsSpan())
        };
    }

    /// <summary>
    /// Attach conservatively recognized list-item identity and continuation
    /// ownership without allocating the preceding list or paragraph text.
    /// </summary>
    public static MarkdownStreamLine AttachOwnership(
        MarkdownStreamCheckpoint previous, MarkdownStreamLine line)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(line);
        if (previous.Start != line.Start)
            throw new ArgumentException("Checkpoint and line offsets disagree.", nameof(line));

        var owners = UpdateOwners(previous, line);
        if (line.Kind != MarkdownStreamLineKind.Markdown)
            return line with { ListDepth = owners.Count };

        if (TryListMarker(line.Content.AsSpan(), out _, out _))
            return line with
            {
                ListOwnerStart = line.Start,
                ListDepth = owners.Count
            };

        var owner = owners.Count == 0 ? null : owners[^1];
        return line with
        {
            ListDepth = owners.Count,
            ListOwnerStart = owner?.Start,
            IsListContinuationCandidate = owner is not null
        };
    }

    private static MarkdownListOwner[] UpdateOwners(
        MarkdownStreamCheckpoint previous, MarkdownStreamLine line)
    {
        var stack = previous.ListOwners.ToList();
        if (line.QuoteDepth != previous.PreviousQuoteDepth)
            stack.Clear();
        if (line.Kind != MarkdownStreamLineKind.Markdown)
            return stack.ToArray();

        var content = line.Content.AsSpan();
        if (TryListMarker(content, out var indent, out var contentIndent))
        {
            while (stack.Count > 0 && stack[^1].Indent >= indent)
                stack.RemoveAt(stack.Count - 1);
            // An unexpected shallower indent cannot retain a child owner.
            while (stack.Count > 0 && indent < stack[^1].ContentIndent)
                stack.RemoveAt(stack.Count - 1);
            if (stack.Count < 32)
                stack.Add(new MarkdownListOwner(line.Start, indent, contentIndent));
            return stack.ToArray();
        }

        if (!LooksLikeParagraph(content))
        {
            stack.Clear();
            return stack.ToArray();
        }

        var indentation = MeasureColumns(content);
        while (stack.Count > 0 && indentation < stack[^1].ContentIndent)
            stack.RemoveAt(stack.Count - 1);
        if (stack.Count > 0)
            return stack.ToArray();

        // Lazy unindented continuation is only a candidate when directly
        // following owned prose/list text with no intervening blank or fence.
        if (!previous.PreviousWasBlank && previous.PreviousOwnedText &&
            line.QuoteDepth == previous.PreviousQuoteDepth &&
            previous.ListOwners.Count > 0)
            return previous.ListOwners.ToArray();
        return Array.Empty<MarkdownListOwner>();
    }

    private static int MeasureColumns(ReadOnlySpan<char> value)
    {
        var columns = 0;
        foreach (var ch in value)
        {
            if (ch == ' ')
                columns++;
            else if (ch == '\t')
                columns += 4 - columns % 4;
            else
                break;
        }
        return columns;
    }

    private static bool TryListMarker(ReadOnlySpan<char> value,
        out int indent, out int contentIndent)
    {
        contentIndent = 0;
        if (!TryListIndent(value, out indent))
            return false;

        var pos = 0;
        var columns = 0;
        while (pos < value.Length && columns < indent)
        {
            columns += value[pos] == '\t' ? 4 - columns % 4 : 1;
            pos++;
        }

        var markerStart = pos;
        if (value[pos] is '-' or '*' or '+')
            pos++;
        else
        {
            while (pos < value.Length && char.IsDigit(value[pos]))
                pos++;
            pos++;
        }

        // The first whitespace after the marker is mandatory; count its
        // display width but conservatively treat extra spaces as content.
        var markerColumns = pos - markerStart;
        var currentColumn = indent + markerColumns;
        var advance = value[pos] == '\t' ? 4 - currentColumn % 4 : 1;
        contentIndent = Math.Min(128, currentColumn + advance);
        return true;
    }

    private static char ExtractMarker(string content) => content.AsSpan().TrimStart(' ')[0];

    private static int ExtractRunLength(string content)
    {
        var span = content.AsSpan().TrimStart(' ');
        var count = 0;
        while (count < span.Length && span[count] == span[0])
            count++;
        return count;
    }

    private static bool LooksLikeParagraph(ReadOnlySpan<char> body)
    {
        var value = body.TrimStart();
        if (value.IsEmpty || TryListIndent(body, out _))
            return false;
        if (value[0] == '#' &&
            (value.Length == 1 || char.IsWhiteSpace(value[1])))
            return false;
        if (value.SequenceEqual("---") || value.SequenceEqual("***") ||
            value.SequenceEqual("___") || value.StartsWith("<"))
            return false;
        return true;
    }

    private static void UpdateListStack(List<int> stack, MarkdownStreamLineKind kind,
        ReadOnlySpan<char> body)
    {
        if (kind != MarkdownStreamLineKind.Markdown)
            return;
        if (TryListIndent(body, out var indent))
        {
            while (stack.Count > 0 && stack[^1] >= indent)
                stack.RemoveAt(stack.Count - 1);
            if (stack.Count < 32)
                stack.Add(indent);
        }
        else if (!body.IsEmpty && !char.IsWhiteSpace(body[0]))
        {
            stack.Clear();
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
