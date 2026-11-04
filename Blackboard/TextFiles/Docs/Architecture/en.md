# Shared local text reader — extraction and consumers

## Rationale

The complete local Unicode reader was originally implemented inside the executable `Blackboard.Showcase`. The first `mdview` implementation temporarily linked its source file with an MSBuild `Compile Include`, because the executable sample was not an appropriate reusable dependency. Keeping this duplicate compilation would make reuse and API evolution fragile.

The implementation is now owned by `Blackboard/TextFiles/Afrowave.Toolbox.Blackboard.TextFiles.csproj`, under namespace `Afrowave.Toolbox.Blackboard.TextFiles`. The original Showcase source file was deleted. `Blackboard.Showcase` and `MdView` both reference the reusable library through normal `ProjectReference` entries. Existing Showcase text, JSON and Markdown preview code imports the new namespace. Showcase's JSON renderer remains separate; this library never parses JSON or Markdown.

## Public types

- `TextPreviewKind`: `Text`, `Empty`, or `BinaryOrUnsupportedEncoding`.
- `TextPreviewResult`: `Kind`, `Content`, `EncodingName`, `Truncated`, and `TotalBytes`.
- `TextFilePreviewReader.ReadAsync`: validated bounded preview, default 16 KiB.
- `TextFilePreviewReader.ReadCompleteAsync`: fully validated local document, decoded incrementally but returned as one string.

The implementation preserves BOM/encoding detection, strict decoder errors, terminal-control rejection and cancellation behavior. The `TextFiles` library intentionally does not depend on Spectre.Console, cache adapters, source pickers or markdown parser.

## Test and release criteria

`TextFiles.Tests` provides standalone regression cases for unicode, limits, an unsafe sequence beyond the prefix, UTF-16 BOM and malformed UTF-8. Existing Showcase coverage remains in its own test assembly. Since public type namespaces changed from `Afrowave.Toolbox.Blackboard.Showcase` to `Afrowave.Toolbox.Blackboard.TextFiles`, future external Showcase consumers must update imports; Showcase is an unpublished executable sample, not a stable exported API.

Verification of this change is **pending** a full `dotnet test Toolbox.sln` on Windows and CI/Linux. The previous 6,438-test success predates this extraction. A future pager should consume a separate block-level streaming interface, not materialize gigabytes through `ReadCompleteAsync`.

[README](../../README.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md).
