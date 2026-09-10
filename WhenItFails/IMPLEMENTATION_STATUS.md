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
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 998/998 tests with zero compiler warnings** before the new validator exception contract.
- A focused ordinary-exception contract is now committed for `IErrorCatalogValidator.Validate(...)` and awaits local RED verification.

## 2026-09-10 — validator ordinary-exception contract

Contract commit: `69fdba1625808a96e00fe823d57a49878a0ec36c`

Added:

`WhenItFails.Tests/Catalog/ErrorCatalogProviderValidatorExceptionContractTests.cs`

Test:

`LoadFromFileAsync_WhenValidatorThrows_ReturnsStableFailure`

The validator throws:

```text
System.InvalidOperationException:
Sensitive error catalog validator detail must not escape.
```

Required stable `ErrorCatalogProvider` contract:

```text
Status: Failed
Data: null
Code: WIF_ERROR_CATALOG_VALIDATOR_FAILED
Message: The error catalog validator failed.
```

The factory fixture throws if reached, so the contract also locks short-circuit behavior after validator failure.

No production code changed. `_validator.Validate(...)` is currently invoked directly, so the focused test is expected to be RED with the raw validator exception escaping.

## 2026-09-10 — 998/998 GREEN normalizer boundary checkpoint

Checkpoint commit: `75276b1091ca399047a3264e6d8bb5f8e475d8e0`.
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

`IErrorCatalogDocumentNormalizer.Normalize(...)` ordinary exceptions normalize to `WIF_ERROR_CATALOG_NORMALIZER_FAILED`; exact-instance cancellation propagates unchanged.

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
3. `IErrorCatalogValidator.Validate(...)` — ordinary-exception contract awaiting RED.
4. `IErrorCatalogFactory.Create(...)`.

Existing malformed-result handling:

- null loader response → `WIF_ERROR_CATALOG_LOADER_RESPONSE_NULL`;
- null normalizer result → `WIF_ERROR_CATALOG_NORMALIZER_RESULT_NULL`;
- null validator result → `WIF_ERROR_CATALOG_VALIDATOR_RESULT_NULL`;
- null factory result → `WIF_ERROR_CATALOG_FACTORY_RESULT_NULL`.

Repository reconnaissance found no existing `ErrorCatalogProvider` propagation/shape contract requiring validator exceptions to escape unchanged. No existing `WIF_ERROR_CATALOG_VALIDATOR_FAILED` code was found before this contract was introduced.

## Verification state

- Clean continuation baseline before the validator test: **998/998 GREEN, zero compiler warnings**.
- Validator null-result behavior is already covered.
- New validator ordinary-exception contract is committed and awaits focused RED verification.
- Production validator call remains unchanged until RED is observed.
- Expected complete-suite count once the new contract eventually passes: **999 tests**.

## Recommended verification

Pull current `master` and run only the new contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadFromFileAsync_WhenValidatorThrows_ReturnsStableFailure"
```

Expected current result: RED with the raw exception text:

```text
Sensitive error catalog validator detail must not escape.
```

## Next recommended step

If RED is confirmed, add the smallest ordinary-exception guard around only `_validator.Validate(...)`, excluding `OperationCanceledException` so exact-instance cancellation can be tested separately afterward.

Keep changes small, tested, documented here, and committed directly to `master`.
