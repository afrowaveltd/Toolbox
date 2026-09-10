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
- `IErrorCatalogValidator.Validate(...)` is complete for the current scope.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1000/1000 tests with zero compiler warnings** before the new factory exception contract.
- A focused ordinary-exception contract is now committed for `IErrorCatalogFactory.Create(...)` and awaits local RED verification.

## 2026-09-10 — factory ordinary-exception contract

Contract commit: `da315a5d826177cdca2c3e773a42bd2f7b84db4a`

Added:

`WhenItFails.Tests/Catalog/ErrorCatalogProviderFactoryExceptionContractTests.cs`

Test:

`LoadFromFileAsync_WhenFactoryThrows_ReturnsStableFailure`

The factory throws:

```text
System.InvalidOperationException:
Sensitive error catalog factory detail must not escape.
```

Required stable `ErrorCatalogProvider` contract:

```text
Status: Failed
Data: null
Code: WIF_ERROR_CATALOG_FACTORY_FAILED
Message: The error catalog factory failed.
```

No production code changed. `_factory.Create(...)` is currently invoked directly, so the focused test is expected to be RED with the raw factory exception escaping.

## 2026-09-10 — 1000/1000 GREEN validator boundary checkpoint

Checkpoint commit: `7051166407792897fcd80166257bb83267e28d25`.
Validator cancellation contract commit: `0ab95ed05c70d563289742dc9c474490aa3018ea`.
Validator production fix commit: `994d0a5e1ec417648f7be1b21fff5e98c2101ac9`.
Validator ordinary-exception contract commit: `69fdba1625808a96e00fe823d57a49878a0ec36c`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1000
Skipped:  0
Total:  1000
Compiler warnings: 0
```

`IErrorCatalogValidator.Validate(...)` is complete for the current scope. Ordinary exceptions normalize to `WIF_ERROR_CATALOG_VALIDATOR_FAILED`; exact-instance cancellation propagates unchanged.

## Previous completed boundaries

- `IErrorCatalogDocumentNormalizer.Normalize(...)` — complete; ordinary exceptions normalize to `WIF_ERROR_CATALOG_NORMALIZER_FAILED`, exact cancellation propagates unchanged.
- `IErrorCatalogLoader.LoadFromFileAsync(...)` — complete; ordinary exceptions normalize to `WIF_ERROR_CATALOG_LOADER_FAILED`, exact cancellation propagates unchanged.

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
3. `IErrorCatalogValidator.Validate(...)` — complete for current scope.
4. `IErrorCatalogFactory.Create(...)` — ordinary-exception contract awaiting RED.

Existing malformed-result handling:

- null loader response → `WIF_ERROR_CATALOG_LOADER_RESPONSE_NULL`;
- null normalizer result → `WIF_ERROR_CATALOG_NORMALIZER_RESULT_NULL`;
- null validator result → `WIF_ERROR_CATALOG_VALIDATOR_RESULT_NULL`;
- null factory result → `WIF_ERROR_CATALOG_FACTORY_RESULT_NULL`.

Repository reconnaissance found no existing `ErrorCatalogProvider` propagation/shape contract requiring factory exceptions to escape unchanged. No existing `WIF_ERROR_CATALOG_FACTORY_FAILED` code was found before this contract was introduced.

## Verification state

- Clean continuation baseline before the factory test: **1000/1000 GREEN, zero compiler warnings**.
- Factory null-result behavior is already covered.
- New factory ordinary-exception contract is committed and awaits focused RED verification.
- Production factory call remains unchanged until RED is observed.
- Expected complete-suite count once the new contract eventually passes: **1001 tests**.

## Recommended verification

Pull current `master` and run only the new contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadFromFileAsync_WhenFactoryThrows_ReturnsStableFailure"
```

Expected current result: RED with the raw exception text:

```text
Sensitive error catalog factory detail must not escape.
```

## Next recommended step

If RED is confirmed, add the smallest ordinary-exception guard around only `_factory.Create(...)`, excluding `OperationCanceledException` so exact-instance cancellation can be tested separately afterward.

Keep changes small, tested, documented here, and committed directly to `master`.
