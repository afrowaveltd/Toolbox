# Markdown.Cache — implementation status

## 2026-11-03 — disk storage foundation

- New independent `net10.0` non-packable Markdown.Cache assembly plus xUnit test project, both registered in `Toolbox.sln`.
- `VerifiedDocumentCache` writes streamed data to a temporary file, enforces a configurable document limit, uses strict UTF-8 and terminal-control validation, SHA-256 content-addressed blobs and an atomically replaced JSON source index.
- Read path verifies source key, length, content hash and complete Unicode/control safety. Failed replacement preserves the last valid indexed copy.
- Initial tests: cross-instance persistence, rejection of unsafe replacements, invalid UTF-8 preservation, tamper rejection, limit rejection, and cancellation preservation.
- **Verification pending:** no dotnet SDK is available in the local execution environment, and no new CI success has yet been observed. Tests have been authored but must not be described as passed.
- Not yet implemented: ETag, HTTP/GitHub providers, TTL, global budget/eviction, cache cleaning, cross-process synchronization, adversarial symlink protection, full crash-durability guarantees, or the `mdview` app.

Next: run and correct Windows/Linux solution CI; then add fault-injection tests, safe per-user root selection and cross-process locking before attaching network providers.

## 2026-11-03 — writer lock and cache directory hardening

- Added a cross-process exclusive `.write.lock` acquired with cancellation-aware retry, in addition to the existing in-instance semaphore. Existing cache reads remain independent and verify the published entry.
- Refuse direct symlink/reparse-point cache roots, blob/index directories and target files (including dangling link checks), and repair digest-named corrupt blobs when writing verified replacement bytes. Remaining TOCTOU and trusted-parent-path limitations are explicitly documented.
- Added `CacheDirectory.GetDefaultPath()` using per-user Windows LocalApplicationData or XDG cache / `~/.cache` on Unix; no directory is implicitly created by this helper.
- Added four tests: independent instances writing one key, corrupt blob replacement, Linux symlinked directory rejection and per-user absolute cache path. **Total authored tests: ten; not executed or CI-confirmed.**
- Local container has no `dotnet` executable and no outward DNS to install it. No new GitHub Actions success was established. Next: CI verification, test fixes, then fault injection, secure root provisioning, stale/conditional metadata and HTTP fake-handler tests.

## 2026-11-03 — conditional HTTP source and metadata

- Extended source index metadata additively with optional `ETag`, `LastModified` and `CheckedAtUtc`; legacy entries without validators remain readable. `PutAsync` still defaults to previous calling convention.
- Added `MarkNotModifiedAsync` under the per-process + cross-process write locks. It rechecks the indexed SHA-256 and refuses to update a changed/missing entry.
- Added `HttpMarkdownDocumentSource` using injected `HttpClient`: conditional GET, validated `200` replacement, `304` reuse, explicit stale/error states, and cancellation propagation. Restricted to credentials-free HTTPS URLs without query/fragment by default. HTTP opt-in is for development/tests.
- Authored eight isolated HTTP handler tests: 200→304 across instances, replacement 200, network failure stale fallback, invalid response old-entry preservation, 404 without cache, insecure/secret URL refusal, Last-Modified persistence, and unexpected 304 without cache. Total authored cache tests **18** (previous ten plus eight).
- **Not yet verified by a .NET build:** no local SDK. The GitHub status and PR-triggered workflow lookups for the intermediate test commit showed no runs/statuses. This is not proof of green CI.
- Limitations: automatic redirect behavior must be disabled on injected `HttpClient`; no GitHub README API adapter, per-user auth isolation, rate limit policy, global quota, safe automatic cache cleanup or paged Markdown rendering. Follow-up: CI/build fixes, redirect handling, controlled cache eviction and GitHub README provider.

## 2026-11-03 — public GitHub preferred README adapter

- Added `GitHubReadmeSource` using official GitHub REST preferred README endpoint, `Accept: application/vnd.github.raw+json`, User-Agent/API version, strict owner/repo validation and lowercased owner/repo cache identity (not ref/path).
- Reuses `VerifiedDocumentCache` and its `200`/ETag/`304`/SHA-256/offline-stale semantics. No branch/file guesses or authentication; redirects refused under explicitly non-redirecting caller-supplied HTTP handler.
- Added [GitHub README fake-handler tests](../Markdown.Cache.Tests/GitHubReadmeSourceTests.cs) covering API request headers, case-insensitive cache aliases, 304 after restarting the reader, stale offline, 404 without cache, corrupt body fallback, invalid identifiers including trailing newline and unexpected 304. **Tests authored, not run**.
- Documentation in [Docs/GitHub-Readme/en.md](Docs/GitHub-Readme/en.md). The standalone CLI/TUI, GitHub private auth, canonical metadata, redirects and linked-document resolution remain unimplemented.
- No .NET SDK available locally; CI result for this revision not yet verified. Next: fix any CI failure, integrate a small CLI command dispatcher after stable source contract, then add source-aware README links and pager work separately.

## 2026-11-03 — first CLI consumer

- The new [mdview executable](../../Tools/MdView/README.md) consumes HTTP and GitHub source adapters and prints explicit `Stale` state when offline. Cache library stays independent of CLI and renderer.
- No conditional-download behavior changed in the cache. Integration tests and CI results for mdview remain outstanding.

## 2026-11-03 — user Windows regression confirmation

- `Markdown.Cache.Tests` passed **32/32** in the user-provided full Windows solution test log, with no failed/skipped tests. This supersedes earlier 'test execution pending' statements for this snapshot on Windows. Linux CI and manual network scenarios remain unverified.
- Nonfatal XML documentation warnings (CS1591/CS1573) are logged for public cache APIs and should be cleaned without changing runtime behavior.

## 2026-11-04 — GitHub public repository catalog

- Added `GitHubRepositoryCatalogSource` with `ListAsync(owner)`, read-only typed public repositories, GitHub account-type discovery, user/org specific endpoints, controlled pagination (100 per page, maximum ten pages), 4 MiB response cap and explicit truncation warning.
- Valid owner/name checks and public-only filtering; raw descriptions are sanitized and labels escaped by the CLI. HTTP redirects are refused and caller must disable automatic redirects. No credentials, README downloads or network-dependent Markdown rendering in the catalog layer.
- An independent case-insensitive SHA-256 verified cache snapshot stays fresh for 30 minutes, then refreshes atomically; errors use an explicitly stale last valid list (or an error if none exists). A `TimeProvider` injection supports deterministic expiry tests.
- [Architecture and API usage](Docs/GitHub-Repository-Catalog/en.md). Added fake-HTTP regression tests. **Not yet compiled or regression-tested in this execution environment**, which lacks a .NET SDK. User's previously confirmed Windows 6,443 test baseline was before these additions.
