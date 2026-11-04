# Blank-separated paragraphs inside explicitly quoted lists

This bounded extension handles **quote-prefixed blank separator lines** inside a list that is itself written under an explicit Markdown blockquote. It complements ordinary [loose lists](../Loose-List-Paragraphs/en.md) and [quoted list hierarchy](../Quoted-Lists/en.md).

## Example

```markdown
> - Parent
>   12) First paragraph
>
>       Second paragraph with [link](../en.md).
>   Back to parent
```

The blank `>` is not eagerly classified as belonging to the list item. The parser holds its **source index** until the next nonblank line. A continuation that meets the owning item's content indentation and retains the **same explicit quote depth** attaches the blank to that item. An ancestor may take ownership after a nested numbered child.

## Read-only metadata

- `MarkdownDocument.QuoteListRoots` remains separate from ordinary `ListRoots`.
- `MarkdownListItemNode.SeparatorBlocks` and `ContentBlocks` expose the correctly ordered source-preserving blank quote lines and attached continuation paragraphs.
- `MarkdownListItemNode.IsLoose` becomes true only when at least one separator is actually assigned.
- `MarkdownBlock.ListBlankOwnerStart` records the owning list marker's original source offset. The blank quote line retains `Kind == BlockQuote`, `QuoteDepth`, `RawText`, `Start` and `Length`, rather than becoming an unquoted `Blank` block.
- Exact original Markdown, line endings, source offsets and link spans are retained by `MarkdownWriter.Write`.

## Deliberate boundaries

A quote-prefixed blank is **not** attached when followed by an unindented quoted paragraph, a new list marker, a different quote nesting level, an unquoted line or an unrelated blank boundary. Code fences remain protected: a blank inside a quoted fence is a literal `CodeLine`, not a list separator. Unsupported lazy continuation *across* a blank line and arbitrary CommonMark block nesting are not implied.

[Implementation status](../../IMPLEMENTATION_STATUS.md).
