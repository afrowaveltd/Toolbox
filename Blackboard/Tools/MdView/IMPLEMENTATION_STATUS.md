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

## 2026-11-04 — Windows regression checkpoint confirmed

- The preceding full solution log showed **MdView.Tests 42/42 green**, including source pager navigation and GitHub owner-selection coverage. Maintainer now confirms the remaining unrelated mock-pagination test has also been fixed and the full Windows suite passes.
- Normal formatted rendering and the opt-in `source PATH` physical-line pager remain separate modes. Next: incremental Markdown block-state parsing and eventual visual-row/formatted pager, keeping compatibility and bounded resource budgets.

## 2026-11-04 — persistent interactive session after full-document rendering

- Implemented `MdViewDocumentCompletionPrompt`: after a successfully rendered full Markdown document, Enter returns to the main source menu, Escape exits, and all other keys (including `F`, page navigation and arrows) are intentionally ignored.
- Extracted the lifecycle loop into testable `MdViewSession`. It distinguishes actual rendering from user cancellation and errors via `MdViewDisplayResult`, and reuses the existing local/URL/GitHub loading and renderer. Previous source menu, GitHub discovery, TextMate colors, cache policy and error codes remain unchanged.
- The CLI skips completion waiting when stdin/stdout is redirected or Spectre.Console reports noninteractive capabilities, keeping `mdview ... > output.txt` a one-shot command. `--help`, errors, cancelled repository chooser and the separate `source PATH` raw pager are excluded from the completion prompt.
- Added 5 key-handling tests and 8 session integration tests with Spectre `TestConsole`, covering Enter → menu → next document → Escape, filtering unrelated keys, noninteractive behavior and no double prompt. [Detailed interaction contract](Docs/Document-End-Navigation/en.md).
- **New change not yet compiled/tested** in the current environment (no .NET SDK). Last maintainer-confirmed full Windows suite was green before this change. Next: `dotnet test Toolbox.sln` and manual verification in Windows Terminal/Debian.

## 2026-11-04 — future indexed topic/phrase search captured

- New [Indexed Search](Docs/Indexed-Search/en.md) plan: future `F` shows seeded English README terms and heading titles or accepts an arbitrary word/phrase; `N`/`P` navigate previous/next indexed occurrences. Add unknown phrases on demand with a bounded scan, store compact offsets against a verified digest, and invalidate on document changes. Later support user-configurable seed terms and language options.
- **This is design only.** Current completion keys stay Enter→menu, Escape→exit; `F/N/P` and page keys remain inactive. The new standalone [ExchangeHouse incremental line scanner](../../../ExchangeHouse/Markdown/Docs/Incremental-Source-Scanner/en.md) is not yet integrated into the rendered mdview path. Continue from its test validation and future block events before implementing search.

## 2026-11-07 — mdview public XML documentation verified

- Documented every `MdViewSourceKind` enum value, `MdViewCommand` positional-record parameters and the `Parse` contract (arguments, result and relevant exceptions). Eliminates its previously reported `CS1591` diagnostics without changing command parsing behavior.
- Confirmed by [GitHub Actions run 42311109763](https://github.com/afrowaveltd/Toolbox/actions/runs/42311109763): fresh Linux build **0 warnings/0 errors**; Windows UI tests and documentation links passed.
- Recheck documentation on each new public API; do not disable `GenerateDocumentationFile`.

## 2026-11-08 — opt-in bounded rendered preview of validated source pages

- Added `MdViewSourceKind.FormattedPreview` and case-insensitive `mdview preview PATH` grammar (`4d31301ff9b46f27cdbf3af5841e4da44fa57d2c`), a separate `MdViewFormattedPreviewPager` (`754466f97251295dfaee04421b4f86952ca3c398`), and Program entrypoint wiring (`4fba512fe79801f25902379b295e05caf58fc6a2`).
- Uses `Utf8ValidatedSnapshot` and bounded `Utf8DiskLineIndex.ReadPageAsync` pages, then calls the existing `MarkdownSpectreRenderer` with **only the current source fragment**. Supports Next, Previous, Go to source line and Exit through Spectre.Console.
- Added command grammar and interactive pager tests (`ddb56e93d02538075624464d81865015fb577556`, `cae0180021afccd2d9624a2e80ae7d5cdeb99484`); English guide [Experimental Formatted Preview](Docs/Experimental-Formatted-Preview/en.md). Source `local`, `source`, GitHub and HTTPS behavior are unchanged.
- **Not a completed formatted visual pager**: independent fragment parsing cannot carry fence/list/quote state across page boundaries; line count is not actual terminal row count, and scanner-based block spans are not integrated. The next milestone is stateful container-aware incremental rendering with explicit boundary tests.
- **Fresh build/test/CI validation pending**. Last maintainer-reported green Windows solution run was before this new mdview mode. Run `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj` and `dotnet test Toolbox.sln`; then manually verify on Windows and Debian.


## 2026-11-08 — bounded container-aware fenced-code preview

- Added `Utf8ValidatedSnapshot.OpenReader()` as an independent strict UTF-8 forward reader over the already validated private copy (commit `54ba6fe6338a0492082717bf92b05135d60f3fcd`).
- Added `MdViewContainerPageCursor` with bounded `ReadAsync(firstLine, maxLines)`, retaining incremental `MarkdownIncrementalContainerScanner` state when progressing forward and replaying on reverse/jump (`9f8fbdfeb9681ba5bfb9a5ecc6aea85b89b71a56`). Added explicit direct project reference `b12659d6eaabf756f16617a3d82a93acff85d483`.
- Updated `MdViewFormattedPreviewPager` to render confirmed fence/code/closer events as literal Spectre Text (never parse Markdown-looking code), and ordinary runs with the existing Markdown renderer (`f5e5bd475b640952379c6d06732891a1ca9cd865`). New cross-page tests in `48bf8bc47064741cdc810f3fd279f9d7c147cc49`.
- **Scope:** preserves forward code fence classification, not full CommonMark, TextMate syntax colors for code, or terminal row paging. Reverse/jump replays from the start and may be slow on very long files. Existing default renderer is untouched.
- **Fresh .NET tests pending.** Run `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`, `dotnet test Blackboard/TextFiles.Tests/Afrowave.Toolbox.Blackboard.TextFiles.Tests.csproj` and `dotnet test Toolbox.sln`. Windows/SSH/Debian acceptance and possible renderer regressions remain to verify.
