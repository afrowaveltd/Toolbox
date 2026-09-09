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
- `ErrorCatalogContextProvider` is intentionally a transparent orchestration boundary for exceptions thrown by its five internal catalog providers; do not normalize them there.
- `ErrorCatalogProvider` is the current normalization boundary under audit.
- The focused loader ordinary-exception contract is verified RED and the smallest production guard is now committed.

## 2026-09-09 — error catalog loader ordinary-exception fix

Production fix commit: `49ab1d418217d3745f256b347efee379fe374934`

Changed only the `_loader.LoadFromFileAsync(...)` invocation inside `ErrorCatalogProvider.LoadFromFileAsync(...)`.

Ordinary exceptions are now converted to:

```text
Status: Failed
Data: null
Code: WIF_ERROR_CATALOG_LOADER_FAILED
Message: The error catalog loader failed.
```

The catch filter excludes `OperationCanceledException`, so cancellation continues to propagate unchanged.

The production commit diff was checked and contains only the intended loader exception guard. Existing null-response, failed-response, null-document, normalizer, validator, factory, and payload behavior remain unchanged.

## 2026-09-09 — verified RED error catalog loader contract

Contract commit: `67a0650343d0af60f975e98aa6fcf3b5d387dd27`

Focused test:

`WhenItFails.Tests.Catalog.ErrorCatalogProviderLoaderExceptionContractTests.LoadFromFileAsync_WhenLoaderThrows_ReturnsStableFailure`

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
Sensitive error catalog loader detail must not escape.
```

The exception escaped directly from `_loader.LoadFromFileAsync(...)` through `ErrorCatalogProvider.LoadFromFileAsync(...)`, confirming the missing loader exception boundary.

The normalizer, validator, and factory fixtures are configured to throw if reached, so the contract also requires short-circuit behavior after loader failure.

## 2026-09-09 — 994/994 GREEN recovery checkpoint

Checkpoint commit: `5fc6be65091af3bc4823772632463469e636d31f`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 994
Skipped:  0
Total:  994
Compiler warnings: 0
```

The `ErrorCatalogContextProvider` transparency recovery is complete and no earlier initializer/runtime hardening was reverted.

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

It already converts malformed dependency outputs into stable responses:

- null loader response → `WIF_ERROR_CATALOG_LOADER_RESPONSE_NULL`;
- null normalizer result → `WIF_ERROR_CATALOG_NORMALIZER_RESULT_NULL`;
- null validator result → `WIF_ERROR_CATALOG_VALIDATOR_RESULT_NULL`;
- null factory result → `WIF_ERROR_CATALOG_FACTORY_RESULT_NULL`.

Repository searches found no existing `ErrorCatalogProvider` contract requiring ordinary dependency exceptions to propagate unchanged.

## Verification state

- Clean baseline before the new contract: **994/994 GREEN, zero compiler warnings**.
- Loader ordinary-exception contract is verified RED before the production fix.
- Production loader guard is committed and awaits focused local GREEN verification.
- Expected complete-suite count after the new contract passes: **995 tests**.

## Recommended verification

Pull current `master` and run only the loader contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadFromFileAsync_WhenLoaderThrows_ReturnsStableFailure"
```

Expected result after the production fix: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **995/995 GREEN with zero compiler warnings**.

## Next recommended step

After **995/995 GREEN** is confirmed, add a separate exact-instance cancellation contract for `IErrorCatalogLoader.LoadFromFileAsync(...)`.

If that passes without production changes, consider the loader boundary complete for the current scope and then inspect the normalizer exception boundary separately, again checking existing propagation/shape contracts before adding a RED test.

Keep changes small, tested, documented here, and committed directly to `master`.
