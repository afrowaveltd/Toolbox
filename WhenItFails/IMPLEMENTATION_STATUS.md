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
- Initializer null-response and ordinary-exception behavior are covered and verified.
- `InitializeCoreAsync(...)` converts ordinary `IErrorCatalogInitializer.InitializeAsync(...)` exceptions into `WIF_INITIALIZER_FAILED` without exposing raw dependency exception text.
- The complete `WhenItFails.Tests` suite is verified GREEN at 979/979 tests.
- A focused initializer cancellation contract now requires the exact original `OperationCanceledException` instance to propagate unchanged.

## Latest committed steps

### 2026-09-08 — ErrorCatalogRuntime initializer cancellation contract

Contract commit: `ba3a1137bfcb25f64787b54bff7fada037285b7a`

Updated:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeInitializerExceptionContractTests.cs`

Added:

`InitializeAsync_WhenInitializerCancels_RethrowsSameOperationCanceledException`

Contract:

```text
IErrorCatalogInitializer.InitializeAsync(...)
    => faulted task carrying an OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)`, so future refactoring cannot wrap cancellation, replace it with another cancellation exception, or convert it into `WIF_INITIALIZER_FAILED`.

The test fixture was lightly refactored through a shared `CreateRuntime(...)` helper. All unrelated dependencies remain throwing sentinels, so the contract isolates only the initializer invocation/await boundary.

No production code changed in this step. The current initializer catch filter excludes `OperationCanceledException`, so this focused contract is expected to be GREEN.

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

This confirms that ordinary initializer exceptions become:

```text
Status: Failed
Code: WIF_INITIALIZER_FAILED
Message: The error catalog initializer failed.
```

without exposing raw dependency exception text.

Existing response validation remains outside the exception boundary and preserves distinct null/malformed-response contracts.

### 2026-09-08 — verified ErrorCatalogRuntime context-store cancellation contract

Contract commit: `bde451cb05a9484b193b347f042c728cbbe0340f`

Verified locally at 978/978 tests GREEN. The exact original `OperationCanceledException` instance propagates through the runtime context-store `GetCurrent()` boundary.

## Verification state

- Complete verified continuation baseline: 979/979 tests GREEN.
- Runtime descriptor-service, profile-selection, and context-store `GetCurrent()` exception/cancellation boundaries are complete for the current scope.
- Initializer null-response and ordinary-exception behavior are verified GREEN.
- Initializer cancellation contract is committed and awaits focused local verification.
- No production change is expected for the cancellation contract.

## Recommended verification

Pull current `master` and run the focused initializer cancellation contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenInitializerCancels_RethrowsSameOperationCanceledException"
```

If green, run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite count: 980 tests.

## Next recommended step

After 980/980 GREEN is confirmed, consider the runtime initializer boundary complete for the current scope.

Then inspect the next distinct initialization dependency boundary. The strongest candidates are `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` and `_contextStore.Set(...)`; choose the next contract only after confirming existing null/malformed coverage and keep the same exception/cancellation separation.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
