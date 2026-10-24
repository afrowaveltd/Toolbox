# Blackboard.Showcase.Tests — Implementation Status

## 2026-10-24 — initial automated tests

- Created an independent .NET 10 xUnit project referencing only the Showcase and the stable Spectre.Console.Testing runtime.
- Added tests for text previews (valid Unicode, binary, invalid UTF-8, unsafe controls, 16 KiB read bounding, empty), JSON previews (valid/invalid/oversize/binary/empty), and the modular menu/JSON-specific picker workflow via virtual-keyboard TestConsole.
- Registered the test project in Toolbox.sln with the other Blackboard and Toolbox projects.
- GitHub Actions result must be verified after these files are committed; the tests have not been manually validated in real terminals.

Next: confirm CI and add regression cases for any Windows/SSH differences that appear during live testing.
