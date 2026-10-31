# Afrowave.Toolbox — repository implementation status

**Updated 2026-10-31. Source of truth:** [GitHub master](https://github.com/afrowaveltd/Toolbox/tree/master). This file is a **cross-project continuation overview**; authoritative technical details remain in each component's `IMPLEMENTATION_STATUS.md`.

## Completed implementations and owned status

| Area | Current implementation | Continue here |
|---|---|---|
| Essentials | Results, responses, metadata, diagnostics, guards, interfaces; NuGet 0.2.0 published | [Status](Essentials/IMPLEMENTATION_STATUS.md) |
| WhenItFails | Error catalogs/runtime, resolution, validation and recovery; NuGet 1.0.0 published | [Status](WhenItFails/IMPLEMENTATION_STATUS.md) |
| UnderstandMe | Offline localization, dictionaryless JSON storage, placeholders and Configurator; large orchestration roadmap remains future | [Status](UnderstandMe/IMPLEMENTATION_STATUS.md) |
| Blackboard FileBrowser | Interactive picker, masks/regex/tree/search; NuGet 0.1.0-preview.1 published | [Status](Blackboard/SpectreConsole/FileBrowser/IMPLEMENTATION_STATUS.md) |
| ExchangeHouse.Markdown | Source-preserving parsing/writing of supported Markdown, inline links/images and contiguous nested/numbered-list metadata | [Status](ExchangeHouse/Markdown/IMPLEMENTATION_STATUS.md) |
| Blackboard Markdown | Themed Spectre renderer, safe image text alternatives and nested list display | [Status](Blackboard/SpectreConsole/Markdown/IMPLEMENTATION_STATUS.md) |
| Showcase | FileBrowser demos and four-theme Markdown gallery | [Status](Blackboard/Samples/Blackboard.Showcase/IMPLEMENTATION_STATUS.md) |
| UnderstandMe Configurator | Settings editing/validation and offline help via Blackboard Markdown | [Status](UnderstandMe/Tools/Configurator/IMPLEMENTATION_STATUS.md) |
| SeeMe console | WhenItFails presentation adapter | [Status](SeeMe/WhenItFails/Console/IMPLEMENTATION_STATUS.md) |
| Toolroom / Setter | Catalog authoring/maintenance and release comparison/validation tools | [Status](Toolroom/IMPLEMENTATION_STATUS.md) |

Publication of **Essentials 0.2.0, WhenItFails 1.0.0 and FileBrowser 0.1.0-preview.1** was confirmed by the maintainer; the two Markdown library projects are **not recorded as published**. Version metadata alone is not evidence of publication. [Release status](Docs/Release-Status/en.md).

## Latest verified functional checkpoint

- [GitHub Actions #41244480220](https://github.com/afrowaveltd/Toolbox/actions/runs/41244480220): full Linux solution and Windows FileBrowser/Configurator/Showcase/localization/Markdown regressions passed for nested/numbered Markdown implementation.
- [GitHub Actions #41244630027](https://github.com/afrowaveltd/Toolbox/actions/runs/41244630027): subsequent documentation checkpoint passed Linux and Windows.
- User confirmed Markdown Showcase and Configurator Help work in Windows manual testing; Debian testing of some latest visuals is still pending.
- This documentation refresh changes navigation and records previously released packages; it does **not** modify executable code or imply new package publication. CI for the refreshed documentation commit must be checked separately.

## 2026-10-31 — repository-wide documentation reconciliation

- Replaced the obsolete Essentials-only root README/docs directory with a full Toolbox index, overview, architecture, repository getting-started, development/verification and release inventory.
- Reconciled stale FileBrowser publication status, implemented Blackboard/ExchangeHouse Markdown and first consumer integrations in family READMEs.
- Added missing family entries for SeeMe, Toolroom, AJIS design notes, tests and their status/topic documents.
- The root `Docs/Getting-Started/en.md` and `Docs/API-Reference/en.md` remain **Essentials-specific**, explicitly linked from the new Toolbox-wide docs index.
- Preserved community and historical Id documents as design/background. AJIS/A2 notes are not labeled a compiled runtime.
- Cross-repo markdown link audit and final CI status must be recorded after verification.

## Work discipline

1. Inspect latest GitHub master code and the owning project's implementation status.
2. Make a small change together with tests, English README/topic docs and status update.
3. Commit immediately and validate on CI; identify exact commit/run.
4. Never mark work green from a pending CI run or infer NuGet publication from a `.csproj` version.

## Next engineering focus

Continue [ExchangeHouse Markdown](ExchangeHouse/Markdown/IMPLEMENTATION_STATUS.md) with multiline/loose lists in bounded increments, or improve Markdown documentation/localization link protection. No full CommonMark support has been claimed.
