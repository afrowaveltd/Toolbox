# Offline Markdown cache manager — Showcase experiment

**Status:** seventh Blackboard Showcase module, on unmerged draft PR #3.
The manager works with an already populated per-user
`MarkdownRemoteDocumentCache` and does **not** download documents,
enumerate GitHub repositories, or replace the future Docs Reader.

## Open the terminal menu

```bash
dotnet run --project Blackboard/Samples/Blackboard.Showcase
```

Select **Markdown — offline document cache (experimental)**.
The screen reports recognized cache-owned file count and consumed /
configured disk bytes. Then choose **Browse saved Markdown documents**,
**Prune old document versions**, or **Back to Showcase**.

The browser opens only an **eight-entry window** at a time.
`MarkdownRemoteDocumentCache.ListPageAsync(skip, pageSize)` enumerates
saved manifests with a bounded result list (up to 64); it neither
downloads the entire catalog nor loads Markdown source into RAM.
Next/previous navigation is based on filesystem enumeration order.
After any removal, browsing restarts at the beginning to avoid
stale offsets. On very large directories, skipping pages may require
rescanning manifest filenames; a disk-indexed catalog is future work.

Each displayed entry shows a sanitized, shortened source path,
a byte count, and a short cache key. The URL is shown **without
query parameters**. The library verifies that a stored source URL
hashes to the manifest key, avoiding falsely attributed entries.
Legacy manifests without saved URLs are still openable and exportable
under the label **legacy source unknown**; URL-specific removal is
disabled for these old records.

## Read and manage a saved document

Choose one entry for these actions:

- **Read saved copy offline:** opens the exact cached file via the
  same Markdown source pager and Balanced/HDD/SSD/LowRAM profile picker
  used by the sixth Showcase module. No network request is made.
  Disposing page/context indexes does not delete the source cache.
- **Save a permanent copy elsewhere:** enter a destination path,
  absolute or relative to the original starting directory.
  The export creates a complete independent file and **never
  overwrites an existing target by default**.
- **Remove this saved document:** offered when the URI is known and
  verified. Requires explicit confirmation; the initial selection is
  **No, keep**. Removes the URL-specific manifest and saved versions.
  Do not delete documents with active readers in other processes.
- **Back to saved documents** or Escape: leave without destructive
  changes.

**Prune old document versions** is a separate explicit, confirmable
menu command. It keeps the manifest-selected offline copy and,
by default, only removes redundant source versions older than
24 hours. It does **not** force deletion of recently read versions,
automatically run a cache sweeper or evict the last offline copy.

## Limitations and security

- The manager is **local-only**. No remote URL input, GitHub tree API
  provider, background updates or finished virtual Docs Reader yet.
- A freshly downloaded cache manifest stores a normalized original
  HTTPS source URI so a document can be listed across application
  restarts. Query parameters may contain secrets. Store private or
  signed-URL documents in an isolated per-user cache directory and do
  not share its metadata between identities. Menu labels omit queries.
- The cache is cooperative, not a cross-process lock or a hard
  filesystem quota. Concurrent external maintenance may invalidate
  menu entries or page offsets.
- Cache browsing is intentionally conservative on 1-GiB systems:
  bounded result pages, no unbounded sorted catalog, no source bodies
  read until explicitly opened. **The real 1-vCPU/1-GiB HDD field
  benchmark is still pending**.
- This demonstration uses the existing optional network cache
  library. Populate that cache through `GetAsync` in a calling
  application or the test harness; the Showcase does not yet provide
  an online Download URL / GitHub repository UI.

See [reusable source cache contract](../../../../../ExchangeHouse/Markdown/Docs/Offline-Document-Cache/en.md),
[local disk pager](Markdown-Paged-Source-Viewer/en.md), and
[future Docs Reader design](../../../../../Docs/Unified-Docs-Reader-Proposal/en.md).
