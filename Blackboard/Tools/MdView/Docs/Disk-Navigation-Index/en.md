# Disk-backed Markdown heading and link navigation

## Purpose

`MdViewDiskNavigationIndex` is a **bounded-memory, disk-backed**
catalog of source coordinates for Markdown headings and links in a
completed `Utf8ValidatedSnapshot`. It is an opt-in companion to the
existing disk line index, container-event journal and scanner
checkpoints. It does **not** construct or keep a full Markdown AST,
and it does not change the verified contents of the source snapshot.

This first integration is available in the **local experimental
formatted preview**:

```bash
dotnet run --project Blackboard/Tools/MdView -- preview README.md
```

Choose **Find heading (disk index)** or **Find link location (disk
index)** in the terminal navigation prompt. The index is built
**lazily** on first selection, with one sequential pass through the
validated source. Subsequent selections reuse the same index until
the preview ends.

## On-disk representation

The index owns three independent temporary files:

- A bounded, length-prefixed record stream containing entry type,
  zero-based physical source line, decoded UTF-16 source offset,
  bounded display label and optional original link destination.
- A 64-bit offset table for headings, in source order.
- A 64-bit offset table for inline links, in source order.

Only a small output page of catalog entries is read into RAM at one
time. `MdViewDiskNavigationPicker` displays **12 entries per
selection page**, with Next results, Previous results and Escape/Back.
A selected entry takes the existing preview to its source line
without materializing preceding document contents.

```csharp
await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(path);
await using var index = await MdViewDiskNavigationIndex.CreateAsync(snapshot);

var headings = await index.ReadAsync(
    MdViewIndexedItemKind.Heading, firstIndex: 0, maxItems: 12);

var links = await index.ReadAsync(
    MdViewIndexedItemKind.Link, firstIndex: 0, maxItems: 12);
```

All record and offset bytes share a **128 MiB default disk budget**.
Per-record storage is capped at 16 KiB. Heading/link labels are
limited to 160 UTF-16 code units and long labels are shortened without
splitting surrogate pairs. Inline link destinations longer than
2,048 UTF-16 code units are conservatively skipped. The picker
truncates long labels on screen without changing source coordinates.
If the budget is exceeded, index construction fails and deletes its
partial temporary files; the original validated snapshot is still
available to the ordinary line pager.

The snapshot itself has its own independent byte, physical-line and
disk-index budgets. The index is associated only with the live
validated snapshot; it is never reused across files, modified
sources, sessions or application launches. Disposing it closes and
deletes all three temporary index files, not the source snapshot.

## Parsing contract

The index uses the existing
`MarkdownIncrementalContainerScanner.ScanAsync` and
`MarkdownInlineParser.Parse` on **one classified source line at a
time**. Recognized top-level ATX headings are recorded; code fences,
their content and closing delimiters are excluded. Inline Markdown
links in ordinary text, list items and quotes can be found, including
links inside emphasis. Images, escaped links and inline code spans
are not treated as link entries.

The index intentionally **does not yet** claim full CommonMark
coverage: reference-style links, multiline links, setext headings,
nested quote headings and all indented-code edge cases may be
missing or conservatively approximated. A link entry records its
position and original destination but is **not** proof that following
that destination would be safe or supported.

**Find link location** scrolls to the source line containing the
link; it does not open another file or contact a network service.
The existing `mdview browse` and `browse-git` commands retain
their independent validated link-following policies.

## Startup, performance and low-RAM policy

The new navigation scan is lazy to avoid delaying ordinary paging.
It is one additional **sequential** read when first requested; it
does not require the full document in memory. Stored result offsets
make subsequent result-page navigation independent of the number
of preceding headings or links. Jumping to a distant source line
continues using the existing `MdViewContainerPageCursor`: its
incremental disk event journal may still need to classify unseen
preceding lines, so no constant-time first jump is claimed.

For a future production large-document browser, the separate sparse
checkpoint index can avoid replaying all unseen context. The
present local preview remains experimental, and precise terminal-row
wrapping, rich cross-page syntax highlight state, and streaming
GitHub documents larger than 1 MiB remain future work.

## Verification

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
```

New regressions exercise code-fence exclusions, inline/quoted
links, large distant result pages, Unicode and mixed line endings,
small disk-budget rollback, record bounds, result selection and
cancellation, lazy preview integration and recoverable empty catalogs.

**These new tests have not been executed in this environment.**
Maintain the previous green checkpoint separately until the
maintainer confirms a fresh run.

[MdView README](../../README.md) ·
[Experimental preview](../Experimental-Formatted-Preview/en.md) ·
[Implementation status](../../IMPLEMENTATION_STATUS.md).
