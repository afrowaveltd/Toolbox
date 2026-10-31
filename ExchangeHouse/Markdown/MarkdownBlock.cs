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

    /// <summary>Source marker of the owning item for explicitly indented continuation paragraphs.</summary>
    public MarkdownListMarker? ListContinuationMarker { get; init; }

    /// <summary>Absolute source offset of the continuation's owning list item line.</summary>
    public int? ListContinuationOwnerStart { get; init; }
}
