# Rendering loose quoted list paragraphs in Spectre.Console

The renderer uses `MarkdownBlock.QuoteDepth` to display the configured quote marker and `ListContinuationMarker` to align the second paragraph under the item's visible content. Its behavior remains independent from the parser's source-preserving ownership decision.

## Source example

```markdown
> 12) First paragraph
>
>     Second **important** paragraph.
> 13) Next item
```

The blank quote-prefix line is still rendered, the following paragraph is indented to match the original `12)` item and no extra marker is drawn. Theme options `QuotePrefix`, `QuoteMarker`, `ListIndent` and `ListBullet` stay independent; original ordered numbers remain unchanged.

No new runtime dependency or text rewrite is required. `Blackboard.SpectreConsole.Markdown` does not interpret code blocks as executable instructions or Spectre markup.

[Parser design](../../../../../ExchangeHouse/Markdown/Docs/Quoted-Loose-Paragraphs/en.md) · [Renderer status](../../IMPLEMENTATION_STATUS.md).
