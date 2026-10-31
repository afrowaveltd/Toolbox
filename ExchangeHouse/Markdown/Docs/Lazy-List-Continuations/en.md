# Bounded lazy list paragraph continuations

Implemented 2026-10-31 as an additive extension of the source-preserving Markdown scanner. This is **not** a complete CommonMark block-container parser.

## Behavior

Paragraph text immediately following an active list item or its continuation may omit the item's usual indentation. The line remains a `Paragraph` block owned by the **deepest active list item**. If an explicitly indented line later fits an ancestor's content column, the parser's existing ancestor-selection logic restores that owner.

```markdown
- Parent
  12) Child
Child continues without indentation.
  Parent continues after the child.
```

The child owns the lazy line; the parent owns the final explicit line. `MarkdownBlock.IsLazyListContinuation` is true on the lazy line and false for the indented one. `ListContinuationOwnerStart`, `ListContinuationMarker`, `MarkdownListItemNode.ContinuationBlocks` and `ContentBlocks` work exactly as before. Inline nodes and link destination offsets still refer to the original source; `MarkdownWriter.Write` returns the untouched document.

## Boundaries

- Blank lines end lazy attachment. Existing blank-separated paragraphs still require **explicit indentation** to resume ownership.
- Recognized list markers start list items rather than lazy paragraphs.
- Headings, fenced-code openers, blockquotes and thematic-break markers do not become lazy continuations.
- Nested code blocks, full list-container parsing, arbitrary HTML blocks and complete CommonMark compliance remain outside the supported grammar.
- There is no UI/runtime dependency in ExchangeHouse and no source normalization.

[Loose list paragraphs](../Loose-List-Paragraphs/en.md) · [List continuations](../List-Continuations/en.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md).
