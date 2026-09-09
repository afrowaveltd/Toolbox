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
- `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` null-response and ordinary-exception behavior are verified.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 993/993 tests with zero compiler warnings** before the new context-provider cancellation contract.
- A focused exact-instance cancellation contract is now committed for the initializer context-provider invocation and awaits local verification.

## Latest committed steps

### 2026-09-09 — initializer context-provider cancellation contract

Contract commit: `94bf93da613fccd2204eb266055613f8dabfc763`

Updated:

`WhenItFails.Tests/Initialization/ErrorCatalogInitializerContextProviderExceptionContractTests.cs`

Added:

`InitializeAsync_WhenContextProviderCancels_RethrowsSameOperationCanceledException`

Contract:

```text
IErrorCatalogContextProvider.LoadFromJsonsAsync(...)
    => throws a specific OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)`, so cancellation cannot be wrapped, replaced, or normalized into `WIF_INITIALIZER_CONTEXT_PROVIDER_FAILED`.

A previous context is preloaded into the store. The test verifies that it remains unchanged, and the store fixture throws `Unexpected Set call.` if the initializer attempts to write after cancellation.

No production code changed. The current context-provider catch filter excludes `OperationCanceledException`, so the focused contract is expected to be GREEN.

### 2026-09-09 — 993/993 GREEN context-provider exception checkpoint

Checkpoint commit: `720f378d1ce91b71056ae1207b761845ef94cb76`

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

The initializer context-provider ordinary exception is converted to `WIF_INITIALIZER_CONTEXT_PROVIDER_FAILED` without leaking raw dependency text.

## Verification state

- Clean continuation baseline: **993/993 GREEN, zero compiler warnings**.
- Bootstrapper boundary is complete for the current scope.
- Context-provider null-response and ordinary-exception behavior are verified.
- Exact-instance cancellation contract for the same context-provider invocation is committed and awaits focused local verification.
- No production change is expected for this cancellation contract.
- Expected complete-suite count after the contract passes: **994 tests**.

## Recommended verification

Pull current `master` and run the focused cancellation contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenContextProviderCancels_RethrowsSameOperationCanceledException"
```

Expected result: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **994/994 GREEN with zero compiler warnings**.

## Next recommended step

After 994/994 GREEN is confirmed, consider the initializer context-provider boundary complete for the current scope.

Then continue reconnaissance for the next unguarded external dependency call in the initialization/catalog pipeline. Keep the same sequence: focused ordinary-exception contract first, smallest production fix second, exact-instance cancellation contract third.

Keep changes small, tested, documented here, and committed directly to `master`.
