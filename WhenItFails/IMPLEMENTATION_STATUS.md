# Implementation status

Last updated: 2026-09-09

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening initialization dependency boundaries against malformed behavior, raw exception leakage, and cancellation corruption.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service and profile-selection exception/cancellation boundaries are complete.
- All currently known `IErrorCatalogContextStore` read/write boundaries in the active runtime/initializer scope are complete.
- `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` null-response and ordinary-exception behavior are verified.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 991/991 tests with zero compiler warnings**.
- A focused exact-instance cancellation contract is now committed for the initializer bootstrapper invocation and awaits local verification.

## Latest committed steps

### 2026-09-09 — initializer bootstrapper cancellation contract

Contract commit: `dc7700c61dfc0165a374fafac8c98145be4ca8ad`

Updated:

`WhenItFails.Tests/Initialization/ErrorCatalogInitializerBootstrapperExceptionContractTests.cs`

Added:

`InitializeAsync_WhenBootstrapperCancels_RethrowsSameOperationCanceledException`

Contract:

```text
IJsonsBootstrapper.EnsureWorkspaceAsync(...)
    => throws a specific OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)`, so cancellation cannot be wrapped, replaced, or normalized into `WIF_INITIALIZER_BOOTSTRAPPER_FAILED`.

It also verifies that the context provider is not invoked and the previous context remains unchanged when bootstrapper cancellation occurs.

The test diff was checked and contains only the new cancellation contract and its `CancelingBootstrapper` fixture.

No production code changed. The current bootstrapper catch filter excludes `OperationCanceledException`, so the focused contract is expected to be GREEN.

### 2026-09-09 — 991/991 GREEN bootstrapper exception checkpoint

Checkpoint commit: `c6a5ec7899baf133644fb9f7eab69ac66957552a`

Production fix commit: `c4951648527c5fa123f64714e2f87e1c0352e125`

Ordinary-exception contract commit: `cfe6fc4caf24bcb3a4eda476f135b29f868043d6`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 991
Skipped:  0
Total:  991
Compiler warnings: 0
```

The initializer bootstrapper ordinary exception is converted to `WIF_INITIALIZER_BOOTSTRAPPER_FAILED` without leaking raw dependency text.

## Verification state

- Clean continuation baseline: **991/991 GREEN, zero compiler warnings**.
- Bootstrapper null-response and ordinary-exception behavior are verified.
- Exact-instance cancellation contract for the same bootstrapper invocation is committed and awaits focused local verification.
- No production change is expected for this cancellation contract.
- Expected complete-suite count after the contract passes: **992 tests**.

## Recommended verification

Pull current `master` and run the focused cancellation contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenBootstrapperCancels_RethrowsSameOperationCanceledException"
```

Expected result: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **992/992 GREEN with zero compiler warnings**.

## Next recommended step

After 992/992 GREEN is confirmed, consider the initializer bootstrapper boundary complete for the current scope.

Then move separately to `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)`: start with one focused ordinary-exception contract, observe RED, and only then add the smallest production boundary. Follow with exact-instance cancellation as a separate contract.

Do not combine bootstrapper and context-provider hardening in one production step.

Keep changes small, tested, documented here, and committed directly to `master`.
