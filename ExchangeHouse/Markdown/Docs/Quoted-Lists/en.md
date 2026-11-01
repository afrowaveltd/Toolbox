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

Unquoted lines, quote-depth changes and quote-prefixed headings interrupt the active list container. Quote-prefixed blanks are now held as **deferred separators**: a sufficiently indented following paragraph at the same quote depth attaches them to the owning list item. An unindented paragraph or new list marker does not. See [loose quoted paragraphs](../Quoted-Loose-Paragraphs/en.md). Arbitrary nested blocks remain unsupported. Same-depth explicit fenced code inside quoted lists is now supported as a bounded extension; see [quoted code](../Quoted-Code-Fences/en.md).
No grammar coloring or executable code is required in ExchangeHouse.

[Earlier code/quote support](../Code-and-Quote-Containers/en.md) ·
[Implementation status](../../IMPLEMENTATION_STATUS.md).
