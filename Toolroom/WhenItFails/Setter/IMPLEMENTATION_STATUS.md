# Implementation status

Last updated: 2026-08-02

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The latest user-verified complete Setter run is green:

- **1,241 passed**;
- **0 failed**;
- **0 skipped**;
- **1,241 total**.

The green run was completed after synchronizing the bundled reference expectations with the THERMAL catalog increment and removing one obsolete implementation-status test literal.

The complete `WhenItFails.Tests` core suite remains user-verified green with **703 passed, 0 failed, 0 skipped** after adding and documenting `AFW_THM_0011`.

The runtime/public-API audit has verified defensive handling of provider failures, null tasks, null responses, null payloads, runtime-null issue collections and elements, cross-validation envelopes, successful-provider diagnostic aggregation, status normalization, and required inner members of `ErrorCatalogProviderPayload`.

## Verification status

Current user-verified gates:

- Complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 passed, 0 failed, 0 skipped**.
- Complete `WhenItFails.Tests`: **703 passed, 0 failed, 0 skipped**.
- Complete catalog validation after adding `AFW_THM_0011`: **0 errors, 0 warnings, 0 information issues**.
- Repository Markdown-link check: **45 Markdown files, 424 local links, 0 broken links**.
- `ThermalFallbackProtectionActionFailedCatalogTests`: **1 passed, 0 failed, 0 skipped**.
- `DefaultJsonsTemplateProviderTests.GetTemplateFiles_ShouldIncludeThermalCatalogAndRevisedOwnerRanges`: user-verified green.

The THERMAL increment raised the bundled reference catalog to:

- **17 categories**, including `THERMAL`;
- **10 code groups**, including `THERMAL`;
- **5 profiles**;
- **37 bundled reference errors**.

The reference summary tests now assert both the updated counts and the presence of the THERMAL category and code group.

## Thermal catalog state

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

`THERMALFALLBACKPROTECTIONACTIONFAILED` represents confirmed failure of an approved fallback response after the primary thermal protection action failed or remained unverified. It must not be emitted merely because a fallback exists or was considered; runtime evidence must show that the selected fallback was attempted and failed.

Because bootstrap templates are generated from embedded authoritative catalogs, all eleven thermal definitions flow into newly initialized workspaces after rebuild. Existing project-local catalogs remain untouched by bootstrap.

The authoritative owner catalog continues to use non-overlapping ranges:

- `AFW`: `0–1099999`;
- `APP`: `1100000–1999999`;
- `PLUGIN`: `2000000–2999999`;
- `USER`: `9000000–9999999`.

## Focused verification commands

Complete Setter suite:

```bash
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Latest user-verified result: **1,241 passed, 0 failed, 0 skipped**.

Complete core suite:

```bash
dotnet test WhenItFails.Tests
```

Latest user-verified result: **703 passed, 0 failed, 0 skipped**.

Catalog validation:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Latest user-verified result: **0 errors, 0 warnings, and 0 information issues**.

Markdown-link validation:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
```

Latest user-verified result: **45 Markdown files checked, 424 local links checked, 0 broken links**.

The documentation-key gate and `git diff --check` remain the final repository hygiene gates to reconfirm after the latest status and regression-test commits.

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
- `Docs/Catalog Author Checklist/en.md`;
- `WhenItFails/Docs/Bootstrap/en.md`;
- `WhenItFails/Docs/Thermal Errors/en.md`.

`Docs/Catalog Author Checklist/en.md` links to the actual `Docs/Checking Documentation Keys/en.md` topic instead of the obsolete `Docs/Documentation Keys/en.md` path.

`WhenItFails/Docs/Thermal Errors/en.md` documents all eleven current thermal contracts.

## Latest regression repair

The first complete Setter rerun after the THERMAL increment reported three failures:

- the implementation-status test required obsolete literal `Next documentation target:`;
- the object reference summary expected 16 categories instead of 17;
- the JSON reference summary expected 16 categories instead of 17.

After those corrections, the next run exposed two remaining stale expectations:

- the object reference summary expected 9 code groups instead of 10;
- the JSON reference summary expected 9 code groups instead of 10.

The final corrections changed both code-group expectations to 10 and added explicit object-summary verification for code group `THERMAL`. The complete Setter suite then passed all 1,241 tests.

Relevant commits:

```text
9d61bc8c761c57be670e45bbc7fa0699d13ac071
Update reference category count expectation

ee912570d584442758a786abd17b92938a5d58aa
Update JSON reference category count expectation

85dfe9433615b9782a45660b55857a4a4c297d06
Align implementation status continuation contract

e067060d52089307ce83dc1b025613bfcd192b6d
Update reference code-group count expectation

f3b23d705d86719a82e70f5d23e41213194cb9f5
Update JSON reference code-group count expectation
```

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

Pull the green-checkpoint commit and run the remaining repository hygiene gates:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
git diff --check
```

If both are green, record their exact results here and commit the final clean repository checkpoint. Only then select the next small implementation slice. The likely next catalog slice is a twelfth thermal definition, but its contract must be chosen from an actual missing operational state rather than added merely to increase the count.

## Last completed change

The complete Setter suite is user-verified green with **1,241 passed, 0 failed, 0 skipped** after synchronizing all THERMAL category and code-group reference expectations. This status file now records the final green checkpoint and the remaining hygiene gates.