# Implementation status

Last updated: 2026-08-02

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **1,241 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

The complete `WhenItFails.Tests` core suite is user-verified green with **701 passed, 0 failed, 0 skipped** before the new intentionally red action-verification contract.

The runtime/public-API audit has verified defensive handling of provider failures, null tasks, null responses, null payloads, runtime-null issue collections and elements, cross-validation envelopes, successful-provider diagnostic aggregation, status normalization, and required inner members of `ErrorCatalogProviderPayload`.

## Verification status

Recently verified focused contracts include:

- `TemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- `CriticalTemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- `TemperatureSensorReadingInvalidCatalogTests`: **1 user-verified green**.
- `TemperatureReadingStaleCatalogTests`: **1 user-verified green**.
- `TemperatureRateOfChangeExceededCatalogTests`: **1 user-verified green**.
- `TemperatureSensorDisagreementCatalogTests`: **1 user-verified green**.
- `TemperatureBelowMinimumLimitCatalogTests`: **1 user-verified green**.
- `CriticalTemperatureBelowMinimumLimitCatalogTests`: **1 user-verified green**.
- `ThermalProtectionActionFailedCatalogTests`: **1 user-verified green**.
- `DefaultJsonsTemplateProviderTests.GetTemplateFiles_ShouldIncludeThermalCatalogAndRevisedOwnerRanges`: **1 user-verified green**.
- Complete catalog validation after adding `AFW_THM_0009`: **0 errors, 0 warnings, 0 information issues**.
- Complete `WhenItFails.Tests`: **701 user-verified green, 0 failed, 0 skipped** before the intentional red test.
- Complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 user-verified green, 0 failed, 0 skipped**.

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The first nine thermal definitions are complete and verified:

- `AFW_THM_0001` / `1000001` / `TEMPERATURELIMITEXCEEDED` / `Warning`;
- `AFW_THM_0002` / `1000002` / `CRITICALTEMPERATURELIMITEXCEEDED` / `Critical`;
- `AFW_THM_0003` / `1000003` / `TEMPERATURESENSORREADINGINVALID` / `Error`;
- `AFW_THM_0004` / `1000004` / `TEMPERATUREREADINGSTALE` / `Error`;
- `AFW_THM_0005` / `1000005` / `TEMPERATURERATEOFCHANGEEXCEEDED` / `Warning`;
- `AFW_THM_0006` / `1000006` / `TEMPERATURESENSORDISAGREEMENT` / `Warning`;
- `AFW_THM_0007` / `1000007` / `TEMPERATUREBELOWMINIMUMLIMIT` / `Warning`;
- `AFW_THM_0008` / `1000008` / `CRITICALTEMPERATUREBELOWMINIMUMLIMIT` / `Critical`;
- `AFW_THM_0009` / `1000009` / `THERMALPROTECTIONACTIONFAILED` / `Critical`.

A standalone TDD contract now defines the planned tenth thermal state:

- ID `AFW_THM_0010`;
- code `1000010`;
- name `THERMALPROTECTIONACTIONUNVERIFIED`;
- title `Thermal protection action unverified`;
- message `Thermal protection action {action} for {component} could not be verified while handling {condition}.`;
- default severity `Critical`;
- categories `THERMAL` and `GENERAL`;
- subcategories `PROTECTION_ACTION` and `VERIFICATION`;
- tags `THERMAL`, `FAIL_SAFE`, `VERIFICATION_REQUIRED`, `OPERATOR_ACTION_REQUIRED`, and `USER_VISIBLE`;
- documentation key `when-it-fails/errors/thermal/thermal-protection-action-unverified`.

This contract represents an indeterminate outcome after a protection command was issued or initiated but completion could not be verified. It must remain distinct from `THERMALPROTECTIONACTIONFAILED`, which requires evidence of failure. An unverified outcome must not be silently treated as success, confirmed failure, or authorization to restart.

The production catalog has not yet been changed. The focused test is expected to be red because `THERMALPROTECTIONACTIONUNVERIFIED` does not exist yet.

Because bootstrap templates are generated from embedded authoritative catalogs, completed thermal definitions flow into newly initialized workspaces after rebuild. Existing project-local catalogs remain untouched by bootstrap.

The authoritative owner catalog continues to use non-overlapping ranges:

- `AFW`: `0–1099999`;
- `APP`: `1100000–1999999`;
- `PLUGIN`: `2000000–2999999`;
- `USER`: `9000000–9999999`.

## Focused verification

Run the new action-verification TDD contract:

```bash
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ThermalProtectionActionUnverifiedCatalogTests
```

Expected result: **1 red test** reporting that catalog item `THERMALPROTECTIONACTIONUNVERIFIED` was not found. Do not add the production definition until this exact red gate is user-verified.

The complete core baseline before the intentional red test is:

```bash
dotnet test WhenItFails.Tests
```

Latest user-verified result: **701 passed, 0 failed, 0 skipped**.

The complete catalog workspace remains user-verified green:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Latest user-verified result: **0 errors, 0 warnings, and 0 information issues**.

The complete Setter suite remains available through:

```bash
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Before committing further catalog changes, also run these commands individually:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
git diff --check
```

## Documentation synchronization completed

Maintained English documentation includes:

- `README.md` and `Readme/en.md`;
- `Docs/Overview/en.md`;
- `Docs/Commands/en.md`;
- `Docs/Known Limitations/en.md`;
- `Docs/Roadmap and Future Work/en.md`;
- `Docs/Getting-Started/en.md`;
- `Docs/FAQ/en.md`;
- `Docs/Testing and CI/en.md`;
- `Docs/Reviewing Catalog Changes/en.md`;
- `WhenItFails/Docs/Bootstrap/en.md`;
- `WhenItFails/Docs/Thermal Errors/en.md`.

`WhenItFails/Docs/Thermal Errors/en.md` documents all nine completed thermal contracts. Add the unverified-action contract only after its focused contract and workspace validation are green.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, automatic translation generation, remote catalog synchronization, package publishing, a GUI, complete source-code dependency discovery, or automatic runtime behavior for humorous message variants.

Thermal easter eggs are explicitly deferred. Any future alternative wording must never alter the structured contract, severity, metadata, thresholds, control flow, shutdown decision, restart policy, sensor trust decision, data-freshness decision, thermal-trend decision, redundancy decision, low-temperature decision, critical low-temperature decision, protection-action decision, action-verification decision, or fail-safe policy.

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

Run `ThermalProtectionActionUnverifiedCatalogTests` and confirm the exact missing-item failure. Do not change the production catalog until that expected red gate is user-verified.

## Last completed change

The `AFW_THM_0009` increment remains closed at **701/701 green core tests**. A standalone red contract now distinguishes an unverified protection-action outcome from confirmed action failure; no production catalog data has been changed.

Commits:

```text
f9eaeb99e8bd49d512e301456253c06ea6faa9e8
Record green thermal protection action core gate

7b8c651b64f2ae4fc0547a474b9f4ed892c4b8d0
Add thermal protection action unverified catalog contract
```
