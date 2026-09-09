# Implementation status

Last updated: 2026-09-09

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening initialization dependency boundaries against malformed behavior, raw exception leakage, and cancellation corruption.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service and profile-selection exception/cancellation boundaries are complete.
- All currently known `IErrorCatalogContextStore` read/write boundaries in the active runtime/initializer scope are complete.
- `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` is complete for the current scope: null-response, ordinary-exception, and exact-instance cancellation behavior are covered.
- `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` null-response and ordinary-exception behavior are now verified.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 993/993 tests with zero compiler warnings**.

## Latest committed steps

### 2026-09-09 — 993/993 GREEN context-provider exception checkpoint

Production fix commit: `f5485fd244b860514c0f683bee855e6cc6e7a69c`

Ordinary-exception contract commit: `98a14a04feb702345eade7b1217b347e72910dda`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 993
Skipped:  0
Total:  993
Compiler warnings: 0
```

The initializer now converts ordinary exceptions from `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` into:

```text
Status: Failed
Data: null
Code: WIF_INITIALIZER_CONTEXT_PROVIDER_FAILED
Message: The error catalog context provider failed during initialization.
```

without leaking raw dependency text.

The previous context remains unchanged and the context-store write path is not reached when the provider fails.

### 2026-09-09 — verified RED initializer context-provider exception contract

Focused test:

`WhenItFails.Tests/Initialization/ErrorCatalogInitializerContextProviderExceptionContractTests.InitializeAsync_WhenContextProviderThrows_ReturnsStableFailure`

Observed before the production fix:

```text
System.InvalidOperationException:
Sensitive initializer context provider detail must not escape.
```

This confirmed the missing context-provider exception boundary before the production guard was added.

## Verification state

- Clean continuation baseline: **993/993 GREEN, zero compiler warnings**.
- Bootstrapper boundary is complete for the current scope.
- Context-provider null-response and ordinary-exception behavior are verified.
- The current context-provider catch filter excludes `OperationCanceledException`, so cancellation is intended to propagate unchanged.

## Recommended verification

No additional production verification is pending for the ordinary-exception contract.

## Next recommended step

Add one focused exact-instance cancellation contract for the same `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` invocation.

The test should throw a specific `OperationCanceledException` instance from the provider, assert `Assert.Same(...)` on the exception observed from `ErrorCatalogInitializer.InitializeAsync(...)`, and verify that the previous context remains unchanged and the store write path is not reached.

No production change is expected because the current catch filter excludes `OperationCanceledException`.

After the cancellation contract is GREEN, consider the initializer context-provider boundary complete for the current scope.

Keep changes small, tested, documented here, and committed directly to `master`.
