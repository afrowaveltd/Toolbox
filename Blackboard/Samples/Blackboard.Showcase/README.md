# Blackboard Showcase — interactive test application

**Status:** first functional demonstration app for Afrowave.Toolbox.Blackboard (2026-10-24). This is an independently buildable .NET 10 **executable**, not a NuGet package and not a production administration tool.

This app groups demonstrations of reusable Blackboard UI components behind **one menu**. Each showcase scenario implements `IBlackboardShowcaseDemo`, registering a unique ID, display title and `RunAsync` method. Future widgets can add demos without adding dependencies to the FileBrowser library, rewriting the menu, or forcing users to install an entire TUI suite.

## Quick start

Run from the Toolbox repository root in an interactive terminal:

```bash
dotnet run --project Blackboard/Samples/Blackboard.Showcase
dotnet run --project Blackboard/Samples/Blackboard.Showcase -- /home
```

The optional first positional argument is an existing starting directory, otherwise the application uses the current working directory. Arrow keys choose a demonstration, Enter runs it and Escape exits/returns. Each scenario returns to the main menu when complete.

## Current demonstrations

1. **FileBrowser — choose file and preview readable text:** use the shared Spectre.Console FileBrowser to pick a local file. If the file is UTF-8, UTF-16 or UTF-32 text, its first **16 KiB** are shown as plain text. Binary data, invalid Unicode and unsafe terminal controls are not printed. The preview explicitly marks truncation. No selected file is modified.
2. **FileBrowser — filter JSON and show colored JSON preview:** FileBrowser initially shows **only `*.json` files**, but keeps directories navigable. Once selected, an entire valid JSON document up to **64 KiB** is syntax-highlighted by `Spectre.Console.Json` using `JsonText`. Malformed, binary, oversized or unsupported-encoding files receive a diagnostic and are not dumped raw.
3. **FileBrowser — choose a directory:** demonstrates local folder selection without reading or modifying files.

The picker itself also contains its own interactive filename-filter menu (All, JSON, text, image, `*.ajis;*.json` masks or regex), lazy tree navigation and recursive searching. The JSON demo uses its **public generic filter API**, not a custom JSON-only browser implementation.

## Safety and separation

- The Showcase app references the standalone FileBrowser NuGet **project**, and alone references `Spectre.Console.Json`. **FileBrowser does not depend on JSON rendering or Showcase.**
- Only **local files** can be previewed. A source-qualified FTP/SFTP selection cannot be treated as a local filesystem path.
- Files are opened **read-only**; previews never enumerate file contents recursively or display arbitrary binary bytes.
- The text/JSON preview is deliberately bounded; no unlimited large files/terminal flood, no partial malformed JSON rendered.
- No direct dependency on UnderstandMe. Actual translation dictionary or settings interpretation remains with its consumer project.
- A preview is user-initiated and prints selected file contents to the local terminal: **avoid opening credentials/secrets on a shared or recorded terminal session**.
- Detection is conservative, not a universal detector for CP852/Windows-1250/ISO-8859-2. Legacy code-page previews are not supported yet.

## Extend the app

Create an implementation of `IBlackboardShowcaseDemo` in this project (or a separately referenced sample module), then add it to the `modules` array in `Program.cs`. The host validates unique IDs and returns to the common menu.

This project has its own README, [documentation](Docs/Showcase/en.md), [implementation status](IMPLEMENTATION_STATUS.md), and test project [Blackboard.Showcase.Tests](../Blackboard.Showcase.Tests/README.md).

## Verification

```bash
dotnet test Blackboard/Samples/Blackboard.Showcase.Tests/Blackboard.Showcase.Tests.csproj
dotnet build Blackboard/Samples/Blackboard.Showcase/Blackboard.Showcase.csproj
```

GitHub CI builds/tests the whole `Toolbox.sln`. Automated `Spectre.Console.Testing` tests check menu selection, JSON filtering and rendered output; **manual real-terminal smoke tests** are still required before considering terminal behavior fully verified.
