# Quoted lists nested under ordinary Markdown list items

This bounded, source-preserving extension recognizes an **ordinary list item
containing an explicitly indented quote with its own nested list**.
It complements [quoted lists](../Quoted-Lists/en.md) and
[quoted code fences](../Quoted-Code-Fences/en.md).

## Example

~~~~markdown
- Install the application
  > - **Warning:** Check the runtime.
  >   12) Review the dependency versions.
  >       ```bash
  >       dotnet --list-runtimes
  >       ```
  Continue the installation.
- Finish
~~~~

The outer item is recognized before interpreting an indented physical
`>` as a standalone quote. This matters for physical one-to-three-space
indentation and for code lines and blank separators inside the quote.

## Independent ownership

- `MarkdownDocument.ListRoots` retains the ordinary list hierarchy.
- `MarkdownDocument.QuoteListRoots` retains the quoted list hierarchy.
- `MarkdownBlock.QuoteContainerListOwnerStart` records the enclosing
  ordinary item's **absolute source offset** and
  `QuoteContainerListMarker` exposes its marker for presentation.
- `ListContinuationOwnerStart` retains its meaning: for paragraphs,
  fenced code and attached content inside the quote, it identifies the
  **inner quoted-list item**. Ordinary quoted prose may still attach directly
  to its enclosing ordinary list item.
- The ordinary item's `EmbeddedBlocks` and `ContentBlocks` contain
  the embedded quoted lines in source order; the quoted roots maintain
  their own independent tree.
- A quoted-list run is scoped to **both** its quote depth and outer item
  offset, so equal-depth quotes inside different ordinary items do not merge.
- Original ordered numbers, link offsets, quote markers, whitespace and
  CR/LF line endings are unchanged by the lossless writer.

## Bounds

This implementation requires explicit quote prefixes and correct list-content
indentation. Lazy unmarked blockquote continuation, arbitrary recursive
container combinations and complete CommonMark are not supported.
Code is never executed, and syntax coloring is outside the parser.

[Parser implementation status](../../IMPLEMENTATION_STATUS.md).
