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

The latest user-verified Setter test run is green with 1,233 tests after the architecture-overview documentation synchronization.

The current new-command documentation change adds one documentation-contract test, so the next successful focused run is expected to report 1,234 tests.
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
- `Docs/Reviewing Catalog Changes/en.md`;
- `Docs/Catalog Author Checklist/en.md`;
- `Docs/Safe Writes/en.md`;
- `Docs/Backups and Recovery/en.md`;
- `Docs/Exit Codes and Automation/en.md`;
- `Docs/Contributing to Setter/en.md`;
- `Docs/Architecture Overview/en.md`;
- `Docs/Adding a New Command/en.md`.

The synchronized documentation no longer presents implemented capabilities such as `add-error`, `remove-error`, `next-code`, `restore-backup`, JSON output, profile explanation, or documentation checks as missing or future work.

The Testing and CI guide documents focused and repository-wide runs, service and command contracts, temporary workspaces, persistence and backup invariants, rich/plain/JSON output, exit codes, documentation checks, failure diagnosis, and the immediate one-change/one-test rule.

The catalog review guide provides a practical review gate for scope, working-tree inspection, validation, reference checks, profiles and mappings, documentation checks, output contracts, safe-write invariants, focused tests, and the rule that red changes are not approved.

The catalog author checklist follows the current Setter workflow: inspect reference catalogs, prepare codes and documentation keys, create or edit entries with explicit commands, inspect references and profile behavior, validate documentation, review backups and diffs, run tests immediately, and update this continuation file.

The safe-writes guide describes the current single-file persistence contract across catalog targets: validation before replacement, temporary-file serialization, timestamped backups, success and rejection invariants, structured failure handling, concurrency boundaries, backup listing, explicit restoration, post-write inspection, and focused tests.

The backups-and-recovery guide uses the implemented `list-backups` and `restore-backup` workflow, requires content-based backup selection, complete-workspace validation, affected-contract inspection, Git diff review, focused tests, and a stop rule after unverified restoration.

The exit-codes-and-automation guide documents the stable `0`/`1`/`2`/`3` process classification, the distinction between exit and issue codes, JSON-first machine integration, plain versus rich output, Bash and PowerShell capture patterns, pipeline safety, write and restore verification, and the rule that unexplained failures must not be converted into green automation.

The contribution guide consolidates the current repository workflow: GitHub `master` as the source of truth, one logical change per commit, immediate corresponding tests, focused verification, README plus `Docs/<topic>/en.md` documentation, mandatory implementation-status maintenance, safe-write and restore review, cross-platform checks, and the red-suite stop rule.

The architecture overview maps the actual entry-point, command, service, workspace-model, validation, persistence, recovery, rendering, and testing boundaries. It records the intended dependency direction and the distinction between rich, plain, and JSON output without presenting implemented capabilities as future work.

The new-command guide now defines the complete command lifecycle: contract design, dispatch, command and service responsibilities, workspace validation, read and write flows, safe persistence, rich/plain/JSON surfaces, exit and issue codes, documentation updates, cross-platform review, and immediate focused tests.

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

First verify the current focused Setter run and record whether the expected 1,234 tests are green.

Next documentation target: `Docs/Maintainer Notes/en.md` with one corresponding documentation-contract test.

That guide should summarize the source-of-truth rule, continuation status, focused green-step workflow, architecture boundaries, documentation synchronization, safe persistence, automation contracts, and recovery expectations without duplicating every specialized guide.

After the remaining high-value documentation is synchronized, begin a runtime/public-API audit of WhenItFails integration points, mappings, and profile behavior.

## Last completed change

`Docs/Adding a New Command/en.md` was replaced with a current command-authoring workflow, and `AddingANewCommandDocumentationTests.cs` now protects dispatch, service separation, command-input failures, rich/plain/JSON output, exit and issue codes, focused verification, required documentation updates, status maintenance, and the red-suite stop rule.

Commits in this change sequence:

```text
ae24252ee4078f51754723ce7013998c6b55a450
Add new command documentation contract

b80a68630806dc9116e8e64f34f41bb9e7fadf61
Refresh new command authoring guide
```
