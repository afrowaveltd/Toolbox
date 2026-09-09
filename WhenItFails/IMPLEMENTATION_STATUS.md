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
- The flexible-fallback `_contextStore.Set(...)` ordinary-exception behavior is now locally verified GREEN.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 987/987 tests with zero compiler warnings**.
- `ErrorCatalogInitializer` store-write behavior remains intentionally untouched.

## Latest verified checkpoint

### 2026-09-09 — 987/987 GREEN flexible fallback store-write checkpoint

Production fix commit: `30d67bfb5b7e2dffdbed950ae39558dd6eeed330`

Contract commit: `c0d2f39998cd8f316d75bce18dbf4e8079ff2475`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 987
Skipped:  0
Total:  987
Compiler warnings: 0
```

The flexible-fallback context-store write now converts ordinary `IErrorCatalogContextStore.Set(...)` exceptions into an internal:

```text
Status: Failed
Data: null
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

and preserves the established public wrapper:

```text
Status: Failed
Data: null
Code: WIF_DEFAULT_FALLBACK_FAILED
Message: The configured error catalog failed and the bundled default catalog could not be activated.
```

with normalized `WhenItFails.FallbackFailure.*` metadata.

The raw dependency exception text does not escape.

## Verification state

- Clean continuation baseline: **987/987 GREEN, zero compiler warnings**.
- Explicit reset store-write boundary is complete for the current scope.
- Flexible-fallback store-write ordinary-exception behavior is verified GREEN.
- The production catch filter excludes `OperationCanceledException`; exact-instance cancellation behavior for this invocation is the next contract.
- Expected complete-suite count after adding the cancellation contract: **988 tests**.

## Next recommended step

Add one focused exact-instance cancellation contract for `_contextStore.Set(fallbackResponse.Data)` inside `CreateBuiltInFallbackResponseAsync(...)`.

The fixture must still report no previous context from `GetCurrent()` so execution reaches the flexible fallback, then throw a specific `OperationCanceledException` instance from `Set(...)`.

Require `Assert.Same(...)` on the exception observed through `InitializeAsync(...)`.

No production change is expected because the current store-write catch filter excludes `OperationCanceledException`.

After this contract is GREEN, consider both `ErrorCatalogRuntime` store-write invocation sites complete for the current scope and move to the separate `_contextStore.Set(...)` inside `ErrorCatalogInitializer`, ordinary exception first and cancellation second.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.