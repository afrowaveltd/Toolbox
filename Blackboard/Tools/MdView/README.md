# mdview — early terminal Markdown reader

An independent .NET 10 **source-first CLI prototype** for opening complete Markdown documents with the existing Afrowave parser/renderer. It uses the separate Markdown.Cache library for HTTP(S) documents and public GitHub repository READMEs. This prototype is **not published**, not a native page-based reader and not a finished interactive TUI.

## Start from the Toolbox root

```bash
dotnet run --project Blackboard/Tools/MdView -- --help
dotnet run --project Blackboard/Tools/MdView -- local README.md
dotnet run --project Blackboard/Tools/MdView -- README.md
dotnet run --project Blackboard/Tools/MdView -- url https://example.org/manual.md
dotnet run --project Blackboard/Tools/MdView -- git afrowaveltd toolbox
dotnet run --project Blackboard/Tools/MdView -- github:AfrowaveLtd/Toolbox
dotnet run --project Blackboard/Tools/MdView
```

Without arguments, a **Spectre.Console source menu** offers an interactive local Markdown FileBrowser, HTTPS URL prompt, GitHub `owner/repo` prompt and Exit. Escape from FileBrowser returns to the source menu; Escape from the source menu exits. See [interactive source selection](Docs/Interactive-Source-Picker/en.md). The command parser accepts verbs without case sensitivity but never changes Linux local path casing or ref/path names. `mdview` is the compiled executable assembly name; it is not installed as a system command.

Local files support `.md` and `.markdown` and reuse the complete, strict Unicode/control-safe reader from [Blackboard.TextFiles](../../TextFiles/README.md). Remote sources use fully verified UTF-8 cache entries, ETag/Last-Modified checks and explicit stale status when offline. Public GitHub README retrieval uses the preferred REST endpoint, not guessed filenames or branches. HTTP(S) URLs with query strings, fragments or embedded credentials are rejected by the current generic provider; plain HTTP is disabled by default. GitHub private repositories, redirects and links to other files are not supported.

**Safety and resource limits:** input is rendered as literal Spectre text; fenced code is never executed. The CLI currently materializes a full Markdown string and parser model in RAM and dumps the entire result to the terminal. A large file warning is informational, **not** silent truncation; for large documents choose a future paged reader when implemented. Network timeout is 30 seconds; redirects and transparent HTTP decompression are disabled. The configured cache enforces the per-document limit (256 MiB by default), distinct from Markdown render size. The cache should be used only with a trusted user-owned directory, and never for authenticated/private content in this release.

## Verification

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
```

The 10 original command grammar tests passed on Windows as part of the user-verified **6,443/6,443** baseline. Eleven additional interactive picker test cases are authored but have not yet been run (no local SDK); manual Windows Terminal, Debian console and SSH output checks remain necessary.

[CLI topic guide](Docs/CLI/en.md) · [Implementation status](IMPLEMENTATION_STATUS.md) · [Cache source](../../SpectreConsole/Markdown.Cache/README.md).

## Historical build fix (2026-11-03)

The first CLI commit referenced `TextFilePreviewReader` as though it were exported by FileBrowser. It was actually located in Showcase. A temporary linked source file corrected the initial build; the subsequent extraction into [Blackboard.TextFiles](../../TextFiles/README.md) fully replaced this bridge. This section records the historical issue, not a current dependency.

## Windows test verification (2026-11-03)

The user-provided `dotnet test Toolbox.sln` log after the shared reader fix shows `mdview.dll` successfully built and `MdView.Tests` **10/10 passed**; all twelve solution test assemblies passed **6,438/6,438** on Windows (net10.0 target, .NET 11 preview SDK). Manual CLI usage and Linux CI are still pending; the pending cross-project linked-source reader extraction remains an architecture task.

## Independent local reader (2026-11-03)

The temporary Showcase `Compile Include` bridge is now removed. The local file decoder lives in [Blackboard.TextFiles](../../TextFiles/README.md), with a dedicated `ProjectReference`. The CLI has no dependency on the Showcase executable. Rerun the full solution tests to verify this refactoring; the previously reported green Windows run predates it.

## Spectre interactive source selection (2026-11-03)

The no-argument entry now uses `MdViewInteractiveSourcePicker` with an injected `IAnsiConsole` and the existing local `SpectreFileBrowser`. Local filename filtering starts with `*.md;*.markdown` and is verified again after selection; URL and GitHub options use Spectre text prompts. Escape inside FileBrowser returns to the menu; Escape from the menu exits. Explicit command-line modes are unchanged. The picker clears the screen on each return to the menu and rejects non-interactive stdin.

Eleven new TestConsole-driven scenarios cover the menu, cancellation, real file selection, GitHub/HTTPS prompts and invalid input without network calls. **New code/tests are not yet run**; the last user-confirmed 6,443-green baseline predates this update. [Details](Docs/Interactive-Source-Picker/en.md).

## Color syntax highlighting for installed grammar languages (2026-11-04)

The CLI now enables our optional [TextMate grammar adapter](../../SpectreConsole/Markdown.TextMate/Docs/Supported-Grammars/en.md) when rendering local, HTTPS and GitHub Markdown. Its previous default Markdown theme omitted the syntax provider. More than 50 languages supported by the installed `TextMateSharp.Grammars` bundle can be selected directly or by file extension, with common README aliases including `js`, `ts`, `py`, `cs`, `sh`, `json`, `yml`, `sql`, `html`, `css`, `rust` and `go`. Code never executes; unknown languages, native backend failures and long lines safely render as plain text. TextMate's native dependency is optional to base Markdown/ExchangeHouse, but included in this CLI. New highlighter and integration regression tests have been committed; no new test run has yet been verified.

## Browse public GitHub repositories by owner (2026-11-04)

You no longer need to remember an exact repository name. The following commands list an owner's public repositories and open a Spectre selection interface:

```bash
dotnet run --project Blackboard/Tools/MdView -- afrowaveltd
dotnet run --project Blackboard/Tools/MdView -- git afrowaveltd
dotnet run --project Blackboard/Tools/MdView -- github:afrowaveltd
```

The existing `git OWNER REPOSITORY` and `github:OWNER/REPOSITORY` forms still open a README immediately. The no-argument GitHub source prompt accepts just the owner too. Small catalogs show an owner/repository tree; all catalogs offer a paged selection prompt with a name/description filter and Escape to cancel without downloading a README. The catalog uses public GitHub REST endpoints, a 30-minute independently verified local cache with explicit stale/offline fallback and a bounded first-1,000-repositories limit. Owner-only browsing requires an interactive terminal; scripts should give a full owner/repository pair.

[Detailed owner discovery guide](Docs/GitHub-Owner-Discovery/en.md) · [GitHub catalog source](../../SpectreConsole/Markdown.Cache/Docs/GitHub-Repository-Catalog/en.md). New catalog and UI tests are committed but not yet executed here.
