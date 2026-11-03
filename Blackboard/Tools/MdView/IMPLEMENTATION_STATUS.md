# mdview — implementation status

## 2026-11-03 — first CLI executable and command grammar

- Created `Blackboard/Tools/MdView` as a standalone `net10.0` executable project (`AssemblyName=mdview`), plus `MdView.Tests`, registered in `Toolbox.sln`.
- Added pure `MdViewCommand.Parse` for interactive/no-args, explicit local path, direct URL, `github:owner/repo`, `git OWNER REPO` and `git OWNER/REPO` forms; verbs ignore case without changing path case.
- Local file handling reuses FileBrowser's strict full-document decoding and validates Markdown file extensions. Remote HTTP/GitHub use previously implemented cache-validated source adapters with explicit stale state. All output delegates to existing MarkdownSpectreRenderer, not a new renderer.
- A simple numbered text menu (Local/URL/GitHub) is implemented, **not** a Spectre FileBrowser or pageable TUI. No browser/file link navigation, styling controls or independent Markdown streaming.
- Added CLI grammar xUnit tests. **Build and test execution not verified** in this session; .NET SDK unavailable in current container. Local UI acceptance on Windows/Debian/SSH is pending.
- Run: `dotnet run --project Blackboard/Tools/MdView -- git afrowaveltd toolbox`.
- Next: verify solution CI and correct any build/test issues; add CLI integration tests, full no-args FileBrowser, and eventually disk-indexed native pager. Do not claim large-document bounded-RAM rendering or NuGet release yet.

## 2026-11-03 — Windows full-suite compilation feedback

- User's Windows .NET 11 preview SDK full solution run reported CS0103 in `MdView/Program.cs`: `TextFilePreviewReader` and `TextPreviewKind` were not in FileBrowser; their implementation lives in `Blackboard.Showcase`.
- Fixed `MdView.csproj` by linking the existing tested `Blackboard.Showcase/TextFilePreviewReader.cs` source into the new CLI compilation; corrected the namespace import to `Afrowave.Toolbox.Blackboard.Showcase`. This avoids a runtime dependency on the Showcase executable and preserves existing strict Unicode/BOM validation. It is a short-term source-sharing bridge: next extract the reader into a UI-neutral shared project with both consumers referencing it.
- User-provided log also confirms **Markdown.Cache.Tests 32/32 passed** on Windows; these tests predate the CLI correction. Re-run `dotnet test Toolbox.sln` to verify the corrected CLI; not yet confirmed locally or in CI.

## 2026-11-03 — Windows verification passed

- User's follow-up full `dotnet test Toolbox.sln` output confirms `MdView` built as `mdview.dll` and `MdView.Tests` passed **10/10**, with no failures or skips. The earlier missing `TextFilePreviewReader`/ `TextPreviewKind` CS0103 errors are resolved by the temporary shared-source link.
- This is a Windows local-test confirmation, not a verified GitHub Actions build or a manual interactive CLI acceptance test. The linked Showcase reader should next be extracted to an independent library rather than retained as cross-project source inclusion.

## 2026-11-03 — reader extraction follow-up

- Removed the temporary linked Showcase source from `MdView.csproj`. `mdview` now references [TextFiles](../../TextFiles/README.md) as a normal library project and uses `Afrowave.Toolbox.Blackboard.TextFiles` namespace.
- Local-file decoding behavior is intentionally unchanged. The 6,438/6,438 user-confirmed Windows test result predates this extraction; full solution tests are pending for the new dependency graph.

## 2026-11-03 — full Windows extraction regression passed

- Windows solution test report after the independent TextFiles migration shows `mdview.dll` built and **MdView.Tests 10/10 passed**, alongside extracted TextFiles.Tests 5/5 and Showcase.Tests 39/39; total solution result **6,443/6,443**.
- Interactive CLI navigation and large-file paging still unverified/unimplemented, respectively.
