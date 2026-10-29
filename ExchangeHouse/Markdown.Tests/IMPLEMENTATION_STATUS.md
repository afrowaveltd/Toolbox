# ExchangeHouse.Markdown.Tests — Implementation status

## 2026-10-29 — initial regression suite

- Added xUnit project to `Toolbox.sln` covering first-stage Markdown parser/writer behavior.
- Local SDK is unavailable in this execution environment. **Verified:** [GitHub Actions #40846902206](https://github.com/afrowaveltd/Toolbox/actions/runs/40846902206) SUCCESS for commit `3252650b` (whole solution build and test including this project).
- Current suite is intentionally scoped to the block scanner. Add independent tests before extending syntax support.
