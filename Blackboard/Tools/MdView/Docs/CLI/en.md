# mdview CLI: first executable slice

**Status:** implemented as in-repository .NET 10 console project, 2026-11-03. No public package or standalone installed command. Compile/test verification pending.

## Source selection

Positional forms:

```text
mdview PATH
mdview local PATH
mdview URL
mdview url HTTPS_URL
mdview git OWNER REPO
mdview git OWNER/REPO
mdview github:OWNER/REPO
mdview --help
```

With no arguments, an elementary numbered Local / URL / GitHub menu prompts for the source; future work will add real FileBrowser navigation, back/forward history and theme choice. Explicit source commands bypass the menu. The argument parser is independent and unit tested. GitHub owner/repo capitalization is left as input for network requests and normalized by the source adapter for cache keys; local filename casing is preserved.

## Rendering and storage

Local documents are decoded with `TextFilePreviewReader.ReadCompleteAsync` (including supported BOM encodings). Remote documents use the cache-validated UTF-8 blob path; the HTTP and GitHub adapters expose explicit `Downloaded`, `NotModified`, `Stale`, or `Error` status. Stale fallback is printed to stderr, never silently represented as freshly verified. The app uses `MarkdownSpectreRenderer` and does not embed a second parser. The command currently reads the whole Markdown string and renders all lines; it does **not** use `IAsyncEnumerable` streaming Markdown blocks or screen paging.

## Exit and error handling

`0`: displayed successfully or user requested help/quit. `1`: I/O, source validation or network failure with no usable cache. `2`: invalid command/menu arguments or an unsupported local extension. Exceptions are caught and shown without printing fetched Markdown as executable markup. A large-document notice appears above one MiB, but does not cap output.

## Deliberate exclusions and follow-up

- No automatic following of HTTP redirects or GitHub renamed repository redirects.
- No authenticated GitHub/API clients, network query URLs or arbitrary remote protocols.
- No `Docs/**` navigation, external Markdown links, anchors, pager or reverse search.
- Current parser/renderer remain in-memory; a 1 GB Markdown benchmark is still a target, not demonstrated.
- An interactive Spectre FileBrowser and automated CLI end-to-end tests should follow after successful CI of these projects.

[README](../../README.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md).
