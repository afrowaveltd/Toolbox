# Implementation status

Last updated: 2026-08-01

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **1,241 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

The complete `WhenItFails.Tests` core suite is user-verified green with **693 passed, 0 failed, 0 skipped**, including the documentation and reference-catalog synchronization.

The runtime/public-API audit has verified defensive handling of provider failures, null tasks, null responses, null payloads, runtime-null issue collections and elements, cross-validation envelopes, successful-provider diagnostic aggregation, status normalization, and required inner members of `ErrorCatalogProviderPayload`.

## Verification status

Recently verified focused contracts include:

- `ErrorCatalogContextProviderSuccessfulEnvelopeSuppressionTests`: **2 user-verified green**.
- `ErrorCatalogContextProviderSuccessfulStatusNormalizationTests`: **3 user-verified green**.
- `ErrorCatalogContextProviderEmptyWarningEnvelopeNormalizationTests`: **3 user-verified green**.
- `ErrorCatalogContextProviderNullInnerPayloadTests`: **2 user-verified green**.
- `TemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- `DefaultJsonsTemplateProviderTests.GetTemplateFiles_ShouldIncludeThermalCatalogAndRevisedOwnerRanges`: **1 user-verified green**.
- Complete `WhenItFails.Tests`: **693 user-verified green, 0 failed, 0 skipped**.
- Complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 user-verified green, 0 failed, 0 skipped**.

The thermal domain is registered with category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The first thermal definition is present and verified:

- ID `AFW_THM_0001`;
- code `1000001`;
- name `TEMPERATURELIMITEXCEEDED`;
- default severity `Warning`;
- documentation key `when-it-fails/errors/thermal/temperature-limit-exceeded`.

A focused TDD contract has now been added for the second thermal definition. It requires:

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

The production catalog has not yet been changed. The new focused test is expected to be red because `CRITICALTEMPERATURELIMITEXCEEDED` does not yet exist.

The authoritative owner catalog uses these non-overlapping ranges:

- `AFW`: `0–1099999`;
- `APP`: `1100000–1999999`;
- `PLUGIN`: `2000000–2999999`;
- `USER`: `9000000–9999999`.

No existing error code was renumbered.

Bootstrap synchronization is user-verified green on Linux. `DefaultJsonsTemplateProvider` reads embedded copies of the authoritative catalogs under `Jsons/WhenItFails` instead of maintaining large duplicate raw JSON strings. The four `*.en.json` resources explicitly set `<WithCulture>false</WithCulture>` so MSBuild keeps them in the main assembly rather than treating them as English satellite resources.

The resource-backed bootstrap architecture is documented in `WhenItFails/Docs/Bootstrap/en.md`. The maintained reference catalogs contain category `THERMAL` and code group `THERMAL/THM`.

## Focused verification

Run the new critical thermal TDD contract:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~CriticalTemperatureLimitExceededCatalogTests
```

Expected result: **1 red test** reporting that catalog item `CRITICALTEMPERATURELIMITEXCEEDED` was not found. Do not add the production definition until this exact red gate is user-verified.

The complete core project remains user-verified green before this new intentionally red test:

```powershell
dotnet test WhenItFails.Tests
```

Latest baseline: **693 passed, 0 failed, 0 skipped**.

The complete catalog workspace remains available through:

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

Next documentation target: add the critical temperature definition after its catalog contract is green.

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

Run `CriticalTemperatureLimitExceededCatalogTests` and confirm the expected missing-item failure. After that exact red gate, add only `AFW_THM_0002` to the authoritative error catalog, then rerun the focused test and complete catalog validation before updating thermal documentation.

## Last completed change

The documentation/reference synchronization remains user-verified green at **693/693 core tests**. The next thermal slice has started with a standalone red catalog contract for `CRITICALTEMPERATURELIMITEXCEEDED`; no production catalog data has been changed yet.

Commits:

```text
42e2f5bf66ac937499257c1a297867658f1f8334
Document resource-backed bootstrap templates

70d6fc445741329ceb630831a092d330c275653b
Add thermal reference category

41ca5150ddd84816c42a20daeca474e4a9f0dc59
Add thermal reference code group

b539dffc0c165c7c92b3b3212b536d12a4c0c34f
Add critical temperature limit catalog contract
```
