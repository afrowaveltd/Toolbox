# TextFiles — implementation status

## 2026-11-03 — extraction from Showcase

- Created `Blackboard/TextFiles` as an independent `net10.0` library, with the original strict `TextFilePreviewReader`, `TextPreviewKind` and `TextPreviewResult` implementation moved from `Blackboard/Samples/Blackboard.Showcase`; namespace changed to `Afrowave.Toolbox.Blackboard.TextFiles`.
- Original sample copy removed. Both `MdView` and `Blackboard.Showcase` now use normal `ProjectReference` and importing the new namespace, with no cross-project `Compile Include` and no executable dependency.
- Updated Showcase JSON preview, text preview, Markdown preview and text/JSON tests to consume the extracted public types. Existing behavior intended unchanged.
- Added `Blackboard/TextFiles.Tests` with five independent tests (unicode, bounded prefix, unsafe terminal character beyond prefix, UTF-16 BOM, invalid UTF-8).
- Both projects registered in `Toolbox.sln`.
- **Tests not yet rerun for this extraction**; the user-confirmed 6,438-test Windows pass refers to the preceding commit. Current compile/test success is not yet verified. Next: run full solution tests and ensure no namespace/solution setup regressions; then work on user-visible `mdview` FileBrowser selection and paging.

## 2026-11-03 — Windows test log review: missing test project

- User's second Windows `dotnet test Toolbox.sln` output confirms the new TextFiles **library builds** and the existing suites pass, including `MdView.Tests` 10/10 and Showcase.Tests 39/39.
- **Important:** `TextFiles.Tests` was absent from the executed assembly list. The output reports only 27 project restore entries while `Toolbox.sln` contains 28 C# projects; the five new reader-specific tests therefore are **not confirmed passed**.
- Corrected the solution's project configuration mapping for both TextFiles projects by adding `Debug|x64`, `Debug|x86`, `Release|x64`, `Release|x86` ActiveCfg and Build.0 entries, consistent with other solution projects. Missing mappings are a likely reason the tests were skipped under a non-Any-CPU solution platform; this is not yet independently confirmed.
- Required follow-up: rerun `dotnet test Toolbox.sln` and check specifically that `Afrowave.Toolbox.Blackboard.TextFiles.Tests.dll` executes **5/5**. A direct project test command is also useful: `dotnet test Blackboard/TextFiles.Tests/Afrowave.Toolbox.Blackboard.TextFiles.Tests.csproj`.

## 2026-11-03 — extraction verification passed on Windows

- Follow-up user-provided `dotnet test Toolbox.sln` log confirms the extracted TextFiles library builds successfully and all **5/5 TextFiles.Tests pass**. Whole solution: **6,443/6,443 green, no failures/skips**.
- The earlier omitted test project was corrected by complete x64/x86 solution configuration entries. This local Windows verification does not establish separate Linux CI or interactive terminal acceptance.

## 2026-11-04 — bounded UTF-8 disk line index

- Added `Utf8DiskLineIndex` and `Utf8LinePage` in TextFiles. The source is validated sequentially as strict UTF-8 before pages are made available. CRLF (including buffer boundaries), LF, CR and UTF-8 BOM are supported; unsafe terminal controls and invalid bytes anywhere reject the file. UTF-16/32 remain supported only in the older complete reader.
- Writes 64-bit physical-line offsets to a temporary `DeleteOnClose` index (8 bytes per line plus sentinel, 256 MiB default disk limit). No full-document line table or Markdown syntax tree is kept in RAM. Reading is bounded to 128 source lines and 4 MiB of bytes per page, with explicit 1 MiB/line default failure and `NextLine` for lossless continuation. Validated content is read-only.
- Added 15 focused tests in TextFiles.Tests for empty/trailing lines, mixed endings, Unicode/BOM and buffer splits, random access, byte/index/line budgets, late-invalid UTF-8, unsafe controls, UTF16 rejection, cancellation and invalid parameters.
- [Detailed implementation](Docs/Disk-Line-Index/en.md). A first explicit raw-source terminal consumer is in [mdview](../Tools/MdView/Docs/Raw-Source-Pager/en.md); the normal complete Markdown renderer is unchanged.
- **New tests and compilation are not confirmed** in this environment (no .NET SDK). The previously reported 6,443/6,443 Windows pass predates this work. Next: user/CI `dotnet test Toolbox.sln`; afterward implement stateful incremental Markdown parsing and visual-row paging separately.

## 2026-11-04 — Windows validation checkpoint

- The earlier full Windows test log showed **TextFiles.Tests 20/20 green**, including the newly added strict UTF-8 disk line index. Maintainer subsequently confirmed that the final, corrected solution test run is entirely green.
- The disk index is the verified **raw source** navigation primitive, not a completed formatted streaming Markdown parser. Further Linux, performance and mutation-race hardening remain future work.

## 2026-11-07 — bounded UTF-16 line-column to UTF-8 byte positions

- Added `Utf8DiskLineIndex.GetByteOffsetAsync(long lineNumber, int utf16Column, CancellationToken)`, a source-preserving read-only mapping from a **physical line and decoded UTF-16 content column** to the corresponding absolute UTF-8 byte offset in the indexed file. It reuses one-line bounded validation and a disk-backed line-start entry; optional first-line BOM contributes three bytes.
- The API rejects negative/out-of-range lines or columns and a column splitting an emoji surrogate pair. It maps a line-end column to the first terminator byte or EOF and supports mixed CRLF/LF/CR. It does not yet map a document-absolute UTF-16 position, validate a stable immutable snapshot identity, or offer visual Markdown paging.
- Source commit `db31e7af6b7a33ef2f6698820e9f7a62cabb9721`; four focused test cases `85d3bccd5a6f203aba22742334e61297ad24a874`; English [technical guide](Docs/Disk-Line-Index/en.md) and README updated.
- **Fresh test and CI validation pending in this environment.** The user-confirmed green solution state applies to the preceding Markdown regression milestone, not necessarily to this newly introduced method.
- Next: source-identity/immutability and cumulative UTF-16 line-start mapping with explicit source-coordinate semantics before connecting the completed Markdown block spans to a formatted disk pager.

## 2026-11-07 — document-absolute UTF-16 to UTF-8 disk mapping

- Introduced independent `Utf8DocumentOffsetIndex` with a temporary 16-byte-per-physical-line table containing UTF-16 line starts, decoded content lengths and line terminator lengths. It maps document-absolute UTF-16 offsets, including CR/LF/CRLF and EOF, to UTF-8 source byte positions by disk-backed binary search plus existing bounded line decoding. Initial BOM is excluded from UTF-16 coordinates, retained in byte coordinates.
- Source commit `525266761512b202b545169a210a609f20896ee9`; four regression tests in `b9b7fb8bd02a95ecca3947f8f1d8a377675afe82`; English docs in [Disk Line Index](Docs/Disk-Line-Index/en.md) and README.
- Table construction remains bounded-memory but performs per-line random seeks, which is unoptimized for slow HDDs. Source file is not yet an immutable snapshot; a same-length concurrent mutation cannot be excluded. No mdview renderer integration or NuGet release is claimed.
- **Compilation and new test execution pending** (no local .NET SDK). Next verify solution tests, then optimize construction to a single sequential indexing pass and design source immutability rules before formatting untrusted pages.

## 2026-11-07 — batched UTF-16 index construction

- Added internal `Utf8DiskLineIndex.ReadLineStartsAsync` for one bounded read of up to 129 consecutive 64-bit line offsets (128 lines plus sentinel).
- Reworked `Utf8DocumentOffsetIndex.CreateAsync` to compute content UTF-8 byte lengths and newline widths from each decoded bounded page and its batch offsets. The BOM is checked once; there are no longer two repeated `GetByteOffsetAsync` source/index lookups for every line.
- Commits: `4abd07692df0ee7d3676b4ab83c376958b382b62` (batch API), `a20d5faace219ef4720ff1155c4ea2a62a44564d` (producer), `7e0cd3790f776d6258aa638e366d1c3aacedbd7f` and `378bc31aa7b75b34e2290248185a9d21bf6a4dfe` (regressions).
- The full reader still seeks per source line while creating pages; this is **not a completed single-sequential-pass HDD optimization**. Limits, source identity caveats and existing API semantics are unchanged. No mdview integration.
- **CI/build pending** for this revision, because .NET SDK is unavailable in this environment. Next: run TextFiles and full Toolbox tests, then optimize physical-line page IO and immutable snapshot policy.


## 2026-11-07 — contiguous physical-line page reads

- Reworked `Utf8DiskLineIndex.ReadPageAsync` to select the largest admissible page from one on-disk offset-table read, then perform **one contiguous source seek/read** for that selected page. It decodes each bounded slice with strict UTF-8 and preserves BOM, CRLF/LF/CR, unsafe-control checks, random page access and `NextLine`.
- Source commit `133f28a2cd8fdb7d51d974ff935e930eff83a464`; regression commit `16bf7eca7554b6a0e1f8f56dc32988503135f089` (261-line Unicode/BOM/terminator boundaries and 100 long lines crossing a 4 MiB page budget).
- The maximum page stays 128 lines / 4 MiB and the existing per-line cap still applies. Only the source-page read pattern changed; no public API, Markdown renderer, snapshot integrity guarantee or NuGet publication changed.
- **Fresh .NET/CI verification not yet confirmed** for this increment. Run TextFiles tests and `dotnet test Toolbox.sln` and record actual results. Do not infer throughput gains without benchmarking on representative HDD/SSD.
- Next: consider combining initial validation/indexing with metadata generation and create a stable source snapshot policy before enabling formatted streaming markdown pages.


## 2026-11-07 — validated private UTF-8 snapshot foundation

- Added `Utf8ValidatedSnapshot.CreateAsync` as an opt-in, read-only source isolation layer. A 64 KiB buffer copies source bytes sequentially to a new private temporary directory under an explicit default 16 GiB byte budget. The **completed copy** is then validated using the existing strict `Utf8DiskLineIndex.OpenAsync`, before it is exposed.
- The snapshot owns its physical-line index and read-only copy guard; disposal removes temporary storage. Callers may create `Utf8DocumentOffsetIndex` on `snapshot.Lines`, disposing that derived index first. The existing `mdview` renderer remains unchanged.
- Precise boundary: future changes to the original file do not change the copied bytes, but concurrent modification **during** copying could yield a mixed (independently validated) copy. This is not a cryptographically authenticated instant-in-time snapshot, a same-identity adversarial tamper defense, or full Linux mandatory locking.
- Production commit `4957d3728ad303e2338ef8707c47b52cd711ff22`; four new regression tests in `5e69419c92c74f02618130d15a95c4de1fa7bcb3`. New snapshot and test compilation **not yet verified by CI**; run TextFiles focused and full Toolbox tests.
- Next: validate snapshot lifecycle on Windows and Debian (including file sharing and cleanup), then integrate a snapshot-backed opt-in formatted pager only after the remaining Markdown source-span tests pass.
