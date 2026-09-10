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
- `IErrorCatalogLoader.LoadFromFileAsync(...)` is complete for the current scope.
- `IErrorCatalogDocumentNormalizer.Normalize(...)` is complete for the current scope.
- `IErrorCatalogValidator.Validate(...)` ordinary-exception normalization is locally verified GREEN.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 999/999 tests with zero compiler warnings** before the new validator cancellation contract.
- A focused exact-instance cancellation contract is now committed for `IErrorCatalogValidator.Validate(...)` and awaits local verification.

## 2026-09-10 — validator cancellation contract

Contract commit: `0ab95ed05c70d563289742dc9c474490aa3018ea`

Updated:

`WhenItFails.Tests/Catalog/ErrorCatalogProviderValidatorExceptionContractTests.cs`

Added:

`LoadFromFileAsync_WhenValidatorCancels_RethrowsSameOperationCanceledException`

Contract:

```text
IErrorCatalogValidator.Validate(...)
    => throws a specific OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)`, so validator cancellation cannot be wrapped, replaced, or converted into `WIF_ERROR_CATALOG_VALIDATOR_FAILED`.

The factory fixture throws if reached, so the test also locks short-circuit behavior after validator cancellation.

No production code changed. The validator exception guard excludes `OperationCanceledException`, so this focused contract is expected to be GREEN.

## 2026-09-10 — 999/999 GREEN validator exception checkpoint

Checkpoint commit: `fbd92f310ec454fa1dd4140c94a524b3c56df588`.
Validator production fix commit: `994d0a5e1ec417648f7be1b21fff5e98c2101ac9`.
Validator ordinary-exception contract commit: `69fdba1625808a96e00fe823d57a49878a0ec36c`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 999
Skipped:  0
Total:  999
Compiler warnings: 0
```

`IErrorCatalogValidator.Validate(...)` ordinary exceptions normalize to:

```text
Status: Failed
Data: null
Code: WIF_ERROR_CATALOG_VALIDATOR_FAILED
Message: The error catalog validator failed.
```

## 2026-09-10 — 998/998 GREEN normalizer boundary checkpoint

Checkpoint commit: `75276b1091ca399047a3264e6d8bb5f8e475d8e0`.
Normalizer cancellation contract commit: `35e92a49afdee0f98c7685387ca88e66715cc165`.
Normalizer production fix commit: `ee06db4fa33b1e8f8c4cacef45448bb40be5d221`.
Normalizer ordinary-exception contract commit: `7c019211b48edc611f293ac49624a3ad4cfdd0b4`.

`IErrorCatalogDocumentNormalizer.Normalize(...)` is complete for the current scope. Ordinary exceptions normalize to `WIF_ERROR_CATALOG_NORMALIZER_FAILED`; exact-instance cancellation propagates unchanged.

## 2026-09-10 — 996/996 GREEN loader boundary checkpoint

Checkpoint commit: `2d923b51a70105e02240333f9b8ee956c1a6006b`.
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
3. `IErrorCatalogValidator.Validate(...)` — ordinary exception verified; exact-instance cancellation awaiting GREEN.
4. `IErrorCatalogFactory.Create(...)`.

Existing malformed-result handling:

- null loader response → `WIF_ERROR_CATALOG_LOADER_RESPONSE_NULL`;
- null normalizer result → `WIF_ERROR_CATALOG_NORMALIZER_RESULT_NULL`;
- null validator result → `WIF_ERROR_CATALOG_VALIDATOR_RESULT_NULL`;
- null factory result → `WIF_ERROR_CATALOG_FACTORY_RESULT_NULL`.

Repository reconnaissance found no existing `ErrorCatalogProvider` propagation/shape contract requiring validator exceptions to escape unchanged.

## Verification state

- Clean continuation baseline: **999/999 GREEN, zero compiler warnings**.
- Validator null-result and ordinary-exception behavior are covered and locally verified.
- Exact-instance cancellation originating from the validator dependency is committed and awaits focused local verification.
- No production change is expected for the cancellation contract.
- Expected complete-suite count after the contract passes: **1000 tests**.

## Recommended verification

Pull current `master` and run the focused validator cancellation contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadFromFileAsync_WhenValidatorCancels_RethrowsSameOperationCanceledException"
```

Expected result: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **1000/1000 GREEN with zero compiler warnings**.

## Next recommended step

After **1000/1000 GREEN** is confirmed, consider `IErrorCatalogValidator.Validate(...)` complete for the current scope.

Then inspect `IErrorCatalogFactory.Create(...)` separately. Before adding a RED test, search existing `ErrorCatalogProvider` propagation/shape contracts for factory exceptions; only add normalization behavior if no established pass-through contract exists.

Keep changes small, tested, documented here, and committed directly to `master`.
