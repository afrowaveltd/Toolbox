# Implementation status

Last updated: 2026-08-08

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The `AFW_THM_0012` slice is complete. The runtime/public-API audit protects bootstrap DTOs, validation contracts, stable enum values, descriptor and definition models, provider payloads, catalog context and initialization payloads, `CatalogProviderPipeline`, `ErrorCatalog`, factories, resolvers, runtime services, provider composition, bootstrap initialization, read-only profile-resolution results, and malformed dependency-result boundaries. `ErrorCatalogCrossValidator` now converts runtime-null entries in all five document collections it consumes directly — `Errors`, `Owners`, `CodeGroups`, `Categories`, and `Profiles` — into stable validation errors instead of throwing `NullReferenceException`. Its public `Validate(...)` API documentation is restored and the focused verification builds without the prior CS1591 warning.

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

Completed runtime/public-API focused checkpoints include:

- bootstrap value and payload contracts: **7 passed**;
- validation issue, result, and severity contracts: **15 passed**;
- catalog context source and runtime state contracts: **8 passed**;
- descriptor contract family: **7 passed**;
- definition and catalog-document contract family: **16 passed**;
- provider-payload contract family: **10 passed**;
- `ErrorCatalogContextContractTests`: **2 passed**;
- `CatalogProviderPipelineTests`: **10 passed**;
- `CatalogProviderPipelineNullResultContractTests`: **4 passed**;
- `ErrorCatalogInitializationPayloadContractTests`: **5 passed**;
- `ErrorCatalogSnapshotContractTests`: **4 passed**;
- `ErrorCatalogEmptyLookupContractTests`: **2 passed**;
- `ErrorCatalogNonPositiveCodeContractTests`: **2 passed**;
- `ErrorCatalogDuplicateSingleValueKeyContractTests`: **3 passed**;
- `ErrorCatalogMultiValueIndexContractTests`: **2 passed**;
- `ErrorCatalogMultiValueNormalizationContractTests`: **6 passed**;
- `ErrorCatalogUnusableSingleValueLookupContractTests`: **6 passed**;
- `ErrorCatalogFactoryContractTests`: **2 passed**;
- `ErrorDefinitionResolverInputBoundaryContractTests`: **5 passed**;
- `ErrorDefinitionResolverResponseShapeContractTests`: **3 passed**;
- `ErrorDescriptorFactoryContractTests`: **3 passed**;
- `ErrorDescriptorFactoryNullBoundaryContractTests`: **2 passed**;
- `ErrorDescriptorResolverFallbackContractTests`: **2 passed**;
- `ErrorDescriptorServiceNullResponseContractTests`: **3 passed**;
- `ErrorCatalogRuntimeNullContextPayloadContractTests`: **4 passed**;
- `ErrorCatalogRuntimeNullDownstreamResponseContractTests`: **4 passed**;
- `ErrorCatalogRuntimeNullInitializationDependencyResponseContractTests`: **2 passed**;
- `ErrorCatalogRuntimeNullContextStoreResponseContractTests`: **5 passed**;
- `ErrorCatalogRuntimeNullFlexibleFallbackResponseContractTests`: **1 passed**;
- `ErrorCatalogRuntimeNullInitializationPayloadContractTests`: **1 passed**;
- `ErrorCatalogRuntimeInvalidInitializationPayloadContractTests`: **2 passed**;
- `ErrorProfileSelectionServiceNullResolverResultContractTests`: **1 passed**;
- `ErrorCatalogContextProviderNullDependencyResponseContractTests`: **5 passed**;
- `ErrorCatalogProviderNullDependencyResultContractTests`: **4 passed**;
- `BuiltInErrorCatalogContextProviderNullContextResponseContractTests`: **1 passed**;
- `ErrorCatalogInitializerNullDependencyResponseContractTests`: **2 passed**;
- `JsonsBootstrapperNullTemplateResultContractTests`: **2 passed**;
- `ErrorProfileResolverNormalizationContractTests`: **8 passed**;
- `ErrorProfileResolverReadOnlyResultContractTests`: **1 passed**;
- `ErrorCatalogContextProviderNullResponseTaskResultTests`: **5 passed**;
- `ErrorCatalogCrossValidatorNullErrorDefinitionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullOwnerDefinitionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullCodeGroupDefinitionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullCategoryDefinitionContractTests`: **1 passed**;
- `ErrorCatalogCrossValidatorNullProfileDefinitionContractTests`: **1 passed**;
- all five cross-validator null-definition contracts re-verified together: **5 passed**, with the prior CS1591 warning removed.

The catalog contracts protect source-sequence snapshots, ordering, exact object identity, read-only collection exposure, safe empty and unusable lookups, non-positive numeric indexing, deterministic duplicate-key handling, multi-value ordering and de-duplication, and consistent normalization. Provider and runtime boundaries convert malformed dependency responses into stable `Invalid` responses rather than dereferencing null. `ErrorProfileResolver` returns a genuinely read-only collection while preserving source order and exact `ErrorDefinition` identities. `ErrorCatalogCrossValidator` records `ErrorDefinitionIsNull` at `errors[index]`, `OwnerDefinitionIsNull` at `ownerCatalog.owners[index]`, `CodeGroupDefinitionIsNull` at `codeGroupCatalog.codeGroups[index]`, `CategoryDefinitionIsNull` at `categoryCatalog.categories[index]`, and `ProfileDefinitionIsNull` at `profileCatalog.profiles[index]`, skipping those malformed entries during indexing or cross-validation.

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

Continue the runtime/public-API audit from the **872-test** green baseline, but move on from the completed cross-validator null-definition family. Prefer a genuinely different semantic boundary such as malformed nested collection values, validation status fidelity, or public collection ownership/read-only behavior; avoid multiplying structurally identical null permutations.

Prefer one narrow contract with a clear public response shape.

## Last completed change

All five `ErrorCatalogCrossValidator` null-definition contract tests are user-verified green together: **5 passed**. Runtime-null entries in `Errors`, `Owners`, `CodeGroups`, `Categories`, and `Profiles` now become stable validation issues instead of `NullReferenceException`, and the public `Validate(...)` XML documentation was restored so the focused build is warning-free.