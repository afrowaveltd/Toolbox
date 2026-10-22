# Configuration file discovery and Spectre.Console file picker

**Status:** inspected source and approved integration direction (2026-10-22). No picker/runtime change implemented in this step.

## Reviewed reference

Public repository: https://github.com/Lutonet/SpectreConsoleFileBrowser

- Inspected `FileBrowser.cs`, `FileBrowser.csproj`, `README.md` and `LICENSE.txt` from `master`, commit `568f7f360a91b043d1a9e2313510eac2c5b22402`.
- Single ~9.7 kB `FileBrowser.cs` defines `FileBrowser.Browser` using Spectre.Console `SelectionPrompt<string>`, configurable page size (15), optional icons, Windows drive selection, parent navigation, directory creation, and file/directory selection. The methods are `GetPath`, `GetFilePath` and `GetFolderPath`, returning selected paths.
- Basic **manually configurable UI text properties** already exist (e.g. `SelectFileText`, `LevelUpText`, `MoreChoicesText`); no integrated localization service, resource catalog or locale selection was found in the inspected code. Some errors remain hard-coded in English.
- `FileBrowser.csproj` targets `netstandard2.0` and references `Spectre.Console 0.43.1-preview.0.43`. Last code push on the public repository was 2022-10-07, so do not assume compatibility with a modern Spectre.Console package without rebuilding and testing.
- Repository declares MIT in the project metadata and GitHub license metadata; the `LICENSE.txt` body still includes template placeholders `[year] [fullname]`. Before copying substantial source, resolve proper author/copyright attribution and preserve applicable license notices. A fresh implementation of the browsing behavior can avoid importing old source and dependencies.

## Technical risks in the current implementation

1. `Directory.SetCurrentDirectory(ActualFolder)` mutates the **whole process's current working directory**, which can break path resolution in a running host or editor. The picker must operate only on local absolute path state.
2. Folder/file enumeration is not filtered; `GetFilePath` offers all files, not just `UnderstandMeSettings.json` / `*.json`. `Directory.GetFiles` and some path operations lack targeted error handling. A directory can disappear or become inaccessible mid-navigation.
3. `AnsiConsole.Clear()` replaces the screen on each iteration and the picker owns the global console, potentially destroying the parent editor's footer/state.
4. UI labels embed raw filesystem names into Spectre markup; reserved markup characters in filenames can corrupt rendering. Render untrusted path labels as escaped or plain text.
5. Magic sentinel strings (`/////`, `///new`) model actions as fake paths; replace with typed navigation actions so unusual paths cannot collide with command selection.
6. Existing async methods do not await any asynchronous operations; no cancellation/abort result, rollback or explicit distinction between existing-file selection and new-file creation.
7. Drive selection is Windows-specific; on Linux/macOS navigate from the filesystem root and mounts. File/dir selection must not silently return an inaccessible, nonexistent or invalid JSON config.
8. No current configurable shortcut integration, searchable directory listing, file-type filters or meaningful test seams. Avoid relying on a specific old Spectre.Console version.

## Proposed UnderstandMe approach

Keep `UnderstandMeSettingsJsonStore` as the single authority for **loading, validating and saving** configuration. The picker is a separate TUI-only layer (optional Spectre.Console dependency), never imported into the core library. Do not couple it to the physical location of the executable.

Proposed flow:
- `understandme config [--config <path>]`: explicit argument, when provided, takes precedence. Otherwise look at a documented, bounded set of candidate project paths (working directory and project ancestors, host-designated config path, user config location), showing matching `UnderstandMeSettings.json` files for confirmation; avoid unbounded recursive filesystem searches.
- If no unique candidate is chosen, open **Browse for configuration** starting from the most relevant directory. Navigate parents, volumes/mounts, and filter `UnderstandMeSettings.json` first with optional `*.json` view. Provide manual path entry as an optional escape hatch, **not** the mandatory workflow.
- After selection, call `UnderstandMeSettingsJsonStore.Load(selectedPath, descriptors)`; unsupported schema or invalid settings produce a clear recovery screen without rewriting the file. Support a **Create New Configuration** action when no file exists, using validated defaults and explicit final save.
- Keep the selected absolute path and SHA-256 loaded revision in the in-RAM draft. The picker does not itself edit or save any settings. `Ctrl+V` validates the draft, `Ctrl+S` reviews changes, confirms, revalidates and writes via shared persistence.
- File chooser exposes typed results (`Selected(path)`, `Cancelled`, optionally `CreateNew(path)`), never a magic string path. Esc cancels selection; keyboard guidance stays visible. Handle SSH disconnect and terminal resize.
- Expose string resource keys/default English texts so UnderstandMe can localize the picker **offline**, with a built-in English fallback even if backend configuration is invalid.

Tests when implemented: Linux paths and symlinks; Windows drives; paths with spaces/brackets/non-Latin names; inaccessible/deleted directories; empty folders; JSON filters; no file selected; cancellation; unknown schema; duplicate candidate selection; direct `--config`; no implicit write/cwd mutation; no secrets/markup injection; concurrent on-disk changes; narrow terminal and SSH interruption.

## Recommendation

**Reuse the UX and configuration-string approach, not the unmodified 2022 implementation.** Implement the modernized picker as a small independent Spectre.Console component inside the Configurator project, and keep path discovery and JSON persistence outside it. Do not introduce a dependency on the old prerelease package or on Spectre.Console in the UnderstandMe core.

See [Configurator TUI](../Configurator-TUI/en.md) and [Central Configuration](../Central-Configuration/en.md).

## Shared Blackboard home (2026-10-22)

The reusable FileBrowser is now planned for [Blackboard/SpectreConsole/FileBrowser](../../../Blackboard/SpectreConsole/FileBrowser/README.md), independently of any particular consumer. The standalone UnderstandMe Configurator (suggested under `Toolroom/UnderstandMe/Configurator`) will use it through a picker contract and then call `UnderstandMeSettingsJsonStore`. `Blackboard` is a documentation/scaffold-only project family at this stage; no picker has been implemented.
