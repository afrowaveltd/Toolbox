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
[mdview preview](../../../Blackboard/Tools/MdView/Docs/Experimental-Formatted-Preview/en.md).

## Explicit scanner state machine (2026-11-08)

The container scanner now exposes
`MarkdownIncrementalContainerScanner.StateMachine`, which performs exactly
one conservative classification for each **already decoded, consecutive**
`MarkdownStreamLine`. The existing public `ScanAsync(TextReader)` entry
continues to own the physical-line scanner, but delegates classification
to the same state machine; it does not require callers to use checkpoints.

```csharp
var state = new MarkdownIncrementalContainerScanner.StateMachine();

await foreach (var line in MarkdownIncrementalLineScanner.ScanAsync(reader))
{
    var classified = state.Process(line);
    // Render or journal only after the entire source has been validated.
    var saved = MarkdownScannerCheckpointCodec.Encode(state.Capture());
    state = MarkdownIncrementalContainerScanner.StateMachine.Restore(
        MarkdownScannerCheckpointCodec.Decode(saved));
}
```

`Capture()` materializes independent arrays for both list stacks, current
quote and pending-blank flags, and any open fence. `Restore()` validates
the checkpoint and reconstitutes all frames, including the ordinary-list
object referenced by an active quoted fence. An unresolved outer fence
owner is rejected rather than guessed. `Process()` rejects out-of-order
line numbers and UTF-16 offsets, and advances the cursor by the exact
decoded text-plus-terminator length.

The first golden parity suite restores the state **before every physical
line** for several mixed CR/LF/CRLF documents with lazy/nested lists, quote
fences, interrupted fences and ordinary headings. The output must exactly
equal the unchanged `ScanAsync` classification. Further tests exercise
an active quoted fence and malformed restoration.

**Still missing:** exporting an arbitrary running `ScanAsync` iterator
is not necessary because classification is now explicitly stateful, but
restoring the **physical line reader** at an offset is not implemented.
Consumers must provide lines from the same validated immutable snapshot
with correct line numbers and decoded UTF-16 offsets. A structurally
valid checkpoint, on its own, is not evidence that it matches that
snapshot. The `mdview` integration remains on its proven disk journal
until source identity, byte seeking, checkpoint persistence and parity
tests have been added.

**Compatibility:** the original `ScanAsync` signature and source output
are intended to remain identical. This is a testable refactoring, not a
claim of a completed random-access visual pager. New focused tests and a
fresh full solution test run must be verified separately.
