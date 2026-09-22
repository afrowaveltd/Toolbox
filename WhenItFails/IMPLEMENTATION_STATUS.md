# Implementation status

Last updated: 2026-09-22

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context/configuration handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite: **1105/1105 GREEN**, confirmed locally by the maintainer after nested current-directory `ErrorCatalogFileName` validation. The compiler-warning count and platform coverage were not separately reported for this full-suite checkpoint.
- The SDK emits `NETSDK1057` informational messages because the local SDK is `.NET 11.0.100-rc.1`; these are SDK support-policy messages, not compiler warnings from Toolbox code.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` invocation boundary is complete for malformed direct results, ordinary-exception normalization and exact-instance cancellation propagation; deferred failures while consuming the returned collection are under audit.
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
- The escaping `ProfilesFileName` caller-configuration contract is locally verified GREEN; invalid filename paths are rejected before filesystem mutation and template-provider invocation.
- All five caller-configured catalog filename containment guards are locally verified GREEN.
- Malformed `RootDirectory` caller-configuration contract is locally verified GREEN; syntactically invalid root paths are normalized to a stable `Invalid` response before filesystem mutation or template-provider invocation.
- Malformed `PackageDirectoryName` caller-configuration contract is locally verified GREEN; syntactically invalid package directory names are normalized to a stable `Invalid` response before filesystem mutation or template-provider invocation.
- Malformed `ErrorCatalogFileName` caller-configuration contract is locally verified GREEN; syntactically invalid error catalog filenames are normalized to a stable `Invalid` response before filesystem mutation or template-provider invocation.
- Malformed `CategoryCatalogFileName` caller-configuration contract is locally verified GREEN; syntactically invalid category catalog filenames are normalized to a stable `Invalid` response before filesystem mutation or template-provider invocation.
- Malformed `CodeGroupCatalogFileName` caller-configuration contract is locally verified GREEN; syntactically invalid code-group catalog filenames are normalized to a stable `Invalid` response before filesystem mutation or template-provider invocation.
- Malformed `OwnerCatalogFileName` caller-configuration contract is locally verified GREEN; syntactically invalid owner catalog filenames are normalized to a stable `Invalid` response before filesystem mutation or template-provider invocation.
- Malformed `ProfilesFileName` caller-configuration contract is locally verified GREEN; syntactically invalid profile catalog filenames are normalized to a stable `Invalid` response before filesystem mutation or template-provider invocation.
- Malformed template-provider `TargetFileName` contract is locally verified GREEN; syntactically invalid provider target filenames are normalized to stable `Invalid` while preserving null, whitespace, and outside-package contracts.
- Valid nested template-target creation contract is locally verified GREEN; validated nested targets create missing parent directories while preserving existing files.
- Null template-name provider-output contract is locally verified GREEN; null logical template names are rejected before target validation and file creation.
- Whitespace template-name provider-output contract is locally verified GREEN; whitespace-only logical names are rejected before target validation and file creation.
- Template collection enumeration-exception contract is locally verified GREEN; ordinary deferred collection failures are normalized to the stable provider-failure response without leaking provider detail.
- Template collection enumeration-cancellation regression contract is locally verified GREEN; exact-instance `OperationCanceledException` propagation is preserved during returned-collection enumeration.
- Later-null-template-item no-partial-write contract is locally verified GREEN; the full materialized template snapshot is validated before any template file write.
- Directory-only template-target contract is locally verified GREEN; directory-only provider targets are rejected during provider-output validation before filesystem mutation.
- Directory-only `ErrorCatalogFileName` caller-configuration contract is locally verified GREEN; directory-only error catalog filenames are rejected before provider invocation and filesystem mutation.
- Directory-only `CategoryCatalogFileName` caller-configuration contract is locally verified GREEN; directory-only category catalog filenames are rejected before provider invocation and filesystem mutation.
- Directory-only `CodeGroupCatalogFileName` caller-configuration contract is locally verified GREEN; directory-only code-group catalog filenames are rejected before provider invocation and filesystem mutation.
- Directory-only `OwnerCatalogFileName` caller-configuration contract is locally verified GREEN; directory-only owner catalog filenames are rejected before provider invocation and filesystem mutation.
- Directory-only `ProfilesFileName` caller-configuration contract is locally verified GREEN; all five caller-configured catalog filenames reject directory-only targets before provider invocation or filesystem mutation.
- Existing-directory provider-target contract is locally verified GREEN; provider targets resolving to existing directories are rejected during full-snapshot validation before any template write.
- Existing-directory `ErrorCatalogFileName` caller-configuration contract is locally verified GREEN; caller configuration resolving to an existing directory is rejected before provider invocation.
- Existing-directory `CategoryCatalogFileName` caller-configuration contract is locally verified GREEN; caller configuration resolving to an existing directory is rejected before provider invocation.
- Existing-directory `CodeGroupCatalogFileName` caller-configuration contract is locally verified GREEN; caller configuration resolving to an existing directory is rejected before provider invocation.
- Existing-directory `OwnerCatalogFileName` caller-configuration contract is locally verified GREEN; caller configuration resolving to an existing directory is rejected before provider invocation.
- Existing-directory `ProfilesFileName` caller-configuration contract is locally verified GREEN; all five caller-configured catalog filenames now reject paths resolving to existing directories before provider invocation.
- Provider `TargetFileName = "."` semantic-directory contract is locally verified GREEN; the package-directory target returns the invalid-target code without changing the shared containment helper.
- Caller `ErrorCatalogFileName = "."` semantic-directory contract is locally verified GREEN; a package-directory target is now classified as an invalid caller filename without changing the shared containment helper.
- Caller `CategoryCatalogFileName = "."` semantic-directory contract is locally verified GREEN; package-directory targets are classified as invalid caller filenames without changing the shared containment helper.
- Caller `CodeGroupCatalogFileName = "."` semantic-directory contract is locally verified GREEN; the package-directory target returns a code-group-specific invalid filename code.
- Caller `OwnerCatalogFileName = "."` semantic-directory contract is locally verified GREEN; the package-directory target returns the owner-specific invalid filename code.
- Caller `ProfilesFileName = "."` semantic-directory contract is locally verified GREEN; all five caller catalog filename fields classify package-directory targets as invalid filenames.
- Provider-target existing-file parent contract is locally verified GREEN; an existing regular-file ancestor is rejected during full-snapshot validation before template writes.
- Caller error-catalog existing-file parent contract is locally verified GREEN; its existing regular-file ancestor is rejected before template-provider invocation.
- Caller category-catalog existing-file parent contract is locally verified GREEN; an existing regular-file ancestor is rejected before template-provider invocation.
- Caller code-group existing-file parent contract is locally verified GREEN; an existing regular-file ancestor is rejected before template-provider invocation.
- Caller owner-catalog existing-file parent contract is locally verified GREEN; an existing regular-file ancestor is rejected before template-provider invocation.
- Caller profiles existing-file parent contract is locally verified GREEN; all five caller-configured catalog filename fields now reject existing regular-file ancestors before provider invocation.
- Existing-file package-directory-path contract is locally verified GREEN; a regular file occupying the resolved package path is rejected before workspace creation or provider invocation.
- Existing-file root-directory-path contract is locally verified GREEN; a regular file occupying the configured root path is rejected before package containment or provider invocation.
- Root-directory existing-file-parent contract is locally verified GREEN; existing regular-file ancestors of the configured root are rejected before package containment or provider invocation.
- Package-directory existing-file-parent contract is locally verified GREEN; existing regular-file ancestors between the package directory and configured root are rejected before workspace creation or provider invocation.
- Package-directory current-directory semantic contract is locally verified GREEN; package paths resolving exactly to the configured root are classified as invalid names rather than outside-root escapes.
- Nested current-directory template-target contract is locally verified GREEN; terminal current-directory segments are rejected during full-snapshot validation before template writes.
- Nested current-directory `ErrorCatalogFileName` contract is locally verified GREEN; terminal current-directory segments are rejected before workspace creation or template-provider invocation.
- Nested current-directory `CategoryCatalogFileName` contract confirmed RED on Windows; terminal current-directory-segment guard is committed, awaiting focused/full GREEN verification.

## 2026-09-22 — nested current-directory category-catalog filename contract

Contract commit:
`dc2802122b0053dc575240c2d0b165ec392f5b55`

Baseline: **1105/1105 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedCurrentDirectoryCategoryCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameEndsWithCurrentDirectorySegment_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration sets:

```csharp
CategoryCatalogFileName = Path.Combine("Nested", ".")
```

The value is lexically contained inside the package but semantically resolves to the `Nested` directory itself rather than a file beneath it. Because `Nested` does not yet exist, the existing-directory guard cannot identify the problem.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

The template provider must not be invoked and the root/package workspace must remain uncreated.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The nested semantic-directory category filename passed caller validation and reached successful bootstrap completion.

Production fix commit:
`513e53c4c6f0a63281e31505349d87c154f777ea`

Documentation commit:
`1cebfcad76060e0d4ca68ab580272726abd985c2`

After normalization and before containment/provider invocation, `CategoryCatalogFileName` now rejects a final current-directory segment such as `Nested/.`:

```text
Status: Invalid
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

Valid nested category filenames, package-directory equality classification, outside-package handling, existing-directory detection, and file-parent guards remain unchanged.

**Focused GREEN and complete-suite 1106/1106 GREEN are pending local verification.**

## 2026-09-22 — 1105/1105 GREEN nested error-catalog current-directory checkpoint

Contract commit: `022a094b8c69b4cdadcb4650e0a11819eeca250e`

Production fix commit: `e33f61441d5f16c6cccd2b95a67f01fb304e5eb6`

Documentation commit: `3bd6bce684d4e6d962c820b02681c61d92fbfd1b`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1105/1105 GREEN
```

Nested semantic-directory error-catalog filenames such as `Nested/.` are now rejected before workspace creation or template-provider invocation.

## 2026-09-22 — nested current-directory error-catalog filename contract

Contract commit:
`022a094b8c69b4cdadcb4650e0a11819eeca250e`

Baseline: **1104/1104 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedCurrentDirectoryErrorCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenErrorCatalogFileNameEndsWithCurrentDirectorySegment_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration sets:

```csharp
ErrorCatalogFileName = Path.Combine("Nested", ".")
```

The value is lexically contained inside the package. Because `Nested` does not yet exist, `Directory.Exists(...)` cannot identify the semantic-directory target. Canonical resolution, however, identifies the configured target as the `Nested` directory itself rather than a file beneath it.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

The template provider must not be invoked and the root/package workspace must remain uncreated.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The nested semantic-directory filename passed caller validation, the workspace was created, and the template provider was invoked. This confirmed that the missing guard was in caller configuration validation rather than filesystem failure normalization.

Production fix commit:
`e33f61441d5f16c6cccd2b95a67f01fb304e5eb6`

Documentation commit:
`3bd6bce684d4e6d962c820b02681c61d92fbfd1b`

After normalization and before containment/provider invocation, `ErrorCatalogFileName` now rejects a final current-directory segment such as `Nested/.`:

```text
Status: Invalid
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

Valid nested filenames, package-directory equality classification, outside-package handling, existing-directory detection, and file-parent guards remain unchanged.

**Focused GREEN and complete-suite 1105/1105 GREEN are pending local verification.**

## 2026-09-22 — 1104/1104 GREEN nested current-directory template-target checkpoint

Contract commit: `2cbcb2772ee61af450ba4b6d995bb242692a05ca`

Production fix commit: `4e885a5334b78503e77bbe1dce910bc628a9964a`

Documentation commit: `0f3ac729e99f26080e609a6f3f82071e539563ea`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1104/1104 GREEN
```

Nested semantic-directory provider targets such as `Nested/.` are now rejected during full-snapshot validation before any template write begins.

## 2026-09-22 — nested current-directory template-target contract

Contract commit:
`2cbcb2772ee61af450ba4b6d995bb242692a05ca`

Baseline: **1103/1103 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedCurrentDirectoryTemplateTargetContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenLaterTemplateTargetEndsWithCurrentDirectorySegment_ReturnsInvalidWithoutPartialWrites`

The provider returns two templates. The first target is a valid `first.json`. The second target is:

```csharp
TargetFileName = Path.Combine("Nested", ".")
```

The second path is lexically contained inside the package and `Nested` does not yet exist. Canonical resolution, however, identifies the target as the `Nested` directory itself rather than a file beneath it.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

The complete provider snapshot must be rejected before writes begin: `first.json` must remain absent and the `Nested` directory must not be created.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The nested semantic-directory target passed full-snapshot validation and reached filesystem creation, where it was normalized as a generic failure. This also meant the invalid target was being detected too late to uphold the intended no-partial-write validation boundary.

Production fix commit:
`4e885a5334b78503e77bbe1dce910bc628a9964a`

Documentation commit:
`0f3ac729e99f26080e609a6f3f82071e539563ea`

During provider snapshot validation, bootstrap now rejects a normalized target whose final path segment is `.` before containment/write processing:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

Valid nested file targets remain supported. The later invalid target is rejected before any template write begins, preserving the no-partial-write guarantee.

**Focused GREEN and complete-suite 1104/1104 GREEN are pending local verification.**

## 2026-09-22 — 1103/1103 GREEN package root-target checkpoint

Contract commit: `1a6368ee9b3580806e76dbbd4d0fa298015cf9b4`

Production fix commit: `ff45147b74495140f1a1aa0ad7d8061ff3ab427b`

Documentation commit: `f3c667fb07f57ac2d0ef43eab6618b0adad69854`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1103/1103 GREEN
```

Package-directory equality with `RootDirectory` now has stable invalid-name classification while true outside-root paths retain the escape-specific contract.

## 2026-09-22 — package-directory current-directory semantic contract

Contract commit:
`1a6368ee9b3580806e76dbbd4d0fa298015cf9b4`

Baseline: **1102/1102 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperCurrentDirectoryPackageDirectoryNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenPackageDirectoryNameResolvesToRoot_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration sets:

```csharp
PackageDirectoryName = "."
```

After normalization and canonical path resolution, the package directory resolves exactly to `RootDirectory`. This is not a path escape outside the configured root; it is semantically invalid package-directory configuration because the package directory name does not identify a child directory.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID
Message: The package directory name is invalid.
```

The template provider must not be invoked and the root directory must remain uncreated.

The focused contract confirmed the expected RED on Windows:

```text
Expected: WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID
Actual:   WIF_JSONS_PACKAGE_DIRECTORY_NAME_OUTSIDE_ROOT
```

The response status was already `Invalid`; only the issue classification was wrong. The shared containment helper correctly rejects equality because it requires a child path, but equality with `RootDirectory` is semantically different from a true escape.

Production fix commit:
`ff45147b74495140f1a1aa0ad7d8061ff3ab427b`

Documentation commit:
`f3c667fb07f57ac2d0ef43eab6618b0adad69854`

When package containment returns false, bootstrap now compares canonical root and package paths. Exact equality returns:

```text
Status: Invalid
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID
Message: The package directory name is invalid.
```

Only a non-equal path outside the root returns `WIF_JSONS_PACKAGE_DIRECTORY_NAME_OUTSIDE_ROOT`. The shared strict containment helper, true escape behavior, nested package paths, and filesystem guards remain unchanged.

**Focused GREEN and complete-suite 1103/1103 GREEN are pending local verification.**

## 2026-09-22 — 1102/1102 GREEN package file-parent checkpoint

Contract commit: `6d7e970bb612328d8d91cc27650bfead2a0a6c45`

Production fix commit: `3634b11b63a69446d118ace6c68e213321c99526`

Documentation commit: `09897c076ed538a95e51e102ab52bce5c29b8df5`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1102/1102 GREEN
```

Nested package-directory paths now reject regular-file ancestors inside the configured root before workspace creation or template-provider invocation.

## 2026-09-22 — 1101/1101 GREEN root file-parent checkpoint

Contract commit: `53206e6f2745897df1160bc1db38beeef7d8f773`

Production fix commit: `aa7a7d666eb15b2a81d16a81116286e09db75466`

Documentation commit: `579d5fa922488f6069958658ad494ec9f8cc7c75`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1101/1101 GREEN
```

The configured root and its existing parent chain now reject regular-file collisions before package containment, workspace creation, or template-provider invocation.

## 2026-09-22 — package-directory existing-file-parent contract

Contract commit:
`6d7e970bb612328d8d91cc27650bfead2a0a6c45`

Baseline: **1101/1101 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperFileParentPackageDirectoryContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenPackageDirectoryParentIsExistingFile_ReturnsInvalidBeforeProvider`

The configured root already exists and contains a regular file `Packages` with contents `Keep me.`. Caller configuration sets:

```csharp
PackageDirectoryName = Path.Combine("Packages", "WhenItFails")
```

The nested package path is syntactically valid and remains inside the root, but its parent `Packages` is a regular file and cannot contain the requested package directory.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID
Message: The package directory name is invalid.
```

The template provider must not be invoked. The existing parent file must remain unchanged and no package directory may be created.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The exact package-directory path did not exist, but its parent `Packages` was a regular file. Package containment therefore succeeded and the collision reached directory creation, where the outer I/O catch normalized it as a generic failure.

Production fix commit:
`3634b11b63a69446d118ace6c68e213321c99526`

Documentation commit:
`09897c076ed538a95e51e102ab52bce5c29b8df5`

After package containment and the exact package-file guard, bootstrap now walks the canonical package parent chain up to (but excluding) the configured root. Any existing regular-file ancestor returns:

```text
Status: Invalid
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID
Message: The package directory name is invalid.
```

The validation runs before workspace creation or template-provider invocation. Valid nested package paths, valid existing package directories, exact package-file handling, and root-path classification remain unchanged.

**Focused GREEN and complete-suite 1102/1102 GREEN are pending local verification.**

## 2026-09-22 — root-directory existing-file-parent contract

Contract commit:
`53206e6f2745897df1160bc1db38beeef7d8f773`

Baseline: **1100/1100 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperFileParentRootDirectoryContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenRootDirectoryParentIsExistingFile_ReturnsInvalidBeforeProvider`

A unique temporary test directory contains an existing regular file `Blocked` with contents `Keep me.`. Caller configuration sets:

```csharp
RootDirectory = Path.Combine(existingParentFilePath, "Jsons"),
PackageDirectoryName = "WhenItFails"
```

The configured root is syntactically valid and does not itself exist as a file or directory. Its parent `Blocked`, however, is a regular file and therefore cannot contain the requested root directory.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ROOT_DIRECTORY_INVALID
Message: The JSON root directory path is invalid.
```

The template provider must not be invoked. The existing parent file must remain a regular file with unchanged contents, and neither the root directory nor package directory may be created.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The configured root itself did not exist, but its parent `Blocked` was a regular file. Root syntax and lexical containment therefore succeeded, and the collision reached directory creation where the outer I/O catch normalized it as a generic failure.

Production fix commit:
`aa7a7d666eb15b2a81d16a81116286e09db75466`

Documentation commit:
`579d5fa922488f6069958658ad494ec9f8cc7c75`

After root syntax validation and the exact-root-file guard, bootstrap now walks the canonical parent chain of `RootDirectory`. Any existing regular-file ancestor returns:

```text
Status: Invalid
Code: WIF_JSONS_ROOT_DIRECTORY_INVALID
Message: The JSON root directory path is invalid.
```

The validation runs before package containment, workspace creation, or template-provider invocation. Valid missing-root paths and valid existing directories remain unchanged, and the original parent file is preserved.

**Focused GREEN and complete-suite 1101/1101 GREEN are pending local verification.**

## 2026-09-22 — 1100/1100 GREEN existing-file root-directory checkpoint

Contract commit: `e691a6405e116dc5eca951d915041866c00f5376`

Production fix commit: `40ec16aed6b63e9580dd32f593a98e454770a38d`

Documentation commit: `595cedd3195531d6134dd86870ce4befc5c578cc`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1100/1100 GREEN
```

The configured root path now distinguishes an existing regular file from a valid or missing directory before package containment, filesystem mutation, or template-provider invocation.

## 2026-09-22 — existing-file root-directory-path contract

Contract commit:
`e691a6405e116dc5eca951d915041866c00f5376`

Baseline: **1099/1099 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperExistingFileRootDirectoryContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenRootDirectoryPathIsExistingFile_ReturnsInvalidBeforeProvider`

A unique temporary test directory contains a regular file named `Jsons` with contents `Keep me.`. Caller configuration sets that file path as:

```csharp
RootDirectory = rootDirectory,
PackageDirectoryName = "WhenItFails"
```

The configured root path is syntactically valid, but it cannot represent the JSON workspace root because it is already occupied by a regular file.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ROOT_DIRECTORY_INVALID
Message: The JSON root directory path is invalid.
```

The tracking template provider must not be invoked. The existing root file must remain a file with unchanged contents, and no package directory may be created beneath it.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The configured root path was syntactically valid but occupied by a regular file. Lexical containment therefore succeeded and the collision reached directory creation, where the outer I/O catch normalized it as a generic failure.

Production fix commit:
`40ec16aed6b63e9580dd32f593a98e454770a38d`

Documentation commit:
`595cedd3195531d6134dd86870ce4befc5c578cc`

After root syntax validation, bootstrap now rejects an existing regular file at `RootDirectory` before package containment, workspace creation, or template-provider invocation:

```text
Status: Invalid
Code: WIF_JSONS_ROOT_DIRECTORY_INVALID
Message: The JSON root directory path is invalid.
```

Missing root directories remain valid for later workspace creation; valid existing directories remain unchanged; the original colliding file is preserved.

**Focused GREEN and complete-suite 1100/1100 GREEN are pending local verification.**

## 2026-09-22 — 1099/1099 GREEN existing-file package-directory checkpoint

Contract commit: `1471d0a940390a06070a69b99b52c9e879739ac8`

Production fix commit: `3b75438d332ad03e7aa6bface3e0fd6554d94c2c`

Documentation commit: `49b935b6cfc483909c19f7d36ae23a4cbc254374`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1099/1099 GREEN
```

The resolved package-directory path now distinguishes an existing regular file from an existing directory before filesystem mutation or template-provider invocation.

## 2026-09-22 — existing-file package-directory-path contract

Contract commit:
`1471d0a940390a06070a69b99b52c9e879739ac8`

Baseline: **1098/1098 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperExistingFilePackageDirectoryContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenPackageDirectoryPathIsExistingFile_ReturnsInvalidBeforeProvider`

The JSON root directory already exists and contains a regular file named `WhenItFails` with contents `Keep me.`. Caller configuration sets:

```csharp
RootDirectory = rootDirectory,
PackageDirectoryName = "WhenItFails"
```

The resolved package-directory path is lexically valid and remains inside the configured root, but it cannot represent a directory because that path is already occupied by a regular file.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID
Message: The package directory name is invalid.
```

The tracking template provider must not be invoked. The existing file must remain a file, retain its original contents, and must not be replaced or mutated.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The resolved package-directory path was occupied by a regular file. Because `Directory.Exists(packageDirectoryPath)` is false for a file, the previous implementation reached `Directory.CreateDirectory(packageDirectoryPath)` and the outer I/O catch normalized the collision as a generic failure.

Production fix commit:
`3b75438d332ad03e7aa6bface3e0fd6554d94c2c`

Documentation commit:
`49b935b6cfc483909c19f7d36ae23a4cbc254374`

After package containment succeeds and `packageDirectoryPath` is resolved, bootstrap now rejects an existing regular file at that exact path before any caller filename validation, workspace creation, or template-provider invocation:

```text
Status: Invalid
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID
Message: The package directory name is invalid.
```

Existing valid directories still remain untouched, missing directories can still be created, and the original colliding file is preserved.

**Focused GREEN and complete-suite 1099/1099 GREEN are pending local verification.**

## 2026-09-22 — 1098/1098 GREEN caller profiles file-parent checkpoint

Contract commit: `3185dc5b222d8c71e6352df5984660ba86af788e`

Production fix commit: `7f9f6f4e2d424c49274460832460386890039e6f`

Documentation commit: `fe34b6c5e6bd3070eceebd098a3a96cb41e1f0ef`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1098/1098 GREEN
```

The existing-file ancestor boundary is now covered consistently for `ErrorCatalogFileName`, `CategoryCatalogFileName`, `CodeGroupCatalogFileName`, `OwnerCatalogFileName`, and `ProfilesFileName`.

## 2026-09-22 — caller profiles existing-file parent contract

Contract commit:
`3185dc5b222d8c71e6352df5984660ba86af788e`

Baseline: **1097/1097 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperFileParentProfilesFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenProfilesFileNameParentIsExistingFile_ReturnsInvalidBeforeProvider`

The package workspace contains an existing regular file `Nested` with contents `Keep me.`. Caller configuration sets:

```csharp
ProfilesFileName = Path.Combine("Nested", "profiles.json")
```

The target remains lexically inside the package and does not itself resolve to an existing directory. Its parent `Nested`, however, is a regular file and cannot serve as a directory.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

The tracking template provider must not be invoked; the nested target must remain absent; and `Nested` must remain an unchanged regular file.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The profile filename passed lexical containment despite its parent `Nested` being an existing regular file. An empty tracking provider allowed bootstrap to return success.

Production fix commit:
`7f9f6f4e2d424c49274460832460386890039e6f`

Documentation commit:
`fe34b6c5e6bd3070eceebd098a3a96cb41e1f0ef`

After containment and existing-directory validation of `ProfilesFileName`, the bootstrapper now checks canonical parent paths inside the package up to (but excluding) the package directory. An existing regular-file ancestor returns:

```text
Status: Invalid
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

The validation runs before provider invocation and leaves the original file untouched. Valid nested filenames, true outside-package paths, provider-target validation, and the established profile error classifications remain unchanged.

**Focused GREEN and complete-suite 1098/1098 GREEN are pending local verification.**

## 2026-09-22 — 1097/1097 GREEN caller owner catalog file-parent checkpoint

Contract commit: `20f3f4f343a84386db0c73e99229031cfffae78e`

Production fix commit: `2e5e498fabf67f1150b985d7a1507aa68747ba8a`

Documentation commit: `f9500de7c202fc036838bacebf4d7cda5bb6a5f0`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1097
Total:  1097
```

Caller `OwnerCatalogFileName` paths with existing regular-file ancestors inside the package now return the owner-specific invalid response before template-provider invocation, preserving the ancestor file.

Final caller-configured field for this boundary: `ProfilesFileName`.

## 2026-09-22 — caller owner catalog existing-file parent contract

Contract commit:
`20f3f4f343a84386db0c73e99229031cfffae78e`

Baseline: **1096/1096 GREEN**, confirmed locally by the maintainer before the new contract.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperFileParentOwnerCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenOwnerCatalogFileNameParentIsExistingFile_ReturnsInvalidBeforeProvider`

The package workspace contains an existing regular file `Nested` with contents `Keep me.`. Caller configuration sets:

```csharp
OwnerCatalogFileName = Path.Combine("Nested", "owners.json")
```

The target is inside the package and does not itself resolve to an existing directory. However, its parent `Nested` is a regular file, not a usable directory.

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

The tracking template provider must not be called; the nested target must remain absent; and `Nested` must remain an unchanged regular file.

Current production checks the owner filename for containment and a terminal existing-directory target, but does not yet validate existing-file ancestors. An empty tracking provider can therefore allow bootstrap to return `Success`.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The owner catalog filename passed lexical containment despite its parent `Nested` being an existing regular file. An empty tracking provider allowed bootstrap to return success.

Production fix commit:
`2e5e498fabf67f1150b985d7a1507aa68747ba8a`

Documentation commit:
`f9500de7c202fc036838bacebf4d7cda5bb6a5f0`

After containment and existing-directory validation of `OwnerCatalogFileName`, the bootstrapper now checks canonical parent paths inside the package up to (but excluding) the package directory. An existing regular-file ancestor returns:

```text
Status: Invalid
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

The validation runs before provider invocation and leaves the original file untouched. The other caller fields, actual outside-package paths, and the shared containment helper remain unchanged.

**Complete-suite 1097/1097 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1096/1096 GREEN caller code group file-parent checkpoint

Contract commit: `bd8ca8b8683a4a2d28a5a3e6036dbad416b484f3`

Production fix commit: `680d6140cb7fab2c238f2e7ae61f49c50bd25045`

Documentation commit: `c4e13a06b4192de531ab72f5f9312558d7be7916`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1096
Total:  1096
```

Caller `CodeGroupCatalogFileName` paths with existing regular-file ancestors inside the package now return the code-group-specific invalid response before template-provider invocation, leaving the ancestor file untouched.

Next caller-configured field: `OwnerCatalogFileName`.

## 2026-09-22 — caller code group catalog existing-file parent contract

Contract commit:
`bd8ca8b8683a4a2d28a5a3e6036dbad416b484f3`

Baseline: **1095/1095 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperFileParentCodeGroupCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameParentIsExistingFile_ReturnsInvalidBeforeProvider`

The package workspace already contains a regular file `Nested` with contents `Keep me.`. Caller configuration uses:

```csharp
CodeGroupCatalogFileName = Path.Combine("Nested", "code-groups.json")
```

The target is lexically inside the package, but its parent `Nested` is a regular file rather than a directory. The target itself does not resolve to an existing directory.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

The tracking template provider must not be invoked. The nested target must remain absent, and `Nested` must remain a regular file with unchanged contents.

Current production checks code-group filename containment and whether its terminal target is an existing directory, but does not yet validate existing regular-file ancestors. With an empty tracking provider, bootstrap can return `Success`.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The code-group catalog filename passed lexical containment despite its `Nested` parent being an existing regular file; the tracking provider returned no templates and bootstrap reported success.

Production fix commit:
`680d6140cb7fab2c238f2e7ae61f49c50bd25045`

Documentation commit:
`c4e13a06b4192de531ab72f5f9312558d7be7916`

After containment and existing-directory validation of `CodeGroupCatalogFileName`, `JsonsBootstrapper` now checks canonical parent paths inside the package up to (but excluding) the package directory. Any existing regular-file ancestor returns:

```text
Status: Invalid
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

The check runs before workspace creation or template-provider invocation and does not modify the existing parent file. Actual escape paths retain the outside-package response; other caller fields are unchanged.

**Complete-suite 1096/1096 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1095/1095 GREEN caller category catalog file-parent checkpoint

Contract commit: `389e5487c03a9e498b8f0bd106358167d55c1b08`

Production fix commit: `f83293b1acebc1dc613995cdd5545c1b81b3e0b2`

Documentation commit: `3feac61a6a071bc6e0a0e4c1b6ea34f1200efe7a`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1095
Total:  1095
```

Caller `CategoryCatalogFileName` paths with existing regular-file ancestors inside the package now return the category-specific invalid response before template-provider invocation. The original ancestor file remains untouched.

Next caller-configured field: `CodeGroupCatalogFileName`.

## 2026-09-22 — caller category catalog existing-file parent contract

Contract commit:
`389e5487c03a9e498b8f0bd106358167d55c1b08`

Baseline: **1094/1094 GREEN**, confirmed locally by the maintainer before the new contract.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperFileParentCategoryCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameParentIsExistingFile_ReturnsInvalidBeforeProvider`

The package workspace contains an existing regular file `Nested` with contents `Keep me.`. Caller configuration sets:

```csharp
CategoryCatalogFileName = Path.Combine("Nested", "categories.json")
```

The target is lexically inside the package and does not itself resolve to an existing directory. Its parent `Nested`, however, is a regular file and cannot be used as a directory.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

The tracking template provider must not be called, the nested target must remain absent, and `Nested` must remain an unchanged regular file.

Current production checks the category filename for containment and an existing-directory terminal target, but does not validate existing-file ancestors. A tracking provider returning no templates can therefore allow bootstrap to return `Success`.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The category catalog filename passed lexical containment despite its `Nested` parent being an existing regular file; the tracking provider returned no templates and bootstrap reported success.

Production fix commit:
`f83293b1acebc1dc613995cdd5545c1b81b3e0b2`

Documentation commit:
`3feac61a6a071bc6e0a0e4c1b6ea34f1200efe7a`

After containment and existing-directory validation of `CategoryCatalogFileName`, `JsonsBootstrapper` now checks canonical parent paths inside the package up to (but excluding) the package directory. Any existing regular-file ancestor returns:

```text
Status: Invalid
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

The check runs before workspace creation or template-provider invocation and does not modify the existing parent file. Actual escape paths retain the outside-package response; other caller fields are unchanged.

**Complete-suite 1095/1095 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1094/1094 GREEN caller error catalog file-parent checkpoint

Contract commit: `764002ee53b8acf22e1426a17183fc0581c88a63`

Production fix commit: `55f2f2cc3dd7ea4216fd13fd309e9b1fc80893ac`

Documentation commit: `48d8e237f2a327d4db132b25ada833031d189cd5`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1094
Total:  1094
```

Caller `ErrorCatalogFileName` paths with existing regular-file ancestors inside the package are rejected as invalid configuration before template-provider invocation. The existing ancestor file remains untouched. The analogous provider-target contract is already GREEN.

Next separate field: `CategoryCatalogFileName`.

## 2026-09-22 — caller error catalog existing-file parent contract

Contract commit:
`764002ee53b8acf22e1426a17183fc0581c88a63`

Baseline: **1093/1093 GREEN**, confirmed locally by the maintainer before the new contract.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperFileParentErrorCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenErrorCatalogFileNameParentIsExistingFile_ReturnsInvalidBeforeProvider`

The package workspace contains a regular file `Nested` with contents `Keep me.`. Caller configuration sets:

```csharp
ErrorCatalogFileName = Path.Combine("Nested", "errors.json")
```

The target is lexically inside the package, and its terminal target is not an existing directory. However, its parent `Nested` is a regular file rather than a usable directory.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

The tracking template provider must not be called, the target must remain absent, and `Nested` must remain an unchanged regular file.

Current production validates caller error-catalog containment and the target's existing-directory state, but not whether its parent paths are existing files. A tracking provider returning no templates can therefore allow bootstrap to report `Success`.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The caller's nested error catalog filename passed lexical containment, and the tracking provider could return success despite its parent being an existing regular file.

Production fix commit:
`55f2f2cc3dd7ea4216fd13fd309e9b1fc80893ac`

Documentation commit:
`48d8e237f2a327d4db132b25ada833031d189cd5`

After containment and existing-directory validation of `ErrorCatalogFileName`, `JsonsBootstrapper` now checks each canonical parent path inside the package up to (but excluding) the package directory. Any existing regular-file ancestor returns:

```text
Status: Invalid
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

This guard executes before workspace creation and template-provider invocation; it leaves the original file untouched. Actual escape paths keep the outside-package response, and the other caller filename fields are unchanged.

**Complete-suite 1094/1094 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1093/1093 GREEN provider file-parent checkpoint

Contract commit: `a6dd14f6a497bda3c1cf8d4308b4fcd9c0d8e4ca`

Production fix commit: `8578aa7ef35bbd12d6f5dadf9358dc7c18261606`

Documentation commit: `1d0d339e87b86efa7492d0913cd14468a38c27d0`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1093
Total:  1093
```

Provider targets with existing regular-file parent paths inside the package are now rejected during full-snapshot validation before template writes. Existing files are preserved, and the separate valid nested-target creation contract remains covered.

Next distinct boundary: a caller-configured nested catalog filename with an existing regular file at one of its parent paths should be rejected before template-provider invocation.

## 2026-09-22 — provider target with existing file parent contract

Contract commit:
`a6dd14f6a497bda3c1cf8d4308b4fcd9c0d8e4ca`

Baseline: **1092/1092 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperFileParentTemplateTargetContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenLaterTemplateTargetParentIsExistingFile_ReturnsInvalidWithoutPartialWrites`

The existing package workspace contains a regular file `Nested` with the contents `Keep me.`. The provider returns two templates: a valid `first.json` followed by a nested target `Nested/child.json`. The second target remains lexically inside the package and does not itself exist as a directory, but its parent cannot be used as a directory.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

The full provider snapshot must be validated before any template write: `first.json` and `Nested/child.json` must not exist afterward, and `Nested` must remain a regular file with unchanged contents.

Current production checks whether a provider target is itself an existing directory, but does not reject parent paths that are existing regular files. The later target is expected to fail during filesystem writes after the first template has been created.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The first template was eligible for writing, but the later nested target used `Nested` as a directory even though that path was an existing regular file. It was only rejected during filesystem I/O.

Production fix commit:
`8578aa7ef35bbd12d6f5dadf9358dc7c18261606`

Documentation commit:
`1d0d339e87b86efa7492d0913cd14468a38c27d0`

During validation of the complete materialized provider snapshot, `JsonsBootstrapper` now traverses the canonical parent paths for each contained template target up to the package directory. If any parent inside the package is an existing regular file, the bootstrapper returns:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

The guard executes before writing template files, protecting earlier template targets and the existing regular-file parent for this deterministic invalid configuration. It does not add a transaction or prevent unrelated concurrent filesystem changes.

**Complete-suite 1093/1093 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1092/1092 GREEN five-field current-directory checkpoint

Final contract commit: `51f428cfa517c754c539e6b9b33c0b2c0beb2376`

Final production fix commit: `a39fcb881f5b6f0cabbf642a1dc794d067a6e7b3`

Final documentation commit: `4310b612e029250096a3ccdd0c5e2d8117339329`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1092
Total:  1092
```

The provider target and all five caller-configured catalog filename fields now classify paths that resolve exactly to the package directory as invalid filename targets, rather than escape paths. True paths outside the package keep their separate outside-package errors; the shared containment helper remains unchanged.

Next distinct boundary: provider target whose parent path already exists as a regular file. A later nested template could otherwise fail only during file creation, after earlier templates have been written.

## 2026-09-22 — caller current-directory profiles filename contract

Contract commit:
`51f428cfa517c754c539e6b9b33c0b2c0beb2376`

Baseline: **1091/1091 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperCurrentDirectoryProfilesFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenProfilesFileNameResolvesToPackageDirectory_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration:

```csharp
ProfilesFileName = "."
```

The value resolves exactly to the package directory, so it does not identify a catalog file and is not a path escaping the package.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

The tracking template provider must not be invoked, and the initially nonexistent workspace root must remain absent.

Current profile filename containment reports false when its target resolves to the package directory itself and classifies that input as `WIF_JSONS_PROFILE_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`. The other four caller filename fields have already received their separate classification fixes.

The focused contract confirmed the expected RED on Windows:

```text
Expected: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Actual:   WIF_JSONS_PROFILE_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
```

The response status was already `Invalid`; only the error classification was wrong. `ProfilesFileName = "."` resolves exactly to the package directory, not outside it.

Production fix commit:
`a39fcb881f5b6f0cabbf642a1dc794d067a6e7b3`

Documentation commit:
`4310b612e029250096a3ccdd0c5e2d8117339329`

When profile filename containment fails but its canonical target equals the canonical package directory, the bootstrapper now returns:

```text
Status: Invalid
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

True escapes still return `WIF_JSONS_PROFILE_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`. The shared containment helper and the other caller fields remain unchanged. All five caller-configured filename fields now include this classification in production.

**Complete-suite 1092/1092 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1091/1091 GREEN caller current-directory owner checkpoint

Contract commit: `96e2ee67c829b5dfb290bffc2c96529bf32d72e1`

Production fix commit: `d24e62b0fd3404a3ea17acaea450643226e9125e`

Documentation commit: `8b2e4b994e47d8627cf544eaa89b1979508a4687`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1091
Total:  1091
```

`OwnerCatalogFileName = "."` now returns the owner-specific invalid filename code before provider invocation or workspace mutation. Actual escapes still return `WIF_JSONS_OWNER_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`.

Final caller-configured field for this boundary: `ProfilesFileName`.

## 2026-09-22 — caller current-directory owner catalog filename contract

Contract commit:
`96e2ee67c829b5dfb290bffc2c96529bf32d72e1`

Baseline: **1090/1090 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperCurrentDirectoryOwnerCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenOwnerCatalogFileNameResolvesToPackageDirectory_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration:

```csharp
OwnerCatalogFileName = "."
```

The value resolves exactly to the package directory and does not identify a catalog file. It does not escape the package.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

The tracking template provider must not be invoked; the initially nonexistent workspace root must remain absent.

Current owner containment returns false for the package-directory target and classifies it as `WIF_JSONS_OWNER_CATALOG_FILE_NAME_OUTSIDE_PACKAGE` instead of an invalid filename.

The focused contract confirmed the expected RED on Windows:

```text
Expected: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Actual:   WIF_JSONS_OWNER_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
```

The response status was already `Invalid`; only the error classification was wrong. `OwnerCatalogFileName = "."` resolves exactly to the package directory, not outside it.

Production fix commit:
`d24e62b0fd3404a3ea17acaea450643226e9125e`

Documentation commit:
`8b2e4b994e47d8627cf544eaa89b1979508a4687`

When owner filename containment fails but the canonical target equals the canonical package directory, the bootstrapper now returns:

```text
Status: Invalid
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

True escapes still return `WIF_JSONS_OWNER_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`. Other caller fields and the shared containment helper remain unchanged.

**Complete-suite 1091/1091 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1090/1090 GREEN caller current-directory code group checkpoint

Contract commit: `81c6375a4f8e1cc6898fe9aaabaefb720b894800`

Production fix commit: `24a21904c1dc0f2ad7952cbef186e4cc99fa9341`

Documentation commit: `a3c3b8e7c0a5d5b13c11997999e1db5f744bdea0`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1090
Total:  1090
```

`CodeGroupCatalogFileName = "."` now returns the code-group-specific invalid filename code before provider invocation or workspace mutation. Actual escapes still return `WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`.

Next caller-configured field: `OwnerCatalogFileName`.

## 2026-09-22 — caller current-directory code group catalog filename contract

Contract commit:
`81c6375a4f8e1cc6898fe9aaabaefb720b894800`

Baseline: **1089/1089 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperCurrentDirectoryCodeGroupCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameResolvesToPackageDirectory_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration:

```csharp
CodeGroupCatalogFileName = "."
```

The target resolves exactly to the package directory and does not identify a catalog file. This is not a path escaping the package.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

The tracking template provider must not be invoked and the initially nonexistent workspace root must remain absent.

Current code-group containment rejects the package-directory target as `WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`. The other already fixed caller fields are not being changed by this test.

The focused contract confirmed the expected RED on Windows:

```text
Expected: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Actual:   WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
```

The response status was already `Invalid`; the code group filename was incorrectly classified as escaping the package despite resolving exactly to the package directory.

Production fix commit:
`24a21904c1dc0f2ad7952cbef186e4cc99fa9341`

Documentation commit:
`a3c3b8e7c0a5d5b13c11997999e1db5f744bdea0`

After the shared containment check reports false, `JsonsBootstrapper` compares the canonical code group target against the canonical package directory. Equality now returns:

```text
Status: Invalid
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

True escapes still return `WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`. Other caller fields and the shared containment helper remain unchanged.

**Complete-suite 1090/1090 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1089/1089 GREEN caller current-directory category checkpoint

Contract commit: `864595065ad7f3c95fa7d954e62a3aa094b2ff34`

Production fix commit: `9a76620d14f92e3b62f4186a21800efddb864464`

Documentation commit: `323f44cf69d09196c920d3c350446b1027cb33bd`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1089
Total:  1089
```

`CategoryCatalogFileName = "."` now returns the category-specific invalid filename code before provider invocation or workspace mutation. Actual escapes still return `WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`.

Next caller-configured field: `CodeGroupCatalogFileName`.

## 2026-09-22 — caller current-directory category catalog filename contract

Contract commit:
`864595065ad7f3c95fa7d954e62a3aa094b2ff34`

Baseline: **1088/1088 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperCurrentDirectoryCategoryCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameResolvesToPackageDirectory_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration:

```csharp
CategoryCatalogFileName = "."
```

This value resolves exactly to the package directory, not outside it, and cannot identify a catalog file.

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

The tracking template provider must not be invoked; the initially nonexistent workspace root must remain absent.

Existing caller category containment is expected to report false when the target resolves to the package directory itself. Unlike the already fixed error catalog field, the category field does not yet distinguish this semantic-directory value from an actual escape. Focused RED is expected due to the `WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_OUTSIDE_PACKAGE` code.

The focused contract confirmed the expected RED on Windows:

```text
Expected: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Actual:   WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
```

The response status was already `Invalid`; only the caller-specific error classification was wrong. The target `"." ` resolves exactly to the package directory rather than escaping it.

Production fix commit:
`9a76620d14f92e3b62f4186a21800efddb864464`

Documentation commit:
`323f44cf69d09196c920d3c350446b1027cb33bd`

If the category filename fails containment but its canonical path equals the canonical package directory path, bootstrap now returns:

```text
Status: Invalid
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

True escapes still return `WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`. Shared containment logic, package-directory semantics, and the other caller fields were not changed.

**Complete-suite 1089/1089 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1088/1088 GREEN caller current-directory error filename checkpoint

Contract commit: `bf1e0e9fb853359d95f9f030882ea368847d0d70`

Production fix commit: `209505b4e79beb25571088c882a82ee55b31f420`

Documentation commit: `4b431d1d0b132fbcf0f891cb61f9de5ad0b9a70a`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1088
Total:  1088
```

`ErrorCatalogFileName = "."` now returns the field-specific invalid filename code before provider invocation or filesystem mutation. Actual escapes continue to return `WIF_JSONS_ERROR_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`.

Next caller-configured field to audit separately: `CategoryCatalogFileName`.

## 2026-09-22 — caller current-directory error filename contract

Contract commit:
`bf1e0e9fb853359d95f9f030882ea368847d0d70`

Baseline: **1087/1087 GREEN**, confirmed locally by the maintainer before the contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperCurrentDirectoryErrorCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenErrorCatalogFileNameResolvesToPackageDirectory_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration:

```csharp
ErrorCatalogFileName = "."
```

The value resolves exactly to the package directory itself. It does not escape the package but cannot identify a catalog file.

Required result:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

The tracking provider must not be invoked and the initially nonexistent workspace root must remain absent.

The existing shared containment helper reports false when a path resolves to the directory itself; current caller validation is therefore expected to return `WIF_JSONS_ERROR_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`, which has the wrong classification for this input.

The focused contract confirmed the expected RED on Windows:

```text
Expected: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Actual:   WIF_JSONS_ERROR_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
```

The response status was already `Invalid`; only the error classification was wrong. The value `"." ` resolves exactly to the package directory, not outside it.

Production fix commit:
`209505b4e79beb25571088c882a82ee55b31f420`

Documentation commit:
`4b431d1d0b132fbcf0f891cb61f9de5ad0b9a70a`

If the caller error catalog filename fails containment but its canonical path equals the canonical package directory path, the bootstrapper returns:

```text
Status: Invalid
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

Actual escapes retain `WIF_JSONS_ERROR_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`. The shared containment helper, package-directory behavior, and the other four caller filename fields were not changed.

**Complete-suite 1088/1088 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1087/1087 GREEN provider current-directory checkpoint

Contract commit: `c4367fd76d4d1f8e28314e72b5e6f337e53e090b`

Production fix commit: `cc01f3aa25d640bcc1fdc5e0ca3b35ae3e6eb7b2`

Documentation commit: `4d4a0461f4ef716210897a59d0710ed4e71dedb4`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1087
Total:  1087
```

Provider `TargetFileName = "."` now returns the invalid-target code rather than outside-package. This fixes error classification without modifying shared containment behavior or the distinct escape-path contract.

Next audit: caller-configured `ErrorCatalogFileName = "."` resolves to the package directory and should receive the caller-specific invalid filename code before any provider invocation or workspace creation.

## 2026-09-21 — provider current-directory target contract

Contract commit:
`c4367fd76d4d1f8e28314e72b5e6f337e53e090b`

Baseline: **1086/1086 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperCurrentDirectoryTemplateTargetContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenLaterTemplateTargetResolvesToPackageDirectory_ReturnsInvalidWithoutPartialWrites`

The provider returns a valid `first.json` followed by:

```csharp
TargetFileName = "."
```

The target does not escape the package; it resolves exactly to the package directory and therefore does not identify a file.

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

Because the full provider snapshot is validated before template writes, `first.json` must not be created.

Current containment logic compares the resolved target against a package-directory prefix. A target resolving exactly to the package directory does not start with that prefix and is therefore currently expected to be classified as `WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_OUTSIDE_PACKAGE` instead of the required invalid-target contract.

The focused contract confirmed the expected RED on Windows:

```text
Expected: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Actual:   WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_OUTSIDE_PACKAGE
```

The response status was already `Invalid`; the defect was classification. `TargetFileName = "."` resolves exactly to the package directory, so it is a directory target rather than an escaping path.

Production fix commit:
`cc01f3aa25d640bcc1fdc5e0ca3b35ae3e6eb7b2`

Documentation commit:
`4d4a0461f4ef716210897a59d0710ed4e71dedb4`

The provider-target path now gets a narrow equality check only when the shared containment helper reports false. If the canonical target path equals the canonical package directory path, the bootstrapper returns:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

True escapes such as parent-directory targets remain `WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_OUTSIDE_PACKAGE`. The shared `IsPathInsideDirectory` helper and `PackageDirectoryName` semantics were not changed.

**Complete-suite 1087/1087 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1086/1086 GREEN five-field existing-directory checkpoint

Final contract commit: `eeb55feb66f53e4be9fb5bed739ef7c14f44d4c0`

Final production fix commit: `d8c1cb78c7eaf362fd7693c972e645bc30571177`

Final documentation commit: `bd09568f490a88c5bd1a230223c5d55b67d1e8f5`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1086
Total:  1086
```

All five caller-configured catalog filename fields now reject values whose resolved paths already exist as directories before template-provider invocation, with their field-specific stable invalid codes and messages. The earlier provider-target existing-directory contract remains separate and GREEN.

Next distinct audit: semantic directory targets such as `TargetFileName = "."`, which currently resolve to the package directory itself and are classified by containment before the existing-directory guard can run.

## 2026-09-21 — existing-directory profiles filename contract

Contract commit:
`eeb55feb66f53e4be9fb5bed739ef7c14f44d4c0`

Baseline: **1085/1085 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperExistingDirectoryProfilesFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenProfilesFileNameResolvesToExistingDirectory_ReturnsInvalidBeforeProvider`

The package workspace already contains a `Nested` directory with `preserve.txt`, while caller configuration uses:

```csharp
ProfilesFileName = "Nested"
```

The value is lexically valid, contained in the package, and does not end with a directory separator, but resolves to a directory rather than a file.

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

The template provider must not be invoked, and the existing directory and its contents must remain unchanged.

Current production has existing-directory guards for the other four caller-configured catalog filenames; `ProfilesFileName` still stops after lexical containment, so focused RED is expected.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The lexically valid contained profile filename resolved to an existing directory, the tracking provider was reachable, and bootstrap returned success instead of rejecting caller configuration.

Production fix commit:
`d8c1cb78c7eaf362fd7693c972e645bc30571177`

Documentation commit:
`bd09568f490a88c5bd1a230223c5d55b67d1e8f5`

After the existing containment check, `JsonsBootstrapper` now resolves `ProfilesFileName` inside the package workspace and rejects it when that path already exists as a directory:

```text
Status: Invalid
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

The guard runs before template-provider invocation. Existing directory contents remain untouched, and the existing null, whitespace, malformed-path, trailing-separator, and outside-package contracts remain separate.

All five caller-configured catalog filename fields now contain the existing-directory guard in production.

**Complete-suite 1086/1086 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1085/1085 GREEN existing-directory owner filename checkpoint

Contract commit: `40ac5516b7c4c0f6d1c8d711f64f99ff55a23d35`

Production fix commit: `ce3748f1c843ba42051fee82585bbbefa3528411`

Documentation commit: `ae8324f8dd80a32d2cef9f14f59425ef5215cd9c`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1085
Total:  1085
```

`OwnerCatalogFileName` values resolving to existing directories are now rejected as caller configuration before provider invocation. Existing directory contents remain unchanged.

Final caller-configured field in this series: `ProfilesFileName`.

## 2026-09-21 — existing-directory owner catalog filename contract

Contract commit:
`40ac5516b7c4c0f6d1c8d711f64f99ff55a23d35`

Baseline: **1084/1084 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperExistingDirectoryOwnerCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenOwnerCatalogFileNameResolvesToExistingDirectory_ReturnsInvalidBeforeProvider`

The package workspace already contains a `Nested` directory with `preserve.txt`, while caller configuration uses:

```csharp
OwnerCatalogFileName = "Nested"
```

The value is lexically valid, contained in the package, and does not end with a directory separator, but resolves to a directory rather than a file.

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

The template provider must not be invoked, and the existing directory and its contents must remain unchanged.

Current production has existing-directory guards for the error, category, and code-group fields only; the owner field still stops after lexical containment, so focused RED is expected.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The lexically valid contained owner filename resolved to an existing directory, the tracking provider was reachable, and bootstrap returned success instead of rejecting caller configuration.

Production fix commit:
`ce3748f1c843ba42051fee82585bbbefa3528411`

Documentation commit:
`ae8324f8dd80a32d2cef9f14f59425ef5215cd9c`

After the existing containment check, `JsonsBootstrapper` now resolves `OwnerCatalogFileName` inside the package workspace and rejects it when that path already exists as a directory:

```text
Status: Invalid
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

The guard runs before template-provider invocation. Existing directory contents remain untouched, and the existing null, whitespace, malformed-path, trailing-separator, and outside-package contracts remain separate.

**Complete-suite 1085/1085 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1084/1084 GREEN existing-directory code group filename checkpoint

Contract commit: `541630e28b68109f5f9ad2bb4b216f794ace985d`

Production fix commit: `43ca724ee46dfe8aae71d16860bafd51810f9dad`

Documentation commit: `38c9a8ba5b65f52bb810ca43299d63dd898306eb`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1084
Total:  1084
```

`CodeGroupCatalogFileName` values resolving to existing directories are now rejected as caller configuration before provider invocation. Existing directory contents remain unchanged.

Next caller-configured field: `OwnerCatalogFileName`.

## 2026-09-21 — existing-directory code group catalog filename contract

Contract commit:
`541630e28b68109f5f9ad2bb4b216f794ace985d`

Baseline: **1083/1083 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperExistingDirectoryCodeGroupCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameResolvesToExistingDirectory_ReturnsInvalidBeforeProvider`

The package workspace already contains a `Nested` directory with `preserve.txt`, while caller configuration uses:

```csharp
CodeGroupCatalogFileName = "Nested"
```

The value is lexically valid, contained in the package, and does not end with a directory separator, but resolves to a directory rather than a file.

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

The template provider must not be invoked, and the existing directory and its contents must remain unchanged.

Current production has existing-directory guards for the error and category fields only; the code-group field still stops after lexical containment, so focused RED is expected.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The lexically valid contained code-group filename resolved to an existing directory, the tracking provider was reachable, and bootstrap returned success instead of rejecting caller configuration.

Production fix commit:
`43ca724ee46dfe8aae71d16860bafd51810f9dad`

Documentation commit:
`38c9a8ba5b65f52bb810ca43299d63dd898306eb`

After the existing containment check, `JsonsBootstrapper` now resolves `CodeGroupCatalogFileName` inside the package workspace and rejects it when that path already exists as a directory:

```text
Status: Invalid
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

The guard runs before template-provider invocation. Existing directory contents remain untouched, and the existing null, whitespace, malformed-path, trailing-separator, and outside-package contracts remain separate.

**Complete-suite 1084/1084 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1083/1083 GREEN existing-directory category filename checkpoint

Contract commit: `043634a61d6f7de7d91c665db81191d39b3f9614`

Production fix commit: `dacff0d269f327600906262ce25d206521db2393`

Documentation commit: `1c9852d025a18a3f4a4297af43a06445ebe82453`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1083
Total:  1083
```

`CategoryCatalogFileName` values resolving to existing directories are now rejected as caller configuration before provider invocation. Existing directory contents remain unchanged.

Next caller-configured field: `CodeGroupCatalogFileName`.

## 2026-09-21 — existing-directory category catalog filename contract

Contract commit:
`043634a61d6f7de7d91c665db81191d39b3f9614`

Baseline: **1082/1082 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperExistingDirectoryCategoryCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameResolvesToExistingDirectory_ReturnsInvalidBeforeProvider`

The package workspace already contains a `Nested` directory with `preserve.txt`, while caller configuration uses:

```csharp
CategoryCatalogFileName = "Nested"
```

The value is lexically valid, contained in the package, and does not end with a directory separator, but resolves to a directory rather than a file.

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

The template provider must not be invoked, and the existing directory and its contents must remain unchanged.

Current production has the existing-directory guard only for `ErrorCatalogFileName`; the category field still stops after lexical containment, so focused RED is expected.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The lexically valid contained category filename resolved to an existing directory, the tracking provider was reachable, and bootstrap returned success instead of rejecting caller configuration.

Production fix commit:
`dacff0d269f327600906262ce25d206521db2393`

Documentation commit:
`1c9852d025a18a3f4a4297af43a06445ebe82453`

After the existing containment check, `JsonsBootstrapper` now resolves `CategoryCatalogFileName` inside the package workspace and rejects it when that path already exists as a directory:

```text
Status: Invalid
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

The guard runs before template-provider invocation. Existing directory contents remain untouched, and the existing null, whitespace, malformed-path, trailing-separator, and outside-package contracts remain separate.

**Complete-suite 1083/1083 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1082/1082 GREEN existing-directory error filename checkpoint

Contract commit: `39e90ff34020818a7f1c7bccb3ee27249f18619a`

Production fix commit: `deb62864b575195fc0d8ce63dcafe80ed109ef1b`

Documentation commit: `bf12036cf2d32a20e766a4e6740bfb8903aff5ef`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1082
Total:  1082
```

`ErrorCatalogFileName` values resolving to existing directories are now rejected as caller configuration before provider invocation. Existing directory contents remain unchanged.

Next caller-configured field: `CategoryCatalogFileName`.

## 2026-09-21 — existing-directory error catalog filename contract

Contract commit:
`39e90ff34020818a7f1c7bccb3ee27249f18619a`

Baseline: **1081/1081 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperExistingDirectoryErrorCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenErrorCatalogFileNameResolvesToExistingDirectory_ReturnsInvalidBeforeProvider`

The package workspace already contains a `Nested` directory with `preserve.txt`, while caller configuration uses:

```csharp
ErrorCatalogFileName = "Nested"
```

The value is lexically valid, contained in the package, and does not end with a directory separator, but it resolves to a directory rather than a file.

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

The template provider must not be invoked, and the existing directory and its contents must remain unchanged.

Current production validates lexical containment but does not check whether the caller-configured target already resolves to a directory. With the tracking provider returning no templates, current behavior is expected to complete successfully.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The lexically valid contained caller filename resolved to an existing directory, the tracking provider was reachable, and bootstrap returned success instead of rejecting caller configuration.

Production fix commit:
`deb62864b575195fc0d8ce63dcafe80ed109ef1b`

Documentation commit:
`bf12036cf2d32a20e766a4e6740bfb8903aff5ef`

After the existing containment check, `JsonsBootstrapper` now resolves `ErrorCatalogFileName` inside the package workspace and rejects it when that path already exists as a directory:

```text
Status: Invalid
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

The guard runs before template-provider invocation. Existing directory contents remain untouched, and the existing null, whitespace, malformed-path, trailing-separator, and outside-package contracts remain separate.

**Complete-suite 1082/1082 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1081/1081 GREEN existing-directory provider-target checkpoint

Contract commit: `fe1c1ab2b8886f0c3cf2af3584eaa7c5885ace33`

Production fix commit: `3567953e5d83613b41f667251f83bad154cfe615`

Documentation commit: `afc99aab299391255953217fd48a298aef46a847`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1081
Total:  1081
```

Provider targets resolving to existing directories are rejected during full-snapshot validation before any template write. Existing directory contents remain untouched and valid nested file targets remain supported.

Next caller-configuration audit: distinguish a file-name option that lexically looks valid but resolves to an already existing directory before invoking the provider.

## 2026-09-21 — existing-directory template target contract

Contract commit:
`fe1c1ab2b8886f0c3cf2af3584eaa7c5885ace33`

Baseline: **1080/1080 GREEN**, locally confirmed by the maintainer before this new test.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperExistingDirectoryTemplateTargetContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenLaterTemplateTargetIsExistingDirectory_ReturnsInvalidWithoutPartialWrites`

The workspace contains an existing `Nested` directory with `preserve.txt`. The provider returns two templates: a valid `first.json` followed by `TargetFileName = "Nested"` (no trailing separator). The latter is syntactically acceptable and inside the package lexically, but names a directory rather than a file.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

Require that `first.json` is not written and the pre-existing directory and its contents remain unchanged. This checks validation of the entire provider snapshot before file writes.

Current production rejects directory-only names with trailing separators but does not check whether a separator-free target already resolves to a directory. The second write is expected to fail as a filesystem error, after the first template was created.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The later provider target named an existing directory. Current production classified it only at the write phase, after the first valid template could already be written.

Production fix commit:
`3567953e5d83613b41f667251f83bad154cfe615`

Documentation commit:
`afc99aab299391255953217fd48a298aef46a847`

During full-snapshot validation, `JsonsBootstrapper` now checks whether each contained, normalized provider target resolves to an existing directory and rejects it before any template files are written:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

Existing directories and their files remain untouched, and valid nested file targets remain supported. This is a validation-time safeguard, not a transactional guarantee against unrelated I/O failures or concurrent filesystem changes.

**Complete-suite 1081/1081 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1080/1080 GREEN five-field directory-only checkpoint

Contract commit: `306b7be4148dc73a268376dd5c36fdc36fb5d1b2`

Production fix commit: `e74fff6c9624e93e857fba49769694b3c9b14622`

Documentation commit: `85969b3a56ea1336c432e95da84c312404a99076`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1080
Total:  1080
```

Directory-only caller filenames are now rejected for all five catalog fields before invoking the provider or mutating the workspace. Previously verified null, whitespace, malformed-syntax, containment, and valid nested-target contracts remain covered by the complete suite.

Next boundary to audit: a provider target that syntactically looks like a file, but resolves to an existing directory in the workspace.

## 2026-09-21 — directory-only profiles filename contract

Contract commit:
`306b7be4148dc73a268376dd5c36fdc36fb5d1b2`

Baseline: **1079/1079 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperDirectoryProfilesFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenProfilesFileNameEndsWithDirectorySeparator_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration uses:

```csharp
ProfilesFileName =
    "Nested" + Path.DirectorySeparatorChar
```

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

The template provider must not be invoked and the workspace root must not be created.

Current caller validation checks containment but does not yet reject a directory-only profile catalog target, so focused RED is expected.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The caller-configured directory-only profile filename passed early validation and allowed bootstrap to complete successfully.

Production fix commit:
`e74fff6c9624e93e857fba49769694b3c9b14622`

Documentation commit:
`85969b3a56ea1336c432e95da84c312404a99076`

`JsonsBootstrapper` now normalizes `ProfilesFileName` and rejects values ending with a directory separator before containment, provider invocation, or filesystem mutation:

```text
Status: Invalid
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

Its existing malformed-path and outside-package contracts remain separate. All five caller-configured filename fields now contain the directory-only guard in production, but full-suite verification of the last guard is still pending.

**Complete-suite 1080/1080 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1079/1079 GREEN directory-only owner filename checkpoint

Contract commit: `2c7abb14f8d442a51bf377ef2e51d6e4dabf4f8d`

Production fix commit: `90b10457c57abe87af8465037f6fa9409ba8af3e`

Documentation commit: `82f54c23d63287a6928c8c48f3d204314f5a59c8`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1079
Total:  1079
```

Directory-only `OwnerCatalogFileName` values are now rejected as caller configuration before provider invocation or filesystem mutation.

Final caller-configured field in this group: `ProfilesFileName`.

## 2026-09-21 — directory-only owner catalog filename contract

Contract commit:
`2c7abb14f8d442a51bf377ef2e51d6e4dabf4f8d`

Baseline: **1078/1078 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperDirectoryOwnerCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenOwnerCatalogFileNameEndsWithDirectorySeparator_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration uses:

```csharp
OwnerCatalogFileName =
    "Nested" + Path.DirectorySeparatorChar
```

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

The template provider must not be invoked and the workspace root must not be created.

Current caller validation checks containment but does not yet reject a directory-only owner catalog target, so focused RED is expected.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The caller-configured directory-only owner catalog filename passed early validation and allowed bootstrap to complete successfully.

Production fix commit:
`90b10457c57abe87af8465037f6fa9409ba8af3e`

Documentation commit:
`82f54c23d63287a6928c8c48f3d204314f5a59c8`

`JsonsBootstrapper` now normalizes `OwnerCatalogFileName` and rejects values ending with a directory separator before containment, provider invocation, or filesystem mutation:

```text
Status: Invalid
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

Its existing malformed-path and outside-package contracts remain separate.

**Complete-suite 1079/1079 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1078/1078 GREEN directory-only code group filename checkpoint

Contract commit: `b6a5edd311c0fa912664e9efbb437d72112148f7`

Production fix commit: `4cf9fd93aa2a7eeeddc45e0b80c0ed7e5894ccd1`

Documentation commit: `5722d1f59596d28afca83bae5f291b20ec2265df`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1078
Total:  1078
```

Directory-only `CodeGroupCatalogFileName` values are now rejected as caller configuration before provider invocation or filesystem mutation.

Next caller-configured field: `OwnerCatalogFileName`.

## 2026-09-21 — directory-only code group catalog filename contract

Contract commit:
`b6a5edd311c0fa912664e9efbb437d72112148f7`

Baseline: **1077/1077 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperDirectoryCodeGroupCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameEndsWithDirectorySeparator_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration uses:

```csharp
CodeGroupCatalogFileName =
    "Nested" + Path.DirectorySeparatorChar
```

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

The template provider must not be invoked and the workspace root must not be created.

Current caller validation checks containment but does not yet reject a directory-only code-group catalog target, so focused RED is expected.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The caller-configured directory-only code-group catalog filename passed early validation and allowed bootstrap to complete successfully.

Production fix commit:
`4cf9fd93aa2a7eeeddc45e0b80c0ed7e5894ccd1`

Documentation commit:
`5722d1f59596d28afca83bae5f291b20ec2265df`

`JsonsBootstrapper` now normalizes `CodeGroupCatalogFileName` and rejects values ending with a directory separator before containment, provider invocation, or filesystem mutation:

```text
Status: Invalid
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

Its existing malformed-path and outside-package contracts remain separate.

**Complete-suite 1078/1078 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1077/1077 GREEN directory-only category filename checkpoint

Contract commit: `73fd638f4f4d7c9906e0ed4b7f13d41ba81bc418`

Production fix commit: `cb605b2628ed9dd55e16bd3a89e9bf580f87afda`

Documentation commit: `eb126168b4e5c7a794c0a3bbc07158723c21e1a8`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1077
Total:  1077
```

Directory-only `CategoryCatalogFileName` values are now rejected as caller configuration before provider invocation or filesystem mutation.

Next caller-configured field: `CodeGroupCatalogFileName`.

## 2026-09-21 — directory-only category catalog filename contract

Contract commit:
`73fd638f4f4d7c9906e0ed4b7f13d41ba81bc418`

Baseline: **1076/1076 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperDirectoryCategoryCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameEndsWithDirectorySeparator_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration uses:

```csharp
CategoryCatalogFileName =
    "Nested" + Path.DirectorySeparatorChar
```

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

The template provider must not be invoked and the workspace root must not be created.

Current caller validation checks containment but does not yet reject a directory-only category catalog target, so focused RED is expected.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The caller-configured directory-only category catalog filename passed early validation and allowed bootstrap to complete successfully.

Production fix commit:
`cb605b2628ed9dd55e16bd3a89e9bf580f87afda`

Documentation commit:
`eb126168b4e5c7a794c0a3bbc07158723c21e1a8`

`JsonsBootstrapper` now normalizes `CategoryCatalogFileName` and rejects values ending with a directory separator before containment, provider invocation, or filesystem mutation:

```text
Status: Invalid
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

Its existing malformed-path and outside-package contracts remain separate.

**Complete-suite 1077/1077 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1076/1076 GREEN directory-only error filename checkpoint

Contract commit: `10e5ade814b41bd78ac5cdd15235386e836f1a00`

Production fix commit: `65fdd4baf0579b4fda6822c1b456c1ec438a9111`

Documentation commit: `a5c4aea3d66ef31c1858e9095fc652c2679e4c2d`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1076
Total:  1076
```

Directory-only `ErrorCatalogFileName` values are now rejected as caller configuration before provider invocation or filesystem mutation.

Next caller-configured field: `CategoryCatalogFileName`.

## 2026-09-21 — directory-only error catalog filename contract

Contract commit:
`10e5ade814b41bd78ac5cdd15235386e836f1a00`

Baseline: **1075/1075 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperDirectoryErrorCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenErrorCatalogFileNameEndsWithDirectorySeparator_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration uses:

```csharp
ErrorCatalogFileName =
    "Nested" + Path.DirectorySeparatorChar
```

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

The template provider must not be invoked and the workspace root must not be created.

This keeps malformed caller configuration distinct from malformed provider output. Current caller validation checks containment but not whether the configured catalog target denotes a file, so this value is expected to pass early validation.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The caller-configured directory-only error catalog filename passed early validation and allowed bootstrap to complete successfully.

Production fix commit:
`65fdd4baf0579b4fda6822c1b456c1ec438a9111`

Documentation commit:
`a5c4aea3d66ef31c1858e9095fc652c2679e4c2d`

`JsonsBootstrapper` now normalizes `ErrorCatalogFileName` and rejects values ending with a directory separator before containment, provider invocation, or filesystem mutation:

```text
Status: Invalid
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

The existing outside-package and malformed-path contracts remain separate.

**Complete-suite 1076/1076 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1075/1075 GREEN directory-only provider-target checkpoint

Contract commit: `521e6390293d464dd7cda6fe2d2f2834099e112c`

Production fix commit: `481c91eab264de58a55599dbd542db309e077b77`

Documentation commit: `fede769c4fd70a621f91565aa04edb0e3dc537b5`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1075
Total:  1075
```

Directory-only provider targets are rejected as stable invalid provider output before nested-directory creation or file writes. Valid nested file targets remain supported.

Next caller-configuration audit: apply the same file-target requirement to caller-configured catalog filenames before provider invocation and filesystem mutation.

## 2026-09-21 — directory-only template target contract

Contract commit:
`521e6390293d464dd7cda6fe2d2f2834099e112c`

Baseline: **1074/1074 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperDirectoryTemplateTargetFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenTemplateTargetEndsWithDirectorySeparator_ReturnsInvalidBeforeCreatingNestedDirectory`

The provider returns a target constructed as:

```csharp
"Nested" + Path.DirectorySeparatorChar
```

This path resolves inside the package workspace, so containment alone accepts it, but it denotes a directory path rather than a file target.

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

The nested directory must not be created.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The directory-only target passed containment and was classified only after the filesystem write path failed.

Production fix commit:
`481c91eab264de58a55599dbd542db309e077b77`

Documentation commit:
`fede769c4fd70a621f91565aa04edb0e3dc537b5`

`WhenItFails/Docs/Bootstrap/en.md` now documents that template targets must identify files, not directory-only paths ending with a directory separator.

`JsonsBootstrapper` now normalizes the provider target during the full-snapshot validation phase and rejects values ending with a directory separator before containment/write processing:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

Valid nested file targets such as `Nested/errors.en.json` remain supported.

**Complete-suite 1075/1075 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1074/1074 GREEN full template-snapshot validation checkpoint

Contract commit: `22afb9d9aba703f015adadc22495a8a839b67d5c`

Production fix commit: `2f7759369ca0aab51e9c46205628195cbf36b244`

Documentation commit: `c6ed1c36b1e092a3df3c2fff8f966cd048fb69ec`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1074
Total:  1074
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system.

The complete materialized provider snapshot is now validated before the first template file write, preventing malformed later items from leaving earlier provider files partially created.

Next provider-target audit: distinguish a valid nested file target from a directory-only target that ends with a directory separator.

## 2026-09-21 — later null template item no-partial-write contract

Contract commit:
`22afb9d9aba703f015adadc22495a8a839b67d5c`

Baseline: **1073/1073 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperLaterNullTemplateItemContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenLaterTemplateItemIsNull_ReturnsInvalidBeforeWritingAnyTemplateFiles`

The provider returns a materialized collection containing:

1. a valid first template targeting `first.json`,
2. a later `null` template item.

Require the existing malformed-item response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_ITEM_NULL
Message: The JSON template provider returned a null template item.
```

and require that `first.json` is not written.

This verifies that a malformed provider snapshot is validated as a whole before any template file mutation. Current production validates and writes in the same loop, so the first valid template is expected to be created before the later null item is discovered.

The focused contract confirmed the expected RED on Windows: the bootstrap correctly returned the existing `WIF_JSONS_TEMPLATE_ITEM_NULL` response, but `first.json` had already been created before the later null item was discovered.

Production fix commit:
`2f7759369ca0aab51e9c46205628195cbf36b244`

Documentation commit:
`c6ed1c36b1e092a3df3c2fff8f966cd048fb69ec`

`WhenItFails/Docs/Bootstrap/en.md` now documents full-snapshot validation before the first template file write and the resulting no-partial-write guarantee for malformed provider output.

Template processing now uses two phases:

```text
materialize provider collection
→ validate every template item
→ only then write template files
```

All existing provider-item error codes and messages remain unchanged. Cancellation checks remain present during validation and again before each file write.

**Complete-suite 1074/1074 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1073/1073 GREEN template enumeration-cancellation checkpoint

Contract commit: `62c0ece1813790bfcb586e1a046bf7bff89b444c`

No production change was required.

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1073
Total:  1073
```

The exact `OperationCanceledException` instance thrown while enumerating the returned template collection propagates unchanged, and no template file is written.

Next provider-output hardening target: validate the entire materialized template snapshot before any template file write so a malformed later item cannot leave an earlier valid item partially written.

## 2026-09-21 — template collection enumeration cancellation contract

Contract commit:
`62c0ece1813790bfcb586e1a046bf7bff89b444c`

Baseline: **1072/1072 GREEN**, confirmed locally by the maintainer before this regression contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperTemplateCollectionEnumerationCancellationContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenTemplateCollectionEnumerationCancels_RethrowsSameOperationCanceledException`

The provider call succeeds and returns an `IReadOnlyList<JsonsTemplateFile>`, but the returned collection throws a specific `OperationCanceledException` instance from `GetEnumerator()`.

Require:

- the same exception instance to propagate unchanged,
- no conversion into `WIF_JSONS_TEMPLATE_PROVIDER_FAILED`,
- no template file write.

The current collection-materialization catch filter already excludes `OperationCanceledException`, so this is expected to be a focused **GREEN regression contract** with no production change.

**Focused and complete-suite GREEN were subsequently confirmed locally by the maintainer; total suite: 1073/1073.**

## 2026-09-21 — 1072/1072 GREEN template enumeration-failure checkpoint

Contract commit: `6abb4d38ecae08d2cf4d495907671855304b08fd`

Production fix commit: `f26676980a5654efb31f5e7ac0012942f9daa613`

Documentation commit: `8f8ef9583dfc9e713381fcbddbf0d8febfd9be6a`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1072
Total:  1072
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system.

Ordinary exceptions thrown while consuming the returned template collection now normalize to `WIF_JSONS_TEMPLATE_PROVIDER_FAILED` without exposing provider detail. Collection materialization also prevents partial template writes caused by a later enumeration failure.

Next regression contract: exact-instance `OperationCanceledException` propagation when cancellation is thrown during returned-collection enumeration.

## 2026-09-21 — template collection enumeration exception contract

Contract commit:
`6abb4d38ecae08d2cf4d495907671855304b08fd`

Baseline: **1071/1071 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperTemplateCollectionEnumerationExceptionContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenTemplateCollectionEnumerationThrows_ReturnsStableProviderFailureWithoutExceptionDetail`

The provider call itself succeeds and returns an `IReadOnlyList<JsonsTemplateFile>`, but the returned collection throws `InvalidOperationException` from `GetEnumerator()`.

Require:

```text
Status: Failed
Data: null
Code: WIF_JSONS_TEMPLATE_PROVIDER_FAILED
Message: The JSON template provider failed.
```

The provider/collection exception detail must not escape into either the response message or issue message.

This is distinct from the already verified direct `GetTemplateFiles(...)` exception contract: the current `try/catch` ends before the returned collection is enumerated, so deferred provider-output failures can currently escape.

The focused contract confirmed the expected RED on Windows:

```text
System.InvalidOperationException:
Sensitive template collection enumeration detail must not escape.
```

The exception escaped from the provider collection's `GetEnumerator()` after `GetTemplateFiles(...)` itself had already returned successfully.

Production fix commit:
`f26676980a5654efb31f5e7ac0012942f9daa613`

Documentation commit:
`8f8ef9583dfc9e713381fcbddbf0d8febfd9be6a`

`WhenItFails/Docs/Bootstrap/en.md` now documents collection-consumption failure normalization and preservation of cancellation semantics.

The returned template collection is now materialized into a snapshot inside a narrow provider-boundary `try/catch`. Ordinary exceptions raised while consuming the collection map to:

```text
Status: Failed
Code: WIF_JSONS_TEMPLATE_PROVIDER_FAILED
Message: The JSON template provider failed.
```

`OperationCanceledException` remains excluded from normalization and continues to propagate unchanged. Template items are processed only after successful collection materialization, so an enumeration failure cannot cause partial template writes.

**Complete-suite 1072/1072 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1071/1071 GREEN whitespace template-name checkpoint

Contract commit: `b82283c81eef5af862194f5b5dde8547d8d542a7`

Production fix commit: `ded57fb513b614a5f0b3c5a0d3675400e86a2327`

Documentation commit: `e058dae8d0afce8fbf70e81f37a66864b08fe218`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1071
Total:  1071
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system.

Null and whitespace-only logical template names are now rejected before target validation and before any template file write.

Next provider boundary audit: failures that occur while enumerating the collection returned by `IJsonsTemplateProvider.GetTemplateFiles(...)`, after the provider call itself has already succeeded.

## 2026-09-21 — whitespace template name provider-output contract

Contract commit:
`b82283c81eef5af862194f5b5dde8547d8d542a7`

Baseline: **1070/1070 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperWhitespaceTemplateNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenTemplateNameIsWhitespace_ReturnsInvalidBeforeWritingTemplateFile`

A malformed provider returns:

```csharp
Name = "   "
TargetFileName = "errors.en.json"
Content = "{}"
```

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_NAME_EMPTY
Message: The JSON template provider returned a template with an empty name.
```

The target file must not be written.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

Current production created the template file and returned `Success` despite `Name = "   "`.

Production fix commit:
`ded57fb513b614a5f0b3c5a0d3675400e86a2327`

Documentation commit:
`e058dae8d0afce8fbf70e81f37a66864b08fe218`

`JsonsBootstrapper` now rejects whitespace-only logical names before target validation and before writing the target file:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_NAME_EMPTY
Message: The JSON template provider returned a template with an empty name.
```

`WhenItFails/Docs/Bootstrap/en.md` now documents both null and whitespace-only logical template names as invalid provider output.

**Complete-suite 1071/1071 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1070/1070 GREEN null template-name checkpoint

Contract commit: `d2ca307b856d0fb973f9f3f738a667617fb0c760`

Production fix commit: `25942620071fcbb28de37739096b167b46c93bf8`

Documentation commit: `1b5479baf216d1cc6a9f2892ae4a10aed7a57bd2`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1070
Total:  1070
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system.

Null logical template names are now rejected before target validation and before any template file write.

Next provider-output boundary: whitespace-only logical template names.

## 2026-09-21 — null template name provider-output contract

Contract commit:
`d2ca307b856d0fb973f9f3f738a667617fb0c760`

Baseline: **1069/1069 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNullTemplateNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenTemplateNameIsNull_ReturnsInvalidBeforeWritingTemplateFile`

A malformed provider returns:

```csharp
Name = null
TargetFileName = "errors.en.json"
Content = "{}"
```

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_NAME_NULL
Message: The JSON template provider returned a template with a null name.
```

The target file must not be written.

This closes a distinct provider-output gap: `JsonsTemplateFile.Name` and `JsonsBootstrapFileResult.Name` are public non-nullable strings, but current bootstrap code does not validate the logical name before creating the file and copying the value into the result.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

Current production created the template file and returned `Success` despite `Name = null`.

Production fix commit:
`25942620071fcbb28de37739096b167b46c93bf8`

Documentation commit:
`1b5479baf216d1cc6a9f2892ae4a10aed7a57bd2`

`JsonsBootstrapper` now rejects a null logical template name before target validation and before writing the target file:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_NAME_NULL
Message: The JSON template provider returned a template with a null name.
```

`WhenItFails/Docs/Bootstrap/en.md` now documents logical template names as provider output and records that null names are rejected before template writes.

**Complete-suite 1070/1070 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1069/1069 GREEN nested template-target checkpoint

Contract commit: `60b790fa37b57e23d77b865e766a7d64b4fa8043`

Production fix commit: `eab18d989552581caeb24e1ad5cc7177747101a3`

Documentation commit: `cbba2875f8c0afac217ce891d739055fbc8b06be`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1069
Total:  1069
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system.

Valid nested provider targets are now supported: missing parent directories are created only for missing validated targets, while existing files remain preserved and skipped.

Next provider-output audit target: null logical template `Name`, which is copied into the public non-nullable `JsonsBootstrapFileResult.Name`.

## 2026-09-21 — valid nested template target contract

Contract commit:
`60b790fa37b57e23d77b865e766a7d64b4fa8043`

Baseline: **1068/1068 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedTemplateTargetFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenTemplateTargetIsNestedInsidePackage_CreatesParentDirectoryAndFile`

The provider returns a valid target inside the package workspace:

```text
Nested/errors.en.json
```

The contract requires successful bootstrap, creation of the missing `Nested` parent directory, creation of the target file with the provider content, and a successful `JsonsBootstrapFileResult`.

This is a positive path contract, not malformed-input normalization. The existing containment guard already classifies this target as inside the package. Current `EnsureTemplateFileAsync` writes directly to the nested target but does not create its parent directory, so the current implementation is expected to return `JsonsWorkspaceInputOutputError` through the outer `IOException` normalization.

The focused contract confirmed RED on Windows: the bootstrap response was not successful because the nested target parent directory did not exist and the write degraded into the existing workspace I/O failure path.

Production fix commit:
`eab18d989552581caeb24e1ad5cc7177747101a3`

Documentation commit:
`cbba2875f8c0afac217ce891d739055fbc8b06be`

`WhenItFails/Docs/Bootstrap/en.md` now documents valid nested template targets, automatic creation of missing parent directories, containment inside the package workspace, and preservation of existing files.

`EnsureTemplateFileAsync` now creates the missing parent directory for a validated target only when the target file itself does not already exist. Existing files still return immediately as `Skipped` and are never overwritten.

**Complete-suite 1069/1069 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1068/1068 GREEN malformed provider-target checkpoint

Contract commit: `aea40cc601b04f13fc9f69355e3d4a85cc52bbfb`

Production fix commit: `11409034e05e891863f7f17bc4d517ff2a79edfc`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1068
Total:  1068
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system.

Malformed provider target syntax now returns stable `Invalid` without changing the established null, whitespace, or outside-package target contracts.

Next audit target: valid nested template targets inside the package workspace. Containment permits them, but `EnsureTemplateFileAsync` currently writes directly to the nested target without creating its parent directory.

## 2026-09-21 — malformed template target filename path contract

Contract commit:
`aea40cc601b04f13fc9f69355e3d4a85cc52bbfb`

Baseline: **1067/1067 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperInvalidTemplateTargetFileNamePathContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenTemplateTargetFileNameContainsNullCharacter_ReturnsInvalidBeforeWritingTemplateFiles`

The template provider returns an invalid first target:

```text
errors\0.en.json
```

followed by a valid sentinel target. Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

The contract also requires that the later sentinel template file is not written. Unlike caller-configuration validation, the package workspace may already exist at this provider-output boundary because provider invocation occurs after workspace creation.

This remains distinct from the existing `WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_OUTSIDE_PACKAGE` containment contract.

The focused contract confirmed the expected RED on Windows:

```text
System.ArgumentException: Null character in path.
```

The exception escaped from `Path.GetFullPath(...)` inside `IsPathInsideDirectory(...)` after workspace creation but before any template file was written.

Production fix commit:
`11409034e05e891863f7f17bc4d517ff2a79edfc`

Template-target containment evaluation is now isolated in a narrow `try/catch (ArgumentException)`. Malformed provider target syntax maps to:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

The existing null, whitespace, and outside-package target contracts remain unchanged.

**Complete-suite 1068/1068 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1067/1067 GREEN malformed caller-path checkpoint

Contract commit: `514339b43269b7d25584a851acfc7138fe15d48a`

Production fix commit: `32357c6033b8be4c6ba8247baf39d73f5c21b385`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1067
Total:  1067
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system.

Malformed-path syntax handling is now locally verified GREEN for:

- `RootDirectory`
- `PackageDirectoryName`
- `ErrorCatalogFileName`
- `CategoryCatalogFileName`
- `CodeGroupCatalogFileName`
- `OwnerCatalogFileName`
- `ProfilesFileName`

The next distinct boundary is malformed `TargetFileName` returned by `IJsonsTemplateProvider`; preserve the existing provider-target containment contract separately.

## 2026-09-21 — malformed profile-catalog filename path contract

Contract commit:
`514339b43269b7d25584a851acfc7138fe15d48a`

Baseline: **1066/1066 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperInvalidProfilesFileNamePathContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenProfilesFileNameContainsNullCharacter_ReturnsInvalidBeforeProviderOrFilesystem`

With `ProfilesFileName = "profiles\0.en.json"`, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

The contract also requires no template-provider invocation and no workspace-root creation.

The focused contract confirmed the expected RED on Windows:

```text
System.ArgumentException: Null character in path.
```

The exception escaped from `Path.GetFullPath(...)` inside `IsPathInsideDirectory(...)` before filesystem mutation or template-provider invocation.

Production fix commit:
`32357c6033b8be4c6ba8247baf39d73f5c21b385`

The profile-catalog containment evaluation is now isolated in a narrow `try/catch (ArgumentException)`. Malformed profile-catalog filename syntax maps to:

```text
Status: Invalid
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

A syntactically valid filename outside the package continues to use the separate `WIF_JSONS_PROFILE_CATALOG_FILE_NAME_OUTSIDE_PACKAGE` contract.

**Complete-suite 1067/1067 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1066/1066 GREEN malformed owner-catalog filename checkpoint

Contract commit: `b0084b060adb8234848c567f9018b004f0af1939`

Production fix commit: `a2157487af81894a62c8989339a93c516661a8cb`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1066
Total:  1066
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system. A syntactically malformed `OwnerCatalogFileName` now returns stable `Invalid` instead of leaking `ArgumentException`.

Next malformed-path boundary: `ProfilesFileName`.

## 2026-09-21 — malformed owner-catalog filename path contract

Contract commit:
`b0084b060adb8234848c567f9018b004f0af1939`

Baseline: **1065/1065 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperInvalidOwnerCatalogFileNamePathContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenOwnerCatalogFileNameContainsNullCharacter_ReturnsInvalidBeforeProviderOrFilesystem`

With `OwnerCatalogFileName = "owners\0.en.json"`, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

The contract also requires no template-provider invocation and no workspace-root creation.

The focused contract confirmed the expected RED on Windows:

```text
System.ArgumentException: Null character in path.
```

The exception escaped from `Path.GetFullPath(...)` inside `IsPathInsideDirectory(...)` before filesystem mutation or template-provider invocation.

Production fix commit:
`a2157487af81894a62c8989339a93c516661a8cb`

The owner-catalog containment evaluation is now isolated in a narrow `try/catch (ArgumentException)`. Malformed owner-catalog filename syntax maps to:

```text
Status: Invalid
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

A syntactically valid filename outside the package continues to use the separate `WIF_JSONS_OWNER_CATALOG_FILE_NAME_OUTSIDE_PACKAGE` contract.

**Complete-suite 1066/1066 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1065/1065 GREEN malformed code-group filename checkpoint

Contract commit: `7efc266266f15437f6c533814e8648d29d51e48d`

Production fix commit: `d1013def0656dde8493e907ec9bd6545b0cabbcd`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1065
Total:  1065
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system. A syntactically malformed `CodeGroupCatalogFileName` now returns stable `Invalid` instead of leaking `ArgumentException`.

Next malformed-path boundary: `OwnerCatalogFileName`.

## 2026-09-21 — malformed code-group-catalog filename path contract

Contract commit:
`7efc266266f15437f6c533814e8648d29d51e48d`

Baseline: **1064/1064 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperInvalidCodeGroupCatalogFileNamePathContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameContainsNullCharacter_ReturnsInvalidBeforeProviderOrFilesystem`

With `CodeGroupCatalogFileName = "code-groups\0.en.json"`, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

The contract also requires no template-provider invocation and no workspace-root creation.

The focused contract confirmed the expected RED on Windows:

```text
System.ArgumentException: Null character in path.
```

The exception escaped from `Path.GetFullPath(...)` inside `IsPathInsideDirectory(...)` before filesystem mutation or template-provider invocation.

Production fix commit:
`d1013def0656dde8493e907ec9bd6545b0cabbcd`

The code-group catalog containment evaluation is now isolated in a narrow `try/catch (ArgumentException)`. Malformed code-group catalog filename syntax maps to:

```text
Status: Invalid
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

A syntactically valid filename outside the package continues to use the separate `WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_OUTSIDE_PACKAGE` contract.

**Complete-suite 1065/1065 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1064/1064 GREEN malformed category-catalog filename checkpoint

Contract commit: `f061fddb3aff140ecf40e41fe28ec538efef0c45`

Production fix commit: `a7e4add893b84b74347a6bd21e5c563a635b9c5c`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1064
Total:  1064
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system. A syntactically malformed `CategoryCatalogFileName` now returns stable `Invalid` instead of leaking `ArgumentException`.

Next malformed-path boundary: `CodeGroupCatalogFileName`.

## 2026-09-21 — malformed category-catalog filename path contract

Contract commit:
`f061fddb3aff140ecf40e41fe28ec538efef0c45`

Baseline: **1063/1063 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperInvalidCategoryCatalogFileNamePathContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameContainsNullCharacter_ReturnsInvalidBeforeProviderOrFilesystem`

With `CategoryCatalogFileName = "categories\0.en.json"`, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

The contract also requires no template-provider invocation and no workspace-root creation.

The focused contract confirmed the expected RED on Windows:

```text
System.ArgumentException: Null character in path.
```

The exception escaped from `Path.GetFullPath(...)` inside `IsPathInsideDirectory(...)` before filesystem mutation or template-provider invocation.

Production fix commit:
`a7e4add893b84b74347a6bd21e5c563a635b9c5c`

The category-catalog containment evaluation is now isolated in a narrow `try/catch (ArgumentException)`. Malformed category-catalog filename syntax maps to:

```text
Status: Invalid
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

A syntactically valid filename outside the package continues to use the separate `WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_OUTSIDE_PACKAGE` contract.

**Complete-suite 1064/1064 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1063/1063 GREEN malformed error-catalog filename checkpoint

Contract commit: `bfdfefdb9e84b0574ff4fe233c104df481191996`

Production fix commit: `e3ea09bcfa07a4e6bfed38d84025abec121dbf7f`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1063
Total:  1063
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system. A syntactically malformed `ErrorCatalogFileName` now returns stable `Invalid` instead of leaking `ArgumentException`.

Next malformed-path boundary: `CategoryCatalogFileName`.

## 2026-09-21 — malformed error-catalog filename path contract

Contract commit:
`bfdfefdb9e84b0574ff4fe233c104df481191996`

Baseline: **1062/1062 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperInvalidErrorCatalogFileNamePathContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenErrorCatalogFileNameContainsNullCharacter_ReturnsInvalidBeforeProviderOrFilesystem`

With `ErrorCatalogFileName = "errors\0.en.json"`, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

The contract also requires no template-provider invocation and no workspace-root creation.

The focused contract confirmed the expected RED on Windows:

```text
System.ArgumentException: Null character in path.
```

The exception escaped from `Path.GetFullPath(...)` inside `IsPathInsideDirectory(...)` before any filesystem mutation or template-provider invocation.

Production fix commit:
`e3ea09bcfa07a4e6bfed38d84025abec121dbf7f`

The error-catalog containment evaluation is now isolated in a narrow `try/catch (ArgumentException)`. Malformed error-catalog filename syntax maps to:

```text
Status: Invalid
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

A syntactically valid filename outside the package continues to use the separate `WIF_JSONS_ERROR_CATALOG_FILE_NAME_OUTSIDE_PACKAGE` contract.

**Complete-suite 1063/1063 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1062/1062 GREEN malformed package-directory checkpoint

Contract commit: `8c63d3aee47df25d809d72c559efab16f4a32b87`

Production fix commit: `0200ee5b600ccce8f9fa16551fd2b29a04f3658a`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1062
Total:  1062
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system. A syntactically malformed `PackageDirectoryName` now returns stable `Invalid` instead of leaking `ArgumentException`.

Next malformed-path boundary: `ErrorCatalogFileName`.

## 2026-09-21 — malformed package-directory-name path contract

Contract commit:
`8c63d3aee47df25d809d72c559efab16f4a32b87`

Baseline: **1061/1061 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperInvalidPackageDirectoryNamePathContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenPackageDirectoryNameContainsNullCharacter_ReturnsInvalidBeforeProviderOrFilesystem`

With `PackageDirectoryName = "When\0ItFails"`, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID
Message: The package directory name is invalid.
```

The contract also requires no template-provider invocation and no workspace-root creation.

The focused contract confirmed the expected RED on Windows:

```text
System.ArgumentException: Null character in path.
```

The exception escaped from `Path.GetFullPath(...)` inside `IsPathInsideDirectory(...)` before any filesystem mutation or template-provider invocation.

Production fix commit:
`0200ee5b600ccce8f9fa16551fd2b29a04f3658a`

The package containment evaluation is now isolated in a narrow `try/catch (ArgumentException)`. Malformed package-directory syntax maps to:

```text
Status: Invalid
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID
Message: The package directory name is invalid.
```

A syntactically valid package path outside the root continues to use the separate `WIF_JSONS_PACKAGE_DIRECTORY_NAME_OUTSIDE_ROOT` contract.

**Complete-suite 1062/1062 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1061/1061 GREEN malformed root-directory checkpoint

Contract commit: `3cff3ca67386f04afbc8ea65d9a5c8380086518c`

Production fix commit: `bbdbfe3ad7479593153ebda6ebab060c0793a7bb`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1061
Total:  1061
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system. A syntactically malformed `RootDirectory` now returns stable `Invalid` instead of leaking `ArgumentException`.

Next caller-controlled malformed-path boundary: `PackageDirectoryName`.

## 2026-09-21 — malformed root-directory path contract

Contract commit:
`3cff3ca67386f04afbc8ea65d9a5c8380086518c`

Baseline: **1060/1060 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperInvalidRootDirectoryPathContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenRootDirectoryContainsNullCharacter_ReturnsInvalidBeforeProviderOrFilesystem`

With a `RootDirectory` containing a null character, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ROOT_DIRECTORY_INVALID
Message: The JSON root directory path is invalid.
```

The contract also requires no template-provider invocation and no creation of the safe temporary parent directory. This aligns the bootstrap caller-configuration boundary with the existing loader/writer malformed-path behavior, while keeping bootstrap-specific error codes.

The focused contract confirmed the expected RED on Windows:

```text
System.ArgumentException: Null character in path.
```

The exception escaped from `Path.GetFullPath(...)` inside `IsPathInsideDirectory(...)` before any filesystem mutation or template-provider invocation.

Production fix commit:
`bbdbfe3ad7479593153ebda6ebab060c0793a7bb`

`JsonsBootstrapper` now validates the normalized `RootDirectory` with `Path.GetFullPath(...)` before containment evaluation and maps `ArgumentException` to:

```text
Status: Invalid
Code: WIF_JSONS_ROOT_DIRECTORY_INVALID
Message: The JSON root directory path is invalid.
```

No broader exception normalization was introduced.

**Complete-suite 1061/1061 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1060/1060 GREEN complete catalog-filename containment checkpoint

Contract commit: `f20a7041a37b9e0af2761d5817392076cd3fc242`

Production guard commit: `8182490e51f5d7e393e5efbcebed0d1014c4f439`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1060
Total:  1060
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system. All five caller-configured catalog filename fields now have locally verified null, whitespace, and package-containment guards before filesystem mutation and template-provider invocation.

Next audit target: syntactically malformed caller paths. `JsonCatalogDocumentLoader` and `JsonCatalogDocumentWriter` already normalize malformed paths to `Invalid`, while `JsonsBootstrapper` can currently allow `Path.GetFullPath(...)` argument failures to escape.

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

**Complete-suite 1060/1060 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

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
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~EnsureWorkspaceAsync_WhenCategoryCatalogFileNameEndsWithCurrentDirectorySegment_ReturnsInvalidBeforeProviderOrFilesystem"
dotnet test WhenItFails.Tests
```

Expected after the committed fix: **one focused GREEN** and **1106/1106 GREEN** for the complete suite. A zero-test filter match is not a valid checkpoint.

## Next recommended step

After **1106/1106 GREEN** is confirmed locally, record the checkpoint and continue the same semantic-directory audit with `CodeGroupCatalogFileName`. Preserve the incremental one-field-at-a-time workflow.