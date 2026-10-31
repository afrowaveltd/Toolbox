namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// A source-preserving inline token. <see cref="Start"/> is an absolute offset
/// into the document when provided by <see cref="MarkdownParser"/>.
/// </summary>
/// <param name="Kind">Recognized syntax category.</param>
/// <param name="Start">Zero-based offset into the source text (including optional base offset).</param>
/// <param name="Length">Complete original token length, including Markdown markers.</param>
/// <param name="RawText">Exact original token including Markdown punctuation.</param>
/// <param name="Text">Inner content before further inline interpretation; for links, visible label.</param>
/// <param name="Destination">Unmodified link target, when applicable; never automatically localized or followed.</param>
public sealed record MarkdownInlineNode(
    MarkdownInlineKind Kind,
    int Start,
    int Length,
    string RawText,
    string Text,
    string? Destination = null)
{
    /// <summary>Nested label or emphasis elements; empty for literal text and inline code.</summary>
    public IReadOnlyList<MarkdownInlineNode> Children { get; init; } = [];

    /// <summary>Optional original title without its quote delimiters.</summary>
    public string? Title { get; init; }

    /// <summary>Absolute start offset of the original destination text.</summary>
    public int? DestinationStart { get; init; }

    /// <summary>Character length of the original destination, excluding angle brackets.</summary>
    public int DestinationLength { get; init; }
}
