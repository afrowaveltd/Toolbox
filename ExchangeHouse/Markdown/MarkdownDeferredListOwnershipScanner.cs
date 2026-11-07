using System.Runtime.CompilerServices;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Optional bounded lookahead for assigning loose-list blank separators after
/// a subsequent explicitly indented continuation proves their ownership.
/// </summary>
/// <remarks>
/// Source bytes should be validated in full before showing progressive output
/// to an untrusted terminal. This adapter keeps only a bounded run of pending
/// blank lines and does not retain a complete Markdown document.
/// </remarks>
public static class MarkdownDeferredListOwnershipScanner
{
    /// <summary>Maximum blank lines buffered before ownership resolution by default.</summary>
    public const int DefaultMaxPendingBlankLines = 1024;

    /// <summary>Default upper bound on the total UTF-16 characters held in pending blanks.</summary>
    public const int DefaultMaxPendingBlankCharacters = 1024 * 1024;

    /// <summary>
    /// Enumerates physical lines and assigns <see cref="MarkdownContainerLine.ListBlankOwnerUtf16Offset"/>
    /// only when the next nonblank event explicitly continues a list in the
    /// same quote depth and outer list container.
    /// </summary>
    /// <param name="source">Already decoded source reader, owned by the caller.</param>
    /// <param name="maxLineCharacters">Maximum physical-line length passed to the underlying scanner.</param>
    /// <param name="maxPendingBlankLines">Maximum number of blank events held before their ownership is known.</param>
    /// <param name="maxPendingBlankCharacters">Maximum sum of UTF-16 characters in buffered blank lines.</param>
    /// <param name="cancellationToken">Cancellation token for stream reads and processing.</param>
    /// <returns>A forward-only sequence of source-preserving container events.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A pending buffer budget is not positive.</exception>
    /// <exception cref="InvalidDataException">A pending run exceeds either configured budget.</exception>
    public static async IAsyncEnumerable<MarkdownContainerLine> ScanAsync(
        TextReader source,
        int maxLineCharacters = MarkdownIncrementalLineScanner.DefaultMaxLineCharacters,
        int maxPendingBlankLines = DefaultMaxPendingBlankLines,
        int maxPendingBlankCharacters = DefaultMaxPendingBlankCharacters,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (maxPendingBlankLines < 1)
            throw new ArgumentOutOfRangeException(nameof(maxPendingBlankLines));
        if (maxPendingBlankCharacters < 1)
            throw new ArgumentOutOfRangeException(nameof(maxPendingBlankCharacters));

        var pending = new List<MarkdownContainerLine>();
        long pendingCharacters = 0;

        await foreach (var item in MarkdownIncrementalContainerScanner.ScanAsync(
            source, maxLineCharacters, cancellationToken)
            .WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (item.Kind == MarkdownContainerLineKind.Blank)
            {
                if (pending.Count > 0 &&
                    (pending[0].QuoteDepth != item.QuoteDepth ||
                     pending[0].QuoteContainerListOwnerUtf16Offset !=
                         item.QuoteContainerListOwnerUtf16Offset))
                {
                    foreach (var oldBlank in pending)
                        yield return oldBlank;
                    pending.Clear();
                    pendingCharacters = 0;
                }

                if (pending.Count >= maxPendingBlankLines ||
                    pendingCharacters + item.Source.Raw.Length > maxPendingBlankCharacters)
                    throw new InvalidDataException(
                        "A pending Markdown blank separator run exceeds the configured budget.");

                pending.Add(item);
                pendingCharacters += item.Source.Raw.Length;
                continue;
            }

            if (pending.Count > 0)
            {
                // Only explicit indentation after a blank can establish an owner.
                // An unindented lazy paragraph or new list marker never does.
                var confirmed = item.Kind == MarkdownContainerLineKind.ListContinuation &&
                    !item.IsLazyListContinuation &&
                    item.ListOwnerUtf16Offset is not null &&
                    item.QuoteDepth == pending[0].QuoteDepth &&
                    item.QuoteContainerListOwnerUtf16Offset ==
                        pending[0].QuoteContainerListOwnerUtf16Offset;

                foreach (var blank in pending)
                    yield return confirmed
                        ? blank with { ListBlankOwnerUtf16Offset = item.ListOwnerUtf16Offset }
                        : blank;
                pending.Clear();
                pendingCharacters = 0;
            }

            yield return item;
        }

        // A trailing run has no following continuation to prove ownership.
        foreach (var blank in pending)
            yield return blank;
    }
}
