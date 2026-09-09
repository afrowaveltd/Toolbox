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
- `ErrorCatalogInitializer.InitializeAsync(...)` now converts ordinary context-store write exceptions into `WIF_CONTEXT_STORE_FAILED` without exposing raw dependency text.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 989/989 tests with zero compiler warnings**.

## Latest committed steps

### 2026-09-09 — 989/989 GREEN initializer store-write checkpoint

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

The initializer store-write ordinary exception is converted to:

```text
Status: Failed
Data: null
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

without exposing the original `IErrorCatalogContextStore.Set(...)` exception text.

### 2026-09-09 — verified RED initializer context-store Set contract

Before the production fix, the focused contract failed with the original dependency exception:

```text
System.InvalidOperationException:
Sensitive initializer context store Set detail must not escape.
```

This confirmed the initializer store-write boundary independently from the two runtime store-write boundaries.

## Verification state

- Clean continuation baseline: **989/989 GREEN, zero compiler warnings**.
- Both `ErrorCatalogRuntime` store-write boundaries are complete.
- `ErrorCatalogInitializer` store-write ordinary-exception behavior is verified GREEN.
- The initializer store-write catch filter excludes `OperationCanceledException`; exact-instance cancellation behavior is the next contract.

## Next recommended step

Add one focused exact-instance cancellation contract for `_contextStore.Set(contextResponse.Data)` inside `ErrorCatalogInitializer.InitializeAsync(...)`.

The contract must verify that a specific `OperationCanceledException` thrown by `Set(...)` propagates unchanged using `Assert.Same(...)`.

No production change is expected. After that contract is GREEN, consider all currently known context-store write boundaries complete for the current scope and inspect the next unguarded initializer dependency boundary separately.

Likely next candidate: `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` ordinary exception behavior, followed separately by cancellation. Do not combine it with `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)`.

Keep changes small, tested, documented here, and committed directly to `master`.
