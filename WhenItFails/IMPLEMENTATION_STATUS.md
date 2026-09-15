# Implementation status

Last updated: 2026-09-15

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite: **1035/1035 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` now classifies null `ErrorCatalogDocument.Errors` and all resolver-consumed profile collections (`IncludeOwners`, `IncludeCodeGroups`, `IncludeCategories`, `IncludeSubcategories`, `IncludeTags`, `ExcludeTags`, `IncludeErrors`, `ExcludeErrors`) as malformed input rather than resolver failure.

## 2026-09-15 — 1035/1035 GREEN malformed profile collection series complete

Final malformed-profile contract commit:
`66662fea33e04940e197b2f5550921cfe333b6a1`

Final production guard commit:
`40bf621b8925cc923460890dd0652a1070e2909f`

Previous checkpoint commit:
`47ac36a2c8b4973862917891847244ce7066d036`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1035
Skipped:  0
Total:  1035
Compiler warnings: 0
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` rejects a null `ErrorProfileDefinition.ExcludeErrors` collection before invoking the resolver and reuses the established validator/cross-validator contract:

```text
Status: Invalid
Data: null
Code: ProfileExcludeErrorsCollectionIsNull
Message: Profile exclude errors collection is null.
```

This completes the current malformed resolver-consumed profile collection series.

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

- 1032/1032 — profile selection null include-tags classification complete.
- 1033/1033 — profile selection null exclude-tags classification complete.
- 1034/1034 — profile selection null include-errors classification complete.
- 1035/1035 — profile selection null exclude-errors classification complete; malformed resolver-consumed profile collection series closed.

## Next recommended step

Perform fresh reconnaissance for the next uncovered `ErrorProfileSelectionService` malformed-context or dependency-boundary contract. Compare the service's preconditions with `ErrorProfileCatalogValidator` and existing selection-service contract tests before adding any new guard.
