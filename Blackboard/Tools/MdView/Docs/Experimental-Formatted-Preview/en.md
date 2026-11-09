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

## Snapshot-bound checkpoint resume (2026-11-09)

`MdViewCheckpointPageReader` is a separate **opt-in building block**
for stateful jumping to previously captured source boundaries. It combines:
the caller-owned `Utf8ValidatedSnapshot`, a
`Utf8DocumentOffsetIndex` made from that snapshot's `Lines`, and the
`MarkdownIncrementalContainerScanner.StateMachine` checkpoint.

```csharp
await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(path);
await using var offsets = await Utf8DocumentOffsetIndex.CreateAsync(snapshot.Lines);
var pages = new MdViewCheckpointPageReader(snapshot, offsets);
var first = await pages.ReadAsync(pages.Start, 40);
var next = await pages.ReadAsync(first.Next, 40);
```

Every read maps the checkpoint's global UTF-16 position to a source byte
offset, validates that it equals the indexed start of the expected physical
line, opens a fresh strict UTF-8 reader at that byte boundary, restores
list/quote/fence state and classifies at most 128 lines. The result contains
another opaque checkpoint tied to this reader instance. Reusing a checkpoint
is repeatable; it does not consume shared iterator state. An EOF checkpoint
returns an empty page.

Checkpoint objects cannot be transferred between reader instances or
deserialized from arbitrary external data. The caller must provide offsets
for this exact snapshot and keep the snapshot and offsets alive throughout
reads. This is a source-lifetime ownership contract, **not cryptographic
protection against same-user tampering**.

This opt-in API does **not** make arbitrary unseen offsets instantly
navigable: a trustworthy checkpoint first has to be obtained by sequential
classification (or future source-bound disk checkpoint construction). It
does not replace the existing disk event journal or change the normal mdview
CLI pager. The embedded `MarkdownStreamLine.Kind` is locally
reclassified after a seek; the authoritative container classification is
`MarkdownContainerLine.Kind`. Do not infer source-global top-level fence
metadata from the embedded raw line kind after resumed reads.
New focused tests await Windows/Debian execution.

## Sparse disk scanner checkpoints (2026-11-09)

`MdViewDiskCheckpointIndex` is an independent **opt-in** random-page
classification index over a private `Utf8ValidatedSnapshot` and its
`Utf8DocumentOffsetIndex`.

```csharp
await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(path);
await using var positions = await Utf8DocumentOffsetIndex.CreateAsync(snapshot.Lines);
await using var checkpoints = await MdViewDiskCheckpointIndex.CreateAsync(
    snapshot, positions, stride: 1024);
var page = await checkpoints.ReadAsync(firstLine: 500_000, maxLines: 40);
```

Creation **first classifies the entire source** in bounded pages and
persists versioned scanner state every 1,024 lines by default, plus the
initial state. Checkpoint payloads are stored in a temporary length-prefixed
disk file with an independent 8-byte-per-checkpoint offset table. Total
temporary checkpoint storage is capped at 256 MiB by default; stride is
configurable from 1 to 65,536 lines. Both files use `DeleteOnClose`.

A later request locates the nearest preceding checkpoint in constant-time
slot arithmetic, restores validated state against the same snapshot,
replays **fewer than stride** preceding lines and returns up to 128
classified physical source lines. Reverse/jump requests do not depend on
a shared iterator. If the requested line is EOF, the resulting page is
empty. All reads validate checkpoint source-line alignment.

The checkpoint index contains **state, not a complete event journal**.
It does not change the existing preview UI, raw source pager, or legacy
renderer. Index construction itself is correctness-first: it currently
reopens a snapshot reader per bounded page and does not claim a
single-pass HDD throughput benchmark. Large initial indexing can take
time and uses temporary disk space, but not a full-document RAM model.
An index is tied to the live validated copy; do not share checkpoint
files across changed documents. External checkpoint deserialization is
not exposed by the new index.

The new tests cover cross-checkpoint code-fence parity, distant backward
navigation, repeated reads, disk budgets, invalid inputs and empty input.
Run focused and solution test suites before release.
