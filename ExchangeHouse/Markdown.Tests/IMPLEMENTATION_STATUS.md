# ExchangeHouse.Markdown.Tests — Implementation status

## 2026-10-29 — initial regression suite

- Added xUnit project to `Toolbox.sln` covering first-stage Markdown parser/writer behavior.
- Local SDK is unavailable in this execution environment. **Verified:** [GitHub Actions #40846902206](https://github.com/afrowaveltd/Toolbox/actions/runs/40846902206) SUCCESS for commit `3252650b` (whole solution build and test including this project).
- Current suite is intentionally scoped to the block scanner. Add independent tests before extending syntax support.

## 2026-10-29 — inline scanner tests

- Added a separate `MarkdownInlineParserTests` suite for conservative inline syntax and source-safe integration with existing blocks.
- The suite is part of the same solution CI; verification for the latest implementation commit is pending.

### Verified CI — 2026-10-29

- [GitHub Actions #40848846720](https://github.com/afrowaveltd/Toolbox/actions/runs/40848846720) **SUCCESS** for inline parser commit `b566ad670bb58e52049c714b8dbba6d1f3838287` — full solution build and test passed.
- This verifies the conservative supported syntax and regressions only. Full CommonMark, editable AST, conversion and UI rendering are not yet claimed.
