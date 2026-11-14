using Spectre.Console;
using Spectre.Console.Rendering;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>Why a native table cannot be safely placed into the requested visual page.</summary>
public enum MdViewNativeTableFallbackReason
{
    /// <summary>A native table was measured and is ready for display.</summary>
    None,
    /// <summary>The viewport cannot show the ordinary multi-column native grid.</summary>
    NarrowTerminal,
    /// <summary>The rounded border and native header alone exceed the row budget.</summary>
    HeaderTooTall,
    /// <summary>Even one source data row would exceed the remaining row budget.</summary>
    FirstRowTooTall,
    /// <summary>A continuation was requested after the table data had ended.</summary>
    NoContinuationRow
}

/// <summary>
/// Measured native table fragment, including exact source continuation.
/// No rows are consumed if <see cref="FallbackReason"/> is not None.
/// </summary>
/// <param name="HeaderLine">Physical source line containing the original Markdown header.</param>
/// <param name="FirstDataLine">First physical data line considered for this fragment.</param>
/// <param name="NextDataLine">First unconsumed data line, safe to resume on another page.</param>
/// <param name="DataRows">Number of physical source data rows included in the widget.</param>
/// <param name="VisualRows">Actual Spectre-rendered height of the widget, including borders.</param>
/// <param name="HasMore">True if the immediate next source line belongs to this table.</param>
/// <param name="Widget">Bounded native table to render, or null when compact fallback is required.</param>
/// <param name="FallbackReason">Reason the native fragment cannot be used.</param>
public sealed record MdViewNativeTablePageChunk(
    long HeaderLine, long FirstDataLine, long NextDataLine,
    int DataRows, int VisualRows, bool HasMore, Table? Widget,
    MdViewNativeTableFallbackReason FallbackReason)
{
    /// <summary>Whether this fragment has a measured, renderable native table.</summary>
    public bool IsNative => Widget is not null &&
        FallbackReason == MdViewNativeTableFallbackReason.None;
}

/// <summary>
/// Measures bounded source-table fragments for a future native in-page
/// Spectre layout. Does not write output or change the existing page index.
/// </summary>
/// <remarks>
/// The caller supplies the *remaining* visual content-row budget, not the
/// full terminal height. This planner produces exactly one native widget
/// with its source-bound resume line, or requests the compact fallback
/// without consuming any source line. Wide grid rendering deliberately
/// follows the native inspector's conservative width rule; narrow displays
/// retain the existing compact source layout until table-aware indexing
/// and transpose-source navigation are supported.
/// </remarks>
public static class MdViewNativeTablePagePlanner
{
    /// <summary>Maximum source rows in a measured native page fragment.</summary>
    public const int MaximumSourceRows = MdViewSpectreTableBlockReader.MaximumRowsPerChunk;

    /// <summary>
    /// Determines whether a native multi-column grid can fit horizontally.
    /// </summary>
    /// <param name="terminalWidth">Live terminal columns.</param>
    /// <param name="columnCount">Validated header cell count.</param>
    /// <returns>False when the compact format is safer.</returns>
    public static bool SupportsNativeGrid(int terminalWidth, int columnCount) =>
        columnCount is >= 2 and <= 6 &&
        terminalWidth >= Math.Max(64, 14 * columnCount);

    /// <summary>
    /// Plans one height-measured source fragment without writing to the
    /// target console or reading more than six data rows plus one lookahead.
    /// </summary>
    /// <param name="cursor">Caller-owned validated disk-backed source cursor.</param>
    /// <param name="header">Header returned by the source-bound block reader.</param>
    /// <param name="firstDataLine">Initial data line (header + 2), or an earlier fragment's NextDataLine.</param>
    /// <param name="console">Console whose actual render capabilities are used for measurement.</param>
    /// <param name="availableContentRows">Remaining viewport content rows, from 1 to 128.</param>
    /// <param name="maximumSourceRows">Source row cap from 1 to 6.</param>
    /// <param name="cancellationToken">Cancels source reads and bounded measurement.</param>
    /// <returns>Measured widget and exact resume line, or zero-consumption compact fallback.</returns>
    public static async Task<MdViewNativeTablePageChunk> PlanAsync(
        MdViewContainerPageCursor cursor,
        MdViewSpectreTableHeader header,
        long firstDataLine,
        IAnsiConsole console,
        int availableContentRows,
        int maximumSourceRows = MaximumSourceRows,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cursor);
        ArgumentNullException.ThrowIfNull(header);
        ArgumentNullException.ThrowIfNull(console);
        if (availableContentRows is < 1 or > 128)
            throw new ArgumentOutOfRangeException(nameof(availableContentRows));
        if (maximumSourceRows is < 1 or > MaximumSourceRows)
            throw new ArgumentOutOfRangeException(nameof(maximumSourceRows));
        if (header.SourceLine < 0 ||
            header.SourceLine > long.MaxValue - 2 ||
            header.Columns.Count is < 2 or > MdViewTerminalPipeTableLayout.MaximumColumns ||
            header.Separators.Count != header.Columns.Count)
            throw new ArgumentException("Source table header is invalid.", nameof(header));
        if (firstDataLine < header.SourceLine + 2)
            throw new ArgumentOutOfRangeException(nameof(firstDataLine));
        cancellationToken.ThrowIfCancellationRequested();

        MdViewNativeTablePageChunk Fallback(MdViewNativeTableFallbackReason reason) =>
            new(header.SourceLine, firstDataLine, firstDataLine,
                0, 0, false, null, reason);

        var terminalWidth = Math.Max(1, console.Profile.Width);
        if (!SupportsNativeGrid(terminalWidth, header.Columns.Count))
            return Fallback(MdViewNativeTableFallbackReason.NarrowTerminal);

        var current = MdViewSpectreTableFactory.CreateTable(
            terminalWidth, grid: true, header.Columns, header.Separators);
        var measured = MdViewSpectreTableHeight.CountRows(
            current, console, availableContentRows);
        if (measured > availableContentRows)
            return Fallback(MdViewNativeTableFallbackReason.HeaderTooTall);

        var consumed = 0;
        var hasMore = false;
        var accepted = new List<IRenderable[]>(maximumSourceRows);
        while (consumed < maximumSourceRows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var lineNumber = firstDataLine + consumed;
            var source = await cursor.ReadAsync(lineNumber, 1, cancellationToken)
                .ConfigureAwait(false);
            if (source.Count == 0 ||
                !MdViewSpectreTableBlockReader.TryReadDataRow(
                    source[0], header.Columns.Count, out var cells))
                break;

            // The same clipping and literal-data policy is used by the
            // inspector: no markup is evaluated from untrusted Markdown.
            var maxCell = Math.Clamp(
                (terminalWidth - 6) * 2 / header.Columns.Count,
                24, MdViewSpectreTableViewer.MaximumCellCharacters);
            IRenderable[] visualRow = cells.Select(cell =>
                (IRenderable)new Text(
                    MdViewSpectreTableFactory.PlainCell(cell, maxCell)))
                .ToArray();
            var candidate = MdViewSpectreTableFactory.CreateTable(
                terminalWidth, grid: true, header.Columns, header.Separators);
            foreach (var previous in accepted)
                candidate.AddRow(previous);
            candidate.AddRow(visualRow);

            var candidateHeight = MdViewSpectreTableHeight.CountRows(
                candidate, console, availableContentRows);
            if (candidateHeight > availableContentRows)
            {
                if (consumed == 0)
                    return Fallback(MdViewNativeTableFallbackReason.FirstRowTooTall);
                hasMore = true;
                break;
            }
            accepted.Add(visualRow);
            current = candidate;
            measured = candidateHeight;
            consumed++;
        }

        if (consumed == 0 && firstDataLine > header.SourceLine + 2)
            return Fallback(MdViewNativeTableFallbackReason.NoContinuationRow);

        // Probe just the next physical source line when the chunk cap
        // is reached. A row rejected by measurement already proves that
        // the same source row must be retried on the next visual page.
        if (consumed == maximumSourceRows && !hasMore)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var next = await cursor.ReadAsync(firstDataLine + consumed, 1,
                cancellationToken).ConfigureAwait(false);
            hasMore = next.Count != 0 &&
                MdViewSpectreTableBlockReader.TryReadDataRow(
                    next[0], header.Columns.Count, out _);
        }

        return new MdViewNativeTablePageChunk(
            header.SourceLine, firstDataLine, firstDataLine + consumed,
            consumed, measured, hasMore, current,
            MdViewNativeTableFallbackReason.None);
    }
}
