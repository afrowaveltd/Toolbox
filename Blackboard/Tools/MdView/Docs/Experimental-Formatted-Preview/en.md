# Experimental bounded Markdown page preview

**Status:** opt-in, source-page-scoped rendered preview. The existing `local`,
`url`, `git` and `source` modes are unchanged.

## Run

```bash
dotnet run --project Blackboard/Tools/MdView -- preview README.md
```

This command uses `Utf8ValidatedSnapshot.CreateAsync` to make a private
temporary, fully UTF-8-validated working copy of a local file. Each request
reads no more than 24 physical lines by default (maximum 128) using
`Utf8DiskLineIndex.ReadPageAsync`. The existing
`MarkdownSpectreRenderer` formats **only the current bounded fragment**.
The original document and a full Markdown syntax tree are never retained
as one string in this mode.

Spectre.Console offers **Next page**, **Previous page**, **Go to source line**
and **Exit preview**. Physical source-line positions are 1-based in the
navigation prompt. Returning to a page reparses only that bounded portion
of the validated snapshot. The scanner and UTF-16 document index are not
yet connected to this mode.

## Intentional limitations

This is a proof of the terminal and snapshot-to-renderer workflow, **not
context-preserving Markdown visual pagination**. Because pages are parsed
independently, a code fence, nested list, quote or another Markdown
container spanning two source pages can display differently at a boundary.
A physical-line count does not equal a terminal-row budget; long lines can
wrap and a page may exceed the screen height. Syntax-highlight sessions,
heading search/indexes, cross-page state and page layout caching are not
yet available.

No untrusted bytes are displayed until the **completed private copy** has
passed whole-file strict UTF-8 and terminal-control validation. The copy is
removed on normal disposal; operating-system process termination can leave
orphaned temporary data. The snapshot remains a private working copy, not
a verified instant-in-time capture if the original changes while copying.
The snapshot default byte budget is 16 GiB and temporary disk space must be
available.

Only an interactive local terminal is supported by this preview command.
To produce full-document output, use `mdview local PATH`. To navigate
validated raw source, use `mdview source PATH`.

## Verification

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
```

`MdViewFormattedPreviewPagerTests` covers opt-in navigation, headings,
source integrity, invalid bytes and noninteractive terminals. Tests are
committed but **fresh execution has not been confirmed** in this session.

## Next milestone

Feed completed container-aware Markdown spans from
`MarkdownIncrementalContainerScanner` into a bounded renderer with
retained fence/list/quote state; then paginate by actual terminal rows
and revalidate page boundaries against the original full parser. The
legacy modes should remain the default until golden cases and Windows/
Debian acceptance are complete.

[mdview README](../../README.md) ·
[Raw source pager](../Raw-Source-Pager/en.md) ·
[TextFiles snapshot](../../../../TextFiles/README.md) ·
[Implementation status](../../IMPLEMENTATION_STATUS.md).


## Incremental fenced-block classification (2026-11-08)

The formatted preview now has a validated-snapshot-backed
`MdViewContainerPageCursor` that maintains a forward-only
`MarkdownIncrementalContainerScanner` iterator. Each requested source
page obtains matching classified events. A proven fence opener,
code-content line or matching closer is written **literally** through
Spectre `Text` rather than being passed to the page-fragment Markdown
parser. Markdown-looking source within a continued fence therefore
cannot turn into a heading/list merely because a source-page boundary
occurs.

Ordinary non-code runs remain independently parsed bounded fragments,
and code lines currently use plain literal rendering rather than
TextMate syntax highlighting. Reverse/jump navigation resets and replays
from the beginning; no claim of O(1) random page navigation for
container metadata is made. Resource use remains bounded (one 128-line
page plus scanner state and on-disk indexes), but seeking far backward
may require scanning many earlier lines.

This remains an **experimental** pager. Quoted/lazy list ownership,
visual terminal row wrapping, context-aware inline Markdown and truly
random indexed syntax checkpoints are not solved by this change.


## Disk-backed classified-event journal (2026-11-08)

The previous `MdViewContainerPageCursor` used a forward-only scanner
and replayed all prior input whenever the user navigated backward.
It now writes complete classified events to a temporary serialized
journal and a separate 8-byte-per-physical-line offset index.

When a page is within the scanned high-water mark, the cursor seeks
directly to the recorded events and deserializes at most the requested
128 lines. It does not rescan preceding source text. Beyond the high-water
mark, the same incremental scanner is resumed and the intervening
events are added to the journal before the requested page is returned.
The page renderer continues to treat fence open/content/close as
literal Spectre text and other lines as bounded Markdown fragments.

Two independent disk budgets apply by default: 512 MiB for serialized
events and 256 MiB for offsets. Individual serialized records are
limited to 8 MiB. On budget exhaustion, partial-write errors or scanner
failure the cursor becomes unusable (fail-closed). The snapshot is owned
by the caller; the cursor disposes its own scanner and temporary
delete-on-close journal files.

**Not a true scanner checkpoint:** the underlying scanner maintains
list/quote/fence state inside an async iterator, so no arbitrary state
restoration is claimed. The journal caches already classified output,
not the iterator state. Previously unseen distant jumps still require
a scan of the intervening lines, and the 512 MiB default is a disk-space
limit rather than a promise to index unlimited document sizes.

Tests cover reverse/forward parity, fence classification, both budgets
and empty-source handling. Execution and Windows/Debian acceptance remain
to be confirmed after this change.

## Per-request cancellation boundaries (2026-11-08)

The disk-journal cursor's incremental scanner now uses a lifetime-independent
enumerator token. Cancelling or disposing the token for an **earlier completed**
`ReadAsync` does not affect subsequent forward navigation or fence state.
Each call still checks its own cancellation token before scanning, after
each asynchronous line advance and during journal reads/writes.

**Limitation:** an in-flight underlying stream read cannot presently be
interrupted directly by a caller-specific cancellation token; cancellation
is observed at the next boundary. A cancelled scan after advancing the
iterator is conservatively failed closed to avoid journaling inconsistent
positions. The reader is a local validated snapshot, not an arbitrary
network source. Two regression tests cover completed-token cancellation
and pre-cancelled calls on a usable cursor.
