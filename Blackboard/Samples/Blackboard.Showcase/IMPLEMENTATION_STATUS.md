# Blackboard.Showcase — Implementation Status

## 2026-10-24 — first shared interactive test application

- Created independently executable .NET 10 project at `Blackboard/Samples/Blackboard.Showcase`; not a NuGet library. Added common `IBlackboardShowcaseDemo` contract and `BlackboardShowcaseHost` with interactive selection menu and deterministic registered modules.
- Implemented demos for FileBrowser local file selection + bounded readable text preview, JSON-only filtered selection + syntax-highlighted `Spectre.Console.Json.JsonText` render, and directory selection.
- Added `TextFilePreviewReader` with conservative strict UTF-8/UTF-16/UTF-32, no binary/control characters or unbounded memory reads (16 KiB default). Added `JsonFilePreviewReader` with at most 64 KiB complete valid JSON validated via `System.Text.Json` before passing to the Spectre renderer.
- `Spectre.Console.Json 0.57.2` is referenced **only by the Showcase executable**, never by the independent FileBrowser NuGet. The Showcase uses generic filter `*.json`, demonstrating how other applications can supply independent mask/regex filters.
- Added sibling `Blackboard.Showcase.Tests` project and registered both projects in `Toolbox.sln`. Tests cover safe previews, binary/invalid/oversized cases, sample module dispatch and virtual-terminal JSON filtering and rendering.
- Documentation: [README](README.md), [Showcase](Docs/Showcase/en.md), and per-test-project README/status.

**Current verification:** GitHub CI run for the solution-project registration must be checked; do not assert green until an actual completed workflow confirms it. Manual Debian/Windows/SSH terminal smoke testing remains pending. No published NuGet or FTP connector was added.

### Next steps

1. Confirm green GitHub Actions for both new projects and fix compile/test issues if detected.
2. Manually exercise menu, filename selection and JSON rendering on Debian and Windows consoles/SSH.
3. Add new Blackboard widget modules here (do not couple them into FileBrowser).
