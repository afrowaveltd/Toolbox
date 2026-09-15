# Implementation status

Last updated: 2026-09-15

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite: **1028/1028 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies null `ErrorCatalogDocument.Errors` as malformed input rather than resolver failure.
- `ErrorProfileSelectionService` classifies null `ErrorProfileDefinition.IncludeOwners` as malformed input rather than resolver failure.

## 2026-09-15 — 1028/1028 GREEN null include-owners checkpoint

Malformed-profile contract commit:
`22c8e33b28edc0501cf4f9202e57fed6eea7d8ac`

Production guard commit:
`19ff6c4c97fe967ba9ee6e7159ac436581e08505`

Previous checkpoint commit:
`1ad1743b1aebec4ca5d19bc036b1e1046a5895be`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1028
Skipped:  0
Total:  1028
Compiler warnings: 0
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` now rejects a null `ErrorProfileDefinition.IncludeOwners` collection before invoking the resolver and reuses the established validator contract:

```text
Status: Invalid
Data: null
Code: ProfileIncludeOwnersCollectionIsNull
Message: Profile include owners collection is null.
```

This prevents malformed profile data from being misclassified as `Failed / WIF_PROFILE_RESOLVER_FAILED`.

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

## Next recommended step

Add one focused contract for `ErrorProfileDefinition.IncludeCodeGroups = null` in `ErrorProfileSelectionService`. Expected stable malformed-profile response:

```text
Status: Invalid
Code: ProfileIncludeCodeGroupsCollectionIsNull
Message: Profile include code groups collection is null.
```

Production should remain unchanged until the focused RED run confirms the current misclassification as `Failed / WIF_PROFILE_RESOLVER_FAILED`.
