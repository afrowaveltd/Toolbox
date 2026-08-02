# Implementation status

Last updated: 2026-08-02

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **1,241 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

The complete `WhenItFails.Tests` core suite is user-verified green with **702 passed, 0 failed, 0 skipped** before the `AFW_THM_0011` catalog test was added. The next complete run should contain **703 tests**.

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
- `ThermalProtectionActionUnverifiedCatalogTests`: **1 user-verified green**.
- `ThermalFallbackProtectionActionFailedCatalogTests`: **1 user-verified green**.
- `DefaultJsonsTemplateProviderTests.GetTemplateFiles_ShouldIncludeThermalCatalogAndRevisedOwnerRanges`: **1 user-verified green**.
- Complete catalog validation after adding `AFW_THM_0011`: **0 errors, 0 warnings, 0 information issues**.
- Complete `WhenItFails.Tests`: **702 user-verified green, 0 failed, 0 skipped** before the latest test addition.
- Complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 user-verified green, 0 failed, 0 skipped**.

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The first eleven thermal definitions are present and focused-validation verified:

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
- `AFW_THM_0011` / `1000011` / `THERMALFALLBACKPROTECTIONACTIONFAILED` / `Critical`.

`THERMALFALLBACKPROTECTIONACTIONFAILED` uses message `Thermal fallback protection action {fallbackAction} failed for {component} after {primaryAction} while handling {condition}.`, categories `THERMAL` and `GENERAL`, subcategories `FALLBACK_ACTION` and `FAIL_SAFE`, tags `THERMAL`, `FAIL_SAFE`, `FALLBACK_FAILED`, `OPERATOR_ACTION_REQUIRED`, and `USER_VISIBLE`, and documentation key `when-it-fails/errors/thermal/thermal-fallback-protection-action-failed`.

This contract represents confirmed failure of an approved fallback response after the primary thermal protection action failed or remained unverified. It remains separate from the triggering thermal condition and the primary action result. It must not be emitted merely because a fallback exists or was considered; runtime evidence must show that the selected fallback was attempted and failed.

Because bootstrap templates are generated from embedded authoritative catalogs, all eleven thermal definitions flow into newly initialized workspaces after rebuild. Existing project-local catalogs remain untouched by bootstrap.

The authoritative owner catalog continues to use non-overlapping ranges:

- `AFW`: `0–1099999`;
- `APP`: `1100000–1999999`;
- `PLUGIN`: `2000000–2999999`;
- `USER`: `9000000–9999999`.

## Focused verification

The fallback-action contract is user-verified green:

```bash
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ThermalFallbackProtectionActionFailedCatalogTests
```

Latest user-verified result: **1 passed, 0 failed, 0 skipped**.

The complete catalog workspace is user-verified green after adding `AFW_THM_0011`:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Latest user-verified result: **0 errors, 0 warnings, and 0 information issues**.

After the thermal documentation update, run the complete core project:

```bash
dotnet test WhenItFails.Tests
```

Expected next full result: **703 passed, 0 failed, 0 skipped**.

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

`WhenItFails/Docs/Thermal Errors/en.md` now documents all eleven thermal contracts. The latest section requires evidence that an approved fallback was actually selected, attempted, and failed; preserves the triggering condition and primary-action result; records structured fallback evidence; warns against unsafe retries; and leaves remaining options, escalation, and restart authorization to application policy.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, automatic translation generation, remote catalog synchronization, package publishing, a GUI, complete source-code dependency discovery, or automatic runtime behavior for humorous message variants.

Thermal easter eggs are explicitly deferred. Any future alternative wording must never alter the structured contract, severity, metadata, thresholds, control flow, shutdown decision, restart policy, sensor trust decision, data-freshness decision, thermal-trend decision, redundancy decision, low-temperature decision, critical low-temperature decision, protection-action decision, action-verification decision, fallback-action decision, or fail-safe policy.

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

Run the complete `WhenItFails.Tests` project. Do not add another thermal definition until the expected **703-test** core gate is user-verified green.

## Last completed change

The English thermal documentation now includes `THERMALFALLBACKPROTECTIONACTIONFAILED` and preserves the distinction between the triggering thermal condition, primary-action outcome, and confirmed fallback failure. The remaining gate is the complete core regression suite.

Commits:

```text
fa982187d95e1cebc0993ef0facb336e3667d9fa
Add thermal fallback protection action failed catalog contract

b661d0fc2d93514a07548337cb4b021108ec0114
Add thermal fallback protection action failure catalog error

9600137d357877a2ac37301ccf52158581a42639
Document thermal fallback protection action failure
```
