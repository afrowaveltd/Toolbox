# Blackboard and Whiteboard: related UI toolkit families

**Status:** naming and architecture proposal recorded 2026-10-22; implementation pending.

## Names and roles

- **Blackboard** — reusable keyboard-driven *terminal* UI controls.
- **Whiteboard** — proposed symmetric family for future *graphical* UI components, beginning with an optional Avalonia adapter if adopted.
- The names identify the interaction medium, not a prescribed dark/light color theme. Neither should bind users to Spectre.Console or Avalonia.
- `SeeMe` remains presentation-specific output components; `Toolroom` continues to host existing utilities. Future domain-specific production editors belong logically to the projects they serve. Avoid moving unrelated existing code solely to match a new name.

## Project and dependency boundaries

Proposed directory structure:

```text
Blackboard/
  README.md
  IMPLEMENTATION_STATUS.md
  Docs/Architecture/en.md
  SpectreConsole/                     # organization by technology; not an umbrella package
    README.md
    FileBrowser/                      # independently packaged library
      README.md
      IMPLEMENTATION_STATUS.md
      Docs/Overview/en.md             # design docs now; code comes later
      Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser.csproj  # future
    [other independent UI component projects]
  Samples/                             # future demonstration apps, not production utilities

Whiteboard/                            # future family, not created yet
  Avalonia/                            # individual packages, not mandatory whole-Avalonia bundle
  Samples/                             # future GUI demonstrations

UnderstandMe/Tools/Configurator/       # proposed location of UnderstandMe-owned executable
```

**Every independently useful control is its own project/NuGet unit** when shipped as a package. The first proposed package name is `Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser`; later controls may have separate packages under the same technology folder. Do **not** ship a mandatory `Blackboard`, `Whiteboard`, `SpectreConsole` or `Avalonia` umbrella package: requesting a file picker must not bring an entire UI toolkit's unrelated controls. Dependencies are opt-in and minimal.

A shared framework-neutral contracts package should be introduced **only after real repeated usage demonstrates a need**. If a later convenience meta-package is useful, it must remain explicitly optional and must not replace component packages. A demo application can reference several tools but must never be an implicit dependency of any NuGet component.

The file picker must remain usable without UnderstandMe. The UnderstandMe-owned Configurator passes relevant English/localized text and receives an absolute path or cancellation outcome; only UnderstandMe's validated JSON settings store loads/saves `UnderstandMeSettings.json`. Its source tree, executable and runtime configuration remain with UnderstandMe, not with Blackboard. Demos under Blackboard/Whiteboard show reusable widget usage without becoming actual configuration, administration or domain applications.

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

Each independently published component project receives tests, its own `IMPLEMENTATION_STATUS.md`, English root README, and topic documentation under its own `Docs/<topic>/en.md`; the Blackboard family-level implementation status also summarizes progress. Changes are committed to GitHub and CI-verified. Samples/executables have their own isolated project/test dependencies. Avoid claiming build/test success from documentation-only scaffolding.

## FileBrowser source extensions

The default independently published FileBrowser package will include local filesystem navigation. Protocol extensions are optional **separate packages**; the proposed FTP/FTPS adapter depends on the FileBrowser source contract, not the reverse. The generic UI does not need to know FTP commands or remote login details. Never treat a remotely selected file as a local path, or require remote connection libraries for local-only selection. See [Storage Providers](../../SpectreConsole/FileBrowser/Docs/Storage-Providers/en.md).

## Terminal capabilities

UI components must respect detected interactive, Unicode and ANSI terminal capabilities. Decorative glyphs are optional, not a prerequisite for navigation or accessibility. A basic Unicode-capable console may still lack emoji glyphs; prefer conservative Auto fallback and explicit user overrides, without globally rewriting output encoding. FileBrowser's detailed design: [Terminal Compatibility](../SpectreConsole/FileBrowser/Docs/Terminal-Compatibility/en.md).
