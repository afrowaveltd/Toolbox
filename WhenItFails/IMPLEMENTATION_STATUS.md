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
- The pipeline normalizer ordinary-exception contract is verified RED and the smallest production guard is committed; local GREEN verification is pending.

## 2026-09-10 — pipeline normalizer ordinary-exception fix

Production fix commit: `2f25dd58a3644ee2e87685aaf2c5f909cea9adb4`.

Changed only the `normalize(loadResponse.Data)` invocation inside `CatalogProviderPipeline.LoadNormalizeValidateAsync(...)`.

Ordinary normalizer exceptions are now converted to:

```text
Status: Failed
Data: null
Code: WIF_CATALOG_PIPELINE_NORMALIZER_FAILED
Message: The catalog provider pipeline normalizer failed.
```

The catch filter excludes `OperationCanceledException`, so cancellation originating from the normalizer delegate continues to propagate unchanged.

The production commit diff was checked and contains only the intended normalizer exception guard. Existing null-result handling and the validator and payload-factory phases remain unchanged.

## 2026-09-10 — verified RED pipeline normalizer contract

Contract commit: `81db615ca52e878f2583b6167df14a7d85124be2`.

Focused test:

`WhenItFails.Tests.Catalog.CatalogProviderPipelineNormalizerExceptionContractTests.LoadNormalizeValidateAsync_WhenNormalizerThrows_ReturnsStableFailure`

Observed locally before the production fix:

```text
Failed: 1
Passed: 0
Skipped: 0
Total: 1
```

Failure:

```text
System.InvalidOperationException:
Sensitive catalog provider pipeline normalizer detail must not escape.
```

The exception escaped directly from the normalizer delegate through `CatalogProviderPipeline.LoadNormalizeValidateAsync(...)`, confirming the missing normalizer exception boundary.

The validator and payload-factory delegates in the test throw if reached, so the contract also locks short-circuit behavior after normalizer failure.

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
2. normalizer delegate — null result covered; ordinary-exception guard committed and awaiting GREEN verification.
3. validator delegate — null result covered; exception semantics not yet hardened.
4. payload-factory delegate — null result covered; exception semantics not yet hardened.

Stable malformed-result codes already include:

- `WIF_CATALOG_PIPELINE_LOADER_RESPONSE_NULL`
- `WIF_CATALOG_PIPELINE_NORMALIZER_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_VALIDATOR_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_PAYLOAD_NULL`

Repository reconnaissance found no `CatalogProviderPipeline` exception-shape/pass-through contract and no concrete category/code-group/owner/profile provider contract requiring normalizer exceptions to propagate unchanged.

## Verification state

- Clean continuation baseline before the normalizer test: **1004/1004 GREEN, zero compiler warnings**.
- Pipeline loader boundary is complete for the current scope.
- Pipeline normalizer null-result behavior is already covered.
- Pipeline normalizer ordinary-exception contract is verified RED before the production fix.
- Production normalizer guard is committed and awaits focused local GREEN verification.
- Expected complete-suite count after the contract passes: **1005 tests**.

## Recommended verification

Pull current `master` and run only the normalizer exception contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadNormalizeValidateAsync_WhenNormalizerThrows_ReturnsStableFailure"
```

Expected result after the production fix: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **1005/1005 GREEN with zero compiler warnings**.

## Next recommended step

After **1005/1005 GREEN** is confirmed, add a separate exact-instance cancellation contract for the `CatalogProviderPipeline` normalizer delegate.

If that passes without production changes, consider the pipeline normalizer boundary complete for the current scope and inspect the validator delegate separately before adding any new contract.

Do not change validator or payload-factory exception behavior in the same production commit.

Keep changes small, tested, documented here, and committed directly to `master`.
