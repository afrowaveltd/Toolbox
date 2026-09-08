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
- `ErrorCatalogRuntime` context-store `GetCurrent()` null-response, ordinary-exception, and cancellation behavior is complete for the current scope.
- The complete `WhenItFails.Tests` suite is verified GREEN at 978/978 tests.
- Initialization dependency null-response behavior already covers a null `IErrorCatalogInitializer.InitializeAsync(...)` response and a null built-in-provider response.

## Latest committed steps

### 2026-09-08 — verified ErrorCatalogRuntime context-store cancellation contract

Contract commit: `bde451cb05a9484b193b347f042c728cbbe0340f`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 978
Skipped:  0
Total:  978
```

This confirms that an `OperationCanceledException` thrown by `IErrorCatalogContextStore.GetCurrent()` propagates through `ErrorCatalogRuntime.GetCurrentContext()` as the exact original exception instance.

No production change was required because `GetCurrentContextResponse()` already excludes `OperationCanceledException` from its ordinary-exception conversion.

### 2026-09-08 — ErrorCatalogRuntime context-store exception fix

Production fix commit: `e83d2e60e5a4ac049ff3a92003b705f8a8f41872`

Ordinary `IErrorCatalogContextStore.GetCurrent()` exceptions become:

```text
Status: Failed
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

without exposing raw dependency exception text.

### 2026-09-08 — verified ErrorCatalogRuntime profile-selection cancellation contract

Contract commit: `15be30a6e03f333c9f2153124591aac79a82bf5b`

Verified locally at 976/976 tests GREEN.

## Verification state

- Complete verified continuation baseline: 978/978 tests GREEN.
- Runtime descriptor-service, profile-selection, and context-store `GetCurrent()` exception/cancellation boundaries are complete for the current scope.
- Initialization null-response behavior is already covered.
- No initializer ordinary-exception contract currently exists.

## Recommended verification

No verification is pending for the 978-test checkpoint.

## Next recommended step

Add one focused contract for `ErrorCatalogRuntime.InitializeAsync()` when injected `IErrorCatalogInitializer.InitializeAsync(...)` faults with an ordinary exception.

Expected stable runtime response:

```text
Status: Failed
Code: WIF_INITIALIZER_FAILED
Message: The error catalog initializer failed.
```

The original exception text must not escape. Do not change production code until the focused RED state is observed.

After ordinary-exception behavior is fixed and verified, add a focused cancellation contract proving that initializer cancellation propagates as the exact original `OperationCanceledException` instance.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
