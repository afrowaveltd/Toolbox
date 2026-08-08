# Implementation status

Last updated: 2026-08-08

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The `AFW_THM_0012` slice is complete. The runtime/public-API audit protects bootstrap DTOs, validation contracts, stable enum values, descriptor and definition models, provider payloads, catalog context and initialization payloads, `CatalogProviderPipeline`, `ErrorCatalog`, factories, resolvers, runtime services, provider composition, bootstrap initialization, read-only profile-resolution results, and malformed dependency-result boundaries. `ErrorCatalogCrossValidator` now converts runtime-null entries in all five document collections it consumes directly — `Errors`, `Owners`, `CodeGroups`, `Categories`, and `Profiles` — into stable validation errors instead of throwing `NullReferenceException`. Its public `Validate(...)` API documentation is restored and the focused verification builds without the prior CS1591 warning. The standalone `ErrorProfileCatalogValidator`, `ErrorOwnerCatalogValidator`, and `ErrorCodeGroupCatalogValidator` now likewise convert runtime-null definitions into stable validation issues instead of dereferencing them.

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

- `ErrorCatalogCrossValidatorNullErrorDefinitionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullOwnerDefinitionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullCodeGroupDefinitionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullCategoryDefinitionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullProfileDefinitionContractTests`: **1 passed**;
- all five cross-validator null-definition contracts re-verified together: **5 passed**, warning-free;
- `ErrorProfileCatalogValidatorNullProfileDefinitionContractTests`: **1 passed**;
- `ErrorOwnerCatalogValidatorNullOwnerDefinitionContractTests`: **1 passed**;
- `ErrorCodeGroupCatalogValidatorNullCodeGroupDefinitionContractTests`: **1 passed**.

Earlier verified runtime/public-API checkpoints cover bootstrap DTOs, validation result/severity contracts, context/runtime enums, descriptor and definition models, provider payloads, `CatalogProviderPipeline`, `ErrorCatalog` snapshots/lookups/indexes, factories, resolvers, runtime dependency-null boundaries, provider composition, bootstrap initialization, profile normalization, and read-only profile-resolution results.

`ErrorCatalogCrossValidator` records `ErrorDefinitionIsNull` at `errors[index]`, `OwnerDefinitionIsNull` at `ownerCatalog.owners[index]`, `CodeGroupDefinitionIsNull` at `codeGroupCatalog.codeGroups[index]`, `CategoryDefinitionIsNull` at `categoryCatalog.categories[index]`, and `ProfileDefinitionIsNull` at `profileCatalog.profiles[index]`. `ErrorProfileCatalogValidator` uses `ProfileDefinitionIsNull` at `profiles[index]`. `ErrorOwnerCatalogValidator` uses `OwnerDefinitionIsNull` at `owners[index]` and safely skips null owners in normalized-name and range-overlap processing. `ErrorCodeGroupCatalogValidator` uses `CodeGroupDefinitionIsNull` at `codeGroups[index]` and safely skips null code groups in range-overlap processing.

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

Continue the runtime/public-API audit from the **872-test** green baseline by inspecting the remaining standalone catalog validators for malformed collection-item boundaries. Next inspect `ErrorCategoryCatalogValidator`; add a red-first contract only if a runtime-null item can still escape as an exception.

Prefer one narrow contract with a clear public response shape.

## Last completed change

`ErrorCodeGroupCatalogValidatorNullCodeGroupDefinitionContractTests` is user-verified green: **1 passed**. A runtime-null item in `ErrorCodeGroupCatalogDocument.CodeGroups` now becomes `CodeGroupDefinitionIsNull` at `codeGroups[index]` instead of throwing `NullReferenceException`, including safe handling in range-overlap processing.