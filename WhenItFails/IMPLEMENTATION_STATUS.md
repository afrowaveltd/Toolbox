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
- The malformed-context RED for null `ErrorCatalogDocument.Errors` is locally confirmed and the smallest production guard is committed; focused/full GREEN verification is pending.

## 2026-09-15 — profile selection null errors collection fix

Contract commit:
`d64257e3525057192981f755a9948b4d28123467`

Production fix commit:
`9395b64ab28fadf1a499bc26db14e94eb6936ba6`

Baseline checkpoint commit:
`e8d0ca536f11d6a7ba16fddb55722abe6315a3ac`

Focused RED was locally confirmed:

```text
Expected: Invalid
Actual:   Failed
```

The null `ErrorCatalogDocument.Errors` collection reached `ErrorProfileResolver.Resolve(...)` and was therefore misclassified by the dependency exception boundary as `Failed / WIF_PROFILE_RESOLVER_FAILED`.

Production change in `WhenItFails/Resolution/ErrorProfileSelectionService.cs` is intentionally minimal: immediately after the null document guard, the service now validates `context.ErrorCatalogDocument.Errors` and returns the same stable malformed-input response already used by `ErrorCatalogValidator` and `ErrorCatalogCrossValidator`:

```text
Status: Invalid
Data: null
Code: CatalogErrorsCollectionIsNull
Message: Error catalog errors collection is null.
```

No resolver, cancellation, profile lookup, or other response behavior was changed.

Expected complete-suite count after verification: **1027/1027 GREEN with zero compiler warnings**.

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

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveByProfileName_WhenErrorCatalogErrorsCollectionIsNull_ReturnsInvalidResponse"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1027/1027 GREEN
Compiler warnings: 0
```

## Next recommended step

After **1027/1027 GREEN** is confirmed, record the checkpoint and resume fresh reconnaissance for the next smallest real malformed-input or injected-dependency asymmetry outside already hardened areas.
