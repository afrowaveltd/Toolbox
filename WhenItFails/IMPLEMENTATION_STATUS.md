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
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 996/996 tests with zero compiler warnings** before the normalizer exception contract.
- The normalizer ordinary-exception contract is now verified RED and the smallest production guard is committed.

## 2026-09-10 — normalizer ordinary-exception fix

Production fix commit: `ee06db4fa33b1e8f8c4cacef45448bb40be5d221`

Changed only the `_normalizer.Normalize(...)` invocation inside `ErrorCatalogProvider.LoadFromFileAsync(...)`.

Ordinary exceptions are now converted to:

```text
Status: Failed
Data: null
Code: WIF_ERROR_CATALOG_NORMALIZER_FAILED
Message: The error catalog document normalizer failed.
```

The catch filter excludes `OperationCanceledException`, so cancellation originating from the normalizer continues to propagate unchanged.

The production commit diff was checked and contains only the intended normalizer exception guard. Existing null-result handling and validator/factory behavior remain unchanged.

## 2026-09-10 — verified RED normalizer contract

Contract commit: `7c019211b48edc611f293ac49624a3ad4cfdd0b4`

Focused test:

`WhenItFails.Tests.Catalog.ErrorCatalogProviderNormalizerExceptionContractTests.LoadFromFileAsync_WhenNormalizerThrows_ReturnsStableFailure`

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
Sensitive error catalog normalizer detail must not escape.
```

The exception escaped directly from `_normalizer.Normalize(...)` through `ErrorCatalogProvider.LoadFromFileAsync(...)`, confirming the missing normalizer exception boundary.

The validator and factory fixtures are configured to throw if reached, so the contract also requires short-circuit behavior after normalizer failure.

## 2026-09-10 — 996/996 GREEN loader boundary checkpoint

Checkpoint commit: `2d923b51a70105e02240333f9b8ee956c1a6006b`

Loader cancellation contract commit: `6586cf4e41de535421faed2bcfab68faad16c946`.
Loader production fix commit: `49ab1d418217d3745f256b347efee379fe374934`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 996
Skipped:  0
Total:  996
Compiler warnings: 0
```

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
2. `IErrorCatalogDocumentNormalizer.Normalize(...)` — ordinary-exception guard committed; awaiting GREEN verification.
3. `IErrorCatalogValidator.Validate(...)`.
4. `IErrorCatalogFactory.Create(...)`.

Existing malformed-result handling:

- null loader response → `WIF_ERROR_CATALOG_LOADER_RESPONSE_NULL`;
- null normalizer result → `WIF_ERROR_CATALOG_NORMALIZER_RESULT_NULL`;
- null validator result → `WIF_ERROR_CATALOG_VALIDATOR_RESULT_NULL`;
- null factory result → `WIF_ERROR_CATALOG_FACTORY_RESULT_NULL`.

Repository reconnaissance found no existing `ErrorCatalogProvider` propagation/shape contract requiring normalizer exceptions to escape unchanged.

## Verification state

- Clean continuation baseline before the normalizer test: **996/996 GREEN, zero compiler warnings**.
- Normalizer ordinary-exception contract is verified RED before the production fix.
- Production normalizer guard is committed and awaits focused local GREEN verification.
- Expected complete-suite count after the contract passes: **997 tests**.

## Recommended verification

Pull current `master` and run only the normalizer exception contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadFromFileAsync_WhenNormalizerThrows_ReturnsStableFailure"
```

Expected result after the production fix: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **997/997 GREEN with zero compiler warnings**.

## Next recommended step

After **997/997 GREEN** is confirmed, add a separate exact-instance cancellation contract for `IErrorCatalogDocumentNormalizer.Normalize(...)`.

If that passes without production changes, consider the normalizer boundary complete for the current scope and inspect `IErrorCatalogValidator.Validate(...)` separately, again checking existing propagation/shape contracts before adding a RED test.

Keep changes small, tested, documented here, and committed directly to `master`.
