# Implementation status

Last updated: 2026-08-01

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **1,241 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

The complete `WhenItFails.Tests` core suite is user-verified green with **694 passed, 0 failed, 0 skipped**.

The runtime/public-API audit has verified defensive handling of provider failures, null tasks, null responses, null payloads, runtime-null issue collections and elements, cross-validation envelopes, successful-provider diagnostic aggregation, status normalization, and required inner members of `ErrorCatalogProviderPayload`.

## Verification status

Recently verified focused contracts include:

- `TemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- `DefaultJsonsTemplateProviderTests.GetTemplateFiles_ShouldIncludeThermalCatalogAndRevisedOwnerRanges`: **1 user-verified green**.
- `CriticalTemperatureLimitExceededCatalogTests`: **1 user-verified green**.
- Complete catalog validation after adding `AFW_THM_0002`: **0 errors, 0 warnings, 0 information issues**.
- Complete `WhenItFails.Tests`: **694 user-verified green, 0 failed, 0 skipped**.
- Complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 user-verified green, 0 failed, 0 skipped**.

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The first thermal definition remains unchanged and verified:

- ID `AFW_THM_0001`;
- code `1000001`;
- name `TEMPERATURELIMITEXCEEDED`;
- default severity `Warning`;
- documentation key `when-it-fails/errors/thermal/temperature-limit-exceeded`.

The second thermal definition is complete and verified:

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

Because bootstrap templates are generated from embedded authoritative catalogs, both definitions flow into newly initialized workspaces after the project rebuilds. Existing project-local catalogs remain untouched by bootstrap.

The authoritative owner catalog continues to use non-overlapping ranges:

- `AFW`: `0–1099999`;
- `APP`: `1100000–1999999`;
- `PLUGIN`: `2000000–2999999`;
- `USER`: `9000000–9999999`.

## Focused verification

The complete core project is user-verified green:

```powershell
dotnet test WhenItFails.Tests
```

Latest user-verified result: **694 passed, 0 failed, 0 skipped**.

The critical thermal contract is user-verified green:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~CriticalTemperatureLimitExceededCatalogTests
```

Latest user-verified result: **1 passed, 0 failed, 0 skipped**.

The complete catalog workspace is user-verified green after adding the critical definition:

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

`WhenItFails/Docs/Thermal Errors/en.md` documents both thermal contracts and explicitly distinguishes safe-limit warnings from critical shutdown-limit states. It also records that the catalog describes the condition but does not itself execute shutdown or restart actions.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, automatic translation generation, remote catalog synchronization, package publishing, a GUI, complete source-code dependency discovery, or automatic runtime behavior for humorous message variants.

Thermal easter eggs are explicitly deferred. Any future absurd-temperature wording must never alter the structured contract, severity, metadata, thresholds, control flow, shutdown decision, or restart policy.

## Working rules

- GitHub `master` is the source of truth.
- Keep each change narrow and intentional.
- Add or update tests immediately.
- Do not advance while the current slice is red.
- Update this file after every change.
- Prefer one file plus its directly related test, then commit.

## Recommended next step

Add a focused red catalog contract for a third thermal condition representing an invalid or untrustworthy sensor reading. Keep it separate from both real temperature-limit conditions because an application must not make safety decisions from data it cannot trust.

## Last completed change

The `AFW_THM_0002` increment is closed. The focused contract, full catalog validation, documentation, and complete core regression suite are all user-verified green. The current core baseline is **694 passed, 0 failed, 0 skipped**.

Commits:

```text
b539dffc0c165c7c92b3b3212b536d12a4c0c34f
Add critical temperature limit catalog contract

11cad246ad67bcbaac8a9c3cdb002c8993024308
Add critical temperature limit exceeded catalog error

37fd76e34b2319c5013e837914c3cb6968bc9d1b
Record green critical thermal validation

8b23c8f2e8bc8de7bf9657e885ee392603817956
Document critical thermal shutdown limit

a79469e5bac28d879618e73d2ca75dfa79666556
Update critical thermal implementation status
```