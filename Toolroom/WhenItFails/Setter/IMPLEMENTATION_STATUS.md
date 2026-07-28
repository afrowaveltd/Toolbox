# Implementation status

Last updated: 2026-07-28

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the WhenItFails JSON catalog workspace under `Jsons/WhenItFails`.

Implemented areas include:

- workspace initialization, validation, summary, and reference inspection;
- error listing, filtering, detail inspection, creation, removal, and focused field editing;
- tags, aliases, ownership, categories, code groups, and documentation keys;
- profile creation, selectors, mappings, metadata, and explanation output;
- safe writes with timestamped backups, backup listing, and restore operations;
- rich, plain, and JSON output with stable exit-code conventions;
- documentation-key validation and local Markdown link checking.

## Verification status

- Setter: **1,241 tests user-verified green** after normalized `errors --profile` and `show-profile --json` command-boundary contracts.
- Runtime `ErrorProfileSelectionServiceTests`: **10 focused tests user-verified green**.
- Runtime `ErrorCatalogRuntimeProfileDisplayNameTests`: **1 focused test user-verified green**.
- Runtime `ErrorProfileResolverTests`: **19 focused tests user-verified green**, including mapping-selection invariance.
- Runtime `ErrorProfileCatalogProviderMetadataTests`: **1 focused test user-verified green**.
- Runtime `ErrorCatalogContextProfileMetadataIntegrationTests`: **1 focused test user-verified green**, including writer/loader round-trip, provider/context preservation, metadata values, and normalized mappings.
- Runtime `ErrorProfileDefinitionNormalizerTests`: **10 focused tests user-verified green** after independent metadata, mapping, and selector-list copy semantics.
- Runtime `ErrorProfileCatalogDocumentNormalizerTests`: **7 focused tests user-verified green** after independent catalog metadata, profile collection, and profile instance copy semantics.
- Current catalog-tag ownership slice adds one focused document-normalizer test, so the next successful focused run is expected to report **8 tests**.

Focused verification command for the current slice:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ErrorProfileCatalogDocumentNormalizerTests
```

Primary Setter verification command:

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

## Documentation synchronization

High-value Setter documentation synchronization is complete. The maintained English documentation covers the implemented command surface, architecture, testing, safe writes, backups, automation, contribution workflow, and maintainer continuation rules.

Primary synchronized documents include:

- `README.md` and `Readme/en.md`;
- `Docs/Overview/en.md`;
- `Docs/Commands/en.md` and `Docs/Command Quick Reference/en.md`;
- `Docs/Testing and CI/en.md`;
- `Docs/Safe Writes/en.md` and `Docs/Backups and Recovery/en.md`;
- `Docs/Architecture Overview/en.md`;
- `Docs/Contributing to Setter/en.md`;
- `Docs/Maintainer Notes/en.md`.

## Runtime/public-API audit

Completed audit slices:

1. Runtime profile lookup accepts normalized `Name` or `DisplayName`, aligned with Setter.
2. Setter `errors --profile`, `show-profile`, and `explain-profile` share one normalized lookup path.
3. Command-level JSON contracts protect normalized selectors, exit codes, envelopes, failure fields, and no-backup behavior.
4. Profile explanation reports include matches and final exclusion vetoes consistently with runtime selection.
5. `DefaultMappings` remain consumer recommendations and do not affect `ErrorProfileResolver` selection.
6. Profile metadata survives JSON load, normalization, validation, provider payload creation, safe writer/loader round-trip, and final `ErrorCatalogContext` construction.
7. `MetadataBag` keys are case-insensitive only; separator normalization is intentionally not applied.
8. `ErrorProfileDefinitionNormalizer` creates an independent `MetadataBag` copy.
9. `DefaultMappings` are independently copied and normalized.
10. All profile selector lists are independently copied and normalized.
11. `ErrorProfileCatalogDocumentNormalizer` copies catalog metadata instead of sharing mutable state with the source document.
12. The normalized `Profiles` collection and each normalized profile instance are independent from the source catalog.
13. The current focused contract verifies the same mutation isolation for catalog `Tags`, while preserving normalized and de-duplicated values.

## Current intentional boundaries

Setter currently does not provide:

- automatic schema migration;
- a multi-file atomic transaction;
- a multi-process locking contract;
- automatic backup-retention cleanup;
- complete localization lifecycle management;
- remote catalog synchronization;
- package publishing;
- a GUI or interactive TUI;
- complete source-code dependency discovery;
- a full security audit or semantic duplicate detector.

These are boundaries or future candidates, not undocumented defects.

## Working rules

- GitHub `master` is the source of truth.
- Keep each change narrow and intentional.
- Add or update tests immediately with each implementation or documentation change.
- Run the affected focused test project after every commit-sized change.
- Do not advance while the current test slice is red.
- Keep README and `Docs/<topic>/en.md` aligned with actual behavior.
- Update this file after every change.

## Recommended next step

First verify all focused `ErrorProfileCatalogDocumentNormalizerTests`; the expected count is **8 green tests**.

If green, the profile-catalog ownership audit is complete. Continue with one adjacent runtime/public-API boundary only after inspecting current provider and context source plus existing tests; prefer a concrete externally observable behavior over broad refactoring.

Do not invent automatic `DefaultMappings` consumption.

## Last completed change

The sixteenth runtime/public-API audit slice added an explicit mutation-isolation contract for catalog tags. The implementation already creates a normalized list copy through `NormalizeStringList`, so no production change was required.

Commits in this change sequence:

```text
c3571f5fb71fcd984eb455ef099e9994e9d622e0
Protect profile catalog tag copy isolation
```
