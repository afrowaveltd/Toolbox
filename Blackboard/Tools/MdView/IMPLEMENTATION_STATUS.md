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


## 2026-11-08 — disk-journal reverse navigation, distinct from scanner checkpoints

- Replaced reverse replay in `MdViewContainerPageCursor` with a disk-backed journal of complete serialized `MarkdownContainerLine` events plus an 8-byte-per-line offset table (commit `a30c6283dcaac88774c6c59a24e6e15fd11250ad`).
- Already scanned pages are loaded from the on-disk journal without replaying parser state; the live incremental scanner continues only for requests beyond the scanned high-water mark. The cursor exposes `CachedLineCount` and `JournalBytes` for testing/diagnostics.
- Explicit 512 MiB event and 256 MiB offset default disk budgets, 8 MiB per-record cap, cancellation support and fail-closed behavior after scanning/write failure. Both journals use `DeleteOnClose` and are disposed independently of the caller-owned validated snapshot.
- Added five focused tests in `487e50ea9d73e134b7a13cf1761d291ad1b736ad`: backward reuse, forward-after-reverse fence parity, journal budget, offset budget and empty file.
- **Not a scanner-state checkpoint** or constant-time navigation to previously unseen source lines. Large forward skips still require intermediate scanner work. This is a bounded correctness-first event cache; no benchmarked HDD performance claims.
- **Fresh tests not yet run in this environment.** Verify `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj` and `dotnet test Toolbox.sln`, plus Debian/Windows file-cleanup behavior.

## 2026-11-08 — Maintainer-confirmed Windows green checkpoint

- Maintainer reports the Windows Toolbox test run is **still green** after the disk-backed classified-event journal implementation (`a30c6283dcaac88774c6c59a24e6e15fd11250ad`) and its focused regression tests (`487e50ea9d73e134b7a13cf1761d291ad1b736ad`).
- This is a user-confirmed test result. No detailed test count, run artifact, or Linux verification was supplied; do not infer any of those.
- Next planned step: design restorable, serializable incremental scanner state with correct fence/list/quote parity, independent budget controls and regression coverage before enabling arbitrary-position checkpoint seeks.


## 2026-11-08 — external scanner checkpoint format prepared

- ExchangeHouse.Markdown now has a versioned validated `MarkdownScannerCheckpoint` data contract (commit `9aa8bfb0f44452779f1646b3297aff601d280f90`) and five focused codec tests, documented at `ExchangeHouse/Markdown/Docs/Scanner-Checkpoints/en.md`.
- This does **not** alter `mdview preview`. Its classified-event disk journal remains the only implemented cache for reverse navigation. Scanner export, resume, source binding and fast skipping into unseen source regions remain pending.
- Previous user-confirmed Windows green baseline predates this new codec; fresh compile/test verification pending.


## 2026-11-08 — ExchangeHouse incremental parser state-machine API available

- `MarkdownIncrementalContainerScanner.StateMachine` now supports `Process`, `Capture` and `Restore` (commit `eed96080d2567ea5d68a3b679a5c8c83840ab766`) with focused golden parity tests (`5ad6259202aa739f8e409309eb963233f60999f4`).
- **No mdview runtime integration in this step**: the existing disk-backed classified-line journal is retained until source identity and bounded resumption from a byte/UTF-16 boundary are implemented and tested. A checkpoint codec is not evidence of correct source ownership. New parser tests still require a green rerun.

## 2026-11-08 — persistent scanner cancellation ownership fix

- Corrected `MdViewContainerPageCursor` so the long-lived async scanner
  never captures the first `ReadAsync` caller's cancellation token.
  Previously an unrelated cancellation **after a successful first request**
  could terminate future incremental navigation.
- New regression tests cover late cancellation of a completed request
  followed by fence-aware forward navigation, and a pre-cancelled call
  followed by a successful page request.
- Source commit `0d8a8ac24c405480f4fff3dda7801466483614e4`;
  tests `5a7ae69e4d526c41656c9931a1568c81d3b18364`.
- Per-call cancellation is checked at each scanner advance and journal
  operation, but cannot directly interrupt an in-flight local source
  stream read. Cancellation during advancing a scanner remains fail-closed.
- **Fresh test execution not verified**; continue with focused mdview
  tests, full solution tests and Windows/Debian acceptance before release.

## 2026-11-09 — maintainer-confirmed green test checkpoint

- Maintainer reports **6,726 passing tests** after the mdview scanner cancellation-lifetime change and its regression tests. This is a maintainer-confirmed result, not an independently inspected CI run; test runner platform and detailed counts were not supplied.
- The tested change is production commit `0d8a8ac24c405480f4fff3dda7801466483614e4` and tests commit `5a7ae69e4d526c41656c9931a1568c81d3b18364`.
- Next: bind serializable scanner checkpoints to a validated source and enable bounded seeking to previously unseen Markdown sections; retain the existing journal and legacy viewer behavior until parity is tested.

## 2026-11-09 — snapshot-bound checkpoint resume foundation

- Added `Utf8ValidatedSnapshot.OpenReaderAtByteOffset` for independently owned
  strict UTF-8 readers at validated byte boundaries (source commit
  `57a0cecbba5b4491545ea9511fe50a3969fbc9fe`).
- Added isolated opt-in `MdViewCheckpointPageReader` which binds opaque
  checkpoint positions to one reader instance and combines
  `Utf8DocumentOffsetIndex`, physical-line byte-start alignment checks,
  fresh snapshot readers, and `StateMachine.Restore` to process bounded
  pages at previously obtained checkpoint positions.
- Implementation commit `d615853d8d6ada9492ff8f19dfeb2fdce96641b3`;
  focused tests commit `bf8c892977700f2c9f5f2a6a8f5583a934abc257`.
- **No change to the existing mdview CLI pager/journal.** No arbitrary
  unseen-line O(1) navigation yet; checkpoint production still needs forward
  classification. Checkpoint objects are instance-bound but not
  cryptographically tied to bytes. Raw line `Kind` is locally classified
  after seeking; the restored container `Kind` is authoritative.
- **New .NET test execution unverified** (SDK unavailable in this execution
  environment). Last maintainer-confirmed checkpoint: 6,726 tests green,
  before these commits. Next: test Windows/Debian, then build bounded
  persisted checkpoints bound to the validated snapshot.

## 2026-11-09 — disk-backed sparse Markdown scanner checkpoint index

- Added internal trusted checkpoint rebinding to `MdViewCheckpointPageReader`
  (commit `ad9c024778239dde7f8f334affffed705d425eb6`).
- Added `MdViewDiskCheckpointIndex` (commit
  `05256266a6a92615018381c7037852c88c75ea28`): one-time source
  classification, bounded JSON checkpoint storage and an 8-byte slot table,
  configurable stride (default 1,024 lines), default 256 MiB combined disk
  budget and delete-on-close cleanup. Restores nearest previous checkpoint,
  then replays fewer than stride preceding lines. Existing journal and
  default mdview UI remain unchanged.
- Follow-up source commit `d88e0678f9b70ce0272136da4ecc1087f2d1843a` makes the fully initialized offset stream's nullable-flow contract explicit for the checkpoint writer.
- Added tests for sparse forward/backward/repeat fence-state parity,
  exact-EOF checkpoints, invalid arguments/budgets and empty input
  (commits `2c4967efa33fa344d28e0ad9996f4fd1ed7a0141`,
  `e31ece89e2ac375585ec1e88d30fd5987db9f3a9`).
- **Build/test status:** unverified after latest changes; maintainer's
  preceding test run was green, but does not cover this new index. Do not
  claim benchmarked HDD gains or O(1) first-time indexing. Next:
  validate focused/full suites, benchmark source indexing on HDD/SSD and
  consider reusing the same sequential reader during checkpoint creation
  before wiring any new UI mode.

## 2026-11-09 — one-pass checkpoint construction; link navigation roadmap

- Changed `MdViewDiskCheckpointIndex.CreateAsync` to classify the private
  snapshot through **one forward reader**, capturing sparse
  `StateMachine` checkpoints at the chosen stride instead of repeatedly
  reopening the source after each 128-line slice. Source commit
  `cb6b814bf2d3234c384e84c7e58b789e3ad01b3f`.
- Added Unicode, CRLF/CR/LF and nontrivial stride random-access parity test
  in `6efabec11171dacc4a3de3e6ec3ae91fbac89634`. Index remains
  opt-in; existing CLI preview and disk journal remain unchanged.
- Added [Link Navigation](Docs/Link-Navigation/en.md) plan in commit
  `888853b20dbc1c6b46ba8ee06d2276263efa11f0`.
  **Interactive links are not yet implemented.** Roadmap includes typed
  local/HTTPS/GitHub bases, relative links, in-document heading anchors,
  explicit link selection and Back/Forward history, with safe resolution,
  cache/offline handling and regression tests.
- Maintainer reports the prior full suite still green; **this new code
  and test increment has not been independently rerun**. Next validate
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj` and
  `dotnet test Toolbox.sln`. Then implement UI-neutral source-aware link
  resolution before changing interactive navigation.

## 2026-11-09 — UI-neutral relative Markdown link resolution

- Added `MdViewDocumentOrigin`, `MdViewResolvedLink` and
  `MdViewLinkResolver` in commit `7e978cd885727726650b6e2d01047fd5c0c4c8cf`.
  Local navigation bases are explicit canonical roots; HTTPS bases
  preserve secure URI origin; GitHub bases retain owner/repository/ref/path.
  No document is opened by this resolver.
- Added focused local, HTTPS, GitHub, anchor and unsafe target tests
  (`5279fab4e9bb03929db8e7d254df3262a7cb151d`) and a nested
  relative parent-path correction
  (`53ecda0e5b7aac91583a929dfe49f069986f0a48`).
- This implementation is deliberately conservative: no encoded
  link paths, query URLs, cross-origin navigation or local symlink
  traversal protection. Filesystem identity must be checked by the
  later document-opening layer. Link extraction/selection, anchor
  heading index and Back/Forward UI are **not implemented**.
- Maintainer confirmed green baseline before this code. **Fresh new
  tests not yet confirmed**. Next: run focused and full solution
  tests, then add safe source loading and explicit link selection.

## 2026-11-09 — local relative Markdown link loader

- Maintainer reports **89/89 MdView tests and 6,745/6,745 solution
  tests green** after the typed resolver increment. This confirmation
  predates the loader changes in this entry.
- Added optional `MdViewLocalLinkLoader` and owned
  `MdViewLocalLinkedDocument` in commit
  `8d18686c8b9e46e0861286007a37883511c6386e`.
  It resolves only permitted local Markdown paths, checks extension and
  lexical root, rejects symlink/reparse components and validates the entire
  target through the existing budgeted private UTF-8 snapshot.
- Added five regression tests in
  `777b918bc0cb1a82e49f7a3eb63cefc84f1285ec`
  (nested Docs link, parent navigation, non-Markdown/traversal,
  invalid UTF-8 and disk limits, platform-conditional symlink check).
- **Security boundary:** directory attribute checks before open are not
  resistant to concurrent path substitution. No claim of OS-enforced
  confinement, hostile same-user tamper resistance or cross-source loading.
- **New test execution pending.** No change to the default CLI or
  rendered link interaction. Next: verify tests and build a safe source-
  specific navigation controller/history using local documents first;
  HTTPS and GitHub can reuse their own authenticated/validated providers.
