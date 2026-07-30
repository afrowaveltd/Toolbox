# Implementation status

Last updated: 2026-07-30

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
- `ErrorCatalogContextProviderFailureFallbackTests`: **7 user-verified green**, covering empty provider failures across all five boundaries, source-message preservation, and source-code preservation with a fallback message.
- `ErrorCatalogContextProviderCategorySourceMessageFallbackTests`: **1 user-verified green** for category source-message preservation without source issues.
- Current full category source-envelope slice adds a second focused test to `ErrorCatalogContextProviderCategorySourceMessageFallbackTests`; the next successful run is expected to report **2 tests**.

Focused verification command for the current slice:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ErrorCatalogContextProviderCategorySourceMessageFallbackTests
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
11. error fallback preserves `ResultStatus.NotFound` and synthesizes `ErrorCatalogContextErrorCatalogLoadFailed` plus its documented message;
12. category fallback preserves `ResultStatus.NotSupported` and synthesizes `ErrorCatalogContextCategoryCatalogLoadFailed` plus its documented message;
13. code-group fallback preserves `ResultStatus.Cancelled` and synthesizes `ErrorCatalogContextCodeGroupCatalogLoadFailed` plus its documented message;
14. owner fallback preserves `ResultStatus.Failed` and synthesizes `ErrorCatalogContextOwnerCatalogLoadFailed` plus its documented message;
15. profile fallback preserves `ResultStatus.Invalid` and synthesizes `ErrorCatalogContextProfileCatalogLoadFailed` plus its documented message;
16. a provider's non-empty message is preserved in both the outer response and synthesized issue while only the provider-specific fallback code is added when no source issues exist;
17. the first source issue code is preserved while the provider-specific fallback message is supplied when the source response message is empty;
18. category source-message preservation keeps the provider's message in both the outer response and synthesized issue while adding only the category fallback code;
19. the current full category source-envelope contract preserves `NotSupported`, the first source issue code, and the source response message, emits exactly one issue, discards later source issues, returns no context, and prevents later provider execution.

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

First verify `ErrorCatalogContextProviderCategorySourceMessageFallbackTests`; the expected count is **2 green tests**.

Next documentation target: keep this file synchronized while the runtime/public-API audit continues; no separate documentation expansion is currently required.

If green, inspect exactly one adjacent full-source failure-envelope boundary, preferably code-group after successful error and category providers.

## Last completed change

The seventy-ninth runtime/public-API audit slice protects the complete category failure-envelope transformation. After a valid error-catalog response, the category provider returns `ResultStatus.NotSupported`, a non-empty response message, and two source issues. `ErrorCatalogContextProvider` must preserve `NotSupported`, preserve the response message in both the outer response and synthesized issue, use only the first source issue code, discard later source issues, return no context, and invoke no code-group, owner, or profile provider.

Commit in this change sequence:

```text
6956927bcccfdb826b8cb33c8d7299454aa6881b
Protect full category failure source envelope
```
