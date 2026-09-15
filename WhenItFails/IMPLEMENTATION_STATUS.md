# Implementation status

Last updated: 2026-09-15

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and failure cleanup while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite: **1026/1026 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary exception normalization and exact-instance cancellation propagation.

## 2026-09-15 — 1026/1026 GREEN profile resolver cancellation checkpoint

Profile resolver ordinary-exception contract commit:
`3a3a81a7c816fd72d63c5f1450d07c675bb12288`

Production normalization fix commit:
`7d739c3d9499951304da0d3463308668b7b67e19`

Exact-cancellation contract commit:
`5a3ed3ea2091c919f11bfb843b1964a9588181cc`

Previous checkpoint commit:
`05acce74af16be90cdaa967d0239a474c777025a`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1026
Skipped:  0
Total:  1026
Compiler warnings: 0
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` now explicitly guarantees:

- null resolver result → `Invalid / WIF_PROFILE_RESOLVER_RESULT_NULL`;
- ordinary resolver exception → `Failed / WIF_PROFILE_RESOLVER_FAILED`;
- public message: `The error profile resolver failed.`;
- raw dependency exception detail does not escape;
- exact supplied `OperationCanceledException` instance propagates unchanged.

No production change was required for the cancellation contract because the ordinary-exception filter already excludes `OperationCanceledException`.

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

- 1022/1022 — `JsonsBootstrapper` template-provider cancellation complete.
- 1023/1023 — writer serialization-failure temporary-file cleanup complete.
- 1024/1024 — writer pre-cancellation side-effect contract complete.
- 1025/1025 — profile resolver ordinary-exception normalization complete.
- 1026/1026 — profile resolver exact-cancellation propagation complete.

## Next recommended step

Perform fresh reconnaissance outside the already hardened areas. Prefer a real public/injected dependency boundary where behavior is asymmetric (for example null result is normalized but exceptions are not), and search existing propagation/exception-shape/cancellation contracts before adding a test.

Work test-first and one boundary at a time:

1. add one focused contract;
2. commit test;
3. update this file;
4. run focused test locally;
5. make the smallest production change only if RED confirms a real gap;
6. run the complete suite;
7. record the new GREEN checkpoint.
