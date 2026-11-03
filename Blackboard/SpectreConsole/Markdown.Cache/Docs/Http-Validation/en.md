# Conditional HTTP document source

**Implementation slice, 2026-11-03.** This document describes code present in the `Markdown.Cache` assembly, not a finished viewer.

## Protocol and freshness

1. Read and fully verify the indexed cached document (SHA-256, length, strict UTF-8, terminal controls).
2. If the cached manifest holds ETag and/or Last-Modified, send standard `If-None-Match` and `If-Modified-Since` headers.
3. On `200 OK`, stream response content into the existing bounded cache writer; accept only if the entire payload validates, then atomically replace the source index with new content and fresh validators.
4. On `304 Not Modified`, do not download content; revalidate that the indexed digest is unchanged under the writer lock and atomically update `CheckedAtUtc`. If the entry disappeared or changed during the request, return an error/retry decision rather than treating another version as validated.
5. Network errors, non-200/304 statuses and unsafe bodies preserve the old validated record. They return `Stale` with a visible warning when a record exists, otherwise `Error`. The viewer must label stale/offline content clearly.
6. Explicit caller cancellation is propagated, not mistaken for an offline response.

`CachedDocument.StoredAtUtc` tracks accepted content; `CheckedAtUtc` tracks the last confirmed refresh/check. Repeated `304` does not modify the blob or its accepted-content timestamp.

## Boundaries

This initial public URL adapter requires HTTPS, with no userinfo, query or fragment. Plain HTTP is opt-in for tests. Caller must supply a configured `HttpClient` with **automatic redirects disabled**, request timeout, and appropriate trust settings. Never supply authentication headers or private source URLs to this early shared cache. A redirect response is rejected rather than silently changing source. The current API checks the response's final request URI, but this alone does not prevent an injected auto-redirect handler from making a network request; the caller remains responsible for disabling redirects.

The cache writer enforces a per-document upper bound rather than silently truncating content. Response compression ratios, global cache quotas, multi-process read/write adversarial races, storage-root permission enforcement and full crash durability remain future work. The returned blob path is not a lifetime-pinned safe handle: callers should not use untrusted shared writable cache roots.

## Verification

Fake `HttpMessageHandler` tests cover conditional headers, restart reuse, changed bodies, offline fallback, corrupted content preservation, unsupported/insecure URL rejection, 404 and spurious 304. Authored tests have not yet been executed in this environment; do not report them as passed until CI verifies.

[Cache README](../../README.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md) · [Viewer cache design](../../../Markdown/Docs/Document-Cache-Policy/en.md).
