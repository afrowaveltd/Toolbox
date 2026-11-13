using System.Text;
using Afrowave.Toolbox.ExchangeHouse.Markdown;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>Semantic display roles for source-bound Markdown text spans.</summary>
public enum MdViewTerminalSpanRole
{
    /// <summary>Ordinary prose.</summary>
    Body,
    /// <summary>A heading.</summary>
    Heading,
    /// <summary>A list or quote marker and continuation indentation.</summary>
    Marker,
    /// <summary>Quoted prose.</summary>
    Quote,
    /// <summary>Strong emphasis.</summary>
    Strong,
    /// <summary>Emphasis.</summary>
    Emphasis,
    /// <summary>Visible label of an inline link.</summary>
    Link,
    /// <summary>Inline or fenced source code.</summary>
    Code,
    /// <summary>Image alternative description.</summary>
    Image
}

/// <summary>Literal visible text and its semantic presentation role.</summary>
/// <param name="Text">Text to print without interpreting terminal markup.</param>
/// <param name="Role">Presentation role used by the renderer.</param>
public sealed record MdViewTerminalStyledSpan(string Text, MdViewTerminalSpanRole Role);

/// <summary>One formatted terminal row and the source-code clipping flag.</summary>
/// <param name="Spans">Ordered visible spans that occupy exactly one layout row.</param>
/// <param name="IsClippedCode">True when its code source continues beyond the viewport.</param>
public sealed record MdViewTerminalDisplayRow(
    IReadOnlyList<MdViewTerminalStyledSpan> Spans, bool IsClippedCode)
{
    /// <summary>Visible plain text, useful for source-preserving layout tests.</summary>
    public string Text => string.Concat(Spans.Select(span => span.Text));
}

/// <summary>
/// Bounded, line-local Markdown presentation used identically by the disk
/// visual-page index and by the terminal page renderer.
/// </summary>
/// <remarks>
/// Links use their visible labels rather than destinations; list and quote
/// prefixes occupy real display cells and wrapped continuations retain their
/// hanging indentation. This is conservative Markdown, not full CommonMark.
/// The shared container scanner protects active code fences from inline
/// parsing; original input and disk-backed source indexes are not modified.
/// </remarks>
public static class MdViewTerminalLineLayout
{
    /// <summary>
    /// Produces exactly the rows counted by visual-page pagination, without
    /// materializing other source lines or evaluating markup from the input.
    /// </summary>
    /// <param name="line">Already container-classified source line.</param>
    /// <param name="columns">Available terminal content width, 8–4096 cells.</param>
    /// <returns>At least one visible row with safe styles and widths.</returns>
    public static IReadOnlyList<MdViewTerminalDisplayRow> Layout(
        MarkdownContainerLine line, int columns)
    {
        ArgumentNullException.ThrowIfNull(line);
        if (columns is < 8 or > 4096)
            throw new ArgumentOutOfRangeException(nameof(columns));

        if (MdViewTerminalPageIndex.IsLiteralCode(line))
        {
            var original = line.Source.Text.Replace("\t", "    ",
                StringComparison.Ordinal);
            var clipped = MdViewTerminalWordWrap.MeasureColumns(original) > columns;
            var shown = MdViewTerminalWordWrap.Wrap(original, columns, code: true)[0];
            return [new MdViewTerminalDisplayRow(
                [new MdViewTerminalStyledSpan(shown, MdViewTerminalSpanRole.Code)],
                clipped)];
        }

        var source = line.Source.Text;
        var prefix = new StringBuilder();
        var contents = StripStructuralPrefix(line, source, prefix);
        var continuation = prefix.ToString();
        var firstPrefix = continuation;
        var role = line.QuoteDepth > 0
            ? MdViewTerminalSpanRole.Quote : MdViewTerminalSpanRole.Body;

        if (line.Kind == MarkdownContainerLineKind.ListItem &&
            line.ListMarker is { Length: > 0 } marker)
        {
            // Use the scanner's confirmed marker, never guess ownership
            // from a line resembling a nested Markdown list.
            var trimmed = contents.TrimStart(' ', '\t');
            if (trimmed.StartsWith(marker, StringComparison.Ordinal) &&
                (trimmed.Length == marker.Length ||
                 char.IsWhiteSpace(trimmed[marker.Length])))
            {
                contents = trimmed[marker.Length..].TrimStart(' ', '\t');
                var indent = new string(' ',
                    Math.Clamp(line.ListDepth, 0, 8) * 2);
                var bullet = marker is "-" or "*" or "+" ? "•" : marker;
                firstPrefix += indent + bullet + " ";
                continuation += indent +
                    new string(' ', MdViewTerminalWordWrap.MeasureColumns(bullet) + 1);
            }
        }
        else if (line.Kind == MarkdownContainerLineKind.ListContinuation)
        {
            contents = contents.TrimStart(' ', '\t');
            var indent = new string(' ',
                Math.Clamp(line.ListDepth + 1, 1, 8) * 2);
            firstPrefix += indent;
            continuation += indent;
        }

        var heading = line.Kind == MarkdownContainerLineKind.Plain &&
            line.Source.Kind == MarkdownStreamLineKind.Heading &&
            line.Source.HeadingLevel is >= 1 and <= 6;
        if (heading)
        {
            var stripped = contents.TrimStart(' ');
            var level = line.Source.HeadingLevel;
            if (stripped.Length >= level &&
                stripped.AsSpan(0, level).IndexOfAnyExcept('#') < 0)
            {
                contents = stripped[level..].TrimStart(' ', '\t');
                // Conservative ATX closing-marker stripping.
                var suffix = contents.TrimEnd();
                var end = suffix.Length;
                while (end > 0 && suffix[end - 1] == '#')
                    end--;
                if (end < suffix.Length && end > 0 &&
                    char.IsWhiteSpace(suffix[end - 1]))
                    contents = suffix[..end].TrimEnd();
            }
            role = MdViewTerminalSpanRole.Heading;
        }

        // A very narrow terminal must retain space for readable text.
        // Container metadata remains intact in the disk scanner even
        // when visible indentation cannot fit on this particular screen.
        var firstWidth = MdViewTerminalWordWrap.MeasureColumns(firstPrefix);
        var contWidth = MdViewTerminalWordWrap.MeasureColumns(continuation);
        if (Math.Max(firstWidth, contWidth) > columns - 8)
        {
            firstPrefix = "";
            continuation = "";
        }

        var styled = new List<MdViewTerminalStyledSpan>();
        AppendInlines(styled, MarkdownInlineParser.Parse(contents), role);
        var visible = string.Concat(styled.Select(span => span.Text));
        var contentColumns = columns - Math.Max(
            MdViewTerminalWordWrap.MeasureColumns(firstPrefix),
            MdViewTerminalWordWrap.MeasureColumns(continuation));
        var slices = MdViewTerminalWordWrap.SliceProse(visible, contentColumns);
        var rows = new List<MdViewTerminalDisplayRow>(slices.Count);
        for (var index = 0; index < slices.Count; index++)
        {
            var rowPrefix = index == 0 ? firstPrefix : continuation;
            var parts = new List<MdViewTerminalStyledSpan>();
            if (rowPrefix.Length != 0)
                parts.Add(new MdViewTerminalStyledSpan(
                    rowPrefix, MdViewTerminalSpanRole.Marker));

            var slice = slices[index];
            var running = 0;
            foreach (var segment in styled)
            {
                var from = Math.Max(slice.Start, running);
                var to = Math.Min(slice.Start + slice.Length,
                    running + segment.Text.Length);
                if (to > from)
                    parts.Add(new MdViewTerminalStyledSpan(
                        segment.Text.Substring(from - running, to - from),
                        segment.Role));
                running += segment.Text.Length;
                if (running >= slice.Start + slice.Length)
                    break;
            }
            rows.Add(new MdViewTerminalDisplayRow(parts, false));
        }
        return rows;
    }

    private static string StripStructuralPrefix(
        MarkdownContainerLine line, string source, StringBuilder prefix)
    {
        if (line.QuoteDepth <= 0)
            return source;
        var current = 0;
        var matched = 0;
        while (matched < Math.Min(line.QuoteDepth, 32) && current < source.Length)
        {
            var cursor = current;
            var spaces = 0;
            while (cursor < source.Length && source[cursor] == ' ' && spaces++ < 3)
                cursor++;
            if (cursor >= source.Length || source[cursor] != '>')
                break;
            cursor++;
            if (cursor < source.Length && source[cursor] is ' ' or '\t')
                cursor++;
            current = cursor;
            matched++;
        }
        prefix.Append(string.Concat(Enumerable.Repeat("│ ", matched)));
        return source[current..];
    }

    private static void AppendInlines(
        List<MdViewTerminalStyledSpan> output,
        IReadOnlyList<MarkdownInlineNode> nodes,
        MdViewTerminalSpanRole inherited)
    {
        foreach (var node in nodes)
        {
            switch (node.Kind)
            {
                case MarkdownInlineKind.Link:
                    AppendInlines(output, node.Children,
                        MdViewTerminalSpanRole.Link);
                    break;
                case MarkdownInlineKind.Image:
                    Add(output, "[Image: ", MdViewTerminalSpanRole.Image);
                    AppendInlines(output, node.Children,
                        MdViewTerminalSpanRole.Image);
                    Add(output, "]", MdViewTerminalSpanRole.Image);
                    break;
                case MarkdownInlineKind.Code:
                    Add(output, node.Text, MdViewTerminalSpanRole.Code);
                    break;
                case MarkdownInlineKind.Strong:
                    AppendInlines(output, node.Children,
                        inherited is MdViewTerminalSpanRole.Link or
                            MdViewTerminalSpanRole.Image
                            ? inherited : MdViewTerminalSpanRole.Strong);
                    break;
                case MarkdownInlineKind.Emphasis:
                    AppendInlines(output, node.Children,
                        inherited is MdViewTerminalSpanRole.Link or
                            MdViewTerminalSpanRole.Image
                            ? inherited : MdViewTerminalSpanRole.Emphasis);
                    break;
                default:
                    Add(output, node.Text, inherited);
                    break;
            }
        }
    }

    private static void Add(
        List<MdViewTerminalStyledSpan> output,
        string text, MdViewTerminalSpanRole role)
    {
        if (text.Length == 0)
            return;
        var expanded = text.Replace("\t", "    ", StringComparison.Ordinal);
        if (output.Count > 0 && output[^1].Role == role)
            output[^1] = output[^1] with { Text = output[^1].Text + expanded };
        else
            output.Add(new MdViewTerminalStyledSpan(expanded, role));
    }
}
