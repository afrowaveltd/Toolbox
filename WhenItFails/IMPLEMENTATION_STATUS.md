# Implementation status

Last updated: 2026-09-08

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and service boundaries against malformed dependency behavior, raw exception leakage, and cancellation corruption.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service and profile-selection exception/cancellation boundaries are complete.
- `IErrorCatalogContextStore.GetCurrent()` null-response, ordinary-exception, and exact-instance cancellation behavior are complete.
- `IErrorCatalogInitializer.InitializeAsync(...)` null-response, ordinary-exception, and exact-instance cancellation behavior are complete.
- `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` is hardened for both explicit `ResetToDefaultsAsync()` and flexible initialization fallback paths, including null response, ordinary exception, and exact-instance cancellation contracts.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 984/984 tests with zero compiler warnings** before the new store-write contract.
- `_contextStore.Set(...)` remains a distinct dependency boundary at three invocation sites: `ResetToDefaultsAsync()`, flexible fallback in `ErrorCatalogRuntime`, and `ErrorCatalogInitializer`.
- The explicit `ResetToDefaultsAsync()` store-write ordinary-exception contract is verified RED before the production fix.
- `ResetToDefaultsAsync()` now converts ordinary context-store `Set(...)` exceptions into `WIF_CONTEXT_STORE_FAILED` without exposing raw dependency text.

## Latest committed steps

### 2026-09-08 — ResetToDefaults context-store Set exception fix

Production fix commit: `dc5eb34f862df31736eff5f0726d090598ff55f9`

Changed only the `_contextStore.Set(...)` invocation inside `ResetToDefaultsAsync()`.

The store write is now wrapped in a narrow exception boundary:

```text
IErrorCatalogContextStore.Set(...)
    => ordinary exception
             ↓
Status: Failed
Data: null
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

The original store exception text is not copied into the public response.

The catch filter excludes `OperationCanceledException`, so cancellation is still intended to propagate unchanged.

The production diff was checked and contains only this one reset-path store-write guard. The separate flexible-fallback and initializer `Set(...)` sites remain unchanged.

### 2026-09-08 — verified RED ResetToDefaults context-store Set contract

Contract commit: `7f5b53a578e7b20ea613e474f9bb463387551373`

Focused test:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeContextStoreSetExceptionContractTests.ResetToDefaultsAsync_WhenContextStoreSetThrows_ReturnsStableFailure`

Observed locally on Windows before the production fix:

```text
Failed: 1
Passed: 0
Skipped: 0
Total: 1
```

Failure:

```text
System.InvalidOperationException:
Sensitive runtime context store Set detail must not escape.
```

The exception escaped directly from `ThrowingSetContextStore.Set(...)` through `ErrorCatalogRuntime.ResetToDefaultsAsync(...)`, confirming the missing runtime store-write boundary.

### 2026-09-08 — clean 984/984 checkpoint

Checkpoint commit: `6e1624db07efe583d2a4779c667ab978f9b9d3b6`

Verified locally after warning cleanup:

```text
WhenItFails.Tests
Failed:   0
Passed: 984
Skipped:  0
Total:  984
Compiler warnings: 0
```

## Verification state

- Clean continuation baseline before the new store-write contract: **984/984 GREEN, zero compiler warnings**.
- Built-in provider boundary is complete for the current scope.
- Explicit reset store-write ordinary-exception contract is verified RED before the production fix.
- Production `ResetToDefaultsAsync()` store-write guard is committed and awaits focused local GREEN verification.
- Expected complete-suite count after the new contract passes: **985 tests**.

## Recommended verification

Pull current `master` and run the focused contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResetToDefaultsAsync_WhenContextStoreSetThrows_ReturnsStableFailure"
```

Expected result after the production fix: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **985/985 GREEN with zero compiler warnings**.

## Next recommended step

After 985/985 GREEN is confirmed, add one focused exact-instance cancellation contract for the same `ResetToDefaultsAsync()` `_contextStore.Set(...)` invocation.

If that passes without production changes, consider the explicit reset store-write boundary complete for the current scope. Then move to the separate flexible-fallback `_contextStore.Set(...)` invocation, again ordinary exception first and cancellation second.

Do not modify `ErrorCatalogInitializer` in the same step.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.