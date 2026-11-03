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

## 2026-11-02 — experimental indexed streaming scanner and diskless stress harness

- Introduced forward-only MarkdownStreamingReader.ScanAsync with 64-bit source character offsets, bounded input-line buffer and minimal open-fence/list index state. Long code fences do not accumulate in memory.
- Stress runner: Toolroom/Markdown/Stress, with deterministic virtual Markdown for 10M, 100M, 1G and 10G; optional Spectre render to null output; 50k-line open-fence regression.
- Existing full-document parser and normal Showcase viewer remain unchanged. Full CommonMark-equivalent streaming state, deferred references and paged terminal navigation are not yet supported.
- **CI and large stress execution pending**; the change is on a review branch, not asserted merged to master.

## 2026-11-02 — temporary disk-backed paging regression suite (draft PR #3)

- Added `MarkdownDiskPageIndexTests` covering random page navigation,
  raw UTF-8 source/byte offsets including emoji, BOM handling, CRLF/LF/CR,
  EOF boundaries, empty file, invalid UTF-8 and terminal controls, maximum
  physical line size, temporary index size quota/cleanup, mutable source,
  invalid options and a 25,000-line / 500-page on-disk index.
- This verifies **source pagination**, not multi-page Markdown semantic
  rendering or 1 GiB/10 GiB interactive acceptance. Test artifacts are
  isolated under unique temporary directories and deleted after use.
- Relevant .NET Linux/Windows CI: [draft PR #3](https://github.com/afrowaveltd/Toolbox/pull/3).

## 2026-11-02 — fence checkpoint regression coverage

- Added `MarkdownFenceCheckpointIndexTests` for restartable classification
  of code fences across arbitrary page boundaries; opening length and
  quote-depth constraints; UTF-8/BOM and UTF-16 source offsets; 96-byte
  per-page index, quota and disposal; empty and BOM-only documents.
- CI validates the exact commits in [draft PR #3](https://github.com/afrowaveltd/Toolbox/pull/3).

## 2026-11-02 — bounded structural checkpoint regressions

- Extended `MarkdownFenceCheckpointIndexTests` for list-depth restoration
  across arbitrary page reads, same-depth quoted prose continuity,
  quote-depth interruption, blank/fence paragraph resets and updated
  128-byte checkpoint records/quotas.
- Candidate labels intentionally do not imply CommonMark AST equivalence.
  See [PR #3](https://github.com/afrowaveltd/Toolbox/pull/3) for exact CI.

## 2026-11-02 — reverse-order parity against uninterrupted source scanning

- Added `Random_access_pages_match_uninterrupted_scan_for_bounded_structure`:
  read UTF-8 source pages in **reverse order**, then compare every line
  against the one-pass scanner's kind, UTF-16 start, raw bytes-as-text,
  quote depth, list candidate depth, fence start/language and paragraph
  continuation candidate. The fixture covers nested lists, quoted code,
  headings, blank lines and CRLF.
- This verifies parity for the defined conservative scanner **only**, not
  CommonMark rendering equivalence. Exact CI evidence follows the PR.


## 2026-11-02 — persisted owner offset regression coverage

- `MarkdownFenceCheckpointIndexTests` now checks 512-byte per-page
  state, owner marker offsets across nested/parent continuations,
  unindented lazy candidates, blank and quote interruptions and parity
  against an uninterrupted scanner in reverse page order.
- Tests validate conservative owner identities rather than claiming
  full CommonMark container parsing. CI: draft PR #3.


## 2026-11-02 — sparse checkpoint replay tests (draft PR #3)

- Verified default 16-page spacing/512-byte records and deletion on
  disposal; configurable 1/4/16 stride behavior.
- Added reverse-order read parity across sparse boundaries for list
  owner offsets, quote depth, paragraph candidates and code fence state.
  Added saved-record quota and invalid stride (0/257) tests.
- Source and checkpoint index remain bounded; regression tests are
  synthetic, not 1/10-GiB interactive proof. See PR #3 CI.


## 2026-11-02 — paragraph source-offset checkpoint regressions

- `MarkdownFenceCheckpointIndexTests` now compares `ParagraphStart`
  between uninterrupted scans and sparse page replay.
- New fixtures cover child/parent list ownership changes without a blank
  separator, continued prose in the same owner, a blank before independent
  prose, explicit quote transitions, and fence start/close resets.
- Verifies source-position identity, not full CommonMark layout equivalence.

## 2026-11-02 — blank-separated list evidence regressions (draft PR #3)

- Added tests for `MarkdownListSpacingHint` on a new paragraph of an
  existing list item, a new sibling marker after a blank, uninterrupted
  tight candidates, quote-depth isolation and unindented prose outside
  a list. Compare reverse sparse-page views with uninterrupted scanning.
- Checked that multiple blank lines restart paragraph identity and
  retain bounded list-state semantics; no full CommonMark guarantee.

## 2026-11-02 — disk-backed retroactive list-group regression suite

- Added `MarkdownListSpacingIndexTests` exercising retroactive
  list-wide loose candidates after later blank-separated siblings,
  multi-paragraph items, tight candidates, nested/quoted list grouping,
  bullet/numbered type switches, fenced-code membership, quota failure
  cleanup, disposal, no-list sources and 2,000 sequential item markers.
- Checks 16-byte marker records, 24-byte group records and exact source
  preservation. Deliberately does not claim full CommonMark list
  equivalence or acceptance on 1-vCPU/2-GiB VM.

## 2026-11-03 — profile policy regressions

- `MarkdownPagingProfilesTests` cover the four validated presets, custom record overrides, invalid bounds and enum values.
- Every preset builds both disk indexes and resumes an arbitrarily sought page inside a long code fence; original source is preserved.
- Tests are synthetic; do not mistake them for real 1-vCPU/1-GiB HDD measurements.

## 2026-11-03 — persistent remote cache tests

- Added `MarkdownRemoteDocumentCacheTests` for byte-perfect downloads, re-open from a different cache instance without internet, HTTP ETag/Last-Modified conditional requests and 304, explicitly forced fresh versions and retention of open-version paths.
- Tests also cover broken connections and 5xx stale copies, 4xx error propagation, no-network cache misses, download byte quota with old-copy preservation, permanent export, explicit per-URL deletion and rejection of insecure URLs/corrupt path metadata.
- Tests use local stubbed HttpClient handlers and isolated temporary directories; no live GitHub networking or 1-GiB HDD acceptance is claimed.

## 2026-11-03 — persistent cache quota and manual pruning tests

- Extended `MarkdownRemoteDocumentCacheTests` with combined cache
  disk-budget rejection while preserving the previous offline file,
  byte/file-count usage accounting, explicit pruning of old versions
  across multiple URLs, default 24h grace retention, corrupt manifest
  non-deletion, and input validation.
- The tests are local and use stubbed HTTP response handlers. They
  verify single-process behavior only, not cross-process locking,
  physical HDD latency or live network connectivity.

## 2026-11-03 — unknown-length HTTP quota regression

- Extended the cache quota suite with a deliberately **unknown
  Content-Length** stream. Confirms the **total cache disk budget**
  is enforced while receiving chunks, partial files are removed,
  and the previously saved offline document remains byte-identical.
- This is a stubbed HTTP response, not a live network benchmark.

## 2026-11-03 — malicious/oversized cache metadata regressions

- Added tests ensuring oversized saved manifests are not loaded into
  memory or treated as a valid current version, and remote ETag values
  above 16 KiB cannot publish invalid metadata, consume orphan space
  or overwrite the last saved offline copy.

## 2026-11-03 — paged catalog and compatibility tests

- Added bounded `ListPageAsync` tests for multi-document pagination
  and `HasMore`, explicit 1..64 limits, exclusion of unrelated
  files, old manifest without `SourceUrl`, and forged mismatching
  URL metadata. All use local disk and stub HTTP.
