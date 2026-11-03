# Temporary on-disk cache for remote Markdown

## Current experimental implementation

`RemoteMarkdownTempDocument.DownloadAsync(HttpClient, Uri, ...)` is a
read-only spooling helper in the Showcase sample project. It is not wired
into the interactive Markdown file picker yet.

The helper requests a trusted **HTTPS** document using `ResponseHeadersRead`,
copies bytes directly to a unique file under `Path.GetTempPath()` in small
bounded buffers, and closes the HTTP response before returning the session.
The original bytes are preserved. A paged viewer may seek the temporary file
freely without keeping the network connection open while a user reads page 150.

```csharp
using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
await using var document = await RemoteMarkdownTempDocument.DownloadAsync(
    http, new Uri("https://example.org/guide.md"));

await using var file = document.OpenRead();
// Build a line/page index and render from the seekable local stream.
// document.DisposeAsync() deletes the temporary copy.
```

The default disk limit is 16 GiB; the caller can choose a smaller `maxBytes`.
Declared Content-Length is checked early, and actual received bytes are
checked on each chunk so chunked transfers cannot bypass the limit. Partial
files are deleted on download failure and cancellation. Successful sessions
delete their file on dispose. Close all paging streams before disposing.

Use the test-only optional tempDirectory override to isolate file lifecycle
regressions. The default location remains the operating system Temp.

## Operational limitations

- HTTPS URLs only; userinfo and downgraded HTTP redirect are rejected.
- The supplied HttpClient owns proxies, TLS validation, redirect policy,
  connection timeouts and cancellation.
- This helper is **not hardened against SSRF** if an untrusted caller supplies
  URLs in a server process: host/DNS/IP allowlisting must be enforced there.
- This step handles raw bytes, not UTF-8/UTF-16 validity or Markdown parsing.
  Validate text encoding and control characters before terminal output.
- Temporary files are deleted on normal dispose and handled errors, but a
  process crash or power failure can leave a file behind. A future cleanup
  service should remove stale files safely without touching other sessions.
- The first implementation returns after downloading the complete file.
  Concurrent streaming display/translation while download is in progress
  remains a separate design and must synchronize access to growing files.
- Restrict local file permissions and avoid storing private documents on
  an unsafe shared Temp partition.

## Why not pause the HTTP stream?

A long-running read session may outlive HTTP client/server idle timeouts,
reverse proxies, authentication sessions and application lifetimes. Download
once to disk, close HTTP, then seek/page locally. For enormous documents,
reuse the same cache rather than downloading the original again for every
language or page turn.

[Live translation design](../Live-Markdown-Translation/en.md) describes how
the raw spooling and future UnderstandMe processing can overlap safely.

## Separate reusable offline cache added (2026-11-03)

The disposal semantics above remain unchanged for `RemoteMarkdownTempDocument`: it is a temporary **session spool**, not persistent storage. The new, independent `ExchangeHouse.MarkdownRemoteDocumentCache` supports stable per-user copies across launches, HTTPS ETag/Last-Modified revalidation, offline fallback, explicit export and explicit per-URL removal. Working Markdown page indexes remain temporary. See [persistent cache documentation](../../../../../ExchangeHouse/Markdown/Docs/Offline-Document-Cache/en.md). The Showcase remote picker is not yet wired to this service.
