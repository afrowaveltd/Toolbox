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
- The first dependency, `IErrorCatalogProvider.LoadFromFileAsync(...)`, now has a focused ordinary-exception contract, a verified RED baseline, and a production exception guard awaiting local GREEN verification.

## Latest committed steps

### 2026-09-09 — error catalog provider ordinary-exception fix

Production fix commit: `1dc3957772b08334862e66eb89ff71e5086122ae`

Changed only the `_errorCatalogProvider.LoadFromFileAsync(...)` invocation inside `ErrorCatalogContextProvider.LoadFromJsonsAsync(...)`.

Ordinary exceptions are now converted to:

```text
Status: Failed
Data: null
Code: WIF_ERROR_CATALOG_PROVIDER_FAILED
Message: The error catalog provider failed.
```

The catch filter excludes `OperationCanceledException`, so cancellation is still intended to propagate unchanged.

The production diff was checked and contains only the intended first-provider exception guard. Existing null-response, failed-response, null-payload, issue aggregation, and all later provider calls remain unchanged.

### 2026-09-09 — verified RED error catalog provider exception contract

Contract commit: `6139e450047bafd828803bd7c154613beaf1f5c4`

Focused test:

`WhenItFails.Tests.Catalog.ErrorCatalogContextProviderErrorCatalogProviderExceptionContractTests.LoadFromJsonsAsync_WhenErrorCatalogProviderThrows_ReturnsStableFailure`

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
Sensitive error catalog provider detail must not escape.
```

The exception escaped directly from `_errorCatalogProvider.LoadFromFileAsync(...)` through `ErrorCatalogContextProvider.LoadFromJsonsAsync(...)`, confirming the missing first-provider exception boundary.

The remaining category/code-group/owner/profile provider fixtures throw `Unexpected ... call.` if reached, so the contract also locks short-circuit behavior after the first dependency failure.

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

- Clean continuation baseline before the first-provider exception contract: **994/994 GREEN, zero compiler warnings**.
- First-provider ordinary-exception contract is verified RED before the production fix.
- Production guard for `_errorCatalogProvider.LoadFromFileAsync(...)` is committed and awaits focused local GREEN verification.
- Expected complete-suite count after the new contract passes: **995 tests**.
- After 995/995 GREEN, add a separate exact-instance cancellation contract for the same first provider before moving to the category provider.

## Recommended verification

Pull current `master` and run only the first-provider exception contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadFromJsonsAsync_WhenErrorCatalogProviderThrows_ReturnsStableFailure"
```

Expected result after the production fix: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **995/995 GREEN with zero compiler warnings**.

## Next recommended step

After 995/995 GREEN is confirmed, add one focused exact-instance cancellation contract for `IErrorCatalogProvider.LoadFromFileAsync(...)`.

If that passes without production changes, consider the first internal provider boundary complete for the current scope and move separately to `IErrorCategoryCatalogProvider.LoadFromFileAsync(...)` ordinary-exception hardening.

Keep changes small, tested, documented here, and committed directly to `master`.
