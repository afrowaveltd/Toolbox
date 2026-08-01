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

The focused catalog test is user-verified green. The complete workspace validation is also user-verified green after correcting the built-in owner ranges.

The authoritative owner catalog now uses these non-overlapping ranges:

- `AFW`: `0–1099999`;
- `APP`: `1100000–1999999`;
- `PLUGIN`: `2000000–2999999`;
- `USER`: `9000000–9999999`.

No existing error code was renumbered. The formerly reserved `APP` range had no catalog definitions, so moving its lower boundary preserved compatibility while allowing the built-in thermal block to remain `1000000–1099999`.

A new focused bootstrap contract has now been added to `DefaultJsonsTemplateProviderTests`. It parses the embedded JSON templates and requires newly initialized workspaces to receive:

- the revised `AFW` and `APP` owner boundaries;
- category `THERMAL`;
- code group `THERMAL` with prefix `THM` and range `1000000–1099999`;
- the first thermal error `AFW-THM-0001` with code `1000001`.

The production bootstrap templates have not yet been changed. The focused test should therefore establish the expected red baseline before implementation.

## Focused verification

Run the new bootstrap synchronization gate:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~DefaultJsonsTemplateProviderTests.GetTemplateFiles_ShouldIncludeThermalCatalogAndRevisedOwnerRanges
```

Expected current result: **1 red test**. The first failure should identify a stale embedded owner boundary or a missing thermal template item.

The complete catalog workspace is user-verified green:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Latest user-verified result: **0 errors, 0 warnings, and 0 information issues**.

The focused thermal catalog contract remains available through:

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

Next documentation target: after the bootstrap synchronization test is repaired and verified, synchronize owner-range documentation and maintained reference catalog copies. Keep this file synchronized while the runtime/public-API audit continues.

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

Run the focused bootstrap-template test and capture the exact red result. Do not modify `DefaultJsonsTemplateProvider` until the stale embedded contract is demonstrated. After diagnosis, update the embedded owner, category, code-group, and error templates as one tightly coupled production change, then rerun the focused test.

## Last completed change

The latest slice adds one integration-style bootstrap test without changing production. Unlike earlier string-fragment checks, the test parses the five generated templates and verifies their semantic content. It protects the revised non-overlapping owner ranges and ensures that a newly initialized workspace includes the same thermal category, code group, and first thermal error already present in the authoritative project-local catalogs.

Commits:

```text
0cfed041c4ca76fa0850b839ac3c740db28048fc
Add thermal bootstrap template contract test

2bbacfca4fae47cf73118eaaebeffe0d86cabd37
Record green thermal catalog validation
```
