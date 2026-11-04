# Afrowave.Toolbox.Blackboard.TextFiles.Tests

Focused unit tests for the independent safe local text reader, in addition to the established Showcase integration tests. Coverage: complete UTF-8 Unicode, bounded prefix read, late unsafe control rejection, UTF-16 BOM and malformed UTF-8.

```bash
dotnet test Blackboard/TextFiles.Tests/Afrowave.Toolbox.Blackboard.TextFiles.Tests.csproj
```

The original five extracted-reader tests were confirmed **5/5 passed** in the user's Windows solution run. The new index tests described below **have not yet been executed**.

[TextFiles library](../TextFiles/README.md).

## Indexed UTF-8 physical-line reader regressions (2026-11-04)

Added `Utf8DiskLineIndexTests` for random-access forward/backward pages, mixed CRLF/LF/CR and buffer boundaries, BOM and Unicode, no trailing phantom line, empty files, malformed/unsafe data beyond the initial buffer, UTF-16 rejection, bounded per-line/page/index budgets, cancellation and invalid arguments. These cases verify the **source index** only, not formatted Markdown rendering. New tests await a Windows/Linux run; the earlier five reader tests passed in the 6,443/6,443 Windows baseline.
