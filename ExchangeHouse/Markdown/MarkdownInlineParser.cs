namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Conservative inline scanner for Markdown emphasis, code spans and inline links.
/// Unsupported or malformed constructs remain literal; no HTML execution or link access occurs.
/// Not yet a CommonMark-compliant delimiter algorithm.
/// </summary>
public static class MarkdownInlineParser
{
    private const int MaxDepth = 16;

    /// <summary>
    /// Parse an inline text fragment with optional absolute source offset.
    /// Recognizes only simple inline links, strong/emphasis and single-line code spans.
    /// </summary>
    public static IReadOnlyList<MarkdownInlineNode> Parse(string text, int sourceOffset = 0)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentOutOfRangeException.ThrowIfNegative(sourceOffset);
        return ParseRange(text, 0, text.Length, sourceOffset, 0, allowLinks: true);
    }

    private static IReadOnlyList<MarkdownInlineNode> ParseRange(
        string text, int start, int end, int baseOffset, int depth, bool allowLinks)
    {
        var nodes = new List<MarkdownInlineNode>();
        var plainStart = start;
        var index = start;

        while (index < end)
        {
            if (depth < MaxDepth &&
                TryReadNode(text, index, start, end, baseOffset, depth, allowLinks,
                    out var node, out var next))
            {
                AddText(nodes, text, plainStart, index, baseOffset);
                nodes.Add(node);
                index = next;
                plainStart = index;
            }
            else
            {
                index++;
            }
        }

        AddText(nodes, text, plainStart, end, baseOffset);
        return nodes.AsReadOnly();
    }

    private static void AddText(
        List<MarkdownInlineNode> nodes, string source, int start, int end, int baseOffset)
    {
        if (end == start)
            return;

        var original = source[start..end];
        nodes.Add(new MarkdownInlineNode(
            MarkdownInlineKind.Text, baseOffset + start, end - start, original, original));
    }

    private static bool TryReadNode(
        string source, int index, int rangeStart, int end, int baseOffset, int depth, bool allowLinks,
        out MarkdownInlineNode node, out int next)
    {
        node = null!;
        next = index;
        if (IsEscaped(source, index, rangeStart))
            return false;

        if (source[index] == '`' && TryCode(source, index, end, baseOffset, out node, out next))
            return true;

        if (allowLinks && source[index] == '[' &&
            TryLink(source, index, end, baseOffset, depth, out node, out next))
            return true;

        if (source[index] is '*' or '_' &&
            TryEmphasis(source, index, end, baseOffset, depth, out node, out next))
            return true;

        return false;
    }

    private static bool TryCode(
        string source, int start, int end, int baseOffset, out MarkdownInlineNode node, out int next)
    {
        node = null!;
        next = start;
        if (!TryCodeBounds(source, start, end, out var innerStart, out var innerEnd, out next))
            return false;

        node = new MarkdownInlineNode(
            MarkdownInlineKind.Code, baseOffset + start, next - start,
            source[start..next], source[innerStart..innerEnd]);
        return true;
    }

    private static bool TryCodeBounds(
        string source, int start, int end, out int innerStart, out int innerEnd, out int next)
    {
        innerStart = innerEnd = next = start;
        if (source[start] != '`')
            return false;

        var count = 0;
        while (start + count < end && source[start + count] == '`')
            count++;

        innerStart = start + count;
        for (var i = innerStart; i < end;)
        {
            if (source[i] != '`')
            {
                i++;
                continue;
            }
            var run = 0;
            while (i + run < end && source[i + run] == '`')
                run++;
            if (run == count)
            {
                innerEnd = i;
                next = i + count;
                return true;
            }
            i += run;
        }
        return false;
    }

    private static bool TryLink(
        string source, int start, int end, int baseOffset, int depth,
        out MarkdownInlineNode node, out int next)
    {
        node = null!;
        next = start;
        var labelEnd = FindLabelEnd(source, start + 1, end);
        if (labelEnd < 0 || labelEnd + 1 >= end || source[labelEnd + 1] != '(')
            return false;

        var destinationStart = labelEnd + 2;
        var destinationEnd = FindDestinationEnd(source, destinationStart, end);
        if (destinationEnd < 0)
            return false;

        next = destinationEnd + 1;
        var visible = source[(start + 1)..labelEnd];
        node = new MarkdownInlineNode(
            MarkdownInlineKind.Link, baseOffset + start, next - start,
            source[start..next], visible, source[destinationStart..destinationEnd])
        {
            Children = ParseRange(source, start + 1, labelEnd, baseOffset, depth + 1, allowLinks: false)
        };
        return true;
    }

    private static int FindLabelEnd(string source, int start, int end)
    {
        var nesting = 1;
        for (var i = start; i < end;)
        {
            if (IsEscaped(source, i, start))
            {
                i++;
                continue;
            }
            if (source[i] == '`' &&
                TryCodeBounds(source, i, end, out _, out _, out var codeEnd))
            {
                i = codeEnd;
                continue;
            }
            if (source[i] == '[')
                nesting++;
            else if (source[i] == ']' && --nesting == 0)
                return i;
            i++;
        }
        return -1;
    }

    private static int FindDestinationEnd(string source, int start, int end)
    {
        var nesting = 1;
        for (var i = start; i < end; i++)
        {
            if (IsEscaped(source, i, start))
                continue;
            if (char.IsWhiteSpace(source[i]) || source[i] == '<' || source[i] == '>')
                return -1; // titles / angle destinations are a later increment
            if (source[i] == '(')
                nesting++;
            else if (source[i] == ')' && --nesting == 0)
                return i;
        }
        return -1;
    }

    private static bool TryEmphasis(
        string source, int start, int end, int baseOffset, int depth,
        out MarkdownInlineNode node, out int next)
    {
        node = null!;
        next = start;
        var marker = source[start];
        var length = start + 1 < end && source[start + 1] == marker ? 2 : 1;
        if (start + length < end && source[start + length] == marker)
            return false; // unsupported triple marker run: preserve it literally
        if (marker == '_' && start > 0 && char.IsLetterOrDigit(source[start - 1]))
            return false;

        var innerStart = start + length;
        if (innerStart >= end || char.IsWhiteSpace(source[innerStart]))
            return false;

        var closing = FindClosingEmphasis(source, innerStart, end, marker, length);
        if (closing <= innerStart)
            return false;

        next = closing + length;
        node = new MarkdownInlineNode(
            length == 2 ? MarkdownInlineKind.Strong : MarkdownInlineKind.Emphasis,
            baseOffset + start, next - start, source[start..next], source[innerStart..closing])
        {
            Children = ParseRange(source, innerStart, closing, baseOffset, depth + 1, allowLinks: true)
        };
        return true;
    }

    private static int FindClosingEmphasis(string source, int start, int end, char marker, int length)
    {
        for (var i = start; i + length <= end; i++)
        {
            if (IsEscaped(source, i, start))
                continue;

            if (source[i] == '`' &&
                TryCodeBounds(source, i, end, out _, out _, out var codeEnd))
            {
                i = codeEnd - 1;
                continue;
            }

            if (source[i] != marker || (length == 2 && source[i + 1] != marker))
                continue;

            if (i + length < end && source[i + length] == marker)
                continue;
            if (char.IsWhiteSpace(source[i - 1]))
                continue;
            if (marker == '_' && i + length < end && char.IsLetterOrDigit(source[i + length]))
                continue;

            return i;
        }
        return -1;
    }

    private static bool IsEscaped(string source, int index, int start)
    {
        var backslashes = 0;
        for (var i = index - 1; i >= start && source[i] == '\\'; i--)
            backslashes++;
        return (backslashes & 1) != 0;
    }
}
