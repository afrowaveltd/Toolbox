using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Resumable source position for a mixed Markdown/native-table visual page.
/// A compact table retains its header and column count across page breaks.
/// </summary>
/// <param name="Line">Zero-based physical source line.</param>
/// <param name="WrapRow">First remaining visual row within the source line.</param>
/// <param name="CompactHeaderLine">Header of an active compact table, or -1.</param>
/// <param name="CompactColumns">Column count of the active compact table, or 0.</param>
public readonly record struct MdViewMixedVisualPosition(
    long Line, int WrapRow, long CompactHeaderLine = -1, int CompactColumns = 0)
{
    /// <summary>Beginning of a new Markdown document.</summary>
    public static MdViewMixedVisualPosition Beginning => new(0, 0);
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
public sealed record MdViewMixedVisualPage(
    MdViewMixedVisualPosition Start,
    MdViewMixedVisualPosition Next,
    int Rows, bool ContainsClippedCode, bool AtEnd,
    int TerminalWidth, int TerminalHeight, bool Unicode,
    IReadOnlyList<MdViewMixedVisualBlock> Blocks);

/// <summary>
/// Experimental one-page planner mixing formatted Markdown rows and complete
/// small native Spectre tables under one visual row budget.
/// </summary>
/// <remarks>
/// This deliberately does not replace MdViewTerminalPageIndex. The planner
/// accepts only confirmed whole tables with at most six data lines that fit
/// a *fresh* page. Longer or over-height tables are displayed in their
/// entirety through the existing compact line layout, with explicit compact
/// source state across page boundaries. All source access uses the validated
/// disk-backed cursor. No output is written during planning.
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
    /// <returns>One page with an exact next source position and renderable blocks.</returns>
    public static async Task<MdViewMixedVisualPage> PlanAsync(
        MdViewContainerPageCursor cursor, MdViewMixedVisualPosition start,
        IAnsiConsole console, int contentRows,
        CancellationToken cancellationToken = default)
    {
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
                 start.CompactColumns is < 2 or > MdViewTerminalPipeTableLayout.MaximumColumns)))
            throw new ArgumentOutOfRangeException(nameof(start));
        cancellationToken.ThrowIfCancellationRequested();

        var width = Math.Max(1, console.Profile.Width);
        var height = Math.Max(1, console.Profile.Height);
        var columns = Math.Clamp(width - 2, 8, 4096);
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
                            width, header.Columns.Count))
                    {
                        full = await MdViewNativeTablePagePlanner.PlanAsync(
                            cursor, header, position.Line + 2,
                            console, contentRows,
                            cancellationToken: cancellationToken).ConfigureAwait(false);
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

        return new MdViewMixedVisualPage(
            start, position, used, clipped, atEnd,
            width, height, console.Profile.Capabilities.Unicode,
            blocks);
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
