# ExchangeHouse.Markdown.Tests — Implementation status

## 2026-10-29 — initial regression suite

- Added xUnit project to `Toolbox.sln` covering first-stage Markdown parser/writer behavior.
- Local SDK is unavailable in this execution environment. **Verified:** [GitHub Actions #40846902206](https://github.com/afrowaveltd/Toolbox/actions/runs/40846902206) SUCCESS for commit `3252650b` (whole solution build and test including this project).
- Current suite is intentionally scoped to the block scanner. Add independent tests before extending syntax support.

## 2026-10-29 — inline scanner tests

- Added a separate `MarkdownInlineParserTests` suite for conservative inline syntax and source-safe integration with existing blocks.
- The suite is part of the same solution CI; verification for the latest implementation commit is pending.

### Verified CI — 2026-10-29

- [GitHub Actions #40848846720](https://github.com/afrowaveltd/Toolbox/actions/runs/40848846720) **SUCCESS** for inline parser commit `b566ad670bb58e52049c714b8dbba6d1f3838287` — full solution build and test passed.
- This verifies the conservative supported syntax and regressions only. Full CommonMark, editable AST, conversion and UI rendering are not yet claimed.

## 2026-10-29 — analyzer cleanup

- Use xUnit `Assert.Single(collection, predicate)` instead of filtered LINQ `Where` to eliminate xUnit2031 without changing test semantics. CI pending.

### Verified on both platforms — 2026-10-29

- [GitHub Actions #40857564421](https://github.com/afrowaveltd/Toolbox/actions/runs/40857564421) **SUCCESS** for code/test commit `ae52a29345f0fcbef08cbf6cda41686461bc25bd`.
- Ubuntu full solution restore/build/test **PASS**; new Windows runner's **FileBrowser**, **UnderstandMe Configurator**, and **Blackboard Showcase** project test steps all **PASS**.
- The original Windows UI path-wrap failures are resolved in the automated Windows environment. Manual developer-local retest with the latest checkout remains distinct from Windows CI.

## 2026-10-29 — image/link identity test cases

- Added image, title, angle destination, escaping, nested link/image, malformed input and destination offset regressions; CI pending.

### Verified cross-platform CI — 2026-10-29

- Parser implementation commit `fb7a8108`: [GitHub Actions #40881801829](https://github.com/afrowaveltd/Toolbox/actions/runs/40881801829) **SUCCESS** — full Linux solution build/tests and existing Windows regression job.
- Image presentation implementation commit `7696fef8`: [GitHub Actions #40882016971](https://github.com/afrowaveltd/Toolbox/actions/runs/40882016971) **SUCCESS** — full Linux solution build/tests and Windows FileBrowser, Configurator, Showcase, localization **plus newly added focused ExchangeHouse Markdown and Blackboard Markdown tests**.
- No direct manual Debian or Windows terminal test of this new image fallback has been reported. Full CommonMark, reference-style links, actual image drawing and translated document link routing are still not implemented.

## 2026-10-31 — nested list parsing tests

- Added focused tests for immutable list hierarchy, mixed nested ordered items and original markers, tabs, reset boundaries, source/inline spans and rejected unsupported markers. CI pending.

### Cross-platform CI verified — 2026-10-31

- [GitHub Actions #41244480220](https://github.com/afrowaveltd/Toolbox/actions/runs/41244480220) **SUCCESS** for implementation up through commit `415065a4c272838f3bc0e4ed94ab8ea90003a295`.
- Linux: full Toolbox solution **build and tests passed**; final build reported **0 warnings, 0 errors**.
- Windows: **ExchangeHouse.Markdown.Tests 41/41**, **Blackboard.SpectreConsole.Markdown.Tests 13/13**, **Blackboard.Showcase.Tests 26/26**; FileBrowser 160/160, Configurator 44/44, and targeted localization tests 34/34 passed.
- Intermediate runs on parser/renderer commits failed at **test compilation** due to literal `\\`` C# escape sequences mistakenly introduced when authoring Markdown code examples. Fixed in commits `e7b858aa` and `415065a4`; the final CI is green. Existing production source was not weakened to make tests pass.
- Manual real-terminal testing of newly added nested-list visuals in Debian/Windows remains unreported. The current model does not claim CommonMark-compatible loose/multiline lists.

## 2026-10-31 — list continuation regression cases

- Added five cases for continuation ownership, CRLF/source offsets, ancestor after child, numbered/tabbed indentation, interruption and code protection. CI pending.

### Verified CI — 2026-10-31 list continuation increment

- [GitHub Actions #41248289922](https://github.com/afrowaveltd/Toolbox/actions/runs/41248289922) **SUCCESS**, source through commit `35efe934031a99a5407550f6624d96656b9c03a3`: Linux full solution build/tests passed; Windows ExchangeHouse Markdown **46/46**, Blackboard Markdown **16/16**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**.
- Markdown local link job **PASS**: 269 Markdown files, 1,374 local file targets checked with none missing. Source-aware continuation behavior also tested on Windows; manual Debian and real-terminal visual checks remain separate.
- Initial renderer integration CI failed **one incorrect test expectation** (custom indent `....` erroneously checked as six literal spaces). Fixed the assertion in `35efe934`, retaining custom-indent coverage. The failed earlier CI runs are not counted as successes.
- This milestone handles only **explicitly indented** continuation lines. Loose lists, lazy continuation and nested CommonMark block containers remain unsupported. No public NuGet release was made.

## 2026-10-31 — bounded loose-list regression tests

- Six new cases check separated paragraphs, marker boundaries, offsets, ancestor ownership and literal code. Existing explicit-continuation blank-boundary fixture now uses unindented text, so it still tests the unsupported case. CI pending.

### Cross-platform verification — 2026-10-31

- [GitHub Actions #41249127101](https://github.com/afrowaveltd/Toolbox/actions/runs/41249127101) **SUCCESS** for source/docs commit `df4da1b285d280cbe1d17dee0fded388f178d0b8`. Linux full solution built/tested; Windows ExchangeHouse Markdown **55/55**, Blackboard Markdown **19/19**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localized tests **34/34**.
- Blank-separated explicitly indented list paragraphs now have source-preserving ownership and terminal alignment. Interim custom-indent test assertion was fixed in `2b34cb46`, with no production rendering change.
- Markdown docs link job passed. No public package release, full CommonMark claim or manual Debian visual acceptance made.

## 2026-10-31 — lazy paragraph continuation regression cases

- Added five focused cases for unindented inline/source offsets, nested ownership, blank boundaries, block-starter interruptions and mixed explicit/lazy/loose paragraphs.
- Previous no-lazy fixture was adjusted to include a blank paragraph boundary, retaining its intended non-attachment guarantee.
- **CI pending**; prior 55/55 suite remains the last independently verified parser result.

### Final green lazy-paragraph checkpoint — 2026-10-31

- [GitHub Actions #41250061417](https://github.com/afrowaveltd/Toolbox/actions/runs/41250061417) **SUCCESS** for commit `f6b7fc555c76d964ec12a27f86accc1beab3e9f8`: Linux full solution build and tests passed, Windows targeted UI/Markdown suites passed and local Markdown documentation-link check passed.
- Windows counts: **ExchangeHouse Markdown 60/60**, **Blackboard Markdown 21/21**, **Showcase 26/26**, **FileBrowser 160/160**, **Configurator 44/44**, **UnderstandMe localization 34/34**.
- Documentation audit: **273 Markdown files, 1,399 local targets checked**, zero missing links. Cross-family link typo from the first commit was fixed in `2c8c3fa`; the final gallery sample now uses an actually unindented lazy line.
- No public Markdown NuGet publication, no full CommonMark or real-terminal Debian acceptance is implied by CI.

## 2026-10-31 — code/quote regression cases

Added parser tests for source-aware language hints, fenced-code isolation from Markdown, nested list parent/child ownership, quotes/inline offsets and blank-separated containers. An older top-level quote expectation was updated to `BlockQuote`. CI pending for final implementation.

### Cross-platform green verification — 2026-10-31

- [GitHub Actions #41251065292](https://github.com/afrowaveltd/Toolbox/actions/runs/41251065292) **SUCCESS** for source commit `5b80ad1e3f9cde50ae793da53b3b81c30d727d08`. Linux complete solution restore/build/tests passed, Windows targeted UI and grammar tests passed, and Markdown link validation passed.
- Windows: **ExchangeHouse Markdown 66/66**, **Blackboard Markdown 24/24**, **TextMate adapter 6/6**, **Showcase 26/26**, **FileBrowser 160/160**, **Configurator 44/44**, **UnderstandMe localization 34/34**.
- The documentation link audit examined **280 Markdown files and 1,423 local targets** with no missing targets. A later docs-only commit clarified a nested-fence illustration; final docs CI is tracked separately.
- Real TextMate C# tokenization was exercised under Linux and Windows CI, including platform-specific Oniguruma dependencies. This does **not** establish manual SSH/tmux or Debian terminal acceptance, complete CommonMark support, NuGet publication, or a completed third-party transitive-license review.

## 2026-11-01 — quoted list parser regressions

- Five new tests for nested ordered/regular quoted lists, exact source roundtrip and offsets, explicit/lazy continuation ownership, quote-depth/list boundaries, heading/blank interruption and conservative quote-prefixed fenced-code fallback. CI pending; prior independently verified parser count 66.

### Verified cross-platform quoted-list milestone — 2026-11-01

- [GitHub Actions #41251673070](https://github.com/afrowaveltd/Toolbox/actions/runs/41251673070) **SUCCESS** for feature commit `aa3ec1246fc3125f17d1a506e415cc3ad7db418b`: Linux full solution build/test, Windows targeted UI tests and documentation-link job passed.
- Windows results: ExchangeHouse Markdown **71/71**, Blackboard Markdown **26/26**, optional TextMate **6/6**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**.
- The subsequent documentation commit introduced one incorrect cross-family relative link, fixed in `1e89cf7532c03737a0599d51c26b54cd57bb345a`. Link verification on the fix run passed; final complete CI for the documentation state is tracked independently.
- No new NuGet publication, full CommonMark conformance, or real-terminal acceptance is claimed. This feature intentionally does not tokenize fenced code inside explicitly quote-prefixed containers.

## 2026-11-01 — quoted fenced code coverage

- Tests for literal Markdown markers inside code, root and numbered-list quote ownership, multiline CRLF preservation, closer marker length and conservative quote interruption.
- The first CI run failed an incorrect test count (expected 5 instead of actual six opener/body/closer blocks), fixed in `836914ba`. [Verified implementation CI #41254427784](https://github.com/afrowaveltd/Toolbox/actions/runs/41254427784) **SUCCESS**.

### Full cross-platform CI checkpoint — 2026-11-01

- [GitHub Actions #41254717060](https://github.com/afrowaveltd/Toolbox/actions/runs/41254717060) **SUCCESS** for final code/English-docs fix commit `8f25d8e2a72df6ffe915362ef11d64dbd6c89d6a`: Linux complete solution build/tests, Windows targeted UI/Markdown tests and repository local documentation-link validation passed.
- Windows focused tests: ExchangeHouse Markdown **75/75**, Blackboard Markdown **29/29**, Markdown.TextMate **6/6**, Blackboard Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, UnderstandMe localization **34/34**.
- Markdown local-link checker verified **284 files / 1,452 local targets**, zero broken links. An earlier documentation-only CI failed one wrong Showcase-to-renderer relative path, fixed in `8f25d8e`; the final checkpoint above is green.
- No new NuGet release or manual real-terminal Windows/Debian acceptance claimed.

## 2026-11-01 — loose quoted list parser regressions

- Test multi-blank CRLF ownership and exact inline links, nested-child to parent resumption, quoted paragraph/list boundaries, quote-depth/unquoted boundaries and distinction between quote separators and code-fence literal blank lines.
- CI pending. Previous verified parser count was 75/75.

### Verified cross-platform milestone — 2026-11-01

- [GitHub Actions #41256065289](https://github.com/afrowaveltd/Toolbox/actions/runs/41256065289) **SUCCESS** on source/documentation commit `882f15862d89214e8cc1204d8b5b7baaedd6d18a`: Linux complete solution build and tests, Windows UI/Markdown targeted tests and local Markdown documentation links all passed.
- Windows tests: ExchangeHouse Markdown **82/82**, Blackboard Markdown **31/31**, TextMate adapter **6/6**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**.
- Markdown checker processed **286 files / 1,470 local targets**, with zero missing local links.
- The test-preserving increment has not been published to NuGet and has not undergone manual Debian/Windows/SSH visual acceptance; full CommonMark nesting remains outside this milestone.

## 2026-11-01 — quoted sublist under ordinary list tests

- Added six cases covering distinct outer/inner source owners, nested numbered list parent, separated runs, quoted fenced code, quoted loose separators and existing plain quote behavior.
- Initial failing tests caught ambiguity of standalone quote detection under a regular item and a test's too-small child indent; both corrected before verified green CI.
- [CI #41257477307](https://github.com/afrowaveltd/Toolbox/actions/runs/41257477307): **88/88 passed**.

## 2026-11-04 — incremental line-scanning regression suite

- Added `MarkdownIncrementalLineScannerTests.cs` (16 test cases: 13 Facts and 3 Theory rows). These exercise streaming laziness, exact source offsets and line endings, top-level fenced-code state/language, heading parity with the current full parser for flat examples, literal nested-container lines, Unicode, cancellation, bounded line length and late unsafe controls.
- Existing parser and renderer tests remain unchanged. New tests have **not yet been executed** in this environment; after syncing, run `dotnet test ExchangeHouse/Markdown.Tests/Afrowave.Toolbox.ExchangeHouse.Markdown.Tests.csproj` and the full solution. The stream scanner is opt-in and cannot yet replace the full Markdown parser.

## 2026-11-07 — xUnit analyzer warnings eliminated

- Replaced two `Assert.Equal(1, collection.Count)` checks in `MarkdownParserTests.Quoted_fence_interruption_reclassifies_unquoted_and_differently_nested_lines` with `Assert.Single(collection)`. This preserves and strengthens the assertion semantics while addressing `xUnit2013`.
- [GitHub Actions run 42311109763](https://github.com/afrowaveltd/Toolbox/actions/runs/42311109763) confirms fresh Linux build **0 warnings/0 errors** with test suites passing and Windows UI regressions green.

## 2026-11-07 — block span regression coverage

- Added `MarkdownIncrementalBlockScannerTests` (16 cases) with exact contiguous UTF-16 source spans and line offsets; complete CR/LF/CRLF reconstruction, Unicode, huge code/paragraph regions, unterminated fence metadata and conservative opaque quote/list behavior. Also asserts early-event laziness and propagates cancellation/unsafe controls.
- Compared flat heading/blank/paragraph/fenced code output with existing `MarkdownParser.Parse` on a restricted golden input. Test is intentionally not proof of full CommonMark or nested container parity.
- **Fresh CI results pending** for this new change. Do not state a new green total until verified.


### CI verification — 2026-11-07 completed block spans

- [GitHub Actions run 42313009028](https://github.com/afrowaveltd/Toolbox/actions/runs/42313009028) **SUCCESS** on the test-implementation commit `34ad5e5d8d0da6b720230711a91a3984a0bc8383`. Full Linux build reported **0 warnings, 0 errors** and passed all test assemblies, including **ExchangeHouse.Markdown.Tests 111/111** (the new 16 block-span cases included).
- Windows UI regression job and Markdown local link validation also **SUCCESS** in the same run.
- Subsequent changes are documentation/status updates only. No full CommonMark parity, manual terminal rendering or multi-gigabyte performance claims are implied.

## 2026-11-07 — quote/list streaming scanner tests

- Added `MarkdownIncrementalContainerScannerTests` with **18 test cases**: nested quote depth; nested ordered/unordered list markers; provable indented parent/child continuations; code-fence protection and interruption on quote-depth changes; quoted list-owned fences; source/UTF-16 offset fidelity; avoiding stale owners; tab stops, marker bounds, incremental reading, cancellation and invalid controls.
- Test fixture includes a **restricted parity assertion** for explicitly marked list/fence lines against `MarkdownParser.Parse`. Do not use it as proof of full nested CommonMark equivalence.
- CI verification: pending after feature/test commit; do not claim a green result until the full workflow completes.

### Verified CI — 2026-11-07 explicit container scanner

- [GitHub Actions #42314716633](https://github.com/afrowaveltd/Toolbox/actions/runs/42314716633) **SUCCESS**, including the feature and all **18 new explicit-container tests** after correcting the test's hand-computed CR/LF fixture offsets. Linux clean solution build: **0 warnings / 0 errors**; test assemblies: **6,603 passed, zero failed**. ExchangeHouse Markdown tests: **138/138**.
- Windows UI regression job **SUCCESS**, Markdown local link checker **SUCCESS**. The implementation and test behavior are therefore verified; subsequent README/status updates are documentation-only.
- Scope remains conservative: explicit markers, quote depth and quoted fences only, with no full CommonMark/list ownership parity or visual Markdown paging claim.

## 2026-11-07 — dual-owner scanner regression tests

- Added `MarkdownIncrementalQuoteOwnershipTests` (**17 cases**). Explicit ordinary-list and quote-local list owners are asserted independently, including nested list descendants, owner changes, quoted/unquoted boundaries, quoted fences, CR/LF/CRLF Unicode offsets and early streaming yield.
- Three cases enforce precise list-owned fence indentation: excessively indented code delimiters must not spuriously open or close a fence. Compares supported classifications with `MarkdownParser.Parse`.
- The first CI run of the new tests caught an incorrect expected root list depth (`-1` rather than `0`); corrected in commit `afc7d53dabaa09d5c0b11c7b10b61e0f6f9056ba`. Production implementation already returned the expected depth. Final post-correction CI run pending.

### Compatibility/test correction — 2026-11-07

- CI for the 17-case suite identified an incorrect assumption in the overindented-opener **test**, not a compiler error: `MarkdownParser.Parse` currently classifies an overindented list-owned opening delimiter as `CodeFence`, while the conservative incremental scanner returns `ListContinuation`. The test now documents/asserts **both actual behaviors** rather than pretending they match.
- This known gap is tracked in [Dual List Ownership](../Markdown/Docs/Dual-List-Ownership/en.md) and must be reconciled before adopting the streaming scanner as a replacement renderer.

### Verified CI — 2026-11-07 dual-owner and fence-indentation regressions

- [GitHub Actions #42317726747](https://github.com/afrowaveltd/Toolbox/actions/runs/42317726747) **SUCCESS** on commit `75f6e548da063fe95e39ab2f57d7f7049ecb3fd6`, including production dual-ownership changes, all **17** new quote-ownership cases, the exact-indent fix and the documented full-parser compatibility gap. Linux solution build/tests and Windows UI regressions passed; Markdown link checker passed.
- [GitHub Actions #42317702162](https://github.com/afrowaveltd/Toolbox/actions/runs/42317702162) on test correction commit `25b352ed32a1afe4507c850591ba3188dabe4ee4`: full Linux **6,620/6,620 tests green**, **0 Warning(s), 0 Error(s)**; Windows UI regression and local Markdown links **SUCCESS**.
- A mismatched root-list depth expectation in the initial tests was corrected. A subsequent test asserted false parity for an overindented opening fence; it was changed to record both real behaviors, without changing the existing full parser or renderer. The compatibility gap remains explicitly tracked for future reconciliation.
- The following README and status commits are documentation-only. The new scanner is an opt-in parsing layer and is **not** enabled in mdview.

## 2026-11-07 — deferred list ownership regression checkpoint

- Created `MarkdownDeferredListOwnershipScannerTests` with **20 cases**: simple and quote-owned loose-list blank separators, two independent owners, EOF and scope interruption, ordinary and quoted lazy continuations, heading/fence/thematic interruption, bounded pending-run limits, early emission and full-parser comparison on supported syntax.
- Adjusted the existing ordered-marker and standalone-quote tests to assert valid unindented continuation ownership, rather than the former generic text classification. Corrected thematic-break fixture to unambiguous `***`.
- Confirmed Linux full solution `6,640/6,640` tests green, zero build warnings/errors on commit `9822713461466a4cc2849c3963211075dc14bd27`. Windows UI regression and Markdown link checks are tracked in GitHub Actions.

### Verified CI — 2026-11-07 deferred list-ownership milestone

- [GitHub Actions run 42359532384](https://github.com/afrowaveltd/Toolbox/actions/runs/42359532384), on commit `711df48e5e3038e5c78940b333107f00f6aacd4b`: **SUCCESS**. Linux solution **6,640/6,640 tests passing**, **0 compiler warnings, 0 build errors**. `ExchangeHouse.Markdown.Tests` **175/175** passing (including 20 new deferred-ownership cases); Windows UI regression tests **SUCCESS**, Markdown local documentation links **SUCCESS**.
- The two earlier generic-text fixture expectations were updated for confirmed lazy paragraph ownership; thematic-break fixture was made unambiguous (`***`). These corrections did not require modifications to the established whole-document `MarkdownParser`.
- Subsequent changes only update English README, documentation index and status notes. Full rendered Markdown paging and `F/N/P` indexed navigation remain future milestones.

## 2026-11-07 — incremental container-aware block spans

- Added `MarkdownIncrementalContainerBlockScannerTests.cs` (**22 cases**): disjoint exact UTF-16 source span coverage and physical line boundary reconstruction; independent list/quote/blank ownership; lazy vs explicit continuation runs; matched, interrupted and EOF-terminated fences; large paragraphs/code runs without storing text; Unicode/CRLF/CR/LF; early streaming, cancellation and resource limits.
- [CI for first test snapshot](https://github.com/afrowaveltd/Toolbox/actions/runs/42360132099) passed Linux/Windows builds/tests and links: **6,662 passed, zero failed**. It exposed two **xUnit2031** style warnings on `Assert.Single(spans.Where(...))`; corrected in commit `1bc41eeca49d003ed5e28f2d0d875c6120ce39fc`. A tautological heading equality was replaced with an offset assertion. Fresh zero-warning verification pending.

### Verified CI — completed container-aware spans, 2026-11-07

- [GitHub Actions run 42360297028](https://github.com/afrowaveltd/Toolbox/actions/runs/42360297028) on commit `1bc41eeca49d003ed5e28f2d0d875c6120ce39fc`: **SUCCESS** for Linux full solution build and tests (**6,662 passed, 0 failed**), **0 Warning(s), 0 Error(s)**; Windows UI regression job **SUCCESS**, local Markdown documentation links **SUCCESS**.
- `ExchangeHouse.Markdown.Tests`: **197/197 passing**, including all **22** new completed-container-span tests. Earlier two `xUnit2031` warnings were fixed without suppression; post-fix build reports none.
- Subsequent changes are README, documentation-index and implementation-status updates only. No production behavior changes after this verified code snapshot. Known list-owned code-fence opener indentation discrepancy remains explicitly deferred from any mdview renderer integration.


## 2026-11-08 — checkpoint codec focused tests

- Added five `MarkdownScannerCheckpointTests`: nested ownership/fence JSON roundtrip, empty initial state, unsupported version and owner reference rejection, invalid fence/nesting rejection, malformed and oversized JSON.
- Production contract `9aa8bfb0f44452779f1646b3297aff601d280f90`; tests `b7b13ad5c36c7a0b7571bbf90198a1e332d0e407`. These tests do **not** demonstrate live scanner export/restore or source-binding correctness. Fresh .NET test run pending.


## 2026-11-08 — live checkpoint state-machine parity tests

- Added five `InlineData` examples in `MarkdownContainerStateMachineTests.CaptureAndRestoreAfterEveryLineMatchesUninterruptedScan` for list nesting, quote ownership, fenced code and CRLF/mixed newline continuation.
- Added three focused tests for active quoted fence state restoration, rejection of misaligned next source lines and rejection of an unresolvable outer fence-owner.
- Implementation `eed96080d2567ea5d68a3b679a5c8c83840ab766`, tests `5ad6259202aa739f8e409309eb963233f60999f4`. These tests check state-machine classification, not arbitrary on-disk seek, snapshot identity or rendered terminal row parity.
- **Fresh CI/manual test verification pending.** Previous green Windows suite was confirmed before this refactor.
