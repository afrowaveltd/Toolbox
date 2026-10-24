# Interactive FileBrowser: navigation, tree, search and universal filters

**Status:** first functional Spectre.Console adapter implemented 2026-10-24, using Spectre.Console 0.57.2 and the independent `Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser` .NET 10 package project. CLI demo built as a separate application under `Blackboard/Samples/FileBrowser.Sample`. Runtime keyboard usability in each platform still requires manual terminal smoke testing; CI compilation alone does not prove the rendered interface works on all hosts.

## Start it

From Toolbox root:

```bash
dotnet run --project Blackboard/Samples/FileBrowser.Sample -- .
dotnet run --project Blackboard/Samples/FileBrowser.Sample -- /home --directory
dotnet run --project Blackboard/Samples/FileBrowser.Sample -- --filter "*.ajis;*.json"
dotnet run --project Blackboard/Samples/FileBrowser.Sample -- --regex "\.(ajis|json)$"
dotnet run --project Blackboard/Samples/FileBrowser.Sample -- --icons-off
```

Starting folder defaults to the current working directory. The demo is never a dependency of the published FileBrowser component. Its stdout outputs the **selected original absolute path** after confirmed selection; cancellation yields a separate result.

## Interactive menu

- Browse immediate folders and files, with category icons when supported. Directory entries are visually one ASCII space farther left than files. Local known locations may have distinct folder icons. The browser does **not** change process working directory.
- Enter a folder by selecting it. Return to parent via the explicit menu choice, with Esc cancelling the menu.
- In directory selection mode, use "Select current folder"; in file mode select any visible file. The file is never opened or changed.
- Select **File filter**: All files (`*`), JSON, Text, Images, or input arbitrary filename masks (`*.ajis;*.json`) / explicit .NET regex. A custom filter is applied to filenames only; folders remain listed so navigation is always possible.
- Select **Recursive search**: choose simple masks (`cohledame.txt`, `*.json`, `*.ajis;*.json`) or regular expressions (`\.(ajis|json)$`). It searches below the currently selected folder, not full-text file contents.
- Search results are presented as a pruned tree preview when short enough, and as a **paged keyboard selection list containing every returned match**. Each row displays a relative path (`FOLDER/SUBFOLDER/file.json`) so identical names in different directories remain distinguishable. Confirming a result opens its parent directory and highlights its entry; cancellation or no match does not navigate.
- Select **Browse directory tree** for a lazy folder-only view. Select subdirectories to expand; "Open this folder in browser" navigates the main picker to that folder. This initial implementation presents one folder level at a time with full current path, rather than a persistently expanded multicOLUMN tree.
- Every main/submenu has a Back/Cancel choice, so terminal shortcuts intercepted by SSH/tmux are not required.

## Filename matcher contracts

`FileBrowserNameMatcher(pattern, mode, ignoreCase, regexTimeout)` implements shared matching for listing filters and recursive searches.

| Mode | Example | Meaning |
| --- | --- | --- |
| Wildcard | `*` | All filenames |
| Wildcard | `*.ajis` | All AJIS files |
| Wildcard | `*.ajis;*.json` | Either extension (semicolon is OR) |
| Wildcard | `dictionary?.json` | Exactly one arbitrary character at `?` |
| Wildcard | `*.json;*.jsonc;*.yaml` | Multiple possible formats |
| Regex | `\.(ajis|json)$` | File ends with one of the extensions |
| Regex | `^translation_.*\.json$` | Matches an exact name scheme |

- Glob syntax is only simple `*` / `?`, not regex, braces or full-path globbing. Semicolon delimits masks. No implicit path searching: slash/backslash is rejected in a wildcard filename mask.
- Regular expressions are **explicitly** selected in UI; they are not silently inferred. A .NET regular expression is applied to **one filename**, with a default 200-ms per-match timeout (configurable between 1 ms and 5 seconds) and a 2048-character input limit.
- Case sensitivity follows the backing storage adapter in interactive mode; caller-constructed matchers can override.
- Filesystem types (`FileBrowserFileKind.Json` etc.) and extension constraints from `FileBrowserFileFilter` can be AND-combined with this name matcher. Directory navigation is preserved by default.
- Wildcards and regex are filename filters, not file content or global filesystem searching, and never read file contents. Regex failures/timeouts in an interactive filter are reported and the filter is cleared, so users can recover.

## Search limits and no-move guarantee

- Recursive search uses limits on depth, visited directories, entries per listing and matches (defaults: 32, 10,000, 20,000, 1000). The status explicitly says `LimitReached` / `Partial` when search results might be incomplete; the UI displays that status rather than claiming an exhaustive result.
- Search, listing and tree use the provider-neutral `IFileBrowserSource`. Only the local provider is presently implemented. Directory symlinks and reparse points are not recursively traversed by default.
- A search cannot relocate the current folder merely by finding one or more files. Explicit user selection is the commit point; Cancel and no results preserve original location/selection.
- Noninteractive terminals are rejected rather than waiting forever for keyboard input. Host owns connection establishment, CLI arguments and external cancellation tokens; library never modifies encoding, CWD or storage.
- Filename display must escape Spectre markup/control characters, retaining original provider-qualified identity independent of the visible label.

## Test status and limitations

Previous data/service tests cover universal name masks, regex, listing filters, known folder icons, directory tree and recursive multi-match results. GitHub CI compiles the Spectre adapter and the independent sample. Actual interactive arrows/Enter/Esc, host code pages, terminal width, UTF-8/emoji metrics, Windows-specific behavior and SSH/tmux must still be manually exercised before a stable NuGet release.

The sample does not provide remote FTP/FTPS or choose newly created nonexistent filenames; these remain separately planned features. There is no runtime linkage to UnderstandMe yet. The eventual UnderstandMe Configurator may consume FileBrowser to choose dictionary/configuration paths, with actual JSON validation performed by its own settings store.

See [Storage Providers](../Storage-Providers/en.md), [Tree and Recursive Search](../Tree-and-Recursive-Search/en.md) and [Terminal Compatibility](../Terminal-Compatibility/en.md).
