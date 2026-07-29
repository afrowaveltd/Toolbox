# Implementation status

Last updated: 2026-07-29

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the WhenItFails JSON catalog workspace under `Jsons/WhenItFails`.

Implemented areas include workspace initialization and validation, catalog inspection and editing, profile management, safe writes and backups, rich/plain/JSON output, documentation-key validation, and local Markdown link checking.

## Verification status

The latest user-verified Setter test run reported **5,029 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

Recent runtime/public-API audit verification:

- `ErrorCatalogContextProviderShortCircuitTests`: **5 green**.
- `ErrorCatalogContextProviderCallOrderTests`: **1 green**.
- `ErrorCatalogContextProviderNullPayloadShortCircuitTests`: **5 green**.
- `ErrorCatalogContextProviderCancellationPropagationTests`: **4 green**.
- `ErrorCatalogContextProviderPostProfileCancellationTests`: **1 green**.
- `ErrorCatalogContextProviderCrossValidationFailureContractTests`: **1 green**.
- `ErrorCatalogContextProviderCrossValidationWarningTests`: **3 green**.
- `ErrorCatalogContextProviderCrossValidationErrorSelectionTests`: **2 green**.
- `ErrorCatalogContextProviderInputOrderingTests`: **9 green**.
- `ErrorCatalogContextProviderProviderExceptionPropagationTests`: **6 green**.
- `ErrorCatalogContextProviderExceptionShapeTests`: **8 green**.
- `ErrorCatalogContextProviderOwnerNullTaskTests`: **1 green**.
- `ErrorCatalogContextProviderProfileNullTaskTests`: **1 green**.
- `ErrorCatalogContextProviderNullResponseTaskResultTests`: **5 user-verified green**, completing null-response-result coverage across all five provider boundaries.
- `ErrorCatalogContextProviderFailureFallbackTests`: **1 user-verified green** for the error-provider fallback contract.
- Current category fallback slice still expects **2 focused tests**. The first local build failed because the test used nonexistent `ResultStatus.Unauthorized`; the test now uses the real `ResultStatus.NotSupported` member and requires re-verification.

Focused verification command for the current slice:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ErrorCatalogContextProviderFailureFallbackTests
```

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

## Documentation synchronization completed

High-value Setter documentation synchronization is complete. Maintained English documentation includes:

- `README.md` and `Readme/en.md`;
- `Docs/Overview/en.md`;
- `Docs/Commands/en.md` and `Docs/Command Quick Reference/en.md`;
- `Docs/Getting-Started/en.md`;
- `Docs/FAQ/en.md`;
- `Docs/Known Limitations/en.md`;
- `Docs/Roadmap and Future Work/en.md`;
- `Docs/Testing and CI/en.md`;
- `Docs/Reviewing Catalog Changes/en.md`;
- `Docs/Safe Writes/en.md` and `Docs/Backups and Recovery/en.md`;
- `Docs/Architecture Overview/en.md`;
- `Docs/Contributing to Setter/en.md`;
- `Docs/Maintainer Notes/en.md`.

## Runtime/public-API audit

Completed contracts now protect:

1. normalized profile lookup, selection, metadata, mappings, and independent-copy semantics;
2. provider output aggregation, reference behavior, exact order, and configured path routing;
3. short-circuit behavior for provider failures and null payloads at every boundary;
4. cancellation before, between, and after provider calls;
5. cross-validation success, warnings, information diagnostics, error selection, and failure envelopes;
6. constructor and method-entry validation ordering;
7. transparent propagation of synchronous exceptions, faulted tasks, canceled tasks, custom exception state, and exact exception identity;
8. null `Task` behavior at every provider boundary;
9. completed provider tasks yielding a null `Response<T>` at every provider boundary;
10. failure responses with no issues and no message use provider-specific fallback details while preserving the original status and short-circuiting later providers;
11. the current category fallback contract preserves `ResultStatus.NotSupported`, synthesizes `ErrorCatalogContextCategoryCatalogLoadFailed` and the documented category message, returns no context, and prevents later provider work.

Do not invent automatic `DefaultMappings` consumption.

## Current intentional boundaries

Setter currently does not provide:

- automatic schema migration;
- a multi-file atomic transaction;
- a multi-process locking contract;
- automatic backup-retention cleanup;
- complete localization lifecycle management;
- remote catalog synchronization;
- package publishing;
- a GUI or interactive TUI;
- complete source-code dependency discovery;
- a full security audit or semantic duplicate detector.

These are boundaries or future candidates, not undocumented defects.

## Working rules

- GitHub `master` is the source of truth.
- Keep each change narrow and intentional.
- Add or update tests immediately with each implementation or documentation change.
- Run the affected focused test project after every commit-sized change.
- Do not advance while the current test slice is red.
- Keep README and `Docs/<topic>/en.md` aligned with actual behavior.
- Update this file after every change.

## Recommended next step

First re-run `ErrorCatalogContextProviderFailureFallbackTests`; the expected count remains **2 green tests** after replacing the invalid enum member.

Next documentation target: keep this file synchronized while the runtime/public-API audit continues; no separate documentation expansion is currently required.

If green, inspect exactly one adjacent fallback boundary, preferably the code-group-provider failure fallback after successful error and category providers.

## Last completed change

The seventy-second runtime/public-API audit slice extends failure fallback coverage to the category-provider boundary. After a valid error response, a category provider returns `ResultStatus.NotSupported` with no issues and no message. `ErrorCatalogContextProvider` must preserve `NotSupported`, synthesize `ErrorCatalogContextCategoryCatalogLoadFailed` and the documented category fallback message, return no context, and invoke no code-group, owner, or profile provider. The initial test revision incorrectly referenced nonexistent `ResultStatus.Unauthorized`; commit `062fd7ded1c18e3dee7621f2e9388868a842d4ec` corrects the test to the real enum member.

Commits in this change sequence:

```text
29374f42b564a53a5c846cf55f0104fa6b495ba7
Protect category failure fallback details

062fd7ded1c18e3dee7621f2e9388868a842d4ec
Fix category fallback test status
```
