# Implementation status

Last updated: 2026-08-01

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **1,241 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

The complete `WhenItFails.Tests` core suite is user-verified green with **695 passed, 0 failed, 0 skipped** before the new intentionally red stale-reading contract.

The runtime/public-API audit has verified defensive handling of provider failures, null tasks, null responses, null payloads, runtime-null issue collections and elements, cross-validation envelopes, successful-provider diagnostic aggregation, status normalization, and required inner members of `ErrorCatalogProviderPayload`.

## Verification status

Recently verified focused contracts include:

- `TemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- `CriticalTemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- `TemperatureSensorReadingInvalidCatalogTests`: **1 user-verified green**.
- `DefaultJsonsTemplateProviderTests.GetTemplateFiles_ShouldIncludeThermalCatalogAndRevisedOwnerRanges`: **1 user-verified green**.
- Complete catalog validation after adding `AFW_THM_0003`: **0 errors, 0 warnings, 0 information issues**.
- Complete `WhenItFails.Tests`: **695 user-verified green, 0 failed, 0 skipped** before the intentional red test.
- Complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 user-verified green, 0 failed, 0 skipped**.

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The first three thermal definitions are complete and verified:

- `AFW_THM_0001` / `1000001` / `TEMPERATURELIMITEXCEEDED` / `Warning`;
- `AFW_THM_0002` / `1000002` / `CRITICALTEMPERATURELIMITEXCEEDED` / `Critical`;
- `AFW_THM_0003` / `1000003` / `TEMPERATURESENSORREADINGINVALID` / `Error`.

A focused TDD contract has now been added for a fourth, semantically separate state:

- ID `AFW_THM_0004`;
- code `1000004`;
- name `TEMPERATUREREADINGSTALE`;
- title `Temperature reading stale`;
- message `Temperature reading from sensor {sensor} is stale; its age of {age} exceeds the configured maximum age of {maxAge}.`;
- default severity `Error`;
- categories `THERMAL` and `VALIDATION`;
- subcategories `SENSOR` and `STALE_READING`;
- tags `THERMAL`, `TEMPERATURE`, `SENSOR`, `STALE_DATA`, `FAIL_SAFE`, and `USER_VISIBLE`;
- documentation key `when-it-fails/errors/thermal/temperature-reading-stale`.

This contract represents a reading that may be structurally valid and plausible but is too old for safe decision-making. It must not be treated as current, silently refreshed, or merged with invalid-reading or limit-exceeded contracts.

The production catalog has not yet been changed. The focused test is expected to be red because `TEMPERATUREREADINGSTALE` does not exist yet.

Because bootstrap templates are generated from embedded authoritative catalogs, any later production definition will automatically flow into newly initialized workspaces. Existing project-local catalogs remain untouched by bootstrap.

The authoritative owner catalog continues to use non-overlapping ranges:

- `AFW`: `0–1099999`;
- `APP`: `1100000–1999999`;
- `PLUGIN`: `2000000–2999999`;
- `USER`: `9000000–9999999`.

## Focused verification

Run the new stale-reading TDD contract:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~TemperatureReadingStaleCatalogTests
```

Expected result: **1 red test** reporting that catalog item `TEMPERATUREREADINGSTALE` was not found. Do not add the production definition until this exact red gate is user-verified.

The complete core baseline before the intentional red test is:

```powershell
dotnet test WhenItFails.Tests
```

Latest user-verified result: **695 passed, 0 failed, 0 skipped**.

The complete catalog workspace remains user-verified green:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Latest user-verified result: **0 errors, 0 warnings, and 0 information issues**.

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

The thermal document currently covers the first three verified contracts. Add the stale-reading definition only after its focused contract and workspace validation are green.

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

Run `TemperatureReadingStaleCatalogTests` and confirm the exact missing-item failure. After that expected red gate, add only `AFW_THM_0004` to the authoritative error catalog, then rerun the focused test and complete catalog validation before updating documentation.

## Last completed change

The `AFW_THM_0003` increment remains closed at **695/695 green core tests**. A standalone red contract now defines the intended semantics of `TEMPERATUREREADINGSTALE`; no production catalog data has been changed.

Commits:

```text
9a9361ac17f7d58413398daf02fca8e3e88dc770
Record green invalid sensor reading core gate

203c6335fb0188e89991e7581b9ab93d13c7b296
Add stale temperature reading catalog contract
```