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
- The null `ErrorProfileDefinition.ExcludeErrors` RED is locally confirmed and the smallest production guard is committed; focused/full GREEN verification is pending.

## 2026-09-15 — profile selection null exclude-errors fix

Contract commit:
`66662fea33e04940e197b2f5550921cfe333b6a1`

Production guard commit:
`40bf621b8925cc923460890dd0652a1070e2909f`

Baseline checkpoint commit:
`47ac36a2c8b4973862917891847244ce7066d036`

Focused RED was locally confirmed:

```text
Expected: Invalid
Actual:   Failed
```

The null `ErrorProfileDefinition.ExcludeErrors` collection reached `ErrorProfileResolver.Resolve(...)` and was therefore misclassified by the dependency exception boundary as `Failed / WIF_PROFILE_RESOLVER_FAILED`.

Production change in `WhenItFails/Resolution/ErrorProfileSelectionService.cs` is intentionally minimal: after the existing profile collection guards, the service now validates `profile.ExcludeErrors` and reuses the established validator/cross-validator contract:

```text
Status: Invalid
Data: null
Code: ProfileExcludeErrorsCollectionIsNull
Message: Profile exclude errors collection is null.
```

No resolver exception, cancellation, profile lookup, or other response behavior was changed.

Expected complete-suite count after verification: **1035/1035 GREEN with zero compiler warnings**.

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

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveByProfileName_WhenProfileExcludeErrorsCollectionIsNull_ReturnsInvalidResponse"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1035/1035 GREEN
Compiler warnings: 0
```

## Next recommended step

After **1035/1035 GREEN** is confirmed, record the checkpoint. This closes the current malformed resolver-consumed profile collection series; then perform reconnaissance for the next uncovered `ErrorProfileSelectionService` malformed-context or dependency-boundary contract rather than adding further guards speculatively.
