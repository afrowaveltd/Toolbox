# Setting an error title

The `set-title` command changes the `Title` field of one error definition in `errors.en.json`.

It is intended for focused, explicit edits of short human-facing error labels.

## Command syntax

```text
set-title <path> <id|code|name> <title>
```

Example:

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network is not available"
```

When running from the Toolbox repository:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-title . AFW_NET_0001 \
  "Network is not available"
```

## Workspace path

The `<path>` argument may point to:

* a project root,
* the `Jsons/WhenItFails` directory directly.

Examples:

```text
.
./MyProject
./MyProject/Jsons/WhenItFails
```

## Error lookup

The target error may be selected by:

* stable ID,
* numeric code,
* symbolic name.

Examples:

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network is not available"
```

```bash
when-it-fails-setter set-title . \
  600001 \
  "Network is not available"
```

```bash
when-it-fails-setter set-title . \
  NETWORKUNAVAILABLE \
  "Network is not available"
```

Text lookup is case-insensitive.

## What the title represents

`Title` is a short human-facing label describing the error.

A good title should be:

* concise,
* specific,
* naturally capitalized,
* understandable without implementation details,
* suitable for tables, dialogs, logs, and summaries.

Good example:

```text
Network is not available
```

Weak example:

```text
Error
```

Overly technical example:

```text
HttpRequestException caused by socket failure
```

Technical diagnosis belongs in `DeveloperHint`, not in the title.

## Title versus message

The title identifies the problem.

The message explains it.

Example:

```text
Title:
Network is not available
```

```text
Message:
The application could not reach the remote service.
```

The title should not try to contain the complete explanation.

## Argument handling

Everything after the lookup value is joined into the new title.

Example:

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  Network is not available
```

is interpreted as:

```text
Network is not available
```

Quoting is still recommended:

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network is not available"
```

Quoting avoids shell interpretation problems and preserves the author’s intent more clearly.

## Empty title

An empty or whitespace-only title is rejected.

Examples:

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  ""
```

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "   "
```

Failure code:

```text
TitleIsEmpty
```

The catalog is not saved.

## Whitespace trimming

The new title is trimmed before storage.

Input:

```text
"  Network is not available  "
```

Stored title:

```text
Network is not available
```

Internal spaces remain unchanged.

## Safe-edit workflow

The command follows this sequence:

```text
validate command arguments
→ resolve workspace path
→ load errors.en.json
→ normalize catalog document
→ locate target error
→ remember old title
→ assign new title in memory
→ validate edited error catalog
→ save through safe writer
→ report result
```

## Loading

The command loads the error catalog through:

```csharp
JsonErrorCatalogLoader
```

The edit fails when the catalog:

* does not exist,
* cannot be read,
* contains invalid JSON,
* cannot be deserialized.

The command does not create a missing error catalog.

Use:

```bash
when-it-fails-setter init .
```

to create missing workspace files.

## Normalization

The loaded document is normalized before lookup and editing.

Normalization makes symbolic values consistent and trims display values.

This may also mean that a successful save rewrites the catalog in normalized, indented JSON form.

Always review the resulting Git diff.

## Lookup failure

When the target error cannot be found, the editor returns:

```text
ErrorDefinitionNotFound
```

The message explains that lookup supports:

```text
Id
Code
Name
```

No save is attempted.

## In-memory edit

The previous title is remembered.

The new title is applied only to the in-memory normalized document before validation.

At this stage, the source file has not yet been changed.

## Validation

The edited error catalog is validated through:

```csharp
ErrorCatalogValidator
```

If validation fails:

```text
old title restored in memory
→ save not attempted
→ project file remains unchanged
```

Failure code:

```text
EditedErrorCatalogIsInvalid
```

This validation covers the error catalog document.

A complete workspace validation should still be run afterward to verify all cross-catalog relationships.

## Safe writer

A valid edited document is passed to:

```csharp
JsonCatalogDocumentWriter
```

The writer:

```text
writes a temporary file
→ creates a backup of the existing target
→ moves the temporary file over the target
```

The target is not directly truncated and rewritten from the beginning.

## Temporary file

The new catalog is first serialized to a uniquely named temporary file in the same directory.

Example pattern:

```text
.errors.en.json.<GUID>.tmp
```

A successful save moves this temporary file over the active target, so it should not remain afterward.

## Backup file

Before replacing an existing catalog, the writer creates a timestamped backup.

Current pattern:

```text
<name>.<UTC timestamp>.bak<extension>
```

Example:

```text
errors.en.20260627-095820-480.bak.json
```

Timestamp format:

```text
yyyyMMdd-HHmmss-fff
```

The backup contains the original `errors.en.json` before the title change.

## Successful output

Example command:

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network is not available"
```

Typical success output:

```text
Updated title: AFW_NET_0001
New title: Network is not available
Error title changed from 'Network unavailable' to 'Network is not available'.
```

The exact paths and previous value depend on the workspace.

## Same old and new title

The current implementation does not reject a no-op edit.

Example:

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network unavailable"
```

when the title is already:

```text
Network unavailable
```

may still:

* validate,
* create a backup,
* serialize the normalized document,
* replace the target,
* report success.

Inspect the current value before editing to avoid unnecessary rewrites and backups.

## Recommended pre-edit check

Run:

```bash
when-it-fails-setter details . AFW_NET_0001
```

Verify:

```text
Id
Code
Name
Owner
Code group
Primary category
Current title
```

Then apply the title change.

## Recommended post-edit check

Inspect the error again:

```bash
when-it-fails-setter details . AFW_NET_0001
```

Validate the complete workspace:

```bash
when-it-fails-setter validate .
```

Review the file change:

```bash
git diff -- Jsons/WhenItFails/errors.en.json
```

## Exit codes

```text
0
→ title updated successfully
```

```text
1
→ required arguments missing
```

```text
2
→ lookup, loading, validation, backup, or save failed
```

Unexpected top-level failures may return:

```text
3
```

## Missing arguments

An incomplete command returns a validation-style error.

Example:

```bash
when-it-fails-setter set-title .
```

Failure code:

```text
MissingSetTitleArguments
```

Expected syntax is shown as:

```text
set-title <path> <id|code|name> <title>
```

## Save failures

Possible save-related codes include:

```text
AccessDenied
InputOutputError
JsonSerializationFailed
```

Possible causes include:

* read-only directory,
* denied permissions,
* disk full,
* unavailable mount,
* file lock,
* file-system error,
* failed backup creation,
* failed target replacement.

After a save failure, inspect:

* the active catalog,
* any created backup,
* any remaining temporary file.

Do not assume the target state without checking.

## Cancellation

Cancellation is propagated as:

```text
OperationCanceledException
```

It is not converted into an ordinary title-edit failure.

A cancelled operation may leave a temporary file depending on when cancellation occurred.

## Git workflow

Recommended sequence:

```bash
when-it-fails-setter validate .

when-it-fails-setter details . AFW_NET_0001

when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network is not available"

when-it-fails-setter details . AFW_NET_0001

when-it-fails-setter validate .

git diff -- Jsons/WhenItFails/errors.en.json
```

Then commit:

```bash
git add Jsons/WhenItFails/errors.en.json

git commit -m "Improve network unavailable error title"
```

## Writing guidance

A title should usually:

* describe one failure,
* avoid trailing punctuation unless necessary,
* avoid raw exception names,
* avoid internal hostnames or paths,
* avoid blaming the user,
* remain useful outside one specific UI.

Good:

```text
Configuration file was not found
```

Less useful:

```text
Bad configuration
```

Too technical:

```text
FileNotFoundException while loading appsettings.json
```

## Common mistakes

### Title is too vague

Poor:

```text
Operation failed
```

Better:

```text
Remote request failed
```

### Title contains the entire explanation

Poor:

```text
The remote request failed because the DNS lookup did not return an address and the proxy was unavailable
```

Better title:

```text
Remote request failed
```

Better message:

```text
The application could not resolve or reach the remote service.
```

### Title contains temporary data

Avoid:

```text
Connection to server-17.example.internal failed
```

Prefer:

```text
Remote service is not available
```

Runtime-specific values belong in occurrence details, not stable catalog text.

### Edit is made without inspection

Always confirm the selected error first:

```bash
when-it-fails-setter details . <lookup>
```

### Full validation is skipped

The editor validates the error catalog, but the complete workspace should still be checked:

```bash
when-it-fails-setter validate .
```

## Related documentation

* [Editing error fields](../Editing%20Error%20Fields/en.md)
* [Command reference](../Commands/en.md)
* [Safe writes](../Safe%20Writes/en.md)
* [Getting started](../Getting-Started/en.md)

## Central principle

> A title should identify the failure clearly, and changing it should remain explicit, validated, backed up, and reviewable.
