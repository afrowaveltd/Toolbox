# Implementation status

Last updated: 2026-08-02

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The `AFW_THM_0012` slice is complete. The runtime/public-API audit has started with the bootstrap payload surface. Direct contract tests now protect `JsonsBootstrapPayload`, `JsonsBootstrapFileResult`, and `JsonsTemplateFile`.

The current user-verified regression baseline is fully green:

- complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 passed, 0 failed, 0 skipped**;
- complete `WhenItFails.Tests`: **708 passed, 0 failed, 0 skipped**.

## Verification status

The latest user-verified Setter test run:

```bash
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Result: **1,241 passed, 0 failed, 0 skipped**.

The latest complete core test run:

```bash
dotnet test WhenItFails.Tests
```

Result: **708 passed, 0 failed, 0 skipped**.

The completed bootstrap public-contract slice was verified with:

```bash
dotnet test WhenItFails.Tests --filter FullyQualifiedName~JsonsBootstrapPayloadContractTests
```

Result: **4 passed, 0 failed, 0 skipped**.

The contract verifies safe default values for all three bootstrap DTOs and guarantees that `JsonsBootstrapPayload.Files` is non-null, initially empty, mutable, and stable across repeated access.

Other verified gates for the completed thermal slice:

- focused `ThermalFallbackProtectionActionUnverifiedCatalogTests`: **1 passed, 0 failed, 0 skipped**;
- catalog validation after `AFW_THM_0012`: user-verified green;
- documentation-key validation: user-verified green;
- Markdown-link validation: user-verified green;
- `git diff --check`: user-verified clean.

The bundled reference catalog contains **17 categories**, **10 code groups**, **5 profiles**, and **37 bundled reference errors**. The project-local authoritative catalog additionally contains twelve thermal definitions through `AFW_THM_0012`.

## Thermal catalog state

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The current thermal contracts run from `AFW_THM_0001` through `AFW_THM_0012`. `AFW_THM_0011` represents confirmed failure of a distinct approved fallback. `AFW_THM_0012` represents a distinct approved fallback that was initiated but whose required result remains unverified. An unknown fallback outcome must not be collapsed into confirmed failure.

## Documentation synchronization completed

The English documentation is maintained in the project root and localized topic folders. Current synchronized documentation includes:

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

`WhenItFails/Docs/Thermal Errors/en.md` documents `AFW_THM_0001` through `AFW_THM_0012`.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, automatic translation generation, remote catalog synchronization, package publishing, a GUI, complete source-code dependency discovery, or automatic runtime behavior for humorous message variants.

Thermal easter eggs are explicitly deferred. Alternative wording must never alter the structured contract, severity, metadata, thresholds, control flow, shutdown decision, restart policy, sensor-trust decision, data-freshness decision, thermal-trend decision, redundancy decision, protection-action decision, action-verification decision, fallback-action decision, fallback-verification decision, or fail-safe policy.

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

Continue the runtime/public-API audit with `ErrorDescriptorRequest`, the public request object used to select an error by ID, name, or numeric code and optionally override selected descriptor fields.

Add a focused contract test first. Verify safe null defaults and exact round-trip storage of every public property. Do not change runtime implementation unless the focused test exposes an actual defect.

After the focused test passes, run the complete core suite and record the exact total.

## Last completed change

The first runtime/public-API audit slice is complete. `JsonsBootstrapPayloadContractTests` passed all **4 focused tests**, and the complete core suite then passed **708 tests, 0 failed, 0 skipped**.