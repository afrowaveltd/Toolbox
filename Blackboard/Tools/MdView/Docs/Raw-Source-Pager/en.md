# mdview raw UTF-8 source pager

**Status:** first interactive, disk-indexed source reader, 2026-11-04. This is a separate **raw source** mode; the normal Markdown display is unchanged.

## Run

```bash
dotnet run --project Blackboard/Tools/MdView -- source README.md
```

The explicit `source PATH` verb opens a local UTF-8 or UTF-8 BOM text file and builds a disk-backed index of every physical line before displaying anything. Unlike `mdview local README.md`, which uses the complete Markdown model and optional TextMate highlighting, this mode prints literal Markdown source lines with one-based numbers. It does not parse headings, lists, links or fenced code.

An interactive Spectre.Console prompt offers **Next page**, **Previous page**, **Go to line** (1-based physical line number) and **Exit**. Escape exits. Previous-page navigation uses a stack of actual page starts, so it works even when a byte budget makes a page shorter than 24 lines. A direct jump clears navigation history. The terminal is cleared before displaying each page; the underlying file remains unmodified.

## Limits and safety

Each page requests up to **24 physical lines** by default and may return fewer when the **4 MiB source-page byte budget** is reached. Lines have a default **1 MiB** individual limit; the temporary disk index has a default **256 MiB** budget. No content is cut off without notice: oversized lines and indexes fail explicitly. The entire source is strictly UTF-8 validated in one sequential scan before display, rejecting malformed encodings and unsafe control sequences anywhere in the file, even beyond the first page.

The index is **temporary disk metadata** and automatically removed when the reader closes; it is not the persistent network download cache. For BOM-marked UTF-16/32 files, use normal `mdview local` until a dedicated source-indexing/stream-transcoding implementation is added.

Do not use `source` on redirected/noninteractive stdin: it requires a real keyboard and terminal. It is not a replacement for `less`, a CommonMark streaming renderer or a visual-page navigator. Long lines may wrap in the terminal, but the index still counts physical source lines.

## Test commands

```bash
dotnet test Blackboard/TextFiles.Tests/Afrowave.Toolbox.Blackboard.TextFiles.Tests.csproj
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
```

Tests use simulated Spectre.Console keyboards for next/back/jump/Escape, literal control-safe display, read-only behavior and refusal of invalid source. No .NET SDK was available to run new tests in the current execution environment; this milestone requires Windows and Debian verification before declaring green.

[Disk-backed index design](../../../TextFiles/Docs/Disk-Line-Index/en.md) · [mdview README](../../README.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md).
