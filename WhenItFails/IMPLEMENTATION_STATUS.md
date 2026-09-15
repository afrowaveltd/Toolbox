# Implementation status

Last updated: 2026-09-15

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite baseline: **1027/1027 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies null `ErrorCatalogDocument.Errors` as malformed input rather than resolver failure.
- The null `ErrorProfileDefinition.IncludeOwners` RED is locally confirmed and the smallest production guard is committed; focused/full GREEN verification is pending.

## 2026-09-15 — profile selection null include-owners fix

Contract commit:
`22c8e33b28edc0501cf4f9202e57fed6eea7d8ac`

Production fix commit:
`19ff6c4c97fe967ba9ee6e7159ac436581e08505`

Baseline checkpoint commit:
`1ad1743b1aebec4ca5d19bc036b1e1046a5895be`

Focused RED was locally confirmed:

```text
Expected: Invalid
Actual:   Failed
```

The null `ErrorProfileDefinition.IncludeOwners` collection reached `ErrorProfileResolver.Resolve(...)` and was therefore misclassified by the dependency exception boundary as `Failed / WIF_PROFILE_RESOLVER_FAILED`.

Production change in `WhenItFails/Resolution/ErrorProfileSelectionService.cs` is intentionally minimal: after the matching profile is found and before invoking the resolver, the service now validates `profile.IncludeOwners` and reuses the existing validator contract:

```text
Status: Invalid
Data: null
Code: ProfileIncludeOwnersCollectionIsNull
Message: Profile include owners collection is null.
```

No resolver exception, cancellation, profile lookup, or other response behavior was changed.

Expected complete-suite count after verification: **1028/1028 GREEN with zero compiler warnings**.

## 2026-09-15 — 1027/1027 GREEN null error collection checkpoint

Malformed-context contract commit:
`d64257e3525057192981f755a9948b4d28123467`

Production guard commit:
`9395b64ab28fadf1a499bc26db14e94eb6936ba6`

Checkpoint commit:
`1ad1743b1aebec4ca5d19bc036b1e1046a5895be`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1027
Skipped:  0
Total:  1027
Compiler warnings: 0
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` rejects a null `ErrorCatalogDocument.Errors` collection before invoking the resolver and reuses the established validator/cross-validator contract:

```text
Status: Invalid
Data: null
Code: CatalogErrorsCollectionIsNull
Message: Error catalog errors collection is null.
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

## Established documented behavior — preserve

`JsonCatalogDocumentLoader.InvalidJson` deliberately includes the JSON parser message. `WhenItFails/Docs/Loading-and-Normalization/en.md` documents this behavior; do not sanitize it as incidental hardening.

## Recent verified checkpoints

- 1024/1024 — writer pre-cancellation side-effect contract complete.
- 1025/1025 — profile resolver ordinary-exception normalization complete.
- 1026/1026 — profile resolver exact-cancellation propagation complete.
- 1027/1027 — profile selection null error collection classification complete.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveByProfileName_WhenProfileIncludeOwnersCollectionIsNull_ReturnsInvalidResponse"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1028/1028 GREEN
Compiler warnings: 0
```

## Next recommended step

After **1028/1028 GREEN** is confirmed, record the checkpoint. Then decide whether to continue the same malformed-profile pattern one collection at a time for the remaining resolver-consumed profile collections (`IncludeCodeGroups`, `IncludeCategories`, `IncludeSubcategories`, `IncludeTags`, `ExcludeTags`, `IncludeErrors`, `ExcludeErrors`) or move to another core boundary if reconnaissance finds a higher-value gap.
