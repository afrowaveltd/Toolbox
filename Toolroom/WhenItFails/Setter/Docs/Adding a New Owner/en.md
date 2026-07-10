# Adding a New Owner

This guide explains how to add a new owner to the WhenItFails catalog.

Owners live in:

```text
Jsons/WhenItFails/owners.en.json
```

An owner describes responsibility for a family of error definitions.

It answers:

```text
Who owns this error definition?
```

not:

```text
What kind of problem is this?
```

## Main principle

A good owner is:

```text
stable
responsibility-based
range-aware
documented
validated
```

Do not create an owner for every feature, category, or temporary project idea.

Owners should represent responsibility boundaries.

## What an owner is

An owner describes who is responsible for maintaining a definition.

Default owners may include:

```text
AFW
APP
PLUGIN
USER
```

These represent broad responsibility areas:

```text
AFW
→ built-in Afrowave definitions
```

```text
APP
→ project-local application definitions
```

```text
PLUGIN
→ optional plugin or extension definitions
```

```text
USER
→ user-defined local definitions and experiments
```

## Owner versus category

An owner says:

```text
who is responsible
```

A category says:

```text
what kind of problem this is
```

Example:

```text
AFW
→ owner
```

```text
NETWORK
→ category
```

Do not create an owner named `NETWORK` merely because network errors exist.

That is a category or code group concern.

## Owner versus code group

An owner describes responsibility.

A code group describes numeric and symbolic error-code organization.

Example:

```text
APP
→ project-local responsibility
```

```text
VALIDATION
→ numeric range and prefix for validation errors
```

An APP-owned error may use the VALIDATION code group if that is the catalog policy.

## Owner versus profile

An owner describes responsibility.

A profile describes a usage context.

Example:

```text
PLUGIN
→ owner
```

```text
WEB
→ profile
```

A web profile may include errors from AFW, APP, PLUGIN, and USER owners.

## When to add an owner

Add a new owner when:

- a new responsibility boundary exists,
- a team, package family, extension system, or product area owns catalog entries,
- the catalog needs a distinct code range for responsibility tracking,
- support needs to know who maintains the definitions,
- project-local and built-in definitions need clearer separation.

## When not to add an owner

Do not add an owner when:

- the concept is a category,
- the concept is a code group,
- the concept is a profile,
- the concept is a tag,
- the distinction is temporary,
- the owner would only contain one experimental error,
- an existing owner clearly fits.

Weak owners:

```text
MISC
TEMP
NETWORK
DATABASE
TODAY
TEST
```

These usually indicate that another catalog concept should be used instead.

## Recommended workflow

Use this workflow:

```text
validate current workspace
→ inspect current owners
→ decide responsibility boundary
→ choose owner name and range
→ add owner
→ update related errors or profiles if needed
→ validate
→ inspect summary
→ review Git diff
→ commit
```

Bash:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .

# edit Jsons/WhenItFails/owners.en.json

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .

git diff -- Jsons/WhenItFails/owners.en.json
```

PowerShell:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- summary .

# edit Jsons/WhenItFails/owners.en.json

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- summary .

git diff -- Jsons/WhenItFails/owners.en.json
```

## Owner shape

A typical owner contains:

```json
{
  "name": "MODULE",
  "displayName": "Module",
  "description": "Error definitions owned by the module package family.",
  "codeFrom": 3000000,
  "codeTo": 3999999,
  "isBuiltIn": false,
  "aliases": [ "MODULES" ],
  "defaultMappings": {
    "catalogRole": "module",
    "editable": "true"
  }
}
```

Use the active catalog conventions.

Do not copy an example without checking available ranges and project policy.

## Name

The `name` field is the stable owner identifier.

Example:

```json
"name": "APP"
```

A good owner name is:

- stable,
- uppercase if that is the catalog convention,
- machine-friendly,
- short,
- clearly tied to responsibility,
- not tied to temporary wording.

Good:

```text
AFW
APP
PLUGIN
USER
MODULE
INTEGRATION
```

Weak:

```text
TEMP
NETWORK
ERRORS
OTHER
TEST
```

## Display name

The `displayName` field is the human-readable label.

Example:

```json
"displayName": "Application"
```

A good display name is readable in summary output and documentation.

It can be nicer than the stable owner name.

## Description

The `description` field explains what the owner is responsible for.

Good:

```json
"description": "Project-local application error definitions."
```

Weak:

```json
"description": "App stuff."
```

The description should help future catalog authors choose the right owner.

## Numeric owner range

Fields:

```text
codeFrom
codeTo
```

Example:

```json
"codeFrom": 1000000,
"codeTo": 1999999
```

An owner range describes the numeric space associated with this owner.

Before choosing a range, inspect existing owner ranges.

Bash:

```bash
grep -n '"name"\|"codeFrom"\|"codeTo"' Jsons/WhenItFails/owners.en.json
```

PowerShell:

```powershell
Select-String `
  -Path Jsons/WhenItFails/owners.en.json `
  -Pattern '"name"|"codeFrom"|"codeTo"'
```

Avoid accidental overlap unless the catalog policy explicitly allows it.

## Range checklist

Before committing an owner range, ask:

- Does it overlap unintentionally with another owner?
- Is it large enough for future definitions?
- Is it too large for the owner’s scope?
- Is it consistent with current catalog policy?
- Will support understand the range?
- Are existing codes affected?

Ranges are long-term catalog policy.

Do not change released ranges casually.

## Built-in flag

Field:

```text
isBuiltIn
```

Example:

```json
"isBuiltIn": true
```

Use:

```text
true
```

for built-in catalog definitions maintained as part of Afrowave or the base package.

Use:

```text
false
```

for project-local, plugin, extension, or user-defined definitions.

This flag may also guide future editing tools.

## Aliases

The `aliases` array contains alternate owner names.

Example:

```json
"aliases": [ "APPLICATION", "PROJECT" ]
```

Aliases are useful for:

- older terminology,
- common synonyms,
- migration,
- future lookup support.

Do not add aliases that belong to another owner.

Do not use aliases as random keywords.

## Default mappings

The `defaultMappings` object stores owner-level policy or metadata.

Example:

```json
"defaultMappings": {
  "catalogRole": "application",
  "editable": "true"
}
```

Mappings are string key-value pairs.

Common useful ideas:

```text
catalogRole
editable
```

Use predictable keys and string values consistent with existing catalog style.

## Editable mapping

The default owner catalog may use:

```json
"editable": "true"
```

or:

```json
"editable": "false"
```

This is a string value.

It may describe whether local tooling should normally allow edits to that owner’s definitions.

Do not treat it as a security boundary unless runtime code explicitly enforces it.

## Catalog role mapping

The `catalogRole` mapping can describe the owner’s role.

Examples:

```json
"catalogRole": "built-in"
```

```json
"catalogRole": "application"
```

```json
"catalogRole": "plugin"
```

```json
"catalogRole": "user"
```

Keep roles stable and meaningful.

## Adding related errors

After adding an owner, errors may reference it through:

```text
owner
```

Example:

```json
"owner": "MODULE"
```

Only update errors that are truly owned by the new owner.

Do not move existing definitions casually.

Changing owner can affect responsibility, profiles, summary output, and support workflows.

## Updating profiles

Profiles may include owners through:

```text
includeOwners
```

Example:

```json
"includeOwners": [ "AFW", "APP", "MODULE" ]
```

Add the new owner to a profile only if that profile should include the owner’s errors.

Production-facing profiles deserve extra review.

## Validate after editing

Run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

Expected exit code:

```text
0
```

If validation fails, fix the owner or references before continuing.

## Inspect summary

Run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .
```

Check that the owner appears correctly.

Review:

- owner name,
- display name,
- range if shown,
- counts if affected.

## List related errors

After adding or moving errors:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --owner MODULE
```

Use the actual owner name.

Also inspect affected definitions:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001
```

Use actual affected IDs.

## Review profile effects

If profiles changed:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile WEB
```

or the affected profile name.

Remember that profile browsing is a view over the catalog and current Setter filtering is intentionally simplified.

## Stable owner names

Changing an owner name later may break:

- error definitions,
- profile filters,
- documentation,
- tests,
- scripts,
- support procedures,
- runtime consumers.

Prefer aliases or migration support over casually renaming a released owner.

## Deprecating an owner

If an owner becomes obsolete, do not delete it casually.

First check:

- existing error definitions,
- profile references,
- documentation links,
- tests,
- scripts,
- runtime consumers,
- support references.

A deprecation period may be safer than immediate removal.

## Common mistakes

Common owner mistakes include:

- using owner as category,
- using owner as code group,
- creating an owner for a temporary experiment,
- overlapping ranges accidentally,
- changing released owner names casually,
- using display names in references,
- forgetting to update profiles when needed,
- moving existing errors without review,
- setting `isBuiltIn` incorrectly,
- treating mapping strings as JSON booleans.

## Review Git diff

Run:

```bash
git diff -- Jsons/WhenItFails/owners.en.json
```

If errors or profiles were updated:

```bash
git diff -- Jsons/WhenItFails
```

Then:

```bash
git diff --check
```

Confirm:

- intended owner was added,
- range is intentional,
- built-in flag is correct,
- mappings are predictable,
- no unrelated owners changed,
- no accidental backup files are staged.

## Review checklist

Before commit, confirm:

```text
owner name is stable
display name is readable
description explains responsibility
range is intentional
range does not overlap unintentionally
isBuiltIn is correct
aliases are meaningful
mappings are predictable
related errors use the owner intentionally
affected profiles were reviewed
validation passes
summary looks reasonable
Git diff is intentional
```

## Commit message

Good:

```text
Add module owner
```

```text
Add integration owner
```

```text
Add project package owner
```

Weak:

```text
update owners
```

```text
json stuff
```

```text
new owner maybe
```

## Related documentation

- [Catalog Files](../Catalog%20Files/en.md)
- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Adding a New Code Group](../Adding%20a%20New%20Code%20Group/en.md)
- [Adding a New Error Definition](../Adding%20a%20New%20Error%20Definition/en.md)
- [Adding a New Profile](../Adding%20a%20New%20Profile/en.md)
- [Validation](../Validation/en.md)
- [Workspace Summary](../Workspace%20Summary/en.md)
- [Browsing and Filtering Errors](../Browsing%20and%20Filtering%20Errors/en.md)
- [Glossary](../Glossary/en.md)

## Central principle

> Add an owner when the catalog needs a stable responsibility boundary, not merely a new label for a kind of error.
