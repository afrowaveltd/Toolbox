# Implementation status

Last updated: 2026-08-02

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The thermal catalog now contains twelve definitions through `AFW_THM_0012` / `THERMALFALLBACKPROTECTIONACTIONUNVERIFIED`. This contract represents an approved fallback action that was selected and initiated, but whose required result cannot be established from trustworthy evidence. It is neither confirmed success nor confirmed failure and deliberately does not carry the `FALLBACK_FAILED` tag.

The complete `WhenItFails.Tests` core suite is user-verified green with **704 passed, 0 failed, 0 skipped**.

The latest Setter rerun reported **1,240 passed, 1 failed, 0 skipped, 1,241 total**. The only failure was `ImplementationStatusDocumentationTests.Documentation_ProvidesCurrentContinuationPoint`, because this file no longer contained the required `## Verification status` continuation marker. The implementation and catalog behavior were not implicated.

## Verification status

The latest user-verified Setter test run:

```bash
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Result before this documentation repair:

- **1,240 passed**;
- **1 failed**;
- **0 skipped**;
- **1,241 total**;
- failure: required implementation-status marker `## Verification status` was missing.

Other current user-verified gates:

- complete `WhenItFails.Tests`: **704 passed, 0 failed, 0 skipped**;
- focused `ThermalFallbackProtectionActionUnverifiedCatalogTests`: **1 passed, 0 failed, 0 skipped**;
- expected TDD red checkpoint before adding `AFW_THM_0012`: **0 passed, 1 failed**, catalog item not found;
- catalog validation after `AFW_THM_0012`: user-verified green;
- documentation-key validation: user-verified green before the latest documentation synchronization;
- Markdown-link validation: user-verified green before the latest documentation synchronization;
- `git diff --check`: user-verified clean before the latest documentation synchronization.

The bundled reference catalog contains **17 categories**, **10 code groups**, **5 profiles**, and **37 bundled reference errors**. The project-local authoritative catalog additionally contains the twelve thermal definitions through `AFW_THM_0012`.

## Thermal catalog state

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The current thermal contracts are:

- `AFW_THM_0001` / `1000001` / `TEMPERATURELIMITEXCEEDED` / `Warning`;
- `AFW_THM_0002` / `1000002` / `CRITICALTEMPERATURELIMITEXCEEDED` / `Critical`;
- `AFW_THM_0003` / `1000003` / `TEMPERATURESENSORREADINGINVALID` / `Error`;
- `AFW_THM_0004` / `1000004` / `TEMPERATUREREADINGSTALE` / `Error`;
- `AFW_THM_0005` / `1000005` / `TEMPERATURERATEOFCHANGEEXCEEDED` / `Warning`;
- `AFW_THM_0006` / `1000006` / `TEMPERATURESENSORDISAGREEMENT` / `Warning`;
- `AFW_THM_0007` / `1000007` / `TEMPERATUREBELOWMINIMUMLIMIT` / `Warning`;
- `AFW_THM_0008` / `1000008` / `CRITICALTEMPERATUREBELOWMINIMUMLIMIT` / `Critical`;
- `AFW_THM_0009` / `1000009` / `THERMALPROTECTIONACTIONFAILED` / `Critical`;
- `AFW_THM_0010` / `1000010` / `THERMALPROTECTIONACTIONUNVERIFIED` / `Critical`;
- `AFW_THM_0011` / `1000011` / `THERMALFALLBACKPROTECTIONACTIONFAILED` / `Critical`;
- `AFW_THM_0012` / `1000012` / `THERMALFALLBACKPROTECTIONACTIONUNVERIFIED` / `Critical`.

`AFW_THM_0011` requires evidence that a distinct approved fallback was attempted and failed. `AFW_THM_0012` requires evidence that a distinct approved fallback was initiated, while its required result remains unverified. An unknown fallback outcome must not be collapsed into confirmed failure.

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

`WhenItFails/Docs/Thermal Errors/en.md` now documents `AFW_THM_0001` through `AFW_THM_0012`, including the distinction between confirmed fallback failure and an unverified fallback outcome, required runtime evidence, retry hazards, escalation boundaries, and restart requirements.

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

Pull this documentation-contract repair and rerun the complete Setter suite:

```bash
dotnet test Toolroom/WhenItFails/Setter.Tests
```

If the suite is green, rerun `check-doc-keys .`, `check-doc-links .`, `git diff --check`, and the complete core suite. Record the exact totals as the final `AFW_THM_0012` checkpoint.

After that clean checkpoint, continue the runtime/public-API audit rather than adding another thermal definition automatically. The next implementation slice should come from a demonstrated missing runtime contract, not from increasing the catalog count.

## Last completed change

`AFW_THM_0012` is implemented, documented, catalog-validated, and core-regression verified with **704 passed, 0 failed, 0 skipped**. The subsequent Setter regression exposed only a stale structure problem in this continuation document: the required `## Verification status` section was absent. This change restores the complete documentation contract and records the exact red checkpoint pending user verification.