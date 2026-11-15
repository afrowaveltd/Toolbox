using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Resumable source position for a mixed Markdown/native-table visual page.
/// A compact table retains its header and column count across page breaks.
/// </summary>
/// <param name="Line">Zero-based physical source line.</param>
/// <param name="WrapRow">First remaining visual row within the source line.</param>
/// <param name="CompactHeaderLine">Header line of the active table, or -1 when not in a table. Also stores the repeated native header line during opt-in continuation.</param>
/// <param name="CompactColumns">Compact table column count, zero when inactive, or -1 for opt-in native continuation.</param>
public readonly record struct MdViewMixedVisualPosition(
    long Line, int WrapRow, long CompactHeaderLine = -1, int CompactColumns = 0)
{
    /// <summary>Beginning of a new Markdown document.</summary>
    public static MdViewMixedVisualPosition Beginning => new(0, 0);

    /// <summary>
    /// True when the existing fixed-size page record stores a long native
    /// table's repeating header rather than a compact table column count.
    /// </summary>
    public bool IsNativeContinuation =>
        CompactHeaderLine >= 0 && CompactColumns == -1;

    /// <summary>
    /// Constructs the exact next-source-line state for a continuing native
    /// table without increasing the 56-byte disk-page record size.
    /// </summary>
    /// <param name="headerLine">Original, validated table header line.</param>
    /// <param name="nextDataLine">First unconsumed physical table data line.</param>
    /// <returns>Source-aware native table continuation point.</returns>
    public static MdViewMixedVisualPosition NativeContinuation(
        long headerLine, long nextDataLine)
    {
        if (headerLine < 0 || headerLine > long.MaxValue - 2 ||
            nextDataLine < headerLine + 2)
            throw new ArgumentOutOfRangeException(nameof(nextDataLine));
        return new MdViewMixedVisualPosition(nextDataLine, 0, headerLine, -1);
    }
}

/// <summary>One bounded visual block in a planned mixed Markdown page.</summary>
/// <param name="VisualRows">Actual number of occupied terminal display rows.</param>
public abstract record MdViewMixedVisualBlock(int VisualRows);

/// <summary>Source-classified ordinary or compact Markdown display rows.</summary>
/// <param name="Source">Original container-classified physical source line.</param>
/// <param name="Rows">Bounded styled rows from the existing common layout.</param>
public sealed record MdViewMixedVisualTextBlock(
    MarkdownContainerLine Source, IReadOnlyList<MdViewTerminalDisplayRow> Rows)
    : MdViewMixedVisualBlock(Rows.Count);

/// <summary>One complete, native Spectre grid inside a mixed document page.</summary>
/// <param name="HeaderLine">Zero-based original table-header source line.</param>
/// <param name="NextSourceLine">First source line after this complete table.</param>
/// <param name="DataRows">Number of original data rows shown in the widget.</param>
/// <param name="Height">Actual measured Spectre widget height, including its border.</param>
/// <param name="Widget">Bounded Spectre table, formatted as literal source data.</param>
public sealed record MdViewMixedVisualTableBlock(
    long HeaderLine, long NextSourceLine, int DataRows, int Height, Table Widget)
    : MdViewMixedVisualBlock(Height);

/// <summary>
/// Planned page with explicit source resume state and bounded visual blocks.
/// Rendering and future disk page indexing can consume the same plan.
/// </summary>
/// <param name="Start">Source and compact-table state at the page beginning.</param>
/// <param name="Next">Precise first unconsumed source row/state.</param>
/// <param name="Rows">Visual rows occupied by all blocks.</param>
/// <param name="ContainsClippedCode">True if a displayed literal code row was clipped.</param>
/// <param name="AtEnd">True if the next source position is the document end.</param>
/// <param name="TerminalWidth">Terminal columns used to plan the page.</param>
/// <param name="TerminalHeight">Terminal rows used to plan the page.</param>
/// <param name="Unicode">Unicode capability used for Spectre measurement.</param>
/// <param name="Blocks">Ordered bounded blocks to render without reparsing source.</param>
/// <param name="MaximumContentColumns">Optional shared content width cap, or zero for automatic.</param>
/// <param name="LongNativeTables">Whether opt-in long native table continuation is enabled for this page.</param>
public sealed record MdViewMixedVisualPage(
    MdViewMixedVisualPosition Start,
    MdViewMixedVisualPosition Next,
    int Rows, bool ContainsClippedCode, bool AtEnd,
    int TerminalWidth, int TerminalHeight, bool Unicode,
    IReadOnlyList<MdViewMixedVisualBlock> Blocks,
    int MaximumContentColumns = 0, bool LongNativeTables = false);

/// <summary>
/// Experimental one-page planner mixing formatted Markdown rows and complete
/// small native Spectre tables under one visual row budget.
/// </summary>
/// <remarks>
/// This does not replace MdViewTerminalPageIndex. By default only confirmed
/// whole tables fitting a fresh page use native Spectre. When explicitly
/// enabled, source-complete long tables that pass a bounded streaming
/// preflight can use repeated-header, six-row native fragments across pages.
/// Unsafe or excessively long tables remain compact in their entirety.
/// Source access uses the validated disk-backed cursor; planning writes no output.
/// </remarks>
public static class MdViewMixedVisualPagePlanner
{
    /// <summary>
    /// Creates a bounded page plan from a resumable source position.
    /// </summary>
    /// <param name="cursor">Caller-owned validated source cursor.</param>
    /// <param name="start">Source position, including an active compact table if any.</param>
    /// <param name="console">Spectre terminal whose actual geometry is used.</param>
    /// <param name="contentRows">Available visible content rows, 1–128.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="maximumContentColumns">Optional maximum content width; zero retains full terminal width.</param>
    /// <param name="longNativeTables">Opt in to source-complete, repeated-header long native fragments.</param>
    /// <returns>One page with an exact next source position and renderable blocks.</returns>
    public static async Task<MdViewMixedVisualPage> PlanAsync(
        MdViewContainerPageCursor cursor, MdViewMixedVisualPosition start,
        IAnsiConsole console, int contentRows,
        CancellationToken cancellationToken = default,
        int maximumContentColumns = 0, bool longNativeTables = false)
    {
        MdViewMixedContentWidth.Validate(maximumContentColumns);
        ArgumentNullException.ThrowIfNull(cursor);
        ArgumentNullException.ThrowIfNull(console);
        if (contentRows is < 1 or > 128 ||
            contentRows > Math.Max(1, console.Profile.Height))
            throw new ArgumentOutOfRangeException(nameof(contentRows));
        if (start.Line < 0 || start.WrapRow < 0 ||
            start.CompactHeaderLine < -1 ||
            (start.CompactHeaderLine == -1 && start.CompactColumns != 0) ||
            (start.CompactHeaderLine >= 0 &&
                (start.CompactHeaderLine > start.Line ||
                 (start.CompactColumns != -1 &&
                  start.CompactColumns is < 2 or > MdViewTerminalPipeTableLayout.MaximumColumns))) ||
            (start.IsNativeContinuation &&
             (start.WrapRow != 0 ||
              start.CompactHeaderLine > long.MaxValue - 2 ||
              start.Line < start.CompactHeaderLine + 2 ||
              !longNativeTables))
            throw new ArgumentOutOfRangeException(nameof(start));
        cancellationToken.ThrowIfCancellationRequested();

        var width = Math.Max(1, console.Profile.Width);
        var height = Math.Max(1, console.Profile.Height);
        var unicode = console.Profile.Capabilities.Unicode;
        var effectiveWidth = MdViewMixedContentWidth.EffectiveTerminalWidth(
            width, maximumContentColumns);
        var columns = MdViewMixedContentWidth.ContentColumns(
            width, maximumContentColumns);
        var blocks = new List<MdViewMixedVisualBlock>();
        var position = start;
        var used = 0;
        var clipped = false;
        var atEnd = false;

        while (used < contentRows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = await cursor.ReadAsync(
                position.Line, 1, cancellationToken).ConfigureAwait(false);
            if (source.Count == 0)
            {
                atEnd = true;
                position = position with { CompactHeaderLine = -1, CompactColumns = 0 };
                break;
            }

            var classified = source[0];
            if (classified.Source.LineNumber != position.Line)
                throw new InvalidDataException("Mixed preview source cursor lost line alignment.");

            // Reconstruct exactly one continuing native fragment. The
            // source-complete preflight at the original table header has
            // already proved every fragment fits the fresh-page budget.
            // Repeated headers are included in the measured page height.
            if (position.IsNativeContinuation)
            {
                if (used != 0 || position.WrapRow != 0)
                    throw new InvalidDataException(
                        "Native table continuation must begin a visual page.");
                var repeatingHeader = await MdViewSpectreTableBlockReader.ReadHeaderAsync(
                    cursor, position.CompactHeaderLine, cancellationToken)
                    .ConfigureAwait(false);
                if (repeatingHeader is null ||
                    !MdViewSpectreTableBlockReader.TryReadDataRow(
                        classified, repeatingHeader.Columns.Count, out _))
                    throw new InvalidDataException(
                        "Native table continuation lost its validated source rows.");

                var continuation = await MdViewNativeTablePagePlanner.PlanAsync(
                    cursor, repeatingHeader, position.Line, console,
                    contentRows, cancellationToken: cancellationToken,
                    maximumContentColumns: maximumContentColumns)
                    .ConfigureAwait(false);
                if (!continuation.IsNative || continuation.DataRows == 0 ||
                    continuation.NextDataLine <= position.Line ||
                    continuation.Widget is null)
                    throw new InvalidDataException(
                        "Native table fragment no longer matches the indexed layout.");

                blocks.Add(new MdViewMixedVisualTableBlock(
                    repeatingHeader.SourceLine, continuation.NextDataLine,
                    continuation.DataRows, continuation.VisualRows,
                    continuation.Widget));
                used += continuation.VisualRows;
                position = continuation.HasMore
                    ? MdViewMixedVisualPosition.NativeContinuation(
                        repeatingHeader.SourceLine, continuation.NextDataLine)
                    : new MdViewMixedVisualPosition(continuation.NextDataLine, 0);
                if (continuation.HasMore)
                    break;
                continue;
            }

            // Preserve compact fallback for the whole source table, even
            // when that table spans many visual pages or individual rows.
            if (position.CompactHeaderLine >= 0 &&
                position.Line >= position.CompactHeaderLine + 2 &&
                !MdViewSpectreTableBlockReader.TryReadDataRow(
                    classified, position.CompactColumns, out _))
            {
                position = position with
                {
                    CompactHeaderLine = -1,
                    CompactColumns = 0
                };
            }

            if (position.WrapRow == 0 && position.CompactHeaderLine == -1 &&
                MayStartPipeHeader(classified))
            {
                var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(
                    cursor, position.Line, cancellationToken).ConfigureAwait(false);
                if (header is not null)
                {
                    // A six-row source read plus a single lookahead proves
                    // the table is complete. We never emit a native *part*
                    // of an unbounded table and later silently switch mode.
                    var sourceChunk = await MdViewSpectreTableBlockReader.ReadChunkAsync(
                        cursor, header, position.Line + 2,
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                    MdViewNativeTablePageChunk? full = null;
                    if (!sourceChunk.HasMore &&
                        MdViewNativeTablePagePlanner.SupportsNativeGrid(
                            effectiveWidth, header.Columns.Count))
                    {
                        full = await MdViewNativeTablePagePlanner.PlanAsync(
                            cursor, header, position.Line + 2,
                            console, contentRows,
                            cancellationToken: cancellationToken,
                            maximumContentColumns: maximumContentColumns)
                            .ConfigureAwait(false);
                    }

                    var entireNative = full is { IsNative: true, HasMore: false } &&
                        full.DataRows == sourceChunk.Rows.Count &&
                        full.NextDataLine == sourceChunk.NextDataLine;
                    if (entireNative)
                    {
                        var native = full!;
                        var remaining = contentRows - used;
                        if (native.VisualRows > remaining)
                        {
                            // Fits the next empty page. Do not consume a
                            // partial header or turn it into a compact table.
                            if (used > 0)
                                break;
                            throw new InvalidDataException(
                                "Native table did not fit its measured page budget.");
                        }

                        blocks.Add(new MdViewMixedVisualTableBlock(
                            header.SourceLine, native.NextDataLine,
                            native.DataRows, native.VisualRows, native.Widget!));
                        used += native.VisualRows;
                        position = new MdViewMixedVisualPosition(
                            native.NextDataLine, 0);
                        continue;
                    }

                    // Long-table opt-in is all-or-nothing: a bounded scan
                    // checks every fragment before any native source row is
                    // consumed. Otherwise the entire table stays compact.
                    if (longNativeTables &&
                        MdViewNativeTablePagePlanner.SupportsNativeGrid(
                            effectiveWidth, header.Columns.Count))
                    {
                        var fragment = await MdViewNativeTablePagePlanner.PlanAsync(
                            cursor, header, position.Line + 2, console,
                            contentRows, cancellationToken: cancellationToken,
                            maximumContentColumns: maximumContentColumns)
                            .ConfigureAwait(false);
                        if (fragment.IsNative && fragment.DataRows > 0 &&
                            await MdViewLongNativeTableEligibility
                                .CanRenderEntireTableAsync(
                                    cursor, header, console, contentRows,
                                    maximumContentColumns, cancellationToken)
                                .ConfigureAwait(false))
                        {
                            // Defer only a truly eligible native fragment.
                            // If preflight fails, preserve ordinary compact
                            // wrapping on the current page without creating
                            // an unnecessary split before the table header.
                            var remaining = contentRows - used;
                            if (fragment.VisualRows > remaining && used > 0)
                                break;

                            blocks.Add(new MdViewMixedVisualTableBlock(
                                header.SourceLine, fragment.NextDataLine,
                                fragment.DataRows, fragment.VisualRows,
                                fragment.Widget!));
                            used += fragment.VisualRows;
                            position = fragment.HasMore
                                ? MdViewMixedVisualPosition.NativeContinuation(
                                    header.SourceLine, fragment.NextDataLine)
                                : new MdViewMixedVisualPosition(
                                    fragment.NextDataLine, 0);
                            if (fragment.HasMore)
                                break;
                            continue;
                        }
                    }

                    // Full-table compact fallback: retain state across
                    // pages, including a header/delimiter split and rows
                    // whose soft-wrap positions cross page boundaries.
                    position = position with
                    {
                        CompactHeaderLine = header.SourceLine,
                        CompactColumns = header.Columns.Count
                    };
                }
            }

            var rows = MdViewTerminalLineLayout.Layout(classified, columns);
            if (rows.Count == 0 || position.WrapRow >= rows.Count)
                throw new InvalidDataException(
                    "Mixed page position exceeds the source-line layout.");
            var take = Math.Min(rows.Count - position.WrapRow,
                contentRows - used);
            var selected = rows.Skip(position.WrapRow).Take(take).ToArray();
            blocks.Add(new MdViewMixedVisualTextBlock(classified, selected));
            clipped |= selected.Any(row => row.IsClippedCode);
            used += take;

            position = position.WrapRow + take < rows.Count
                ? position with { WrapRow = position.WrapRow + take }
                : position with { Line = position.Line + 1, WrapRow = 0 };
        }

        // A page may end exactly on the last visual row. One bounded
        // lookahead identifies document completion without a phantom page.
        if (!atEnd && position.WrapRow == 0)
        {
            atEnd = (await cursor.ReadAsync(
                position.Line, 1, cancellationToken).ConfigureAwait(false)).Count == 0;
            if (atEnd)
                position = position with { CompactHeaderLine = -1, CompactColumns = 0 };
        }

        if (console.Profile.Width != width ||
            console.Profile.Height != height ||
            console.Profile.Capabilities.Unicode != unicode)
            throw new InvalidOperationException(
                "Terminal geometry changed during mixed-page planning.");

        return new MdViewMixedVisualPage(
            start, position, used, clipped, atEnd,
            width, height, unicode,
            blocks, maximumContentColumns, longNativeTables);
    }

    private static bool MayStartPipeHeader(MarkdownContainerLine line)
    {
        if (line.Kind != MarkdownContainerLineKind.Plain ||
            line.QuoteDepth != 0 ||
            MdViewTerminalPageIndex.IsLiteralCode(line))
            return false;

        // Avoid a disk-journal two-line probe on every ordinary prose
        // line. Use no temporary trimmed copy of a possibly long line.
        var source = line.Source.Text;
        var start = 0;
        while (start < source.Length && char.IsWhiteSpace(source[start]))
            start++;
        return source.Length - start >= 5 && source[start] == '|';
    }
}
