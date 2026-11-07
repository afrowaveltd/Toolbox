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
