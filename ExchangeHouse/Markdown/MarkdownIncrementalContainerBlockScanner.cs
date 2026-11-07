using System.Runtime.CompilerServices;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Conservative source span categories for completed forward-only container
/// units. These are not rendered CommonMark blocks or complete list subtrees.
/// </summary>
public enum MarkdownContainerSpanKind
{
    /// <summary>A top-level ATX heading, represented by one physical source line.</summary>
    Heading,
    /// <summary>Contiguous plain top-level lines without a parsed inline structure.</summary>
    PlainRun,
    /// <summary>Contiguous explicitly quoted lines in the same quote and outer-list scope.</summary>
    QuoteRun,
    /// <summary>One explicit list item marker line, not its entire descendant subtree.</summary>
    ListItem,
    /// <summary>Contiguous list-owned continuation lines with identical owner and lazy flag.</summary>
    ListContentRun,
    /// <summary>Consecutive blank separators with the same resolved or unresolved owner.</summary>
    BlankRun,
    /// <summary>One top-level or quoted fenced code region, possibly interrupted or unclosed.</summary>
    FencedCode
}

/// <summary>How a fenced-code source span ended.</summary>
public enum MarkdownFenceSpanEnd
{
    /// <summary>The span is not a fenced code region.</summary>
    NotApplicable,
    /// <summary>A valid matching closing delimiter was encountered.</summary>
    Closed,
    /// <summary>A quote or outer-list scope change interrupted the fence.</summary>
    Interrupted,
    /// <summary>The source ended before a closing delimiter was encountered.</summary>
    EndOfFile
}

/// <summary>
/// A completed, contiguous source span with the ownership metadata needed by
/// a future disk-backed block index. The span retains no source text.
/// </summary>
/// <param name="Kind">Conservative source span category.</param>
/// <param name="StartUtf16Offset">Inclusive zero-based decoded UTF-16 source offset.</param>
/// <param name="EndUtf16Offset">Exclusive UTF-16 offset, including original line terminators.</param>
/// <param name="StartLine">Inclusive zero-based physical line number.</param>
/// <param name="EndLineExclusive">Exclusive zero-based physical line number.</param>
/// <param name="QuoteDepth">Depth of explicitly quoted source, or zero outside a quote.</param>
/// <param name="ListDepth">List depth relative to the current ordinary or quote-local list; -1 if none.</param>
/// <param name="ListOwnerUtf16Offset">Current ordinary or quote-local list item owner, when known.</param>
/// <param name="QuoteContainerListOwnerUtf16Offset">Separately tracked outer ordinary-list owner of a quote, when known.</param>
/// <param name="ListBlankOwnerUtf16Offset">Owner resolved by lookahead for a blank separator run, if any.</param>
/// <param name="IsLazyListContinuation">Whether the run is an unindented lazy list continuation.</param>
/// <param name="HeadingLevel">Top-level ATX heading level, or zero for other spans.</param>
/// <param name="ListMarker">Original list item marker only for a list-item span.</param>
/// <param name="CodeLanguage">First word of a code-fence information string, if known.</param>
/// <param name="FenceStartUtf16Offset">Original opening delimiter offset, only for fenced-code spans.</param>
/// <param name="FenceEnd">Matched, interrupted, or unterminated fence status; not applicable to other spans.</param>
public sealed record MarkdownContainerBlockSpan(
    MarkdownContainerSpanKind Kind,
    long StartUtf16Offset,
    long EndUtf16Offset,
    long StartLine,
    long EndLineExclusive,
    int QuoteDepth = 0,
    int ListDepth = -1,
    long? ListOwnerUtf16Offset = null,
    long? QuoteContainerListOwnerUtf16Offset = null,
    long? ListBlankOwnerUtf16Offset = null,
    bool IsLazyListContinuation = false,
    int HeadingLevel = 0,
    string? ListMarker = null,
    string? CodeLanguage = null,
    long? FenceStartUtf16Offset = null,
    MarkdownFenceSpanEnd FenceEnd = MarkdownFenceSpanEnd.NotApplicable)
{
    /// <summary>Decoded UTF-16 characters in the span, including physical line endings.</summary>
    public long Length => EndUtf16Offset - StartUtf16Offset;

    /// <summary>Number of physical source lines in this span.</summary>
    public long LineCount => EndLineExclusive - StartLine;
}

/// <summary>
/// Transforms bounded, validated-as-characters container events into disjoint
/// completed source spans. A code fence is emitted only after a closer,
/// interruption or EOF; long runs do not accumulate their source text.
/// </summary>
/// <remarks>
/// List-item spans cover their marker line only. The scanner does not attempt
/// to buffer entire list/quote subtrees or claim full CommonMark equivalence.
/// The caller must validate untrusted original bytes before sending any
/// progressive Markdown content to a terminal.
/// </remarks>
public static class MarkdownIncrementalContainerBlockScanner
{
    /// <summary>
    /// Reads a decoded Markdown source and yields disjoint completed spans with
    /// bounded lookahead for loose-list blank separators.
    /// </summary>
    /// <param name="source">Decoded reader owned by the caller.</param>
    /// <param name="maxLineCharacters">Per-line decoded character budget.</param>
    /// <param name="maxPendingBlankLines">Maximum deferred blank lines retained.</param>
    /// <param name="maxPendingBlankCharacters">Maximum decoded characters retained in deferred blanks.</param>
    /// <param name="cancellationToken">Cancellation token for all source reads.</param>
    /// <returns>Completed source spans in physical source order.</returns>
    public static async IAsyncEnumerable<MarkdownContainerBlockSpan> ScanAsync(
        TextReader source,
        int maxLineCharacters = MarkdownIncrementalLineScanner.DefaultMaxLineCharacters,
        int maxPendingBlankLines = MarkdownDeferredListOwnershipScanner.DefaultMaxPendingBlankLines,
        int maxPendingBlankCharacters = MarkdownDeferredListOwnershipScanner.DefaultMaxPendingBlankCharacters,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (maxLineCharacters is < 1 or > 4 * 1024 * 1024)
            throw new ArgumentOutOfRangeException(nameof(maxLineCharacters));
        if (maxPendingBlankLines < 1)
            throw new ArgumentOutOfRangeException(nameof(maxPendingBlankLines));
        if (maxPendingBlankCharacters < 1)
            throw new ArgumentOutOfRangeException(nameof(maxPendingBlankCharacters));

        MarkdownContainerBlockSpan? pending = null;
        await foreach (var line in MarkdownDeferredListOwnershipScanner.ScanAsync(
                           source, maxLineCharacters, maxPendingBlankLines,
                           maxPendingBlankCharacters, cancellationToken)
                           .WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var end = checked(line.Source.Utf16Offset + line.Source.Raw.Length);
            var endLine = checked(line.Source.LineNumber + 1);

            if (pending?.Kind == MarkdownContainerSpanKind.FencedCode)
            {
                if (line.Kind is MarkdownContainerLineKind.CodeLine or
                    MarkdownContainerLineKind.FenceClose &&
                    line.FenceStartUtf16Offset == pending.FenceStartUtf16Offset)
                {
                    var closed = line.Kind == MarkdownContainerLineKind.FenceClose;
                    pending = pending with
                    {
                        EndUtf16Offset = end,
                        EndLineExclusive = endLine,
                        FenceEnd = closed ? MarkdownFenceSpanEnd.Closed :
                            MarkdownFenceSpanEnd.NotApplicable
                    };
                    if (closed)
                    {
                        yield return pending;
                        pending = null;
                    }
                    continue;
                }

                // The next source line has already been classified outside this
                // fence. Preserve both source intervals without inventing a
                // closing delimiter or consuming the interrupting line.
                yield return pending with { FenceEnd = MarkdownFenceSpanEnd.Interrupted };
                pending = null;
            }

            if (line.Kind is MarkdownContainerLineKind.CodeLine or
                MarkdownContainerLineKind.FenceClose)
                throw new InvalidDataException(
                    "A fenced code continuation did not have an opening span.");

            var span = Create(line, end, endLine);
            if (span.Kind == MarkdownContainerSpanKind.FencedCode)
            {
                if (pending is not null)
                    yield return pending;
                pending = span;
                continue;
            }

            if (pending is not null)
            {
                if (CanMerge(pending, span))
                {
                    pending = pending with
                    {
                        EndUtf16Offset = span.EndUtf16Offset,
                        EndLineExclusive = span.EndLineExclusive
                    };
                    continue;
                }
                yield return pending;
            }
            pending = span;
        }

        if (pending is not null)
            yield return pending.Kind == MarkdownContainerSpanKind.FencedCode
                ? pending with { FenceEnd = MarkdownFenceSpanEnd.EndOfFile }
                : pending;
    }

    private static MarkdownContainerBlockSpan Create(
        MarkdownContainerLine line, long end, long endLine)
    {
        var kind = line.Kind switch
        {
            MarkdownContainerLineKind.ListItem => MarkdownContainerSpanKind.ListItem,
            MarkdownContainerLineKind.ListContinuation => MarkdownContainerSpanKind.ListContentRun,
            MarkdownContainerLineKind.Blank => MarkdownContainerSpanKind.BlankRun,
            MarkdownContainerLineKind.Quote => MarkdownContainerSpanKind.QuoteRun,
            MarkdownContainerLineKind.FenceOpen => MarkdownContainerSpanKind.FencedCode,
            MarkdownContainerLineKind.Plain when
                line.Source.Kind == MarkdownStreamLineKind.Heading =>
                MarkdownContainerSpanKind.Heading,
            MarkdownContainerLineKind.Plain => MarkdownContainerSpanKind.PlainRun,
            _ => throw new InvalidDataException("Unexpected source line classification.")
        };
        return new MarkdownContainerBlockSpan(
            kind, line.Source.Utf16Offset, end,
            line.Source.LineNumber, endLine,
            line.QuoteDepth, line.ListDepth,
            line.ListOwnerUtf16Offset,
            line.QuoteContainerListOwnerUtf16Offset,
            line.ListBlankOwnerUtf16Offset,
            line.IsLazyListContinuation,
            kind == MarkdownContainerSpanKind.Heading ? line.Source.HeadingLevel : 0,
            kind == MarkdownContainerSpanKind.ListItem ? line.ListMarker : null,
            kind == MarkdownContainerSpanKind.FencedCode ? line.CodeLanguage : null,
            kind == MarkdownContainerSpanKind.FencedCode ? line.FenceStartUtf16Offset : null);
    }

    private static bool CanMerge(
        MarkdownContainerBlockSpan left, MarkdownContainerBlockSpan right)
    {
        if (left.Kind != right.Kind ||
            left.Kind is MarkdownContainerSpanKind.FencedCode or
                MarkdownContainerSpanKind.Heading or MarkdownContainerSpanKind.ListItem)
            return false;

        return left.EndUtf16Offset == right.StartUtf16Offset &&
               left.EndLineExclusive == right.StartLine &&
               left.QuoteDepth == right.QuoteDepth &&
               left.ListDepth == right.ListDepth &&
               left.ListOwnerUtf16Offset == right.ListOwnerUtf16Offset &&
               left.QuoteContainerListOwnerUtf16Offset ==
                   right.QuoteContainerListOwnerUtf16Offset &&
               left.ListBlankOwnerUtf16Offset == right.ListBlankOwnerUtf16Offset &&
               left.IsLazyListContinuation == right.IsLazyListContinuation;
    }
}
