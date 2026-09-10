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
- `IErrorCatalogDocumentNormalizer.Normalize(...)` is complete for the current scope: null result, ordinary exception normalization, and exact-instance cancellation are covered.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 998/998 tests with zero compiler warnings**.
- The next boundary under inspection is `IErrorCatalogValidator.Validate(...)`.

## 2026-09-10 — 998/998 GREEN normalizer boundary checkpoint

Checkpoint commit: this commit.

Normalizer cancellation contract commit: `35e92a49afdee0f98c7685387ca88e66715cc165`.
Normalizer production fix commit: `ee06db4fa33b1e8f8c4cacef45448bb40be5d221`.
Normalizer ordinary-exception contract commit: `7c019211b48edc611f293ac49624a3ad4cfdd0b4`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 998
Skipped:  0
Total:  998
Compiler warnings: 0
```

`IErrorCatalogDocumentNormalizer.Normalize(...)` ordinary exceptions normalize to:

```text
Status: Failed
Data: null
Code: WIF_ERROR_CATALOG_NORMALIZER_FAILED
Message: The error catalog document normalizer failed.
```

A specific `OperationCanceledException` instance thrown by the normalizer propagates unchanged and is verified with `Assert.Same(...)`.

## 2026-09-10 — 996/996 GREEN loader boundary checkpoint

Checkpoint commit: `2d923b51a70105e02240333f9b8ee956c1a6006b`

Loader cancellation contract commit: `6586cf4e41de535421faed2bcfab68faad16c946`.
Loader production fix commit: `49ab1d418217d3745f256b347efee379fe374934`.

`IErrorCatalogLoader` boundary is complete for the current scope. Ordinary exceptions normalize to `WIF_ERROR_CATALOG_LOADER_FAILED`; exact loader cancellation propagates unchanged.

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
2. `IErrorCatalogDocumentNormalizer.Normalize(...)` — complete for current scope.
3. `IErrorCatalogValidator.Validate(...)` — next boundary under inspection.
4. `IErrorCatalogFactory.Create(...)`.

Existing malformed-result handling:

- null loader response → `WIF_ERROR_CATALOG_LOADER_RESPONSE_NULL`;
- null normalizer result → `WIF_ERROR_CATALOG_NORMALIZER_RESULT_NULL`;
- null validator result → `WIF_ERROR_CATALOG_VALIDATOR_RESULT_NULL`;
- null factory result → `WIF_ERROR_CATALOG_FACTORY_RESULT_NULL`.

Repository reconnaissance found no existing `ErrorCatalogProvider` propagation/shape contract requiring validator exceptions to escape unchanged. No existing `WIF_ERROR_CATALOG_VALIDATOR_FAILED` code was found.

## Verification state

- Clean continuation baseline: **998/998 GREEN, zero compiler warnings**.
- Loader and normalizer boundaries are complete for the current scope.
- Validator null-result behavior is already covered.
- No validator ordinary-exception contract has been added yet at this checkpoint.

## Next recommended step

Add one focused ordinary-exception contract for `IErrorCatalogValidator.Validate(...)` before changing production code.

If RED confirms raw validator exception leakage, add the smallest guard around only `_validator.Validate(...)`, excluding `OperationCanceledException` so exact-instance cancellation can be tested separately afterward.

Keep changes small, tested, documented here, and committed directly to `master`.
