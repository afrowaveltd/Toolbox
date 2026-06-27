# Safe writes

All WhenItFails Setter write commands use a conservative save workflow designed to reduce the risk of damaged or accidentally lost catalog data.

The workflow is shared by the underlying JSON catalog writer and the Setter workspace editor.

## Purpose

A catalog write should never begin by directly truncating the active JSON file.

Instead, Setter prepares and validates the new document first, writes it separately, creates a backup of the current file, and only then replaces the target.

The intended sequence is:

```text
load
→ normalize
→ locate target
→ modify in memory
→ validate
→ serialize to temporary file
→ create backup
→ replace target file
```

## Current write commands

The following commands modify `errors.en.json`:

| Command                 | Modified field     |
| ----------------------- | ------------------ |
| `set-title`             | `Title`            |
| `set-message`           | `Message`          |
| `set-developer-hint`    | `DeveloperHint`    |
| `set-severity`          | `DefaultSeverity`  |
| `set-documentation-key` | `DocumentationKey` |

All currently supported Setter edits affect one error definition at a time.

## Step 1: resolve the workspace

The supplied path may point to:

* the project root,
* the `Jsons/WhenItFails` directory.

Setter resolves the corresponding `JsonsOptions` and determines the error catalog path.

Example project layout:

```text
MyProject/
└── Jsons/
    └── WhenItFails/
        └── errors.en.json
```

## Step 2: load the existing catalog

Setter loads the existing error catalog through:

```csharp
JsonErrorCatalogLoader
```

The edit stops when the file:

* does not exist,
* cannot be read,
* contains invalid JSON,
* does not deserialize into an error catalog.

Typical failure codes may include:

```text
FileNotFound
InvalidJson
AccessDenied
InputOutputError
ErrorCatalogLoadFailed
```

No save operation is attempted after a loading failure.

## Step 3: normalize the document

The loaded catalog is normalized before lookup and editing.

This creates consistent runtime values for:

* IDs,
* names,
* owners,
* code prefixes,
* code groups,
* categories,
* tags,
* display fields.

Normalization does not save anything by itself.

It prepares the in-memory document for reliable lookup and validation.

## Step 4: locate the error

The target error may be found by:

```text
stable ID
numeric code
symbolic name
```

Examples:

```text
AFW_NET_0001
600001
NETWORKUNAVAILABLE
```

Text lookup is case-insensitive.

When no matching definition is found, Setter returns:

```text
ErrorDefinitionNotFound
```

The catalog remains unchanged.

## Step 5: validate the new value

Text write commands reject empty or whitespace-only input.

Possible codes include:

```text
TitleIsEmpty
MessageIsEmpty
DeveloperHintIsEmpty
DocumentationKeyIsEmpty
```

Severity has additional validation.

Supported values are:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Severity matching is case-insensitive and the stored value is normalized to the canonical spelling.

Unsupported input returns:

```text
UnsupportedSeverity
```

## Step 6: modify only the in-memory document

Setter stores the previous field value and applies the requested change to the normalized in-memory document.

For text fields, surrounding whitespace is removed.

Example:

```text
"  Network is not available  "
```

is stored as:

```text
Network is not available
```

At this point, the project file has not yet been changed.

## Step 7: validate the edited catalog

After the field is changed, Setter validates the resulting error catalog.

This validation checks the complete error document rather than only the changed field.

If validation fails, Setter restores the previous value in memory and returns:

```text
EditedErrorCatalogIsInvalid
```

The modified document is not passed to the writer.

The original project file remains untouched.

## Validation scope

Current editor validation includes the rules enforced by:

```csharp
ErrorCatalogValidator
```

This includes matters such as:

* required fields,
* valid numeric codes,
* supported severities,
* duplicate IDs,
* duplicate names,
* duplicate numeric codes,
* owner and prefix structure,
* normalized collection values.

The editor currently validates the edited error catalog document before saving.

A complete workspace validation should still be run after editing to check all cross-catalog relationships.

Recommended sequence:

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network is not available"

when-it-fails-setter validate .
```

## Step 8: serialize to a temporary file

The writer creates a uniquely named temporary file in the same directory as the target catalog.

Example:

```text
.errors.en.json.14de6c969c14495a8eb727e4f6b0a53c.tmp
```

The exact GUID value changes for every write.

Using the same directory is important because the final move remains on the same file system under normal conditions.

The temporary file is created with:

```text
CreateNew
Write access
No sharing
```

This prevents accidental reuse of an existing temporary path.

## JSON output

The document is serialized using indented JSON.

The file stream is flushed before replacement is attempted.

The saved catalog therefore becomes normalized, consistently serialized JSON.

This may produce formatting differences beyond the one edited field, especially when the source file previously used:

* unusual indentation,
* comments,
* trailing commas,
* inconsistent property casing,
* non-canonical key formatting.

Runtime normalization and serialization are intentional parts of the current edit workflow.

Always review the resulting Git diff.

## Step 9: create a timestamped backup

When the target file already exists, the writer copies it before replacement.

The current naming pattern is:

```text
<name>.<UTC timestamp>.bak<extension>
```

For `errors.en.json`, an example is:

```text
errors.en.20260627-142311-123.bak.json
```

The timestamp format is:

```text
yyyyMMdd-HHmmss-fff
```

It uses UTC and includes milliseconds.

The backup is a complete copy of the original target file before the new document replaces it.

## Why the extension remains at the end

The backup file keeps `.json` as its final extension:

```text
errors.en.20260627-142311-123.bak.json
```

This makes it easy to:

* open in JSON editors,
* inspect with JSON tooling,
* compare with the active catalog,
* restore manually.

The `.bak` marker remains visible before the extension.

## Backup creation behavior

The backup copy uses:

```text
overwrite: false
```

Therefore an existing file with the same generated backup name is never replaced.

The millisecond timestamp makes a collision unlikely, but a collision still causes the save operation to fail rather than silently destroy an older backup.

## Step 10: replace the target

After the temporary file is successfully serialized and the backup is created, the writer moves the temporary file over the target path with overwrite enabled.

Conceptually:

```text
temporary file
→ move over target
```

This is safer than opening the active catalog and writing directly into it.

The move occurs only after the complete new JSON file exists.

## What safe write guarantees

The current workflow provides these practical protections:

* the target is not directly truncated before serialization,
* invalid editor input is rejected early,
* invalid edited catalogs are not passed to the writer,
* the new JSON is fully serialized to a separate file first,
* the existing target is backed up before replacement,
* the original file remains available as a timestamped backup after success,
* write failures are returned as structured responses,
* cancellation is not hidden as an ordinary save error.

## What safe write does not guarantee

The workflow is conservative, but it is not a full transactional storage engine.

It does not currently provide:

* distributed locking,
* multi-file transactions,
* simultaneous atomic updates across all catalogs,
* automatic backup retention,
* automatic rollback from backup,
* automatic cleanup of every abandoned temporary file,
* protection against another process editing the same file concurrently,
* crash recovery journal,
* schema migration.

These concerns should not be inferred merely from the term “safe write.”

## Single-file scope

Each current write command modifies only:

```text
errors.en.json
```

The backup and replacement apply to that one file.

A future command that changes several catalogs would require a broader transaction design if all files must succeed or fail together.

## Failure before temporary-file creation

Examples:

* invalid path,
* missing file,
* malformed JSON,
* error not found,
* empty new value,
* unsupported severity,
* validation failure.

Result:

```text
no temporary file written
no backup created
target unchanged
```

## Serialization failure

If JSON serialization fails, the writer returns:

```text
JsonSerializationFailed
```

The target replacement is not attempted.

Depending on exactly when the failure occurred, a temporary file may remain in the catalog directory and can be removed manually after inspection.

Temporary files use the pattern:

```text
.<target filename>.<GUID>.tmp
```

## Access failure

When file access is denied, the writer returns:

```text
AccessDenied
```

Possible causes:

* read-only directory,
* denied file permissions,
* restricted service account,
* locked-down container volume,
* security software,
* another process preventing access.

The user should inspect both the active catalog and any temporary or backup files before retrying.

## Input/output failure

General file-system write failures return:

```text
InputOutputError
```

Possible causes:

* disk full,
* disconnected mount,
* hardware failure,
* file-system error,
* network share interruption,
* file lock,
* failed move operation,
* backup collision.

The response message includes the underlying I/O error.

## Cancellation

Cancellation is propagated as:

```text
OperationCanceledException
```

It is not converted into:

```text
InputOutputError
```

or another ordinary failure response.

A cancelled operation may leave a temporary file if cancellation occurs after that file was created.

The active target should be inspected before another write attempt.

## Save response

A successful save returns a message containing:

* the saved target path,
* the backup path when a backup was created.

Conceptual example:

```text
JSON catalog file was saved:
Jsons/WhenItFails/errors.en.json.

Backup:
Jsons/WhenItFails/errors.en.20260627-142311-123.bak.json
```

Setter displays this information after a successful edit.

## In-memory rollback

The editor remembers the old field value.

When validation or saving fails, it restores that value in the in-memory object before returning the failure response.

This keeps the returned editor state internally consistent.

It does not perform a disk rollback because the original target should either:

* remain unchanged before replacement,
* or exist in the timestamped backup after replacement.

## Concurrent writers

Setter does not currently coordinate several processes editing the same catalog simultaneously.

A possible race:

```text
process A loads catalog
process B loads catalog
process A saves change
process B saves older in-memory copy
```

The second save may unintentionally overwrite the first change.

Recommended rule:

> Only one authoring process should write to a workspace at a time.

Use version control and review diffs to detect accidental overwrites.

## Backup retention

Backups are not deleted automatically.

Repeated edits therefore create multiple files:

```text
errors.en.20260627-142311-123.bak.json
errors.en.20260627-143002-551.bak.json
errors.en.20260627-145844-091.bak.json
```

This is intentional.

Automatic deletion would require a documented retention policy.

Projects may later choose policies based on:

* age,
* count,
* deployment environment,
* available storage,
* audit requirements.

Until then, cleanup is manual.

## Restoring a backup

Setter does not currently provide a dedicated restore command.

Manual recovery should be deliberate.

Recommended process:

```text
1. Stop all Setter writes.
2. Identify the correct backup.
3. Compare backup and active file.
4. Preserve the current active file.
5. Copy the selected backup to errors.en.json.
6. Run complete workspace validation.
7. Review the Git diff.
```

Example:

```bash
cp \
  Jsons/WhenItFails/errors.en.20260627-142311-123.bak.json \
  Jsons/WhenItFails/errors.en.json
```

Then:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

Do not restore solely because a backup is newer or older. Inspect its content first.

## Version control

Timestamped backups are a local safety mechanism.

They do not replace Git.

Git provides:

* meaningful history,
* authorship,
* review,
* rollback,
* branching,
* comparison,
* remote copies.

Recommended workflow:

```text
validate
→ inspect target
→ edit
→ validate
→ git diff
→ test
→ commit
```

## Backup files and Git

Projects should decide explicitly whether generated backup files belong in version control.

A common policy is to ignore them locally while committing the real catalog changes.

Potential `.gitignore` pattern:

```gitignore
Jsons/WhenItFails/*.bak.json
```

Use this only when it matches the project’s audit and recovery policy.

Do not add a blanket rule without considering whether backups are intentionally archived.

## Reviewing the diff

Because the writer serializes the normalized document, always inspect:

```bash
git diff -- Jsons/WhenItFails/errors.en.json
```

Confirm:

* the intended field changed,
* the correct error was selected,
* no identity changed unexpectedly,
* no unrelated errors changed semantically,
* formatting changes are understood,
* severity uses canonical spelling,
* no sensitive data was added.

## Recommended safe-write workflow

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-message . AFW_NET_0001 \
  "The network is currently unavailable."

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

git diff -- Jsons/WhenItFails/errors.en.json
```

## Recommended operational rules

1. Validate before editing.
2. Inspect the selected error before editing.
3. Make one focused logical change.
4. Never run concurrent writers against one workspace.
5. Validate the complete workspace after editing.
6. Review the resulting Git diff.
7. Keep or remove backups according to explicit policy.
8. Treat severity changes as behavioral changes.
9. Inspect temporary files after interrupted writes.
10. Do not assume safe write means multi-file transaction.
11. Keep project catalogs under version control.
12. Test the consuming application before committing.

## Central principle

> Prepare and validate the replacement first, preserve the previous file, and make every catalog change explicit and reviewable.
