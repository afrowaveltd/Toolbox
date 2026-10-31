# Afrowave.Toolbox overview

Toolbox is a repository of **independently reusable .NET 10 libraries**, supporting test suites, optional demonstration executables, and prospective design material. Each independently useful component owns its documentation and packaging choices.

## Implemented library families

- [Essentials](../../Essentials/README.md): result/response models, issues, diagnostics, common interfaces, metadata and value objects.
- [WhenItFails](../../WhenItFails/README.md): structured and validated error catalogs, runtime error resolution and recovery.
- [UnderstandMe](../../UnderstandMe/README.md): offline localization core, dictionaryless registration, placeholder validation and JSON dictionary tooling; broader queues/providers/translations are still in design/increments.
- [Blackboard](../../Blackboard/README.md): TUI components, currently a published preview FileBrowser and an unpublished Spectre.Console Markdown renderer.
- [ExchangeHouse](../../ExchangeHouse/README.md): UI-independent formats; Markdown scanner, source-preserving writer, inline links/images and nested-list models with explicitly indented paragraph continuations exist; converters do not.
- [SeeMe](../../SeeMe/README.md): console presentation of WhenItFails data.
- [Toolroom](../../Toolroom/README.md): utility executables, especially [Setter](../../Toolroom/WhenItFails/Setter/README.md).

## Executables and demonstration projects

[Blackboard.Showcase](../../Blackboard/Samples/Blackboard.Showcase/README.md) shows independent FileBrowser and Markdown UI features; [FileBrowser.Sample](../../Blackboard/Samples/FileBrowser.Sample/README.md) is a minimal picker consumer. [UnderstandMe Configurator](../../UnderstandMe/Tools/Configurator/README.md) manages settings and offline help. Showcases are **examples**, not packages users must install for reusable components.

## Design-only items

[AJIS / A2](../../Ajis/README.md) is design material; the historic implementation is in the separate Ajis.Dotnet repository. Id, Observer and Uplink docs in the root Docs folder describe proposed systems. The future families GetConnected, Suitcase, SemTam/TamTam and TalkToMe are **not** hereby claimed as built Toolbox libraries.

## Publication vs source

Maintainer-confirmed NuGet publications: Essentials 0.2.0, WhenItFails 1.0.0, Blackboard FileBrowser 0.1.0-preview.1. The Markdown parser and renderer carry project preview versions but have not been reported as published. See [Release status](../Release-Status/en.md).

## Further reading

[Architecture](../Architecture/en.md) · [Getting started](../Toolbox-Getting-Started/en.md) · [Documentation index](../Documentation/en.md).

## 2026-10-31 — list continuation increment

ExchangeHouse Markdown now attaches explicitly indented paragraphs to list items with exact source offsets and original marker widths; Blackboard renders them beneath the owning item. The library boundaries remain unchanged. [Parser behavior](../../ExchangeHouse/Markdown/Docs/List-Continuations/en.md) · [Renderer behavior](../../Blackboard/SpectreConsole/Markdown/Docs/List-Continuation-Presentation/en.md) · [CI verification](https://github.com/afrowaveltd/Toolbox/actions/runs/41248289922). This is not full CommonMark or an external package publication.
