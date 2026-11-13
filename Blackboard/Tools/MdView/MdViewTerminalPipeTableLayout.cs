using Afrowave.Toolbox.ExchangeHouse.Markdown;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Conservative, line-local GFM-style pipe-table view for small terminals.
/// A source table row becomes numbered cell rows so a reader never loses
/// columns off the right edge or needs a full-document/table allocation.
/// </summary>
/// <remarks>
/// Only explicitly framed, unquoted pipe rows with 2–32 cells are handled.
/// Separator rows are represented by a single divider. This deliberately
/// avoids guessing at ambiguous delimiter-free pipe paragraphs and does
/// not claim full GFM table alignment, column widths or header semantics.
/// </remarks>
public static class MdViewTerminalPipeTableLayout
{
    /// <summary>Maximum columns accepted in one source pipe row.</summary>
    public const int MaximumColumns = 32;

    /// <summary>
    /// Recognizes and lays out one clearly framed table row outside source code.
    /// Both the disk page index and renderer consume the returned visual rows.
    /// </summary>
    /// <param name="line">Container-classified validated source line.</param>
    /// <param name="columns">Current terminal content width (8–4096).</param>
    /// <param name="rows">Formatted visual rows, or empty on non-table lines.</param>
    /// <returns>True only when the full framed row was recognized.</returns>
    public static bool TryLayout(
        MarkdownContainerLine line, int columns,
        out IReadOnlyList<MdViewTerminalDisplayRow> rows)
    {
        ArgumentNullException.ThrowIfNull(line);
        if (columns is < 8 or > 4096)
            throw new ArgumentOutOfRangeException(nameof(columns));
        rows = [];

        if (!TryReadCells(line, out var cells, out var isSeparator))
            return false;

        if (isSeparator)
        {
            rows = [new MdViewTerminalDisplayRow(
                [new MdViewTerminalStyledSpan(
                    new string('─', Math.Min(columns, 32)),
                    MdViewTerminalSpanRole.Marker)], false)];
            return true;
        }

        var result = new List<MdViewTerminalDisplayRow>();
        for (var i = 0; i < cells.Count; i++)
        {
            var first = (i + 1).ToString(
                System.Globalization.CultureInfo.InvariantCulture) + " │ ";
            var hanging = new string(' ', first.Length);
            // Preserve a minimum eight-column text area. When a very
            // narrow terminal cannot fit a cell index, show the text in
            // its own row rather than overflowing the screen.
            if (MdViewTerminalWordWrap.MeasureColumns(first) > columns - 8)
                first = hanging = "";
            result.AddRange(MdViewTerminalLineLayout.LayoutStyledContents(
                cells[i], columns, first, hanging,
                MdViewTerminalSpanRole.Body));
        }
        rows = result;
        return true;
    }

    /// <summary>
    /// Reads a conservatively recognized pipe row as bounded decoded cells.
    /// Shared by the compact reader and the opt-in native Spectre table
    /// inspector; it never treats fences, quoted text or list text as tables.
    /// </summary>
    /// <param name="line">Validated, container-classified source line.</param>
    /// <param name="cells">Unescaped cell contents, owned by the caller.</param>
    /// <param name="isSeparator">True for an all-delimiter alignment row.</param>
    /// <returns>True for a clearly framed two-to-32-column pipe row.</returns>
    public static bool TryReadCells(
        MarkdownContainerLine line, out IReadOnlyList<string> cells,
        out bool isSeparator)
    {
        ArgumentNullException.ThrowIfNull(line);
        cells = [];
        isSeparator = false;
        if (line.Kind != MarkdownContainerLineKind.Plain ||
            line.QuoteDepth != 0 ||
            line.Source.Kind == MarkdownStreamLineKind.Heading ||
            !TrySplitCells(line.Source.Text, out var parsed))
            return false;
        isSeparator = parsed.All(IsDelimiterCell);
        cells = isSeparator ? parsed :
            parsed.Select(UnescapePipes).ToArray();
        return true;
    }

    private static bool TrySplitCells(
        string source, out IReadOnlyList<string> cells)
    {
        cells = [];
        var line = source.Trim();
        if (line.Length < 5 || line[0] != '|' ||
            line[^1] != '|' || IsEscaped(line, line.Length - 1))
            return false;

        var found = new List<string>();
        var start = 1;
        var codeDelimiterLength = 0;
        for (var i = 1; i < line.Length; i++)
        {
            if (line[i] == '\u0060' && !IsEscaped(line, i))
            {
                var run = 1;
                while (i + run < line.Length && line[i + run] == '\u0060')
                    run++;
                if (codeDelimiterLength == 0)
                    codeDelimiterLength = run;
                else if (codeDelimiterLength == run)
                    codeDelimiterLength = 0;
                i += run - 1;
                continue;
            }

            if (line[i] != '|' || codeDelimiterLength != 0 ||
                IsEscaped(line, i))
                continue;

            if (found.Count >= MaximumColumns)
                return false;
            found.Add(line[start..i].Trim());
            start = i + 1;
        }

        if (start != line.Length || found.Count < 2 ||
            codeDelimiterLength != 0)
            return false;

        cells = found;
        return true;
    }

    private static bool IsDelimiterCell(string text)
    {
        var value = text.AsSpan().Trim();
        if (value.Length > 0 && value[0] == ':')
            value = value[1..];
        if (value.Length > 0 && value[^1] == ':')
            value = value[..^1];
        return value.Length >= 3 &&
            value.IndexOfAnyExcept('-') < 0;
    }

    private static bool IsEscaped(string source, int index)
    {
        var backslashes = 0;
        for (var i = index - 1; i >= 0 && source[i] == '\\'; i--)
            backslashes++;
        return (backslashes & 1) != 0;
    }

    private static string UnescapePipes(string text)
    {
        if (!text.Contains(@"\|", StringComparison.Ordinal))
            return text;

        // Escaping a table delimiter is a Markdown presentation rule,
        // but inside an inline code span the backslash is literal
        // source code. Preserve code spans and all other escapes.
        var builder = new System.Text.StringBuilder(text.Length);
        var codeDelimiterLength = 0;
        for (var index = 0; index < text.Length; index++)
        {
            if (text[index] == '\u0060' && !IsEscaped(text, index))
            {
                var run = 1;
                while (index + run < text.Length &&
                       text[index + run] == '\u0060')
                    run++;
                if (codeDelimiterLength == 0)
                    codeDelimiterLength = run;
                else if (codeDelimiterLength == run)
                    codeDelimiterLength = 0;
                builder.Append(text, index, run);
                index += run - 1;
                continue;
            }

            if (codeDelimiterLength == 0 && text[index] == '\\' &&
                index + 1 < text.Length && text[index + 1] == '|' &&
                !IsEscaped(text, index))
            {
                builder.Append('|');
                index++;
                continue;
            }
            builder.Append(text[index]);
        }
        return builder.ToString();
    }
}
