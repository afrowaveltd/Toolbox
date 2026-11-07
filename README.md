# Afrowave.Toolbox

**Afrowave.Toolbox** is a collection of small, reusable **.NET 10** libraries, console utilities, test projects and examples. It is **not** one mandatory all-in-one package: each family and useful UI control has its own responsibilities and dependencies.

**GitHub is the source of truth:** [afrowaveltd/Toolbox](https://github.com/afrowaveltd/Toolbox) (branch `master`).

[Documentation index](Docs/Documentation/en.md) · [Warning-free builds](Docs/Warning-Free-Build/en.md) · [Repository overview](Docs/Toolbox-Overview/en.md) · [Architecture](Docs/Architecture/en.md) · [Build and verification](Docs/Development-and-Verification/en.md) · [Implementation status](IMPLEMENTATION_STATUS.md)

## Package and project directory

| Library / family | What it does | Status |
|---|---|---|
| [Essentials](Essentials/README.md) | `Result`, `Response<T>`, issues, metadata, guards and shared contracts | **Published on NuGet**, `0.2.0` |
| [WhenItFails](WhenItFails/README.md) | Structured error catalogs, validation, runtime resolution, recovery and profiles | **Published on NuGet**, `1.0.0` |
| [Blackboard FileBrowser](Blackboard/SpectreConsole/FileBrowser/README.md) | Interactive Spectre.Console file/folder picker, filters, tree and search | **Published on NuGet**, `0.1.0-preview.1` |
| [UnderstandMe](UnderstandMe/README.md) | Offline localization, dictionaryless keys, JSON dictionaries and validation | Implemented offline core; broader translation service is planned |
| [ExchangeHouse Markdown](ExchangeHouse/Markdown/README.md) | UI-neutral lossless Markdown parser, links/images, nested lists, code-language and quote metadata | Implemented preview project, **not yet published** |
| [Blackboard Markdown](Blackboard/SpectreConsole/Markdown/README.md) | Spectre.Console Markdown display, safe images, quotes, code blocks and optional syntax colors | Implemented preview project, **not yet published** |
| [Markdown.TextMate](Blackboard/SpectreConsole/Markdown.TextMate/README.md) | Optional real code syntax coloring for Markdown with TextMateSharp grammars | Implemented source adapter; **not yet published** |
| [Blackboard.TextFiles](Blackboard/TextFiles/README.md) | Strict local Unicode reader and disk-backed UTF-8 physical-line index for bounded-memory paging | Source implementation, **not yet published** |
| [mdview](Blackboard/Tools/MdView/README.md) | Terminal Markdown reader for local, HTTPS and GitHub READMEs; owner browsing, optional raw indexed source pager | In-repository CLI preview; **not yet published** |
| [SeeMe](SeeMe/README.md) | Console presentation helpers for WhenItFails | Implemented source project |
| [Toolroom](Toolroom/README.md) | Runnable developer/maintenance tools including WhenItFails Setter | Implemented utilities |

The three NuGet publications above are **maintainer-confirmed**; the package IDs/versions match repository project metadata. Do not confuse a project `Version` with public publication. Details: [Release status](Docs/Release-Status/en.md).

## NuGet-first reusable component policy

As soon as a self-contained .NET library is useful, documented, tested and
installable, we publish a **NuGet prerelease** rather than waiting for a full
`1.0.0`. Frequent preview releases are welcome; downstream projects consume
stable package IDs and may opt into updates without changing shared source
paths. Published versions are immutable and consumers must explicitly enable
prerelease updates where their package manager requires it. This is a
**publication policy**, not a claim that every preview project has already
been released. See [NuGet release policy](Docs/NuGet-Release-Policy/en.md)
and [current package status](Docs/Release-Status/en.md).

## Applications and examples

- [Blackboard.Showcase](Blackboard/Samples/Blackboard.Showcase/README.md): interactive demonstrations of FileBrowser text and JSON previews, directory selection, and four Markdown themes (Classic, Ocean, Forest, Monochrome), with links, images and nested/ordered lists.
- [FileBrowser.Sample](Blackboard/Samples/FileBrowser.Sample/README.md): focused file picker example.
- [UnderstandMe Configurator](UnderstandMe/Tools/Configurator/README.md): JSON settings editing and validation, provider checks and offline help rendered by Blackboard Markdown.
- [WhenItFails Setter](Toolroom/WhenItFails/Setter/README.md): error catalog authoring, inspection and validation.

## Architecture

```text
Essentials
    +-- WhenItFails
    |     +-- SeeMe / WhenItFails Console
    |     +-- Toolroom / Setter
    +-- shared infrastructure consumed by other projects

ExchangeHouse.Markdown --(document model)--> Blackboard.SpectreConsole.Markdown
                                                 +-- Blackboard.Showcase
                                                 +-- UnderstandMe Configurator

Blackboard.SpectreConsole.FileBrowser
    +-- FileBrowser.Sample / Blackboard.Showcase
    +-- UnderstandMe Configurator

UnderstandMe -- offline localization core; broader service in design
```

The diagram is conceptual; for exact package vs project references, see the `.csproj` files and [architecture](Docs/Architecture/en.md). ExchangeHouse never needs a UI renderer; Blackboard components are independent of their consumers.

## Getting started

Install a supported **.NET 10 SDK**. From the repository root:

```bash
dotnet restore Toolbox.sln
dotnet build Toolbox.sln -c Release
dotnet test Toolbox.sln -c Release
```

Try the Markdown theme gallery and FileBrowser demos:

```bash
dotnet run --project Blackboard/Samples/Blackboard.Showcase
```

Or try the offline localization configurator:

```bash
dotnet run --project UnderstandMe/Tools/Configurator
```

[Repository getting started](Docs/Toolbox-Getting-Started/en.md) includes per-project testing and examples of adding a published NuGet package.

## Documentation and development rules

- [Main index](Docs/Documentation/en.md), [architecture](Docs/Architecture/en.md), [release status](Docs/Release-Status/en.md), [verification](Docs/Development-and-Verification/en.md).
- Every library/app should maintain an English root `README.md`, topic-level `Docs/<Topic>/en.md` and `IMPLEMENTATION_STATUS.md`. English is the source for later automatic translations.
- Each functional step is tested, documented and committed on GitHub; Windows-focused CI complements the full Linux solution build.
- Latest verified nested-list regression: [GitHub Actions #41244480220](https://github.com/afrowaveltd/Toolbox/actions/runs/41244480220). See the [status files](IMPLEMENTATION_STATUS.md) for subsequent changes.

## Design-only and future work

- [AJIS / A2 notes](Ajis/README.md) are **design notes**, not a shipped AJIS 2.0 Toolbox runtime; the historic project is [Ajis.Dotnet](https://github.com/afrowaveltd/Ajis.Dotnet).
- Suitcase, GetConnected, SemTam/TamTam, TalkToMe and future ExchangeHouse converters are separate future work; this listing does **not** imply completed projects.
- The repository's [Afrowave Id rebuild](Docs/Id-Rebuild-Review/en.md), [high-availability](Docs/Id-High-Availability-and-Replication/en.md), [regional routing](Docs/Id-Autodiscovery-and-Regional-Routing/en.md) and [Observer](Docs/Id-Observer-and-Uplink/en.md) documents are exploratory designs, not implemented Id services in Toolbox.

## Community-first service and transparent infrastructure funding

Afrowave intends to give everyone the maximum feasible access to useful hosted services, with no artificial paywalls or personal enrichment. Community support, when needed, is intended for transparent, verifiable real operating costs. Those future hosted-service policies **do not impose payment requirements on Toolbox libraries**. No donations or bank-backed funding dashboard are implemented by these repository projects. See [Community access and transparent funding](Docs/Afrowave-Community-Access-and-Transparent-Funding/en.md).

## License

See [LICENSE.txt](LICENSE.txt).

## Quoted sublists inside ordinary list items (2026-11-01)

Markdown parsing and Spectre presentation now support **ordinary list → explicitly indented quote → nested list → fenced code**, preserving the enclosing ordinary item and the independent quoted-list hierarchy. See the [ExchangeHouse parser guide](ExchangeHouse/Markdown/Docs/Quoted-Lists-Inside-Lists/en.md) and [Blackboard presentation guide](Blackboard/SpectreConsole/Markdown/Docs/Quoted-List-Containers/en.md). No new NuGet release.

## View actual Markdown README files in the Showcase (2026-11-02)

The fifth [Blackboard Showcase](Blackboard/Samples/Blackboard.Showcase/README.md) module uses the existing FileBrowser to select local `*.md;*.markdown` files and displays their **entire validated document**, including C# / Bash grammar coloring through the optional TextMate adapter. Files larger than 64 KiB receive only an informational notice; they are **not truncated or rejected by size**. Strict UTF-8 or BOM-marked UTF-16/32 decoding, terminal-control screening and read-only access apply to the whole file. [File-viewer guide](Blackboard/Samples/Blackboard.Showcase/Docs/Markdown-File-Viewer/en.md). Large documents still require memory for the full parsed Markdown model.

## First bounded-memory source-paging milestone (2026-11-04)

Our Markdown reader now has an **optional indexed raw UTF-8 source pager** alongside the existing fully formatted, whole-document viewer:

```bash
dotnet run --project Blackboard/Tools/MdView -- source README.md
```

`Blackboard.TextFiles.Utf8DiskLineIndex` strictly validates all bytes in one sequential pass, writes source-line offsets to a temporary bounded disk index and reads forward/backward pages with a small RAM budget. The source pager uses Spectre.Console prompts for Next, Previous, Go to line and Exit. This first mode intentionally displays **raw Markdown source**, not rendered blocks; the regular mdview renderer, code highlighting, FileBrowser and GitHub support remain unchanged. Proper formatted streaming Markdown paging (including lists, quote/fence state and terminal wrapping) is a separate next milestone. New code and tests have been committed, but the latest full-solution test run has not yet been confirmed for this increment.

[Disk index design](Blackboard/TextFiles/Docs/Disk-Line-Index/en.md) · [mdview source pager](Blackboard/Tools/MdView/Docs/Raw-Source-Pager/en.md).

## Interactive mdview document completion (2026-11-04)

After successfully opening a fully rendered Markdown document in an interactive terminal, `mdview` now waits for **Enter** to reopen the source menu or **Escape** to exit. Other keys are ignored for now, reserving `F`/PageUp/PageDown/arrows for later search and page navigation. A direct local, URL or GitHub README launch gains the same repeatable-session behavior, so users can open a second document without restarting the tool. When stdin or stdout is redirected, the CLI completes without waiting. The separate `mdview source PATH` disk-backed raw source pager retains its own navigation.

[mdview documentation](Blackboard/Tools/MdView/Docs/Document-End-Navigation/en.md) · [mdview implementation status](Blackboard/Tools/MdView/IMPLEMENTATION_STATUS.md). New tests are committed; a fresh full Windows test run is pending.

## Future README search and incremental scanner (2026-11-04)

For future visual paging, we now have an **independent forward-only Markdown source-line scanner** in ExchangeHouse. It preserves physical newlines and UTF-16 source offsets and tracks top-level fenced-code state without materializing the complete document. It is **not** wired into mdview or advertised as a complete Markdown parser; the existing renderer remains unchanged. [Scanner design](ExchangeHouse/Markdown/Docs/Incremental-Source-Scanner/en.md).

We also captured a **design-only** next-stage navigation idea: a few common English README subjects (e.g., Installation, Build, Compilation, Configuration), a heading outline and on-demand indexed phrases. A future `F` action would choose/add a query and `N/P` would navigate its hits. Match positions should use small, disk-backed, content-digest-bound indexes rather than copying every paragraph. **Search keys are not active yet.** [Indexed search plan](Blackboard/Tools/MdView/Docs/Indexed-Search/en.md).

New scanner and test code has been committed, but a fresh full Windows/.NET test run is still needed before this increment can be considered verified.


## XML API documentation and warning-free build (2026-11-07)

We completed the public XML documentation of the remaining Markdown cache and mdview APIs, including source modes, freshness statuses, constructors and all positional record parameters. Two ExchangeHouse test assertions were adjusted to `Assert.Single` to resolve xUnit analyzer suggestions; GitHub Actions was upgraded to Node 24-compatible `actions/checkout@v5` and `actions/setup-dotnet@v5`, and reported local Markdown link errors were fixed. No source behavior was intentionally changed, and **XML documentation generation remains enabled**.

[GitHub Actions run 42311109763](https://github.com/afrowaveltd/Toolbox/actions/runs/42311109763) confirmed a fresh Linux build with **0 warnings and 0 errors**, successful tests, Windows UI regressions and Markdown links. [Maintenance and verification policy](Docs/Warning-Free-Build/en.md).

## Incremental Markdown block ranges (2026-11-07)

The next stage after streamed physical lines is now implemented in `ExchangeHouse.Markdown`: `MarkdownIncrementalBlockScanner` emits **completed, non-overlapping UTF-16 source spans** for conservative top-level headings, simple paragraphs, blank lines and fenced code, without allocating a copy of entire code blocks. List/quote structures remain opaque until ownership semantics can be faithfully modeled. The new 16-case regression suite checks mixed line endings, Unicode offsets, source coverage, laziness, huge code ranges, explicit unclosed fences and parity with the existing parser on a limited flat subset.

[Block range architecture](ExchangeHouse/Markdown/Docs/Incremental-Block-Spans/en.md) · [Existing whole-document parser](ExchangeHouse/Markdown/README.md). `mdview` is unchanged: this is infrastructure for later visual paging, not yet a rendered stream or the planned `F/N/P` indexed search.

## Conservative Markdown quote/list streaming (2026-11-07)

The incremental parser groundwork now includes `MarkdownIncrementalContainerScanner`, a separate `IAsyncEnumerable<MarkdownContainerLine>` adapter for **explicit quote depths, nested ordered/unordered list markers, provable indented continuations, and code-fence lifetime inside quotes**. It retains only the current bounded source line and a maximum of 32 list frames rather than buffering the full document. Quote-depth mismatch interrupts a quote-owned fence instead of swallowing unrelated Markdown as code.

This is a **deliberately conservative** lexical metadata layer, not a full CommonMark AST, a formatted streaming renderer or a change in `mdview`. Lazy continuations, deferred loose-list blank ownership and an ordinary list's outer ownership of nested quotes remain future work. Added 18 focused tests, including restricted parity checks with the current full `MarkdownParser`. CI passed on [run 42314716633](https://github.com/afrowaveltd/Toolbox/actions/runs/42314716633) with **6,603 passing tests, zero warnings and zero errors**, plus successful Windows UI and link-validation jobs.

[Explicit container architecture](ExchangeHouse/Markdown/Docs/Explicit-Container-Scanner/en.md) · [ExchangeHouse status](ExchangeHouse/Markdown/IMPLEMENTATION_STATUS.md).

## Markdown: independent outer and quoted-list ownership (2026-11-07)

The incremental Markdown scanner now distinguishes **the ordinary list item that owns a quote** from **the list item within the quote**. New init-only metadata on `MarkdownContainerLine` records the outer owner's source offset, depth and original marker, while existing `ListOwnerUtf16Offset` continues to identify the quote-local owner. This preserves both nesting identities across nested list items, quoted code fences and changes in quote depth, without buffering a full document or changing an existing constructor.

Quoted/list-owned fence parsing was also hardened to strip **only the owning item's required indentation**. Extra indentation after that prefix stays significant: a delimiter with four remaining spaces is code text, not a closing fence. The new 17-case regression suite compares supported owner and fence metadata with the established full `MarkdownParser` and covers Unicode and mixed line endings.

The normal `mdview` renderer and all user-facing navigation remain unchanged. This is a conservative source-metadata milestone, not complete CommonMark, paged rendering or active `F/N/P` search. [Technical ownership guide](ExchangeHouse/Markdown/Docs/Dual-List-Ownership/en.md) · [Tests](ExchangeHouse/Markdown.Tests/README.md).

## Streaming Markdown: deferred list separators and lazy continuations (2026-11-07)

The optional incremental scanner now tracks **lazy list-paragraph continuations** without indentation, provided no blank or interrupting block starter has broken the active item. The new `MarkdownDeferredListOwnershipScanner` then buffers a **bounded run of blank lines** until an explicitly indented continuation proves their owner in the same quote/outer-list scope. It preserves original line endings and UTF-16 source offsets, and leaves unresolved blanks unowned.

The lookahead has two explicit resource limits—**1,024 pending lines** and **1 Mi UTF-16 characters** by default—and fails clearly rather than silently truncating source. The additive `MarkdownContainerLine` properties `IsLazyListContinuation` and `ListBlankOwnerUtf16Offset` preserve the existing positional constructor signature. Twenty new regression cases exercise loose lists, quote scopes, lazy paragraphs, interruption, bounded lookahead and source fidelity.

[GitHub Actions run 42359532384](https://github.com/afrowaveltd/Toolbox/actions/runs/42359532384) verified Linux/Windows tests and Markdown links, with **6,640 passing tests and zero compiler warnings/errors**. The established full Markdown parser and `mdview` remain unchanged. [Detailed deferred-ownership contract](ExchangeHouse/Markdown/Docs/Deferred-List-Ownership/en.md).
