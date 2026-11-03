# Afrowave Docs Reader — deferred architecture note

**Status:** idea captured on 2026-11-02; no implementation, no NuGet,
no commitment to a specific API. Continue the Markdown paging work first.

## Use case

The Toolbox documentation convention is `Docs/<Topic>/en.md` (with
more localized language files later). Instead of opening individual
topic documents, a reader could show an **entire documentation tree as
one navigable book**. Every source document would be preceded by a
generated topic heading derived from its parent directory, followed by
its Markdown content. The result is a logical view rather than a new
huge concatenated source file.

Possible local and GitHub sources:

- A local `Docs` directory (including nested topic directories).
- A GitHub repository and requested commit/ref, enumerated as a tree.
- Future HTTPS manifests or other source providers behind a separate
  abstraction.

Keep source selection separate from Markdown parsing and console
rendering. An explicit language option should select `en.md` initially;
future locales need an intentional fallback rule and must not silently
include both language versions in the same book.

## Proposed virtual document model (not implemented)

A **source manifest** enumerates deterministic, sorted document segments.
Each segment retains its path, topic, source identity and exact logical
position. The reader generates a heading segment immediately before
the related source and uses the existing disk-backed Markdown pager to
seek across segment boundaries. This avoids loading the combined body
into RAM; even a long book could be read backwards and forwards.

Navigation must preserve provenance so relative links, headings, line
positions and diagnostics can refer back to their original document.
Later work may add a table of contents, links, search and language
selection, with an option to view original source separately.

## Safety and scale concerns

- Traversal must stay inside the selected docs root, with explicit
  symlink/submodule policies for local and Git-backed sources.
- Path names from remote repositories must never be interpreted as
  executable console markup or escape sequences.
- GitHub tree APIs can enumerate paths, but large repositories,
  pagination/truncation, rate limits, authentication and revision
  stability must be handled by the provider. Never assume that
  downloading every document at once is acceptable.
- Generated topic headings must not silently alter original Markdown
  semantics; keep a distinct segment boundary and parser-state reset.
- Network and disk use must be bounded, cancellable and have clear
  cleanup behavior. Avoid retaining the full virtual book in RAM.

## Dependency and release order

1. Finish the current single-document paging and contextual Markdown
   renderer.
2. Define an independent virtual-document/segment abstraction and
   test it against a local Docs tree without remote network access.
3. Add a GitHub source provider with stable revision semantics.
4. Build a small CLI/TUI user experience; publish a NuGet prerelease
   only after package restore, tests and documentation pass.

The proposal deliberately does **not** expand the current draft PR's
feature acceptance criteria. It only preserves an idea for future work.

## Source-cache foundation added (2026-11-03)

A reusable `ExchangeHouse.MarkdownRemoteDocumentCache` now provides a separate **implemented library foundation** for the future remote Docs Reader. It saves versioned HTTPS Markdown source files in a per-user cache, conditionally revalidates them using HTTP ETag/Last-Modified, retains an explicitly flagged stale copy for offline use, and supports export and per-URL removal. Unlike this source cache, disposable page/fence indexes remain safe to rebuild. No GitHub repository tree provider, virtual book source manifest, offline GUI/TUI menu, project-wide synchronization or finished Docs Reader has been implemented. Read [cache API and limitations](../../ExchangeHouse/Markdown/Docs/Offline-Document-Cache/en.md).

## Offline cache storage budget and user-initiated maintenance (2026-11-03)

The implemented ExchangeHouse source cache now supplies a configurable
default **1 GiB total cache-owned disk budget**, disk-usage inspection
and an **explicit** older-version pruning operation that retains
the manifest-selected offline copy. It does not silently evict documents
on full disks; a requested refresh fails without losing the prior
usable copy. A future Docs Reader should show cache usage and offer
export, remove-document and prune-versions commands while keeping
cache management optional. The 24h pruning grace period protects
recent versions, not simultaneous readers in other processes. No
Docs Reader TUI or GitHub repository provider is implemented yet.
[Cache contract](../../ExchangeHouse/Markdown/Docs/Offline-Document-Cache/en.md).
