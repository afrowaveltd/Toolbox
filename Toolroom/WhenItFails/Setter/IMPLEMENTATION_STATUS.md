# Implementation status

Last updated: 2026-08-01

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **1,241 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

The complete `WhenItFails.Tests` core suite is user-verified green with **694 passed, 0 failed, 0 skipped** before the new intentionally red sensor-reading contract.

The runtime/public-API audit has verified defensive handling of provider failures, null tasks, null responses, null payloads, runtime-null issue collections and elements, cross-validation envelopes, successful-provider diagnostic aggregation, status normalization, and required inner members of `ErrorCatalogProviderPayload`.

## Verification status

Recently verified focused contracts include:

- `TemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- `CriticalTemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- `DefaultJsonsTemplateProviderTests.GetTemplateFiles_ShouldIncludeThermalCatalogAndRevisedOwnerRanges`: **1 user-verified green**.
- Complete catalog validation after adding `AFW_THM_0002`: **0 errors, 0 warnings, 0 information issues**.
- Complete `WhenItFails.Tests`: **694 user-verified green, 0 failed, 0 skipped** before the new intentional red test.
- Complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 user-verified green, 0 failed, 0 skipped**.

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The first two thermal definitions are complete and verified:

- `AFW_THM_0001` / `1000001` / `TEMPERATURELIMITEXCEEDED` / `Warning`;
- `AFW_THM_0002` / `1000002` / `CRITICALTEMPERATURELIMITEXCEEDED` / `Critical`.

A focused TDD contract has now been added for a third, semantically separate condition:

- ID `AFW_THM_0003`;
- code `1000003`;
- name `TEMPERATURESENSORREADINGINVALID`;
- title `Temperature sensor reading invalid`;
- message `Temperature sensor {sensor} reported an invalid or unreliable reading.`;
- default severity `Error`;
- categories `THERMAL` and `VALIDATION`;
- subcategories `SENSOR` and `INVALID_READING`;
- tags `THERMAL`, `TEMPERATURE`, `SENSOR`, `FAIL_SAFE`, and `USER_VISIBLE`;
- documentation key `when-it-fails/errors/thermal/temperature-sensor-reading-invalid`.

This contract does not claim that a real thermal limit was exceeded. It represents the loss of trustworthy temperature input, so application safety decisions must follow the configured fail-safe policy rather than treating the reported value as valid.

The production catalog has not yet been changed. The focused test is expected to be red because `TEMPERATURESENSORREADINGINVALID` does not exist yet.

Because bootstrap templates are generated from embedded authoritative catalogs, any later production definition will automatically flow into newly initialized workspaces. Existing project-local catalogs remain untouched by bootstrap.

The authoritative owner catalog continues to use non-overlapping ranges:

- `AFW`: `0–1099999`;
- `APP`: `1100000–1999999`;
- `PLUGIN`: `2000000–2999999`;
- `USER`: `9000000–9999999`.

## Focused verification

Run the new invalid-sensor-reading TDD contract:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~TemperatureSensorReadingInvalidCatalogTests
```

Expected result: **1 red test** reporting that catalog item `TEMPERATURESENSORREADINGINVALID` was not found. Do not add the production definition until this exact red gate is user-verified.

The complete core baseline before the intentional red test is:

```powershell
dotnet test WhenItFails.Tests
```

Latest user-verified result: **694 passed, 0 failed, 0 skipped**.

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

The thermal document currently covers the first two verified contracts. Add the invalid sensor-reading definition only after its focused contract and workspace validation are green.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, automatic translation generation, remote catalog synchronization, package publishing, a GUI, complete source-code dependency discovery, or automatic runtime behavior for humorous message variants.

Thermal easter eggs are explicitly deferred. Any future absurd-temperature wording must never alter the structured contract, severity, metadata, thresholds, control flow, shutdown decision, restart policy, sensor trust decision, or fail-safe policy.

## Working rules

- GitHub `master` is the source of truth.
- Keep each change narrow and intentional.
- Add or update tests immediately.
- Do not advance while the current slice is red.
- Update this file after every change.
- Prefer one file plus its directly related test, then commit.

## Recommended next step

Run `TemperatureSensorReadingInvalidCatalogTests` and confirm the exact missing-item failure. After that expected red gate, add only `AFW_THM_0003` to the authoritative error catalog, then rerun the focused test and complete catalog validation before updating documentation.

## Last completed change

The `AFW_THM_0002` increment remains closed at **694/694 green core tests**. A new standalone red contract now defines the intended semantics of `TEMPERATURESENSORREADINGINVALID`; no production catalog data has been changed.

Commits:

```text
a3d0a12fb0d4d1fcd0a04706d33eb038e872c1ff
Record green critical thermal core gate

0a39b43b4255d2405b42aaf7fa7ee90911aad308
Add invalid temperature sensor reading catalog contract
```