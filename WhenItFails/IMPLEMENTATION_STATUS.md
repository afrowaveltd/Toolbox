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
- `IErrorCatalogLoader.LoadFromFileAsync(...)` is complete for the current scope.
- `IErrorCatalogDocumentNormalizer.Normalize(...)` is complete for the current scope.
- `IErrorCatalogValidator.Validate(...)` is complete for the current scope.
- `IErrorCatalogFactory.Create(...)` is complete for the current scope.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1002/1002 tests with zero compiler warnings**.
- The next normalization boundary under reconnaissance is the shared internal `CatalogProviderPipeline` used by category, code-group, owner, and profile catalog providers.

## 2026-09-10 — 1002/1002 GREEN `ErrorCatalogProvider` checkpoint

Checkpoint commit: this commit.

Factory cancellation contract commit: `ae724530c67bfb02a427f46a9fe9401c763454c5`.
Factory production fix commit: `51444e6cb6be6df360d3861f4b251d1c1b8b8c88`.
Factory ordinary-exception contract commit: `da315a5d826177cdca2c3e773a42bd2f7b84db4a`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1002
Skipped:  0
Total:  1002
Compiler warnings: 0
```

The exact `OperationCanceledException` instance thrown by `IErrorCatalogFactory.Create(...)` propagates unchanged. Together with the previously verified loader, normalizer, and validator contracts, this closes the `ErrorCatalogProvider` dependency-boundary audit for the current scope.

## Completed `ErrorCatalogProvider` dependency boundaries

- `IErrorCatalogLoader.LoadFromFileAsync(...)`
  - null response → `WIF_ERROR_CATALOG_LOADER_RESPONSE_NULL`;
  - ordinary exception → `WIF_ERROR_CATALOG_LOADER_FAILED`;
  - exact cancellation propagates unchanged.
- `IErrorCatalogDocumentNormalizer.Normalize(...)`
  - null result → `WIF_ERROR_CATALOG_NORMALIZER_RESULT_NULL`;
  - ordinary exception → `WIF_ERROR_CATALOG_NORMALIZER_FAILED`;
  - exact cancellation propagates unchanged.
- `IErrorCatalogValidator.Validate(...)`
  - null result → `WIF_ERROR_CATALOG_VALIDATOR_RESULT_NULL`;
  - ordinary exception → `WIF_ERROR_CATALOG_VALIDATOR_FAILED`;
  - exact cancellation propagates unchanged.
- `IErrorCatalogFactory.Create(...)`
  - null result → `WIF_ERROR_CATALOG_FACTORY_RESULT_NULL`;
  - ordinary exception → `WIF_ERROR_CATALOG_FACTORY_FAILED`;
  - exact cancellation propagates unchanged.

## Established transparent boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` must preserve exceptions from its five provider dependencies.

Pre-existing tests require preservation of synchronous and faulted-task exception identity, exception type, inner exception references, `Exception.Data`, custom properties, cancellation information, and short-circuit behavior.

Relevant suites:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`

## `CatalogProviderPipeline` reconnaissance

`CatalogProviderPipeline.LoadNormalizeValidateAsync(...)` is used by:

1. `ErrorCategoryCatalogProvider`
2. `ErrorCodeGroupCatalogProvider`
3. `ErrorOwnerCatalogProvider`
4. `ErrorProfileCatalogProvider`

The pipeline currently performs:

1. loader delegate await;
2. null loader response handling;
3. failed loader response forwarding/fallback;
4. successful payload null check;
5. normalizer delegate call;
6. null normalizer result handling;
7. validator delegate call;
8. null validator result handling;
9. invalid validation handling;
10. payload factory delegate call;
11. null payload handling.

Existing stable malformed-result codes include:

- `WIF_CATALOG_PIPELINE_LOADER_RESPONSE_NULL`
- `WIF_CATALOG_PIPELINE_NORMALIZER_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_VALIDATOR_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_PAYLOAD_NULL`

Repository searches found no `CatalogProviderPipeline` exception-shape/pass-through contract and no concrete category/code-group/owner/profile provider contract requiring internal loader exceptions to propagate unchanged. Existing `ErrorCatalogContextProvider` propagation tests concern exceptions thrown by the provider dependency itself and remain a separate outer-boundary contract.

## Verification state

- Clean continuation baseline: **1002/1002 GREEN, zero compiler warnings**.
- `ErrorCatalogProvider` dependency-boundary audit is closed for the current scope.
- No production change has yet been made to `CatalogProviderPipeline` exception handling.

## Next recommended step

Add one focused ordinary-exception contract around the `CatalogProviderPipeline` loader delegate before changing production code.

Proposed stable contract:

```text
Status: Failed
Data: null
Code: WIF_CATALOG_PIPELINE_LOADER_FAILED
Message: The catalog provider pipeline loader failed.
```

The test should also require short-circuit behavior so normalize, validate, and payload creation are not reached after loader failure. Cancellation should remain a separate exact-instance contract after ordinary-exception behavior is verified.

Keep changes small, tested, documented here, and committed directly to `master`.
