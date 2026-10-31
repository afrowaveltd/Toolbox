using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;

/// <summary>Optional syntax backend; a fresh session is created for each declared-language code fence.</summary>
public interface IMarkdownSyntaxHighlighter
{
    /// <summary>Return null for unsupported language hints; never execute the supplied code.</summary>
    IMarkdownHighlightSession? CreateSession(string language);
}

/// <summary>Stateful code-line tokenizer. Implementation may retain multiline grammar state.</summary>
public interface IMarkdownHighlightSession : IDisposable
{
    /// <summary>Ordered non-overlapping spans relative to the given original line.</summary>
    IReadOnlyList<MarkdownHighlightSpan> HighlightLine(string codeLine);
}

/// <summary>Styled half-open source interval within a single code line.</summary>
public sealed record MarkdownHighlightSpan(int Start, int Length, Style Style);
