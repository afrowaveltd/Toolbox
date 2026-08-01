# Implementation status

Last updated: 2026-08-01

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **1,241 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

The complete `WhenItFails.Tests` core suite was most recently user-verified green with **693 passed, 0 failed, 0 skipped** before the new critical-temperature TDD slice.

The runtime/public-API audit has verified defensive handling of provider failures, null tasks, null responses, null payloads, runtime-null issue collections and elements, cross-validation envelopes, successful-provider diagnostic aggregation, status normalization, and required inner members of `ErrorCatalogProviderPayload`.

## Verification status

Recently verified focused contracts include:

- `TemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- `DefaultJsonsTemplateProviderTests.GetTemplateFiles_ShouldIncludeThermalCatalogAndRevisedOwnerRanges`: **1 user-verified green**.
- `CriticalTemperatureLimitExceededCatalogTests`: **1 user-verified expected red**, reporting that `CRITICALTEMPERATURELIMITEXCEEDED` was not found.
- Complete `WhenItFails.Tests`: **693 user-verified green, 0 failed, 0 skipped** before the intentional red test.
- Complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 user-verified green, 0 failed, 0 skipped**.

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The first thermal definition remains unchanged and verified:

- ID `AFW_THM_0001`;
- code `1000001`;
- name `TEMPERATURELIMITEXCEEDED`;
- default severity `Warning`;
- documentation key `when-it-fails/errors/thermal/temperature-limit-exceeded`.

The second thermal definition is now present in the authoritative error catalog but requires user verification:

- ID `AFW_THM_0002`;
- code `1000002`;
- name `CRITICALTEMPERATURELIMITEXCEEDED`;
- title `Critical temperature limit exceeded`;
- message `The reported temperature {temperature}{unit} exceeds the configured critical shutdown limit of {limit}{unit}.`;
- default severity `Critical`;
- categories `THERMAL` and `VALIDATION`;
- subcategories `CRITICAL_LIMIT` and `SHUTDOWN`;
- tags `THERMAL`, `TEMPERATURE`, `SHUTDOWN`, and `USER_VISIBLE`;
- documentation key `when-it-fails/errors/thermal/critical-temperature-limit-exceeded`.

Because bootstrap templates are generated from embedded authoritative catalogs, this new definition will also flow into newly initialized workspaces after the project rebuilds. Existing project-local catalogs remain untouched by bootstrap.

The authoritative owner catalog continues to use non-overlapping ranges:

- `AFW`: `0–1099999`;
- `APP`: `1100000–1999999`;
- `PLUGIN`: `2000000–2999999`;
- `USER`: `9000000–9999999`.

## Focused verification

Run the critical thermal contract:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~CriticalTemperatureLimitExceededCatalogTests
```

Expected result: **1 green test**.

Then validate the complete catalog workspace:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Expected result: **0 errors, 0 warnings, and 0 information issues**.

After both focused gates are green, rerun the complete core project:

```powershell
dotnet test WhenItFails.Tests
```

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

Next documentation target: add the critical temperature definition after its focused catalog contract and complete validation are green.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, automatic translation generation, remote catalog synchronization, package publishing, a GUI, complete source-code dependency discovery, or automatic runtime behavior for humorous message variants.

Thermal easter eggs are explicitly deferred. Any future absurd-temperature wording must never alter the structured contract, severity, metadata, thresholds, or application decision-making.

## Working rules

- GitHub `master` is the source of truth.
- Keep each change narrow and intentional.
- Add or update tests immediately.
- Do not advance while the current slice is red.
- Update this file after every change.
- Prefer one file plus its directly related test, then commit.

## Recommended next step

Pull the latest commits, rerun `CriticalTemperatureLimitExceededCatalogTests`, and run `Setter validate .`. Do not update thermal documentation or add another thermal definition until both gates are green.

## Last completed change

The expected red contract was user-verified with the exact missing-item failure. `AFW_THM_0002` is now added to `Jsons/WhenItFails/errors.en.json` as a distinct critical shutdown condition, without changing the warning-level `AFW_THM_0001` contract.

Commits:

```text
b539dffc0c165c7c92b3b3212b536d12a4c0c34f
Add critical temperature limit catalog contract

11cad246ad67bcbaac8a9c3cdb002c8993024308
Add critical temperature limit exceeded catalog error
```
