# Implementation status

Last updated: 2026-08-05

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The `AFW_THM_0012` slice is complete. The runtime/public-API audit now protects bootstrap DTOs, validation contracts, stable enum values, descriptor and definition models, provider payloads, catalog context and initialization payloads, `CatalogProviderPipeline`, `ErrorCatalog`, `ErrorCatalogFactory`, and `ErrorDefinitionResolver` input and response contracts.

The current user-verified complete regression baselines are fully green:

- complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 passed, 0 failed, 0 skipped**;
- complete `WhenItFails.Tests`: **780 passed, 0 failed, 0 skipped**.

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

Result: **780 passed, 0 failed, 0 skipped**.

Completed runtime/public-API focused checkpoints include:

- bootstrap value and payload contracts: **7 passed**;
- validation issue, result, and severity contracts: **15 passed**;
- catalog context source and runtime state contracts: **8 passed**;
- descriptor contract family: **7 passed**;
- definition and catalog-document contract family: **16 passed**;
- provider-payload contract family: **10 passed**;
- `ErrorCatalogContextContractTests`: **2 passed**;
- `CatalogProviderPipelineTests`: **10 passed**;
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
- `ErrorDefinitionResolverResponseShapeContractTests`: **3 passed**.

The catalog contracts protect source-sequence snapshots, ordering, exact object identity, read-only collection exposure, safe empty and unusable lookups, non-positive numeric indexing, deterministic duplicate-key handling, multi-value ordering and de-duplication, and consistent normalization. The factory creates a usable empty catalog and snapshots `document.Errors` at creation time. The resolver returns stable responses for invalid, not-found, and successful resolution paths, including exact data and issue shapes.

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

Add focused `ErrorDescriptorFactory` mapping and ownership contract coverage.

Verify scalar values map exactly, list values are copied into independent collections, and metadata retains its intentional shared-instance behavior. Preserve the complete **780-test** core baseline until the next full regression run.

## Last completed change

`ErrorDefinitionResolverResponseShapeContractTests` passed all **3 focused tests**. Invalid and not-found paths return null data with one stable issue, while successful resolution returns the exact catalog definition instance without issues.