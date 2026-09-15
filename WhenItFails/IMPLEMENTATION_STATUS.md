# Implementation status

Last updated: 2026-09-15

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite: **1027/1027 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` now classifies null `ErrorCatalogDocument.Errors` as malformed input rather than resolver failure.

## 2026-09-15 — 1027/1027 GREEN null error collection checkpoint

Malformed-context contract commit:
`d64257e3525057192981f755a9948b4d28123467`

Production guard commit:
`9395b64ab28fadf1a499bc26db14e94eb6936ba6`

Previous checkpoint commit:
`e8d0ca536f11d6a7ba16fddb55722abe6315a3ac`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1027
Skipped:  0
Total:  1027
Compiler warnings: 0
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` now rejects a null `ErrorCatalogDocument.Errors` collection before invoking the resolver and reuses the established validator/cross-validator contract:

```text
Status: Invalid
Data: null
Code: CatalogErrorsCollectionIsNull
Message: Error catalog errors collection is null.
```

This prevents malformed context from being misclassified as `Failed / WIF_PROFILE_RESOLVER_FAILED`.

## Established transparent lower boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` intentionally preserves exceptions and null-task behavior from its five internal catalog providers.

Relevant suites include:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderOwnerNullTaskTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProfileNullTaskTests.cs`

Do not replace those transparent contracts with normalization at that layer.

## Established documented behavior — preserve

`JsonCatalogDocumentLoader.InvalidJson` deliberately includes the JSON parser message. `WhenItFails/Docs/Loading-and-Normalization/en.md` documents this behavior; do not sanitize it as incidental hardening.

## Recent verified checkpoints

- 1023/1023 — writer serialization-failure temporary-file cleanup complete.
- 1024/1024 — writer pre-cancellation side-effect contract complete.
- 1025/1025 — profile resolver ordinary-exception normalization complete.
- 1026/1026 — profile resolver exact-cancellation propagation complete.
- 1027/1027 — profile selection null error collection classification complete.

## Next recommended step

Perform fresh reconnaissance for the next smallest real malformed-input or injected-dependency asymmetry outside already hardened areas. Search existing propagation, exception-shape, cancellation and malformed-collection contracts before adding a test.

Work test-first and one boundary at a time:

1. add one focused contract;
2. commit test;
3. update this file;
4. run focused test locally;
5. make the smallest production change only if RED confirms a real gap;
6. run the complete suite;
7. record the new GREEN checkpoint.
