# Backups and Recovery

WhenItFails Setter creates timestamped local backups before replacing an existing catalog file through its safe-write workflow.

A backup is a recovery aid for one catalog write. It is not a substitute for Git history, external backups, release artifacts, or a complete workspace snapshot.

> Do not restore by timestamp alone.

## What a Setter backup contains

A backup contains the previous contents of one target catalog file.

Typical names end with:

```text
.bak.json
```

A backup therefore represents a single catalog file, not a complete workspace snapshot. Its contents may be valid JSON while still being incompatible with newer owners, categories, code groups, profiles, mappings, or metadata in the rest of the workspace.

## Backup creation

For an existing target, the safe-write sequence is:

```text
validate proposed change
→ write complete temporary file
→ flush temporary file
→ copy existing target to timestamped backup
→ replace target with temporary file
```

Rejected input or validation failure must leave the target file unchanged and must not create a backup.

A successful replacement normally leaves the previous target beside the active catalog as a timestamped backup.

## Backup naming and location

Backups are stored beside the catalog they protect. Their names preserve the final JSON extension and include a UTC timestamp.

Example:

```text
errors.en.20260728-154210-123.bak.json
```

The exact timestamp is operational metadata, not proof that the file is the correct recovery point.

## One writer per workspace

Use one active writer per workspace.

Setter does not coordinate concurrent authoring processes. Two commands, editors, or CI jobs writing the same workspace can overwrite each other's work even though each individual save uses the safe-write mechanism.

Stop other writers before recovery.

## Inspect the working tree first

Before listing or restoring backups, run:

```powershell
git status --short
git diff --check
git diff
```

Confirm:

- the intended workspace is open;
- no unrelated catalog edit is in progress;
- backup files have not been staged accidentally;
- the current active file is understood before it is replaced.

Preserve or commit valuable current work before recovery.

## List available backups

Use `list-backups` instead of relying on shell-specific filename searches:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- list-backups .
```

Review the reported target, backup filename, and timestamp.

The newest backup is only the state before the newest successful write. It may already contain an earlier unwanted change, an incomplete logical edit, or data from the wrong authoring sequence.

## Select a recovery candidate

Before restoring, inspect candidate contents and compare them with:

- the active catalog;
- the intended change;
- Git history;
- related catalogs and profiles;
- any affected runtime or documentation contract.

Do not restore by timestamp alone.

Where practical, test the candidate in a temporary copy of the complete workspace. Replacing only one file in isolation cannot prove cross-catalog compatibility.

## Restore with Setter

Use `restore-backup` for the selected backup:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- restore-backup . <backup-file>
```

Use the exact backup identifier or path shape documented by the command help for the current version.

Restoration is a write operation. Treat it with the same care as any other catalog edit.

## What restore does not mean

A successful file replacement does not prove that:

- the selected backup was semantically correct;
- the complete workspace is valid;
- newer intentional changes should have been discarded;
- runtime consumers remain compatible;
- documentation and tests still agree with the restored data.

Restore success is the beginning of verification, not the end of recovery.

## Validate immediately after restore

Run complete workspace validation:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

When documentation keys or Markdown may be affected, also run:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
```

Do not continue editing after an unverified restore.

## Inspect the restored state

Use the relevant inspection commands after validation.

For an error definition:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- details . AFW_NET_0001
```

For a profile:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- show-profile . WEB
dotnet run --project Toolroom/WhenItFails/Setter -- explain-profile . WEB
```

For compatibility-sensitive error changes, inspect references:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- error-references . AFW_NET_0001
```

Confirm that the recovered state is the state you intended, not merely an older state.

## Review the recovery diff

After restoration, run:

```powershell
git status --short
git diff --check
git diff
```

The diff should show only the intended recovery and any deliberately updated tests or documentation.

Unexpected broad changes may indicate:

- the wrong backup was selected;
- the wrong workspace was targeted;
- the backup came from another branch or schema state;
- newer catalog changes were unintentionally discarded;
- serialization normalization changed more than expected.

## Run tests

The minimum focused gate is:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Run the repository-wide test suite when shared runtime contracts, mappings, profiles, or other projects may be affected:

```powershell
dotnet test
```

Do not weaken a correct test merely because an older backup no longer satisfies the current contract.

## Recovery failure

When listing or restoration fails:

1. read the exact structured failure;
2. verify the workspace and backup path;
3. inspect file permissions and available disk space;
4. confirm that another process is not writing the workspace;
5. inspect the active file and candidate backup before retrying;
6. preserve all relevant files until the failure is understood.

Do not retry blindly. A second write can hide evidence from the first failure.

## Backup retention

Setter does not automatically remove old backups.

Before deleting local recovery files, confirm that:

- the active workspace validates;
- intended changes are committed;
- no recovery investigation remains open;
- external backups or Git history provide the required longer-term protection.

Backup cleanup is a deliberate maintenance action, not part of successful restore verification.

## Complete recovery checklist

- [ ] stop other writers;
- [ ] confirm the workspace path;
- [ ] inspect `git status --short` and the current diff;
- [ ] use `list-backups`;
- [ ] compare candidate contents rather than choosing by timestamp alone;
- [ ] preserve valuable current work;
- [ ] use `restore-backup` for the selected file;
- [ ] validate the complete workspace;
- [ ] run documentation checks when relevant;
- [ ] inspect affected errors, references, profiles, and mappings;
- [ ] review `git diff --check` and the actual diff;
- [ ] run `dotnet test Toolroom/WhenItFails/Setter.Tests`;
- [ ] update `IMPLEMENTATION_STATUS.md`;
- [ ] continue only when the recovery is understood and green.

## Stop rule

> Do not continue editing after an unverified restore.

## Related documentation

- [Safe Writes](../Safe%20Writes/en.md)
- [Reviewing Catalog Changes](../Reviewing%20Catalog%20Changes/en.md)
- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Testing and CI](../Testing%20and%20CI/en.md)
- [Known Limitations](../Known%20Limitations/en.md)

## Central principle

> A backup is useful only when the selected contents, complete workspace, Git diff, and verification evidence all agree.
