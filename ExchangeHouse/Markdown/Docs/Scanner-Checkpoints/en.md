# Incremental Markdown scanner checkpoints — format foundation

**Status (2026-11-08):** versioned, bounded, serializable **state contract**
and validation only. Scanner export and restore are **not yet implemented**,
and `mdview preview` still uses its disk-backed classified-event journal.

## Purpose

Fast seeking into a large Markdown document must not start parsing in
the middle of a code fence or lose ownership of a nested list/quote.
A file byte offset alone is **not** a sufficient parser checkpoint.
The scanner maintains ordinary-list and quoted-list stacks independently,
pending blank-line state, active quote depth/outer ownership and active
fenced-code delimiter information.

`MarkdownScannerCheckpoint` makes the required pieces explicit:

- Schema `Version`; next physical source line and document-absolute
  decoded UTF-16 offset, excluding the initial UTF-8 BOM.
- Ordinary and quote-local list-frame arrays, each retaining marker
  indentation, content indentation, document-start offset and marker text.
- Current quote depth and outer ordinary-list owner plus both pending
  blank separator flags.
- Optional fence state: delimiter character and length; quoted owner
  depth; opening offset; language; list depth/owner and content indent;
  outer ordinary-list owner/depth.

`MarkdownScannerCheckpointCodec.Encode` and `Decode` use JSON via
`System.Text.Json`, with strict validation. The current schema version
is 1. A checkpoint is limited to **32 KiB**, each list stack to
**32 frames**, and other offset, marker, indentation and fence fields
are validated before a decoded state may be used. Unknown versions and
inconsistent owner pointers are rejected instead of silently adjusted.

## What has not been implemented

- No scanner API exports the active iterator state or starts scanning
  from a supplied checkpoint. A checkpoint made by arbitrary user code
  may be structurally valid without corresponding to real source syntax.
- No source identity, immutable-snapshot digest or byte-range binding
  is encoded yet. Do **not** accept an external checkpoint as trusted or
  use it to bypass source validation.
- A small format does not guarantee correctness for all future parser
  features. The initial codec deliberately rejects unsupported extremes
  such as very long fence delimiters.
- A future resume API will need exact UTF-16-to-byte source mapping,
  validated snapshot ownership, deterministic output parity with a
  fresh scanner and tests at boundaries (CRLF, BOM, nested quote/list,
  interrupted fences). `mdview` will remain on the event journal until
  those are proven.

## Focused verification

```bash
dotnet test ExchangeHouse/Markdown.Tests/Afrowave.Toolbox.ExchangeHouse.Markdown.Tests.csproj
dotnet test Toolbox.sln
```

Five new regression tests cover complete roundtrip, empty state, invalid
version/owner, fence/nesting bounds and malformed/oversized JSON. Execution
of these newly committed tests has not yet been confirmed.

[Markdown README](../../README.md) ·
[Implementation status](../../IMPLEMENTATION_STATUS.md) ·
[mdview preview](../../../../Blackboard/Tools/MdView/Docs/Experimental-Formatted-Preview/en.md).
