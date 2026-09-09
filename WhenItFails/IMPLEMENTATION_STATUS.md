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
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 988/988 tests with zero compiler warnings**.
- The remaining distinct store-write boundary is `_contextStore.Set(...)` inside `ErrorCatalogInitializer.InitializeAsync(...)`.

## Latest verified checkpoint

### 2026-09-09 — 988/988 GREEN runtime store-write checkpoint

Checkpoint commit records local verification after the flexible-fallback store-write cancellation contract.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 988
Skipped:  0
Total:  988
Compiler warnings: 0
```

The exact original `OperationCanceledException` instance thrown by `_contextStore.Set(...)` in the flexible fallback path propagates unchanged through `ErrorCatalogRuntime.InitializeAsync(...)`.

Together with the previously verified ordinary-exception behavior, both runtime store-write invocation sites are now complete for the current scope.

Relevant commits immediately preceding this checkpoint:

- flexible-fallback ordinary-exception fix: `30d67bfb5b7e2dffdbed950ae39558dd6eeed330`
- flexible-fallback cancellation contract: `7dc40fbc83a7bc3d2eb9791ca76e96d85e57e954`

## Next recommended step

Inspect and harden the separate `_contextStore.Set(...)` call inside `ErrorCatalogInitializer.InitializeAsync(...)`.

Start test-first with one focused ordinary-exception contract. After successful bootstrap and context loading, make the context store throw an ordinary exception from `Set(...)` and require the initializer to return:

```text
Status: Failed
Data: null
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

The raw store exception text must not escape.

Observe RED before changing production code. Then add only the smallest guard around this initializer store-write call. Follow with a separate exact-instance `OperationCanceledException` contract.

Avoid broader refactoring or touching bootstrapper/context-provider exception behavior in the same step.