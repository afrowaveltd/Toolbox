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
- The complete `WhenItFails.Tests` suite is verified GREEN at 976/976 tests before the context-store exception contract.
- Existing context-store null-response coverage includes `GetCurrentContext()`, all three descriptor entry points, and `ResolveProfile(...)`.
- `GetCurrentContextResponse()` now converts ordinary `IErrorCatalogContextStore.GetCurrent()` exceptions into a stable runtime failure without exposing raw exception text.
- `OperationCanceledException` is deliberately excluded from that conversion and is intended to propagate unchanged.

## Latest committed steps

### 2026-09-08 — ErrorCatalogRuntime context-store exception fix

Production fix commit: `e83d2e60e5a4ac049ff3a92003b705f8a8f41872`

Changed:

`WhenItFails/Services/ErrorCatalogRuntime.cs`

`GetCurrentContextResponse()` now wraps only `_contextStore.GetCurrent()` in a narrow exception boundary.

Ordinary context-store exceptions become:

```text
Status: Failed
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

The original exception message is deliberately not copied into the public runtime response.

The exception filter excludes `OperationCanceledException`, so cancellation continues to propagate naturally.

The existing null-response guard remains unchanged and still returns:

```text
Status: Invalid
Code: WIF_CONTEXT_STORE_RESPONSE_NULL
Message: The error catalog context store returned a null response.
```

The production commit diff was checked after commit and contains only the intended `GetCurrentContextResponse()` exception boundary.

### 2026-09-08 — verified RED context-store exception contract

Contract commit: `8e892674ecc110bded3ac57040610954768b22a9`

Focused test:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeContextStoreExceptionContractTests.GetCurrentContext_WhenStoreThrows_ReturnsStableFailure`

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
Sensitive runtime context store detail must not escape.
```

The exception escaped directly from `IErrorCatalogContextStore.GetCurrent()` through `GetCurrentContextResponse()` and `GetCurrentContext()`, confirming the missing runtime-facade boundary and raw diagnostic-text leak.

### 2026-09-08 — verified ErrorCatalogRuntime profile-selection cancellation contract

Contract commit: `15be30a6e03f333c9f2153124591aac79a82bf5b`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 976
Skipped:  0
Total:  976
```

The exact original `OperationCanceledException` instance propagates through the runtime profile-selection boundary.

## Verification state

- Verified continuation baseline before the new context-store contract: 976/976 tests GREEN.
- Runtime descriptor-service ordinary-exception and cancellation behavior is complete for the current scope.
- Runtime profile-selection ordinary-exception and cancellation behavior is complete for the current scope.
- Context-store null-response behavior is already broadly covered.
- Context-store ordinary-exception contract is verified RED before the production fix.
- Production `GetCurrentContextResponse()` exception boundary is committed and awaits focused local verification.
- Expected complete-suite count after the new contract passes: 977 tests.

## Recommended verification

Pull current `master` and run the focused context-store contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~GetCurrentContext_WhenStoreThrows_ReturnsStableFailure"
```

Expected result after the production fix: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: 977/977 GREEN.

## Next recommended step

After 977/977 GREEN is confirmed, add one focused cancellation contract proving that an `OperationCanceledException` from `IErrorCatalogContextStore.GetCurrent()` propagates as the exact original instance rather than becoming `WIF_CONTEXT_STORE_FAILED`.

Because all descriptor/profile public paths share `GetCurrentContextResponse()`, avoid duplicating ordinary-exception symmetry tests across every method unless a later refactor creates separate paths.

If cancellation passes without production changes, consider the runtime `GetCurrent()` boundary complete and inspect the next distinct dependency boundary, especially `_contextStore.Set(...)`, `_builtInContextProvider.LoadAsync(...)`, or `_initializer.InitializeAsync(...)`, based on existing coverage.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
