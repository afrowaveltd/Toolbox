# Code and quote rendering

The independent Blackboard Markdown renderer consumes ExchangeHouse's source owner metadata, quote depth and fenced-code information strings.

- Indented quote/code lines align beneath list content, including nested numbered items.
- Fenced code displays literally. By default the opening language hint is shown in a subtle label when delimiters are hidden; themes can disable labels or show the original delimiters.
- `MarkdownSpectreTheme.SyntaxHighlighter` optionally supplies one `IMarkdownHighlightSession` per code fence. Spans must be ordered, non-overlapping and within source-line bounds.
- Bad spans or provider failures use plain safe code styling; terminal controls are sanitized and Spectre markup is never parsed from source strings.

For real TextMate-based coloring, opt in to the sibling [Markdown.TextMate adapter](../../../Markdown.TextMate/README.md) and see its [integration guide](../../../Markdown.TextMate/Docs/Grammar-Adapter/en.md).
No code is executed, and no full CommonMark quote nesting is claimed.

[Renderer status](../../IMPLEMENTATION_STATUS.md).
