# Implementation status

Last updated: 2026-08-01

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **1,241 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

The complete `WhenItFails.Tests` core suite is user-verified green with **696 passed, 0 failed, 0 skipped** before the new temperature-rate catalog test was added.

The runtime/public-API audit has verified defensive handling of provider failures, null tasks, null responses, null payloads, runtime-null issue collections and elements, cross-validation envelopes, successful-provider diagnostic aggregation, status normalization, and required inner members of `ErrorCatalogProviderPayload`.

## Verification status

Recently verified focused contracts include:

- `TemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- `CriticalTemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- `TemperatureSensorReadingInvalidCatalogTests`: **1 user-verified green**.
- `TemperatureReadingStaleCatalogTests`: **1 user-verified green**.
- `TemperatureRateOfChangeExceededCatalogTests`: **1 user-verified expected red**, reporting that `TEMPERATURERATEOFCHANGEEXCEEDED` was not found.
- `DefaultJsonsTemplateProviderTests.GetTemplateFiles_ShouldIncludeThermalCatalogAndRevisedOwnerRanges`: **1 user-verified green**.
- Complete catalog validation after adding `AFW_THM_0004`: **0 errors, 0 warnings, 0 information issues**.
- Complete `WhenItFails.Tests`: **696 user-verified green, 0 failed, 0 skipped** before the latest test addition.
- Complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 user-verified green, 0 failed, 0 skipped**.

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The first four thermal definitions remain complete and verified:

- `AFW_THM_0001` / `1000001` / `TEMPERATURELIMITEXCEEDED` / `Warning`;
- `AFW_THM_0002` / `1000002` / `CRITICALTEMPERATURELIMITEXCEEDED` / `Critical`;
- `AFW_THM_0003` / `1000003` / `TEMPERATURESENSORREADINGINVALID` / `Error`;
- `AFW_THM_0004` / `1000004` / `TEMPERATUREREADINGSTALE` / `Error`.

The fifth thermal definition is now present in the authoritative catalog and requires user verification:

- ID `AFW_THM_0005`;
- code `1000005`;
- name `TEMPERATURERATEOFCHANGEEXCEEDED`;
- title `Temperature rate of change exceeded`;
- message `Temperature from sensor {sensor} changed at {rate}{unitPerTime}, exceeding the configured maximum rate of {maxRate}{unitPerTime}.`;
- default severity `Warning`;
- categories `THERMAL` and `VALIDATION`;
- subcategories `SENSOR` and `RATE_OF_CHANGE`;
- tags `THERMAL`, `TEMPERATURE`, `SENSOR`, `TREND`, and `USER_VISIBLE`;
- documentation key `when-it-fails/errors/thermal/temperature-rate-of-change-exceeded`.

This contract concerns the speed of temperature change, not the absolute temperature. A reading may be valid, fresh, and below the critical limit while its trend still exceeds the configured safe rate.

Because bootstrap templates are generated from embedded authoritative catalogs, this definition also flows into newly initialized workspaces after rebuild. Existing project-local catalogs remain untouched by bootstrap.

The authoritative owner catalog continues to use non-overlapping ranges:

- `AFW`: `0–1099999`;
- `APP`: `1100000–1999999`;
- `PLUGIN`: `2000000–2999999`;
- `USER`: `9000000–9999999`.

## Focused verification

Run the temperature-rate contract:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~TemperatureRateOfChangeExceededCatalogTests
```

Expected result: **1 green test**.

Then validate the complete catalog workspace:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Expected result: **0 errors, 0 warnings, and 0 information issues**.

After both focused gates are green, update the thermal documentation and rerun the complete core project. The next full run should contain **697 tests**.

The complete Setter suite remains available through:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Before committing further catalog changes, also run:

```powershell
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

The thermal document currently covers the first four verified contracts. Add the rate-of-change definition only after its focused contract and workspace validation are green.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, automatic translation generation, remote catalog synchronization, package publishing, a GUI, complete source-code dependency discovery, or automatic runtime behavior for humorous message variants.

Thermal easter eggs are explicitly deferred. Any future alternative wording must never alter the structured contract, severity, metadata, thresholds, control flow, shutdown decision, restart policy, sensor trust decision, data-freshness decision, thermal-trend decision, or fail-safe policy.

## Working rules

- GitHub `master` is the source of truth.
- Keep each change narrow and intentional.
- Add or update tests immediately.
- Do not advance while the current slice is red.
- Update this file after every change.
- Prefer one file plus its directly related test, then commit.

## Recommended next step

Pull the latest commits, rerun `TemperatureRateOfChangeExceededCatalogTests`, and run `Setter validate .`. Do not update thermal documentation or add another thermal definition until both gates are green.

## Last completed change

The expected red temperature-rate contract was user-verified with the exact missing-item failure. `AFW_THM_0005` is now added to `Jsons/WhenItFails/errors.en.json` as a distinct thermal-trend condition, without changing the absolute-limit, sensor-validity, or data-freshness contracts.

Commits:

```text
5c09d66e5d54bd77bfbe5f1c9ab3f502403722bf
Add temperature rate of change catalog contract

287ffa9343f66ef5f504a3f4367043d5a0386d58
Add temperature rate of change catalog error
```
