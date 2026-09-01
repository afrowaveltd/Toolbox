# Implementation status

Last updated: 2026-09-01

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The `AFW_THM_0012` slice is complete. The runtime/public-API audit protects bootstrap DTOs, validation contracts, stable enum values, descriptor and definition models, provider payloads, catalog context and initialization payloads, `CatalogProviderPipeline`, `ErrorCatalog`, factories, resolvers, runtime services, provider composition, bootstrap initialization, read-only profile-resolution results, and malformed dependency-result boundaries.

`ErrorCatalogCrossValidator` now converts runtime-null document-level `Errors`, `Owners`, `CodeGroups`, `Categories`, and `Profiles` collections into stable validation issues before direct dereferences. It also converts null entries in those collections into stable null-definition issues. Nested cross-validator protection currently includes `ErrorDefinition.Categories`, `ErrorOwnerDefinition.Aliases`, `ErrorCategoryDefinition.Aliases`, `ErrorProfileDefinition.IncludeErrors`, `ErrorProfileDefinition.ExcludeErrors`, `ErrorProfileDefinition.IncludeOwners`, `ErrorProfileDefinition.IncludeCodeGroups`, and `ErrorProfileDefinition.IncludeCategories`. The owner/category remains indexable by its primary `Name` while malformed aliases are skipped, and malformed profile collections audited by the cross-validator no longer block independent profile checks. `ErrorProfileSelectionService` now converts both a runtime-null profile collection and null profile entries into stable Invalid responses instead of allowing LINQ/predicate dereferences to throw. `ErrorCatalogRuntime` now tolerates dependency responses whose mutable `Issues` collection was set to null in the explicit reset failure path, the built-in flexible-fallback failure path, context failure forwarding, recovery status recording, recovery metadata generation, warning-detail formatting, and initialization-failure metadata generation. Flexible previous-context recovery now remains operational for both hidden and visible recoverable failures even when initializer diagnostics are runtime-null, and fallback-failure metadata uses stable defaults rather than throwing when the project initializer diagnostics are absent. `ErrorDescriptorResolver` now treats a failed definition response whose public `Issues` collection is runtime-null the same as an empty diagnostics collection and falls back to `ErrorDefinitionResolveFailed` instead of throwing. `CatalogProviderPipeline` now does the same for failed loader responses, preserving the caller-supplied load failure code/message when loader diagnostics are runtime-null. `ErrorCatalogProvider` now also tolerates a failed `IErrorCatalogLoader` response whose `Issues` collection is runtime-null and falls back to `CatalogLoadFailed` instead of throwing. `ErrorCatalogInitializer` now treats failed bootstrapper or context-provider responses with runtime-null `Issues` as missing diagnostics and uses the caller-supplied stable fallback code/message instead of throwing. `BuiltInErrorCatalogContextProvider` now converts a runtime-null template collection from `IJsonsTemplateProvider` into stable Invalid response `WIF_BUILT_IN_TEMPLATES_NULL` before any collection dereference or context-provider call. `JsonsBootstrapper` now rejects runtime-null template content, runtime-null target file names, and empty/whitespace target file names with stable Invalid responses before filesystem handling. It also prevents template target paths from resolving outside the intended package directory; the existing production guard is now directly protected by a focused contract. `ErrorCatalogContextProvider` now rejects runtime-null and empty/whitespace values for all five public `JsonsOptions` catalog file-name properties before evaluating derived paths or invoking any catalog provider. It also rejects runtime-null and empty/whitespace `RootDirectory` and `PackageDirectoryName` values before any derived path getter or catalog provider is reached. The standalone validators retain their already verified collection-null contracts for errors, owners, code groups, categories, profiles, and their audited nested mutable collections.

The current user-verified complete regression baselines are fully green:

- complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 passed, 0 failed, 0 skipped**;
- complete `WhenItFails.Tests`: **872 passed, 0 failed, 0 skipped**.

## Verification status

The latest user-verified Setter test run:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Result: **1,241 passed, 0 failed, 0 skipped**.

The latest complete core test run:

```powershell
dotnet test WhenItFails.Tests
```

Result: **872 passed, 0 failed, 0 skipped**.

Focused runtime/public-API checkpoints added after that complete core baseline include:

- all five cross-validator null-definition contracts: **5 passed**, warning-free;
- `ErrorCatalogCrossValidatorNullErrorsCollectionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullOwnersCollectionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullCodeGroupsCollectionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullCategoriesCollectionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullProfilesCollectionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullErrorCategoriesCollectionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullOwnerAliasesCollectionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullCategoryAliasesCollectionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullProfileIncludeErrorsCollectionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullProfileExcludeErrorsCollectionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullProfileIncludeOwnersCollectionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullProfileIncludeCodeGroupsCollectionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullProfileIncludeCategoriesCollectionContractTests`: **1 passed**;
- `ErrorProfileSelectionServiceNullProfilesCollectionContractTests`: **1 passed**;
- `ErrorProfileSelectionServiceNullProfileDefinitionContractTests`: **1 passed**;
- `ErrorCatalogRuntimeNullResetIssuesCollectionContractTests`: **1 passed**;
- `ErrorCatalogRuntimeNullFallbackIssuesCollectionContractTests`: **1 passed**;
- `ErrorCatalogRuntimeNullContextIssuesCollectionContractTests`: **1 passed**;
- `ErrorCatalogRuntimeNullRecoveryStatusIssuesCollectionContractTests`: **1 passed**;
- `ErrorCatalogRuntimeNullRecoveryDetailsIssuesCollectionContractTests`: **1 passed**;
- `ErrorCatalogRuntimeNullInitializationFailureIssuesCollectionContractTests`: **1 passed**;
- `ErrorDescriptorResolverNullIssuesCollectionContractTests`: **1 passed**;
- `CatalogProviderPipelineNullIssuesCollectionContractTests`: **1 passed**;
- `ErrorCatalogProviderNullIssuesCollectionContractTests`: **1 passed**;
- `ErrorCatalogInitializerNullIssuesCollectionContractTests`: **1 passed**;
- `BuiltInErrorCatalogContextProviderNullTemplatesCollectionContractTests`: **1 passed**;
- `JsonsBootstrapperNullTemplateContentContractTests`: **1 passed**;
- `JsonsBootstrapperNullTemplateTargetFileNameContractTests`: **1 passed**;
- `JsonsBootstrapperWhitespaceTemplateTargetFileNameContractTests`: **1 passed**;
- `JsonsBootstrapperEscapingTemplateTargetFileNameContractTests`: **1 passed**;
- `ErrorCatalogContextProviderNullErrorCatalogFileNameContractTests`: **1 passed**;
- `ErrorCatalogContextProviderNullSupportingCatalogFileNameContractTests`: **4 passed**;
- `ErrorCatalogContextProviderWhitespaceCatalogFileNameContractTests`: **5 passed**;
- `ErrorCatalogContextProviderNullWorkspacePathOptionsContractTests`: **2 passed**;
- `ErrorCatalogContextProviderWhitespaceWorkspacePathOptionsContractTests`: **2 passed**.

Earlier verified runtime/public-API checkpoints cover bootstrap DTOs, validation result/severity contracts, context/runtime enums, descriptor and definition models, provider payloads, `CatalogProviderPipeline`, `ErrorCatalog` snapshots/lookups/indexes, factories, resolvers, runtime dependency-null boundaries, provider composition, bootstrap initialization, profile normalization, read-only profile-resolution results, standalone top-level null definitions, standalone document-level mutable collections, error `Categories`/`Subcategories`/`Tags`, category `Aliases`/`ParentCategories`/`DefaultTags`, owner `Aliases`, code-group `DefaultCategories`/`DefaultTags`, and profile include/exclude collections.

`ErrorCatalogCrossValidator` uses `CatalogErrorsCollectionIsNull` at `errors`, `OwnerCatalogOwnersCollectionIsNull` at `ownerCatalog.owners`, `CodeGroupCatalogCodeGroupsCollectionIsNull` at `codeGroupCatalog.codeGroups`, `CategoryCatalogCategoriesCollectionIsNull` at `categoryCatalog.categories`, `ProfileCatalogProfilesCollectionIsNull` at `profileCatalog.profiles`, `ErrorCategoriesCollectionIsNull` at `errors[index].categories`, `OwnerAliasesCollectionIsNull` at `ownerCatalog.owners[index].aliases`, `CategoryAliasesCollectionIsNull` at `categoryCatalog.categories[index].aliases`, `ProfileIncludeErrorsCollectionIsNull` at `profiles[index].includeErrors`, `ProfileExcludeErrorsCollectionIsNull` at `profiles[index].excludeErrors`, `ProfileIncludeOwnersCollectionIsNull` at `profiles[index].includeOwners`, `ProfileIncludeCodeGroupsCollectionIsNull` at `profiles[index].includeCodeGroups`, and `ProfileIncludeCategoriesCollectionIsNull` at `profiles[index].includeCategories`. `ErrorProfileSelectionService` uses `ErrorProfileCatalogProfilesCollectionIsNull` for a runtime-null profile collection and `ErrorProfileDefinitionIsNull` for a null profile entry. `ErrorCatalogRuntime` treats null `Issues` collections in explicit reset and built-in fallback failure responses as absent diagnostics; `ForwardContextFailure` likewise falls back to `ErrorCatalogContextUnavailable` when a context-store failure has null `Issues`. During flexible recovery, `RecordStatus` and `AddRecoveryMetadata` use the stable fallback code `WIF_INITIALIZATION_FAILED` and fallback message `The requested error catalog initialization failed.` when initializer diagnostics are absent. `CreateRecoveryDetails` treats null `Issues` as no additional diagnostics and returns either the initializer message or `No additional initialization diagnostics were provided.`. `AddInitializationFailureMetadata` likewise uses `WIF_INITIALIZATION_FAILED` when a failed initializer response has null or empty diagnostics. `ErrorDescriptorResolver.GetFirstIssueCode` now treats null `Issues` as missing diagnostics and falls back to its caller-supplied stable code. `CatalogProviderPipeline.GetFirstIssueCode` follows the same rule for failed loader responses. `ErrorCatalogProvider.GetFirstIssueCode` now applies the same null-safe fallback for failed error-catalog loader responses. `ErrorCatalogInitializer.CreateFailedResponse` now applies the same rule to failed bootstrapper and context-provider responses. `BuiltInErrorCatalogContextProvider` now distinguishes a null template collection from an empty one: null returns `WIF_BUILT_IN_TEMPLATES_NULL`, while empty retains `WIF_BUILT_IN_TEMPLATES_EMPTY`. `JsonsBootstrapper` distinguishes runtime-null template content from valid empty content, runtime-null target file names from valid names, and empty/whitespace target names from valid names, rejecting malformed values before filesystem operations. Its `IsPathInsideDirectory` guard rejects `..` traversal and other paths that resolve outside the package directory with `WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_OUTSIDE_PACKAGE`. `ErrorCatalogContextProvider` now treats null values for `ErrorCatalogFileName`, `CategoryCatalogFileName`, `CodeGroupCatalogFileName`, `OwnerCatalogFileName`, and `ProfilesFileName` as invalid configuration before any provider invocation; empty/whitespace values for the same properties are likewise rejected with stable `*_EMPTY` Invalid responses before any provider invocation. Runtime-null and empty/whitespace `RootDirectory` and `PackageDirectoryName` are rejected with stable `WIF_JSONS_ROOT_DIRECTORY_NULL`, `WIF_JSONS_ROOT_DIRECTORY_EMPTY`, `WIF_JSONS_PACKAGE_DIRECTORY_NAME_NULL`, and `WIF_JSONS_PACKAGE_DIRECTORY_NAME_EMPTY` Invalid responses before evaluating `PackageDirectoryPath`.

Numeric compatibility is directly protected for validation severity, catalog context source, and runtime state. `ErrorCatalogInitializationMode` remains protected by the existing numeric-value theory in `WhenItFailsOptionsTests`.

Other verified gates for the completed thermal slice:

- focused `ThermalFallbackProtectionActionUnverifiedCatalogTests`: **1 passed**;
- catalog validation after `AFW_THM_0012`: user-verified green;
- documentation-key validation: user-verified green;
- Markdown-link validation: user-verified green;
- `git diff --check`: user-verified clean.

The bundled reference catalog contains **17 categories**, **10 code groups**, **5 profiles**, and **37 bundled reference errors**. The project-local authoritative catalog additionally contains twelve thermal definitions through `AFW_THM_0012`.

## Thermal catalog state

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The current thermal contracts run from `AFW_THM_0001` through `AFW_THM_0012`. `AFW_THM_0011` represents confirmed failure of a distinct approved fallback. `AFW_THM_0012` represents a distinct approved fallback that was initiated but whose required result remains unverified.

## Documentation synchronization completed

The English documentation remains synchronized in the project root and topic-based `Docs` folders. Only `en.md` localized documents are maintained manually at this stage.

Current synchronized documentation includes:

- `README.md` and `Readme/en.md`;
- `Docs/Overview/en.md`;
- `Docs/Commands/en.md`;
- `Docs/Known Limitations/en.md`;
- `Docs/Roadmap and Future Work/en.md`;
- `Docs/Getting-Started/en.md`;
- `Docs/FAQ/en.md`;
- `Docs/Testing and CI/en.md`;
- `Docs/Reviewing Catalog Changes/en.md`;
- `Docs/Catalog Author Checklist/en.md`;
- `WhenItFails/Docs/Bootstrap/en.md`;
- `WhenItFails/Docs/Thermal Errors/en.md`.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, automatic translation generation, remote catalog synchronization, package publishing, a GUI, complete source-code dependency discovery, or automatic runtime behavior for humorous message variants.

## Working rules

- GitHub `master` is the source of truth.
- Keep each change narrow and intentional.
- Add or update tests immediately.
- Do not advance while the current slice is red.
- Update this file after every change.
- Prefer one file plus its directly related test, then commit.
- Match command examples to the user's current PowerShell environment.
- Use the user's `to-clipboard` helper whenever a long local output or file content is needed.

## Recommended next step

Continue the runtime/public-API audit from the **872-test** green baseline beyond the now-protected `JsonsOptions` null/whitespace path boundaries. Avoid mechanically adding more shape variants unless a concrete path redirect or exception is proven. Inspect adjacent composition/runtime helpers for malformed dependency values or mutable response data that can still escape as exceptions or unstable public responses, and add one narrow red-first contract for the next proven gap.

Prefer one narrow contract with a clear public response shape.

## Last completed change

`ErrorCatalogContextProviderWhitespaceWorkspacePathOptionsContractTests` is user-verified green: **2 passed**. Empty/whitespace `JsonsOptions.RootDirectory` and `JsonsOptions.PackageDirectoryName` values are now rejected during `ErrorCatalogContextProvider` preflight validation before any provider invocation or derived path evaluation. Stable Invalid responses are `WIF_JSONS_ROOT_DIRECTORY_EMPTY` / `The JSON root directory cannot be empty.` and `WIF_JSONS_PACKAGE_DIRECTORY_NAME_EMPTY` / `The package directory name cannot be empty.` Production commit: `38076666a32f490f1c457175153c33c80d4d73ff`. Red-first test commit: `8fee1889d5494411e90a3152da194f0d46435a6a`.
