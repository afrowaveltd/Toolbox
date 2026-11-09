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

## Implemented foundation — pure typed resolver (2026-11-09)

The first **UI-neutral** slice is now implemented as
`MdViewDocumentOrigin`, `MdViewResolvedLink` and
`MdViewLinkResolver.Resolve`. Local origins carry a canonical file path
and explicit navigation root; HTTPS origins carry an absolute secure
source URI; GitHub origins carry owner, repository, explicit resolved
ref and repository-relative path. Anchor-only links retain the source
origin; path links preserve the appropriate source identity.

The resolver does no disk/network access, does not select links from
rendered output, and does not yet integrate with mdview's interactive
terminal session. It rejects absolute/unsupported schemes, query strings,
encoded link paths, backslashes, cross-origin HTTPS jumps, local links
outside the configured lexical root and repository parent traversal.
The current conservative policy excludes percent-encoded filenames and
fragments with slashes until a dedicated decoding and validation design
is tested. It also deliberately rejects externally hosted links here;
a separate explicit external-opening action would be required.

**Security boundary:** local lexical path containment does not
resolve symlinks or reparse points; a future file opener must verify
the resolved filesystem target before opening it. GitHub references
must come from the provider's resolved branch/ref metadata, not an
unverified guessed default branch.

Example, without opening a file:

```csharp
var origin = MdViewDocumentOrigin.FromLocal(
    Path.GetFullPath("README.md"), Directory.GetCurrentDirectory());
var target = MdViewLinkResolver.Resolve(origin, "Docs/Installation/en.md");
Console.WriteLine(target.Origin.Location);
```

See `MdViewLinkResolverTests` for initial local, HTTPS, GitHub,
fragment and unsafe-target regression cases. Compilation and test
execution of this increment remain to be confirmed.

## Local link loading foundation (2026-11-09)

`MdViewLocalLinkLoader.OpenAsync` now connects the pure local resolver to
`Utf8ValidatedSnapshot.CreateAsync`. A caller can explicitly follow a
relative link to a local Markdown file, obtaining a validated private
working copy and its source origin/fragment. Both `.md` and `.markdown`
extensions are recognized; other types and malformed/unsafe UTF-8 are
rejected. Returned `MdViewLocalLinkedDocument` is `IAsyncDisposable`
and owns its snapshot.

```csharp
var source = MdViewDocumentOrigin.FromLocal(
    Path.GetFullPath("README.md"), Directory.GetCurrentDirectory());
await using var linked = await MdViewLocalLinkLoader.OpenAsync(
    source, "Docs/Installation/en.md");
using var reader = linked.Snapshot.OpenReader();
```

Navigation roots are lexical: before staging, the loader rejects
symbolic-link/reparse-point components including intermediate directories,
but this is **not** a race-free OS-enforced sandbox. The caller chooses
the trusted navigation root and must not rely on these checks alone against
concurrent hostile filesystem changes. The link fragment is carried as
metadata; it does **not** yet move the reader to a heading.

The mdview CLI and interactive UI remain unchanged. External HTTPS/GitHub
loading must be built on their existing validated cache/providers rather
than this local loader.

## Bounded history and local navigation session (2026-11-09)

Two independent opt-in components now exist:

- `MdViewNavigationHistory`: source-agnostic bounded history of typed
  document origins and fragments, default 128 entries (maximum 4096).
  Supports Back, Forward, branch-after-back (discard forward entries),
  duplicate-visit suppression, and eviction of oldest entries.
- `MdViewLocalNavigationSession`: owns **one** validated local Markdown
  snapshot and a history. `OpenAsync` starts from a local source;
  `FollowAsync`, `BackAsync`, and `ForwardAsync` use the existing
  safe relative-link loader. Destination validation happens before
  committing a transition. Missing, invalid or disallowed destinations
  leave the current document/history unchanged.

```csharp
var origin = MdViewDocumentOrigin.FromLocal(
    Path.GetFullPath("README.md"), Directory.GetCurrentDirectory());
await using var navigation = await MdViewLocalNavigationSession.OpenAsync(origin);
await navigation.FollowAsync("Docs/Install/en.md#setup");
await navigation.BackAsync();
await navigation.ForwardAsync();
```

History stores only link coordinates; it **reopens and revalidates**
destinations on Back/Forward. It does not retain all visited snapshots,
eliminating unbounded document-memory growth. A failed destination open
does not modify history. The controller is async-disposable and closes
its current snapshot; the caller must not continue using previously
replaced snapshots.

The implementation is **not wired to terminal keyboard controls yet**.
No link extraction from rendered Markdown, heading anchor jumps, or
network/GitHub navigation is claimed. Local path reparse checks are
best-effort and remain subject to concurrent path substitution.

## Parser-backed link discovery and explicit terminal selection (2026-11-09)

Two new **opt-in** components reuse the existing ExchangeHouse Markdown
inline parser rather than implementing a second Markdown grammar:

- `MdViewDocumentLinkExtractor.Extract(markdown, origin)` returns ordered
  `MdViewDocumentLink` values, each carrying the original label,
  destination, UTF-16 source offset and a typed resolved target.
  It traverses recognized inline links (including links inside emphasis)
  but deliberately does not offer image targets, inline-code text,
  fenced-code content, malformed/escaped syntax, unsupported schemes or
  links blocked by the conservative relative-link resolver.
- `MdViewDocumentLinkPicker` offers those links through a Spectre.Console
  selection prompt. Only an explicit Enter choice returns a link; Escape
  cancels. It never opens or downloads a target.

Extraction currently builds the regular document Markdown AST and is
**limited to 1,048,576 UTF-16 characters** and **4,096 links** per call.
This is a deliberate separate small-document path, **not** a streaming
index for 1 GiB files; a future source-index-backed link catalog must
avoid full-document materialization. Existing raw/preview paging is not
changed by this first slice.

```csharp
var links = MdViewDocumentLinkExtractor.Extract(markdown, origin);
var chosen = await new MdViewDocumentLinkPicker(console).PickAsync(links);
if (chosen is not null)
{
    // Future CLI integration: validate/open chosen.Target, then commit history.
}
```

This backend selector is **not yet wired to the default mdview CLI,
the existing document-completion prompt or the local navigation session**.
Link-following must still be invoked explicitly by a caller through the
already-implemented local loader/session. Test coverage includes fenced
code, inline code, images, escaped syntax, resolved source positions,
unsafe/external links, keyboard choice, cancellation and noninteractive
terminal rejection.
