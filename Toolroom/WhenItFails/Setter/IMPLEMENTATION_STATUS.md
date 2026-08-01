# Implementation status

Last updated: 2026-08-02

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **1,241 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

The complete `WhenItFails.Tests` core suite is user-verified green with **698 passed, 0 failed, 0 skipped** before the new minimum-temperature catalog test was added.

The runtime/public-API audit has verified defensive handling of provider failures, null tasks, null responses, null payloads, runtime-null issue collections and elements, cross-validation envelopes, successful-provider diagnostic aggregation, status normalization, and required inner members of `ErrorCatalogProviderPayload`.

## Verification status

Recently verified focused contracts include:

- `TemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- `CriticalTemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- `TemperatureSensorReadingInvalidCatalogTests`: **1 user-verified green**.
- `TemperatureReadingStaleCatalogTests`: **1 user-verified green**.
- `TemperatureRateOfChangeExceededCatalogTests`: **1 user-verified green**.
- `TemperatureSensorDisagreementCatalogTests`: **1 user-verified green**.
- `TemperatureBelowMinimumLimitCatalogTests`: **1 user-verified expected red**, reporting that `TEMPERATUREBELOWMINIMUMLIMIT` was not found.
- `DefaultJsonsTemplateProviderTests.GetTemplateFiles_ShouldIncludeThermalCatalogAndRevisedOwnerRanges`: **1 user-verified green**.
- Complete catalog validation after adding `AFW_THM_0006`: **0 errors, 0 warnings, 0 information issues**.
- Complete `WhenItFails.Tests`: **698 user-verified green, 0 failed, 0 skipped** before the latest test addition.
- Complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 user-verified green, 0 failed, 0 skipped**.

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The first six thermal definitions remain complete and verified:

- `AFW_THM_0001` / `1000001` / `TEMPERATURELIMITEXCEEDED` / `Warning`;
- `AFW_THM_0002` / `1000002` / `CRITICALTEMPERATURELIMITEXCEEDED` / `Critical`;
- `AFW_THM_0003` / `1000003` / `TEMPERATURESENSORREADINGINVALID` / `Error`;
- `AFW_THM_0004` / `1000004` / `TEMPERATUREREADINGSTALE` / `Error`;
- `AFW_THM_0005` / `1000005` / `TEMPERATURERATEOFCHANGEEXCEEDED` / `Warning`;
- `AFW_THM_0006` / `1000006` / `TEMPERATURESENSORDISAGREEMENT` / `Warning`.

The seventh thermal definition is now present in the authoritative catalog and requires user verification:

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

Because bootstrap templates are generated from embedded authoritative catalogs, this definition also flows into newly initialized workspaces after rebuild. Existing project-local catalogs remain untouched by bootstrap.

The authoritative owner catalog continues to use non-overlapping ranges:

- `AFW`: `0–1099999`;
- `APP`: `1100000–1999999`;
- `PLUGIN`: `2000000–2999999`;
- `USER`: `9000000–9999999`.

## Focused verification

Run the minimum-temperature contract:

```bash
dotnet test WhenItFails.Tests --filter FullyQualifiedName~TemperatureBelowMinimumLimitCatalogTests
```

Expected result: **1 green test**.

Then validate the complete catalog workspace:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Expected result: **0 errors, 0 warnings, and 0 information issues**.

After both focused gates are green, update the thermal documentation and rerun the complete core project. The next full run should contain **699 tests**.

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

Pull the latest commits, rerun `TemperatureBelowMinimumLimitCatalogTests`, and run `Setter validate .`. Do not update thermal documentation or add another thermal definition until both gates are green.

## Last completed change

The expected red minimum-temperature contract was user-verified with the exact missing-item failure. `AFW_THM_0007` is now added to `Jsons/WhenItFails/errors.en.json` as a distinct lower operating-boundary condition, without changing the upper-limit, sensor-validity, freshness, trend, or redundancy contracts.

Commits:

```text
7c2a95bc75cd688266610ccb725ddf793eaacfb7
Add temperature below minimum limit catalog contract

c09177baa5a67b893b871d670110399e2a40ef09
Add temperature below minimum limit catalog error
```
