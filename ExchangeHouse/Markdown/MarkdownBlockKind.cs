namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>First-pass classification of a physical Markdown line; not yet a complete CommonMark AST.</summary>
public enum MarkdownBlockKind
{
    /// <summary>An empty or whitespace-only line.</summary>
    Blank,
    /// <summary>An ATX heading (levels 1-6).</summary>
    Heading,
    /// <summary>An ordinary line of text that is not a recognized block marker.</summary>
    Paragraph,
    /// <summary>A simple ordered or unordered list item.</summary>
    ListItem,
    /// <summary>Opening or closing fenced code delimiter.</summary>
    CodeFence,
    /// <summary>A line inside a fenced code block.</summary>
    CodeLine
}
