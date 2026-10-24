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
