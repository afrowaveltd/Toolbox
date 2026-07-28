# Safe Writes

Setter write commands use a conservative single-file persistence workflow for WhenItFails catalog data.

The goal is simple:

> Validate before replacement, preserve the previous file, and report failures explicitly.

Safe write reduces the chance of a damaged active catalog. It does not turn several catalog files into one database transaction.

## Which commands use safe writes

Safe-write behavior applies to commands that create, remove, or modify catalog-backed data, including error definitions, focused error fields, tags, aliases, profile selectors, mappings, metadata, and backup restoration where applicable.

The exact target file depends on the command. A write may affect an error catalog, profile catalog, or another workspace catalog.

Do not assume every write targets `errors.en.json`.

## Core workflow

A successful write follows this conceptual sequence:

```text
resolve workspace
→ load current target
→ validate command input
→ apply the change in memory
→ validate the resulting document
→ serialize a complete temporary file
→ create a timestamped backup of the active target
→ replace the target file
→ return the saved and backup paths
```

The target is not opened and truncated before the replacement document is ready.

## Validation before replacement

Setter must validate before replacement.

Validation may include:

- command-specific input rules;
- lookup of the requested error, profile, mapping, owner, category, or code group;
- uniqueness and reference rules;
- supported severity and identifier formats;
- validation of the resulting catalog document.

When validation fails before persistence:

```text
target file remains unchanged
no backup is created
no replacement is attempted
```

A rejected write must not create recovery noise for a change that never became valid.

## Temporary-file persistence

Setter serializes the complete replacement document to a temporary file before replacing the active target.

The temporary file is created in the target directory so the final move normally remains on the same filesystem.

Conceptually:

```text
.<target-name>.<unique-id>.tmp
```

The temporary file must contain the complete serialized document before replacement begins.

A successful in-memory response is not enough. Persistence tests should reload the target file and verify the stored result.

## Timestamped backup

Before replacing an existing target, Setter creates a timestamped backup.

Backup names contain:

```text
.bak.json
```

A representative pattern is:

```text
<catalog-name>.<UTC-timestamp>.bak.json
```

The backup contains the previous active file, not the new document.

Backup creation is part of the successful write contract. Tests for a successful write should verify that the expected backup exists.

## Successful-write invariants

A successful write should provide evidence for all of these outcomes:

1. the command or service response succeeds;
2. the returned data represents the requested change;
3. reloading the target confirms the persisted value;
4. a timestamped backup of the previous target exists;
5. the workspace remains valid after the write;
6. the response reports useful target and backup information.

Do not test only the returned object.

## Rejected-write invariants

For invalid input, failed lookup, or validation failure, tests should verify:

1. the response contains the expected issue or failure contract;
2. the target file remains unchanged;
3. no backup is created;
4. no partial replacement is accepted;
5. a later valid operation can still use the workspace.

This is as important as testing the success path.

## Failure during serialization or replacement

A filesystem failure may occur because of:

- access permissions;
- a read-only location;
- a full disk;
- a disconnected or failing mount;
- another process holding the file;
- antivirus or security software interference;
- a backup-name collision;
- a failed move or replacement operation.

Setter should return a structured failure rather than pretending that the write succeeded.

A temporary file or backup may remain depending on the exact failure point.

> Do not retry blindly after a failed write.

First inspect the active target, temporary files, backups, command output, and Git diff. Confirm which file is authoritative before trying another write.

## Cancellation

Cancellation is not an ordinary validation or save failure.

A cancellation may occur after a temporary file was created. Inspect the workspace before retrying, especially when the caller cannot prove whether replacement began.

## Single-file operation

Each safe write is a single-file operation.

The workflow protects one target file for one command execution. It is not a multi-file transaction.

A command or future workflow that must update several catalogs atomically requires a broader transaction design. Safe replacement of each file independently does not guarantee that all files succeed or fail together.

## Concurrent writers

Setter is not a multi-process locking system.

Two writers can load the same old state and later overwrite one another:

```text
writer A loads
writer B loads
writer A saves
writer B saves its older in-memory document
```

Use one authoring process at a time for the same workspace. Pull or refresh the latest Git state before writing, and inspect the diff immediately afterward.

## What safe write guarantees

The current workflow provides practical protection against common single-file authoring failures:

- command input is checked before persistence;
- an invalid resulting document is not intentionally installed;
- the active target is not directly truncated first;
- the complete replacement is serialized separately;
- the previous active file is preserved as a timestamped backup on success;
- failures are surfaced through structured responses and exit behavior;
- persisted state can be reviewed in Git.

## What safe write does not guarantee

Safe write does not provide:

- a multi-file transaction;
- distributed or multi-process locking;
- automatic conflict merging;
- automatic schema migration;
- a crash-recovery journal;
- guaranteed cleanup of every abandoned temporary file;
- automatic backup-retention cleanup;
- proof that the authored meaning is semantically correct;
- a substitute for Git history, tests, or external backups.

These are intentional boundaries, not hidden guarantees.

## List backups

Use `list-backups` to inspect recoverable files known to the workspace:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- list-backups .
```

Review the filename, target relationship, and timestamp before choosing a recovery source.

Backup files should normally remain local. Check `git status --short` before staging so `.bak.json` files are not committed accidentally.

## Restore a backup

Use `restore-backup` for an explicit selected backup:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- restore-backup . <backup-file>
```

Restoration is itself a write operation. Review the selected source carefully and validate the workspace afterward.

After restoration:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
git diff --check
git status --short
git diff
```

Do not assume the newest backup is automatically the correct one.

## Post-write inspection

After every write:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
git status --short
git diff --check
git diff
```

When documentation keys or Markdown changed, also run:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
```

Inspect the complete diff. Normalized serialization may reveal changes beyond the intended field, and those changes must be understood before commit.

## Test expectations

Every new or changed write path should test:

- valid persistence;
- persisted reload;
- backup creation;
- rejected input;
- unchanged source after rejection;
- absence of a backup after rejection;
- relevant failure output and exit behavior;
- temporary-workspace isolation.

Primary focused gate:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Do not proceed to another change while the current write behavior is red.

## Recovery decision

When recovery is needed:

1. stop further writers;
2. inspect the active target;
3. list available backups;
4. compare candidate backups with the target and Git history;
5. restore one explicit file only after choosing intentionally;
6. validate and run tests;
7. inspect the final diff;
8. update `IMPLEMENTATION_STATUS.md` when the project state changes.

## Central principle

> A safe write is complete only when the replacement is valid, the previous file is recoverable, the persisted result is verified, and the diff is understood.
