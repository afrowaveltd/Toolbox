# Multiline list item presentation in Spectre.Console

**Added 2026-10-31:** continuation lines indented under a list item are rendered under the item's **visible text**, not as new bullets.

The Markdown parser belongs to ExchangeHouse; this renderer reads `MarkdownBlock.ListContinuationMarker` when the block is a recognized continuation. Its line still has `Kind == MarkdownBlockKind.Paragraph` and its original source remains untouched. Rendering computes indentation from the theme's `ListIndent`, the owner's nesting depth and the **displayed marker width**. Thus, `12)` and a custom `+` bullet are both aligned correctly. Inline emphasis, text and links are rendered using existing safe text writers.

Example input:

- `- First`
- `  Continued **bold** text`
- `  [Manual](../manual.md)`

Default output shows a single bullet for First and the following text directly below the item's content. Blank and unindented lines become normal paragraphs.

**Scope:** only explicitly indented lines are attached; loose lists separated by blank lines, lazy continuations, nested code/heading blocks and full CommonMark are not yet supported. Rendering does not edit the Markdown document, follow links or access files.

[Parser contract](../../../../../ExchangeHouse/Markdown/Docs/List-Continuations/en.md) · [Renderer implementation status](../../IMPLEMENTATION_STATUS.md).
