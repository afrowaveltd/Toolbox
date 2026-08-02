# Implementation status

Last updated: 2026-08-02

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

Current user-verified gates:

- complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 passed, 0 failed, 0 skipped**;
- complete `WhenItFails.Tests`: **704 passed, 0 failed, 0 skipped**;
- catalog validation after `AFW_THM_0012`: user-verified green;
- Markdown-link validation: **45 Markdown files, 424 local links, 0 broken links**;
- documentation-key validation: user-verified green;
- `git diff --check`: user-verified clean.

The bundled reference catalog contains:

- **17 categories**, including `THERMAL`;
- **10 code groups**, including `THERMAL`;
- **5 profiles**;
- **37 bundled reference errors**.

## Thermal catalog state

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The first twelve thermal definitions are now present:

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
- `AFW_THM_0011` / `1000011` / `THERMALFALLBACKPROTECTIONACTIONFAILED` / `Critical`;
- `AFW_THM_0012` / `1000012` / `THERMALFALLBACKPROTECTIONACTIONUNVERIFIED` / `Critical`.

`AFW_THM_0012` applies only when an approved fallback action was selected and initiated, but trustworthy evidence cannot establish whether its required result completed. It is an indeterminate fallback outcome, not confirmed success and not confirmed failure. It deliberately does not carry the `FALLBACK_FAILED` tag.

## TDD checkpoint for AFW_THM_0012

The focused test was added first:

```bash
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ThermalFallbackProtectionActionUnverifiedCatalogTests
```

Expected red result before the catalog entry existed:

- **0 passed**;
- **1 failed**;
- failure: catalog item `THERMALFALLBACKPROTECTIONACTIONUNVERIFIED` was not found.

After adding the catalog definition, the focused test passed. The complete core suite then passed **704 tests, 0 failed, 0 skipped**.

Relevant commits:

```text
44594207369bc8d3f1a5a972049dc4b129e96c24
Add unverified thermal fallback action contract test
```

The catalog entry was committed locally and pushed to `master` with message:

```text
Add unverified thermal fallback action error
```

## Documentation status

`WhenItFails/Docs/Thermal Errors/en.md` currently documents `AFW_THM_0001` through `AFW_THM_0011`. The next required slice is to document `AFW_THM_0012` and update the choosing-the-correct-definition guidance so that unverified fallback outcome remains distinct from confirmed fallback failure.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, automatic translation generation, remote catalog synchronization, package publishing, a GUI, complete source-code dependency discovery, or automatic runtime behavior for humorous message variants.

Thermal easter eggs are explicitly deferred. Alternative wording must never alter the structured contract, severity, metadata, thresholds, control flow, shutdown decision, restart policy, sensor trust decision, data-freshness decision, thermal-trend decision, redundancy decision, protection-action decision, action-verification decision, fallback-action decision, or fail-safe policy.

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

Document `AFW_THM_0012` in `WhenItFails/Docs/Thermal Errors/en.md`.

The documentation must state that:

- the fallback was actually selected and initiated;
- its required result cannot be verified from trustworthy evidence;
- this is neither confirmed success nor confirmed failure;
- uncontrolled retry may repeat an action that already completed;
- the triggering thermal condition and primary-action result remain visible;
- restart or return to normal operation requires policy-approved evidence.

After the documentation update, run the focused documentation test if one exists, then the complete core suite, `check-doc-keys .`, `check-doc-links .`, and `git diff --check`.

## Last completed change

`AFW_THM_0012` is present on GitHub `master`, its focused TDD contract is green, catalog validation is green, and the complete core suite is user-verified green with **704 passed, 0 failed, 0 skipped**.