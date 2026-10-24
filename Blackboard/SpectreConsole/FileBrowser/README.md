# Afrowave Toolbox — Spectre.Console FileBrowser

Reusable, read-only, interactive file and directory picker for **.NET 10** applications using [Spectre.Console](https://spectreconsole.net/). Designed as an independent Blackboard TUI component: applications do not need to install UnderstandMe, Blackboard Showcase, or other Afrowave widgets.

**Package ID:** `Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser`  
**Current project version:** `0.1.0-preview.1` (preview; not a claim of publication on nuget.org)  
**Dependency:** `Spectre.Console 0.57.2`  
**Status:** implemented, automated tests passing in GitHub CI; manual cross-terminal compatibility verification and public NuGet publication remain outstanding.

## Installation

When published to your selected NuGet feed:

```bash
dotnet add package Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser --version 0.1.0-preview.1
```

Until publication, build a local package from this repository:

```bash
dotnet pack Blackboard/SpectreConsole/FileBrowser/Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser.csproj -c Release -o ./artifacts/nuget
```

Reference the resulting `.nupkg` from a configured local NuGet source, or reference the project directly while developing. The package is not the Showcase sample application.

## Quick start

Run in an **interactive terminal** (console input is required):

```csharp
using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;

var browser = new SpectreFileBrowser(new FileBrowserLocalSource());
var result = await browser.PickAsync(
    new FileBrowserLocation("local", Environment.CurrentDirectory),
    FileBrowserPickMode.File,
    new SpectreFileBrowserOptions
    {
        FileFilter = new FileBrowserFileFilter(
            nameMatcher: new FileBrowserNameMatcher("*.ajis;*.json")),
        PageSize = 12
    });

if (result.IsSelected)
{
    // Check SourceId before interpreting Value as a local filesystem path.
    Console.WriteLine(result.Location!.Value);
}
else
{
    Console.WriteLine("Selection cancelled.");
}
```

Select a directory by using `FileBrowserPickMode.Directory`. `FileBrowserPickResult` exposes `IsSelected`, `IsDirectory` and a provider-qualified `Location`; cancellation returns no location. `PickAsync` also accepts a cancellation token. An interactive console is required; a redirected/noninteractive terminal is not supported for selection.

### Filename filters

Use `FileBrowserNameMatcher` with `FileBrowserFileFilter`:

- `*.json` — JSON files.
- `*.ajis;*.json` — OR-combined masks.
- `*` — all filenames.
- `new FileBrowserNameMatcher(@"\.(ajis|json)$", FileBrowserPatternMode.RegularExpression)` — .NET regular expression.

Wildcards support `*` and `?`; regular expressions are opt-in and constrained by validation/timeouts. The picker also offers an **interactive filter menu** (all, JSON, text, images, masks or regex). Directories remain navigable by default when filtering files.

### Navigation and searching

- Local filesystem navigation without changing the process working directory.
- Existing-file or existing-directory selection and explicit cancellation.
- Parent navigation, lazy directory tree and bounded recursive search.
- Search results show selectable relative paths; a pruned result tree can be previewed. A search does not navigate until the user explicitly chooses a match.
- Source-qualified `FileBrowserLocation` and `IFileBrowserSource` support future independent storage adapters. The base package includes **only** `FileBrowserLocalSource`.
- File-type icons, known-folder presentation, configurable `Auto`/`On`/`Off` icon policy, and injectable English UI labels via `SpectreFileBrowserLabels`. Unicode names are preserved internally.

The picker does **not** parse or preview selected files. The optional [Blackboard Showcase](https://github.com/afrowaveltd/Toolbox/blob/master/Blackboard/Samples/Blackboard.Showcase/README.md) demonstrates safe, bounded text preview and separately highlights JSON with `Spectre.Console.Json`; JSON rendering is **not** a FileBrowser dependency.

## Examples and verification

```bash
dotnet run --project Blackboard/Samples/FileBrowser.Sample -- --filter "*.ajis;*.json"
dotnet run --project Blackboard/Samples/FileBrowser.Sample -- --regex "\\.(ajis|json)$"
dotnet run --project Blackboard/Samples/Blackboard.Showcase
dotnet test Blackboard/SpectreConsole/FileBrowser.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser.Tests.csproj
dotnet pack Blackboard/SpectreConsole/FileBrowser/Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser.csproj -c Release
```

See the runnable [FileBrowser.Sample](https://github.com/afrowaveltd/Toolbox/blob/master/Blackboard/Samples/FileBrowser.Sample/README.md) and [Blackboard.Showcase](https://github.com/afrowaveltd/Toolbox/blob/master/Blackboard/Samples/Blackboard.Showcase/README.md).

## Safety and current limits

Browsing itself does not modify files or directories, change global console encoding or change CWD. Directory links are not followed during recursive search by default; search caps, inaccessible directories and incomplete results are surfaced. File names are escaped for Spectre markup.

The current picker selects **existing** files and folders only: creating a new file, writing or uploading data, creating folders, and configuring FTP/FTPS/SFTP are **not implemented**. Legacy terminal encoding overrides, compatibility across Windows Terminal/cmd/Debian/SSH/tmux, and some Unicode/font scenarios still require real-terminal verification. The preview version is not a stability guarantee.

## Further documentation

- [Interactive Browser](https://github.com/afrowaveltd/Toolbox/blob/master/Blackboard/SpectreConsole/FileBrowser/Docs/Interactive-Browser/en.md)
- [Known Folders and Filters](https://github.com/afrowaveltd/Toolbox/blob/master/Blackboard/SpectreConsole/FileBrowser/Docs/Known-Folders-and-Filters/en.md)
- [Tree and Recursive Search](https://github.com/afrowaveltd/Toolbox/blob/master/Blackboard/SpectreConsole/FileBrowser/Docs/Tree-and-Recursive-Search/en.md)
- [File-type Icons](https://github.com/afrowaveltd/Toolbox/blob/master/Blackboard/SpectreConsole/FileBrowser/Docs/File-Type-Icons/en.md)
- [Terminal Compatibility](https://github.com/afrowaveltd/Toolbox/blob/master/Blackboard/SpectreConsole/FileBrowser/Docs/Terminal-Compatibility/en.md)
- [Storage Providers](https://github.com/afrowaveltd/Toolbox/blob/master/Blackboard/SpectreConsole/FileBrowser/Docs/Storage-Providers/en.md) (future connectors)
- [Encoding Support](https://github.com/afrowaveltd/Toolbox/blob/master/Blackboard/SpectreConsole/FileBrowser/Docs/Encoding-Support/en.md) (design/limitations)
- [Implementation Status](https://github.com/afrowaveltd/Toolbox/blob/master/Blackboard/SpectreConsole/FileBrowser/IMPLEMENTATION_STATUS.md)

Inspired by [Lutonet/SpectreConsoleFileBrowser](https://github.com/Lutonet/SpectreConsoleFileBrowser). Source: [Afrowave Toolbox](https://github.com/afrowaveltd/Toolbox).
