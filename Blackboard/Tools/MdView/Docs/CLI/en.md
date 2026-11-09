# mdview CLI: first executable slice

**Status:** in-repository .NET 10 executable. The command parser and TextFiles extraction passed the user-provided Windows solution baseline; the new Spectre interactive source picker is authored and awaiting its own test rerun. No public package or system-wide installed command.

## Source selection

Positional forms:

```text
mdview PATH
mdview local PATH
mdview source PATH           # interactive raw UTF-8 line pager
mdview URL
mdview url HTTPS_URL
mdview git OWNER REPO
mdview git OWNER/REPO
mdview github:OWNER/REPO
mdview --help
```

With no arguments, a Spectre.Console menu offers Local Markdown file, HTTPS URL, GitHub README and Exit. The Local choice opens the existing SpectreFileBrowser with a Markdown filename filter, folder navigation and search; Escape inside the browser returns to the source menu. URL and GitHub use Spectre text prompts. [Interaction details](../Interactive-Source-Picker/en.md). Future work includes in-document navigation, back/forward history and theme choice. Explicit source commands bypass the menu. The argument parser is independent and unit tested. GitHub owner/repo capitalization is left as input for network requests and normalized by the source adapter for cache keys; local filename casing is preserved.

## Rendering and storage

Local documents are decoded with `TextFilePreviewReader.ReadCompleteAsync` (including supported BOM encodings). Remote documents use the cache-validated UTF-8 blob path; the HTTP and GitHub adapters expose explicit `Downloaded`, `NotModified`, `Stale`, or `Error` status. Stale fallback is printed to stderr, never silently represented as freshly verified. The app uses `MarkdownSpectreRenderer` and does not embed a second parser. The command currently reads the whole Markdown string and renders all lines; it does **not** use `IAsyncEnumerable` streaming Markdown blocks or screen paging.

## Exit and error handling

`0`: displayed successfully or user requested help/quit. `1`: I/O, source validation or network failure with no usable cache. `2`: invalid command/menu arguments or an unsupported local extension. Exceptions are caught and shown without printing fetched Markdown as executable markup. A large-document notice appears above one MiB, but does not cap output.

## Deliberate exclusions and follow-up

- No automatic following of HTTP redirects or GitHub renamed repository redirects.
- No authenticated GitHub/API clients, network query URLs or arbitrary remote protocols.
- No `Docs/**` navigation, external Markdown links, anchors, **formatted Markdown pager**, or reverse search. An explicitly raw, indexed source-line pager is available via `source PATH`.
- Current parser/renderer remain in-memory; a 1 GB Markdown benchmark is still a target, not demonstrated.
- The no-argument Spectre FileBrowser and virtual-terminal picker tests are implemented, but the new test run, full CLI end-to-end rendering checks and live terminal acceptance remain pending.

[README](../../README.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md).

## Syntax highlighting (2026-11-04)

`mdview` now creates a Markdown theme with the optional TextMate grammar adapter, so fenced blocks in supported bundled languages are colored whether the document came from a local file, HTTPS or GitHub README. This is **syntax highlighting**, not code execution. The language follows the Markdown fence tag; missing tags and unsupported language IDs remain plain. Language names and common filename suffix aliases are case-insensitive and the adapter checks the entire bundled grammar catalog. See the [supported-language guide](../../../../SpectreConsole/Markdown.TextMate/Docs/Supported-Grammars/en.md). No automatic guessing from the code body, remote grammar downloads or 1 GB streaming rendering are implied.

## GitHub owner-only selection (2026-11-04)

```text
mdview afrowaveltd
mdview git afrowaveltd
mdview github:afrowaveltd
```

These forms open the public [GitHub owner/repository selector](../GitHub-Owner-Discovery/en.md). A short tree is shown for up to 20 repositories; the paged `SelectionPrompt` can filter names and descriptions or cancel with Escape. Only the selected README is downloaded. The catalog is cached independently of README contents for 30 minutes and supports explicit stale/offline fallback. The first 1,000 repositories are displayed with a warning if more exist. Bare owners are interpreted as GitHub; use `local PATH` for local paths without extensions. Noninteractive scripts should use `git OWNER REPOSITORY`.

## Disk-backed raw source pager (2026-11-04)

`mdview source PATH` opens an opt-in, strict UTF-8 **raw source** pager on local files. It uses a temporary disk-backed physical-line index and exposes Next page, Previous page, Go to line and Exit. Unlike normal `mdview local PATH`, it does **not** parse or syntax-color Markdown; normal fully rendered behavior remains unchanged. Indexing validates all bytes before display, limits index and page allocations and makes no silent truncation. A formatted, visual-row-aware streaming Markdown pager is still future work.

[Source pager guide](../Raw-Source-Pager/en.md) · [Disk index](../../../../TextFiles/Docs/Disk-Line-Index/en.md).

## Reopen the source menu after reading (2026-11-04)

After any successfully displayed **fully rendered** Markdown document in an interactive terminal, `mdview` waits for a single completion action: **Enter** reopens the main source menu, **Escape** exits and other keys are ignored. The behavior applies to direct local, URL and GitHub invocation as well as documents chosen from the no-argument menu. This provides repeated reading without restarting `mdview`.

Redirected stdin/stdout or a noninteractive console skips the prompt and exits normally. The `--help` and error paths retain their exit behavior. The independent `mdview source PATH` raw pager is not double-prompted because it already supports its own page navigation. This change does not yet add `F` search, PageUp/PageDown or formatted Markdown paging.

[Document completion details](../Document-End-Navigation/en.md).

## Planned relative links and document history (2026-11-09)

The existing viewer does **not** yet support interactive link navigation,
even though the renderer can display Markdown link text. The accepted
[design and security contract](../Link-Navigation/en.md) covers relative
local file paths, HTTPS bases, GitHub repository paths/refs, in-document
anchors, keyboard selection and bounded Back/Forward history. All are
planned features, not existing commands.

## Opt-in local link browser (2026-11-09)

```text
mdview browse README.md
```

This separate interactive mode uses a validated local snapshot, the
existing Markdown parser/renderer, a selectable list of relative
Markdown links and bounded Back/Forward history. The initial document
directory is the permitted lexical navigation root. Only `.md` and
`.markdown` documents inside that root are allowed; source files above
1 MiB UTF-8 are rejected. External links and online/GitHub navigation
are not yet supported. A selected anchor is recorded but scrolling
to the heading is not implemented. A failed link does not replace
the currently open document. See
[Link Navigation](../Link-Navigation/en.md).

Existing `local`, `url`, `git`, `source` and `preview` behavior
remains unchanged; the default completion prompt is not used in
`browse` mode.
