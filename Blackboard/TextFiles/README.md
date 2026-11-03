# Afrowave.Toolbox.Blackboard.TextFiles

Independent .NET 10 library for safe, read-only local text-file decoding. Extracted from the Blackboard.Showcase sample on 2026-11-03 and reused by Showcase and the standalone `mdview` CLI. No dependency on Spectre.Console, the Markdown parser, HTTP, GitHub or an executable sample.

## API

```csharp
using Afrowave.Toolbox.Blackboard.TextFiles;

var shortPreview = await TextFilePreviewReader.ReadAsync("notes.txt", maxPreviewBytes: 4096);
var fullDocument = await TextFilePreviewReader.ReadCompleteAsync("README.md");

if (fullDocument.Kind == TextPreviewKind.Text)
    Console.WriteLine(fullDocument.Content);
```

`ReadAsync` validates a bounded prefix (default 16 KiB, configurable 256 bytes–256 KiB); its `Truncated` property reports whether more content exists. `ReadCompleteAsync` decodes the entire stream in small buffers, then returns one complete string (not a bounded-memory document representation). Both detect strict UTF-8, UTF-8 BOM and UTF-16/UTF-32 BOM endianness; invalid byte sequences or unsafe terminal control characters cause `BinaryOrUnsupportedEncoding` rather than replacement text. No file is modified and no fetched text is interpreted as terminal markup.

The source implementation was **moved, not reimplemented**. Existing Showcase text/JSON/Markdown tests still exercise the same code through the new project dependency. Five additional reader-only regression tests were authored in `TextFiles.Tests`.

```bash
dotnet test Blackboard/TextFiles.Tests/Afrowave.Toolbox.Blackboard.TextFiles.Tests.csproj
dotnet test Toolbox.sln
```

**Verification pending:** the user-provided 6,438/6,438 green Windows run applies to the snapshot **before** this extraction. New tests and consumer dependency changes need another run. Large-file paging, progressive Markdown parse, filesystem race protection and symlink policy are outside this decoder.

[Extraction guide](Docs/Architecture/en.md) · [Implementation status](IMPLEMENTATION_STATUS.md).
