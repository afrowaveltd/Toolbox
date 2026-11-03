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

## Total disk quota, usage reporting and explicit pruning (2026-11-03)

The reusable source cache now has a **default 1 GiB combined disk budget**
for cache-owned documents, version manifests and incomplete cache writes.
This is configurable independently of the default **256 MiB
per-document** transfer limit. These are **disk byte limits**, not a RAM
cache allocation, and do not change the temporary 8-byte/page source
offset and 512-byte sparse context indexes.

```csharp
var cache = new MarkdownRemoteDocumentCache(
    maxTotalCacheBytes: 128L * 1024 * 1024);
var usage = await cache.GetUsageAsync();
Console.WriteLine(
    $"Used: {usage.UsedBytes:N0} bytes in {usage.FileCount:N0} files");
Console.WriteLine($"Remaining: {usage.RemainingBytes:N0} bytes");

// Explicit maintenance, not an automatic eviction policy:
var removed = await cache.PruneObsoleteVersionsAsync();

// Only after verifying readers no longer need older versions:
var removedNow = await cache.PruneObsoleteVersionsAsync(TimeSpan.Zero);
```

**Retention rules:**

- Quota checks run before a new transfer and on each received chunk,
  counting old versions as well as the new one. Small metadata headroom
  is reserved, and the manifest is checked before publishing. A failed
  refresh cleans up its unfinished or unreferenced new version; an
  earlier valid offline manifest remains untouched.
- `GetUsageAsync` performs a sequential enumeration of **cache-owned
  filenames only**, without building a content catalog or reading
  Markdown bodies into RAM. Unrelated files and separately managed
  page indexes are excluded. Existing cached files can still be opened
  **offline when the cache is already over its configured quota**.
- When the disk budget cannot hold the next version, the download
  fails with `InvalidDataException`. **The cache does not silently
  delete any document merely to make room.** The caller may export,
  remove a specific URL's cache or explicitly prune obsolete versions.
- `PruneObsoleteVersionsAsync` deletes only recognizable old `.md`
  version files when **a different, verified manifest-selected version
  for the same URL exists**. Missing or corrupt manifests never trigger
  speculative deletion of an orphan that might be the last recoverable
  offline source.
- The default pruning grace period is **24 hours**. This protects
  recently refreshed files, but is **not** an operating-system
  cross-process lease: an older version may still be open in another
  reader. Applications should close such sessions before maintenance;
  passing `TimeSpan.Zero` explicitly removes the grace interval.
- Crash-leftover `.part` files count toward the quota, but are **not
  automatically removed** by obsolete-version pruning. Safe orphan
  recovery, per-document pinned retention, cross-process coordination,
  and explicit UI storage management remain future enhancements.
- The cap is enforced cooperatively inside a single cache instance.
  Separate application processes, external file changes and unrelated
  files in the directory are **not** controlled by this mechanism.
  It must not be described as an operating-system hard filesystem quota.

For a weak machine, choose a conservative **disk** budget and keep
large caches on storage with enough free space. The metadata enumeration
uses little RAM but can cost I/O on a rotational HDD; it runs on
downloads/maintenance, **not** on every page turn.

The future Docs Reader can surface usage, export, refresh and manual
pruning actions. None of those interactive menu entries are currently
wired into the Showcase or a standalone Docs Reader.
