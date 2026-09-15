# Implementation status

Last updated: 2026-09-15

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite baseline: **1035/1035 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` now classifies null `ErrorCatalogDocument.Errors` and all resolver-consumed profile collections (`IncludeOwners`, `IncludeCodeGroups`, `IncludeCategories`, `IncludeSubcategories`, `IncludeTags`, `ExcludeTags`, `IncludeErrors`, `ExcludeErrors`) as malformed input rather than resolver failure.
- Fresh reconnaissance found the next malformed-context boundary: a null `ErrorDefinition` inside `ErrorCatalogDocument.Errors` is rejected by `ErrorCatalogValidator` but is not yet classified explicitly by `ErrorProfileSelectionService`.

## 2026-09-15 — profile selection null error-definition contract

Contract commit:
`ce50bfcd9dfd6fc09b9be32ca16b28eb06323cd7`

Baseline checkpoint commit:
`471f7366b55e927ec73afb436f6fbe3c10773fbf`

Added:

`WhenItFails.Tests/Resolution/ErrorProfileSelectionServiceNullErrorDefinitionContractTests.cs`

Contract:

`ResolveByProfileName_WhenErrorCatalogContainsNullDefinition_ReturnsInvalidResponse`

`ErrorCatalogValidator` already defines the stable malformed-error contract:

```text
Status: Invalid
Code: ErrorDefinitionIsNull
Message: Error catalog contains a null error definition.
```

Production is intentionally unchanged before the focused run. `ErrorProfileResolver.Resolve(...)` iterates every error definition and dereferences `error.Id` immediately through exclusion matching, so a null definition is expected to throw and then be normalized by `ErrorProfileSelectionService` as `Failed / WIF_PROFILE_RESOLVER_FAILED`.

Expected eventual complete-suite count after this contract passes: **1036/1036 GREEN with zero compiler warnings**.

## 2026-09-15 — 1035/1035 GREEN malformed profile collection series complete

Final malformed-profile contract commit:
`66662fea33e04940e197b2f5550921cfe333b6a1`

Final production guard commit:
`40bf621b8925cc923460890dd0652a1070e2909f`

Checkpoint commit:
`471f7366b55e927ec73afb436f6fbe3c10773fbf`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1035
Skipped:  0
Total:  1035
Compiler warnings: 0
```

This closes the malformed resolver-consumed profile collection series.

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

## Reconnaissance notes

`ErrorProfileResolver` also consumes each `ErrorDefinition`'s `Subcategories` and `Tags` collections. `ErrorCatalogValidator` already has stable malformed contracts for null error definitions and null resolver-consumed error collections. Continue one concrete contract at a time, starting with a null error definition before auditing nested collections.

## Recommended verification

Pull current `master` and run only the focused contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveByProfileName_WhenErrorCatalogContainsNullDefinition_ReturnsInvalidResponse"
```

Expected current result: **RED** with `Actual: Failed` rather than the expected `Invalid`.

## Next recommended step

If RED confirms the response-shape mismatch, add the smallest `ErrorProfileSelectionService` guard that detects any null error definition before invoking `IErrorProfileResolver`, reusing exactly:

```text
ErrorDefinitionIsNull
Error catalog contains a null error definition.
```

Then run focused and complete suites before proceeding to nested resolver-consumed error collections.
