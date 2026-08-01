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
- `ErrorCatalogContextProviderNullInnerPayloadTests`: **2 user-verified green**.
- `TemperatureLimitExceededCatalogTests`: **1 user-verified green**.
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

The focused catalog test is user-verified green. The subsequent complete workspace validation correctly exposed one owner-range error: code `1000001` was outside the former built-in `AFW` range `0–999999`.

No actual catalog definition currently uses owner `APP`; its range was only reserved. The authoritative owner catalog now preserves the thermal code and removes overlap by using:

- `AFW`: `0–1099999`;
- `APP`: `1100000–1999999`;
- `PLUGIN`: unchanged at `2000000–2999999`;
- `USER`: unchanged at `9000000–9999999`.

This owner-range repair is committed and requires validation before bootstrap templates or range documentation are synchronized.

## Focused verification

Rerun the complete catalog validation:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Expected result: **0 errors, 0 warnings, and 0 information issues**.

The focused thermal contract remains available through:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~TemperatureLimitExceededCatalogTests
```

Latest user-verified result: **1 green test**.

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

Next documentation target: after the owner-range repair validates, synchronize `DefaultJsonsTemplateProvider`, owner-range documentation, and any maintained reference catalog copies. Keep this file synchronized while the runtime/public-API audit continues.

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

Run `Setter validate .`. Do not continue with another thermal definition until the revised non-overlapping owner ranges are confirmed to accept `AFW_THM_0001`. After a green result, synchronize the bootstrap owner template and maintained range documentation.

## Last completed change

The latest slice records `TemperatureLimitExceededCatalogTests` as **1 user-verified green test** and the complete workspace validation as **1 expected owner-range error**. The error was not in the thermal definition itself: the newly reserved built-in thermal block extended beyond the former `AFW` owner boundary. Because the `APP` owner range is unused, the authoritative owner catalog now extends `AFW` through `1099999` and starts `APP` at `1100000`, preserving all existing error codes and avoiding overlap.

Commits:

```text
c4c0afd5f744404132ee3e633b26988d57114d2a
Add temperature limit exceeded catalog error

4fd02d927057a3290539627d737eb89bd2a0264b
Extend AFW owner range for thermal errors
```
