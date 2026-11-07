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
    string? CodeLanguage = null)
{
    /// <summary>
    /// Explicit ordinary-list owner containing this quoted source line, if provable.
    /// Separate from <see cref="ListOwnerUtf16Offset"/>, which identifies a quote-local list.
    /// </summary>
    public long? QuoteContainerListOwnerUtf16Offset { get; init; }

    /// <summary>Zero-based outer ordinary-list nesting depth, or -1 when absent.</summary>
    public int QuoteContainerListDepth { get; init; } = -1;

    /// <summary>Original marker of the containing ordinary list item, when known.</summary>
    public string? QuoteContainerListMarker { get; init; }
}

/// <summary>
/// Incrementally recognizes explicit quote/list markers and the lifetime of
/// top-level or quote-owned fences. Produces bounded per-line metadata for
/// later container-aware Markdown block parsing.
/// </summary>
/// <remarks>
/// Indented and unindented paragraph continuations without explicit ownership
/// are deliberately conservative. Quoted lists are kept separate from ordinary
/// lists, while provably indented quotes retain their enclosing ordinary-list
/// owner as separate metadata. This is not full CommonMark or an mdview renderer.
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

        // Ordinary items and quoted items have independent ownership stacks.
        // A quote nested in an ordinary item does not consume that outer stack.
        var ordinaryLists = new List<ListFrame>(MaximumDepth);
        var quoteLists = new List<ListFrame>(MaximumDepth);
        var currentQuoteDepth = 0;
        long? currentOuterQuoteOwner = null;

        char fenceMarker = '\0';
        int fenceLength = 0;
        int fenceQuoteDepth = 0;
        long fenceStart = 0;
        string? fenceLanguage = null;
        int fenceListDepth = -1;
        long? fenceListOwner = null;
        int fenceContentIndent = 0;
        ListFrame? fenceOuterOwner = null;
        int fenceOuterDepth = -1;

        MarkdownContainerLine Make(
            MarkdownStreamLine line, MarkdownContainerLineKind kind,
            int quoteDepth, int listDepth = -1, string? listMarker = null,
            long? listOwner = null, long? codeFenceStart = null,
            string? language = null, ListFrame? outerOwner = null, int outerDepth = -1) =>
            new(line, kind, quoteDepth, listDepth, listMarker,
                listOwner, codeFenceStart, language)
            {
                QuoteContainerListOwnerUtf16Offset = outerOwner?.Start,
                QuoteContainerListDepth = outerOwner is null ? -1 : outerDepth,
                QuoteContainerListMarker = outerOwner?.Marker
            };

        await foreach (var line in MarkdownIncrementalLineScanner.ScanAsync(
            source, maxLineCharacters, cancellationToken)
            .WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var text = line.Text.AsSpan();
            ListFrame? outerOwner = null;
            var outerDepth = -1;
            var quoteDepth = 0;
            var content = text;

            // Probe ordinary-list content indentation BEFORE standalone quotes.
            // "  > quote" can belong to "- item" even though a standalone
            // quote with up to three spaces would also be syntactically valid.
            var (outerCandidateDepth, _) = FindContinuation(ordinaryLists, text);
            if (outerCandidateDepth >= 0)
            {
                var stripped = text[LeadingIndentLength(text)..];
                if (TryQuote(stripped, out var nestedDepth, out var nestedIndex))
                {
                    quoteDepth = nestedDepth;
                    content = stripped[nestedIndex..];
                    outerDepth = outerCandidateDepth;
                    outerOwner = ordinaryLists[outerDepth];
                    ordinaryLists.RemoveRange(outerDepth + 1,
                        ordinaryLists.Count - outerDepth - 1);
                }
            }
            if (quoteDepth == 0 && TryQuote(text, out var directDepth, out var directIndex))
            {
                quoteDepth = directDepth;
                content = text[directIndex..];
            }

            if (fenceLength > 0)
            {
                // An ordinary fence can contain arbitrary literal markers.
                // A quoted fence requires the exact quote depth AND its outer
                // list owner; a change means an interruption, not a closer.
                if (fenceQuoteDepth == 0 ||
                    (quoteDepth == fenceQuoteDepth &&
                     outerOwner?.Start == fenceOuterOwner?.Start))
                {
                    var codeContent = fenceQuoteDepth > 0 ? content : text;
                    // Strip only the owning item's content indent, never all
                    // whitespace: four remaining spaces cannot close a fence.
                    if (fenceListOwner is not null)
                        codeContent = StripIndentUpTo(codeContent, fenceContentIndent);
                    var closing = IsClosingFence(codeContent, fenceMarker, fenceLength);
                    yield return Make(line,
                        closing ? MarkdownContainerLineKind.FenceClose :
                            MarkdownContainerLineKind.CodeLine,
                        fenceQuoteDepth, fenceListDepth, listOwner: fenceListOwner,
                        codeFenceStart: fenceStart, language: fenceLanguage,
                        outerOwner: fenceOuterOwner, outerDepth: fenceOuterDepth);
                    if (closing)
                    {
                        fenceMarker = '\0';
                        fenceLength = 0;
                        fenceLanguage = null;
                        fenceListDepth = -1;
                        fenceListOwner = null;
                        fenceContentIndent = 0;
                        fenceOuterOwner = null;
                        fenceOuterDepth = -1;
                    }
                    continue;
                }

                fenceMarker = '\0';
                fenceLength = 0;
                fenceLanguage = null;
                fenceListDepth = -1;
                fenceListOwner = null;
                fenceContentIndent = 0;
                fenceOuterOwner = null;
                fenceOuterDepth = -1;
            }

            if (quoteDepth != currentQuoteDepth ||
                (quoteDepth > 0 && outerOwner?.Start != currentOuterQuoteOwner))
            {
                quoteLists.Clear();
                currentQuoteDepth = quoteDepth;
                currentOuterQuoteOwner = outerOwner?.Start;
            }

            // An unowned standalone quote interrupts any previous ordinary
            // list context; a later indented quote must not revive that owner.
            if (quoteDepth > 0 && outerOwner is null)
                ordinaryLists.Clear();

            var activeLists = quoteDepth > 0 ? quoteLists : ordinaryLists;
            var (indentOwnerDepth, indentOwner) = FindContinuation(activeLists, content);
            var ownerContentIndent = indentOwnerDepth >= 0
                ? activeLists[indentOwnerDepth].ContentIndent : 0;
            var fenceContent = indentOwnerDepth >= 0
                ? StripIndentUpTo(content, ownerContentIndent)
                : content;

            if (TryFence(fenceContent, out var marker, out var count))
            {
                // For a fence directly inside a quote, previous quote-local
                // list markers are not allowed to acquire this block.
                if (indentOwnerDepth < 0)
                    activeLists.Clear();
                fenceMarker = marker;
                fenceLength = count;
                fenceQuoteDepth = quoteDepth;
                fenceStart = line.Utf16Offset;
                fenceLanguage = ReadLanguage(fenceContent, marker, count);
                fenceListDepth = indentOwnerDepth;
                fenceListOwner = indentOwner;
                fenceContentIndent = ownerContentIndent;
                fenceOuterOwner = outerOwner;
                fenceOuterDepth = outerDepth;
                yield return Make(line, MarkdownContainerLineKind.FenceOpen,
                    quoteDepth, fenceListDepth, listOwner: fenceListOwner,
                    codeFenceStart: fenceStart, language: fenceLanguage,
                    outerOwner: outerOwner, outerDepth: outerDepth);
                continue;
            }

            if (content.IsEmpty || IsBlank(content))
            {
                // Blank ownership is deferred in the full parser. We record
                // the outer quote container, but do not invent an inner owner.
                yield return Make(line, MarkdownContainerLineKind.Blank,
                    quoteDepth, outerOwner: outerOwner, outerDepth: outerDepth);
                continue;
            }

            if (TryListMarker(content, out var listMarker, out var indent,
                    out var contentIndent) &&
                (indent <= 3 ||
                 activeLists.Count > 0 && indent >= activeLists[0].MarkerIndent))
            {
                while (activeLists.Count > 0 &&
                       activeLists[^1].MarkerIndent >= indent)
                    activeLists.RemoveAt(activeLists.Count - 1);
                if (activeLists.Count < MaximumDepth)
                {
                    var listDepth = activeLists.Count;
                    activeLists.Add(new ListFrame(indent, contentIndent,
                        line.Utf16Offset, listMarker));
                    yield return Make(line, MarkdownContainerLineKind.ListItem,
                        quoteDepth, listDepth, listMarker, line.Utf16Offset,
                        outerOwner: outerOwner, outerDepth: outerDepth);
                    continue;
                }
            }

            if (indentOwnerDepth >= 0)
            {
                activeLists.RemoveRange(indentOwnerDepth + 1,
                    activeLists.Count - indentOwnerDepth - 1);
                yield return Make(line, MarkdownContainerLineKind.ListContinuation,
                    quoteDepth, indentOwnerDepth, listOwner: indentOwner,
                    outerOwner: outerOwner, outerDepth: outerDepth);
                continue;
            }

            activeLists.Clear();
            // A standalone quoted paragraph cannot keep a stale ordinary
            // list owner alive for later, unrelated indented quote lines.
            yield return Make(line,
                quoteDepth > 0 ? MarkdownContainerLineKind.Quote :
                    MarkdownContainerLineKind.Plain,
                quoteDepth, outerOwner: outerOwner, outerDepth: outerDepth);
        }
    }

    private static ReadOnlySpan<char> StripIndentUpTo(
        ReadOnlySpan<char> text, int requiredColumns)
    {
        var columns = 0;
        var index = 0;
        while (index < text.Length &&
               (text[index] == ' ' || text[index] == '\t') &&
               columns < requiredColumns)
        {
            columns += text[index] == '\t' ? 4 - columns % 4 : 1;
            index++;
        }
        return columns >= requiredColumns ? text[index..] : text;
    }

    private static int LeadingIndentLength(ReadOnlySpan<char> text)
    {
        var index = 0;
        while (index < text.Length && (text[index] == ' ' || text[index] == '\t'))
            index++;
        return index;
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
        foreach (var character in text[(i + length)..])
        {
            if (character is not (' ' or '\t'))
                return false;
        }
        return true;
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

    private sealed record ListFrame(int MarkerIndent, int ContentIndent, long Start, string Marker);
}
