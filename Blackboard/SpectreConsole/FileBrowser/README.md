# FileBrowser — future reusable Spectre.Console file picker

**Status:** design placeholder, not implemented.

Inspired by the author's original [Lutonet/SpectreConsoleFileBrowser](https://github.com/Lutonet/SpectreConsoleFileBrowser).

Planned reusable contract:

- Accept a starting directory, selection mode (existing file / existing directory / candidate new file), optional file-name/extension filter, a configurable page size, and injected UI labels.
- Return a typed outcome: selected absolute path or explicit cancellation, with recoverable error diagnostics rather than an arbitrary path sentinel.
- Navigate roots and parent directories, Windows drive roots and Unix mounts without calling `Directory.SetCurrentDirectory`.
- Filter `UnderstandMeSettings.json` or `*.json` when used by the UnderstandMe Configurator, without hardcoding any application filename into the generic picker.
- Display breadcrumb/current folder, accessible keyboard shortcuts and terminal-width aware selections; escape filenames before rendering Spectre markup.
- Handle inaccessible/disappearing directories and symlinks; directory creation is optional and disabled by default for safe config selection.
- Preserve manual-label override support with built-in English defaults, allowing consumers to localize without mandatory application dependencies.

Validation, backup and saving of a settings file stay outside the picker. No file operation is allowed merely by browsing, apart from explicit optional directory creation when enabled and confirmed.

Planned tests include Windows/Linux navigation, Unicode and markup characters in names, empty directories, file filters, permissions, cancel, nonexistent paths, key interception over SSH, and verification that the process working directory remains unchanged.

See [Blackboard architecture](../../Docs/Architecture/en.md) and the [UnderstandMe file picker analysis](../../../UnderstandMe/Docs/Configuration-File-Browser/en.md).
