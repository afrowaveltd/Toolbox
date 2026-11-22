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
  reserved terminal rows. The default now uses viewport height
automatically (up to 128 visual rows); an explicit 1–128 cap remains supported.
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

- Follow-up safeguard: `8edf72f7fc45eebc0959d4a7b58b44fecee086f0`
  conservatively treats unowned indented source lines as code
  for no-wrap/clipping behavior, not only explicit fences.
  Regression test: `7f8a9b85148dc5bf1aefb0da58cc0bf6f687f31c`.
  Post-change tests remain pending.

- Height adaptation follow-up: `3004bfc4fb2af9a3789343b79a31b2f8ac3c9de5`
  and `8a6defbeb986cb254f147d813959bce44cb4c3a1`
  remove the old fixed 24-row default; automatic pages can grow
  up to 128 visual rows. Test `996a03c2ab7abccf3520b853ad374411722b940d`
  compares narrow, wide, tall and explicitly capped viewports.
  Fresh .NET test verification remains pending.

## 2026-11-12 — 174/176 responsive-preview test failures corrected

- Maintainer's Debian `MdView.Tests` output: **174 passed,
  2 failed, 0 skipped, 176 total**.
- `MdViewTerminalPageIndexTests.CodeLineClippedWithArrowIsOneVisualRow`
  failed because its assertion expected literal `# Ending`,
  while `MarkdownSpectreRenderer` intentionally consumes the
  ATX heading marker and renders `Ending`. Production heading
  rendering did not need changing. Assertion corrected in
  `ff2411594f620b618dafad085cd48064dfd074b1`.
- `MdViewFormattedPreviewPagerTests.CodeOverflowIsMarkedButCodeSourceRemainsUnchanged`
  failed because the 31-column test terminal wrapped the long
  grey overflow explanation; the asserted uninterrupted string
  was absent. The pager now emits a compact single-row warning
  chosen by content width, preserving the full explanation on
  wider terminals: commits
  `c1889b53d3f62de5c7372674b54ccac1d6b9c4f3`,
  `d58d11d2be89b1ee8ca803a55b8cb6003173ccd4`.
- Updated the narrow-viewport expectation in
  `a33ecffbf780947b060059e2142bd288b1c6bbbf`
  and added a wide-viewport regression in
  `679628725495c72a8c91fde21ca2770edd42e6b5`.
- No changes were made to the validated source data,
  disk page index, source pagination algorithm, code-line clipping
  or Github/HTTPS navigation. The change only corrects the
  display-width-aware status text and test expectations.
- **Post-fix tests not yet run here or maintainer-confirmed.**
  Run `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  followed by `dotnet test Toolbox.sln`. Do not label the new
  checkpoint green until those results are available.

## 2026-11-13 — maintainer-confirmed green responsive-preview checkpoint

- Maintainer reports **all tests green / no remaining failures** after updating to the responsive-preview heading assertion and width-aware code-overflow notice fixes.
- This confirms the post-fix test checkpoint following the previously reported 174 passed / 2 failed result. The current exact pass/skip totals were not supplied and are **not inferred**.
- Includes the fixes for formatted ATX heading expectations, narrow-viewport code warning and wide-viewport warning regression. No further production changes at this checkpoint.
- Next incremental milestone: continue refining responsive Markdown visual-row fidelity (especially complex lists, quotes and inline formatting) and code overflow usability while preserving disk-indexed source navigation and verified cache contracts.

## 2026-11-13 — shared source-aware visual rows for lists, quotes and links

- Following a **maintainer-confirmed all-green** responsive MdView
  test checkpoint, the next planned implementation increment addresses
  page count/render disagreement for Markdown containers and inlines.
- Root cause: the disk visual-page index counted wrapped **raw Markdown**
  while the renderer parsed each soft-wrapped fragment as a separate
  Markdown document. Invisible URL destinations, displayed list bullets,
  quote gutters and Markdown marker removal could produce a different
  number of actual rows or make repeated list markers appear.
- Added `MdViewTerminalLineLayout`:
  `421c027d6ba3f71473d7c7130031f874c3a29324`.
  It consumes one already-classified Markdown source line, separates
  visible container markers and styled inline text, applies hanging
  indentation to wrapped list items and repeatable quote gutters,
  preserves link label/inline emphasis/code styling across wraps,
  and leaves fenced/indented code literal and horizontally clipped.
- Both `MdViewTerminalPageIndex`
  (`3d535cabe7513a09f852ac193c70a1391866374f`) and
  `MdViewTerminalPageRenderer`
  (`072f58a1e60de968b0771b27d5dd39f40d8d2758`) now use
  the **same** `Layout` rows. The renderer writes literal
  `Spectre.Console.Text` spans with exactly one newline per
  indexed visual row, rather than reparsing each soft fragment.
- Added `MdViewTerminalWordWrap.SliceProse` to preserve exact
  UTF-16 display offsets for styled spans:
  `7b2f1bdecd8c5921fc9e5739a831969806c32b58`.
  The original wrapping API now calls the same slice algorithm:
  `b19a1a9d3aaa0d7a4f92d3b1b901ae03e0c13165`.
- Per-line inline span slicing uses a monotonic token cursor and
  bounded adjacent-token merging for improved long-line behavior:
  `fe389c44a8cb5da71941ce6551c2cf726aa7d9c6`.
  The snapshot's existing line-size and disk-index budgets remain.
- Nine new `MdViewTerminalLineLayoutTests` verify hanging
  indentation, quotes, nested quoted numbered markers, wrapped
  and quoted links, strong/inline-code roles, fenced code literal
  handling, escaped/malformed markup, and disk index/renderer
  resume-point agreement:
  `8bfe2de61757c609d052eeee6afa0cb82feba071`,
  `83b3372f4f870f9eefbe94153b0d88b597779c03`.
- New guide: `Docs/Styled-Visual-Layout/en.md`.
  Existing GitHub/HTTPS browser, raw source pager, validated
  snapshot and disk navigation index behavior are unchanged.
- **Fresh post-change focused/full test runs not yet confirmed.**
  The prior all-green report applies to the preceding checkpoint.
  Run `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln` on Debian/Windows.
- Limits remain: conservative incremental scanner ownership
  (not full CommonMark), no table/setext/reference-link layout,
  no terminal-specific grapheme width guarantee, and no
  interactive link following from local large-file `preview`.

## 2026-11-13 — maintainer test acceptance with one warning pending triage

- Maintainer reports that tests for the new shared styled visual-row layout are **passing** and that the local preview works correctly in practice.
- One **orange warning** was observed, but its exact text, diagnostic ID, source and category have not yet been provided. Do **not** record a zero-warning build or mark the warning as resolved.
- Automated test totals and separate full-solution results were not supplied; do not invent counts.
- Next action: request the verbatim warning (or screenshot), identify its source, fix and test the cause, then update this status. This does not block recording the successful functional test checkpoint.

## 2026-11-13 — orange warning resolved by .NET 11 final upgrade

- Maintainer clarified that the sole orange warning was a **.NET preview-SDK warning**, not a MdView analyzer, compiler, or runtime defect.
- After updating the local environment to **.NET 11 final**, the warning disappeared. All MdView tests and functional behavior were previously reported as passing.
- **No source change required**. The earlier warning triage note remains as historical context, superseded by this resolution. Exact test counts were not supplied.

## 2026-11-13 — conservative compact pipe-table cells for local preview

- The previously confirmed **green** test checkpoint and resolved
  .NET preview-SDK warning are the baseline. This increment adds
  new code and tests; **fresh test execution is pending**.
- Added `MdViewTerminalPipeTableLayout`
  (`bab282198b2c5f349e331b109ee4874d55ee94ba`)
  for `mdview preview PATH`. Explicit, top-level
  `| cell | cell |` Markdown rows become numbered vertical
  cell rows that remain readable at narrow widths without
  loading whole tables in RAM. Separator rows become a visual
  divider; 2–32 columns are supported. Ambiguous, unframed,
  quoted/list-owned and fenced-code rows fall back to the
  established rendering path.
- `MdViewTerminalLineLayout` now delegates conservative pipe
  rows to the table component, and shares an extracted
  `LayoutStyledContents` helper for ordinary paragraphs and
  table cells (`d8656439985e66c3761d8c868e8ce5fa2b788bc2`).
  This preserves link/emphasis/inline-code styling while using
  **exactly the same rows** in the disk page index and renderer.
  Existing snapshot, cache and low-RAM contracts are retained.
- Inline pipe recognizer distinguishes escaped delimiters and
  backtick code spans. After a hardening pass,
  `d538533e7a63633a6ce1e1ae85e059bb04c92be3`
  preserves literal `\\|` inside inline code and only removes
  escape backslashes for ordinary cell display text.
- **Ten** focused `MdViewTerminalPipeTableLayoutTests`:
  `0d1aa364035a51f2d6d551b10832fbb446b3cb71`
  and `63dab5bf84b2bc8320decfe833ae1d675fc4ad1f`.
  Coverage: framed header rows, alignment divider, escaped
  and code-contained pipes, inline-code escape preservation,
  link styling, fallback for ambiguous pipe text,
  exclusion inside fenced code, eight-column terminal width,
  32-column input bound, and disk-index/render parity.
- English guide: `Docs/Compact-Pipe-Tables/en.md`;
  MdView README and CLI/responsive/styled layout docs updated.
- **Limitations:** this is a line-local compact reading layout,
  not full GitHub-Flavored Markdown table rendering. It does
  not infer header semantics across source lines, align fixed
  column widths across rows, accept delimiter-free rows or
  handle deeply nested table ownership. Those require later
  bounded lookahead and/or a disk-backed table context index.
- **Verification status:** new code not compiled or test-executed
  in this assistant environment; the previous green baseline
  does not imply these table tests pass. Run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  then `dotnet test Toolbox.sln` and smoke-test local preview
  on a Markdown document with pipe tables.

## 2026-11-13 — opt-in native Spectre.Console rounded table inspector

- Building on the **previous maintainer-confirmed green** compact
  table checkpoint, introduced an optional native Spectre table view
  without replacing the source-page visual layout. The user requested
  real Spectre.Console tables with borders and smarter parsing.
- `MdViewTerminalPipeTableLayout.TryReadCells` is now a public,
  XML-documented shared parser for conservatively framed, validated
  top-level pipe rows (`6b42956a3c397d4f820feabaf0402a3d02e5bf9e`).
  Compact visual cells retain their existing semantics. Escaped
  pipes/backtick code span handling is reused without duplicating
  the parser.
- Added `MdViewSpectreTableViewer`
  (`c78d01bd2489e913069493c912daa43dd44d0cef`):
  after a matching table header and alignment separator, an
  interactive modal constructs actual `Spectre.Console.Table`
  widgets with rounded grey borders. A wide viewport shows a
  multi-column grid with column alignment from `---`,
  `:---`, `---:`, `:---:`. Narrower viewports display an
  actual 2-column **Column / Value** Spectre table. The
  modal re-queries terminal dimensions per chunk.
- The native view uses the caller's existing validated, disk-backed
  `MdViewContainerPageCursor`, retaining only a limited chunk
  (at most 6 source rows in grid mode or 8 transposed fields per
  modal page). Cells are visibly clipped at 256 characters at
  most, or a smaller viewport-dependent bound; the original
  snapshot is not modified. A width under 28 columns leaves
  the compact source-page view as the usable fallback.
- Navigation: `Next table chunk` for large tables, with an
  end-of-table probe that avoids showing a false Next item
  (`4cd9af9185ebc6d2ae10f386aa6f09c4c710b52a`);
  Back/Escape restores the same source page. Headers are
  `Markup.Escape`d and cells rendered as safe literal
  `Spectre.Console.Text` rather than evaluated markup.
  Added small-terminal fallback and clear clipped-cell
  disclosure in `2642e09c9c4e410b0988866e2ef90952f7268cfd`.
- Integrated **View table (Spectre)** into
  `MdViewFormattedPreviewPager` for source pages containing a
  confidently recognized top-level header + matching delimiter
  row, and only for terminals wide enough for the widget:
  `5c74c4e6fdd1abc44c514e8b8ee5b322dc6807ca`,
  `10905bcf479d56891b890baeb9f44294efcb1195`.
- **Deliberate compatibility boundary:** main Markdown source
  pages still use the earlier compact cell layout and its
  exact disk-indexed `Page X of Y`. This optional native
  modal does not claim main document visual page rows measured
  by Spectre's border/wrapped-cell renderer. Integrating native
  frames directly into source pages requires a bounded table-
  block scan and accurate Spectre measurement on both index
  and display paths. This prevents loss of source navigation
  or miscounting page boundaries.
- Eight focused `MdViewSpectreTableViewerTests`
  (`603284757ba82a9a038b771ff0fcad023aa8f5ce`);
  three preview integration tests
  (`e060e79d203611417c2f2c7b1d67cef1bc4381bc`).
  Test cases cover native rounded borders, alignment,
  wide/narrow layouts, incremental chunks, source preservation,
  malicious-looking markup as literal data, code-fence
  exclusion, accurate menu eligibility and restoration of
  the original source page.
- New English guide:
  `Docs/Native-Spectre-Tables/en.md` and updated MdView
  README, tests README, CLI, responsive and compact-table docs.
- **These new tests have NOT been run in the assistant's
  environment**. The last confirmed all-green test suite
  predates this change. Run the focused MdView.Tests and full
  Toolbox.sln suites on Debian/Windows and manually inspect
  a Markdown table via `mdview preview PATH`.

## 2026-11-14 — native Spectre table test compilation fixes (maintainer build)

- Maintainer's first full-solution test run after adding the native Spectre table viewer reported `CS0236` in two locations of the *test fixture* initializer (`MdViewSpectreTableViewerTests.cs:176`). The fixture's `Path` instance property shadowed `System.IO.Path`. Corrected with full type qualification, commit `ef6ee546b5513ef481cdd12ca4eb124a49531d14`.
- Also resolved a `xUnit2013` test analyzer warning (`MdViewDiskNavigationIndexTests.cs:95`) by using `Assert.Single(links)`, commit `35e34fdc07bdd8c9228d8d3e3d911a9881d37624`.
- **No MdView production implementation change** was required for either diagnostic. Other Toolbox test projects shown in the maintainer log passed; the native table viewer regression suite remains **not yet run successfully** because the test project did not compile.
- Next verification: targeted MdView tests, followed by full `Toolbox.sln` after `git pull`. Do not claim a fresh green state prematurely.

## 2026-11-14 — native Spectre table chunk status assertion corrected (206/207)

- After prior `CS0236` and `xUnit2013` test fixes, maintainer reran `MdView.Tests`: **206 passed, 1 failed, 207 total, 0 skipped**. The remaining case was a test-only capitalization mismatch: production displays `Data rows shown: 4 · rows visited: 8`, while the assertion expected uppercase `Rows visited: 8`.
- The regression now checks the real lowercase label and the four-row chunk count; commit `3088f73e1e64b134ee9ed93afba1102c25754f43`. Production `MdViewSpectreTableViewer`, verified source, paging index and table navigation are unchanged.
- **Re-run still pending:** focused `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj` then full `dotnet test Toolbox.sln`. No new all-green status asserted until results arrive.

## 2026-11-14 — maintainer-confirmed green native Spectre table baseline

- Maintainer confirms the native Spectre table tests now pass after fixing the `rows visited` assertion. The previously observed 206/207 failure is resolved; current precise test counts and any separate full-solution result were not supplied.
- This is the **green baseline before introducing render-segment height measurement**. No production changes are included in this checkpoint.

## 2026-11-14 — native Spectre table visual height measurement


- The maintainer explicitly confirmed the previous native Spectre table checkpoint **green** after the `rows visited` case-sensitive regression was fixed. This is the baseline for the changes below; no new test counts are inferred.
- Added `MdViewSpectreTableHeight` (commit `879283fbce05693f86beeb74cf3b04e5027688a3`, XML documentation adjustment `993f9d5c6c76437f6cc6f40f1946f77d35b661cb`). It uses the actual Spectre `IRenderable.GetSegments` pipeline, counts `Segment.IsLineBreak` and embedded line breaks, and caps measurements at 512 visual rows. `IRenderable.Measure` reports **width**, so it is not used to estimate vertical table height.
- Refactored the optional native `MdViewSpectreTableViewer` modal to trial bounded candidate tables before accepting each row/field (`75c3577dcf8f998100fe953270e7b111f2f6e9ce`). The next row/field remains pending when its actual rounded-border/header/wrapped-cell height would exceed the current terminal's content-row budget. This is a small, bounded extra render per candidate, not a full-document render. The previous hard limits of 6 data rows / 8 transpose fields still apply.
- Measured height is additionally capped for very tall profiles (`e946707035799cad82db6ee0f6ce50bd3bbff7be`). Modal grey header/status rows now clip to one terminal line, optional informational text is suppressed in smaller windows, and 7–8 terminal rows are reserved for control surface/prompt (`1425f9da346b62e0924042c3ac006aff999e85a9`). If a single native row cannot fit, the modal returns to the compact source-page layout instead of looping or corrupting navigation.
- Added 4 `MdViewSpectreTableHeightTests` (`d7f52e42c520afe6648410c234f699ca4e2473f9`), including narrow/wide cell wrapping, non-writing measurement, row budget and argument guards; added `WrappedNativeTableRowsFitMeasuredTerminalHeight` integration regression (`68e35628e0e668314362da5ac5b82b54cd69baa1`).
- Updated English documentation: `Docs/Native-Spectre-Tables/en.md`, `Docs/Responsive-Preview/en.md`, MdView README, and test README. The main `MdViewTerminalPageIndex` and compact source-page renderer **remain unchanged**; native embedding in the main page requires a separate bounded table-block-aware page index.
- **Fresh focused/full .NET test verification pending** (no SDK available in this environment). After `git pull`, run `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj` then `dotnet test Toolbox.sln`, and smoke-test long table cells in 28–40 column terminals. Do not call the new increment green until the maintainer confirms the results.
- Follow-up native-modal safety guard: `a86dc0c40237645e3a37b9bc3fe8d53025081f63` requires at least 28 columns **and 12 terminal rows**, and measures even a header-only native table before output. If its border/header alone cannot fit, control safely returns to compact preview without showing an unusable prompt.
- Tests `TooShortTerminalFallsBackWithoutOpeningNativePrompt` and `OversizedHeaderAloneDoesNotOverflowTinyNativeModal` added in `355ddc9f7655c512d89f3e1c21232271b0279f9b`. Total new regression cases for this measurement increment: 4 helper tests + 3 viewer integration/guard tests = **7**.
- English native-table guide and READMEs refreshed. **Post-change focused/full tests pending**.

## 2026-11-14 — bounded source-aware table blocks

- Maintainer reported **all tests green** after the preceding native
  Spectre geometry changes. Exact counts were not provided; do not
  substitute earlier status notes for that newer acceptance.
- Created `MdViewSpectreTableBlockReader.cs` with a validated
  two-line header/delimiter lookahead and source-bound
  `MdViewSpectreTableHeader` and `MdViewSpectreTableChunk`
  models. A header ending the current page can be checked against
  the next source delimiter without changing page boundaries.
- The new `ReadChunkAsync` reads 1–6 contiguous data rows
  from the disk-backed cursor, returns the exact unconsumed
  `NextDataLine`, probes only one more line for `HasMore`,
  and stops on fenced, quoted, mismatched or non-table content.
- Existing `MdViewSpectreTableViewer` delegates header discovery,
  header validation and row recognition to this shared reader;
  terminal UI, native height checks and compact fallback are
  intentionally preserved.
- New regressions in `MdViewSpectreTableBlockReaderTests.cs`
  (6 `[Fact]` cases); detailed English guide:
  `Docs/Native-Table-Blocks/en.md`. README and native guide updated.
- **Fresh tests for this increment not executed in the assistant
  environment (no dotnet SDK).** Run:
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`.
- **Next:** design the source-bound native table visual event stream
  and disk index with exact Spectre border/wrap heights and per-block
  compact fallback; only then enable automatic in-page native tables.
  Main `Page X of Y` continues to count compact rows.

## 2026-11-14 — native table tiny-header regression follow-up (219/220)

- Maintainer ran `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  on Debian .NET 10: **219 passed, 1 failed, 0 skipped, 220 total**.
- Failure: `MdViewSpectreTableViewerTests.OversizedHeaderAloneDoesNotOverflowTinyNativeModal`
  raised `System.InvalidOperationException: No input available` because
  an unbroken 120-character heading is truncated by the bounded native
  header and can fit in the five-row table budget at 64×12. The modal
  correctly prompts for a key; the test supplied none.
- The single overbroad fixture has been replaced by **two** regressions:
  (1) a long unbroken header *can* fit and is dismissed via Escape;
  (2) genuinely word-wrappable oversized headers *must* fail the
  measured height preflight and return to compact view without a prompt.
  The measured-height production guard was not weakened, and source
  Markdown is unchanged.
- Test commit: `a5f511165d1fafe0a94121cb23c3fb0ba9af4c4c`.
- **Post-fix verification pending.** Run focused MdView.Tests and
  `dotnet test Toolbox.sln` after `git pull`.
  Do not mark this checkpoint green until a new successful run.

## 2026-11-14 — measured native table page fragments (not yet enabled in main preview)

- Maintainer confirmed the previous tiny-header test correction and
  the complete local test run **all green**. Exact totals were not
  supplied for the latest green run; this is the verified baseline
  before the changes below.
- Extracted unchanged native rounded table/escaped heading/literal-cell
  construction to internal `MdViewSpectreTableFactory.cs`
  (commit `ab23bc0ffbf71d5ca4d333b663bf076d7221f37b`),
  and delegated the modal inspector's two existing helpers to it
  (commit `439232615df0cebc956429259663c0c43a1a2bfc`).
- Added `MdViewNativeTablePagePlanner.cs`
  (commit `368745065e2e6802bf3d93142a94f24bc3d26eeb`).
  It prepares a native wide-grid fragment for a validated
  source-table header and an exact source data-row start. The
  fragment contains at most six accepted physical source data rows,
  real Spectre-rendered height (borders, header, wrapped cells),
  the first unconsumed source line and a bounded HasMore probe.
- The planner does not render/write output, fetch network resources,
  or modify original Markdown. It supports an explicit
  zero-consumption compact fallback for a narrow terminal,
  header/first-row overflow, and invalid continuation. A repeated
  header is counted again on each future native page fragment.
- Added six `MdViewNativeTablePagePlannerTests` (commit
  `d6a1ac761bd549a2d5b72838ae4e10214be27d7a`), with a
  nullable assertion cleanup (commit
  `82606d3a93bd8621bc10293a1f8d56a97b873c9f`).
- English documentation: `Docs/Native-Table-Page-Planner/en.md`;
  MdView README and source/native table guides updated.
- **Fresh tests for this increment pending on the maintainer's
  .NET installation.** Run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`, then smoke-test
  `mdview preview README.md`. No new green test claim is made.
- **Next:** implement an on-disk visual block-event page index and
  renderer that both use this planner's source continuation,
  exact native visual height and compact fallback decisions.
  Only then expose optional embedded Spectre tables. The existing
  compact `Page X of Y` remains untouched and the default.

## 2026-11-15 — single-table disk-backed native fragment index

- Maintainer confirmed that the **previous measured native table page
  planner checkpoint is fully green** (focused and full tests), without
  exact test totals. This acceptance predates the new files below.
- Added `MdViewNativeTableFragmentIndex.cs`, commit
  `078a4163e1058c6a24256c02c821e1d2e6ea5af7`.
  It builds one source-bound native table index with fixed **28-byte
  disk records** (first/next physical data source line, data row count,
  actual Spectre visual height, continuation flag). It performs
  random 28-byte reads and a binary search for an original source
  line, mapping the header/delimiter to fragment zero.
- Uses bounded `MdViewNativeTablePagePlanner` on each chunk
  (at most six data rows + one lookahead), with a 64 MiB default
  index budget, DeleteOnClose temporary storage, validated header
  preflight, and a whole-table compact fallback if any fragment
  cannot safely fit. Source snapshot/cursor remain caller-owned.
- Added `MdViewNativeTableFragmentRenderer.cs`, commit
  `594f06305c71118732cd5a837f4354ce02e82b2b`.
  It reconstructs only the selected native widget and verifies
  indexed source resume, rendered height and continuation before
  writing. Added source cursor identity and canonical header
  checks (commits `3aa00095372888f15eb9d7136c1e733d2a028833`
  and `3204d496ec8de486e7c1f3c32aec214516e6b702`);
  viewport width/height/Unicode must match the index. The
  content-row budget cannot exceed the physical terminal height.
- Added **12 `MdViewNativeTableFragmentIndexTests`**,
  beginning with commit `83a094933e203d3809482039c3a4fd1c9e5aa019`,
  including a regression for a late oversized row that rolls back
  all previously stored native records (commit
  `d1baf7087756de98b38e62800d0b4f8a50e317fb`),
  a corrected 0-based second-fragment resume assertion
  (`66ed6734c436dd667e9537b35e67db00de9cec44`)
  and substituted-source regression
  (`e5840e94c2b46dff0350db290f8250f8ba443de9`).
- English documentation:
  `Docs/Native-Table-Fragment-Index/en.md`;
  project README, test README, root README and page planner guide
  now link this single-table milestone.
- **Fresh post-change .NET verification pending** (SDK unavailable
  in this assistant environment). Run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`; keep last green checkpoint distinct.
- **Next milestone:** mixed Markdown visual-block event stream/index
  for ordinary prose, source-bound native table fragments and compact
  fallbacks, using a *remaining page content-row* budget. Only
  then offer optional inline native tables in `mdview preview`.
  Existing default `MdViewTerminalPageIndex`, renderer, and modal
  table viewer are unchanged.

## 2026-11-15 — first source-aware mixed Markdown/native visual pages

- Maintainer confirmed the preceding **single-table native fragment disk
  index and renderer tests all green**, including the full solution.
  Exact totals were not supplied; this is the accepted baseline.
- Added `MdViewMixedVisualPagePlanner.cs`, commit
  `e655cbfae7365200f63cc4c180abfcfc3646e8d4`. The planner uses
  the validated `MdViewContainerPageCursor` and the existing
  `MdViewTerminalLineLayout` to combine ordinary styled Markdown
  rows with **complete small native Spectre tables** on one bounded
  visual page. The planned page includes exact source/wrap resume
  coordinates and a compact-table header/column continuation state.
- Deliberate safe rule: only confirmed wide tables with **0–6 data
  rows**, all fitting a fresh page, can be native. A native table
  that does not fit the *remaining* page space is deferred intact
  to the next page. Tables with more than six rows, narrow
  layouts or over-height native widgets stay compact for their
  entire source block, including through page breaks. Ordinary
  Markdown preserves existing styled row counts.
- Added `MdViewMixedVisualPageRenderer.cs` (commit
  `23e291d311f09a4bfe26f503f0f2218c9c13049b`).
  It checks terminal width/height/Unicode, total block rows and
  actual native Spectre rendered height **before any output**,
  and prints text spans using the established Markdown style
  resolver. That resolver is now `internal` and XML documented
  (commit `2e1b1b2faf2e862b37ae377e1b76108fedb04589`).
- Avoided two-line disk-journal header probing for ordinary prose
  by checking the source-line's possible pipe prefix without
  materializing a trimmed long string (commit
  `8cbc254c70df633aa1081a17f855298f897862db`).
- Added **12 `MdViewMixedVisualPageTests`** (commits
  `e18455a986cf0993ee20813fde2632fdb4dbc906`,
  `3904f213b32bbbae8a02ea2d25957bc683c86ab1`).
  They cover compact/native source progression, height/page
  boundaries, fenced and indented literal code pipes, old compact
  page-start parity,
  narrow terminals, resize and source/cancellation guards.
- English documentation:
  `Docs/Mixed-Visual-Page-Planning/en.md`. Updated MdView README,
  MdView.Tests README, root README, and previous table index guide.
- **Fresh post-change .NET tests pending**. Run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`; no new green count is claimed
  without the maintainer's run.
- This stage **does not change the default `mdview preview PATH`**
  or `MdViewTerminalPageIndex`. Next: create a disk-backed
  **mixed-document page-start index** using this shared planner,
  with source-line jumps, geometry rebuilds and rendering parity
  before offering optional inline native tables in the UI.

- Additional conservative native table guard: `MdViewSpectreTableBlockReader`
  rejects four-space-indented literal code both for the header pair
  and for data-row continuation; the mixed planner skips the table
  probe for literal code (commits `71f5b66dcf5656f7306acb9c00bab16f780697f0`,
  `4de37faae36c3f0d6f4ef3abf8aa2843b8b68dc7`). A seventh
  `MdViewSpectreTableBlockReaderTests` case and an eleventh mixed-page
  case verify this behavior. These are **unexecuted** here.

- A further text-only parity regression compares the new mixed renderer's
  actual terminal output with the established compact renderer
  byte-for-byte, including headings, prose, links, lists and fenced code
  (`a2abec622156ae998c72ed1d330b42f30442b163`, explicit theme argument
  in `6a45794e5224ca08ea4e84f4a33198c248361537`). **Pending execution**.

## 2026-11-15 — disk-backed complete mixed-document visual page index

- Maintainer confirmed **all-green tests** for the preceding mixed
  Markdown/native single-page planner checkpoint. Exact counts
  were not supplied. This is the green baseline *before* this
  whole-document indexing increment.
- Added `MdViewMixedVisualPageIndex.cs` (commit
  `c23bf21e9773ced7acc53d1b7fb35c7b3e21d578`).
  It repeatedly consumes the existing mixed visual planner and
  stores each canonical page as a fixed **56-byte little-endian
  temporary disk record**: source line/wrap row, compact table
  continuation state at both ends, visual row count and
  end/clipped-code flags. Default index budget 64 MiB.
- `GetPageAsync` performs a single fixed record read.
  `FindPageAsync` uses a disk binary search for physical
  source line or within-line visual row; ordinary text and
  source lines inside native/compact tables map to their
  canonical page. No whole-document page-start list is
  held in RAM.
- Added `MdViewMixedVisualIndexedRenderer.cs` (commit
  `a4d992073672c9d2620b93277de87abd6611aca1`).
  It enforces the **original validated cursor**, stable
  terminal width/height/Unicode, and exact planner/index
  `Start`, `Next`, visual row count, EOF/clipped-code
  parity before printing. The existing mixed renderer
  additionally preflights native Spectre widget heights.
- Improved index invariants and resize guard (commit
  `37a3250f1511ff8b6e3fb8be481174fda4f13585`):
  stable Spectre profile during indexing, monotonic page
  progression, a single empty page for empty files,
  no phantom page after exact EOF, validated final disk
  flags/positions and explicit budget rejection.
- Added **11 `MdViewMixedVisualPageIndexTests`** in commit
  `495f75f6eabd3d10912fe63c960e656e4638916e`.
  Covers native tables among Markdown, long compact tables,
  source/wrap-row search, header/delimiter page boundaries,
  two native tables, empty/exact final pages, source/viewport
  guard, budget failure, argument/disposal validation and
  cancellation.
- Updated root/project/test README, previous mixed page
  design guide and the new English
  `Docs/Mixed-Visual-Page-Index/en.md`.
- **Fresh .NET focused/full test verification pending.**
  Run `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  then `dotnet test Toolbox.sln`. No new green counts
  claimed; this assistant environment has no .NET SDK.
- **Next:** opt-in terminal UI for indexed mixed pages
  (Page X of Y, next/back and canonical source jumps),
  with resize-triggered index rebuild and unchanged
  compact default. Source line jumps into tables must
  resolve via `FindPageAsync` rather than forming
  unvalidated custom source starts.

## 2026-11-15 — opt-in mixed native preview CLI and xUnit2020 warning cleanup

- **Maintainer-confirmed green baseline:** the preceding
  document-wide mixed visual page-index work passed **263/263**
  MdView tests, **0 failed / 0 skipped** on .NET 10. One
  analyzer warning remained: `xUnit2020` in
  `MdViewMixedVisualPageTests`.
- Corrected warning without changing its test semantics:
  `Assert.True(false, message)` → `Assert.Fail(message)`.
  Commit `7f1bc4bf1388026588d728c25877f4ce34eed94c`.
- Added **`MdViewMixedPreviewPager.cs`** (commit
  `84d5d73474c903cadfba5ffbbd6aa5f08a2d8925`).
  It uses the validated UTF-8 snapshot, source cursor,
  `MdViewMixedVisualPageIndex` and
  `MdViewMixedVisualIndexedRenderer` for an interactive
  explicitly selected mixed Markdown/native Spectre preview.
- CLI: new `MdViewSourceKind.MixedPreview`,
  case-insensitive `mdview preview-native PATH` parsing
  (`49adadc08816cff0cefe542e0e681389dec7c215`),
  Program dispatch and help
  (`43ab78fa777ecad5ab8bbd2f96f23a87fc2505f1`).
  The existing `mdview preview PATH` and source pager
  **remain unchanged**. The session returns without
  a duplicate completion prompt.
- UI features: canonical `Page X of Y`,
  Next/Previous by disk index, one-based physical-line jump
  (including lines inside native tables), lazy disk-indexed
  heading/link location selection, code-clipping indicator,
  and explicit Escape/Exit. A resize rebuilds page
  boundaries from the current physical source-line anchor.
- Safety: minimum terminal size 28×18; 16 rows always
  reserved for headers/status/prompts, including when the
  caller supplies a large `linesPerPage` value
  (`f99563b2a8e49cb1444bfe28db8ca6e18ac1bc64`).
  A small/unsupported viewport returns without opening
  the source; source files remain read-only. Native grids
  are limited to complete small tables; long/over-height
  source tables remain compact.
- Tests: a new CLI parsing test
  (`b515f5f4465ee99138ca8b2714c50bd8c2268d06`)
  and **13 `MdViewMixedPreviewPagerTests`** (starting
  `e29e8fb3c405db6bb576b64f213d000cc9da063f`,
  additional navigation/viewport checks in
  `c47a7b74c82f546756bc511d70c465fe8602c28e`).
  Covers native table between prose, navigation/jumps,
  heading/link location, compact fallback, empty source,
  invalid input, tiny/noninteractive terminals, and
  unsafe content. No tests claim to have run here.
- English guide:
  `Docs/Mixed-Native-Preview/en.md`; README and mixed
  page index guide updated. Project/root/test statuses
  updated for continuity.
- **Fresh focused/full .NET tests pending.** Verify with:
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`,
  `dotnet test Toolbox.sln`, then smoke-test
  `dotnet run --project Blackboard/Tools/MdView --
  preview-native README.md`. The previous 263/263 is
  **not** a verification of this new UI.
- **Next:** respond to compiler/analyzer/test feedback
  first; if green, improve UI consistency and validate
  terminal resize and long-file/low-RAM behavior before
  considering a broader native table block model.

## 2026-11-15 — optional reading-width cap for native mixed preview

- User visually inspected `mdview preview-native` in three Konsole
  window sizes. The mixed index and source-page ranges updated with
  the viewport, from 13 to 8 pages on a wider display. No fresh
  complete .NET test transcript accompanied these screenshots.
- Based on the user's agreement to improve readability on large
  monitors, added `MdViewMixedContentWidth.cs` (commit
  `16d6c207acb55e3d8cbc62d47e03f422df699c31`).
  This documents and enforces an explicit optional
  **40–4096 content-column** maximum. Zero means automatic
  physical-terminal width. The console's actual profile is
  never modified.
- Native table measurement now uses the **same effective
  width** as Markdown text wrapping and compact table cells.
  `MdViewNativeTablePagePlanner` accepts
  `maximumContentColumns` as an optional trailing argument
  (commit `1026c812c86fd32ff023447f2917c95add9447ec`).
  Mixed-page planning passes the cap through to the native
  table factory and appends it to the page metadata
  (`fb51ec69b377251e9cfdfefc2eebb2d2bd56f2eb`).
- `MdViewMixedVisualPageIndex` retains
  `MaximumContentColumns` in **instance metadata**, preserving
  its existing 56-byte page-record disk format
  (`97b0f2a40ed6dd537a984bff240bd843c15a0017`).
  Indexed rendering replans using the same limit before
  verifying source position and visual geometry
  (`dbf02568b60e0737f64f3a5d887226fe01da6047`).
- The native mixed pager accepts
  `RunAsync(..., maximumContentColumns: N)` and displays
  `max N columns` in its header
  (`cae2f9e450389236f34e22b23b385e0e33f4c6ba`).
  CLI parsing now supports
  **`mdview preview-native PATH --max-width N`**, case
  insensitive with strict integer/range validation, while
  leaving the record's original three constructor parameters
  and existing `preview PATH` syntax intact
  (`333b1c9825fadebfcd4b8c99a82d7a585aa6c25a`,
  `9359fd7b2cc272ff6f3a3a45efe1ccfd93f0675c`).
- Added `MdViewMixedContentWidthTests.cs` (commit
  `67b7ede97ddcce31be8bcdb2cfc113ec0c9e715a`)
  with parameterized physical/effective width policies,
  invalid input/CLI cases, page-count/source-index parity,
  bounded prose rows, native table measurement and fallback,
  unchanged original source/terminal profile and resize
  rejection. An additional parameterized case checks actual
  widths 40, 63, 64, 80, 120 and 220 and native table
  deferral at visual page boundaries
  (`ae57a117c4ac2c19a3b303be2fb3a2ee14c2d9a8`).
  Made the post-table prose expectation independent of
  native border height (`fb943c124b666eb2901ae3ee8552aac24a8cb039`).
- English document `Docs/Mixed-Content-Width/en.md`,
  root/MdView/test READMEs and mixed preview/index guides
  have been updated.
- **Fresh focused/full .NET tests for the width-cap change
  remain pending.** The assistant environment has no .NET SDK.
  Run `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`, then manually compare
  `preview-native README.md` with
  `preview-native README.md --max-width 100`.
- **Next:** resolve any test/analyzer findings, smoke-test
  native tables and source jumps on narrow/wide terminals,
  then consider optional text centering while keeping
  exact disk-index layout parity and compact default.
- Follow-up fixture hardening: a canonical page beginning inside a
  long soft-wrapped source line is not required to repeat the
  `Line N:` prefix. The constrained-page regression now checks
  visible wrapped prose instead (commit
  `a03ce6d78d83276453446e748ff991de45765556`).
  No production behavior was altered by this test cleanup.

## 2026-11-15 — optional centered reading column for mixed native Markdown

- Maintainer confirmed the preceding optional mixed-preview
  content-width work and entire local test run **all green**
  (exact counts not provided). This is the accepted
  checkpoint **before** the centering increment.
- Added `MdViewMixedContentAlignment.cs` (commit
  `8971364414a3b9273a841f0b9d981a06faaea0e9`).
  It computes the centered left gutter from the actual
  physical terminal width and existing
  `MdViewMixedContentWidth.EffectiveTerminalWidth`.
  It never changes the terminal profile. Native Spectre
  grids are wrapped in a `Padder` with horizontal left
  indentation and **zero vertical padding**.
- Extended `MdViewMixedVisualPageRenderer.Render` with
  optional `centerContent` (commit
  `7cec4c2d89108f0535e3c56a7d5cd78287c888a3`).
  Every source-styled Markdown display row gets the same
  left margin; each native widget is padded with that
  margin. The renderer measures **padded** Spectre table
  segments before writing a single output row and rejects
  any change in planned native table height. Source
  styling, width-based wrapping and canonical page
  boundaries otherwise remain unchanged.
- Extended `MdViewMixedVisualIndexedRenderer` with
  trailing optional `centerContent` (commit
  `30bda58de9dde35b95f1a6057f8bcb250202797f`).
  Centering is output-only and does **not** change the
  disk-backed mixed page index or its existing 56-byte
  record format. Source-line jumps remain canonical.
- `MdViewMixedPreviewPager` accepts a trailing
  `centerContent` flag and displays `centered` in
  the UI header (commit
  `0c390eb7f531940b59463a3ce3eaccc949947a22`).
  The default is false; centering alone without a
  content-width cap has no visible effect on a
  full-width reading area.
- CLI: `MdViewCommand.CenterContent` and case-insensitive
  `mdview preview-native PATH --max-width 100 --center`
  in **either flag order** (commit
  `23e0753af62ee90e54db6f2b7b709f0465abc0d8`).
  The parser refuses duplicate/unknown options. Program
  forwards the flag and documents its usage
  (`0ddd8ffe4776f4f2c9bea350ca960be6dcceeed8`).
  Existing command kinds, left-aligned default, compact
  `preview PATH`, source pager and remote readers unchanged.
- Added 8 test methods with parameterized cases in
  `MdViewMixedContentAlignmentTests.cs` (commit
  `7c8d235885409d06f5c32e30a667f819b91cc0f9`,
  deterministic invalid-option cases in
  `3dfacc8903b0b6afaa20edd375621770b4706368`).
  The tests check deterministic gutters, native widget
  row-height parity, same text/native left gutter,
  literal-safe source bytes, index/renderer canonical
  page equality, default output without a width cap,
  CLI parsing/invalid options and visible preview status.
- Documentation: new English
  `Docs/Mixed-Content-Alignment/en.md`, plus MdView
  README, test README, main Toolbox README and cross-links
  from mixed-width and mixed-native guides.
- **Fresh focused/full .NET tests pending.** The assistant
  environment has no .NET SDK. Run:
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`,
  `dotnet test Toolbox.sln`, then compare
  `dotnet run --project Blackboard/Tools/MdView --
  preview-native README.md --max-width 100` with
  the same command plus `--center`. Check page
  counts and native borders across different window sizes.
- **Next:** resolve compiler/analyzer/test feedback,
  and only after green verification consider preserving
  centered column alignment in other optional reader
  modes or improving very-long native table behavior.

## 2026-11-15 — centered-column test compilation correction

- Maintainer's .NET 10 build of the new centered-column suite found
  **two CS1503 errors** at lines 36–37 and **one CS0619 obsolete
  xUnit assertion error** at line 40 of
  `MdViewMixedContentAlignmentTests.cs`. Compilation stopped before
  a new test result; the previous maintainer-confirmed green
  checkpoint still applies to the earlier width-cap stage.
- Fixed only the test fixture (commit
  `59dc31b6c3199a368b46adb900d20d87c15a93fc`).
  The argument-validation test now constructs a plain
  `Spectre.Console.Table` without unnecessary chained string
  column extensions. Two synchronous
  `Assert.Throws<T>` calls now use explicit block-bodied
  `Action` lambdas, avoiding the obsolete async overload
  (`Assert.Throws<T>(Func<Task>)`).
- No production code or CLI behavior changed.
- **Fresh MdView.Tests and full Toolbox.sln builds still pending.**
  The assistant environment has no .NET SDK. Run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`, then smoke-test the centered
  native preview.

## 2026-11-15 — unified whole-screen centered mixed preview

- User-provided Konsole screenshot demonstrated that
  `preview-native README.md --max-width 100 --center`
  correctly centered the Markdown body but left the application
  title/file header, `Page X of Y` and keyboard selection menu
  at physical terminal column zero. The user requested that
  **the entire screen share the same centered column**.
- Added `MdViewIndentedPromptConsole.cs` (commit
  `cff07f5b4388565085ff2626d2c1130a737cc916`).
  The internal `IAnsiConsole` proxy forwards the original
  `Profile`, `Input`, `Cursor`, `ExclusivityMode`,
  `Pipeline`, `Clear` and `WriteAnsi` operations.
  Only complete interactive `SelectionPrompt` renderables
  are wrapped in Spectre's native horizontal-only `Padder`.
  No fake terminal profile, new key parser, or vertical padding.
- Updated `MdViewMixedPreviewPager.cs` (commit
  `bbfe9b4cdcd40e55adda9757d11e33ad678ba341`):
  calculate one `MdViewMixedContentAlignment.LeftGutter`
  per viewport; apply it to the file/title header, status,
  source line range, feedback, code clipping, empty label,
  and interactive page and disk-indexed heading/link menus.
  The document renderer was already centered and is **not**
  padded a second time.
- The incremental `TextPrompt` for source-line input uses
  the **original console** with a single indented title.
  It is deliberately excluded from the prompt proxy to
  prevent repeated indentation of typed characters.
  Short chrome strings clip to the same capped reading width.
- **Default behavior remains unchanged** when `--center`
  is omitted or no horizontal free space exists.
  The existing compact `preview PATH`, source indexing,
  canonical page positions, 56-byte record schema,
  navigation semantics and resize rebuild remain intact.
- Added **9 `MdViewMixedScreenAlignmentTests`**
  (initial commit
  `1dc77f20c27f3f5db90f5d5f479c7432d4953cfb`,
  terminal newline hardening
  `b2836a02d8b7caa0b1da1fd72e8fb5d6596a7f36`,
  additional link/no-results regressions
  `84b694f214f19702585100813ec063f79f2f6746`).
  Tests check aligned header/body/footer/menu and selected
  arrow; next/previous; source-line input; heading/link
  pickers; no-result message; empty source and default/no-spare
  terminal alignment.
- Updated root/MdView/test README and English
  `Docs/Mixed-Content-Alignment/en.md`,
  `Docs/Mixed-Native-Preview/en.md`,
  `Docs/Mixed-Content-Width/en.md`. Outdated descriptions
  of left-aligned chrome under `--center` were corrected.
- **Fresh .NET tests pending** in the maintainer's environment.
  No current green checkpoint has been asserted for this
  correction; the assistant has no .NET SDK. Run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`, then smoke-test
  `dotnet run --project Blackboard/Tools/MdView --
  preview-native README.md --max-width 100 --center`.
  Verify the whole screen's horizontal alignment and
  arrow-key/Enter/Escape navigation on both narrow
  and wide terminals.
- **Next:** resolve any compiler/analyzer or interactive
  prompt rendering feedback first. After verified green,
  continue with long native table pagination, preserving
  low-RAM disk-backed source and visual indexes.

## 2026-11-15 — mixed screen alignment test fixture compilation fix

- Maintainer's .NET 10 build confirms the production `MdView` project builds successfully. `MdView.Tests` failed with **CS0236** (twice, line 226) and **CS1061** (line 232) in the new `MdViewMixedScreenAlignmentTests.Fixture`.
- Root cause: the fixture property `Path` shadowed the static `System.IO.Path` class used in field initialization and file-path construction. Changed both `Path.Combine` calls and `Path.GetTempPath` to explicit `System.IO.Path` references (commit `6463ac5e56fca90b1207916e7bd8cc5a6e9758b6`).
- No production source or test intent changed. **Fresh MdView.Tests and full-solution results still pending** after this focused compiler repair.

## 2026-11-15 — live Spectre prompt centering fix after 8 failed tests

- Maintainer tested the earlier centered-screen implementation:
  **333 MdView.Tests total, 325 passed, 8 failed, 0 skipped**.
  All eight failures were in
  `MdViewMixedScreenAlignmentTests`. Konsole also showed
  the entire header and Markdown correctly centered but
  the bottom live navigation menu still at column zero.
  No green result exists yet for this replacement.
- Investigation into Spectre.Console sources confirmed
  `SelectionPrompt` delegates to `ListPrompt` and attaches
  `ListPromptRenderHook`, which emits the live menu
  *after* the initial `IAnsiConsole.Write`. The original
  `MdViewIndentedPromptConsole` wrapped the initial write
  and therefore failed to position the actual live widget.
- Added `MdViewCenteredPrompt.cs` (commit
  `f1f5d55af783ce65e848c9c838752aee67996d75`):
  `MdViewCenteredPromptRenderHook : IRenderHook` plus
  `MdViewCenteredPrompt.ShowAsync<T>`. The hook is attached
  before the native prompt and adds zero-vertical horizontal
  `Padder` positioning to live renderables **after**
  Spectre's own prompt hook runs. `ControlCode` objects
  are passed through unchanged to preserve cursor movement,
  refresh, Enter, Escape and arrow keys. The hook scope
  is disposed after the prompt returns.
- Updated the mixed pager (commit
  `da3d487e0f94f18a861006edd816e8bd78133377`)
  to use the new helper for its main menu and pass the
  same gutter to the disk navigation picker. Updated
  `MdViewDiskNavigationPicker` (commit
  `1d6b7eebe12ad2c082d002fd22719aa021f78fa2`)
  to use it for both heading/link lists and indent
  empty-result feedback. Disk-indexed labels are now
  clipped to the centered reading width (commit
  `3351e554d3bdc77506b584249a255b51055cc459`).
  Source-line free-text `TextPrompt` still receives
  a **single prefixed title** on the original console,
  avoiding per-character indentation.
- Removed unused/ineffective
  `MdViewIndentedPromptConsole.cs` (commit
  `49833321a72ddf030b2bd405700f5301740024cb`).
  No source parser, native table planner, main indexed
  renderer, 56-byte page record or compact default changed.
- Corrected the test harness (commit
  `bebf9629663de3e85666616003d12935c9d158f4`):
  enable `TestConsole.EmitAnsiSequences` so the
  actual Spectre render pipeline and live hooks run;
  remove ANSI control sequences for visible-text checks.
  Added direct pipeline positioning/validation tests
  and a scoped-prompt lifetime test (commit
  `ec924d940cce1597d9bd1c9aa1d8c8e7c9da66fe`).
  The previously failing alignment cases remain in the
  suite; none have been weakened solely to suppress failures.
- Updated MdView/root/test README and English mixed
  native preview/alignment guides, explicitly describing
  why the prior proxy failed and how the live hook fixes it.
- **Fresh focused/full .NET verification pending.**
  The assistant environment has no .NET SDK. Run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`,
  `dotnet test Toolbox.sln` and visually retry
  `dotnet run --project Blackboard/Tools/MdView --
  preview-native README.md --max-width 100 --center`.
  Confirm native bottom menu title, arrow and options
  share the document gutter, and heading/link pickers
  still work with Enter/Escape. Next respond to any
  remaining test or UI evidence before long-table work.
- Follow-up test hardening (commit
  `09ae5e0d4b2c138780c18c309531434d8d1e4e20`):
  the scoped-prompt test checks the **actual Spectre render
  pipeline** contains no remaining gutter hook after the
  prompt exits. It no longer infers the post-prompt
  cursor location from a flattened ANSI transcript
  containing earlier erased/redrawn prompt frames.
  The output text is still checked for visibility.

## 2026-11-15 — live menu visually aligned; test internals access fixed

- User's new Konsole screenshot confirms the whole `preview-native
  README.md --max-width 100 --center` screen is aligned: title,
  rendered Markdown, `Page X of Y` and interactive navigation
  share the same centered reading column. The scoped live Spectre
  render hook is therefore **visually confirmed working**.
- The same latest `.NET 10` run successfully compiled the
  production `MdView` assembly but stopped before executing
  `MdView.Tests` with **three CS0122** diagnostics at lines
  226, 246 and 273 of `MdViewMixedScreenAlignmentTests.cs`.
  These refer to the two purposely `internal` implementation
  helpers `MdViewCenteredPromptRenderHook` and
  `MdViewCenteredPrompt`.
- Added `Properties/AssemblyInfo.cs` with
  `[assembly: InternalsVisibleTo("MdView.Tests")]`
  (commit `75e232b3ab65a0de006bb7ae0810ba5139a94fea`).
  This follows existing repository conventions
  (`WhenItFails/Properties/AssemblyInfo.cs`), keeps
  production helper classes **internal** for consumers,
  and grants only the named test assembly access.
- No changes to live prompt behavior, navigation, source
  indexing, renderer or 56-byte disk page records.
- **Post-fix .NET build and test results pending.**
  Run `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`. The earlier 325/333
  run and current CS0122-blocked build are historical,
  not outcomes for this new compiler fix.

## 2026-11-15 — two remaining centered-screen failures after 334/336 passing

- Maintainer's latest focused .NET 10 run: **336 total, 334 passed,
  2 failed, 0 skipped**, with the production MdView and test assembly
  building successfully. The previous three CS0122 accessibility
  diagnostics are resolved. Real Konsole screenshot already confirms
  the main centered interface and native selection menu are aligned.
- `CenteredSourceLinePromptIndentsTitleButNotEachTypedCharacter`
  failed at its title-gutter assertion (test line 80).
  Verified against Spectre.Console's `TextPrompt.WritePrompt`:
  it calls `builder.ToString().Trim()`, intentionally removing
  leading spaces from the markup title. The pager now emits the
  literal gutter as a separate `Text` write, **without newline**,
  immediately before invoking the native incremental `TextPrompt`;
  the title itself contains no leading spaces. This keeps the
  typed digits on the same line and avoids per-character padding.
  Production commit `c6465f4ffe63af30ddbcfee590733d899eb4b117`.
- `CenteredEmptyDocumentAndExitAreInsideReadingColumn`
  failed at test line 176, expecting the bare `Exit mixed preview`
  label immediately after the gutter. The menu contains only this
  one item, which Spectre selects automatically and prefixes with
  its native `> ` arrow. Updated the test to assert the **entire**
  selected item, `> Exit mixed preview` (commit
  `b8e66c66a149acebc01da6cb33ac1aed369e8ca3`).
- Existing two regression tests are retained, not skipped; other
  334 passing tests were not modified. No changes to page indexes,
  Markdown styling, compact mode or native table geometry.
- **New focused and full-solution test reruns pending** after these
  two targeted corrections. Next: if green, finish centered viewer
  checkpoint, then proceed with long native-table continuation.

## 2026-11-15 — maintainer confirms green after centered-screen corrections

- Maintainer reports **approximately 7,000 tests green** after the
  final centered text-entry and selected-exit regressions were fixed.
  Treat this as a user-confirmed green checkpoint; the latest raw
  test log and per-project totals were not supplied in this message.
- Prior failing MdView screen alignment cases concerned Spectre's
  trimmed TextPrompt title and the native selection marker on an
  empty document. The production prompt gutter fix
  (`c6465f4ffe63af30ddbcfee590733d899eb4b117`) and assertion
  fix (`b8e66c66a149acebc01da6cb33ac1aed369e8ca3`) are
  included in the confirmed current checkout.
- The previous Konsole screenshot also visually confirmed the
  centered Markdown content, header, status and live menu share one
  horizontal reading column. Default compact mode remains unchanged.
- **Next implementation milestone:** long native Spectre tables
  with bounded streaming chunks, repeated headers, measured row
  geometry and canonical disk-indexed Page X of Y navigation.
  Preserve low RAM use, source jumps, resize rebuild and existing
  compact fallback where a native fragment cannot fit.

## 2026-11-15 — opt-in long native Spectre table continuation

- **Previous green checkpoint:** maintainer reported approximately
  **7,000 Toolbox tests green** after centered live-menu fixes.
  No per-project test counts were provided. The new long-native-table
  changes below have **not yet been tested with .NET** in the
  assistant environment, which does not have the SDK.
- Added `MdViewLongNativeTableEligibility.cs` (commit
  `ab6d0fddb9376a6092cc201f9df8536c9789ef85`):
  bounded, source-complete streaming preflight for all fragments of
  a validated pipe table. It measures at most **2,048 fragments**,
  with at most six physical source data rows per fragment,
  and chooses entire-table compact fallback if any fragment fails
  or the limit is reached. It retains no accumulated native
  widget array or complete source table in RAM. An internal
  restricted `maximumFragments` argument supports bounded
  regression tests without changing the default policy
  (`4dfb1a2cdb4f8daa547bc04f99de6c3fb950388f`).
- Extended `MdViewMixedVisualPagePlanner.cs` (commit
  `e90c246277add7930b5fb269d601f723eacef145`;
  validation correction
  `303ce0db45a462324095d9b4fde08f4a65c0023b`):
  when the optional `longNativeTables` flag is enabled, a
  source-complete long table can emit consecutive measured,
  repeated-header native fragments. The first native fragment
  defers before its header when the remainder of the current
  page is insufficient; subsequent fragments start on fresh
  visual pages. Data-row source positions advance exactly.
  Invalid/malformed continuation state is rejected.
- Kept the established **56-byte** mixed disk page record
  format. `MdViewMixedVisualPosition.NativeContinuation`
  uses `CompactHeaderLine = original source header` and
  `CompactColumns = -1` with `WrapRow = 0`; compact
  state continues using positive column counts.
  `MdViewMixedVisualPageIndex.LongNativeTables` records the
  opt-in flag as instance metadata and validates the native
  sentinel only when enabled (commit
  `85c957007e716778c525904ec10ef70b6ee4c91d`).
  The indexed renderer re-plans using the exact same mode
  (`dcbbd8519ca03b7319f7177ca300f8d21fa6f48d`).
- CLI parser, Program dispatch/help and pager now support
  **`mdview preview-native PATH --long-tables`**, optionally
  combined in any order with `--max-width N` and
  `--center` (commits
  `b33630e70cb3d687557c7609da9f616aac6cb223`,
  `6a9b4d20d18f032a0966635407a911cebc9e5f82`,
  `1a70d9c332f4efa64a4c7592bc48a16bd74f749d`).
  The existing compact `mdview preview PATH` and
  non-opted-in `preview-native PATH` remain unchanged.
- Added **13 test methods** in
  `MdViewLongNativeMixedPagingTests.cs` (initial commit
  `a1ca25d0f46e84f4bf351c2f8ad80893a227fd79`,
  all-or-nothing scan limit and fresh-page deferral tests
  `cbc0d03b7690627739e3cff30c5c2b6cb1b399d9`).
  Covers exact-once source rows and repeated table headings
  across pages, source jumps, exactly filled EOF, original
  compact fallback, narrow cap, centered output, resize,
  invalid continuation, cancellation, bounded preflight,
  header deferral, CLI and interactive read-only source.
- Documentation: new English
  `Docs/Long-Native-Tables/en.md`; root/project/test README,
  mixed native preview, mixed visual page index and
  standalone fragment-index guides updated.
- **Verification pending**: run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`,
  `dotnet test Toolbox.sln`, then smoke-test
  `dotnet run --project Blackboard/Tools/MdView --
  preview-native README.md --max-width 100 --center --long-tables`
  against a Markdown document containing at least 14 data
  rows. Confirm every continuation page repeats the header
  and source-line jumps land in the correct fragment.
- **Next:** respond immediately to compile/analyzer/functional
  test feedback. After a new green checkpoint, consider
  native long-table caching/index throughput and very
  large low-memory workloads. Do not alter compact defaults.

## 2026-11-15 — long native preflight layout and resize final hardening

- Fixed the order of native long-table promotion in
  `MdViewMixedVisualPagePlanner` (commit
  `cca3a455cfe7f438006ec78238435d2830b17960`):
  first confirm **whole-table native eligibility**, then defer the
  native header to a fresh page if needed. Tables that ultimately
  require compact fallback now retain normal compact line flow
  rather than prematurely ending the previous prose page.
- Source-state validation now rejects native continuation headers
  too close to `long.MaxValue` for safe `header + 2` arithmetic.
- A terminal width, height or Unicode capability change **during
  planning**, not only between pages, now fails explicitly before
  recording inconsistent mixed visual geometry. Both guards are
  active in default and opt-in modes.
- These post-test hardening changes are committed but have
  **not yet been executed under .NET**. Previous maintainer green
  report still refers to the pre-long-table checkout.

## 2026-11-15 — CS1026 compiler correction for long native tables

- Maintainer's .NET 10 build reported **CS1026 (expected ')')**
  in `MdViewMixedVisualPagePlanner.cs` at line 136 before the
  new long native table tests could run.
- Verified the exact current GitHub source and corrected the
  native continuation validation's missing final closing
  parenthesis (`!longNativeTables)))`) in commit
  `66a826192fa6aa1988088a89162c08b9712239f6`.
  No behavior, source indexing, CLI flags or fallback
  policy was changed.
- **Fresh MdView.Tests and full solution results pending.**
  After `git pull`, run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`. Resolve any new compiler
  or test errors before calling the long-table milestone green.

## 2026-11-15 — HDD-oriented bounded native table journal batching

- **Preceding checkpoint:** maintainer reported all green after
  the `--long-tables` validation-parenthesis fix. Exact updated
  focused/full test totals were not supplied in that message.
  This new optimization has **not** yet been built with .NET
  in the assistant environment.
- Found a concrete repeated journal-request pattern in
  `MdViewNativeTablePagePlanner.PlanAsync`: a fragment of
  up to six source data rows was read via one
  `MdViewContainerPageCursor.ReadAsync` call per row,
  followed by another one-line call to check whether
  a seventh source row continued the table.
- Production change
  `1467c64f33a143fc83da2e9fd73a85417f7fc036`:
  request **one bounded source window** of
  `maximumSourceRows + 1` (2–7) classified lines,
  then use its entries for the same sequential row
  eligibility/geometry checks and lookahead. The planner
  still measures each candidate native Spectre table,
  leaves rejected rows unconsumed, and preserves precise
  `NextDataLine`, `HasMore` and fallback semantics.
  This changes no CLI, page index or widget contracts.
- Diagnostic-only change
  `e6c447e9b3453774b43042ba0b3d8d3416226158`:
  internal `MdViewContainerPageCursor.ReadRequestCount`
  counts validated `ReadAsync` invocations. It is
  **internal**, not new public API, and does not measure
  actual OS I/O operations or elapsed HDD time.
- Added five deterministic xUnit tests in
  `MdViewNativeTableBatchReadTests.cs`
  (commit `a62ac9f523f79468b1d8cc8afdff45379b854dba`).
  Verify exactly one journal request per fragment,
  three requests for a 14-row table's three-fragment
  eligibility scan, height-limited row retry without loss,
  EOF/other Markdown safety, unchanged source bytes,
  and no journal request for narrow/pre-cancelled paths.
- New English guide
  `Docs/Native-Table-Journal-Batching/en.md`, and
  root/project/test README plus native table planner
  and long-table guides were updated.
- **Limits:** this reduces per-fragment cursor API/offset
  lookups, not the per-event JSON decoding/reads. A bounded
  window can classify up to six more source lines than the
  old stop-at-first-non-table path; these remain in the
  disk-backed journal and are not counted as table rows.
  There is no measured HDD speedup claim.
- **Verification pending:** run
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`. Manually inspect
  `preview-native README.md --max-width 100 --center
  --long-tables`. Resolve any errors/warnings before
  calling the batching milestone green.
- **Next:** after a green checkpoint, evaluate a bounded
  contiguous journal-event disk read strategy and a
  repeatable HDD cold/warm-cache benchmark rather than
  changing source/index architecture speculatively.

## 2026-11-15 — bounded contiguous JSON event-journal reads

- **Previous checkpoint confirmed green by maintainer:** the
  native-table source-journal request batching milestone
  (one `MdViewContainerPageCursor.ReadAsync` per fragment).
  The new changes below **postdate** that test report.
- Found the next HDD-sensitive bottleneck in
  `MdViewContainerPageCursor.ReadAsync`: after one
  offset-table read it still performed **two separate
  event-stream reads for every classified source line**
  (four-byte record header and JSON payload). One
  native-table fragment request therefore still produced
  many underlying event-stream operations.
- Updated `MdViewContainerPageCursor.cs` (commit
  `839e67bd82104e48d131c6bf96c76c605398562b`).
  The cursor now reads at most one **additional following
  offset** to determine the exact end of a requested
  middle-of-journal window; the current `_eventBytes`
  is used as the sentinel at the scanned end.
  Strictly bounded record spans are validated before
  event reads. Complete neighboring events are grouped
  into **at most 256 KiB contiguous event-stream reads**;
  a single valid bigger event is read individually,
  up to the original 8-MiB JSON + 4-byte header bound.
  Original Markdown and journal file formats unchanged.
- Additional guard (commit
  `2e885d9b2404b4b99563000be22120cf535ee1bf`):
  verify monotonic, bounded source-record offsets
  **before** subtracting two long offsets, avoiding
  malformed offset overflow. Each decoded record's
  stored four-byte length must exactly equal its
  offset-defined byte span minus four; JSON and original
  physical line number remain validated.
- Added internal `EventReadRequestCount` for actual
  event-stream `ReadExactlyAsync` invocations.
  The previously added internal `ReadRequestCount`
  still counts high-level `ReadAsync` requests.
  These are diagnostic API counters, not OS physical
  disk I/O counters or HDD elapsed-time benchmarks.
- Added **7 xUnit methods** in
  `MdViewContiguousJournalReadTests.cs` (commit
  `c66331e1b89d8865786ac9021c154ac2632eea20`):
  one contiguous read for 20 short records;
  multiple complete-record windows for 90 large lines;
  oversized individual source events; offset lookahead
  vs end-of-journal sentinel; Unicode/CRLF and backward
  journal reuse; explicit private-journal corruption of
  stored JSON length and offset monotonicity; pre-cancel
  behavior and subsequent successful cursor reuse.
- Added English
  `Docs/Contiguous-Journal-Reads/en.md`, linked it from
  the existing native-table batching guide and updated
  root, MdView and test READMEs. No CLI flags, native
  Spectre geometry, compact fallback or 56-byte mixed
  visual page record changed.
- **Fresh .NET 10 focused/full test execution pending:**
  `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
  and `dotnet test Toolbox.sln`. The assistant
  environment has no .NET SDK, so no new green claim is
  made. Preserve and investigate any compiler/test
  warnings before proceeding.
- **Next after green:** compare cold/warm-cache HDD
  timings and profile per-event JSON deserialization;
  do not claim a physical I/O or throughput improvement
  solely from this deterministic read-call reduction.

## 2026-11-15 — maintainer green confirmation: contiguous journal reads

- Maintainer confirms **all tests green** after the bounded
  contiguous-event read implementation and its seven
  `MdViewContiguousJournalReadTests` were committed.
  This supersedes the pending-verification note directly
  above. No exact test count or machine-readable log
  was supplied with this confirmation; do not invent one.
- Green checkpoint includes 256-KiB contiguous event windows,
  separate oversized-record reads, strict offset/length and
  JSON/source-line checks, cancellation, Unicode/CRLF,
  and journal reuse while preserving source read-only behavior.
- `ReadRequestCount` and `EventReadRequestCount` measure
  internal request counts; they are not elapsed-time or physical
  disk-operation measurements. No claim of a measured HDD
  speedup follows solely from passing these tests.
- **Next:** design a reproducible opt-in throughput benchmark
  against a representative Markdown source. Capture cold
  versus warm OS cache separately, request counts, elapsed
  time and memory, without adding noise to the normal tests,
  changing the 56-byte page index or compromising low-RAM
  compatibility.

## 2026-11-15 — isolated 2-GiB HGST benchmark harness prepared

- Maintainer's most recent message requested a **manual, private HDD
  benchmark** against an approximately 80-GB HGST 5400-RPM test disk,
  proposed mounted at `/mnt/TEST`. A full 2-GiB Markdown source
  must be generated reproducibly, all snapshot/journal/TEMP I/O
  redirected to that disk, and comprehensive logs retained for
  future before/after performance comparisons.
- Added a separate **non-packable `MdView.Benchmarks` project** under
  `Blackboard/Tools/`, intentionally not inserted in
  `Toolbox.sln`, so normal MdView builds/test runs do not
  automatically create multi-gigabyte datasets.
- Launcher `MdView.Benchmarks/run.sh` validates the mount via
  `findmnt`, rejects system-root volume, unexpected symlinks
  and nested mounts, reserves at least `6 × data size + 1 GiB`,
  creates `data/tmp/logs` **on the selected disk**, and exports
  `TMPDIR/TMP/TEMP` **before** .NET starts. Free-space
  measurement is taken from `df -Pk` on the selected mount
  and passed to the .NET process, avoiding accidental
  `DriveInfo` selection of the system root filesystem.
  The runner verifies the resulting `Path.GetTempPath()`
  points at the target `ROOT/tmp`. No device formatting,
  cache clearing, privileged commands or automatic
  source-data removal.
- `MdView.Benchmarks/Program.cs` creates **exactly
  2,147,483,648 source bytes** by streaming a repeating
  deterministic Markdown block (headings, long prose,
  inline links, 24-row pipe table, code fences,
  Unicode), with an optional small `--size-mib 8`
  smoke fixture. `FileMode.CreateNew` protects
  existing files; a complete matching-size existing
  test fixture is reused only when its marker matches.
- `measure` performs a source snapshot + disk line validation,
  sequential classified journal population in bounded
  source windows, deterministic first/repeated random
  source-line seeks with identical seeds and optional
  multiple full passes. The benchmark-only cursor
  budgets are **8 GiB event journal / 512 MiB offsets**,
  leaving the normal 512-MiB/256-MiB production
  settings unchanged.
- `index` is deliberately a **separate** optional
  complete mixed visual page-index measurement with
  fixed TestConsole geometry, optional `--long-tables`,
  canonical FindPage/GetPage lookups and a
  benchmark-only 256-MiB mixed page index limit.
  The ordinary `all` mode does not invoke the potentially
  lengthy whole-document index on the 5400-RPM HDD.
- Per-run JSONL/TXT logs contain UTC events,
  process timing/CPU/GC/working-set data,
  journal sizes and read-request counters,
  detailed random seek samples/p50/p95/p99,
  Linux `/proc/self/io` deltas and failures.
  Separate `system-*.txt` captures mount, disk
  rotational indicator, filesystem, free memory,
  OS and runtime. They remain on `ROOT/logs`.
  Cache is never forcibly dropped, so first pass
  is **not automatically asserted cold**.
- Granted test-only friend access
  `InternalsVisibleTo("MdView.Benchmarks")`
  to MdView's existing internal journal counters;
  production public APIs, viewer behavior, source
  parsing, snapshot logic, mixed indexes and
  standard test suite are unchanged.
- Documentation in new
  `MdView.Benchmarks/README.md` and
  `MdView.Benchmarks/Docs/HDD-Benchmark/en.md`,
  with dedicated
  `MdView.Benchmarks/IMPLEMENTATION_STATUS.md`.
  Updated root and MdView README.
- **Verification pending**: the assistant has no .NET SDK or
  HDD mount. Maintainer must first run:
  `bash Blackboard/Tools/MdView.Benchmarks/run.sh
  /mnt/TEST all --size-mib 8 --seeks 10`;
  only then the 2048-MiB scenario.
  This test is **not yet green**; the preceding
  maintainer-confirmed Toolbox green checkpoint
  predates the benchmark project.
- **Next:** inspect the first build/8-MiB test log,
  fix any real issues on GitHub, then collect the
  2-GiB scan/seek logs and optional full-index
  measurements. Make performance decisions from
  measured numbers rather than theoretical estimates.

## 2026-11-15 — benchmark launcher additional mount safety

- Manual HDD bench now explicitly refuses a bind
  mount sharing the system root volume's MAJ:MIN
  device ID and non-disk targets `tmpfs`,
  `ramfs` or `overlay`.
- `df -Pk` mount-verified free space is passed
  into the .NET harness; no reliance on a potentially
  different `DriveInfo` root volume for the
  conservative disk-space guard. Benchmark metrics
  avoid JSON serialization of live snapshots/indexes.
- Program source checked for balanced lexical
  braces/parentheses; **no local .NET SDK exists**,
  therefore compile and disk smoke-run pending.

## 2026-11-22 — optional per-stage journal profile

After the completed 2-GiB Windows H: journal run
(1,614,865 lines, **5,022,177,346 bytes** journal,
**123,166.942 ms** journal scan, all **200/200** sampled
source-line seek checks correct, peak **72,130,560
bytes** process working set), we implemented an
**internal, opt-in diagnostic counter** for
`MdViewContainerPageCursor.ScanUntilAsync`.

New `MdViewJournalScanTimings.cs` uses a fixed-size
five-stage array and records per-stage aggregate
`Stopwatch` ticks, call counts, longest call,
>100ms counts and corresponding source line /
journal offset; no I/O or per-event allocated
diagnostic objects. Stage names: scanner advancement,
JSON serialization, event prefix write, event
payload write and offset table write. The observer
is null by default and introduces no changes to
the serialized journal format, budget limits,
public APIs or production write behavior.
Time values represent logical operation boundaries,
not proven physical disk I/O.

`MdViewContainerPageCursorTests` gained
`OptionalJournalTimingsMeasureEachWriteWithoutChangingJournal`,
covering two source lines, cached backward reuse,
one further forward line, correct data, expected
write counts and resetting per-window snapshots.
Existing cancellation, offset/journal budget and
page tests remain intact. Dedicated
[English documentation](Docs/Journal-Io-Diagnostics/en.md)
describes interpretation and test commands.

**Next:** verify Linux/Windows CI builds and
the focused new regression; have the maintainer
first run 128-MiB with `-TraceJournalIo` and
then the prepared 1-GiB fixture, without
changing source files. Attribute stalls to
individual logical phases before considering
optional write coalescing. Performance improvements
are not yet claimed.

## 2026-11-22 — journal readback included in optional timings

The 128-MiB five-stage physical Windows H: test
`mdview-hdd-20261122T145233390Z-5ca3b47a`
passed **200/200 indexed seek checks** with a
**4,159.724-ms** journal scan. Of this,
the five original stages accounted for
**1,391.197 ms**. The three logical journal
write awaits collectively accounted for only
**44.750 ms**; there were no per-stage calls
at or above 100 ms. Consequently the
remainder was not correctly attributable
solely to journal writes.

The optional `MdViewJournalScanTimings`
observer was expanded to eight stages, adding:
- `OffsetTableRead` for the offset-table
  `ReadExactlyAsync`;
- `EventWindowRead` for the bounded
  contiguous journal `ReadExactlyAsync`;
- `DeserializeEvent` for the replay
  `JsonSerializer.Deserialize`.

These are opt-in, fixed-size in-memory
aggregates; the default null observer,
persisted bytes, format and bounds
are unchanged. The existing test
`OptionalJournalTimingsMeasureEachWriteWithoutChangingJournal`
now checks the new counters during first
forward read, cached backward read and
further forward extension, plus reset.

The physical 128-MiB result predates
the three readback counters; **no
readback timings are inferred** from
that old file. CI verification of the
new stage counters is tracked separately.
Next: prepared **1-GiB** profile with
`-TraceJournalIo` after a green CI.

## 2026-11-22 — 1-GiB eight-stage stall traced; position setters instrumented

Maintainer evidence:
`mdview-hdd-20261122T145716681Z-543ebae1`
(JSONL and TXT; Windows H: physical HDD,
.NET 10.0.15, one 1-GiB pass, 1% progress,
eight-stage `TraceJournalIo=true`,
RAM-buffered diagnostic export).
**807,433 source lines**; snapshot
**25,123.9971 ms**, journal
**47,723.939 ms**, journal
**2,510,275,305 bytes**, all
**200/200** sampled source-line checks
correct. Peak process working set
**73,170,944 bytes (69.781 MiB)**.
Journal managed allocations
**13,113,580,248 bytes**; GC pause
**753.859 ms**.

Across all 100 intervals, the eight
stage totals were: scanner advance
**3,807.094 ms**; serializer
**1,838.367 ms**; event length write
**88.150 ms**; payload write
**115.499 ms**; offset write
**68.508 ms**; offset table read
**80.465 ms**; event window read
**573.736 ms**; JSON deserialization
**1,920.612 ms**. Sum
**8,492.430 ms** against
**47,723.939 ms** of journal wall time.
The original three write-await stages
accounted for **272.157 ms** total.

**Critical reproduction:** the 67–68%
progress interval at event-journal bytes
**1,682,002,689 → 1,707,059,208**
lasted **23,035.0882 ms**, with
six `ReadAsync` calls over 100ms
and a maximum single call of
**14,803.9228 ms** starting at
source line **544,512**, journal
offset **1,692,778,274 bytes**.
Eight stage totals in that entire
interval were only **75.655 ms**;
GC pause **7.797 ms**; preceding
diagnostic log write **0.0238 ms**.
No old stage had an operation above
100ms. The 1-GiB stall therefore
occurs inside `ReadAsync` but
**outside** the eight timed boundaries.
Do not attribute it to Windows disk
hardware without tracing physical I/O.

**Code change:** instrument the
synchronous setters for
`_events.Position` and
`_offsets.Position` before both
writing and reading, adding four
new `MdViewJournalScanStage` values
and extending the fixed-size
observer to **twelve** counters.
The original stage API, event
format, journal bounds, cancellation,
failure-closed semantics and default
null observer are unchanged.
The focused test now verifies that
cached navigation performs reads
but not new write-position operations.
GitHub CI verification follows this
source change; no optimization claimed.

**NEXT:** rerun the existing 1 GiB
physical-HDD fixture with
`-ProgressStep 1 -TraceJournalIo`
and compare `EventWritePosition`,
`OffsetWritePosition`, `OffsetReadPosition`,
`EventReadPosition` maxima with
`interval_cursor_read_max_ms`.
If the 1.7-GiB region stalls again,
the new boundary should help identify
whether seeking/repositioning is
responsible. If unaccounted, further
break down allocations/validation
and runtime transitions. No physical
sector defect is established.

## 2026-11-22 — third 1-GiB trace identifies synchronous journal seeks

Physical Windows H: test
`mdview-hdd-20261122T150433389Z-44ba10df`
used the same 1-GiB input with twelve opt-in
trace stages and RAM-only diagnostics until
completion. Snapshot **24,956.187 ms**;
journal **46,506.152 ms**;
**807,433** source lines; all **200/200**
sampled source-line seeks correct; peak
process working set **71,368,704 B
(68.06 MiB)**. The twelve stages
accounted for **45,595.327 ms**, leaving
only about **910.825 ms** not timed.

The synchronous append-position setters
are the measured bottleneck:
`EventWritePosition` **11,659.125 ms**,
`OffsetWritePosition` **25,440.455 ms**,
together **37,099.580 ms** of the entire
46.5-s phase. At 69–70% an
`_offsets.Position` setter at source line
**564,885**, journal byte **1,756,107,492**
blocked **13,489.504 ms**. At 70–71%
further setters included **2,923.605 ms**
offset and **1,506.843 ms** event
position-setting delays. No direct
physical-media failure is established.

**Implementation:** before *append*
only, assign `_events.Position` and
`_offsets.Position` **only when
different from the known end offset**.
Sequential appends no longer repeat
FileStream Position setters; backward
readback can still move the streams and
the following forward append restores
correct ends. Per-stream cumulative
internal setter counters are exposed
in the benchmark's 1% progress records.
A new regression tests 3×128 lines
with backward navigation between pages,
valid Markdown fence classification
and exact append reposition counts.
No persistent journal format, limits,
public API or RAM architecture changed.

**Next:** await green Windows/Linux CI,
then rerun unchanged 1-GiB physical-HDD
fixture to verify that actual setters
are rare and seek-stage time falls.
Do **not** report speedup until measured.

## 2026-11-22 — verification after redundant-position fix

GitHub Actions workflow `.github/workflows/dotnet.yml`
now runs the targeted
`SequentialJournalAppendsDoNotSeekAgainUntilBackwardReplayMovesStream`
regression on **both Ubuntu and Windows** in addition to the
existing optional-journal-timing regression and RAM-log self-test.

Verification:
- Source/test/documentation change
  `133250d85fa5ade5334961f7c04905168d6abc4b`:
  run `45037094393` **fully green** (normal build and
  tests, Windows UI regressions, Linux/Windows benchmark
  compilation and focused tests, Markdown links).
- Dedicated cross-platform regression workflow change
  `8b9b671b795a6937e647d47c8f9b0cf5c2f053ab`:
  run `45037207910` **fully green**. The new specific
  no-redundant-position-setters test passed on both OS runners.

These are correctness and build validations on CI hosts.
The real Windows H: physical HDD has **not yet been retested
after the fix**. Performance impact remains pending. The next
owner action is an unchanged 1-GiB
`-ProgressStep 1 -TraceJournalIo` measurement and examination
of cumulative actual position setter counts. No journal format
migration, API break or additional memory allocation policy
is introduced.

## 2026-11-22 — post-change 1-GiB regression; A/B mode added

New paired physical H: 1-GiB test
`mdview-hdd-20261122T154929922Z-93528de2`
on Windows / .NET 10.0.15 is fully successful:
807,433 source lines; journal
**2,510,275,305 bytes**; **200/200**
sampled source-line checks correct; journal
GC pause **817.440 ms**; peak working set
**72,675,328 bytes (69.31 MiB)**. Snapshot
**24,283.946 ms**; journal **125,347.666 ms**.
Both cumulative actual append-position
setter counters remained **zero**.

This **regresses** from the immediately prior
twelve-stage HDD run's **46,506.152 ms**
journal duration (2.70× slower); do not claim
the optimization improved performance.
The `EventPayloadWrite` timer instead took
**115,955.056 ms (92.5%)** of the new
journal phase. The two append-position
stages totaled just **47.697 ms** versus
**37,099.580 ms** before. This measured
shift in blocking location does not
establish whether the kernel, filesystem
cache or thermal/device state caused
the entire additional delay.

**New diagnostic experiment:** an internal
`ForceJournalAppendPositionSetters` property
defaults false, but a benchmark-only flag
can restore the original unconditional
event and offset `FileStream.Position`
assignments before each row. The benchmark
labels the mode in session_start; Windows
PowerShell forwards the A/B option.
A new regression tests the forced old
path with backward replay, three pages
and exact setter counts. Run it on both
Windows and Linux GitHub Actions.
Normal MdView default, public surface,
journal bytes, bounds and cancellation
semantics remain unchanged.

**NEXT:** alternate manual conditional/
forced/conditional 1-GiB runs on the
same prepared HDD source, compare total
time and per-stage latency, not just
the location of wait time. Pending
CI verification of the new A/B wiring.

## 2026-11-22 — post-regression A/B CI verification

Commit `daf5dc2ef2515b0c2a967475cd90bb05f590190e`
introduced an internal, benchmark-controlled switch to
compare original unconditional journal append-position
setters against the current conditional setters.

GitHub Actions workflow run
`45041293918` **passed all five jobs**:
- Ubuntu complete solution build and tests: **PASS**;
- Windows UI regression suite: **PASS**;
- Ubuntu benchmark compile, in-memory log test,
  timing regression, conditional-position regression
  and new forced-legacy-position regression: **PASS**;
- Windows benchmark compile, PowerShell launcher
  parse, in-memory log test, timing regression,
  conditional-position regression and new
  forced-legacy-position regression: **PASS**;
- Local Markdown documentation links: **PASS**.

No physical HDD A/B run has been performed since
the A/B flag was introduced. The last observed
conditional 1-GiB run was 125.348 seconds,
significantly slower than the earlier 46.506-second
unconditional measurement. **Next action:**
measure the forced legacy variant on the same
prepared physical H: HDD source and repeat the
conditional mode for comparison; do not infer
a causal throughput advantage until repeated
physical data is available.

CI evidence: https://github.com/afrowaveltd/Toolbox/actions/runs/45041293918

## 2026-11-22 18:00 UTC — first physical forced-legacy A result

After CI proved the benchmark-only A/B
flag correct on Windows and Linux,
the real Windows H: drive run
`mdview-hdd-20261122T180043896Z-568a1d79`
completed successfully in explicitly
selected `legacy_unconditional_position_setters`
mode. 807,433 rows; event journal
2,510,275,305 B; all 200 random
source-line checks correct.
Snapshot 24,431.445 ms; journal
**47,552.161 ms** (21.534 MiB/s).
Peak working set **71,708,672 B
(68.39 MiB)**, GC pause 763.672 ms.
Actual append position setters:
**807,433 / 807,433** for the
event and offset streams.

Comparison: earlier original A
**46,506.152 ms**, current
conditional B **125,347.666 ms**;
forced A again **47,552.161 ms**.
When `FileStream.Position` is
called on each append, event-payload
write time falls from 115.955 s
in B to **0.110 s** in A; the
event-position setter instead
accounts for **32.208 s**. This
supports a buffering/write-back
explanation but does not itself
prove the kernel-level cause.
Separate 70–73% long I/O stalls
remain observable in both A runs.

**NEXT:** repeat default conditional B
without the benchmark-only flag
on the exact same prepared source.
Do not modify production append
logic before interpreting that result.

## 2026-11-22 18:19 UTC — B–A–B closed; production A restored

Uploaded complete paired B logs
`mdview-hdd-20261122T181955063Z-bfed61a5`
confirm the conditional B experiment
a second time on the prepared 1-GiB
physical Windows H: HDD.
Snapshot **24,512.968 ms**;
journal **125,310.169 ms**;
807,433 source lines;
2,510,275,305 journal bytes;
200/200 indexed seek checks correct.
GC pause **809.174 ms**,
managed allocation **12,831,515,824 B**,
peak process working set
**74,280,960 B (70.84 MiB)**.
Both actual append-position setter
counters remained **zero**.
`EventPayloadWrite` alone
consumed **115,916.902 ms**,
92.5% of journal wall time.

The measured B–A–B journal times
were **125,347.666 / 47,552.161 /
125,310.169 ms** respectively.
B runs differed by **37.497 ms
(0.030%)**; fast A was 2.635×
faster than repeated B.
The earlier original-code A
baseline was 46,506.152 ms,
further supporting this choice.
Exact OS/NTFS FileStream buffering
cause remains unverified and no
HDD media fault is concluded.

**Production fix committed** as
`a78133bb19003f31ec3bdf4225fe485529dcf3f8`:
`MdViewContainerPageCursor` now
defaults to **unconditional Position
setters for event and offset appends**.
The optional conditional slower B
remains internal and accessible only
with explicit benchmark setting
`--conditional-append-position-setters`
(or `-ConditionalAppendPositionSetters`
via Windows launcher). Previous
`--force-append-position-setters`
still selects original A; conflict
with conditional flag is rejected.

Regression A validates the
**constructor default** and three
128-row pages with cached backward
replay and fenced Markdown context;
regression B checks zero redundant
setters during streaming and correct
reposition on backward navigation.
Both are in Windows/Linux Actions.
The source, journal format, resource
limits, cancellation behavior, and
public API are unchanged.

**Next:** check all CI jobs green
for fix and docs commits. Keep the
known-good A default. The separate
70–73% intermittent stall in A
can be researched later without
changing the proven A/B decision.

## 2026-11-22 — final B–A–B implementation verified in CI

Production restore commit
`a78133bb19003f31ec3bdf4225fe485529dcf3f8`
has **all five jobs green** in
https://github.com/afrowaveltd/Toolbox/actions/runs/45055236315.

The combined production + documentation and fixed
cross-topic Markdown link at
`301439f5ff95de8cac1d5e9d9f3664716e5afaeb`
has **all five jobs green** in
https://github.com/afrowaveltd/Toolbox/actions/runs/45055406762:
complete solution build and tests; Windows UI regression;
Ubuntu and Windows benchmark compile, RAM-log validation,
journal I/O diagnostics and both journal-mode regressions;
local Markdown documentation links.

Current verified default: **unconditional A**
for production MdView and benchmark;
explicit `--conditional-append-position-setters`
only for optional experimental B. The measured
B–A–B journal times are
**125,347.666 / 47,552.161 / 125,310.169 ms**,
with 200/200 correct seeks per run, no format changes
and bounded process working set. This regression is
**closed**. The intermittent ~70–73% write stall
of A is a separate remaining performance research topic;
no additional physical HDD run or speculative optimization
is necessary to close B–A–B.

## 2026-11-22 — first MdView executable preview candidate

- Set the MdView executable metadata to **0.1.0-preview.1**;
  `--version` and `version` are parsed as explicit CLI actions,
  printing the `AssemblyInformationalVersion` with any optional
  source-revision suffix removed. Added xUnit parsing regressions.
- Added a separate `.github/workflows/mdview-preview.yml`
  CI distribution workflow to build self-contained **win-x64**
  and **linux-x64** archives with native dependencies
  (single-file untrimmed publish), MIT license,
  preview README, project README, and SHA-256 checksum.
- The workflow requires the **entire MdView.Tests suite**
  to pass on each OS, then smoke tests the *published*
  executable with `--version`, `--help` and a local
  Markdown document. It additionally extracts the final
  archive and checks the executable/version and
  license/README presence. Ordinary pushes upload
  preview artifacts only; they do not publish a release.
- First packaging workflow run
  https://github.com/afrowaveltd/Toolbox/actions/runs/45056270564
  succeeded on **Windows x64 and Linux x64** (published
  binary smoke and archive creation). It predates the
  additional extracted-archive smoke step.
- A GitHub prerelease for the fixed version will be
  published only when a separately named branch
  `release/mdview-v0.1.0-preview.1` is created and
  both build/smoke jobs pass. Do not assert a public
  release exists until GitHub actually reports it.
- Release text with explicit current capabilities,
  installation steps and limitations:
  `Docs/First-Preview-Release/en.md`.
- **Scope:** full-document render and opt-in local
  bounded-memory pagers with HTTPS/GitHub navigation.
  Not a finished native page-based default or a fully
  accepted interactive TUI. No MSI/DEB installer.
  Manual Windows Terminal, Debian and SSH terminal
  acceptance remains pending. No change to the
  verified-fast unconditional journal append behavior.

## 2026-11-22 — first public MdView CLI prerelease verified

**Published** [MdView 0.1.0-preview.1]
(https://github.com/afrowaveltd/Toolbox/releases/tag/mdview-v0.1.0-preview.1)
on 2026-11-22 at **18:44:31 UTC**.
GitHub release tag: `mdview-v0.1.0-preview.1`,
source commit:
`c66859be5dbbfa9d3bf60135ab4a6cc16119cad8`.
Release is `prerelease=true`, `draft=false`,
containing four files:
- `mdview-0.1.0-preview.1-win-x64.zip`
- `mdview-0.1.0-preview.1-win-x64.zip.sha256`
- `mdview-0.1.0-preview.1-linux-x64.tar.gz`
- `mdview-0.1.0-preview.1-linux-x64.tar.gz.sha256`

Verified release workflow
https://github.com/afrowaveltd/Toolbox/actions/runs/45056640320
completed **all three jobs successfully**:
- full focused MdView xUnit tests on Ubuntu 24.04 and Windows;
- self-contained, single-file untrimmed executable publishing;
- packaged CLI `--version`, `--help` and safe local Markdown
  rendering on both operating systems;
- extraction of produced ZIP/TAR.GZ and running the
  extracted binary with `--version`, plus checking
  the license and packaged README;
- release job checks both archives then publishes
  prerelease assets and checksum files to GitHub.

The wider .NET solution CI for the pre-release source
commit is also green (see GitHub Actions). The
packaged executables themselves have been checked
under CI, not manually on physical Windows/Debian/SSH
terminal environments. No native installers, ARM
builds, private GitHub auth or stable fully paged TUI
are claimed. This is an explicitly limited first
preview milestone.

**Next suggested acceptance:** Windows Terminal and
Debian Konsole interactive source menu, FileBrowser,
`source`, `preview`, `preview-native` and remote browsing;
SSH no-GUI terminal. Investigate limitations and
issue a new immutable preview version for fixes.
Do not overwrite the published tag or artifacts.
