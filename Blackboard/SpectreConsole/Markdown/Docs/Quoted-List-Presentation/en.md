# Quoted list rendering in Spectre.Console

The renderer now consumes `MarkdownBlock.QuoteDepth` plus the existing list
metadata to render quote-contained nested items and their paragraphs:

- Quote markers appear **outside** list indentation and follow
  `MarkdownSpectreTheme.QuotePrefix` / `QuoteMarker`.
- Original ordered markers like `12)` and `13.` are retained.
- `ListIndent`, `ListBullet` and custom quote styles stay independent.
- Quote levels do not accidentally merge unrelated list hierarchies.
- Inline links and emphasized text remain safe, literal Spectre text.

The source parser stays UI-independent; the renderer does not rewrite Markdown,
load remote content or execute code. Full CommonMark nesting and code fences
inside explicitly quote-prefixed containers remain future increments.

[Quoted-list parser design](../../../../ExchangeHouse/Markdown/Docs/Quoted-Lists/en.md) ·
[Renderer status](../../IMPLEMENTATION_STATUS.md).
