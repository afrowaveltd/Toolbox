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
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1016/1016 tests with zero compiler warnings**.

## 2026-09-10 — 1016/1016 GREEN initializer bootstrapper null-task checkpoint

Checkpoint commit: this status commit.
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

The initializer bootstrapper dependency now explicitly covers:

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

Fresh null-task reconnaissance found no previous dedicated initializer null-task contract for `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)`.

The context-provider await in `ErrorCatalogInitializer.InitializeAsync(...)` is already inside the established ordinary-exception normalization guard, so a null `Task` should normalize to:

```text
Status: Failed
Data: null
Code: WIF_INITIALIZER_CONTEXT_PROVIDER_FAILED
Message: The error catalog context provider failed during initialization.
```

The existing context must remain untouched.

## Verification state

- Clean continuation baseline: **1016/1016 GREEN, zero compiler warnings**.
- Initializer bootstrapper null-task behavior is locally verified.
- No new context-provider null-task test has been locally run yet.

## Next recommended step

Add one focused null-task contract for `ErrorCatalogInitializer`'s injected `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` dependency.

The contract should require stable `WIF_INITIALIZER_CONTEXT_PROVIDER_FAILED`, null payload, preservation of the existing context-store instance, and no production change if current guarded-await behavior is preserved.

Keep changes small, tested, documented here, and committed directly to `master`.
