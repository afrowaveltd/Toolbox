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
- Pipeline loader ordinary-exception normalization is locally verified GREEN.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1003/1003 tests with zero compiler warnings**.

## 2026-09-10 — 1003/1003 GREEN pipeline loader exception checkpoint

Checkpoint commit: this commit.

Pipeline loader production fix commit: `42e2e74cbf43adbc62fbaa9616c33b9db9bcb835`.
Pipeline loader ordinary-exception contract commit: `2a5232c4c15b242be0cc1dfae40653d57964223e`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1003
Skipped:  0
Total:  1003
Compiler warnings: 0
```

The pipeline loader ordinary-exception contract is now stable:

```text
Status: Failed
Data: null
Code: WIF_CATALOG_PIPELINE_LOADER_FAILED
Message: The catalog provider pipeline loader failed.
```

The loader guard excludes `OperationCanceledException`, so exact cancellation should continue to propagate unchanged.

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

Current pipeline phases:

1. loader delegate — null response and ordinary exception covered; exact cancellation next.
2. normalizer delegate — null result covered; exception semantics not yet hardened.
3. validator delegate — null result covered; exception semantics not yet hardened.
4. payload-factory delegate — null result covered; exception semantics not yet hardened.

Existing stable malformed-result codes:

- `WIF_CATALOG_PIPELINE_LOADER_RESPONSE_NULL`
- `WIF_CATALOG_PIPELINE_NORMALIZER_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_VALIDATOR_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_PAYLOAD_NULL`

Repository searches found no `CatalogProviderPipeline` exception-shape/pass-through contract and no concrete category/code-group/owner/profile provider contract requiring internal pipeline exceptions to propagate unchanged.

## Verification state

- Clean continuation baseline: **1003/1003 GREEN, zero compiler warnings**.
- Pipeline loader ordinary-exception behavior is locally verified.
- No exact-instance loader cancellation contract has been added yet at this checkpoint.

## Next recommended step

Add a focused exact-instance cancellation contract for the `CatalogProviderPipeline` loader delegate.

The test should use a specific `OperationCanceledException` instance and `Assert.Same(...)`, while normalizer, validator, and payload factory throw if reached.

No production change is expected because the loader guard explicitly excludes `OperationCanceledException`.

After that contract is GREEN, consider the pipeline loader boundary complete and inspect the pipeline normalizer delegate separately.

Keep changes small, tested, documented here, and committed directly to `master`.
