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
