# Blackboard and Whiteboard: related UI toolkit families

**Status:** naming and architecture proposal recorded 2026-10-22; implementation pending.

## Names and roles

- **Blackboard** — reusable keyboard-driven *terminal* UI controls.
- **Whiteboard** — proposed symmetric family for future *graphical* UI components, beginning with an optional Avalonia adapter if adopted.
- The names identify the interaction medium, not a prescribed dark/light color theme. Neither should bind users to Spectre.Console or Avalonia.
- `SeeMe` remains presentation-specific output components; `Toolroom` remains complete utilities and editors. Avoid moving unrelated existing code solely to match a new name.

## Project and dependency boundaries

Proposed directory structure:

```text
Blackboard/
  README.md
  IMPLEMENTATION_STATUS.md
  Docs/Architecture/en.md
  SpectreConsole/
    README.md
    FileBrowser/
      README.md
      [future source and tests]
    [future generic prompts, tables, navigation]

Whiteboard/                         # future, not created yet
  Avalonia/
    [future reusable GUI widgets]

Toolroom/UnderstandMe/Configurator/ # future complete TUI executable
```

When implemented, the Spectre.Console adapter can become its own independently versioned assembly/NuGet package (proposed name `Afrowave.Toolbox.Blackboard.SpectreConsole`). This is not a hard requirement to create an empty `Blackboard` core DLL. Extract framework-neutral contracts only when shared by genuine multiple consumers; avoid needless abstractions.

The file picker must remain usable without UnderstandMe. The Configurator passes relevant English/localized text and receives an absolute path or cancellation outcome; only UnderstandMe's validated JSON settings store loads/saves `UnderstandMeSettings.json`.

## Shared experience principles

- Consistent keyboard controls and visible contextual shortcuts, with text-based menu alternatives if a terminal intercepts a key.
- Draft-only mutation until an explicitly confirmed save: the picker itself must never modify configurations.
- Cancellation and errors are first-class results rather than magic file-path sentinel strings.
- No global process working directory mutation during navigation; support Windows drive roots, Linux/macOS roots and mounts, SSH and noninteractive/no-color terminals.
- Escape potentially hostile filenames, paths and metadata before passing to Spectre markup rendering.
- Localizable UI text via overrideable strings or callbacks and built-in English fallback, independent of external translation.
- Independent testable directory navigation logic with filesystem edge-case coverage.

## Reuse from Lutonet

Reference: https://github.com/Lutonet/SpectreConsoleFileBrowser at commit `568f7f3` (2022).

Reuse the *ideas* (select directory/file, parent navigation, page size, icon mode, replaceable English labels). Update for modern Spectre.Console, cancel/filters/errors/security without importing old preview dependencies or changing process CWD. The user identifies as the author of the original package; preserve attribution if copying substantial code and maintain MIT-compatible license notices.

## Release discipline

Each implemented change receives tests, a GitHub commit, an updated project `IMPLEMENTATION_STATUS.md`, an English README, and topic documentation under `Docs/<topic>/en.md`. Avoid claiming build/test success from documentation-only scaffolding.
