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
- Pipeline normalizer boundary is complete for the current scope.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1006/1006 tests with zero compiler warnings**.
- The pipeline validator delegate is the next boundary under audit.

## 2026-09-10 — 1006/1006 GREEN pipeline normalizer boundary checkpoint

Checkpoint commit: this commit.
Normalizer cancellation contract commit: `87d7502f8e49a46d1d3a5af89619d5c7b6d7a45b`.
Normalizer production fix commit: `2f25dd58a3644ee2e87685aaf2c5f909cea9adb4`.
Normalizer ordinary-exception contract commit: `81db615ca52e878f2583b6167df14a7d85124be2`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1006
Skipped:  0
Total:  1006
Compiler warnings: 0
```

Pipeline normalizer boundary is complete for the current scope:

- null result → `WIF_CATALOG_PIPELINE_NORMALIZER_RESULT_NULL`;
- ordinary exception → `WIF_CATALOG_PIPELINE_NORMALIZER_FAILED`;
- exact `OperationCanceledException` instance propagates unchanged.

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
2. normalizer delegate — complete for current scope.
3. validator delegate — null result covered; ordinary-exception and cancellation semantics not yet hardened.
4. payload-factory delegate — null result covered; exception semantics not yet hardened.

Stable malformed-result codes already include:

- `WIF_CATALOG_PIPELINE_LOADER_RESPONSE_NULL`
- `WIF_CATALOG_PIPELINE_NORMALIZER_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_VALIDATOR_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_PAYLOAD_NULL`

## Validator reconnaissance

Repository searches found no `CatalogProviderPipeline` validator exception-shape/pass-through contract and no concrete category/code-group/owner/profile provider contract requiring validator exceptions to propagate unchanged.

No existing `WIF_CATALOG_PIPELINE_VALIDATOR_FAILED` code was found before introducing a validator ordinary-exception contract.

## Verification state

- Clean continuation baseline: **1006/1006 GREEN, zero compiler warnings**.
- Pipeline loader and normalizer boundaries are complete for the current scope.
- Pipeline validator null-result behavior is already covered by `CatalogProviderPipelineNullResultContractTests`.
- No validator ordinary-exception test has been committed yet at this checkpoint.

## Next recommended step

Add one focused ordinary-exception contract for the pipeline validator delegate.

Expected stable contract:

```text
Status: Failed
Data: null
Code: WIF_CATALOG_PIPELINE_VALIDATOR_FAILED
Message: The catalog provider pipeline validator failed.
```

The payload factory must not be reached. Production code must remain unchanged until the focused RED is observed.

Keep changes small, tested, documented here, and committed directly to `master`.
