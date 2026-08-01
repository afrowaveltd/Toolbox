# Implementation status

Last updated: 2026-08-02

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **1,241 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

The complete `WhenItFails.Tests` core suite is user-verified green with **698 passed, 0 failed, 0 skipped** before the new intentionally red minimum-temperature contract.

The runtime/public-API audit has verified defensive handling of provider failures, null tasks, null responses, null payloads, runtime-null issue collections and elements, cross-validation envelopes, successful-provider diagnostic aggregation, status normalization, and required inner members of `ErrorCatalogProviderPayload`.

## Verification status

Recently verified focused contracts include:

- `TemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- `CriticalTemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- `TemperatureSensorReadingInvalidCatalogTests`: **1 user-verified green**.
- `TemperatureReadingStaleCatalogTests`: **1 user-verified green**.
- `TemperatureRateOfChangeExceededCatalogTests`: **1 user-verified green**.
- `TemperatureSensorDisagreementCatalogTests`: **1 user-verified green**.
- `DefaultJsonsTemplateProviderTests.GetTemplateFiles_ShouldIncludeThermalCatalogAndRevisedOwnerRanges`: **1 user-verified green**.
- Complete catalog validation after adding `AFW_THM_0006`: **0 errors, 0 warnings, 0 information issues**.
- Complete `WhenItFails.Tests`: **698 user-verified green, 0 failed, 0 skipped** before the intentional red test.
- Complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 user-verified green, 0 failed, 0 skipped**.

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The first six thermal definitions are complete and verified:

- `AFW_THM_0001` / `1000001` / `TEMPERATURELIMITEXCEEDED` / `Warning`;
- `AFW_THM_0002` / `1000002` / `CRITICALTEMPERATURELIMITEXCEEDED` / `Critical`;
- `AFW_THM_0003` / `1000003` / `TEMPERATURESENSORREADINGINVALID` / `Error`;
- `AFW_THM_0004` / `1000004` / `TEMPERATUREREADINGSTALE` / `Error`;
- `AFW_THM_0005` / `1000005` / `TEMPERATURERATEOFCHANGEEXCEEDED` / `Warning`;
- `AFW_THM_0006` / `1000006` / `TEMPERATURESENSORDISAGREEMENT` / `Warning`.

A focused TDD contract has now been added for a seventh, semantically separate state:

- ID `AFW_THM_0007`;
- code `1000007`;
- name `TEMPERATUREBELOWMINIMUMLIMIT`;
- title `Temperature below minimum limit`;
- message `The reported temperature {temperature}{unit} is below the configured minimum operating limit of {limit}{unit}.`;
- default severity `Warning`;
- categories `THERMAL` and `VALIDATION`;
- subcategories `MINIMUM_LIMIT` and `TEMPERATURE`;
- tags `THERMAL`, `TEMPERATURE`, `LOW_TEMPERATURE`, and `USER_VISIBLE`;
- documentation key `when-it-fails/errors/thermal/temperature-below-minimum-limit`.

This contract represents a trusted, current reading below the configured minimum operating boundary. It must not be merged with upper-limit, invalid-reading, stale-reading, trend, or sensor-disagreement contracts.

The production catalog has not yet been changed. The focused test is expected to be red because `TEMPERATUREBELOWMINIMUMLIMIT` does not exist yet.

Because bootstrap templates are generated from embedded authoritative catalogs, any later production definition will automatically flow into newly initialized workspaces. Existing project-local catalogs remain untouched by bootstrap.

The authoritative owner catalog continues to use non-overlapping ranges:

- `AFW`: `0–1099999`;
- `APP`: `1100000–1999999`;
- `PLUGIN`: `2000000–2999999`;
- `USER`: `9000000–9999999`.

## Focused verification

Run the new minimum-temperature TDD contract:

```bash
dotnet test WhenItFails.Tests --filter FullyQualifiedName~TemperatureBelowMinimumLimitCatalogTests
```

Expected result: **1 red test** reporting that catalog item `TEMPERATUREBELOWMINIMUMLIMIT` was not found. Do not add the production definition until this exact red gate is user-verified.

The complete core baseline before the intentional red test is:

```bash
dotnet test WhenItFails.Tests
```

Latest user-verified result: **698 passed, 0 failed, 0 skipped**.

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

The thermal document currently covers the first six verified contracts. Add the minimum-temperature definition only after its focused contract and workspace validation are green.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, automatic translation generation, remote catalog synchronization, package publishing, a GUI, complete source-code dependency discovery, or automatic runtime behavior for humorous message variants.

Thermal easter eggs are explicitly deferred. Any future alternative wording must never alter the structured contract, severity, metadata, thresholds, control flow, shutdown decision, restart policy, sensor trust decision, data-freshness decision, thermal-trend decision, redundancy decision, low-temperature decision, or fail-safe policy.

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

Run `TemperatureBelowMinimumLimitCatalogTests` and confirm the exact missing-item failure. After that expected red gate, add only `AFW_THM_0007` to the authoritative error catalog, then rerun the focused test and complete catalog validation before updating documentation.

## Last completed change

The `AFW_THM_0006` increment remains closed at **698/698 green core tests**. A standalone red contract now defines the intended semantics of `TEMPERATUREBELOWMINIMUMLIMIT`; no production catalog data has been changed.

Commits:

```text
d723a3dd4f7c55085281d23d4eae756682ea38db
Record green temperature sensor disagreement core gate

7c2a95bc75cd688266610ccb725ddf793eaacfb7
Add temperature below minimum limit catalog contract
```
