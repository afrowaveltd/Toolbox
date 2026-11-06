using System.Runtime.CompilerServices;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Conservative kinds of source spans produced by the incremental block scanner.
/// These are navigation/index units, not a complete Markdown syntax tree.
/// </summary>
public enum MarkdownStreamBlockKind
{
    /// <summary>A standalone top-level ATX heading line.</summary>
    Heading,
    /// <summary>Consecutive top-level plain paragraph lines, with no parsing of inline content.</summary>
    ParagraphRun,
    /// <summary>A standalone blank source line.</summary>
    Blank,
    /// <summary>A top-level fenced-code region, including its opening and optional closing delimiters.</summary>
    FencedCode,
    /// <summary>An unparsed quote or list-marker line; nested ownership remains unresolved.</summary>
    OpaqueContainer
}

/// <summary>
/// A completed source-range event, without an allocation proportional to the
/// number of lines in the represented block.
/// </summary>
/// <param name="Kind">Conservative block classification.</param>
/// <param name="StartUtf16Offset">Inclusive character offset in decoded UTF-16 code units.</param>
/// <param name="EndUtf16Offset">Exclusive character offset, including the last line terminator.</param>
/// <param name="StartLine">Inclusive zero-based physical line number.</param>
/// <param name="EndLineExclusive">Exclusive zero-based physical line number.</param>
/// <param name="HeadingLevel">ATX heading level (1–6), or zero for other blocks.</param>
/// <param name="CodeLanguage">First language token of a top-level fence, if present.</param>
/// <param name="FenceClosed">False only for a fenced-code block reaching EOF without a closing fence.</param>
public sealed record MarkdownStreamBlock(
    MarkdownStreamBlockKind Kind,
    long StartUtf16Offset,
    long EndUtf16Offset,
    long StartLine,
    long EndLineExclusive,
    int HeadingLevel = 0,
    string? CodeLanguage = null,
    bool FenceClosed = true)
{
    /// <summary>Number of UTF-16 code units occupied by the source range.</summary>
    public long Length => EndUtf16Offset - StartUtf16Offset;

    /// <summary>Number of complete physical lines covered by the source range.</summary>
    public long LineCount => EndLineExclusive - StartLine;
}

/// <summary>
/// Projects a bounded forward-only Markdown line stream into non-overlapping
/// source block spans without retaining the text of a complete code fence or
/// paragraph. Designed for future disk indexing, not for direct rendering.
/// </summary>
/// <remarks>
/// Only top-level ATX headings, fenced code, blank lines and plain paragraph
/// runs are grouped. Lists and blockquotes are emitted as opaque single-line
/// spans, preventing premature claims of CommonMark-compatible containment.
/// Inputs are already decoded; callers must validate an untrusted file in full
/// before displaying any progressively scanned content to a terminal.
/// </remarks>
public static class MarkdownIncrementalBlockScanner
{
    /// <summary>
    /// Enumerates source block spans with one-line lookahead and bounded memory.
    /// A fenced-code span is emitted when its closing fence is reached, or at EOF
    /// with <see cref="MarkdownStreamBlock.FenceClosed"/> set to false.
    /// </summary>
    /// <param name="source">Decoded source reader, owned and disposed by the caller.</param>
    /// <param name="maxLineCharacters">Maximum characters per physical line, passed to the line scanner.</param>
    /// <param name="cancellationToken">Cancels reads and the incremental scan.</param>
    /// <returns>A forward-only stream of non-overlapping source-range events.</returns>
    public static async IAsyncEnumerable<MarkdownStreamBlock> ScanAsync(
        TextReader source,
        int maxLineCharacters = MarkdownIncrementalLineScanner.DefaultMaxLineCharacters,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (maxLineCharacters is < 1 or > 4 * 1024 * 1024)
            throw new ArgumentOutOfRangeException(nameof(maxLineCharacters));

        long pendingStart = 0;
        long pendingEnd = 0;
        long pendingFirstLine = 0;
        long pendingLastLineExclusive = 0;
        string? pendingLanguage = null;
        bool inFence = false;
        bool inParagraph = false;

        await foreach (var line in MarkdownIncrementalLineScanner.ScanAsync(
                           source, maxLineCharacters, cancellationToken)
                           .WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            var end = checked(line.Utf16Offset + line.Text.Length + line.Terminator.Length);
            var nextLine = checked(line.LineNumber + 1);

            if (inFence)
            {
                // No code body text is stored. An arbitrarily long code fence
                // costs one pending range, not a growing StringBuilder.
                pendingEnd = end;
                pendingLastLineExclusive = nextLine;
                if (line.Kind == MarkdownStreamLineKind.FenceClose)
                {
                    yield return new MarkdownStreamBlock(MarkdownStreamBlockKind.FencedCode,
                        pendingStart, pendingEnd, pendingFirstLine,
                        pendingLastLineExclusive, CodeLanguage: pendingLanguage);
                    inFence = false;
                    pendingLanguage = null;
                }
                continue;
            }

            if (inParagraph)
            {
                if (line.Kind == MarkdownStreamLineKind.ParagraphLine)
                {
                    pendingEnd = end;
                    pendingLastLineExclusive = nextLine;
                    continue;
                }

                // The first non-paragraph line completes the preceding run.
                // This line is still processed below, so no source is dropped.
                yield return new MarkdownStreamBlock(MarkdownStreamBlockKind.ParagraphRun,
                    pendingStart, pendingEnd, pendingFirstLine, pendingLastLineExclusive);
                inParagraph = false;
            }

            if (line.Kind == MarkdownStreamLineKind.FenceOpen)
            {
                inFence = true;
                pendingStart = line.Utf16Offset;
                pendingEnd = end;
                pendingFirstLine = line.LineNumber;
                pendingLastLineExclusive = nextLine;
                pendingLanguage = line.CodeLanguage;
            }
            else if (line.Kind == MarkdownStreamLineKind.ParagraphLine)
            {
                inParagraph = true;
                pendingStart = line.Utf16Offset;
                pendingEnd = end;
                pendingFirstLine = line.LineNumber;
                pendingLastLineExclusive = nextLine;
            }
            else
            {
                var kind = line.Kind switch
                {
                    MarkdownStreamLineKind.Heading => MarkdownStreamBlockKind.Heading,
                    MarkdownStreamLineKind.Blank => MarkdownStreamBlockKind.Blank,
                    MarkdownStreamLineKind.OpaqueContainer => MarkdownStreamBlockKind.OpaqueContainer,
                    _ => throw new InvalidDataException(
                        "Unexpected line-scanner state outside a fenced-code region.")
                };
                yield return new MarkdownStreamBlock(kind,
                    line.Utf16Offset, end, line.LineNumber, nextLine,
                    HeadingLevel: line.HeadingLevel);
            }
        }

        if (inParagraph)
            yield return new MarkdownStreamBlock(MarkdownStreamBlockKind.ParagraphRun,
                pendingStart, pendingEnd, pendingFirstLine, pendingLastLineExclusive);

        if (inFence)
            yield return new MarkdownStreamBlock(MarkdownStreamBlockKind.FencedCode,
                pendingStart, pendingEnd, pendingFirstLine,
                pendingLastLineExclusive, CodeLanguage: pendingLanguage, FenceClosed: false);
    }
}
