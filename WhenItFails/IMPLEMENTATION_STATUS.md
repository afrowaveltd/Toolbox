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
- The complete `WhenItFails.Tests` suite is verified GREEN at 976/976 tests.
- Existing context-store null-response coverage includes `GetCurrentContext()`, all three descriptor entry points, and `ResolveProfile(...)`.
- A new focused contract now defines runtime behavior when `IErrorCatalogContextStore.GetCurrent()` throws an ordinary exception.

## Latest committed steps

### 2026-09-08 — ErrorCatalogRuntime context-store exception contract

Contract commit: `8e892674ecc110bded3ac57040610954768b22a9`

Added:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeContextStoreExceptionContractTests.GetCurrentContext_WhenStoreThrows_ReturnsStableFailure`

Contract:

```text
IErrorCatalogContextStore.GetCurrent() => throws ordinary exception
                         ↓
Status: Failed
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

The injected context store throws an `InvalidOperationException` containing sensitive diagnostic text. The runtime facade must not expose that raw text.

The test uses `GetCurrentContext()` as the narrowest public path and supplies throwing sentinels for every unrelated dependency, so only `GetCurrent()` is allowed to participate.

No production code changed in this step.

Current `GetCurrentContextResponse()` directly calls `_contextStore.GetCurrent()` before its existing null-response guard, so this focused contract is expected to be RED with the original exception escaping.

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

This confirms that an `OperationCanceledException` thrown by `IErrorProfileSelectionService.ResolveByProfileName(...)` propagates through `ErrorCatalogRuntime.ResolveProfile(...)` as the exact original exception instance.

### 2026-09-08 — ErrorCatalogRuntime profile-selection exception fix

Production fix commit: `67771ee0ea22c24d30adef93327535f3690d98af`

Ordinary profile-selection exceptions become `WIF_PROFILE_SELECTION_FAILED` without exposing raw dependency exception text.

## Verification state

- Complete verified continuation baseline: 976/976 tests GREEN.
- Runtime descriptor-service ordinary-exception and cancellation behavior is complete for the current scope.
- Runtime profile-selection ordinary-exception and cancellation behavior is complete for the current scope.
- Context-store null-response behavior is already broadly covered.
- New context-store ordinary-exception contract is committed and awaits focused local verification.
- Production `GetCurrentContextResponse()` remains unchanged until the RED state is observed.

## Recommended verification

Pull current `master` and run only the new context-store contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~GetCurrentContext_WhenStoreThrows_ReturnsStableFailure"
```

Expected current result: RED with the original exception text:

```text
Sensitive runtime context store detail must not escape.
```

Expected complete-suite count once this new contract eventually passes: 977 tests.

## Next recommended step

If the focused context-store contract fails as expected, add the smallest exception boundary around `_contextStore.GetCurrent()` inside `GetCurrentContextResponse()`.

Convert ordinary exceptions into:

```text
Status: Failed
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

while allowing `OperationCanceledException` to propagate unchanged.

Because every descriptor/profile path already flows through `GetCurrentContextResponse()`, do not add symmetry tests in the same production step. Verify the narrow `GetCurrentContext()` contract first, then determine whether shared-path coverage needs one focused follow-up rather than duplicating every public method.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
