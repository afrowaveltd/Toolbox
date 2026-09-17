# Implementation status

Last updated: 2026-09-17

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context/configuration handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite: **1042/1042 GREEN**.
- The latest complete build reported **1 compiler warning**, `CS8604`, in `JsonsBootstrapper.cs` for repeated access to nullable `PackageDirectoryName` after validation.
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
- `JsonsBootstrapper` rejects null/whitespace `PackageDirectoryName` and null `RootDirectory` before filesystem mutation.
- The `RootDirectory = null` contract is locally GREEN; a nullable-flow-only production cleanup for the remaining `PackageDirectoryName` CS8604 warning is committed and awaits local verification.

## 2026-09-17 — 1042/1042 GREEN null root-directory checkpoint with nullable cleanup pending

Root-directory contract commit:
`157118d9ec08a879da8fc364c03b3e4f3b260ec6`

Root-directory production guard commit:
`f30cd7cea9ff3e9f1a5e7b4ac2ff29b26fe3fe5d`

Nullable-flow cleanup commit:
`8f27377ff6a156eee0f3930fd6ec7ec715ac4ef7`

Previous verified checkpoint commit:
`86e8018eab7e7be03eaebd6dd8edb4d970d7fe3c`

Locally verified after the root-directory guard:

```text
WhenItFails.Tests
Failed:   0
Passed: 1042
Skipped:  0
Total:  1042
```

The root-null contract now returns:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ROOT_DIRECTORY_NULL
Message: The JSON root directory cannot be null.
```

The same build still reported one production compiler warning:

```text
WhenItFails/Bootstrap/JsonsBootstrapper.cs(57,57): warning CS8604:
Possible null reference argument for parameter 'path' in
'string JsonsBootstrapper.NormalizePath(string path)'.
```

This warning is on `PackageDirectoryName`, not `RootDirectory`. The existing null/whitespace contracts are already GREEN; the compiler warning occurs because repeated reads from the mutable `JsonsOptions.PackageDirectoryName` property do not preserve a stable non-null flow state through the method.

The cleanup commit does not change public behavior. It snapshots `options.PackageDirectoryName` into a local nullable variable, performs the established null and whitespace guards against that local, and then passes the proven non-null local to `NormalizePath(...)`.

Expected verification after the cleanup:

```text
Complete suite: 1042/1042 GREEN
Compiler warnings: 0
```

`NETSDK1057` may still appear and remains informational rather than a Toolbox compiler warning.

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

- 1040/1040 — bootstrap whitespace package directory name rejected before filesystem mutation.
- 1041/1041 — bootstrap null package directory name rejected before filesystem mutation.
- 1042/1042 — bootstrap null root directory rejected before filesystem mutation; behavior GREEN, nullable-flow warning cleanup pending verification.

## Recommended verification

Pull current `master` and run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected result:

```text
Complete suite: 1042/1042 GREEN
Compiler warnings: 0
```

Ignore `NETSDK1057` when counting compiler warnings; it is an SDK informational support-policy message.

## Next recommended step

After **1042/1042 GREEN, zero compiler warnings** is confirmed, record the clean checkpoint and continue the bootstrap `JsonsOptions` audit with the adjacent whitespace `RootDirectory` contract. Keep production unchanged until that focused test establishes the current response and filesystem side-effect shape.
