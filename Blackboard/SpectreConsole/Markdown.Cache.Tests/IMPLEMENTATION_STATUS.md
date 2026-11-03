# Markdown.Cache.Tests — implementation status

## 2026-11-03

Created six xUnit scenarios for disk persistence across instances, replacement rejection for unsafe control characters and invalid UTF-8, blob tampering, size limits and cancellation. Registered test project in `Toolbox.sln`. Tests are not yet executed in this environment: .NET SDK unavailable; Linux/Windows CI verification pending. Next: run CI and add interruption/fault-injection and multiprocess coverage.
