# ExchangeHouse — Implementation status

## 2026-10-29 — independent format family foundation

- Established `ExchangeHouse` as an organizational family, **not** a monolithic NuGet package.
- Added first independent `Markdown` project plus tests; see [Markdown implementation status](Markdown/IMPLEMENTATION_STATUS.md).
- Documented separation from `Blackboard.SpectreConsole.Markdown` and the future independent conversion helpers.
- Planned conversions include Markdown/plain text, HTML/plain text, Markdown/HTML, JSON/AJIS, JSON/XML and AJIS/XML; these **do not yet exist**.
- **Verified:** [GitHub Actions #40846902206](https://github.com/afrowaveltd/Toolbox/actions/runs/40846902206) SUCCESS for commit `3252650b` (restore, build and complete solution test workflow).

## 2026-10-29 — first inline Markdown recognition

- ExchangeHouse.Markdown now provides source-aware conservative inline nodes for emphasis, code and simple links, without depending on UI or translation runtimes.
- Advanced CommonMark, converters, localizing link resolver and the Blackboard Spectre renderer remain planned. CI verification is pending for the implementation commit.
