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
- Both `ErrorCatalogRuntime` `_contextStore.Set(...)` invocation sites are complete for the current scope.
- `ErrorCatalogInitializer` store-write ordinary-exception behavior is locally verified GREEN.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 989/989 tests with zero compiler warnings**.
- A focused exact-instance cancellation contract is now committed for the initializer `_contextStore.Set(...)` invocation and awaits local verification.

## Latest committed steps

### 2026-09-09 — initializer context-store Set cancellation contract

Contract commit: `68fc86b6c5ef6201c18f555825d33af3c5eeed6e`

Updated:

`WhenItFails.Tests/Initialization/ErrorCatalogInitializerContextStoreSetExceptionContractTests.cs`

Added:

`InitializeAsync_WhenContextStoreSetCancels_RethrowsSameOperationCanceledException`

Contract:

```text
IErrorCatalogContextStore.Set(...)
    => throws a specific OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The fixture supplies successful bootstrap and context-provider responses so the initializer reaches exactly the store-write boundary. The test uses `Assert.Same(...)`, so cancellation cannot be wrapped, replaced, or normalized into `WIF_CONTEXT_STORE_FAILED`.

The test diff was checked and contains only the new cancellation contract and its `CancelingSetContextStore` fixture.

No production code changed. The current initializer store-write catch filter excludes `OperationCanceledException`, so the focused contract is expected to be GREEN.

### 2026-09-09 — 989/989 GREEN initializer store-write checkpoint

Checkpoint commit: `f9fed4be649fba1bbffd07a42030f4252b96ba6f`

Production fix commit: `6e18b9ed39a327d241e007dbda69bc81a508e3f5`

Ordinary-exception contract commit: `71ee95f3621f13eed1cd01f0bdcc0767b98d1754`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 989
Skipped:  0
Total:  989
Compiler warnings: 0
```

The initializer store-write ordinary exception is converted to `WIF_CONTEXT_STORE_FAILED` without exposing raw dependency text.

## Verification state

- Clean continuation baseline: **989/989 GREEN, zero compiler warnings**.
- Both `ErrorCatalogRuntime` store-write boundaries are complete.
- `ErrorCatalogInitializer` store-write ordinary-exception behavior is verified GREEN.
- Exact-instance cancellation contract for the initializer store-write invocation is committed and awaits focused local verification.
- No production change is expected for this cancellation contract.
- Expected complete-suite count after the contract passes: **990 tests**.

## Recommended verification

Pull current `master` and run the focused cancellation contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenContextStoreSetCancels_RethrowsSameOperationCanceledException"
```

Expected result: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **990/990 GREEN with zero compiler warnings**.

## Next recommended step

After 990/990 GREEN is confirmed, consider all currently known `IErrorCatalogContextStore.Set(...)` boundaries complete for the current scope.

Then inspect the next unguarded initializer dependency boundary separately. Start with `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` ordinary-exception behavior, observe RED, and only then add the smallest production boundary. Follow with exact-instance cancellation as a separate contract.

Do not combine bootstrapper and `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` hardening in the same step.

Keep changes small, tested, documented here, and committed directly to `master`.
