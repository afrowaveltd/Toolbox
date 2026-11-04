# mdview — implementation status

## 2026-11-03 — first CLI executable and command grammar

- Created `Blackboard/Tools/MdView` as a standalone `net10.0` executable project (`AssemblyName=mdview`), plus `MdView.Tests`, registered in `Toolbox.sln`.
- Added pure `MdViewCommand.Parse` for interactive/no-args, explicit local path, direct URL, `github:owner/repo`, `git OWNER REPO` and `git OWNER/REPO` forms; verbs ignore case without changing path case.
- Local file handling reuses FileBrowser's strict full-document decoding and validates Markdown file extensions. Remote HTTP/GitHub use previously implemented cache-validated source adapters with explicit stale state. All output delegates to existing MarkdownSpectreRenderer, not a new renderer.
- A simple numbered text menu (Local/URL/GitHub) is implemented, **not** a Spectre FileBrowser or pageable TUI. No browser/file link navigation, styling controls or independent Markdown streaming.
- Added CLI grammar xUnit tests. **Build and test execution not verified** in this session; .NET SDK unavailable in current container. Local UI acceptance on Windows/Debian/SSH is pending.
- Run: `dotnet run --project Blackboard/Tools/MdView -- git afrowaveltd toolbox`.
- Next: verify solution CI and correct any build/test issues; add CLI integration tests, full no-args FileBrowser, and eventually disk-indexed native pager. Do not claim large-document bounded-RAM rendering or NuGet release yet.

## 2026-11-03 — Windows full-suite compilation feedback

- User's Windows .NET 11 preview SDK full solution run reported CS0103 in `MdView/Program.cs`: `TextFilePreviewReader` and `TextPreviewKind` were not in FileBrowser; their implementation lives in `Blackboard.Showcase`.
- Fixed `MdView.csproj` by linking the existing tested `Blackboard.Showcase/TextFilePreviewReader.cs` source into the new CLI compilation; corrected the namespace import to `Afrowave.Toolbox.Blackboard.Showcase`. This avoids a runtime dependency on the Showcase executable and preserves existing strict Unicode/BOM validation. It is a short-term source-sharing bridge: next extract the reader into a UI-neutral shared project with both consumers referencing it.
- User-provided log also confirms **Markdown.Cache.Tests 32/32 passed** on Windows; these tests predate the CLI correction. Re-run `dotnet test Toolbox.sln` to verify the corrected CLI; not yet confirmed locally or in CI.

## 2026-11-03 — Windows verification passed

- User's follow-up full `dotnet test Toolbox.sln` output confirms `MdView` built as `mdview.dll` and `MdView.Tests` passed **10/10**, with no failures or skips. The earlier missing `TextFilePreviewReader`/ `TextPreviewKind` CS0103 errors are resolved by the temporary shared-source link.
- This is a Windows local-test confirmation, not a verified GitHub Actions build or a manual interactive CLI acceptance test. The linked Showcase reader should next be extracted to an independent library rather than retained as cross-project source inclusion.

## 2026-11-03 — reader extraction follow-up

- Removed the temporary linked Showcase source from `MdView.csproj`. `mdview` now references [TextFiles](../../TextFiles/README.md) as a normal library project and uses `Afrowave.Toolbox.Blackboard.TextFiles` namespace.
- Local-file decoding behavior is intentionally unchanged. The 6,438/6,438 user-confirmed Windows test result predates this extraction; full solution tests are pending for the new dependency graph.

## 2026-11-03 — full Windows extraction regression passed

- Windows solution test report after the independent TextFiles migration shows `mdview.dll` built and **MdView.Tests 10/10 passed**, alongside extracted TextFiles.Tests 5/5 and Showcase.Tests 39/39; total solution result **6,443/6,443**.
- Interactive CLI navigation and large-file paging still unverified/unimplemented, respectively.

## 2026-11-03 — interactive Spectre source picker and FileBrowser

- Replaced the old no-argument `Console.ReadLine` numbered menu with `MdViewInteractiveSourcePicker` using injected `IAnsiConsole`, `SelectionPrompt`, `TextPrompt` and our existing `SpectreFileBrowser` over `FileBrowserLocalSource`.
- Local choice begins at process working directory, filters `*.md;*.markdown`, supports existing folder navigation/tree/recursive search and independently validates the selected extension because filter editing is allowed. Escape in browser returns to source menu; Escape in top menu exits. The menu clears on return and reports invalid input. Non-interactive/no-argument mode fails with a readable message rather than blocking.
- URL choice accepts public HTTPS addresses without query/userinfo/fragment, GitHub choice accepts `owner/repository` via the unchanged argument grammar. Selection does not download; actual local decode, HTTP caching and renderer behavior remain unchanged. Explicit command-line invocation bypasses the menu as before.
- Added `Spectre.Console.Testing` dependency to MdView.Tests and **11 new virtual-terminal test cases** (7 Facts + 4 Theory rows): exit, actual local selection/no-write, browser back, GitHub input, HTTPS input, URL validation, noninteractive guard and extension checks. The previously user-confirmed Windows 6,443/6,443 solution test result applies **before** this code; **new tests have not been run** due to absent local .NET SDK.
- Docs: [Interactive source picker](Docs/Interactive-Source-Picker/en.md), updated [CLI](Docs/CLI/en.md) and root README.
- Next: run `dotnet test Toolbox.sln` (expect MdView.Tests **21** and total **6,454** tests if no other changes), correct any failures, and perform manual Windows Terminal/Debian/SSH interactive smoke checks. Native paged Markdown rendering still absent.

## 2026-11-04 — syntax coloring for installed TextMate grammars

- Added `MdViewHighlighting.CreateTheme()` and a project reference from mdview to `Blackboard.SpectreConsole.Markdown.TextMate`. All local, HTTP(S) and public GitHub README render paths now use the shared syntax-enabled theme, fixing the previous default plain code output for JavaScript.
- Optional native TextMate initialisation errors degrade to the normal safe Markdown theme. No code block is ever evaluated. The expanded adapter resolves all installed bundle language IDs or file-extension mappings, with common alias normalization; unrecognized or unsupported languages remain plain.
- Added 3 mdview integration tests to check highlighter wiring, JavaScript tokens/literal markup, and unknown-language safe display. [Grammar guide](../../SpectreConsole/Markdown.TextMate/Docs/Supported-Grammars/en.md) and CLI README updated.
- **Compilation/regression runs for this feature still pending** (no local .NET SDK). The 6,443-green user Windows checkpoint is pre-feature; an additional earlier interactive-picker increment was also not yet confirmed in a test log.

## 2026-11-04 — GitHub owner-only repository selection

- Added `mdview afrowaveltd`, `mdview git afrowaveltd`, and `mdview github:afrowaveltd` in command parsing; complete GitHub paths and explicit local paths remain supported. The no-argument source menu also accepts an owner alone.
- New `MdViewGitHubRepositoryPicker` displays a short tree for up to 20 repositories and a paged selection prompt with name/description filter, safe escaped labels and Escape cancellation; it only downloads the selected README through the existing adapter.
- Uses independent `GitHubRepositoryCatalogSource`: public user/org API, 30-minute validated disk cache, stale/offline fallback, capped 1,000-repository result with truncation warning.
- [Owner discovery guide](Docs/GitHub-Owner-Discovery/en.md). New command, picker and cache tests authored but **not yet executed** in this environment.

## 2026-11-04 — first disk-indexed raw source pager

- Added `MdViewSourcePager` using the new UI-independent `TextFiles.Utf8DiskLineIndex`. The new explicit `mdview source PATH` command supports UTF-8 source pages (24 physical lines by default) and Spectre.Console Next page, Previous page, Go to line and Escape/Exit. Previous page remembers actual start offsets and a jump discards stale page history.
- Source pager is read-only, noninteractive terminals receive an explicit error, entire UTF-8 text is validated before display and code-like Spectre markup remains literal. Page/index/line budgets are explicit and bounded. This is **raw Markdown source text**, not fully rendered or syntax-highlighted Markdown. The existing `mdview local PATH`, URL and GitHub rendering paths remain unchanged.
- Added six virtual-terminal `MdViewSourcePagerTests` plus an explicit `source` command grammar test. [Source pager documentation](Docs/Raw-Source-Pager/en.md) and [line index design](../../TextFiles/Docs/Disk-Line-Index/en.md).
- **No .NET SDK in this session:** new build and test pass pending. Next: verify all solution tests on Windows and Linux, manually exercise `mdview source README.md`; then design incremental Markdown block-state parsing/visual-line paging.
