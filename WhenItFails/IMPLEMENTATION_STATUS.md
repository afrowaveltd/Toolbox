# Implementation status

Last updated: 2026-09-09

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening initialization and catalog dependency boundaries against malformed behavior, raw exception leakage, and cancellation corruption.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service and profile-selection exception/cancellation boundaries are complete.
- All currently known `IErrorCatalogContextStore` read/write boundaries in the active runtime/initializer scope are complete.
- `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` is complete for the current scope.
- `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` as consumed by `ErrorCatalogInitializer` is complete for the current scope.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 994/994 tests with zero compiler warnings** before the new internal provider exception contract.
- `ErrorCatalogContextProvider` has five sequential internal provider dependencies whose null-response behavior is already covered.
- A focused ordinary-exception contract is now committed for the first dependency, `IErrorCatalogProvider.LoadFromFileAsync(...)`, and awaits local RED verification.

## Latest committed steps

### 2026-09-09 — error catalog provider ordinary-exception contract

Contract commit: `6139e450047bafd828803bd7c154613beaf1f5c4`

Added:

`WhenItFails.Tests/Catalog/ErrorCatalogContextProviderErrorCatalogProviderExceptionContractTests.cs`

Test:

`LoadFromJsonsAsync_WhenErrorCatalogProviderThrows_ReturnsStableFailure`

The first dependency throws:

```text
System.InvalidOperationException:
Sensitive error catalog provider detail must not escape.
```

Required stable contract:

```text
Status: Failed
Data: null
Code: WIF_ERROR_CATALOG_PROVIDER_FAILED
Message: The error catalog provider failed.
```

The remaining category/code-group/owner/profile provider fixtures throw `Unexpected ... call.` if reached, so the test also locks short-circuit behavior after the first dependency failure.

No production code changed. `_errorCatalogProvider.LoadFromFileAsync(...)` is currently awaited directly inside `ErrorCatalogContextProvider.LoadFromJsonsAsync(...)`, so the focused test is expected to be RED with the raw first-provider exception escaping.

### 2026-09-09 — 994/994 GREEN initializer context-provider checkpoint

Checkpoint commit: `5cc1d1378f8818973eb01104a91b1256c97e5349`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 994
Skipped:  0
Total:  994
Compiler warnings: 0
```

The initializer bootstrapper and initializer context-provider boundaries are complete for the current scope.

## Reconnaissance

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` invokes these dependencies in order:

1. `IErrorCatalogProvider.LoadFromFileAsync(...)`
2. `IErrorCategoryCatalogProvider.LoadFromFileAsync(...)`
3. `IErrorCodeGroupCatalogProvider.LoadFromFileAsync(...)`
4. `IErrorOwnerCatalogProvider.LoadFromFileAsync(...)`
5. `IErrorProfileCatalogProvider.LoadFromFileAsync(...)`

All five already have stable null-response handling (`WIF_*_PROVIDER_RESPONSE_NULL`). Ordinary exception/cancellation hardening is proceeding one provider at a time.

## Verification state

- Clean continuation baseline: **994/994 GREEN, zero compiler warnings**.
- New first-provider ordinary-exception contract is committed and awaits focused RED verification.
- Production code remains unchanged until RED is observed.
- Expected complete-suite count once the new contract eventually passes: **995 tests**.

## Recommended verification

Pull current `master` and run only the new contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadFromJsonsAsync_WhenErrorCatalogProviderThrows_ReturnsStableFailure"
```

Expected current result: RED with the raw exception text:

```text
Sensitive error catalog provider detail must not escape.
```

## Next recommended step

If RED is confirmed, add the smallest exception boundary around only `_errorCatalogProvider.LoadFromFileAsync(...)` inside `ErrorCatalogContextProvider.LoadFromJsonsAsync(...)`.

Convert ordinary exceptions into `WIF_ERROR_CATALOG_PROVIDER_FAILED` while allowing `OperationCanceledException` to propagate unchanged.

After focused and full GREEN, add a separate exact-instance cancellation contract for the same first provider before moving to the category provider.

Keep changes small, tested, documented here, and committed directly to `master`.
