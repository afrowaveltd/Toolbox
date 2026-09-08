# Implementation status

Last updated: 2026-09-08

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and service boundaries against malformed dependency behavior, raw exception leakage, cancellation corruption, and compiler warnings in contract tests.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service and profile-selection exception/cancellation boundaries are complete.
- `IErrorCatalogContextStore.GetCurrent()` null-response, ordinary-exception, and exact-instance cancellation behavior are complete.
- `IErrorCatalogInitializer.InitializeAsync(...)` null-response, ordinary-exception, and exact-instance cancellation behavior are complete.
- `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` is hardened for both explicit `ResetToDefaultsAsync()` and flexible initialization fallback paths, including null response, ordinary exception, and exact-instance cancellation contracts.
- The complete `WhenItFails.Tests` suite is locally verified GREEN at **984/984 tests** before the warning cleanup.
- The prior `CS8767` warning in `ErrorCatalogProviderNullFirstIssueContractTests.cs` was caused by a nullable-interface mismatch in the `UnexpectedValidator` test sentinel.

## Latest committed steps

### 2026-09-08 — nullability warning cleanup

Commit: `431f9132ed8bfc19b6eb8f050413f6cba65c4395`

Changed only:

```csharp
public ErrorCatalogValidationResult Validate(ErrorCatalogDocument document)
```

to:

```csharp
public ErrorCatalogValidationResult Validate(ErrorCatalogDocument? document)
```

This aligns the test sentinel with `IErrorCatalogValidator.Validate(ErrorCatalogDocument? document)` and should remove compiler warning `CS8767` without changing runtime behavior or test count.

The commit diff was checked and contains exactly this one nullability annotation change.

### 2026-09-08 — 984/984 GREEN checkpoint

Checkpoint commit: `2496b0c993704ad5c8734496b441c15c490fdf77`

The flexible-fallback built-in-provider cancellation contract is locally verified GREEN. The exact original `OperationCanceledException` instance propagates unchanged through the flexible initialization fallback path.

This completes the built-in provider boundary for both invocation sites in the current scope.

## Verification state

- Last locally verified suite result: **984/984 GREEN**.
- Warning cleanup is committed but has not yet been locally re-run after the change.
- Expected next result: **984/984 GREEN with zero compiler warnings**.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests
```

Expected:

```text
Failed:   0
Passed: 984
Skipped:  0
Total:  984
```

and no `CS8767` warning.

## Next recommended step

After **984/984 GREEN with zero compiler warnings** is confirmed, inspect `_contextStore.Set(...)` as the next distinct runtime dependency boundary. Keep ordinary-exception and cancellation behavior separate and test-first.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
