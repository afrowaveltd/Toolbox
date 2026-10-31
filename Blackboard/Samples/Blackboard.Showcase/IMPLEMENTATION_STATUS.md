# Blackboard.Showcase — Implementation Status

## 2026-10-24 — first shared interactive test application

- Created independently executable .NET 10 project at `Blackboard/Samples/Blackboard.Showcase`; not a NuGet library. Added common `IBlackboardShowcaseDemo` contract and `BlackboardShowcaseHost` with interactive selection menu and deterministic registered modules.
- Implemented demos for FileBrowser local file selection + bounded readable text preview, JSON-only filtered selection + syntax-highlighted `Spectre.Console.Json.JsonText` render, and directory selection.
- Added `TextFilePreviewReader` with conservative strict UTF-8/UTF-16/UTF-32, no binary/control characters or unbounded memory reads (16 KiB default). Added `JsonFilePreviewReader` with at most 64 KiB complete valid JSON validated via `System.Text.Json` before passing to the Spectre renderer.
- `Spectre.Console.Json 0.57.2` is referenced **only by the Showcase executable**, never by the independent FileBrowser NuGet. The Showcase uses generic filter `*.json`, demonstrating how other applications can supply independent mask/regex filters.
- Added sibling `Blackboard.Showcase.Tests` project and registered both projects in `Toolbox.sln`. Tests cover safe previews, binary/invalid/oversized cases, sample module dispatch and virtual-terminal JSON filtering and rendering.
- Documentation: [README](README.md), [Showcase](Docs/Showcase/en.md), and per-test-project README/status.

**Current verification:** GitHub CI run for the solution-project registration must be checked; do not assert green until an actual completed workflow confirms it. Manual Debian/Windows/SSH terminal smoke testing remains pending. No published NuGet or FTP connector was added.

### Next steps

1. Confirm green GitHub Actions for both new projects and fix compile/test issues if detected.
2. Manually exercise menu, filename selection and JSON rendering on Debian and Windows consoles/SSH.
3. Add new Blackboard widget modules here (do not couple them into FileBrowser).

## 2026-10-24 — verified green CI and JSON preview

- GitHub Actions [40108814777](https://github.com/afrowaveltd/Toolbox/actions/runs/40108814777) **SUCCESS** after fixing the explicit byte-array type for BOM detection in `TextFilePreviewReader` (commit `b86378f2`). `Blackboard.Showcase.Tests`: **18 passed**. `FileBrowser.Tests`: **148 passed**, and the full Toolbox solution built and tested green.
- The virtual Spectre.Console JSON test selected a valid `dictionary.json` with `*.json` filter, confirmed a `.txt` file was absent from selection, and verified `JsonText` output. A second test rejects malformed JSON. Bounded UTF-8/16, binary/control, empty and oversized JSON cases also passed.
- The shared Showcase currently registers three scenarios (safe text preview, syntax-highlighted JSON preview, folder selection) and remains independent of production utilities. No NuGet package was published. Live Debian/Windows/SSH manual testing remains outstanding.

### 2026-10-24 — 20 Showcase tests green

- Added direct virtual-terminal tests of the actual **text-preview demo** (readable Czech text chosen through FileBrowser) and **directory-selection demo** (choose current folder without modification), commit `7320d00b`.
- GitHub Actions [40109040501](https://github.com/afrowaveltd/Toolbox/actions/runs/40109040501) **SUCCESS** with **20 Showcase tests**, **148 FileBrowser tests**, and the green full Toolbox build and all existing test projects. This supersedes the preceding 18-test result.
- Existing focused `FileBrowser.Sample` continues alongside the expandable `Blackboard.Showcase`, with a menu that can accept future UI tools without changing their NuGet dependencies.

## 2026-10-25 — clear Showcase terminal at launch

- `BlackboardShowcaseHost.RunAsync` calls the Spectre.Console `IAnsiConsole.Clear()` extension once after its interactive guard and before the first menu, hiding command history above the TUI.
- Returning from a demo does not clear its output. A virtual-terminal regression asserts the clear ANSI sequence occurs at the very beginning exactly once.
- This changes only the Showcase host, **not** the separately published FileBrowser component.

### Startup clear verified in CI (2026-10-25)

- [GitHub Actions 40230127035](https://github.com/afrowaveltd/Toolbox/actions/runs/40230127035) **SUCCESS** for implementation commit `f46b80d9`, including the ANSI one-time clear assertion in the Showcase menu test and all **20 Showcase tests**. Full solution tests and final build succeeded.

## 2026-10-29 — Markdown gallery module

- Registered the standalone `MarkdownThemeDemo` as fourth Showcase menu item; added a **direct project reference only in Showcase** to `Blackboard.SpectreConsole.Markdown`.
- Interactive Classic/Ocean/Forest/Monochrome presets plus Compare all; all render one pre-parsed immutable ExchangeHouse document. No filesystem/network operations or changes to other demos/Configurator.
- Added `MarkdownThemeDemoTests` (cancel, default, switching, compare, host integration, cancellation token), English README and `Docs/Markdown-Themes/en.md`.
- CI status: **pending**, not yet verified. The next action is to confirm full Linux build/test and targeted Windows Showcase CI; real terminal acceptance still requires manual checking.

### Cross-platform CI verification — 2026-10-29

- [GitHub Actions #40876131365](https://github.com/afrowaveltd/Toolbox/actions/runs/40876131365) **SUCCESS** on implementation commit `f711e66538941118592ff6d1221bf5b7f0aff57d`.
- Linux: full Toolbox solution restore, build, and test **passed** (includes the new Markdown theme-gallery unit/virtual-terminal tests).
- Windows: FileBrowser, UnderstandMe Configurator, Blackboard Showcase (including the new gallery), and UnderstandMe localization concurrency suites **passed**.
- Manual interactive visual confirmation on an actual Windows/Debian terminal is still outstanding; automatic tests do not certify terminal font/theme appearance.

## 2026-10-31 — nested list sample

- Expanded the embedded Markdown showcase sample with ordered steps and a nested unordered item to demonstrate list indentation in all four themes; no new prompt/menu. CI pending.

### Cross-platform CI verified — 2026-10-31

- [GitHub Actions #41244480220](https://github.com/afrowaveltd/Toolbox/actions/runs/41244480220) **SUCCESS** for implementation up through commit `415065a4c272838f3bc0e4ed94ab8ea90003a295`.
- Linux: full Toolbox solution **build and tests passed**; final build reported **0 warnings, 0 errors**.
- Windows: **ExchangeHouse.Markdown.Tests 41/41**, **Blackboard.SpectreConsole.Markdown.Tests 13/13**, **Blackboard.Showcase.Tests 26/26**; FileBrowser 160/160, Configurator 44/44, and targeted localization tests 34/34 passed.
- Intermediate runs on parser/renderer commits failed at **test compilation** due to literal `\\`` C# escape sequences mistakenly introduced when authoring Markdown code examples. Fixed in commits `e7b858aa` and `415065a4`; the final CI is green. Existing production source was not weakened to make tests pass.
- Manual real-terminal testing of newly added nested-list visuals in Debian/Windows remains unreported. The current model does not claim CommonMark-compatible loose/multiline lists.

## 2026-10-31 — multiline list sample

- Expanded built-in theme-gallery document with indented continuation paragraphs beneath ordered and nested list items. No changes to FileBrowser or interactive menus. CI pending.

### Verified CI — 2026-10-31 list continuation increment

- [GitHub Actions #41248289922](https://github.com/afrowaveltd/Toolbox/actions/runs/41248289922) **SUCCESS**, source through commit `35efe934031a99a5407550f6624d96656b9c03a3`: Linux full solution build/tests passed; Windows ExchangeHouse Markdown **46/46**, Blackboard Markdown **16/16**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**.
- Markdown local link job **PASS**: 269 Markdown files, 1,374 local file targets checked with none missing. Source-aware continuation behavior also tested on Windows; manual Debian and real-terminal visual checks remain separate.
- Initial renderer integration CI failed **one incorrect test expectation** (custom indent `....` erroneously checked as six literal spaces). Fixed the assertion in `35efe934`, retaining custom-indent coverage. The failed earlier CI runs are not counted as successes.
- This milestone handles only **explicitly indented** continuation lines. Loose lists, lazy continuation and nested CommonMark block containers remain unsupported. No public NuGet release was made.

## 2026-10-31 — blank-separated list demo

- Expanded the shared Markdown sample with a loose-list paragraph case. Added renderer-specific tests; FileBrowser samples and interaction flow unchanged. CI pending.

### Verified CI — 2026-10-31 loose-list paragraph milestone

- [GitHub Actions #41249127101](https://github.com/afrowaveltd/Toolbox/actions/runs/41249127101) **SUCCESS** on source/docs commit `df4da1b285d280cbe1d17dee0fded388f178d0b8`: full Linux solution build/tests and Windows UI/Markdown regressions passed.
- Windows focused test results: ExchangeHouse Markdown **55/55**, Blackboard Markdown **19/19**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**. Earlier linked documentation job verified **271 Markdown files and 1,388 local links** with no missing local targets; the final run's link job also passed.
- Added source-preserving `IsLoose`, blank separator/content collections and source owner metadata for explicitly indented blank-separated list paragraphs. Safe terminal rendering aligns subsequent paragraphs beneath the original marker; Showcase updated. No source rewrite, full CommonMark grammar or NuGet publication.
- Interim #41249032554 failed one incorrect custom-indent **test expectation**, corrected in `2b34cb46`, without production renderer change. Later CI is green. Manual acceptance of new visuals in real Debian/Windows terminals remains pending.

## 2026-10-31 — lazy list paragraph gallery example

- The existing single parsed Markdown sample now includes a lazy unindented continuation after an explicitly indented and blank-separated paragraph.
- Reuses existing theme presets and the renderer; no new Showcase feature or dependency. **CI pending**.

### Final green lazy-paragraph checkpoint — 2026-10-31

- [GitHub Actions #41250061417](https://github.com/afrowaveltd/Toolbox/actions/runs/41250061417) **SUCCESS** for commit `f6b7fc555c76d964ec12a27f86accc1beab3e9f8`: Linux full solution build and tests passed, Windows targeted UI/Markdown suites passed and local Markdown documentation-link check passed.
- Windows counts: **ExchangeHouse Markdown 60/60**, **Blackboard Markdown 21/21**, **Showcase 26/26**, **FileBrowser 160/160**, **Configurator 44/44**, **UnderstandMe localization 34/34**.
- Documentation audit: **273 Markdown files, 1,399 local targets checked**, zero missing links. Cross-family link typo from the first commit was fixed in `2c8c3fa`; the final gallery sample now uses an actually unindented lazy line.
- No public Markdown NuGet publication, no full CommonMark or real-terminal Debian acceptance is implied by CI.

## 2026-10-31 — code and quotation gallery sample

Existing Markdown-theme sample now includes a quote and a declared-language `csharp` code fence (rendered as literal text by the base renderer). The optional TextMate adapter is a separate package and not yet connected to the Showcase sample. CI pending.

## 2026-10-31 — TextMate code sample integration

- Showcase now references the **optional** Markdown.TextMate project and reuses one grammar registry for three colored themes; monochrome deliberately disables it.
- The gallery sample includes a nested quote and `bash` fence under a list item plus a top-level `csharp` block.
- The base Blackboard Markdown and ExchangeHouse projects retain zero TextMate dependency. CI pending at this commit.

### Cross-platform green verification — 2026-10-31

- [GitHub Actions #41251065292](https://github.com/afrowaveltd/Toolbox/actions/runs/41251065292) **SUCCESS** for source commit `5b80ad1e3f9cde50ae793da53b3b81c30d727d08`. Linux complete solution restore/build/tests passed, Windows targeted UI and grammar tests passed, and Markdown link validation passed.
- Windows: **ExchangeHouse Markdown 66/66**, **Blackboard Markdown 24/24**, **TextMate adapter 6/6**, **Showcase 26/26**, **FileBrowser 160/160**, **Configurator 44/44**, **UnderstandMe localization 34/34**.
- The documentation link audit examined **280 Markdown files and 1,423 local targets** with no missing targets. A later docs-only commit clarified a nested-fence illustration; final docs CI is tracked separately.
- Real TextMate C# tokenization was exercised under Linux and Windows CI, including platform-specific Oniguruma dependencies. This does **not** establish manual SSH/tmux or Debian terminal acceptance, complete CommonMark support, NuGet publication, or a completed third-party transitive-license review.

## 2026-11-01 — quoted list sample

- Theme-gallery source adds numbered/nested quoted-list content across separate quote levels, without new dependencies. CI pending.

### Verified cross-platform quoted-list milestone — 2026-11-01

- [GitHub Actions #41251673070](https://github.com/afrowaveltd/Toolbox/actions/runs/41251673070) **SUCCESS** for feature commit `aa3ec1246fc3125f17d1a506e415cc3ad7db418b`: Linux full solution build/test, Windows targeted UI tests and documentation-link job passed.
- Windows results: ExchangeHouse Markdown **71/71**, Blackboard Markdown **26/26**, optional TextMate **6/6**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**.
- The subsequent documentation commit introduced one incorrect cross-family relative link, fixed in `1e89cf7532c03737a0599d51c26b54cd57bb345a`. Link verification on the fix run passed; final complete CI for the documentation state is tracked independently.
- No new NuGet publication, full CommonMark conformance, or real-terminal acceptance is claimed. This feature intentionally does not tokenize fenced code inside explicitly quote-prefixed containers.
