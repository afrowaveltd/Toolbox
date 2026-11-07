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

## 2026-11-04 — optional incremental Markdown physical-line scanner

- Created `MarkdownIncrementalLineScanner`, `MarkdownStreamLineKind`, and `MarkdownStreamLine` in the existing `ExchangeHouse/Markdown` .NET project, without modifying `MarkdownParser.Parse` or rendering behavior.
- `ScanAsync(TextReader)` uses a 4096-character source buffer and yields bounded physical lines with exact CRLF/LF/CR terminators, UTF-16 source offsets and a stateful **top-level** fenced code scope (opening/closing markers, language hints), plus ATX headings and opaque list/quote markers.
- Enforces a configurable per-line character bound (default 1 Mi UTF-16 units, maximum 4 Mi), checks cancellation and rejects unsafe controls. Since output is progressive, callers must separately validate entire untrusted byte streams before permitting terminal output; source encoding validation and full CommonMark/nested container ownership are not claimed.
- Added `MarkdownIncrementalLineScannerTests` (13 Facts + 3 Theory rows = **16 new cases**) with round-trip newlines, chunk-boundary CRLF, fenced-code state, heading parity for a flat subset, opaque container preservation, Unicode offsets, cancellation, limits and late-invalid input. No source-to-renderer integration yet.
- [English technical documentation](Docs/Incremental-Source-Scanner/en.md). **Compilation/test execution pending** in this environment (no .NET SDK); the last maintainer-confirmed Windows green full-solution run predates this addition. Next: confirm `dotnet test Toolbox.sln`, then build and compare completed block events with the existing parser before adapting mdview.

## 2026-11-07 — incremental completed-block span projection

- Added `MarkdownIncrementalBlockScanner` with additive public `MarkdownStreamBlock` record and `MarkdownStreamBlockKind` enum. It turns the existing async physical-line events into **non-overlapping source ranges** for top-level headings, simple paragraph runs, blank lines, top-level code fences, and opaque quote/list marker lines.
- Each span contains decoded UTF-16 start/end offsets and zero-based physical line boundaries. Code fence language, heading level, and explicit `FenceClosed=false` on unterminated EOF remain available. A long paragraph or fence costs constant-size **block metadata**, not an in-memory copy of its contents. No source text is silently discarded.
- The conservative scanner explicitly **does not** support nested list/quote ownership, in-container fences, full CommonMark rendering or byte-offset random access; the original parser and all mdview/renderer behavior remain unchanged. Untrusted bytes require whole-file validation before progressive terminal output.
- Added 16 focused tests, including long ranges, exact source reconstruction and a restricted golden equivalence check against `MarkdownParser.Parse`.
- [Detailed English documentation](Docs/Incremental-Block-Spans/en.md). Compilation and test results for this change pending. Next: implement well-bounded nested container state and golden comparisons before integrating a formatted visual pager.


### CI verification — 2026-11-07 completed block spans

- [GitHub Actions run 42313009028](https://github.com/afrowaveltd/Toolbox/actions/runs/42313009028) **SUCCESS** on the test-implementation commit `34ad5e5d8d0da6b720230711a91a3984a0bc8383`. Full Linux build reported **0 warnings, 0 errors** and passed all test assemblies, including **ExchangeHouse.Markdown.Tests 111/111** (the new 16 block-span cases included).
- Windows UI regression job and Markdown local link validation also **SUCCESS** in the same run.
- Subsequent changes are documentation/status updates only. No full CommonMark parity, manual terminal rendering or multi-gigabyte performance claims are implied.

## 2026-11-07 — explicit quote/list streaming metadata

- Added public `MarkdownIncrementalContainerScanner`, `MarkdownContainerLine`, and `MarkdownContainerLineKind` with complete English XML API comments, without modifying the existing `MarkdownParser`, `MarkdownIncrementalBlockScanner`, or any rendering paths.
- Parses **explicit** quote nesting up to depth 32, ordered/unordered nested list markers and sufficiently indented continuation owners with a maximum 32 list frames, and top-level/quote-owned code fences including delimiter/quote-depth validation.
- Source is preserved through `MarkdownStreamLine` and UTF-16 offsets. Fences keep literal code protected from accidental list/heading classifications; a quote-depth mismatch interrupts an active quoted fence and the current source line is reclassified. Stale list owners are dropped on unrelated content.
- Does **not** claim lazy continuation ownership, loose-list blank ownership, the outer list owner of indented nested quoted content, complete CommonMark or byte-offset random access. It is an isolated source metadata foundation pending container-aware *completed block* semantics. Does not change mdview output.
- Added 18 fake-stream tests including a restricted parity check with the established full parser, a documentation page [Explicit Container Scanner](Docs/Explicit-Container-Scanner/en.md), and root documentation links.
- Next: verify new CI test run and reconcile any discrepancies, then separately model outer list plus quote-local owners and finish container block boundaries.

### Verified CI — 2026-11-07 explicit container scanner

- [GitHub Actions #42314716633](https://github.com/afrowaveltd/Toolbox/actions/runs/42314716633) **SUCCESS**, including the feature and all **18 new explicit-container tests** after correcting the test's hand-computed CR/LF fixture offsets. Linux clean solution build: **0 warnings / 0 errors**; test assemblies: **6,603 passed, zero failed**. ExchangeHouse Markdown tests: **138/138**.
- Windows UI regression job **SUCCESS**, Markdown local link checker **SUCCESS**. The implementation and test behavior are therefore verified; subsequent README/status updates are documentation-only.
- Scope remains conservative: explicit markers, quote depth and quoted fences only, with no full CommonMark/list ownership parity or visual Markdown paging claim.

## 2026-11-07 — separate outer and quote-local ownership

- Extended `MarkdownIncrementalContainerScanner` with **two independent 32-frame list stacks** and three additive `MarkdownContainerLine` init-only properties: outer ordinary-list offset/depth/marker. Existing positional constructor, inner `ListOwnerUtf16Offset`, quote-local nesting depth and source offset contract remain unchanged.
- Probe explicit indentation before recognizing a standalone quote, retaining the owner of `- parent\n  > quote` even where a bare quote is syntactically plausible. The quote-local stack resets when quote depth or outer-owner identity changes. An unowned standalone quote invalidates stale ordinary-list ownership.
- Protect quote-owned fences across body/closing lines and interrupt them if quote depth or outer-owner identity changes; do not synthesize a closing fence. Tightened fence opening/closing to strip **exactly the owning item's content indentation**, preserving overindented literal code.
- Still **not** full CommonMark or deferred loose-list/blank/lazy continuation ownership. The existing full parser, renderer, mdview and cache are unchanged.
- Added 17 focused regressions, including parity for supported owners with `MarkdownParser.Parse`, UTF-16/Unicode offsets, code fences and invalid overly-indented code markers. New [ownership documentation](Docs/Dual-List-Ownership/en.md). CI result for final snapshot to be recorded separately.

### Compatibility note — overindented list-fence opener

- The full parser `MarkdownParser.Parse` currently strips **all** leading continuation indentation when checking an *opening* fence under a list item. The new incremental scanner strips only the owner's required indentation, so it conservatively rejects an opener with four or more remaining spaces. The full parser already uses a more restricted rule for *closing* fences. This is a **known semantic difference**, captured in `MarkdownIncrementalQuoteOwnershipTests.OverIndentedListFenceOpenerIsNotRecognizedAsCode`.
- Do **not** wire this scanner into the existing rendered mdview path until opener/closer indentation is reconciled and cross-parser golden cases are green. The original parser/renderer is deliberately unchanged.

### Verified CI — 2026-11-07 dual-owner and fence-indentation regressions

- [GitHub Actions #42317726747](https://github.com/afrowaveltd/Toolbox/actions/runs/42317726747) **SUCCESS** on commit `75f6e548da063fe95e39ab2f57d7f7049ecb3fd6`, including production dual-ownership changes, all **17** new quote-ownership cases, the exact-indent fix and the documented full-parser compatibility gap. Linux solution build/tests and Windows UI regressions passed; Markdown link checker passed.
- [GitHub Actions #42317702162](https://github.com/afrowaveltd/Toolbox/actions/runs/42317702162) on test correction commit `25b352ed32a1afe4507c850591ba3188dabe4ee4`: full Linux **6,620/6,620 tests green**, **0 Warning(s), 0 Error(s)**; Windows UI regression and local Markdown links **SUCCESS**.
- A mismatched root-list depth expectation in the initial tests was corrected. A subsequent test asserted false parity for an overindented opening fence; it was changed to record both real behaviors, without changing the existing full parser or renderer. The compatibility gap remains explicitly tracked for future reconciliation.
- The following README and status commits are documentation-only. The new scanner is an opt-in parsing layer and is **not** enabled in mdview.

## 2026-11-07 — deferred loose-list blank and lazy paragraph semantics

- Extended `MarkdownContainerLine` with additive init-only `IsLazyListContinuation` and `ListBlankOwnerUtf16Offset`; the existing positional constructor stays binary/source compatible.
- The existing forward-only `MarkdownIncrementalContainerScanner` now emits `ListContinuation` with `IsLazyListContinuation=true` for uninterrupted, unindented paragraph text. Explicit headings, new list markers, code fences, quote prefixes, thematic breaks and pending blank separators prevent lazy ownership. Ordinary/quote-local blank-pending contexts remain independently scoped.
- Added `MarkdownDeferredListOwnershipScanner.ScanAsync` with a delayed and bounded pending-blank run. Only a subsequent **explicit** `ListContinuation` in the same quote depth/outer ordinary-list scope can claim those blanks. No owner is fabricated after a new item, an interrupted quote, unindented prose or EOF.
- Two resource limits (default 1,024 pending blank lines and 1,048,576 aggregate UTF-16 characters) prevent unbounded buffering. Exceeding either raises `InvalidDataException`; source text and newline fidelity are preserved.
- Added **20 focused tests** for loose-list ownership/laziness, outer-inner quote scopes, bounded memory and incremental emission, plus updated two existing fixtures. Full renderer, API links, TextMate and mdview output remain unchanged.
- [Detailed design](Docs/Deferred-List-Ownership/en.md). Known full-parser fence-opening indentation divergence remains documented; nested containers and formatted streaming paging are not yet fully implemented.
- CI on the functional/test snapshot `9822713461466a4cc2849c3963211075dc14bd27`: Linux solution **6,640/6,640 passing tests, 0 warnings, 0 errors**. Windows and final documentation checks were still in progress when this status was authored.

### Verified CI — 2026-11-07 deferred list-ownership milestone

- [GitHub Actions run 42359532384](https://github.com/afrowaveltd/Toolbox/actions/runs/42359532384), on commit `711df48e5e3038e5c78940b333107f00f6aacd4b`: **SUCCESS**. Linux solution **6,640/6,640 tests passing**, **0 compiler warnings, 0 build errors**. `ExchangeHouse.Markdown.Tests` **175/175** passing (including 20 new deferred-ownership cases); Windows UI regression tests **SUCCESS**, Markdown local documentation links **SUCCESS**.
- The two earlier generic-text fixture expectations were updated for confirmed lazy paragraph ownership; thematic-break fixture was made unambiguous (`***`). These corrections did not require modifications to the established whole-document `MarkdownParser`.
- Subsequent changes only update English README, documentation index and status notes. Full rendered Markdown paging and `F/N/P` indexed navigation remain future milestones.

## 2026-11-07 — completed incremental container block source spans

- Added `MarkdownIncrementalContainerBlockScanner.ScanAsync` as a separate forward-only metadata layer over `MarkdownDeferredListOwnershipScanner`. Additive public `MarkdownContainerBlockSpan` record with XML-documented start/end UTF-16 offsets, physical line range, quote/list/blank owner keys, lazy continuation flag, heading/list/fence metadata; new `MarkdownContainerSpanKind` and `MarkdownFenceSpanEnd` enums fully XML-documented.
- Produces **contiguous non-overlapping source intervals** for top-level headings, plain/quote/list continuation runs, blank runs, list marker lines and completed fenced code. Never accumulates paragraph/fence content, only one pending source interval; inherits line/blank limits and cancellation.
- Fenced spans end with `Closed`, `Interrupted`, or `EndOfFile`, distinguishing interrupted quoted/outer-list scopes without inventing a closer or consuming the interrupting line.
- Preserves **two independent list owner spaces**, resolved blank owner vs unowned, and lazy vs explicit continuation. Separate list-item marker spans do **not** represent complete nested subtrees. UTF-16 source coordinates still need byte-offset mapping against a validated immutable file before random-access disk paging.
- **Not wired to mdview**. The existing full parser, TextMate renderer, cache and UI remain untouched. The known overindented list-owned opening fence discrepancy remains tracked; do not claim complete CommonMark compatibility.
- Added 22 focused tests and [English design/usage documentation](Docs/Incremental-Container-Block-Spans/en.md). Initial CI confirmed all 6,662 tests passing; two xUnit2031 warnings in the new tests were subsequently fixed in commit `1bc41eeca49d003ed5e28f2d0d875c6120ce39fc`. Awaiting final fresh post-fix CI warning verification.

### Verified CI — completed container-aware spans, 2026-11-07

- [GitHub Actions run 42360297028](https://github.com/afrowaveltd/Toolbox/actions/runs/42360297028) on commit `1bc41eeca49d003ed5e28f2d0d875c6120ce39fc`: **SUCCESS** for Linux full solution build and tests (**6,662 passed, 0 failed**), **0 Warning(s), 0 Error(s)**; Windows UI regression job **SUCCESS**, local Markdown documentation links **SUCCESS**.
- `ExchangeHouse.Markdown.Tests`: **197/197 passing**, including all **22** new completed-container-span tests. Earlier two `xUnit2031` warnings were fixed without suppression; post-fix build reports none.
- Subsequent changes are README, documentation-index and implementation-status updates only. No production behavior changes after this verified code snapshot. Known list-owned code-fence opener indentation discrepancy remains explicitly deferred from any mdview renderer integration.

## 2026-11-07 — list-owned opening fence indentation parity

- Changed `MarkdownParser.Parse` to evaluate a list-owned opening code fence after stripping **only the owning item's required content indentation**, not all leading whitespace. Applied to both ordinary and quote-local list continuations. The source-preserving `content` and its offsets remain unchanged for ordinary paragraph continuations.
- Updated `MarkdownIncrementalQuoteOwnershipTests.OverIndentedListFenceOpenerIsNotRecognizedAsCode` to expect `Paragraph` in the established full parser, matching the incremental scanner's `ListContinuation` rather than an incorrectly opened code fence. This resolves the specifically recorded overindented opening-fence discrepancy; it does **not** establish complete parser equivalence.
- Production commit `06cda232002210fcae6df61eb1ca13ed71290111`; regression updates `bdf9de981438eefd20ca58d57c8b90e492ee8a0c` and `710ab0eab0242a4fb848304a46f5efe4eb718541` (added explicit quoted-list parity case). **Fresh CI results pending** for this change; do not count the preceding 6,662 green tests as validating this fix.
- No `mdview`, rendered Markdown pager, indexed F/N/P search, NuGet release or CommonMark conformance change is claimed.

### 2026-11-07 — boundary golden cases and documentation reconciliation

- Added `FenceOpeningIndentationBoundaryMatchesFullParser`: **10** xUnit theory rows for ordinary and quote-local list continuations with 0, 1, 2, 3 and 4 *additional* spaces beyond the item's two-space content indentation. These compare `MarkdownParser.Parse` with `MarkdownIncrementalContainerScanner` for fence-opening classification, `CodeFenceStart`/stream fence offset and exact round-trip source preservation.
- Commit `39dd20eb3127dbae8ddccfb33a5b5590f47e7b9a` contains the test cases. Updated the formerly obsolete known-gap section in [Dual List Ownership](Docs/Dual-List-Ownership/en.md) in commit `0d283fdcace758c85519bce1542059f36cdf0b10`.
- **Fresh CI verification pending.** Previous 6,662 passing tests apply to the state *before* the production and regression changes in this milestone. Do not claim those counts for the new snapshot.
- Remaining: additional list/quote/tab golden cases, validated UTF-8 byte-offset mapping, and a resource-budgeted formatted pager before mdview can consume the streaming scanner.
