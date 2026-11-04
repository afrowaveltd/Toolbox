# Afrowave.Toolbox — documentation index

This is the main documentation entry point for **the entire Toolbox**, not only Essentials. The localized English root [README](../../README/en.md) follows the primary repository [README](../../README.md) with adjusted relative links. Implemented code, published packages and future designs are explicitly distinguished. Each topic starts with an English `Docs/<Topic>/en.md`.

## Begin here

1. [Toolbox overview](../Toolbox-Overview/en.md) — actual libraries, executables, demos and future areas
2. [Repository getting started](../Toolbox-Getting-Started/en.md) — install, build and run examples
3. [Architecture](../Architecture/en.md) — ownership and dependency direction
4. [Development and verification](../Development-and-Verification/en.md) — testing, docs, GitHub and CI
5. [Release and package status](../Release-Status/en.md) — confirmed NuGet packages vs unpublished source projects
6. [NuGet-first preview release policy](../NuGet-Release-Policy/en.md) — publish useful components early, version safely and update external consumers

## Libraries, tools and project documentation

| Project | Documentation |
|---|---|
| **Essentials** | [README](../../Essentials/README.md) · [Overview](../Essentials-Overview/en.md) · [Getting started](../Getting-Started/en.md) |
| **WhenItFails** | [README](../../WhenItFails/README.md) · [Getting started](../../WhenItFails/Docs/Getting-Started/en.md) |
| **UnderstandMe** | [README](../../UnderstandMe/README.md) · [Offline localization](../../UnderstandMe/Docs/Localization/First-Implementation/en.md) |
| **UnderstandMe Configurator** | [README](../../UnderstandMe/Tools/Configurator/README.md) · [Offline help](../../UnderstandMe/Tools/Configurator/Docs/Offline-Help/en.md) |
| **Blackboard** | [Family overview](../../Blackboard/README.md) · [Architecture](../../Blackboard/Docs/Architecture/en.md) |
| **FileBrowser** | [README](../../Blackboard/SpectreConsole/FileBrowser/README.md) · [Interactive browser](../../Blackboard/SpectreConsole/FileBrowser/Docs/Interactive-Browser/en.md) |
| **Blackboard Markdown** | [README](../../Blackboard/SpectreConsole/Markdown/README.md) · [Themes](../../Blackboard/SpectreConsole/Markdown/Docs/Rendering-and-Themes/en.md) · [Nested lists](../../Blackboard/SpectreConsole/Markdown/Docs/Nested-List-Presentation/en.md) · [Loose list display](../../Blackboard/SpectreConsole/Markdown/Docs/Loose-List-Presentation/en.md) |
| **ExchangeHouse** | [Family overview](../../ExchangeHouse/README.md) · [Conversion roadmap](../../ExchangeHouse/Docs/Converters/en.md) |
| **ExchangeHouse Markdown** | [README](../../ExchangeHouse/Markdown/README.md) · [Links/images](../../ExchangeHouse/Markdown/Docs/Images-and-Links/en.md) · [Nested lists](../../ExchangeHouse/Markdown/Docs/Nested-Lists/en.md) · [Loose list paragraphs](../../ExchangeHouse/Markdown/Docs/Loose-List-Paragraphs/en.md) · [Incremental source scanner](../../ExchangeHouse/Markdown/Docs/Incremental-Source-Scanner/en.md) |
| **Showcase** | [README](../../Blackboard/Samples/Blackboard.Showcase/README.md) · [Theme gallery](../../Blackboard/Samples/Blackboard.Showcase/Docs/Markdown-Themes/en.md) |\n| **mdview** | [README](../../Blackboard/Tools/MdView/README.md) · [End-of-document navigation](../../Blackboard/Tools/MdView/Docs/Document-End-Navigation/en.md) · [Future indexed search (design)](../../Blackboard/Tools/MdView/Docs/Indexed-Search/en.md) |
| **SeeMe** | [README](../../SeeMe/README.md) · [Console adapter](../../SeeMe/WhenItFails/Console/README.md) |
| **Toolroom** | [README](../../Toolroom/README.md) · [Setter](../../Toolroom/WhenItFails/Setter/README.md) |

## Essentials reference documentation

These existing pages cover **Essentials specifically**, not the public API of every Toolbox library:

- [Results and Responses](../Results-and-Responses/en.md)
- [Issues](../Issues/en.md)
- [Metadata](../Metadata/en.md)
- [Diagnostics](../Diagnostics/en.md)
- [Extensions](../Extensions/en.md)
- [Interfaces](../Interfaces/en.md)
- [Enums](../Enums/en.md)
- [Guards](../Guards/en.md)
- [Value Objects](../Value-Objects/en.md)
- [Essentials API Reference](../API-Reference/en.md)

## Architecture and future designs

- [Afrowave design philosophy](../Design-Philosophy/en.md): explore useful ideas before combining overlapping mechanisms into a small, composable core; share cross-language semantics through idiomatic APIs.
- [AJIS / A2 design notes](../../Ajis/README.md) — design, no AJIS 2 runtime in this solution.
- [Id rebuild](../Id-Rebuild-Review/en.md) · [High availability](../Id-High-Availability-and-Replication/en.md) · [Observer/Uplink](../Id-Observer-and-Uplink/en.md) · [Regional routing](../Id-Autodiscovery-and-Regional-Routing/en.md) — design/reference material only.
- [Community and transparent operating funds](../Afrowave-Community-Access-and-Transparent-Funding/en.md) — policy/planning, not implemented financial infrastructure.

## Maintenance and release trail

[Root implementation status](../../IMPLEMENTATION_STATUS.md) · [Release notes](../../RELEASE_NOTES.md) · [GitHub repository](https://github.com/afrowaveltd/Toolbox)

Documentation should describe what the source and tests actually implement. Planned APIs must never be presented as shipped; record each relevant CI run in the owning project's status file.
