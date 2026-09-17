# Implementation status

Last updated: 2026-09-17

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context/configuration handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite baseline: **1039/1039 GREEN**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` now classifies null `ErrorCatalogDocument.Errors`, null error definitions, all three resolver-consumed `ErrorDefinition` collections (`Categories`, `Subcategories`, `Tags`), and all resolver-consumed profile collections as malformed input rather than resolver failure.
- Scalar `ErrorDefinition` reconnaissance did not reveal an exception boundary: `ErrorProfileResolver` treats missing scalar filter values as non-matches. Duplicating the full validator inside selection service is therefore intentionally deferred.
- Fresh bootstrap reconnaissance found a concrete malformed-configuration gap: `JsonsBootstrapper` consumes `JsonsOptions` before `ErrorCatalogContextProvider`, but does not currently reuse the provider's stable path-option validation contracts.

## 2026-09-17 — bootstrap whitespace package-directory-name contract

Contract commit:
`e3ddf3e94be3e562382a1fcd7b27719fa29fd90d`

Baseline checkpoint commit:
`715659225484ff68d50a4c9d92185ba960587eda`

Added:

`WhenItFails.Tests/Bootstrap/JsonsBootstrapperWhitespacePackageDirectoryNameContractTests.cs`

Contract:

`EnsureWorkspaceAsync_WhenPackageDirectoryNameIsWhitespace_ReturnsInvalidWithoutCreatingWorkspace`

`ErrorCatalogContextProvider.ValidateJsonsOptions(...)` already defines the stable contract for the same `JsonsOptions` field:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_EMPTY
Message: The package directory name cannot be empty.
```

The focused bootstrap contract additionally requires no filesystem side effect: an invalid package directory name must be rejected before the root workspace directory is created.

Production is intentionally unchanged before the focused run. Current `JsonsBootstrapper` trims whitespace to an empty package name, combines it with `RootDirectory`, creates/uses the root itself as the package directory, and with an empty template provider returns `Success`.

Expected current focused result:

```text
Expected: Invalid
Actual:   Success
```

Expected eventual complete-suite count after the contract passes: **1040/1040 GREEN**.

## 2026-09-17 — 1039/1039 GREEN null error-categories checkpoint

Contract commit:
`f34a2c0a3c90920771db962a7140ab881af53b2a`

Production guard commit:
`2872fce9108a85ebb76d0836c426df523ae1dbd9`

Checkpoint commit:
`715659225484ff68d50a4c9d92185ba960587eda`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1039
Skipped:  0
Total:  1039
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` rejects an `ErrorDefinition` whose `Categories` collection is null before invoking the resolver and reuses the established validator/cross-validator contract:

```text
Status: Invalid
Data: null
Code: ErrorCategoriesCollectionIsNull
Message: Error categories collection is null.
```

This completes the current resolver-consumed collection-shape audit for `ErrorProfileSelectionService`.

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

- 1037/1037 — profile selection null error-subcategories classification complete.
- 1038/1038 — profile selection null error-tags classification complete.
- 1039/1039 — profile selection null error-categories classification complete; resolver-consumed collection-shape audit closed.

## Recommended verification

Pull current `master` and run only the focused bootstrap contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~EnsureWorkspaceAsync_WhenPackageDirectoryNameIsWhitespace_ReturnsInvalidWithoutCreatingWorkspace"
```

Production must remain unchanged until the local RED confirms the current bootstrap behavior.

## Next recommended step

If the focused run confirms `Expected: Invalid / Actual: Success`, add the smallest `JsonsBootstrapper` precondition for whitespace `PackageDirectoryName` using the already established `WIF_JSONS_PACKAGE_DIRECTORY_NAME_EMPTY` contract and ensure it runs before any filesystem operation. Then run focused and complete suites before expanding to the adjacent `RootDirectory` path contracts.
