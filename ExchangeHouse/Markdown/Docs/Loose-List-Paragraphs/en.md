# Conservative loose-list paragraphs

**Implemented 2026-10-31** as a source-preserving extension, not full CommonMark.

An item containing explicitly indented continuation paragraphs may now retain one or more intervening blank lines. Each blank is owned **only if** the next nonblank line is a sufficiently indented *paragraph continuation* of that item (or an ancestor). Otherwise it remains an ordinary blank line and the existing list-run boundary rules apply.

## Immutable public model

- `MarkdownListItemNode.IsLoose`: true when at least one blank separator was assigned to that item.
- `MarkdownListItemNode.SeparatorBlocks`: blank lines owned by this item.
- `MarkdownListItemNode.ContentBlocks`: continuation paragraphs and owned blank lines, in exact source order; excludes nested child list marker lines.
- `MarkdownBlock.ListBlankOwnerStart`: optional original source offset of the separator's owning list item.
- Existing `MarkdownDocument.Blocks`, `ListRoots`, `ContinuationBlocks`, source offsets, constructors and unchanged `MarkdownWriter.Write` semantics are retained.

Example:

```markdown
- First
  First detail.

  A separate paragraph with **emphasis**.
- Second
```

Both detail lines and their blank separator belong to First. CRLF, LF, CR, tabs, whitespace and link target spelling remain untouched in the source snapshot.

## Deliberate boundaries

- A trailing blank, an unindented paragraph or an intervening new list marker **does not** create an owned separator. Separate lists remain separate.
- Blank lines followed by **nested list markers** are not yet attached; mixed nested loose list containers require a proper CommonMark block parser.
- Unindented lazy continuation, nested fenced code and arbitrary CommonMark list-item contents remain unsupported.
- The parser holds only indices for pending blank lines and resolves ownership when the next actual content arrives; no network access and no speculative source rewriting.

[Explicit list continuations](../List-Continuations/en.md) · [Nested list baseline](../Nested-Lists/en.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md).
