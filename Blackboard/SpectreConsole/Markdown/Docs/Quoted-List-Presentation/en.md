# Quoted list rendering in Spectre.Console

The renderer now consumes `MarkdownBlock.QuoteDepth` plus the existing list
metadata to render quote-contained nested items and their paragraphs:

- Quote markers appear **outside** list indentation and follow
  `MarkdownSpectreTheme.QuotePrefix` / `QuoteMarker`.
- Original ordered markers like `12)` and `13.` are retained.
- `ListIndent`, `ListBullet` and custom quote styles stay independent.
- Quote levels do not accidentally merge unrelated list hierarchies.
- Inline links and emphasized text remain safe, literal Spectre text.
- Quote-prefixed blank separators in a loose list remain visible as empty quoted lines, while the following indented paragraph aligns beneath the owning bullet or number. See [quoted loose paragraph presentation](../Quoted-Loose-Presentation/en.md).

The source parser stays UI-independent; the renderer does not rewrite Markdown,
load remote content or execute code. Full CommonMark nesting remains future work. Explicit quote-prefixed code fences now render within quoted lists; see [quote-code rendering](../Quoted-Code-Presentation/en.md).

[Quoted-list parser design](../../../../../ExchangeHouse/Markdown/Docs/Quoted-Lists/en.md) ·
[Renderer status](../../IMPLEMENTATION_STATUS.md).
