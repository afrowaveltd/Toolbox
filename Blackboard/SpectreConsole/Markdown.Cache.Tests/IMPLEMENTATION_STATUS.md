# Markdown.Cache.Tests — implementation status

## 2026-11-03

Created six xUnit scenarios for disk persistence across instances, replacement rejection for unsafe control characters and invalid UTF-8, blob tampering, size limits and cancellation. Registered test project in `Toolbox.sln`. Tests are not yet executed in this environment: .NET SDK unavailable; Linux/Windows CI verification pending. Next: run CI and add interruption/fault-injection and multiprocess coverage.

## 2026-11-03 — hardening test additions

Added four regression cases for independent concurrent cache instances, corrupted digest blob repair, Linux symlinked storage guard and default per-user cache location (ten total authored). No local SDK; **tests have not yet run**. Confirm through Linux/Windows CI before accepting this checkpoint as green.

## 2026-11-03 — HTTP fake-handler suite

Added eight tests for initial `200`, ETag/Last-Modified `304`, changed `200`, offline stale fallback, unsafe response protection, missing entry errors and strict URL policy. Total cache tests authored: **18**. Verification in CI pending; local SDK unavailable.

## 2026-11-03 — GitHub preferred README source regressions

Added GitHub fake-handler tests for official raw README endpoint, conditional 304 caching across object lifetimes, mixed-case repository identifiers, offline fallback, rejection of unsafe input, and malformed responses. No SDK in this execution environment; test execution and CI confirmation are pending. Next: verify Linux/Windows builds before progressing to CLI work.

## 2026-11-04 — GitHub public repository catalog tests

- Added fake-HTTP `GitHubRepositoryCatalogSourceTests` for account type discovery (user/organization), public-only listings, pagination, invalid/duplicate/private filtering, case-insensitive 30-minute cache, deterministic offline stale fallback, 404 and malformed JSON errors, invalid owner strings and the 10-page cap. No live GitHub calls.
- **New tests unverified**; the earlier 32/32 user-run Windows cache result predates these changes.

## 2026-11-04 — Windows regression log, pagination fixture correction

- User's full Windows `dotnet test Toolbox.sln` run compiled the new GitHub repository catalog and all other projects. `Markdown.Cache.Tests`: **43 passed, 1 failed** (out of 44); all other suites were green. The failing test was `GitHubRepositoryCatalogSourceTests.MultiplePagesAreCombinedAndPrivateOrUnsafeEntriesAreIgnored`, expecting 3 HTTP calls but counting 11.
- Root cause confirmed in the **fake-HTTP test fixture**, not the production catalog: `Query.Contains("page=1")` also matched the `per_page=100` parameter. Consequently the mock returned page 1 for every request and falsely simulated endless `Link: next` pagination up to the real client's ten-page safety limit.
- Committed a minimal test fix: match `page=1` as an exact ampersand-separated query parameter, then assert all three request URLs explicitly (owner lookup, page 1, page 2). Catalog implementation and cache remain unchanged.
- A fresh test run after this commit is **pending**; do not claim 44/44 or full solution green yet. Recommended: `dotnet test Blackboard/SpectreConsole/Markdown.Cache.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache.Tests.csproj` followed by `dotnet test Toolbox.sln`.

## 2026-11-04 — catalog pagination regression resolved

- Maintainer confirmed **all Windows solution tests green** after the test fixture was corrected to distinguish `page=1` from `per_page=100`.
- Previous failure (43/44 Cache.Tests) has been closed; no production code change was needed. The exact per-assembly rerun log was not provided separately.
