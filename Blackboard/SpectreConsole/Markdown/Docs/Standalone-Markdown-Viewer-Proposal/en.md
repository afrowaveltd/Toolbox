# Proposal: standalone terminal Markdown viewer

**Status:** Idea / design proposal only, captured 2026-11-02. **Not implemented.**
**Trigger:** Revisit after the ExchangeHouse Markdown parser and the Blackboard Spectre renderer have reached a mutually agreed stable feature/test baseline. This document does not claim CommonMark completeness or a product release.

## User goal

Read a real Markdown document or repository README directly in a terminal, including Windows Terminal, Debian and remote SSH sessions, without opening a browser or manually downloading the README. The app must work offline for local files, display source safely, and support the **entire document** rather than truncating larger help files.

The provisional command name `mdview` is **not a finalized product name**.

## Proposed entry points

```text
mdview
mdview README.md
mdview ./Docs/Help/en.md
mdview github:afrowaveltd/Toolbox
mdview git afrowaveltd toolbox
mdview git AfrowaveLtd Toolbox
mdview local ./Docs/Help/en.md
mdview url https://example.org/manual.md
mdview https://example.org/manual.md
mdview --source github --repository afrowaveltd/Toolbox
```

With **no arguments**, open an interactive menu:
1. Local file: reuse Spectre FileBrowser, directory navigation and `*.md;*.markdown` filtering.
2. HTTP(S) Markdown: enter an explicit URL; HTTPS is preferred. Validate redirects and content before rendering.
3. GitHub repository: enter `owner/repository`, `github:owner/repository` or a recognized GitHub repository URL.

An explicit path/URL/repository selection bypasses the source menu. Back/exit returns to the menu instead of exiting unexpectedly.

### CLI grammar and forgiving inputs

A compact Linux-style positional command is preferred: `mdview git OWNER REPO`,
`mdview local PATH`, or `mdview url URL`. A bare local path, direct
HTTP(S) URL and `github:OWNER/REPO` are aliases. Parse command verbs
case-insensitively and report usage for missing/excess arguments; preserve
literal local paths, which can be case-sensitive on Linux. Quote paths
containing spaces using ordinary shell quoting.

**GitHub repository owner and repository parameters are already case-insensitive**
in the official REST API. `afrowaveltd toolbox`, `AfrowaveLtd Toolbox`
and `AFROWAVELTD TOOLBOX` need only **one API request**, not a loop
trying capitalization permutations (which would waste quota and latency).
For display and cache identity, resolve the canonical `full_name` from the
repository metadata when needed, or use a normalized case-insensitive cache
key; the originally typed casing should not matter. **Do not** apply that
rule to branch/tag names, file paths or Linux filesystem paths, which can
be case-sensitive.

A repository truly missing from GitHub should produce a useful not-found
message, not a speculative search across guessed spellings. Account/repo
renames can be handled using the API's documented redirects. A private
repository may also appear as 404 to a caller without access.

## Source adapters; one document renderer

A future `IMarkdownDocumentSource` (name provisional) should return a validated document, origin, optional revision and a base URI/path for navigation. Proposed providers:

- **Local:** local filesystem, using the present FileBrowser and strict full-document Unicode decoding; never modify the selected file.
- **HTTP(S):** bounded-buffer network download and strict text validation, with timeout, cancellation, redirect policy, status/content-type handling, and warnings for insecure HTTP. Do not execute code or interpret fetched content as terminal markup.
- **GitHub README:** accept any capitalization of owner/repository without extra retry combinations; normalize the resulting canonical repository identity rather than performing fuzzy global search. GitHub REST provides `GET https://api.github.com/repos/{owner}/{repo}/readme` and a raw Markdown media type (`Accept: application/vnd.github.raw+json`), using the default branch when `ref` is omitted. It finds GitHub's preferred README rather than guessing `main`, `master`, `README.md`, or `README.MD`. Follow reported path/ref for linked documents. Optional branch/tag/commit selection and subdirectory READMEs can follow in later versions.

GitHub's unauthenticated public REST API has a rate limit; use conditional requests (ETag/Last-Modified) and an optional disk cache to avoid unnecessary downloads. Add opt-in credentials for private repositories later, with secure secret storage and no token leakage in logs, output or links.

Current external references:
- [GitHub API: Get a repository README](https://docs.github.com/en/rest/repos/contents#get-a-repository-readme)
- [GitHub README conventions](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes)
- [GitHub REST API rate limits](https://docs.github.com/en/rest/using-the-rest-api/rate-limits-for-the-rest-api)
- [GitHub REST API: get a repository (case-insensitive owner/repo)](https://docs.github.com/en/rest/repos/repos#get-a-repository)
- [CommonMark specification: block and inline parsing](https://spec.commonmark.org/)

The viewer, not ExchangeHouse, should own fetching, caching, security and navigation. Rendering remains delegated to Blackboard's Markdown Spectre component with optional TextMate language highlighter. Do not introduce GitHub or HTTP dependencies into the parsing library.

## Document navigation and accessibility

The first useful version should show headings, lists, quotes, code, inline links, link destinations and meaningful alt text for images. Follow local relative file links or GitHub repository-relative links in their correct context; resolve `./`, `../`, repository-root paths, fragments and documented branch/ref without silently switching to a different revision.

Desired later conveniences: an outline of headings, `/` text search, `n` for next match, Back/Forward history, optional theme selection, bookmarks, and a clear source/revision indicator. These are *proposed capabilities*, not parser claims.

Terminal-only image fallbacks should be text/alt information; image fetching or sixel/kitty graphics are separate optional enhancements. All output remains safe literal terminal text and code fences are **never executed**.

## Large documents and resource safety

**Do not use an arbitrary 64 KiB content cutoff** or silently display only the first N lines of a help file. Continue full-document support, including lengthy localized help and TextScape documentation. Inform the user when a file may take time to display. Local documents must work offline.

The existing Showcase reader validates a complete document with a small decoding buffer but materializes both its full string and Markdown model in RAM. Before promising unlimited practical size, investigate a line/block index, disk-backed temporary cache, paged presentation and bounded resident memory (possibly leveraging Toolbox's future storage tools). Network input also needs sane protections against unexpectedly large/decompression-bomb responses, but resource safeguards must be explicit rather than silently truncating valid Markdown.

A `404`, authentication error, rate limit, invalid encoding, timeout or unavailable network must produce an actionable message and preserve the user interface. Cached content should display its source and fetch date to distinguish stale/offline documents.

## Incremental parsing and Unix-style paging (research, not implemented)

The user proposed rendering completed Markdown blocks as they arrive using
`yield return`, and a pager similar to `less` so a multi-gigabyte document
need not be loaded into RAM or dumped into terminal scrollback. We should
retain this as a **separate parser/renderer architecture milestone**.

The existing `MarkdownParser.Parse(string)` produces a full in-memory
document with absolute source offsets, list/quote ownership and inline
nodes. `TextFilePreviewReader.ReadCompleteAsync` reads bytes in chunks,
but **still materializes the complete string** and parse model; this is not
streamed Markdown rendering.

A potential **new optional incremental interface** could look like:

```csharp
IAsyncEnumerable<MarkdownBlockEvent> ParseBlocksAsync(
    TextReader source, MarkdownStreamOptions options,
    CancellationToken cancellationToken = default);
```

This is an illustrative API sketch, not a committed signature. A parser state
machine would track open fences, quote/list stacks, current paragraph, pending
blank separators, source positions, indentation and minimal lookahead. It
could yield **completed and validated** leaf blocks rather than returning
the whole document. Markdown is not XML: it does not have universally paired
closing tags, and some inline constructs depend on reference definitions
that may be declared later. CommonMark explicitly separates block discovery
from inline resolution, so a strictly single-pass renderer cannot
automatically promise identical results for every legal input.

Proposed policy for deferred references: retain a small definition index,
optionally store it in a disk-backed structure or run a two-pass scan on
seekable sources; non-seekable HTTP streams may need a disk spool. Avoid
silently losing link destinations or producing permanently incorrect
formatting merely to achieve one-pass output. Preserve bounded lookahead
and emit when the block is safe to finalize, or explicitly support
placeholder/patch behavior in an interactive screen-buffer renderer.
Complex containers or a single extremely long unbroken block may still
need disk-backed buffering to achieve bounded RAM.

A separate `MarkdownPager` would accept formatted **visual lines** (after
Unicode width, terminal resize and soft wrapping), not merely source lines.
Suggested keyboard behavior: Space/PageDown next page, Enter next line,
`b`/PageUp previous page, `/` search, `n` next match, `q` quit.
Forward-only streams could initially permit forward paging only; reverse
navigation, hyperlinks, heading outline and search across earlier content
require a bounded screen cache plus a disk-backed source/block/line index,
or a repeatable seek/reparse path. Redirecting output to a pipe should
detect noninteractive terminals and avoid escape sequences; optional
`mdview ... | less -R` is a compatibility path, not a replacement for a
native pager. On Unix, `less` expects terminal control and line wrapping
behavior appropriate to pipes.

A **1 GB Markdown document** is a performance/reliability **target to
benchmark**, not a presently proven capability. Measure resident memory,
allocations, throughput, wrap/search latency, non-ASCII/emoji, links,
nested containers, malformed/unterminated fences, slow HTTP and early
cancellation. Streaming text validation must protect the entire rendered
prefix, including control characters and encoding errors that occur late
in a stream. For files that must be rejected before *any* content is shown,
validate in a separate pass first (or spool and validate), then render.

No silent truncation. Explicit cancel, hardware/resource failures and
potentially large disk cache are separate operational concerns. The
parser core, source adapters and terminal pager remain three independent
layers with focused tests.

## Suggested delivery slices (after parser completion)

1. **Local standalone viewer:** use existing complete local Markdown loading + FileBrowser and a reusable navigation/keyboard host. Test Windows, Debian, SSH, UTF-8/16/32, long documents and code escape safety. Keep the streaming interface as a separately tested future enhancement, not an implicit dependency of the first version.
2. **GitHub README provider:** `owner/repo` input, official preferred README API, default ref, correct errors and safe cache; test both sample repositories and fake HTTP handlers.
3. **General HTTP(S) provider:** explicit URL, safe redirects, streaming validation, retries/timeouts, offline cache, test servers and error paths.
4. **Linked document navigation:** relative links, anchors, history, search, outline and future language selection for `Docs/**/en.md`.
5. **Incremental parser + native pager (research track):** design block events, reference-definition resolution, optional two-pass/spooled input, bounded resident memory, disk-backed reverse/search index, real terminal wrapping and multi-GB stress tests. Do not promise unlimited size before benchmarks.

Actual naming, assembly placement, public API, package policy and exact keybindings are open questions. **No new project should be created solely because of this proposal.**

## Existing building blocks

- [ExchangeHouse Markdown parser](../../../../../ExchangeHouse/Markdown/README.md)
- [Blackboard renderer README](../../README.md)
- [Blackboard Showcase](../../../../Samples/Blackboard.Showcase/README.md), which already opens whole local Markdown files in read-only mode.

[Markdown implementation status](../../IMPLEMENTATION_STATUS.md).

## 2026-11-03 — persistent cache contract

The intended reader cache retains successfully verified remote documents **across application restarts**, uses conditional HTTP checks where supported and offers a clearly labeled stale copy when offline. Normal exit does not wipe cached content; the user can inspect/clear viewer-owned entries. Persistent storage lives outside disposable OS temporary directories by default, with resource budgets and atomic validated writes. These are **acceptance criteria, not an implementation**. See the [offline-first cache policy](Document-Cache-Policy/en.md) for state transitions, security decisions and concrete future tests.
