# ExchangeHouse — Implementation status

## 2026-10-29 — independent format family foundation

- Established `ExchangeHouse` as an organizational family, **not** a monolithic NuGet package.
- Added first independent `Markdown` project plus tests; see [Markdown implementation status](Markdown/IMPLEMENTATION_STATUS.md).
- Documented separation from `Blackboard.SpectreConsole.Markdown` and the future independent conversion helpers.
- Planned conversions include Markdown/plain text, HTML/plain text, Markdown/HTML, JSON/AJIS, JSON/XML and AJIS/XML; these **do not yet exist**.
- GitHub CI verification for the new commit is pending until a successful workflow has been inspected.
