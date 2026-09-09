# Implementation status

Last updated: 2026-09-09

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and service boundaries against malformed dependency behavior, raw exception leakage, and cancellation corruption.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service and profile-selection exception/cancellation boundaries are complete.
- `IErrorCatalogContextStore.GetCurrent()` null-response, ordinary-exception, and exact-instance cancellation behavior are complete.
- `IErrorCatalogInitializer.InitializeAsync(...)` null-response, ordinary-exception, and exact-instance cancellation behavior are complete.
- `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` is hardened for both explicit `ResetToDefaultsAsync()` and flexible initialization fallback paths.
- The explicit `ResetToDefaultsAsync()` `_contextStore.Set(...)` ordinary-exception and exact-instance cancellation boundary is complete for the current scope.
- The flexible-fallback `_contextStore.Set(...)` ordinary-exception behavior is locally verified GREEN.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 987/987 tests with zero compiler warnings**.
- A focused exact-instance cancellation contract is now committed for the flexible-fallback store-write boundary and awaits local verification.
- `ErrorCatalogInitializer` store-write behavior remains intentionally untouched.

## Latest committed steps

### 2026-09-09 — flexible fallback context-store Set cancellation contract

Contract commit: `7dc40fbc83a7bc3d2eb9791ca76e96d85e57e954`

Updated:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeContextStoreSetExceptionContractTests.cs`

Added:

`InitializeAsync_WhenFlexibleFallbackContextStoreSetCancels_RethrowsSameOperationCanceledException`

Contract:

```text
IErrorCatalogContextStore.Set(...)
    => throws a specific OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The fixture reports no previous context from `GetCurrent()`, allowing the failed configured initialization to enter the built-in flexible fallback path. The built-in provider returns a successful context, then `Set(...)` throws the specific cancellation instance.

The test uses `Assert.Same(...)`, so cancellation cannot be wrapped, replaced, normalized to `WIF_CONTEXT_STORE_FAILED`, or converted to `WIF_DEFAULT_FALLBACK_FAILED`.

The commit diff was checked and contains only the new cancellation contract and its `EmptyCancelingSetContextStore` fixture.

No production code changed. The current flexible-fallback store-write catch filter excludes `OperationCanceledException`, so the focused contract is expected to be GREEN.

### 2026-09-09 — 987/987 GREEN flexible fallback store-write checkpoint

Checkpoint commit: `e178daa9d5a9026ea422b501b9308ad8fe1fd6c0`

Production fix commit: `30d67bfb5b7e2dffdbed950ae39558dd6eeed330`

Ordinary-exception contract commit: `c0d2f39998cd8f316d75bce18dbf4e8079ff2475`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 987
Skipped:  0
Total:  987
Compiler warnings: 0
```

The flexible-fallback store-write ordinary exception is normalized to `WIF_CONTEXT_STORE_FAILED` inside the established `WIF_DEFAULT_FALLBACK_FAILED` wrapper and does not leak raw dependency text.

## Verification state

- Clean continuation baseline: **987/987 GREEN, zero compiler warnings**.
- Explicit reset store-write boundary is complete for the current scope.
- Flexible-fallback store-write ordinary-exception behavior is verified GREEN.
- Flexible-fallback exact-instance cancellation contract is committed and awaits focused local verification.
- No production change is expected for this cancellation contract.
- Expected complete-suite count after the contract passes: **988 tests**.

## Recommended verification

Pull current `master` and run the focused cancellation contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenFlexibleFallbackContextStoreSetCancels_RethrowsSameOperationCanceledException"
```

Expected result: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **988/988 GREEN with zero compiler warnings**.

## Next recommended step

After 988/988 GREEN is confirmed, consider both `ErrorCatalogRuntime` `_contextStore.Set(...)` invocation sites complete for the current scope.

Then inspect the separate `_contextStore.Set(...)` inside `ErrorCatalogInitializer`. Start with one focused ordinary-exception contract, observe RED, and only then add the smallest production boundary. Follow with exact-instance cancellation as a separate contract.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.