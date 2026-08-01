# Implementation status

Last updated: 2026-08-01

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **1,241 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

The runtime/public-API audit has also verified defensive handling of provider failures, null tasks, null responses, null payloads, runtime-null issue collections and elements, cross-validation envelopes, successful-provider diagnostic aggregation, status normalization, and required inner members of `ErrorCatalogProviderPayload`.

## Verification status

Recently verified focused contracts include:

- `ErrorCatalogContextProviderSuccessfulEnvelopeSuppressionTests`: **2 user-verified green**.
- `ErrorCatalogContextProviderSuccessfulStatusNormalizationTests`: **3 user-verified green**.
- `ErrorCatalogContextProviderEmptyWarningEnvelopeNormalizationTests`: **3 user-verified green**.
- `ErrorCatalogContextProviderNullInnerPayloadTests`: **2 user-verified green**, confirming both `Document == null` and `Catalog == null` return `Invalid` with `ErrorCatalogContextPayloadIsNull` before later providers run.
- Complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 user-verified green, 0 failed, 0 skipped**.

A new thermal domain is now registered:

- category `THERMAL`;
- code group `THERMAL`;
- prefix `THM`;
- range `1000000–1099999`.

The user verified `Setter validate .` after this registration with **0 errors, 0 warnings, and 0 information issues**.

A new focused TDD test, `TemperatureLimitExceededCatalogTests`, is committed and currently expects the first thermal definition:

- ID `AFW_THM_0001`;
- code `1000001`;
- name `TEMPERATURELIMITEXCEEDED`;
- parameterized message using `{temperature}`, `{unit}`, and `{limit}`;
- warning severity;
- thermal and validation categories;
- documentation key `when-it-fails/errors/thermal/temperature-limit-exceeded`.

The error definition is intentionally not yet present, so the focused test should first fail by reporting that catalog item `TEMPERATURELIMITEXCEEDED` was not found.

## Focused verification

Run the thermal TDD gate:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~TemperatureLimitExceededCatalogTests
```

Expected current result: **1 red test** because the category and code group exist but `AFW_THM_0001` has not yet been added to `errors.en.json`.

The complete Setter suite remains available through:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Before committing catalog changes, also run:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
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
- `Docs/Reviewing Catalog Changes/en.md`.

Next documentation target: add the focused English document for `TemperatureLimitExceeded` after its catalog definition is committed and validated. Keep this file synchronized while the runtime/public-API audit continues.

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

Run `TemperatureLimitExceededCatalogTests`. Confirm the expected red result before adding `AFW_THM_0001` to `Jsons/WhenItFails/errors.en.json`. After the definition is added, rerun the focused test and `Setter validate .`, then add `Docs/Temperature Limit Exceeded/en.md`.

## Last completed change

The latest slice introduces the thermal domain incrementally. `THERMAL` was added to the category catalog, `THERMAL/THM` was assigned range `1000000–1099999`, and the registration passed catalog validation. A focused integration-style test now locks the complete intended contract for the first thermal error before implementation.

Commits:

```text
a4d5996ab11de803b641213b9fcae578c24e60ef
Register thermal category

7715d1b46ae74c72cad7e31d81fdc1929e90a8bd
Register thermal code group

8acba4041a9603e4175cc3e8d76efb9813219b64
Add thermal catalog contract test
```
