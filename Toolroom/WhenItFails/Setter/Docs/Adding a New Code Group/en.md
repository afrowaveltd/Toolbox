# Adding a New Code Group

This guide explains how to add a new code group to the WhenItFails catalog.

Code groups live in:

```text
Jsons/WhenItFails/code-groups.en.json
```

A code group defines a numeric error-code range and a symbolic prefix for a family of related errors.

## Main principle

A good code group is:

```text
stable
range-based
prefix-based
broad enough to grow
specific enough to guide numbering
validated
```

Do not create a new code group for one isolated error unless that family is expected to grow.

## What a code group is

A code group controls:

```text
numeric code range
symbolic prefix
default categories
default tags
```

Example:

```text
NETWORK
→ prefix NET
→ range 600000 to 699999
```

A code group helps keep error identifiers and numeric codes organized.

## Code group versus category

A code group controls numbering.

A category describes the kind of problem.

They may share names, but they are not the same concept.

Example:

```text
NETWORK code group
→ numeric range and prefix
```

```text
NETWORK category
→ network-related problem classification
```

A code group can have default categories, but it is not itself a category.

## Code group versus owner

An owner describes responsibility.

A code group describes the numeric and symbolic family.

Example:

```text
AFW
→ owner
```

```text
NETWORK
→ code group
```

Do not use owners as numbering groups.

## Code group versus profile

A profile selects errors for a context.

A code group organizes error numbering.

Example:

```text
DATABASE
→ code group
```

```text
SERVICE
→ profile that may include database, network, and external-service errors
```

Do not use profiles as numbering groups.

## When to add a code group

Add a new code group when:

- a new technical family needs its own numeric range,
- the family will likely contain multiple errors,
- a distinct symbolic prefix improves clarity,
- catalog authors need a stable numbering area,
- support and logging benefit from recognizing the family,
- the existing ranges do not fit the concept well.

## When not to add a code group

Do not add a code group when:

- an existing code group clearly fits,
- the concept is only one temporary error,
- the concept is really a category,
- the concept is really an owner,
- the concept is really a tag,
- the numeric range would be arbitrary and unused.

Weak code groups:

```text
MISC
OTHER
TEMP
SPECIAL
NEW
```

These usually become numbering junk drawers.

## Recommended workflow

Use this workflow:

```text
validate current workspace
→ inspect existing code groups
→ choose name, prefix, and range
→ add code group
→ add or update related errors if needed
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

# edit Jsons/WhenItFails/code-groups.en.json

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .

git diff -- Jsons/WhenItFails/code-groups.en.json
```

PowerShell:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- summary .

# edit Jsons/WhenItFails/code-groups.en.json

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- summary .

git diff -- Jsons/WhenItFails/code-groups.en.json
```

## Code group shape

A typical code group contains:

```json
{
  "name": "STORAGE",
  "displayName": "Storage",
  "codePrefix": "STO",
  "codeFrom": 1000000,
  "codeTo": 1099999,
  "description": "Disk, volume, storage-device, capacity, health, and storage-operation errors.",
  "defaultCategories": [ "STORAGE" ],
  "defaultTags": [ "STORAGE" ],
  "defaultMappings": {}
}
```

Use the active catalog conventions.

Do not copy the example without checking available ranges and categories.

## Name

The `name` field is the stable code-group identifier.

Example:

```json
"name": "STORAGE"
```

A good code-group name is:

- stable,
- uppercase if that is the catalog convention,
- machine-friendly,
- short,
- specific,
- broad enough to contain multiple errors.

Good:

```text
STORAGE
LOCALIZATION
MESSAGING
OBSERVABILITY
CACHE
```

Weak:

```text
ERRORS2
TEMP
MISC
NEW_STUFF
ONE_OFF
```

## Display name

The `displayName` field is the human-readable label.

Example:

```json
"displayName": "Storage"
```

A good display name is readable, short, and suitable for summary output.

## Code prefix

The `codePrefix` field is the symbolic prefix for this group.

Example:

```json
"codePrefix": "STO"
```

A good prefix is:

- short,
- stable,
- unique,
- readable,
- clearly related to the group name.

Good:

```text
STO
LOC
MSG
OBS
```

Weak:

```text
X
AAA
TMP
NEW
```

The prefix is often used in error IDs.

Example:

```text
AFW_STO_0001
```

## Numeric range

Fields:

```text
codeFrom
codeTo
```

Example:

```json
"codeFrom": 1000000,
"codeTo": 1099999
```

The range should be:

- large enough for expected growth,
- not overlapping unintentionally with existing groups,
- easy to understand,
- documented by the code group,
- stable after release.

Do not choose random tiny ranges unless there is a deliberate numbering policy.

## Description

The `description` field explains what belongs in the code group.

Good:

```json
"description": "Disk, volume, storage-device, capacity, health, and storage-operation errors."
```

Weak:

```json
"description": "Storage stuff."
```

The description should help future authors decide where a new numeric code belongs.

## Default categories

The `defaultCategories` array lists categories normally associated with the code group.

Example:

```json
"defaultCategories": [ "STORAGE" ]
```

Default categories should reference known category names.

They are not a substitute for reviewing each error definition.

An individual error still has:

```text
primaryCategory
categories
```

## Default tags

The `defaultTags` array lists tags naturally associated with this code group.

Example:

```json
"defaultTags": [ "STORAGE" ]
```

Default tags should be stable and meaningful.

Do not use default tags as a dumping ground for vague labels.

## Default mappings

The `defaultMappings` object stores group-level defaults.

Example:

```json
"defaultMappings": {}
```

Mappings are string key-value pairs.

Use predictable keys only when there is a clear consumer or future need.

## Choosing a numeric range

Before choosing a range, inspect existing ranges.

Bash:

```bash
grep -n '"codeFrom"\|"codeTo"\|"name"' Jsons/WhenItFails/code-groups.en.json
```

PowerShell:

```powershell
Select-String `
  -Path Jsons/WhenItFails/code-groups.en.json `
  -Pattern '"name"|"codeFrom"|"codeTo"'
```

Check for overlap before committing.

## Range checklist

Before committing a range, ask:

- Does it overlap an existing code group?
- Is it large enough?
- Is it too large for the purpose?
- Does it fit owner ranges where applicable?
- Is the range easy to explain?
- Will future maintainers understand why it exists?
- Is migration needed for existing codes?

## Prefix checklist

Before committing a prefix, ask:

- Is it unique?
- Is it short?
- Is it stable?
- Does it match the group name?
- Does it conflict with existing IDs?
- Is it likely to be confused with another prefix?

Do not change a released prefix casually.

## Adding related category first

If the new code group needs a new category, add the category first.

Recommended order:

```text
add category
→ validate
→ add code group
→ validate
→ add error definitions
→ validate
```

This keeps reference errors easier to diagnose.

## Adding related errors

After adding the code group, add errors that use it.

An error may reference the code group through:

```text
codeGroup
codePrefix
code
```

Example:

```json
"code": 1000001,
"codePrefix": "STO",
"codeGroup": "STORAGE"
```

The code should fall inside the group range.

The prefix should match the group prefix.

## Updating profiles

Profiles may include code groups through:

```text
includeCodeGroups
```

Example:

```json
"includeCodeGroups": [ "STORAGE" ]
```

Add the new code group to a profile only if that profile should include those errors.

Do not add it to every profile automatically.

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

If validation fails, fix the code group or references before continuing.

## Inspect summary

Run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .
```

Check whether the new code group appears.

Review name, display name, prefix, range, and related counts if shown by current summary output.

## List related errors

After adding errors in the new group:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --group STORAGE
```

Also search:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --search storage
```

Use the actual code-group name.

## Review profile effects

If profiles were changed:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile DISK_TOOL
```

or the affected profile name.

Remember that current Setter profile browsing uses include owners, code groups, and categories, but not the full runtime-style tag logic.

## Stable code groups

Changing a code group later may break:

- error definitions,
- error IDs,
- numeric-code expectations,
- documentation,
- tests,
- support procedures,
- external consumers,
- profile filters.

Prefer adding a new group or migration support over casually renaming a released group.

## Deprecating a code group

If a code group becomes obsolete, do not delete it casually.

First check:

- existing error definitions,
- profiles,
- docs,
- tests,
- scripts,
- runtime consumers,
- support references.

A deprecation period may be safer than immediate removal.

## Common mistakes

Common code-group mistakes include:

- overlapping numeric ranges,
- prefix conflicts,
- using a display name in references,
- adding a code group when a category was enough,
- adding a code group for one temporary error,
- changing released ranges casually,
- forgetting to update profiles when needed,
- adding errors with codes outside the group range,
- using a code prefix that does not match the group,
- forgetting validation after manual JSON edits.

## Review Git diff

Run:

```bash
git diff -- Jsons/WhenItFails/code-groups.en.json
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

- intended code group was added,
- JSON formatting is clean,
- ranges are intentional,
- prefix is intentional,
- no unrelated groups changed,
- no accidental backup files are staged.

## Review checklist

Before commit, confirm:

```text
code group name is stable
display name is readable
prefix is unique
range is intentional
range does not overlap unintentionally
description is useful
default categories exist
default tags are meaningful
mappings are predictable
related errors use matching codePrefix
related errors use codes inside the range
affected profiles were reviewed
validation passes
summary looks reasonable
Git diff is intentional
```

## Commit message

Good:

```text
Add storage code group
```

```text
Add localization code group
```

```text
Add observability code group
```

Weak:

```text
update code groups
```

```text
json stuff
```

```text
ranges
```

## Related documentation

- [Catalog Files](../Catalog%20Files/en.md)
- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Adding a New Category](../Adding%20a%20New%20Category/en.md)
- [Adding a New Error Definition](../Adding%20a%20New%20Error%20Definition/en.md)
- [Adding a New Profile](../Adding%20a%20New%20Profile/en.md)
- [Validation](../Validation/en.md)
- [Workspace Summary](../Workspace%20Summary/en.md)
- [Browsing and Filtering Errors](../Browsing%20and%20Filtering%20Errors/en.md)
- [Glossary](../Glossary/en.md)

## Central principle

> Add a code group when the catalog needs a stable numbered home for a growing family of related errors.
