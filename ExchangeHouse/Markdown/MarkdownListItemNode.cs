namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Immutable hierarchy of physically adjacent list-item lines. A document may contain
/// several root nodes from separate list runs; it is not yet a full CommonMark list AST.
/// </summary>
public sealed class MarkdownListItemNode
{
    internal MarkdownListItemNode(
        MarkdownBlock block, IReadOnlyList<MarkdownListItemNode> children)
    {
        Block = block;
        Children = children;
    }

    /// <summary>Original line, source span, inline nodes and its list marker metadata.</summary>
    public MarkdownBlock Block { get; }

    /// <summary>Nested list items belonging to this preceding item.</summary>
    public IReadOnlyList<MarkdownListItemNode> Children { get; }
}
