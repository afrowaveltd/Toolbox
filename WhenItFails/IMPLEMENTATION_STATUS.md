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
- `ErrorCatalogContextProvider` is intentionally a transparent orchestration boundary for exceptions thrown by its five internal catalog providers; do not normalize them there.
- `ErrorCatalogProvider` is the current normalization boundary under audit.
- The loader ordinary-exception contract and production guard are locally verified GREEN.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 995/995 tests with zero compiler warnings**.

## 2026-09-09 — 995/995 GREEN loader exception checkpoint

Checkpoint commit: this status commit.

Production fix commit: `49ab1d418217d3745f256b347efee379fe374934`

Ordinary-exception contract commit: `67a0650343d0af60f975e98aa6fcf3b5d387dd27`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 995
Skipped:  0
Total:  995
Compiler warnings: 0
```

`IErrorCatalogLoader.LoadFromFileAsync(...)` ordinary exceptions are now converted to:

```text
Status: Failed
Data: null
Code: WIF_ERROR_CATALOG_LOADER_FAILED
Message: The error catalog loader failed.
```

The loader failure short-circuits the normalizer, validator, and factory.

## Established transparent boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` must preserve exceptions from its five provider dependencies.

Pre-existing tests require preservation of synchronous and faulted-task exception identity, exception type, inner exception references, `Exception.Data`, custom properties, cancellation information, and short-circuit behavior.

Relevant suites:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`

## `ErrorCatalogProvider` boundary reconnaissance

`ErrorCatalogProvider.LoadFromFileAsync(...)` composes:

1. `IErrorCatalogLoader.LoadFromFileAsync(...)`
2. `IErrorCatalogDocumentNormalizer.Normalize(...)`
3. `IErrorCatalogValidator.Validate(...)`
4. `IErrorCatalogFactory.Create(...)`

It converts malformed dependency outputs into stable responses:

- null loader response → `WIF_ERROR_CATALOG_LOADER_RESPONSE_NULL`;
- null normalizer result → `WIF_ERROR_CATALOG_NORMALIZER_RESULT_NULL`;
- null validator result → `WIF_ERROR_CATALOG_VALIDATOR_RESULT_NULL`;
- null factory result → `WIF_ERROR_CATALOG_FACTORY_RESULT_NULL`.

The loader now also has stable ordinary-exception normalization. Its catch filter excludes `OperationCanceledException`.

## Verification state

- Clean continuation baseline: **995/995 GREEN, zero compiler warnings**.
- Loader null-response and ordinary-exception behavior are covered.
- Existing general cancellation test covers cancellation requested before provider execution.
- Exact-instance cancellation originating from the loader dependency itself is the next focused contract.
- No production change is expected for that cancellation contract because the loader catch filter excludes `OperationCanceledException`.
- Expected complete-suite count after one new cancellation contract passes: **996 tests**.

## Next recommended step

Add one focused exact-instance cancellation contract for `IErrorCatalogLoader.LoadFromFileAsync(...)`.

The contract must prove that a specific `OperationCanceledException` instance supplied by the loader propagates unchanged and that normalizer, validator, and factory are not invoked.

After focused and full GREEN, consider the loader boundary complete for the current scope and then inspect the normalizer exception boundary separately, again checking existing propagation/shape contracts before adding a RED test.

Keep changes small, tested, documented here, and committed directly to `master`.
