namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>Supported inline syntax for the initial conservative Markdown scanner.</summary>
public enum MarkdownInlineKind
{
    /// <summary>Literal text, including unrecognized or malformed syntax.</summary>
    Text,
    /// <summary>Content surrounded by a single * or _ marker.</summary>
    Emphasis,
    /// <summary>Content surrounded by a double ** or __ marker.</summary>
    Strong,
    /// <summary>Literal inline code delimited by matching backtick runs.</summary>
    Code,
    /// <summary>A simple inline Markdown link: [label](destination).</summary>
    Link
}
