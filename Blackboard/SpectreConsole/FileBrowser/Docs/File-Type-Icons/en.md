# File-type icons and terminal-safe display

**Status:** classification and icon-selection API implemented 2026-10-22. Full interactive FileBrowser/Spectre.Console navigation remains pending.

## Motivation

FileBrowser should be visually useful as well as robust. Recognizing common file types makes a TUI feel closer to a GUI file browser: folders, text, source code, JSON, documents, PDF, spreadsheets, images, music, videos, archives and unknown files are visually distinguishable. The user's original Lutonet browser already had a `DisplayIcons` switch; the new implementation extends the idea while retaining capability fallbacks.

## Implemented API

- `FileBrowserFileKind` defines semantic file categories.
- `FileBrowserFileIcons.Classify(fileName, isDirectory)` classifies **entry names only**, not MIME headers or file contents; the caller's directory flag is authoritative. Unknown extensions fall back to `Unknown`.
- `FileBrowserFileIcons.GetIcon(kind, mode, capabilities)` returns an emoji, conservative Unicode symbol, or an empty string. `FileBrowserIconMode` supports `Auto`, `On` and `Off`; `FileBrowserTerminalCapabilities` holds observed Unicode, interactive and optionally confirmed emoji support.
- `FileBrowserFileIcons.GetLabel(kind)` supplies a small English type label for icon-free accessibility and for optional caller localization.
- Instance-level extension and exact-name overrides allow domain-specific classifications; e.g. `.abc` as audio, or `README` as text. The mapping is isolated per resolver and uses case-insensitive matching. Compound extensions are checked before shorter suffixes.
- Built-in mappings cover text/Markdown, source code/scripts, JSON/config, office documents, PDF, spreadsheets, presentations, images, audio, video, compressed archives, installers, databases and fonts. A folder is categorized as a folder even if its name ends in a file extension.

## Example

```csharp
using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;

var icons = new FileBrowserFileIcons();
var kind = icons.Classify("music.flac", isDirectory: false);

// The host supplies terminal capability hints, not hard-coded OS guesses.
var terminal = new FileBrowserTerminalCapabilities(
    SupportsUnicode: true, IsInteractive: true, SupportsEmoji: true);

var glyph = FileBrowserFileIcons.GetIcon(kind, FileBrowserIconMode.Auto, terminal);
// glyph == "🎵"; category label == "Audio"
var label = FileBrowserFileIcons.GetLabel(kind);
```

The future Spectre.Console picker will prepend the selected optional glyph to a **separately escaped file name**. File names and selected paths are not rewritten, even when decoration is omitted. Classification must not cause file I/O, request remote content, or inspect personal file contents.

## Terminal policy

| Mode | Behavior |
| --- | --- |
| Auto + noninteractive / no Unicode | No icon |
| Auto + Unicode (emoji unconfirmed) | Conservative narrow Unicode symbols |
| Auto + confirmed emoji support | Type-specific emoji |
| On | Force type-specific emoji even on unknown/legacy terminals |
| Off | Never display icons |

Unicode encoding does **not** imply an emoji-capable font. `Auto` therefore refuses to guess emoji support from UTF-8 alone. A future terminal diagnostics screen can let users explicitly confirm icons. English type labels remain available independently of decorations.

## Package and implementation boundaries

The standalone package `Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser` and its xUnit project are now registered in `Toolbox.sln`. At this stage the package's C# code covers **icon classification and terminal icon selection only**; no Spectre.Console widget or dependency has yet been introduced. The functionality is intentionally independent of Spectre and FTP so it can be tested before rendering and storage adapters exist. NuGet **publication** has not occurred.

## Test coverage

- Mixed-case and compound extensions, exact filenames, folders whose names resemble files, unknown/hidden names.
- Per-instance extension/filename overrides; invalid extension or kind errors.
- Distinct icons for audio/folder and other types; optional emoji vs conservative Unicode vs ASCII/no icons.
- Accessible labels and unchanged Unicode names and process working directory.
- Future integration tests must add actual Spectre rendering, markup safety, output encoding, terminal width, SSH sessions, and filenames unsupported by the output font.

See [Terminal Compatibility](../Terminal-Compatibility/en.md), [Encoding Support](../Encoding-Support/en.md) and [Overview](../Overview/en.md).
