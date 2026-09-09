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
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 992/992 tests with zero compiler warnings**.
- The next initializer dependency boundary is `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)`; null-response behavior is already covered, ordinary-exception behavior is next.

## Latest committed steps

### 2026-09-09 — 992/992 GREEN bootstrapper checkpoint

Checkpoint commit: this commit.

Bootstrapper cancellation contract commit: `dc7700c61dfc0165a374fafac8c98145be4ca8ad`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 992
Skipped:  0
Total:  992
Compiler warnings: 0
```

The exact original `OperationCanceledException` instance from `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` propagates unchanged. The context provider is not invoked and the previous context remains unchanged when bootstrapper cancellation occurs.

Together with the existing null-response and ordinary-exception contracts, this completes the initializer bootstrapper boundary for the current scope.

### 2026-09-09 — initializer bootstrapper ordinary-exception fix

Production fix commit: `c4951648527c5fa123f64714e2f87e1c0352e125`

Ordinary bootstrapper exceptions are converted to:

```text
Status: Failed
Data: null
Code: WIF_INITIALIZER_BOOTSTRAPPER_FAILED
Message: The JSON workspace bootstrapper failed.
```

without leaking raw dependency text. `OperationCanceledException` is excluded from the catch boundary.

## Verification state

- Clean continuation baseline: **992/992 GREEN, zero compiler warnings**.
- Bootstrapper boundary is complete for the current scope.
- Context-provider null-response behavior is already covered by `WIF_INITIALIZER_CONTEXT_PROVIDER_RESPONSE_NULL`.
- No focused ordinary-exception contract currently exists for `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)`.

## Recommended verification

No additional verification is required for the bootstrapper boundary.

## Next recommended step

Add one focused ordinary-exception contract for `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` as invoked by `ErrorCatalogInitializer.InitializeAsync(...)`.

The contract should require a stable initializer-level failure without exposing raw provider exception text, preserve any previous context, and ensure the context store is not updated.

Observe RED before changing production code. Then add the smallest exception boundary around only the context-provider invocation. Follow with exact-instance cancellation as a separate contract.

Keep changes small, tested, documented here, and committed directly to `master`.
