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

### Verified cross-platform CI — 2026-10-29

- Parser implementation commit `fb7a8108`: [GitHub Actions #40881801829](https://github.com/afrowaveltd/Toolbox/actions/runs/40881801829) **SUCCESS** — full Linux solution build/tests and existing Windows regression job.
- Image presentation implementation commit `7696fef8`: [GitHub Actions #40882016971](https://github.com/afrowaveltd/Toolbox/actions/runs/40882016971) **SUCCESS** — full Linux solution build/tests and Windows FileBrowser, Configurator, Showcase, localization **plus newly added focused ExchangeHouse Markdown and Blackboard Markdown tests**.
- No direct manual Debian or Windows terminal test of this new image fallback has been reported. Full CommonMark, reference-style links, actual image drawing and translated document link routing are still not implemented.

## 2026-10-31 — nested list source representation

- ExchangeHouse.Markdown now exposes original list numbering and indentation plus an immutable hierarchy, with exact unchanged-source round trips. The Spectre display update is a following independent increment; parser CI pending.

### Cross-platform CI verified — 2026-10-31

- [GitHub Actions #41244480220](https://github.com/afrowaveltd/Toolbox/actions/runs/41244480220) **SUCCESS** for implementation up through commit `415065a4c272838f3bc0e4ed94ab8ea90003a295`.
- Linux: full Toolbox solution **build and tests passed**; final build reported **0 warnings, 0 errors**.
- Windows: **ExchangeHouse.Markdown.Tests 41/41**, **Blackboard.SpectreConsole.Markdown.Tests 13/13**, **Blackboard.Showcase.Tests 26/26**; FileBrowser 160/160, Configurator 44/44, and targeted localization tests 34/34 passed.
- Intermediate runs on parser/renderer commits failed at **test compilation** due to literal `\\`` C# escape sequences mistakenly introduced when authoring Markdown code examples. Fixed in commits `e7b858aa` and `415065a4`; the final CI is green. Existing production source was not weakened to make tests pass.
- Manual real-terminal testing of newly added nested-list visuals in Debian/Windows remains unreported. The current model does not claim CommonMark-compatible loose/multiline lists.

## 2026-10-31 — Markdown list continuation support

- ExchangeHouse Markdown now models explicitly indented paragraph continuations within list items; renderer integration follows as an independent change. The source parser remains UI-neutral. CI pending.

### Verified CI — 2026-10-31 list continuation increment

- [GitHub Actions #41248289922](https://github.com/afrowaveltd/Toolbox/actions/runs/41248289922) **SUCCESS**, source through commit `35efe934031a99a5407550f6624d96656b9c03a3`: Linux full solution build/tests passed; Windows ExchangeHouse Markdown **46/46**, Blackboard Markdown **16/16**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**.
- Markdown local link job **PASS**: 269 Markdown files, 1,374 local file targets checked with none missing. Source-aware continuation behavior also tested on Windows; manual Debian and real-terminal visual checks remain separate.
- Initial renderer integration CI failed **one incorrect test expectation** (custom indent `....` erroneously checked as six literal spaces). Fixed the assertion in `35efe934`, retaining custom-indent coverage. The failed earlier CI runs are not counted as successes.
- This milestone handles only **explicitly indented** continuation lines. Loose lists, lazy continuation and nested CommonMark block containers remain unsupported. No public NuGet release was made.

## 2026-10-31 — blank-separated Markdown list paragraphs

- ExchangeHouse Markdown now retains explicit blank separators inside list items when followed by safely indented paragraphs. UI-neutral, unchanged-source and additive API; full CommonMark loose list containers remain future work. CI pending.
