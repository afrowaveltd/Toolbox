# Blackboard SpectreConsole FileBrowser — Implementation Status

## 2026-10-22 — Separate NuGet component reserved

- Reserved `Blackboard/SpectreConsole/FileBrowser` as a **standalone** reusable file/folder picker project rather than part of an all-in-one Blackboard/Spectre.Console library.
- Proposed NuGet package: `Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser`. Its future `.csproj` should reference only required packages (such as Spectre.Console), with no dependency on UnderstandMe, other widgets, a sample app or the prospective Whiteboard GUI family.
- Core functional requirements: independent path navigation without changing process CWD, file/extension filtering, explicit cancel, typed result, SSH/Linux/Windows behavior, robust errors, localization via injectable text labels, safe markup and no implicit writes.
- See [component README](README.md), [component overview](Docs/Overview/en.md) and [family architecture](../../Docs/Architecture/en.md).

**Not implemented:** no C# source code, project file, tests, compiled package, demo or solution registration. Do not report this component as built or tested.

## Next step

Create the standalone .NET project and xUnit project, implement the testable navigation/picker with a supported Spectre.Console version, check GitHub CI, document the public APIs and update this status after every commit.

## 2026-10-22 — storage source modularity and platform detection (design)

- New [Storage Providers](Docs/Storage-Providers/en.md) design: source-qualified locations and a small async browse contract; local source bundled with picker and improved Windows/Unix detection using supported .NET APIs.
- Separate optional FTP/FTPS adapter package is proposed for read-only navigation, no implicit transfers, safe credential handling and FTPS as recommended default. No FTP dependency in the base component; SFTP is a different protocol and remains optional future work.
- Next implementation should prioritize small local browser, unit tests and package; remote connectors follow later. **Still documentation only, not implemented or tested.**

## 2026-10-22 — terminal Unicode/emoji fallback design

- Verified original Lutonet `FileBrowser.cs` already exposes `DisplayIcons = true`. Modern Spectre.Console exposes detected Unicode/legacy/interactive capabilities.
- Proposed **Auto/On/Off** display modes (default **Auto**): no decorative icons for non-Unicode, redirected or uncertain terminals; conservative narrow Unicode symbols for supported terminals; richer emoji only on explicit override. Unicode support **does not** imply glyph/font/emoji support.
- Avoid modifying global output encoding, code page or process console profile. Keep Unicode filenames intact; suppress *only decorative icons*. Planned tests for Windows Terminal/legacy hosts, Linux/SSH/tmux, fallback, user override, redirected output and inaccessible glyphs.
- Detailed specification: [Terminal Compatibility](Docs/Terminal-Compatibility/en.md). **No new C# code or CI tests added in this design commit.**

## 2026-10-22 — terminal character encoding policy (design)

- Extended the earlier icon/Unicode fallback proposal to distinguish **terminal byte encoding**, rendering support and font glyph coverage. Proposed Auto detection from current .NET console input/output encodings and renderer/locale hints; explicit application-level manual override (UTF-8, CP852, CP1250, ISO-8859-2 and supported others).
- Library must not silently change global code pages/Console encoding. Unsupported glyphs or unrepresentable filenames get safe ASCII display escapes, while real file paths remain lossless. Optional `System.Text.Encoding.CodePages` compatibility adapter only where needed.
- Added [Encoding Support](Docs/Encoding-Support/en.md) plus display diagnostics/testing plan. **Specification only; no new C# code or tests yet.**

## 2026-10-22 — first compiled FileBrowser API: file-type icons

- Implemented `FileBrowserFileKind`, `FileBrowserIconMode`, `FileBrowserTerminalCapabilities` and `FileBrowserFileIcons` in the **independently packageable** .NET 10 FileBrowser project. File types are recognized by case-insensitive file extension/exact name (including compound extensions); custom overrides are per instance. `Auto` uses plain output on noninteractive/non-Unicode terminals, conservative symbols where Unicode is supported and full emoji only when capability explicitly confirmed; `On` forces emoji and `Off` disables all glyphs. Accessible English labels remain available; Unicode filenames and current directory are never modified.
- Added xUnit project `FileBrowser.Tests` with **37 passing test cases**, registered both projects in `Toolbox.sln`. Verified GitHub Actions run [39853748075](https://github.com/afrowaveltd/Toolbox/actions/runs/39853748075) **SUCCESS** for commit `0be57bf9` (code plus test suite); source commits `c8f240d`, `35b30fc`, tests `da1fe6d`.
- Added [File-type Icons](Docs/File-Type-Icons/en.md) and test-project README/implementation status. **Scope limitation**: this is icon-selection logic, not a running Spectre.Console browser; the full file selection UI, OS detection, FTP modules and actual NuGet publication remain future work. No Spectre.Console dependency was introduced yet.

## 2026-10-22 — directory tree, bounded recursive search and multi-match path tree

- Added `FileBrowserLocation`, `FileBrowserSourceEntry` and the read-only `IFileBrowserSource` contract for storage-qualified, path-opaque navigation (`9ba005e`). Implemented `FileBrowserLocalSource` (`8ab9d75`), `FileBrowserDirectoryTree` lazy child expansion (`7ae9b33`), and `FileBrowserRecursiveSearch` with exact name/`*`/`?` matching, provider-aware case rules, bounded breadth-first traversal, cancellation, directory/link-cycle safeguards, partial/limit statuses and **explicit** match selection through `FileBrowserNavigationSession` (`44788f7`).
- Further implemented per-match search-relative directory breadcrumbs (`949f8d2`), `FileBrowserSearchResultsPresentation.CreateRows` returning a relative path for **every returned match** with stable original match indices, and `CreateTree` returning a pruned directory tree containing only branches leading to matched entries (`a8a54a2`). The UI must show all returned matches and permit picking among them. **Nothing navigates when there is no result or the user cancels.**
- xUnit `FileBrowserTreeSearchTests` cover fake-source trees and actual local temp files, duplicate names in separate folders, explicit navigation, unchanged location on miss, wildcard/case matching, filters, inaccessible folders, cycle prevention, cancellation and safety caps. Fixed a conflicting test helper (`d97c957`) and corrected the `?` wildcard test expectation (`cc1997a`) after CI exposed the issues.
- [Tree and Recursive Search](Docs/Tree-and-Recursive-Search/en.md) details semantics and the intended search-result picker. **Only data/backend logic exists**: no implemented interactive Spectre.Console tree/results modal, keyboard selection/paging, FTP/FTPS adapter, published NuGet or download/upload support. GitHub Actions re-run after final test correction is required before claiming all tests green.

### CI verification for search/tree implementation

- GitHub Actions [39858309064](https://github.com/afrowaveltd/Toolbox/actions/runs/39858309064) **SUCCESS** for commit `cc1997a` after correcting the two test defects: build succeeded and **119 FileBrowser tests passed**. Other solution test projects also passed. Subsequent documentation changes do not alter compiled behavior; verify their own workflow status separately.

## 2026-10-24 — first interactive Spectre.Console picker and universal filename patterns

- Added the stable `Spectre.Console 0.57.2` dependency **only** to the standalone FileBrowser component (`18acb1a`). Created `SpectreFileBrowser`, `SpectreFileBrowserOptions`, `SpectreFileBrowserLabels`, `FileBrowserPickMode` and typed `FileBrowserPickResult` (`f340e9a`). It uses the previously implemented local source, type-aware icons and one-character file indentation, keyboard menus, parent navigation, existing file/folder selection and explicit cancellation, without changing CWD or touching files.
- File filtering is **not restricted to JSON or predefined categories**. Implemented `FileBrowserNameMatcher` (`b6c9f64`) with unlimited-choice wildcard masks within a bounded 64-mask list (example `*.ajis;*.json`), `*` for all, and opt-in .NET regex with validated syntax, bounded input length and timeout. Integrated the same matcher with existing type/extension filters (`b5ac2fd`) and recursive search (`cdf0482`); application/CLI may override source case sensitivity.
- Interactive File filter menu now offers All, JSON, Text, Images, custom mask lists or regex. Recursive search accepts mask lists or regex, displays every collected hit with a relative path and source identity, previews the matched-directory pruned tree and navigates **only after** explicit selection. Tree navigation is lazy and rendered with Spectre.Console `Tree`, using `TreeGuide.Ascii` when Unicode is unavailable (`7a8387f`).
- Added separate `Blackboard/Samples/FileBrowser.Sample` runnable .NET 10 executable (`beafb1b`..`499fbe0`), registered in `Toolbox.sln` (`1d987dc`); this is not published or bundled with the independent NuGet. Try: `dotnet run --project Blackboard/Samples/FileBrowser.Sample -- --filter "*.ajis;*.json"` or `--regex "\.(ajis|json)$"`.
- Added tests for wildcard OR masks, exact/regex matching, case rules, invalid patterns, timeout bounds, folder-preserving filters and recursive search, plus `Spectre.Console.Testing 0.57.2` virtual keyboard tests for Enter/ESC and filter (`98e38cf`, `a685bc7`, `dd0d93e`). Verified [GitHub Actions run 40099094982](https://github.com/afrowaveltd/Toolbox/actions/runs/40099094982) **SUCCESS with 145 FileBrowser tests and green full solution build**. Follow-up tests of interactive multi-match selection and no-hit cancellation (`6821dba`) were added; check their own CI run before reporting totals.
- [Interactive Browser](Docs/Interactive-Browser/en.md) documents the implemented UI, presets and custom masks/regex, limits and usage.

**Remaining:** real/manual terminal testing on Windows Terminal, cmd/legacy console, Debian, SSH/tmux, narrow terminal, unsupported encodings and font glyphs; no FTP/FTPS provider, remote config editor integration, comprehensive ANSI/code-page fallback or stable NuGet publication. The current picker selects **existing** locations only. No claim of full production readiness.

### Follow-up interactive path-choice CI verification

- GitHub Actions [40099273392](https://github.com/afrowaveltd/Toolbox/actions/runs/40099273392) **SUCCESS**, full solution build green and **147 FileBrowser tests passed**. This includes actual `Spectre.Console.Testing` keyboard-sequence tests proving a multi-result search displays `A/target.json` and `B/target.json` and selects the explicitly chosen **second** match, plus that a failed search followed by Esc does not select or navigate.
- This supersedes the earlier count of 145 tests, not the still-pending manual tests on physical/remote terminals.

### 2026-10-24 — unreadable-folder handling, final verification

- Fixed the interactive directory-mode picker so it does not offer “Select current folder” if listing the current directory failed. On regular-expression timeout in a listing filter, the picker clears the filter and **rebuilds the entire listing** instead of leaving a half-filtered result (`9285d1c`).
- Added `SpectreFileBrowserInteractionTests.Cannot_select_unreadable_current_directory` (`6622b4e`). GitHub Actions [40099579962](https://github.com/afrowaveltd/Toolbox/actions/runs/40099579962) **SUCCESS**: build and all existing solution test projects green; **148 FileBrowser tests passed**. This supersedes earlier counts of 145 and 147.
- Manual cross-platform keyboard/terminal testing and NuGet publication remain outstanding; FTP/FTPS remains future optional connector work.

## 2026-10-24 — NuGet documentation and metadata audit

- Rewrote component README to describe the **implemented** interactive .NET 10 picker rather than a future-only design, including local package build, installation *after publication*, minimal C# usage, typed cancellation, filter masks/regex, navigation, search, showcase and current limitations.
- Added NuGet-friendly absolute links to detailed English documentation and corrected the per-project test command. Showcase JSON preview remains clearly outside the independent FileBrowser package.
- Updated project NuGet description, tags, project URL and preview release notes to reflect the actual capabilities without implying stable release or NuGet publication. Kept version `0.1.0-preview.1` and dependency `Spectre.Console 0.57.2` unchanged.
- Documentation/metadata commits: `08cdfbb`, `187e3e2`, `4cd3b4b`. Previous verified result remains **148 FileBrowser tests**, GitHub Actions run `40099579962`. These documentation/metadata changes have **not** been independently compiled or packaged in this session: verify CI and inspect a generated `.nupkg` before publishing.

**Next release gates:** run `dotnet test Blackboard/SpectreConsole/FileBrowser.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser.Tests.csproj`, `dotnet pack Blackboard/SpectreConsole/FileBrowser/Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser.csproj -c Release`, inspect NuGet metadata/readme/license/dependency entries, and perform real-terminal smoke tests on target hosts. No claim of public publication.

## 2026-10-24 — optional UnderstandMe localization adapter proposed (design only)

- Captured an optional `UnderstandingFileBrowser` adapter that maps existing `SpectreFileBrowserLabels` to the host application's existing UnderstandMe context, without coupling the base FileBrowser to the translation system.
- Preserve DictionaryLess fallback/registration, safe prompt-boundary updates, English fallback and optional separate offline dictionary packages. No implementation, new NuGet package or runtime tests were added.
- Design: [UnderstandMe Localization Adapter](Docs/UnderstandMe-Localization-Adapter/en.md) (`40409e8`). Priorities remain manual cross-terminal FileBrowser verification and the UnderstandMe configuration workflow.

## 2026-10-24 — Linux Auto icon detection after Debian manual smoke test

- User reported Debian terminal navigation, file selection and Showcase text preview working; `--icons-on` displayed rich icons, while default `Auto` displayed none.
- Added `FileBrowserTerminalDetection` to supplement a false-negative Spectre.Console Unicode flag **only** in a real interactive Linux/Unix UTF-8 terminal. Safety exclusions include noninteractive or redirected output, `TERM=dumb`, Windows hosts, and non-UTF-8/unknown Unix locales. Virtual/test consoles preserve their injected Spectre profile rather than using the host process environment.
- `Auto` continues to select conservative Unicode symbols rather than emoji; forced `On` remains available for terminals with known emoji fonts. File paths and console encoding are untouched.
- Added focused xUnit tests for common Linux UTF-8 locales, rejected contexts and conservative icon selection.
- Commits: `ce48a03`, `688e906`, `4ac433c`. **CI and post-fix manual Debian verification are pending**; do not claim this is proven until they pass. Manual Windows CMD/PowerShell/Windows Terminal checks are still pending.

### 2026-10-24 — Debian smoke test feedback

- User's Debian terminal screenshot after pulling the Linux Auto detection fix shows the FileBrowser running with conservative Unicode `▸` directory icons in the default mode, confirming the expected visible result in that environment.
- User ran the FileBrowser test project: **160 passed, 0 failed, 0 skipped**, .NET 10 build successful. One xUnit2006 warning from an unrelated existing assertion was observed.
- Replaced the redundant `Assert.Equal(originalName, name)` with an explicit ordinal equality condition to silence xUnit2006 (`045c11a`); **this follow-up change still needs a fresh build/test run**.
- Windows CMD/PowerShell/Windows Terminal smoke testing remains pending.

## 2026-10-24 — lowest-compatible framework strategy (design, pending build)

- User requested the reusable FileBrowser library target the lowest practical .NET standard, rather than inherit .NET 10 merely from Toolbox. Keep test executables/Showcase on net10.0.
- NuGet Spectre.Console 0.57.2 supplies netstandard2.0 assets, so a portable FileBrowser library is plausible. Reviewed actual code and found newer BCL calls (OperatingSystem.IsWindows/IsLinux/IsMacOS, Path.IsPathFullyQualified, ArgumentNullException.ThrowIfNull, generic Enum.IsDefined, span/string overloads) requiring compatible replacements; records/init may need support attributes.
- Added [Target Framework Compatibility](Docs/Target-Framework-Compatibility/en.md) with migration audit and explicit net48 consumer, pack and terminal verification gates (`62103b9`).
- **No target framework change was committed yet**: no local .NET SDK is available in this tool environment, so declaring netstandard2.0 without compilation would risk breaking the proven project. Existing FileBrowser remains net10.0 until compilation and regression tests pass.

## 2026-10-25 — manual Windows terminal matrix completed

- Following updates, the maintainer reported that the FileBrowser tests/scenarios worked correctly in the available Windows terminal profiles: Command Prompt, PowerShell, Developer Command Prompt and Git shell (hosted in Windows Terminal).
- The legacy **Windows Console Host** was initially absent from the Windows Terminal profile choices. The maintainer then directly launched `conhost.exe` and confirmed **PASS**, including correct color rendering. The Windows Terminal-hosted `cmd.exe` profile and direct `conhost.exe` are distinct test contexts; both are covered by the report.
- Previous Debian manual smoke verification (navigation and Auto Unicode symbol rendering) remains valid. **SSH/tmux, narrow console, unsupported fonts and legacy code pages are not included in this new report.**
- CI provenance: [GitHub Actions run 40121980599](https://github.com/afrowaveltd/Toolbox/actions/runs/40121980599) succeeded on master commit `85ac731e`. The previous recorded manual unit-test count was **160 passing FileBrowser tests**; this documentation update adds no automated tests and makes no claim of a new test run.
- **Readiness:** the independently packable `0.1.0-preview.1` FileBrowser foundation is a **preview-release candidate**. No NuGet release has been published. Before publication, execute Release build/test/pack, inspect package contents/metadata and test installation from the generated package in a clean consumer. The potential `netstandard2.0` migration remains a separately gated future change: current library target is `net10.0`.
- Documentation updated: component README and [Terminal Compatibility](Docs/Terminal-Compatibility/en.md). No runtime code or functional behavior was changed by this documentation work.

## 2026-10-25 — maintainer confirms Release build and local NuGet pack

- The maintainer reported a **successful local project build** and that local NuGet packages were generated using `dotnet pack -c Release ...` after the Windows/Debian terminal smoke checks. This is an **externally reported local verification**, not a build executed by the documentation author. No package file, path, contents, or clean-consumer installation was provided for independent inspection.
- The documentation-only terminal verification commit `d87edbce` passed [GitHub Actions 40181152070](https://github.com/afrowaveltd/Toolbox/actions/runs/40181152070) (SUCCESS). This is CI evidence independent of the local pack report; do not infer that an installable NuGet package was downloaded and exercised from CI.
- **Release gates now remaining:** inspect the generated `.nupkg` (target framework, NuGet metadata, README, license and package dependencies), install it from a local feed in a clean independent consumer and execute a basic file/folder selection smoke test. Public NuGet publication has **not** been performed. Current source `net10.0` and `0.1.0-preview.1` are unchanged; `netstandard2.0` is a separate future proposal.
- Documentation/status-only milestone; no source, API, or automated tests were changed.

## 2026-10-25 — independent local NuGet consumer smoke PASS (maintainer report)

- The maintainer confirmed that the Linux/PowerShell 7 **independent smoke consumer** workflow finished without errors after correcting Windows-only temporary-directory assumptions in the initial commands.
- A temporary **.NET 10 console application outside the Toolbox project graph** used the existing `Blackboard/Samples/FileBrowser.Sample/Program.cs` application source and a **NuGet PackageReference** to local package `Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser` version `0.1.0-preview.1`, restored from the local package directory and nuget.org. This tests package-based consumption instead of the original `ProjectReference`.
- The maintainer reported the full corrected test workflow passed, including the interactive file filter invocation (`--filter "*.ajis;*.json"`) and directory-picker invocation (`--directory`). These are **user-reported manual results**, not a test run performed by GitHub CI or this documentation update.
- The previous documentation commit `d47522a7` has green [GitHub Actions 40183371359](https://github.com/afrowaveltd/Toolbox/actions/runs/40183371359).
- **Remaining before a public preview release:** examine the actual generated `.nupkg` package contents/manifest (README, license, framework, dependency metadata) and make an explicit publication decision. Additional SSH/tmux, legacy encoding/font and `netstandard2.0` compatibility checks are separate follow-up work and are not implied to be verified.
- No runtime/source/test-code change or NuGet publication in this commit.
