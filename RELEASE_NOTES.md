# Afrowave.Toolbox — release and project notes

For the complete up-to-date inventory, see [Release status](Docs/Release-Status/en.md) and [implementation status](IMPLEMENTATION_STATUS.md). Source `<Version>` and NuGet publication are separate facts.

## Public NuGet releases (maintainer-confirmed)

### Essentials 0.2.0

Reusable results/responses, metadata, diagnostics, guards, issues and value objects. Packaging includes an Essentials icon, README and license. [NuGet](https://www.nuget.org/packages/Afrowave.Toolbox.Essentials/0.2.0) · [Source](Essentials/README.md).

### WhenItFails 1.0.0

Stable error catalog/runtime and recovery surface with project-local schema-versioned JSON and validation. See [release checklist](WhenItFails/Docs/Release-Checklist/en.md), [NuGet](https://www.nuget.org/packages/Afrowave.Toolbox.WhenItFails/1.0.0), [source](WhenItFails/README.md).

### Blackboard Spectre.Console FileBrowser 0.1.0-preview.1

Interactive independent file and folder picker, with filtering and bounded search. This is a **prerelease** package. The NuGet archive contains historical README wording that cannot be changed without publishing a new version. [NuGet](https://www.nuget.org/packages/Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser/0.1.0-preview.1) · [Source](Blackboard/SpectreConsole/FileBrowser/README.md).

## In-repository components, not yet publicly released

- [ExchangeHouse.Markdown](ExchangeHouse/Markdown/README.md): source-preserving parser/writer with links, images and nested lists; `0.1.0-preview.1` source metadata.
- [Blackboard.SpectreConsole.Markdown](Blackboard/SpectreConsole/Markdown/README.md): themeable console renderer, also `0.1.0-preview.1` source metadata.
- [UnderstandMe](UnderstandMe/README.md), [Configurator](UnderstandMe/Tools/Configurator/README.md), [SeeMe](SeeMe/README.md) and [Toolroom](Toolroom/README.md) have independently tracked implementation milestones; do not infer a public NuGet release for them.

## Release validation

```bash
dotnet restore Toolbox.sln
dotnet build Toolbox.sln -c Release
dotnet test Toolbox.sln -c Release
```

A release additionally requires inspecting the exact `.nupkg` and its dependencies, documentation, license, source tag and independent consumer restore. Never overwrite an already-published immutable NuGet package ID/version. The latest verified cross-platform nested-list source CI is [#41244480220](https://github.com/afrowaveltd/Toolbox/actions/runs/41244480220).

## Portable application prerelease: MdView 0.1.0-preview.1 (2026-11-22)

**Published** as [GitHub prerelease](https://github.com/afrowaveltd/Toolbox/releases/tag/mdview-v0.1.0-preview.1), not NuGet.
This first .NET 10 terminal Markdown preview includes portable
self-contained Windows x64 ZIP and Linux x64 TAR.GZ builds,
plus SHA-256 files and MIT license. The release is tagged
`mdview-v0.1.0-preview.1` at commit
`c66859be5dbbfa9d3bf60135ab4a6cc16119cad8`.
Both packages passed per-platform focused tests, packaged
binary help/version/local render smoke and extracted
archive validation. Link navigation, local disk-backed
page previews and native table modes are available,
but advanced paging is explicitly opt-in and still experimental.

[Usage and known limitations](Blackboard/Tools/MdView/Docs/First-Preview-Release/en.md).

## MdView 0.1.0-preview.2 — official desktop icon (2026-11-22)

[Published GitHub prerelease](https://github.com/afrowaveltd/Toolbox/releases/tag/mdview-v0.1.0-preview.2), **not NuGet**,
from source commit
`b9c28a5a22b97cf20d7304817ac6e5a4a875a238`.
This preserves the Markdown CLI/TUI functionality from
preview.1 and integrates the original
`Blackboard/Tools/MdView/Assets/icon.png`.

- Windows x64 ZIP: standard Windows executable icon generated
  at build time as seven-size ICO (16–256 px), connected via
  MSBuild `ApplicationIcon` and checked in the published EXE.
- Linux x64 TAR.GZ: packaged original `icon.png` and optional
  root-free `install-desktop.sh`, which adds a personal
  Freedesktop `Terminal=true` menu launcher pointing at
  the unpacked binary/icon. No global installation occurs.
- Both archives contain SHA-256 checksums and license.
  **All 3 release jobs passed**, including 367/367 focused
  MdView xUnit tests per OS, publish/extracted-archive smoke
  tests, icon validation, and Linux launcher registration:
  https://github.com/afrowaveltd/Toolbox/actions/runs/45064168702

Old `preview.1` artifacts remain unchanged. Terminal-only
behavior and the production journal algorithm are unchanged.
No MSI or DEB installer is implied by this release.
[Preview.2 usage](Blackboard/Tools/MdView/Docs/Second-Preview-Release/en.md).
