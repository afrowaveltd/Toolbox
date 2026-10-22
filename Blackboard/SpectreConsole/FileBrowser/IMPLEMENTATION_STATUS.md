# Blackboard SpectreConsole FileBrowser — Implementation Status

## 2026-10-22 — Separate NuGet component reserved

- Reserved `Blackboard/SpectreConsole/FileBrowser` as a **standalone** reusable file/folder picker project rather than part of an all-in-one Blackboard/Spectre.Console library.
- Proposed NuGet package: `Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser`. Its future `.csproj` should reference only required packages (such as Spectre.Console), with no dependency on UnderstandMe, other widgets, a sample app or the prospective Whiteboard GUI family.
- Core functional requirements: independent path navigation without changing process CWD, file/extension filtering, explicit cancel, typed result, SSH/Linux/Windows behavior, robust errors, localization via injectable text labels, safe markup and no implicit writes.
- See [component README](README.md), [component overview](Docs/Overview/en.md) and [family architecture](../../Docs/Architecture/en.md).

**Not implemented:** no C# source code, project file, tests, compiled package, demo or solution registration. Do not report this component as built or tested.

## Next step

Create the standalone .NET project and xUnit project, implement the testable navigation/picker with a supported Spectre.Console version, check GitHub CI, document the public APIs and update this status after every commit.

## 2026-10-22 — storage source modularity and platform detection (design)

- New [Storage Providers](Docs/Storage-Providers/en.md) design: source-qualified locations and a small async browse contract; local source bundled with picker and improved Windows/Unix detection using supported .NET APIs.
- Separate optional FTP/FTPS adapter package is proposed for read-only navigation, no implicit transfers, safe credential handling and FTPS as recommended default. No FTP dependency in the base component; SFTP is a different protocol and remains optional future work.
- Next implementation should prioritize small local browser, unit tests and package; remote connectors follow later. **Still documentation only, not implemented or tested.**

## 2026-10-22 — terminal Unicode/emoji fallback design

- Verified original Lutonet `FileBrowser.cs` already exposes `DisplayIcons = true`. Modern Spectre.Console exposes detected Unicode/legacy/interactive capabilities.
- Proposed **Auto/On/Off** display modes (default **Auto**): no decorative icons for non-Unicode, redirected or uncertain terminals; conservative narrow Unicode symbols for supported terminals; richer emoji only on explicit override. Unicode support **does not** imply glyph/font/emoji support.
- Avoid modifying global output encoding, code page or process console profile. Keep Unicode filenames intact; suppress *only decorative icons*. Planned tests for Windows Terminal/legacy hosts, Linux/SSH/tmux, fallback, user override, redirected output and inaccessible glyphs.
- Detailed specification: [Terminal Compatibility](Docs/Terminal-Compatibility/en.md). **No new C# code or CI tests added in this design commit.**

## 2026-10-22 — terminal character encoding policy (design)

- Extended the earlier icon/Unicode fallback proposal to distinguish **terminal byte encoding**, rendering support and font glyph coverage. Proposed Auto detection from current .NET console input/output encodings and renderer/locale hints; explicit application-level manual override (UTF-8, CP852, CP1250, ISO-8859-2 and supported others).
- Library must not silently change global code pages/Console encoding. Unsupported glyphs or unrepresentable filenames get safe ASCII display escapes, while real file paths remain lossless. Optional `System.Text.Encoding.CodePages` compatibility adapter only where needed.
- Added [Encoding Support](Docs/Encoding-Support/en.md) plus display diagnostics/testing plan. **Specification only; no new C# code or tests yet.**

## 2026-10-22 — first compiled FileBrowser API: file-type icons

- Implemented `FileBrowserFileKind`, `FileBrowserIconMode`, `FileBrowserTerminalCapabilities` and `FileBrowserFileIcons` in the **independently packageable** .NET 10 FileBrowser project. File types are recognized by case-insensitive file extension/exact name (including compound extensions); custom overrides are per instance. `Auto` uses plain output on noninteractive/non-Unicode terminals, conservative symbols where Unicode is supported and full emoji only when capability explicitly confirmed; `On` forces emoji and `Off` disables all glyphs. Accessible English labels remain available; Unicode filenames and current directory are never modified.
- Added xUnit project `FileBrowser.Tests` with **37 passing test cases**, registered both projects in `Toolbox.sln`. Verified GitHub Actions run [39853748075](https://github.com/afrowaveltd/Toolbox/actions/runs/39853748075) **SUCCESS** for commit `0be57bf9` (code plus test suite); source commits `c8f240d`, `35b30fc`, tests `da1fe6d`.
- Added [File-type Icons](Docs/File-Type-Icons/en.md) and test-project README/implementation status. **Scope limitation**: this is icon-selection logic, not a running Spectre.Console browser; the full file selection UI, OS detection, FTP modules and actual NuGet publication remain future work. No Spectre.Console dependency was introduced yet.
