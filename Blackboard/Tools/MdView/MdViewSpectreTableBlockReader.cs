using Afrowave.Toolbox.ExchangeHouse.Markdown;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Source-bound header of an explicitly framed, top-level Markdown pipe table.
/// The delimiter cells are preserved for future Spectre column alignment.
/// </summary>
/// <param name="SourceLine">Zero-based physical line containing the table header.</param>
/// <param name="Columns">Decoded header cells, in source order.</param>
/// <param name="Separators">Original GFM alignment delimiter cells.</param>
public sealed record MdViewSpectreTableHeader(
    long SourceLine, IReadOnlyList<string> Columns,
    IReadOnlyList<string> Separators);

/// <summary>
/// One bounded, contiguous group of Markdown data rows. The next line is the
/// exact resume point, so no row is skipped or repeated between chunks.
/// </summary>
/// <param name="FirstDataLine">Zero-based first requested data line.</param>
/// <param name="NextDataLine">Zero-based first source line not consumed.</param>
/// <param name="Rows">Decoded data cells for at most six physical source rows.</param>
/// <param name="HasMore">True when the immediately following line is also a data row.</param>
public sealed record MdViewSpectreTableChunk(
    long FirstDataLine, long NextDataLine,
    IReadOnlyList<IReadOnlyList<string>> Rows, bool HasMore);

/// <summary>
/// Small, source-aware pipe-table block reader shared by the native inspector
/// and the future table-block visual paginator.
/// </summary>
/// <remarks>
/// Header recognition uses a two-line lookahead, including across a visual
/// page boundary; data rows must be immediately contiguous and match the
/// header width. This component never scans or buffers a complete table.
/// Source text is read only through the validated disk-backed cursor.
/// </remarks>
public static class MdViewSpectreTableBlockReader
{
    /// <summary>Maximum source data rows in a single bounded chunk.</summary>
    public const int MaximumRowsPerChunk = 6;

    /// <summary>
    /// Reads a genuine header and the immediately following matching
    /// delimiter row. Returns null for loose pipe text or unsafe containers.
    /// </summary>
    /// <param name="cursor">Validated, caller-owned source cursor.</param>
    /// <param name="headerLine">Zero-based candidate header line.</param>
    /// <param name="cancellationToken">Cancels the bounded read.</param>
    /// <returns>Validated header and delimiters, or null.</returns>
    public static async Task<MdViewSpectreTableHeader?> ReadHeaderAsync(
        MdViewContainerPageCursor cursor, long headerLine,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cursor);
        if (headerLine < 0)
            throw new ArgumentOutOfRangeException(nameof(headerLine));
        cancellationToken.ThrowIfCancellationRequested();
        if (headerLine > long.MaxValue - 2)
            return null;

        var pair = await cursor.ReadAsync(headerLine, 2, cancellationToken)
            .ConfigureAwait(false);
        if (pair.Count != 2 ||
            !MdViewTerminalPipeTableLayout.TryReadCells(
                pair[0], out var headings, out var firstIsSeparator) ||
            firstIsSeparator ||
            !MdViewTerminalPipeTableLayout.TryReadCells(
                pair[1], out var separators, out var isSeparator) ||
            !isSeparator || headings.Count != separators.Count)
            return null;

        return new MdViewSpectreTableHeader(
            headerLine, headings.ToArray(), separators.ToArray());
    }

    /// <summary>
    /// Finds the first table whose header begins in the requested visible
    /// physical-line range. A delimiter just beyond the page may be inspected
    /// but does not change the visible page's source boundaries.
    /// </summary>
    /// <param name="cursor">Validated, caller-owned source cursor.</param>
    /// <param name="firstLine">Inclusive first visible source line.</param>
    /// <param name="lastLineExclusive">Exclusive last visible source line.</param>
    /// <param name="cancellationToken">Cancels bounded lookahead.</param>
    /// <returns>First matching header, or null.</returns>
    public static async Task<MdViewSpectreTableHeader?> FindFirstHeaderAsync(
        MdViewContainerPageCursor cursor,
        long firstLine, long lastLineExclusive,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cursor);
        if (firstLine < 0 || lastLineExclusive < firstLine)
            throw new ArgumentOutOfRangeException(nameof(firstLine));

        var limit = Math.Min(lastLineExclusive,
            firstLine > long.MaxValue - 128 ? long.MaxValue : firstLine + 128);
        for (var line = firstLine; line < limit; line++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var header = await ReadHeaderAsync(cursor, line, cancellationToken)
                .ConfigureAwait(false);
            if (header is not null)
                return header;
        }
        return null;
    }

    /// <summary>
    /// Reads at most six matching data lines from an already validated header
    /// and identifies whether a following contiguous data line remains.
    /// </summary>
    /// <param name="cursor">Validated, caller-owned source cursor.</param>
    /// <param name="header">Previously validated header.</param>
    /// <param name="firstDataLine">Resume line, at or after header + delimiter.</param>
    /// <param name="maximumRows">Requested data rows, from 1 to 6.</param>
    /// <param name="cancellationToken">Cancels bounded reads.</param>
    /// <returns>Bounded chunk with an exact next source position.</returns>
    public static async Task<MdViewSpectreTableChunk> ReadChunkAsync(
        MdViewContainerPageCursor cursor, MdViewSpectreTableHeader header,
        long firstDataLine, int maximumRows = MaximumRowsPerChunk,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cursor);
        ArgumentNullException.ThrowIfNull(header);
        if (maximumRows is < 1 or > MaximumRowsPerChunk)
            throw new ArgumentOutOfRangeException(nameof(maximumRows));
        if (header.SourceLine < 0 || header.SourceLine > long.MaxValue - 2 ||
            header.Columns.Count is < 2 or > MdViewTerminalPipeTableLayout.MaximumColumns ||
            header.Separators.Count != header.Columns.Count)
            throw new ArgumentException("The table header is invalid.", nameof(header));
        if (firstDataLine < header.SourceLine + 2)
            throw new ArgumentOutOfRangeException(nameof(firstDataLine));
        cancellationToken.ThrowIfCancellationRequested();

        var rows = new List<IReadOnlyList<string>>(maximumRows);
        var next = firstDataLine;
        while (rows.Count < maximumRows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = await cursor.ReadAsync(next, 1, cancellationToken)
                .ConfigureAwait(false);
            if (source.Count == 0 ||
                !TryReadDataRow(source[0], header.Columns.Count, out var cells))
                break;
            rows.Add(cells.ToArray());
            next++;
        }

        var hasMore = false;
        if (rows.Count == maximumRows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var following = await cursor.ReadAsync(next, 1, cancellationToken)
                .ConfigureAwait(false);
            hasMore = following.Count != 0 &&
                TryReadDataRow(following[0], header.Columns.Count, out _);
        }

        return new MdViewSpectreTableChunk(
            firstDataLine, next, rows, hasMore);
    }

    /// <summary>
    /// Recognizes one contiguous data row of exactly the specified width.
    /// Fences, lists, quotes and delimiter rows are excluded.
    /// </summary>
    /// <param name="line">Container-classified source line.</param>
    /// <param name="expectedColumns">Header column count, 2–32.</param>
    /// <param name="cells">Decoded source cells or an empty list on failure.</param>
    /// <returns>True when the row belongs to the current table.</returns>
    public static bool TryReadDataRow(
        MarkdownContainerLine line, int expectedColumns,
        out IReadOnlyList<string> cells)
    {
        ArgumentNullException.ThrowIfNull(line);
        if (expectedColumns is < 2 or > MdViewTerminalPipeTableLayout.MaximumColumns)
            throw new ArgumentOutOfRangeException(nameof(expectedColumns));
        if (!MdViewTerminalPipeTableLayout.TryReadCells(
                line, out cells, out var separator) ||
            separator || cells.Count != expectedColumns)
        {
            cells = [];
            return false;
        }
        return true;
    }
}
