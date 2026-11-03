# Interactive mdview source selection

**Implementation date:** 2026-11-03. This document describes the new no-argument source picker, not a full-screen Markdown pager.

## Starting the picker

From the Toolbox repository root:

```bash
dotnet run --project Blackboard/Tools/MdView
```

The default command enters a Spectre.Console selection prompt with four choices: Local Markdown file, HTTPS Markdown URL, GitHub repository README, and Exit. Press Escape to exit without opening a document. This requires an interactive terminal; piped or non-interactive invocations should pass a source as an argument instead.

### Local Markdown file

The Local option delegates to the existing `SpectreFileBrowser` with `FileBrowserLocalSource`. The initial folder is the process working directory. It starts with the `*.md;*.markdown` filename filter, but still permits the library's standard folder navigation, filter editing, search and directory tree browsing. After selection, mdview **independently checks** the `.md` or `.markdown` extension because the user can modify the filter in FileBrowser. The source file is only read later, using the independent `Blackboard.TextFiles` safe complete-document reader; selection itself is read-only.

Escape within FileBrowser returns to the mdview source selection menu; it does not end the application. The menu clears when it is re-entered, keeping the console tidy. Invalid non-Markdown selections show an error on the next menu display.

### URL and GitHub

The HTTPS option prompts for an absolute URL. It disallows embedded credentials, queries and fragments, matching the existing anonymous HTTP adapter's restrictions. An empty entry returns to the menu. The GitHub option prompts for `owner/repository`, reusing the existing `MdViewCommand.Parse("git", value)` grammar; the transport adapter further validates the owner/repository identifiers. The picker does not make network requests. HTTP retrieval, ETag/Last-Modified validation and explicit stale/offline messages occur later in the unchanged execution path.

The menu does not add support for private GitHub repositories, automatically followed redirects, local files without Markdown extensions or arbitrary network schemes.

## Tests and verification

The picker accepts an injected `IAnsiConsole`; tests use `Spectre.Console.Testing.TestConsole` and an isolated temporary directory. Authored tests cover Escape from the top menu, selecting a real Markdown file without modifying bytes, FileBrowser cancellation back to the menu, GitHub input, HTTPS input, rejecting query-bearing URLs, non-interactive console refusal and extension checking for both supported and unsupported names.

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
```

The previous user-verified baseline was **6,443/6,443 green on Windows**, before this interaction update. The new tests and source code are committed but have not yet been executed in this environment (no .NET SDK). Manual verification is needed for Windows Terminal, Debian shell and SSH terminal quirks.

## Non-goals

Interactive source selection is not an interactive viewer after rendering. The existing renderer still materializes the entire Markdown document and outputs it in one pass. True low-memory paging, screen history, in-document navigation, GitHub-linked file navigation and large-file streaming are separate future milestones.

[mdview README](../../README.md) · [CLI usage](../CLI/en.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md).
