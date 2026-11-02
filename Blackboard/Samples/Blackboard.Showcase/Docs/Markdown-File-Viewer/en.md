# View complete local Markdown documents

Blackboard.Showcase includes a fifth demonstration, **Markdown — open local README or .md file**, for displaying an existing local `README.md` or any `*.md` / `*.markdown` document using the independent Afrowave Markdown renderer.

## Usage

From the Toolbox repository root:

```bash
dotnet run --project Blackboard/Samples/Blackboard.Showcase
```

Select **Markdown — open local README or .md file**, navigate through the existing FileBrowser, and choose `README.md` or another Markdown file. The picker initially filters `*.md;*.markdown` but allows directory navigation. Selected extensions are checked again because the picker can change filters.

The full Markdown is rendered with the same Classic theme and optional TextMate highlighting as [Markdown Themes](../Markdown-Themes/en.md). The file is never edited, and embedded fenced code is **displayed**, not executed. Escape/cancel returns without opening anything.

## File size: notice, not a restriction

The selected Markdown document is **not cut off at 64 KiB**. Files larger than that show an informational notice but are still decoded, parsed and displayed in full. This matters for lengthy README files and comprehensive application help.

The read path works with an approximately 8 KiB decoding buffer rather than loading all file bytes in one read. It then materializes the resulting complete text as a string and the parser constructs its block/inline model in memory. Thus this is not yet an out-of-core indexed or paginated viewer: memory usage and terminal scrollback scale with document size. Very large documents remain limited by the .NET process and available system resources, not a configurable fixed byte cap. Interactive page navigation and disk-first/streaming Markdown parsing are future considerations.

## Validation before rendering

The whole file is decoded with strict Unicode rules: plain/BOM UTF-8, BOM-marked UTF-16 LE/BE or UTF-32 LE/BE. Decoder errors and unsafe terminal control characters, including ESC and NUL appearing **after many kilobytes**, reject the entire document without displaying an unsafe prefix. No replacement characters are silently introduced for corrupt Unicode.

An empty or BOM-only file reports an empty document. Unsupported legacy code pages are not automatically guessed. Read and permission errors yield a simple diagnostic. Source is passed as literal text to the Spectre renderer, never as markup or a terminal escape sequence.

This verifies *safe text input*, not full Markdown grammar compliance: Markdown syntax is deliberately tolerant and the current ExchangeHouse parser supports a documented bounded subset of CommonMark. Unknown constructs can still be displayed as literal text.

## Boundaries and dependency separation

- The viewer reuses `FileBrowserLocalSource`, `SpectreFileBrowser`, `TextFilePreviewReader.ReadCompleteAsync`, `MarkdownParser` and `MarkdownSpectreRenderer`.
- No network, HTML/webview, source modification, code execution or new reusable package dependency is introduced.
- The other **FileBrowser text preview** intentionally retains its small 16 KiB prefix limit.
- The separate JSON viewer still requires a complete, validated document under its own 64 KiB limit; this change only relaxes Markdown help display.
- Virtual-terminal tests verify actual file selection and rejection; manual Windows/Debian testing of this *new viewer* is still required.

[Showcase README](../../README.md) ·
[Implementation status](../../IMPLEMENTATION_STATUS.md) ·
[Showcase test coverage](../../../Blackboard.Showcase.Tests/README.md).
