# Component-level packages and example applications

Architecture decision — 2026-10-22. No package or sample application was created in this change.

## Core rule

**Blackboard** (TUI) and future **Whiteboard** (GUI) are *organizational project families*. Do not make users install a monolithic umbrella package when they need one widget. A framework folder such as `Blackboard/SpectreConsole` or `Whiteboard/Avalonia` is a grouping, not necessarily a build project.

Every component worth releasing has its own:
- `.csproj` and independent NuGet identity and version.
- necessary dependencies only; no unrelated widgets, demos or production tools.
- tests, root README, `IMPLEMENTATION_STATUS.md`, and English topic docs in `Docs/<topic>/en.md`.

The first independent package project is `Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser`; it now contains a Spectre.Console picker, but no NuGet version has been published. Its project dependencies do not bring in other Blackboard widgets. Independent releases also let us stabilize components at different rates.

Shared infrastructure may be extracted later **only when actual consumer reuse justifies it**; it must not create a mandatory heavyweight dependency graph. Optional convenience package bundles, if ever introduced, must never replace the individual packages.

## Applications and examples

- Blackboard now provides a **standalone runnable example** in [FileBrowser.Sample](../../Samples/FileBrowser.Sample/README.md); Whiteboard may later provide its own samples. Samples showcase reusable components but are not NuGet dependencies.
- **Domain-specific real tools** stay with their related domain project: for example an UnderstandMe configuration editor belongs to UnderstandMe; it consumes Blackboard's FileBrowser, settings metadata and validation from UnderstandMe. Proposed source directory: `UnderstandMe/Tools/Configurator`; confirm naming at implementation.
- Existing `Toolroom/WhenItFails` tooling stays where it is. No large-scale file moves are required to enforce future ownership rules.
- Samples may depend on multiple UI packages; **UI packages must never depend on samples**. A published library should not automatically depend on the domain application's executable.

## Dependency examples

```text
UnderstandMe.Configurator (application)
  ├── UnderstandMe (settings and validation library)
  └── Blackboard.SpectreConsole.FileBrowser (only needed picker)
       └── Spectre.Console

Blackboard.Samples (future optional executable)
  ├── Blackboard.SpectreConsole.FileBrowser
  └── future independently selected Blackboard widgets
```

Preserve minimal transitive NuGet references. Where an application needs generic forms or navigation in addition to file browsing, it references those packages **explicitly**, rather than importing a complete UI suite by default.

See [Blackboard architecture](../Architecture/en.md), [FileBrowser](../../SpectreConsole/FileBrowser/README.md) and [Blackboard implementation status](../../IMPLEMENTATION_STATUS.md).

## FileBrowser plugin example: no unnecessary transitive dependencies

The initial FileBrowser ships with the **local** source built in. A separate opt-in package may add FTP/FTPS browsing, without increasing dependencies or network/security attack surface for every local-only caller. SFTP, cloud storage and other connectors remain future independent choices. See [FileBrowser Storage Providers](../../SpectreConsole/FileBrowser/Docs/Storage-Providers/en.md).

## Shared multi-tool demonstration application

The [Blackboard.Showcase](../../Samples/Blackboard.Showcase/README.md) is the expandable interactive demo host, with modules registered through `IBlackboardShowcaseDemo` and a separate xUnit test suite. It intentionally lives in `Blackboard/Samples`, not within any component package. The JSON-only selection and syntax-highlighting example installs `Spectre.Console.Json` only in this app. The [FileBrowser.Sample](../../Samples/FileBrowser.Sample/README.md) remains as a tiny focused sample and regression entry point; neither sample is a NuGet transitive dependency.
