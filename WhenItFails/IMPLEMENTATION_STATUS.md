# Implementation status

Last updated: 2026-09-17

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context/configuration handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite: **1040/1040 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies all resolver-consumed nullable collections currently audited as malformed input rather than resolver failure.
- Scalar `ErrorDefinition` reconnaissance did not reveal an exception boundary; duplicating the full validator inside selection service remains intentionally deferred.
- `JsonsBootstrapper` rejects a whitespace `PackageDirectoryName` before any filesystem operation.
- The focused `PackageDirectoryName = null` RED is locally confirmed and the smallest pre-filesystem production guard is committed; focused/full GREEN verification is pending.

## 2026-09-17 — bootstrap null package-directory-name fix

Contract commit:
`d4d4cea2e00f3ba5a7968afa8eedeb99c8a4c99a`

Production guard commit:
`de85b3c460448d0dbe255dd3663e79cc05e4db4f`

Baseline checkpoint commit:
`b8e2b7161584936d4b7c570db7c07d4a746397e4`

Contract:

`EnsureWorkspaceAsync_WhenPackageDirectoryNameIsNull_ReturnsInvalidWithoutCreatingWorkspace`

`ErrorCatalogContextProvider.ValidateJsonsOptions(...)` defines the stable contract:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_NULL
Message: The package directory name cannot be null.
```

The focused run confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, `NormalizePath(null)` collapsed the null package directory name to an empty string, allowing the bootstrapper to use/create the root directory itself and return `Success` with an empty template provider.

Production change in `WhenItFails/Bootstrap/JsonsBootstrapper.cs` is intentionally narrow: a null `PackageDirectoryName` is now rejected immediately after cancellation/options checks and before the existing whitespace guard or any filesystem operation.

The focused RED build reported **1 compiler warning**. The warning text was not included in the reported excerpt, so its source is not yet classified. The last verified complete-suite checkpoint remains **1040/1040 GREEN with zero warnings**; warning status must be checked again on the post-fix focused/full run rather than assumed.

Expected complete-suite count after verification: **1041/1041 GREEN**.

## 2026-09-17 — 1040/1040 GREEN bootstrap whitespace package-directory checkpoint

Contract commit:
`e3ddf3e94be3e562382a1fcd7b27719fa29fd90d`

Production guard commit:
`c1ca5693f37bc129edde761b2452f4548edc2a0b`

Checkpoint commit:
`b8e2b7161584936d4b7c570db7c07d4a746397e4`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1040
Skipped:  0
Total:  1040
Compiler warnings: 0
```

`JsonsBootstrapper.EnsureWorkspaceAsync(...)` rejects a whitespace `PackageDirectoryName` before entering the filesystem block and returns:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_EMPTY
Message: The package directory name cannot be empty.
```

The focused contract also verifies that invalid configuration creates no root workspace directory.

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

This completed the current resolver-consumed collection-shape audit for `ErrorProfileSelectionService`.

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

- 1038/1038 — profile selection null error-tags classification complete.
- 1039/1039 — profile selection null error-categories classification complete; resolver-consumed collection-shape audit closed.
- 1040/1040 — bootstrap whitespace package directory name rejected before filesystem mutation.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~EnsureWorkspaceAsync_WhenPackageDirectoryNameIsNull_ReturnsInvalidWithoutCreatingWorkspace"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1041/1041 GREEN
```

Also check the compiler-warning count. If a warning is still present, capture its warning code/file/line before continuing so it can be classified rather than silently accepted.

## Next recommended step

After **1041/1041 GREEN** is confirmed, record the checkpoint and continue the `JsonsOptions` bootstrap-boundary audit one contract at a time. The next adjacent cases are null/whitespace `RootDirectory`, which already have stable contracts in `ErrorCatalogContextProvider`; add no production behavior until focused tests establish the current bootstrap response and side-effect shape.
