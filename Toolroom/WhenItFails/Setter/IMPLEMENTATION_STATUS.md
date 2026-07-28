# Implementation status

Last updated: 2026-07-28

This file is the continuation point for `Toolroom/WhenItFails/Setter` development.
Update it after every implementation or documentation change that alters the current state, completed work, known limitations, or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the WhenItFails JSON catalog workspace under `Jsons/WhenItFails`.

The current implementation supports:

- workspace initialization, validation, summary, and reference inspection;
- error listing, filtering, detail inspection, creation, removal, and focused field editing;
- tags, aliases, ownership, categories, code groups, and documentation keys;
- profile creation, selectors, mappings, metadata, and explanation output;
- safe writes with timestamped backups, backup listing, and restore operations;
- rich terminal output, plain output, JSON output, and stable exit-code conventions;
- documentation-key validation and local Markdown link checking.

## Verification status

The latest user-verified Setter test run is green with 1,226 tests after the implementation-status test was made resilient.

The current catalog-review documentation change adds one documentation-contract test, so the next successful focused run is expected to report 1,227 tests.
Do not mark that count as user-verified until the run is confirmed green.

Primary verification command:

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

## Documentation synchronization completed

The following high-level Setter documents have been synchronized with the current command surface and protected by documentation tests:

- `README.md`;
- `Readme/en.md`;
- `Docs/Commands/en.md`;
- `Docs/Command Quick Reference/en.md`;
- `Docs/Known Limitations/en.md`;
- `Docs/Roadmap and Future Work/en.md`;
- `Docs/Getting-Started/en.md`;
- `Docs/FAQ/en.md`;
- `Docs/Testing and CI/en.md`;
- `Docs/Reviewing Catalog Changes/en.md`.

The synchronized documentation no longer presents implemented capabilities such as `add-error`, `remove-error`, `next-code`, `restore-backup`, JSON output, profile explanation, or documentation checks as missing or future work.

The Testing and CI guide documents focused and repository-wide runs, service and command contracts, temporary workspaces, persistence and backup invariants, rich/plain/JSON output, exit codes, documentation checks, failure diagnosis, and the immediate one-change/one-test rule.

The catalog review guide now provides a practical review gate for scope, working-tree inspection, validation, reference checks, profiles and mappings, documentation checks, output contracts, safe-write invariants, focused tests, and the rule that red changes are not approved.

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
- Run the Setter test project after every commit-sized change.
- Do not advance to another area until the current tests are green.
- Keep README and localized English documentation aligned with actual behavior.
- Update this file after every change so another session can continue without reconstructing project history.

## Recommended next step

First verify the current focused Setter run and record whether the expected 1,227 tests are green.

Next documentation target: `Docs/Catalog Author Checklist/en.md` with one corresponding documentation-contract test.

That guide should align the author workflow with `reference`, `next-code`, `suggest-doc-key`, `add-error`, focused edits, `details`, `error-references`, profile explanation, documentation checks, focused tests, Git diff review, and implementation-status maintenance.

After the remaining high-value documentation is synchronized, begin a runtime/public-API audit of WhenItFails integration points, mappings, and profile behavior.

## Last completed change

`Docs/Reviewing Catalog Changes/en.md` was replaced with a concise current review workflow, and `ReviewingCatalogChangesDocumentationTests.cs` now protects its validation gate, reference checks, profile explanation, documentation checks, output contracts, backup review, focused-commit rule, and red-change stop rule.

Commits in this change sequence:

```text
dc39a8d75e5de6957fa0691a4c0ab3ba342ed8ff
Add catalog review documentation contract

fdb5fa062885ebb17922991e883e142757fdfc5e
Refresh catalog change review guide
```
