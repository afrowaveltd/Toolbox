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

## 2026-11-08 — experimental formatted preview regressions

- Added `MdViewCommandTests.ExplicitPreviewCommandIsOptInAndCaseInsensitive` and four `MdViewFormattedPreviewPagerTests` for heading rendering, source-preserving snapshot use, Next/Previous navigation, rejection of unsafe terminal controls and noninteractive guard behavior.
- Code commits: `4d31301ff9b46f27cdbf3af5841e4da44fa57d2c`, `754466f97251295dfaee04421b4f86952ca3c398`, `4fba512fe79801f25902379b295e05caf58fc6a2`; test commits: `ddb56e93d02538075624464d81865015fb577556`, `cae0180021afccd2d9624a2e80ae7d5cdeb99484`.
- **Fresh test/build execution pending.** These tests cover independently parsed source fragments only, not stateful cross-page Markdown or screen-row fitting.

## 2026-11-08 — fix test-console navigation sequence

- Windows xUnit feedback reported `MdViewFormattedPreviewPagerTests.NextAndPreviousPreviewPagesKeepSourceNavigation` throwing `InvalidOperationException: No input available` in the `Go to source line` TextPrompt.
- Root cause is in the simulated input: on the last page the `Next page` menu entry is omitted, so `Previous page` is the **first** choice. The test's extra `DownArrow` selected `Go to source line` instead, with no following text input.
- Removed the extra key and added a separate `GoToSourceLineReadsRequestedPage` test that explicitly supplies line `3`. Commit `191b304252daadd6e1bbf3fa4a545bcdfa5f4613`.
- **Fresh test execution pending.** No production pager behavior changed. Re-run `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj` and the solution suite; count remains unverified until then.


## 2026-11-08 — cross-page fence lifecycle regression coverage

- Added `FenceCrossingPageBoundaryKeepsHeadingLookingCodeLiteral` and `ReturningToEarlierPageReplaysScannerWithoutLosingFenceContext` against the opt-in preview CLI using TestConsole.
- These exercise literal Markdown-looking lines inside a fence across the 2-line source-page boundary and scanner replay after page navigation.
- Test commit `48bf8bc47064741cdc810f3fd279f9d7c147cc49`. **Not yet executed** in this environment; rerun focused mdview tests and full solution.


## 2026-11-08 — disk-backed container event journal tests

- Added five `MdViewContainerPageCursorTests` for previously scanned reverse-page reuse, forward-after-backward code fence continuation, failure on event byte budget exhaustion, failure on offset budget exhaustion and empty-source/argument checks.
- Cursor implementation commit `a30c6283dcaac88774c6c59a24e6e15fd11250ad`; test commit `487e50ea9d73e134b7a13cf1761d291ad1b736ad`.
- This verifies a cached **classified event journal**, not restorable scanner iterator state or fast jumps into previously unseen ranges. New test execution pending; no verified total pass count for this increment.

## 2026-11-08 — Maintainer-confirmed Windows green checkpoint

- Maintainer reports the Windows Toolbox test run is **still green** after the disk-backed classified-event journal implementation (`a30c6283dcaac88774c6c59a24e6e15fd11250ad`) and its focused regression tests (`487e50ea9d73e134b7a13cf1761d291ad1b736ad`).
- This is a user-confirmed test result. No detailed test count, run artifact, or Linux verification was supplied; do not infer any of those.
- Next planned step: design restorable, serializable incremental scanner state with correct fence/list/quote parity, independent budget controls and regression coverage before enabling arbitrary-position checkpoint seeks.
