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
- `IErrorCatalogDocumentNormalizer.Normalize(...)` ordinary-exception normalization is locally verified GREEN.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 997/997 tests with zero compiler warnings** before the new normalizer cancellation contract.
- A focused exact-instance cancellation contract is now committed for `IErrorCatalogDocumentNormalizer.Normalize(...)` and awaits local verification.

## 2026-09-10 — normalizer cancellation contract

Contract commit: `35e92a49afdee0f98c7685387ca88e66715cc165`

Updated:

`WhenItFails.Tests/Catalog/ErrorCatalogProviderNormalizerExceptionContractTests.cs`

Added:

`LoadFromFileAsync_WhenNormalizerCancels_RethrowsSameOperationCanceledException`

Contract:

```text
IErrorCatalogDocumentNormalizer.Normalize(...)
    => throws a specific OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)`, so normalizer cancellation cannot be wrapped, replaced, or converted into `WIF_ERROR_CATALOG_NORMALIZER_FAILED`.

The validator and factory fixtures still throw if reached, so the test also locks short-circuit behavior after normalizer cancellation.

No production code changed. The normalizer exception guard excludes `OperationCanceledException`, so this focused contract is expected to be GREEN.

## 2026-09-10 — 997/997 GREEN normalizer exception checkpoint

Checkpoint commit: `496bf5dbd9905b7ff6342ae0fc5a57246f410179`

Normalizer production fix commit: `ee06db4fa33b1e8f8c4cacef45448bb40be5d221`.
Normalizer ordinary-exception contract commit: `7c019211b48edc611f293ac49624a3ad4cfdd0b4`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 997
Skipped:  0
Total:  997
Compiler warnings: 0
```

`IErrorCatalogDocumentNormalizer.Normalize(...)` ordinary exceptions normalize to:

```text
Status: Failed
Data: null
Code: WIF_ERROR_CATALOG_NORMALIZER_FAILED
Message: The error catalog document normalizer failed.
```

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
2. `IErrorCatalogDocumentNormalizer.Normalize(...)` — ordinary exception verified; exact-instance cancellation awaiting GREEN.
3. `IErrorCatalogValidator.Validate(...)`.
4. `IErrorCatalogFactory.Create(...)`.

Existing malformed-result handling:

- null loader response → `WIF_ERROR_CATALOG_LOADER_RESPONSE_NULL`;
- null normalizer result → `WIF_ERROR_CATALOG_NORMALIZER_RESULT_NULL`;
- null validator result → `WIF_ERROR_CATALOG_VALIDATOR_RESULT_NULL`;
- null factory result → `WIF_ERROR_CATALOG_FACTORY_RESULT_NULL`.

Repository reconnaissance found no existing `ErrorCatalogProvider` propagation/shape contract requiring normalizer exceptions to escape unchanged.

## Verification state

- Clean continuation baseline: **997/997 GREEN, zero compiler warnings**.
- Normalizer null-result and ordinary-exception behavior are covered and locally verified.
- Exact-instance cancellation originating from the normalizer dependency is committed and awaits focused local verification.
- No production change is expected for the cancellation contract.
- Expected complete-suite count after the contract passes: **998 tests**.

## Recommended verification

Pull current `master` and run the focused normalizer cancellation contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadFromFileAsync_WhenNormalizerCancels_RethrowsSameOperationCanceledException"
```

Expected result: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **998/998 GREEN with zero compiler warnings**.

## Next recommended step

After **998/998 GREEN** is confirmed, consider `IErrorCatalogDocumentNormalizer.Normalize(...)` complete for the current scope.

Then inspect `IErrorCatalogValidator.Validate(...)` separately, checking existing propagation/shape contracts before adding any new RED test.

Keep changes small, tested, documented here, and committed directly to `master`.
