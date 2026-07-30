# Implementation status

Last updated: 2026-07-30

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the WhenItFails JSON catalog workspace under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **5,029 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

## Runtime/public-API verification

Recently verified focused contracts:

- `ErrorCatalogContextProviderNullResponseTaskResultTests`: **5 green**.
- `ErrorCatalogContextProviderFailureFallbackTests`: **7 green**.
- `ErrorCatalogContextProviderCategorySourceMessageFallbackTests`: **2 green**.
- `ErrorCatalogContextProviderCodeGroupSourceEnvelopeTests`: **1 green**.
- `ErrorCatalogContextProviderOwnerSourceEnvelopeTests`: **1 green**.
- `ErrorCatalogContextProviderProfileSourceEnvelopeTests`: **1 green**.
- `ErrorCatalogContextProviderWhitespaceSourceMessageFallbackTests`: **1 green**.
- `ErrorCatalogContextProviderWhitespaceSourceIssueCodeFallbackTests`: **3 green**.
- `ErrorCatalogContextProviderFailureIssueSeverityTests`: **1 user-verified green** for severity normalization and source issue immutability.
- Current source-only-field slice adds a second test to `ErrorCatalogContextProviderFailureIssueSeverityTests`; the expected count is **2 tests**.

The audit protects provider ordering and configured paths, short-circuiting, cancellation, exception identity and shape, null tasks, null responses, null payloads, cross-validation envelopes, provider-specific fallbacks, source-message and source-code selection, malformed envelopes, first-issue selection, later-issue suppression, synthesized issue severity, and source object immutability.

The current response-shape contract requires the synthesized failure issue to preserve only the selected code and response-level message, normalize severity to `Error`, and suppress source-only `Number`, `Details`, and `Metadata`. The source `IssueInfo` and its metadata bag must remain unchanged.

Do not invent automatic `DefaultMappings` consumption.

## Focused verification

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ErrorCatalogContextProviderFailureIssueSeverityTests
```

Expected result: **2 green tests**.

Primary Setter verification command:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Before committing catalog changes, also run:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
git diff --check
```

## Documentation status

High-value Setter documentation synchronization is complete. Maintain `README.md`, `Readme/en.md`, and the existing English documents under `Docs/<topic>/en.md` in line with actual behavior.

Next documentation target: keep this file synchronized while the runtime/public-API audit continues; no separate documentation expansion is currently required.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, backup-retention cleanup, complete localization lifecycle management, remote catalog synchronization, package publishing, a GUI or interactive TUI, complete source-code dependency discovery, or a full security audit and semantic duplicate detector.

## Working rules

- GitHub `master` is the source of truth.
- Keep each change narrow and intentional.
- Add or update tests immediately.
- Do not advance while the current slice is red.
- Update this file after every change.

## Recommended next step

Run `ErrorCatalogContextProviderFailureIssueSeverityTests`; the expected count is **2 green tests**.

If green, record verification and inspect exactly one adjacent synthesized-response shape contract, preferably default response fields not derived from the source issue.

## Last completed change

The eighty-ninth runtime/public-API audit slice extends `ErrorCatalogContextProviderFailureIssueSeverityTests`. A source failure issue carries a number, provider-only details, warning severity, and metadata. `ErrorCatalogContextProvider` must synthesize a distinct error issue containing only the selected source code and response-level message, leaving `Number` and `Details` null and output metadata empty while preserving the complete source issue and its original metadata bag.

Commit:

```text
f515195185c905461e45bfaae1e0795b737ba444
Suppress source-only failure issue fields
```
