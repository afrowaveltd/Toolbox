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
- The complete `WhenItFails.Tests` suite is verified GREEN at 978/978 tests.
- Initialization dependency null-response behavior already covers a null `IErrorCatalogInitializer.InitializeAsync(...)` response and a null built-in-provider response.
- A new focused contract now defines ordinary-exception behavior for `IErrorCatalogInitializer.InitializeAsync(...)`.

## Latest committed steps

### 2026-09-08 — ErrorCatalogRuntime initializer exception contract

Contract commit: `e184100471e2ee8c5946ec2b3ad0f8c30b7c91c3`

Added:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeInitializerExceptionContractTests.InitializeAsync_WhenInitializerThrows_ReturnsStableFailure`

Contract:

```text
IErrorCatalogInitializer.InitializeAsync(...) => faulted task with ordinary exception
                         ↓
Status: Failed
Code: WIF_INITIALIZER_FAILED
Message: The error catalog initializer failed.
```

The injected initializer returns a faulted task containing an `InvalidOperationException` with sensitive diagnostic text. The runtime facade must not expose that raw text.

All unrelated dependencies are throwing sentinels, so this test isolates only the initializer invocation/await boundary.

No production code changed in this step.

Current `ErrorCatalogRuntime.InitializeCoreAsync(...)` directly awaits `_initializer.InitializeAsync(...)`, so this focused contract is expected to be RED with the original exception escaping.

### 2026-09-08 — verified ErrorCatalogRuntime context-store cancellation contract

Contract commit: `bde451cb05a9484b193b347f042c728cbbe0340f`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 978
Skipped:  0
Total:  978
```

This confirms that an `OperationCanceledException` thrown by `IErrorCatalogContextStore.GetCurrent()` propagates as the exact original exception instance.

### 2026-09-08 — ErrorCatalogRuntime context-store exception fix

Production fix commit: `e83d2e60e5a4ac049ff3a92003b705f8a8f41872`

Ordinary context-store exceptions become `WIF_CONTEXT_STORE_FAILED` without exposing raw dependency exception text.

## Verification state

- Complete verified continuation baseline: 978/978 tests GREEN.
- Runtime descriptor-service, profile-selection, and context-store `GetCurrent()` exception/cancellation boundaries are complete for the current scope.
- Initializer null-response behavior is already covered.
- New initializer ordinary-exception contract is committed and awaits focused local verification.
- Production `InitializeCoreAsync(...)` remains unchanged until the RED state is observed.

## Recommended verification

Pull current `master` and run only the new initializer contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenInitializerThrows_ReturnsStableFailure"
```

Expected current result: RED with the original exception text:

```text
Sensitive runtime initializer detail must not escape.
```

Expected complete-suite count once the new contract eventually passes: 979 tests.

## Next recommended step

If the focused initializer contract fails as expected, add the smallest exception boundary around the `_initializer.InitializeAsync(...)` invocation/await in `InitializeCoreAsync(...)`.

Convert ordinary exceptions into:

```text
Status: Failed
Code: WIF_INITIALIZER_FAILED
Message: The error catalog initializer failed.
```

while allowing `OperationCanceledException` to propagate unchanged.

Do not add cancellation or built-in-provider exception contracts in the same production step. Verify the single initializer contract first.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
