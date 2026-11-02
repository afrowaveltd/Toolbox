# Afrowave.Toolbox

**Afrowave.Toolbox** is a collection of small, reusable **.NET 10** libraries, console utilities, test projects and examples. It is **not** one mandatory all-in-one package: each family and useful UI control has its own responsibilities and dependencies.

**GitHub is the source of truth:** [afrowaveltd/Toolbox](https://github.com/afrowaveltd/Toolbox) (branch `master`).

[Documentation index](../Docs/Documentation/en.md) · [Repository overview](../Docs/Toolbox-Overview/en.md) · [Architecture](../Docs/Architecture/en.md) · [Build and verification](../Docs/Development-and-Verification/en.md) · [Implementation status](../IMPLEMENTATION_STATUS.md)

## Package and project directory

| Library / family | What it does | Status |
|---|---|---|
| [Essentials](../Essentials/README.md) | `Result`, `Response<T>`, issues, metadata, guards and shared contracts | **Published on NuGet**, `0.2.0` |
| [WhenItFails](../WhenItFails/README.md) | Structured error catalogs, validation, runtime resolution, recovery and profiles | **Published on NuGet**, `1.0.0` |
| [Blackboard FileBrowser](../Blackboard/SpectreConsole/FileBrowser/README.md) | Interactive Spectre.Console file/folder picker, filters, tree and search | **Published on NuGet**, `0.1.0-preview.1` |
| [UnderstandMe](../UnderstandMe/README.md) | Offline localization, dictionaryless keys, JSON dictionaries and validation | Implemented offline core; broader translation service is planned |
| [ExchangeHouse Markdown](../ExchangeHouse/Markdown/README.md) | UI-neutral lossless Markdown parser, links/images, nested lists, code-language and quote metadata | Implemented preview project, **not yet published** |
| [Blackboard Markdown](../Blackboard/SpectreConsole/Markdown/README.md) | Spectre.Console Markdown display, safe images, quotes, code blocks and optional syntax colors | Implemented preview project, **not yet published** |
| [Markdown.TextMate](../Blackboard/SpectreConsole/Markdown.TextMate/README.md) | Optional real code syntax coloring for Markdown with TextMateSharp grammars | Implemented source adapter; **not yet published** |
| [SeeMe](../SeeMe/README.md) | Console presentation helpers for WhenItFails | Implemented source project |
| [Toolroom](../Toolroom/README.md) | Runnable developer/maintenance tools including WhenItFails Setter | Implemented utilities |

The three NuGet publications above are **maintainer-confirmed**; the package IDs/versions match repository project metadata. Do not confuse a project `Version` with public publication. Details: [Release status](../Docs/Release-Status/en.md).

## NuGet-first reusable component policy

As soon as a self-contained .NET library is useful, documented, tested and
installable, we publish a **NuGet prerelease** rather than waiting for a full
`1.0.0`. Frequent preview releases are welcome; downstream projects consume
stable package IDs and may opt into updates without changing shared source
paths. Published versions are immutable and consumers must explicitly enable
prerelease updates where their package manager requires it. This is a
**publication policy**, not a claim that every preview project has already
been released. See [NuGet release policy](../Docs/NuGet-Release-Policy/en.md)
and [current package status](../Docs/Release-Status/en.md).

## Applications and examples

- [Blackboard.Showcase](../Blackboard/Samples/Blackboard.Showcase/README.md): interactive demonstrations of FileBrowser text and JSON previews, directory selection, and four Markdown themes (Classic, Ocean, Forest, Monochrome), with links, images and nested/ordered lists.
- [FileBrowser.Sample](../Blackboard/Samples/FileBrowser.Sample/README.md): focused file picker example.
- [UnderstandMe Configurator](../UnderstandMe/Tools/Configurator/README.md): JSON settings editing and validation, provider checks and offline help rendered by Blackboard Markdown.
- [WhenItFails Setter](../Toolroom/WhenItFails/Setter/README.md): error catalog authoring, inspection and validation.

## Architecture

```text
Essentials
    +-- WhenItFails
    |     +-- SeeMe / WhenItFails Console
    |     +-- Toolroom / Setter
    +-- shared infrastructure consumed by other projects

ExchangeHouse.Markdown --(document model)--> Blackboard.SpectreConsole.Markdown
                                                 +-- Blackboard.Showcase
                                                 +-- UnderstandMe Configurator

Blackboard.SpectreConsole.FileBrowser
    +-- FileBrowser.Sample / Blackboard.Showcase
    +-- UnderstandMe Configurator

UnderstandMe -- offline localization core; broader service in design
```

The diagram is conceptual; for exact package vs project references, see the `.csproj` files and [architecture](../Docs/Architecture/en.md). ExchangeHouse never needs a UI renderer; Blackboard components are independent of their consumers.

## Getting started

Install a supported **.NET 10 SDK**. From the repository root:

```bash
dotnet restore Toolbox.sln
dotnet build Toolbox.sln -c Release
dotnet test Toolbox.sln -c Release
```

Try the Markdown theme gallery and FileBrowser demos:

```bash
dotnet run --project Blackboard/Samples/Blackboard.Showcase
```

Or try the offline localization configurator:

```bash
dotnet run --project UnderstandMe/Tools/Configurator
```

[Repository getting started](../Docs/Toolbox-Getting-Started/en.md) includes per-project testing and examples of adding a published NuGet package.

## Documentation and development rules

- [Main index](../Docs/Documentation/en.md), [architecture](../Docs/Architecture/en.md), [release status](../Docs/Release-Status/en.md), [verification](../Docs/Development-and-Verification/en.md).
- Every library/app should maintain an English root `README.md`, topic-level `Docs/<Topic>/en.md` and `IMPLEMENTATION_STATUS.md`. English is the source for later automatic translations.
- Each functional step is tested, documented and committed on GitHub; Windows-focused CI complements the full Linux solution build.
- Latest verified nested-list regression: [GitHub Actions #41244480220](https://github.com/afrowaveltd/Toolbox/actions/runs/41244480220). See the [status files](../IMPLEMENTATION_STATUS.md) for subsequent changes.

## Design-only and future work

- [AJIS / A2 notes](../Ajis/README.md) are **design notes**, not a shipped AJIS 2.0 Toolbox runtime; the historic project is [Ajis.Dotnet](https://github.com/afrowaveltd/Ajis.Dotnet).
- Suitcase, GetConnected, SemTam/TamTam, TalkToMe and future ExchangeHouse converters are separate future work; this listing does **not** imply completed projects.
- The repository's [Afrowave Id rebuild](../Docs/Id-Rebuild-Review/en.md), [high-availability](../Docs/Id-High-Availability-and-Replication/en.md), [regional routing](../Docs/Id-Autodiscovery-and-Regional-Routing/en.md) and [Observer](../Docs/Id-Observer-and-Uplink/en.md) documents are exploratory designs, not implemented Id services in Toolbox.

## Community-first service and transparent infrastructure funding

Afrowave intends to give everyone the maximum feasible access to useful hosted services, with no artificial paywalls or personal enrichment. Community support, when needed, is intended for transparent, verifiable real operating costs. Those future hosted-service policies **do not impose payment requirements on Toolbox libraries**. No donations or bank-backed funding dashboard are implemented by these repository projects. See [Community access and transparent funding](../Docs/Afrowave-Community-Access-and-Transparent-Funding/en.md).

## License

See [LICENSE.txt](../LICENSE.txt).
