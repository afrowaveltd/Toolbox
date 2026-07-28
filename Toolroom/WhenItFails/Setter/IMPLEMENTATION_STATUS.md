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
- Current document-metadata ownership slice changes `ErrorProfileCatalogDocumentNormalizer` to copy the catalog `MetadataBag`. The focused document-normalizer suite is not yet user-verified.

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
11. The current document-level ownership slice changes `ErrorProfileCatalogDocumentNormalizer` to copy catalog metadata instead of sharing a mutable `MetadataBag` with the source document.

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

First verify all focused `ErrorProfileCatalogDocumentNormalizerTests`.

If green, record the result and continue with one separate document-level ownership contract for the `Profiles` list and normalized profile instances. Do not combine that verification with unrelated catalog behavior.

Do not invent automatic `DefaultMappings` consumption. Inspect current source and tests before implementation.

## Last completed change

The fourteenth runtime/public-API audit slice changed profile-catalog metadata normalization from shared mutable reference semantics to an independent copy. The focused contract verifies metadata value preservation and mutation isolation between the source catalog and normalized catalog.

Commits in this change sequence:

```text
9aa08e11a2f69a3613c967ae434a6ed6e22a4a6c
Require independent normalized profile catalog metadata

5c2a2cc4a2bc31d17e7b60ff144a1cfa19695b73
Copy profile catalog metadata during normalization
```
