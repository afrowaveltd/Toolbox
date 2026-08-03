# Implementation status

Last updated: 2026-08-03

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The `AFW_THM_0012` slice is complete. The runtime/public-API audit now directly protects the bootstrap payload DTOs, bootstrap DTO assigned-value behavior, `ErrorCatalogValidationIssue`, `ErrorDescriptorRequest`, base `ErrorDescriptor`, generic `ErrorDescriptor<TAttachment>`, the definition and catalog-document model families, the full provider-payload DTO family, the complete `ErrorCatalogContext` runtime snapshot, `ErrorCatalogInitializationPayload`, and the complete current contract of `CatalogProviderPipeline`.

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
- `ErrorCategoryCatalogProviderPayloadContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorCodeGroupCatalogProviderPayloadContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorOwnerCatalogProviderPayloadContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorProfileCatalogProviderPayloadContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorCatalogProviderPayloadContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorCatalogContextContractTests`: **2 passed, 0 failed, 0 skipped**;
- `CatalogProviderPipelineTests`: **10 passed, 0 failed, 0 skipped**;
- `ErrorCatalogInitializationPayloadContractTests`: **5 passed, 0 failed, 0 skipped**.

The bootstrap default contracts verify safe empty defaults and a live file-result collection. The bootstrap value contracts verify exact preservation of assigned template-file, file-result, and payload path/state values. The validation-issue contract verifies safe defaults and exact preservation of severity, code, message, related error identity, and property path. The focused definition contracts verify safe scalar defaults and independently allocated mutable containers. The catalog-document contracts verify the `1.0` schema and `en` language defaults plus independent tag, metadata, and contained-definition collections. The provider payload contracts verify null defaults for required reference properties and exact preservation of assigned instances. The context contract verifies all seven required references of the atomically published runtime snapshot. The initialization payload contract verifies project-catalog defaults, exact reference preservation, and the complete `IsDegraded` truth table. The pipeline tests cover successful execution, all current failure and short-circuit paths, cancellation, configured loader fallbacks, and required delegate null guards.

Other verified gates for the completed thermal slice:

- focused `ThermalFallbackProtectionActionUnverifiedCatalogTests`: **1 passed, 0 failed, 0 skipped**;
- catalog validation after `AFW_THM_0012`: user-verified green;
- documentation-key validation: user-verified green;
- Markdown-link validation: user-verified green;
- `git diff --check`: user-verified clean.

The bundled reference catalog contains **17 categories**, **10 code groups**, **5 profiles**, and **37 bundled reference errors**. The project-local authoritative catalog additionally contains twelve thermal definitions through `AFW_THM_0012`.

## Thermal catalog state

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The current thermal contracts run from `AFW_THM_0001` through `AFW_THM_0012`. `AFW_THM_0011` represents confirmed failure of a distinct approved fallback. `AFW_THM_0012` represents a distinct approved fallback that was initiated but whose required result remains unverified. An unknown fallback outcome must not be collapsed into confirmed failure.

## Documentation synchronization completed

The English documentation is maintained in the project root and localized topic folders. Current synchronized documentation includes:

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

`WhenItFails/Docs/Thermal Errors/en.md` documents `AFW_THM_0001` through `AFW_THM_0012`.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, automatic translation generation, remote catalog synchronization, package publishing, a GUI, complete source-code dependency discovery, or automatic runtime behavior for humorous message variants.

Thermal easter eggs are explicitly deferred. Alternative wording must never alter the structured contract, severity, metadata, thresholds, control flow, shutdown decision, restart policy, sensor-trust decision, data-freshness decision, thermal-trend decision, redundancy decision, protection-action decision, action-verification decision, fallback-action decision, fallback-verification decision, or fail-safe policy.

## Working rules

- GitHub `master` is the source of truth.
- Keep each change narrow and intentional.
- Add or update tests immediately.
- Do not advance while the current slice is red.
- Update this file after every change.
- Prefer one file plus its directly related test, then commit.
- Prefer one-line shell commands and avoid line-continuation characters where practical.
- Match command examples to the user's current shell; the current working shell is PowerShell on Windows.
- Use the user's `to-clipboard` helper whenever a long local output or file content is needed.

## Recommended next step

Extend `ErrorCatalogValidationResultTests` with focused contract coverage for exact issue-instance preservation and the live read-only `Issues` view.

Do not duplicate the already verified severity, validity, or null-guard behavior. Preserve the complete **762-test** core baseline until the next full regression run.

## Last completed change

`ErrorCatalogValidationIssueContractTests` passed all **2 focused tests**. Validation issues now have direct coverage for safe defaults and exact preservation of all assigned public values.