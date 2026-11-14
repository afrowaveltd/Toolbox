using System.Globalization;
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
    /// <param name="console">Interactive terminal that owns table rendering.</param>
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
        var header = await MdViewSpectreTableBlockReader.FindFirstHeaderAsync(
            cursor, firstLine, lastLineExclusive, cancellationToken)
            .ConfigureAwait(false);
        return header?.SourceLine;
    }

    /// <summary>
    /// Interactively browses a real Spectre table from a validated header.
    /// Keeps at most six data rows and at most eight transpose fields on
    /// screen per chunk, allowing next-chunk navigation without loading
    /// the full table. Escape restores the existing Markdown preview.
    /// </summary>
    /// <param name="cursor">Caller-owned disk-backed source cursor.</param>
    /// <param name="headerLine">Zero-based validated Markdown table header line.</param>
    /// <param name="cancellationToken">Cancels reads and terminal prompts.</param>
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

        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(
            cursor, headerLine, cancellationToken).ConfigureAwait(false);
        if (header is null)
            throw new InvalidDataException(
                "The source line is not followed by a valid pipe-table delimiter.");

        var headings = header.Columns;
        var separators = header.Separators;
        var sourceLine = headerLine + 2;
        var columnStart = 0;
        var completedRows = 0L;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Query the live console dimensions again on every chunk.
            var width = Math.Max(1, _console.Profile.Width);
            var height = Math.Max(1, _console.Profile.Height);
            if (width < 28 || height < 12)
            {
                _console.Write(new Text(
                    "Native table view requires at least 28 columns and 12 rows. " +
                    "Use the compact Markdown preview or enlarge the terminal." +
                    Environment.NewLine));
                return;
            }
            var grid = width >= Math.Max(64, 14 * headings.Count) &&
                headings.Count <= 6;

            var current = await cursor.ReadAsync(sourceLine, 1, cancellationToken)
                .ConfigureAwait(false);
            var hasRow = current.Count > 0 &&
                TryDataRow(current[0], headings.Count, out _);
            // Budget the actual Spectre-rendered rows, not an estimated
            // number of lines per Markdown cell. Leave enough room for
            // a header, status, navigation prompt and a blank separator.
            var reservedRows = height >= 24 ? 8 : 7;
            var tableHeightBudget = Math.Min(
                MdViewSpectreTableHeight.MaximumMeasuredRows,
                Math.Max(5, height - reservedRows));
            var table = CreateTable(width, grid, headings, separators);

            long followingLine = sourceLine;
            var nextColumn = 0;
            var displayedRows = 0;
            var acceptedRows = new List<IRenderable[]>();

            if (grid)
            {
                // Keep the former conservative chunk cap: the exact
                // measurement may lower it further for wrapped cells.
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
                    IRenderable[] candidateRow = cells.Select(cell =>
                        (IRenderable)new Text(PlainCell(cell, maxCell))).ToArray();
                    var candidate = CreateTable(width, grid, headings, separators);
                    foreach (var previous in acceptedRows)
                        candidate.AddRow(previous);
                    candidate.AddRow(candidateRow);
                    if (MdViewSpectreTableHeight.CountRows(
                        candidate, _console, tableHeightBudget) > tableHeightBudget)
                    {
                        if (displayedRows == 0)
                        {
                            _console.Write(new Text(
                                "Table cells cannot fit this terminal height. " +
                                "Use the compact Markdown preview." + Environment.NewLine));
                            return;
                        }
                        break;
                    }

                    acceptedRows.Add(candidateRow);
                    table = candidate;
                    followingLine++;
                    displayedRows++;
                }
            }
            else
            {
                if (hasRow)
                {
                    _ = TryDataRow(current[0], headings.Count, out var values);
                    var fields = Math.Min(MaximumFieldsPerChunk,
                        Math.Max(1, height - 12));
                    var stop = Math.Min(headings.Count, columnStart + fields);
                    for (var i = columnStart; i < stop; i++)
                    {
                        IRenderable[] candidateRow =
                        [
                            new Text(PlainCell(headings[i], 80)),
                            new Text(PlainCell(values[i],
                                Math.Clamp(width - 16, 24, MaximumCellCharacters)))
                        ];
                        var candidate = CreateTable(width, grid, headings, separators);
                        foreach (var previous in acceptedRows)
                            candidate.AddRow(previous);
                        candidate.AddRow(candidateRow);
                        if (MdViewSpectreTableHeight.CountRows(
                            candidate, _console, tableHeightBudget) > tableHeightBudget)
                        {
                            if (displayedRows == 0)
                            {
                                _console.Write(new Text(
                                    "Table field cannot fit this terminal height. " +
                                    "Use the compact Markdown preview." + Environment.NewLine));
                                return;
                            }
                            break;
                        }

                        acceptedRows.Add(candidateRow);
                        table = candidate;
                        displayedRows++;
                    }

                    var nextField = columnStart + displayedRows;
                    if (nextField < headings.Count)
                        nextColumn = nextField;
                    else
                        followingLine++;
                }
            }
            // A header-only source table must obey the same height
            // contract as a table with rows. Avoid letting a long
            // heading fill the terminal before the navigation prompt.
            if (MdViewSpectreTableHeight.CountRows(
                table, _console, tableHeightBudget) > tableHeightBudget)
            {
                _console.Write(new Text(
                    "The native table header cannot fit this terminal height. " +
                    "Use compact Markdown preview." + Environment.NewLine));
                return;
            }

            _console.Clear();
            WriteGrayLine(
                "Native Spectre table · source line " +
                (headerLine + 1).ToString(CultureInfo.InvariantCulture), width);
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
            WriteGrayLine(
                grid
                    ? "Data rows shown: " + displayedRows +
                      " · rows visited: " + completedRows
                    : "Source row " + (sourceLine + 1) +
                      " · fields " + (columnStart + 1) + "–" +
                      (nextColumn != 0 ? nextColumn : headings.Count) +
                      " of " + headings.Count,
                width);
            if (height >= 24)
                WriteGrayLine(
                    "… = clipped cell; full value remains in Markdown source.",
                    width);

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

    private void WriteGrayLine(string message, int terminalWidth)
    {
        var columns = Math.Clamp(terminalWidth - 2, 8, 4096);
        var shown = MdViewTerminalWordWrap.Wrap(
            message, columns, code: true)[0];
        _console.Write(new Text(shown + Environment.NewLine,
            new Style(foreground: Color.Grey)));
    }

    private static Table CreateTable(
        int width, bool grid,
        IReadOnlyList<string> headings, IReadOnlyList<string> separators)
    {
        var table = new Table()
            .RoundedBorder()
            .BorderColor(Color.Grey);
        table.Width(Math.Min(width - 2, 140));
        if (grid)
        {
            for (var column = 0; column < headings.Count; column++)
            {
                var heading = PlainCell(headings[column], 96);
                var alignment = separators[column].Trim();
                table.AddColumn(Markup.Escape(heading), configured =>
                {
                    if (alignment.StartsWith(':') && alignment.EndsWith(':'))
                        configured.Centered();
                    else if (alignment.EndsWith(':'))
                        configured.RightAligned();
                });
            }
        }
        else
        {
            table.AddColumn("Column");
            table.AddColumn("Value");
        }
        return table;
    }

    private static bool TryDataRow(
        MarkdownContainerLine line, int expectedColumns,
        out IReadOnlyList<string> cells) =>
        MdViewSpectreTableBlockReader.TryReadDataRow(
            line, expectedColumns, out cells);

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
