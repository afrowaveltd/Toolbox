# Strict UTF-8, disk-backed physical line index

**Implementation:** 2026-11-04. This is the first *source indexing* milestone toward a future native Markdown pager. It is **not** a streaming Markdown parser, a wrapped visual-line index, or a replacement for the existing complete-document renderer.

## Goal and boundaries

Large README and help files should be navigable without allocating memory proportional to the entire file. `Utf8DiskLineIndex` provides a reusable, UI-independent primitive in `Afrowave.Toolbox.Blackboard.TextFiles`, with the existing whole-document API unchanged.

On `OpenAsync`, the reader opens a local file **read-only**, validates every byte as strict UTF-8 and rejects unsafe terminal controls (except CR, LF and tab). This is a sequential, bounded-buffer pass, so a malformed sequence **at the end** rejects the file **before any page is displayed**. UTF-8 BOM is supported. The reader writes physical line start offsets to a temporary disk file; it never stores the full offset table in RAM.

CRLF (including across scan-buffer boundaries), LF and CR are treated as line endings. The index stores one 64-bit offset per physical line plus a final file-length sentinel. A trailing newline does not introduce a phantom line; internal empty lines are retained. Files with UTF-16 or UTF-32 BOMs are **not** accepted in this initial indexed mode; the existing `TextFilePreviewReader.ReadCompleteAsync` still supports those encodings for normal fully rendered Markdown documents.

## Public API

```csharp
using Afrowave.Toolbox.Blackboard.TextFiles;

await using var index = await Utf8DiskLineIndex.OpenAsync("README.md");
Console.WriteLine($"Lines: {index.LineCount}; disk index: {index.IndexBytes} bytes");

var page = await index.ReadPageAsync(firstLine: 0, maxLines: 24);
foreach (var line in page.Lines)
    Console.WriteLine(line);

if (page.HasMore)
{
    var next = await index.ReadPageAsync(page.NextLine, 24);
    // Reverse lookup also works: index.ReadPageAsync(0, 24).
}
```

`Utf8LinePage` returns the zero-based `FirstLine`, the continuation point `NextLine`, `TotalLines`, a bounded `Lines` list, and `HasMore`. The page is read using source/offset seeks; it is possible to request an earlier physical line without reparsing the entire document. All source text is rendered literally by callers, never interpreted as terminal markup.

## UTF-16 line-column to UTF-8 byte mapping (2026-11-07)

`GetByteOffsetAsync(lineNumber, utf16Column)` is a bounded, random-access
mapping for **one validated physical line**. The line number is zero-based,
and the UTF-16 column measures decoded line *content*, excluding the
terminator and the optional initial UTF-8 BOM. An end-of-line content column
maps to the first line-terminator byte, or EOF if there is no terminator.

```csharp
await using var index = await Utf8DiskLineIndex.OpenAsync("README.md");
long byteOffset = await index.GetByteOffsetAsync(lineNumber: 12, utf16Column: 4);
```

The implementation reuses `ReadPageAsync(..., 1)` to enforce the existing
maximum physical-line budget and strict decoding. It counts UTF-8 bytes in
the selected UTF-16 prefix, adds the on-disk line-start offset and handles
the optional three BOM bytes only on the first line. Columns splitting a
surrogate pair and columns outside a line are rejected. Files are not
modified, and the full document is never materialized.

This API deliberately does **not** translate an absolute UTF-16 position
into a byte offset: doing so requires a separately maintained cumulative
UTF-16 line-start index (and a defined BOM/line-ending coordinate policy).
It is also not an immutable content snapshot: a same-size in-place source
change during indexing/reading can escape size-only detection. Stronger
snapshot identity is required before using stored block offsets for
untrusted progressive terminal rendering.

## Document-wide UTF-16 to UTF-8 positions (2026-11-07)

`Utf8DocumentOffsetIndex` is a separate, optional disk-backed table that
maps **absolute decoded UTF-16 positions** (as produced by Markdown's
source-preserving scanner) to positions in the original UTF-8 file.
It records each physical line's UTF-16 start, content length and line-ending
length (16 bytes per line) in a temporary `DeleteOnClose` file.
The initial UTF-8 BOM counts as **zero** UTF-16 units but **three** bytes;
CRLF contributes two UTF-16 units, LF/CR one each.

```csharp
await using var lines = await Utf8DiskLineIndex.OpenAsync("README.md");
await using var positions = await Utf8DocumentOffsetIndex.CreateAsync(lines);
long byteOffset = await positions.GetByteOffsetAsync(markdownSpan.StartUtf16Offset);
```

Construction walks bounded source pages and performs per-line byte-position
lookups. Queries binary-search the derived disk table and use the existing
strict one-line mapper. Middle-of-surrogate-pair positions are rejected rather
than converted into replacement characters. EOF maps to the exact source
byte length; a position between CR and LF maps between those two bytes.
Empty sources have a zero-length table.

This first implementation is **correctness-first**, not an HDD performance
claim: construction currently issues multiple random seeks per physical line.
A later coordinated sequential index writer can significantly reduce I/O.
The index must be disposed before its borrowed `Utf8DiskLineIndex`.
Its 256 MiB default disk budget is independent of the source's physical-line
index budget. No full-document string or in-memory offset array is retained.

**Integrity limitation:** index construction and later lookups use the
existing read-only handle but do not pin an immutable snapshot. Concurrent
same-length replacement or mutation is not ruled out, particularly on Unix.
Before progressive rendering of untrusted source, use an immutable validated
spool or implement a stronger source-identity/content policy. This API does
not yet enable formatted paging in mdview.

## Resource budgets

- Scan/validation buffer: **64 KiB** of source bytes plus a small UTF-8 decoder buffer.
- Temporary offset table: **8 bytes per line + 8 bytes sentinel**, capped at **256 MiB** by default. A budget overrun reports an explicit error rather than partially indexing a file.
- One physical line: maximum **1 MiB** by default; configurable in the `OpenAsync` call from 4 KiB to 4 MiB. Oversized lines are reported explicitly on page read and never silently cut off.
- Page: up to **128 physical lines** and **4 MiB of source bytes**. A page may stop early at the byte budget; `NextLine` preserves the exact continuation point.
- Temporary index lifecycle: the random index file is opened with `DeleteOnClose` and removed when the index is disposed or indexing fails. It is *derived* local metadata; it is unrelated to the separately persistent and offline-friendly remote Markdown document cache.
- Initial scan: **O(file bytes)** before the first displayed page. Page reads after indexing seek to their indexed byte range. No constant-time first-page claim for a new multi-gigabyte source.

A successful scan validates the complete byte stream at that point. Page reads recheck each decoded line and source length to detect common modifications, but concurrent in-place same-length mutation cannot be perfectly excluded on every filesystem. For untrusted or mutable files, a future immutable snapshot/spool policy must provide stronger guarantees.

## Explicit source pager in mdview

The first consumer is a separate **raw source** pager:

```bash
dotnet run --project Blackboard/Tools/MdView -- source README.md
```

This shows complete validated physical lines with their line numbers and controls for Next page, Previous page, Go to line and Exit. It is deliberately **not** advertised as a rendered Markdown pager: it does not preserve Markdown block state, syntax colors, inline links, soft-wrap visual rows, heading navigation or browser history. The ordinary `mdview local README.md` and GitHub/HTTPS commands still use the existing complete formatted renderer.

## Verification

```bash
dotnet test Blackboard/TextFiles.Tests/Afrowave.Toolbox.Blackboard.TextFiles.Tests.csproj
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
```

Regression tests cover UTF-8 BOM and Unicode, CRLF/LF/CR, cross-buffer multibyte code points and CRLF, forward/backward pages, final newline semantics, late unsafe bytes, invalid UTF-8, UTF-16 rejection, per-line and index budgets, byte-limited pages, cancellation and invalid parameters. CLI virtual-terminal tests cover paging, jump, cancellation, literal markup and rejection of unsafe input.

**No build/test execution is confirmed for this increment** in the current environment without .NET SDK. Earlier user-confirmed 6,443/6,443 Windows tests refer to previous source snapshots.

## Next Markdown milestones

1. A source-aware incremental Markdown block parser with explicit fence/list/quote state and bounded lookahead.
2. Disk-backed rendering/block index and a visual-line pager (terminal-width wrapping, colors, reverse navigation, navigation to headings and links).
3. Optional persistent/reusable content-addressed line indexes for unchanged downloaded documents, avoiding repeated full scans.
4. UTF-16/UTF-32 source indexing or safe transcoding to an immutable UTF-8 spool where appropriate.
5. Measured 1 GiB+ files on slow HDD/old hardware, including cancellation, Unicode/emoji and terminal resize.

[TextFiles README](../../README.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md) · [mdview source pager](../../../Tools/MdView/Docs/Raw-Source-Pager/en.md).

## Batched UTF-16 table construction (2026-11-07)

`Utf8DocumentOffsetIndex.CreateAsync` now fetches up to **128 physical
line-start offsets with one table read**, matching the bounded
`Utf8DiskLineIndex.ReadPageAsync` window. For each decoded line, the
UTF-8 byte length of its content is subtracted from the difference between
the current and following physical line starts. The remaining one or two
bytes identify LF/CR or CRLF, while an optional initial UTF-8 BOM is
accounted for once.

This replaces two calls to `GetByteOffsetAsync` **per line** with a
single batched read of the existing offset table **per page**. The
resulting 16-byte-per-line UTF-16 metadata is still written incrementally
and retains the same disk budget, source-coordinate semantics, and
binary-search lookup API.

The physical-line reader still seeks for individual lines within each page,
and source immutability has not been proven. This is a reduction in random
index access, **not** a verified one-pass source scanner or benchmarked HDD
throughput improvement. A future coordinated first-pass writer or immutable
spool could eliminate remaining per-line seeks.

Additional regressions sample offsets across more than 300 mixed
Unicode/newline lines, including 128-line batch boundaries, and cover
a BOM-only source and final unterminated line. Compilation and CI
verification must be recorded separately.


## Contiguous bounded page reads (2026-11-07)

`Utf8DiskLineIndex.ReadPageAsync` now reads the selected physical
lines as **one contiguous source-byte range** instead of seeking and
reading separately for every physical line. The existing small disk-offset
table read determines the included prefix before the source read.
Each selected line still undergoes strict UTF-8 decoding, unsafe-control
rejection, BOM handling and newline trimming independently.

Selection enforces the existing maximum 128 lines and maximum 4 MiB
of source bytes per page, plus the configured per-line byte limit.
If the next line would exceed the page budget, that line is not consumed:
`NextLine` still points to its exact physical-line number.
Reverse/random-access pages, empty lines and CRLF/LF/CR boundaries
retain the same public contract.

This removes per-line **source seeks within a page** and allows the OS to
service a contiguous read efficiently, including on HDDs. It does not
turn the initial full-file validation into a one-pass combined index,
make the source immutable, or prove a particular throughput improvement.
The same-size concurrent file mutation limitation still applies.

Two new regressions exercise a 261-line BOM/Unicode document with
mixed terminators across 128-line page boundaries, and a page-byte
budget interruption across 100 long lines with exact continuation.
Fresh CI and build results are tracked in the implementation status.


## Validated private source snapshot (2026-11-07)

For consumers that cannot tolerate a changing original file between
validation and paging, `Utf8ValidatedSnapshot.CreateAsync(path)` creates
a **private temporary working copy** before opening
`Utf8DiskLineIndex`. It copies with a fixed 64 KiB buffer (16 GiB
default copy-size cap), performs strict whole-file UTF-8 and terminal
control validation on the *completed copy*, and only then returns access
to its read-only line index. Cancellation, invalid content and budget
failures dispose partial resources and remove the temporary directory.

```csharp
await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync("README.md");
var firstPage = await snapshot.Lines.ReadPageAsync(0, 24);
await using var positions = await Utf8DocumentOffsetIndex.CreateAsync(snapshot.Lines);
long startByte = await positions.GetByteOffsetAsync(12);
```

A snapshot owns its physical-line index; callers must dispose dependent
offset indexes first. The snapshot's temporary directory is removed upon
disposal. The separate remote document cache is not modified.

**Exact scope:** the displayed bytes are isolated from subsequent changes
to the original path, but the source could change during copying; that may
create a mixed (still independently validated) byte stream rather than a
point-in-time original version. Access by same-identity or privileged
processes to private temporary storage is not treated as hostile. On
Windows a read-only guard shared for reads rejects ordinary attempts to
open the snapshot for writing; Unix filesystem behavior is not claimed
as a mandatory lock. To establish original-version identity later, add
content digests and a defined source-change policy.

This does not by itself implement progressive Markdown rendering, a
single-pass combined index or a benchmarked HDD throughput guarantee.
