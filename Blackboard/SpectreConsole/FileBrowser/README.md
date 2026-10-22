# FileBrowser — future reusable Spectre.Console file picker

**Status:** early implementation: .NET 10 component and test projects exist, with tested-in-CI file-type classification and icon-selection APIs; interactive Spectre.Console navigation, storage adapters and NuGet publication remain pending.

Inspired by the author's original [Lutonet/SpectreConsoleFileBrowser](https://github.com/Lutonet/SpectreConsoleFileBrowser).

**Planned NuGet ID:** `Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser`. This must be an independently buildable, testable and versionable library, without references to other Blackboard widgets, UnderstandMe, demo applications or unnecessary GUI dependencies.

This README serves the component project; [Implementation Status](IMPLEMENTATION_STATUS.md) tracks implemented code, verification and remaining navigation work.

The first reusable C# slice provides `FileBrowserFileIcons` with built-in mappings for text, code, JSON/configuration, images, audio, video, archives, PDF, office files and more. Decorative icons honor the `Auto`/`On`/`Off` terminal policy; names and paths are never changed. [File-type Icons](Docs/File-Type-Icons/en.md).

Planned navigation contract:

- Accept a starting directory, selection mode (existing file / existing directory / candidate new file), optional file-name/extension filter, a configurable page size, and injected UI labels.
- Return a typed outcome: selected absolute path or explicit cancellation, with recoverable error diagnostics rather than an arbitrary path sentinel.
- Navigate roots and parent directories, Windows drive roots and Unix mounts without calling `Directory.SetCurrentDirectory`.
- Filter `UnderstandMeSettings.json` or `*.json` when used by the UnderstandMe Configurator, without hardcoding any application filename into the generic picker.
- Display breadcrumb/current folder, accessible keyboard shortcuts and terminal-width aware selections; escape filenames before rendering Spectre markup.
- Handle inaccessible/disappearing directories and symlinks; directory creation is optional and disabled by default for safe config selection.
- Preserve manual-label override support with built-in English defaults, allowing consumers to localize without mandatory application dependencies.

Validation, backup and saving of a settings file stay outside the picker. No file operation is allowed merely by browsing, apart from explicit optional directory creation when enabled and confirmed.

Planned tests include Windows/Linux navigation, Unicode and markup characters in names, empty directories, file filters, permissions, cancel, nonexistent paths, key interception over SSH, and verification that the process working directory remains unchanged.

See [Blackboard architecture](../../Docs/Architecture/en.md) and the [UnderstandMe file picker analysis](../../../UnderstandMe/Docs/Configuration-File-Browser/en.md). A future Blackboard `Samples` application may demonstrate this package, but will never be included in its dependencies.

## Planned multi-source navigation (FileBrowser 2.0)

The **base package contains local filesystem browsing only**; future FTP/FTPS support will live in a separate opt-in adapter package (proposed `Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser.Ftp`). Source-qualified location/selection results will prevent remote paths being mistaken for local paths. A small source interface allows the same TUI to browse different storage types without adding dependencies for unused protocols. On .NET 10 use `OperatingSystem.IsWindows()` as needed rather than the old OS-name substring check. Browsing must not mutate process CWD, and network listings should remain read-only, cancellable and secure-by-default. [Source provider design](Docs/Storage-Providers/en.md).

## Automatic icon/Unicode compatibility

The original Lutonet browser exposed `DisplayIcons` (default `true`). The modern component is planned to use **Auto / On / Off** icon policy with **Auto as default**. Spectre.Console's detected Unicode capability informs the default; uncertain/legacy/redirected terminals receive plain text instead of broken decorative glyphs. A Unicode-capable host is not guaranteed to render emoji, so Auto prefers conservative Unicode symbols and lets users force richer icons. Always keep meaningful navigation text, preserve Unicode filenames internally, and never globally change the console encoding. See [Terminal Compatibility](Docs/Terminal-Compatibility/en.md).

## Legacy console encoding support

Beyond icons and Unicode glyph support, the browser should show **detected input/output encodings** and allow an explicit host-configured encoding override for unusual legacy terminals. The regular default is Auto/UTF-8 where supported. The browser itself never changes the process-wide console code page. Where selected encodings cannot display a filename, render an ASCII-safe escaped label without altering its actual path. See [Encoding Support](Docs/Encoding-Support/en.md).
