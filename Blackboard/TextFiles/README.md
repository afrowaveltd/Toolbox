# Afrowave.Toolbox.Blackboard.TextFiles

Independent .NET 10 library for safe, read-only local text-file decoding. Extracted from the Blackboard.Showcase sample on 2026-11-03 and reused by Showcase and the standalone `mdview` CLI. No dependency on Spectre.Console, the Markdown parser, HTTP, GitHub or an executable sample.

## API

```csharp
using Afrowave.Toolbox.Blackboard.TextFiles;

var shortPreview = await TextFilePreviewReader.ReadAsync("notes.txt", maxPreviewBytes: 4096);
var fullDocument = await TextFilePreviewReader.ReadCompleteAsync("README.md");

if (fullDocument.Kind == TextPreviewKind.Text)
    Console.WriteLine(fullDocument.Content);
```

`ReadAsync` validates a bounded prefix (default 16 KiB, configurable 256 bytes–256 KiB); its `Truncated` property reports whether more content exists. `ReadCompleteAsync` decodes the entire stream in small buffers, then returns one complete string (not a bounded-memory document representation). Both detect strict UTF-8, UTF-8 BOM and UTF-16/UTF-32 BOM endianness; invalid byte sequences or unsafe terminal control characters cause `BinaryOrUnsupportedEncoding` rather than replacement text. No file is modified and no fetched text is interpreted as terminal markup.

The source implementation was **moved, not reimplemented**. Existing Showcase text/JSON/Markdown tests still exercise the same code through the new project dependency. Five additional reader-only regression tests were authored in `TextFiles.Tests`.

```bash
dotnet test Blackboard/TextFiles.Tests/Afrowave.Toolbox.Blackboard.TextFiles.Tests.csproj
dotnet test Toolbox.sln
```

**Verification:** the reader extraction was later confirmed by the user on Windows as part of a **6,443/6,443** solution test run, including **5/5 TextFiles tests**. The new disk-backed line index and its additional tests are a **later, unverified** increment. Full visual Markdown streaming, reusable persistent indexes and stronger filesystem mutation/symlink policy remain future work.

[Extraction guide](Docs/Architecture/en.md) · [Implementation status](IMPLEMENTATION_STATUS.md).

## Low-memory UTF-8 line index (2026-11-04)

Added `Utf8DiskLineIndex` for bounded-memory, read-only, **disk-backed random access to physical UTF-8 lines**. It validates the full file in one sequential scan before offering pages, rejects malformed UTF-8 and unsafe terminal controls, and indexes line start byte offsets on disk (8 bytes per line plus a sentinel, default 256 MiB index cap). Pages are limited to 128 physical lines and 4 MiB of source bytes, with a 1 MiB per-line default; budget failures are explicit, not silent truncation. UTF-8 BOM, CRLF/LF/CR and Unicode are supported, including buffer-boundary splits.

```csharp
await using var index = await Utf8DiskLineIndex.OpenAsync("README.md");
var page = await index.ReadPageAsync(0, maxLines: 24);
if (page.HasMore)
    page = await index.ReadPageAsync(page.NextLine, maxLines: 24);
```

The derived temporary index disappears on disposal and does not affect our persistent downloaded README cache. This is a **source line index**, not an incremental Markdown parser; the normal complete-document renderer remains unchanged. The first terminal consumer is `mdview source README.md`, explicitly displaying raw source pages. [Architecture, safety and limits](Docs/Disk-Line-Index/en.md). New tests are committed, but a build/test pass is still pending.

## UTF-16 line-column to UTF-8 byte mapping (2026-11-07)

After `OpenAsync` validates the full source, `GetByteOffsetAsync(lineNumber,
utf16Column)` maps a **zero-based physical line** and **UTF-16 content column**
to a physical UTF-8 byte offset. It reads only one bounded line and its disk
index entry, accounts for initial UTF-8 BOM bytes, handles multibyte Unicode
and mixed CRLF/LF/CR, and rejects columns that split surrogate pairs.

```csharp
await using var index = await Utf8DiskLineIndex.OpenAsync("README.md");
long offset = await index.GetByteOffsetAsync(0, 5);
```

This is not yet an **absolute document-wide UTF-16-offset** mapper or an
immutable-file snapshot. The source can still change in place between
validation and a later read. Do not attach untrusted streaming Markdown
rendering to this helper until stable source identity and cumulative UTF-16
line-start mapping are implemented.

[Line-index byte-offset guide](Docs/Disk-Line-Index/en.md). Four focused
tests were added for BOM/Unicode/CRLF, surrogate boundaries, empty lines
and random access across large lines. **Fresh build/tests pending**.

## Document-wide UTF-16 offsets (2026-11-07)

`Utf8DocumentOffsetIndex` adds a **separate disk-backed index** of decoded
UTF-16 line starts and terminator lengths. It accepts the absolute positions
emitted by incremental Markdown source scanners; initial UTF-8 BOM bytes are
excluded from UTF-16 coordinates, while CR/LF/CRLF terminators are included.

```csharp
await using var lines = await Utf8DiskLineIndex.OpenAsync("README.md");
await using var positions = await Utf8DocumentOffsetIndex.CreateAsync(lines);
long byteOffset = await positions.GetByteOffsetAsync(42);
```

The temporary table uses 16 bytes per physical line (256 MiB default cap),
is deleted on disposal, and does not retain the whole document in RAM.
Creation currently performs multiple random reads per line, so large HDD
throughput remains **unoptimized and unbenchmarked**. The source file is
not guaranteed immutable across indexing and display; do not yet use this
as authorization for progressive untrusted rendering.

[Details and resource limits](Docs/Disk-Line-Index/en.md).
New tests and compilation **await verification**.

## Batched disk index construction (2026-11-07)

Creating `Utf8DocumentOffsetIndex` now uses a bounded batch of up to
128 physical line starts from the existing disk index instead of repeatedly
decoding/seeking each line solely to discover terminator lengths.
The public API and 16-byte-per-line temporary format are unchanged.
This eliminates per-line byte-position lookups during table creation, but
`ReadPageAsync` still performs per-line source seeks; a strictly
single-pass HDD-optimized scan and immutable-source handling remain future
work. No performance figures are claimed. Additional large mixed-encoding
regressions are committed; fresh test execution is pending.


## Contiguous source paging (2026-11-07)

`Utf8DiskLineIndex.ReadPageAsync` now loads a selected page with
one **contiguous source read**, after selecting offsets from the
on-disk line table. The existing byte and line limits remain:
128 physical lines, 4 MiB per page, configurable per-line cap,
strict UTF-8 validation and `NextLine` continuation.

The public API has not changed. This eliminates one seek per line
during a source-page read but does not yet deliver a fully immutable
source snapshot or guarantee measured HDD throughput. Two focused
regressions cover large mixed Unicode/BOM/newline pages and
byte-budget page boundaries. Verification is pending.


## Private validated UTF-8 snapshots (2026-11-07)

`Utf8ValidatedSnapshot` creates a private temporary copy of a local file,
then builds a strict UTF-8 line index **from that completed copy** before
exposing its `Lines` property. The original file is never written.
The copy uses a 64 KiB sequential buffer and an explicit default 16 GiB
temporary-space budget; size overruns and late-invalid UTF-8 or unsafe
terminal controls fail before the snapshot is returned.

```csharp
await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync("README.md");
var page = await snapshot.Lines.ReadPageAsync(0, maxLines: 24);
await using var positions = await Utf8DocumentOffsetIndex.CreateAsync(snapshot.Lines);
```

The snapshot owns and disposes its line index, read-only guard and private
temporary directory. Dispose any dependent UTF-16 document index **before**
disposing the snapshot. Updates to the original path after copying do not
alter the validated copy. This is a stable *private working copy*, not a
cryptographically authenticated snapshot of the original at one instant:
source changes during the copy may produce a mixed but independently
validated copy, and same-user/privileged filesystem tampering remains
outside this component's threat guarantees. On Windows a read-sharing
read-only guard also blocks ordinary write opens to the copy.

This is an opt-in foundation only. Existing mdview rendering remains
unchanged; future formatted paging can consume the snapshot after
additional renderer and source-span parity work. Four new tests were
authored; new build/CI verification is pending.


## Configurable snapshot workspace and cleanup (2026-11-07)

`Utf8ValidatedSnapshot.CreateAsync` now accepts an optional
`temporaryRoot` argument **after** the existing cancellation token, preserving
previous positional callers. It must designate an existing directory; when
omitted, the OS temporary directory is used. A unique
`afrowave-utf8-snapshot-*` child directory is created inside it; the
caller-owned root is never removed.

```csharp
await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(
    "README.md",
    temporaryRoot: "/mnt/large-temp");
```

The path is illustrative; use an existing location suitable for the current
operating system and available free space. The snapshot removes its own
temporary files on disposal and after a failed copy/validation. Ordinary
cancellation before copying does not create a working directory. On Windows,
the read-only handles are closed before deletion.

Four lifecycle tests cover successful disposal, invalid-content cleanup,
byte-budget failure cleanup and pre-cancelled operations using an isolated
test directory. **Fresh CI/build confirmation is pending**. Workspaces are
not a defense against malicious same-user tampering and process termination
may leave orphaned temporary directories; optional startup cleanup remains
future work.
