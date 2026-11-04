# Afrowave.Toolbox — repository implementation status

**Updated 2026-10-31. Source of truth:** [GitHub master](https://github.com/afrowaveltd/Toolbox/tree/master). This file is a **cross-project continuation overview**; authoritative technical details remain in each component's `IMPLEMENTATION_STATUS.md`.

## 2026-11-02 — repository-wide NuGet-first release decision

- **Agreed policy, not a new package publication:** independently usable,
  tested and documented reusable libraries should be published on NuGet
  promptly, even as prereleases. Repeated `preview.N` revisions are
  welcome; don't wait for a complete family or a stable 1.0.0.
- External consumers depend on stable package IDs and intentionally chosen
  versions instead of cross-repository source paths. Keep source project
  references for local integration when useful.
- Every published version must have a validated `.nupkg`, passing relevant
  tests, working independent consumer restore, documentation, a unique
  version and recorded publish confirmation. NuGet consumers must opt into
  preview updates where applicable; no automatic upgrade is promised.
- Record package releases and their concrete URLs in the owning project's
  `IMPLEMENTATION_STATUS.md`; don't confuse version metadata with actual
  publication. No additional package was published as part of this
  documentation-only decision.
- Policy: [NuGet-first preview releases](Docs/NuGet-Release-Policy/en.md).
  Current published/unpublished snapshot:
  [Release status](Docs/Release-Status/en.md).

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

## 2026-11-03 — Markdown offline cache acceptance contract

- The standalone Markdown viewer remains **design-only**, while the existing local Showcase reader remains functional and previously CI-verified.
- Documented durable cache behavior: online conditional refresh, visible offline/stale fallback, crash-safe validated writes, separate per-user cache location and explicit user-controlled cleanup. [Cache policy](Blackboard/SpectreConsole/Markdown/Docs/Document-Cache-Policy/en.md) and [renderer status](Blackboard/SpectreConsole/Markdown/IMPLEMENTATION_STATUS.md).
- No cache or `mdview` code is claimed; .NET tests and full CI have not been run for this documentation increment.

## 2026-11-03 — verified Markdown disk cache initial code

- Added separate .NET 10 [Markdown.Cache](Blackboard/SpectreConsole/Markdown.Cache/README.md) assembly and [xUnit tests](Blackboard/SpectreConsole/Markdown.Cache.Tests/README.md), registered in `Toolbox.sln`.
- Implemented bounded streamed spooling, strict UTF-8/control validation, SHA-256 content-addressed blobs, atomic source-index publication, independent cache read verification and prior-entry preservation on failed replacements. Documentation and per-project implementation checkpoints are present.
- **Not yet CI verified:** new tests authored but have not run locally (dotnet SDK unavailable). Do not claim successful builds. No HTTP/GitHub refresh, offline viewer UI, cache eviction or cross-process hardening yet.

## 2026-11-03 — Markdown.Cache writer safety increment

- Improved [Markdown.Cache](Blackboard/SpectreConsole/Markdown.Cache/README.md) with an exclusive cross-process writer lock, direct symlink/reparse-point checks, content-addressed blob repair and a default per-user cache directory helper.
- Authored four additional xUnit tests (ten in total). [Cache status](Blackboard/SpectreConsole/Markdown.Cache/IMPLEMENTATION_STATUS.md) records unresolved TOCTOU/permissions and crash-durability limitations.
- **Build/test verification still outstanding:** .NET SDK unavailable in local execution and no successful CI run observed for these commits. No HTTP or GitHub reader integration yet.

## 2026-11-03 — HTTP conditional-document cache slice

- Added a conditional HTTP source to [Markdown.Cache](Blackboard/SpectreConsole/Markdown.Cache/README.md) with persisted ETag/Last-Modified validators, full validated `200` replacement, `304` reuse and explicit stale fallback for offline errors. Separate fake-handler tests were authored for positive and negative HTTP paths.
- Source layer remains separate from Markdown parsing and terminal rendering. No standalone `mdview`, GitHub README discovery, cache eviction or public package release.
- **Verification pending** for new source and tests: no local .NET SDK, and no successful GitHub CI result established for this increment.

## 2026-11-03 — public GitHub preferred README source

- Added [GitHubReadmeSource](Blackboard/SpectreConsole/Markdown.Cache/Docs/GitHub-Readme/en.md) to independent Markdown.Cache with official GitHub REST README retrieval, raw Markdown media type, case-insensitive repository cache keys and persistent conditional HTTP validation.
- Added in-memory HTTP regression cases; no GitHub-auth tokens, ref/file selection, redirects, linked documents or CLI application are implemented in this slice.
- **Tests/CI unverified** for this new feature (no local .NET SDK). Await concrete build/test evidence before claiming a passing release.

## 2026-11-03 — initial mdview CLI application

- Created [mdview](Blackboard/Tools/MdView/README.md) .NET 10 executable and [CLI grammar tests](Blackboard/Tools/MdView.Tests/README.md); registered both in `Toolbox.sln`.
- Direct/local HTTP(S)/GitHub README commands reuse existing Markdown renderer, FileBrowser complete strict local decoder and verified remote cache source adapters. No-argument path has a simple text-menu prototype, not the planned Spectre picker/pager.
- Whole source and Markdown model are still materialized in memory; no new 1 GB streaming claim or installed `mdview` executable distribution.
- **Build/test and terminal acceptance remain unverified** in this execution environment (no SDK). Follow-up: confirm Linux/Windows CI and add e2e and interactive FileBrowser tests.

## 2026-11-03 — Windows build/test feedback and focused corrections

- User-provided `dotnet test Toolbox.sln` output showed **Markdown.Cache 32/32**, ExchangeHouse Markdown 88/88, Blackboard Markdown 34/34, FileBrowser 160/160, Essentials 3198/3198 and WhenItFails 1475/1475 passed. The solution was **not green**: MdView had three CS0103 unresolved reader symbol errors and UnderstandMe Configurator had two English UI expectation failures (42/44 passed).
- MdView now links the original Showcase `TextFilePreviewReader.cs` source and imports its actual namespace. This is a temporary compilation bridge until the reader is extracted as a shared library rather than duplicating implementation.
- The two Configurator tests now inject isolated preference files, preventing an installed Czech language preference from changing tests intended to exercise English UI. Production UI untouched.
- .NET 11 preview `NETSDK1057` is informational; CS1591/CS1573 XML documentation warnings and xUnit2013 analyzer warnings are nonfatal and remain for cleanup. **Full corrected Windows/Linux CI is not yet established.**

## 2026-11-03 — full Windows solution test confirmation (user-run)

- User supplied the output of `dotnet test Toolbox.sln` on Windows, using .NET SDK `11.0.100-rc.3.26522.148` targeting `net10.0` projects. **All 6,438 tests passed; 0 failed, 0 skipped** across twelve test assemblies. This is a user-reported local run, not an independently observed GitHub Actions CI result.
- Per assembly: Essentials 3198; WhenItFails 1475; Setter 1241; FileBrowser 160; UnderstandMe 111; Configurator 44; ExchangeHouse Markdown 88; Blackboard Markdown 34; Markdown.Cache 32; mdview 10; Blackboard.Showcase 39; Markdown.TextMate 6.
- Confirms that the corrected mdview source link/import now compiles and the two Configurator locale-isolation test fixes pass. Prior unverified checkpoints for these sources are superseded by this Windows test run.
- Remaining nonblocking warnings: CS1591/CS1573 on public XML API comments in Markdown.Cache and MdView; two xUnit2013 assertions in ExchangeHouse Markdown tests; and NETSDK1057 informing that the SDK is preview. Still pending: clean Linux CI verification, interactive mdview smoke test, and extraction of complete text reader from the Showcase sample into a neutral shared project.

## 2026-11-03 — extracted standalone TextFiles library

- Moved the strict BOM-aware, terminal-safe `TextFilePreviewReader` implementation out of the Blackboard.Showcase executable into [Blackboard.TextFiles](Blackboard/TextFiles/README.md), preserving functionality while changing its namespace to `Afrowave.Toolbox.Blackboard.TextFiles`.
- Both Showcase (text, JSON, Markdown previews) and [mdview](Blackboard/Tools/MdView/README.md) now use a normal `ProjectReference` to the shared assembly; mdview's temporary linked `Compile Include` is removed and the original Showcase copy deleted.
- Added five isolated `TextFiles.Tests` cases and registered new library + tests in `Toolbox.sln`; all changes and docs committed directly to GitHub.
- **Verification not yet established after extraction.** The user-provided 6,438/6,438 Windows pass applies to the preceding snapshot. Next run: `dotnet test Toolbox.sln` on updated checkout, plus manual `mdview local README.md` smoke test.

## 2026-11-03 — Windows TextFiles extraction test evidence gap

- The user-supplied Windows `dotnet test Toolbox.sln` log after extraction shows all **6,438 tests that actually ran** passed with 0 failures/skips, and `Afrowave.Toolbox.Blackboard.TextFiles.dll` built successfully. `mdview.dll` also built; its 10 tests and Showcase's 39 tests passed.
- **Do not call the new reader-specific test suite green yet:** `TextFiles.Tests` is absent from the run output, despite existing in the GitHub solution/project files (28 C# projects registered vs 27 projects reported for restore). This is a missing-coverage issue, not a failing assertion.
- Added x64/x86 ActiveCfg/Build.0 configuration mappings for both new projects in `Toolbox.sln` to prevent solution-configuration skips. This likely explains omission; verification remains pending.
- Run `dotnet test Toolbox.sln` again after syncing and verify `TextFiles.Tests` appears with 5 passed. If still absent, run `dotnet test Blackboard/TextFiles.Tests/Afrowave.Toolbox.Blackboard.TextFiles.Tests.csproj` and inspect the active solution platform.

## 2026-11-03 — TextFiles solution verification confirmed on Windows

- User-supplied full `dotnet test Toolbox.sln` log after the solution-platform configuration fix confirms **6,443 tests passed / 6,443 total, 0 failed, 0 skipped** across all 13 test assemblies using Windows and `.NET SDK 11.0.100-rc.3.26522.148` with `net10.0` targets.
- The previously missing `Afrowave.Toolbox.Blackboard.TextFiles.Tests.dll` is now explicitly built and executed: **5/5** tests passed. `TextFiles` library, `mdview` and Showcase all built; `MdView.Tests` 10/10, Showcase.Tests 39/39.
- This closes the extraction test-coverage gap. Remaining non-blocking warnings are XML documentation CS1591/CS1573, two xUnit2013 suggestions and NETSDK1057 preview SDK notices. No claim of a separate Linux CI or interactive manual `mdview` acceptance test.
- Next: manual `mdview` smoke testing and then Spectre.Console FileBrowser selection/pager integration. Preserve the verified snapshot during incremental changes.

## 2026-11-03 — first interactive mdview source picker

- Replaced mdview's no-arg numbered `Console.ReadLine` menu with a Spectre.Console menu and real `SpectreFileBrowser` for local `.md`/`.markdown` files. HTTPS and GitHub source entry use Spectre prompts. Esc cancels the nested browser back to source menu, or exits at the root menu. No changes to the parser, renderer or HTTP cache.
- Added [interactive picker tests](Blackboard/Tools/MdView.Tests/README.md) using injected TestConsole and isolated files; **11 new xUnit cases** are authored. Projected totals if all pass: MdView.Tests **21/21**, solution **6,454/6,454**. These are **not yet confirmed**: the last user-reported green Windows checkpoint was **6,443/6,443** before this update.
- [Picker documentation](Blackboard/Tools/MdView/Docs/Interactive-Source-Picker/en.md) and CLI README were updated. Next run: `dotnet test Toolbox.sln` after pulling current commits; also test no-args interactive menu manually. Low-memory paging remains future work.

## 2026-11-04 — Markdown code syntax highlighting across bundled languages

- User reported that C# code was colored but JavaScript lacked highlighting in the standalone mdview experience. Root cause: mdview's `MarkdownSpectreRenderer.Render` call used the default plain-code theme rather than the existing optional TextMate adapter.
- Added `MdViewHighlighting.CreateTheme()`, referencing `Blackboard.SpectreConsole.Markdown.TextMate` and passing the syntax-enabled theme for all local, HTTPS and GitHub Markdown rendering. Missing native assets gracefully degrade to safe plain-code rendering.
- TextMate's language resolution now attempts all installed grammar IDs and extension-based registry lookups, with comprehensive aliases for common Markdown fence spellings. TextMateSharp.Grammars 2.0.4 provides 50+ languages; unsupported grammars remain plain text. The base Markdown renderer and ExchangeHouse parser gain no new native dependency.
- Expanded TextMate grammar tests (alias families, JS/TS/Python/JSON/YAML/web/SQL/etc.) and mdview syntax-wiring tests. Added JavaScript, JSON and YAML demo code in Showcase and corresponding checks. [Supported Grammars documentation](Blackboard/SpectreConsole/Markdown.TextMate/Docs/Supported-Grammars/en.md).
- **Changes committed but not yet compiled/tested** in the current environment. Previous Windows 6,443/6,443 test baseline predates both this feature and the immediately prior interactive picker increment; do not claim a new green count. Next: `dotnet test Toolbox.sln`, then manual colored `mdview` local/GitHub check, and adjust any missing grammar IDs observed in that run.

### Grammar-discovery refinement (2026-11-04)

- The new adapter now explicitly prefers exact bundled language IDs over fallback aliases and tries installed extension lookup using both original and normalized hints; added a conditional `.jsx` resolution test. README, supported-grammars guide and example links reflect this precedence. Full compilation/tests still pending for this increment.

## 2026-11-04 — owner-only GitHub repository browser

- Implemented GitHub user/organization public repository catalog in Markdown.Cache and Spectre selection in mdview, supporting `mdview afrowaveltd`, `mdview git afrowaveltd` and `mdview github:afrowaveltd`.
- Catalog has a separate SHA-256 verified 30-minute cache, offline stale fallback, bounded 100-item pages (ten maximum) and explicit truncation warning. No private repositories, authentication or README downloads during listing.
- Added fake-HTTP cache tests, owner-only CLI parser tests and virtual-terminal chooser tests. New tests not yet run; the prior 6,443-green Windows baseline predates this addition.

## 2026-11-04 — first bounded-memory, disk-indexed source pager

- Added [TextFiles.Utf8DiskLineIndex](Blackboard/TextFiles/Docs/Disk-Line-Index/en.md), a separate strict-UTF-8 physical-line index and bounded random-access page reader. One sequential scan validates the entire file before exposing lines; 64-bit line offsets are written to a temporary on-disk table, capped at 256 MiB by default. No full-file Markdown parse/model is held by this new reader. UTF-8 BOM, CRLF/LF/CR and Unicode splits are supported, while invalid UTF-8 and unsafe terminal controls are rejected.
- New opt-in `mdview source PATH` runs a read-only **raw source** TUI pager with Next, Previous, Jump to physical line and Exit; the existing formatted `mdview local/URL/GitHub` behavior remains unchanged. This is not yet an incremental Markdown renderer or visual-line-aware pager; source/visual line breaks and Markdown container states differ.
- Added 15 TextFiles index tests, six mdview terminal paging tests and one new CLI grammar test. Updated project/root README, English docs and per-project status files.
- **New tests not run** in the current environment; previous user-confirmed green Windows solution result (6,443/6,443) is an older checkpoint, before owner repository selection, syntax highlighter and this pager work. Next: `dotnet test Toolbox.sln`, manual `dotnet run --project Blackboard/Tools/MdView -- source README.md`, then stateful incremental Markdown parser/block index.
