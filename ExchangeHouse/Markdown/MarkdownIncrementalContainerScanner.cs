using System.Runtime.CompilerServices;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>Conservative explicit line categories within Markdown quote and list containers.</summary>
public enum MarkdownContainerLineKind
{
    /// <summary>An unowned ordinary line, including a top-level heading.</summary>
    Plain,
    /// <summary>A physical blank line; no list ownership is inferred.</summary>
    Blank,
    /// <summary>An explicitly marked quote line, without a confirmed list item.</summary>
    Quote,
    /// <summary>An explicitly marked ordered or unordered list item.</summary>
    ListItem,
    /// <summary>A sufficiently indented continuation of an explicit list marker.</summary>
    ListContinuation,
    /// <summary>Opening delimiter of a top-level or explicitly quote-prefixed code fence.</summary>
    FenceOpen,
    /// <summary>Literal content inside an active fence, never interpreted as a list or heading.</summary>
    CodeLine,
    /// <summary>A matching closing delimiter of an active fence.</summary>
    FenceClose
}

/// <summary>
/// One complete physical Markdown line with conservative source container metadata.
/// No nested Markdown document tree or block contents are retained.
/// </summary>
/// <param name="Source">Original physical line, including its decoded UTF-16 offset and terminator.</param>
/// <param name="Kind">Conservative classification; no implicit lazy ownership.</param>
/// <param name="QuoteDepth">Number of explicit quote markers (zero when absent).</param>
/// <param name="ListDepth">Zero-based explicit list nesting level; -1 for lines with no list owner.</param>
/// <param name="ListMarker">Original list marker, if this line opens a list item.</param>
/// <param name="ListOwnerUtf16Offset">Start of the explicit list item for this line, if provable.</param>
/// <param name="FenceStartUtf16Offset">Start of the active fenced region, if any.</param>
/// <param name="CodeLanguage">First language token of the active opening fence, if present.</param>
public sealed record MarkdownContainerLine(
    MarkdownStreamLine Source,
    MarkdownContainerLineKind Kind,
    int QuoteDepth = 0,
    int ListDepth = -1,
    string? ListMarker = null,
    long? ListOwnerUtf16Offset = null,
    long? FenceStartUtf16Offset = null,
    string? CodeLanguage = null);

/// <summary>
/// Incrementally recognizes explicit quote/list markers and the lifetime of
/// top-level or quote-owned fences. Produces bounded per-line metadata for
/// later container-aware Markdown block parsing.
/// </summary>
/// <remarks>
/// Indented and unindented paragraph continuations without explicit ownership
/// are deliberately conservative. Quoted lists are kept separate from ordinary
/// lists, and an indented quote nested in an outer list does not claim that outer
/// list owner yet. This is not full CommonMark and is not connected to mdview.
/// Caller must fully validate untrusted file bytes before progressive display.
/// </remarks>
public static class MarkdownIncrementalContainerScanner
{
    private const int MaximumDepth = 32;
    private const int MaximumIndentColumns = 256;

    /// <summary>Reads a decoded stream and yields explicit container state per physical line.</summary>
    /// <param name="source">Already decoded source reader, owned by the caller.</param>
    /// <param name="maxLineCharacters">Maximum characters per physical line.</param>
    /// <param name="cancellationToken">Cancels pending reads and line processing.</param>
    /// <returns>One source-preserving metadata event per completed physical line.</returns>
    public static async IAsyncEnumerable<MarkdownContainerLine> ScanAsync(
        TextReader source,
        int maxLineCharacters = MarkdownIncrementalLineScanner.DefaultMaxLineCharacters,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (maxLineCharacters is < 1 or > 4 * 1024 * 1024)
            throw new ArgumentOutOfRangeException(nameof(maxLineCharacters));

        var lists = new List<ListFrame>(MaximumDepth);
        var currentQuoteDepth = 0;
        char fenceMarker = '\0';
        int fenceLength = 0;
        int fenceQuoteDepth = 0;
        long fenceStart = 0;
        string? fenceLanguage = null;
        int fenceListDepth = -1;
        long? fenceListOwner = null;

        await foreach (var line in MarkdownIncrementalLineScanner.ScanAsync(
            source, maxLineCharacters, cancellationToken)
            .WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var text = line.Text.AsSpan();
            var hasQuote = TryQuote(text, out var depth, out var contentIndex);
            var content = hasQuote ? text[contentIndex..] : text;
            var quoteDepth = hasQuote ? depth : 0;

            if (fenceLength > 0)
            {
                // An explicitly quoted fence cannot consume an unquoted line or
                // a line with a different quote depth. Its closure is implicit
                // at the boundary, not an invented closing delimiter.
                if (fenceQuoteDepth == 0 || quoteDepth == fenceQuoteDepth)
                {
                    var codeContent = fenceQuoteDepth > 0 ? content : text;
                    var closing = IsClosingFence(codeContent, fenceMarker, fenceLength);
                    yield return new MarkdownContainerLine(line,
                        closing ? MarkdownContainerLineKind.FenceClose :
                            MarkdownContainerLineKind.CodeLine,
                        fenceQuoteDepth, fenceListDepth, null, fenceListOwner,
                        fenceStart, fenceLanguage);
                    if (closing)
                    {
                        fenceMarker = '\0';
                        fenceLength = 0;
                        fenceLanguage = null;
                        fenceListDepth = -1;
                        fenceListOwner = null;
                    }
                    continue;
                }

                fenceMarker = '\0';
                fenceLength = 0;
                fenceLanguage = null;
                fenceListDepth = -1;
                fenceListOwner = null;
            }

            if (quoteDepth != currentQuoteDepth)
            {
                lists.Clear();
                currentQuoteDepth = quoteDepth;
            }

            if (TryFence(content, out var marker, out var count))
            {
                fenceMarker = marker;
                fenceLength = count;
                fenceQuoteDepth = quoteDepth;
                fenceStart = line.Utf16Offset;
                fenceLanguage = ReadLanguage(content, marker, count);
                (fenceListDepth, fenceListOwner) = FindContinuation(lists, content);
                yield return new MarkdownContainerLine(line, MarkdownContainerLineKind.FenceOpen,
                    quoteDepth, fenceListDepth, null, fenceListOwner,
                    fenceStart, fenceLanguage);
                continue;
            }

            if (content.IsEmpty || IsBlank(content))
            {
                // A later explicitly indented continuation may still prove
                // ownership; this first scanner never assigns blank ownership.
                yield return new MarkdownContainerLine(line, MarkdownContainerLineKind.Blank, quoteDepth);
                continue;
            }

            if (TryListMarker(content, out var listMarker, out var indent,
                    out var contentIndent) &&
                (indent <= 3 || lists.Count > 0 && indent >= lists[0].MarkerIndent))
            {
                while (lists.Count > 0 && lists[^1].MarkerIndent >= indent)
                    lists.RemoveAt(lists.Count - 1);
                if (lists.Count < MaximumDepth)
                {
                    var listDepth = lists.Count;
                    var frame = new ListFrame(indent, contentIndent, line.Utf16Offset);
                    lists.Add(frame);
                    yield return new MarkdownContainerLine(line, MarkdownContainerLineKind.ListItem,
                        quoteDepth, listDepth, listMarker, line.Utf16Offset);
                    continue;
                }
            }

            var (ownerDepth, ownerOffset) = FindContinuation(lists, content);
            if (ownerDepth >= 0)
            {
                // The same explicit indentation rule as the existing parser:
                // attach to the deepest item with sufficient content indent.
                lists.RemoveRange(ownerDepth + 1, lists.Count - ownerDepth - 1);
                yield return new MarkdownContainerLine(line,
                    MarkdownContainerLineKind.ListContinuation,
                    quoteDepth, ownerDepth, ListOwnerUtf16Offset: ownerOffset);
                continue;
            }

            if (quoteDepth > 0)
            {
                yield return new MarkdownContainerLine(line, MarkdownContainerLineKind.Quote, quoteDepth);
            }
            else
            {
                yield return new MarkdownContainerLine(line, MarkdownContainerLineKind.Plain);
            }
        }
    }

    private static (int Depth, long? Owner) FindContinuation(
        List<ListFrame> frames, ReadOnlySpan<char> text)
    {
        var columns = 0;
        var index = 0;
        while (index < text.Length && (text[index] == ' ' || text[index] == '\t'))
        {
            columns += text[index] == '\t' ? 4 - columns % 4 : 1;
            if (columns > MaximumIndentColumns)
                return (-1, null);
            index++;
        }
        if (index == 0 || index == text.Length)
            return (-1, null);

        for (var i = frames.Count - 1; i >= 0; i--)
        {
            if (columns >= frames[i].ContentIndent)
                return (i, frames[i].Start);
        }
        return (-1, null);
    }

    private static bool TryQuote(
        ReadOnlySpan<char> text, out int depth, out int contentIndex)
    {
        depth = 0;
        contentIndex = 0;
        var i = 0;
        while (i < text.Length && i < 4 && text[i] == ' ')
            i++;
        if (i > 3 || i >= text.Length || text[i] != '>')
            return false;

        while (i < text.Length && text[i] == '>' && depth < MaximumDepth)
        {
            depth++;
            i++;
            if (i < text.Length && (text[i] == ' ' || text[i] == '\t'))
                i++;
            if (i >= text.Length || text[i] != '>')
                break;
        }
        contentIndex = i;
        return true;
    }

    private static bool TryListMarker(
        ReadOnlySpan<char> text, out string marker, out int indentColumns,
        out int contentIndent)
    {
        marker = "";
        indentColumns = 0;
        contentIndent = 0;
        var index = 0;
        while (index < text.Length && (text[index] == ' ' || text[index] == '\t'))
        {
            indentColumns += text[index] == '\t' ? 4 - indentColumns % 4 : 1;
            if (indentColumns > MaximumIndentColumns)
                return false;
            index++;
        }
        if (index >= text.Length)
            return false;

        var markerStart = index;
        if (text[index] is '-' or '+' or '*')
            index++;
        else if (char.IsAsciiDigit(text[index]))
        {
            var digits = 0;
            while (index < text.Length && char.IsAsciiDigit(text[index]) && digits < 10)
            {
                digits++;
                index++;
            }
            if (digits is < 1 or > 9 || index >= text.Length ||
                text[index] is not ('.' or ')') ||
                !int.TryParse(text[markerStart..index], out _))
                return false;
            index++;
        }
        else
            return false;

        if (index < text.Length && text[index] is not (' ' or '\t'))
            return false;
        marker = text[markerStart..index].ToString();
        while (index < text.Length && (text[index] == ' ' || text[index] == '\t'))
            index++;
        contentIndent = CountColumns(text[..index]);
        return true;
    }

    private static int CountColumns(ReadOnlySpan<char> text)
    {
        var columns = 0;
        foreach (var c in text)
            columns += c == '\t' ? 4 - columns % 4 : 1;
        return columns;
    }

    private static bool TryFence(ReadOnlySpan<char> text, out char marker, out int length)
    {
        marker = '\0';
        length = 0;
        var indent = 0;
        while (indent < 4 && indent < text.Length && text[indent] == ' ')
            indent++;
        if (indent > 3 || indent >= text.Length ||
            text[indent] is not ('`' or '~'))
            return false;
        marker = text[indent];
        var i = indent;
        while (i < text.Length && text[i] == marker)
            i++;
        length = i - indent;
        return length >= 3;
    }

    private static bool IsClosingFence(
        ReadOnlySpan<char> text, char expected, int minLength)
    {
        if (!TryFence(text, out var marker, out var length) ||
            marker != expected || length < minLength)
            return false;
        var i = 0;
        while (i < 4 && i < text.Length && text[i] == ' ')
            i++;
        return text[(i + length)..].Trim(' ', '\t').IsEmpty;
    }

    private static string? ReadLanguage(
        ReadOnlySpan<char> text, char marker, int markerLength)
    {
        var i = 0;
        while (i < 4 && i < text.Length && text[i] == ' ')
            i++;
        var info = text[(i + markerLength)..].Trim();
        if (info.IsEmpty || marker == '`' && info.Contains('`'))
            return null;
        var length = 0;
        while (length < info.Length && !char.IsWhiteSpace(info[length]))
            length++;
        return length is > 0 and <= 64 ? info[..length].ToString() : null;
    }

    private static bool IsBlank(ReadOnlySpan<char> content)
    {
        foreach (var character in content)
        {
            if (!char.IsWhiteSpace(character))
                return false;
        }
        return true;
    }

    private sealed record ListFrame(int MarkerIndent, int ContentIndent, long Start);
}
