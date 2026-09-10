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
- Pipeline normalizer ordinary-exception normalization is locally verified GREEN.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1005/1005 tests with zero compiler warnings**.

## 2026-09-10 — 1005/1005 GREEN pipeline normalizer exception checkpoint

Checkpoint commit: this commit.
Production fix commit: `2f25dd58a3644ee2e87685aaf2c5f909cea9adb4`.
Ordinary-exception contract commit: `81db615ca52e878f2583b6167df14a7d85124be2`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1005
Skipped:  0
Total:  1005
Compiler warnings: 0
```

Pipeline normalizer ordinary exceptions normalize to:

```text
Status: Failed
Data: null
Code: WIF_CATALOG_PIPELINE_NORMALIZER_FAILED
Message: The catalog provider pipeline normalizer failed.
```

The production guard excludes `OperationCanceledException`, so exact cancellation should continue to propagate unchanged.

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
2. normalizer delegate — null result and ordinary exception covered; exact cancellation next.
3. validator delegate — null result covered; exception semantics not yet hardened.
4. payload-factory delegate — null result covered; exception semantics not yet hardened.

Stable malformed-result codes already include:

- `WIF_CATALOG_PIPELINE_LOADER_RESPONSE_NULL`
- `WIF_CATALOG_PIPELINE_NORMALIZER_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_VALIDATOR_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_PAYLOAD_NULL`

Repository reconnaissance found no `CatalogProviderPipeline` exception-shape/pass-through contract and no concrete category/code-group/owner/profile provider contract requiring normalizer exceptions to propagate unchanged.

## Verification state

- Clean continuation baseline: **1005/1005 GREEN, zero compiler warnings**.
- Pipeline loader boundary is complete for the current scope.
- Pipeline normalizer null-result and ordinary-exception behavior are locally verified.
- No exact-instance normalizer cancellation contract has been added yet at this checkpoint.

## Next recommended step

Add a focused exact-instance cancellation contract for the `CatalogProviderPipeline` normalizer delegate.

The test should throw a specific `OperationCanceledException` instance from `normalize(...)`, use `Assert.Same(...)`, and ensure validator and payload factory are not reached.

No production change is expected because the normalizer guard explicitly excludes `OperationCanceledException`.

After that contract is GREEN, consider the pipeline normalizer boundary complete and inspect the validator delegate separately.

Keep changes small, tested, documented here, and committed directly to `master`.
