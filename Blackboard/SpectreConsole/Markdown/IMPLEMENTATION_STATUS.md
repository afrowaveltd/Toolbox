# Blackboard.SpectreConsole.Markdown — Implementation status

## 2026-10-29 — independent renderer foundation

- Created separate .NET 10 `Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown` library, referencing only `ExchangeHouse.Markdown` and Spectre.Console 0.57.2.
- Added `MarkdownSpectreTheme` with six heading styles, paragraph/inline/code/link styles and options for bullets, indentation, link destinations and code fences.
- Added `MarkdownSpectreRenderer` for Markdown string and already-parsed documents. No network/file operations, screen clearing or link-opening; terminal control characters sanitized.
- Created separate virtual-terminal test project and documentation. Both projects added to `Toolbox.sln`.
- No full CommonMark, no Configurator migration, no package publication. CI pending until workflow verified.

### Next
Verify CI, manual terminal smoke; then build a Showcase demo and integrate the Configurator in small separate commits.

### Verified CI — 2026-10-29

- [GitHub Actions #40851606376](https://github.com/afrowaveltd/Toolbox/actions/runs/40851606376) **SUCCESS** for commit `6f917928538f3e8342e9cd56f1b84be2feb8a104`: full solution restore, build and test passed, including Markdown renderer virtual-terminal tests.
- The first attempt used unsupported mutable `Spectre.Console.Text.Append`; the next build revealed one overly broad ANSI test assertion. Both were corrected in commits `6c754fa0` and `6f917928` respectively, then confirmed green. No manual Debian/Windows terminal validation or NuGet publication claimed.

## 2026-10-29 — first production consumer integration

- `UnderstandMe/Tools/Configurator` now references this independent renderer and uses its `MarkdownSpectreRenderer` via a small theme adapter instead of duplicating a Markdown parser/display loop.
- Existing embedded offline Help and navigation semantics are preserved. This library still has no UnderstandMe dependency; the dependency is one-way from consumer to Blackboard to ExchangeHouse.
- Windows and Linux CI verification pending for the consumer integration commit. Advanced CommonMark and live clickable link navigation are not implemented.

### Verified GitHub CI — 2026-10-29

- [GitHub Actions #40880298677](https://github.com/afrowaveltd/Toolbox/actions/runs/40880298677) **SUCCESS** for consumer integration commit `18d0281ee232bdd8080f00923f91f5d8b34adb1b`.
- Linux whole-solution restore/build/test **PASS**. Windows FileBrowser, UnderstandMe Configurator (including shared Markdown help tests), Blackboard Showcase and UnderstandMe localization concurrency suites **PASS**.
- No release/published Markdown NuGet was created; Configurator references the in-repo library project. Windows real-terminal Markdown Help visuals and Debian manual acceptance remain to be checked by a person; virtual terminal tests prove the covered behavior only.

## 2026-10-29 — image alternative rendering

- Added safe text-only image rendering, configurable image theme and optional source text, without file/network access. Added renderer tests; Linux and Windows CI pending.

### Verified cross-platform CI — 2026-10-29

- Parser implementation commit `fb7a8108`: [GitHub Actions #40881801829](https://github.com/afrowaveltd/Toolbox/actions/runs/40881801829) **SUCCESS** — full Linux solution build/tests and existing Windows regression job.
- Image presentation implementation commit `7696fef8`: [GitHub Actions #40882016971](https://github.com/afrowaveltd/Toolbox/actions/runs/40882016971) **SUCCESS** — full Linux solution build/tests and Windows FileBrowser, Configurator, Showcase, localization **plus newly added focused ExchangeHouse Markdown and Blackboard Markdown tests**.
- No direct manual Debian or Windows terminal test of this new image fallback has been reported. Full CommonMark, reference-style links, actual image drawing and translated document link routing are still not implemented.

## 2026-10-31 — numbered and nested list presentation

- The renderer consumes ExchangeHouse `ListMarker` depth and original ordered numbers; theme exposes `ListIndent` and `PreserveUnorderedMarkers`, retaining `ListBullet` behavior for unordered items.
- Added tests for real numbered markers, nested mixed lists, custom indentation/original bullet markers and code isolation. Showcase sample includes a nested ordered list. No source changes or parsing performed in Blackboard.
- Full CI including Windows Markdown and Showcase tests pending for this commit.

### Cross-platform CI verified — 2026-10-31

- [GitHub Actions #41244480220](https://github.com/afrowaveltd/Toolbox/actions/runs/41244480220) **SUCCESS** for implementation up through commit `415065a4c272838f3bc0e4ed94ab8ea90003a295`.
- Linux: full Toolbox solution **build and tests passed**; final build reported **0 warnings, 0 errors**.
- Windows: **ExchangeHouse.Markdown.Tests 41/41**, **Blackboard.SpectreConsole.Markdown.Tests 13/13**, **Blackboard.Showcase.Tests 26/26**; FileBrowser 160/160, Configurator 44/44, and targeted localization tests 34/34 passed.
- Intermediate runs on parser/renderer commits failed at **test compilation** due to literal `\\`` C# escape sequences mistakenly introduced when authoring Markdown code examples. Fixed in commits `e7b858aa` and `415065a4`; the final CI is green. Existing production source was not weakened to make tests pass.
- Manual real-terminal testing of newly added nested-list visuals in Debian/Windows remains unreported. The current model does not claim CommonMark-compatible loose/multiline lists.

## 2026-10-31 — list continuation display

- Display paragraphs owned by list items aligned beneath the themed visible marker, including numbered/nested lists and custom bullets. No new parser implementation in Blackboard.
- Added three virtual-terminal regressions, expanded theme gallery sample and [topic documentation](Docs/List-Continuation-Presentation/en.md). CI pending for this increment.

### Verified CI — 2026-10-31 list continuation increment

- [GitHub Actions #41248289922](https://github.com/afrowaveltd/Toolbox/actions/runs/41248289922) **SUCCESS**, source through commit `35efe934031a99a5407550f6624d96656b9c03a3`: Linux full solution build/tests passed; Windows ExchangeHouse Markdown **46/46**, Blackboard Markdown **16/16**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**.
- Markdown local link job **PASS**: 269 Markdown files, 1,374 local file targets checked with none missing. Source-aware continuation behavior also tested on Windows; manual Debian and real-terminal visual checks remain separate.
- Initial renderer integration CI failed **one incorrect test expectation** (custom indent `....` erroneously checked as six literal spaces). Fixed the assertion in `35efe934`, retaining custom-indent coverage. The failed earlier CI runs are not counted as successes.
- This milestone handles only **explicitly indented** continuation lines. Loose lists, lazy continuation and nested CommonMark block containers remain unsupported. No public NuGet release was made.

## 2026-10-31 — blank-separated list paragraph presentation

- Added regression coverage for blank line display, nested numbered list alignment under custom theme, and unrelated paragraphs. No runtime changes needed in the reusable renderer; it already reads continuation metadata and renders blank blocks correctly.
- Docs and Showcase sample updated. CI pending for this commit.

### Verified CI — 2026-10-31 loose-list paragraph milestone

- [GitHub Actions #41249127101](https://github.com/afrowaveltd/Toolbox/actions/runs/41249127101) **SUCCESS** on source/docs commit `df4da1b285d280cbe1d17dee0fded388f178d0b8`: full Linux solution build/tests and Windows UI/Markdown regressions passed.
- Windows focused test results: ExchangeHouse Markdown **55/55**, Blackboard Markdown **19/19**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**. Earlier linked documentation job verified **271 Markdown files and 1,388 local links** with no missing local targets; the final run's link job also passed.
- Added source-preserving `IsLoose`, blank separator/content collections and source owner metadata for explicitly indented blank-separated list paragraphs. Safe terminal rendering aligns subsequent paragraphs beneath the original marker; Showcase updated. No source rewrite, full CommonMark grammar or NuGet publication.
- Interim #41249032554 failed one incorrect custom-indent **test expectation**, corrected in `2b34cb46`, without production renderer change. Later CI is green. Manual acceptance of new visuals in real Debian/Windows terminals remains pending.

## 2026-10-31 — rendering lazy paragraph continuations

- Reused existing list-owner alignment for newly recognized unindented lazy paragraph lines; no renderer source changes or new runtime dependencies.
- Added virtual-terminal checks for bullet and custom nested-numbered alignment; Showcase gallery includes a lazy line.
- [Presentation details](Docs/Lazy-List-Presentation/en.md). **CI pending** for new parser and renderer tests; no NuGet release.

### Final green lazy-paragraph checkpoint — 2026-10-31

- [GitHub Actions #41250061417](https://github.com/afrowaveltd/Toolbox/actions/runs/41250061417) **SUCCESS** for commit `f6b7fc555c76d964ec12a27f86accc1beab3e9f8`: Linux full solution build and tests passed, Windows targeted UI/Markdown suites passed and local Markdown documentation-link check passed.
- Windows counts: **ExchangeHouse Markdown 60/60**, **Blackboard Markdown 21/21**, **Showcase 26/26**, **FileBrowser 160/160**, **Configurator 44/44**, **UnderstandMe localization 34/34**.
- Documentation audit: **273 Markdown files, 1,399 local targets checked**, zero missing links. Cross-family link typo from the first commit was fixed in `2c8c3fa`; the final gallery sample now uses an actually unindented lazy line.
- No public Markdown NuGet publication, no full CommonMark or real-terminal Debian acceptance is implied by CI.

## 2026-10-31 — quoted code containers and optional highlighter contract

- Render quote nesting and fenced code as attached list content; preserve literal code and optionally display declared language names.
- Added session-based `IMarkdownSyntaxHighlighter`/`IMarkdownHighlightSession` contract and defensive source-span validation; providers are optional, never supplied with executable code.
- A separate TextMate adapter lives in sibling `Markdown.TextMate`, not inside this base renderer.
- [Code presentation](Docs/Code-Quote-Presentation/en.md). CI pending for the finalized tests.

### Cross-platform green verification — 2026-10-31

- [GitHub Actions #41251065292](https://github.com/afrowaveltd/Toolbox/actions/runs/41251065292) **SUCCESS** for source commit `5b80ad1e3f9cde50ae793da53b3b81c30d727d08`. Linux complete solution restore/build/tests passed, Windows targeted UI and grammar tests passed, and Markdown link validation passed.
- Windows: **ExchangeHouse Markdown 66/66**, **Blackboard Markdown 24/24**, **TextMate adapter 6/6**, **Showcase 26/26**, **FileBrowser 160/160**, **Configurator 44/44**, **UnderstandMe localization 34/34**.
- The documentation link audit examined **280 Markdown files and 1,423 local targets** with no missing targets. A later docs-only commit clarified a nested-fence illustration; final docs CI is tracked separately.
- Real TextMate C# tokenization was exercised under Linux and Windows CI, including platform-specific Oniguruma dependencies. This does **not** establish manual SSH/tmux or Debian terminal acceptance, complete CommonMark support, NuGet publication, or a completed third-party transitive-license review.

## 2026-11-01 — quote + list combinations

- Render explicit quote prefixes before list marker and owner-aligned paragraph continuation while preserving original ordered numbers.
- Two virtual-console regressions for mixed nested lists and quoted paragraph alignment with custom quote/list theme. [Presentation guide](Docs/Quoted-List-Presentation/en.md). CI pending, not published.

### Verified cross-platform quoted-list milestone — 2026-11-01

- [GitHub Actions #41251673070](https://github.com/afrowaveltd/Toolbox/actions/runs/41251673070) **SUCCESS** for feature commit `aa3ec1246fc3125f17d1a506e415cc3ad7db418b`: Linux full solution build/test, Windows targeted UI tests and documentation-link job passed.
- Windows results: ExchangeHouse Markdown **71/71**, Blackboard Markdown **26/26**, optional TextMate **6/6**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**.
- The subsequent documentation commit introduced one incorrect cross-family relative link, fixed in `1e89cf7532c03737a0599d51c26b54cd57bb345a`. Link verification on the fix run passed; final complete CI for the documentation state is tracked independently.
- No new NuGet publication, full CommonMark conformance, or real-terminal acceptance is claimed. This feature intentionally does not tokenize fenced code inside explicitly quote-prefixed containers.

## 2026-11-01 — quote-aware syntax rendering

- Display quoted code delimiters/language/body with independent list indentation and quote theme. Optional syntax-highlighter sessions close when quoted code is interrupted.
- Three virtual-terminal regression tests; [renderer documentation](Docs/Quoted-Code-Presentation/en.md).
- [Implementation CI #41254427784](https://github.com/afrowaveltd/Toolbox/actions/runs/41254427784) **SUCCESS**. No NuGet release.

### Full cross-platform CI checkpoint — 2026-11-01

- [GitHub Actions #41254717060](https://github.com/afrowaveltd/Toolbox/actions/runs/41254717060) **SUCCESS** for final code/English-docs fix commit `8f25d8e2a72df6ffe915362ef11d64dbd6c89d6a`: Linux complete solution build/tests, Windows targeted UI/Markdown tests and repository local documentation-link validation passed.
- Windows focused tests: ExchangeHouse Markdown **75/75**, Blackboard Markdown **29/29**, Markdown.TextMate **6/6**, Blackboard Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, UnderstandMe localization **34/34**.
- Markdown local-link checker verified **284 files / 1,452 local targets**, zero broken links. An earlier documentation-only CI failed one wrong Showcase-to-renderer relative path, fixed in `8f25d8e`; the final checkpoint above is green.
- No new NuGet release or manual real-terminal Windows/Debian acceptance claimed.

## 2026-11-01 — quoted loose-list display

- No renderer production changes needed: the existing `BlockQuote`, quote depth and owner-aware paragraph logic display blank quote-prefix lines and separately indented continuation correctly.
- Added two virtual-terminal regressions for numbered quoted lists and nested-child ancestor resumption. [Guide](Docs/Quoted-Loose-Presentation/en.md). CI pending.

### Verified cross-platform milestone — 2026-11-01

- [GitHub Actions #41256065289](https://github.com/afrowaveltd/Toolbox/actions/runs/41256065289) **SUCCESS** on source/documentation commit `882f15862d89214e8cc1204d8b5b7baaedd6d18a`: Linux complete solution build and tests, Windows UI/Markdown targeted tests and local Markdown documentation links all passed.
- Windows tests: ExchangeHouse Markdown **82/82**, Blackboard Markdown **31/31**, TextMate adapter **6/6**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**.
- Markdown checker processed **286 files / 1,470 local targets**, with zero missing local links.
- The test-preserving increment has not been published to NuGet and has not undergone manual Debian/Windows/SSH visual acceptance; full CommonMark nesting remains outside this milestone.

## 2026-11-01 — quote container indent composition

- Separate outer list content indent, theme quote prefix and quoted-list marker/paragraph indent. Optional language-label rendering now also receives the outer indent; no double indent for directly owned plain quoted text.
- Three new virtual-terminal tests, plus [renderer guide](Docs/Quoted-List-Containers/en.md).
- [CI #41257477307](https://github.com/afrowaveltd/Toolbox/actions/runs/41257477307) **SUCCESS**, renderer **34/34**; TextMate **6/6**.

## 2026-11-02 — future standalone TUI viewer design, not implemented

- A future standalone viewer would wrap the Markdown renderer with document-source adapters (Local, HTTP(S), GitHub README), navigation for relative links, themes and readable errors; it must not duplicate Markdown parsing.
- GitHub provider should use `/repos/{owner}/{repo}/readme` rather than probing guessed branch/file paths. Maintain independent cache, credentials, and transport concerns outside the renderer.
- Design deferred until the current parser is judged ready. [Proposed architecture](Docs/Standalone-Markdown-Viewer-Proposal/en.md). No new code, tests or release.

## 2026-11-02 — streaming/pager research noted (no implementation)

- The future viewer proposal now covers case-insensitive GitHub owner/repo arguments (one API request), and a separate block-event parser/terminal pager research track using `IAsyncEnumerable` with bounded lookahead, optional disk spooling and full-document fidelity.
- Current parser still materializes the entire document. No 1 GB runtime capacity is claimed. [Design details](Docs/Standalone-Markdown-Viewer-Proposal/en.md).

## 2026-11-03 — offline-first cache design checkpoint

- Added [persistent document cache policy](Docs/Document-Cache-Policy/en.md): retain valid remote README content across restarts; use ETag/Last-Modified conditional validation; explicitly distinguish stale/offline content; atomic writes and corruption-safe fallback; safe cache roots, budgets, opt-in cleanup and tests.
- Updated the [standalone viewer proposal](Docs/Standalone-Markdown-Viewer-Proposal/en.md) to link the cache acceptance contract.
- **Documentation only:** no source provider, cache implementation, standalone `mdview`, paging engine or NuGet publication. Existing full-document local Showcase behavior is unchanged. Automated tests for this contract are not yet implemented.

## 2026-11-03 — separate verified cache foundation introduced

- New independent [Markdown.Cache library](../Markdown.Cache/README.md) and [tests](../Markdown.Cache.Tests/README.md) contain the first disk-store implementation. Parser/renderer itself is unchanged and has no cache dependency.
- The [viewer cache contract](Docs/Document-Cache-Policy/en.md) remains the wider target; current store does not implement conditional network refresh, cleanup, per-user path selection or paging.
- New cache tests have not yet passed CI or been run locally; .NET SDK unavailable in execution environment. See [cache status](../Markdown.Cache/IMPLEMENTATION_STATUS.md).

## 2026-11-03 — independent cache hardening checkpoint

- Separate [Markdown.Cache](../Markdown.Cache/IMPLEMENTATION_STATUS.md) now includes cross-process writer admission, direct symlink checks and per-user cache location selection. No change to Markdown parsing/rendering.
- Ten cache tests authored, **not yet executed/CI verified**. Viewer network adapters and offline UI remain future implementation.

## 2026-11-03 — conditional HTTP cache provider

- [Markdown.Cache](../Markdown.Cache/README.md) adds optional HTTP conditional-fetch transport, ETag/Last-Modified persistence and distinguishable fresh/unchanged/stale/error results. The renderer and ExchangeHouse parser remain independent and unchanged.
- Eight HTTP tests use injected in-memory handlers. No viewer command, GitHub-specific README resolver or terminal UI integrated. Build/CI still unverified; see [cache checkpoint](../Markdown.Cache/IMPLEMENTATION_STATUS.md).

## 2026-11-03 — GitHub README source provider foundation

- A separate [Markdown.Cache GitHub README provider](../Markdown.Cache/Docs/GitHub-Readme/en.md) is now implemented for public repository default-branch READMEs. It reuses safe disk-backed conditional HTTP cache and does not modify the renderer or parser.
- Tests are authored with fake HTTP handlers; .NET build/CI has not been confirmed. No standalone `mdview` executable or GitHub file/link navigation exists yet.
