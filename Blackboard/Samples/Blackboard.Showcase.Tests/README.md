# Blackboard.Showcase.Tests

Tests for the shared Blackboard interactive showcase application and its read-only preview helpers.

Run independently:

```bash
dotnet test Blackboard/Samples/Blackboard.Showcase.Tests/Blackboard.Showcase.Tests.csproj
```

- `TextAndJsonPreviewTests` cover UTF-8 Czech characters, UTF-16 BOM, binary NUL, invalid UTF-8, escape controls, bounded/truncated reads, empty files, valid/invalid JSON, over-limit JSON and unsafe characters.
- `ShowcaseInteractionTests` use `Spectre.Console.Testing.TestConsole` to exercise the extensible demo host, menu Exit/dispatch, JSON filename filter, valid JSON syntax display and refusal of malformed JSON.

The tests operate on temporary local files and do **not** require GUI, network, FTP or a real terminal. Actual Windows/Debian/SSH keyboard and font behavior needs manual validation.

See [Showcase README](../Blackboard.Showcase/README.md) and [Implementation Status](IMPLEMENTATION_STATUS.md).

## Markdown gallery interaction tests

`MarkdownThemeDemoTests` exercise theme navigation, Classic/Ocean/Forest/Monochrome previews, compare-all, returning to the Showcase host and cancellation. The sample uses an embedded source: no file writes, network calls or host settings are needed. Runs in Windows UI CI and whole-solution Linux CI.

## Complete Markdown file viewer regression coverage (2026-11-02)

`MarkdownFilePreviewDemoTests` exercises actual FileBrowser selection of `.md`/`.markdown`, nested Markdown with safe C# syntax, Unicode BOM, invalid/binary/control rejection, uppercase extension, cancelled/empty file and full display beyond 64 KiB (including the document tail). The reader tests additionally verify 512 KiB+ complete source, UTF-8 multibyte data across read buffers and rejection of corrupt data far beyond the former limit. All file operations are read-only and temporary test fixtures are removed.
