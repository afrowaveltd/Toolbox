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
Aliases include `cs`, `bash`, `pwsh`, `js`, `ts`, `py` and many others. The adapter now tries every direct language ID and filename extension available in the bundled registry instead of limiting the selection to a small hard-coded list.
The adapter never executes code or fetches external grammars. Unknown languages
fall back to plain text; a tokenizer session retains multiline grammar state.
The renderer validates source spans and sanitizes terminal control characters.

[Supported languages, aliases and examples](Docs/Supported-Grammars/en.md) · [Implementation details](Docs/Grammar-Adapter/en.md) ·
[Implementation status](IMPLEMENTATION_STATUS.md).

**Not published on NuGet.**

## Broad grammar coverage and mdview integration (2026-11-04)

The grammar adapter resolves the entire **installed** TextMateSharp.Grammars language catalog (50+ languages upstream) by language ID or file extension, with common Markdown fence aliases. This includes JavaScript/TypeScript, Python, JSON/YAML/XML, HTML/CSS, SQL, Rust/Go/Java/C/C++ and other bundled grammars. A language missing from the package remains plain code; adding a language does not require changing the parser. `mdview` now opts into this adapter by default when rendering local files and remote documents; its prior default theme did not. Optional native assets still degrade safely to plain code.

Tests for this update are authored but not yet executed here; consult [Supported Grammars](Docs/Supported-Grammars/en.md). The earlier user-verified 6,443 tests precede this update.
