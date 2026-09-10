# Implementation status

Last updated: 2026-09-10

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening dependency boundaries while preserving established public exception contracts, including malformed async dependency results such as null `Task` values.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `ErrorCatalogContextProvider` is intentionally transparent for exceptions and null tasks from its internal providers; do not normalize those lower-layer contracts.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope, including injected context-provider null `Task` normalization.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1017/1017 tests with zero compiler warnings**.
- Next work should move outside the completed initializer and built-in context-provider boundaries.

## 2026-09-10 — 1017/1017 GREEN initializer context-provider null-task checkpoint

Checkpoint commit: `93af740552ddf16a1727027db18eddecf9986407`.
Context-provider null-task contract commit: `40404b8e07fef0bb24e2f3fb5251cda03c99cd8b`.
Bootstrapper null-task checkpoint commit: `2e650171ed1aac8cb57b5586b57b9d041c220cd1`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1017
Skipped:  0
Total:  1017
Compiler warnings: 0
```

`ErrorCatalogInitializer` now explicitly covers both async dependency boundaries:

### `IJsonsBootstrapper.EnsureWorkspaceAsync(...)`

- null `Response` → `WIF_INITIALIZER_BOOTSTRAPPER_RESPONSE_NULL`;
- ordinary exception → `WIF_INITIALIZER_BOOTSTRAPPER_FAILED`;
- null `Task` → `WIF_INITIALIZER_BOOTSTRAPPER_FAILED`;
- exact `OperationCanceledException` instance propagates unchanged;
- downstream context provider does not run after bootstrapper failure;
- previously stored context is preserved.

### `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)`

- null `Response` → `WIF_INITIALIZER_CONTEXT_PROVIDER_RESPONSE_NULL`;
- ordinary exception → `WIF_INITIALIZER_CONTEXT_PROVIDER_FAILED`;
- null `Task` → `WIF_INITIALIZER_CONTEXT_PROVIDER_FAILED`;
- exact `OperationCanceledException` instance propagates unchanged;
- context store is not replaced after failure;
- previously stored context is preserved.

No production code was required for either null-task contract because both awaits already sit inside ordinary-exception normalization guards.

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

Initializer and built-in context-provider malformed async null-task edges are now explicit and locally verified.

## Verification state

- Clean continuation baseline: **1017/1017 GREEN, zero compiler warnings**.
- `ErrorCatalogInitializer` async dependency null-task audit is complete for the current scope.
- `BuiltInErrorCatalogContextProvider` async dependency null-task audit is complete for the current scope.
- No unverified test is currently pending.

## Next recommended step

Perform fresh repository reconnaissance for the next smallest unverified dependency boundary outside the completed initializer and built-in context-provider areas.

Before introducing new behavior, search for existing:

- propagation contracts;
- exception-shape contracts;
- cancellation contracts;
- null-task contracts;
- malformed/null `Response` contracts;
- documentation that intentionally exposes dependency/parser details.

Keep changes small, tested, documented here, and committed directly to `master`.
