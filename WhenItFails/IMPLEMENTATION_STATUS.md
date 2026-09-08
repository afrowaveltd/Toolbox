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
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 986/986 tests with zero compiler warnings**.
- The explicit `ResetToDefaultsAsync()` `_contextStore.Set(...)` boundary is complete for the current scope: ordinary exceptions are normalized to `WIF_CONTEXT_STORE_FAILED`, and the exact original `OperationCanceledException` instance propagates unchanged.
- Two distinct store-write sites remain to harden: flexible fallback in `ErrorCatalogRuntime` and project-catalog activation in `ErrorCatalogInitializer`.

## Latest verified checkpoint

### 2026-09-08 — 986/986 GREEN reset store-write checkpoint

The focused cancellation contract

`ErrorCatalogRuntimeContextStoreSetExceptionContractTests.ResetToDefaultsAsync_WhenContextStoreSetCancels_RethrowsSameOperationCanceledException`

is locally verified GREEN together with the complete suite:

```text
WhenItFails.Tests
Failed:   0
Passed: 986
Skipped:  0
Total:  986
Compiler warnings: 0
```

This completes the explicit reset store-write boundary for the current scope.

Relevant commits:

- ordinary-exception contract: `7f5b53a578e7b20ea613e474f9bb463387551373`
- production guard: `dc5eb34f862df31736eff5f0726d090598ff55f9`
- cancellation contract: `a0d49219265cad2aa0d093a76264c456109d097b`

## Verification state

- Clean continuation baseline: **986/986 GREEN, zero compiler warnings**.
- Explicit reset store-write boundary is complete.
- Flexible-fallback `_contextStore.Set(...)` still has no dedicated ordinary-exception or cancellation contract and remains a direct unguarded call.
- `ErrorCatalogInitializer` store-write boundary remains intentionally untouched until the runtime flexible-fallback path is complete.

## Next recommended step

Add one focused ordinary-exception contract for `_contextStore.Set(...)` inside `CreateBuiltInFallbackResponseAsync(...)`.

The established public flexible-fallback wrapper must remain:

```text
Status: Failed
Code: WIF_DEFAULT_FALLBACK_FAILED
Message: The configured error catalog failed and the bundled default catalog could not be activated.
```

The store dependency failure should be preserved in metadata as:

```text
WhenItFails.FallbackFailure.Code = WIF_CONTEXT_STORE_FAILED
WhenItFails.FallbackFailure.Status = Failed
WhenItFails.FallbackFailure.Message = The error catalog context store failed.
```

The original store exception text must not escape. Do not modify production code until the focused RED state is observed.

After ordinary-exception behavior is fixed and verified, add an exact-instance cancellation contract for the same flexible-fallback `Set(...)` invocation.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.