# FileBrowser: independently packageable TUI file/folder picker

Status: **partial implementation**, 2026-10-22: classification/icon APIs plus xUnit tests exist; interactive file picker and FTP modules are still designs.

FileBrowser is now its own .NET 10 project under `Blackboard/SpectreConsole/FileBrowser`, intended for independent NuGet packaging. The first code slice is icon classification/selection; publication and the actual Spectre.Console picker have not happened. It is not the entire Blackboard or Spectre.Console toolkit.

## Implemented file-type icon foundation

`FileBrowserFileIcons` maps extensions and exact filenames to semantic categories and icon glyphs. Its output is chosen from emoji, small Unicode symbols or none according to terminal capability hints; it always offers English fallback labels and preserves original file paths. See [File-type Icons](../File-Type-Icons/en.md).

## Planned scope

- Allow applications to select files or folders, optionally filter by filename/extension, navigate parent directories and platform roots and cancel explicitly.
- Work inside SSH terminals and local consoles without changing the host process's working directory or overwriting application-specific state.
- Show navigation and help shortcuts, supply reliable English labels, and allow arbitrary clients to override text for localization.
- Return a typed selection/cancellation outcome; error handling belongs in the component while schema validation and file writes belong to the consumer.
- Reference only the UI/framework dependencies necessary for this picker. It must not reference UnderstandMe or other Blackboard controls.

## Consumers

The future UnderstandMe Configurator will use this package to select `UnderstandMeSettings.json`; it will own its validation and save workflow. Other programs can use the same picker without installing or invoking any translation code. Blackboard may later contain a separate `Samples` app showing the picker in action; it must not be a dependency of this package.

## Release requirements

The first actual source change should include a self-contained `.csproj`, automated tests, documentation of public APIs (English XML comments), the updated [implementation status](../IMPLEMENTATION_STATUS.md), plus integration in Toolbox CI. Validate behavior before declaring a release.

## Optional storage sources

FileBrowser 2.0 is planned around small pluggable storage sources. The base library includes local navigation; FTP/FTPS is a separate opt-in package, and SFTP is a distinct protocol and separate possible future adapter. Never make FTP libraries transitive dependencies of the local picker. Source-qualified results distinguish local paths from remote locations, and the consumer decides whether and how to open selected content. [Storage Providers](../Storage-Providers/en.md).

## Accessible icon fallback

The FileBrowser will support **Auto (default), On and Off** icon display. Unicode capability can be detected by Spectre.Console, but emoji font support is not reliably discoverable, so auto mode must use conservative glyphs or textual fallback. File/folder navigation and real Unicode path data must remain available even when decorative glyphs are hidden. See [Terminal Compatibility](../Terminal-Compatibility/en.md).

## Non-Unicode terminal encodings

Show the detected terminal input/output encoding and provide an explicit host-level override for CP852, Windows-1250 or ISO-8859-2, in addition to UTF-8. Avoid guessing from OS alone or globally changing codepages inside a reusable widget. The actual file selection always retains the original Unicode path even when a filename needs to be shown through ASCII escapes. See [Encoding Support](../Encoding-Support/en.md).

## File presentation and filters (implemented foundation)

`FileBrowserKnownFolders` can assign recognizable folder icons **only from exact local paths**, e.g. `/home` or actual Windows `X:\Users`. `FileBrowserEntryPresentation` adds exactly one leading ASCII space to file rows but not directory rows; no icons are required. `FileBrowserFileFilter` accepts type/extension filters, preserving directory traversal by default. These are reusable library APIs, not yet active controls in a Spectre.Console picker. [Known Folders and Filters](../Known-Folders-and-Filters/en.md).

## Directory tree and search data services

The initial `FileBrowserLocalSource` and generic `IFileBrowserSource`, `FileBrowserDirectoryTree` (lazy expansion), `FileBrowserRecursiveSearch` (`*` and `?` wildcard matching with safe traversal bounds), `FileBrowserSearchResultsPresentation` (all matches as relative paths or pruned tree), and `FileBrowserNavigationSession` (explicit navigation only) are implemented in the reusable library. The eventual interactive Spectre.Console picker will consume these services; it is not implemented yet. See [Tree and Recursive Search](../Tree-and-Recursive-Search/en.md).
