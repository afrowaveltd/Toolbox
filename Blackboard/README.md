# Blackboard — reusable terminal UI components

**Status:** project family reserved / architecture documented. No runtime library or NuGet package has been implemented yet.

Blackboard is the proposed home of reusable **terminal user interface (TUI)** components for Afrowave.Toolbox. The name complements **Whiteboard**, the proposed future graphical UI (GUI) family. The pairing is a naming/design direction, not a claim that either runtime toolkit has shipped.

## Goals

- Make SSH/headless administration accessible without requiring a graphical environment.
- Provide keyboard-first interactive elements with visible shortcuts, readable help, no-color fallback and localization-friendly English defaults.
- Avoid mandatory dependencies on any single TUI framework in common application/domain services.
- Allow multiple presentation implementations under distinct adapters, initially `SpectreConsole`.
- Keep reusable components independent of UnderstandMe and other consumers; consumer applications inject texts, behavior and validation rather than embedding one application's rules.

## Planned layout

- [Architecture](Docs/Architecture/en.md) — responsibilities, dependency boundaries and naming.
- [SpectreConsole](SpectreConsole/README.md) — initial adapter family.
- [FileBrowser](SpectreConsole/FileBrowser/README.md) — reusable file/folder picker, informed by the author's earlier `Lutonet/SpectreConsoleFileBrowser` project.
- [Implementation status](IMPLEMENTATION_STATUS.md) — verified milestones and outstanding tasks.

## Relationship to Toolbox

`Blackboard` provides **widgets and terminal interaction**. `SeeMe` already has console presentation components; `Toolroom` is the home of complete utilities (for example the existing WhenItFails Setter). Avoid duplicating those projects' responsibilities.

The planned UnderstandMe settings configurator is an **application** that consumes the UnderstandMe configuration model/validator and Blackboard's TUI components. Its project should live with application tools (proposed `Toolroom/UnderstandMe/Configurator`), not inside reusable widgets.

## Framework and localization policies

Start with .NET 10 and a modern tested stable Spectre.Console version for the optional presentation package only. Do not reference Spectre.Console from UnderstandMe's configuration core.

Every visible string must have a stable English default and optional caller override; no mandatory dependency on `UnderstandMe` is allowed to render help or recover from a broken translation service. Public C# APIs receive XML documentation and non-obvious logic receives short English comments.

No compiled Blackboard assembly, executable, shortcut handler, or picker exists in the Toolbox repository at this milestone.
