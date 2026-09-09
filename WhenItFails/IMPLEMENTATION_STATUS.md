# Implementation status

Last updated: 2026-09-09

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening initialization and runtime dependency boundaries against malformed behavior, raw exception leakage, and cancellation corruption.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service and profile-selection exception/cancellation boundaries are complete.
- `IErrorCatalogContextStore.GetCurrent()` null-response, ordinary-exception, and exact-instance cancellation behavior are complete.
- `IErrorCatalogInitializer.InitializeAsync(...)` as consumed by `ErrorCatalogRuntime` has null-response, ordinary-exception, and exact-instance cancellation contracts.
- `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` is hardened for both explicit `ResetToDefaultsAsync()` and flexible initialization fallback paths.
- Both `ErrorCatalogRuntime` `_contextStore.Set(...)` invocation sites are complete for the current scope: explicit reset and flexible fallback each have ordinary-exception and exact-instance cancellation contracts.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 988/988 tests with zero compiler warnings** before the initializer store-write contract.
- The initializer `_contextStore.Set(...)` ordinary-exception contract is verified RED before the production fix.
- `ErrorCatalogInitializer.InitializeAsync(...)` now converts ordinary context-store write exceptions into `WIF_CONTEXT_STORE_FAILED` without exposing raw dependency text.

## Latest committed steps

### 2026-09-09 — initializer context-store Set exception fix

Production fix commit: `6e18b9ed39a327d241e007dbda69bc81a508e3f5`

Changed only `_contextStore.Set(contextResponse.Data)` inside `ErrorCatalogInitializer.InitializeAsync(...)`.

Ordinary exceptions are now converted to:

```text
Status: Failed
Data: null
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

The catch filter excludes `OperationCanceledException`, so cancellation is still intended to propagate unchanged.

The production diff was checked and contains only the intended initializer store-write guard.

### 2026-09-09 — verified RED initializer context-store Set contract

Contract commit: `71ee95f3621f13eed1cd01f0bdcc0767b98d1754`

Focused test:

`WhenItFails.Tests/Initialization/ErrorCatalogInitializerContextStoreSetExceptionContractTests.InitializeAsync_WhenContextStoreSetThrows_ReturnsStableFailure`

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
Sensitive initializer context store Set detail must not escape.
```

The exception escaped directly from `ThrowingSetContextStore.Set(...)` through `ErrorCatalogInitializer.InitializeAsync(...)`, confirming the missing initializer store-write boundary.

### 2026-09-09 — 988/988 GREEN runtime store-write checkpoint

Checkpoint commit: `522d9b54326f51c7eb11ca1c8bc34bc17b948395`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 988
Skipped:  0
Total:  988
Compiler warnings: 0
```

Both runtime store-write invocation sites are complete for the current scope.

## Verification state

- Clean continuation baseline before the initializer store-write contract: **988/988 GREEN, zero compiler warnings**.
- Both `ErrorCatalogRuntime` store-write boundaries are complete.
- Initializer store-write ordinary-exception contract is verified RED before the production fix.
- Production initializer store-write guard is committed and awaits focused local GREEN verification.
- Expected complete-suite count after the new contract passes: **989 tests**.

## Recommended verification

Pull current `master` and run the focused contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenContextStoreSetThrows_ReturnsStableFailure"
```

Expected result after the production fix: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **989/989 GREEN with zero compiler warnings**.

## Next recommended step

After 989/989 GREEN is confirmed, add one focused exact-instance cancellation contract for the same initializer `_contextStore.Set(...)` invocation.

If that passes without production changes, consider all currently known context-store write boundaries complete for the current scope. Then inspect the next unguarded initializer dependency boundary separately; likely candidates are bootstrapper and context-provider ordinary exception behavior. Do not combine those in one step.

Keep changes small, tested, documented here, and committed directly to `master`.
