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

The thermal domain is registered with:

- category `THERMAL`;
- code group `THERMAL`;
- prefix `THM`;
- range `1000000–1099999`.

The user verified `Setter validate .` after registration with **0 errors, 0 warnings, and 0 information issues**.

`TemperatureLimitExceededCatalogTests` then produced the expected red result because `TEMPERATURELIMITEXCEEDED` was not yet present. The first thermal definition has now been added to `errors.en.json`:

- ID `AFW_THM_0001`;
- code `1000001`;
- name `TEMPERATURELIMITEXCEEDED`;
- parameterized message using `{temperature}`, `{unit}`, and `{limit}`;
- default severity `Warning`;
- categories `THERMAL` and `VALIDATION`;
- documentation key `when-it-fails/errors/thermal/temperature-limit-exceeded`.

The English documentation is now present at `WhenItFails/Docs/Thermal Errors/en.md`. The focused test and catalog validation require user verification after these commits.

## Focused verification

Run the implemented thermal contract:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~TemperatureLimitExceededCatalogTests
```

Expected result: **1 green test**.

Then validate the complete catalog workspace:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Expected result: **0 errors, 0 warnings, and 0 information issues**.

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
- `WhenItFails/Docs/Thermal Errors/en.md`.

Next documentation target: keep the thermal document synchronized as the error family expands. Keep this file synchronized while the runtime/public-API audit continues.

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

Run `TemperatureLimitExceededCatalogTests` and `Setter validate .`. Do not add another thermal definition until the first catalog item is confirmed green and the complete workspace validates without issues.

## Last completed change

The latest slice records the expected red TDD result and implements the first thermal catalog definition. `AFW_THM_0001` now represents a reported temperature above the configured safe operating limit without conflating that condition with a critical or shutdown threshold. Its message is parameterized, its developer guidance covers sensor and cooling verification, and its documentation explicitly preserves structured data as the source of truth. Humorous extreme-temperature wording remains deferred and contract-neutral.

Commits:

```text
8acba4041a9603e4175cc3e8d76efb9813219b64
Add thermal catalog contract test

c4c0afd5f744404132ee3e633b26988d57114d2a
Add temperature limit exceeded catalog error

620f34b715142bd06acefb78c98068379625827f
Document temperature limit exceeded error
```
