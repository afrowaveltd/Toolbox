# Optional Markdown TextMate syntax highlighting

A **separate, opt-in** .NET 10 adapter for Markdown code fences using
[TextMateSharp](https://github.com/danipen/TextMateSharp) and bundled TextMate grammars
(MIT license, `TextMateSharp.Grammars` 2.0.4).
The parser (ExchangeHouse) and base renderer (Blackboard Markdown) remain independent
of this package and of its native Oniguruma dependencies.

```csharp
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.TextMate;
var theme = new MarkdownSpectreTheme
{
    SyntaxHighlighter = new MarkdownTextMateHighlighter()
};
MarkdownSpectreRenderer.Render(console, readmeMarkdown, theme);
```

The first token of a fence's information string supplies a language hint.
Aliases include `cs`, `bash`, `pwsh`, `js`, `ts` and `py`.
The adapter never executes code or fetches external grammars. Unknown languages
fall back to plain text; a tokenizer session retains multiline grammar state.
The renderer validates source spans and sanitizes terminal control characters.

[Implementation details](Docs/Grammar-Adapter/en.md) ·
[Implementation status](IMPLEMENTATION_STATUS.md).

**Not published on NuGet.**
