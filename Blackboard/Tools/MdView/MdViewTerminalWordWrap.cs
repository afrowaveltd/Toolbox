using System.Globalization;
using System.Text;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>Deterministic visual-width-aware source-line wrapping for terminal preview.</summary>
/// <remarks>
/// Prose prefers sentence punctuation, clauses and word boundaries, while
/// fenced code is never soft-wrapped: a long code line is clipped with a
/// right-arrow indicator and the validated source remains unchanged.
/// Width is measured in terminal cells, not UTF-16 code units.
/// </remarks>
public static class MdViewTerminalWordWrap
{
    /// <summary>Default right-side marker for horizontally clipped code.</summary>
    public const string CodeOverflowMarker = "→";

    /// <summary>
    /// Lays out one physical source line into display rows for a known width.
    /// The result is also used to build exact deterministic page boundaries.
    /// </summary>
    /// <param name="source">One validated physical line without newline.</param>
    /// <param name="columns">Available terminal columns, 8–4096.</param>
    /// <param name="code">True for code fence, code body or closing fence.</param>
    /// <returns>One or more display rows, preserving source content where possible.</returns>
    public static IReadOnlyList<string> Wrap(string source, int columns, bool code = false)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (columns is < 8 or > 4096)
            throw new ArgumentOutOfRangeException(nameof(columns));

        var expanded = source.Replace("\t", "    ", StringComparison.Ordinal);
        if (code)
            return [ClipCode(expanded, columns)];
        // Both ordinary callers and styled Markdown layout must use
        // identical source positions and break rules.
        return SliceProse(expanded, columns)
            .Select(slice => slice.Text).ToArray();
    }

    /// <summary>
    /// Returns source-positioned rows of already display-normalized prose.
    /// Unlike a list of strings, each slice identifies the exact visible
    /// characters needed to retain inline styling across soft line breaks.
    /// </summary>
    /// <param name="text">Display text with tabs already expanded.</param>
    /// <param name="columns">Content width measured in terminal cells.</param>
    /// <returns>At least one row, with UTF-16-safe start and length offsets.</returns>
    public static IReadOnlyList<MdViewTerminalWrapSlice> SliceProse(
        string text, int columns)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (columns is < 8 or > 4096)
            throw new ArgumentOutOfRangeException(nameof(columns));
        if (text.Length == 0)
            return [new MdViewTerminalWrapSlice(0, 0, "")];

        var rows = new List<MdViewTerminalWrapSlice>();
        var start = 0;
        while (start < text.Length)
        {
            var limit = FittingEnd(text, start, columns);
            if (limit >= text.Length)
            {
                rows.Add(new MdViewTerminalWrapSlice(start,
                    text.Length - start, text[start..]));
                break;
            }
            var breakAt = FindNaturalBreak(text, start, limit);
            if (breakAt <= start)
                breakAt = limit;
            var end = breakAt;
            while (end > start && char.IsWhiteSpace(text[end - 1]))
                end--;
            rows.Add(new MdViewTerminalWrapSlice(start, end - start,
                text[start..end]));
            start = breakAt;
            while (start < text.Length && text[start] == ' ')
                start++;
        }
        return rows.Count == 0
            ? [new MdViewTerminalWrapSlice(0, 0, "")] : rows;
    }

    /// <summary>Counts visual terminal cells, with a conservative wide-rune model.</summary>
    /// <param name="source">Unicode text.</param>
    /// <returns>Measured columns, treating combining marks as zero width.</returns>
    public static int MeasureColumns(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var columns = 0;
        foreach (var rune in source.EnumerateRunes())
            columns = checked(columns + CellWidth(rune));
        return columns;
    }

    private static string ClipCode(string source, int columns)
    {
        if (MeasureColumns(source) <= columns)
            return source;
        var end = FittingEnd(source, 0, columns - 1);
        return source[..end] + CodeOverflowMarker;
    }

    private static int FittingEnd(string source, int start, int columns)
    {
        var width = 0;
        var index = start;
        while (index < source.Length)
        {
            var rune = Rune.GetRuneAt(source, index);
            var cells = CellWidth(rune);
            if (width + cells > columns)
                break;
            width += cells;
            index += rune.Utf16SequenceLength;
        }
        return index;
    }

    private static int FindNaturalBreak(string source, int start, int limit)
    {
        var minimum = start + (limit - start) / 2;
        var word = -1;
        var clause = -1;
        var sentence = -1;
        for (var index = start + 1; index <= limit; index++)
        {
            // A break is only legal after a complete word (and is safe
            // from splitting Unicode surrogate pairs).
            if (!char.IsWhiteSpace(source[index - 1]))
                continue;
            var previous = index - 2;
            while (previous >= start && char.IsWhiteSpace(source[previous]))
                previous--;
            if (previous < start)
                continue;
            word = index;
            if (index >= minimum)
            {
                if (source[previous] is '.' or '!' or '?')
                    sentence = index;
                else if (source[previous] is ',' or ';' or ':')
                    clause = index;
            }
        }
        return sentence > start ? sentence : clause > start ? clause : word;
    }

    private static int CellWidth(Rune rune)
    {
        var category = Rune.GetUnicodeCategory(rune);
        if (category is UnicodeCategory.NonSpacingMark or
            UnicodeCategory.EnclosingMark or UnicodeCategory.Format)
            return 0;

        var value = rune.Value;
        // Conservative East-Asian wide and emoji ranges. A terminal's
        // font/emoji configuration can differ; the algorithm is stable
        // across builds and never splits UTF-16 pairs.
        return value is >= 0x1100 and <= 0x115F or
            >= 0x2329 and <= 0x232A or
            >= 0x2E80 and <= 0xA4CF or
            >= 0xAC00 and <= 0xD7A3 or
            >= 0xF900 and <= 0xFAFF or
            >= 0xFE10 and <= 0xFE6F or
            >= 0xFF00 and <= 0xFF60 or
            >= 0xFFE0 and <= 0xFFE6 or
            >= 0x1F300 and <= 0x1FAFF or
            >= 0x20000 and <= 0x3FFFD ? 2 : 1;
    }
}

/// <summary>
/// Displayed prose segment with exact UTF-16 coordinates in its
/// previously normalized visible text, not the original Markdown source.
/// </summary>
/// <param name="Start">Zero-based display-text offset.</param>
/// <param name="Length">Displayed UTF-16 length.</param>
/// <param name="Text">Visible row text.</param>
public sealed record MdViewTerminalWrapSlice(int Start, int Length, string Text);
