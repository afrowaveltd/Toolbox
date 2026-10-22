# Lazy folder tree and recursive search with all selectable matches

**Status: implemented storage-independent data services and tests, 2026-10-22.** Spectre.Console interactive tree/navigation, search prompt and result picker remain pending. The source APIs are part of the standalone FileBrowser library; no new NuGet release has been published.

## Intended user experience

From any current directory the user may choose:

- **Tree view:** expand/collapse folders on demand. The tree is **lazy**; browsing one branch must not recursively enumerate the entire local volume or an FTP site.
- **Recursive search:** enter an exact entry name such as `cohledame.txt`, or a simple wildcard pattern such as `*.json` or `config?.xml`. The default searches file names, not file content. The current folder is the search origin.
- **All matching results:** show every returned match, not merely the first one, with its **path relative to that origin**, and a compact **pruned tree** containing only relevant branches.
- **Explicit navigation:** the user selects a specific result with the keyboard and confirms; only then navigate to its parent directory and highlight the entry. Cancel, no matches and incomplete/failed searches leave the current location and highlighted entry unchanged.
- **Large result sets:** eventual TUI will scroll or page through results, indicating how many matches were returned. Since recursive traversal is deliberately bounded, it must also show `LimitReached` or `Partial` prominently and offer a way to rerun with higher limits/narrower filters; never misrepresent truncated results as exhaustive.

Suggested results for the pattern `cohledame.txt`:

```text
Search from: /start
Matches: 2

ANOTHERFOLDER/
  cohledame.txt
FOLDER/
  SUBFOLDER/
    cohledame.txt

Up/Down: choose result    Enter: open folder & highlight    Esc: cancel
```

Equivalent selectable flat path list (useful in small terminals or no-color mode):

```text
ANOTHERFOLDER/cohledame.txt
FOLDER/SUBFOLDER/cohledame.txt
```

The results are **paths reconstructed from traversed entry names**, not the opaque native path of a provider. Thus the presentation is valid for local and future FTP/SFTP sources without assuming a particular slash syntax.

## Implemented API

### Storage-neutral browsing

- `FileBrowserLocation(sourceId, value)` – opaque, source-qualified location; a remote location must not be passed to `System.IO.File`.
- `FileBrowserSourceEntry` – exact name, location, directory flag, and `CanTraverse` flag.
- `IFileBrowserSource` – asynchronous `ListAsync` and `GetParentAsync` plus `Id` and `IsCaseSensitive`. It is **read-only**.
- `FileBrowserLocalSource` – lists local files and directories, supports parent navigation, uses fully qualified paths and never calls `Directory.SetCurrentDirectory`. Directory symlinks/reparse points are marked non-traversable by recursive operations by default.
- `FileBrowserDirectoryTree.GetChildrenAsync` – lists immediate directory children only, for lazy tree expansion.

### Recursive search

- `FileBrowserRecursiveSearch.FindAsync(origin, pattern, options, cancellationToken)` – breadth-first traversal, one file name or `*`/`?` wildcard pattern, matching according to source case rules. A provider can mark links/untrusted directory references non-traversable.
- `FileBrowserSearchOptions` – limits depth, visited directory count, entries per directory and match count; optional filter by kind/extension and opt-in matching of directories.
- `FileBrowserSearchResult` – includes all collected matches, count of enumerated/unreadable directories, and completion status: `Completed`, `Partial` (some directories inaccessible), or `LimitReached` (bounded search was truncated). Cancelling propagates through the caller's cancellation token.
- `FileBrowserSearchMatch.RelativeDirectories` – retains ordered directory names traversed between origin and match parent; this avoids guessing provider-native path semantics.
- `FileBrowserSearchResultsPresentation.CreateRows` – returns **one selectable row for each returned match** with a relative path and the original index in the match list, even when rows are sorted.
- `FileBrowserSearchResultsPresentation.CreateTree` – groups the matched paths into a compact tree with containing branches and selectable leaves; no extra I/O.
- `FileBrowserNavigationSession.SearchAsync` – never changes current directory. `SelectSearchResult(result, index)` only moves to selected match's parent and highlights the selected entry after an explicit user action.

## Safety/performance principles

- Default search limits (initial): maximum 32 directory levels, 10,000 directories, 1,000 matches and 20,000 entries per directory. These are **safety caps, not maximum possible file counts**; a caller can increase limits within validated ranges. A result containing exactly the cap is conservatively reported as potentially incomplete.
- A visited-location set guards against duplicate/cyclic provider graph locations. Real symbolic links/reparse points are not followed by the local source's recursive traversal.
- Permission/IO errors on an individual directory are skipped and counted; the search status is `Partial`, not silently `Completed`. Unexpected implementation failures propagate. No partial on-disk updates.
- The search respects cancellation between listings and entries. A remote adapter must itself implement cancellable, time-bounded network listing. The current local filesystem source uses synchronous enumeration behind the `Task` API and does not claim hard cancellation during a blocking operating-system call.
- Filenames are kept exactly as supplied by the storage source; terminal control characters and embedded separators are **escaped for display only**. Never select an entry based on a potentially duplicated visible label; keep its source-qualified identity.
- The result tree contains **only branches leading to search matches**, never every entry in the searched directory.

## Example

```csharp
using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;

var source = new FileBrowserLocalSource();
var start = new FileBrowserLocation("local", "/home/example");
var session = new FileBrowserNavigationSession(source, start);

var result = await session.SearchAsync("*.json");
var rows = FileBrowserSearchResultsPresentation.CreateRows(
    result, isCaseSensitive: source.IsCaseSensitive);
var tree = FileBrowserSearchResultsPresentation.CreateTree(
    result, isCaseSensitive: source.IsCaseSensitive);

// Display rows/tree and require a user selection before navigating.
if (rows.Count > 0 && userConfirmedSelection)
{
    session.SelectSearchResult(result, rows[0].MatchIndex);
}
```

The `userConfirmedSelection` placeholder is application UI state, not an implemented prompt. The FileBrowser library does not currently have an interactive TUI search screen.

## Tests and next steps

The FileBrowser xUnit project covers source-qualified paths, lazy tree listing, repeated filenames in separate subdirectories, exact/wildcard searches, case sensitivity, depth/directory/match caps, cancellation, untraversable links/cycles, unreadable folders, JSON filters and explicit no-move/selected-move behavior. Separate tests verify local filesystem enumeration and parent navigation without process-CWD mutation.

Next implement Spectre.Console tree rendering and search modal/paged choice, with explicit progress/error/limit notices, SSH keyboard fallback and filename-markup escaping. Add real FTP/FTPS integration tests only when that optional adapter exists. Keep new rendering dependencies scoped to the independent FileBrowser NuGet.

See [Known Folders and Filters](../Known-Folders-and-Filters/en.md), [Storage Providers](../Storage-Providers/en.md) and [Terminal Compatibility](../Terminal-Compatibility/en.md).
