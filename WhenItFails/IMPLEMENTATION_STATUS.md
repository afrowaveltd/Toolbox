# Implementation status

Last updated: 2026-09-09

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening initialization dependency boundaries against malformed behavior, raw exception leakage, and cancellation corruption.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service and profile-selection exception/cancellation boundaries are complete.
- `IErrorCatalogContextStore.GetCurrent()` null-response, ordinary-exception, and exact-instance cancellation behavior are complete.
- `IErrorCatalogInitializer.InitializeAsync(...)` as consumed by `ErrorCatalogRuntime` has null-response, ordinary-exception, and exact-instance cancellation contracts.
- `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` is hardened for both explicit `ResetToDefaultsAsync()` and flexible initialization fallback paths.
- All currently known `IErrorCatalogContextStore.Set(...)` invocation sites are complete for the current scope: explicit reset, flexible fallback, and `ErrorCatalogInitializer` each have ordinary-exception and exact-instance cancellation contracts.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 990/990 tests with zero compiler warnings**.
- The next distinct unguarded initializer dependency boundary is `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` ordinary-exception behavior.

## Latest committed steps

### 2026-09-09 — 990/990 GREEN initializer store-write checkpoint

Checkpoint commit: pending current commit.

Cancellation contract commit: `68fc86b6c5ef6201c18f555825d33af3c5eeed6e`

Production fix commit: `6e18b9ed39a327d241e007dbda69bc81a508e3f5`

Ordinary-exception contract commit: `71ee95f3621f13eed1cd01f0bdcc0767b98d1754`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 990
Skipped:  0
Total:  990
Compiler warnings: 0
```

The exact original `OperationCanceledException` instance thrown by the initializer context-store `Set(...)` invocation propagates unchanged. Together with the verified ordinary-exception behavior, this completes the initializer store-write boundary and all currently known context-store write invocation sites for the current scope.

## Verification state

- Clean continuation baseline: **990/990 GREEN, zero compiler warnings**.
- All currently known context-store read/write dependency boundaries in the active runtime/initializer scope are complete.
- `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` already has a null-response contract (`WIF_INITIALIZER_BOOTSTRAPPER_RESPONSE_NULL`) but does not yet have a focused ordinary-exception boundary contract at the initializer call site.

## Recommended next step

Add one focused contract for `ErrorCatalogInitializer.InitializeAsync(...)` when `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` throws an ordinary exception.

Expected stable initializer contract:

```text
Status: Failed
Data: null
Code: WIF_INITIALIZER_BOOTSTRAPPER_FAILED
Message: The JSON workspace bootstrapper failed.
```

The raw bootstrapper exception text must not escape. The context provider must not be called, and any existing context-store state must remain unchanged.

Observe RED before changing production code. Then add only the smallest exception boundary around the bootstrapper invocation while allowing `OperationCanceledException` to propagate unchanged.

After ordinary-exception behavior is GREEN, add exact-instance bootstrapper cancellation as a separate contract. Do not combine this with `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` hardening.

Keep changes small, tested, documented here, and committed directly to `master`.
