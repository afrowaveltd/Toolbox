# Blackboard — reusable terminal UI components

**Status:** first independently packaged component project and tests created; file-type classification and icon-selection API implemented in FileBrowser. The interactive TUI browser and NuGet publication remain pending.

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

A `FileBrowser` .NET library project and xUnit test project are now in the solution, with file-type icon classification/terminal fallback logic. **No full file picker, Spectre.Console renderer, executable, downloadable NuGet release or published package exists yet.**

## FileBrowser presentation helpers

The standalone FileBrowser project now also includes strict special-folder identification for a few well-known local paths, plain-text presentation with a one-space file indent, and reusable file filters (e.g. JSON-only while folders remain browsable). See [Known Folders and Filters](SpectreConsole/FileBrowser/Docs/Known-Folders-and-Filters/en.md). This is underlying library logic; an interactive Spectre.Console browser has not been released.
