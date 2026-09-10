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
- `ErrorCatalogInitializer` bootstrapper ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1016/1016 tests with zero compiler warnings** before the new initializer context-provider null-task contract.
- A focused null-task contract for `ErrorCatalogInitializer`'s injected `IErrorCatalogContextProvider` is committed and awaits local verification.

## 2026-09-10 — initializer context-provider null-task contract

Contract commit: `40404b8e07fef0bb24e2f3fb5251cda03c99cd8b`.
Baseline checkpoint commit: `2e650171ed1aac8cb57b5586b57b9d041c220cd1`.

Updated:

`WhenItFails.Tests/Initialization/ErrorCatalogInitializerContextProviderExceptionContractTests.cs`

Added:

`InitializeAsync_WhenContextProviderReturnsNullTask_ReturnsStableFailure`

The fake `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` returns `null!` instead of a `Task<Response<ErrorCatalogContext>>`.

Required contract:

```text
Status: Failed
Data: null
Code: WIF_INITIALIZER_CONTEXT_PROVIDER_FAILED
Message: The error catalog context provider failed during initialization.
```

The previously stored context must remain the exact same instance. `TrackingContextStore.Set(...)` throws if reached, so the test also guarantees no replacement attempt occurs after the malformed dependency result.

No production code changed. The context-provider `await` is already inside the initializer's ordinary-exception normalization guard, so the null-task `NullReferenceException` is expected to normalize to the established context-provider failure.

## 2026-09-10 — 1016/1016 GREEN initializer bootstrapper null-task checkpoint

Checkpoint commit: `2e650171ed1aac8cb57b5586b57b9d041c220cd1`.
Bootstrapper null-task contract commit: `84f3fe36469882aedfed03507d857b267f89d1c1`.
Previous 1015 checkpoint commit: `08719f81d1cf1487fce2dcebf0b6bbcd208ba0c1`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1016
Skipped:  0
Total:  1016
Compiler warnings: 0
```

The initializer bootstrapper dependency explicitly covers:

- null `Response` → `WIF_INITIALIZER_BOOTSTRAPPER_RESPONSE_NULL`;
- ordinary exception → `WIF_INITIALIZER_BOOTSTRAPPER_FAILED`;
- null `Task` → `WIF_INITIALIZER_BOOTSTRAPPER_FAILED`;
- exact `OperationCanceledException` instance propagates unchanged;
- downstream context provider does not run after bootstrapper failure;
- previously stored context is preserved.

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

`BuiltInErrorCatalogContextProvider` explicitly covers template-provider and injected context-provider null/malformed/ordinary-exception/cancellation behavior for the current scope, including null `Task` normalization at the injected async context-provider boundary.

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

Fresh null-task reconnaissance found no prior dedicated initializer null-task contract for `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)`.

Both async initializer dependency awaits now have explicit malformed-null-task contracts in tests.

## Verification state

- Clean continuation baseline: **1016/1016 GREEN, zero compiler warnings**.
- Initializer bootstrapper null-task behavior is locally verified.
- Initializer context-provider null-task contract is committed and awaits local verification.
- No production change is expected.
- Expected complete-suite count after it passes: **1017/1017 GREEN with zero compiler warnings**.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenContextProviderReturnsNullTask_ReturnsStableFailure"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1017/1017 GREEN
Compiler warnings: 0
```

## Next recommended step

After **1017/1017 GREEN** is confirmed, record that checkpoint and perform fresh reconnaissance for the next smallest async dependency null-task or malformed-result gap outside the already completed initializer and built-in context-provider boundaries.

Keep changes small, tested, documented here, and committed directly to `master`.
