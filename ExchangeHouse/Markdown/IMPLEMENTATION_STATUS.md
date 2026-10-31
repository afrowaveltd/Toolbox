# ExchangeHouse.Markdown — Implementation status

## 2026-10-29 — block scanner and lossless source writer

- Added independent .NET 10 `Afrowave.Toolbox.ExchangeHouse.Markdown` project, no external package dependencies.
- Added source-preserving first-pass `MarkdownParser`, `MarkdownDocument` / `MarkdownBlock` and `MarkdownWriter`.
- Recognizes headings, list markers, blank lines and fenced code without interpreting protected code as markup. Preserves original line endings, source offsets and exact unmodified round trips.
- Added xUnit tests in `../Markdown.Tests`; registered both projects in `Toolbox.sln`.
- Does **not** implement inline parsing, editable AST, format conversion or full CommonMark behavior. Does not yet integrate with Configurator; existing Help renderer remains in place.
- **Verified:** [GitHub Actions #40846902206](https://github.com/afrowaveltd/Toolbox/actions/runs/40846902206) SUCCESS for commit `3252650b`; whole solution build and test succeeded. NuGet publishing is still deferred until API scope and release readiness are explicitly reviewed.

### Next

1. Confirm CI and manual development build.
2. Design/test proper inline nodes (emphasis, code spans, links including escaped delimiters and reference links) before rewriting.
3. Implement editable writer plus loss-awareness for future text/HTML converters.
4. Build independently packageable Blackboard Spectre.Console Markdown renderer with customizable theme.

## 2026-10-29 — conservative inline tokens

- Added `MarkdownInlineParser`, `MarkdownInlineNode` and `MarkdownInlineKind` with raw source spans, source offsets, nested children and separate link destinations.
- `MarkdownParser` populates inline nodes on heading/list/paragraph lines and excludes fenced code completely. Conservative support for emphasis, strong text, backtick code spans, and simple inline links; malformed syntax is retained literally.
- Added targeted regression tests for escaped brackets, nested link destination parentheses, nested emphasis, absolute offsets, preserved URLs/email as code, and source-preserving write.
- No reference-style links, images, link titles, full delimiter algorithm, editable writer or conversion API yet. Integration with the Configurator and Spectre theme remains a separate future increment.
- CI verification pending at initial commit; do not assert success until the workflow finishes.

### Verified CI — 2026-10-29

- [GitHub Actions #40848846720](https://github.com/afrowaveltd/Toolbox/actions/runs/40848846720) **SUCCESS** for inline parser commit `b566ad670bb58e52049c714b8dbba6d1f3838287` — full solution build and test passed.
- This verifies the conservative supported syntax and regressions only. Full CommonMark, editable AST, conversion and UI rendering are not yet claimed.

## 2026-10-29 — initial image and link metadata

- Added image tokens with exact destination offsets and optional titles; retained original link API construction and raw source preservation. Tests cover valid/malformed forms. No full CommonMark. CI pending.

### Verified cross-platform CI — 2026-10-29

- Parser implementation commit `fb7a8108`: [GitHub Actions #40881801829](https://github.com/afrowaveltd/Toolbox/actions/runs/40881801829) **SUCCESS** — full Linux solution build/tests and existing Windows regression job.
- Image presentation implementation commit `7696fef8`: [GitHub Actions #40882016971](https://github.com/afrowaveltd/Toolbox/actions/runs/40882016971) **SUCCESS** — full Linux solution build/tests and Windows FileBrowser, Configurator, Showcase, localization **plus newly added focused ExchangeHouse Markdown and Blackboard Markdown tests**.
- No direct manual Debian or Windows terminal test of this new image fallback has been reported. Full CommonMark, reference-style links, actual image drawing and translated document link routing are still not implemented.

## 2026-10-31 — list markers and nesting model

- Added immutable `MarkdownListMarker`, `MarkdownListItemNode`, `MarkdownDocument.ListRoots` and optional `MarkdownBlock.ListMarker` while preserving flat Blocks and lossless writing.
- The scanner recognizes nested/mixed and ordered lists (original marker, start number, delimiter, indentation, depth, content offsets), bounded to contiguous single-line items. Tabs recognized at four-column stops; source never normalized.
- Added parser regressions for hierarchy, source spans, fence isolation, non-list markers and interruptions. **CI pending** at implementation commit; no full CommonMark list grammar asserted.

### Cross-platform CI verified — 2026-10-31

- [GitHub Actions #41244480220](https://github.com/afrowaveltd/Toolbox/actions/runs/41244480220) **SUCCESS** for implementation up through commit `415065a4c272838f3bc0e4ed94ab8ea90003a295`.
- Linux: full Toolbox solution **build and tests passed**; final build reported **0 warnings, 0 errors**.
- Windows: **ExchangeHouse.Markdown.Tests 41/41**, **Blackboard.SpectreConsole.Markdown.Tests 13/13**, **Blackboard.Showcase.Tests 26/26**; FileBrowser 160/160, Configurator 44/44, and targeted localization tests 34/34 passed.
- Intermediate runs on parser/renderer commits failed at **test compilation** due to literal `\\`` C# escape sequences mistakenly introduced when authoring Markdown code examples. Fixed in commits `e7b858aa` and `415065a4`; the final CI is green. Existing production source was not weakened to make tests pass.
- Manual real-terminal testing of newly added nested-list visuals in Debian/Windows remains unreported. The current model does not claim CommonMark-compatible loose/multiline lists.

## 2026-10-31 — explicitly indented continuation model

- Added additive source-aware metadata for list continuation lines, and read-only per-item continuation collections. Preserves flat Blocks, raw source, existing constructors and writer output.
- Handles original numbered marker content columns, tabs and reattachment to an ancestor after nested content. Blank/unindented lines terminate attachment. No loose/lazy CommonMark claims.
- Added five focused parser regression tests and [design/behavior notes](Docs/List-Continuations/en.md). **CI verification pending** for this increment.
