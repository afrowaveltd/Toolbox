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
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 986/986 tests with zero compiler warnings** before the flexible-fallback store-write contract.
- The explicit `ResetToDefaultsAsync()` `_contextStore.Set(...)` boundary is complete for the current scope.
- The flexible-fallback `_contextStore.Set(...)` ordinary-exception contract is verified RED before the production fix.
- `CreateBuiltInFallbackResponseAsync(...)` now normalizes ordinary context-store write exceptions to `WIF_CONTEXT_STORE_FAILED` and preserves the established `WIF_DEFAULT_FALLBACK_FAILED` wrapper.
- `ErrorCatalogInitializer` store-write behavior remains intentionally untouched.

## Latest committed steps

### 2026-09-09 — flexible fallback context-store Set exception fix

Production fix commit: `30d67bfb5b7e2dffdbed950ae39558dd6eeed330`

Changed only the `_contextStore.Set(fallbackResponse.Data)` invocation inside `CreateBuiltInFallbackResponseAsync(...)`.

The store write is now guarded narrowly. Ordinary exceptions are converted to an internal context failure:

```text
Status: Failed
Data: null
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

That internal failure is passed through the existing `CreateBuiltInFallbackFailureResponse(...)`, preserving the public wrapper:

```text
Status: Failed
Data: null
Code: WIF_DEFAULT_FALLBACK_FAILED
Message: The configured error catalog failed and the bundled default catalog could not be activated.
```

and fallback metadata:

```text
WhenItFails.FallbackFailure.Code = WIF_CONTEXT_STORE_FAILED
WhenItFails.FallbackFailure.Status = Failed
WhenItFails.FallbackFailure.Message = The error catalog context store failed.
```

The configured initialization failure remains available in `WhenItFails.ProjectFailure.*` metadata.

The catch filter excludes `OperationCanceledException`, so cancellation is still intended to propagate unchanged.

The production diff was checked and contains only the intended flexible-fallback store-write guard.

### 2026-09-09 — verified RED flexible fallback context-store Set contract

Contract commit: `c0d2f39998cd8f316d75bce18dbf4e8079ff2475`

Focused test:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeContextStoreSetExceptionContractTests.InitializeAsync_WhenFlexibleFallbackContextStoreSetThrows_ReturnsStableFallbackFailure`

Observed locally on Windows before the production fix:

```text
Failed: 1
Passed: 0
Skipped: 0
Total: 1
```

Failure:

```text
System.InvalidOperationException:
Sensitive flexible fallback context store Set detail must not escape.
```

The exception escaped directly from `EmptyThrowingSetContextStore.Set(...)` through `CreateBuiltInFallbackResponseAsync(...)`, confirming the distinct unguarded store-write boundary.

### 2026-09-08 — 986/986 GREEN reset store-write checkpoint

Checkpoint commit: `9a4d01e74056eab9fce0ce845267544d637d7baa`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 986
Skipped:  0
Total:  986
Compiler warnings: 0
```

The exact original `OperationCanceledException` instance from the explicit reset store-write path propagates unchanged. Together with the ordinary-exception contract, this completes that invocation site for the current scope.

## Verification state

- Clean continuation baseline before the new flexible-fallback contract: **986/986 GREEN, zero compiler warnings**.
- Explicit reset store-write boundary is complete.
- Flexible-fallback store-write ordinary-exception contract is verified RED before the production fix.
- Production flexible-fallback store-write guard is committed and awaits focused local GREEN verification.
- Expected complete-suite count after the new contract passes: **987 tests**.

## Recommended verification

Pull current `master` and run the focused contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenFlexibleFallbackContextStoreSetThrows_ReturnsStableFallbackFailure"
```

Expected result after the production fix: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **987/987 GREEN with zero compiler warnings**.

## Next recommended step

After 987/987 GREEN is confirmed, add one focused exact-instance cancellation contract for the same flexible-fallback `_contextStore.Set(...)` invocation.

If that passes without production changes, consider both runtime store-write invocation sites complete for the current scope. Then inspect the separate `_contextStore.Set(...)` inside `ErrorCatalogInitializer`, again ordinary exception first and cancellation second.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.