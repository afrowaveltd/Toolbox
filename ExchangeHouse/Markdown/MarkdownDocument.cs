namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Immutable source snapshot with a read-only collection of parsed lines.
/// This first increment does not expose a mutable syntax tree.
/// </summary>
public sealed class MarkdownDocument
{
    internal MarkdownDocument(string source, IReadOnlyList<MarkdownBlock> blocks,
        IReadOnlyList<MarkdownListItemNode> listRoots,
        IReadOnlyList<MarkdownListItemNode> quoteListRoots)
    {
        Source = source;
        Blocks = blocks;
        ListRoots = listRoots;
        QuoteListRoots = quoteListRoots;
    }

    /// <summary>Exact original source, including encoding-independent character content and line endings.</summary>
    public string Source { get; }

    /// <summary>Basic line/block tokens with source offsets in original order.</summary>
    public IReadOnlyList<MarkdownBlock> Blocks { get; }

    /// <summary>Top-level list-item nodes, ordered by their physical appearance.</summary>
    public IReadOnlyList<MarkdownListItemNode> ListRoots { get; }

    /// <summary>
    /// List roots inside explicitly quote-prefixed lines, in source order. Their
    /// marker depth is relative to the owning quote level (see Block.QuoteDepth).
    /// This bounded grammar does not merge independent quote containers.
    /// </summary>
    public IReadOnlyList<MarkdownListItemNode> QuoteListRoots { get; }
}

