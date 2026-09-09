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
- Reconnaissance has moved one layer down to `ErrorCatalogProvider`, whose malformed/null dependency outputs are already normalized into stable responses.
- A focused ordinary-exception contract is now committed for `IErrorCatalogLoader.LoadFromFileAsync(...)` and awaits local RED verification.

## 2026-09-09 — error catalog loader ordinary-exception contract

Contract commit: `67a0650343d0af60f975e98aa6fcf3b5d387dd27`

Added:

`WhenItFails.Tests/Catalog/ErrorCatalogProviderLoaderExceptionContractTests.cs`

Test:

`LoadFromFileAsync_WhenLoaderThrows_ReturnsStableFailure`

The loader returns a faulted task containing:

```text
System.InvalidOperationException:
Sensitive error catalog loader detail must not escape.
```

Required stable `ErrorCatalogProvider` contract:

```text
Status: Failed
Data: null
Code: WIF_ERROR_CATALOG_LOADER_FAILED
Message: The error catalog loader failed.
```

The raw loader exception text must not be exposed through the response.

The normalizer, validator, and factory fixtures throw if called, so the test also locks short-circuit behavior after loader failure.

No production code changed. `ErrorCatalogProvider.LoadFromFileAsync(...)` currently awaits `_loader.LoadFromFileAsync(...)` directly, so the focused contract is expected to be RED with the raw loader exception escaping.

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
- Loader ordinary-exception contract is committed and awaits focused RED verification.
- Production `ErrorCatalogProvider` remains unchanged until RED is observed.
- Expected complete-suite count after this contract eventually passes: **995 tests**.

## Recommended verification

Pull current `master` and run only the new loader contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadFromFileAsync_WhenLoaderThrows_ReturnsStableFailure"
```

Expected current result: RED with the original exception text:

```text
Sensitive error catalog loader detail must not escape.
```

## Next recommended step

If RED is confirmed, add the smallest exception boundary around only `_loader.LoadFromFileAsync(...)` inside `ErrorCatalogProvider.LoadFromFileAsync(...)`.

Convert ordinary exceptions into `WIF_ERROR_CATALOG_LOADER_FAILED` / `The error catalog loader failed.` while allowing `OperationCanceledException` to propagate unchanged.

After focused and full GREEN, add exact-instance cancellation as a separate loader contract before considering normalizer exception behavior.

Keep changes small, tested, documented here, and committed directly to `master`.
