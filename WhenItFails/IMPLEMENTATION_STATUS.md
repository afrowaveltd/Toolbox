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
- `ErrorCatalogRuntime` initializer null-response, ordinary-exception, and cancellation behavior is complete for the current scope.
- The complete `WhenItFails.Tests` suite is verified GREEN at 980/980 tests.
- Built-in catalog-provider null-response behavior is already covered for `ResetToDefaultsAsync()` and flexible initialization fallback.

## Latest committed steps

### 2026-09-08 — verified ErrorCatalogRuntime initializer cancellation contract

Contract commit: `ba3a1137bfcb25f64787b54bff7fada037285b7a`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 980
Skipped:  0
Total:  980
```

The exact original `OperationCanceledException` instance from `IErrorCatalogInitializer.InitializeAsync(...)` propagates unchanged through the runtime initializer boundary. No production change was required.

### 2026-09-08 — ErrorCatalogRuntime initializer exception fix

Production fix commit: `5413dd81365008113dafe4f0e052613578a51978`

Ordinary initializer exceptions become:

```text
Status: Failed
Code: WIF_INITIALIZER_FAILED
Message: The error catalog initializer failed.
```

without exposing raw dependency exception text.

## Verification state

- Complete verified continuation baseline: 980/980 tests GREEN.
- Runtime descriptor-service, profile-selection, context-store `GetCurrent()`, and initializer exception/cancellation boundaries are complete for the current scope.
- Built-in catalog-provider null-response behavior is already covered.
- No ordinary-exception contract currently exists for `IBuiltInErrorCatalogContextProvider.LoadAsync(...)`.

## Recommended next step

Add one focused `ResetToDefaultsAsync()` contract for an ordinary exception from `IBuiltInErrorCatalogContextProvider.LoadAsync(...)`.

Proposed stable runtime contract:

```text
Status: Failed
Code: WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED
Message: The bundled default catalog provider failed.
```

The injected provider should expose sensitive text in the thrown exception so the test proves the runtime facade does not leak it.

Do not change production code until the focused RED state is observed. After that, add the smallest exception boundary around the built-in provider invocation/await while preserving `OperationCanceledException` propagation and the existing null-response contract.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
