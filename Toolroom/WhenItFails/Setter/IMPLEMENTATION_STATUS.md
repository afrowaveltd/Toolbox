# Implementation status

Last updated: 2026-08-01

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **1,241 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

The complete `WhenItFails.Tests` core suite is also user-verified green with **693 passed, 0 failed, 0 skipped**.

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

The first thermal definition is present:

- ID `AFW_THM_0001`;
- code `1000001`;
- name `TEMPERATURELIMITEXCEEDED`;
- parameterized message using `{temperature}`, `{unit}`, and `{limit}`;
- default severity `Warning`;
- categories `THERMAL` and `VALIDATION`;
- documentation key `when-it-fails/errors/thermal/temperature-limit-exceeded`.

The focused catalog test and complete workspace validation are user-verified green.

The authoritative owner catalog uses these non-overlapping ranges:

- `AFW`: `0–1099999`;
- `APP`: `1100000–1999999`;
- `PLUGIN`: `2000000–2999999`;
- `USER`: `9000000–9999999`.

No existing error code was renumbered.

Bootstrap synchronization is user-verified green on Linux. `DefaultJsonsTemplateProvider` reads embedded copies of the authoritative catalogs under `Jsons/WhenItFails` instead of maintaining large duplicate raw JSON strings. The error template preserves the established bootstrap representation by converting IDs such as `AFW_THM_0001` to `AFW-THM-0001` and deriving PascalCase names such as `TemperatureLimitExceeded` from documentation keys.

The four `*.en.json` resources explicitly set `<WithCulture>false</WithCulture>` so MSBuild keeps them in the main assembly rather than treating them as English satellite resources.

The resource-backed bootstrap architecture is now documented in `WhenItFails/Docs/Bootstrap/en.md`. The maintained reference catalogs also contain:

- category `THERMAL` in `WhenItFails/ReferenceCatalog/Core/categories.en.json`;
- code group `THERMAL`, prefix `THM`, range `1000000–1099999` in `WhenItFails/ReferenceCatalog/Core/code-groups.en.json`.

These documentation and reference-catalog changes require a regression run before the next thermal error is added.

## Focused verification

Run the complete core project after the documentation and reference synchronization:

```powershell
dotnet test WhenItFails.Tests
```

Expected result: **all tests green**. The previously verified baseline is **693 passed, 0 failed, 0 skipped**.

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

Next documentation target: keep the thermal document synchronized as the error family expands.

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

Run the complete `WhenItFails.Tests` project. If it remains green, add a focused red catalog test for the second thermal definition, `CriticalTemperatureLimitExceeded`, without changing the existing warning-level `TemperatureLimitExceeded` contract.

## Last completed change

The resource-backed bootstrap design is now documented, and the maintained reference category and code-group catalogs include the thermal domain. Historical reference-catalog differences outside this thermal slice were intentionally left unchanged. The next gate is the complete core test project before beginning the second thermal error.

Commits:

```text
42e2f5bf66ac937499257c1a297867658f1f8334
Document resource-backed bootstrap templates

70d6fc445741329ceb630831a092d330c275653b
Add thermal reference category

41ca5150ddd84816c42a20daeca474e4a9f0dc59
Add thermal reference code group
```
