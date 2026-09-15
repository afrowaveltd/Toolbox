# Implementation status

Last updated: 2026-09-15

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite baseline: **1029/1029 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies null `ErrorCatalogDocument.Errors`, `ErrorProfileDefinition.IncludeOwners`, and `ErrorProfileDefinition.IncludeCodeGroups` as malformed input rather than resolver failure.
- A focused malformed-profile contract for null `ErrorProfileDefinition.IncludeCategories` is committed and awaits local RED verification.

## 2026-09-15 — profile selection null include-categories contract

Contract commit:
`3f46933be0fa4a8b080ec5533860c26d8bbcf03c`

Baseline checkpoint commit:
`7a00d57676286fd7de7644bf056cfbcbd8029fff`

Added:

`WhenItFails.Tests/Resolution/ErrorProfileSelectionServiceNullIncludeCategoriesCollectionContractTests.cs`

Contract:

`ResolveByProfileName_WhenProfileIncludeCategoriesCollectionIsNull_ReturnsInvalidResponse`

`ErrorProfileCatalogValidator` already defines the stable malformed-profile contract:

```text
Status: Invalid
Code: ProfileIncludeCategoriesCollectionIsNull
Message: Profile include categories collection is null.
```

Production is intentionally unchanged before the focused run. Current `ErrorProfileResolver.Resolve(...)` passes `profile.IncludeCategories` to `CreateNormalizedSet(...)`, so a null collection is expected to throw and then be normalized by `ErrorProfileSelectionService` as `Failed / WIF_PROFILE_RESOLVER_FAILED`.

Expected eventual complete-suite count after this contract passes: **1030/1030 GREEN with zero compiler warnings**.

## 2026-09-15 — 1029/1029 GREEN null include-code-groups checkpoint

Malformed-profile contract commit:
`806580ada38916c069b8eb11161999356be6f73c`

Production guard commit:
`e55b359cb4232e383e2087ea9238b237a9175cc5`

Checkpoint commit:
`7a00d57676286fd7de7644bf056cfbcbd8029fff`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1029
Skipped:  0
Total:  1029
Compiler warnings: 0
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` rejects a null `ErrorProfileDefinition.IncludeCodeGroups` collection before invoking the resolver and reuses the established validator contract:

```text
Status: Invalid
Data: null
Code: ProfileIncludeCodeGroupsCollectionIsNull
Message: Profile include code groups collection is null.
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

- 1026/1026 — profile resolver exact-cancellation propagation complete.
- 1027/1027 — profile selection null error collection classification complete.
- 1028/1028 — profile selection null include-owners classification complete.
- 1029/1029 — profile selection null include-code-groups classification complete.

## Reconnaissance notes

No existing `ErrorProfileSelectionService` malformed-profile contracts were found for the remaining resolver-consumed collections:

- `IncludeCategories`
- `IncludeSubcategories`
- `IncludeTags`
- `ExcludeTags`
- `IncludeErrors`
- `ExcludeErrors`

`ErrorProfileCatalogValidator` already defines stable codes/messages for each null collection. Continue one collection at a time so each RED/GREEN step stays explicit and reviewable.

## Recommended verification

Pull current `master` and run only the focused contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveByProfileName_WhenProfileIncludeCategoriesCollectionIsNull_ReturnsInvalidResponse"
```

Expected current result: **RED** with `Actual: Failed` rather than the expected `Invalid`, because the null collection reaches `ErrorProfileResolver` and is normalized as `WIF_PROFILE_RESOLVER_FAILED`.

## Next recommended step

If RED confirms the response-shape mismatch, add the smallest guard for `profile.IncludeCategories is null` after the matching profile is found and before the resolver call. Reuse exactly:

```text
ProfileIncludeCategoriesCollectionIsNull
Profile include categories collection is null.
```

Then run focused and complete suites. Record **1030/1030 GREEN** before moving to the next collection.
