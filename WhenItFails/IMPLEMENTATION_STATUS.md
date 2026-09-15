# Implementation status

Last updated: 2026-09-15

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite baseline: **1033/1033 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies null `ErrorCatalogDocument.Errors`, `IncludeOwners`, `IncludeCodeGroups`, `IncludeCategories`, `IncludeSubcategories`, `IncludeTags`, and `ExcludeTags` as malformed input rather than resolver failure.
- A focused malformed-profile contract for null `ErrorProfileDefinition.IncludeErrors` is committed and awaits local RED verification.

## 2026-09-15 — profile selection null include-errors contract

Contract commit:
`e21f859aab8154769d088d434fb28ae932529b73`

Baseline checkpoint commit:
`9e876a9f4edb5d51a1bb3157b9597802da43f87c`

Added:

`WhenItFails.Tests/Resolution/ErrorProfileSelectionServiceNullIncludeErrorsCollectionContractTests.cs`

Contract:

`ResolveByProfileName_WhenProfileIncludeErrorsCollectionIsNull_ReturnsInvalidResponse`

`ErrorProfileCatalogValidator` and `ErrorCatalogCrossValidator` already define the stable malformed-profile contract:

```text
Status: Invalid
Code: ProfileIncludeErrorsCollectionIsNull
Message: Profile include errors collection is null.
```

Production is intentionally unchanged before the focused run. Current `ErrorProfileResolver.Resolve(...)` consumes `profile.IncludeErrors`, so a null collection is expected to throw and then be normalized by `ErrorProfileSelectionService` as `Failed / WIF_PROFILE_RESOLVER_FAILED`.

Expected eventual complete-suite count after this contract passes: **1034/1034 GREEN with zero compiler warnings**.

## 2026-09-15 — 1033/1033 GREEN null exclude-tags checkpoint

Malformed-profile contract commit:
`ed3d0519704d7e17c4e503804fcd02347f0ac6ab`

Production guard commit:
`ea00a602d69924c93e572a71c47fdad7ebb7bfdb`

Checkpoint commit:
`9e876a9f4edb5d51a1bb3157b9597802da43f87c`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1033
Skipped:  0
Total:  1033
Compiler warnings: 0
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` rejects a null `ErrorProfileDefinition.ExcludeTags` collection before invoking the resolver and reuses the established validator contract:

```text
Status: Invalid
Data: null
Code: ProfileExcludeTagsCollectionIsNull
Message: Profile exclude tags collection is null.
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

- 1030/1030 — profile selection null include-categories classification complete.
- 1031/1031 — profile selection null include-subcategories classification complete.
- 1032/1032 — profile selection null include-tags classification complete.
- 1033/1033 — profile selection null exclude-tags classification complete.

## Reconnaissance notes

After the current contract, the only remaining resolver-consumed profile collection still without `ErrorProfileSelectionService` malformed-profile coverage is:

- `ExcludeErrors`

`ErrorProfileCatalogValidator` and `ErrorCatalogCrossValidator` already define stable codes/messages for both error-id collections.

## Recommended verification

Pull current `master` and run only the focused contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveByProfileName_WhenProfileIncludeErrorsCollectionIsNull_ReturnsInvalidResponse"
```

Expected current result: **RED** with `Actual: Failed` rather than the expected `Invalid`, because the null collection reaches `ErrorProfileResolver` and is normalized as `WIF_PROFILE_RESOLVER_FAILED`.

## Next recommended step

If RED confirms the response-shape mismatch, add the smallest guard for `profile.IncludeErrors is null` after the existing profile collection guards and before the resolver call. Reuse exactly:

```text
ProfileIncludeErrorsCollectionIsNull
Profile include errors collection is null.
```

Then run focused and complete suites. Record **1034/1034 GREEN** before moving to the final `ExcludeErrors = null` collection.
