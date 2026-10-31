# Source-aware code and quote containers within lists

This bounded extension retains original text, CRLF/LF/CR line breaks and source offsets while associating indented fenced code and blockquotes with a preceding Markdown list item.

~~~~markdown
- Build
  > Keep the following command literal.

  ```sh
  echo "hello"
  ```
~~~~

## Model

- `MarkdownBlock.CodeFenceInfo` retains the information string, `CodeLanguage` the first token, `CodeFenceStart` the opener's source offset, and `IsClosingCodeFence` the matching end marker.
- `MarkdownBlock.QuoteDepth` identifies quote marker count; quote inline links keep absolute source offsets.
- `MarkdownListItemNode.EmbeddedBlocks` contains owned code fences, code lines and quotes. `ContentBlocks` maintains overall attached-source order; `ContinuationBlocks` remains paragraph-only.
- `MarkdownWriter.Write` leaves the source unchanged; fenced code is not parsed as Markdown, and no code is executed.

## Boundaries

This is not a full CommonMark block-container parser. Arbitrary nested quote-level block ASTs, unfenced indented code blocks and HTML blocks are still unsupported. Conservative quoted lists and explicit same-depth quoted fences are now supported: see [quoted lists](../Quoted-Lists/en.md) and [quoted code](../Quoted-Code-Fences/en.md). Outdented top-level quote/fence markers are not forced into the preceding list item. Long/unknown language hints are handled as literal source.

[Parser status](../../IMPLEMENTATION_STATUS.md) · [Markdown README](../../README.md).
