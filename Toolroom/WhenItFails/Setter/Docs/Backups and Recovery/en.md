# Backups and recovery

WhenItFails Setter creates timestamped backups before replacing an existing JSON catalog file.

Backups provide a local recovery point for individual Setter writes.

They do not replace:

* source control,
* repository history,
* external backups,
* filesystem snapshots,
* release artifacts,
* tested deployment procedures.

## Safe-write sequence

A successful write follows this sequence:

```text
serialize new document
→ write temporary file
→ flush temporary file
→ copy existing target to timestamped backup
→ move temporary file over target
```

The existing catalog is copied before the new file replaces it.

## Backup creation condition

A backup is created only when the target file already exists.

Existing target:

```text
target exists
→ backup created
→ target replaced
```

New target:

```text
target does not exist
→ no backup needed
→ new target created
```

Setter edit commands normally modify an existing:

```text
errors.en.json
```

so a successful edit normally creates a backup.

## Backup filename

The current filename pattern is:

```text
<file-name-without-extension>.<UTC timestamp>.bak<extension>
```

Example:

```text
errors.en.20260627-095820-480.bak.json
```

Components:

```text
errors.en
→ original filename without final extension
```

```text
20260627-095820-480
→ UTC timestamp
```

```text
.bak
→ backup marker
```

```text
.json
→ original extension
```

## Timestamp format

The timestamp uses:

```text
yyyyMMdd-HHmmss-fff
```

Meaning:

```text
yyyy
→ four-digit year
```

```text
MM
→ month
```

```text
dd
→ day
```

```text
HH
→ hour in 24-hour UTC time
```

```text
mm
→ minute
```

```text
ss
→ second
```

```text
fff
→ milliseconds
```

## Timestamp timezone

Backup timestamps are generated using UTC.

They do not necessarily match the workstation’s local clock.

Example:

```text
20260627-095820-480
```

means:

```text
2026-06-27
09:58:20.480 UTC
```

When comparing with local logs, account for the local timezone offset.

## Backup location

The backup is created in the same directory as the target file.

Example:

```text
Jsons/WhenItFails/
├── errors.en.json
└── errors.en.20260627-095820-480.bak.json
```

Keeping backup and target together simplifies recovery and ensures both are on the same filesystem location.

It also means directory write permission is required.

## Temporary filename

Before backup and replacement, the new document is written to a temporary file.

Pattern:

```text
.<target-file-name>.<GUID>.tmp
```

Example:

```text
.errors.en.json.79e0917a3a6d48e1ad61b28a50f91976.tmp
```

The temporary file is created in the same directory as the target.

## Why the temporary file is in the same directory

Using the same directory helps ensure that the final move usually occurs on the same filesystem.

This avoids depending on a cross-filesystem move.

It also means the directory must allow:

* file creation,
* backup creation,
* final replacement.

## Backup is a copy

The existing target is copied with:

```text
overwrite: false
```

The backup operation does not intentionally overwrite an existing backup file.

If a backup filename collision occurs, the write fails with an I/O result rather than silently replacing the earlier backup.

## Millisecond filename precision

Backup names include milliseconds.

Two writes targeting the same file within the same UTC millisecond could theoretically calculate the same backup filename.

Because backup overwrite is disabled, such a collision should fail rather than destroy an existing backup.

Automation should not assume that unlimited parallel writes are safe.

## One writer per workspace

Recommended rule:

```text
one active writer per workspace
```

Avoid:

* two Setter edit commands at the same time,
* Setter and a text editor saving simultaneously,
* several CI jobs editing the same checkout,
* scripts modifying the same catalog concurrently.

The current safe-write mechanism protects one write operation.

It is not a multi-process transaction coordinator.

## Finding backups

List all catalog backups:

```bash
find Jsons/WhenItFails \
  -maxdepth 1 \
  -type f \
  -name '*.bak.json' \
  -printf '%f\n' \
  | sort
```

List only error-catalog backups:

```bash
find Jsons/WhenItFails \
  -maxdepth 1 \
  -type f \
  -name 'errors.en.*.bak.json' \
  -printf '%f\n' \
  | sort
```

## Sort by modification time

Newest first:

```bash
find Jsons/WhenItFails \
  -maxdepth 1 \
  -type f \
  -name 'errors.en.*.bak.json' \
  -printf '%T@ %f\n' \
  | sort -nr
```

Human-readable:

```bash
ls -lt \
  Jsons/WhenItFails/errors.en.*.bak.json
```

Do not select a backup solely because it is newest.

Inspect its contents first.

## Why newest is not always correct

The newest backup represents the state before the newest successful write.

That state may already contain:

* an earlier unwanted edit,
* formatting normalization,
* an incomplete logical change,
* values copied from the wrong workspace.

Recovery should be content-based, not timestamp-only.

## Compare active file with one backup

```bash
diff -u \
  Jsons/WhenItFails/errors.en.20260627-095820-480.bak.json \
  Jsons/WhenItFails/errors.en.json
```

This shows what changed between the backup and the active file.

## Git-aware comparison

When the workspace is under Git:

```bash
git diff --no-index -- \
  Jsons/WhenItFails/errors.en.20260627-095820-480.bak.json \
  Jsons/WhenItFails/errors.en.json
```

`git diff --no-index` may return exit code `1` when differences exist.

That exit code means:

```text
files differ
```

not necessarily that the command malfunctioned.

## Inspect one field

For a quick comparison with `jq`:

```bash
jq '
  .errors[]
  | select(.id == "AFW_NET_0001")
  | {
      id,
      title,
      message,
      defaultSeverity,
      developerHint,
      documentationKey
    }
' \
  Jsons/WhenItFails/errors.en.json
```

Then run the same query against the backup:

```bash
jq '
  .errors[]
  | select(.id == "AFW_NET_0001")
  | {
      id,
      title,
      message,
      defaultSeverity,
      developerHint,
      documentationKey
    }
' \
  Jsons/WhenItFails/errors.en.20260627-095820-480.bak.json
```

## Validate a backup before restoration

A backup is an earlier catalog file, not a complete independent workspace.

To validate it safely:

1. Copy the complete workspace to a temporary directory.
2. Replace the temporary active catalog with the selected backup.
3. Validate the temporary workspace.

Example:

```bash
rm -rf /tmp/when-it-fails-recovery-test

cp -a \
  Jsons/WhenItFails \
  /tmp/when-it-fails-recovery-test
```

Replace the temporary active catalog:

```bash
cp \
  Jsons/WhenItFails/errors.en.20260627-095820-480.bak.json \
  /tmp/when-it-fails-recovery-test/errors.en.json
```

Validate:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate /tmp/when-it-fails-recovery-test
```

Clean up:

```bash
rm -rf /tmp/when-it-fails-recovery-test
```

## Why validate inside a complete workspace

The error catalog has relationships with:

* owners,
* code groups,
* categories,
* profiles.

A backup may be individually valid but incompatible with newer versions of the other catalogs.

Complete workspace validation detects those cross-catalog problems.

## Recovery preparation

Before restoring:

```text
stop writers
→ confirm workspace path
→ inspect Git status
→ list backups
→ compare candidates
→ validate selected candidate in a temporary workspace
```

Useful commands:

```bash
pwd
```

```bash
realpath Jsons/WhenItFails
```

```bash
git status --short
```

```bash
pgrep -af \
  'WhenItFails.*Setter'
```

## Preserve the current active file

Before replacing the current active catalog manually, preserve it separately.

Example:

```bash
cp \
  Jsons/WhenItFails/errors.en.json \
  /tmp/errors.en.before-recovery.json
```

This protects the current state even when it is believed to be wrong.

## Restore one backup manually

After reviewing and validating the selected backup:

```bash
cp \
  Jsons/WhenItFails/errors.en.20260627-095820-480.bak.json \
  Jsons/WhenItFails/errors.en.json
```

Setter currently does not provide a dedicated restore command.

Manual restoration should therefore be deliberate and reviewable.

## Validate immediately after restoration

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

Do not continue editing until validation succeeds or the remaining problems are understood.

## Inspect the restored definition

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001
```

Confirm the intended values are present.

## Review the Git diff

```bash
git diff -- \
  Jsons/WhenItFails/errors.en.json
```

The diff should match the intended recovery.

Unexpected large changes may indicate:

* wrong backup,
* wrong workspace,
* formatting differences,
* older catalog structure,
* restoration from an unrelated branch.

## Complete recovery workflow

```text
identify unwanted change
→ stop all writers
→ preserve current active file
→ list candidate backups
→ compare candidate contents
→ test candidate in temporary workspace
→ restore selected backup
→ validate complete workspace
→ inspect affected definitions
→ review Git diff
→ commit or continue editing
```

## Example recovery session

List backups:

```bash
find Jsons/WhenItFails \
  -maxdepth 1 \
  -type f \
  -name 'errors.en.*.bak.json' \
  -printf '%f\n' \
  | sort
```

Preserve active file:

```bash
cp \
  Jsons/WhenItFails/errors.en.json \
  /tmp/errors.en.before-recovery.json
```

Compare:

```bash
diff -u \
  Jsons/WhenItFails/errors.en.20260627-095820-480.bak.json \
  Jsons/WhenItFails/errors.en.json
```

Test:

```bash
rm -rf /tmp/when-it-fails-recovery-test

cp -a \
  Jsons/WhenItFails \
  /tmp/when-it-fails-recovery-test

cp \
  Jsons/WhenItFails/errors.en.20260627-095820-480.bak.json \
  /tmp/when-it-fails-recovery-test/errors.en.json

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate /tmp/when-it-fails-recovery-test
```

Restore:

```bash
cp \
  Jsons/WhenItFails/errors.en.20260627-095820-480.bak.json \
  Jsons/WhenItFails/errors.en.json
```

Validate:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

Review:

```bash
git diff -- \
  Jsons/WhenItFails/errors.en.json
```

Clean temporary files:

```bash
rm -rf /tmp/when-it-fails-recovery-test

rm -f /tmp/errors.en.before-recovery.json
```

## Restore only the affected field

Sometimes restoring the whole catalog would discard unrelated good edits.

In that case:

1. Inspect the field in the backup.
2. Use the corresponding Setter command to restore only that field.
3. Validate.
4. Review the diff.

Example restoring a title:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-title . \
  AFW_NET_0001 \
  "Network unavailable"
```

This creates another backup of the current active file before saving the corrected title.

## Field-level recovery advantages

Field-level restoration preserves:

* unrelated newer edits,
* other definitions,
* updated metadata,
* newer catalog structure.

It is preferable when only one known field needs correction.

## Whole-file recovery advantages

Whole-file restoration is useful when:

* the latest edit rewrote many unintended values,
* the active JSON is corrupted,
* many related changes must be reverted together,
* the backup is known to represent the desired complete state.

Choose based on the scope of damage.

## Whole-file recovery risks

Restoring an older complete file may discard:

* valid later edits,
* newly added errors,
* severity corrections,
* documentation updates,
* metadata,
* normalization improvements.

Always compare first.

## Recovery with Git

When the desired version exists in Git history, Git may be a better source than Setter backups.

Inspect history:

```bash
git log -- \
  Jsons/WhenItFails/errors.en.json
```

Show a previous version:

```bash
git show \
  <commit>:Jsons/WhenItFails/errors.en.json
```

Restore from the current branch index:

```bash
git restore \
  Jsons/WhenItFails/errors.en.json
```

Warning:

```text
git restore
→ discards uncommitted changes in that file
```

Use it only after reviewing and preserving anything needed.

## Backup versus Git history

Setter backup:

```text
captures the state immediately before one local write
```

Git history:

```text
captures reviewed committed project states
```

Both are useful.

Backups are faster for immediate undo.

Git is stronger for long-term traceability.

## Failed edit and backup expectations

No backup should be expected when an edit fails before the writer reaches backup creation.

Examples:

* arguments missing,
* error not found,
* whitespace-only value,
* unsupported severity,
* loading failed,
* edited document failed validation,
* serialization failed before backup,
* cancellation occurred before backup.

## Failure after backup creation

The current workflow is:

```text
temporary file written
→ backup copied
→ temporary file moved over target
```

If the final move fails after the backup was created:

* the original target may still remain,
* the backup may exist,
* the temporary file may remain.

Inspect all three before taking action.

## Inspect target, backup, and temporary file

```bash
ls -la \
  Jsons/WhenItFails
```

Find relevant files:

```bash
find Jsons/WhenItFails \
  -maxdepth 1 \
  -type f \
  \( \
    -name 'errors.en.json' \
    -o -name 'errors.en.*.bak.json' \
    -o -name '.errors.en.json.*.tmp' \
  \) \
  -printf '%f\n' \
  | sort
```

## Temporary file after interruption

A stale temporary file may contain a fully serialized candidate document.

Do not move it over the target blindly.

First:

```text
confirm no writer is active
→ inspect file type
→ validate JSON syntax
→ compare with target and backup
→ test in temporary workspace
```

## Inspect stale temporary JSON

```bash
python -m json.tool \
  Jsons/WhenItFails/.errors.en.json.<guid>.tmp \
  > /dev/null
```

Or:

```bash
jq empty \
  Jsons/WhenItFails/.errors.en.json.<guid>.tmp
```

These tools require strict JSON.

The writer itself emits normal indented JSON, so a completed temporary serialization should normally pass.

## Temporary file may be incomplete

An interrupted serialization may leave:

* truncated JSON,
* empty file,
* partially written object,
* incomplete array.

Never assume a `.tmp` file is recoverable merely because it exists.

## Cleaning stale temporary files

After confirming no Setter process is active:

```bash
find Jsons/WhenItFails \
  -maxdepth 1 \
  -type f \
  -name '.*.tmp' \
  -print
```

Delete one reviewed stale file:

```bash
rm \
  Jsons/WhenItFails/.errors.en.json.<guid>.tmp
```

Avoid broad deletion until each file is understood.

## Access-denied recovery

If saving fails with:

```text
AccessDenied
```

check both the target and directory:

```bash
ls -l \
  Jsons/WhenItFails/errors.en.json
```

```bash
ls -ld \
  Jsons/WhenItFails
```

```bash
test -w \
  Jsons/WhenItFails/errors.en.json \
  && echo target-writable \
  || echo target-not-writable
```

```bash
test -w \
  Jsons/WhenItFails \
  && echo directory-writable \
  || echo directory-not-writable
```

The directory must permit creation of temporary files and backups.

## Input/output failure recovery

If saving returns:

```text
InputOutputError
```

inspect:

```bash
df -h .
```

```bash
df -i .
```

```bash
dmesg --level=err,warn |
tail -n 50
```

Possible causes include:

* disk full,
* no free inodes,
* read-only mount,
* disconnected storage,
* backup filename collision,
* failed final move,
* file lock,
* filesystem error.

## Backup retention

Setter currently creates backups but does not automatically remove old ones.

Over time, the directory may accumulate:

```text
errors.en.<timestamp>.bak.json
```

A retention policy should be deliberate.

Possible project policies:

* retain all backups during active development,
* retain the newest fixed number,
* retain backups for a fixed number of days,
* remove backups only after commits,
* archive backups outside the repository.

## Do not delete backups blindly

Before cleanup, confirm:

* the active catalog validates,
* important changes are committed,
* no recovery investigation is in progress,
* backups are not part of a release audit,
* the cleanup pattern targets only intended files.

## Preview backup cleanup

List backups older than 30 days:

```bash
find Jsons/WhenItFails \
  -maxdepth 1 \
  -type f \
  -name '*.bak.json' \
  -mtime +30 \
  -print
```

This only previews them.

## Delete old backups carefully

After reviewing the preview:

```bash
find Jsons/WhenItFails \
  -maxdepth 1 \
  -type f \
  -name '*.bak.json' \
  -mtime +30 \
  -delete
```

Use deletion only when the project retention policy allows it.

## Keep the newest backups

Example preview sorted newest first:

```bash
find Jsons/WhenItFails \
  -maxdepth 1 \
  -type f \
  -name 'errors.en.*.bak.json' \
  -printf '%T@ %p\n' \
  | sort -nr
```

Removing all except a fixed number requires careful scripting and should be tested first with printed filenames.

## Backups in Git

Whether backup files should be committed is a project policy decision.

Commonly, timestamped local backups are:

```text
not committed
```

because Git already tracks reviewed history and backups create noisy changes.

However, do not add ignore rules without considering:

* audit requirements,
* release processes,
* existing repository policy,
* whether backups are intentionally preserved.

## Check repository status

```bash
git status --short
```

If backups appear as untracked files, decide deliberately whether to:

* retain locally,
* archive,
* ignore by policy,
* remove after confirmed commit.

## Suggested ignore pattern

A possible pattern is:

```gitignore
Jsons/WhenItFails/*.bak.json
```

This is only a suggestion.

Do not add it automatically when a repository may intentionally track recovery artifacts.

## Backup integrity check

Calculate a checksum:

```bash
sha256sum \
  Jsons/WhenItFails/errors.en.20260627-095820-480.bak.json
```

Checksums are useful when:

* copying backups elsewhere,
* attaching them to diagnostics,
* verifying archive integrity,
* comparing apparently identical files.

## Preserve file metadata separately when needed

The Setter backup is a content copy.

Do not assume it preserves every possible filesystem property such as:

* extended attributes,
* access-control lists,
* external metadata,
* alternate data streams.

When those matter, use an appropriate filesystem or backup tool.

## Recovery test

Create a disposable workspace:

```bash
rm -rf /tmp/when-it-fails-backup-test

mkdir -p /tmp/when-it-fails-backup-test

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- init /tmp/when-it-fails-backup-test
```

Save the original checksum:

```bash
sha256sum \
  /tmp/when-it-fails-backup-test/Jsons/WhenItFails/errors.en.json
```

Perform an edit:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-title \
  /tmp/when-it-fails-backup-test \
  AFW_NET_0001 \
  "Network is not available"
```

List backups:

```bash
find \
  /tmp/when-it-fails-backup-test/Jsons/WhenItFails \
  -maxdepth 1 \
  -type f \
  -name 'errors.en.*.bak.json' \
  -printf '%f\n'
```

Select the backup:

```bash
backup_file="$(
  find \
    /tmp/when-it-fails-backup-test/Jsons/WhenItFails \
    -maxdepth 1 \
    -type f \
    -name 'errors.en.*.bak.json' \
    -print \
  | sort \
  | tail -n 1
)"
```

Confirm:

```bash
printf 'Selected backup: %s\n' \
  "$backup_file"
```

Restore:

```bash
cp \
  "$backup_file" \
  /tmp/when-it-fails-backup-test/Jsons/WhenItFails/errors.en.json
```

Validate:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate /tmp/when-it-fails-backup-test
```

Inspect:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details \
  /tmp/when-it-fails-backup-test \
  AFW_NET_0001
```

Clean up:

```bash
rm -rf /tmp/when-it-fails-backup-test
```

## Recovery checklist

Before restoration:

* stop active writers,
* confirm the exact workspace path,
* inspect Git status,
* preserve the active file,
* list available backups,
* compare backup contents,
* account for UTC timestamps,
* validate the candidate in a temporary workspace.

After restoration:

* validate the complete workspace,
* inspect affected definitions,
* review the Git diff,
* confirm no stale temporary files remain,
* commit the corrected state when appropriate,
* retain or clean backups according to policy.

## Current limitations

Setter currently does not provide:

* restore command,
* backup listing command,
* automatic retention,
* backup metadata index,
* backup checksums,
* multi-file snapshots,
* transactional restoration,
* concurrent writer coordination,
* rollback of several catalog files as one unit.

A backup belongs to one target-file write.

It is not a complete workspace snapshot.

## Future improvements

Possible future additions include:

* `backups` command,
* `restore` command,
* dry-run restoration,
* backup validation before restore,
* backup retention configuration,
* named snapshots,
* multi-catalog transactions,
* backup reason metadata,
* comparison view,
* automatic stale-temporary-file reporting.

These are future possibilities, not current guarantees.

## Related documentation

* [Safe Writes](../Safe%20Writes/en.md)
* [Editing Error Fields](../Editing%20Error%20Fields/en.md)
* [Validation](../Validation/en.md)
* [Troubleshooting](../Troubleshooting/en.md)
* [Testing and CI](../Testing%20and%20CI/en.md)
* [Workspace Paths and Initialization](../Workspace%20Paths%20and%20Initialization/en.md)

## Central principle

> A backup is a recovery candidate, not an automatic answer: inspect it, test it, restore it deliberately, and validate afterward.
