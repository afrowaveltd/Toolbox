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

### Cross-platform CI verified — 2026-10-31

- [GitHub Actions #41244480220](https://github.com/afrowaveltd/Toolbox/actions/runs/41244480220) **SUCCESS** for implementation up through commit `415065a4c272838f3bc0e4ed94ab8ea90003a295`.
- Linux: full Toolbox solution **build and tests passed**; final build reported **0 warnings, 0 errors**.
- Windows: **ExchangeHouse.Markdown.Tests 41/41**, **Blackboard.SpectreConsole.Markdown.Tests 13/13**, **Blackboard.Showcase.Tests 26/26**; FileBrowser 160/160, Configurator 44/44, and targeted localization tests 34/34 passed.
- Intermediate runs on parser/renderer commits failed at **test compilation** due to literal `\\`` C# escape sequences mistakenly introduced when authoring Markdown code examples. Fixed in commits `e7b858aa` and `415065a4`; the final CI is green. Existing production source was not weakened to make tests pass.
- Manual real-terminal testing of newly added nested-list visuals in Debian/Windows remains unreported. The current model does not claim CommonMark-compatible loose/multiline lists.
