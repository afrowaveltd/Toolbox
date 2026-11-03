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
