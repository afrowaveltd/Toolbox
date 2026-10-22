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
