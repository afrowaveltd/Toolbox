# Implementation status

Last updated: 2026-09-08

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and service boundaries against malformed dependency behavior, raw exception leakage, and internally inconsistent responses.

## Current state

- `WhenItFails` provides structured error catalogs, runtime error resolution, profiles, diagnostics, initialization/recovery behavior, and project-local catalog handling.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening blocks are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service ordinary-exception and cancellation behavior is complete for the current scope.
- `ErrorCatalogRuntime` profile-selection ordinary-exception and cancellation behavior is complete for the current scope.
- Context-store null-response behavior is broadly covered across `GetCurrentContext()`, all descriptor entry points, and `ResolveProfile(...)`.
- `GetCurrentContextResponse()` converts ordinary `IErrorCatalogContextStore.GetCurrent()` exceptions into `WIF_CONTEXT_STORE_FAILED` without exposing raw exception text.
- `OperationCanceledException` is deliberately excluded from that conversion.
- The complete `WhenItFails.Tests` suite is verified GREEN at 977/977 tests.

## Latest committed steps

### 2026-09-08 — verified ErrorCatalogRuntime context-store exception fix

Production fix commit: `e83d2e60e5a4ac049ff3a92003b705f8a8f41872`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 977
Skipped:  0
Total:  977
```

This confirms that an ordinary exception from `IErrorCatalogContextStore.GetCurrent()` becomes:

```text
Status: Failed
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

without exposing the original dependency exception text.

The existing null-response guard remains independent and still returns `WIF_CONTEXT_STORE_RESPONSE_NULL`.

### 2026-09-08 — verified RED context-store exception contract

Contract commit: `8e892674ecc110bded3ac57040610954768b22a9`

Before the production fix, the focused `GetCurrentContext(...)` contract failed with the raw `InvalidOperationException` text `Sensitive runtime context store detail must not escape.` escaping the runtime facade.

### 2026-09-08 — verified ErrorCatalogRuntime profile-selection cancellation contract

Contract commit: `15be30a6e03f333c9f2153124591aac79a82bf5b`

Verified locally at 976/976 tests GREEN. The exact original `OperationCanceledException` instance propagates through the runtime profile-selection boundary.

## Verification state

- Complete verified continuation baseline: 977/977 tests GREEN.
- Runtime descriptor-service ordinary-exception and cancellation behavior is complete for the current scope.
- Runtime profile-selection ordinary-exception and cancellation behavior is complete for the current scope.
- Runtime context-store null-response and ordinary-exception behavior is verified GREEN.
- No context-store cancellation contract is committed yet.

## Recommended verification

No verification is pending for the 977-test checkpoint.

## Next recommended step

Add one focused cancellation contract proving that an `OperationCanceledException` from `IErrorCatalogContextStore.GetCurrent()` propagates as the exact original instance rather than becoming `WIF_CONTEXT_STORE_FAILED`.

Because all descriptor/profile public paths share `GetCurrentContextResponse()`, use `GetCurrentContext()` as the narrowest public verification path and avoid duplicating the same cancellation behavior across every facade entry point.

If cancellation passes without production changes, consider the runtime `GetCurrent()` boundary complete and inspect the next distinct dependency boundary, especially `_contextStore.Set(...)`, `_builtInContextProvider.LoadAsync(...)`, or `_initializer.InitializeAsync(...)`, based on existing contract coverage.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
