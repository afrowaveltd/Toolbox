# Bounded contiguous event-journal reads

## Context

MdView's `MdViewContainerPageCursor` writes each classified Markdown
physical source line to a temporary, disk-backed JSON event journal.
A second disk file holds eight-byte little-endian offsets, one per
source line. Scanned events are reused for backward navigation,
repeated native-table measurements and source-line jumps.

The preceding optimization made
`MdViewNativeTablePagePlanner` ask the cursor for up to **six data
rows and one lookahead** at a time. However, the cursor still
performed **two event-stream reads per source row**: one for the
four-byte length and one for the serialized JSON payload.

This increment optimizes **inside the cursor itself**, benefiting
every caller of `ReadAsync`, including the native table planner,
the mixed page index and ordinary source-aware readers.

## Contiguous source window algorithm

`MdViewContainerPageCursor.ReadAsync(firstLine, maxLines)`
continues to enforce `maxLines` of **1–128**.

1. Extend the already existing bounded offset-table read by **at
   most one** offset, when the next classified line exists. When
   the requested window reaches the scanned end of the journal,
   the in-memory `_eventBytes` high-water mark is its exclusive
   endpoint.
2. Validate that source-record offsets are **strictly increasing**,
   non-negative, bounded by `_eventBytes`, and that every
   individual stored span is between 5 bytes and **8 MiB + 4
   bytes**. Check monotonicity before subtracting offsets to
   avoid overflow from malformed long values.
3. Coalesce consecutive **complete** records into an event-journal
   read window of at most **256 KiB**. Never split a JSON record
   merely to fill a batch.
4. If a single legitimate serialized line exceeds 256 KiB, read
   that record separately. Its existing maximum is **8 MiB
   plus the four-byte length header**.
5. Decode each record within its own slice of the window. The
   stored four-byte length must be positive and **equal to the
   exact byte span defined by the offsets minus four**. Then
   deserialize JSON and verify the original source line number.
6. Return the same source-classified lines in the same order.
   No additional file is created and no full-document buffer
   is retained.

The maximum memory used by one bulk-event buffer is bounded by
the ordinary **256 KiB**, or one already permissible oversized
record of **8 MiB + 4 bytes**. The method still returns at most
128 source line events, per its existing API contract. The
current read uses one small offset array (up to 129 entries)
and a separate bounded event buffer, not a contiguous copy of
the entire journal.

## Read-only, safe and backwards compatible

- UTF-8 snapshot validation and the incremental Markdown
  classification pipeline are unchanged.
- Serialized events and offset-file layout are unchanged;
  existing temporary journals and source-line positions still
  have the same encoding.
- The cursor's **512 MiB event journal** and **256 MiB offset
  journal** default storage budgets remain intact.
- Invalid offsets, overlaps, truncated/oversized records,
  length-header mismatches, malformed JSON and mismatched
  source-line numbers still result in explicit failures.
- Existing cancellation checks are preserved before any
  read and while decoding a batch. A cancelled read does
  not change the validated source file.
- Native Spectre table geometry, repeated headers,
  `--long-tables`, `--max-width`, `--center`, compact
  fallback, canonical source jumps and the independent
  **56-byte mixed visual page records** are unaffected.
- No new public CLI flags or NuGet dependencies.

## What gets counted

The existing internal `ReadRequestCount` counts validated
`MdViewContainerPageCursor.ReadAsync` calls. This number
does **not** change merely because a cursor request now
coalesces journal records.

A new internal `EventReadRequestCount` counts the calls to
`_events.ReadExactlyAsync` for contiguous windows.
For many small rows, one cursor request can result in **one
event-stream read**, replacing two reads per record. Windows
larger than 256 KiB become multiple bounded event-stream
reads. A jumbo line gets its own read.

These counters are test instrumentation, **not OS physical
I/O counters**. `ReadExactlyAsync` may use multiple lower-level
reads, and disk caching, filesystem read-ahead, and workload
distribution affect actual elapsed time. A performance
improvement on HDDs is plausible but **not yet benchmarked**.

## Regression verification

Added `MdViewContiguousJournalReadTests` with **seven test
methods** covering:

- Twenty small records returned through one contiguous
  event-stream read and read again backwards without
  reclassification.
- Ninety long lines spanning **more than 256 KiB** and
  splitting only at complete journal record boundaries.
- One source event **larger than 256 KiB**, read in a
  single bounded stream operation.
- Offset lookahead for a middle-of-journal read versus
  the terminal `_eventBytes` sentinel, with Unicode
  and CRLF source lines.
- Corruption of the stored record's four-byte length,
  rejected before JSON deserialization.
- Corruption of a following offset, rejected before
  allocating a large event buffer.
- Pre-cancelled reads performing no new event-stream
  operations, with subsequent cursor reuse intact.

Existing source cursor, native table planner, mixed
visual index, long-table continuation and compatibility
tests are unchanged and remain part of the focused suite.

## Commands

```bash
git pull
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
dotnet run --project Blackboard/Tools/MdView -- preview-native README.md --max-width 100 --center --long-tables
```

The **preceding** maintainer report confirmed all tests
green after native fragment request batching. The new
contiguous event-read implementation still requires a
fresh .NET build and test run.

See [native table journal request batching](../Native-Table-Journal-Batching/en.md),
[container journal](../Container-Page-Journal/en.md), and
[implementation status](../../IMPLEMENTATION_STATUS.md).
