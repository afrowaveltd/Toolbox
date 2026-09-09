# Implementation status

Last updated: 2026-09-09

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening initialization and catalog dependency boundaries against malformed behavior, raw exception leakage, and cancellation corruption.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service and profile-selection exception/cancellation boundaries are complete.
- All currently known `IErrorCatalogContextStore` read/write boundaries in the active runtime/initializer scope are complete.
- `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` is complete for the current scope: null-response, ordinary-exception, and exact-instance cancellation behavior are covered.
- `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` as consumed by `ErrorCatalogInitializer` is complete for the current scope: null-response, ordinary-exception, and exact-instance cancellation behavior are covered.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 994/994 tests with zero compiler warnings**.
- Reconnaissance of `ErrorCatalogContextProvider` found five sequential internal provider calls whose null-response behavior is already covered but whose ordinary exception/cancellation boundaries are not yet hardened individually.

## Latest committed steps

### 2026-09-09 — 994/994 GREEN initializer context-provider checkpoint

Checkpoint commit: this commit.

Cancellation contract commit: `94bf93da613fccd2204eb266055613f8dabfc763`

Production exception guard commit: `f5485fd244b860514c0f683bee855e6cc6e7a69c`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 994
Skipped:  0
Total:  994
Compiler warnings: 0
```

The exact original `OperationCanceledException` instance from `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` propagates unchanged through `ErrorCatalogInitializer.InitializeAsync(...)`. Together with the existing null-response and ordinary-exception contracts, this completes the initializer context-provider boundary for the current scope.

## Reconnaissance

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` currently invokes these dependencies in order:

1. `IErrorCatalogProvider.LoadFromFileAsync(...)`
2. `IErrorCategoryCatalogProvider.LoadFromFileAsync(...)`
3. `IErrorCodeGroupCatalogProvider.LoadFromFileAsync(...)`
4. `IErrorOwnerCatalogProvider.LoadFromFileAsync(...)`
5. `IErrorProfileCatalogProvider.LoadFromFileAsync(...)`

All five calls already have stable null-response handling (`WIF_*_PROVIDER_RESPONSE_NULL`), but the awaits themselves are currently unguarded against ordinary dependency exceptions.

## Verification state

- Clean continuation baseline: **994/994 GREEN, zero compiler warnings**.
- Initializer bootstrapper boundary is complete for the current scope.
- Initializer context-provider boundary is complete for the current scope.
- The next target is the first internal dependency in `ErrorCatalogContextProvider`: `IErrorCatalogProvider.LoadFromFileAsync(...)`.

## Next recommended step

Add one focused ordinary-exception contract for `_errorCatalogProvider.LoadFromFileAsync(...)` inside `ErrorCatalogContextProvider.LoadFromJsonsAsync(...)`.

Use the stable dependency contract:

```text
Status: Failed
Data: null
Code: WIF_ERROR_CATALOG_PROVIDER_FAILED
Message: The error catalog provider failed.
```

The raw provider exception text must not escape. Do not change production code until the focused RED is observed.

After that contract is fixed and GREEN, add exact-instance cancellation for the same first provider before moving to category/code-group/owner/profile providers.

Keep changes small, tested, documented here, and committed directly to `master`.
