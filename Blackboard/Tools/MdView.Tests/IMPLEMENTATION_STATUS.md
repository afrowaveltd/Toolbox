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

## 2026-11-08 — scanner lifetime cancellation regressions

- Added two tests for `MdViewContainerPageCursor`: cancellation of a
  successfully completed prior request cannot invalidate a future
  fence-aware forward read; an already-cancelled page request is rejected
  without poisoning an existing cursor.
- Source commit `0d8a8ac24c405480f4fff3dda7801466483614e4`,
  tests commit `5a7ae69e4d526c41656c9931a1568c81d3b18364`.
- Test execution pending; use `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and then `dotnet test Toolbox.sln`.

## 2026-11-09 — maintainer-confirmed green test checkpoint

- Maintainer reports **6,726 passing tests** after the mdview scanner cancellation-lifetime change and its regression tests. This is a maintainer-confirmed result, not an independently inspected CI run; test runner platform and detailed counts were not supplied.
- The tested change is production commit `0d8a8ac24c405480f4fff3dda7801466483614e4` and tests commit `5a7ae69e4d526c41656c9931a1568c81d3b18364`.
- Next: bind serializable scanner checkpoints to a validated source and enable bounded seeking to previously unseen Markdown sections; retain the existing journal and legacy viewer behavior until parity is tested.

## 2026-11-09 — opt-in checkpoint resume tests

- Three focused tests for resuming a fenced code block across independently
  reopened physical pages; reusing a checkpoint; mixed CRLF/CR/LF offsets,
  terminal EOF, and rejection of positions created by a different reader.
- Changes under test: `Utf8ValidatedSnapshot.OpenReaderAtByteOffset`
  (`57a0cecbba5b4491545ea9511fe50a3969fbc9fe`) and
  `MdViewCheckpointPageReader`
  (`d615853d8d6ada9492ff8f19dfeb2fdce96641b3`).
  Tests: `bf8c892977700f2c9f5f2a6a8f5583a934abc257`.
- Test execution **pending** after the earlier 6,726-green baseline.
  Verify with `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`.

## 2026-11-09 — sparse disk checkpoint index regression tests

- Added three focused tests for `MdViewDiskCheckpointIndex`:
  270-line mixed forward/backward reads across sparse code-fence
  boundaries, strict index disk budgets and invalid requests, and empty
  validated source behavior. Includes exact EOF when line count is a
  multiple of checkpoint stride.
- Source commit `05256266a6a92615018381c7037852c88c75ea28`;
  tests `2c4967efa33fa344d28e0ad9996f4fd1ed7a0141` and
  `e31ece89e2ac375585ec1e88d30fd5987db9f3a9`.
- **Pending Windows/Debian .NET verification.** Last user-confirmed
  baseline green predates the sparse index. Run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`.

## 2026-11-09 — sequential checkpoint construction regression

- Added `SequentialCheckpointBuildKeepsUnicodeAndMixedNewlineOffsets`:
  301 physical lines with Unicode and mixed CRLF/CR/LF, sparse stride 37,
  forward/back random positions, code fence classification and EOF.
- Test commit `6efabec11171dacc4a3de3e6ec3ae91fbac89634` checks
  production optimization `cb6b814bf2d3234c384e84c7e58b789e3ad01b3f`.
- **New test execution pending.** Next add pure source-origin/relative-link
  resolution tests per the documented navigator design.

## 2026-11-09 — relative-link resolver regression tests

- Created `MdViewLinkResolverTests` covering relative local
  README/Docs paths, upward navigation within root, escape rejection,
  HTTPS source URI resolution and absolute root paths, cross-origin/
  scheme/query rejection, GitHub nested paths and refs, anchor-only
  targets and unsupported percent-encoded links.
- Test commit `5279fab4e9bb03929db8e7d254df3262a7cb151d`,
  corrected deep GitHub path test `53ecda0e5b7aac91583a929dfe49f069986f0a48`.
  **Fresh execution pending**; run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`.

## 2026-11-09 — validated local linked document tests

- Maintainer confirms the preceding typed resolver baseline:
  **89 passing MdView tests and 6,745 passing solution tests**.
- Added five focused `MdViewLocalLinkLoaderTests` validating nested
  local README/Docs following, parent-relative navigation, extension
  and root restrictions, invalid UTF-8 and snapshot budget, and
  symlink/reparse rejection where the environment supports symlinks.
- Production commit `8d18686c8b9e46e0861286007a37883511c6386e`;
  tests commit `777b918bc0cb1a82e49f7a3eb63cefc84f1285ec`.
- These new tests have **not yet been confirmed**; run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`.

## 2026-11-09 — source-aware history and validated local navigation tests

- Added `MdViewNavigationHistoryTests`: bounded/branching
  source-neutral history, oldest eviction, relative local navigation,
  Back/Forward after validated snapshot loading, failed-link rollback
  and missing history destination rollback.
- Production commits `63f1eea7fca5b5916e61009cc7b0d950658d3a2d`
  and `2427df924e1bf521108717b20727f1bf45fd4045`;
  tests `c7696d3db911c8dc113846f9460f3c1d8aeeff7c`.
- New focused and full solution tests **pending**. Run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`.

## 2026-11-09 — navigation regression green checkpoint

- Maintainer reports green tests after the bounded history and local
  navigation regression cases were added.
- No specific post-change count or CI run ID was supplied. Previous
  independently recorded count must not be mistaken for this checkpoint.
- Next coverage: Markdown link extraction, escaped literal/code-fence
  exclusion, safe user selection and failed-open history preservation.

## 2026-11-09 — Markdown links and explicit picker tests

- Added `MdViewDocumentLinkTests` covering source-offset-aware
  nested links, rejected fenced and inline code, images, escaped syntax,
  external and unsafe targets, extraction budget, Spectre picker choice,
  Escape cancellation, empty catalog and noninteractive guard.
- Production commits `3bddf6f6e072faf5c1befe6099136efe58924364`,
  `96746b60374c2db9575781801b9f411e3072202a`.
  Test commits `0ba10326c6d0795866fdde1386ab4cd81f8b29fb`,
  `445a8f4bc08ea23667c9aea8f82691856e4fab74`.
- The preceding local-navigation test suite was maintainer-confirmed
  green. **The new link-extraction tests are not yet verified.**
  Run focused and complete solution tests before connecting the
  picker to a user-facing flow.

## 2026-11-09 — maintainer-confirmed green link discovery checkpoint

- Maintainer confirms **all tests green** after the opt-in Markdown link extractor, Spectre.Console link picker, and their new regression tests. No precise new counts or independent CI run are asserted.
- Verified-by-maintainer source commits: `3bddf6f6e072faf5c1befe6099136efe58924364`, `96746b60374c2db9575781801b9f411e3072202a`; test commits: `0ba10326c6d0795866fdde1386ab4cd81f8b29fb`, `445a8f4bc08ea23667c9aea8f82691856e4fab74`.
- Next: opt-in connection of explicit link selection to `MdViewLocalNavigationSession`, then regression-check actual document transitions and history before changing default CLI behavior.

## 2026-11-09 — opt-in local link browser terminal integration

- Added `MdViewLocalLinkBrowserTests`: explicit `browse` grammar,
  actual README → Docs link following, Back/Forward/Exit
  virtual-terminal sequence, recoverable missing Markdown target,
  code-fence-only link exclusion and noninteractive terminal rejection.
- Source commits `c29bb36afe71d48ce591fa97f7ef5f0bb53c67ea`,
  `cdacb01c0d27bbb5105ff283897155eda93549ef`,
  `a5a42d2d41bc22ea8111509f38b1b271fb6bf498`;
  test commits `43dd4090fbf8283f89b26677c2841fcafe695c6e`
  and `a7d60224c15d254facae7134450654076ba8a174`.
- **New tests not independently executed in this environment**.
  Last maintainer-confirmed full suite green predates these commits.
  Verify `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`.

- Windows handle-lifetime follow-up: `e35a0bc151717a8b72539c07e05e0b4c61d1b0d1` closes the snapshot reader before any Follow/Back/Forward transition can dispose the old snapshot; this avoids open-reader blocking deletion of the temporary source on Windows. Fresh tests still pending.

## 2026-11-09 — heading anchor navigation tests

- Maintainer reports preceding browser navigation increment functioning
  and all tests green.
- New `MdViewHeadingIndexTests` covers duplicate heading slugs,
  Unicode, fenced-code exclusion, missing heading lookup, bounded
  input, and virtual-terminal anchor navigation/feedback.
- Production commits `d482d5a6a6dcd6b224307fb4a4cac6c10079a2dd`,
  `17a92b687ab6ff738aa707251863cba941b5a9e2`;
  tests `a0fb36799521561c962ba57f0567a428d7f3d368`,
  `c071d0e31d19ef9b6d94679fb57aebcbaa9347a4`.
- New tests **not yet run** in this environment. Focused/full suite
  and Windows/Debian browse smoke checks required.

## 2026-11-09 — heading anchor test input regression fixed

- Debian SDK 11 RC test log: **112 passed / 1 failed / 113 total**. The only failing case was `FragmentOnlyLinkStartsRenderingAtTargetHeading` due to a depleted `TestConsoleInput` queue, not a compiler or Markdown parser error.
- The post-Back original document has **Follow, Forward, Exit**; the previous script used one DownArrow and selected Forward. Added the missing second DownArrow to select Exit (commit `8b6884e26a7ef5766815e84c2c928bb8f34caa10`). Production implementation unchanged.
- Retest focused and complete solution suites to verify. The `NETSDK1057` preview notice is informational.

## 2026-11-09 — same-origin HTTPS navigation and offline cache regression

- Maintainer confirms full tests green after the previous
  heading-anchor input-sequence fix; this is the baseline preceding
  HTTPS implementation.
- Added `MdViewHttpsNavigationTests` in
  `e9e987a251fd3a69abd08490b2599d9fb4c664fe`.
  Cases cover explicit `browse-url` parsing, remote relative path
  and Back/Forward requests, anchor-only navigation without refetch,
  unavailable remote target rollback, stale offline cached copy,
  interactive Spectre linked-doc navigation and noninteractive guard.
- Production commits:
  `d63ad06e868ca2a2636a3eb21d314e5b36c6451b`,
  `4211f92b1fcabd76927f4bcac04fdf1f5f1d2054`,
  `ec9c07d11f0fa4ef3587b25a69ffc4ed019e40ca`,
  `0e3a542a7c1cace0d747ab46b7aff0a229e36be1`.
- **Focused and full test execution still pending for this change.**
  GitHub branch/path provider tests will be a future separate batch.

- Follow-up HTTPS navigation safety regression commit
  `e1ae4cee8ce4fd421c7f6921e8b3b2e1d85b750e`
  adds oversized-document rollback and cross-origin request rejection.
  New tests have not yet been verified by the maintainer.

## 2026-11-09 — interactive GitHub file navigation tests

- Maintainer confirmed preceding mdview HTTPS navigation fully green.
- Added seven `MdViewGitHubNavigationTests`
  (`ad8d7a445754789834216c013b85a85bc362d117`):
  explicit `browse-git` grammar and preservation of original `git`
  mode, exact-ref repo-relative link following, metadata-derived slash
  branch, Back/Forward revalidation, same-file fragments without fetch,
  missing/oversize target rollback, virtual Spectre terminal traversal,
  and noninteractive guard.
- Production commits: `cd0a025b4f59efd27c420feefa0b37c04d1e30b5`,
  `498fccc1d2d8d0afbd52c05b2c45f7850a43bfb2`,
  `ae88207c13b00d1ab7e1aac57d1462b13da61684`,
  `8535579c933e6fd743b1a989ec6bda307e46dea6`.
- **Fresh focused/full test execution not yet confirmed.**
  Run `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln` then report any failures before next
  milestone.

## 2026-11-09 — Toolbox README-shaped link detection regression

- Maintainer smoke-tested `browse-git`: README displayed but
  navigable links were not apparent in the terminal.
- Added `ToolboxStyleReadmeLinksAreResolvedFromGitHubRepositoryRoot`
  test in `b895ecfc5eeeb61b1e9a08eb1a15cf2927cc43a5`:
  four representative repo-local links in documentation navigation
  and a Markdown project table must resolve under the `master`
  ref; an external GitHub website link must be excluded.
- Extended `MdViewGitHubNavigationTests` in
  `d2b4bb09551dab366d6351f15ae8269708c82e3a` to assert
  the displayed `Navigable Markdown links: 1` count and
  `Follow a link` action for its simulated README.
- Production UX commit:
  `528c83196516bb9e9c6a56e978d545913ddf4c11`.
- **Retest required.** New tests have not yet been confirmed;
  do not report them as green until the maintainer runs them.

## 2026-11-09 — GitHub catalog HTTP 403/manual fallback tests

- User observed a GitHub owner catalog HTTP 403 and confirmed
  an API rate-limit reset was required; this was not a parser failure.
- Picker test updates in
  `24a6e1b412920aa0999b354e8b7488e238831cdb`
  cover HTTP 403 manual repository selection without catalog retry,
  HTTP 404 cancellation, local rejection of malformed repository
  names, and manual selection after an empty catalog result.
- Production fallback implemented in
  `c1a52dc013340f4b7b6c8939fdf86717e475057d`.
- **Focused/full new tests unverified.** The manual prompt does
  not bypass GitHub API limits; subsequent README loads can still
  fail until the reported reset.

## 2026-11-11 — GitHub interactive menu link visibility regressions

- `MdViewInteractiveSourcePickerTests` now expects
  `GitHubLinkBrowser` for the no-argument GitHub menu
  with owner-only or direct `owner/repo` input; added explicit
  `git` one-shot compatibility assertion
  (commit `63230ff5553a5774b99c1a291f8e4d9cfb9a7cc6`).
- `MdViewGitHubRepositoryPickerTests` now checks
  `PickBrowseAsync` returns a browsable GitHub command after
  owner-only catalog selection and after HTTP 403 manual fallback
  (commit `740cd6a54f914de461470272df22420dad852dd9`).
- `MdViewGitHubNavigationTests` asserts that the browser displays
  both the parsed link count and **actual first-link labels**
  before rendering (commit
  `7a7af4799e4c0c967d71fb4e506e0509aa31eaf7`).
- `MdViewSessionTests` checks that the no-argument source menu's
  GitHub choice is dispatched as `GitHubLinkBrowser` and does
  not invoke a legacy post-render completion prompt (commit
  `09d2474862294e8d3da11574bfd19e2b227ac960`).
- The real user symptom was a non-navigable README opened from
  the original interactive GitHub menu despite `browse-git`
  existing as an opt-in command. No parser or cache behavior
  was changed in this increment.
- **Fresh focused/full tests pending.** Last maintainer-confirmed
  green suite predates these changes; run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln` to verify.

## 2026-11-11 — manual MemView GitHub browsing accepted

- Maintainer confirms an actual `afrowaveltd/MemView` documentation session renders correctly and navigates Markdown links. Screenshot shows an English Git lesson in the terminal.
- This observation supports **manual GitHub integration/UX acceptance** after the main-menu `GitHubLinkBrowser` fix. It does **not** confirm execution of the newly added focused/full xUnit suite; those remain separately pending until explicit test results are provided.
- Continue with `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj` and `dotnet test Toolbox.sln` for a new automated green checkpoint.

## 2026-11-11 — GitHub bounded source-page regression suite

- Six `MdViewSmallMarkdownPageIndexTests` were added:
  `d5e6b5ddeab7fe0209df836df5446aeb27e094ab` and
  `0d6d65a29fd02e8d59206d51054cf8e63e78dbad`.
  They validate physical-line page ranges, correct fence-code
  classification when a page starts inside C code, source-offset
  lookup including Unicode, long-line display budget,
  UTF-16 surrogate safety and invalid/budget input guards.
- Five new `MdViewGitHubNavigationTests` cases added in
  `2ce5a596dd19c2bed1187c9cebf457c8f9757dc4` and
  `024897d3a37d69a54c3ae6cda43212d192c53c3c`:
  a long README displays only 24-line pages, Next/Previous
  navigation reuses the same one-time verified GitHub fetch,
  Go to heading and Go to source line work, and Follow a link
  still opens a new Markdown file with Back history.
- Existing short README terminal scenarios intentionally retain
  the same menu order and full-document display.
- **New focused/solution tests not yet run or confirmed green**;
  previous MemView success was a separate manual acceptance check.
  Test with `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`, then smoke-test GitHub paging.

- Additional display hardening (no behavior change to navigation):
  `86afdc02e85860187a50b072f0fb441124eb4647` bounds displayed
  heading-choice labels and `1081d13cb6e16df2dffb010d1d261ca7ecc5d348`
  avoids splitting UTF-16 surrogate pairs when shortening labels.
  Regression tests remain pending execution.

## 2026-11-11 — Unicode page truncation test failure and correction

- Reported Debian test run: **147 passed / 1 failed / 148 total,
  0 skipped**. Only
  `MdViewSmallMarkdownPageIndexTests.PageCharacterBudgetDoesNotSplitUnicodeSurrogatePair`
  failed (16,384 actual vs. 16,383 expected characters).
- Root cause: page renderer skipped half an emoji at the
  remaining UTF-16 budget boundary, but used its final code unit
  to emit one character from the next physical source line.
- Production fix: `fe9ccc8b734ac7b399f1fca5bbb3ddaa134c927f`
  terminates the page on truncated source line and never consumes
  a new source line that cannot display its first scalar.
- Test fix and new regressions:
  `b276c12c2de5344f3ae313e28038c8a717f8e850`
  keeps the original strict 16,383-char assertion,
  asserts that the next heading remains on the next page,
  and tests leftover-one-code-unit handling before an emoji.
- **Tests have not yet been rerun after the fix.**
  Verify focused MdView suite then complete solution; retain
  the 147/148 outcome as the last observed pre-fix state.

## 2026-11-11 — maintainer-confirmed green after Unicode page-boundary correction

- Maintainer confirms **no errors** after updating to the UTF-16 source-page boundary fix and new regression tests. This supersedes the previously reported 147 passed / 1 failed result; exact current test totals were not provided and are not inferred.
- Verified-by-maintainer source fix: `fe9ccc8b734ac7b399f1fca5bbb3ddaa134c927f`; regression tests: `b276c12c2de5344f3ae313e28038c8a717f8e850`.
- Next milestone: disk-backed heading/link indexing for large Markdown files, building on validated snapshots and existing disk line/scanner checkpoint indexes; retain the 1 MiB bounded GitHub rendering cap until a genuinely streaming navigation path is tested.

## 2026-11-11 — disk-backed Markdown navigation tests added

- Previous `MdView.Tests` suite confirmed green by maintainer after
  Unicode page boundary regression fix. New tests below pending.
- Added `MdViewDiskNavigationIndexTests` covering top-level
  headings, nested and quoted inline links, fenced/indented/inline
  code exclusion, image exclusion, distant disk result pages,
  mixed line-ending source offsets, bounded disk space rollback,
  Unicode label clipping, oversized destination rejection,
  argument guards and post-disposal read rejection.
- Added `MdViewDiskNavigationPickerTests` covering disk result
  pages beyond the first 12 items, Escape without invalidating
  the index, empty catalogs and noninteractive rejection.
- Expanded `MdViewFormattedPreviewPagerTests` with virtual-terminal
  scenarios to jump to a distant heading and a link's source line,
  and to return from an empty heading catalog without losing the
  current validated preview.
- Implementation commits: `6ed7131b6502a6712e5a6a96ad835dc0a572ade2`,
  `05d27fa6068de1c24c56ae7730c5a564bde15734`,
  `47e56f1112983f1c53d88a0b4615ee3b3252e138`,
  `77706689f963a746f5a55d32720cae02e29ebeb6`.
  Test commits: `51a622bbba1dea415d7104e96a86d62911d7695e`,
  `e86e8a63e5511cd52c5bb2c1b1d5faf0d53e9822`,
  `6d0d3482e2d9afbade716c2f4cb9a58d70756b91`,
  `262df9bb5ce89da3ad1cfaad508afff40ff15a9b`,
  `750bca818071a7b5d9e04a50271b6ecc873656ba`.
- **New test execution not yet confirmed.** Run focused MdView tests,
  then full Toolbox.sln; verify manual long-file source jumps.

## 2026-11-11 — adaptive terminal page regression checkpoint

- Added five `MdViewTerminalWordWrapTests` covering punctuation-
  aware sentence/clause wrapping, fallback word splits, no-wrap
  code clipping `→`, combining/emoji/CJK handling and
  argument validation; source/test commits
  `c72f5f0df7811441c1184d9a73c7ccdfea4c2a61`,
  `6a607adcb02e395b8b9e2cc7e0e3c0dbc219d619`.
- Added five `MdViewTerminalPageIndexTests` in
  `259eebca8119b08791bf105d11974e96a05b0368`:
  different viewport size yields different page count,
  extremely long physical prose line resumes at nonzero
  wrap offset, code stays one row with overflow indicator,
  page index failure leaves validated input intact, and
  invalid/disposed page positions are rejected.
- Added two `MdViewFormattedPreviewPagerTests` cases in
  `7752320d5738afee1b0c0c113ab9ee389cfbdb0c`:
  Page X of Y + physical range in narrow viewport, and code
  overflow without changing source bytes. Corrected the
  expectation for an intentionally one-row-clipped header
  in `1d4b1961dab83af11963a767bd1c0e259fef7825`.
- Existing virtual-terminal source browsing, page history,
  lazy disk heading/link jumps and fenced code cases must
  remain green; no intentional change in these APIs.
- **These new tests have not yet been run or maintainer-confirmed
  green.** Execute focused MdView and full Toolbox.sln suites.

- An additional `MdViewTerminalPageIndexTests` case
  `FourSpaceIndentedCodeIsNotSoftWrapped`
  (`7f8a9b85148dc5bf1aefb0da58cc0bf6f687f31c`)
  confirms that conservative indented source code is clipped
  rather than soft-wrapped. The preceding responsive tests
  are still pending independent execution.

- Height adaptation follow-up: `3004bfc4fb2af9a3789343b79a31b2f8ac3c9de5`
  and `8a6defbeb986cb254f147d813959bce44cb4c3a1`
  remove the old fixed 24-row default; automatic pages can grow
  up to 128 visual rows. Test `996a03c2ab7abccf3520b853ad374411722b940d`
  compares narrow, wide, tall and explicitly capped viewports.
  Fresh .NET test verification remains pending.

## 2026-11-12 — two responsive-preview assertions corrected

- Reported `MdView.Tests`: **174 passed, 2 failed, 176 total,
  0 skipped**.
- `CodeLineClippedWithArrowIsOneVisualRow` now expects a
  formatted `Ending` heading rather than raw `# Ending`
  source syntax, and asserts the literal marker is absent:
  `ff2411594f620b618dafad085cd48064dfd074b1`.
- `CodeOverflowIsMarkedButCodeSourceRemainsUnchanged`
  now asserts the width-appropriate short status line for its
  31-column terminal instead of an uninterrupted long sentence:
  `a33ecffbf780947b060059e2142bd288b1c6bbbf`.
- Added `WideTerminalShowsFullSingleLineCodeOverflowExplanation`
  to preserve the complete warning in sufficiently wide windows:
  `679628725495c72a8c91fde21ca2770edd42e6b5`.
- Production width-aware warning logic:
  `c1889b53d3f62de5c7372674b54ccac1d6b9c4f3`,
  `d58d11d2be89b1ee8ca803a55b8cb6003173ccd4`.
- **Fresh tests are pending.** Prior 174/176 result remains the
  observed pre-fix state, not evidence of a new passing run.

## 2026-11-13 — maintainer-confirmed green responsive-preview checkpoint

- Maintainer explicitly confirms the latest test runs are **green** after both prior responsive-preview failures were addressed. This supersedes the last observed pre-fix 174/176 result.
- The exact new total was not provided; do not claim a fabricated count or a separate CI result.
- Covered correction commits: `ff2411594f620b618dafad085cd48064dfd074b1`, `a33ecffbf780947b060059e2142bd288b1c6bbbf`, and added wide-terminal test `679628725495c72a8c91fde21ca2770edd42e6b5`. Production warning fixes: `c1889b53d3f62de5c7372674b54ccac1d6b9c4f3` and `d58d11d2be89b1ee8ca803a55b8cb6003173ccd4`.
- Continue from this maintainer-verified checkpoint; add regression coverage for future terminal layout changes.

## 2026-11-13 — shared inline/container visual row test checkpoint

- The previous responsive preview suite was explicitly reported
  green by the maintainer. This new increment is **not yet run**.
- Added nine `MdViewTerminalLineLayoutTests`:
  long bullet item keeps one bullet with hanging indentation;
  wrapped block quotes keep a gutter on each row;
  inline link visible label keeps the Link style across wraps
  without counting its hidden destination; strong and inline
  code styling survive wrapping; quoted ordered lists retain
  correct marker/continuation prefixes; fenced code stays
  literal; escaped/malformed markup stays safe; quoted links
  retain styling; and the renderer's Next position agrees
  with disk-indexed page starts through a multi-page
  document combining lists, links, quotes and headings.
- Test sources: `8bfe2de61757c609d052eeee6afa0cb82feba071`
  and `83b3372f4f870f9eefbe94153b0d88b597779c03`.
- Production now shares the exact row layout between
  `MdViewTerminalPageIndex` and `MdViewTerminalPageRenderer`.
  That alignment is an essential regression invariant for
  `Page X of Y`, source-preserving forward/back pages and
  long-line resume positions. The original word-wrap method
  also reuses the new UTF-16-safe visible slice implementation.
- **Verification pending**: focused `MdView.Tests` then
  full `Toolbox.sln`. Do not claim this increment green
  before the maintainer reports results.

## 2026-11-13 — maintainer test acceptance with one warning pending triage

- Maintainer confirms the new Markdown layout tests are **OK/passing** and the local reader works correctly.
- A single orange warning remains to be diagnosed from its verbatim message; **zero warnings is not confirmed**.
- No test totals or independent CI result were supplied. Preserve this as a reported functional/test acceptance, not a fabricated zero-warning verification.
- Once the warning is provided, add a targeted regression test if it reflects a code or analyzer defect.

## 2026-11-13 — orange warning resolved by .NET 11 final upgrade

- The maintainer identified the orange message as coming from a **preview version of .NET**. Upgrading the local SDK to **.NET 11 final** removed the warning.
- This is an **environment resolution**, not an MdView test or source fix. Previously reported tests were passing; no fresh count is inferred and no regression test is warranted for this SDK warning.

## 2026-11-13 — compact pipe-table visual regression checkpoint

- Added ten `MdViewTerminalPipeTableLayoutTests` in
  `0d1aa364035a51f2d6d551b10832fbb446b3cb71`
  and `63dab5bf84b2bc8320decfe833ae1d675fc4ad1f`.
- Scenarios validate: numbered cell rows for a framed Markdown
  table, separator divider, escaped and inline-code pipes as
  non-delimiters, unescaping ordinary pipes without changing
  inline code, preserving links and hiding destinations,
  ambiguous/unframed fallback, fenced code literal behavior,
  eight-column narrow terminals, 32-column bound, and agreement
  between disk visual-page starts and the renderer's next position
  across a multi-page table.
- Source: `MdViewTerminalPipeTableLayout`;
  `MdViewTerminalLineLayout.LayoutStyledContents` shared
  between ordinary text and compact cells. The page index
  and renderer already share `MdViewTerminalLineLayout.Layout`,
  preserving the existing source-position invariant.
- Last maintainer-confirmed test state was green **before**
  these changes. The new tests are **not yet confirmed**.
  Run focused MdView tests then full Toolbox.sln; record any
  failures with exact test names.

## 2026-11-13 — native Spectre table widget tests pending

- Eight new `MdViewSpectreTableViewerTests`
  (`603284757ba82a9a038b771ff0fcad023aa8f5ce`):
  matching Markdown header/delimiter discovery, rounded Unicode
  Spectre borders on wide terminals, transposed Column/Value
  view with field paging on narrow terminals, multi-chunk
  table navigation, literal safe handling of bracketed markup,
  visible long-cell clipping without modifying validated
  source bytes, exclusion of fenced pipe rows, and invalid
  source/noninteractive mode rejection.
- Three new `MdViewFormattedPreviewPagerTests`
  (`e060e79d203611417c2f2c7b1d67cef1bc4381bc`)
  verify that a detected table opens via **View table (Spectre)**
  and returns to the same Markdown source page, and that
  unconfirmed pipe text does not add a table menu action.
- The prior `MdViewTerminalPipeTableLayoutTests` continue
  exercising the same conservative `TryReadCells` parser
  and in-page compact fallback. No intentional changes to
  Unicode, source-line page counts, code-fence ownership
  or validated disk-index navigation.
- **Fresh test verification pending.** Run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`,
  then `dotnet test Toolbox.sln`. No post-change pass
  counts or CI claims are made.

## 2026-11-14 — native Spectre table test compilation fixes (maintainer build)

- The maintainer provided the latest `dotnet test Toolbox.sln` output. The solution's other test projects shown in the log passed, but `MdView.Tests` did **not compile** because `MdViewSpectreTableViewerTests.SourceFixture` had two `CS0236` diagnostics at line 176. Its instance `Path` property shadowed `System.IO.Path` in the `_folder` field initializer.
- Fixed by using fully qualified `System.IO.Path.Combine(System.IO.Path.GetTempPath(), ...)` in `MdViewSpectreTableViewerTests.cs`; commit `ef6ee546b5513ef481cdd12ca4eb124a49531d14`. No production runtime behavior changed.
- The same compiler run reported one `xUnit2013` warning at `MdViewDiskNavigationIndexTests.cs:95`: `Assert.Equal(1, links.Count)` for a one-element collection. Replaced with `Assert.Single(links)`; commit `35e34fdc07bdd8c9228d8d3e3d911a9881d37624`.
- **These corrections have not been recompiled in this assistant environment.** Re-run focused `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj` and then `dotnet test Toolbox.sln`; only record a new green checkpoint after the maintainer reports it. Existing green results for other projects do not establish the new MdView tests are passing.
