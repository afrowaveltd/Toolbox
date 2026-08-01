# Implementation status

Last updated: 2026-08-02

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **1,241 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

The complete `WhenItFails.Tests` core suite is user-verified green with **699 passed, 0 failed, 0 skipped**.

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
- `DefaultJsonsTemplateProviderTests.GetTemplateFiles_ShouldIncludeThermalCatalogAndRevisedOwnerRanges`: **1 user-verified green**.
- Complete catalog validation after adding `AFW_THM_0007`: **0 errors, 0 warnings, 0 information issues**.
- Complete `WhenItFails.Tests`: **699 user-verified green, 0 failed, 0 skipped**.
- Complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 user-verified green, 0 failed, 0 skipped**.

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The first seven thermal definitions are complete and verified:

- `AFW_THM_0001` / `1000001` / `TEMPERATURELIMITEXCEEDED` / `Warning`;
- `AFW_THM_0002` / `1000002` / `CRITICALTEMPERATURELIMITEXCEEDED` / `Critical`;
- `AFW_THM_0003` / `1000003` / `TEMPERATURESENSORREADINGINVALID` / `Error`;
- `AFW_THM_0004` / `1000004` / `TEMPERATUREREADINGSTALE` / `Error`;
- `AFW_THM_0005` / `1000005` / `TEMPERATURERATEOFCHANGEEXCEEDED` / `Warning`;
- `AFW_THM_0006` / `1000006` / `TEMPERATURESENSORDISAGREEMENT` / `Warning`;
- `AFW_THM_0007` / `1000007` / `TEMPERATUREBELOWMINIMUMLIMIT` / `Warning`.

`TEMPERATUREBELOWMINIMUMLIMIT` uses message `The reported temperature {temperature}{unit} is below the configured minimum operating limit of {limit}{unit}.`, categories `THERMAL` and `VALIDATION`, subcategories `MINIMUM_LIMIT` and `TEMPERATURE`, tags `THERMAL`, `TEMPERATURE`, `LOW_TEMPERATURE`, and `USER_VISIBLE`, and documentation key `when-it-fails/errors/thermal/temperature-below-minimum-limit`.

This contract represents a trusted, current reading below the configured minimum operating boundary. It remains distinct from upper-limit, invalid-reading, stale-reading, trend, and sensor-disagreement contracts.

Because bootstrap templates are generated from embedded authoritative catalogs, all seven thermal definitions flow into newly initialized workspaces after rebuild. Existing project-local catalogs remain untouched by bootstrap.

The authoritative owner catalog continues to use non-overlapping ranges:

- `AFW`: `0–1099999`;
- `APP`: `1100000–1999999`;
- `PLUGIN`: `2000000–2999999`;
- `USER`: `9000000–9999999`.

## Focused verification

The minimum-temperature contract is user-verified green:

```bash
dotnet test WhenItFails.Tests --filter FullyQualifiedName~TemperatureBelowMinimumLimitCatalogTests
```

Latest user-verified result: **1 passed, 0 failed, 0 skipped**.

The complete catalog workspace is user-verified green after adding `AFW_THM_0007`:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Latest user-verified result: **0 errors, 0 warnings, and 0 information issues**.

The complete core project is user-verified green:

```bash
dotnet test WhenItFails.Tests
```

Latest user-verified result: **699 passed, 0 failed, 0 skipped**.

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

`WhenItFails/Docs/Thermal Errors/en.md` documents all seven completed thermal contracts. The minimum-temperature section distinguishes a trusted lower-bound violation from invalid sensor data, defines structured diagnostic fields, leaves warm-up and low-temperature actions to application policy, and documents hysteresis and coexistence with trend or redundancy conditions.

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

Add a standalone TDD contract for a trusted, current reading below a configured critical minimum temperature. Keep it distinct from the ordinary minimum-temperature warning and do not change the production catalog until the expected focused red gate is user-verified.

## Last completed change

The `AFW_THM_0007` increment is closed at **699/699 green core tests**. The next slice will define the critical low-temperature boundary as a separate contract.

Commits:

```text
c09177baa5a67b893b871d670110399e2a40ef09
Add temperature below minimum limit catalog error

d72f27034bbcfb67ec18734ce72e69eeef143a21
Document temperature below minimum limit

8d3ac0da9898b3a2249df3806cd72b36d69c749d
Record minimum temperature documentation and pending core gate
```
