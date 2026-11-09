# Markdown link navigation — implementation plan

**Status (2026-11-09): design and acceptance contract only.** Current mdview
renders Markdown but does not provide interactive link selection, follow,
document history, anchor jumps, or relative-document navigation.
The standalone Markdown documentation-link checker is not a viewer navigator.

## Source bases and relative links

Every opened document should carry its typed origin and source base.

- **Local files:** resolve relative Markdown paths against the referring
  document's directory, not the process working directory. Handle
  `./`, `../`, anchors, Unicode and case-sensitive filesystems.
  Canonicalize before access. Decide how to handle paths leaving the
  initial directory and symlink targets; never execute linked content.
- **HTTPS:** resolve paths and fragments with URI-aware logic using the
  actual fetched source URI. Reuse the existing validated conditional
  cache, explicit offline/stale fallback and remote-source restrictions.
  A web HTML page is not automatically a Markdown source.
- **GitHub:** retain owner, repository, resolved ref and canonical file
  path. Resolve repository-relative `Docs/**` targets. Handle branch names
  containing slashes and distinguish `blob` pages from raw/API content.
  The existing README provider alone cannot fetch arbitrary linked files.

## Interaction

Offer keyboard-first link listing/selection (including text-only terminals),
a visible current-document origin, and bounded Back/Forward history.
In-document `#heading` targets should use a tested heading-slug index.
Treat documents and fragments as distinct history locations. Integrate
with future F/search and N/P navigation without breaking the current
completion-prompt keyboard contract.

## Safety and acceptance tests

1. Never auto-open, auto-run or execute anything found in Markdown.
2. Disallow dangerous schemes (`javascript:`, `data:`, etc.) and reject
   unauthorized cross-origin or local-to-remote trust transitions.
3. Cover path traversal, symlinks, percent-encoded separators, redirect
   policy, missing targets, Unicode filenames and case sensitivity.
4. Cover local README → Docs/en.md, nested back-links, HTTPS relative
   paths, GitHub README links, same-page anchors and circular references.
5. Preserve source cache policy, offline/stale indicators, bounded history,
   and default renderer behavior until the navigator passes regression
   and manual Windows/Debian/SSH acceptance.

## Implementation order

1. Introduce a UI-neutral typed document origin and pure link resolver,
   with unit tests for local, HTTPS and GitHub source bases.
2. Add safe document loading through existing sources and cache; extend the
   GitHub provider for repository files when needed.
3. Add link extraction/selection and heading anchors, then bounded
   Back/Forward navigation with user-visible failure handling.
4. Integrate with opt-in formatted paging and later indexed search.

No implementation of the above is claimed by this design document.

[mdview README](../../README.md) ·
[CLI guide](../CLI/en.md) ·
[Implementation status](../../IMPLEMENTATION_STATUS.md).
