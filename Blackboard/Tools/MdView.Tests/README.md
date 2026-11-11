# mdview CLI tests

xUnit tests for command grammar and the independent Spectre.Console interactive picker. The picker tests use `TestConsole` with simulated keyboard input and temporary local Markdown files. No real network is used.

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
```

The original ten command-grammar tests passed in the user-supplied 6,443-green Windows baseline. Eleven new interactive picker cases cover Escape/back behavior, picking real Markdown, GitHub and URL entry, dangerous URL rejection, terminal capability and filename validation. The **new** tests are not yet executed/CI confirmed. Real-terminal visual verification and full command-to-render integration coverage remain pending.

[CLI implementation](../MdView/README.md).

## Owner-only repository chooser tests (2026-11-04)

Added command grammar cases for bare `OWNER`, `git OWNER`, `github:OWNER`, and explicit `local` behavior. The interactive source menu now also has an owner-only input test. New `MdViewGitHubRepositoryPickerTests` use `TestConsole` and fake HTTP to verify selecting repository names, filtering by description, cancelling without fetching any README, literal markup-safe labels, non-interactive rejection and 404 errors. These new tests await execution; the earlier green Windows baseline predates the change.

## Source pager tests (2026-11-04)

Added `MdViewSourcePagerTests` to verify the explicit raw UTF-8 source pager: Escape/exit, read-only literal output, next/back restoring the actual page start, direct jump to physical line, invalid jump feedback, refusal of noninteractive terminals, and rejection of unsafe source before any content is displayed. Command parsing also tests `source PATH`. These new cases await execution; the last user-confirmed full Windows baseline predates them.

## Document-end navigation and repeatable sessions (2026-11-04)

Added `MdViewDocumentCompletionPromptTests` (5 new cases) and `MdViewSessionTests` (8 cases), using the virtual Spectre `TestConsole`. They cover Enter-to-menu, Escape-to-exit, ignoring reserved search/pager keys, noninteractive completion without input, direct-command → menu → second document flow, cancelling the returned menu, preserving errors, suppressing prompts for unread/cancelled sources, and ensuring the raw source pager is not prompted twice. The session tests use in-memory delegate fakes and do not call the network or filesystem.

The last maintainer-confirmed full Windows baseline was green *before* this feature. These 13 new cases have **not yet been executed** in this environment.

## Disk-backed Markdown navigation (2026-11-11)

The new `MdViewDiskNavigationIndexTests`,
`MdViewDiskNavigationPickerTests` and additions to
`MdViewFormattedPreviewPagerTests` verify source-aware headings
and inline link positions in large validated Markdown snapshots.
Coverage includes fenced/indented/inline code, images, escaped links,
source UTF-16 offsets across mixed line endings, distant disk result
pages, disk budget failure recovery, long Unicode labels,
selector cancellation and navigation into local preview pages.
The index is created lazily by `mdview preview PATH` only when the
reader chooses Find heading / Find link location; the existing
GitHub browsing and normal Markdown rendering test paths remain
unchanged.

New tests are committed but **not yet maintainer-confirmed green**.
Run `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
and `dotnet test Toolbox.sln` before the next milestone.

## Responsive preview regressions (2026-11-11)

New `MdViewTerminalWordWrapTests` cover natural language break
priority, long words, Unicode cell widths, emoji safety and
code lines that are clipped rather than wrapped.
`MdViewTerminalPageIndexTests` cover viewport-specific page
counts, resize re-indexing, source positions within a single
very long line, code overflow, disk budget failure and bounds.
New `MdViewFormattedPreviewPagerTests` cases verify a visual
Page X of Y footer and code continuation messages; the
pre-existing source-line jump, fence and navigation scenarios
remain in place.

This source increment has **not yet been verified by a fresh
maintainer test run**. Run the targeted MdView tests and complete
Toolbox solution suite before considering this checkpoint green.
