# Blackboard SpectreConsole FileBrowser — Implementation Status

## 2026-10-22 — Separate NuGet component reserved

- Reserved `Blackboard/SpectreConsole/FileBrowser` as a **standalone** reusable file/folder picker project rather than part of an all-in-one Blackboard/Spectre.Console library.
- Proposed NuGet package: `Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser`. Its future `.csproj` should reference only required packages (such as Spectre.Console), with no dependency on UnderstandMe, other widgets, a sample app or the prospective Whiteboard GUI family.
- Core functional requirements: independent path navigation without changing process CWD, file/extension filtering, explicit cancel, typed result, SSH/Linux/Windows behavior, robust errors, localization via injectable text labels, safe markup and no implicit writes.
- See [component README](README.md), [component overview](Docs/Overview/en.md) and [family architecture](../../Docs/Architecture/en.md).

**Not implemented:** no C# source code, project file, tests, compiled package, demo or solution registration. Do not report this component as built or tested.

## Next step

Create the standalone .NET project and xUnit project, implement the testable navigation/picker with a supported Spectre.Console version, check GitHub CI, document the public APIs and update this status after every commit.
