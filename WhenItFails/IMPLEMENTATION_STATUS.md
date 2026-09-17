# Implementation status

Last updated: 2026-09-17

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context/configuration handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite baseline: **1041/1041 GREEN, zero compiler warnings**.
- The SDK emits `NETSDK1057` informational messages because the local SDK is `.NET 11.0.100-rc.1`; these are SDK support-policy messages, not compiler warnings from Toolbox code.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies all resolver-consumed nullable collections currently audited as malformed input rather than resolver failure.
- `JsonsBootstrapper` rejects null/whitespace `PackageDirectoryName` before filesystem mutation.
- The focused `RootDirectory = null` contract is locally confirmed RED and the smallest pre-filesystem production guard is committed; focused/full GREEN verification is pending.

## 2026-09-17 — bootstrap null root-directory fix

Contract commit:
`157118d9ec08a879da8fc364c03b3e4f3b260ec6`

Production guard commit:
`f30cd7cea9ff3e9f1a5e7b4ac2ff29b26fe3fe5d`

Baseline checkpoint commit:
`86e8018eab7e7be03eaebd6dd8edb4d970d7fe3c`

Contract:

`EnsureWorkspaceAsync_WhenRootDirectoryIsNull_ReturnsInvalidWithoutCreatingWorkspace`

`ErrorCatalogContextProvider.ValidateJsonsOptions(...)` already defines the stable contract:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ROOT_DIRECTORY_NULL
Message: The JSON root directory cannot be null.
```

The focused run confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

It also exposed a real nullable-flow compiler warning in production:

```text
WhenItFails/Bootstrap/JsonsBootstrapper.cs(50,57): warning CS8604:
Possible null reference argument for parameter 'path' in
'string JsonsBootstrapper.NormalizePath(string path)'.
```

`NETSDK1057` remains informational and is not counted as a compiler warning.

The fixture uses an isolated absolute temporary `PackageDirectoryName`, so the pre-fix filesystem side effect remains confined to the temporary directory and cannot create a relative directory in the repository.

Production change is intentionally minimal: `JsonsBootstrapper.EnsureWorkspaceAsync(...)` now rejects `options.RootDirectory is null` immediately after cancellation/options checks and before package-directory validation or any filesystem operation, using exactly the stable contract above.

This guard is expected to resolve both the response-shape mismatch and the CS8604 nullable warning. That must be verified locally rather than assumed.

Expected complete-suite result after verification: **1042/1042 GREEN, zero compiler warnings**.

## 2026-09-17 — 1041/1041 GREEN bootstrap null package-directory checkpoint

Contract commit:
`d4d4cea2e00f3ba5a7968afa8eedeb99c8a4c99a`

Production guard commit:
`de85b3c460448d0dbe255dd3663e79cc05e4db4f`

Checkpoint commit:
`86e8018eab7e7be03eaebd6dd8edb4d970d7fe3c`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1041
Skipped:  0
Total:  1041
Compiler warnings: 0
```

`JsonsBootstrapper.EnsureWorkspaceAsync(...)` rejects `PackageDirectoryName = null` before entering the filesystem block and returns:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_NULL
Message: The package directory name cannot be null.
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

- 1039/1039 — profile selection null error-categories classification complete; resolver-consumed collection-shape audit closed.
- 1040/1040 — bootstrap whitespace package directory name rejected before filesystem mutation.
- 1041/1041 — bootstrap null package directory name rejected before filesystem mutation.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~EnsureWorkspaceAsync_WhenRootDirectoryIsNull_ReturnsInvalidWithoutCreatingWorkspace"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1042/1042 GREEN
Compiler warnings: 0
```

Ignore `NETSDK1057` when counting compiler warnings; it is an SDK informational support-policy message.

## Next recommended step

After **1042/1042 GREEN, zero compiler warnings** is confirmed, record the checkpoint and continue the bootstrap `JsonsOptions` audit with the adjacent whitespace `RootDirectory` contract. Keep production unchanged until that focused test establishes the current response and filesystem side-effect shape.
