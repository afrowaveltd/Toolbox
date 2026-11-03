# Persistent, offline-first document cache (implementation contract)

**Status:** design and acceptance contract only (2026-11-03). No persistent cache, HTTP client, GitHub source adapter or standalone `mdview` executable exists yet. This document refines the [standalone viewer proposal](../Standalone-Markdown-Viewer-Proposal/en.md).

## Purpose and ownership

A README already opened once should remain available after closing and reopening the viewer. Network availability must not be required just to reread an unchanged document. An offline reader may show the last **fully validated** copy with clear source and staleness information. Network fetches, storage, metadata, cleanup and navigation belong to the future viewer/source layer, **not** ExchangeHouse Markdown or Blackboard's renderer.

Retain cache files by default; **do not delete them on normal exit**. Treat OS temporary directories as disposable: if a persistent location is configured/available, use it instead of assuming OS TEMP survives. An explicit `clear cache` command is an opt-in destructive operation against viewer-owned entries only.

## Storage and identity

- Default to a per-user application cache following platform conventions: XDG cache base (`$XDG_CACHE_HOME`, otherwise `~/.cache`) on Unix-like systems and the per-user local application-data directory on Windows. Support an explicit cache directory for portable/offline installations. Never write to the document's original directory.
- Split cache into **content** (immutable, hash-addressed UTF-8 or original validated bytes) and **metadata** (source identity, final effective URI, hash, length, Unicode encoding, successful validation time, fetch/check timestamps, ETag/Last-Modified if supplied, and optional GitHub canonical repository/path/ref/revision).
- Make source keys deterministic: GitHub repository owner/name are case-insensitive; preserve case-sensitive ref and path segments. For generic URLs normalize only proven-safe URI parts (scheme/host/default port), not query strings or paths. Avoid storing sensitive URL userinfo, token-bearing query parameters or private response bodies in an unrestricted shared cache.
- No name derived directly from a URL or remote filename should become a filesystem path. Use digest-derived names and a constrained viewer-owned root; reject symlinks/reparse-point escapes for cleanup operations.
- Write to a unique file in the cache root, validate **complete bytes**, flush, and atomically replace the entry index. Interrupted downloads or invalid Unicode/control bytes must not overwrite the previous good entry. Check content hash and length before displaying cached entries; corruption is a cache miss.
- Keep file writes synchronized per key; concurrent processes should not leave partially written metadata. A minimal atomic-index design may precede SQLite; choose the simplest crash-safe solution that passes tests.

## Source state machine

| State | Action | Result |
| --- | --- | --- |
| First open, online | Fetch and validate whole response, then atomically persist | Display verified content |
| Existing entry, online | Conditional request using ETag and/or Last-Modified when available | `304`: reuse unchanged entry and update check time; `200`: validate and replace atomically |
| Existing entry, offline or request timeout | Offer previous verified entry without discarding it | Clearly mark as **offline/stale**, with last-successful fetch/check time |
| First open, offline | No entry to show | Explain network requirement; return to source selection |
| `404`, `401`, `403`, `429`, server failure | Show actionable status; never silently treat stale data as fresh | Optional stale fallback with explicit warning, not automatic success |
| Invalid or unsafe new response | Reject whole new response | Keep previously validated entry intact, identify failure |
| Local file | Read local file directly; do not require network | Optional local indexing may be added separately; don't cache sensitive local files by default |

A stale entry is not evidence that the remote source still exists or has identical contents. `304 Not Modified` is usable only against the corresponding stored validator and correctly scoped request. GitHub readme responses may change default branch/path: persist effective identity and invalidate mismatched validators.

## Resource and security limits

Cache reuse must **not** bypass whole-document validation, safe terminal output or the future renderer's memory limits. A streaming download can spool bytes to disk first, validate the whole item and only then make it displayable; no unvalidated prefix is ever rendered. Avoid transparent decompression bombs with separate compressed/uncompressed budgets and cancellation.

Make disk budget and retention configurable; proposed initial defaults are **256 MiB per cached remote document** and **1 GiB total cache** (policy proposals, not current behavior). Reject oversized fetches with an explicit message, never a silently truncated document. A user may opt into larger limits (including multi-GB files) on suitable disks; incremental parsing/paging remains a separate prerequisite for bounded-RAM display. Prefer least-recently-used cleanup only when a budget is exceeded and never evict a document currently open. The user can disable persistent cache, inspect its size, clear selected entries or clear all viewer-owned entries after explicit confirmation.

For a future authenticated provider, cache partitioning must respect the identity/authorization scope, and avoid plaintext credentials in metadata/logs. Sensitive authenticated responses should default to no disk persistence until a secure policy exists. Use conservative redirect policies; never forward credentials across origins.

## Minimal acceptance tests (future implementation)

1. Open a public README twice: second request sends a validator; `304` reuses identical verified bytes without downloading its body.
2. Restart the process, disable network, and reopen: a complete previously saved document appears with an offline/stale indicator; no network is required for local documents.
3. Change remote content: successful `200` commits the new validated entry and updates its hash and fetched timestamp.
4. Inject invalid UTF-8 and an ESC character near the end of a new response: neither is rendered and the old verified entry survives unchanged.
5. Interrupt during download, metadata commit, or concurrent writes: no partial content becomes visible; restart can recover the prior valid entry.
6. Verify GitHub owner/repository casing aliases share one cache entry, while distinct case-sensitive paths/refs do not.
7. Simulate `401`/`403`/`404`/`429` and timeout: stale fallback is clearly labeled, errors remain visible and no credentials appear in logs.
8. Exceed size/budget limits: report refusal or safely evict only eligible cache entries; never truncate or remove arbitrary files outside the owned cache directory.
9. Run the same storage tests under Windows and Linux paths, with a read-only cache root and process cancellation.
10. Verify the existing complete local Markdown reader behaves identically; no renderer/parser network or persistence dependency is introduced.

## Proposed delivery order

First implement an injectable cache store and source-key normalization with fake-clock/file-backed tests. Next add a conditional HTTP(S) fetcher and a GitHub README adapter using test handlers. Only then wire cache controls into the future interactive viewer. Measure Linux/Windows memory, disk I/O and offline behavior before promising the 1 GB Markdown target.

[Renderer implementation status](../../IMPLEMENTATION_STATUS.md) · [Toolbox repository status](../../../../../IMPLEMENTATION_STATUS.md).
