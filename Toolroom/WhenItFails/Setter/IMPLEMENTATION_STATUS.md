# Implementation status

Last updated: 2026-08-21

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The `AFW_THM_0012` slice is complete. The runtime/public-API audit protects bootstrap DTOs, validation contracts, stable enum values, descriptor and definition models, provider payloads, catalog context and initialization payloads, `CatalogProviderPipeline`, `ErrorCatalog`, factories, resolvers, runtime services, provider composition, bootstrap initialization, read-only profile-resolution results, and malformed dependency-result boundaries. `ErrorCatalogCrossValidator` now converts a runtime-null main `Errors` collection into `CatalogErrorsCollectionIsNull` before any `.Count`, indexing, or index-building dereference, converts runtime-null supporting `Owners`, `CodeGroups`, `Categories`, and `Profiles` collections into `OwnerCatalogOwnersCollectionIsNull`, `CodeGroupCatalogCodeGroupsCollectionIsNull`, `CategoryCatalogCategoriesCollectionIsNull`, and `ProfileCatalogProfilesCollectionIsNull` before `.Count`, iteration, or index-building dereferences, converts runtime-null entries in all five document collections it consumes directly — `Errors`, `Owners`, `CodeGroups`, `Categories`, and `Profiles` — into stable validation errors instead of throwing `NullReferenceException`, converts a runtime-null `ErrorDefinition.Categories` collection into `ErrorCategoriesCollectionIsNull`, and now converts runtime-null `ErrorOwnerDefinition.Aliases` into `OwnerAliasesCollectionIsNull` while keeping the owner itself indexable by `Name`. The document-level collection null-boundary series in `ErrorCatalogCrossValidator` is complete. Its public `Validate(...)` API documentation is restored and the focused verification builds without the prior CS1591 warning. The standalone `ErrorCatalogValidator`, `ErrorProfileCatalogValidator`, `ErrorOwnerCatalogValidator`, `ErrorCodeGroupCatalogValidator`, and `ErrorCategoryCatalogValidator` likewise convert their audited runtime-null definitions and mutable collections into stable validation issues instead of dereferencing them.

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

Focused runtime/public-API checkpoints added after that complete core baseline include the user-verified cross-validator document-null and nested-null contracts, including:

- all five cross-validator null-definition contracts: **5 passed**, warning-free;
- `ErrorCatalogCrossValidatorNullErrorsCollectionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullOwnersCollectionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullCodeGroupsCollectionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullCategoriesCollectionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullProfilesCollectionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullErrorCategoriesCollectionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullOwnerAliasesCollectionContractTests`: **1 passed**.

Earlier verified runtime/public-API checkpoints cover bootstrap DTOs, validation result/severity contracts, context/runtime enums, descriptor and definition models, provider payloads, `CatalogProviderPipeline`, `ErrorCatalog` snapshots/lookups/indexes, factories, resolvers, runtime dependency-null boundaries, provider composition, bootstrap initialization, profile normalization, read-only profile-resolution results, standalone top-level null definitions, standalone document-level mutable collections, error `Categories`/`Subcategories`/`Tags`, category `Aliases`/`ParentCategories`/`DefaultTags`, owner `Aliases`, code-group `DefaultCategories`/`DefaultTags`, and profile include/exclude collections.

`ErrorCatalogCrossValidator` uses `CatalogErrorsCollectionIsNull` at `errors`, `OwnerCatalogOwnersCollectionIsNull` at `ownerCatalog.owners`, `CodeGroupCatalogCodeGroupsCollectionIsNull` at `codeGroupCatalog.codeGroups`, `CategoryCatalogCategoriesCollectionIsNull` at `categoryCatalog.categories`, `ProfileCatalogProfilesCollectionIsNull` at `profileCatalog.profiles`, `ErrorCategoriesCollectionIsNull` at `errors[index].categories`, and `OwnerAliasesCollectionIsNull` at `ownerCatalog.owners[index].aliases`; it also records stable null-definition diagnostics for all five directly consumed document collections. The standalone validators retain their corresponding stable contracts, including `CategoryAliasesCollectionIsNull`, `CategoryParentCategoriesCollectionIsNull`, `CategoryDefaultTagsCollectionIsNull`, `CodeGroupDefaultCategoriesCollectionIsNull`, `CodeGroupDefaultTagsCollectionIsNull`, and the profile include/exclude collection diagnostics.

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

Continue the runtime/public-API audit from the **872-test** green baseline by proving the next nested mutable collection failure path in `ErrorCatalogCrossValidator`. `BuildCategoryIndex` currently enumerates `ErrorCategoryDefinition.Aliases` directly, so a runtime-null aliases collection is the next concrete candidate even though `ErrorCategoryCatalogValidator` already protects the same malformed model. Add one red-first contract for that cross-validator path if no equivalent test exists.

Prefer one narrow contract with a clear public response shape.

## Last completed change

`ErrorCatalogCrossValidatorNullOwnerAliasesCollectionContractTests` is user-verified green: **1 passed**. A runtime-null `ErrorOwnerDefinition.Aliases` collection now becomes `OwnerAliasesCollectionIsNull` at `ownerCatalog.owners[index].aliases`; alias enumeration is skipped safely while the owner itself remains available in the cross-validator index by its primary `Name`.