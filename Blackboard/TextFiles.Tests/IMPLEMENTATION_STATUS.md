# TextFiles.Tests — implementation status

## 2026-11-03

- Created standalone xUnit test project for the extracted text decoder with five regression cases.
- Configured `net10.0` and registered project in `Toolbox.sln`.
- Previously passing Showcase integration tests still exist, but have not been rerun against the new TextFiles assembly.
- **Verification pending** through local Windows/Linux or GitHub CI.

## 2026-11-03 — omitted project detected in Windows solution log

- The user-provided Windows solution run shows the extracted `TextFiles` library successfully builds, but `TextFiles.Tests` **does not appear** among executed test projects. Existing suites are green; the five new reader tests remain unverified.
- Updated `Toolbox.sln` configurations so the library and tests participate under x64/x86 as well as Any CPU. Confirm by rerunning the full solution and seeing the explicit `TextFiles.Tests.dll` 5/5 summary. A direct project test can independently isolate a remaining issue.
