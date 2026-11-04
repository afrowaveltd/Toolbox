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
