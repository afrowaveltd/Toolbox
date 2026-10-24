# Shared Blackboard test application

## Motivation

A small FileBrowser-only sample already exists. As Blackboard grows, repeating a bespoke console program for every reusable widget would scatter discoverability and testing. `Blackboard.Showcase` is the shared **manual/interactive test application** with its own independent test suite.

This is the natural place for demonstrations of Blackboard controls. Domain-specific production tools, such as UnderstandMe Configurator, stay within their own projects.

## Modular architecture

```text
Blackboard/Samples/
  Blackboard.Showcase/
    Program.cs                         # App entry / demo registrations
    IBlackboardShowcaseDemo.cs         # Stable module contract
    BlackboardShowcaseHost.cs          # Main interactive menu
    FileBrowserPreviewDemo.cs          # Text file picker + bounded preview
    FileBrowserJsonDemo.cs             # JSON-only filter + colored JSON preview
    FileBrowserDirectoryDemo.cs        # Folder selection
    TextFilePreviewReader.cs           # Secure read-only prefix
    JsonFilePreviewReader.cs           # Complete valid small JSON
    README.md
    IMPLEMENTATION_STATUS.md
    Docs/Showcase/en.md
  Blackboard.Showcase.Tests/
    TextAndJsonPreviewTests.cs
    ShowcaseInteractionTests.cs
    README.md
    IMPLEMENTATION_STATUS.md
```

To add more tools, implement `IBlackboardShowcaseDemo` with unique `Id`/`Title` and `RunAsync(IAnsiConsole, startingDirectory, CancellationToken)`, then register it in the main menu. No dynamic code loading, plugin reflection or global service locator is needed.

## Built-in scenarios

### Text file preview

The selected file path comes from `SpectreFileBrowser.PickAsync`. A separate bounded reader detects strict UTF-8 (with/without BOM), BOM-marked UTF-16 and UTF-32. It refuses NUL-containing undecorated text, invalid encodings and terminal control characters (except LF, CR and tab); it reads no more than 16 KiB by default. Partial trailing Unicode units in a truncated chunk are ignored by at most four bytes. It preserves the original file. Output uses `Spectre.Console.Text`, never interpolated markup.

### JSON filter and renderer

Pass `FileBrowserFileFilter(nameMatcher: new FileBrowserNameMatcher("*.json"))` to `SpectreFileBrowserOptions.FileFilter` so only JSON file names appear, while directories are still visible. `JsonFilePreviewReader` obtains the complete file only if it fits under 64 KiB and is valid text, then validates strict syntax using `System.Text.Json.JsonDocument`. `FileBrowserJsonDemo` renders the validated JSON with `new Spectre.Console.Json.JsonText(json)`. The JSON syntax-highlighting package is a **direct dependency only of the Showcase app**.

Invalid JSON, oversize JSON, binary bytes and unsafe control sequences produce concise diagnostics without passing their contents to the renderer. The preview does not write or edit the chosen file.

### Folder picker

Another demo chooses an existing local directory to exercise `FileBrowserPickMode.Directory`.

## Release/test policy

Every change is committed to the GitHub Toolbox source of truth, documented here and in the project's `IMPLEMENTATION_STATUS.md`. The Showcase and its tests belong to `Toolbox.sln` so the existing CI runs them. Unit/injected-terminal coverage is not a substitute for real terminal interaction tests on Debian, Windows, SSH, tmux, Unicode/legacy consoles.

The Showcase must never be shipped as a dependency of the independent FileBrowser NuGet. It can depend on many individual widgets as future sample modules are registered, without affecting downstream consumer dependencies.
