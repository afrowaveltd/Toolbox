# ExchangeHouse.Markdown — Implementation status

## 2026-10-29 — block scanner and lossless source writer

- Added independent .NET 10 `Afrowave.Toolbox.ExchangeHouse.Markdown` project, no external package dependencies.
- Added source-preserving first-pass `MarkdownParser`, `MarkdownDocument` / `MarkdownBlock` and `MarkdownWriter`.
- Recognizes headings, list markers, blank lines and fenced code without interpreting protected code as markup. Preserves original line endings, source offsets and exact unmodified round trips.
- Added xUnit tests in `../Markdown.Tests`; registered both projects in `Toolbox.sln`.
- Does **not** implement inline parsing, editable AST, format conversion or full CommonMark behavior. Does not yet integrate with Configurator; existing Help renderer remains in place.
- **Verified:** [GitHub Actions #40846902206](https://github.com/afrowaveltd/Toolbox/actions/runs/40846902206) SUCCESS for commit `3252650b`; whole solution build and test succeeded. NuGet publishing is still deferred until API scope and release readiness are explicitly reviewed.

### Next

1. Confirm CI and manual development build.
2. Design/test proper inline nodes (emphasis, code spans, links including escaped delimiters and reference links) before rewriting.
3. Implement editable writer plus loss-awareness for future text/HTML converters.
4. Build independently packageable Blackboard Spectre.Console Markdown renderer with customizable theme.

## 2026-10-29 — conservative inline tokens

- Added `MarkdownInlineParser`, `MarkdownInlineNode` and `MarkdownInlineKind` with raw source spans, source offsets, nested children and separate link destinations.
- `MarkdownParser` populates inline nodes on heading/list/paragraph lines and excludes fenced code completely. Conservative support for emphasis, strong text, backtick code spans, and simple inline links; malformed syntax is retained literally.
- Added targeted regression tests for escaped brackets, nested link destination parentheses, nested emphasis, absolute offsets, preserved URLs/email as code, and source-preserving write.
- No reference-style links, images, link titles, full delimiter algorithm, editable writer or conversion API yet. Integration with the Configurator and Spectre theme remains a separate future increment.
- CI verification pending at initial commit; do not assert success until the workflow finishes.

### Verified CI — 2026-10-29

- [GitHub Actions #40848846720](https://github.com/afrowaveltd/Toolbox/actions/runs/40848846720) **SUCCESS** for inline parser commit `b566ad670bb58e52049c714b8dbba6d1f3838287` — full solution build and test passed.
- This verifies the conservative supported syntax and regressions only. Full CommonMark, editable AST, conversion and UI rendering are not yet claimed.

## 2026-10-29 — initial image and link metadata

- Added image tokens with exact destination offsets and optional titles; retained original link API construction and raw source preservation. Tests cover valid/malformed forms. No full CommonMark. CI pending.

### Verified cross-platform CI — 2026-10-29

- Parser implementation commit `fb7a8108`: [GitHub Actions #40881801829](https://github.com/afrowaveltd/Toolbox/actions/runs/40881801829) **SUCCESS** — full Linux solution build/tests and existing Windows regression job.
- Image presentation implementation commit `7696fef8`: [GitHub Actions #40882016971](https://github.com/afrowaveltd/Toolbox/actions/runs/40882016971) **SUCCESS** — full Linux solution build/tests and Windows FileBrowser, Configurator, Showcase, localization **plus newly added focused ExchangeHouse Markdown and Blackboard Markdown tests**.
- No direct manual Debian or Windows terminal test of this new image fallback has been reported. Full CommonMark, reference-style links, actual image drawing and translated document link routing are still not implemented.

## 2026-10-31 — list markers and nesting model

- Added immutable `MarkdownListMarker`, `MarkdownListItemNode`, `MarkdownDocument.ListRoots` and optional `MarkdownBlock.ListMarker` while preserving flat Blocks and lossless writing.
- The scanner recognizes nested/mixed and ordered lists (original marker, start number, delimiter, indentation, depth, content offsets), bounded to contiguous single-line items. Tabs recognized at four-column stops; source never normalized.
- Added parser regressions for hierarchy, source spans, fence isolation, non-list markers and interruptions. **CI pending** at implementation commit; no full CommonMark list grammar asserted.

### Cross-platform CI verified — 2026-10-31

- [GitHub Actions #41244480220](https://github.com/afrowaveltd/Toolbox/actions/runs/41244480220) **SUCCESS** for implementation up through commit `415065a4c272838f3bc0e4ed94ab8ea90003a295`.
- Linux: full Toolbox solution **build and tests passed**; final build reported **0 warnings, 0 errors**.
- Windows: **ExchangeHouse.Markdown.Tests 41/41**, **Blackboard.SpectreConsole.Markdown.Tests 13/13**, **Blackboard.Showcase.Tests 26/26**; FileBrowser 160/160, Configurator 44/44, and targeted localization tests 34/34 passed.
- Intermediate runs on parser/renderer commits failed at **test compilation** due to literal `\\`` C# escape sequences mistakenly introduced when authoring Markdown code examples. Fixed in commits `e7b858aa` and `415065a4`; the final CI is green. Existing production source was not weakened to make tests pass.
- Manual real-terminal testing of newly added nested-list visuals in Debian/Windows remains unreported. The current model does not claim CommonMark-compatible loose/multiline lists.

## 2026-10-31 — explicitly indented continuation model

- Added additive source-aware metadata for list continuation lines, and read-only per-item continuation collections. Preserves flat Blocks, raw source, existing constructors and writer output.
- Handles original numbered marker content columns, tabs and reattachment to an ancestor after nested content. Blank/unindented lines terminate attachment. No loose/lazy CommonMark claims.
- Added five focused parser regression tests and [design/behavior notes](Docs/List-Continuations/en.md). **CI verification pending** for this increment.

### Verified CI — 2026-10-31 list continuation increment

- [GitHub Actions #41248289922](https://github.com/afrowaveltd/Toolbox/actions/runs/41248289922) **SUCCESS**, source through commit `35efe934031a99a5407550f6624d96656b9c03a3`: Linux full solution build/tests passed; Windows ExchangeHouse Markdown **46/46**, Blackboard Markdown **16/16**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**.
- Markdown local link job **PASS**: 269 Markdown files, 1,374 local file targets checked with none missing. Source-aware continuation behavior also tested on Windows; manual Debian and real-terminal visual checks remain separate.
- Initial renderer integration CI failed **one incorrect test expectation** (custom indent `....` erroneously checked as six literal spaces). Fixed the assertion in `35efe934`, retaining custom-indent coverage. The failed earlier CI runs are not counted as successes.
- This milestone handles only **explicitly indented** continuation lines. Loose lists, lazy continuation and nested CommonMark block containers remain unsupported. No public NuGet release was made.

## 2026-10-31 — bounded loose-list paragraph recognition

- Added deferred blank-line ownership for explicitly indented continuation paragraphs without scanning ahead repeatedly. New additive `ListBlankOwnerStart`, `SeparatorBlocks`, `ContentBlocks` and `IsLoose` preserve existing public record construction and unchanged-source writing.
- Supports multiple blank separators, nested-child to parent continuation ownership, tabs and original link offsets. Trailing blanks, unindented text and blank-before-list-marker are not attached. No full CommonMark or nested loose list container semantics claimed.
- Added six focused tests and documentation in `Docs/Loose-List-Paragraphs/en.md`. CI pending for this source commit.

### Verified CI — 2026-10-31 loose-list paragraph milestone

- [GitHub Actions #41249127101](https://github.com/afrowaveltd/Toolbox/actions/runs/41249127101) **SUCCESS** on source/docs commit `df4da1b285d280cbe1d17dee0fded388f178d0b8`: full Linux solution build/tests and Windows UI/Markdown regressions passed.
- Windows focused test results: ExchangeHouse Markdown **55/55**, Blackboard Markdown **19/19**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**. Earlier linked documentation job verified **271 Markdown files and 1,388 local links** with no missing local targets; the final run's link job also passed.
- Added source-preserving `IsLoose`, blank separator/content collections and source owner metadata for explicitly indented blank-separated list paragraphs. Safe terminal rendering aligns subsequent paragraphs beneath the original marker; Showcase updated. No source rewrite, full CommonMark grammar or NuGet publication.
- Interim #41249032554 failed one incorrect custom-indent **test expectation**, corrected in `2b34cb46`, without production renderer change. Later CI is green. Manual acceptance of new visuals in real Debian/Windows terminals remains pending.

## 2026-10-31 — bounded lazy list paragraph continuations

- Recognize unindented paragraph lines immediately following active list text without a blank boundary. They belong to the deepest current item; later explicitly indented lines can resume an ancestor.
- New additive `MarkdownBlock.IsLazyListContinuation` identifies this case. Original `Blocks`, `ListRoots`, content/owner offsets, raw line terminators and unchanged-source writer are preserved.
- Reject heading, fence, blockquote and thematic-break starters as lazy continuation candidates; do not attach after an unresolved blank. Existing renderer consumes the same source-owner metadata and requires no production change.
- New parser, renderer and theme-gallery coverage, plus [technical notes](Docs/Lazy-List-Continuations/en.md). **CI pending for this commit**; no NuGet release or full CommonMark claim.

### Final green lazy-paragraph checkpoint — 2026-10-31

- [GitHub Actions #41250061417](https://github.com/afrowaveltd/Toolbox/actions/runs/41250061417) **SUCCESS** for commit `f6b7fc555c76d964ec12a27f86accc1beab3e9f8`: Linux full solution build and tests passed, Windows targeted UI/Markdown suites passed and local Markdown documentation-link check passed.
- Windows counts: **ExchangeHouse Markdown 60/60**, **Blackboard Markdown 21/21**, **Showcase 26/26**, **FileBrowser 160/160**, **Configurator 44/44**, **UnderstandMe localization 34/34**.
- Documentation audit: **273 Markdown files, 1,399 local targets checked**, zero missing links. Cross-family link typo from the first commit was fixed in `2c8c3fa`; the final gallery sample now uses an actually unindented lazy line.
- No public Markdown NuGet publication, no full CommonMark or real-terminal Debian acceptance is implied by CI.

## 2026-10-31 — code fence and quote list containers

- Added `MarkdownBlockKind.BlockQuote` and source-aware quote depth with inline offsets.
- Added fence info/language/closing metadata on opening, code and closing lines, and retained attached list owner on indented fences, their code lines and quotes.
- `MarkdownListItemNode.EmbeddedBlocks` exposes attached code fences/code lines/quotes without redefining `ContinuationBlocks` (paragraphs only); `ContentBlocks` preserves all attached content in source order.
- New regression tests include literal code, nested numbered list ownership, blank separators, quote nesting and verbatim writer. Conservative scope; no nested arbitrary quote AST or full CommonMark.
- [Code and quote containers](Docs/Code-and-Quote-Containers/en.md). CI pending for final implementation.

### Cross-platform green verification — 2026-10-31

- [GitHub Actions #41251065292](https://github.com/afrowaveltd/Toolbox/actions/runs/41251065292) **SUCCESS** for source commit `5b80ad1e3f9cde50ae793da53b3b81c30d727d08`. Linux complete solution restore/build/tests passed, Windows targeted UI and grammar tests passed, and Markdown link validation passed.
- Windows: **ExchangeHouse Markdown 66/66**, **Blackboard Markdown 24/24**, **TextMate adapter 6/6**, **Showcase 26/26**, **FileBrowser 160/160**, **Configurator 44/44**, **UnderstandMe localization 34/34**.
- The documentation link audit examined **280 Markdown files and 1,423 local targets** with no missing targets. A later docs-only commit clarified a nested-fence illustration; final docs CI is tracked separately.
- Real TextMate C# tokenization was exercised under Linux and Windows CI, including platform-specific Oniguruma dependencies. This does **not** establish manual SSH/tmux or Debian terminal acceptance, complete CommonMark support, NuGet publication, or a completed third-party transitive-license review.

## 2026-11-01 — source-aware quoted list containers

- Additive `MarkdownDocument.QuoteListRoots` contains quoted list roots in source order without mixing them with `ListRoots` outside blockquotes.
- Recognizes ordered/unordered list markers after explicit quote prefixes, preserving quote depth, original marker/number, nested list depth, indented and lazy continuation ownership and inline offsets.
- Quote depth changes, unquoted lines, quote-prefixed headings and empty quote lines interrupt active quoted-list hierarchy.
- Five focused parser tests plus [quoted lists guide](Docs/Quoted-Lists/en.md). Fenced code *inside* quoted containers and full CommonMark remain future work. CI pending; no new NuGet release.

### Verified cross-platform quoted-list milestone — 2026-11-01

- [GitHub Actions #41251673070](https://github.com/afrowaveltd/Toolbox/actions/runs/41251673070) **SUCCESS** for feature commit `aa3ec1246fc3125f17d1a506e415cc3ad7db418b`: Linux full solution build/test, Windows targeted UI tests and documentation-link job passed.
- Windows results: ExchangeHouse Markdown **71/71**, Blackboard Markdown **26/26**, optional TextMate **6/6**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**.
- The subsequent documentation commit introduced one incorrect cross-family relative link, fixed in `1e89cf7532c03737a0599d51c26b54cd57bb345a`. Link verification on the fix run passed; final complete CI for the documentation state is tracked independently.
- No new NuGet publication, full CommonMark conformance, or real-terminal acceptance is claimed. This feature intentionally does not tokenize fenced code inside explicitly quote-prefixed containers.

## 2026-11-01 — explicit quoted code fencing

- Same-depth quote-prefixed backtick/tilde code fences now retain literal body, language metadata, source spans/CRLF, nested quoted-list owner, matched closer and uninterrupted writer output.
- A missing/differently deep quote prefix terminates that fence and reclassifies the boundary line normally. No unmarked lazy quote continuation/full CommonMark claim.
- Parser regression suite passed on [CI #41254427784](https://github.com/afrowaveltd/Toolbox/actions/runs/41254427784). [Design](Docs/Quoted-Code-Fences/en.md). No public package release.

### Full cross-platform CI checkpoint — 2026-11-01

- [GitHub Actions #41254717060](https://github.com/afrowaveltd/Toolbox/actions/runs/41254717060) **SUCCESS** for final code/English-docs fix commit `8f25d8e2a72df6ffe915362ef11d64dbd6c89d6a`: Linux complete solution build/tests, Windows targeted UI/Markdown tests and repository local documentation-link validation passed.
- Windows focused tests: ExchangeHouse Markdown **75/75**, Blackboard Markdown **29/29**, Markdown.TextMate **6/6**, Blackboard Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, UnderstandMe localization **34/34**.
- Markdown local-link checker verified **284 files / 1,452 local targets**, zero broken links. An earlier documentation-only CI failed one wrong Showcase-to-renderer relative path, fixed in `8f25d8e`; the final checkpoint above is green.
- No new NuGet release or manual real-terminal Windows/Debian acceptance claimed.

## 2026-11-01 — source-aware loose quote lists

- Deferred `> ` / `>` separator lines can attach to the deepest eligible same-depth quoted list item (including an ancestor of the last numbered child). They retain `BlockQuote` kind, quote depth and raw source.
- Attachment uses existing `ListBlankOwnerStart`, `SeparatorBlocks`, `ContentBlocks`, `IsLoose`. Multiple quote-marked blanks and CRLF preserved; inline link offsets still absolute. No ownership after a new marker, non-indented quoted text, depth switch or unquoted boundary.
- Quoted fence literal blank lines remain `CodeLine`, never list separators. [Quoted loose paragraph design](Docs/Quoted-Loose-Paragraphs/en.md). Five focused parser tests (+ theory cases); CI pending.

### Verified cross-platform milestone — 2026-11-01

- [GitHub Actions #41256065289](https://github.com/afrowaveltd/Toolbox/actions/runs/41256065289) **SUCCESS** on source/documentation commit `882f15862d89214e8cc1204d8b5b7baaedd6d18a`: Linux complete solution build and tests, Windows UI/Markdown targeted tests and local Markdown documentation links all passed.
- Windows tests: ExchangeHouse Markdown **82/82**, Blackboard Markdown **31/31**, TextMate adapter **6/6**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**.
- Markdown checker processed **286 files / 1,470 local targets**, with zero missing local links.
- The test-preserving increment has not been published to NuGet and has not undergone manual Debian/Windows/SSH visual acceptance; full CommonMark nesting remains outside this milestone.

## 2026-11-01 — quote list nesting under ordinary items

- Additive outer source owner offset + marker on `MarkdownBlock`, preserving separate ordinary/quoted list trees and exact input/writer offsets.
- Explicitly indented quotes, children/paragraphs, blank separators and quoted fences remain owner-aware. Equal quote depths in different outer items do not merge. The parser prioritizes checking active list content indentation before accepting a standalone quote.
- Six new parser regression cases, including code literal safety, CRLF/link offset and nested owner boundary checks. [Parser docs](Docs/Quoted-Lists-Inside-Lists/en.md).
- [CI #41257477307](https://github.com/afrowaveltd/Toolbox/actions/runs/41257477307) **SUCCESS**, parser **88/88**. No NuGet publication.

## 2026-11-02 — experimental indexed streaming scanner and diskless stress harness

- Introduced forward-only MarkdownStreamingReader.ScanAsync with 64-bit source character offsets, bounded input-line buffer and minimal open-fence/list index state. Long code fences do not accumulate in memory.
- Stress runner: Toolroom/Markdown/Stress, with deterministic virtual Markdown for 10M, 100M, 1G and 10G; optional Spectre render to null output; 50k-line open-fence regression.
- Existing full-document parser and normal Showcase viewer remain unchanged. Full CommonMark-equivalent streaming state, deferred references and paged terminal navigation are not yet supported.
- **CI and large stress execution pending**; the change is on a review branch, not asserted merged to master.

## 2026-11-02 — UTF-8 disk-backed source page index (draft PR #3)

- Added `MarkdownDiskPageIndex.BuildAsync` and `ReadPageAsync`: index
  64-bit little-endian **byte offsets** to a temporary disk file with one
  entry per fixed number of physical lines. Each page load has bounded
  memory independent of total source length; supports reverse/forward
  random access and preserves original source. Index does not materialize in RAM.
- Full streaming UTF-8 decoding with strict fallback and terminal-control
  rejection before any page is displayed; CRLF/LF/CR recognition,
  configurable line/page limits, 256-MiB default index quota and cancellation. Temp index deleted
  on failures and disposal; file size and timestamp checked for changes.
- Added unit tests for Unicode/emoji, BOM, byte offsets, line breaks,
  25,000 lines/500 on-disk offsets, random access, invalid controls,
  page bounds, mutations and cleanup.
- **Not** a CommonMark-semantic page or complete viewer; only raw source.
  A future page-state checkpoint is needed for correct multi-page fences,
  lists and reference definitions. No giant interactive viewer is
  claimed validated. No new NuGet publication.
- [Index contract](Docs/Disk-Backed-Paging/en.md). CI evidence for this
  branch is tracked by [PR #3](https://github.com/afrowaveltd/Toolbox/pull/3).
