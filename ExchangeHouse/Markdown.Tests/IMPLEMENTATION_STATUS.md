# ExchangeHouse.Markdown.Tests — Implementation status

## 2026-10-29 — initial regression suite

- Added xUnit project to `Toolbox.sln` covering first-stage Markdown parser/writer behavior.
- Local SDK is unavailable in this execution environment. **Verified:** [GitHub Actions #40846902206](https://github.com/afrowaveltd/Toolbox/actions/runs/40846902206) SUCCESS for commit `3252650b` (whole solution build and test including this project).
- Current suite is intentionally scoped to the block scanner. Add independent tests before extending syntax support.

## 2026-10-29 — inline scanner tests

- Added a separate `MarkdownInlineParserTests` suite for conservative inline syntax and source-safe integration with existing blocks.
- The suite is part of the same solution CI; verification for the latest implementation commit is pending.
