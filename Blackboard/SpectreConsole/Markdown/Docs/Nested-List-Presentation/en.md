# Nested and ordered Markdown presentation

**Added:** 2026-10-31. The underlying structure is provided by ExchangeHouse.Markdown, not parsed by this Spectre renderer.

The renderer retains original ordered list numbers and delimiters: `7)` stays `7)`, and `8.` stays `8.`. Nested items are indented according to the `MarkdownBlock.ListMarker.Depth` metadata. Unordered items display `MarkdownSpectreTheme.ListBullet` (default `•`) unless `PreserveUnorderedMarkers` is enabled to display the original `-`, `+` or `*`. `ListIndent` (default two spaces) controls indentation per nesting level; `ListMarker` controls the marker style.

For example:

```markdown
- Parent
  3) Step
    * Note
  4. Step
```

The default terminal output displays `• Parent`, then indented `3) Step`, then further-indented `• Note`, and indented `4. Step`. Source numbering and whitespace remain unchanged in ExchangeHouse's document and writer. No terminal conversion writes back to the source.

This is a **simple contiguous single-line list** model. Multi-paragraph list items, loose lists, proper CommonMark list containers, nested code blocks, task list checkboxes and multi-line continuation are future work.

Regression tests verify mixed nested lists, ordered numbering, custom original markers, custom indentation and fenced code isolation. Linux and Windows CI verification is documented separately.
