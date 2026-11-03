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
4. **Markdown — preview and compare color themes:** displays the same source-preserved ExchangeHouse Markdown document with Classic, Ocean, Forest or Monochrome themes. Includes links, image alternatives and nested/ordered lists; a Compare all option displays every theme.

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

## Terminal startup

The Showcase clears the terminal once when it enters the interactive main menu, so earlier command output is hidden. It **does not** clear after running each demonstration, preserving the demo result. This is an application-host behavior, not a change to the independent FileBrowser NuGet package.

## Markdown theme gallery (2026-10-29)

The fourth Showcase scenario, **Markdown — preview and compare color themes**, demonstrates the standalone `Blackboard.SpectreConsole.Markdown` renderer. Choose Classic, Ocean, Forest or Monochrome, or compare all four. The gallery parses one embedded source document once with `ExchangeHouse.Markdown` and renders it with independently supplied `MarkdownSpectreTheme` instances. No files or network calls are involved. See [Markdown Themes](Docs/Markdown-Themes/en.md).

Run `dotnet run --project Blackboard/Samples/Blackboard.Showcase` and select the new fourth scenario with arrows and Enter; Escape returns. The existing three FileBrowser demonstrations are unchanged.

## Syntax-colored Markdown preview (2026-10-31)

The Markdown gallery can now opt into the independent TextMate grammar adapter for its
Classic/Ocean/Forest themes. This colors recognized `csharp` and `bash` fenced code,
including indented code nested in a list. Monochrome deliberately stays plain.
The adapter is a **Showcase-only dependency**, not required by the base Markdown renderer.
If native grammar support is unavailable, the gallery retains safe literal code.

[Adapter documentation](../../SpectreConsole/Markdown.TextMate/README.md).

## Classic .NET Core 3.1 code sample (2026-11-01)

The Markdown theme gallery uses a longer, historically typical C# console entry point:
`using System`, a named namespace, `Program`, `private static void Main(string[] args)`,
a conditional branch and a `for` loop. Its declared `csharp` fence exercises
TextMate syntax colors, brace nesting and **source-preserved indentation**.
This is an embedded demonstration of .NET Core 3.1-era syntax, **not** a
change to Toolbox's net10.0 target framework. All four gallery themes
reuse the same parsed Markdown; Monochrome remains uncolored.

## Quote-nested code example (2026-11-01)

The Markdown theme gallery now includes an indented `csharp` fence beneath a numbered list item **inside a blockquote**, in addition to the classic .NET Core 3.1 `Program.Main` sample. Colored themes demonstrate optional TextMate highlighting in both places; Monochrome remains uncolored. [Gallery documentation](Docs/Markdown-Themes/en.md).

## Quote-list second paragraph (2026-11-01)

The Markdown gallery's numbered quoted-list sample includes a blank quote line followed by a second indented paragraph and a C# fence, illustrating loose quote lists alongside ordinary lists and .NET Core 3.1 syntax highlighting. [Markdown themes](Docs/Markdown-Themes/en.md).

## Quote nested under a deployment step (2026-11-01)

The Markdown theme gallery now includes an ordinary deployment item containing a quoted warning, numbered dependency checks and a `bash` fence with `dotnet --list-runtimes`. This adds no project dependencies and does not execute the command. [Theme guide](Docs/Markdown-Themes/en.md).

## Open a complete README.md through FileBrowser (2026-11-02)

Choose the fifth Showcase item **Markdown — open local README or .md file**, navigate folders and select `*.md` or `*.markdown`. The app validates the extension again after selection, because FileBrowser permits interactive filter changes. It then performs a **complete, read-only** decode, parses Markdown and renders all blocks in the Classic theme with optional TextMate code syntax coloring. **There is no 64 KiB display cutoff:** files above 64 KiB merely show an informational warning and continue rendering the entire contents.

Encoding and safety checks cover **the entire file**, not only a prefix. Supported input is strict UTF-8 or BOM-marked UTF-16/UTF-32. Invalid Unicode, NUL/binary text and forbidden terminal control characters cause an explanatory refusal rather than partial unsafe output. An empty file or cancelled picker returns cleanly. The scanner reads in bounded 8 KiB chunks, but the source and parse tree are materialized into RAM; extremely large files therefore still depend on available memory and terminal scrollback. Markdown grammar is tolerant: valid encoding and safe output are *not* a guarantee of strict CommonMark conformance. Code fences are displayed, never executed.

Run from the Toolbox repository root:

```bash
git pull
dotnet run --project Blackboard/Samples/Blackboard.Showcase
```

Then choose the Markdown file viewer and open the root `README.md`. This is an independent Showcase feature, not a new FileBrowser/Markdown NuGet dependency. [Full viewer guide](Docs/Markdown-File-Viewer/en.md).

## Remote Markdown temp-file spooling (experimental, 2026-11-02)

The new reusable-in-sample `RemoteMarkdownTempDocument` downloads a trusted HTTPS Markdown document with bounded memory into a disposable uniquely named temporary file. `OpenRead()` provides independent seekable paging streams after the network request has completed. Disposal deletes the temp file. This helper is **not yet wired into the interactive viewer**; only local selection is available in the menu. See [remote cache](Docs/Remote-Markdown-Cache/en.md), [future live translation proposal](Docs/Live-Markdown-Translation/en.md) and [implementation status](IMPLEMENTATION_STATUS.md).

## Indexed source-page navigation (experimental, 2026-11-02)

The sixth Showcase menu option, **Markdown — disk-indexed source pages**,
uses ExchangeHouse's new temporary byte-offset page index to display
one page of local UTF-8 Markdown source at a time. Navigate **Next** and
**Previous** in either direction, or Escape to close; only the requested
page is held in memory. The entire file is scanned and validated before
paging. Unlike the fifth (fully rendered, in-memory) viewer, this one
intentionally displays **literal source**, not context-dependent Markdown
rendering: code fences and lists could cross page boundaries. It is a
low-memory navigation experiment rather than the final `mdview`.
[Usage and limitations](Docs/Markdown-Paged-Source-Viewer/en.md).

## Context-aware code-fence preview toggle (experimental, 2026-11-02)

The sixth source pager now creates an additional disposable fenced-code
checkpoint index. Its navigation menu offers **Show conservative Markdown
preview** / **Show literal source**. Preview mode reuses the existing
streaming-line Spectre renderer; it can correctly classify code lines on
page N even when their fence opened on an earlier page. This still does
**not** guarantee CommonMark-correct rendering of lists, reference links,
multiline paragraphs or quotes. Source view remains the default.
[Technical contract](../../../ExchangeHouse/Markdown/Docs/Fence-Checkpoints/en.md).


## Owner-aware conservative preview (experimental, 2026-11-02)

The reversible sixth Showcase preview now benefits from its shared
Markdown renderer's explicit source-owner offsets. It can place simple
nested markers and indented child/parent continuation lines using the
restored list depth across page boundaries. It still does not promise
full CommonMark layout; literal source remains the default and the
original complete document renderer remains unchanged.


## Sparse checkpoint storage (2026-11-02)

The sixth Showcase reader automatically uses the ExchangeHouse
checkpoint index with its default 16-page stride. Visiting a page
between stored checkpoints transparently replays up to 15 earlier
source pages to reconstruct the same conservative Markdown context.
No UI change or full-document buffering; the literal source remains
the default display. The tradeoff is disk space versus seek I/O.

## Selectable Markdown paging profiles (2026-11-03)

The sixth Markdown pager now asks for **Balanced, HDD, SSD or LowRAM** after local file selection, displays the selected page size/checkpoint interval, and applies both settings when building the indexes. Escape cancels before indexing. Source mode remains the default. [Profile policy](../../../ExchangeHouse/Markdown/Docs/Paging-Profiles/en.md).
