# Implementation status

Last updated: 2026-08-02

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The `AFW_THM_0012` slice is complete. The runtime/public-API audit now directly protects the bootstrap payload DTOs, `ErrorDescriptorRequest`, the full provider-payload DTO family, the complete `ErrorCatalogContext` runtime snapshot, and the successful, loader-failure, and null-document orchestration paths of `CatalogProviderPipeline`.

The current user-verified complete regression baseline is fully green:

- complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 passed, 0 failed, 0 skipped**;
- complete `WhenItFails.Tests`: **720 passed, 0 failed, 0 skipped**.

## Verification status

The latest user-verified Setter test run:

```bash
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Result: **1,241 passed, 0 failed, 0 skipped**.

The latest complete core test run:

```bash
dotnet test WhenItFails.Tests
```

Result: **720 passed, 0 failed, 0 skipped**.

Completed runtime/public-API focused checkpoints:

- `JsonsBootstrapPayloadContractTests`: **4 passed, 0 failed, 0 skipped**;
- `ErrorDescriptorRequestContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorCategoryCatalogProviderPayloadContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorCodeGroupCatalogProviderPayloadContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorOwnerCatalogProviderPayloadContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorProfileCatalogProviderPayloadContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorCatalogProviderPayloadContractTests`: **2 passed, 0 failed, 0 skipped**;
- `ErrorCatalogContextContractTests`: **2 passed, 0 failed, 0 skipped**;
- `CatalogProviderPipelineTests`: **3 passed, 0 failed, 0 skipped**.

The provider payload contracts verify null defaults for required reference properties and exact preservation of assigned instances. The main payload additionally verifies the `Catalog` reference by using a real empty `ErrorCatalog` instance. The context contract verifies all seven required references of the atomically published runtime snapshot. The pipeline tests verify exact `load → normalize → validate → create payload` ordering, loader-failure propagation, and rejection of a successful load carrying a null document before any later stage runs.

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
- Match command examples to the user's current shell; the current working shell is Bash on Linux.
- Use the user's `to-clipboard` helper whenever a long local output or file content is needed.

## Recommended next step

Add the validation-failure contract to `CatalogProviderPipeline`.

Verify that loading and normalization complete, validation returns an error, payload creation is skipped, and the configured validation failure code and message are returned. Do not change production code unless the focused test exposes an actual defect.

After this focused test passes, add cancellation coverage and then run the complete core suite.

## Last completed change

`CatalogProviderPipelineTests` passed **3 focused tests**. The shared provider orchestration now has direct coverage for the successful flow, loader-failure propagation, and successful-load/null-document rejection. The latest complete core baseline remains **720 passed, 0 failed, 0 skipped** pending the next full regression run.