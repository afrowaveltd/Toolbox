# Implementation status

Last updated: 2026-08-03

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The `AFW_THM_0012` slice is complete. The runtime/public-API audit directly protects bootstrap DTO defaults and assigned values, validation issue/result collection contracts, stable validation-severity numeric values, descriptor request and descriptor models, definition and catalog-document models, provider payloads, `ErrorCatalogContext`, `ErrorCatalogInitializationPayload`, and `CatalogProviderPipeline`.

The current user-verified complete regression baseline is fully green:

- complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 passed, 0 failed, 0 skipped**;
- complete `WhenItFails.Tests`: **762 passed, 0 failed, 0 skipped**.

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

Result: **762 passed, 0 failed, 0 skipped**.

Completed runtime/public-API focused checkpoints:

- `JsonsBootstrapPayloadContractTests`: **4 passed, 0 failed, 0 skipped**;
- `JsonsBootstrapValueContractTests`: **3 passed, 0 failed, 0 skipped**;
- `ErrorCatalogValidationIssueContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorCatalogValidationResultTests`: **10 passed, 0 failed, 0 skipped**;
- `ErrorCatalogValidationSeverityContractTests`: **3 passed, 0 failed, 0 skipped**;
- `ErrorDescriptorRequestContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorDescriptorContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorDescriptorOfTContractTests`: **3 passed, 0 failed, 0 skipped**;
- `ErrorDefinitionContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorCatalogDocumentContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorProfileDefinitionContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorProfileCatalogDocumentContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorCategoryDefinitionContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorCategoryCatalogDocumentContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorCodeGroupDefinitionContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorCodeGroupCatalogDocumentContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorOwnerDefinitionContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorOwnerCatalogDocumentContractTests`: **2 passed, 0 failed, 0 skipped**;
- provider-payload contract family: **10 passed, 0 failed, 0 skipped**;
- `ErrorCatalogContextContractTests`: **2 passed, 0 failed, 0 skipped**;
- `CatalogProviderPipelineTests`: **10 passed, 0 failed, 0 skipped**;
- `ErrorCatalogInitializationPayloadContractTests`: **5 passed, 0 failed, 0 skipped**.

The validation-result collection contract confirms that `AddIssue` preserves the exact supplied instance and that `Issues` is a live read-only view rather than a detached copy. The validation-severity contract protects `Information = 0`, `Warning = 1`, and `Error = 2` from accidental renumbering.

Other verified gates for the completed thermal slice:

- focused `ThermalFallbackProtectionActionUnverifiedCatalogTests`: **1 passed, 0 failed, 0 skipped**;
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

Add a focused public-contract test for the stable numeric values of `ErrorCatalogContextSource`: `ProjectCatalog = 0`, `PreviousContext = 1`, and `BuiltInDefaults = 2`.

Preserve the complete **762-test** core baseline until the next full regression run.

## Last completed change

`ErrorCatalogValidationSeverityContractTests` passed all **3 focused tests**. The public severity enum now has direct protection against accidental numeric renumbering.