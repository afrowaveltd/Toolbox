namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Metadata for a single source-preserved list marker. This is a conservative line-level
/// model; multiline/loose CommonMark list items are not yet represented.
/// </summary>
public sealed record MarkdownListMarker(
    string OriginalMarker,
    bool IsOrdered,
    int? Number,
    int IndentColumns,
    int Depth,
    int ContentStart)
{
    /// <summary>Original source column where the marker line's content begins, using four-column tab stops.</summary>
    public int ContentIndentColumns { get; init; }

    /// <summary>Original ordered delimiter ('.' or ')'), or null for unordered markers.</summary>
    public char? Delimiter => IsOrdered ? OriginalMarker[^1] : null;
}
