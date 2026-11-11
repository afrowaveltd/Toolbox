# mdview — early terminal Markdown reader

An independent .NET 10 **source-first CLI prototype** for opening complete Markdown documents with the existing Afrowave parser/renderer. It uses the separate Markdown.Cache library for HTTP(S) documents and public GitHub repository READMEs. This prototype is **not published**, not a native page-based reader and not a finished interactive TUI.

## Start from the Toolbox root

```bash
dotnet run --project Blackboard/Tools/MdView -- --help
dotnet run --project Blackboard/Tools/MdView -- local README.md
dotnet run --project Blackboard/Tools/MdView -- README.md
dotnet run --project Blackboard/Tools/MdView -- url https://example.org/manual.md
dotnet run --project Blackboard/Tools/MdView -- git afrowaveltd toolbox
dotnet run --project Blackboard/Tools/MdView -- github:AfrowaveLtd/Toolbox
dotnet run --project Blackboard/Tools/MdView
```

Without arguments, a **Spectre.Console source menu** offers an interactive local Markdown FileBrowser, HTTPS URL prompt, **GitHub repository Markdown link browsing** (owner or owner/repo), and Exit. Escape from FileBrowser returns to the source menu; Escape from the source menu exits. See [interactive source selection](Docs/Interactive-Source-Picker/en.md). The command parser accepts verbs without case sensitivity but never changes Linux local path casing or ref/path names. `mdview` is the compiled executable assembly name; it is not installed as a system command.

Local files support `.md` and `.markdown` and reuse the complete, strict Unicode/control-safe reader from [Blackboard.TextFiles](../../TextFiles/README.md). Remote sources use fully verified UTF-8 cache entries, ETag/Last-Modified checks and explicit stale status when offline. Public GitHub README retrieval uses the preferred REST endpoint, not guessed filenames or branches. HTTP(S) URLs with query strings, fragments or embedded credentials are rejected by the current generic provider; plain HTTP is disabled by default. GitHub private repositories, redirects and links to other files are not supported.

**Safety and resource limits:** input is rendered as literal Spectre text; fenced code is never executed. The CLI currently materializes a full Markdown string and parser model in RAM and dumps the entire result to the terminal. A large file warning is informational, **not** silent truncation; for large documents choose a future paged reader when implemented. Network timeout is 30 seconds; redirects and transparent HTTP decompression are disabled. The configured cache enforces the per-document limit (256 MiB by default), distinct from Markdown render size. The cache should be used only with a trusted user-owned directory, and never for authenticated/private content in this release.

## Verification

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
```

The 10 original command grammar tests passed on Windows as part of the user-verified **6,443/6,443** baseline. Eleven additional interactive picker test cases are authored but have not yet been run (no local SDK); manual Windows Terminal, Debian console and SSH output checks remain necessary.

[CLI topic guide](Docs/CLI/en.md) · [Implementation status](IMPLEMENTATION_STATUS.md) · [Cache source](../../SpectreConsole/Markdown.Cache/README.md).

## Historical build fix (2026-11-03)

The first CLI commit referenced `TextFilePreviewReader` as though it were exported by FileBrowser. It was actually located in Showcase. A temporary linked source file corrected the initial build; the subsequent extraction into [Blackboard.TextFiles](../../TextFiles/README.md) fully replaced this bridge. This section records the historical issue, not a current dependency.

## Windows test verification (2026-11-03)

The user-provided `dotnet test Toolbox.sln` log after the shared reader fix shows `mdview.dll` successfully built and `MdView.Tests` **10/10 passed**; all twelve solution test assemblies passed **6,438/6,438** on Windows (net10.0 target, .NET 11 preview SDK). Manual CLI usage and Linux CI are still pending; the pending cross-project linked-source reader extraction remains an architecture task.

## Independent local reader (2026-11-03)

The temporary Showcase `Compile Include` bridge is now removed. The local file decoder lives in [Blackboard.TextFiles](../../TextFiles/README.md), with a dedicated `ProjectReference`. The CLI has no dependency on the Showcase executable. Rerun the full solution tests to verify this refactoring; the previously reported green Windows run predates it.

## Spectre interactive source selection (2026-11-03)

The no-argument entry now uses `MdViewInteractiveSourcePicker` with an injected `IAnsiConsole` and the existing local `SpectreFileBrowser`. Local filename filtering starts with `*.md;*.markdown` and is verified again after selection; URL and GitHub options use Spectre text prompts. Escape inside FileBrowser returns to the menu; Escape from the menu exits. Explicit command-line modes are unchanged. The picker clears the screen on each return to the menu and rejects non-interactive stdin.

Eleven new TestConsole-driven scenarios cover the menu, cancellation, real file selection, GitHub/HTTPS prompts and invalid input without network calls. **New code/tests are not yet run**; the last user-confirmed 6,443-green baseline predates this update. [Details](Docs/Interactive-Source-Picker/en.md).

## Color syntax highlighting for installed grammar languages (2026-11-04)

The CLI now enables our optional [TextMate grammar adapter](../../SpectreConsole/Markdown.TextMate/Docs/Supported-Grammars/en.md) when rendering local, HTTPS and GitHub Markdown. Its previous default Markdown theme omitted the syntax provider. More than 50 languages supported by the installed `TextMateSharp.Grammars` bundle can be selected directly or by file extension, with common README aliases including `js`, `ts`, `py`, `cs`, `sh`, `json`, `yml`, `sql`, `html`, `css`, `rust` and `go`. Code never executes; unknown languages, native backend failures and long lines safely render as plain text. TextMate's native dependency is optional to base Markdown/ExchangeHouse, but included in this CLI. New highlighter and integration regression tests have been committed; no new test run has yet been verified.

## Browse public GitHub repositories by owner (2026-11-04)

You no longer need to remember an exact repository name. The following commands list an owner's public repositories and open a Spectre selection interface:

```bash
dotnet run --project Blackboard/Tools/MdView -- afrowaveltd
dotnet run --project Blackboard/Tools/MdView -- git afrowaveltd
dotnet run --project Blackboard/Tools/MdView -- github:afrowaveltd
```

The existing `git OWNER REPOSITORY` and `github:OWNER/REPOSITORY` forms still open a README immediately. The no-argument GitHub source prompt accepts just the owner too. Small catalogs show an owner/repository tree; all catalogs offer a paged selection prompt with a name/description filter and Escape to cancel without downloading a README. The catalog uses public GitHub REST endpoints, a 30-minute independently verified local cache with explicit stale/offline fallback and a bounded first-1,000-repositories limit. Owner-only browsing requires an interactive terminal; scripts should give a full owner/repository pair.

[Detailed owner discovery guide](Docs/GitHub-Owner-Discovery/en.md) · [GitHub catalog source](../../SpectreConsole/Markdown.Cache/Docs/GitHub-Repository-Catalog/en.md). New catalog and UI tests are committed but not yet executed here.

## Disk-backed source paging — optional raw mode (2026-11-04)

```bash
dotnet run --project Blackboard/Tools/MdView -- source README.md
```

The new `source PATH` command opens a small-RAM, **strict UTF-8 source pager** on an indexed local file. It displays complete physical source lines, not rendered Markdown, and supports **Next page**, **Previous page**, **Go to line** and **Exit** through Spectre.Console. Index offsets reside in a temporary disk file; the source reader validates the entire document before showing the first page. The default index budget is 256 MiB, with per-page/per-line limits and explicit failures instead of silent truncation. UTF-16/32 and styled Markdown remain available through the existing complete-file `local` mode, not this raw source pager.

A formatted, visual-line-aware Markdown pager that preserves lists, code-fence state, links, themes, wrapping and disk-backed backward navigation still needs a separate incremental block parser. The existing local/URL/GitHub commands are unchanged. [Raw pager guide](Docs/Raw-Source-Pager/en.md) · [Disk index guide](../../TextFiles/Docs/Disk-Line-Index/en.md). New source-pager tests are authored; compilation/CI execution not yet confirmed.

## After a document: Enter to main menu, Escape to exit (2026-11-04)

After displaying a fully rendered Markdown document on an interactive console, `mdview` now shows `Enter: main menu | Esc: exit` and **waits for a key**. Enter reopens the existing FileBrowser/URL/GitHub source menu without restarting the application; Escape exits successfully. All other keys, including `F`, PageUp/PageDown and arrows, are deliberately ignored until search and in-document navigation are implemented. This also works after a direct `mdview local PATH`, `mdview url URL` or `mdview git OWNER REPOSITORY` command.

Help, failed or cancelled reads and the separately interactive raw `source PATH` pager do not receive this extra completion prompt. When input or output is redirected, the CLI renders once and exits automatically so scripts cannot hang. [Full interaction contract](Docs/Document-End-Navigation/en.md). The new prompt and session-loop tests are committed but a new test run is still pending.

## Planned indexed document search (design only, 2026-11-04)

A future `F` search interface should offer common English README topics (Installation, Build, Compilation, Configuration, Usage, Examples and others) plus detected heading titles and user-entered phrases. `N`/`P` would move between next/previous occurrences. Instead of indexing every word on every start, index a small configurable seed set and scan/add new phrases only on first use; store compact source offsets tied to a verified document version so stale positions cannot survive README updates.

**Search is not yet implemented; F/N/P still have no action.** The groundwork is an isolated forward-only [incremental line scanner](../../../ExchangeHouse/Markdown/Docs/Incremental-Source-Scanner/en.md) committed alongside this design; it is not wired into mdview's existing fully rendered output. [Full future-search plan](Docs/Indexed-Search/en.md).

## Experimental formatted source-page preview (2026-11-08)

```bash
dotnet run --project Blackboard/Tools/MdView -- preview README.md
```

The new **opt-in** `preview PATH` command creates a validated temporary
UTF-8 snapshot, reads a bounded source page (24 physical lines by default),
and passes only that fragment to the existing Markdown/Spectre renderer.
Spectre navigation provides Next, Previous, Go to source line and Exit.
The normal `local`, `git`, `url` and raw `source` paths are unchanged.

**Known limitation:** source pages are parsed independently. A fenced code
block, list or quote spanning pages can lose syntax/context at the boundary.
The number of rendered terminal rows is not currently constrained, and this
is not yet the planned stateful, index-aware visual pager. It is a
separately named proof of concept for snapshot-backed bounded rendering.
The source Markdown is unchanged; the temporary snapshot is disposed when
navigation exits. CLI requires an interactive terminal.

[Detailed design and limitations](Docs/Experimental-Formatted-Preview/en.md).
New tests and compilation await a fresh Windows/Linux run.


## Incremental fenced-code context across preview pages (2026-11-08)

The opt-in `preview PATH` mode now uses `MdViewContainerPageCursor`,
a bounded cursor over `MarkdownIncrementalContainerScanner` fed by
`Utf8ValidatedSnapshot.OpenReader()`. Forward navigation continues from
the scanner's last physical line without rescanning earlier pages.
Backward navigation and source-line jumps replay scanner state from the
start of the snapshot (correct but potentially slow for very large files).

Confirmed `FenceOpen`, `CodeLine` and `FenceClose` events are emitted
as **literal, safe Spectre text** instead of being reparsed as Markdown
headings, list markers or Spectre markup. Consecutive non-code source
lines still use the existing colored Markdown renderer in bounded chunks.
This preserves basic code-fence classification across page boundaries,
but code syntax coloring, precise quote/list formatting, complete visual
row pagination and cached reverse checkpoints are not yet implemented.

The existing `local`, `source`, `git` and `url` modes remain
unchanged. New compilation and test results must be verified on Windows
and Debian before claiming a green build.


## Disk-backed container journal for reverse navigation (2026-11-08)

The experimental `preview PATH` mode now retains classified
`MarkdownContainerLine` events in a **bounded disk journal** rather than
discarding them after each page. The cursor writes serialized line events
and a separate 64-bit per-line offset table. Previously visited pages,
including their fence/list/quote metadata, are reloaded directly from
the journal: navigating backward no longer forces a replay from the
beginning of the document. Forward scans continue from the previous
scanner position.

`MdViewContainerPageCursor` defaults to a **512 MiB journal byte
budget** plus a **256 MiB offset-table budget**, both configurable in
its constructor. Exceeding either cap fails explicitly, without silently
dropping lines. Temporary files are opened with delete-on-close, and
normal disposal releases the parser, reader and both files. Event text
is stored on disk, not retained in a growing RAM collection.

This is **not** a serializable snapshot of the scanner's private iterator
state. Jumping **forward beyond the scanned high-water mark** still
requires processing intermediate source lines, so it is not yet
constant-time random Markdown navigation. Disk budgets also mean
extremely large files may require explicit configuration or another
metadata representation. Original mdview modes are unchanged.
Focused tests are committed but require a fresh Windows/Debian run.

### Page-request cancellation isolation

`MdViewContainerPageCursor` no longer captures the first `ReadAsync`
call's cancellation token in its persistent scanner enumerator.
Per-request cancellation is checked at scan and journal boundaries;
a cancelled **previously completed** request cannot poison later
navigation. An in-progress local stream read is not interruptible by
the request token, and cancellation after scanner advancement still
fails closed to protect journal consistency.

### Opt-in snapshot checkpoint page reader

`MdViewCheckpointPageReader` resumes classified physical Markdown pages
from opaque, snapshot-bound `MdViewCheckpointPosition` values. It maps the
UTF-16 checkpoint to a validated byte-aligned line start, reopens a strict
UTF-8 reader on the private copy and restores explicit quote/list/fence
state without replaying earlier source lines. Maximum request: 128 lines.

It is intentionally separate from the existing journal pager. It can only
seek to positions whose checkpoints have already been obtained; generating
trusted future checkpoints remains a separate step. Embedded raw
`MarkdownStreamLine.Kind` is locally classified, while the enclosing
`MarkdownContainerLine.Kind` preserves restored container state. See the
[formatted preview guide](Docs/Experimental-Formatted-Preview/en.md).

### Sparse on-disk scanner checkpoints

An opt-in `MdViewDiskCheckpointIndex` supports bounded random physical-line
classification after a **one-time full source indexing pass**. By default it
stores a versioned scanner context every 1,024 lines, with a combined
256 MiB disk limit for checkpoint records and their offsets. Random reads
restore the nearest preceding checkpoint and classify fewer than 1,024
intermediate lines, rather than replaying all preceding lines.

Use `CreateAsync(snapshot, utf16Positions)` then `ReadAsync(line, 40)`.
The index owns temporary files only; callers own and dispose the validated
snapshot and UTF-16 position index. This is opt-in infrastructure, not
yet a new end-user `mdview` navigation mode. Building the index still
opens bounded snapshot readers repeatedly; HDD throughput remains
unbenchmarked. See
[experimental preview and checkpoint guide](Docs/Experimental-Formatted-Preview/en.md).

### Relative Markdown link navigation roadmap (2026-11-09)

Interactive link following is **not yet implemented**. The planned
[link navigator](Docs/Link-Navigation/en.md) will resolve Markdown links
against a typed document origin: the current local file directory, an
HTTPS source URI, or a GitHub repository/ref/path. It will add safe link
selection, heading anchors, and bounded Back/Forward history. No
automatic opening or execution of links is planned; source-specific
validation and offline cache behavior must remain intact.

### Typed relative-link resolver (first code slice)

The **noninteractive** `MdViewLinkResolver` now resolves relative
Markdown paths and same-document fragments against explicit local,
HTTPS or GitHub document origins. It returns a canonical target but
never opens it. Dangerous schemes, cross-origin transitions, encoded
paths, parent-directory escapes and unsupported query links are
rejected. Local path containment is lexical only; a future opener
must separately check symlinks/reparse points. **Clickable navigation,
anchor selection and Back/Forward history are still planned**, not
implemented. Full contract:
[Link Navigation](Docs/Link-Navigation/en.md).

### Local linked Markdown loading (2026-11-09)

`MdViewLocalLinkLoader.OpenAsync(source, href)` now resolves a relative
link from a typed local origin and opens the destination as a caller-owned,
**fully validated UTF-8 snapshot**. It accepts only `.md` and
`.markdown` targets, with the same bounded disk budget as the underlying
snapshot. It rejects lexical paths outside the specified navigation root,
and checks root/intermediate/leaf paths for symlink or reparse-point
attributes before opening.

This is an **opt-in backend API**, not an interactive clickable link UI.
The checks do not provide race-free filesystem confinement: an attacker
could replace a directory between checking and opening. Additional
OS-level file handle constraints would be required to guarantee isolation
from hostile same-user filesystem mutations. HTTPS/GitHub loaders,
heading anchors and history remain future steps.

### History and local navigation controller

Opt-in `MdViewNavigationHistory` keeps a bounded source/fragment
Back/Forward history, including branch truncation and oldest-entry
eviction. `MdViewLocalNavigationSession` combines it with fully
validated local linked snapshots. It opens each destination before
changing navigation state and retains only **one open snapshot** at
a time. Broken links leave the previous document available.

This is backend infrastructure; the existing interactive `mdview`
terminal controls are unchanged. Explicit link selection, anchor
scrolling, and HTTPS/GitHub history will follow separately.
[Detailed contract](Docs/Link-Navigation/en.md).

### Opt-in link discovery and terminal picker

`MdViewDocumentLinkExtractor` enumerates supported relative links using
existing `ExchangeHouse.Markdown` parser tokens, preserving original source
offsets and excluding fenced code, inline code, images and forbidden links.
The 1 Mi UTF-16 character and 4,096-link caps prevent unlimited
in-memory extraction. `MdViewDocumentLinkPicker` provides an explicit
Spectre.Console list with Escape cancellation; it does not open content.
Both components are **not yet integrated into mdview's default display
loop**. Large-document streaming link indexing remains future work.
See [Link Navigation](Docs/Link-Navigation/en.md).

### Interactive local Markdown links — opt-in command

Use `mdview browse README.md` (or
`dotnet run --project Blackboard/Tools/MdView -- browse README.md`)
to open an explicitly **interactive** reader with selectable relative
Markdown links, Back/Forward and Escape to exit. It uses one validated
local snapshot at a time and reopens each target on navigation; invalid
targets preserve the current document. It limits individual sources
to 1 MiB of UTF-8 data and confines link paths lexically to the
starting document directory; symlink checks are best effort rather
than a race-free OS security boundary.

In-document anchors are recorded but not yet scrolled into view.
Nonlocal HTTPS/GitHub links remain unsupported here. The existing
normal renderer, indexed raw pager and experimental formatted preview
are unchanged. [Detailed guide](Docs/Link-Navigation/en.md).

### Local heading anchor navigation

The opt-in `mdview browse PATH` now resolves fragment links such as
`#installation` and `Docs/Setup/en.md#install` against a bounded
`MdViewHeadingIndex`. A matching heading is rendered at the beginning
of the displayed Markdown suffix. Repeated heading slugs receive
`-1`, `-2` suffixes; fenced-code lines are excluded. Back/Forward
preserves the fragment. Unknown fragments show feedback.

This is not yet exact terminal row scrolling or full GitHub heading
slug compatibility. See [Link Navigation](Docs/Link-Navigation/en.md).

### Relative HTTPS Markdown navigation (2026-11-09)

Use `mdview browse-url https://docs.example.org/README.md` to
interactively follow **same-origin** relative HTTPS Markdown links.
The optional browser reuses `HttpMarkdownDocumentSource`,
`VerifiedDocumentCache`, parser-backed link selection and the
source/fragment Back/Forward history. Every different URL is fully
validated before navigation commits. Previous verified content can
serve as an **explicitly labeled Stale/offline fallback**; failed links
without a valid cached copy leave the original page available.

The HTTPS browser limits each document to 1 MiB, does not allow
automatic redirects, and follows no non-HTTPS or cross-origin links.
Same-document anchor jumps do not refetch the source. This is opt-in:
normal `url`, `git`, `local`, `source`, `preview` and
`browse` commands retain their previous behavior.
GitHub repository file navigation is a future separate adapter.
[Design and acceptance details](Docs/Link-Navigation/en.md).

### GitHub repository Markdown browsing (2026-11-09)

The opt-in command `mdview browse-git OWNER REPOSITORY` (also
`browse-git OWNER/REPOSITORY`) resolves the real default branch
and README path from GitHub's official metadata, then allows explicit
navigation of relative `.md` and `.markdown` links **inside the same
repository and ref**. The terminal shows the owner/repository/ref/path,
content cache freshness and metadata freshness, including an explicit
Stale/offline indicator when applicable. It offers Follow a link,
Back/Forward, anchored heading display and Escape/Exit. Missing or
invalid targets leave the previous verified page open.

The same 1 MiB byte limit and small-document parser bounds apply.
This does not yet follow absolute GitHub blob-page links, move between
repositories/branches, open external URLs or handle private GitHub
repositories. Existing `git OWNER REPOSITORY` continues to display
the original one-shot preferred README.
[Link Navigation](Docs/Link-Navigation/en.md).

### GitHub README link visibility (2026-11-09)

`browse-git` now prints **Navigable Markdown links: N** before
rendering each document and explains that links are selected using
**Follow a link** in the keyboard menu **below** the rendered Markdown.
The rendered Markdown text itself is not an interactive click target;
the browser deliberately offers a separate terminal selection prompt.
Unsupported external links are not counted.

The Toolbox root README contains many repository-relative documentation
links, including links inside the project overview table. Regression
coverage checks that those links are recognized while an external
GitHub website URL is excluded. If the displayed count is zero on
that README, investigate the extractor/parser or actual source
identity rather than assuming the README contains no links.

### Repository catalog unavailable or rate-limited (2026-11-09)

When the GitHub owner-only repository catalog reports an error
(including HTTP 403 from an exhausted API rate limit), the interactive
repository picker no longer terminates the application. It displays
the returned warning, offers a **manual repository-name prompt** and
allows an empty entry to return to the source menu. Manual names are
validated before requesting a README; the catalog is not silently
represented as an empty or complete list.

This fallback does **not** bypass GitHub's API limits: a subsequent
README or metadata request can still be rejected until the limit
resets. Previously verified cached content may remain available
under the existing documented stale/offline policy.


### Main menu GitHub links are now navigable (2026-11-11)

Selecting GitHub from the no-argument menu previously opened a
**one-shot README** rather than the link-navigation session. The menu
now starts the same `GitHubLinkBrowser` as `browse-git OWNER REPO`.
When only an owner is entered, the repository selector returns a
browser command after catalog selection or manual-name fallback.
The browser shows the **number of navigable links and the first three
link labels** above a long README. Choose **Follow a link** in the
terminal menu below the rendered Markdown to browse all supported
repository-relative Markdown links. Text-rendered Markdown links are
not directly mouse-clickable in the terminal.

`git OWNER REPO` remains the unchanged one-shot CLI mode. GitHub
metadata, repository and file cache validation remain the same.

### First bounded Markdown page navigation for GitHub (2026-11-11)

Long Markdown files opened by the interactive GitHub menu or by
`mdview browse-git OWNER REPOSITORY` now show **one page at a time**
instead of dumping the entire document. Each page contains at most
**24 physical Markdown source lines** and **16,384 UTF-16 display
characters**. The menu supports **Next page**, **Previous page**,
**Go to heading** (up to the first 512 recognized headings) and
**Go to source line**, alongside the existing **Follow a link**,
document-level Back/Forward and Exit.

Short files of at most 24 lines continue using the previous complete
renderer and menu order. A long document is parsed and indexed once
per verified file/content SHA-256; switching pages does not generate
network requests or reread the entire document. Fenced code stays
literal even at a page boundary. A very long individual line may be
shortened on screen with a warning; it is **not** truncated in the
cached source. Complex quote/list boundaries and full cross-page
TextMate coloring remain experimental.

The GitHub browser still has its original **1 MiB source limit**.
This first source-page UI is not a streaming reader for gigabyte
files or a wrapped visual-terminal-row paginator. The separate
local disk-backed source pager and incremental scanner/checkpoint
infrastructure remain the route for truly large files.

[Read the page-navigation guide](Docs/Paged-GitHub-Browsing/en.md).

### Disk-backed Markdown headings and link locations for large local files

The experimental local formatted preview (`mdview preview PATH`)
now supports **Find heading (disk index)** and **Find link location
(disk index)**. These optional actions build a disk-backed
`MdViewDiskNavigationIndex` **lazily** on first use and then show
headings/links in batches of 12. Selecting an item jumps to its
physical source line, without loading the full Markdown document
or every result into RAM.

The index makes a sequential pass through the previously validated
source snapshot, records recognized ATX headings and inline link
positions outside fenced code, and stores variable-length entries
plus 64-bit per-kind offsets in temporary files. The default combined
index disk budget is 128 MiB. A budget failure leaves normal paging
available; successful indexes are disposed with the preview.

**This is source-position navigation, not cross-document link
following.** The existing small-document `browse` / `browse-git`
modes still own their separate link-following safety contracts.
Reference links, multiline constructs, full CommonMark parsing,
rich cross-page code highlight and constant-time first distant
jumps are not yet implemented.

[Disk Navigation Index](Docs/Disk-Navigation-Index/en.md).
