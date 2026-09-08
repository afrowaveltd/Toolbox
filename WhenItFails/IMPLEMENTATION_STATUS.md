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
- The complete `WhenItFails.Tests` suite is verified GREEN at 978/978 tests before the initializer exception contract.
- Initializer null-response behavior is already covered by `WIF_INITIALIZER_RESPONSE_NULL`.
- `InitializeCoreAsync(...)` now converts ordinary `IErrorCatalogInitializer.InitializeAsync(...)` exceptions into `WIF_INITIALIZER_FAILED` without exposing raw dependency exception text.
- `OperationCanceledException` is deliberately excluded from that conversion and is intended to propagate unchanged.

## Latest committed steps

### 2026-09-08 — ErrorCatalogRuntime initializer exception fix

Production fix commit: `5413dd81365008113dafe4f0e052613578a51978`

Changed:

`WhenItFails/Services/ErrorCatalogRuntime.cs`

Only the `_initializer.InitializeAsync(...)` invocation/await is wrapped in a narrow exception boundary.

Ordinary initializer exceptions now become:

```text
Status: Failed
Code: WIF_INITIALIZER_FAILED
Message: The error catalog initializer failed.
```

The original exception message is not copied into the public runtime response.

The catch filter excludes `OperationCanceledException`, so cancellation continues to propagate naturally.

Existing response validation remains outside the exception boundary, preserving the distinct contracts for:

```text
WIF_INITIALIZER_RESPONSE_NULL
WIF_INITIALIZATION_PAYLOAD_NULL
WIF_INITIALIZATION_BOOTSTRAP_NULL
WIF_INITIALIZATION_CONTEXT_NULL
```

The production diff was checked after commit and contains only the intended initializer invocation/await boundary.

### 2026-09-08 — verified RED initializer exception contract

Contract commit: `e184100471e2ee8c5946ec2b3ad0f8c30b7c91c3`

Focused test:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeInitializerExceptionContractTests.InitializeAsync_WhenInitializerThrows_ReturnsStableFailure`

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
Sensitive runtime initializer detail must not escape.
```

The faulted task exception escaped directly through `ErrorCatalogRuntime.InitializeCoreAsync(...)`, confirming the missing runtime-facade initializer boundary.

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

The exact original `OperationCanceledException` instance propagates through the runtime context-store `GetCurrent()` boundary.

## Verification state

- Verified continuation baseline before the initializer exception contract: 978/978 tests GREEN.
- Runtime descriptor-service, profile-selection, and context-store `GetCurrent()` exception/cancellation boundaries are complete for the current scope.
- Initializer null-response behavior is already covered.
- Initializer ordinary-exception contract is verified RED before the production fix.
- Production initializer exception boundary is committed and awaits focused local verification.
- Expected complete-suite count after the new contract passes: 979 tests.

## Recommended verification

Pull current `master` and run the focused initializer contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenInitializerThrows_ReturnsStableFailure"
```

Expected result after the production fix: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: 979/979 GREEN.

## Next recommended step

After 979/979 GREEN is confirmed, add one focused cancellation contract proving that an `OperationCanceledException` from `IErrorCatalogInitializer.InitializeAsync(...)` propagates as the exact original instance rather than becoming `WIF_INITIALIZER_FAILED`.

If that passes without production changes, consider the runtime initializer boundary complete for the current scope and inspect the next distinct initialization dependency boundary, especially `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` and `_contextStore.Set(...)`.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
