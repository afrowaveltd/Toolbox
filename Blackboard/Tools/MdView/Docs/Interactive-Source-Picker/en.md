# Interactive mdview source selection

**Implementation date:** 2026-11-03. This document describes the new no-argument source picker, not a full-screen Markdown pager.

## Starting the picker

From the Toolbox repository root:

```bash
dotnet run --project Blackboard/Tools/MdView
```

The default command enters a Spectre.Console selection prompt with four choices: Local Markdown file, HTTPS Markdown URL, **GitHub repository (browse Markdown links)**, and Exit. Press Escape to exit without opening a document. This requires an interactive terminal; piped or non-interactive invocations should pass a source as an argument instead.

### Local Markdown file

The Local option delegates to the existing `SpectreFileBrowser` with `FileBrowserLocalSource`. The initial folder is the process working directory. It starts with the `*.md;*.markdown` filename filter, but still permits the library's standard folder navigation, filter editing, search and directory tree browsing. After selection, mdview **independently checks** the `.md` or `.markdown` extension because the user can modify the filter in FileBrowser. The source file is only read later, using the independent `Blackboard.TextFiles` safe complete-document reader; selection itself is read-only.

Escape within FileBrowser returns to the mdview source selection menu; it does not end the application. The menu clears when it is re-entered, keeping the console tidy. Invalid non-Markdown selections show an error on the next menu display.

### URL and GitHub

The HTTPS option prompts for an absolute URL. It disallows embedded credentials, queries and fragments, matching the existing anonymous HTTP adapter's restrictions. An empty entry returns to the menu. The GitHub option accepts `owner` or `owner/repository`. Unlike the explicit one-shot `git OWNER REPOSITORY` CLI, this **interactive** selection dispatches `MdViewSourceKind.GitHubLinkBrowser`. With an owner only, the repository catalog (or its manual-name fallback after API errors) runs before opening the interactive reader. The source picker itself does not make network requests. Subsequent GitHub metadata/file fetching and verified-cache freshness handling occur in the link browser.

The menu does not add support for private GitHub repositories, automatically followed redirects, local files without Markdown extensions or arbitrary network schemes.

## Tests and verification

The picker accepts an injected `IAnsiConsole`; tests use `Spectre.Console.Testing.TestConsole` and an isolated temporary directory. Authored tests cover Escape from the top menu, selecting a real Markdown file without modifying bytes, FileBrowser cancellation back to the menu, GitHub input, HTTPS input, rejecting query-bearing URLs, non-interactive console refusal and extension checking for both supported and unsupported names.

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
```

The previous user-verified baseline was **6,443/6,443 green on Windows**, before this interaction update. The new tests and source code are committed but have not yet been executed in this environment (no .NET SDK). Manual verification is needed for Windows Terminal, Debian shell and SSH terminal quirks.

## Non-goals

Local and HTTPS choices still use the original one-shot reader; the **GitHub menu choice now opens the interactive link browser**, including source/fragment Back/Forward and heading display jumps. This browser is limited to 1 MiB per Markdown file and uses the existing verified cache. True streaming Markdown pages, unrestricted cross-source navigation and exact terminal scrollback positioning remain future milestones.

[mdview README](../../README.md) · [CLI usage](../CLI/en.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md).

## GitHub owner-only discovery (2026-11-04)

The GitHub input prompt now also accepts only a user or organization name (for example `afrowaveltd`) in addition to `owner/repository`. With an owner alone, the next stage opens the [public GitHub repository browser](../GitHub-Owner-Discovery/en.md), with a short tree, searchable/paged Select prompt and explicit Escape cancellation. Its catalog is cached separately from README contents. The source-selection prompt itself does not send any HTTP requests.


## Main menu GitHub navigation correction (2026-11-11)

The menu originally returned the one-shot `MdViewSourceKind.GitHub`,
which rendered the README but never offered `Follow a link`. This was
the cause of a user-visible report that GitHub README links were missing.
The menu now returns `GitHubLinkBrowser`, whereas direct
`git OWNER REPOSITORY` remains a single-page command.

When only the owner is entered, Program resolves a repository using
`MdViewGitHubRepositoryPicker.PickBrowseAsync` (including the
rate-limit/403 manual-name fallback), then starts the same browser.
Before rendering a long README the browser lists its **link count and
first three labels/destinations**; the full selection is through
`Follow a link` below the document. Rendered Markdown labels are not
directly clickable terminal hyperlinks.

Tests cover both menu forms, catalog selection and rate-limit fallback,
`MdViewSession` dispatch, and visible link preview. Newly added tests
require a fresh maintainer run.
