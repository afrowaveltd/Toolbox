# mdview — implementation status

## 2026-11-03 — first CLI executable and command grammar

- Created `Blackboard/Tools/MdView` as a standalone `net10.0` executable project (`AssemblyName=mdview`), plus `MdView.Tests`, registered in `Toolbox.sln`.
- Added pure `MdViewCommand.Parse` for interactive/no-args, explicit local path, direct URL, `github:owner/repo`, `git OWNER REPO` and `git OWNER/REPO` forms; verbs ignore case without changing path case.
- Local file handling reuses FileBrowser's strict full-document decoding and validates Markdown file extensions. Remote HTTP/GitHub use previously implemented cache-validated source adapters with explicit stale state. All output delegates to existing MarkdownSpectreRenderer, not a new renderer.
- A simple numbered text menu (Local/URL/GitHub) is implemented, **not** a Spectre FileBrowser or pageable TUI. No browser/file link navigation, styling controls or independent Markdown streaming.
- Added CLI grammar xUnit tests. **Build and test execution not verified** in this session; .NET SDK unavailable in current container. Local UI acceptance on Windows/Debian/SSH is pending.
- Run: `dotnet run --project Blackboard/Tools/MdView -- git afrowaveltd toolbox`.
- Next: verify solution CI and correct any build/test issues; add CLI integration tests, full no-args FileBrowser, and eventually disk-indexed native pager. Do not claim large-document bounded-RAM rendering or NuGet release yet.
