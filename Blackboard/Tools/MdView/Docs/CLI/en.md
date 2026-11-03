# mdview CLI: first executable slice

**Status:** in-repository .NET 10 executable. The command parser and TextFiles extraction passed the user-provided Windows solution baseline; the new Spectre interactive source picker is authored and awaiting its own test rerun. No public package or system-wide installed command.

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

With no arguments, a Spectre.Console menu offers Local Markdown file, HTTPS URL, GitHub README and Exit. The Local choice opens the existing SpectreFileBrowser with a Markdown filename filter, folder navigation and search; Escape inside the browser returns to the source menu. URL and GitHub use Spectre text prompts. [Interaction details](../Interactive-Source-Picker/en.md). Future work includes in-document navigation, back/forward history and theme choice. Explicit source commands bypass the menu. The argument parser is independent and unit tested. GitHub owner/repo capitalization is left as input for network requests and normalized by the source adapter for cache keys; local filename casing is preserved.

## Rendering and storage

Local documents are decoded with `TextFilePreviewReader.ReadCompleteAsync` (including supported BOM encodings). Remote documents use the cache-validated UTF-8 blob path; the HTTP and GitHub adapters expose explicit `Downloaded`, `NotModified`, `Stale`, or `Error` status. Stale fallback is printed to stderr, never silently represented as freshly verified. The app uses `MarkdownSpectreRenderer` and does not embed a second parser. The command currently reads the whole Markdown string and renders all lines; it does **not** use `IAsyncEnumerable` streaming Markdown blocks or screen paging.

## Exit and error handling

`0`: displayed successfully or user requested help/quit. `1`: I/O, source validation or network failure with no usable cache. `2`: invalid command/menu arguments or an unsupported local extension. Exceptions are caught and shown without printing fetched Markdown as executable markup. A large-document notice appears above one MiB, but does not cap output.

## Deliberate exclusions and follow-up

- No automatic following of HTTP redirects or GitHub renamed repository redirects.
- No authenticated GitHub/API clients, network query URLs or arbitrary remote protocols.
- No `Docs/**` navigation, external Markdown links, anchors, pager or reverse search.
- Current parser/renderer remain in-memory; a 1 GB Markdown benchmark is still a target, not demonstrated.
- The no-argument Spectre FileBrowser and virtual-terminal picker tests are implemented, but the new test run, full CLI end-to-end rendering checks and live terminal acceptance remain pending.

[README](../../README.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md).
