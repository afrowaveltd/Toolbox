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

Without arguments, a simple **numbered text menu** prompts for Local path, HTTP(S) URL or GitHub `owner/repo`; this is not yet the planned Spectre FileBrowser picker. The command parser accepts verbs without case sensitivity but never changes Linux local path casing or ref/path names. `mdview` is the compiled executable assembly name; it is not installed as a system command.

Local files support `.md` and `.markdown` and reuse the full, strict Unicode/control-safe file reader from FileBrowser. Remote sources use fully verified UTF-8 cache entries, ETag/Last-Modified checks and explicit stale status when offline. Public GitHub README retrieval uses the preferred REST endpoint, not guessed filenames or branches. HTTP(S) URLs with query strings, fragments or embedded credentials are rejected by the current generic provider; plain HTTP is disabled by default. GitHub private repositories, redirects and links to other files are not supported.

**Safety and resource limits:** input is rendered as literal Spectre text; fenced code is never executed. The CLI currently materializes a full Markdown string and parser model in RAM and dumps the entire result to the terminal. A large file warning is informational, **not** silent truncation; for large documents choose a future paged reader when implemented. Network timeout is 30 seconds; redirects and transparent HTTP decompression are disabled. The configured cache enforces the per-document limit (256 MiB by default), distinct from Markdown render size. The cache should be used only with a trusted user-owned directory, and never for authenticated/private content in this release.

## Verification

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
```

Command grammar tests are authored but have not yet been run in this execution environment (no .NET SDK). Manual Windows Terminal, Debian console and SSH output tests remain necessary.

[CLI topic guide](Docs/CLI/en.md) · [Implementation status](IMPLEMENTATION_STATUS.md) · [Cache source](../../SpectreConsole/Markdown.Cache/README.md).
