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
- The prior `CS8767` warning in `ErrorCatalogProviderNullFirstIssueContractTests.cs` is fixed by aligning the test sentinel with the nullable `IErrorCatalogValidator` contract.
- `_contextStore.Set(...)` remains a distinct unguarded dependency boundary at three invocation sites: `ResetToDefaultsAsync()`, flexible fallback in `ErrorCatalogRuntime`, and `ErrorCatalogInitializer`.

## Latest committed steps

### 2026-09-08 — clean 984/984 checkpoint

Verified locally after warning cleanup:

```text
WhenItFails.Tests
Failed:   0
Passed: 984
Skipped:  0
Total:  984
Compiler warnings: 0
```

This confirms commit `431f9132ed8bfc19b6eb8f050413f6cba65c4395` removed `CS8767` without changing behavior or test count.

### 2026-09-08 — nullability warning cleanup

Commit: `431f9132ed8bfc19b6eb8f050413f6cba65c4395`

Changed only the `UnexpectedValidator.Validate(...)` test sentinel parameter from `ErrorCatalogDocument` to `ErrorCatalogDocument?`.

### 2026-09-08 — built-in provider boundary complete

Both `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` invocation sites now have null-response, ordinary-exception, and exact-instance cancellation coverage.

## Verification state

- Current clean continuation baseline: **984/984 GREEN, zero compiler warnings**.
- Built-in provider boundary is complete for the current scope.
- `_contextStore.Set(...)` is the next distinct runtime dependency boundary.

## Next recommended step

Add one focused test-first contract for the explicit `ResetToDefaultsAsync()` path:

```text
built-in provider returns a valid context
    -> IErrorCatalogContextStore.Set(...) throws ordinary exception
    -> stable Failed response
       Code: WIF_CONTEXT_STORE_FAILED
       Message: The error catalog context store failed.
```

The raw store exception message must not escape. Do not modify production code until the RED state is observed.

After that contract is fixed and verified, add exact-instance cancellation coverage for the same `Set(...)` site, then inspect the separate flexible-fallback `Set(...)` invocation.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
