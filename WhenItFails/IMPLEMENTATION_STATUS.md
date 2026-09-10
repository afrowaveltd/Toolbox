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
- `ErrorCatalogProvider` is the current normalization boundary under audit.
- `IErrorCatalogLoader.LoadFromFileAsync(...)` is complete for the current scope: null response, ordinary exception normalization, and exact-instance cancellation are covered.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 996/996 tests with zero compiler warnings**.

## 2026-09-10 — 996/996 GREEN loader boundary checkpoint

Checkpoint follows loader cancellation contract commit `6586cf4e41de535421faed2bcfab68faad16c946` and production fix `49ab1d418217d3745f256b347efee379fe374934`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 996
Skipped:  0
Total:  996
Compiler warnings: 0
```

Loader ordinary exceptions are normalized to:

```text
Status: Failed
Data: null
Code: WIF_ERROR_CATALOG_LOADER_FAILED
Message: The error catalog loader failed.
```

Loader `OperationCanceledException` instances propagate unchanged.

## Established transparent boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` must preserve exceptions from its five provider dependencies.

Pre-existing tests require preservation of synchronous and faulted-task exception identity, exception type, inner exception references, `Exception.Data`, custom properties, cancellation information, and short-circuit behavior.

Relevant suites:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`

## `ErrorCatalogProvider` boundary map

`ErrorCatalogProvider.LoadFromFileAsync(...)` composes:

1. `IErrorCatalogLoader.LoadFromFileAsync(...)` — complete for current scope.
2. `IErrorCatalogDocumentNormalizer.Normalize(...)` — next boundary under audit.
3. `IErrorCatalogValidator.Validate(...)`.
4. `IErrorCatalogFactory.Create(...)`.

Existing malformed-result handling:

- null loader response → `WIF_ERROR_CATALOG_LOADER_RESPONSE_NULL`;
- null normalizer result → `WIF_ERROR_CATALOG_NORMALIZER_RESULT_NULL`;
- null validator result → `WIF_ERROR_CATALOG_VALIDATOR_RESULT_NULL`;
- null factory result → `WIF_ERROR_CATALOG_FACTORY_RESULT_NULL`.

Repository reconnaissance found no existing `ErrorCatalogProvider` propagation/shape contract requiring normalizer exceptions to escape unchanged.

## Verification state

- Clean continuation baseline: **996/996 GREEN, zero compiler warnings**.
- Loader boundary complete for current scope.
- Normalizer null-result contract already exists.
- No normalizer ordinary-exception normalization has been implemented yet.

## Next recommended step

Add one focused RED contract for an ordinary exception thrown by `IErrorCatalogDocumentNormalizer.Normalize(...)`.

Expected stable target if RED confirms the gap:

```text
Status: Failed
Data: null
Code: WIF_ERROR_CATALOG_NORMALIZER_FAILED
Message: The error catalog document normalizer failed.
```

The validator and factory must not be called after the normalizer fails. Keep production code unchanged until RED is observed.

Keep changes small, tested, documented here, and committed directly to `master`.
