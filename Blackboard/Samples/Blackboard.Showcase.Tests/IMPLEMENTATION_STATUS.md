# Blackboard.Showcase.Tests — Implementation Status

## 2026-10-24 — initial automated tests

- Created an independent .NET 10 xUnit project referencing only the Showcase and the stable Spectre.Console.Testing runtime.
- Added tests for text previews (valid Unicode, binary, invalid UTF-8, unsafe controls, 16 KiB read bounding, empty), JSON previews (valid/invalid/oversize/binary/empty), and the modular menu/JSON-specific picker workflow via virtual-keyboard TestConsole.
- Registered the test project in Toolbox.sln with the other Blackboard and Toolbox projects.
- GitHub Actions result must be verified after these files are committed; the tests have not been manually validated in real terminals.

Next: confirm CI and add regression cases for any Windows/SSH differences that appear during live testing.

## 2026-10-24 — first 18 tests verified

- GitHub Actions [40108814777](https://github.com/afrowaveltd/Toolbox/actions/runs/40108814777) **SUCCESS**: all **18 Showcase tests** passed, all 148 FileBrowser tests stayed green, and the entire solution built successfully.
- Coverage includes modular menu navigation, JSON-only filtering and colored JSON preview, invalid JSON rejection, safe text encoding and control-character rules, 16 KiB preview bounds and the 64 KiB JSON ceiling. Real-terminal manual testing is still needed.

### 2026-10-24 — 20 tests fully verified

- Added two `Spectre.Console.Testing` tests of `FileBrowserPreviewDemo` and `FileBrowserDirectoryDemo`, including actual file selection via Enter, Unicode text output and no filesystem modification.
- [GitHub Actions 40109040501](https://github.com/afrowaveltd/Toolbox/actions/runs/40109040501): **SUCCESS**, **20 Showcase tests**, **148 FileBrowser tests** and successful full Toolbox solution build. All Showcase scenarios now have direct virtual-terminal coverage.
- Manual real Windows/SSH/legacy-terminal acceptance testing is still pending.
