# Implementation status

Last updated: 2026-09-15

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite baseline: **1028/1028 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies null `ErrorCatalogDocument.Errors` and null `ErrorProfileDefinition.IncludeOwners` as malformed input rather than resolver failure.
- The null `ErrorProfileDefinition.IncludeCodeGroups` RED is locally confirmed and the smallest production guard is committed; focused/full GREEN verification is pending.

## 2026-09-15 — profile selection null include-code-groups fix

Contract commit:
`806580ada38916c069b8eb11161999356be6f73c`

Production guard commit:
`e55b359cb4232e383e2087ea9238b237a9175cc5`

Baseline checkpoint commit:
`1edf5adaad77775d4e9b0e068e60fbd2b2204c47`

Focused RED was locally confirmed:

```text
Expected: Invalid
Actual:   Failed
```

The null `ErrorProfileDefinition.IncludeCodeGroups` collection reached `ErrorProfileResolver.Resolve(...)` and was therefore misclassified by the dependency exception boundary as `Failed / WIF_PROFILE_RESOLVER_FAILED`.

Production change in `WhenItFails/Resolution/ErrorProfileSelectionService.cs` is intentionally minimal: after the matching profile is found and after the existing `IncludeOwners` guard, the service now validates `profile.IncludeCodeGroups` and reuses the established validator contract:

```text
Status: Invalid
Data: null
Code: ProfileIncludeCodeGroupsCollectionIsNull
Message: Profile include code groups collection is null.
```

No resolver exception, cancellation, profile lookup, or other response behavior was changed.

Expected complete-suite count after verification: **1029/1029 GREEN with zero compiler warnings**.

## 2026-09-15 — 1028/1028 GREEN null include-owners checkpoint

Malformed-profile contract commit:
`22c8e33b28edc0501cf4f9202e57fed6eea7d8ac`

Production guard commit:
`19ff6c4c97fe967ba9ee6e7159ac436581e08505`

Checkpoint commit:
`1edf5adaad77775d4e9b0e068e60fbd2b2204c47`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1028
Skipped:  0
Total:  1028
Compiler warnings: 0
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` rejects a null `ErrorProfileDefinition.IncludeOwners` collection before invoking the resolver and reuses the established validator contract:

```text
Status: Invalid
Data: null
Code: ProfileIncludeOwnersCollectionIsNull
Message: Profile include owners collection is null.
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

- 1025/1025 — profile resolver ordinary-exception normalization complete.
- 1026/1026 — profile resolver exact-cancellation propagation complete.
- 1027/1027 — profile selection null error collection classification complete.
- 1028/1028 — profile selection null include-owners classification complete.

## Reconnaissance notes

No existing `ErrorProfileSelectionService` malformed-profile contracts were found for the remaining resolver-consumed collections:

- `IncludeCodeGroups`
- `IncludeCategories`
- `IncludeSubcategories`
- `IncludeTags`
- `ExcludeTags`
- `IncludeErrors`
- `ExcludeErrors`

`ErrorProfileCatalogValidator` already defines stable codes/messages for each null collection. Continue one collection at a time so each RED/GREEN step stays explicit and reviewable.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveByProfileName_WhenProfileIncludeCodeGroupsCollectionIsNull_ReturnsInvalidResponse"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1029/1029 GREEN
Compiler warnings: 0
```

## Next recommended step

After **1029/1029 GREEN** is confirmed, record the checkpoint and continue one collection at a time with the next resolver-consumed malformed profile shape, most naturally `IncludeCategories = null`, reusing the validator's stable `ProfileIncludeCategoriesCollectionIsNull` contract.
