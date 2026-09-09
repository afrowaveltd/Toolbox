# Implementation status

Last updated: 2026-09-09

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening dependency boundaries while preserving established public exception contracts.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service and profile-selection exception/cancellation boundaries are complete.
- All currently known `IErrorCatalogContextStore` read/write boundaries in the active runtime/initializer scope are complete.
- `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` is complete for the current scope.
- `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` as consumed by `ErrorCatalogInitializer` is complete for the current scope.
- The recovery baseline is locally verified **GREEN at 994/994 tests with zero compiler warnings**.
- `ErrorCatalogContextProvider` is intentionally a transparent orchestration boundary for exceptions thrown by its five internal catalog providers. Existing propagation/shape tests require those exceptions to remain unchanged.
- Reconnaissance has moved one layer down to `ErrorCatalogProvider`, which already normalizes null/malformed dependency outputs but currently has unguarded loader/normalizer/validator/factory calls.

## 2026-09-09 — 994/994 GREEN recovery checkpoint

Checkpoint commit records successful local verification after restoring the established `ErrorCatalogContextProvider` exception-transparency contract.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 994
Skipped:  0
Total:  994
Compiler warnings: 0
```

Recovery remains complete:

- `ErrorCatalogContextProvider.cs` is back to the exact pre-experiment blob `79e298aafcdb07bd521ef7eff9d04d0f2e7e88af`.
- The contradictory temporary normalization test is removed.
- No earlier initializer/runtime hardening was reverted.

## Established transparent boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` must preserve exceptions from its five provider dependencies.

Pre-existing tests require preservation of synchronous and faulted-task exception identity, exception type, inner exception references, `Exception.Data`, custom properties, cancellation information, and short-circuit behavior.

Relevant suites:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`

## Reconnaissance — next candidate

`ErrorCatalogProvider.LoadFromFileAsync(...)` composes:

1. `IErrorCatalogLoader.LoadFromFileAsync(...)`
2. `IErrorCatalogDocumentNormalizer.Normalize(...)`
3. `IErrorCatalogValidator.Validate(...)`
4. `IErrorCatalogFactory.Create(...)`

It already converts these malformed dependency outputs into stable responses:

- null loader response → `WIF_ERROR_CATALOG_LOADER_RESPONSE_NULL`;
- null normalizer result → `WIF_ERROR_CATALOG_NORMALIZER_RESULT_NULL`;
- null validator result → `WIF_ERROR_CATALOG_VALIDATOR_RESULT_NULL`;
- null factory result → `WIF_ERROR_CATALOG_FACTORY_RESULT_NULL`.

Repository searches found no existing `ErrorCatalogProvider` contract requiring ordinary dependency exceptions to propagate unchanged. Existing provider tests cover normal flow, failed responses, malformed/null outputs, constructor guards, and cancellation.

## Next recommended step

Add one focused RED-first contract for an ordinary exception from `IErrorCatalogLoader.LoadFromFileAsync(...)` inside `ErrorCatalogProvider`.

Do not touch normalizer/validator/factory exception behavior in the same step.

If RED confirms raw loader exception leakage, add only the smallest loader exception boundary while allowing `OperationCanceledException` to propagate unchanged. Then verify focused + full GREEN before adding exact-instance cancellation for the loader boundary.

Keep changes small, tested, documented here, and committed directly to `master`.
