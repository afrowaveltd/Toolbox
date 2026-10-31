namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Immutable source snapshot with a read-only collection of parsed lines.
/// This first increment does not expose a mutable syntax tree.
/// </summary>
public sealed class MarkdownDocument
{
    internal MarkdownDocument(string source, IReadOnlyList<MarkdownBlock> blocks,
        IReadOnlyList<MarkdownListItemNode> listRoots)
    {
        Source = source;
        Blocks = blocks;
        ListRoots = listRoots;
    }

    /// <summary>Exact original source, including encoding-independent character content and line endings.</summary>
    public string Source { get; }

    /// <summary>Basic line/block tokens with source offsets in original order.</summary>
    public IReadOnlyList<MarkdownBlock> Blocks { get; }

    /// <summary>Top-level list-item nodes, ordered by their physical appearance.</summary>
    public IReadOnlyList<MarkdownListItemNode> ListRoots { get; }
}

