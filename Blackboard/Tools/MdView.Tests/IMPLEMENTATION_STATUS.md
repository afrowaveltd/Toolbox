# mdview CLI tests — implementation status

## 2026-11-03

Created the xUnit `net10.0` test project and command parser regression cases. Project registered in Toolbox.sln. Actual .NET test execution remains unverified due to absent local SDK. Future increments: fake-server dispatch, real complete local file display and Windows/Debian terminal smoke testing.

## 2026-11-03 — user Windows test execution confirmed

- `MdView.Tests`: **10 passed / 10 total, 0 failed, 0 skipped** in the user-provided `dotnet test Toolbox.sln` log. Source and command-grammar test project build succeeded on Windows with the .NET 11 preview SDK targeting net10.0.
- Full CLI end-to-end/manual rendering and Debian CI still require separate checks.

## 2026-11-03 — interactive picker virtual-terminal tests

- Added eleven `Spectre.Console.Testing` cases for `MdViewInteractiveSourcePicker`: Escape at source menu, local Markdown file selection without mutation, FileBrowser cancellation/back, GitHub owner/repo, HTTPS URL, rejecting query-bearing URL, refusing non-interactive consoles, and 4 filename extension cases.
- Added test dependency `Spectre.Console.Testing` 0.57.2. The original MdView command parser test baseline was **10/10** green; with new tests, expected count is **21**. Verification of the new suite is pending in Windows/.NET and CI; no successful run claimed yet.

## 2026-11-04 — default JavaScript highlighter integration

- Added `MdViewHighlightingTests`: confirm mdview's render theme installs TextMateSharp, JavaScript produces styled tokens and literal terminal-safe output, and unknown grammars remain readable. Existing CLI grammar + interactive-prompt tests remain unchanged.
- Test suite not yet rerun for this change; prior 6,443-green Windows baseline predates this feature and the earlier interactive picker.

## 2026-11-04 — GitHub owner-only command and chooser tests

- Added parser regressions for bare owner, `git OWNER`, `github:OWNER` and explicit local path priority; the interactive GitHub source prompt accepts an owner alone.
- Added `MdViewGitHubRepositoryPickerTests` using fake GitHub HTTP responses and `Spectre.Console.Testing`: choose repo, Escape without README fetch, filter by description, markup-safe labels, noninteractive terminal refusal and 404 error.
- Tests authored and committed; **new code has not yet been run** on the user's Windows or Linux toolchain.

## 2026-11-04 — raw source pager virtual-terminal regressions

- Added six `MdViewSourcePagerTests` using `Spectre.Console.Testing`: literal read-only source, Escape, next/back exact page start, one-based jump, invalid jump feedback, noninteractive refusal and invalid data rejection before display. Added a `MdViewCommand.Parse("SOURCE", path)` grammar test.
- Existing Markdown/full-document tests remain intact; source mode is explicitly raw UTF-8 rather than a formatted streaming renderer.
- **Compilation and new tests unverified** without a local .NET SDK. Prior Windows 6,443-green result was for an earlier snapshot.

## 2026-11-04 — end-of-document key actions and session-loop regression tests

- Added `MdViewDocumentCompletionPromptTests` with 5 cases: Enter, Escape, ignored reserved future keys, ignored unrelated keys, noninteractive immediate return.
- Added `MdViewSessionTests` with 8 cases: Enter to main menu then second document, Enter then cancelled menu, Escape without menu, noninteractive one-shot execution, error code preservation, GitHub chooser cancellation without a phantom prompt, raw pager without duplicate completion, interactive startup.
- The 13 new scenarios use `Spectre.Console.Testing.TestConsole` without live network. **Awaiting full Windows/CI test confirmation** after sync.
