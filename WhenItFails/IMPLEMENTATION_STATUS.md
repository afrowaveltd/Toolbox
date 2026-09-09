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
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 995/995 tests with zero compiler warnings** before the new loader cancellation contract.
- A focused exact-instance cancellation contract is now committed for `IErrorCatalogLoader.LoadFromFileAsync(...)` and awaits local verification.

## 2026-09-09 — error catalog loader cancellation contract

Contract commit: `6586cf4e41de535421faed2bcfab68faad16c946`

Updated:

`WhenItFails.Tests/Catalog/ErrorCatalogProviderLoaderExceptionContractTests.cs`

Added:

`LoadFromFileAsync_WhenLoaderCancels_RethrowsSameOperationCanceledException`

Contract:

```text
IErrorCatalogLoader.LoadFromFileAsync(...)
    => faults with a specific OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)`, so loader cancellation cannot be wrapped, replaced, or converted into `WIF_ERROR_CATALOG_LOADER_FAILED`.

The normalizer, validator, and factory fixtures still throw if reached, so the test also requires short-circuit behavior after loader cancellation.

No production code changed. The loader exception guard excludes `OperationCanceledException`, so the focused contract is expected to be GREEN.

## 2026-09-09 — 995/995 GREEN loader exception checkpoint

Checkpoint commit: `32428ba38b491d419088af2e545608e39ab31554`

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

`IErrorCatalogLoader.LoadFromFileAsync(...)` ordinary exceptions are converted to:

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
- Loader null-response and ordinary-exception behavior are covered and locally verified.
- Exact-instance cancellation originating from the loader dependency is committed and awaits focused local verification.
- No production change is expected for the cancellation contract.
- Expected complete-suite count after the contract passes: **996 tests**.

## Recommended verification

Pull current `master` and run the focused loader cancellation contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadFromFileAsync_WhenLoaderCancels_RethrowsSameOperationCanceledException"
```

Expected result: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **996/996 GREEN with zero compiler warnings**.

## Next recommended step

After **996/996 GREEN** is confirmed, consider the `IErrorCatalogLoader` boundary complete for the current scope.

Then inspect the `IErrorCatalogDocumentNormalizer.Normalize(...)` exception boundary separately. Before adding a RED test, search existing `ErrorCatalogProvider` propagation/shape contracts for normalizer exceptions; only add normalization behavior if no established pass-through contract exists.

Keep changes small, tested, documented here, and committed directly to `master`.
