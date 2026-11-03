# Afrowave.Toolbox — repository implementation status

**Updated 2026-11-02. Source of truth:** [GitHub master](https://github.com/afrowaveltd/Toolbox/tree/master). This file is a **cross-project continuation overview**; authoritative technical details remain in each component's `IMPLEMENTATION_STATUS.md`.

## Completed implementations and owned status

| Area | Current implementation | Continue here |
|---|---|---|
| Essentials | Results, responses, metadata, diagnostics, guards, interfaces; NuGet 0.2.0 published | [Status](Essentials/IMPLEMENTATION_STATUS.md) |
| WhenItFails | Error catalogs/runtime, resolution, validation and recovery; NuGet 1.0.0 published | [Status](WhenItFails/IMPLEMENTATION_STATUS.md) |
| UnderstandMe | Offline localization, dictionaryless JSON storage, placeholders and Configurator; large orchestration roadmap remains future | [Status](UnderstandMe/IMPLEMENTATION_STATUS.md) |
| Blackboard FileBrowser | Interactive picker, masks/regex/tree/search; NuGet 0.1.0-preview.1 published | [Status](Blackboard/SpectreConsole/FileBrowser/IMPLEMENTATION_STATUS.md) |
| ExchangeHouse.Markdown | Source-preserving parsing/writing of supported Markdown, inline links/images and contiguous nested/numbered-list metadata | [Status](ExchangeHouse/Markdown/IMPLEMENTATION_STATUS.md) |
| Blackboard Markdown | Themed Spectre renderer, safe image text alternatives and nested list display | [Status](Blackboard/SpectreConsole/Markdown/IMPLEMENTATION_STATUS.md) |
| Showcase | FileBrowser demos and four-theme Markdown gallery | [Status](Blackboard/Samples/Blackboard.Showcase/IMPLEMENTATION_STATUS.md) |
| UnderstandMe Configurator | Settings editing/validation and offline help via Blackboard Markdown | [Status](UnderstandMe/Tools/Configurator/IMPLEMENTATION_STATUS.md) |
| SeeMe console | WhenItFails presentation adapter | [Status](SeeMe/WhenItFails/Console/IMPLEMENTATION_STATUS.md) |
| Toolroom / Setter | Catalog authoring/maintenance and release comparison/validation tools | [Status](Toolroom/IMPLEMENTATION_STATUS.md) |

Publication of **Essentials 0.2.0, WhenItFails 1.0.0 and FileBrowser 0.1.0-preview.1** was confirmed by the maintainer; the two Markdown library projects are **not recorded as published**. Version metadata alone is not evidence of publication. [Release status](Docs/Release-Status/en.md).

## Latest verified functional checkpoint

- [GitHub Actions #41244480220](https://github.com/afrowaveltd/Toolbox/actions/runs/41244480220): full Linux solution and Windows FileBrowser/Configurator/Showcase/localization/Markdown regressions passed for nested/numbered Markdown implementation.
- [GitHub Actions #41244630027](https://github.com/afrowaveltd/Toolbox/actions/runs/41244630027): subsequent documentation checkpoint passed Linux and Windows.
- User confirmed Markdown Showcase and Configurator Help work in Windows manual testing; Debian testing of some latest visuals is still pending.
- This documentation refresh changes navigation and records previously released packages; it does **not** modify executable code or imply new package publication. CI for the refreshed documentation commit must be checked separately.

## 2026-10-31 — repository-wide documentation reconciliation

- Replaced the obsolete Essentials-only root README/docs directory with a full Toolbox index, overview, architecture, repository getting-started, development/verification and release inventory.
- Reconciled stale FileBrowser publication status, implemented Blackboard/ExchangeHouse Markdown and first consumer integrations in family READMEs.
- Added missing family entries for SeeMe, Toolroom, AJIS design notes, tests and their status/topic documents.
- The root `Docs/Getting-Started/en.md` and `Docs/API-Reference/en.md` remain **Essentials-specific**, explicitly linked from the new Toolbox-wide docs index.
- Preserved community and historical Id documents as design/background. AJIS/A2 notes are not labeled a compiled runtime.
- Cross-repo markdown link audit and final CI status must be recorded after verification.

## Work discipline

1. Inspect latest GitHub master code and the owning project's implementation status.
2. Make a small change together with tests, English README/topic docs and status update.
3. Commit immediately and validate on CI; identify exact commit/run.
4. Never mark work green from a pending CI run or infer NuGet publication from a `.csproj` version.

## Next engineering focus

Continue [ExchangeHouse Markdown](ExchangeHouse/Markdown/IMPLEMENTATION_STATUS.md) with multiline/loose lists in bounded increments, or improve Markdown documentation/localization link protection. No full CommonMark support has been claimed.

### Final documentation reconciliation verified — 2026-10-31

- [GitHub Actions #41246089346](https://github.com/afrowaveltd/Toolbox/actions/runs/41246089346) **SUCCESS** for commit `1b63512305c05fad77b1d0f1d5e0878de0ef8fbd`: full Linux solution build and tests, Windows FileBrowser/Configurator/Showcase/ExchangeHouse Markdown/Blackboard Markdown tests, and Markdown documentation-link job.
- Documentation audit scanned **267 Markdown files**, checked **1,341 repository-local links**, and found **zero missing local targets**. Earlier audit caught 81 legacy bad links, repaired in source rather than skipped; Setter README canonical GitHub links are external and not counted in this local-file scan.
- Preserved both existing Setter docs regressions: byte-identical README mirror and explicit Adding Errors guide link; its expected URL now matches canonical publishing paths.
- Three maintainer-confirmed published packages are documented: Essentials `0.2.0`, WhenItFails `1.0.0`, FileBrowser `0.1.0-preview.1`. The two implemented Markdown libraries remain labeled unpublished. The documentation-only reconciliation introduced no runtime behavior changes.
- The checker does not validate external URLs, remote NuGet registry state, heading anchors, reference-link definitions or full CommonMark syntax; do not claim those validations.

## 2026-10-31 — English root README and UnderstandMe status reconciled

- Synced `README/en.md` with the current complete Toolbox root README (relative links adjusted for the localized folder).
- Updated the UnderstandMe opening status to distinguish the working offline core/Configurator from pending translation orchestration, without erasing historical implementation details.
- Documentation-only changes; Linux/Windows full build/test and repository-wide link CI confirmation pending for this final commit.

### Final repository-wide docs checkpoint — verified 2026-10-31

- [GitHub Actions #41246375880](https://github.com/afrowaveltd/Toolbox/actions/runs/41246375880) **SUCCESS** on commit `7e38be728aa6930e86fdc82f253612dfe07790f6` (English root README synchronization and UnderstandMe status).
- Full Linux solution **build PASS, tests PASS**; Windows FileBrowser, Configurator, Showcase, ExchangeHouse Markdown and Blackboard Markdown suites **PASS**.
- Documentation checker scanned **267 Markdown files**, verified **1,364 local link targets** and found **zero broken local targets**. Two Setter README mirrors retain identical working canonical GitHub URLs; those links are external and excluded from the local count.
- The checkpoint validates source/docs and links at that revision; external NuGet listing access and remote URL/heading-anchor validation remain outside this checker. The three publication states follow the maintainer's explicit confirmation.

### Verified CI — 2026-10-31 list continuation increment

- [GitHub Actions #41248289922](https://github.com/afrowaveltd/Toolbox/actions/runs/41248289922) **SUCCESS**, source through commit `35efe934031a99a5407550f6624d96656b9c03a3`: Linux full solution build/tests passed; Windows ExchangeHouse Markdown **46/46**, Blackboard Markdown **16/16**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**.
- Markdown local link job **PASS**: 269 Markdown files, 1,374 local file targets checked with none missing. Source-aware continuation behavior also tested on Windows; manual Debian and real-terminal visual checks remain separate.
- Initial renderer integration CI failed **one incorrect test expectation** (custom indent `....` erroneously checked as six literal spaces). Fixed the assertion in `35efe934`, retaining custom-indent coverage. The failed earlier CI runs are not counted as successes.
- This milestone handles only **explicitly indented** continuation lines. Loose lists, lazy continuation and nested CommonMark block containers remain unsupported. No public NuGet release was made.

### Verified CI — 2026-10-31 loose-list paragraph milestone

- [GitHub Actions #41249127101](https://github.com/afrowaveltd/Toolbox/actions/runs/41249127101) **SUCCESS** on source/docs commit `df4da1b285d280cbe1d17dee0fded388f178d0b8`: full Linux solution build/tests and Windows UI/Markdown regressions passed.
- Windows focused test results: ExchangeHouse Markdown **55/55**, Blackboard Markdown **19/19**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**. Earlier linked documentation job verified **271 Markdown files and 1,388 local links** with no missing local targets; the final run's link job also passed.
- Added source-preserving `IsLoose`, blank separator/content collections and source owner metadata for explicitly indented blank-separated list paragraphs. Safe terminal rendering aligns subsequent paragraphs beneath the original marker; Showcase updated. No source rewrite, full CommonMark grammar or NuGet publication.
- Interim #41249032554 failed one incorrect custom-indent **test expectation**, corrected in `2b34cb46`, without production renderer change. Later CI is green. Manual acceptance of new visuals in real Debian/Windows terminals remains pending.

## 2026-10-31 — bounded lazy list paragraph source support

- ExchangeHouse Markdown recognizes unindented paragraph-only continuation immediately after an active list item/continuation, without traversing blank boundaries or treating headings, fences, quotes or thematic rules as lazy text.
- Blackboard Markdown already aligns owner-aware paragraphs; parser/renderer regressions, localized topic docs and Showcase theme sample added.
- **CI pending** for this commit. No full CommonMark conformance or new NuGet package publication claimed; implementation specifics in [Markdown status](ExchangeHouse/Markdown/IMPLEMENTATION_STATUS.md).

### Final green lazy-paragraph checkpoint — 2026-10-31

- [GitHub Actions #41250061417](https://github.com/afrowaveltd/Toolbox/actions/runs/41250061417) **SUCCESS** for commit `f6b7fc555c76d964ec12a27f86accc1beab3e9f8`: Linux full solution build and tests passed, Windows targeted UI/Markdown suites passed and local Markdown documentation-link check passed.
- Windows counts: **ExchangeHouse Markdown 60/60**, **Blackboard Markdown 21/21**, **Showcase 26/26**, **FileBrowser 160/160**, **Configurator 44/44**, **UnderstandMe localization 34/34**.
- Documentation audit: **273 Markdown files, 1,399 local targets checked**, zero missing links. Cross-family link typo from the first commit was fixed in `2c8c3fa`; the final gallery sample now uses an actually unindented lazy line.
- No public Markdown NuGet publication, no full CommonMark or real-terminal Debian acceptance is implied by CI.

## 2026-10-31 — fenced code, quotes and optional grammar highlighting

- ExchangeHouse Markdown recognizes conservative quote lines and fenced code nested beneath indented list items. `MarkdownBlock.CodeFenceInfo`, `CodeLanguage`, `CodeFenceStart`, `IsClosingCodeFence`, `QuoteDepth` and per-item `EmbeddedBlocks` preserve source/owner metadata. Unchanged Markdown still round-trips character-for-character, including original line endings.
- Blackboard Markdown renders quote nesting and code beneath the list content, with optional language labels and an `IMarkdownSyntaxHighlighter` session contract. Code is not evaluated and terminal escape/markup controls remain sanitized.
- A **separate optional** `Blackboard/SpectreConsole/Markdown.TextMate` project implements the contract using TextMateSharp.Grammars 2.0.4, without adding native grammar dependencies to ExchangeHouse or the base Blackboard Markdown renderer. Standalone .NET 10 xUnit tests and Windows CI step were added; optional package **not published**.
- Full CommonMark blockquote/list nesting and executable code remain out of scope. [Grammar integration](Blackboard/SpectreConsole/Markdown.TextMate/Docs/Grammar-Adapter/en.md).
- **CI pending** for the latest adapter/registration commits; older code/test commits contain a corrected test expectation and a fixed `Style` validation compile error. Do not claim this milestone green until its final Linux + Windows workflow succeeds.

### Cross-platform green verification — 2026-10-31

- [GitHub Actions #41251065292](https://github.com/afrowaveltd/Toolbox/actions/runs/41251065292) **SUCCESS** for source commit `5b80ad1e3f9cde50ae793da53b3b81c30d727d08`. Linux complete solution restore/build/tests passed, Windows targeted UI and grammar tests passed, and Markdown link validation passed.
- Windows: **ExchangeHouse Markdown 66/66**, **Blackboard Markdown 24/24**, **TextMate adapter 6/6**, **Showcase 26/26**, **FileBrowser 160/160**, **Configurator 44/44**, **UnderstandMe localization 34/34**.
- The documentation link audit examined **280 Markdown files and 1,423 local targets** with no missing targets. A later docs-only commit clarified a nested-fence illustration; final docs CI is tracked separately.
- Real TextMate C# tokenization was exercised under Linux and Windows CI, including platform-specific Oniguruma dependencies. This does **not** establish manual SSH/tmux or Debian terminal acceptance, complete CommonMark support, NuGet publication, or a completed third-party transitive-license review.

## 2026-11-01 — independent lists inside blockquotes

- ExchangeHouse Markdown recognizes mixed/nested list items on consecutive explicit quote-prefix lines, retaining original marker numbers, quote depth and inline source offsets. Quoted items are exposed in additive `MarkdownDocument.QuoteListRoots` without changing ordinary `ListRoots` or the lossless writer.
- Blackboard Markdown renders quoted list markers/continuations with the quote prefix outside list indentation; Showcase gallery has an example.
- New parser/renderer regressions and English topic guides. CI validation pending for this increment; no NuGet release or full CommonMark nested-container claim.

### Verified cross-platform quoted-list milestone — 2026-11-01

- [GitHub Actions #41251673070](https://github.com/afrowaveltd/Toolbox/actions/runs/41251673070) **SUCCESS** for feature commit `aa3ec1246fc3125f17d1a506e415cc3ad7db418b`: Linux full solution build/test, Windows targeted UI tests and documentation-link job passed.
- Windows results: ExchangeHouse Markdown **71/71**, Blackboard Markdown **26/26**, optional TextMate **6/6**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**.
- The subsequent documentation commit introduced one incorrect cross-family relative link, fixed in `1e89cf7532c03737a0599d51c26b54cd57bb345a`. Link verification on the fix run passed; final complete CI for the documentation state is tracked independently.
- No new NuGet publication, full CommonMark conformance, or real-terminal acceptance is claimed. This feature intentionally does not tokenize fenced code inside explicitly quote-prefixed containers.

## 2026-11-01 — quoted fenced code / safe terminal display

- ExchangeHouse Markdown now recognizes backtick/tilde fences on same-depth explicit quote-prefixed lines, optionally owned by an indented quoted list item. Original text/line endings/offsets, code language and source writer remain intact. Missing/different quote prefix interrupts the fence rather than swallowing subsequent Markdown.
- Blackboard renders themed quote prefixes outside code/list indentation; TextMate is optional and sessions are scoped to each fence, including interruption. The Showcase includes a `csharp` method under a numbered quoted list alongside the classic .NET Core 3.1 `Main` example.
- Parser, virtual-terminal and Showcase regressions were added, alongside English [parser guide](ExchangeHouse/Markdown/Docs/Quoted-Code-Fences/en.md) and [renderer guide](Blackboard/SpectreConsole/Markdown/Docs/Quoted-Code-Presentation/en.md).
- Implementation CI [#41254427784](https://github.com/afrowaveltd/Toolbox/actions/runs/41254427784) **SUCCESS** on corrected source/test commit `836914ba5fbb0358029b5b296af0c4eed4abca32`; the earlier `fdc1b17a` CI failed one incorrect expected count of embedded blocks, corrected without changing production code.
- Full CI for the latest **documentation/status** commit is tracked separately. This is not complete CommonMark, a new NuGet publication or a manual terminal acceptance result.

### Full cross-platform CI checkpoint — 2026-11-01

- [GitHub Actions #41254717060](https://github.com/afrowaveltd/Toolbox/actions/runs/41254717060) **SUCCESS** for final code/English-docs fix commit `8f25d8e2a72df6ffe915362ef11d64dbd6c89d6a`: Linux complete solution build/tests, Windows targeted UI/Markdown tests and repository local documentation-link validation passed.
- Windows focused tests: ExchangeHouse Markdown **75/75**, Blackboard Markdown **29/29**, Markdown.TextMate **6/6**, Blackboard Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, UnderstandMe localization **34/34**.
- Markdown local-link checker verified **284 files / 1,452 local targets**, zero broken links. An earlier documentation-only CI failed one wrong Showcase-to-renderer relative path, fixed in `8f25d8e`; the final checkpoint above is green.
- No new NuGet release or manual real-terminal Windows/Debian acceptance claimed.

## 2026-11-01 — deferred blank separators in quoted lists

- ExchangeHouse Markdown recognizes **loose quoted list paragraphs**: one or more explicitly quote-prefixed blank lines are owned only when followed by a sufficiently indented continuation at the **same quote depth**. Quoted nested numbered items can return ownership to a parent. A new marker, unindented paragraph, different quote depth or unquoted boundary never steals pending separators.
- This extends existing `MarkdownListItemNode.SeparatorBlocks` / `ContentBlocks` and `MarkdownBlock.ListBlankOwnerStart` without breaking `ListRoots`, `QuoteListRoots`, original character offsets or unchanged-source writing. Quoted blank lines retain `BlockQuote` kind and their visible quote prefix.
- Blackboard's existing owner-aware renderer displays the blank quote markers and aligned paragraphs; Showcase has a concrete quoted numbered-list example. No new renderer production dependency or NuGet release.
- Parser, terminal and Showcase regressions plus English topic docs: [parser](ExchangeHouse/Markdown/Docs/Quoted-Loose-Paragraphs/en.md), [renderer](Blackboard/SpectreConsole/Markdown/Docs/Quoted-Loose-Presentation/en.md).
- Full cross-platform GitHub Actions validation **pending** for this increment. Full CommonMark or lazy continuation *across* blank boundaries is not claimed.

### Verified cross-platform milestone — 2026-11-01

- [GitHub Actions #41256065289](https://github.com/afrowaveltd/Toolbox/actions/runs/41256065289) **SUCCESS** on source/documentation commit `882f15862d89214e8cc1204d8b5b7baaedd6d18a`: Linux complete solution build and tests, Windows UI/Markdown targeted tests and local Markdown documentation links all passed.
- Windows tests: ExchangeHouse Markdown **82/82**, Blackboard Markdown **31/31**, TextMate adapter **6/6**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**.
- Markdown checker processed **286 files / 1,470 local targets**, with zero missing local links.
- The test-preserving increment has not been published to NuGet and has not undergone manual Debian/Windows/SSH visual acceptance; full CommonMark nesting remains outside this milestone.

## 2026-11-01 — nested quote/list container milestone

- The source-preserving Markdown parser recognizes **ordinary list → explicitly indented quote → inner ordered/unordered list**, with optional fenced code and quoted loose paragraphs. Additive `MarkdownBlock.QuoteContainerListOwnerStart` and `QuoteContainerListMarker` identify the outer ordinary owner while existing `QuoteListRoots`, `ListContinuationOwnerStart`, `EmbeddedBlocks` and inline/source positions retain their intended semantics.
- Equally deep quoted runs under different ordinary list items remain independent. The quote normalizer first probes the outer list owner before considering an indented physical quote standalone; quote/code fence interruption and source safety remain intact.
- Blackboard combines outer list indent + quote theme prefix + inner quote-list indent; code language labels and bodies use the same layout. Showcase includes a deployment step with quoted numbered instructions and literal `dotnet --list-runtimes`.
- Six added parser tests, three renderer tests and a strengthened existing Showcase assertion. [Implementation CI #41257477307](https://github.com/afrowaveltd/Toolbox/actions/runs/41257477307) **SUCCESS** for code/test commit `9f0911921db5f0d0ba755959d1b5747ab376b4fd`: Linux complete solution restore/build/test, Windows UI tests and Markdown link validation passed.
- Windows counts: parser **88/88**, renderer **34/34**, TextMate **6/6**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**.
- Documented in [parser guide](ExchangeHouse/Markdown/Docs/Quoted-Lists-Inside-Lists/en.md) and [renderer guide](Blackboard/SpectreConsole/Markdown/Docs/Quoted-List-Containers/en.md). Latest full documentation/status CI tracked separately.
- Not full CommonMark, not a NuGet release and no manual physical terminal acceptance claimed.

## 2026-11-02 — local full-document Markdown reader in Blackboard.Showcase

- Added a fifth independent Showcase menu item `Markdown — open local README or .md file`. Reuses the local Spectre FileBrowser with `*.md;*.markdown` filter, independently checks the selected extension, then reads and renders Markdown with existing ExchangeHouse parser and the same Classic/optional TextMate renderer used by the theme gallery. The original four modules retain their order.
- User-driven design correction: **no 64 KiB read/display cap** for Markdown help. New `TextFilePreviewReader.ReadCompleteAsync` performs chunked, strict decoding of the *entire* file (UTF-8, BOM UTF-16/UTF-32) and rejects invalid encodings and terminal controls anywhere, including beyond former preview limits; only a size notice is displayed for files above 64 KiB. The existing generic `ReadAsync` retains its independently useful 16 KiB bounded-prefix semantics.
- Complete Markdown source is materialized as a string for the current non-streaming parser and renderer; practical max size depends on available memory and terminal output, not an arbitrary file-size check. This is a **Showcase** convenience viewer, not a streaming Markdown document engine or a new NuGet dependency.
- Added tests for real README, Unicode BOM, cancellation/empty and malicious content, 64 KiB+ full rendering, 512 KiB complete reader, multibyte Unicode boundaries and invalid/control bytes appearing far into large files.
- [Usage, validation and limitations](Blackboard/Samples/Blackboard.Showcase/Docs/Markdown-File-Viewer/en.md). CI **pending** for this change; manual test of the new file-backed viewer in Windows/Debian still open. User-provided Windows Terminal screenshots confirm the **previous embedded theme gallery** visually works, but are not file-viewer acceptance evidence.

### Verified full-document Markdown viewer checkpoint — 2026-11-02

- [GitHub Actions #41401243770](https://github.com/afrowaveltd/Toolbox/actions/runs/41401243770) **SUCCESS** on source + English documentation commit `4f0cf18f4367bd436416610af2419fe0d23d5605`. Linux full-solution build/test, Windows UI regression suites and Markdown local link validation passed.
- Windows: Blackboard Showcase **39/39** (includes complete file viewer), ExchangeHouse Markdown **88/88**, Blackboard Markdown **34/34**, TextMate **6/6**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**.
- Documentation check: **289 Markdown files, 1,502 local links**, none missing.
- During implementation initial CI failed on a test referencing a nonpublic file-extension helper; source was corrected to expose the simple reusable validation helper before final green CI. This does not affect the reusable FileBrowser package.
- The reader has **no arbitrary 64 KiB Markdown cut-off**; the size warning is informational. Whole-file Unicode/control validation precedes rendering. Current renderer still materializes full text and model in RAM; true out-of-core/paged help is future work.
- The previous embedded theme gallery was visually confirmed by user screenshots in Windows Terminal. Actual Windows/Debian manual UI verification of the **new file-backed viewer** remains pending; no Markdown NuGet release.

## 2026-11-02 — proposed standalone Markdown terminal viewer (design only)

- Captured a future **standalone** TUI application concept for after the Markdown parser/renderer reaches an agreed completion milestone. It will accept no arguments (interactive Local / HTTP(S) / GitHub source picker) or a direct Markdown path, URL, or `github:owner/repository` shortcut.
- The proposed GitHub adapter uses the official preferred README endpoint, `GET /repos/{owner}/{repo}/readme`, on the repository's default ref rather than guessing `main`, `master`, or filename locations. Optional branch/ref overrides and private-repo credentials are deferred.
- Reuse existing ExchangeHouse Markdown, Blackboard Spectre renderer, FileBrowser and strict full-file source validation. Keep source providers modular, resolve relative Markdown links against the correct repository file/ref, and design for full large-document access without silent byte truncation (disk cache/index or paging when needed).
- **Design only: no executable, new project, implementation, tests, or NuGet package created.** See [viewer proposal](Blackboard/SpectreConsole/Markdown/Docs/Standalone-Markdown-Viewer-Proposal/en.md). Existing full local Markdown Showcase remains the prototype, not the proposed standalone app.

## 2026-11-02 — CLI case-insensitive GitHub identifiers + streaming/pager design refinement

- Clarified intuitive positional command forms `mdview git OWNER REPO`, `mdview local PATH`, `mdview url URL`, and direct/alias forms. No-argument Local / URL / GitHub menu remains the proposed entry point.
- Verified through official GitHub REST docs that **owner/repo are already case-insensitive**. No capitalization-guessing request loops; use one README API request and canonical repo metadata if necessary. Local Linux paths, branch/ref names and repository file paths are not assumed case-insensitive.
- Recorded a separate prospective `IAsyncEnumerable` incremental block parser and Unix-`less`-style pager as a **research track**: quote/list/fence state and bounded lookahead, late reference definitions, optional two-pass/disk-backed spooling, terminal-width aware visual lines, keybindings, cache/index for reverse navigation and 1 GB benchmark targets. Distinguish chunked file decoding from full in-memory parsing.
- Retain strict source validation and no silent help truncation. **Design/documentation only**; no parser API, CLI app, pager, tests, NuGet publication or 1 GB streaming claim has been implemented. [Detailed proposal](Blackboard/SpectreConsole/Markdown/Docs/Standalone-Markdown-Viewer-Proposal/en.md).

## 2026-11-02 — experimental indexed streaming, virtual stress and remote temp caching (draft PR #3)

- ExchangeHouse: additive `MarkdownStreamingReader.ScanAsync(TextReader)` preserves original lines and 64-bit UTF-16 offsets, indexes open code-fence state and bounded list indentation instead of collecting entire logical blocks. Not a full incremental CommonMark AST.
- Blackboard: separate `RenderStreamingLine` entry point; existing whole-document renderer remains intact.
- Toolroom: deterministic virtual Markdown generator with opt-in 10M, 100M, 1G, 10G stress runner, reporting wall time, managed-heap samples and RSS. Small 10M workflow passed; larger sizes not yet tested.
- Showcase: experimental trusted HTTPS download-to-OS-Temp utility, 64-KiB network/file buffer, configurable byte limit, independent seekable reads, error cleanup and delete-on-dispose. Not yet wired into the interactive menu.
- Future UnderstandMe integration, concurrent growing-file translation, full semantic Markdown block segmentation and paged UI documented as design only; do not claim deployed translation.
- GitHub PR: [#3](https://github.com/afrowaveltd/Toolbox/pull/3), draft, not yet merged. Each relevant project README/Docs/en.md/status is updated.

## 2026-11-02 — disk-backed Markdown source paging and Showcase navigation (draft PR #3)

- `ExchangeHouse.MarkdownDiskPageIndex`: strict UTF-8 full-file validation,
  fixed-size 64-bit **byte-offset** disk index with 256 MiB default index
  quota, bounded page reading and arbitrary page access without retaining
  previous page contents in RAM. Temporary index removed on failure/disposal.
- `Blackboard.Showcase` sixth demo: FileBrowser local Markdown selection,
  40-line raw-source pages, Next/Previous/Close selection and safe literal
  console display. The pre-existing fifth **fully rendered** viewer remains
  unchanged. HTTP and GitHub source choices are not yet wired in.
- Regression coverage for UTF-8/emoji/BOM, bad encoding/controls, offset
  correctness, CRLF/LF/CR, index size/line limits, random page navigation,
  file changes and cleanup. [Source index guide](ExchangeHouse/Markdown/Docs/Disk-Backed-Paging/en.md)
  · [Showcase pager guide](Blackboard/Samples/Blackboard.Showcase/Docs/Markdown-Paged-Source-Viewer/en.md).
- **Limitations:** indexing currently scans the whole UTF-8 file once,
  page contents are unformatted Markdown **source**. Contextual semantics,
  page-spanning fence/list state, incremental downloader integration,
  translations and 1 GiB/10 GiB end-to-end viewer validation remain future.
  No NuGet release by these commits.
- Work remains in [draft PR #3](https://github.com/afrowaveltd/Toolbox/pull/3)
  on `feature/markdown-streaming-stress-20261102`; not merged into master.

## 2026-11-02 — fenced-code checkpoints for large Markdown pages (draft PR #3)

- `ExchangeHouse.MarkdownStreamingReader` accepts an optional explicit
  `MarkdownStreamCheckpoint`, restoring global UTF-16 position and
  code-fence state for independent pages.
- `MarkdownFenceCheckpointIndex` records one disposable **96-byte**
  checkpoint per page, separate from raw-source byte-offset index.
  An arbitrary page can now be classified correctly when a code fence
  began elsewhere. Disk quota and cleanup are enforced.
- Sixth Blackboard Showcase demo has a reversible toggle between literal
  source and **conservative** Markdown rendering with resumed fence state.
- Tests and English docs record what works and the remaining
  list/paragraph/reference/quote limitations. BOM-only input is now
  consistently empty. No NuGet package published or stable release declared.
- [Checkpoint docs](ExchangeHouse/Markdown/Docs/Fence-Checkpoints/en.md),
  [draft PR #3](https://github.com/afrowaveltd/Toolbox/pull/3).

## 2026-11-02 — bounded list/quote/paragraph checkpoints (draft PR #3)

- Expanded the experimental page checkpoint from 96 to **128 bytes**
  without storing source lines: maximum 32 list indentation candidates,
  prior quote depth and prior paragraph-continuation candidate now join
  active fenced-code state and UTF-16 offset. Scanner and index share
  `AdvanceCheckpoint` to minimize grammar divergence.
- Added tests of page-order independence, quote-depth transitions,
  nested-list candidate depth and paragraph interruption boundaries.
- **Not** a CommonMark-complete resumable AST, nor a NuGet release.
  Future work: true list ownership, lazy paragraphs, reference resolution
  and contextually accurate Spectre paging.
- [Docs](ExchangeHouse/Markdown/Docs/Fence-Checkpoints/en.md) /
  [draft PR #3](https://github.com/afrowaveltd/Toolbox/pull/3).


## 2026-11-02 — owner-aware list continuation checkpoints (draft PR #3)

- Extended disk-backed Markdown checkpoint from 128 to **512 bytes/page**,
  retaining up to 32 active list-item marker offsets (64-bit UTF-16),
  marker and content indentation, fences, quote/paragraph hints.
- Added `ListOwnerStart` and `IsListContinuationCandidate` to
  streaming line events and tested nested, parent, lazy and quoted
  continuation across arbitrary page reads; content remains bounded.
- The old 128-byte snapshot is superseded inside this unpublished draft.
  Source offset index remains 8 bytes/page. No new NuGet release.
- Conservative ownership is not equivalent to full CommonMark
  rendering. Next: contextual rendering of list continuations and more
  complete paragraph/list container parsing.
- [Architecture](ExchangeHouse/Markdown/Docs/Fence-Checkpoints/en.md)
  and [PR #3](https://github.com/afrowaveltd/Toolbox/pull/3).


## 2026-11-02 — owner-aware Markdown Spectre rendering (draft PR #3)

- The existing paged preview now uses safe owner-aware list indentation
  through `MarkdownSpectreRenderer.RenderStreamingLine`; nested items
  and conservatively owned prose no longer lose all list context on
  a page jump. Custom theme and inline styles remain supported.
- Separate Blackboard renderer and regression suite updated; no
  source-level dependence on Showcase. Full-formatted CommonMark
  semantics still future, and no preview NuGet has been published.


## 2026-11-02 — sparse Markdown page checkpoints (draft PR #3)

- Instead of writing a 512-byte structural record per source page,
  `MarkdownFenceCheckpointIndex` now defaults to one every **16 pages**
  (configurable stride 1..256). `SavedCheckpointCount` reports the
  physical record count. The raw source byte-offset index stays 8 bytes
  **per page**.
- Arbitrary page reads replay at most stride-1 previous pages to
  reconstruct conservative fences, list owners and quote/paragraph
  candidates with bounded RAM. Disk footprint shrinks at the cost of
  additional replay I/O and CPU. Tested sparse crossing boundaries,
  reverse-order parity, index quotas and invalid configuration.
- The sixth Showcase reader uses the new default transparently.
  No new NuGet or production-complete CommonMark viewer claimed.
- [Checkpoint guide](ExchangeHouse/Markdown/Docs/Fence-Checkpoints/en.md).

## 2026-11-02 — deferred Afrowave Docs Reader idea (design only)

- Captured a possible virtual-book reader for the common
  `Docs/<Topic>/en.md` layout: recursively collect topics, insert
  generated topic headings, preserve original file provenance and
  treat the result as one navigable Markdown document without a
  physical concatenation or full-book RAM allocation.
- Future sources may include a local folder and a GitHub repository
  at a pinned revision; implementation should wait for robust
  single-document contextual paging. Source path security, symlinks,
  localization fallback, link rewriting and GitHub tree limits must
  be addressed in a separate design/implementation effort.
- **No source implementation or NuGet release.** Proposal:
  [Afrowave Docs Reader](Docs/Unified-Docs-Reader-Proposal/en.md).


## 2026-11-02 — paragraph source identity in streaming Markdown (draft PR #3)

- Added `ParagraphStart` to conservative line events; sparse page
  checkpoints also persist `OpenParagraphStart` and the associated
  `ParagraphListOwnerStart` without increasing their 512-byte size.
- Continuation now checks consistent list owner *and* quote depth.
  Tests cover list child-to-parent outdents, explicit blank/fence
  interruptions, unchanged owner continuations and sparse parity.
- This advances contextual Markdown pagination, but remains incomplete
  for full CommonMark lazy containers, paragraph fusion and references.
  No NuGet package release.
- [Checkpoint guide](ExchangeHouse/Markdown/Docs/Fence-Checkpoints/en.md).

## 2026-11-02 — blank-separated Markdown list evidence (draft PR #3)

- Streaming Markdown emits `ListSpacingHint` for new paragraphs of
  existing list items and blank-separated new sibling items, preserving
  quote and ownership boundaries after sparse page seeks.
- This is a bounded per-line observation, **not** definitive CommonMark
  tight/loose list classification; full rendering remains a future task.
  The temporary 512-byte checkpoint and raw 8-byte/page index are
  unchanged. Tests and documentation updated; no new NuGet release.
- [Details](ExchangeHouse/Markdown/Docs/Fence-Checkpoints/en.md).

## 2026-11-02 — optional disk-backed list-wide spacing index (draft PR #3)

- `ExchangeHouse/Markdown/MarkdownListSpacingIndex.cs` creates an
  independent, disposable **list-wide candidate** index by a third
  bounded-memory sequential scan. It stores group summaries (24 bytes)
  and item offsets (16 bytes) on temporary disk, rather than holding
  prior paragraphs or list items in RAM.
- Supports retroactive group candidate lookup by source marker offset;
  tests cover nested lists, blank separators, quote/type transitions,
  fenced code, source integrity, disk quota and disposal. The scanner's
  32-owner bound remains; the optional index is not yet wired to TUI.
- Future: compare full parser's tight/loose decisions, integrate
  contextual styling and run a **real** 1-vCPU/2-GiB terminal-only
  Debian acceptance session. Do not call current CI a VM measurement.
- [Index design](ExchangeHouse/Markdown/Docs/List-Spacing-Index/en.md)
  and [low-RAM VM protocol](Docs/Low-Memory-Linux-Acceptance/en.md).
  PR #3 remains draft; no NuGet package released.

## 2026-11-02 — 1-GiB rotational-HDD Debian acceptance baseline

- User-selected field target: **1 virtual CPU, 1 GiB RAM, Debian minimal
  terminal-only and a real rotational HDD-backed virtual disk**.
  Escalate to 2 GiB RAM only if measured limits require it, keeping
  disk, controller, CPU and source workload constant.
- Updated [low-memory test protocol](Docs/Low-Memory-Linux-Acceptance/en.md)
  and Markdown Stress README to separate Release build from runtime
  RSS, measure swap and HDD latency, and compare sparse checkpoint
  strides 1/4/16 in later real-source pager tests.
- Important distinction: a guest `ROTA=1` device hint cannot prove
  the VM disk image resides on a physical rotating HDD. No 1-GiB VM
  runtime or real-HDD acceptance has yet been performed or claimed.
  Changes are documentation-only, PR #3 stays draft.

## 2026-11-03 — selectable Markdown HDD/SSD/LowRAM paging profiles (draft PR #3)

- Added editable validated preset configurations: Balanced 40 source lines/16-page checkpoint stride, HDD 64/1, SSD 64/32, LowRAM 8/16. Independent disk index quotas and maximum line limit are retained.
- Sixth Showcase viewer prompts for profile before indexing and reports chosen settings. Added C# API and TestConsole regressions covering real pager seeks, validation and cancel. No automatic disk inference, no 50–100 MiB page cache, no measured optimal profile or 1-GiB real-HDD acceptance.
- READMEs, Docs and implementation statuses updated. PR #3 remains draft, unmerged; no NuGet publication. [Tuning guide](ExchangeHouse/Markdown/Docs/Paging-Profiles/en.md).

## 2026-11-03 — reusable offline HTTPS Markdown cache for future Docs Reader

- Implemented `ExchangeHouse/Markdown/MarkdownRemoteDocumentCache.cs` as an opt-in per-user source cache separate from disposable page and fence indexes and the existing Showcase session-only HTTPS spool.
- Supports conditional HTTP GET with ETag/Last-Modified and 304 reuse, no-network explicit offline mode, safe stale fallback for broken connections/5xx, forced refresh, immutable cached versions, manual export and per-URL removal. Streamed download buffer stays 64 KiB; per-document default 256 MiB quota.
- Added mock-HTTP regression tests for freshness, interruption, preservation, quotas, security and exports; English README and topic documentation updated. Cached source data may still be purged by OS policy; no reliable permanent storage without explicit SaveCopy.
- The **Docs Reader, GitHub tree/manifest provider, remote TUI selector, total cache quota, cross-process coordination and 1-GiB HDD acceptance remain future work**. PR #3 draft, no merge/release. [Cache contract](ExchangeHouse/Markdown/Docs/Offline-Document-Cache/en.md).

## 2026-11-03 — safe total disk budget for reusable offline Markdown cache (draft PR #3)

- Added default 1 GiB configurable combined cache-owned source/manifest
  disk quota, independent of per-document 256 MiB transfer limit and
  separate disposable Markdown page/context indexes.
- Added bounded-memory `GetUsageAsync`, explicit
  `PruneObsoleteVersionsAsync` (24-hour default grace), protection of
  the valid manifest-selected copy and fail-without-eviction behavior
  on exhausted storage. Cached offline reads remain available even
  if quota is exhausted.
- Tests cover quota rejection, previous version preservation, usage
  accounting, multi-URL pruning, grace interval and corrupt metadata.
  No global automatic LRU, process-crossing locks, orphan-part cleanup,
  1-vCPU/1-GiB physical HDD benchmark or finished Docs Reader.
- Docs and README updated. PR #3 remains draft and unmerged,
  no NuGet publication. [Cache details](ExchangeHouse/Markdown/Docs/Offline-Document-Cache/en.md).

## 2026-11-03 — cache disk quota verified during unknown-length transfer

- Added regression using a deliberately missing HTTP Content-Length:
  combined cache quota is enforced on received bytes, incomplete
  download is cleaned up and the prior offline Markdown copy survives.
  No real-HDD Debian VM measurement or Docs Reader UI integration yet.

## 2026-11-03 — 16-KiB cache manifest memory guard

- Bounded cached HTTP metadata to 16 KiB before parsing and
  publishing. Regressions cover oversized manifests and ETag validators,
  with retention of old offline source and no orphan download files.
- Source-file stream limits and user-controlled disk quota remain
  independent; no Docs Reader TUI or 1-GiB HDD field acceptance yet.
