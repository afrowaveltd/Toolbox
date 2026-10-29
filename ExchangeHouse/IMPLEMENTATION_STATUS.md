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

### Verified CI — 2026-10-29

- [GitHub Actions #40848846720](https://github.com/afrowaveltd/Toolbox/actions/runs/40848846720) **SUCCESS** for inline parser commit `b566ad670bb58e52049c714b8dbba6d1f3838287` — full solution build and test passed.
- This verifies the conservative supported syntax and regressions only. Full CommonMark, editable AST, conversion and UI rendering are not yet claimed.

## 2026-10-29 — image rendering reuse

- Blackboard.SpectreConsole.Markdown displays ExchangeHouse Image nodes as styled text alternatives. No back dependency on UI in ExchangeHouse, no binary image or network operations. CI pending.
