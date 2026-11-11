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

## 2026-11-09 — bounded history and local navigation session

- Maintainer reports the previous local-link loader increment fully
  green, without a new detailed test count; this is the last confirmed
  baseline before the changes below.
- Added `MdViewNavigationHistory` in
  `63f1eea7fca5b5916e61009cc7b0d950658d3a2d`:
  source-neutral bounded document+fragment locations, default capacity
  128, duplicate suppression, Back/Forward, branch truncation, and
  oldest eviction.
- Added opt-in `MdViewLocalNavigationSession` in
  `2427df924e1bf521108717b20727f1bf45fd4045`:
  one owned validated snapshot at a time, history transitions only
  after successful target opening, source revalidation on back/forward,
  async disposal. Existing CLI menus are unchanged.
- Added history and session regression tests in
  `c7696d3db911c8dc113846f9460f3c1d8aeeff7c`.
  New test execution **not yet confirmed**.
- No clickable terminal links, navigation hotkeys, heading-anchor
  scrolling, network/GitHub target loader, or tamper-proof filesystem
  containment is claimed. Next: full tests, then controlled link
  extraction and explicit keyboard selection without changing the
  established completion prompt unexpectedly.

## 2026-11-09 — maintainer-confirmed green navigation checkpoint

- Maintainer confirms that the complete test suite is **still green**
  after introducing `MdViewNavigationHistory`, `MdViewLocalNavigationSession`
  and their regression coverage.
- This is a maintainer-reported verification; no new precise test count,
  platform or independently reviewed CI run is asserted.
- Next: implement bounded extraction and keyboard selection of Markdown
  links, then connect the existing local navigation session without
  changing established default viewer behavior prematurely.

## 2026-11-09 — opt-in Markdown link extraction and Spectre picker

- Previous local navigation-history increment was confirmed green by
  the maintainer (exact count not supplied for that run).
- Added `MdViewDocumentLinkExtractor` using the established
  `MarkdownParser.Parse(...).Blocks[].Inlines` source-aware nodes,
  filtering to resolvable links, with 1 Mi UTF-16 source-character and
  4,096-link budgets; source commit
  `3bddf6f6e072faf5c1befe6099136efe58924364`.
- Added independent `MdViewDocumentLinkPicker` with Spectre.Console
  keyboard selection, Escape cancellation and no implicit document open
  (commit `96746b60374c2db9575781801b9f411e3072202a`).
- New regression cases for nested inline links, original offsets,
  fenced/inline code, images, escaped markup, unsafe destinations,
  bounds and interactive selection (test commits
  `0ba10326c6d0795866fdde1386ab4cd81f8b29fb` and
  `445a8f4bc08ea23667c9aea8f82691856e4fab74`).
- **Not connected to normal CLI, completion keys or local session.**
  New test execution pending; SDK is unavailable here. Next run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`, then join explicit picker selection
  to safe local navigation in an opt-in command/session.

## 2026-11-09 — maintainer-confirmed green link discovery checkpoint

- Maintainer confirms **all tests green** after the opt-in Markdown link extractor, Spectre.Console link picker, and their new regression tests. No precise new counts or independent CI run are asserted.
- Verified-by-maintainer source commits: `3bddf6f6e072faf5c1befe6099136efe58924364`, `96746b60374c2db9575781801b9f411e3072202a`; test commits: `0ba10326c6d0795866fdde1386ab4cd81f8b29fb`, `445a8f4bc08ea23667c9aea8f82691856e4fab74`.
- Next: opt-in connection of explicit link selection to `MdViewLocalNavigationSession`, then regression-check actual document transitions and history before changing default CLI behavior.

## 2026-11-09 — opt-in local Markdown link browsing UI

- Added explicit `MdViewSourceKind.LocalLinkBrowser` and
  `mdview browse PATH` grammar in commit
  `c29bb36afe71d48ce591fa97f7ef5f0bb53c67ea`.
- Added `MdViewLocalLinkBrowser` in
  `cdacb01c0d27bbb5105ff283897155eda93549ef`,
  connecting validated local snapshots, the established parser-backed
  link extractor, Spectre link picker and transactional bounded
  Back/Forward history. Opt-in browser renders each local Markdown
  document only after full validation and has a 1 MiB input byte cap.
- Program/CLI wiring in
  `a5a42d2d41bc22ea8111509f38b1b271fb6bf498`
  returns `Displayed=false` to avoid a second document-completion
  prompt. Default local/GitHub/URL/raw/preview modes are unchanged.
- Virtual-terminal integration and parser tests added in
  `43dd4090fbf8283f89b26677c2841fcafe695c6e`,
  refined for Back/Forward coverage in
  `a7d60224c15d254facae7134450654076ba8a174`.
- Limitations: no heading scrolling for anchor fragments,
  no remote/GitHub target following, no 1 GiB streaming links,
  and no race-free filesystem containment. UI is interactive-only;
  errors on target open preserve current navigation state.
- **Fresh focused/full .NET tests are pending.** Maintainer-confirmed
  green baseline predates this UI integration. Verify
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`, plus a manual Windows/Debian
  terminal smoke run for `mdview browse README.md`.

- Windows handle-lifetime follow-up: `e35a0bc151717a8b72539c07e05e0b4c61d1b0d1` closes the snapshot reader before any Follow/Back/Forward transition can dispose the old snapshot; this avoids open-reader blocking deletion of the temporary source on Windows. Fresh tests still pending.

## 2026-11-09 — local heading-anchor display navigation

- Maintainer reports the opt-in `mdview browse` integration fully
  functional and the tests green before this new change (no precise
  counts supplied).
- Added `MdViewHeadingIndex` in commit
  `d482d5a6a6dcd6b224307fb4a4cac6c10079a2dd`:
  uses existing Markdown parser's heading classification, ignores
  fenced code, preserves original source offsets, generates
  lowercase/dash/Unicode slugs and disambiguates repeats.
- Integrated source-heading display jumps into opt-in
  `MdViewLocalLinkBrowser` in commit
  `17a92b687ab6ff738aa707251863cba941b5a9e2`.
  Fragment targets render from the matching heading; Back/Forward
  already preserves the fragment. Missing anchors display feedback.
- Regression tests added in
  `a0fb36799521561c962ba57f0567a428d7f3d368`,
  terminal input refined in
  `c071d0e31d19ef9b6d94679fb57aebcbaa9347a4`.
- **No claims of full GitHub anchor slug parity or true terminal
  scrollback repositioning.** The current jump renders the Markdown
  suffix, within the existing small-document budget.
- Fresh test execution pending; run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln` before release.

## 2026-11-09 — Debian heading-anchor virtual terminal test correction

- Maintainer's Debian .NET 11 RC SDK test run compiled all mdview dependencies and reported **112 passed, 1 failed, 0 skipped** (113 MdView.Tests total). Only failure: `MdViewHeadingIndexTests.FragmentOnlyLinkStartsRenderingAtTargetHeading` with `InvalidOperationException: No input available` from `Spectre.Console.Testing.TestConsoleInput`.
- Root cause: after following the anchor and returning Back, the source page presents **Follow / Forward / Exit**. The test supplied one DownArrow, accidentally selecting Forward instead of Exit, and exhausted the queued input. Added a second DownArrow, keeping production code unchanged (commit `8b6884e26a7ef5766815e84c2c928bb8f34caa10`).
- `NETSDK1057` is an informational preview SDK message; not the cause of this failure.
- **Retest pending after this correction.** Run `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`, then `dotnet test Toolbox.sln`; do not count the correction as green until confirmed.

## 2026-11-09 — HTTPS verified-cache link navigation

- Maintainer confirms all focused/solution tests green after the previous
  heading-anchor TestConsole input correction
  (`8b6884e26a7ef5766815e84c2c928bb8f34caa10`); no precise
  counts were supplied for this confirmation.
- Added `MdViewHttpsNavigationSession` in
  `d63ad06e868ca2a2636a3eb21d314e5b36c6451b`,
  preserving typed HTTPS document origin and fragment, verifying
  each changed URL via the existing `HttpMarkdownDocumentSource`
  and `VerifiedDocumentCache` before updating history. Back/Forward
  explicitly report verified Stale fallback; same-document anchors
  reuse current content without HTTP.
- Added opt-in `MdViewHttpsLinkBrowser` in
  `4211f92b1fcabd76927f4bcac04fdf1f5f1d2054`:
  1 MiB source byte cap, parser-backed link selection, full
  validated rendering, heading jump, failed-link feedback, Escape,
  and source freshness indicator.
- Added `browse-url HTTPS_URL` command grammar in
  `ec9c07d11f0fa4ef3587b25a69ffc4ed019e40ca`
  and Program wiring in `0e3a542a7c1cace0d747ab46b7aff0a229e36be1`.
  The CLI configures no automatic redirects/decompression; original
  commands remain unchanged.
- Nine isolated tests in
  `e9e987a251fd3a69abd08490b2599d9fb4c664fe`
  cover relative HTTPS links, anchor-only navigation, history,
  missing remote target rollback, Stale offline fallback,
  virtual-terminal Browse/Back and noninteractive rejection.
- GitHub default README adapter does **not** expose a canonical
  file path and resolved ref or fetch arbitrary repository files;
  GitHub relative navigation remains a separate milestone.
- **Fresh post-change .NET tests have not been confirmed.**
  Run `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`; manually smoke-test an HTTPS
  Markdown documentation link in Debian/Windows Terminal.

- Follow-up HTTPS navigation safety regression commit
  `e1ae4cee8ce4fd421c7f6921e8b3b2e1d85b750e`
  adds oversized-document rollback and cross-origin request rejection.
  New tests have not yet been verified by the maintainer.

## 2026-11-09 — opt-in GitHub repository file browsing

- Maintainer confirmed all preceding local/HTTPS Markdown navigation
  tests green before this increment (no exact new totals supplied).
- New `MdViewGitHubNavigationSession`
  (`cd0a025b4f59efd27c420feefa0b37c04d1e30b5`)
  uses the new verified `GitHubRepositoryFileSource` and the existing
  typed origin, parser-backed link resolver and bounded Back/Forward
  history. Initial README branch/ref/path comes from official GitHub
  metadata; changed file destinations are validated before history commits.
  Same-file anchors reuse verified content without a new API request.
- `MdViewGitHubLinkBrowser`
  (`498fccc1d2d8d0afbd52c05b2c45f7850a43bfb2`)
  provides an opt-in interactive terminal flow with exact
  owner/repository/ref/path display, verified content and metadata
  Stale indicators, link selection, heading display, Back/Forward,
  and recoverable invalid-target errors. 1 MiB source budget remains.
- CLI parser (`ae88207c13b00d1ab7e1aac57d1462b13da61684`)
  and Program wiring (`8535579c933e6fd743b1a989ec6bda307e46dea6`)
  support `mdview browse-git OWNER REPOSITORY` and
  `mdview browse-git OWNER/REPOSITORY`; no automatic HTTP redirects.
- Seven `MdViewGitHubNavigationTests` added in
  `ad8d7a445754789834216c013b85a85bc362d117`:
  command isolation, branch with slash, real file transitions,
  Back/Forward, same-file anchors, missing/oversized target rollback,
  virtual-terminal navigation and noninteractive rejection.
- Existing one-shot `git` and all local/HTTPS modes remain unchanged.
  Explicit exclusions: private GitHub repos, cross-repo/ref links, HTML
  blob URLs, attachments and >1 MiB complete-document browsing.
- **New tests pending developer/maintainer verification.**
  Read `Docs/Link-Navigation/en.md` and the new cache-provider
  `Docs/GitHub-Repository-Files/en.md`; run focused and full tests.

## 2026-11-09 — GitHub link visibility after first successful browse smoke test

- Maintainer confirms `browse-git` **loads and displays README
  successfully**, without reporting a runtime failure, but sees no
  links offered in the displayed document. This confirms reading,
  **not** yet successful link selection for a real Toolbox README.
- Source review confirms the root `README.md` contains many
  repository-relative Markdown links, including `Docs/**` and
  project README paths. Rendered Markdown links are not directly
  clickable: the `Follow a link` picker appears after the
  rendered document, when extraction returns navigable links.
- Added a visible `Navigable Markdown links: N` header and menu hint
  in `MdViewGitHubLinkBrowser`: commit
  `528c83196516bb9e9c6a56e978d545913ddf4c11`.
- Added a Toolbox README-shaped extraction regression in
  `b895ecfc5eeeb61b1e9a08eb1a15cf2927cc43a5`;
  added a virtual-terminal link-count assertion in
  `d2b4bb09551dab366d6351f15ae8269708c82e3a`.
- **Post-change tests not yet confirmed.** Follow up with focused/full
  solution tests and run `browse-git afrowaveltd Toolbox` to inspect
  the displayed count and whether `Follow a link` is offered.
  If count is zero, inspect parser output/source identity; if nonzero,
  use the menu at the end of the document.

## 2026-11-09 — GitHub catalog HTTP 403 recovery

- Maintainer observed `GitHub catalog request returned HTTP 403`
  when selecting owner `afrowaveltd` interactively; subsequently
  identified the cause as the GitHub API request-rate limit, with
  a reported approximately 15-minute wait to reset.
- The catalog source already returns a structured Error or verified
  Stale response. Previous `MdViewGitHubRepositoryPicker` threw
  `InvalidOperationException` on Error, terminating the viewer.
- Changed picker to display the warning, accept a manually entered
  repository name, reject malformed names, or return null for Back
  without inventing a catalog (source commit
  `c1a52dc013340f4b7b6c8939fdf86717e475057d`).
- Updated picker regression tests to cover HTTP 403 fallback,
  HTTP 404 Back, invalid manual names and an empty verified catalog
  (test commit `24a6e1b412920aa0999b354e8b7488e238831cdb`).
- This does not evade rate limits: GitHub README/file requests may
  also fail until API access resets. Existing verified stale caches
  remain the only documented offline alternative.
- **New tests not yet confirmed green.** Run MdView focused tests
  and full Toolbox.sln after updating the working checkout.

## 2026-11-11 — fix interactive GitHub README missing link navigation

- User confirms the Markdown reader displays documents correctly but
  still cannot see navigable links after selecting GitHub from the
  **no-argument interactive source menu**.
- Root cause verified in source: `MdViewInteractiveSourcePicker` returned
  `MdViewSourceKind.GitHub`, which routes through the old one-shot
  `GitHubReadmeSource`/renderer. The implemented link browser
  `GitHubLinkBrowser` was only reachable using explicit
  `browse-git` commands; this was a routing/UX gap, not evidence
  of a broken Markdown link parser.
- `MdViewInteractiveSourcePicker` now clearly labels its GitHub
  option as **GitHub repository (browse Markdown links)** and returns
  `GitHubLinkBrowser` for both `owner/repo` and owner-only input
  (commit `a83168d13edea2571addbb74f7755bd852e23ebd`).
- `Program` now routes owner-only interactive choices through
  `MdViewGitHubRepositoryPicker.PickBrowseAsync` and then launches
  the same verified-cache GitHub link browser (commits
  `ebb4197fc3989a57117dc0a9bcb15e2ee6a7f50c` and
  `41c332af042d9b4b73e52c627583a0e5b5165187`).
  The picker method is an opt-in adapter; ordinary one-shot
  `PickAsync` and explicit CLI `git OWNER REPO` remain unchanged
  (commit `ba4482bd83e304a332a950693393010dadad0e12`).
- To make links apparent in long GitHub READMEs, the browser now shows
  the number of navigable links and the **first three actual
  link labels/destinations** before rendering, plus the original
  `Follow a link` keyboard picker after the document
  (commit `24d3e8e04650b6250a9bb72cab671f92f8eec3b2`).
- New/updated regression coverage: main-menu direct repo,
  owner-only catalog and 403 manual fallback, MdViewSession
  dispatch without double completion prompt, one-shot CLI
  compatibility and GitHub link preview. See
  `Blackboard/Tools/MdView.Tests/IMPLEMENTATION_STATUS.md`.
- Documentation refreshed in `README.md`,
  `Docs/CLI/en.md`, `Docs/Interactive-Source-Picker/en.md`
  and `Docs/GitHub-Owner-Discovery/en.md`.
- **New changes not yet tested by the maintainer or .NET SDK in this
  environment.** Run focused and full solution tests, then smoke-test
  no-argument GitHub selection with both `afrowaveltd/Toolbox`
  and owner-only `afrowaveltd`. A still rate-limited GitHub API
  may prevent obtaining metadata even though menu routing is fixed.

## 2026-11-11 — maintainer manual acceptance: MemView GitHub Markdown browsing

- Maintainer reports successful **manual end-to-end use of GitHub Markdown browsing and link navigation** against the independent public `afrowaveltd/MemView` repository after the interactive menu routing correction.
- Screenshot supplied of the rendered English Git lesson confirms readable headings, lists, paragraph text, code examples, and Markdown presentation in a Linux terminal. Maintainer explicitly confirms links work in this repository.
- This is **manual acceptance**, not a newly reported focused or full `.NET` test run. No test totals, CI status, or exhaustive source/anchor scenarios are inferred.
- Next follow-up: retain the working GitHub browsing behavior, separately verify focused/full regression suites, and continue examining usability and large-document paging without weakening verified cache/source boundaries.

## 2026-11-11 — bounded GitHub Markdown source-page navigation

- Maintainer has confirmed the previous GitHub browsing implementation
  worked end-to-end against the separate `afrowaveltd/MemView` repository,
  including working relative Markdown links and readable C code
  documentation in Linux Konsole (screenshots provided). This is the
  **manual baseline**, not verification of this new page increment.
- Added `MdViewSmallMarkdownPageIndex`, `MdViewMarkdownSourcePage`
  and `MdViewMarkdownPageSegment`:
  `e7612a2b5532aec8a45b4a236e80cafcb187f5a2`.
  A document already limited by the verified GitHub browser to 1 MiB
  is parsed with the shared ExchangeHouse parser. The index maps
  UTF-16 source offsets to physical lines and returns at most 24
  source lines and 16 Ki UTF-16 display characters per page by default.
  Source-aware fenced-code classification prevents false Markdown
  headings/lists at a page boundary.
- `MdViewGitHubLinkBrowser` now switches long verified Markdown
  documents to bounded source-page output, offers Next/Previous
  page, Go to heading (first 512 headings) and Go to source line,
  while retaining the existing Follow a link, document-level
  Back/Forward, original cache freshness display and Exit:
  `c03e53300bdcd5dd3db670e8eea70d8cfa885d5a`.
  Short documents (up to 24 lines) keep their previous one-shot
  renderer and menu order. Document/page index and link catalog
  are reused by content SHA-256/source identity so page changes
  do **not** fetch GitHub content or reparse the full source.
- Source-line display truncation is bounded and now avoids splitting
  UTF-16 surrogate pairs:
  `4fa996781a4a9bc289301ec41081f753a6fbd901`.
  Missing anchors report their warning immediately:
  `e00632a556b7ca21d2010ec68ab19bbdea00c026`.
- Six new focused page-index regressions:
  `d5e6b5ddeab7fe0209df836df5446aeb27e094ab` and
  `0d6d65a29fd02e8d59206d51054cf8e63e78dbad`;
  five new virtual-terminal integration regressions:
  `2ce5a596dd19c2bed1187c9cebf457c8f9757dc4` and
  `024897d3a37d69a54c3ae6cda43212d192c53c3c`.
- Full guide: `Docs/Paged-GitHub-Browsing/en.md`.
  Existing local disk-backed raw/source preview, container scanner and
  sparse scanner checkpoint infrastructure remain unchanged.
- **Limits:** the GitHub document is still fully parsed in memory
  and limited to 1 MiB; pages are physical-source-line slices, not
  terminal-row layout. Continued nested list/quote containers and
  cross-page stateful TextMate syntax coloring are experimental.
  Pages with extremely long lines visibly shorten the output without
  altering verified source bytes.
- **Fresh tests pending maintainer confirmation; no local dotnet
  executable is available in this environment.** Run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`. Manually smoke-test long
  `browse-git afrowaveltd MemView` documents and confirm Next,
  Previous, heading, line and link navigation.
- Next milestone: design/implement a **disk-backed heading/link
  source index** and reconnect the existing validated snapshot
  and sparse container checkpoints to context-preserving large-file
  rendering. Do not lift the GitHub 1 MiB cap prematurely.

- Final picker-width hardening: `86afdc02e85860187a50b072f0fb441124eb4647`
  truncates displayed heading labels, and
  `1081d13cb6e16df2dffb010d1d261ca7ecc5d348`
  preserves Unicode surrogate pairs in shortened link/heading labels.
  No new automated green claim is made.

## 2026-11-11 — fix failed UTF-16 page boundary test (147/148)

- Maintainer's Debian `MdView.Tests` result: **147 passed, 1 failed,
  0 skipped, total 148**. The single failure was
  `PageCharacterBudgetDoesNotSplitUnicodeSurrogatePair`
  (expected 16,383 UTF-16 characters, actual 16,384).
- Source analysis identified that `GetPage` correctly avoided
  cutting the emoji surrogate pair but then consumed one character
  from the **next physical line** using the leftover budget. This
  is a pagination boundary issue, not a Markdown parser failure.
- Fixed `MdViewSmallMarkdownPageIndex.GetPage` to stop emitting
  source lines immediately after shortening a long line and to
  leave a next line intact if its first Unicode scalar cannot fit
  the remaining UTF-16 page budget. Source commit
  `fe9ccc8b734ac7b399f1fca5bbb3ddaa134c927f`.
- Strengthened the original Unicode test and added two regression
  cases for no next-line leakage and preservation of an emoji at
  the beginning of the next line: test commit
  `b276c12c2de5344f3ae313e28038c8a717f8e850`.
- Only bounded source-page rendering changed. Existing GitHub
  verified cache, Markdown link navigation, history, one-shot
  commands and indexed large-file pager remain untouched.
- **Post-fix focused/full tests pending maintainer verification.**
  Run `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`; do not claim all green until
  the revised tests are executed.

## 2026-11-11 — maintainer-confirmed green after Unicode page-boundary correction

- Maintainer confirms **no errors** after updating to the UTF-16 source-page boundary fix and new regression tests. This supersedes the previously reported 147 passed / 1 failed result; exact current test totals were not provided and are not inferred.
- Verified-by-maintainer source fix: `fe9ccc8b734ac7b399f1fca5bbb3ddaa134c927f`; regression tests: `b276c12c2de5344f3ae313e28038c8a717f8e850`.
- Next milestone: disk-backed heading/link indexing for large Markdown files, building on validated snapshots and existing disk line/scanner checkpoint indexes; retain the 1 MiB bounded GitHub rendering cap until a genuinely streaming navigation path is tested.

## 2026-11-11 — disk-backed heading/link index for large local Markdown previews

- Previous unicode-page boundary fix and MdView test suite were
  explicitly maintainer-confirmed green. That is the baseline before
  the changes below; new test totals are not asserted.
- Added `MdViewDiskNavigationIndex` in commit
  `6ed7131b6502a6712e5a6a96ad835dc0a572ade2`:
  a sequential, bounded-memory scan of a caller-owned verified
  `Utf8ValidatedSnapshot` through existing incremental Markdown
  container and inline parsers. Source-bound heading and inline-link
  positions are stored in three temporary files (records plus two
  64-bit offset tables), with a combined default 128 MiB disk budget,
  16 KiB record cap, bounded labels/destinations, cancellation and
  cleanup on failure. It keeps no full AST or complete result list
  in RAM. Conservative fenced/indented code exclusions were hardened
  in `77706689f963a746f5a55d32720cae02e29ebeb6`.
- Added `MdViewDiskNavigationPicker` (`05d27fa6068de1c24c56ae7730c5a564bde15734`)
  for keyboard selection in batches of 12 entries, with Next/Previous
  results and Escape/Back.
- Integrated lazy index construction into the existing **local**
  `MdViewFormattedPreviewPager` (`47e56f1112983f1c53d88a0b4615ee3b3252e138`,
  formatting `097594ecb0acf6dd581e0a64d0ca82ca32fdae3e`).
  New menu items: **Find heading (disk index)** and **Find link
  location (disk index)**. Selected entries jump to a physical
  source line; they do not automatically open linked destinations.
  Normal paging does not build the index until it is requested;
  an index build error leaves the normal pager intact.
- Tests: `MdViewDiskNavigationIndexTests` (`51a622bbba1dea415d7104e96a86d62911d7695e`,
  fixture correction `e86e8a63e5511cd52c5bb2c1b1d5faf0d53e9822`,
  offset assertions `7789e49d110f84ef2866283a9f6c673fc23cbfb0`,
  indented-code regression `750bca818071a7b5d9e04a50271b6ecc873656ba`),
  picker tests (`262df9bb5ce89da3ad1cfaad508afff40ff15a9b`),
  preview terminal integration cases (`6d0d3482e2d9afbade716c2f4cb9a58d70756b91`).
- English guide: `Docs/Disk-Navigation-Index/en.md`.
- **New tests have not been executed in this environment.**
  Run focused MdView tests and full Toolbox.sln; smoke-test long
  local `mdview preview PATH` with indexed heading/link jumps.
- Current limitations: conservative line-local inline grammar, no
  reference-link/multiline/CommonMark parity; indexed link entries
  are source locations, not cross-document navigation. Index build
  requires one sequential pass and remains limited by the snapshot's
  own disk/line budgets. Large-document visual-row pagination and
  arbitrary GitHub files above the current 1 MiB cap are still pending.

## 2026-11-11 — viewport-aware Markdown preview, punctuation wrapping and code clipping

- Maintainer previously confirmed the local disk heading/link navigation
  preview works on a real Markdown document. This is the **pre-change
  manual baseline**, not confirmation of the new responsive renderer.
- Added `MdViewTerminalWordWrap` in
  `c72f5f0df7811441c1184d9a73c7ccdfea4c2a61`: deterministic
  terminal-cell-aware line wrapping that prefers sentence, clause
  and word boundaries. Long unbreakable prose tokens are split on
  Unicode scalar boundaries, not between UTF-16 surrogate halves.
  Fenced code is not wrapped and uses a trailing right-arrow `→`
  when its full text does not fit the viewport.
- Added `MdViewTerminalPageIndex`
  (`a85b872b09361e3c449400acdfd146f63d12ee1e`)
  with 12-byte disk-only visual page start records
  (physical source line plus in-line wrap-row offset), default
  64 MiB index disk cap and one complete sequential source scan.
  The index reports PageCount for a known width/height, with 16
  reserved terminal rows and existing 24-visual-content-row default.
  Added line+wrap-row page lookup
  (`c95209013787a122ba691d1d6da3aafe01f63a3d`) and
  first-visual-page source-line lookup correction
  (`ea09bbb3691aa0b521b2793d169ab5f729ae035f`).
- Added `MdViewTerminalPageRenderer`
  (`1b7c517eb315b0aa6e8a1d9fea801dfc76dd008b`):
  visual-row-bounded renderer backed by the existing
  `MdViewContainerPageCursor`, with resumable source+wrap offset.
  Empty source rows render explicitly
  (`33b72f91513f14895dbda08af5c52b0c7756e332`).
- Reworked experimental **local** `MdViewFormattedPreviewPager`
  (`d1e1bae61ce40fd8ea748cab0304de3e18a26342`):
  technical header and footer use grey `Spectre.Console.Text`;
  one blank row separates header from Markdown; footer shows
  **Page X of Y** and **Source lines A–B of C**. Width and height
  are checked before each redraw; resize rebuilds the disk page
  index, retaining current source line but discarding invalid
  prior-width wrap offsets/history. Next/Previous and lazy
  heading/link source jumps remain available; noncanonical
  starts are flagged as `(jump view)`.
- New tests: five `MdViewTerminalWordWrapTests`
  (`6a607adcb02e395b8b9e2cc7e0e3c0dbc219d619`,
  refined `06ecfa802d885932371ac11cd1c65f49729d0b5b`),
  five `MdViewTerminalPageIndexTests`
  (`259eebca8119b08791bf105d11974e96a05b0368`),
  plus two preview integration regressions
  (`7752320d5738afee1b0c0c113ab9ee389cfbdb0c`;
  corrected narrow-header assertion
  `1d4b1961dab83af11963a767bd1c0e259fef7825`).
- Main limitations: total pages are exact for the deterministic
  source/visual-row algorithm, **not guaranteed pixel/cell-exact**
  for arbitrary Markdown decoration, grapheme-cluster emoji
  composition, nested containers and different terminal fonts.
  Long code lines are clipped on screen without horizontal
  scrolling yet; the raw source remains complete.
- Detailed guide: `Docs/Responsive-Preview/en.md`. Existing
  local raw source pager, GitHub/HTTPS source adapters and small
  1 MiB GitHub Markdown link browser are unchanged.
- **Fresh focused/full `.NET` test results not yet supplied.**
  Run `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln` and manually resize Konsole
  between page actions. Do not claim green until results are known.
