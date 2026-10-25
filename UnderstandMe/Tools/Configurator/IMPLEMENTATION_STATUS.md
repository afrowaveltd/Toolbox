# UnderstandMe.Configurator — Implementation Status

## 2026-10-25 — first executable file-picker settings integration

- Added an independent .NET 10 interactive TUI executable consuming the **published FileBrowser 0.1.0-preview.1** NuGet and core UnderstandMe shared settings services.
- Implemented browse JSON, open path, create unsaved defaults, show safe summary, edit source language and worker buffer capacity, validate and confirmed revision-checked save; noninteractive `validate --config`.
- Validation and persistence remain solely in core; no Spectre.Console dependency in UnderstandMe itself.
- Added xUnit + Spectre.Console.Testing regression project registered in `Toolbox.sln`.
- [README](README.md) and [First Integration](Docs/First-Integration/en.md) document commands, boundaries and incomplete advanced features.
- **CI pending at commit time; do not assert successful compilation/test until verified.**
- Next: verify CI, test live terminals, then implement descriptor-driven provider/storage editing, shortcut bindings and expanded recovery UI.

### CI API compatibility fix

- CI found that Spectre.Console 0.57.2 `ConfirmationPrompt` does not expose `DefaultValue(false)`. Removed that unsupported fluent call; the save prompt still requires explicit user confirmation. A new CI run must validate compilation and tests.

### 2026-10-25 — targeted FileBrowser UI integration test

- Added injectable initial browsing directory to the Configurator host. Tests can exercise the **real published FileBrowser picker** in an isolated temporary folder without changing process-wide current directory.
- Added a virtual-terminal test that selects JSON through FileBrowser, loads it using the shared settings store and exits without changing the file. Switching to a different configuration now asks confirmation if there are unsaved edits.
- CI verification of this follow-up is pending.

### Verified GitHub CI (2026-10-25)

- [GitHub Actions run 40229277472](https://github.com/afrowaveltd/Toolbox/actions/runs/40229277472) **SUCCESS** for integration commit `087f5662`. Entire solution build and test workflow completed; the new Configurator project compiled independently while `UnderstandMe.dll` did not require any Spectre UI dependency.
- **8 Configurator.Tests passed**, including a real FileBrowser JSON selection driven by Spectre.Console.Testing, unsaved/cancelled file safety, revision conflict and invalid draft rejection. Existing FileBrowser tests: **160 passed**. The final CI build step reported **0 warnings / 0 errors**.
- Initial CI failures on early commits were resolved by excluding nested `Tools/**/*.cs` from the core's SDK glob and using Spectre.Console 0.57.2's supported confirmation API.
- Real manual terminal interaction and complete metadata-driven provider editing remain outside this successful first slice.

## 2026-10-25 — one-time startup clear and localization direction

- On interactive launch, the Configurator now clears its injected Spectre.Console terminal exactly once before any UI output. Headless `validate` is untouched; FileBrowser navigation does not clear again.
- Added a Spectre.Console.Testing assertion for the initial ANSI clear-screen sequence and its single occurrence.
- Upcoming **design to discuss**: a standalone Configurator with its own per-user application preferences, separate from edited `UnderstandMeSettings.json`; initial manually maintained bundled languages `en` and `cs` and offline English fallback; self-localization through UnderstandMe later. No mandatory command-line parameters: main menu Create, Edit, Test, Configurator Settings and Exit. Optional version checks and downloadable language packs belong to later releases.
- These menu and localization features are **not implemented by this change**. See [Localization and Preferences](Docs/Localization-and-Preferences/en.md).

### Startup clear verified in CI (2026-10-25)

- [GitHub Actions 40230127035](https://github.com/afrowaveltd/Toolbox/actions/runs/40230127035) **SUCCESS** for implementation commit `f46b80d9`. The Configurator's startup-clear test passed and all **8 Configurator tests** succeeded; full solution test suite and final build step succeeded with **0 warnings and 0 errors**. No headless-validate behavior was changed.

## 2026-10-25 — standalone bilingual launcher and preferences (verified)

- Added a parameterless first-level menu: **Create / Edit / Test (offline) / Configurator Settings / Exit**. Interactive actions are selected by enum identity rather than translated strings. Optional direct `--config` and headless `validate` remain available.
- Created `ConfiguratorPreferencesStore`, a **Configurator-only** per-user JSON preferences store, independent of host `UnderstandMeSettings.json`, with `Language: en|cs`, safe same-directory replacement, original SHA-256 revision checking, fail-closed handling of invalid preferences and English fallback for interactive startup.
- Added manually maintained embedded `Resources/en.json` and `Resources/cs.json`, `ConfiguratorStrings` with English offline bootstrap fallback, and translated the consumer-provided Blackboard FileBrowser labels without modifying the published FileBrowser.
- Main menu Create/Edit/Test work over the existing `ConfiguratorSession` and `UnderstandMeSettingsJsonStore`. Test is **offline structural validation only**, not network connectivity. Core validation message strings remain English until core exposes structured/localizable diagnostics.
- **No new shared NuGet library was created**: share only specific identical functions with future AfuTrans, if/when a common capability exists.
- [GitHub Actions #40231883535](https://github.com/afrowaveltd/Toolbox/actions/runs/40231883535) **SUCCESS**, including **14 Configurator tests** and the entire solution test workflow (final build 0 warnings/errors). Previous menu commit CI failures were from tests assuming the old initial menu; they were updated to exercise the new navigation paths.
- Next: review UX on real Debian/Windows terminals; add metadata-generated storage/provider editor and structured validation diagnostics; discuss live test design separately; avoid prematurely implementing network update/language-pack features.

### Final verification — 2026-10-25

- [GitHub Actions #40232402628](https://github.com/afrowaveltd/Toolbox/actions/runs/40232402628) **SUCCESS** on commit `40cb24e7`. All 14 Configurator tests and every other solution test suite passed (6,217 tests total across the workflow); both build steps completed with **0 warnings, 0 errors**. This supersedes intermediate CI runs in this implementation session.
- Parameterless localized launcher and per-user language selection are implemented and documented. Real terminal smoke on Debian/Windows, provider forms, structured core diagnostics, language downloads and online update checks remain future work.

## 2026-10-25 — clean menu navigation and acknowledged results

- Changed `ConfiguratorApp` to clear the Spectre.Console screen at startup, on every return to the main menu, and on every editor menu refresh, eliminating accumulated completed-operation output. No screen clear is performed during FileBrowser directory navigation or noninteractive `validate`.
- Added a bilingual `en`/`cs` Return to menu result acknowledgement. Readable summaries, test/validation results, invalid data errors and save outcomes stay onscreen until Enter/Esc; then the next menu clears the display. Routine successful scalar edits refresh their screen without a separate acknowledgement.
- Added regression coverage for canceling file browsing, returning from preferences, editor summary acknowledgement with ANSI clear position, malformed JSON diagnostics and localized menu/status behavior. The old 14 Configurator tests expanded to **18**.
- [GitHub Actions #40233060410](https://github.com/afrowaveltd/Toolbox/actions/runs/40233060410) **SUCCESS**, 18/18 Configurator tests and the complete Toolbox test workflow passed. Both build steps reported 0 warnings and 0 errors.
- Next: manual interactive Debian/Windows/SSH smoke; full provider/storage descriptor-based forms remain the next functional increment.

## 2026-10-25 — descriptor-based provider/storage editor + optional live checks

- Added `ConfiguratorSession.Components.cs` using the **existing core** `SettingsDescriptorRegistry`: add/remove providers and storages, apply schema defaults, edit typed options, provider Enabled and storage SourceLanguage/SetValueFromKey. Complete-draft dirty tracking detects all component changes; unchanged core revision-safe Save remains authoritative.
- Added TUI menus for provider/storage CRUD and all built-in descriptor fields, with Czech/English resource keys. Secret-reference values are redacted in displays and not used as visible prompt defaults. No additional NuGet shared package or Spectre dependency in UnderstandMe core.
- Added an explicit **advisory** LibreTranslate live check in the Test configuration flow and against the editor's unsaved draft: bounded HTTP GET `/languages`, 2xx + arbitrary valid JSON counts as endpoint responding. No credential or translation data transmitted; failures do not block saving. Disabled/unsupported registrations do not receive requests. No live checks at startup or Save.
- Added deterministic xUnit tests covering schema-default registration, typed validation, conditional secret references, duplicate IDs, deletion, revision conflicts, offline non-mutating live diagnostics and virtual-console navigation.
- Documentation: [Provider/Storage Editor](Docs/Provider-Storage-Editor/en.md), [Live Connection Diagnostics](Docs/Live-Connection-Diagnostics/en.md). Live translation/authentication test and real Debian/Windows/SSH smoke remain future increments.
- **CI verification in progress for the latest integration commit.** Do not claim a verified test count until a completed successful workflow has been inspected.
