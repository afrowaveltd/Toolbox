# Implementation status

Last updated: 2026-08-02

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The current repository checkpoint is user-verified green:

- complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 passed, 0 failed, 0 skipped**;
- complete `WhenItFails.Tests`: **703 passed, 0 failed, 0 skipped**;
- catalog validation: **0 errors, 0 warnings, 0 information issues**;
- Markdown-link validation: **45 Markdown files, 424 local links, 0 broken links**;
- documentation-key validation: user-verified green;
- `git diff --check`: user-verified clean.

The bundled reference catalog currently contains:

- **17 categories**, including `THERMAL`;
- **10 code groups**, including `THERMAL`;
- **5 profiles**;
- **37 bundled reference errors**.

The reference summary tests assert both the updated THERMAL counts and the presence of the THERMAL category and code group.

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

`AFW_THM_0011` requires evidence that an approved, distinct fallback action was selected, attempted, and failed. It is not emitted merely because a fallback existed or was considered.

## Latest regression repair

The THERMAL increment initially left stale Setter expectations:

- category count `16` instead of `17` in object and JSON reference summaries;
- code-group count `9` instead of `10` in object and JSON reference summaries;
- obsolete implementation-status literal `Next documentation target:`.

The expectations were corrected, explicit THERMAL presence assertions were added, and the complete Setter suite then passed all **1,241** tests.

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

bfe3d278ab47130ea96a664b18e69c67021f045e
Record green Setter regression checkpoint
```

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

Implement the twelfth thermal contract:

- ID: `AFW_THM_0012`;
- code: `1000012`;
- name: `THERMALFALLBACKPROTECTIONACTIONUNVERIFIED`;
- default severity: `Critical`.

Use it only when an approved fallback action was selected and initiated, but trustworthy evidence cannot establish whether its required result completed. This is an indeterminate fallback outcome, not confirmed success and not confirmed failure. It must remain distinct from `AFW_THM_0011`.

Implementation order:

1. add a focused catalog contract test;
2. add the catalog definition and embedded bootstrap copy;
3. run the focused test and catalog validation;
4. update `WhenItFails/Docs/Thermal Errors/en.md`;
5. run the complete core and Setter suites;
6. update this file with exact results and commit each verified slice.

## Last completed change

The repository-wide hygiene checkpoint is user-verified clean: documentation keys pass and `git diff --check` reports no whitespace errors. The next thermal contract was selected from an actual missing operational state: an attempted fallback whose result cannot be verified.