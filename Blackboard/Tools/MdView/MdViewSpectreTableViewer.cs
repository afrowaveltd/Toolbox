using System.Globalization;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Optional, bounded native Spectre.Console table inspector for validated
/// Markdown pipe tables, independent of the source-page height model.
/// </summary>
/// <remarks>
/// Only a small chunk of source rows is loaded at once. Narrow displays use
/// a real two-column Spectre table (field/value), with column paging for
/// wide Markdown records. Larger terminals show an ordinary multi-column
/// rounded Spectre table. This is a modal view, not a replacement for the
/// existing disk-indexed visual source pages.
/// </remarks>
public sealed class MdViewSpectreTableViewer
{
    /// <summary>Maximum physical Markdown data rows in one table chunk.</summary>
    public const int MaximumRowsPerChunk = 6;

    /// <summary>Maximum field columns in one transposed modal table chunk.</summary>
    public const int MaximumFieldsPerChunk = 8;

    /// <summary>Maximum source characters inspected per displayed cell.</summary>
    public const int MaximumCellCharacters = 256;

    private readonly IAnsiConsole _console;

    /// <summary>Creates the native table viewer for a Spectre terminal.</summary>
    public MdViewSpectreTableViewer(IAnsiConsole console) =>
        _console = console ?? throw new ArgumentNullException(nameof(console));

    /// <summary>
    /// Searches at most one visible source page for an explicit pipe
    /// header followed immediately by a matching delimiter row.
    /// </summary>
    /// <param name="cursor">Caller-owned validated source cursor.</param>
    /// <param name="firstLine">First visible physical source line.</param>
    /// <param name="lastLineExclusive">End of current visible range.</param>
    /// <param name="cancellationToken">Cancels bounded source reads.</param>
    /// <returns>First matching zero-based header line, or null.</returns>
    public static async Task<long?> FindFirstTableAsync(
        MdViewContainerPageCursor cursor,
        long firstLine, long lastLineExclusive,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cursor);
        if (firstLine < 0 || lastLineExclusive < firstLine)
            throw new ArgumentOutOfRangeException(nameof(firstLine));
        var upper = Math.Min(lastLineExclusive, firstLine + 128);
        for (var sourceLine = firstLine; sourceLine < upper; sourceLine++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pair = await cursor.ReadAsync(sourceLine, 2, cancellationToken)
                .ConfigureAwait(false);
            if (pair.Count < 2 ||
                !MdViewTerminalPipeTableLayout.TryReadCells(
                    pair[0], out var names, out var firstIsDivider) ||
                firstIsDivider ||
                !MdViewTerminalPipeTableLayout.TryReadCells(
                    pair[1], out var separators, out var isDivider) ||
                !isDivider || names.Count != separators.Count)
                continue;
            return sourceLine;
        }
        return null;
    }

    /// <summary>
    /// Interactively browses a real Spectre table from a validated header.
    /// Keeps at most six data rows and at most eight transpose fields on
    /// screen per chunk, allowing next-chunk navigation without loading
    /// the full table. Escape restores the existing Markdown preview.
    /// </summary>
    public async Task RunAsync(
        MdViewContainerPageCursor cursor, long headerLine,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cursor);
        if (headerLine < 0)
            throw new ArgumentOutOfRangeException(nameof(headerLine));
        if (!_console.Profile.Capabilities.Interactive)
            throw new InvalidOperationException(
                "Native Spectre table viewing requires an interactive terminal.");

        var headerPair = await cursor.ReadAsync(headerLine, 2, cancellationToken)
            .ConfigureAwait(false);
        if (headerPair.Count < 2 ||
            !MdViewTerminalPipeTableLayout.TryReadCells(
                headerPair[0], out var headings, out var isHeaderDivider) ||
            isHeaderDivider ||
            !MdViewTerminalPipeTableLayout.TryReadCells(
                headerPair[1], out var separators, out var isSeparator) ||
            !isSeparator || headings.Count != separators.Count)
            throw new InvalidDataException(
                "The source line is not followed by a valid pipe-table delimiter.");

        var sourceLine = headerLine + 2;
        var columnStart = 0;
        var completedRows = 0L;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Query the live console dimensions again on every chunk.
            var width = Math.Max(1, _console.Profile.Width);
            var height = Math.Max(1, _console.Profile.Height);
            if (width < 28)
            {
                _console.Write(new Text(
                    "Native table view needs at least 28 columns. " +
                    "Use the compact Markdown preview or widen the terminal." +
                    Environment.NewLine));
                return;
            }
            var grid = width >= Math.Max(64, 14 * headings.Count) &&
                headings.Count <= 6;

            var current = await cursor.ReadAsync(sourceLine, 1, cancellationToken)
                .ConfigureAwait(false);
            var hasRow = current.Count > 0 &&
                TryDataRow(current[0], headings.Count, out _);
            var table = new Table()
                .RoundedBorder()
                .BorderColor(Color.Grey);
            if (width >= 20)
                table.Width(Math.Min(width - 2, 140));

            long followingLine = sourceLine;
            var nextColumn = 0;
            var displayedRows = 0;
            if (grid)
            {
                for (var column = 0; column < headings.Count; column++)
                {
                    var heading = PlainCell(headings[column], 96);
                    var alignment = separators[column].Trim();
                    table.AddColumn(Markup.Escape(heading), col =>
                    {
                        if (alignment.StartsWith(':') && alignment.EndsWith(':'))
                            col.Centered();
                        else if (alignment.EndsWith(':'))
                            col.RightAligned();
                    });
                }

                var limit = Math.Min(MaximumRowsPerChunk,
                    Math.Max(1, (height - 12) / 3));
                while (displayedRows < limit)
                {
                    var result = await cursor.ReadAsync(
                        followingLine, 1, cancellationToken).ConfigureAwait(false);
                    if (result.Count == 0 ||
                        !TryDataRow(result[0], headings.Count, out var cells))
                        break;
                    var maxCell = Math.Clamp(
                        (width - 6) * 2 / headings.Count,
                        24, MaximumCellCharacters);
                    table.AddRow(cells.Select(cell =>
                        (IRenderable)new Text(PlainCell(cell, maxCell))).ToArray());
                    followingLine++;
                    displayedRows++;
                }
            }
            else
            {
                table.AddColumn("Column");
                table.AddColumn("Value");
                if (hasRow)
                {
                    _ = TryDataRow(current[0], headings.Count, out var values);
                    var fields = Math.Min(MaximumFieldsPerChunk,
                        Math.Max(1, height - 12));
                    var stop = Math.Min(headings.Count, columnStart + fields);
                    for (var i = columnStart; i < stop; i++)
                    {
                        table.AddRow(
                            new Text(PlainCell(headings[i], 80)),
                            new Text(PlainCell(values[i],
                                Math.Clamp(width - 16, 24, MaximumCellCharacters))));
                        displayedRows++;
                    }
                    if (stop < headings.Count)
                        nextColumn = stop;
                    else
                        followingLine++;
                }
            }

            _console.Clear();
            _console.Write(new Text(
                "Native Spectre table · source line " +
                (headerLine + 1).ToString(CultureInfo.InvariantCulture) +
                Environment.NewLine,
                new Style(foreground: Color.Grey)));
            _console.WriteLine();
            _console.Write(table);
            _console.WriteLine();
            if (grid)
                completedRows += displayedRows;
            // A full chunk does not imply another row exists: inspect only
            // the single next source line before offering Next.
            var hasMore = nextColumn != 0;
            if (!hasMore && followingLine > sourceLine)
            {
                var probe = await cursor.ReadAsync(
                    followingLine, 1, cancellationToken).ConfigureAwait(false);
                hasMore = probe.Count > 0 &&
                    TryDataRow(probe[0], headings.Count, out _);
            }
            _console.Write(new Text(
                (grid
                    ? "Data rows shown: " + displayedRows +
                      " · rows visited: " + completedRows
                    : "Source row " + (sourceLine + 1) +
                      " · fields " + (columnStart + 1) + "–" +
                      (nextColumn != 0 ? nextColumn : headings.Count) +
                      " of " + headings.Count) +
                Environment.NewLine,
                new Style(foreground: Color.Grey)));
            _console.Write(new Text(
                "Long cells show an ellipsis; full source remains in Markdown preview." +
                Environment.NewLine,
                new Style(foreground: Color.Grey)));

            const string next = "Next table chunk";
            const string back = "Back to Markdown preview";
            var choices = new List<string>();
            if (hasMore)
                choices.Add(next);
            choices.Add(back);
            var selected = await new SelectionPrompt<string>()
                .Title("Spectre table navigation")
                .PageSize(3)
                .AddChoices(choices)
                .AddCancelResult(back)
                .ShowAsync(_console, cancellationToken).ConfigureAwait(false);
            if (selected == back)
                return;

            if (nextColumn > 0)
                columnStart = nextColumn;
            else
            {
                sourceLine = followingLine;
                columnStart = 0;
            }
        }
    }

    private static bool TryDataRow(
        MarkdownContainerLine line, int expectedColumns,
        out IReadOnlyList<string> cells)
    {
        if (!MdViewTerminalPipeTableLayout.TryReadCells(
                line, out cells, out var delimiter) ||
            delimiter || cells.Count != expectedColumns)
        {
            cells = [];
            return false;
        }
        return true;
    }

    private static string PlainCell(string source, int maximumCharacters)
    {
        // Source line validation already rejects unsafe terminal controls.
        // Clip before the inline parser to cap allocations from untrusted
        // Markdown even if a single source cell is extremely long.
        if (source.Length > maximumCharacters)
        {
            var limit = maximumCharacters - 1;
            if (char.IsHighSurrogate(source[limit - 1]))
                limit--;
            source = source[..limit] + "…";
        }
        var rows = MdViewTerminalLineLayout.LayoutStyledContents(
            source, 4096, "", "", MdViewTerminalSpanRole.Body);
        return string.Concat(rows.Select(row => row.Text));
    }
}
