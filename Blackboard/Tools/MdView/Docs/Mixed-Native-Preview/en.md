# Opt-in mixed Markdown / native Spectre preview

## Usage and compatibility

The CLI now provides an explicitly selected **mixed visual preview**:

```bash
mdview preview-native README.md
dotnet run --project Blackboard/Tools/MdView -- preview-native README.md
```

The earlier commands have **not changed**:

- `mdview preview README.md`: established compact disk-indexed Markdown
  pagination with its existing native table inspector action.
- `mdview source README.md`: existing raw source pager.
- `mdview local README.md`: existing complete Markdown rendering.

The `preview-native` verb is matched case-insensitively. It opens a
local UTF-8 Markdown file validated before display and requires an
interactive Spectre terminal. It does not modify source bytes or fetch
any external resource.

This is still **experimental**. Native in-page grids are limited to
confidently recognized, complete small tables of at most six data rows.
Long or oversized tables and narrow terminal profiles use the
established compact cell layout consistently across all pages. No
full-document text or table is held in RAM by the mixed pager.

## Navigation

The new `MdViewMixedPreviewPager` owns its terminal menu. Available
actions depend on the current page and whether the source is empty:

- **Next page** and **Previous page**: navigate indexed canonical
  document pages with correct `Page X of Y` numbering. Previous
  does not need an in-memory history stack.
- **Go to source line**: enter a one-based original Markdown line.
  A disk-backed binary search identifies the **canonical page
  containing that line**, including a source data row inside a
  native Spectre table. The page may begin *before* the requested
  line; it does not invent a new mid-table starting state.
- **Find heading (disk index)**: use the existing bounded
  `MdViewDiskNavigationIndex` and `MdViewDiskNavigationPicker`
  to choose an actual source heading, then jump to its canonical
  mixed page.
- **Find link location (disk index)**: choose a validated source
  link position and jump to the page containing it. This locates
  the link **without following or opening the destination**.
- **Exit mixed preview** or Escape: return without modifying
  the original Markdown document.

The UI shows a source-line range, canonical `Page X of Y`,
the total physical line count, and a brief horizontal-code-clipping
notice where applicable. All headers and status messages are
clipped to the actual terminal width rather than wrapping into
extra lines.

## Viewport and resize

The mixed pager reserves at least sixteen terminal rows for its
header, status and navigation menu. Its maximum content budget is
128 rows, but **even a user-specified `linesPerPage` limit cannot
consume the reserved control area**.

At least **28 columns and 18 rows** are required. The viewer
returns with a clear message instead of attempting an unusable
prompt below that size. The established compact preview remains
available as a separate command.

The pager checks width, height and Unicode capability before
drawing each new page. On a resize it disposes the old mixed
page index and rebuilds it from the same validated source
snapshot at the new dimensions. It repositions to the canonical
new page containing the original physical source-line anchor,
not the previous width-dependent wrap-row offset.

A resize cannot interrupt a Spectre selection prompt; as with
the established compact pager, it is handled on the next draw.
The `MdViewMixedVisualIndexedRenderer` independently rejects
stale source cursor or viewport geometry before writing page
content.

## Disk, source and memory model

The mixed mode is deliberately built from the already-tested
components rather than implementing a second pagination formula:

1. `Utf8ValidatedSnapshot` validates local source bytes once.
2. `MdViewContainerPageCursor` provides classified source
   lines using its disk-backed event journal.
3. `MdViewMixedVisualPageIndex` stores the full document's
   **56-byte fixed-size page records** in a temporary file.
4. `MdViewMixedVisualIndexedRenderer` reads one selected
   page record, reconstructs a bounded page plan, verifies its
   exact source/row geometry, then renders it.
5. The source navigation index is allocated **lazily**, only
   when heading or link search is selected.

The default mixed index disk budget is 64 MiB and records
are removed when the index is disposed. The original Markdown
document is read-only throughout.

## Tests

The new `MdViewMixedPreviewPagerTests` exercise:

- Complete native grid between prose with original bytes unchanged.
- Next/previous canonical page navigation.
- Source-line jump into a native table row.
- Disk-indexed distant heading and link selection.
- An invalid source-line entry leaving the current page intact.
- Long source tables remaining compact across pages.
- Narrow terminal compact fallback.
- An empty document's single indexed page.
- A very small terminal returning without attempting source I/O.
- Explicit page-row limits respecting the reserved control area.
- Interactive-mode and argument preconditions.
- Rejection of unsafe terminal control characters in source files.

The existing `MdViewCommandTests` also verify case-insensitive
`preview-native` parsing and unchanged `preview` behavior.

**Fresh .NET test execution is still pending for this increment.**
The maintainer's previous report confirmed **263 passed, zero failed,
zero skipped**, with one `xUnit2020` analyzer warning; that warning
was corrected first using `Assert.Fail`.

### Verification

```bash
git pull
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
dotnet run --project Blackboard/Tools/MdView -- preview-native README.md
```

For a manual smoke test, try a Markdown file containing both a small
two-column table and a table with more than six data rows. Check
forward/backward page counts, source-line jumps into each table, and
resizing the terminal between navigation steps.

See [mixed page disk index](../Mixed-Visual-Page-Index/en.md),
[mixed page planner](../Mixed-Visual-Page-Planning/en.md),
and [implementation status](../../IMPLEMENTATION_STATUS.md).

## Optional content width (2026-11-15)

For long prose on a wide monitor, choose a maximum width for
the document body:

```bash
mdview preview-native README.md --max-width 100
```

`N` is a whole number between **40 and 4096**. Omit the
option to retain the full terminal width. Only the content
layout is constrained: page header, footer and interactive
navigation use the actual physical terminal. A narrow cap
can move otherwise eligible Spectre grids to the compact
table mode. Both the disk index and page renderer use the
same cap, preserving canonical source-line jumps and
`Page X of Y` after rewrapping.

Terminal resize rebuilds the index even when the effective
reading width would be unchanged; no console profile is
modified. The content is left-aligned for now.

[Full content-width contract](../Mixed-Content-Width/en.md).

## Optional centered reading column (2026-11-15)

The opt-in `preview-native` UI can center its constrained
document-body column in a wide terminal:

```bash
mdview preview-native README.md --max-width 100 --center
```

It preserves left-alignment of individual Markdown rows
and native table cells **within** the centered reading area.
The **whole viewer** now shares the same left edge: document rows,
file/title header, page status, feedback, selected navigation arrow,
heading/link picker and source-line input prompt. Only the text inside
the reading column remains left-aligned; the physical console profile
is never changed.
Native table padding adds no top/bottom rows and is
measured before display, preserving exact indexed
`Page X of Y` and physical source-line jumps.
The default without `--center` is unchanged.

[Full centered-alignment guide](../Mixed-Content-Alignment/en.md).

## Unified full-screen centering (2026-11-15)

The initial `--center` implementation positioned only Markdown rows
and native Spectre tables. A real terminal screenshot showed the
header, `Page X of Y`, status and keyboard navigation at the far
left. The **same calculated reading-column gutter** now applies to
those controls and feedback as well.

Interactive `SelectionPrompt` lists (including disk-indexed heading
and link pickers) now use a scoped `IRenderHook` with
horizontal-only padding on Spectre's **live renderable**. The
earlier forwarding console adapter failed to center this
late-rendered menu and was removed. The corrected approach
preserves native input, cursor movement and selection arrows. Incremental `TextPrompt` input uses an
indented title on the original console instead, avoiding repeated
spaces before typed characters.

Default uncentered preview, native widget geometry, page indexes,
source jumps and resize rebuilds remain unchanged.
[Detailed layout contract](../Mixed-Content-Alignment/en.md).

### Live selection menu placement (2026-11-15)

A Konsole visual test exposed the remaining left-aligned bottom
menu. Spectre creates the visible selection menu using a
`ListPromptRenderHook` during live rendering, not in the original
`IAnsiConsole.Write` call. The new
`MdViewCenteredPromptRenderHook` is attached before the
selection begins and pads the actual live widget once it exists.
Cursor controls are excluded from padding. The same mechanism
is used for the heading/link pickers.

The result is the equivalent of a **borderless centered menu
container** using Spectre's existing interactive grid, with
no extra border or vertical rows. The previous forwarding console
adapter has been removed. Fresh .NET test and visual verification
is pending.

## Opt-in long native tables (2026-11-15)

The previously compact-only display of long pipe tables
can now be changed explicitly:

```bash
mdview preview-native README.md --max-width 100 --center --long-tables
```

With `--long-tables`, a source-complete bounded preflight
checks every planned native fragment before the first
one is emitted. Eligible long tables are shown as
multiple small native grids with the **original Markdown
header repeated**. Exact original source rows and their
canonical disk page numbers are preserved. A later
fragment that would not fit, an excessively long table
or a narrow reading column forces the **whole table**
back to compact mode.

Without this flag the earlier behavior remains unchanged.
[Complete long native table contract](../Long-Native-Tables/en.md).
