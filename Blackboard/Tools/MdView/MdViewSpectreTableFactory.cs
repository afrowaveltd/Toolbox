using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Shared bounded native Spectre table construction and literal cell
/// presentation for both the modal inspector and future page layout.
/// </summary>
internal static class MdViewSpectreTableFactory
{
    /// <summary>Creates the same bordered, aligned native widget used by the inspector.</summary>
    /// <param name="width">Detected physical terminal width.</param>
    /// <param name="grid">True for an ordinary multi-column grid; otherwise two-column transpose.</param>
    /// <param name="headings">Decoded source header cells.</param>
    /// <param name="separators">Matching GFM alignment cells.</param>
    /// <returns>New, caller-owned native Spectre table.</returns>
    internal static Table CreateTable(
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

    /// <summary>Formats a validated source cell as clipped literal Spectre text.</summary>
    /// <param name="source">Decoded Markdown cell source.</param>
    /// <param name="maximumCharacters">Clipping budget, at least 2 characters.</param>
    /// <returns>Safe visible label, never terminal markup.</returns>
    internal static string PlainCell(string source, int maximumCharacters)
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
