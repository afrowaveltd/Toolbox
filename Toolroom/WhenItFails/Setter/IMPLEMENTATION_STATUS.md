# Implementation status

Last updated: 2026-08-02

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The `AFW_THM_0012` slice is complete. `THERMALFALLBACKPROTECTIONACTIONUNVERIFIED` represents an approved fallback action that was selected and initiated, but whose required result cannot be established from trustworthy evidence. It is neither confirmed success nor confirmed failure and deliberately does not carry the `FALLBACK_FAILED` tag.

The runtime/public-API audit has now started with the bootstrap payload surface. A focused contract test protects the default public behavior of `JsonsBootstrapPayload`, `JsonsBootstrapFileResult`, and `JsonsTemplateFile`.

The latest fully verified regression baseline remains:

- complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 passed, 0 failed, 0 skipped**;
- complete `WhenItFails.Tests`: **704 passed, 0 failed, 0 skipped**.

## Verification status

The latest user-verified Setter test run:

```bash
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Result:

- **1,241 passed**;
- **0 failed**;
- **0 skipped**;
- **1,241 total**.

The last complete core test run:

```bash
dotnet test WhenItFails.Tests
```

Result:

- **704 passed**;
- **0 failed**;
- **0 skipped**.

The first runtime/public-API audit slice was verified with:

```bash
dotnet test WhenItFails.Tests --filter FullyQualifiedName~JsonsBootstrapPayloadContractTests
```

Result:

- **4 passed**;
- **0 failed**;
- **0 skipped**.

The focused bootstrap payload contract verifies:

- new `JsonsBootstrapPayload` instances expose empty string paths and false directory-state flags;
- `Files` is non-null, initially empty, mutable, and returns the same collection instance;
- new `JsonsBootstrapFileResult` instances expose safe empty/default values;
- new `JsonsTemplateFile` instances expose safe empty/default values.

Other verified gates for the completed thermal slice:

- focused `ThermalFallbackProtectionActionUnverifiedCatalogTests`: **1 passed, 0 failed, 0 skipped**;
- catalog validation after `AFW_THM_0012`: user-verified green;
- documentation-key validation: user-verified green;
- Markdown-link validation: user-verified green;
- `git diff --check`: user-verified clean.

The bundled reference catalog contains **17 categories**, **10 code groups**, **5 profiles**, and **37 bundled reference errors**. The project-local authoritative catalog additionally contains twelve thermal definitions through `AFW_THM_0012`.

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

`WhenItFails/Docs/Thermal Errors/en.md` documents `AFW_THM_0001` through `AFW_THM_0012`, including the distinction between confirmed fallback failure and an unverified fallback outcome, required runtime evidence, retry hazards, escalation boundaries, and restart requirements.

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

Run the complete core suite after the new bootstrap payload contract:

```bash
dotnet test WhenItFails.Tests
```

If it remains green, record the new exact total and continue the runtime/public-API audit with the next demonstrated unprotected public surface. Do not modify the bootstrap implementation unless a focused test exposes an actual contract defect.

## Last completed change

Added `WhenItFails.Tests/Bootstrap/JsonsBootstrapPayloadContractTests.cs` and verified all **4 focused tests** green. The tests establish the public default-value and collection-stability contracts for the bootstrap payload types without changing runtime implementation.