# Implementation status

Last updated: 2026-08-01

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **1,241 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

The complete `WhenItFails.Tests` core suite is user-verified green with **695 passed, 0 failed, 0 skipped** before the stale-reading catalog test was added. The next full run should contain **696 tests**.

The runtime/public-API audit has verified defensive handling of provider failures, null tasks, null responses, null payloads, runtime-null issue collections and elements, cross-validation envelopes, successful-provider diagnostic aggregation, status normalization, and required inner members of `ErrorCatalogProviderPayload`.

## Verification status

Recently verified focused contracts include:

- `TemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- `CriticalTemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- `TemperatureSensorReadingInvalidCatalogTests`: **1 user-verified green**.
- `TemperatureReadingStaleCatalogTests`: **1 user-verified green**.
- `DefaultJsonsTemplateProviderTests.GetTemplateFiles_ShouldIncludeThermalCatalogAndRevisedOwnerRanges`: **1 user-verified green**.
- Complete catalog validation after adding `AFW_THM_0004`: **0 errors, 0 warnings, 0 information issues**.
- Complete `WhenItFails.Tests`: **695 user-verified green, 0 failed, 0 skipped** before the latest test addition.
- Complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 user-verified green, 0 failed, 0 skipped**.

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The first four thermal definitions are present and focused-validation verified:

- `AFW_THM_0001` / `1000001` / `TEMPERATURELIMITEXCEEDED` / `Warning`;
- `AFW_THM_0002` / `1000002` / `CRITICALTEMPERATURELIMITEXCEEDED` / `Critical`;
- `AFW_THM_0003` / `1000003` / `TEMPERATURESENSORREADINGINVALID` / `Error`;
- `AFW_THM_0004` / `1000004` / `TEMPERATUREREADINGSTALE` / `Error`.

`TEMPERATUREREADINGSTALE` uses message `Temperature reading from sensor {sensor} is stale; its age of {age} exceeds the configured maximum age of {maxAge}.`, categories `THERMAL` and `VALIDATION`, subcategories `SENSOR` and `STALE_READING`, tags `THERMAL`, `TEMPERATURE`, `SENSOR`, `STALE_DATA`, `FAIL_SAFE`, and `USER_VISIBLE`, and documentation key `when-it-fails/errors/thermal/temperature-reading-stale`.

This contract represents a reading that may be structurally valid and plausible but is too old for safe decision-making. It must not be treated as current, silently refreshed, or merged with invalid-reading or limit-exceeded contracts.

Because bootstrap templates are generated from embedded authoritative catalogs, all four thermal definitions flow into newly initialized workspaces after rebuild. Existing project-local catalogs remain untouched by bootstrap.

The authoritative owner catalog continues to use non-overlapping ranges:

- `AFW`: `0–1099999`;
- `APP`: `1100000–1999999`;
- `PLUGIN`: `2000000–2999999`;
- `USER`: `9000000–9999999`.

## Focused verification

The stale-reading contract is user-verified green:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~TemperatureReadingStaleCatalogTests
```

Latest user-verified result: **1 passed, 0 failed, 0 skipped**.

The complete catalog workspace is user-verified green after adding `AFW_THM_0004`:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Latest user-verified result: **0 errors, 0 warnings, and 0 information issues**.

After the thermal documentation update, rerun the complete core project:

```powershell
dotnet test WhenItFails.Tests
```

Expected next full result: **696 passed, 0 failed, 0 skipped**.

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

Next documentation target: add the verified stale-reading contract and its data-freshness boundary to `WhenItFails/Docs/Thermal Errors/en.md`.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, automatic translation generation, remote catalog synchronization, package publishing, a GUI, complete source-code dependency discovery, or automatic runtime behavior for humorous message variants.

Thermal easter eggs are explicitly deferred. Any future alternative wording must never alter the structured contract, severity, metadata, thresholds, control flow, shutdown decision, restart policy, sensor trust decision, data-freshness decision, or fail-safe policy.

## Working rules

- GitHub `master` is the source of truth.
- Keep each change narrow and intentional.
- Add or update tests immediately.
- Do not advance while the current slice is red.
- Update this file after every change.
- Prefer one file plus its directly related test, then commit.

## Recommended next step

Document `TEMPERATUREREADINGSTALE` and its data-freshness boundary, then run the complete `WhenItFails.Tests` project. Do not add another thermal definition until the expected **696-test** core gate is user-verified green.

## Last completed change

`AFW_THM_0004` is now focused-test and workspace-validation verified. The catalog distinguishes a stale but otherwise plausible reading from both an invalid reading and a trusted current reading.

Commits:

```text
203c6335fb0188e89991e7581b9ab93d13c7b296
Add stale temperature reading catalog contract

9a181e58ece8a6497555da92c71eedbb111cfe6c
Add stale temperature reading catalog error
```