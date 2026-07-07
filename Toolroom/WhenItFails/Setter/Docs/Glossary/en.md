# Glossary

This glossary explains the most important words used in WhenItFails Setter documentation and catalog files.

It is written for catalog authors, maintainers, testers, and developers who need a shared vocabulary.

## Active catalog

The JSON file currently used by the workspace.

Example:

```text
Jsons/WhenItFails/errors.en.json
```

When Setter edits a field, it updates the active catalog and creates a backup of the previous active file.

## Alias

An alternative command name that runs the same command implementation.

Current aliases include:

```text
inspect
→ summary
```

```text
detail
→ details
```

Aliases exist for convenience and readability.

## Backup

A timestamped copy of an existing catalog file created before Setter replaces that file.

Example:

```text
errors.en.20260627-095820-480.bak.json
```

Backups are stored in the same directory as the target catalog file.

A backup is a recovery candidate, not a complete workspace snapshot.

## Catalog

A structured JSON document that defines one type of WhenItFails data.

Current workspace catalogs include:

```text
errors.en.json
categories.en.json
code-groups.en.json
owners.en.json
profiles.json
```

Each catalog has its own responsibility.

## Catalog author

A person who writes or updates catalog content.

Catalog authors usually edit:

- titles,
- messages,
- developer hints,
- severity,
- documentation keys,
- categories,
- profiles,
- ownership information.

Catalog authors should validate the workspace after every meaningful change.

## Category

A logical grouping used to classify errors.

Categories are human-facing and may describe the domain or nature of a problem.

Examples:

```text
NETWORK
CONFIGURATION
STORAGE
SECURITY
VALIDATION
```

An error has one primary category and may have additional categories.

## Category catalog

The catalog file that defines known categories.

Typical file:

```text
categories.en.json
```

It provides the controlled vocabulary used by error definitions.

## CI

Continuous integration.

CI runs automated checks such as:

```text
restore
build
test
validate
diff check
```

CI should treat non-zero Setter exit codes as failures unless a negative test explicitly expects them.

## Code

The stable numeric error code.

Example:

```text
600001
```

Numeric codes are intended for stable machine-readable identification, support, logging, and cross-language usage.

## Code group

A logical group that owns a numeric range and symbolic prefix.

Example:

```text
NETWORK
```

A code group helps keep error numbering organized.

## Code group catalog

The catalog file that defines known code groups.

Typical file:

```text
code-groups.en.json
```

It contains the known code families and their numeric ranges.

## Code prefix

The short symbolic prefix used as part of an error identity.

Example:

```text
NET
```

A code prefix is not the same as a category.

It belongs to the identity and numbering system.

## Command

A top-level Setter operation.

Examples:

```text
validate
summary
errors
details
set-title
```

Commands are selected by the first command-line argument.

## Command argument

A required value passed to a command.

Example:

```text
details <path> <id|code|name>
```

In this example, `<path>` and `<id|code|name>` are command arguments.

## Command option

An optional command modifier.

Examples:

```text
--plain
--search network
--severity Error
--profile WEB
```

Options may filter output or change rendering.

## Default severity

The normal severity assigned to an error definition.

Field:

```text
defaultSeverity
```

Supported values:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Default severity is not necessarily the severity of every runtime occurrence.

## Definition

A reusable catalog entry describing one known error type.

An error definition is not the same thing as one runtime error occurrence.

## Details view

The rendered output of:

```text
details
```

or:

```text
detail
```

It shows one error definition in a complete human-readable form.

## Developer hint

Optional developer-focused guidance attached to an error definition.

Field:

```text
developerHint
```

A developer hint should explain what a developer, operator, or maintainer should investigate next.

It should not contain secrets.

## Documentation key

A stable logical reference to extended documentation.

Field:

```text
documentationKey
```

Example:

```text
when-it-fails/errors/network/network-unavailable
```

A documentation key does not have to be a URL.

## Error catalog

The JSON catalog containing known error definitions.

Typical file:

```text
errors.en.json
```

This is the main catalog modified by current Setter edit commands.

## Error definition

A structured reusable description of a known error.

Important fields include:

```text
id
code
name
owner
codePrefix
codeGroup
primaryCategory
categories
subcategories
title
message
defaultSeverity
developerHint
documentationKey
tags
metadata
```

## Error ID

The stable human-readable identifier of an error definition.

Example:

```text
AFW_NET_0001
```

Follow the convention used by the active catalog.

## Error name

The machine-friendly symbolic name of an error definition.

Example:

```text
NETWORKUNAVAILABLE
```

Names are useful for developer-facing lookup and code references.

## Exit code

The numeric result returned by a process.

Setter uses exit codes such as:

```text
0
1
2
3
```

Scripts should use exit codes for automation control flow.

## Field

One named property inside a JSON catalog entry.

Example fields:

```text
title
message
defaultSeverity
developerHint
documentationKey
```

Setter edit commands update selected fields.

## Filter

A command option that narrows a list.

Examples:

```text
--owner AFW
--group NETWORK
--severity Error
--search timeout
```

Filters help locate relevant definitions.

## Human-readable output

Output intended for people reading a terminal.

It may include tables, colors, rich formatting, or wording that changes over time.

Automation should prefer exit codes and plain output where available.

## Initialization

The process of creating or ensuring the expected workspace structure.

Command:

```text
init
```

Initialization creates the `Jsons/WhenItFails` structure under a project root.

## Issue code

A structured code describing a specific validation or command problem.

Examples:

```text
MissingValidatePath
ErrorDefinitionNotFound
UnsupportedSeverity
```

Do not confuse issue codes with numeric error definition codes.

## JSON catalog package

The collection of WhenItFails catalog files stored in:

```text
Jsons/WhenItFails
```

This package is validated as a whole because files reference one another.

## Language

The language code stored in a catalog document.

Typical current value:

```text
en
```

File names also commonly include language markers, such as:

```text
errors.en.json
```

## Lookup value

The value used to find an error definition.

The `details` command accepts:

```text
id
code
name
```

Example:

```bash
details . AFW_NET_0001
details . 600001
details . NETWORKUNAVAILABLE
```

## Message

The default human-readable explanation of what happened.

Field:

```text
message
```

A message should be a clear reusable sentence.

Example:

```text
The application could not reach the remote service.
```

## Metadata

Optional advanced information attached to a definition.

Field:

```text
metadata
```

Metadata is reserved for scenarios that need extra structured data beyond the common fields.

## Name

The machine-friendly symbolic name of an error.

Field:

```text
name
```

Example:

```text
NETWORKUNAVAILABLE
```

## Normalization

The process of loading catalog data into a consistent in-memory shape.

Normalization may ensure defaults, consistent collections, or predictable representation before validation and rendering.

Do not rely on normalization to fix semantically wrong catalog content.

## Owner

The logical owner of an error definition.

Field:

```text
owner
```

Owners help divide responsibility across packages, domains, teams, or project areas.

## Owner catalog

The catalog file that defines known owners.

Typical file:

```text
owners.en.json
```

Owners may have numeric ranges and built-in flags.

## Package directory

The directory containing the WhenItFails catalog files.

Typical path:

```text
Jsons/WhenItFails
```

Most commands accept the package directory directly.

## Plain output

A simpler line-oriented output mode intended to be easier to read or process.

Supported by commands such as:

```text
errors
details
detail
```

Plain output is still presentation-oriented.

It is not the same as JSON output.

## Primary category

The main category assigned to an error definition.

Field:

```text
primaryCategory
```

The dedicated `--category` filter currently checks the primary category.

## Profile

A named selection of errors for a particular context.

Profiles can include owners, code groups, and categories.

Example use:

```bash
errors . --profile WEB
```

Profiles are useful when a project wants reusable views of the catalog.

## Profile catalog

The catalog file containing profile definitions.

Typical file:

```text
profiles.json
```

Profiles may be selected by name or display name.

## Project root

The root directory of the project that contains:

```text
Jsons/WhenItFails
```

Example:

```text
/home/user/projects/Toolbox
```

The `init` command expects a project root.

## Recovery

The process of restoring or correcting catalog content after an unwanted change.

Recovery may use:

- Setter backups,
- Git history,
- preserved temporary files,
- field-level edit commands,
- manual restoration.

Always validate after recovery.

## Rich output

Terminal output rendered with rich formatting such as tables, colors, and rules.

Setter uses Spectre.Console for rich output.

Rich output is best for humans, not scripts.

## Safe write

A conservative save workflow that writes a temporary file, creates a backup, and then replaces the target.

Safe write reduces the risk of damaging the active catalog during a write.

It is not a complete multi-file transaction system.

## Search

A text-based lookup used by the `errors` command.

Example:

```bash
errors . --search network
```

Search is case-insensitive and substring-based.

It is not a regular expression.

## Severity

The seriousness level of an error.

Supported values:

```text
Trace
Debug
Information
Warning
Error
Critical
```

The catalog stores default severity.

Runtime occurrences may have additional context.

## Smoke test

A small practical test proving that the main workflow works.

Example smoke test:

```text
init temporary workspace
→ validate
→ inspect one error
→ edit one field
→ validate again
```

Smoke tests are not exhaustive.

## Stable ID

An identifier intended to remain stable over time.

For errors, this is usually the `id` field.

Stable IDs are useful for support, documentation, automation, tests, and references.

## Subcategory

A more specific classification under or alongside categories.

Field:

```text
subcategories
```

Subcategories add detail without replacing the primary category.

## Summary

A workspace overview rendered by:

```text
summary
```

or:

```text
inspect
```

It shows catalog counts, owners, code groups, profiles, and primary-category counts.

## Symbolic name

A machine-friendly name for an error definition.

Example:

```text
NETWORKUNAVAILABLE
```

This usually refers to the `name` field.

## Tag

A flexible label assigned to an error definition.

Field:

```text
tags
```

Tags can support filtering, searching, profiles, or future behavior.

## Temporary file

A file written before replacing the active catalog.

Pattern:

```text
.<target-file-name>.<GUID>.tmp
```

Example:

```text
.errors.en.json.79e0917a3a6d48e1ad61b28a50f91976.tmp
```

A stale temporary file after a failed write should be inspected before deletion or recovery.

## Title

The short human-readable label for an error definition.

Field:

```text
title
```

Example:

```text
Network unavailable
```

A title should be short and specific.

## Validation

The process of checking whether the workspace is structurally and logically valid.

Command:

```text
validate
```

Validation checks the catalog package and reports structured issues.

## Validation issue

A specific problem found during validation.

It usually has:

```text
code
message
path
```

The issue code is useful for classification.

## Workspace

The WhenItFails JSON catalog workspace.

Usually this means the complete package:

```text
Jsons/WhenItFails
```

A workspace is validated as a whole.

## Workspace summary

The overview rendered by:

```text
summary
```

or:

```text
inspect
```

It helps confirm that the expected workspace was loaded.

## Writer

The component that saves JSON catalog files.

The current JSON writer uses a conservative safe-write workflow.

## Related documentation

- [Documentation Map](../Documentation%20Map/en.md)
- [Overview](../Overview/en.md)
- [Commands](../Commands/en.md)
- [Command Quick Reference](../Command%20Quick%20Reference/en.md)
- [Workspace Paths and Initialization](../Workspace%20Paths%20and%20Initialization/en.md)
- [Validation](../Validation/en.md)
- [Authoring Error Text](../Authoring%20Error%20Text/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)

## Central principle

> Shared tooling needs shared words: when catalog authors, scripts, tests, and documentation use the same terms, errors become easier to maintain.
