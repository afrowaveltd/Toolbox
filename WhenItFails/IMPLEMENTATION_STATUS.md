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
- Context-store null-response behavior is broadly covered across `GetCurrentContext()`, all descriptor entry points, and `ResolveProfile(...)`.
- `GetCurrentContextResponse()` converts ordinary `IErrorCatalogContextStore.GetCurrent()` exceptions into `WIF_CONTEXT_STORE_FAILED` without exposing raw exception text.
- The complete `WhenItFails.Tests` suite is verified GREEN at 977/977 tests.
- A focused context-store cancellation contract now requires the exact original `OperationCanceledException` instance to propagate unchanged.

## Latest committed steps

### 2026-09-08 — ErrorCatalogRuntime context-store cancellation contract

Contract commit: `bde451cb05a9484b193b347f042c728cbbe0340f`

Updated:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeContextStoreExceptionContractTests.cs`

Added:

`GetCurrentContext_WhenStoreCancels_RethrowsSameOperationCanceledException`

Contract:

```text
IErrorCatalogContextStore.GetCurrent()
    => OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)`, so future refactoring cannot wrap cancellation, replace it with another cancellation exception, or convert it into `WIF_CONTEXT_STORE_FAILED`.

`GetCurrentContext()` remains the narrowest public path for this shared boundary. The test fixture was lightly refactored through a shared `CreateRuntime(...)` helper; production code is unchanged.

The current `GetCurrentContextResponse()` catch filter excludes `OperationCanceledException`, so this focused contract is expected to be GREEN without a production fix.

### 2026-09-08 — verified ErrorCatalogRuntime context-store exception fix

Production fix commit: `e83d2e60e5a4ac049ff3a92003b705f8a8f41872`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 977
Skipped:  0
Total:  977
```

This confirms that ordinary context-store exceptions become:

```text
Status: Failed
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

without exposing the original dependency exception text.

### 2026-09-08 — verified ErrorCatalogRuntime profile-selection cancellation contract

Contract commit: `15be30a6e03f333c9f2153124591aac79a82bf5b`

Verified locally at 976/976 tests GREEN. The exact original `OperationCanceledException` instance propagates through the runtime profile-selection boundary.

## Verification state

- Complete verified continuation baseline: 977/977 tests GREEN.
- Runtime descriptor-service ordinary-exception and cancellation behavior is complete for the current scope.
- Runtime profile-selection ordinary-exception and cancellation behavior is complete for the current scope.
- Runtime context-store null-response and ordinary-exception behavior is verified GREEN.
- Context-store cancellation contract is committed and awaits focused local verification.
- No production change is expected for the cancellation contract.

## Recommended verification

Pull current `master` and run the focused cancellation contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~GetCurrentContext_WhenStoreCancels_RethrowsSameOperationCanceledException"
```

If green, run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite count: 978 tests.

## Next recommended step

After 978/978 GREEN is confirmed, consider the runtime `IErrorCatalogContextStore.GetCurrent()` boundary complete for the current scope.

Then inspect the next distinct runtime dependency boundary. Strong candidates are `_contextStore.Set(...)`, `_builtInContextProvider.LoadAsync(...)`, and `_initializer.InitializeAsync(...)`; choose the next contract only after checking existing tests so null/malformed/exception behavior is not duplicated unnecessarily.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
