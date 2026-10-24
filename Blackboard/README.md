# Blackboard — reusable terminal UI components

**Status (2026-10-24):** the independent FileBrowser .NET 10 project now contains a first interactive Spectre.Console picker, reusable filename filters, local navigation, lazy tree and bounded recursive search; a runnable demo exists under `Samples/FileBrowser.Sample`. No stable NuGet has been published and terminal smoke testing remains pending.

Blackboard is the proposed home of reusable **terminal user interface (TUI)** components for Afrowave.Toolbox. The name complements **Whiteboard**, the proposed future graphical UI (GUI) family. **These names describe project families, not umbrella NuGet packages.** The pairing is a naming/design direction, not a claim that either runtime toolkit has shipped.

## Goals

- Make SSH/headless administration accessible without requiring a graphical environment.
- Provide keyboard-first interactive elements with visible shortcuts, readable help, no-color fallback and localization-friendly English defaults.
- Avoid mandatory dependencies on any single TUI framework in common application/domain services.
- Group technology-specific implementations in folders such as `SpectreConsole`, but **do not publish or require a large aggregate package** just because components share a renderer.
- Each independently useful UI tool gets its own `.csproj`, independently versioned NuGet package, test project, README, topic documentation and implementation status. Dependencies must be limited to that tool's actual requirements.
- Keep reusable components independent of UnderstandMe and other consumers; consumer applications inject texts, behavior and validation rather than embedding one application's rules.

## Planned layout

- [Architecture](Docs/Architecture/en.md) — responsibilities, dependency boundaries and naming.
- [Independent Packaging](Docs/Independent-Packaging/en.md) — one widget per NuGet, with separate examples and domain applications.
- [SpectreConsole](SpectreConsole/README.md) — initial adapter family.
- [FileBrowser](SpectreConsole/FileBrowser/README.md) — future **independently packaged** file/folder picker, informed by the author's earlier `Lutonet/SpectreConsoleFileBrowser` project.
- `Samples/` (future) — one or more demonstration applications showing how Blackboard components are used, not production utilities.
- [Implementation status](IMPLEMENTATION_STATUS.md) — verified milestones and outstanding tasks.

## Relationship to Toolbox

`Blackboard` provides **widgets and terminal interaction**. `SeeMe` already has console presentation components; `Toolroom` contains some existing complete utilities (for example the WhenItFails Setter). Do not move established projects just for naming consistency.

**Production/helper applications belong to their domain projects** rather than Blackboard/Whiteboard. The UnderstandMe Configurator, for instance, is owned by UnderstandMe (proposed location `UnderstandMe/Tools/Configurator`; exact location can be settled when the executable is created). Blackboard itself may later include one or more `Samples` apps, whose only responsibility is demonstrating reusable controls.

## Framework and localization policies

Start with .NET 10 and a modern tested stable Spectre.Console version **in each component package that actually needs it**. The folder `SpectreConsole` does not imply a mandatory shared framework wrapper. Do not reference Spectre.Console from UnderstandMe's configuration core.

Every visible string must have a stable English default and optional caller override; no mandatory dependency on `UnderstandMe` is allowed to render help or recover from a broken translation service. Public C# APIs receive XML documentation and non-obvious logic receives short English comments.

The standalone `FileBrowser` library, xUnit project and executable `Samples/FileBrowser.Sample` are in `Toolbox.sln`. The Spectre.Console picker offers existing-file/folder selection, directory browsing, filename wildcard/regex filters, lazy tree navigation and multi-match recursive searching. **No NuGet package has been published yet**; not every terminal/SSH environment has been smoke-tested.

## FileBrowser presentation helpers

The standalone FileBrowser project now also includes strict special-folder identification for a few well-known local paths, plain-text presentation with a one-space file indent, and reusable file filters (e.g. JSON-only while folders remain browsable). See [Known Folders and Filters](SpectreConsole/FileBrowser/Docs/Known-Folders-and-Filters/en.md). This logic is now consumed by the first interactive Spectre.Console picker; the library remains unpublished on NuGet.

## FileBrowser navigation foundation

The independent FileBrowser project contains a read-only local source, storage-neutral lazy folder tree, bounded recursive wildcard/regex search and multi-match results as both relative paths and a matching-branches tree. A first interactive Spectre.Console adapter is now implemented, with a separately runnable [demonstration](Samples/FileBrowser.Sample/README.md). Further manual UI/terminal validation and source connectors remain future work. See [Tree and Recursive Search](SpectreConsole/FileBrowser/Docs/Tree-and-Recursive-Search/en.md).
