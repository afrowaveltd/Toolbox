namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Metadata for a source-preserved list marker and its content indentation.
/// Explicitly indented paragraph continuations can be owned by this marker;
/// blank-separated loose/complete CommonMark list structures remain unsupported.
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
