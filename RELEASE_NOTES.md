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
