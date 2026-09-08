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
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 985/985 tests with zero compiler warnings**.
- `_contextStore.Set(...)` remains a distinct dependency boundary at three invocation sites: `ResetToDefaultsAsync()`, flexible fallback in `ErrorCatalogRuntime`, and `ErrorCatalogInitializer`.
- The explicit `ResetToDefaultsAsync()` store-write ordinary-exception contract is verified GREEN after the production fix.
- `ResetToDefaultsAsync()` converts ordinary context-store `Set(...)` exceptions into `WIF_CONTEXT_STORE_FAILED` without exposing raw dependency text.

## Latest committed steps

### 2026-09-08 — verified ResetToDefaults context-store Set exception fix

Production fix commit: `dc5eb34f862df31736eff5f0726d090598ff55f9`

Contract commit: `7f5b53a578e7b20ea613e474f9bb463387551373`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 985
Skipped:  0
Total:  985
Compiler warnings: 0
```

The explicit reset store-write boundary now converts ordinary `IErrorCatalogContextStore.Set(...)` exceptions into:

```text
Status: Failed
Data: null
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

without exposing the original dependency exception text.

The catch filter excludes `OperationCanceledException`, so cancellation is still intended to propagate unchanged.

### 2026-09-08 — verified RED ResetToDefaults context-store Set contract

The focused contract was observed RED before the production fix with the raw message:

```text
Sensitive runtime context store Set detail must not escape.
```

This confirmed the missing boundary at the exact `_contextStore.Set(...)` invocation in `ResetToDefaultsAsync()`.

## Verification state

- Clean continuation baseline: **985/985 GREEN, zero compiler warnings**.
- Built-in provider boundary is complete for the current scope.
- Explicit reset store-write ordinary-exception behavior is verified GREEN.
- Cancellation behavior for the same `Set(...)` invocation is not yet locked by a focused exact-instance contract.

## Recommended verification

No additional verification is required for the ordinary-exception contract; 985/985 GREEN is the current verified baseline.

## Next recommended step

Add one focused exact-instance cancellation contract for the same `ResetToDefaultsAsync()` `_contextStore.Set(...)` invocation.

Require the exact original `OperationCanceledException` instance to propagate unchanged. Do not convert cancellation into `WIF_CONTEXT_STORE_FAILED`.

No production change is expected because the current catch filter excludes `OperationCanceledException`.

After that contract is GREEN, consider the explicit reset store-write boundary complete for the current scope. Then move to the separate flexible-fallback `_contextStore.Set(...)` invocation, again ordinary exception first and cancellation second.

Do not modify `ErrorCatalogInitializer` in the same step.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.