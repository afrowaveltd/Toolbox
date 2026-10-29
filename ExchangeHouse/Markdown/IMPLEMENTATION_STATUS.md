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
