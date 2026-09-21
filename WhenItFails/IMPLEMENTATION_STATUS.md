# Implementation status

Last updated: 2026-09-21

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context/configuration handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite: **1059/1059 GREEN**, confirmed locally by the maintainer after the escaping `OwnerCatalogFileName` guard. The compiler-warning count and platform coverage were not separately reported for this full-suite checkpoint.
- The SDK emits `NETSDK1057` informational messages because the local SDK is `.NET 11.0.100-rc.1`; these are SDK support-policy messages, not compiler warnings from Toolbox code.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed provider results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies all resolver-consumed nullable collections currently audited as malformed input rather than resolver failure.
- `JsonsBootstrapper` rejects null/whitespace `RootDirectory` and `PackageDirectoryName` before filesystem mutation.
- `ErrorCatalogFileName = null` is locally verified GREEN and is rejected before template-provider invocation or filesystem mutation.
- `ErrorCatalogFileName` null/whitespace contracts are locally verified GREEN and reject malformed caller configuration before provider invocation or filesystem mutation.
- `CategoryCatalogFileName = null` is locally verified GREEN as part of the 1046-test suite.
- `CategoryCatalogFileName` null/whitespace contracts are locally verified GREEN and reject malformed caller configuration before provider invocation or filesystem mutation.
- `CodeGroupCatalogFileName = null` is locally verified GREEN and is rejected before template-provider invocation or filesystem mutation.
- `CodeGroupCatalogFileName` null/whitespace contracts are locally verified GREEN; invalid values are rejected before provider invocation or filesystem mutation.
- `OwnerCatalogFileName = null` is locally verified GREEN and is rejected before template-provider invocation or filesystem mutation.
- `OwnerCatalogFileName` null/whitespace contracts are locally verified GREEN; invalid values are rejected before provider invocation or filesystem mutation.
- `ProfilesFileName = null` is locally verified GREEN; invalid configuration is rejected before provider invocation or filesystem mutation.
- All five `JsonsOptions` catalog filename fields have locally verified null/whitespace GREEN contracts in `JsonsBootstrapper`, rejecting malformed input before filesystem mutation and template-provider invocation.
- The package-directory escape contract is locally verified GREEN; `JsonsBootstrapper` rejects a package path outside the configured root before filesystem mutation and template-provider invocation.
- A positive regression for valid nested `PackageDirectoryName` values is locally verified GREEN; legitimate nested package directories remain supported.
- The escaping `ErrorCatalogFileName` caller-configuration contract is locally verified GREEN; the bootstrapper rejects that invalid filename before filesystem mutation and template-provider invocation.
- The escaping `CategoryCatalogFileName` caller-configuration contract is locally verified GREEN; invalid filename paths are rejected before filesystem mutation and template-provider invocation.
- The escaping `CodeGroupCatalogFileName` caller-configuration contract is locally verified GREEN; invalid filename paths are rejected before filesystem mutation and template-provider invocation.
- The escaping `OwnerCatalogFileName` caller-configuration contract is locally verified GREEN; invalid filename paths are rejected before filesystem mutation and template-provider invocation.
- The escaping `ProfilesFileName` caller-configuration contract is confirmed RED on Windows; the narrow production guard is committed, awaiting focused/full GREEN verification.

## 2026-09-21 — escaping profiles filename caller-configuration contract

Contract commit:
`f20a7041a37b9e0af2761d5817392076cd3fc242`

Baseline: **1059/1059 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperEscapingProfilesFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenProfilesFileNameEscapesPackage_ReturnsInvalidBeforeProviderOrFilesystem`

With `ProfilesFileName = "../escaped.json"` and a tracking template provider returning no files, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
Message: The profile catalog file name must stay inside the package directory.
```

The contract also asserts no provider invocation, no workspace-root creation, and no escaped file. This is a proposed caller-configuration error code distinct from the established provider-target containment contract.

The focused test confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

Production guard commit:
`8182490e51f5d7e393e5efbcebed0d1014c4f439`

The bootstrapper now calls the existing `IsPathInsideDirectory` helper for caller-configured `ProfilesFileName` immediately after the owner-catalog filename check, before filesystem mutation or template-provider invocation. The established provider-target containment contract remains unchanged.

**Focused GREEN and complete-suite 1060/1060 GREEN are pending local verification.**

## 2026-09-21 — 1059/1059 GREEN escaping owner-catalog filename checkpoint

Contract commit: `ea94faccdab5ff65ae410c62d662cb37662fc8e6`

Production guard commit: `cbef653c0be0e1d4fa368d7bbe1f1f0fe04f344f`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1059
Total:  1059
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system. Caller-configured `OwnerCatalogFileName = "../escaped.json"` now returns `Invalid` before workspace creation or provider invocation. Next focused boundary: `ProfilesFileName` escaping its package directory.

## 2026-09-21 — escaping owner-catalog filename caller-configuration contract

Contract commit:
`ea94faccdab5ff65ae410c62d662cb37662fc8e6`

Baseline: **1058/1058 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperEscapingOwnerCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenOwnerCatalogFileNameEscapesPackage_ReturnsInvalidBeforeProviderOrFilesystem`

With `OwnerCatalogFileName = "../escaped.json"` and a tracking template provider returning no files, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
Message: The owner catalog file name must stay inside the package directory.
```

The contract also asserts no provider invocation, no workspace-root creation, and no escaped file. This is a proposed caller-configuration error code distinct from the established provider-target error contract.

The focused test confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

Production guard commit:
`cbef653c0be0e1d4fa368d7bbe1f1f0fe04f344f`

The bootstrapper now calls the existing `IsPathInsideDirectory` helper for caller-configured `OwnerCatalogFileName` immediately after the code-group catalog filename check, before filesystem mutation or template-provider invocation. The existing provider-target contract remains unchanged.

**Complete-suite 1059/1059 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1058/1058 GREEN escaping code-group-catalog filename checkpoint

Contract commit: `b95f1015af3e839fbc67f42294ed8a15c79c3a88`

Production guard commit: `afa88ba450f99da6aa66ba5178d712c31f65f8a0`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1058
Total:  1058
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system. Caller-configured `CodeGroupCatalogFileName = "../escaped.json"` now returns `Invalid` before workspace creation or provider invocation. Next boundary: `OwnerCatalogFileName` path escaping its package directory.

## 2026-09-21 — escaping code-group-catalog filename caller-configuration contract

Contract commit:
`b95f1015af3e839fbc67f42294ed8a15c79c3a88`

Baseline: **1057/1057 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperEscapingCodeGroupCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameEscapesPackage_ReturnsInvalidBeforeProviderOrFilesystem`

With `CodeGroupCatalogFileName = "../escaped.json"` and a tracking template provider returning no files, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
Message: The code group catalog file name must stay inside the package directory.
```

The contract also asserts no provider invocation, no workspace-root creation, and no escaped file. This is a proposed caller-configuration error code distinct from the existing provider-target error contract.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

Production guard commit:
`afa88ba450f99da6aa66ba5178d712c31f65f8a0`

The bootstrapper now calls the existing `IsPathInsideDirectory` helper for caller-configured `CodeGroupCatalogFileName` immediately after the category-catalog filename check, before any filesystem mutation or template-provider invocation. Existing provider-target behavior remains unchanged.

**Complete-suite 1058/1058 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1057/1057 GREEN escaping category-catalog filename checkpoint

Contract commit: `0bee29d224d3edd467cc88e6746d4bb80dbde66c`

Production guard commit: `80645d303b55529df54a6d41ed06130b3783acd6`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1057
Total:  1057
```

The latest confirmation did not separately report the focused test result, compiler-warning count, or operating system. Caller-configured `CategoryCatalogFileName = "../escaped.json"` now returns `Invalid` before workspace creation and provider invocation. Next focused boundary: `CodeGroupCatalogFileName` escaping its package directory.

## 2026-09-21 — escaping category-catalog filename caller-configuration contract

Contract commit:
`0bee29d224d3edd467cc88e6746d4bb80dbde66c`

Baseline: **1056/1056 GREEN**, confirmed locally by the maintainer before this test was added.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperEscapingCategoryCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameEscapesPackage_ReturnsInvalidBeforeProviderOrFilesystem`

With `CategoryCatalogFileName = "../escaped.json"` and a tracking template provider returning no template files, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
Message: The category catalog file name must stay inside the package directory.
```

The test also requires no provider invocation, no workspace-root creation, and no escaped file. This is a proposed caller-configuration code distinct from the existing provider-target error contract.

The focused test confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

Production guard commit:
`80645d303b55529df54a6d41ed06130b3783acd6`

The bootstrapper now calls the existing `IsPathInsideDirectory` helper for caller-configured `CategoryCatalogFileName` immediately after the existing error-catalog filename check, before any filesystem mutation or template-provider invocation. The existing provider-target error contract remains unchanged.

**Complete-suite 1057/1057 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1056/1056 GREEN escaping error-catalog filename checkpoint

Contract commit: `0ce2dd1e5f8b00a3b12be8f281b9b6a1e54987b8`

Production guard commit: `92a4f351641cba172bf66e3121f5065c1a27f151`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1056
Total:  1056
```

The latest confirmation did not separately report the focused result, compiler-warning count, or operating system. Caller-configured `ErrorCatalogFileName = "../escaped.json"` now returns `Invalid` before workspace creation and provider invocation. Next focused boundary: caller-configured `CategoryCatalogFileName` escaping its package directory.

## 2026-09-21 — escaping error-catalog filename caller-configuration contract

Contract commit:
`0ce2dd1e5f8b00a3b12be8f281b9b6a1e54987b8`

Baseline: **1055/1055 GREEN**, confirmed locally by the maintainer before this test was added.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperEscapingErrorCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenErrorCatalogFileNameEscapesPackage_ReturnsInvalidBeforeProviderOrFilesystem`

With `ErrorCatalogFileName = "../escaped.json"` and an empty tracking template provider, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
Message: The error catalog file name must stay inside the package directory.
```

Also require no template-provider invocation, no workspace-root creation and no escaped file. This is a newly proposed caller-configuration error contract; the established `WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_OUTSIDE_PACKAGE` provider-target contract remains distinct.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

Production guard commit:
`92a4f351641cba172bf66e3121f5065c1a27f151`

After validating package-directory containment and computing its path, the bootstrapper calls the existing `IsPathInsideDirectory` helper on caller-configured `ErrorCatalogFileName` before any `Directory.Exists`, `Directory.CreateDirectory`, or template-provider call. The existing provider-target error contract remains unchanged.

**Full-suite 1056/1056 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report focused test output, compiler-warning count, or operating system.

## 2026-09-21 — 1055/1055 GREEN nested package-directory checkpoint

Contract commit: `8fa2d9409256756e38e690225a1bcec05a1b023f`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1055
Total:  1055
```

The latest confirmation did not separately report the focused result, compiler-warning count, or operating system. Legitimate nested package-directory configuration remains accepted with the root-containment guard. No production changes were needed.

Next: reject an escaping catalog filename supplied directly through caller options, before filesystem mutation or template-provider invocation.

## 2026-09-21 — valid nested package-directory regression contract

Contract commit:
`8fa2d9409256756e38e690225a1bcec05a1b023f`

Baseline: **1054/1054 GREEN**, confirmed locally by the maintainer before adding the regression.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedPackageDirectoryNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenPackageDirectoryNameIsNestedInsideRoot_CreatesWorkspace`

With `PackageDirectoryName = Path.Combine("Packages", "WhenItFails")` under a unique temporary `RootDirectory`, the bootstrapper must return `Success`, report the nested package directory as newly created, invoke the tracking template provider, and create that directory inside the root. The tracking provider returns an empty template list so the contract stays focused on directory containment and creation.

**Full-suite 1055/1055 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result. No production change was made for this regression.

## 2026-09-21 — 1054/1054 GREEN package-directory escape checkpoint

Contract commit: `ccd5cdf9dbede67f95b0817f266d4445e17597c5`

Production guard commit: `0379b4ccb314264dab6069c3322a2188c28cfbbc`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1054
Total:  1054
```

The latest confirmation did not specify operating system or compiler-warning count. The containment guard now rejects `PackageDirectoryName = "../escaped"` before filesystem mutation or template-provider invocation. Next: protect the legitimate nested-package case from regressions.

## 2026-09-21 — bootstrap package-directory escape contract

Contract commit: `ccd5cdf9dbede67f95b0817f266d4445e17597c5`

Baseline: **1053/1053 GREEN**, confirmed locally by the maintainer before this test.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperEscapingPackageDirectoryNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenPackageDirectoryNameEscapesRoot_ReturnsInvalidBeforeProviderOrFilesystem`

Configured input: `PackageDirectoryName = "../escaped"` with a unique temporary `RootDirectory = <temp>/Jsons`. The test requires an invalid response with:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_OUTSIDE_ROOT
Message: The package directory name must stay inside the JSON root directory.
```

It also requires the template provider not to run and no root, escaped sibling directory, or unique temporary parent to be created. This is a new proposed bootstrap-specific contract, not a pre-existing error code of `ErrorCatalogContextProvider`.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

Production guard commit:
`0379b4ccb314264dab6069c3322a2188c28cfbbc`

The guard calls the existing `IsPathInsideDirectory` helper on the normalized JSON root and package-directory name, inside the existing filesystem `try` block but before `Directory.Exists`, `Directory.CreateDirectory`, or template-provider invocation. It rejects a path resolving outside the root without changing the existing template target containment logic.

**Full-suite 1054/1054 GREEN was subsequently confirmed locally by the maintainer; the latest confirmation did not specify operating system or separate focused test output.**

## 2026-09-21 — 1053/1053 GREEN bootstrap whitespace profiles-file-name checkpoint

Contract commit: `aae834ba2fbac7b8729a64a108f08940ab07b452`

Production guard commit: `4318b3a9337f8b9cf40ab08e5cc733023a1d34ce`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1053
Total:  1053
```

The compiler-warning count was not reported separately. All five catalog filename options now reject null and whitespace before workspace creation and template-provider invocation. Next focus: a package-directory path that escapes the configured JSON root.

## 2026-09-20 — bootstrap whitespace profiles-file-name contract

Contract commit:
`aae834ba2fbac7b8729a64a108f08940ab07b452`

Baseline: **1052/1052 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperWhitespaceProfilesFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenProfilesFileNameIsWhitespace_ReturnsInvalidBeforeProviderOrFilesystem`

Expected stable response, matching the established `ErrorCatalogContextProvider` options contract:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_EMPTY
Message: The profile catalog file name cannot be empty.
```

The test also requires no template-provider invocation and no workspace-root creation.

The focused run confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, whitespace `ProfilesFileName` reached workspace creation and template-provider invocation.

Production guard commit:
`4318b3a9337f8b9cf40ab08e5cc733023a1d34ce`

The narrow whitespace guard runs immediately after the existing null guard, before filesystem mutation and template-provider invocation.

**Focused and full-suite 1053/1053 GREEN were subsequently confirmed locally by the maintainer.**

## 2026-09-20 — 1052/1052 GREEN bootstrap null profiles-file-name checkpoint

Contract commit: `c7cf48d775df9c8136184481ff6e50cb7cfbcc98`

Production guard commit: `738b3677a5c9fbfbc46f64463f02fc13506ddcb5`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1052
Total:  1052
```

The compiler-warning count was not reported separately. Null `ProfilesFileName` is rejected before workspace creation and template-provider invocation. Next contract: whitespace `ProfilesFileName`.

## 2026-09-20 — bootstrap null profiles-file-name contract

Contract commit:
`c7cf48d775df9c8136184481ff6e50cb7cfbcc98`

Baseline: **1051/1051 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNullProfilesFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenProfilesFileNameIsNull_ReturnsInvalidBeforeProviderOrFilesystem`

Expected stable response, matching the established `ErrorCatalogContextProvider` options contract:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_NULL
Message: The profile catalog file name cannot be null.
```

The test also requires no template-provider invocation and no workspace-root creation.

The focused test confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, null `ProfilesFileName` passed through to workspace creation and template-provider invocation.

Production guard commit:
`738b3677a5c9fbfbc46f64463f02fc13506ddcb5`

The narrow null guard runs before filesystem mutation and template-provider invocation, after the established owner-catalog guards.

**Focused and full-suite 1052/1052 GREEN were subsequently confirmed locally by the maintainer.**

## 2026-09-20 — 1051/1051 GREEN bootstrap whitespace owner-catalog checkpoint

Contract commit: `9f30c19cbd8319a3f9b45d6dad74ace89918400e`

Production guard commit: `8a666aba9959233dc55bb45aaf93e9da9279f3df`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1051
Total:  1051
```

The compiler-warning count was not reported separately. Both null and whitespace `OwnerCatalogFileName` are rejected before workspace creation and template-provider invocation. Next contract: null `ProfilesFileName`.

## 2026-09-20 — bootstrap whitespace owner-catalog-file-name contract

Contract commit:
`9f30c19cbd8319a3f9b45d6dad74ace89918400e`

Baseline: **1050/1050 GREEN**, confirmed locally by the maintainer before the new test was added.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperWhitespaceOwnerCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenOwnerCatalogFileNameIsWhitespace_ReturnsInvalidBeforeProviderOrFilesystem`

Expected stable response, matching the established `ErrorCatalogContextProvider` option-level contract:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_EMPTY
Message: The owner catalog file name cannot be empty.
```

The test additionally requires that the template provider is not called and the workspace root is not created.

The focused test confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, whitespace `OwnerCatalogFileName` reached workspace creation and template-provider invocation.

Production guard commit:
`8a666aba9959233dc55bb45aaf93e9da9279f3df`

The narrow whitespace guard is placed immediately after the existing null guard, before filesystem mutation and template-provider invocation.

**Focused and full-suite 1051/1051 GREEN were subsequently confirmed locally by the maintainer.**

## 2026-09-20 — 1050/1050 GREEN bootstrap null owner-catalog checkpoint

Contract commit: `d72f135ea9e122e9be94e06545ce24e806cdde0b`

Production guard commit: `d0993d4c6cea0589a145986e87f27e5bea29ffdc`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1050
Total:  1050
```

The compiler-warning count was not reported separately. Null `OwnerCatalogFileName` is rejected before workspace creation and template-provider invocation. Next: whitespace `OwnerCatalogFileName`.

## 2026-09-20 — bootstrap null owner-catalog-file-name contract

Contract commit:
`d72f135ea9e122e9be94e06545ce24e806cdde0b`

Baseline: **1049/1049 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNullOwnerCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenOwnerCatalogFileNameIsNull_ReturnsInvalidBeforeProviderOrFilesystem`

Expected stable response, matching the established `ErrorCatalogContextProvider` option contract:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_NULL
Message: The owner catalog file name cannot be null.
```

The contract additionally requires no template-provider invocation and no workspace-root creation.

The focused test confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, the invalid option passed into workspace creation and template-provider invocation.

Production guard commit:
`d0993d4c6cea0589a145986e87f27e5bea29ffdc`

The guard rejects null `OwnerCatalogFileName` before workspace creation or template-provider invocation.

**Focused and full-suite 1050/1050 GREEN were subsequently confirmed locally by the maintainer.**

## 2026-09-20 — 1049/1049 GREEN bootstrap whitespace code-group-catalog checkpoint

Contract commit: `2cc5ad94aae216c221456a4710bcca1c9472b16f`

Production guard commit: `6adbfb0b0b66e648bce2ef5a075cc066cd5fb944`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1049
Total:  1049
```

The compiler-warning count was not reported separately. Null and whitespace `CodeGroupCatalogFileName` options are rejected before workspace creation and template-provider invocation. Next: null `OwnerCatalogFileName`.

## 2026-09-20 — bootstrap whitespace code-group-catalog-file-name contract

Contract commit:
`2cc5ad94aae216c221456a4710bcca1c9472b16f`

Baseline: **1048/1048 GREEN**, locally confirmed by the maintainer before this test was added.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperWhitespaceCodeGroupCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameIsWhitespace_ReturnsInvalidBeforeProviderOrFilesystem`

Expected stable response, matching the established `ErrorCatalogContextProvider` option-level contract:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_EMPTY
Message: The code group catalog file name cannot be empty.
```

The test additionally checks that the template provider was not called and the workspace root was not created.

The focused run confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

The null guard alone allowed whitespace configuration through to workspace creation and the tracking template provider.

Production guard commit:
`6adbfb0b0b66e648bce2ef5a075cc066cd5fb944`

The new whitespace guard runs immediately after the existing null guard and before workspace creation or template-provider invocation.

**Focused and full-suite 1049/1049 GREEN were subsequently confirmed locally by the maintainer.**

## 2026-09-20 — 1048/1048 GREEN bootstrap null code-group-catalog checkpoint

Contract commit:
`01f9d68fd9fb975d59f6f1f0032408f0e6bf6b4e`

Production guard commit:
`50c39e1e4ec5349afe58005738b8dd30ed65402e`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1048
Total:  1048
```

The compiler-warning count was not separately reported. Null `CodeGroupCatalogFileName` is rejected before workspace creation or template-provider invocation. Next contract: whitespace `CodeGroupCatalogFileName`.

## 2026-09-20 — bootstrap null code-group-catalog-file-name contract

Contract commit:
`01f9d68fd9fb975d59f6f1f0032408f0e6bf6b4e`

Baseline: **1047/1047 GREEN**, confirmed locally before this test was introduced.

Added `WhenItFails.Tests/Bootstrap/JsonsBootstrapperNullCodeGroupCatalogFileNameContractTests.cs` with contract
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameIsNull_ReturnsInvalidBeforeProviderOrFilesystem`.

Expected stable response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_NULL
Message: The code group catalog file name cannot be null.
```

The contract additionally requires that the template provider is not invoked and the workspace root is not created.

The focused run confirmed the expected RED: `Expected: Invalid`, `Actual: Success`. Before the guard, malformed configuration could reach workspace creation and the tracking template provider.

Production guard commit:
`50c39e1e4ec5349afe58005738b8dd30ed65402e`

The guard rejects null `CodeGroupCatalogFileName` immediately after the category-file-name guards and before filesystem mutation or template-provider invocation.

**Focused GREEN and complete-suite 1048/1048 GREEN are pending maintainer verification.**

## 2026-09-20 — 1047/1047 GREEN bootstrap whitespace category-catalog checkpoint

Contract commit:
`9cac09bc34ec17ea4b5b969e3c08528e64651f1d`

Production guard commit:
`a7d60a23a485433361e92af14745c0b6fdb8c19d`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1047
Total:  1047
```

The compiler-warning count was not reported separately. Both null and whitespace `CategoryCatalogFileName` inputs are rejected before workspace creation and template-provider invocation. Next contract: null `CodeGroupCatalogFileName`.

## 2026-09-20 — bootstrap whitespace category-catalog-file-name contract

Contract commit:
`9cac09bc34ec17ea4b5b969e3c08528e64651f1d`

Production guard commit:
`a7d60a23a485433361e92af14745c0b6fdb8c19d`

Previous locally confirmed checkpoint: **1046/1046 GREEN**.

Added contract:
`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameIsWhitespace_ReturnsInvalidBeforeProviderOrFilesystem`

The focused run confirmed RED before the production fix:

```text
Expected: Invalid
Actual:   Success
```

The narrow guard rejects whitespace `CategoryCatalogFileName` after the existing null check and before filesystem mutation or template-provider invocation. Expected stable response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_EMPTY
Message: The category catalog file name cannot be empty.
```

The contract also verifies that the template provider is not called and the workspace root is not created.

**Focused GREEN and complete-suite 1047/1047 GREEN are pending maintainer verification.**

## 2026-09-17 — bootstrap null error-catalog-file-name fix

Contract commit:
`91bba6f1c1cc11c2f7e6b12c5814a2afbb0ef64d`

Production guard commit:
`e3502565d5139132ce16947e7b0fbcb63e235e29`

Baseline checkpoint commit:
`7eaf2eda992ce59632dc086319e2a8c246858b8c`

Contract:

`EnsureWorkspaceAsync_WhenErrorCatalogFileNameIsNull_ReturnsInvalidBeforeProviderOrFilesystem`

`ErrorCatalogContextProvider.ValidateJsonsOptions(...)` defines the stable option-level contract:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_NULL
Message: The error catalog file name cannot be null.
```

The focused run confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, `JsonsBootstrapper` did not validate `ErrorCatalogFileName` before preparing the workspace. With the focused tracking provider, the malformed option therefore allowed workspace creation, provider invocation and a final `Success` response.

The production change is intentionally narrow: `options.ErrorCatalogFileName is null` is rejected after the already-established root/package option guards and before entering the filesystem block or invoking `IJsonsTemplateProvider.GetTemplateFiles(...)`.

The focused contract also protects both ordering guarantees:

```text
template provider invoked: false
workspace root created: false
```

Expected complete-suite result after verification: **1044/1044 GREEN, zero compiler warnings**.

## 2026-09-19 — bootstrap whitespace error-catalog-file-name contract

Contract commit:
`e58599d05993dac8af142a27325fa283b9a37e89`

Baseline checkpoint commit:
`b90d3c4563acae874f50c5b7aafad66ec216ade0`

Added:

`WhenItFails.Tests/Bootstrap/JsonsBootstrapperWhitespaceErrorCatalogFileNameContractTests.cs`

Contract:

`EnsureWorkspaceAsync_WhenErrorCatalogFileNameIsWhitespace_ReturnsInvalidBeforeProviderOrFilesystem`

Stable option-level response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_EMPTY
Message: The error catalog file name cannot be empty.
```

The focused contract also requires:

```text
template provider invoked: false
workspace root created: false
```

Current production validates only the null form. A whitespace value is therefore expected to pass into the filesystem/provider path and, with the tracking provider returning an empty collection, end as `Success`.

The focused run confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, whitespace `ErrorCatalogFileName` passed through the option boundary, allowing workspace creation and template-provider invocation before returning `Success` with the tracking provider.

Production guard commit:
`6d60d321ea2e746e41b3ec0fe6dcb4c821727ab1`

The production change is intentionally narrow: whitespace `ErrorCatalogFileName` is rejected immediately after the existing null guard and before entering the filesystem block or invoking the template provider.

Expected complete-suite result after verification: **1045/1045 GREEN**.

## 2026-09-19 — bootstrap null category-catalog-file-name contract

Contract commit:
`0e55002be070cfbd237b3661b2464e3138c1521f`

Baseline checkpoint commit:
`e1e6a178d0f20e6e0be36c965f6eda6de23878ca`

Added:

`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNullCategoryCatalogFileNameContractTests.cs`

Contract:

`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameIsNull_ReturnsInvalidBeforeProviderOrFilesystem`

Stable option-level response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_NULL
Message: The category catalog file name cannot be null.
```

The focused contract also requires:

```text
template provider invoked: false
workspace root created: false
```

Current production does not validate `CategoryCatalogFileName` before entering the filesystem/provider path. With the tracking provider returning an empty collection, current behavior is expected to create the workspace, invoke the provider and return `Success`.

The focused run confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, `CategoryCatalogFileName = null` passed through the option boundary, allowing workspace creation and template-provider invocation before returning `Success` with the tracking provider.

Production guard commit:
`75b2579c499002f1b073b7abf9cbf2f377cec196`

The production change is intentionally narrow: null `CategoryCatalogFileName` is rejected immediately after the established `ErrorCatalogFileName` guards and before entering the filesystem block or invoking the template provider.

Expected complete-suite result after verification: **1046/1046 GREEN**.

## 2026-09-19 — 1045/1045 GREEN bootstrap whitespace error-catalog-file-name checkpoint

Contract commit:
`e58599d05993dac8af142a27325fa283b9a37e89`

Production guard commit:
`6d60d321ea2e746e41b3ec0fe6dcb4c821727ab1`

Previous checkpoint commit:
`b90d3c4563acae874f50c5b7aafad66ec216ade0`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1045
Skipped:  0
Total:  1045
```

The warning count was not separately included in the latest confirmation, so this checkpoint records the test result only.

`JsonsBootstrapper.EnsureWorkspaceAsync(...)` now rejects whitespace `ErrorCatalogFileName` before any filesystem mutation or template-provider invocation and returns:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_EMPTY
Message: The error catalog file name cannot be empty.
```

Together with the prior null contract, `ErrorCatalogFileName` option handling is complete for the current null/empty scope.

## 2026-09-19 — 1044/1044 GREEN bootstrap null error-catalog-file-name checkpoint

Contract commit:
`91bba6f1c1cc11c2f7e6b12c5814a2afbb0ef64d`

Production guard commit:
`e3502565d5139132ce16947e7b0fbcb63e235e29`

Previous checkpoint commit:
`7eaf2eda992ce59632dc086319e2a8c246858b8c`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1044
Skipped:  0
Total:  1044
```

The warning count was not separately included in the latest confirmation, so this checkpoint records the test result only.

`JsonsBootstrapper.EnsureWorkspaceAsync(...)` now rejects `ErrorCatalogFileName = null` before any filesystem mutation or template-provider invocation and returns:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_NULL
Message: The error catalog file name cannot be null.
```

## 2026-09-17 — 1043/1043 GREEN bootstrap whitespace root-directory checkpoint

Contract commit:
`b35339be52737211d29516a66db57a43b359e7f5`

Production guard commit:
`3f5fcceafac6c7662822c49c3810d4dadd1a77c6`

Checkpoint commit:
`7eaf2eda992ce59632dc086319e2a8c246858b8c`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1043
Skipped:  0
Total:  1043
Compiler warnings: 0
```

`JsonsBootstrapper.EnsureWorkspaceAsync(...)` rejects null/whitespace `RootDirectory` and `PackageDirectoryName` before filesystem mutation, aligning the directory-option shape with `ErrorCatalogContextProvider` for the current scope.

## Completed `JsonsOptions` filename boundary

The five configurable catalog filename options (`ErrorCatalogFileName`, `CategoryCatalogFileName`, `CodeGroupCatalogFileName`, `OwnerCatalogFileName`, `ProfilesFileName`) now have locally verified null/whitespace contracts in `JsonsBootstrapper`, aligned with `ErrorCatalogContextProvider` and enforced before provider invocation and filesystem mutation.

## Next configuration boundary

The package-directory containment guard and positive nested-directory regression are verified. For template outputs, `JsonsBootstrapper` already rejects a provider-supplied target escaping its package directory, but does so only after workspace creation and the provider call.

The five catalog filename options are validated early for null/whitespace but not yet for an escaping relative path. A tracking template provider returning no files can therefore allow an invalid caller-configured filename such as `ErrorCatalogFileName = "../escaped.json"` to produce a successful bootstrap. Test this caller-configuration boundary before making a narrow production change. Preserve the existing separate provider-target error contract.

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

- 1041/1041 — bootstrap null package directory name rejected before filesystem mutation.
- 1042/1042 — bootstrap null root directory rejected before filesystem mutation; nullable-flow cleanup verified with zero compiler warnings.
- 1043/1043 — bootstrap whitespace root directory rejected before filesystem mutation.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~EnsureWorkspaceAsync_WhenProfilesFileNameEscapesPackage_ReturnsInvalidBeforeProviderOrFilesystem"
dotnet test WhenItFails.Tests
```

Expected results after the committed guard: **focused GREEN** and **1060/1060 GREEN** for the complete suite.

## Next recommended step

After **1060/1060 GREEN** is confirmed locally, record the checkpoint and review the remaining JSON workspace path contracts before selecting the next focused case. The five caller-configured catalog filename containment guards will then be complete.