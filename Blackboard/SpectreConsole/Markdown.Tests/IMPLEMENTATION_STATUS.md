# Blackboard.SpectreConsole.Markdown.Tests — Implementation status

## 2026-10-29 — first regression suite

- Added dedicated xUnit + Spectre.Console.Testing project and registered in Toolbox.sln.
- Covered basic formatted rendering, custom theme options, raw markup isolation, control characters, link targets, unchanged document and argument guards.
- CI verification pending. Real terminal smoke has not been performed.

### Verified CI — 2026-10-29

- [GitHub Actions #40851606376](https://github.com/afrowaveltd/Toolbox/actions/runs/40851606376) **SUCCESS** for commit `6f917928538f3e8342e9cd56f1b84be2feb8a104`: full solution restore, build and test passed, including Markdown renderer virtual-terminal tests.
- The first attempt used unsupported mutable `Spectre.Console.Text.Append`; the next build revealed one overly broad ANSI test assertion. Both were corrected in commits `6c754fa0` and `6f917928` respectively, then confirmed green. No manual Debian/Windows terminal validation or NuGet publication claimed.

## 2026-10-29 — image rendering tests

- Added virtual terminal cases for image alt text, independent display of original image paths, style overrides and raw markup safety; CI pending.

### Verified cross-platform CI — 2026-10-29

- Parser implementation commit `fb7a8108`: [GitHub Actions #40881801829](https://github.com/afrowaveltd/Toolbox/actions/runs/40881801829) **SUCCESS** — full Linux solution build/tests and existing Windows regression job.
- Image presentation implementation commit `7696fef8`: [GitHub Actions #40882016971](https://github.com/afrowaveltd/Toolbox/actions/runs/40882016971) **SUCCESS** — full Linux solution build/tests and Windows FileBrowser, Configurator, Showcase, localization **plus newly added focused ExchangeHouse Markdown and Blackboard Markdown tests**.
- No direct manual Debian or Windows terminal test of this new image fallback has been reported. Full CommonMark, reference-style links, actual image drawing and translated document link routing are still not implemented.

## 2026-10-31 — nested list presentation regressions

- Added assertions for ordered number preservation, nested indentation, marker theme switches, and fenced code isolation. Linux/Windows CI pending.

### Cross-platform CI verified — 2026-10-31

- [GitHub Actions #41244480220](https://github.com/afrowaveltd/Toolbox/actions/runs/41244480220) **SUCCESS** for implementation up through commit `415065a4c272838f3bc0e4ed94ab8ea90003a295`.
- Linux: full Toolbox solution **build and tests passed**; final build reported **0 warnings, 0 errors**.
- Windows: **ExchangeHouse.Markdown.Tests 41/41**, **Blackboard.SpectreConsole.Markdown.Tests 13/13**, **Blackboard.Showcase.Tests 26/26**; FileBrowser 160/160, Configurator 44/44, and targeted localization tests 34/34 passed.
- Intermediate runs on parser/renderer commits failed at **test compilation** due to literal `\\`` C# escape sequences mistakenly introduced when authoring Markdown code examples. Fixed in commits `e7b858aa` and `415065a4`; the final CI is green. Existing production source was not weakened to make tests pass.
- Manual real-terminal testing of newly added nested-list visuals in Debian/Windows remains unreported. The current model does not claim CommonMark-compatible loose/multiline lists.

## 2026-10-31 — list continuation display regressions

- Tests cover alignment under bullets, numbered/nested items with custom theme indent, and explicit paragraph boundaries. CI pending.

### Verified CI — 2026-10-31 list continuation increment

- [GitHub Actions #41248289922](https://github.com/afrowaveltd/Toolbox/actions/runs/41248289922) **SUCCESS**, source through commit `35efe934031a99a5407550f6624d96656b9c03a3`: Linux full solution build/tests passed; Windows ExchangeHouse Markdown **46/46**, Blackboard Markdown **16/16**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localization **34/34**.
- Markdown local link job **PASS**: 269 Markdown files, 1,374 local file targets checked with none missing. Source-aware continuation behavior also tested on Windows; manual Debian and real-terminal visual checks remain separate.
- Initial renderer integration CI failed **one incorrect test expectation** (custom indent `....` erroneously checked as six literal spaces). Fixed the assertion in `35efe934`, retaining custom-indent coverage. The failed earlier CI runs are not counted as successes.
- This milestone handles only **explicitly indented** continuation lines. Loose lists, lazy continuation and nested CommonMark block containers remain unsupported. No public NuGet release was made.

## 2026-10-31 — loose-list display regressions

- Added three virtual-terminal tests for blank-separated list text and theme indentation. CI pending.

### Cross-platform verification — 2026-10-31

- [GitHub Actions #41249127101](https://github.com/afrowaveltd/Toolbox/actions/runs/41249127101) **SUCCESS** for source/docs commit `df4da1b285d280cbe1d17dee0fded388f178d0b8`. Linux full solution built/tested; Windows ExchangeHouse Markdown **55/55**, Blackboard Markdown **19/19**, Showcase **26/26**, FileBrowser **160/160**, Configurator **44/44**, localized tests **34/34**.
- Blank-separated explicitly indented list paragraphs now have source-preserving ownership and terminal alignment. Interim custom-indent test assertion was fixed in `2b34cb46`, with no production rendering change.
- Markdown docs link job passed. No public package release, full CommonMark claim or manual Debian visual acceptance made.

## 2026-10-31 — lazy paragraph alignment tests

- Two new virtual-console regressions: bullet-aligned lazy continuation and nested numbered-list custom-theme alignment.
- **CI pending**; existing 19/19 pass was recorded before these tests.

### Final green lazy-paragraph checkpoint — 2026-10-31

- [GitHub Actions #41250061417](https://github.com/afrowaveltd/Toolbox/actions/runs/41250061417) **SUCCESS** for commit `f6b7fc555c76d964ec12a27f86accc1beab3e9f8`: Linux full solution build and tests passed, Windows targeted UI/Markdown suites passed and local Markdown documentation-link check passed.
- Windows counts: **ExchangeHouse Markdown 60/60**, **Blackboard Markdown 21/21**, **Showcase 26/26**, **FileBrowser 160/160**, **Configurator 44/44**, **UnderstandMe localization 34/34**.
- Documentation audit: **273 Markdown files, 1,399 local targets checked**, zero missing links. Cross-family link typo from the first commit was fixed in `2c8c3fa`; the final gallery sample now uses an actually unindented lazy line.
- No public Markdown NuGet publication, no full CommonMark or real-terminal Debian acceptance is implied by CI.
