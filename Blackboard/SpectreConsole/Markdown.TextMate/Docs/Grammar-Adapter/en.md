# Optional TextMate syntax adapter

Separate from the UI-neutral ExchangeHouse parser and base Blackboard Spectre renderer,
this adapter uses TextMateSharp 2.0.4 (MIT) and bundled language grammars.

- Opt-in via `MarkdownSpectreTheme.SyntaxHighlighter`.
- Language aliases from fence hints are normalized to grammar IDs, while original Markdown is untouched.
- Each code fence gets a stateful grammar session; code is never executed.
- Per-line tokenization budget is 40 ms. Unknown languages and tokenizer errors fall back to literal text; renderer rejects unsafe source spans and excessive lines.
- This optional dependency brings Oniguruma native assets; verify Linux, Windows, SSH and restricted terminals.
- Styling is from TextMate themes; GitHub-equivalent highlighting and full CommonMark are not claimed.

Upstream: [TextMateSharp](https://github.com/danipen/TextMateSharp),
[GitHub Linguist](https://github.com/github-linguist/linguist).

## 2026-11-04 — broad bundled language resolution

All registry language IDs and available extension-based grammar mappings are now considered. Alias normalization covers common fenced Markdown spellings, including JavaScript and TS abbreviations, languages, shells and data formats. There is no custom grammar loader or requirement to install all grammars separately. See [Supported Grammars](../Supported-Grammars/en.md) for examples and the limitation that missing bundle grammars stay plain text.

The separate mdview app now opts into the adapter for all three source types. The parser and base Markdown renderer remain dependency-free with respect to TextMateSharp.
