# Markdown.Cache.Tests — implementation status

## 2026-11-03

Created six xUnit scenarios for disk persistence across instances, replacement rejection for unsafe control characters and invalid UTF-8, blob tampering, size limits and cancellation. Registered test project in `Toolbox.sln`. Tests are not yet executed in this environment: .NET SDK unavailable; Linux/Windows CI verification pending. Next: run CI and add interruption/fault-injection and multiprocess coverage.

## 2026-11-03 — hardening test additions

Added four regression cases for independent concurrent cache instances, corrupted digest blob repair, Linux symlinked storage guard and default per-user cache location (ten total authored). No local SDK; **tests have not yet run**. Confirm through Linux/Windows CI before accepting this checkpoint as green.

## 2026-11-03 — HTTP fake-handler suite

Added eight tests for initial `200`, ETag/Last-Modified `304`, changed `200`, offline stale fallback, unsafe response protection, missing entry errors and strict URL policy. Total cache tests authored: **18**. Verification in CI pending; local SDK unavailable.

## 2026-11-03 — GitHub preferred README source regressions

Added GitHub fake-handler tests for official raw README endpoint, conditional 304 caching across object lifetimes, mixed-case repository identifiers, offline fallback, rejection of unsafe input, and malformed responses. No SDK in this execution environment; test execution and CI confirmation are pending. Next: verify Linux/Windows builds before progressing to CLI work.
