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
- A focused malformed-profile contract for null `ErrorProfileDefinition.IncludeOwners` is committed and awaits local RED verification.

## 2026-09-15 — profile selection null include-owners contract

Contract commit:
`22c8e33b28edc0501cf4f9202e57fed6eea7d8ac`

Baseline checkpoint commit:
`1ad1743b1aebec4ca5d19bc036b1e1046a5895be`

Added:

`WhenItFails.Tests/Resolution/ErrorProfileSelectionServiceNullIncludeOwnersCollectionContractTests.cs`

Contract:

`ResolveByProfileName_WhenProfileIncludeOwnersCollectionIsNull_ReturnsInvalidResponse`

`ErrorProfileCatalogValidator` already defines the stable malformed-profile contract:

```text
Status: Invalid
Code: ProfileIncludeOwnersCollectionIsNull
Message: Profile include owners collection is null.
```

The selection service should classify the same malformed profile shape before invoking `IErrorProfileResolver.Resolve(...)` rather than misclassifying the resulting resolver exception as a dependency failure.

Production is intentionally unchanged before the focused run. Current `ErrorProfileResolver.Resolve(...)` calls `CreateNormalizedSet(profile.IncludeOwners)`, so a null collection is expected to throw and then be normalized by `ErrorProfileSelectionService` as `Failed / WIF_PROFILE_RESOLVER_FAILED`.

Expected eventual complete-suite count after this contract passes: **1028/1028 GREEN with zero compiler warnings**.

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

Pull current `master` and run only the focused contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveByProfileName_WhenProfileIncludeOwnersCollectionIsNull_ReturnsInvalidResponse"
```

Expected current result: **RED** with `Actual: Failed` rather than the expected `Invalid`, because the null collection reaches `ErrorProfileResolver` and is normalized as `WIF_PROFILE_RESOLVER_FAILED`.

## Next recommended step

If RED confirms the response-shape mismatch, add the smallest guard for `profile.IncludeOwners is null` after the matching profile is found and before the resolver call. Reuse exactly:

```text
ProfileIncludeOwnersCollectionIsNull
Profile include owners collection is null.
```

Then run focused and complete suites. Record **1028/1028 GREEN** before deciding whether the same malformed-profile pattern warrants one-by-one coverage for the remaining resolver-consumed profile collections.
