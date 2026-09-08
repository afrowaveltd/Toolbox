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
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 984/984 tests with zero compiler warnings**.
- `_contextStore.Set(...)` remains a distinct unguarded dependency boundary at three invocation sites: `ResetToDefaultsAsync()`, flexible fallback in `ErrorCatalogRuntime`, and `ErrorCatalogInitializer`.
- A focused ordinary-exception contract now covers the explicit `ResetToDefaultsAsync()` store-write path and awaits local RED verification.

## Latest committed steps

### 2026-09-08 — ResetToDefaults context-store Set exception contract

Contract commit: `7f5b53a578e7b20ea613e474f9bb463387551373`

Added:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeContextStoreSetExceptionContractTests.ResetToDefaultsAsync_WhenContextStoreSetThrows_ReturnsStableFailure`

The test supplies a successful built-in provider response and a context store whose `Set(...)` throws an ordinary `InvalidOperationException` containing sensitive diagnostic text.

Expected public contract:

```text
Status: Failed
Data: null
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

The raw store exception message must not escape.

No production code changed in this step. `ResetToDefaultsAsync()` currently calls `_contextStore.Set(...)` directly, so the focused test is expected to be RED with the original exception escaping.

### 2026-09-08 — clean 984/984 checkpoint

Checkpoint commit: `6e1624db07efe583d2a4779c667ab978f9b9d3b6`

Verified locally after warning cleanup:

```text
WhenItFails.Tests
Failed:   0
Passed: 984
Skipped:  0
Total:  984
Compiler warnings: 0
```

This confirms the `CS8767` cleanup without changing behavior or test count.

## Verification state

- Clean continuation baseline: **984/984 GREEN, zero compiler warnings**.
- Built-in provider boundary is complete for the current scope.
- New `ResetToDefaultsAsync()` context-store `Set(...)` ordinary-exception contract is committed and awaits focused local verification.
- Production code remains unchanged until the RED state is observed.
- Expected complete-suite count once the new contract eventually passes: 985 tests.

## Recommended verification

Pull current `master` and run only the new contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResetToDefaultsAsync_WhenContextStoreSetThrows_ReturnsStableFailure"
```

Expected current result: RED with the original exception text:

```text
Sensitive runtime context store Set detail must not escape.
```

## Next recommended step

If RED is confirmed, add the smallest exception boundary around only the `_contextStore.Set(...)` call in `ResetToDefaultsAsync()`.

Convert ordinary exceptions into:

```text
Status: Failed
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

while allowing `OperationCanceledException` to propagate unchanged.

Do not modify the separate flexible-fallback or initializer `Set(...)` sites in the same production step.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
