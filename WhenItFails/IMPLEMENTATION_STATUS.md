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
- `_contextStore.Set(...)` is a distinct dependency boundary at three invocation sites: `ResetToDefaultsAsync()`, flexible fallback in `ErrorCatalogRuntime`, and `ErrorCatalogInitializer`.
- The explicit `ResetToDefaultsAsync()` store-write ordinary-exception behavior is verified GREEN.
- A focused exact-instance cancellation contract is now committed for the same reset store-write invocation and awaits local verification.

## Latest committed steps

### 2026-09-08 — ResetToDefaults context-store Set cancellation contract

Contract commit: `a0d49219265cad2aa0d093a76264c456109d097b`

Updated:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeContextStoreSetExceptionContractTests.cs`

Added:

`ResetToDefaultsAsync_WhenContextStoreSetCancels_RethrowsSameOperationCanceledException`

Contract:

```text
IErrorCatalogContextStore.Set(...)
    => throws a specific OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)`, so cancellation cannot be wrapped, replaced, or normalized into `WIF_CONTEXT_STORE_FAILED`.

The fixture now uses a shared `CreateRuntime(...)` helper. The built-in provider returns a successful non-null context so the test reaches exactly the store-write boundary.

No production code changed in this step. The current `ResetToDefaultsAsync()` catch filter around `Set(...)` excludes `OperationCanceledException`, so the focused contract is expected to be GREEN.

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

The explicit reset store-write boundary converts ordinary `IErrorCatalogContextStore.Set(...)` exceptions into:

```text
Status: Failed
Data: null
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

without exposing the original dependency exception text.

## Verification state

- Clean continuation baseline: **985/985 GREEN, zero compiler warnings**.
- Built-in provider boundary is complete for the current scope.
- Explicit reset store-write ordinary-exception behavior is verified GREEN.
- Exact-instance cancellation contract for the same `Set(...)` invocation is committed and awaits focused local verification.
- No production change is expected for this cancellation contract.
- Expected complete-suite count after the new contract passes: **986 tests**.

## Recommended verification

Pull current `master` and run the focused cancellation contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResetToDefaultsAsync_WhenContextStoreSetCancels_RethrowsSameOperationCanceledException"
```

Expected result: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **986/986 GREEN with zero compiler warnings**.

## Next recommended step

After 986/986 GREEN is confirmed, consider the explicit `ResetToDefaultsAsync()` store-write boundary complete for the current scope.

Then add one focused ordinary-exception contract for the separate flexible-fallback `_contextStore.Set(...)` invocation in `CreateBuiltInFallbackResponseAsync(...)`. Keep the established flexible-fallback wrapper semantics intact and do not modify `ErrorCatalogInitializer` in the same step.

After that ordinary-exception behavior is verified/fixed, add the corresponding exact-instance cancellation contract.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.