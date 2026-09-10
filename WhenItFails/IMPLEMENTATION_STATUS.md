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
- Pipeline loader and normalizer boundaries are complete for the current scope.
- Pipeline validator null-result and ordinary-exception behavior are locally verified GREEN.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1007/1007 tests with zero compiler warnings**.

## 2026-09-10 — 1007/1007 GREEN pipeline validator exception checkpoint

Checkpoint commit: this commit.
Validator production fix commit: `c99bc586ff6f8f6e82e4a0b119ec3137ee92d792`.
Validator ordinary-exception contract commit: `25f0e8fe345d89e2e226aecbaaf4f3767a4a8010`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1007
Skipped:  0
Total:  1007
Compiler warnings: 0
```

Pipeline validator ordinary exceptions normalize to:

```text
Status: Failed
Data: null
Code: WIF_CATALOG_PIPELINE_VALIDATOR_FAILED
Message: The catalog provider pipeline validator failed.
```

The production guard excludes `OperationCanceledException`, so exact cancellation should continue to propagate unchanged.

## 2026-09-10 — 1006/1006 GREEN pipeline normalizer boundary checkpoint

Checkpoint commit: `157ce20f93d6a02500c30df352dc57b023e7df15`.
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
3. validator delegate — null result and ordinary exception covered; exact cancellation next.
4. payload-factory delegate — null result covered; exception semantics not yet hardened.

Stable malformed-result codes include:

- `WIF_CATALOG_PIPELINE_LOADER_RESPONSE_NULL`
- `WIF_CATALOG_PIPELINE_NORMALIZER_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_VALIDATOR_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_PAYLOAD_NULL`

Stable ordinary-exception codes currently include:

- `WIF_CATALOG_PIPELINE_LOADER_FAILED`
- `WIF_CATALOG_PIPELINE_NORMALIZER_FAILED`
- `WIF_CATALOG_PIPELINE_VALIDATOR_FAILED`

## Validator reconnaissance

Repository searches found no `CatalogProviderPipeline` validator exception-shape/pass-through contract and no concrete category/code-group/owner/profile provider contract requiring validator exceptions to propagate unchanged.

No existing `WIF_CATALOG_PIPELINE_VALIDATOR_FAILED` code was found before the focused validator contract was introduced.

## Verification state

- Clean continuation baseline: **1007/1007 GREEN, zero compiler warnings**.
- Pipeline loader and normalizer boundaries are complete for the current scope.
- Pipeline validator null-result and ordinary-exception behavior are locally verified.
- No exact-instance validator cancellation contract has been added yet at this checkpoint.

## Next recommended step

Add a focused exact-instance cancellation contract for the `CatalogProviderPipeline` validator delegate. It should throw a specific `OperationCanceledException` instance from `validate(...)`, use `Assert.Same(...)`, and ensure the payload factory is not reached.

No production change is expected because the validator guard explicitly excludes `OperationCanceledException`.

After that contract is GREEN, consider the pipeline validator boundary complete and inspect the payload-factory delegate separately.

Keep changes small, tested, documented here, and committed directly to `master`.
