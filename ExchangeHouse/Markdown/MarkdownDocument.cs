namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Immutable source snapshot with a read-only collection of parsed lines.
/// This first increment does not expose a mutable syntax tree.
/// </summary>
public sealed class MarkdownDocument
{
    internal MarkdownDocument(string source, IReadOnlyList<MarkdownBlock> blocks)
    {
        Source = source;
        Blocks = blocks;
    }

    /// <summary>Exact original source, including encoding-independent character content and line endings.</summary>
    public string Source { get; }

    /// <summary>Basic line/block tokens with source offsets in original order.</summary>
    public IReadOnlyList<MarkdownBlock> Blocks { get; }
}
