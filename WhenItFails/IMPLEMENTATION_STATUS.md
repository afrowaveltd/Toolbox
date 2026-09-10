# Implementation status

Last updated: 2026-09-10

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening dependency boundaries while preserving established public exception contracts.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service and profile-selection exception/cancellation boundaries are complete.
- All currently known `IErrorCatalogContextStore` read/write boundaries in the active runtime/initializer scope are complete.
- `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` is complete for the current scope.
- `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` as consumed by `ErrorCatalogInitializer` is complete for the current scope.
- `ErrorCatalogContextProvider` is intentionally a transparent orchestration boundary for exceptions thrown by its five internal catalog providers; do not normalize them there.
- `ErrorCatalogProvider` dependency-boundary audit is complete for the current scope.
- The shared internal `CatalogProviderPipeline` is the current normalization boundary under audit.
- Pipeline loader boundary is complete for the current scope.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1004/1004 tests with zero compiler warnings** before the new normalizer exception contract.
- A focused ordinary-exception contract is now committed for the pipeline normalizer delegate and awaits local RED verification.

## 2026-09-10 — pipeline normalizer ordinary-exception contract

Contract commit: `81db615ca52e878f2583b6167df14a7d85124be2`.

Added:

`WhenItFails.Tests/Catalog/CatalogProviderPipelineNormalizerExceptionContractTests.cs`

Test:

`LoadNormalizeValidateAsync_WhenNormalizerThrows_ReturnsStableFailure`

The normalizer delegate throws:

```text
System.InvalidOperationException:
Sensitive catalog provider pipeline normalizer detail must not escape.
```

Required stable pipeline contract:

```text
Status: Failed
Data: null
Code: WIF_CATALOG_PIPELINE_NORMALIZER_FAILED
Message: The catalog provider pipeline normalizer failed.
```

The validator and payload-factory delegates throw if reached, so the contract also locks short-circuit behavior after normalizer failure.

No production code changed. The normalizer delegate is currently invoked directly, so the focused test is expected to be RED with the raw normalizer exception escaping.

## 2026-09-10 — 1004/1004 GREEN pipeline loader boundary checkpoint

Checkpoint commit: `37711c6e390b2e6107cfd1271dac5a1d982e71e8`.
Pipeline loader cancellation contract commit: `7ef0585dfabc02edfb52b84298a11f6887586b2b`.
Pipeline loader production fix commit: `42e2e74cbf43adbc62fbaa9616c33b9db9bcb835`.
Pipeline loader ordinary-exception contract commit: `2a5232c4c15b242be0cc1dfae40653d57964223e`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1004
Skipped:  0
Total:  1004
Compiler warnings: 0
```

Pipeline loader ordinary exceptions normalize to `WIF_CATALOG_PIPELINE_LOADER_FAILED`; exact-instance cancellation propagates unchanged.

## Completed `ErrorCatalogProvider` dependency boundaries

- `IErrorCatalogLoader.LoadFromFileAsync(...)` — null response, ordinary exception normalization, exact cancellation propagation.
- `IErrorCatalogDocumentNormalizer.Normalize(...)` — null result, ordinary exception normalization, exact cancellation propagation.
- `IErrorCatalogValidator.Validate(...)` — null result, ordinary exception normalization, exact cancellation propagation.
- `IErrorCatalogFactory.Create(...)` — null result, ordinary exception normalization, exact cancellation propagation.

## Established transparent boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` must preserve exceptions from its five provider dependencies.

Relevant suites:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`

## `CatalogProviderPipeline` boundary map

`CatalogProviderPipeline.LoadNormalizeValidateAsync(...)` is used by:

1. `ErrorCategoryCatalogProvider`
2. `ErrorCodeGroupCatalogProvider`
3. `ErrorOwnerCatalogProvider`
4. `ErrorProfileCatalogProvider`

Current phases:

1. loader delegate — complete for current scope.
2. normalizer delegate — null result covered; ordinary-exception contract awaiting RED.
3. validator delegate — null result covered; exception semantics not yet hardened.
4. payload-factory delegate — null result covered; exception semantics not yet hardened.

Stable malformed-result codes already include:

- `WIF_CATALOG_PIPELINE_LOADER_RESPONSE_NULL`
- `WIF_CATALOG_PIPELINE_NORMALIZER_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_VALIDATOR_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_PAYLOAD_NULL`

Repository reconnaissance found no `CatalogProviderPipeline` exception-shape/pass-through contract and no concrete category/code-group/owner/profile provider contract requiring normalizer exceptions to propagate unchanged. No existing `WIF_CATALOG_PIPELINE_NORMALIZER_FAILED` code was found before this contract was introduced.

## Verification state

- Clean continuation baseline before the normalizer test: **1004/1004 GREEN, zero compiler warnings**.
- Pipeline loader boundary is complete for the current scope.
- Pipeline normalizer null-result behavior is already covered.
- New normalizer ordinary-exception contract is committed and awaits focused RED verification.
- Production normalizer call remains unchanged until RED is observed.
- Expected complete-suite count once the new contract eventually passes: **1005 tests**.

## Recommended verification

Pull current `master` and run only the new contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadNormalizeValidateAsync_WhenNormalizerThrows_ReturnsStableFailure"
```

Expected current result: RED with the raw exception text:

```text
Sensitive catalog provider pipeline normalizer detail must not escape.
```

## Next recommended step

If RED is confirmed, add the smallest ordinary-exception guard around only `normalize(loadResponse.Data)`, excluding `OperationCanceledException` so exact-instance cancellation can be tested separately afterward.

Do not change validator or payload-factory exception behavior in the same production commit.

Keep changes small, tested, documented here, and committed directly to `master`.
