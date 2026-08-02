# Implementation status

Last updated: 2026-08-02

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **1,238 passed, 3 failed, 0 skipped, 1,241 total**. The three failures were traced to stale test/documentation expectations after the THERMAL catalog increment:

- `ImplementationStatusDocumentationTests.Documentation_ProvidesCurrentContinuationPoint` required the obsolete literal `Next documentation target:` even though the status already exposes `## Recommended next step`;
- `ReferenceCommandTests.SummarizeAsync_WithBundledReferenceCatalog_ReturnsExpectedCounts` still expected 16 categories;
- `ReferenceCommandJsonTests.ExecuteAsync_WithJsonListSubcommand_WritesStableEnvelope` still expected 16 categories.

All three expectations are corrected on `master` and now await a user-verified rerun.

The complete `WhenItFails.Tests` core suite remains user-verified green with **703 passed, 0 failed, 0 skipped** after adding and documenting `AFW_THM_0011`.

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
- `CriticalTemperatureBelowMinimumLimitCatalogTests`: **1 user-verified green**.
- `ThermalProtectionActionFailedCatalogTests`: **1 user-verified green**.
- `ThermalProtectionActionUnverifiedCatalogTests`: **1 user-verified green**.
- `ThermalFallbackProtectionActionFailedCatalogTests`: **1 user-verified green**.
- `DefaultJsonsTemplateProviderTests.GetTemplateFiles_ShouldIncludeThermalCatalogAndRevisedOwnerRanges`: **1 user-verified green**.
- Complete catalog validation after adding `AFW_THM_0011`: **0 errors, 0 warnings, 0 information issues**.
- Complete `WhenItFails.Tests`: **703 user-verified green, 0 failed, 0 skipped**.
- Repository Markdown-link check: **45 files, 424 local links, 0 broken links** after pulling the catalog-author-checklist fix.
- Complete `Toolroom/WhenItFails/Setter.Tests`: latest run **1,238 passed, 3 failed, 0 skipped**; fixes are committed and rerun is pending.

The THERMAL increment raised the bundled reference catalog to **17 categories**. The reference summary tests now assert both the count and presence of category `THERMAL`.

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The first eleven thermal definitions are complete and core-regression verified:

- `AFW_THM_0001` / `1000001` / `TEMPERATURELIMITEXCEEDED` / `Warning`;
- `AFW_THM_0002` / `1000002` / `CRITICALTEMPERATURELIMITEXCEEDED` / `Critical`;
- `AFW_THM_0003` / `1000003` / `TEMPERATURESENSORREADINGINVALID` / `Error`;
- `AFW_THM_0004` / `1000004` / `TEMPERATUREREADINGSTALE` / `Error`;
- `AFW_THM_0005` / `1000005` / `TEMPERATURERATEOFCHANGEEXCEEDED` / `Warning`;
- `AFW_THM_0006` / `1000006` / `TEMPERATURESENSORDISAGREEMENT` / `Warning`;
- `AFW_THM_0007` / `1000007` / `TEMPERATUREBELOWMINIMUMLIMIT` / `Warning`;
- `AFW_THM_0008` / `1000008` / `CRITICALTEMPERATUREBELOWMINIMUMLIMIT` / `Critical`;
- `AFW_THM_0009` / `1000009` / `THERMALPROTECTIONACTIONFAILED` / `Critical`;
- `AFW_THM_0010` / `1000010` / `THERMALPROTECTIONACTIONUNVERIFIED` / `Critical`;
- `AFW_THM_0011` / `1000011` / `THERMALFALLBACKPROTECTIONACTIONFAILED` / `Critical`.

## Focused verification

The fallback-action contract is user-verified green:

```bash
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ThermalFallbackProtectionActionFailedCatalogTests
```

Latest user-verified result: **1 passed, 0 failed, 0 skipped**.

The complete catalog workspace is user-verified green after adding `AFW_THM_0011`:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Latest user-verified result: **0 errors, 0 warnings, and 0 information issues**.

The complete core project is user-verified green:

```bash
dotnet test WhenItFails.Tests
```

Latest user-verified result: **703 passed, 0 failed, 0 skipped**.

The local Markdown-link checker is user-verified green after the corrected catalog-author-checklist link:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
```

Latest user-verified result: **45 Markdown files checked, 424 local links checked, 0 broken links**.

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

`Docs/Catalog Author Checklist/en.md` links to the actual `Docs/Checking Documentation Keys/en.md` topic instead of the obsolete `Docs/Documentation Keys/en.md` path.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, automatic translation generation, remote catalog synchronization, package publishing, a GUI, complete source-code dependency discovery, or automatic runtime behavior for humorous message variants.

Thermal easter eggs are explicitly deferred. Any future alternative wording must never alter the structured contract, severity, metadata, thresholds, control flow, shutdown decision, restart policy, sensor trust decision, data-freshness decision, thermal-trend decision, redundancy decision, low-temperature decision, critical low-temperature decision, protection-action decision, action-verification decision, fallback-action decision, or fail-safe policy.

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

Pull `master` and rerun the complete Setter suite:

```bash
dotnet test Toolroom/WhenItFails/Setter.Tests
```

If it is green, update this file with the exact passed/failed/skipped totals and commit that verified checkpoint. Then run `check-doc-keys .` and `git diff --check` if their latest results have not already been captured. Do not add a twelfth thermal definition while the Setter suite is red or unverified.

## Last completed change

The stale Setter regression expectations were corrected:

- reference summary category count changed from 16 to 17;
- JSON reference summary category count changed from 16 to 17;
- the object-summary test now explicitly verifies category `THERMAL`;
- the implementation-status documentation contract no longer requires the obsolete `Next documentation target:` literal and instead relies on the existing `## Recommended next step` continuation section.

Commits:

```text
9d61bc8c761c57be670e45bbc7fa0699d13ac071
Update reference category count expectation

ee912570d584442758a786abd17b92938a5d58aa
Update JSON reference category count expectation

85dfe9433615b9782a45660b55857a4a4c297d06
Align implementation status continuation contract
```
