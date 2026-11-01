namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// A single source-preserving Markdown line. <see cref="RawText"/> includes the original
/// line ending; <see cref="Text"/> is its content without markers for recognized headings/lists.
/// </summary>
/// <param name="Kind">Basic syntax category.</param>
/// <param name="Start">Zero-based character offset in the original Markdown string.</param>
/// <param name="Length">Length including any line ending.</param>
/// <param name="Text">Content for inspection, excluding heading/list markers.</param>
/// <param name="RawText">Exact original line including its line ending, if present.</param>
/// <param name="HeadingLevel">Heading level 1-6; zero for all other kinds.</param>
public sealed record MarkdownBlock(
    MarkdownBlockKind Kind,
    int Start,
    int Length,
    string Text,
    string RawText,
    int HeadingLevel = 0)
{
    /// <summary>Source-offset-aware inline tokens. Empty for blank lines and fenced code.</summary>
    public IReadOnlyList<MarkdownInlineNode> Inlines { get; init; } = [];

    /// <summary>Source-aware list marker, including nesting depth; null for non-list lines.</summary>
    public MarkdownListMarker? ListMarker { get; init; }

    /// <summary>Source marker of the owning item for attached paragraphs, quotes or code.</summary>
    public MarkdownListMarker? ListContinuationMarker { get; init; }

    /// <summary>
    /// True only for a paragraph continuation accepted without sufficient indentation.
    /// False for explicitly indented lines and all non-continuations.
    /// </summary>
    public bool IsLazyListContinuation { get; init; }

    /// <summary>Quote marker depth, zero for non-quote lines.</summary>
    public int QuoteDepth { get; init; }

    /// <summary>Absolute source offset of the opening code fence, for opener/body/closer.</summary>
    public int? CodeFenceStart { get; init; }

    /// <summary>Unmodified opening fence information string (if recognized).</summary>
    public string? CodeFenceInfo { get; init; }

    /// <summary>First token of the fence information string, not an inferred language.</summary>
    public string? CodeLanguage { get; init; }

    /// <summary>True only on the matched closing delimiter of a fenced code block.</summary>
    public bool IsClosingCodeFence { get; init; }

    /// <summary>
    /// Absolute source offset of the owning list item for paragraph, quote or fenced-code
    /// content attached to a list item; null for top-level blocks.
    /// </summary>
    public int? ListContinuationOwnerStart { get; init; }

    /// <summary>
    /// Owner's list-marker source offset when a physical blank or quote-prefixed
    /// blank separates explicitly indented content of a loose list item.
    /// Null for unrelated blanks. Quote-prefixed separators retain their
    /// BlockQuote kind and QuoteDepth so their visible markers are preserved.
    /// </summary>
    public int? ListBlankOwnerStart { get; init; }
}
