namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Immutable hierarchy of source-aware list item marker lines and their explicitly
/// indented or paragraph-only lazy continuation lines and their supported blank separators. Full CommonMark
/// loose lists, list containers and embedded compound blocks remain out of scope.
/// </summary>
public sealed class MarkdownListItemNode
{
    internal MarkdownListItemNode(
        MarkdownBlock block, IReadOnlyList<MarkdownListItemNode> children,
        IReadOnlyList<MarkdownBlock> continuationBlocks,
        IReadOnlyList<MarkdownBlock> separatorBlocks,
        IReadOnlyList<MarkdownBlock> contentBlocks,
        IReadOnlyList<MarkdownBlock> embeddedBlocks)
    {
        Block = block;
        Children = children;
        ContinuationBlocks = continuationBlocks;
        SeparatorBlocks = separatorBlocks;
        ContentBlocks = contentBlocks;
        EmbeddedBlocks = embeddedBlocks;
    }

    /// <summary>Original line, source span, inline nodes and its list marker metadata.</summary>
    public MarkdownBlock Block { get; }

    /// <summary>Nested list items belonging to this preceding item.</summary>
    public IReadOnlyList<MarkdownListItemNode> Children { get; }

    /// <summary>Indented and lazy continuation paragraph lines owned by this item, in source order.</summary>
    public IReadOnlyList<MarkdownBlock> ContinuationBlocks { get; }

    /// <summary>Blank separator lines that belong to this item and precede a valid continuation.</summary>
    public IReadOnlyList<MarkdownBlock> SeparatorBlocks { get; }

    /// <summary>Owned continuation and separator lines together, in original source order.</summary>
    public IReadOnlyList<MarkdownBlock> ContentBlocks { get; }

    /// <summary>Attached quote and fenced-code lines in original source order.</summary>
    public IReadOnlyList<MarkdownBlock> EmbeddedBlocks { get; }

    /// <summary>Whether the item has at least one owned blank separator between content blocks.</summary>
    public bool IsLoose => SeparatorBlocks.Count > 0;
}
