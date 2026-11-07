# TextFiles.Tests — implementation status

## 2026-11-03

- Created standalone xUnit test project for the extracted text decoder with five regression cases.
- Configured `net10.0` and registered project in `Toolbox.sln`.
- Previously passing Showcase integration tests still exist, but have not been rerun against the new TextFiles assembly.
- **Verification pending** through local Windows/Linux or GitHub CI.

## 2026-11-03 — omitted project detected in Windows solution log

- The user-provided Windows solution run shows the extracted `TextFiles` library successfully builds, but `TextFiles.Tests` **does not appear** among executed test projects. Existing suites are green; the five new reader tests remain unverified.
- Updated `Toolbox.sln` configurations so the library and tests participate under x64/x86 as well as Any CPU. Confirm by rerunning the full solution and seeing the explicit `TextFiles.Tests.dll` 5/5 summary. A direct project test can independently isolate a remaining issue.

## 2026-11-03 — all new tests executed

- Confirmed from the user's full Windows solution log: `Afrowave.Toolbox.Blackboard.TextFiles.Tests.dll` was built and executed, with **5 passed, 0 failed, 0 skipped**. This closes the previously missing test execution evidence gap.
- Solution-wide result in the same run: **6,443/6,443 passed**.

## 2026-11-04 — disk-backed physical-line index regression suite

- Added 15 `Utf8DiskLineIndexTests` for bounded memory/disk indexing and random-access UTF-8 source pages, line ending and Unicode edge cases, invalid data rejection before any page, budget errors, cancellation and invalid arguments. The five previously user-confirmed reader extraction tests are unchanged.
- **Execution not yet confirmed** for these new tests; rerun `dotnet test Blackboard/TextFiles.Tests/Afrowave.Toolbox.Blackboard.TextFiles.Tests.csproj` and the full solution. Do not claim a new green total before a test log.

## 2026-11-07 — byte-offset mapping regressions

- Added four `Utf8DiskLineIndexTests`: BOM and mixed newline/Unicode positions; rejection of invalid/surrogate-splitting UTF-16 columns; empty and final-terminated lines; random offsets over a 64 KiB buffer boundary.
- Feature source: `db31e7af6b7a33ef2f6698820e9f7a62cabb9721`. Test commit: `85d3bccd5a6f203aba22742334e61297ad24a874`.
- New tests and full solution build have **not been independently executed in this environment** (no installed .NET SDK). Run `dotnet test Blackboard/TextFiles.Tests/Afrowave.Toolbox.Blackboard.TextFiles.Tests.csproj` and `dotnet test Toolbox.sln` and record the actual results.

## 2026-11-07 — document-absolute offset regression suite

- Added four new tests for the optional `Utf8DocumentOffsetIndex`: UTF-8 BOM/multibyte Unicode/CRLF interior mapping, empty and terminated lines, surrogate middle/out-of-range rejection, index disk budget and zero-length source.
- Production commit `525266761512b202b545169a210a609f20896ee9`, tests `b9b7fb8bd02a95ecca3947f8f1d8a377675afe82`. New tests have **not been run in this environment**. Validate with `dotnet test Blackboard/TextFiles.Tests/Afrowave.Toolbox.Blackboard.TextFiles.Tests.csproj` and `dotnet test Toolbox.sln`.

## 2026-11-07 — batch construction parity regressions

- Added two tests for document-absolute position indexing: mixed UTF-8 multibyte text/CRLF/LF/CR over 320 lines with sampling across 128-line boundaries; BOM-only source and EOF on an unterminated final line.
- Commits `7e0cd3790f776d6258aa638e366d1c3aacedbd7f`, `378bc31aa7b75b34e2290248185a9d21bf6a4dfe`. These are source-based assertions, not an HDD benchmark.
- Fresh `dotnet test Blackboard/TextFiles.Tests/Afrowave.Toolbox.Blackboard.TextFiles.Tests.csproj` and `dotnet test Toolbox.sln` verification pending.


## 2026-11-07 — contiguous source-page regression coverage

- Added `ContiguousPagesPreserveUnicodeBomAndMixedNewlinesAcrossBoundaries` (261 UTF-8 lines with BOM, Unicode and rotating CRLF/CR/LF across the 128-line boundary, forward and random access).
- Added `ContiguousPageByteLimitNeverDropsTheFirstLineOfNextPage` (100 long lines, >4 MiB source total, exact `NextLine` continuation and reconstruction).
- Tests committed in `16bf7eca7554b6a0e1f8f56dc32988503135f089`; runtime change in `133f28a2cd8fdb7d51d974ff935e930eff83a464`. Execution still pending; no claim of green CI for this snapshot.

