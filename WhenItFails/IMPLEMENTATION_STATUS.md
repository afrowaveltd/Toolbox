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
- Initializer null-response behavior is covered by `WIF_INITIALIZER_RESPONSE_NULL`.
- `InitializeCoreAsync(...)` converts ordinary `IErrorCatalogInitializer.InitializeAsync(...)` exceptions into `WIF_INITIALIZER_FAILED` without exposing raw dependency exception text.
- The complete `WhenItFails.Tests` suite is verified GREEN at 979/979 tests.
- `OperationCanceledException` remains intentionally excluded from the initializer exception conversion and is the next contract to verify explicitly.

## Latest committed steps

### 2026-09-08 — verified ErrorCatalogRuntime initializer exception fix

Production fix commit: `5413dd81365008113dafe4f0e052613578a51978`

Contract commit: `e184100471e2ee8c5946ec2b3ad0f8c30b7c91c3`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 979
Skipped:  0
Total:  979
```

This confirms that a faulted initializer task carrying an ordinary exception becomes:

```text
Status: Failed
Code: WIF_INITIALIZER_FAILED
Message: The error catalog initializer failed.
```

without exposing the original dependency exception text.

Only the `_initializer.InitializeAsync(...)` invocation/await is inside the exception boundary. Existing response validation remains independent and preserves the distinct contracts for:

```text
WIF_INITIALIZER_RESPONSE_NULL
WIF_INITIALIZATION_PAYLOAD_NULL
WIF_INITIALIZATION_BOOTSTRAP_NULL
WIF_INITIALIZATION_CONTEXT_NULL
```

### 2026-09-08 — verified ErrorCatalogRuntime context-store cancellation contract

Contract commit: `bde451cb05a9484b193b347f042c728cbbe0340f`

Verified locally at 978/978 tests GREEN. The exact original `OperationCanceledException` instance propagates through the runtime context-store `GetCurrent()` boundary.

## Verification state

- Complete verified continuation baseline: 979/979 tests GREEN.
- Runtime descriptor-service, profile-selection, and context-store `GetCurrent()` exception/cancellation boundaries are complete for the current scope.
- Initializer null-response and ordinary-exception behavior are verified GREEN.
- Initializer cancellation behavior is not yet locked by a focused exact-instance contract.

## Recommended verification

The next step is to add one focused initializer cancellation contract proving that an `OperationCanceledException` from `IErrorCatalogInitializer.InitializeAsync(...)` propagates as the exact original instance rather than becoming `WIF_INITIALIZER_FAILED`.

No production change is expected because the current catch filter explicitly excludes `OperationCanceledException`.

## Next recommended step

Add the initializer cancellation contract, verify it GREEN, and then run the complete suite. Expected complete-suite count after adding that one test: 980 tests.

If the cancellation contract passes without production changes, consider the runtime initializer boundary complete for the current scope and inspect the next distinct initialization dependency boundary, especially `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` and `_contextStore.Set(...)`.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
