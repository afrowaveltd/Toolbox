# Implementation status

Last updated: 2026-09-15

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite baseline: **1034/1034 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies null `ErrorCatalogDocument.Errors`, `IncludeOwners`, `IncludeCodeGroups`, `IncludeCategories`, `IncludeSubcategories`, `IncludeTags`, `ExcludeTags`, and `IncludeErrors` as malformed input rather than resolver failure.
- A focused malformed-profile contract for null `ErrorProfileDefinition.ExcludeErrors` is committed and awaits local RED verification.

## 2026-09-15 — profile selection null exclude-errors contract

Contract commit:
`66662fea33e04940e197b2f5550921cfe333b6a1`

Baseline checkpoint commit:
`47ac36a2c8b4973862917891847244ce7066d036`

Added:

`WhenItFails.Tests/Resolution/ErrorProfileSelectionServiceNullExcludeErrorsCollectionContractTests.cs`

Contract:

`ResolveByProfileName_WhenProfileExcludeErrorsCollectionIsNull_ReturnsInvalidResponse`

`ErrorProfileCatalogValidator` and `ErrorCatalogCrossValidator` already define the stable malformed-profile contract:

```text
Status: Invalid
Code: ProfileExcludeErrorsCollectionIsNull
Message: Profile exclude errors collection is null.
```

Production is intentionally unchanged before the focused run. Current `ErrorProfileResolver.Resolve(...)` consumes `profile.ExcludeErrors`, so a null collection is expected to throw and then be normalized by `ErrorProfileSelectionService` as `Failed / WIF_PROFILE_RESOLVER_FAILED`.

Expected eventual complete-suite count after this contract passes: **1035/1035 GREEN with zero compiler warnings**.

## 2026-09-15 — 1034/1034 GREEN null include-errors checkpoint

Malformed-profile contract commit:
`e21f859aab8154769d088d434fb28ae932529b73`

Production guard commit:
`375a3b2a495c989a9157f12a906decfe45f34746`

Checkpoint commit:
`47ac36a2c8b4973862917891847244ce7066d036`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1034
Skipped:  0
Total:  1034
Compiler warnings: 0
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` rejects a null `ErrorProfileDefinition.IncludeErrors` collection before invoking the resolver and reuses the established validator/cross-validator contract:

```text
Status: Invalid
Data: null
Code: ProfileIncludeErrorsCollectionIsNull
Message: Profile include errors collection is null.
```

## Established transparent lower boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` intentionally preserves exceptions and null-task behavior from its five internal catalog providers.

Relevant suites include:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderOwnerNullTaskTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProfileNullTaskTests.cs`

Do not replace those transparent contracts with normalization at that layer.

## Established documented behavior — preserve

`JsonCatalogDocumentLoader.InvalidJson` deliberately includes the JSON parser message. `WhenItFails/Docs/Loading-and-Normalization/en.md` documents this behavior; do not sanitize it as incidental hardening.

## Recent verified checkpoints

- 1031/1031 — profile selection null include-subcategories classification complete.
- 1032/1032 — profile selection null include-tags classification complete.
- 1033/1033 — profile selection null exclude-tags classification complete.
- 1034/1034 — profile selection null include-errors classification complete.

## Recommended verification

Pull current `master` and run only the focused contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveByProfileName_WhenProfileExcludeErrorsCollectionIsNull_ReturnsInvalidResponse"
```

Expected current result: **RED** with `Actual: Failed` rather than the expected `Invalid`.

## Next recommended step

If RED confirms the response-shape mismatch, add the smallest guard for `profile.ExcludeErrors is null` after the existing profile collection guards and before the resolver call. Reuse exactly:

```text
ProfileExcludeErrorsCollectionIsNull
Profile exclude errors collection is null.
```

Then run focused and complete suites. Record **1035/1035 GREEN**; this closes the current malformed resolver-consumed profile collection series.
