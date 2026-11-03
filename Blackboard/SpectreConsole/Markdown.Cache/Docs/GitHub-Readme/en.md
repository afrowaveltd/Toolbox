# GitHub preferred README adapter

**Implementation date:** 2026-11-03. This is a source adapter within the unpublished Markdown.Cache library, not an implemented `mdview` CLI or TUI.

## Using the adapter

```csharp
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;

using var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
var cache = new VerifiedDocumentCache(CacheDirectory.GetDefaultPath());
var github = new GitHubReadmeSource(http, cache);
var result = await github.OpenAsync("afrowaveltd", "Toolbox");

if (result.Document is not null)
{
    Console.WriteLine($"State: {result.State}");
    Console.WriteLine($"Validated document: {result.Document.ContentPath}");
}
else
{
    Console.Error.WriteLine(result.Warning);
}
```

The caller must configure HTTP timeouts, transport policy and filesystem permissions. Do not provide sensitive credentials to this first public-readme adapter.

## Readme resolution and caching

The adapter requests `GET https://api.github.com/repos/{owner}/{repository}/readme` with GitHub's preferred raw Markdown media type, `Accept: application/vnd.github.raw+json`, a User-Agent and pinned REST API version. It **does not** assume `main`/`master` or a particular README filename.

GitHub treats repository owner/name identifiers case-insensitively; the cache key uses invariant lowercasing, so `AfrowaveLtd/Toolbox` and `afrowaveltd/toolbox` share the same entry. The code does not case-fold file names, refs or local paths. Only public default-branch README retrieval is supported: no custom ref, nested README path, private tokens or GitHub Enterprise server.

GitHub's ETag and Last-Modified validators, when present, are stored with the validated content. On a subsequent call `If-None-Match`/`If-Modified-Since` avoid downloading unchanged bytes. A `304` updates the check timestamp only if the SHA-256 indexed copy still matches. A new `200` is streamed, fully validated and committed atomically. If the server is unavailable, returns an error status, or yields unsafe content, the last **previously validated** record is returned as `Stale` with a warning; if there is none, `Error` is returned. The caller decides whether to show stale content.

## Boundaries and security

- Caller supplies an `HttpClient` with **`AllowAutoRedirect = false`**. This implementation refuses unexpected final request URIs; it does not implement redirect-following or GitHub rename handling.
- The source owner/repository inputs are restricted to GitHub-style ASCII identifiers before constructing the API URI. Names from untrusted input cannot introduce arbitrary hostnames, query parameters, credentials or path traversal.
- The cache is explicitly unauthenticated and scoped to public READMEs. Do not persist private repository data or bearer tokens using this initial store.
- Rate limit status `403`/`429`, `404` and server failures are returned as status + warning (and optional stale copy). Intelligent backoff and `Retry-After` are future work.
- The current cache validates strict UTF-8. Rendering remains outside this library. Relative link resolution will need a trustworthy canonical repository/ref/path from GitHub metadata in a later increment.
- Redirecting or decoding compressed transport, global disk quotas, untrusted cache-root attacks and exact ETag semantics under branch changes require further hardening.
- Automatic builds and tests **have not been verified** in this session. Added fake-handler tests do not prove live GitHub acceptance.

## Test scenarios

The GitHub test file covers raw Accept/path headers, owner/repo capitalization and `304` reuse across cache instances, offline stale behavior, not found, corrupt replacements, malicious input rejection and unexpected `304` without a cached copy. No live network is needed to run the suite.

[Markdown.Cache README](../../README.md) · [Status](../../IMPLEMENTATION_STATUS.md) · [Standalone viewer proposal](../../../Markdown/Docs/Standalone-Markdown-Viewer-Proposal/en.md).
