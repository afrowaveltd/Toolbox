# Explicit Markdown list continuation paragraphs

**Implemented 2026-10-31:** conservative explicitly indented continuation support, not complete CommonMark.

Each existing `MarkdownBlock` still represents one physical source line, and `MarkdownWriter.Write` still returns the exact original string. When a nonempty line is indented to at least the preceding list item's *content column*, it is attached to the nearest eligible list item. The parent-child hierarchy remains independent of its continuation paragraphs.

The public additions are `MarkdownListItemNode.ContinuationBlocks`, `MarkdownBlock.ListContinuationMarker`, `MarkdownBlock.ListContinuationOwnerStart`, and `MarkdownListMarker.ContentIndentColumns`. Inline content offsets remain original absolute string character positions. Tabs advance to four-column stops. A continuation following a nested child can reattach to a parent when the indentation fits the parent's content column.

Example: an item `- Title` followed by `  Additional text` and `  [Guide](../manual.md)` owns both indented paragraphs.

**Limits (updated 2026-10-31):** blank lines can now remain inside a list item when followed by an explicitly indented continuation paragraph; see [Loose list paragraphs](../Loose-List-Paragraphs/en.md). Trailing blanks, blank-before-marker boundaries, lazy unindented continuations, compound blocks, nested code fences and complete CommonMark rules remain unsupported. Some indented heading/fence-like text is treated as literal continuation content. This is intentional to preserve old parsing boundaries and avoid speculative semantics.

[Earlier nested-list model](../Nested-Lists/en.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md).
