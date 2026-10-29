# Blackboard — Implementation Status

## 2026-10-22 — Reserved project family, architecture only

- Established **Blackboard** as proposed reusable TUI widget family, pairing with the prospective GUI **Whiteboard** family (name not frozen).
- Confirmed planned first adapter `SpectreConsole`, starting with reusable `FileBrowser` based on ideas from the author's earlier Lutonet file picker.
- Documented boundaries with `Toolroom`, `SeeMe`, `UnderstandMe` and future Avalonia components. Specific settings editing belongs in a Toolroom application; widgets remain application-independent.
- Created [README](README.md), [architecture document](Docs/Architecture/en.md) and [component outline](SpectreConsole/FileBrowser/README.md).

**Current status:** no `.csproj`, C# implementation, NuGet artifact, tests or solution registration in this step. Names/package boundaries are open for refinement before runtime implementation.

### Next suggested work

1. Confirm naming (Blackboard/Whiteboard pair).
2. Implement framework-independent file navigation and a Spectre.Console adapter with modern stable package dependencies; avoid process-CWD changes.
3. Add CLI/SSH-friendly cancellation, filtering, safe markup, localization injection and error handling.
4. Add automated tests and include real projects in `Toolbox.sln`; check GitHub Actions.
5. Integrate the picker into the future UnderstandMe Configurator using the existing validated JSON settings store.

## 2026-10-22 — component-level NuGet and sample/application separation

- Confirmed Blackboard/Whiteboard are **project families**, not monolithic packages. Each independently useful TUI/GUI control gets a standalone .NET/NuGet project within its technology folder (e.g. `Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser`), with minimal dependencies, its own tests, README, Docs and `IMPLEMENTATION_STATUS.md`.
- Planned one or more future sample/showcase executables under each UI family, strictly outside published component dependency graphs. Production helper applications remain with their **own domain projects** (proposed UnderstandMe `UnderstandMe/Tools/Configurator`); keep existing Toolroom/WhenItFails tools unchanged.
- Added [Independent Packaging](Docs/Independent-Packaging/en.md), [FileBrowser overview](SpectreConsole/FileBrowser/Docs/Overview/en.md) and [per-component status](SpectreConsole/FileBrowser/IMPLEMENTATION_STATUS.md); reconciled root and UnderstandMe documentation.
- **Documentation only**: no Blackboard DLL/NuGet, samples, component tests or TUI executable implemented. CI verification of these commits is tracked separately.

## 2026-10-22 — FileBrowser 2.0 sources and optional FTP/FTPS (design)

- Planned a minimal source-qualified browsing contract and default **local-only** FileBrowser with modern .NET OS detection and no process CWD changes.
- FTP/FTPS will be an **optional separate NuGet extension**, not a dependency of basic navigation; SFTP is a distinct potential future adapter. Initial remote capability is read-only listing and selection. FTP insecure by default; recommend verified FTPS/TLS, externally resolved credentials and cancellable I/O.
- Authored [Storage Providers](SpectreConsole/FileBrowser/Docs/Storage-Providers/en.md), based on the user's experience with the original Lutonet package and public .NET/FTP ecosystem guidance. **No C# code, runtime FTP integration, package or tests implemented in this change.**

## 2026-10-22 — FileBrowser automatic icon fallback

- Reviewed Lutonet FileBrowser's existing `DisplayIcons` boolean; modernized replacement will support **Auto (default), On and Off** with capability-aware graceful fallbacks. Spectre.Console can detect Unicode, but cannot guarantee emoji glyph coverage; avoid mojibake and preserve real Unicode filenames while suppressing unsupported decorations.
- Published [Terminal Compatibility](SpectreConsole/FileBrowser/Docs/Terminal-Compatibility/en.md) and updated component README and status. **Design/documentation only**; FileBrowser runtime remains unimplemented.

## 2026-10-22 — FileBrowser encoding compatibility (design)

- Terminal fallback must address more than Unicode icons: distinguish host/terminal byte encodings (UTF-8 and legacy code pages), Unicode rendering and font glyphs. Proposed detection plus manual encoding override at the host boundary, avoiding implicit global `Console.OutputEncoding`/code page changes.
- Preserve original Unicode file identities even when ASCII-escaped labels are needed. Prepared [Encoding Support](SpectreConsole/FileBrowser/Docs/Encoding-Support/en.md), including optional CP852/CP1250/ISO-8859-2 and tests. **Design/documentation only.**

## 2026-10-22 — first Blackboard library code and tests

- Added independent `Blackboard/SpectreConsole/FileBrowser` .NET 10 library and sibling `FileBrowser.Tests` xUnit project to `Toolbox.sln`.
- Implemented file type classification, configurable custom filename/extension mappings, accessible type labels and capability-sensitive icons (emoji, narrow Unicode or no icons). [File-type Icons](SpectreConsole/FileBrowser/Docs/File-Type-Icons/en.md).
- GitHub Actions [39853748075](https://github.com/afrowaveltd/Toolbox/actions/runs/39853748075) passed, including **37 FileBrowser tests**; project README, test docs and both per-project implementation-status files added.
- **Not yet implemented**: actual interactive file/folder selection or Spectre.Console renderer, storage-source abstraction, terminal encoding integration, FTP/FTPS, example application and published NuGet. The new code is intentionally a small self-contained foundation.

## 2026-10-22 — directory tree and recursive multi-result navigation foundation

- Implemented storage-neutral read-only source types, initial local source, lazy tree expansion, bounded recursive file-name search, and a session that moves **only after explicit selection** of a result. Search and no-result/cancel leave the current folder unchanged.
- For multiple search matches, retained source-qualified results and directory breadcrumbs; the new presentation projection offers **all returned paths** (`FOLDER/SUBFOLDER/target.txt`, `ANOTHERFOLDER/target.txt`) and a pruned hierarchical tree with selectable match indices. Results limits and partial scans are explicit, not silently hidden.
- Added extensive xUnit coverage and [Tree and Recursive Search](SpectreConsole/FileBrowser/Docs/Tree-and-Recursive-Search/en.md). CI caught one test-name conflict and a mistaken wildcard test expectation; both were corrected in commits. **Latest green status is not claimed until the subsequent workflow succeeds.**
- Interactive Spectre.Console tree/results UI, paged choices, FTP source and full editor integration remain pending.

### CI verification

- GitHub Actions [39858309064](https://github.com/afrowaveltd/Toolbox/actions/runs/39858309064) **SUCCESS**, with **119 FileBrowser tests passed** and a green solution build. This proves the present library service/test slice, **not** an implemented keyboard-rendered browser or FTP adapter.

## 2026-10-24 — interactive FileBrowser and runnable showcase

- First functional Spectre.Console 0.57.2 FileBrowser adapter implemented as independent component, with keyboard-first file/folder selection, lazy tree, recursive search showing all collected relative paths and configurable source-neutral file filters. Rich icons are capability aware and have ASCII/no-icon fallback. The optional rendering package remains limited to this component.
- Custom listing/search criteria can be `*.ajis;*.json`, `*`, text/JSON/image categories, or an explicit bounded .NET regular expression. The same reusable matcher is used for filtering and searching. No separate FTP dependency introduced.
- Separate demonstration application under [Blackboard/Samples/FileBrowser.Sample](Samples/FileBrowser.Sample/README.md), intentionally not part of any component NuGet. The application is registered in `Toolbox.sln`.
- Added [Interactive Browser](SpectreConsole/FileBrowser/Docs/Interactive-Browser/en.md). Virtual Spectre.Console keyboard tests cover selection/cancellation/filtering, and GitHub Actions [40099094982](https://github.com/afrowaveltd/Toolbox/actions/runs/40099094982) confirmed **145 passing FileBrowser tests and green build**. Additional path-choice UI tests were committed later, with CI pending at time of this entry.
- **Pending:** manual Windows/SSH/legacy-terminal smoke, advanced output encoding handling, FTP/FTPS adapter, consumer-side UnderstandMe Configurator integration and NuGet release. No binaries published.

### Verified current build

- [GitHub Actions 40099273392](https://github.com/afrowaveltd/Toolbox/actions/runs/40099273392): **success, 147 FileBrowser tests, all solution projects built**. This supersedes the initial 145-case figure, and includes multi-match selected-path and no-result Esc tests.

### 2026-10-24 — verified 148-test interactive picker milestone

- [GitHub Actions 40099579962](https://github.com/afrowaveltd/Toolbox/actions/runs/40099579962) succeeded: **148 FileBrowser tests**, full solution build. Interactive TUI picker, universal mask/regex filename filters, lazy tree, multi-result recursive search and isolated sample app are implemented. Follow-up prevents selecting unreadable directories and recovers from regex timeouts during listing.
- No NuGet publication or live FTP/FTPS integration. Manual runtime verification in real Windows/Debian/SSH/tmux/legacy terminal remains the next release gate.

## 2026-10-24 — shared Blackboard Showcase test application

- Added reusable menu-based [Blackboard.Showcase](Samples/Blackboard.Showcase/README.md) application and independent [Showcase.Tests](Samples/Blackboard.Showcase.Tests/README.md), both included in `Toolbox.sln`. Modules use `IBlackboardShowcaseDemo`, making future tools easy to test without bundling their libraries.
- Initial demos: select any local file and show bounded readable text, select **only `*.json` files** and use `Spectre.Console.Json.JsonText` syntax highlighting for complete validated documents up to 64 KiB, and select a folder. The Spectre JSON renderer is a dependency of Showcase **only**, not FileBrowser.
- CI [40108814777](https://github.com/afrowaveltd/Toolbox/actions/runs/40108814777) green: **18 Showcase tests + 148 FileBrowser tests** with successful full-solution build. Fixed an initial BOM byte-array compile error in `b86378f2` before the green result.
- Still no published NuGet, no FTP/FTPS and no claims of real terminal compatibility beyond the CI virtual console tests. [Showcase documentation](Samples/Blackboard.Showcase/Docs/Showcase/en.md).

### 2026-10-24 — Showcase full scenario tests

- Added test coverage for every initial Showcase demo: general text preview, valid/invalid JSON syntax highlighting with JSON-only filter, directory selection and modular menu interactions.
- [GitHub Actions 40109040501](https://github.com/afrowaveltd/Toolbox/actions/runs/40109040501) **SUCCESS**: **20 Showcase + 148 FileBrowser tests** and green solution build. This supersedes the initial 18-Showcase-test figure.
- The shared Showcase has no NuGet publication and requires manual terminal smoke testing on the target Windows/Linux/SSH environments.

## 2026-10-25 — FileBrowser terminal verification and preview-release candidate

- The maintainer confirmed FileBrowser interactive tests/scenarios worked after updating on available Windows Terminal shell profiles (Command Prompt, PowerShell, Developer Command Prompt and Git shell) and then by **launching legacy `conhost.exe` directly**, including correct color output. This closes the specifically outstanding Windows legacy console smoke check; it is a manual result, not a new CI test.
- Earlier Debian smoke testing and FileBrowser's **160 locally reported passing tests** remain recorded in the component status. GitHub Actions [40121980599](https://github.com/afrowaveltd/Toolbox/actions/runs/40121980599) succeeded for the pre-documentation master.
- The independent FileBrowser `0.1.0-preview.1` (`net10.0`) is now assessed as a **preview-release candidate**. Public NuGet publication is **not completed**; release build/pack, package contents and a clean consumer installation still need verification. SSH/tmux and some font/legacy-code-page cases remain separate follow-up checks. Optional `netstandard2.0` compatibility is design-only.
- Updated the component README, Terminal Compatibility documentation and component/test/family status documents. No feature, API, source-code or automated-test modifications were introduced.

## 2026-10-25 — Release pack confirmed by maintainer

- The maintainer confirmed the FileBrowser project builds and that local Release NuGet packages were generated using `dotnet pack -c Release ...`.
- The preceding manual terminal verification documentation commit `d87edbce` passed [GitHub Actions 40181152070](https://github.com/afrowaveltd/Toolbox/actions/runs/40181152070). No additional automated tests were introduced by this update.
- FileBrowser is a **local-pack-verified preview candidate**, not a publicly published package. Generated `.nupkg` contents and a clean consumer installation remain to be checked. No library implementation changes.

## 2026-10-25 — standalone FileBrowser package consumer verified

- The maintainer successfully ran a separate `net10.0` console project on Linux/PowerShell 7 with a `PackageReference` to the local `0.1.0-preview.1` FileBrowser `.nupkg` instead of a Toolbox `ProjectReference`. The corrected package restore and interactive file-filter/directory selection workflow completed without errors.
- Documentation-only commit `d47522a7` passed [GitHub Actions 40183371359](https://github.com/afrowaveltd/Toolbox/actions/runs/40183371359).
- The separate-consumer NuGet smoke gate is now recorded as **PASS (maintainer report)**. Package contents/manifest inspection remains outstanding; nothing has been published to NuGet, and no source or automated tests were altered.

## 2026-10-25 — FileBrowser package audit green

- Maintainer-provided contents and NuGet manifest checks for `0.1.0-preview.1` show required .NET 10 assembly, XML documentation, README, license file, single `Spectre.Console 0.57.2` dependency and no bundled Showcase/tests; `unzip -t` found no archive errors.
- Previous [GitHub CI 40204020685](https://github.com/afrowaveltd/Toolbox/actions/runs/40204020685) passed. Release build, local package creation and independent consumer smoke were also reported PASS.
- Remaining final publication detail: **repack after current README/release-note updates** (the inspected archive bundled an older 7,009-byte README); recheck the final archive metadata and confirm the license mapping. No public upload yet. The tests and library runtime were not changed in this documentation commit.

## 2026-10-25 — first Blackboard NuGet package published

- [Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser 0.1.0-preview.1](https://www.nuget.org/packages/Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser/0.1.0-preview.1) is now **published on NuGet.org**, confirmed by the maintainer's package-page screenshot. Search indexing was still pending at the time of the screenshot.
- This is the first independently packaged/publicly distributed Blackboard control. Prior local package audits and a standalone `net10.0` consumer smoke test passed; remote NuGet.org restore remains to be checked after indexing.
- The uploaded package's README contains a historical “not yet published” statement, which **cannot be updated in place** for the same version. The GitHub README has been corrected; the next version will carry updated package-embedded documentation. No package/source API changes or speculative git release tag were made in this documentation update.

## 2026-10-25 — first UnderstandMe production consumer

- UnderstandMe Configurator is now an independent consumer of published Blackboard FileBrowser 0.1.0-preview.1 via PackageReference. File selection is intentionally read-only; all JSON settings edits belong to UnderstandMe's core store. CI pending for this integration.

### 2026-10-25 — published FileBrowser verified in first consumer CI

- [GitHub Actions 40229277472](https://github.com/afrowaveltd/Toolbox/actions/runs/40229277472) succeeded after the UnderstandMe Configurator integrated the published FileBrowser `0.1.0-preview.1` via PackageReference. Virtual-terminal JSON selection test passed, in addition to 160 FileBrowser tests. The Blackboard library itself was not modified by the integration.

## 2026-10-29 — independent Markdown renderer

- New library `Blackboard/SpectreConsole/Markdown` and `Markdown.Tests`, registered in solution. Uses ExchangeHouse.Markdown without adding UI dependencies to format/parser libraries.
- Configurable heading/inline/link/code styles; safe Spectre `Text` output, terminal controls sanitized. Existing Configurator Help unchanged.
- CI pending. Full CommonMark and NuGet publication remain future work. See [component status](SpectreConsole/Markdown/IMPLEMENTATION_STATUS.md).

### Verified CI — 2026-10-29

- [GitHub Actions #40851606376](https://github.com/afrowaveltd/Toolbox/actions/runs/40851606376) **SUCCESS** for commit `6f917928538f3e8342e9cd56f1b84be2feb8a104`: full solution restore, build and test passed, including Markdown renderer virtual-terminal tests.
- The first attempt used unsupported mutable `Spectre.Console.Text.Append`; the next build revealed one overly broad ANSI test assertion. Both were corrected in commits `6c754fa0` and `6f917928` respectively, then confirmed green. No manual Debian/Windows terminal validation or NuGet publication claimed.
