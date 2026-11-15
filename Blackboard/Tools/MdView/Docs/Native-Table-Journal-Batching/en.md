# Batched source-journal reads for native Spectre tables

## Goal and measured contract

MdView's opt-in native table pagination uses a validated Markdown
source snapshot and a **disk-backed container classification journal**.
The native fragment planner repeatedly checks a maximum of six physical
table data lines plus one continuation lookahead.

Previously, `MdViewNativeTablePagePlanner.PlanAsync` requested each
source line independently:

- Up to **six one-line calls** to `MdViewContainerPageCursor.ReadAsync`
  for the data rows;
- Plus **one additional one-line call** to decide whether a seventh
  source line continues the table.

Each `ReadAsync` call reads the journal's offset table and associated
event records. On rotational HDDs and other high-latency storage, the
extra per-line offset lookups can be unnecessarily expensive.

The planner now performs **one bounded request**:

```csharp
var sourceWindow = await cursor.ReadAsync(
    firstDataLine, maximumSourceRows + 1, cancellationToken);
```

`maximumSourceRows` is limited to **1–6**, so the returned window
contains at most **seven classified physical lines**, including the
single lookahead. The planner reuses this short window while adding
and measuring candidate Spectre rows. It never buffers an entire table.

## What this improves — and what it does not

For a six-row fragment with a following row, the source journal sees
**one request rather than seven**. For a 14-row fixture displayed as
6 + 6 + 2 source rows, the entire three-fragment preflight needs
**three cursor read requests** instead of the former sequence of
independent row/continuation requests.

This is a deterministic reduction in **API-level journal requests**,
not a guaranteed same-factor reduction in physical reads or elapsed
time. The cursor still accesses and deserializes each serialized
line event from disk. Actual latency and throughput depend on the
filesystem, cache, drive and size of individual Markdown records.
No HDD benchmark result is claimed for this change.

The short batch can classify and materialize up to six extra physical
lines beyond an early end of a table. This increases bounded
lookahead work in that situation, but it never marks those lines as
consumed by the table. Their classified events remain available
through the same disk-backed journal for subsequent Markdown pages.
The existing journal byte/offset limits and fail-closed behavior
remain intact. Each classified source event already obeys the
cursor's **8 MiB serialized record limit**.

## Source, geometry and fallback invariants

- The original header and delimiter source lines are unchanged.
- `FirstDataLine`, `NextDataLine`, `DataRows`,
  `VisualRows`, `HasMore` and `FallbackReason` retain
  their existing definitions.
- Spectre widgets are still measured **after each candidate row**,
  against the actual terminal and requested visual row budget.
- A row that does not fit is **not consumed**; it is retried at
  the same physical source position in the next fragment.
- A continuation row is detected from the already-read window.
- A non-table line remains a normal Markdown source line.
- Header-too-tall and narrow-grid fallback still occur **before**
  any data-window read.
- A pre-cancelled request does not access the journal.
- Centering, reading-width caps, line jumps, the optional
  `--long-tables` mode and the **56-byte mixed page index**
  are unchanged.
- No new public CLI flags or dependency packages are required.

## Deterministic regression checks

`MdViewContainerPageCursor` now exposes an **internal**
`ReadRequestCount` diagnostic property. It counts validated
`ReadAsync` invocations after argument and cancellation checks.
This does **not** estimate bytes read or OS I/O operations.
It is internal to avoid expanding MdView's consumer API.

`MdViewNativeTableBatchReadTests` covers:

1. A 14-data-row table: exactly three journal requests for its
   6 + 6 + 2 native fragments, correct boundaries, no repeated
   or missing rows and unchanged original source bytes.
2. A height-limited row: the rejected next data row remains
   available on the following visual page, with one source
   window request per fragment.
3. End-of-table and missing-continuation behavior: no accidental
   consumption of the prose after the table.
4. The long-table source-complete preflight: precisely three
   journal requests for the three measured fragments.
5. Narrow-grid fallback and pre-cancelled requests: zero new
   journal requests; the validated cursor remains reusable.

The existing native-planner, disk-page-index, whole-document
long-table and compact fallback tests remain in place.

## Verification

```bash
git pull
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
dotnet run --project Blackboard/Tools/MdView -- preview-native README.md --max-width 100 --center --long-tables
```

Check the targeted batch-read tests first, then the full solution.
For real HDD performance analysis, use repeated comparable runs
on the same source and disk, distinguishing cold-cache from
warm-cache results. Do not interpret the API request-count ratio
as a measured speedup.

**The maintainer confirmed the earlier long-native-table stage
green.** This new batching increment still requires a fresh
.NET test run before it can be marked verified.

Related: [native table page planner](../Native-Table-Page-Planner/en.md),
[long native tables](../Long-Native-Tables/en.md), and
[implementation status](../../IMPLEMENTATION_STATUS.md).
