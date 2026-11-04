# Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache

Independent .NET 10 disk cache foundation for **validated UTF-8 remote Markdown documents**. No dependency on Spectre.Console, the Markdown parser, HTTP or GitHub APIs. This is an **unpublished internal building block**, not a complete offline viewer.

`VerifiedDocumentCache.PutAsync(sourceKey, stream)` copies a source stream in bounded chunks to a temporary file, enforces a per-document byte limit (default 256 MiB), validates strict UTF-8 and terminal control characters, computes SHA-256 and publishes a content-addressed immutable blob. Only after the content is verified does it atomically publish JSON metadata for the source key. An unsuccessful replacement leaves the previous valid entry indexed. `GetAsync(sourceKey)` checks file length, digest and Unicode/control validation before returning a `CachedDocument`; missing or invalid records return null. Cache files remain across process restarts.

```csharp
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;

var cache = new VerifiedDocumentCache(cacheDirectory);
await using var source = File.OpenRead("README.md");
await cache.PutAsync("sample-readme", source);
var cached = await cache.GetAsync("sample-readme");
if (cached is not null)
    Console.WriteLine(cached.ContentPath);
```

**Important current boundaries:** this version only accepts strict UTF-8 and permits tabs/newlines/carriage returns; UTF-16/32 conversion is a future source-adapter concern. It does not normalize source identities, perform network requests, manage ETag/Last-Modified, implement eviction, secure an authenticated cache, index Markdown pages, or integrate into the Showcase UI. The caller must provide a trustworthy per-user cache root and stable non-secret key; caller must also revalidate content when consuming the returned file path because files outside this API can be modified. File I/O is currently serialized only **within one cache instance**, not across processes. Unreferenced blobs can remain after interrupted writes or replacements; garbage collection is not yet implemented. Avoid using a shared/untrusted writable directory or symlinked cache tree.

```bash
dotnet test Blackboard/SpectreConsole/Markdown.Cache.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache.Tests.csproj
```

[Design and limitations](Docs/Storage/en.md) · [Implementation status](IMPLEMENTATION_STATUS.md) · [Future viewer cache contract](../Markdown/Docs/Document-Cache-Policy/en.md).

## Cross-process writer and directory hardening (2026-11-03)

Writers now take an exclusive `.write.lock` file handle (retry with cancellation), shared between independent cache instances/processes, before mutating cache storage. Cache root, `blobs`, `entries`, index and blob targets reject symlinks/reparse points when inspected. A corrupt existing digest-named blob is replaced with validated content instead of being trusted. `CacheDirectory.GetDefaultPath()` suggests Windows per-user LocalApplicationData or Unix XDG cache / `~/.cache`; caller still creates/owns the directory.

This is **defense in depth**, not a guarantee against an attacker concurrently replacing directory components (TOCTOU), parent-path symlinks or power-loss durability. Use an isolated trusted user-owned root; private/authenticated caching remains deferred. No full multi-process stress test has run yet.

## Conditional HTTP(S) documents (2026-11-03)

`HttpMarkdownDocumentSource` is a small opt-in adapter over an injected `HttpClient` and this cache. It sends `If-None-Match` and `If-Modified-Since` from the previously verified entry, stores a complete validated `200 OK` response atomically and handles `304 Not Modified` by updating the last-check timestamp **without downloading content**. A failure returns `HttpDocumentState.Stale` with the last valid copy and a warning, or `Error` when no cache is present. Cancellation requested by the caller propagates.

```csharp
using var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
var cache = new VerifiedDocumentCache(CacheDirectory.GetDefaultPath());
var source = new HttpMarkdownDocumentSource(http, cache);
var result = await source.OpenAsync(new Uri("https://example.org/README.md"));
if (result.Document is not null)
    Console.WriteLine($"{result.State}: {result.Document.ContentPath}");
```

The initial adapter deliberately permits **HTTPS URLs without query strings, fragments or embedded credentials** only. HTTP is opt-in for explicit local/test use. Redirects are not supported; callers should disable automatic redirects as shown. Do not inject authenticated handlers or store private documents yet. The cache does not interpret Markdown, nor does it automatically display stale content as fresh. URL identity and metadata are isolated from the parser/renderer; GitHub README discovery is a separate future adapter. The client must configure timeouts and safe transport settings. Resource consumption is controlled by the cache's configured per-document limit; global cache eviction and decompression-ratio bounds are not implemented.

[HTTP validation details](Docs/Http-Validation/en.md).

## GitHub preferred README provider (2026-11-03)

Added `GitHubReadmeSource` for **public** default-branch README documents via GitHub's official `GET /repos/{owner}/{repo}/readme` endpoint and the raw media type. Repository identifiers are validated and treated case-insensitively for cache keys; branch/ref and file path are not guessed or case-folded. ETag/Last-Modified, disk-persisted checks and explicit offline/stale behavior reuse the existing cache. No new dependency in ExchangeHouse Markdown or Spectre renderer. The source uses an injected `HttpClient`; **disable automatic redirects** on its handler and use no authentication credentials for this initial public-only adapter.

See [GitHub README documentation](Docs/GitHub-Readme/en.md). The standalone `mdview` command is **not implemented**.

## GitHub public repository catalog (2026-11-04)

The independent `GitHubRepositoryCatalogSource` uses official GitHub REST endpoints to list **public** repositories belonging to a user or organization. It identifies the account type first and retrieves at most ten 100-item pages; longer lists are clearly marked as truncated. Its validated JSON snapshot uses a separate case-insensitive owner cache key, stays fresh for 30 minutes, and remains usable **with a stale warning** after a failed refresh. No credentials or README bodies are included in the catalog.

[Catalog documentation](Docs/GitHub-Repository-Catalog/en.md) · [mdview repository selector](../../Tools/MdView/Docs/GitHub-Owner-Discovery/en.md). The older 'mdview not implemented' note above is historical; mdview now consumes both GitHub README and owner catalog adapters. New catalog tests are committed but not yet executed in this environment.
