namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// A single source-preserving Markdown line. <see cref="RawText"/> includes the original
/// line ending; <see cref="Text"/> is its content without markers for recognized headings/lists.
/// </summary>
/// <param name="Kind">Basic syntax category.</param>
/// <param name="Start">Zero-based character offset in the original Markdown string.</param>
/// <param name="Length">Length including any line ending.</param>
/// <param name="Text">Content for inspection; no inline Markdown processing is performed.</param>
/// <param name="RawText">Exact original line including its line ending, if present.</param>
/// <param name="HeadingLevel">Heading level 1-6; zero for all other kinds.</param>
public sealed record MarkdownBlock(
    MarkdownBlockKind Kind,
    int Start,
    int Length,
    string Text,
    string RawText,
    int HeadingLevel = 0);
