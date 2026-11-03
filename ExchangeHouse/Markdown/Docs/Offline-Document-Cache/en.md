# Persistent offline cache for remote Markdown documents

**Status:** additive experimental `ExchangeHouse.Markdown` API, draft
PR #3 (2026-11-03). This is a reusable **library foundation**, not a
finished Docs Reader or integrated GitHub picker.

## Why two kinds of storage?

A source document downloaded once should often remain available on the
next application launch. Its temporary *working indexes*, however, are
reproducible and can be removed after the reading session.

The existing `RemoteMarkdownTempDocument` in the Blackboard Showcase
sample keeps its intentionally ephemeral contract: its temp download
is **deleted on Dispose**. No behavior of that API was changed.

The new `MarkdownRemoteDocumentCache` stores independently reusable
source copies in a **per-user application cache**, typically:

- Linux: `$XDG_CACHE_HOME/afrowave/markdown-documents` or
  `~/.cache/afrowave/markdown-documents`
- Windows: the per-user LocalApplicationData under
  `Afrowave/MarkdownDocuments`
- Any caller-provided dedicated `directory`.

This avoids relying on ephemeral OS Temp lifetimes. It is still a
**cache**, not a guaranteed permanent archive: administrators or the
operating system may purge it. Users can explicitly export important
documents to their own chosen directory.

## API

```csharp
using Afrowave.Toolbox.ExchangeHouse.Markdown;

using var http = new HttpClient();
var cache = new MarkdownRemoteDocumentCache();
var url = new Uri(
    "https://raw.githubusercontent.com/afrowaveltd/Toolbox/master/README.md");

// First read downloads. Later reads use HTTP ETag / Last-Modified
// conditional requests when the origin provides those validators.
var copy = await cache.GetAsync(http, url);
Console.WriteLine(copy.LocalPath);
Console.WriteLine($"Cached: {copy.FromCache}, stale: {copy.IsStale}");

// Reopen even with no network request:
var offline = await cache.GetAsync(http, url,
    MarkdownRemoteCacheMode.OfflineOnly);

// Build temporary page indexes independently. Disposing those
// indexes does not delete the cached original file.
await using (var pages = await MarkdownDiskPageIndex.BuildAsync(
    offline.LocalPath))
{
    var first = await pages.ReadPageAsync(0);
}

// Export a private copy out of the cache on explicit user request.
await MarkdownRemoteDocumentCache.SaveCopyAsync(
    offline, "/home/user/Documents/Toolbox-README.md");

// Explicitly remove cached copies of just this URL when desired.
await cache.RemoveAsync(url);
```

`MarkdownRemoteCacheMode` supports:

- `OnlineFirst` (default): if a cached version exists, send
  `If-None-Match` and/or `If-Modified-Since`; HTTP 304 reuses
  the complete local file without downloading it.
- `OfflineOnly`: never call HTTP; use the latest valid cached
  manifest and file, or report a missing copy.
- `ForceRefresh`: download a new version without conditional
  headers, preserving the earlier snapshot.

If a connection fails, times out, or the server responds with HTTP 5xx
and a cached copy exists, online-first returns that copy with
`IsStale=true`. A successful 304 is `FromCache=true` and
`IsStale=false`. HTTP 4xx errors are **not silently mistaken for
successful offline revalidation**. User cancellation is propagated.
Consumers should show a visible offline/stale indicator.

## Safety, disk, and lifetime

- Only trusted absolute **HTTPS** URLs without embedded credentials.
  The supplied `HttpClient` controls redirects, TLS, proxy, timeouts
  and authentication.
- URL-derived cache keys are SHA-256 hashes rather than literal URL
  filenames. The per-URL manifest points to a UUID-named immutable
  source version. File path components loaded from manifests are
  validated to stay inside the designated cache directory.
- A small fixed **64-KiB network buffer** streams downloads directly
  to disk. Default per-document ceiling: **256 MiB**, configurable by
  the caller; declared and actual byte counts are both checked.
  This is *not* a total cache-directory quota.
- New source versions are written completely before being renamed;
  the manifest is atomically replaced only after the version exists.
  Prior versions remain readable while open or until explicit
  removal. A crash may leave unreferenced versions/partial files;
  eventual safe sweeping is future work.
- On Unix, create the cache directory for the user and mark source
  files owner-readable/writable. Treat private/authenticated sources
  specially: use an **isolated cache root per identity**, never share
  cached responses across users/tenants.
- This API is for local interactive use, **not** an SSRF-safe
  arbitrary-URL server endpoint. Validate allowed hosts/DNS/IP at the
  network boundary in any future server product.
- The document cache does not parse HTML/Markdown or bypass strict
  UTF-8/control validation in the page index. A local file can still
  be removed externally, so consumers must handle a missing path.
- No background network polling, automatic cache retention policy,
  full-cache disk quota or cross-process cache locking yet.
  A successful online check is not a claim of perfect remote freshness
  when a server omits HTTP validators or misconfigures caching.

## Planned Docs Reader integration

The future Docs Reader can use the same per-user source cache for local
`README.md`/GitHub raw Markdown/HTTPS sources, then create disposable
page/context indexes on top. On slow or unavailable connectivity it
should offer:

1. **Use saved copy (offline)**, explicitly marked as potentially old.
2. **Check for updates**, using conditional HTTP GET.
3. **Refresh now**, skipping validators.
4. **Save a permanent copy**, outside the cache.
5. **Remove saved copy**, with clear confirmation and awareness of
   any currently open read sessions.

The current library provides the underlying operations; this menu and
GitHub tree provider are **not implemented**.

[Remote temporary spooling in Showcase](../../../../Blackboard/Samples/Blackboard.Showcase/Docs/Remote-Markdown-Cache/en.md) ·
[Virtual Docs Reader proposal](../../../../Docs/Unified-Docs-Reader-Proposal/en.md) ·
[Disk-backed page indexes](../Disk-Backed-Paging/en.md).
