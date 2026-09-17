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
- `JsonsBootstrapper` whitespace `PackageDirectoryName` RED is locally confirmed and the smallest pre-filesystem production guard is committed; focused/full GREEN verification is pending.

## 2026-09-17 — bootstrap whitespace package-directory-name fix

Contract commit:
`e3ddf3e94be3e562382a1fcd7b27719fa29fd90d`

Production guard commit:
`c1ca5693f37bc129edde761b2452f4548edc2a0b`

Baseline checkpoint commit:
`715659225484ff68d50a4c9d92185ba960587eda`

Contract:

`EnsureWorkspaceAsync_WhenPackageDirectoryNameIsWhitespace_ReturnsInvalidWithoutCreatingWorkspace`

`ErrorCatalogContextProvider.ValidateJsonsOptions(...)` already defines the stable contract for the same `JsonsOptions` field:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_EMPTY
Message: The package directory name cannot be empty.
```

The focused run confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, `JsonsBootstrapper` trimmed whitespace to an empty package name, combined it with `RootDirectory`, treated the root itself as the package directory, and could create/use that directory before returning success.

Production change in `WhenItFails/Bootstrap/JsonsBootstrapper.cs` is intentionally narrow: a non-null whitespace `PackageDirectoryName` is now rejected with the established contract above immediately after cancellation/options checks and before entering the filesystem block.

The null `PackageDirectoryName` case is intentionally unchanged until it has its own focused contract, because the established provider contract distinguishes null from empty configuration.

Expected complete-suite count after verification: **1040/1040 GREEN**.

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

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~EnsureWorkspaceAsync_WhenPackageDirectoryNameIsWhitespace_ReturnsInvalidWithoutCreatingWorkspace"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1040/1040 GREEN
```

## Next recommended step

After **1040/1040 GREEN** is confirmed, record the checkpoint and continue the `JsonsOptions` bootstrap-boundary audit one concrete contract at a time. The adjacent null `PackageDirectoryName` and malformed/empty `RootDirectory` cases already have stable contracts in `ErrorCatalogContextProvider`; add no production behavior until each focused test establishes the current bootstrap response/side-effect shape.
