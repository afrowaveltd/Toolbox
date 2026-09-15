# Implementation status

Last updated: 2026-09-15

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite baseline: **1032/1032 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies null `ErrorCatalogDocument.Errors`, `IncludeOwners`, `IncludeCodeGroups`, `IncludeCategories`, `IncludeSubcategories`, and `IncludeTags` as malformed input rather than resolver failure.
- A focused malformed-profile contract for null `ErrorProfileDefinition.ExcludeTags` is committed and awaits local RED verification.

## 2026-09-15 — profile selection null exclude-tags contract

Contract commit:
`ed3d0519704d7e17c4e503804fcd02347f0ac6ab`

Baseline checkpoint commit:
`15c7112e6917cadaba6a001b21a1732e5cadedfb`

Added:

`WhenItFails.Tests/Resolution/ErrorProfileSelectionServiceNullExcludeTagsCollectionContractTests.cs`

Contract:

`ResolveByProfileName_WhenProfileExcludeTagsCollectionIsNull_ReturnsInvalidResponse`

`ErrorProfileCatalogValidator` already defines the stable malformed-profile contract:

```text
Status: Invalid
Code: ProfileExcludeTagsCollectionIsNull
Message: Profile exclude tags collection is null.
```

Production is intentionally unchanged before the focused run. Current `ErrorProfileResolver.Resolve(...)` passes `profile.ExcludeTags` to `CreateNormalizedSet(...)`, so a null collection is expected to throw and then be normalized by `ErrorProfileSelectionService` as `Failed / WIF_PROFILE_RESOLVER_FAILED`.

Expected eventual complete-suite count after this contract passes: **1033/1033 GREEN with zero compiler warnings**.

## 2026-09-15 — 1032/1032 GREEN null include-tags checkpoint

Malformed-profile contract commit:
`9641ffc496aef1864c0f62b08c85a381cf043cc8`

Production guard commit:
`c793f3bc7eb9722985057d44d4c29bf89220738d`

Checkpoint commit:
`15c7112e6917cadaba6a001b21a1732e5cadedfb`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1032
Skipped:  0
Total:  1032
Compiler warnings: 0
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` rejects a null `ErrorProfileDefinition.IncludeTags` collection before invoking the resolver and reuses the established validator contract:

```text
Status: Invalid
Data: null
Code: ProfileIncludeTagsCollectionIsNull
Message: Profile include tags collection is null.
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

- 1029/1029 — profile selection null include-code-groups classification complete.
- 1030/1030 — profile selection null include-categories classification complete.
- 1031/1031 — profile selection null include-subcategories classification complete.
- 1032/1032 — profile selection null include-tags classification complete.

## Reconnaissance notes

Remaining resolver-consumed profile collections still without `ErrorProfileSelectionService` malformed-profile coverage after the current contract:

- `IncludeErrors`
- `ExcludeErrors`

`ErrorProfileCatalogValidator` already defines stable codes/messages for each null collection. Continue one collection at a time so each RED/GREEN step stays explicit and reviewable.

## Recommended verification

Pull current `master` and run only the focused contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveByProfileName_WhenProfileExcludeTagsCollectionIsNull_ReturnsInvalidResponse"
```

Expected current result: **RED** with `Actual: Failed` rather than the expected `Invalid`, because the null collection reaches `ErrorProfileResolver` and is normalized as `WIF_PROFILE_RESOLVER_FAILED`.

## Next recommended step

If RED confirms the response-shape mismatch, add the smallest guard for `profile.ExcludeTags is null` after the existing profile collection guards and before the resolver call. Reuse exactly:

```text
ProfileExcludeTagsCollectionIsNull
Profile exclude tags collection is null.
```

Then run focused and complete suites. Record **1033/1033 GREEN** before moving to `IncludeErrors = null`.
