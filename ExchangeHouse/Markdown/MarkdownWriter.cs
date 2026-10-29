using System.Text;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>Source-preserving Markdown writer. Structured editing will be added in a later increment.</summary>
public static class MarkdownWriter
{
    /// <summary>
    /// Reconstruct unchanged Markdown byte-for-character from its parsed line spans.
    /// The parser preserves CRLF, LF, CR, trailing whitespace and missing final newline.
    /// </summary>
    public static string Write(MarkdownDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var builder = new StringBuilder(document.Source.Length);
        foreach (var block in document.Blocks)
            builder.Append(block.RawText);
        return builder.ToString();
    }
}
