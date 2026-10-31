namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Conservative inline scanner for Markdown emphasis, code spans, inline links and images.
/// Unsupported or malformed constructs remain literal; no HTML execution or link access occurs.
/// Not yet a CommonMark-compliant delimiter algorithm.
/// </summary>
public static class MarkdownInlineParser
{
    private const int MaxDepth = 16;

    /// <summary>
    /// Parse an inline text fragment with optional absolute source offset.
    /// Recognizes simple inline links/images (optionally titled), strong/emphasis and code spans.
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

        if (source[index] == '!' && index + 1 < end && source[index + 1] == '[' &&
            TryLinkOrImage(source, index, end, baseOffset, depth, true, out node, out next))
            return true;

        if (allowLinks && source[index] == '[' &&
            TryLinkOrImage(source, index, end, baseOffset, depth, false, out node, out next))
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

    private static bool TryLinkOrImage(
        string source, int start, int end, int baseOffset, int depth, bool isImage,
        out MarkdownInlineNode node, out int next)
    {
        node = null!;
        next = start;
        int labelStart = start + (isImage ? 2 : 1);
        int labelEnd = FindLabelEnd(source, labelStart, end);
        if (labelEnd < 0 || labelEnd + 1 >= end || source[labelEnd + 1] != '(')
            return false;

        if (!TryDestination(source, labelEnd + 2, end,
                out int destStart, out int destEnd, out string? title, out next))
            return false;

        node = new MarkdownInlineNode(
            isImage ? MarkdownInlineKind.Image : MarkdownInlineKind.Link,
            baseOffset + start, next - start, source[start..next],
            source[labelStart..labelEnd], source[destStart..destEnd])
        {
            Title = title,
            DestinationStart = baseOffset + destStart,
            DestinationLength = destEnd - destStart,
            Children = ParseRange(source, labelStart, labelEnd, baseOffset, depth + 1, allowLinks: false)
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

    private static bool TryDestination(
        string source, int start, int end,
        out int destStart, out int destEnd, out string? title, out int next)
    {
        destStart = destEnd = next = start;
        title = null;
        if (start >= end)
            return false;
        if (source[start] == '<')
        {
            destStart = start + 1;
            int i = destStart;
            while (i < end)
            {
                if (source[i] is '\r' or '\n' or '<')
                    return false;
                if (source[i] == '>' && !IsEscaped(source, i, destStart))
                    break;
                i++;
            }
            if (i == end)
                return false;
            destEnd = i;
            return TryFinish(source, i + 1, end, out title, out next);
        }

        destStart = start;
        int nesting = 0;
        for (int i = start; i < end; i++)
        {
            char c = source[i];
            if (IsEscaped(source, i, start))
                continue;
            if (c is '<' or '>' or '\r' or '\n')
                return false;
            if (char.IsWhiteSpace(c))
            {
                if (nesting != 0)
                    return false;
                destEnd = i;
                return TryFinish(source, i, end, out title, out next);
            }
            if (c == '(' && ++nesting > 32)
                return false;
            if (c == ')')
            {
                if (nesting == 0)
                {
                    destEnd = i;
                    next = i + 1;
                    return true;
                }
                nesting--;
            }
        }
        return false;
    }

    private static bool TryFinish(string source, int pos, int end, out string? title, out int next)
    {
        title = null;
        next = pos;
        if (pos < end && source[pos] == ')')
        {
            next = pos + 1;
            return true;
        }
        int i = pos;
        while (i < end && source[i] is ' ' or '\t')
            i++;
        if (i == pos || i == end)
            return false;
        if (source[i] == ')')
        {
            next = i + 1;
            return true;
        }

        char open = source[i];
        if (open is not ('"' or '\'' or '('))
            return false;
        char close = open == '(' ? ')' : open;
        int titleStart = ++i;
        while (i < end)
        {
            if (source[i] is '\r' or '\n')
                return false;
            if (source[i] == close && !IsEscaped(source, i, titleStart))
                break;
            i++;
        }
        if (i == end)
            return false;
        title = source[titleStart..i];
        i++;
        while (i < end && source[i] is ' ' or '\t')
            i++;
        if (i == end || source[i] != ')')
            return false;
        next = i + 1;
        return true;
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
