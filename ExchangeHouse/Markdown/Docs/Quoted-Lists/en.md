# Source-preserving lists inside blockquotes

This bounded addition recognizes lists on **consecutive explicitly quote-prefixed**
Markdown lines. It is not a complete CommonMark block-container grammar.

## Example

```markdown
> - Parent
>   12) Ordered child
>     - Nested bullet
>   13) Next child
>> - Separate list at deeper quote level
```

The original quoted marker characters, numbering, inline offsets, line endings and
link destinations remain unchanged. `MarkdownWriter.Write` still returns the
unmodified input string.

## Data model

- `MarkdownDocument.ListRoots` contains the unquoted list hierarchy.
- Additive `MarkdownDocument.QuoteListRoots` holds quoted list roots in source order.
- Each quoted item's `Block.QuoteDepth` records the quote level. Its
  `MarkdownListMarker.Depth` is relative to that level.
- A different quote level starts a separate quoted list rather than becoming
  a nested child of the preceding level.
- Consecutive indented and immediate lazy paragraph lines can belong to the
  quoted item. Their source spans and `ListContinuationOwnerStart` are preserved.

## Current limits

Unquoted lines, a quote-depth change, quote-prefixed headings and blank quote lines
interrupt the active list container. Blank-separated quoted list paragraphs,
fenced code within quote containers and arbitrary nested blocks remain unsupported.
No grammar coloring or executable code is required in ExchangeHouse.

[Earlier code/quote support](../Code-and-Quote-Containers/en.md) ·
[Implementation status](../../IMPLEMENTATION_STATUS.md).
