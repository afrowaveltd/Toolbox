namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Immutable hierarchy of source-aware list item marker lines and their explicitly
/// indented continuation paragraphs. Blank-separated loose lists and compound CommonMark
/// block structures are not yet represented.
/// </summary>
public sealed class MarkdownListItemNode
{
    internal MarkdownListItemNode(
        MarkdownBlock block, IReadOnlyList<MarkdownListItemNode> children,
        IReadOnlyList<MarkdownBlock> continuationBlocks)
    {
        Block = block;
        Children = children;
        ContinuationBlocks = continuationBlocks;
    }

    /// <summary>Original line, source span, inline nodes and its list marker metadata.</summary>
    public MarkdownBlock Block { get; }

    /// <summary>Nested list items belonging to this preceding item.</summary>
    public IReadOnlyList<MarkdownListItemNode> Children { get; }

    /// <summary>Explicitly indented continuation paragraph lines owned by this item, in source order.</summary>
    public IReadOnlyList<MarkdownBlock> ContinuationBlocks { get; }
}
