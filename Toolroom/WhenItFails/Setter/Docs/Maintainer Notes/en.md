# Maintainer Notes

This guide is the continuation and operating manual for maintainers of `Toolroom/WhenItFails/Setter`.

Setter should remain explicit, testable, safe, and easy to continue after an interruption.

> GitHub `master` is the source of truth.

## Start from the recorded state

Before changing anything:

1. fetch the current repository state;
2. read `Toolroom/WhenItFails/Setter/IMPLEMENTATION_STATUS.md`;
3. inspect the working tree;
4. run the focused Setter suite;
5. confirm the next recommended step still matches the repository.

```powershell
git status --short
git diff --check
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Do not reconstruct project state from memory when GitHub and `IMPLEMENTATION_STATUS.md` can answer it directly.

## One small green step at a time

Use this maintenance rhythm:

```text
inspect
→ choose one narrow change
→ add or update its test immediately
→ implement or document the change
→ run focused verification
→ update documentation and status
→ commit
→ stop until green
```

One small green step at a time is safer than accumulating several unverified changes.

> Do not continue while the focused Setter suite is red.

## What the continuation status must record

`IMPLEMENTATION_STATUS.md` must remain sufficient for another maintainer to continue without reconstructing history.

Record:

- the latest user-verified focused test result;
- the change completed in the current slice;
- synchronized documentation topics;
- current intentional boundaries;
- the exact next recommended step;
- the commits that completed the latest slice.

Do not mark an expected test count as verified until the run has actually been confirmed green.

## Architectural boundaries

Maintain the intended dependency direction:

```text
entry point
→ command dispatch
→ command workflow
→ reusable services
→ workspace and catalog models
→ loaders, validators, writers
```

Rendering sits beside the command workflow and consumes results. Views must not own catalog rules. Validators must not render or write files. Writers must not decide command semantics.

Expected failures should use structured responses or issues. Unexpected failures may reach the top-level handler and use exit code `3`.

## Command contracts

Treat commands as public interfaces.

A command change may affect:

- canonical name and aliases;
- arguments and options;
- issue codes;
- exit codes;
- rich output;
- plain output;
- `--json` output;
- help and command-reference documentation;
- persistence or backup behavior.

Machine consumers should use exit codes and `--json`. They must not parse rich terminal rendering as a stable schema.

## Exit-code model

Preserve the broad process classification unless a deliberate compatibility change is made:

| Exit code | Meaning |
| --- | --- |
| `0` | successful command completion |
| `1` | invalid command usage or arguments |
| `2` | expected workspace, validation, lookup, editing, persistence, or restore failure |
| `3` | unexpected top-level failure |

Issue codes provide the more specific reason. Tests should assert them where they are part of the contract.

## Workspace and validation

The default catalog package is under:

```text
Jsons/WhenItFails
```

Workspace operations must remain predictable for both project-root and direct-package paths where the command supports them.

Validate before presenting derived workspace information or persisting changes. Do not silently repair, migrate, or reinterpret invalid data unless that behavior is explicitly designed, tested, and documented.

## Safe persistence

Setter write and restore operations are single-file operations. They are not multi-file transactions and they are not a multi-process locking system.

Maintain these invariants:

### Successful write

- the intended target is selected explicitly;
- the resulting data is valid before replacement;
- a complete temporary file is written first;
- an existing target receives a timestamped backup;
- the persisted result is reloaded in tests;
- complete-workspace validation succeeds afterward.

### Rejected write

- the target file remains unchanged;
- no backup is created;
- the failure is represented by the intended response or issue code.

Do not introduce direct truncation or unprotected replacement of active catalog files.

## Backup and recovery maintenance

Current recovery commands include:

- `list-backups` for discovery;
- `restore-backup` for explicit restoration.

A backup must be selected by content and intended target, not merely because it is newest.

After restoration:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
git status --short
git diff --check
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Inspect affected errors, profiles, mappings, and references before accepting the restore.

Backup retention cleanup is not automatic. Generated `.bak.json` files are local recovery artifacts and should not normally be committed.

## Profiles and mappings

Profiles, selectors, mappings, and metadata are implemented authoring surfaces.

When changing them:

- inspect the profile definition;
- use `explain-profile` to review effective selection and diagnostics;
- verify include and exclude selectors;
- verify default and explicit mappings;
- test success and invalid-reference cases;
- keep documentation aligned with actual runtime and Setter behavior.

Do not describe implemented profile editing or explanation as future work.

## Documentation maintenance

Setter documentation has two required levels:

1. `README.md` for the project overview;
2. `Docs/<topic>/en.md` for detailed English documentation.

Only `en.md` is authored manually at this stage. Other localizations will be generated later.

When behavior changes, update the focused guide, command references, help, and README links where relevant. Add or update a documentation-contract test for important claims.

For documentation keys and Markdown links, run:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
```

Do not present implemented commands such as `restore-backup`, JSON output, or profile editing as future possibilities.

## Test maintenance

Tests should protect behavior, not accidental formatting noise.

Use isolated temporary workspaces for mutable operations. Never modify the repository's real catalog workspace from a test.

For write operations, test the response, persisted value, backup creation, post-write validation, and rejected-write invariants.

For output behavior:

- rich tests may cover deliberate layout contracts;
- plain tests should assert meaningful human-readable content;
- JSON tests should parse and assert structure;
- command tests should verify exit codes and issue codes.

Run the focused project after every commit-sized Setter change:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Run broader repository tests when shared libraries or public cross-project contracts change.

## Cross-platform maintenance

Setter is developed for Windows and Linux workflows.

Review:

- path separators and casing;
- paths containing spaces;
- permissions and file locking;
- temporary-directory behavior;
- Bash `$?` and PowerShell `$LASTEXITCODE`;
- shell quoting and line continuation;
- newline differences in text assertions.

Fix the actual platform assumption rather than weakening a correct contract to hide it.

## Review before commit

Before every commit:

```powershell
git status --short
git diff --check
git diff
git diff --cached
```

Confirm that:

- one logical change is present;
- implementation, tests, and documentation agree;
- no backup or temporary files are staged;
- `IMPLEMENTATION_STATUS.md` is current;
- the focused Setter suite is green;
- the actual diff has been read.

## What not to guess

Do not guess:

- whether a command exists;
- whether a capability is current or future;
- the current test count;
- the correct next task;
- the exact catalog relationship;
- whether a backup is safe to restore;
- whether a public contract is unused.

Inspect GitHub, source, tests, documentation, and status instead.

## Completion rule

A maintenance slice is complete only when:

1. the intended behavior or documentation is finished;
2. the corresponding test exists;
3. focused verification is green;
4. documentation agrees with the implementation;
5. `IMPLEMENTATION_STATUS.md` records the new continuation point;
6. the logical change is committed.

## Related documentation

- [Architecture Overview](../Architecture%20Overview/en.md)
- [Contributing to Setter](../Contributing%20to%20Setter/en.md)
- [Adding a New Command](../Adding%20a%20New%20Command/en.md)
- [Testing and CI](../Testing%20and%20CI/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
- [Known Limitations](../Known%20Limitations/en.md)

## Central principle

> Maintain the repository so the next person can trust the source, verify the state, complete one small green step, and continue without guesswork.
