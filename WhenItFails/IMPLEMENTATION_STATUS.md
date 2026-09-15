# Implementation status

Last updated: 2026-09-15

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite baseline: **1026/1026 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary exception normalization and exact-instance cancellation propagation.
- A new malformed-context contract for null `ErrorCatalogDocument.Errors` is committed and awaits focused RED verification.

## 2026-09-15 — profile selection null errors collection contract

Contract commit:
`d64257e3525057192981f755a9948b4d28123467`

Baseline checkpoint commit:
`e8d0ca536f11d6a7ba16fddb55722abe6315a3ac`

Added:

`WhenItFails.Tests/Resolution/ErrorProfileSelectionServiceNullErrorsCollectionContractTests.cs`

Contract:

`ResolveByProfileName_WhenErrorCatalogErrorsCollectionIsNull_ReturnsInvalidResponse`

Rationale:

`ErrorProfileSelectionService` already validates malformed context structure before invoking the injected resolver: null context, null error catalog document, null profile catalog, null profile collection and null profile definitions all return `Invalid` responses. A null `ErrorCatalogDocument.Errors` collection is likewise malformed input and should not be misclassified as an injected resolver failure.

The repository already uses one stable code/message for this condition in both `ErrorCatalogValidator` and `ErrorCatalogCrossValidator`:

```text
Code: CatalogErrorsCollectionIsNull
Message: Error catalog errors collection is null.
```

Expected selection-service response:

```text
Status: Invalid
Data: null
Code: CatalogErrorsCollectionIsNull
Message: Error catalog errors collection is null.
```

Production is intentionally unchanged before the focused RED run. With the current implementation, the null collection reaches `ErrorProfileResolver.Resolve(...)`, throws internally, and is expected to be normalized by the recently added resolver boundary as `Failed / WIF_PROFILE_RESOLVER_FAILED`. The focused contract should therefore RED on response shape, not by leaking an exception.

Expected eventual complete-suite count after the contract passes: **1027/1027 GREEN with zero compiler warnings**.

## 2026-09-15 — 1026/1026 GREEN profile resolver cancellation checkpoint

Profile resolver ordinary-exception contract commit:
`3a3a81a7c816fd72d63c5f1450d07c675bb12288`

Production normalization fix commit:
`7d739c3d9499951304da0d3463308668b7b67e19`

Exact-cancellation contract commit:
`5a3ed3ea2091c919f11bfb843b1964a9588181cc`

Checkpoint commit:
`e8d0ca536f11d6a7ba16fddb55722abe6315a3ac`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1026
Skipped:  0
Total:  1026
Compiler warnings: 0
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` explicitly guarantees:

- null resolver result → `Invalid / WIF_PROFILE_RESOLVER_RESULT_NULL`;
- ordinary resolver exception → `Failed / WIF_PROFILE_RESOLVER_FAILED`;
- public message: `The error profile resolver failed.`;
- raw dependency exception detail does not escape;
- exact supplied `OperationCanceledException` instance propagates unchanged.

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

## Recommended verification

Pull current `master` and run only the focused contract first:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveByProfileName_WhenErrorCatalogErrorsCollectionIsNull_ReturnsInvalidResponse"
```

Expected current result: **RED** because the service currently returns `Failed / WIF_PROFILE_RESOLVER_FAILED` instead of the expected malformed-input `Invalid / CatalogErrorsCollectionIsNull` response.

## Next recommended step

If RED confirms that response-shape mismatch, add the smallest production guard in `ErrorProfileSelectionService.ResolveByProfileName(...)` immediately after the null document check. Reuse exactly:

```text
CatalogErrorsCollectionIsNull
Error catalog errors collection is null.
```

Then run the focused contract and the complete suite. Record **1027/1027 GREEN** before moving on.
