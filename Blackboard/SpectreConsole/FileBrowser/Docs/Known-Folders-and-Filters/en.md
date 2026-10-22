# Known folder icons, one-space file indentation and file type filters

**Status:** .NET 10 classification, filtering and plain-label helpers implemented 2026-10-22; GitHub CI coverage follows the implementation commits. The interactive Spectre.Console picker is **not yet implemented**.

## Why

A file picker can distinguish familiar *locations* as well as file types. Folder entries stay visually to the left of file entries, and a JSON-only filter helps a configuration editor without hardcoding that editor into the reusable picker.

## Well-known local folder roles

New API: `FileBrowserKnownFolders(platform, userHomeDirectory)`, `Classify(absolutePath, isDirectory)`, `GetIcon(kind, mode, capabilities)`, `GetLabel(kind)`, and `ForCurrentPlatform()`.

Only an **exact full local path** can be treated as a known folder, not its basename.

| Platform | Recognized path | Meaning |
| --- | --- | --- |
| Linux | `/home` | User profiles |
| Linux | `/etc` | System configuration |
| Linux | `/var/log` | System logs |
| Linux | `/tmp` | Temporary files |
| macOS | `/Users` | User profiles |
| macOS | `/Applications` | Applications |
| All | Supplied absolute local user-home directory | User home |
| Windows | Parent `X:\Users` derived from user home `X:\Users\name` | User profiles |

Note that Windows normally uses a drive-qualified path such as `C:\Users`, while `/Users` is typical on macOS. Never hardcode the Windows drive letter; use the supplied real user profile. Linux/macOS path matching is case-sensitive and Windows comparison case-insensitive.

A random directory named `home`, `etc` or `Users` somewhere else does not inherit a special icon. Relative paths, traversal segments (`..`) and misleading lookalikes (`/home2`, `C:\Other\Users`) are rejected. The helper does **not** access the disk, resolve symlinks, follow mounts or inspect contents. It knows local platform conventions: remote FTP/SFTP providers must not treat their paths as automatically local.

A regular folder icon is the fallback. All special-folder icons honor the already implemented `Auto / On / Off` policy; labels are English defaults which applications can localize without requiring UnderstandMe.

## One additional space for files

`FileBrowserEntryPresentation.Format(name, isDirectory, icons, mode, terminal, knownFolder)` creates an ordinary text label:

```text
📁 config/
 🧩 appsettings.json
 🧩 translation.json
 📃 README.md
🏠 alice/
 🎵 track.flac
```

Directories have no leading space; **files have precisely one additional ASCII space**. Directory names receive an optional visual trailing slash. That slash and indentation are **not part of the actual path**.

With icons disabled, navigation remains readable:

```text
config/
 appsettings.json
 translation.json
 README.md
```

This API returns plain text, **not Spectre markup**. The eventual renderer should create a `Text` object or escape markup and select by stable entry ID, never display text. Terminal control characters in untrusted names are escaped for display without modifying the original name. Rich emoji may occupy multiple screen cells; terminal-aware column measurement remains the renderer's job.

## JSON, type and extension filters

`FileBrowserFileFilter` supports:
- `includedKinds`, for example `[FileBrowserFileKind.Json]`;
- `includedExtensions`, for example `[".json", ".jsonc"]` or compound suffix `.tar.gz`;
- optional caller-supplied `FileBrowserFileIcons` classifier for custom file types;
- `includeDirectories = true` by default, so navigating into subfolders still works when files are filtered.

Categories are OR-combined within their list, as are extensions. Providing both lists combines them with AND. A null list imposes no constraint; an explicitly empty list matches no files. All matching is case-insensitive for extensions and performs **no file reads, MIME sniffing or downloads**.

```csharp
using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;

var filter = new FileBrowserFileFilter(
    includedKinds: [FileBrowserFileKind.Json]);

var showConfig = filter.Allows("UnderstandMeSettings.json", false); // true
var showFolder = filter.Allows("settings", true); // true
var hideReadme = filter.Allows("README.md", false); // false

var folders = FileBrowserKnownFolders.ForCurrentPlatform();
var role = folders.Classify("/home", true); // UserProfiles on Linux
```

Later, the interactive browser can offer `All / JSON / Text / Image / Audio` presets, alongside extension lists; the library helpers already provide the underlying selection policy. The generic package remains ignorant of UnderstandMe's specific configuration filename.

## Implemented/tested boundaries

These helpers live in the standalone FileBrowser library and have no dependency on Spectre.Console, FTP or a GUI toolkit. They never change process-wide current directory or console encoding. Tests exercise Linux/Windows/macOS path recognition, false positives, case rules, unusual drive letters, Unicode names, Auto/On/Off icons, filtering including compound extensions and folders left navigable, one-space indentation, and escaped control characters.

**Not implemented yet:** actual interactive directory navigation, search/filter controls, keyboard shortcuts, active capability probing, filesystem enumeration and FTP/FTPS connection.

See [File-type Icons](../File-Type-Icons/en.md), [Terminal Compatibility](../Terminal-Compatibility/en.md) and [Storage Providers](../Storage-Providers/en.md).
