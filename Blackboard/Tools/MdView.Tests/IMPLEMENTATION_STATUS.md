# mdview CLI tests — implementation status

## 2026-11-03

Created the xUnit `net10.0` test project and command parser regression cases. Project registered in Toolbox.sln. Actual .NET test execution remains unverified due to absent local SDK. Future increments: fake-server dispatch, real complete local file display and Windows/Debian terminal smoke testing.

## 2026-11-03 — user Windows test execution confirmed

- `MdView.Tests`: **10 passed / 10 total, 0 failed, 0 skipped** in the user-provided `dotnet test Toolbox.sln` log. Source and command-grammar test project build succeeded on Windows with the .NET 11 preview SDK targeting net10.0.
- Full CLI end-to-end/manual rendering and Debian CI still require separate checks.

## 2026-11-03 — interactive picker virtual-terminal tests

- Added eleven `Spectre.Console.Testing` cases for `MdViewInteractiveSourcePicker`: Escape at source menu, local Markdown file selection without mutation, FileBrowser cancellation/back, GitHub owner/repo, HTTPS URL, rejecting query-bearing URL, refusing non-interactive consoles, and 4 filename extension cases.
- Added test dependency `Spectre.Console.Testing` 0.57.2. The original MdView command parser test baseline was **10/10** green; with new tests, expected count is **21**. Verification of the new suite is pending in Windows/.NET and CI; no successful run claimed yet.
