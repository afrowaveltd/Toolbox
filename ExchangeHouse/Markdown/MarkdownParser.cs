namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Dependency-free, source-preserving first-pass Markdown block scanner.
/// It deliberately does not claim full CommonMark support or perform inline tokenization.
/// </summary>
public static class MarkdownParser
{
    /// <summary>Read Markdown without changing content, line endings or protected code text.</summary>
    public static MarkdownDocument Parse(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var blocks = new List<MarkdownBlock>();
        var offset = 0;
        var inCode = false;
        var fenceMarker = '\0';
        var fenceLength = 0;

        while (offset < source.Length)
        {
            var contentEnd = offset;
            while (contentEnd < source.Length && source[contentEnd] is not ('\r' or '\n'))
                contentEnd++;

            var next = contentEnd;
            if (next < source.Length && source[next] == '\r')
                next++;
            if (next < source.Length && source[next] == '\n')
                next++;

            var line = source[offset..contentEnd];
            var raw = source[offset..next];
            var level = 0;
            MarkdownBlockKind kind;
            string content = line;

            if (inCode)
            {
                if (IsClosingFence(line, fenceMarker, fenceLength))
                {
                    kind = MarkdownBlockKind.CodeFence;
                    inCode = false;
                }
                else
                {
                    kind = MarkdownBlockKind.CodeLine;
                }
            }
            else if (TryFence(line, out fenceMarker, out fenceLength))
            {
                kind = MarkdownBlockKind.CodeFence;
                inCode = true;
            }
            else if (string.IsNullOrWhiteSpace(line))
            {
                kind = MarkdownBlockKind.Blank;
            }
            else if (TryHeading(line, out level, out content))
            {
                kind = MarkdownBlockKind.Heading;
            }
            else if (TryListItem(line, out content))
            {
                kind = MarkdownBlockKind.ListItem;
            }
            else
            {
                kind = MarkdownBlockKind.Paragraph;
            }

            blocks.Add(new MarkdownBlock(kind, offset, next - offset, content, raw, level));
            offset = next;
        }

        return new MarkdownDocument(source, blocks.AsReadOnly());
    }

    private static int Indent(string line)
    {
        var index = 0;
        while (index < line.Length && line[index] == ' ' && index < 4)
            index++;
        return index;
    }

    private static bool TryFence(string line, out char marker, out int count)
    {
        marker = '\0';
        count = 0;
        var indent = Indent(line);
        if (indent > 3 || indent >= line.Length ||
            (line[indent] != '`' && line[indent] != '~'))
            return false;

        marker = line[indent];
        var index = indent;
        while (index < line.Length && line[index] == marker)
            index++;

        count = index - indent;
        return count >= 3;
    }

    private static bool IsClosingFence(string line, char marker, int requiredLength)
    {
        if (!TryFence(line, out var found, out var count) || found != marker || count < requiredLength)
            return false;

        var indent = Indent(line);
        return line[(indent + count)..].All(c => c is ' ' or '\t');
    }

    private static bool TryHeading(string line, out int level, out string content)
    {
        level = 0;
        content = line;
        var indent = Indent(line);
        if (indent > 3 || indent >= line.Length || line[indent] != '#')
            return false;

        var index = indent;
        while (index < line.Length && line[index] == '#')
            index++;

        level = index - indent;
        if (level is < 1 or > 6 || (index < line.Length && line[index] is not (' ' or '\t')))
        {
            level = 0;
            return false;
        }

        content = line[index..].TrimStart(' ', '\t');
        return true;
    }

    private static bool TryListItem(string line, out string content)
    {
        content = line;
        var index = Indent(line);
        if (index > 3 || index >= line.Length)
            return false;

        var marker = line[index];
        if (marker is '-' or '*' or '+')
            index++;
        else if (char.IsAsciiDigit(marker))
        {
            var start = index;
            while (index < line.Length && char.IsAsciiDigit(line[index]) && index - start < 10)
                index++;
            if (index - start > 9 || index >= line.Length || line[index] is not ('.' or ')'))
                return false;
            index++;
        }
        else
            return false;

        if (index < line.Length && line[index] is not (' ' or '\t'))
            return false;

        content = line[index..].TrimStart(' ', '\t');
        return true;
    }
}
