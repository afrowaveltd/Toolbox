# Afrowave.Toolbox architecture and dependency boundaries

## Principle: many small packages, not one large dependency

Toolbox is a collection of component projects and support tools. Folders such as Blackboard, ExchangeHouse, Toolroom and SeeMe organize responsibilities; no umbrella `Afrowave.Toolbox` runtime assembly is required. Components are designed to be usable without the demo applications or future Afutrans server.

## Implemented responsibility boundaries

| Layer | Owns | Must not own |
|---|---|---|
| Essentials | Results/responses, interfaces, diagnostics, metadata | Terminal UI, application workflows |
| WhenItFails | Error catalog runtime and recovery | Consumer screens and app-specific UX |
| ExchangeHouse.Markdown | Source text, syntax classification, inline links/images, list nesting, unchanged-source writing | Spectre.Console, translation execution, network fetches |
| Blackboard.SpectreConsole.Markdown | Terminal rendering, themes and safe image alt display | Markdown document mutations, translation, link downloads |
| Blackboard.SpectreConsole.FileBrowser | File/folder browsing, filtering and selection | Configurator-specific settings, Showcase rendering |
| UnderstandMe | Offline localization/dictionary operations; future translation contracts | FileBrowser internals, universal terminal renderer |
| SeeMe | Reusable console display of WhenItFails results | Error catalog ownership |
| Toolroom | Executable authoring/maintenance workflows | Unrelated low-level model ownership |

## Concrete reuse

```text
ExchangeHouse.Markdown
    -> Blackboard.SpectreConsole.Markdown
           -> Blackboard.Showcase
           -> UnderstandMe Configurator (offline Help)

Blackboard.SpectreConsole.FileBrowser
    -> Blackboard.Showcase / FileBrowser.Sample
    -> UnderstandMe Configurator

Essentials -> WhenItFails -> SeeMe / Setter
```

These arrows indicate conceptual dependency/consumption, not a claim that each project uses a direct `ProjectReference`. Some components are obtained through NuGet. The source `.csproj` files are authoritative for concrete dependencies.

## Source preservation and compatibility

ExchangeHouse currently offers a conservative Markdown subset. Flat `Blocks` and a list hierarchy coexist; source and original line endings are preserved by unchanged writing. Complex CommonMark continuation paragraphs, task lists, HTML, reference links and cross-format conversions remain future work. Spectre displays image alt text instead of reading or downloading assets. UnderstandMe's future translation-aware document processing will protect addresses/identifiers and validate translated output separately.

## Families and packages

Each independently released component gets a separate project, README, topic documentation, tests and implementation status. The Blackboard FileBrowser has a public preview; Blackboard Markdown and ExchangeHouse Markdown are still in-repository preview projects. Do not infer a public NuGet release merely from a version in a project file.

## Future architecture

GetConnected, Suitcase, SemTam/TamTam, TalkToMe, AJIS 2 / A2 and new ExchangeHouse converters will be placed by actual responsibilities when implemented. Repository-local AJIS notes and Id/Observer/Uplink root documentation are design/reference material, not a substitute for implemented project code.

[Overview](../Toolbox-Overview/en.md) · [Source modules](../../README.md) · [Documentation index](../Documentation/en.md).
