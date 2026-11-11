using System.Text;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>One source-bounded Markdown display fragment, independent of terminal rows.</summary>
/// <param name="Text">Validated source fragment or literal fenced-code line.</param>
/// <param name="IsLiteralCode">True for a block classified as fenced source code.</param>
public sealed record MdViewMarkdownPageSegment(string Text, bool IsLiteralCode);

/// <summary>One bounded source-line page of Markdown display segments.</summary>
/// <param name="FirstLine">First zero-based physical source line.</param>
/// <param name="NextLine">Exclusive next physical line.</param>
/// <param name="LineCount">Total physical source lines in the bounded document.</param>
/// <param name="Segments">Ordered Markdown/literal-code segments.</param>
/// <param name="HasMore">Whether further lines remain.</param>
/// <param name="TruncatedLine">Whether one oversized line was shortened for display.</param>
public sealed record MdViewMarkdownSourcePage(
    int FirstLine, int NextLine, int LineCount,
    IReadOnlyList<MdViewMarkdownPageSegment> Segments,
    bool HasMore, bool TruncatedLine);

/// <summary>
/// Bounded source-line pagination for the existing at-most-1-MiB
/// whole-document Markdown browser. Uses full-parser block classification
/// to keep code fences literal across page boundaries.
/// </summary>
/// <remarks>
/// This is intentionally not the disk-backed multi-gigabyte pager or a
/// visual-terminal-row layout engine. A long source line is displayed only
/// up to the per-page character budget. Ordinary list/quote fragments may
/// lose context at a source page boundary; fenced code is never reparsed
/// as Markdown prose.
/// </remarks>
public sealed class MdViewSmallMarkdownPageIndex
{
    /// <summary>Default source lines shown in each interactive page.</summary>
    public const int DefaultPageLines = 24;

    /// <summary>Maximum displayed UTF-16 characters per source page.</summary>
    public const int MaximumPageCharacters = 16 * 1024;

    private readonly IReadOnlyList<MarkdownBlock> _blocks;

    /// <summary>Parses a bounded verified Markdown document once.</summary>
    /// <param name="markdown">Complete decoded Markdown input.</param>
    public MdViewSmallMarkdownPageIndex(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        if (markdown.Length > MdViewDocumentLinkExtractor.MaximumDocumentCharacters)
            throw new ArgumentOutOfRangeException(nameof(markdown),
                "This source-page index requires a bounded Markdown document.");
        _blocks = MarkdownParser.Parse(markdown).Blocks;
    }

    /// <summary>Total physical source lines recognized by the shared parser.</summary>
    public int LineCount => _blocks.Count;

    /// <summary>Locates a heading or link source offset by physical source line.</summary>
    /// <param name="sourceOffset">Zero-based UTF-16 offset in the original document.</param>
    /// <returns>Zero-based line containing the source offset.</returns>
    public int FindSourceLine(int sourceOffset)
    {
        if (sourceOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(sourceOffset));
        var lo = 0;
        var hi = _blocks.Count;
        while (lo < hi)
        {
            var middle = lo + (hi - lo) / 2;
            if (_blocks[middle].Start <= sourceOffset)
                lo = middle + 1;
            else
                hi = middle;
        }
        return Math.Clamp(lo - 1, 0, Math.Max(0, _blocks.Count - 1));
    }

    /// <summary>Builds one source-line page without rendering other lines.</summary>
    /// <param name="firstLine">Zero-based first physical line.</param>
    /// <param name="linesPerPage">Source line budget (1–128).</param>
    /// <returns>Segments safe to hand to the existing Markdown renderer.</returns>
    public MdViewMarkdownSourcePage GetPage(
        int firstLine, int linesPerPage = DefaultPageLines)
    {
        if (linesPerPage is < 1 or > 128)
            throw new ArgumentOutOfRangeException(nameof(linesPerPage));
        if (firstLine < 0 || firstLine > _blocks.Count)
            throw new ArgumentOutOfRangeException(nameof(firstLine));

        var segments = new List<MdViewMarkdownPageSegment>();
        var prose = new StringBuilder();
        var remaining = MaximumPageCharacters;
        var end = firstLine;
        var truncated = false;

        void FlushProse()
        {
            if (prose.Length == 0)
                return;
            segments.Add(new MdViewMarkdownPageSegment(prose.ToString(), false));
            prose.Clear();
        }

        while (end < _blocks.Count && end - firstLine < linesPerPage)
        {
            var block = _blocks[end];
            if (remaining == 0)
                break;

            var take = Math.Min(block.RawText.Length, remaining);
            // A UTF-16 character budget must never split a surrogate pair.
            // This source is already validated Unicode, but terminal Text
            // should not be handed an orphan high surrogate.
            if (take > 0 && take < block.RawText.Length &&
                char.IsHighSurrogate(block.RawText[take - 1]))
                take--;
            // If the remaining budget cannot hold the first Unicode scalar,
            // leave this physical line intact for the next page. Never
            // consume a line without displaying any part of it.
            if (take == 0)
                break;
            var text = block.RawText[..take];
            var lineWasTruncated = take < block.RawText.Length;
            if (lineWasTruncated)
                truncated = true;
            if (block.Kind is MarkdownBlockKind.CodeFence or MarkdownBlockKind.CodeLine)
            {
                FlushProse();
                segments.Add(new MdViewMarkdownPageSegment(text, true));
            }
            else
                prose.Append(text);

            remaining -= take;
            end++;
            // The rest of a truncated physical line is intentionally omitted
            // from this display page, not queued as a new source line. Do not
            // spend a leftover UTF-16 code unit on the following line:
            // that would display a misleading fragment of its Markdown.
            if (lineWasTruncated)
                break;
        }
        FlushProse();

        return new MdViewMarkdownSourcePage(
            firstLine, end, _blocks.Count, segments,
            end < _blocks.Count, truncated);
    }

    /// <summary>Renders only the supplied page; fenced source stays literal.</summary>
    /// <param name="console">Destination terminal.</param>
    /// <param name="page">Bounded display page from this index.</param>
    /// <param name="theme">Markdown theme for ordinary prose.</param>
    public static void RenderPage(
        IAnsiConsole console, MdViewMarkdownSourcePage page, MarkdownSpectreTheme theme)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(theme);
        foreach (var segment in page.Segments)
        {
            if (segment.IsLiteralCode)
                console.Write(new Text(segment.Text,
                    new Style(foreground: Color.Yellow)));
            else
                MarkdownSpectreRenderer.Render(console, segment.Text, theme);
        }
    }
}
