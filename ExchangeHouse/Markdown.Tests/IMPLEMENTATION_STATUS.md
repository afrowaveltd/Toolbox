# ExchangeHouse.Markdown.Tests — Implementation status

## 2026-10-29 — initial regression suite

- Added xUnit project to `Toolbox.sln` covering first-stage Markdown parser/writer behavior.
- Local SDK is unavailable in this execution environment. CI on GitHub is the integration verification source; do not claim tests passed until workflow completion.
- Current suite is intentionally scoped to the block scanner. Add independent tests before extending syntax support.
