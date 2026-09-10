# Implementation status

Last updated: 2026-09-10

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening dependency boundaries while preserving established public exception contracts, including malformed async dependency results such as null `Task` values.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogRuntime` dependency boundaries covered so far are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `ErrorCatalogContextProvider` is intentionally transparent for exceptions and null tasks from its internal providers; do not normalize those lower-layer contracts.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope, including injected context-provider null `Task` normalization.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1015/1015 tests with zero compiler warnings** before the new initializer bootstrapper null-task contract.
- A focused null-task contract for `ErrorCatalogInitializer`'s `IJsonsBootstrapper` dependency is committed and awaits local verification.

## 2026-09-10 — initializer bootstrapper null-task contract

Contract commit: `84f3fe36469882aedfed03507d857b267f89d1c1`.
Baseline checkpoint commit: `08719f81d1cf1487fce2dcebf0b6bbcd208ba0c1`.

Updated:

`WhenItFails.Tests/Initialization/ErrorCatalogInitializerBootstrapperExceptionContractTests.cs`

Added:

`InitializeAsync_WhenBootstrapperReturnsNullTask_ReturnsStableFailure`

The fake `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` returns `null!` instead of a `Task<Response<JsonsBootstrapPayload>>`.

Required contract:

```text
Status: Failed
Data: null
Code: WIF_INITIALIZER_BOOTSTRAPPER_FAILED
Message: The JSON workspace bootstrapper failed.
```

The downstream context provider must not run and the previously stored context must remain the exact same instance.

No production code changed. The bootstrapper `await` is already inside the initializer's ordinary-exception normalization guard, so the null-task `NullReferenceException` is expected to normalize to the established bootstrapper failure.

## 2026-09-10 — 1015/1015 GREEN built-in context-provider null-task checkpoint

Checkpoint commit: `08719f81d1cf1487fce2dcebf0b6bbcd208ba0c1`.
Null-task contract commit: `ba3a8533c7d33f352bb43d626fa032b2e906daac`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1015
Skipped:  0
Total:  1015
Compiler warnings: 0
```

`BuiltInErrorCatalogContextProvider` now explicitly covers template-provider and injected context-provider null/malformed/ordinary-exception/cancellation behavior for the current scope, including null `Task` normalization at the injected async context-provider boundary.

## Established transparent lower boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` intentionally preserves exceptions and null-task behavior from its five internal catalog providers.

Relevant suites include:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderOwnerNullTaskTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProfileNullTaskTests.cs`

Do not replace those transparent contracts with normalization at that layer.

## Reconnaissance notes

`JsonCatalogDocumentLoader.InvalidJson` deliberately includes the parser message; `Docs/Loading-and-Normalization/en.md` documents that behavior. Do not sanitize it as an incidental hardening change.

Fresh null-task reconnaissance found no previous dedicated initializer null-task contracts for either:

1. `IJsonsBootstrapper.EnsureWorkspaceAsync(...)`;
2. `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)`.

Both awaits currently sit inside ordinary-exception normalization guards.

## Verification state

- Clean continuation baseline: **1015/1015 GREEN, zero compiler warnings**.
- New initializer bootstrapper null-task contract is committed and awaiting focused local verification.
- No production change is expected.
- Expected complete-suite count after it passes: **1016/1016 GREEN with zero compiler warnings**.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenBootstrapperReturnsNullTask_ReturnsStableFailure"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1016/1016 GREEN
Compiler warnings: 0
```

## Next recommended step

After **1016/1016 GREEN** is confirmed, record the checkpoint and add a separate focused null-task contract for `ErrorCatalogInitializer`'s injected `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` dependency.

That second contract should verify stable `WIF_INITIALIZER_CONTEXT_PROVIDER_FAILED`, no context-store replacement, and no production change if current behavior is preserved.

Keep changes small, tested, documented here, and committed directly to `master`.
