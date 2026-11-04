using System.Runtime.CompilerServices;
using System.Text;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Conservative line classification for a forward-only Markdown stream.
/// Container content is deliberately opaque until a subsequent stateful parser
/// can reproduce the existing list/quote ownership rules.
/// </summary>
public enum MarkdownStreamLineKind
{
    /// <summary>An empty or whitespace-only physical source line.</summary>
    Blank,
    /// <summary>A top-level ATX heading with one to six hash signs.</summary>
    Heading,
    /// <summary>The opening delimiter of a top-level fenced code block.</summary>
    FenceOpen,
    /// <summary>Uninterpreted content of the currently open fenced code block.</summary>
    CodeLine,
    /// <summary>The closing delimiter of the currently open fenced code block.</summary>
    FenceClose,
    /// <summary>A line beginning with a list/quote marker; ownership is not inferred.</summary>
    OpaqueContainer,
    /// <summary>An ordinary line whose multiline container is not yet resolved.</summary>
    ParagraphLine
}

/// <summary>
/// One strictly bounded decoded physical line from a forward-only stream.
/// </summary>
/// <param name="LineNumber">Zero-based physical line number.</param>
/// <param name="Utf16Offset">Zero-based UTF-16 character offset in the supplied decoded reader.</param>
/// <param name="Kind">Conservative syntax classification.</param>
/// <param name="Text">Original line content without its physical line terminator.</param>
/// <param name="Terminator">Original terminator: CRLF, CR, LF, or empty at EOF.</param>
/// <param name="HeadingLevel">ATX heading level, or zero for other lines.</param>
/// <param name="CodeLanguage">Optional first word of the active fence's info string.</param>
public sealed record MarkdownStreamLine(
    long LineNumber,
    long Utf16Offset,
    MarkdownStreamLineKind Kind,
    string Text,
    string Terminator,
    int HeadingLevel = 0,
    string? CodeLanguage = null)
{
    /// <summary>The complete original decoded physical line.</summary>
    public string Raw => Text + Terminator;
}

/// <summary>
/// Forward-only bounded Markdown line scanner with top-level fence and heading state.
/// It is an optional precursor to streaming block parsing, not a CommonMark parser.
/// </summary>
/// <remarks>
/// The caller owns the TextReader and must validate untrusted file bytes before
/// exposing partial results to a terminal. Later invalid text may be discovered
/// after earlier events have already been yielded. This scanner rejects unsafe
/// terminal controls, but does not validate source encoding or resolve references,
/// nested list ownership, setext headings or inline markdown.
/// </remarks>
public static class MarkdownIncrementalLineScanner
{
    /// <summary>Maximum decoded characters of an individual physical line.</summary>
    public const int DefaultMaxLineCharacters = 1024 * 1024;

    /// <summary>
    /// Yields individual physical lines while tracking top-level code-fence state.
    /// A line is emitted only after its terminator is complete (including CRLF
    /// across reader-buffer boundaries), or when EOF ends an unterminated line.
    /// </summary>
    /// <param name="source">Already decoded TextReader; caller owns its lifetime.</param>
    /// <param name="maxLineCharacters">Maximum line length: 1 to 4 Mi characters.</param>
    /// <param name="cancellationToken">Cancels pending asynchronous reads.</param>
    public static async IAsyncEnumerable<MarkdownStreamLine> ScanAsync(
        TextReader source,
        int maxLineCharacters = DefaultMaxLineCharacters,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (maxLineCharacters is < 1 or > 4 * 1024 * 1024)
            throw new ArgumentOutOfRangeException(nameof(maxLineCharacters));

        var buffer = new char[4096];
        var builder = new StringBuilder();
        long offset = 0;
        long start = 0;
        long lineNumber = 0;
        bool pendingCr = false;
        char fenceMarker = '\0';
        int fenceLength = 0;
        string? fenceLanguage = null;

        MarkdownStreamLine Finish(string terminator)
        {
            var line = builder.ToString();
            builder.Clear();

            MarkdownStreamLineKind kind;
            int headingLevel = 0;
            string? language = null;

            if (fenceLength > 0)
            {
                language = fenceLanguage;
                if (IsClosingFence(line, fenceMarker, fenceLength))
                {
                    kind = MarkdownStreamLineKind.FenceClose;
                    fenceMarker = '\0';
                    fenceLength = 0;
                    fenceLanguage = null;
                }
                else
                    kind = MarkdownStreamLineKind.CodeLine;
            }
            else if (string.IsNullOrWhiteSpace(line))
                kind = MarkdownStreamLineKind.Blank;
            else if (TryFence(line, out var marker, out var count))
            {
                kind = MarkdownStreamLineKind.FenceOpen;
                fenceMarker = marker;
                fenceLength = count;
                var info = line[(CountLeadingSpaces(line) + count)..].Trim();
                if (marker == '`' && info.Contains('`'))
                    info = "";
                if (info.Length > 0)
                {
                    var tokenLength = 0;
                    while (tokenLength < info.Length && !char.IsWhiteSpace(info[tokenLength]))
                        tokenLength++;
                    if (tokenLength is > 0 and <= 64)
                        fenceLanguage = info[..tokenLength];
                }
                language = fenceLanguage;
            }
            else if (TryHeading(line, out headingLevel))
                kind = MarkdownStreamLineKind.Heading;
            else if (IsContainerMarker(line))
                kind = MarkdownStreamLineKind.OpaqueContainer;
            else
                kind = MarkdownStreamLineKind.ParagraphLine;

            return new MarkdownStreamLine(lineNumber++, start,
                kind, line, terminator, headingLevel, language);
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var length = await source.ReadAsync(buffer.AsMemory(), cancellationToken)
                .ConfigureAwait(false);
            if (length == 0)
                break;

            for (var i = 0; i < length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var character = buffer[i];
                offset = checked(offset + 1);

                if (pendingCr)
                {
                    pendingCr = false;
                    if (character == '\n')
                    {
                        yield return Finish("\r\n");
                        start = offset;
                        continue;
                    }

                    // A lone CR terminates the previous line. The current
                    // character now belongs to the next line.
                    yield return Finish("\r");
                    start = offset - 1;
                }

                if (character == '\r')
                {
                    pendingCr = true;
                    continue;
                }
                if (character == '\n')
                {
                    yield return Finish("\n");
                    start = offset;
                    continue;
                }
                if (char.IsControl(character) && character != '\t')
                    throw new InvalidDataException(
                        "Markdown stream contains unsafe terminal controls.");
                if (builder.Length >= maxLineCharacters)
                    throw new InvalidDataException(
                        "A Markdown source line exceeds the configured character budget.");
                builder.Append(character);
            }
        }

        if (pendingCr)
            yield return Finish("\r");
        else if (builder.Length > 0)
            yield return Finish("");
    }

    private static int CountLeadingSpaces(string line)
    {
        var count = 0;
        while (count < 4 && count < line.Length && line[count] == ' ')
            count++;
        return count;
    }

    private static bool TryFence(string line, out char marker, out int length)
    {
        marker = '\0';
        length = 0;
        var indent = CountLeadingSpaces(line);
        if (indent > 3 || indent >= line.Length ||
            line[indent] is not ('`' or '~'))
            return false;
        marker = line[indent];
        var i = indent;
        while (i < line.Length && line[i] == marker)
            i++;
        length = i - indent;
        return length >= 3;
    }

    private static bool IsClosingFence(string line, char marker, int required)
    {
        if (!TryFence(line, out var found, out var count) ||
            found != marker || count < required)
            return false;
        return line[(CountLeadingSpaces(line) + count)..]
            .All(character => character is ' ' or '\t');
    }

    private static bool TryHeading(string line, out int level)
    {
        level = 0;
        var indent = CountLeadingSpaces(line);
        if (indent > 3 || indent >= line.Length || line[indent] != '#')
            return false;
        var i = indent;
        while (i < line.Length && line[i] == '#')
            i++;
        level = i - indent;
        if (level is < 1 or > 6 || (i < line.Length && line[i] is not (' ' or '\t')))
        {
            level = 0;
            return false;
        }
        return true;
    }

    private static bool IsContainerMarker(string line)
    {
        var indent = CountLeadingSpaces(line);
        if (indent > 3 || indent >= line.Length)
            return false;
        var c = line[indent];
        if (c == '>')
            return true;
        if (c is '-' or '+' or '*')
            return indent + 1 < line.Length &&
                (line[indent + 1] == ' ' || line[indent + 1] == '\t');
        if (!char.IsAsciiDigit(c))
            return false;

        var i = indent;
        while (i < line.Length && char.IsAsciiDigit(line[i]) && i - indent < 10)
            i++;
        return (i - indent is >= 1 and <= 9) &&
            i + 1 < line.Length &&
            (line[i] == '.' || line[i] == ')') &&
            (line[i + 1] == ' ' || line[i + 1] == '\t');
    }
}
