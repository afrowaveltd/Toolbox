# Adding a New Category

This guide explains how to add a new category to the WhenItFails catalog.

Categories live in:

```text
Jsons/WhenItFails/categories.en.json
```

A category is a stable classification for error definitions. It should describe the domain or nature of a problem.

## Main principle

A good category is:

```text
stable
broad enough to reuse
specific enough to mean something
documented
validated
```

Do not create a new category for every individual error.

A category should be useful across several errors or across a meaningful domain.

## What a category is

A category classifies an error by problem domain.

Examples:

```text
GENERAL
CONFIGURATION
VALIDATION
FILE_SYSTEM
SECURITY
NETWORK
DATABASE
EXTERNAL_SERVICE
SERIALIZATION
```

A category is not:

- an owner,
- a profile,
- a tag,
- a code group,
- a severity,
- a runtime exception type.

## Category versus code group

A code group controls numeric ranges and prefixes.

A category describes the kind of problem.

They may often have the same name, but they are not the same concept.

Example:

```text
NETWORK code group
→ numeric range and prefix
```

```text
NETWORK category
→ network-related problem classification
```

Do not assume every category must have a matching code group.

Do not assume every code group must be used as a category in every error.

## Category versus profile

A category says:

```text
what kind of problem this is
```

A profile says:

```text
which errors are relevant for this application context
```

Example:

```text
NETWORK
```

may be a category.

```text
WEB
```

may be a profile that includes network, validation, security, and serialization errors.

## Category versus tag

A category is controlled classification.

A tag is a flexible label.

Use a category when the concept is central and stable.

Use a tag when the concept is secondary, flexible, or used for profile/runtime hints.

Example:

```text
SECURITY
```

is likely a category.

```text
USER_VISIBLE
```

is likely a tag.

## When to add a category

Add a new category when:

- existing categories do not describe the problem well,
- the concept will likely be reused,
- authors need a stable classification,
- profiles or documentation need to reference the concept,
- summary output would benefit from showing the concept,
- the category helps users navigate the catalog.

## When not to add a category

Do not add a category when:

- an existing category already fits,
- the concept is only one temporary implementation detail,
- the concept is better represented as a tag,
- the concept is only a runtime exception type,
- the concept is only an owner responsibility,
- the concept is too vague to guide authors.

Weak categories:

```text
MISC
OTHER
STUFF
TEMP
SPECIAL
```

These usually become junk drawers.

## Recommended workflow

Use this workflow:

```text
validate current workspace
→ inspect existing categories
→ decide whether a new category is justified
→ add category
→ update errors or profiles that use it
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

# edit Jsons/WhenItFails/categories.en.json

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .

git diff -- Jsons/WhenItFails/categories.en.json
```

PowerShell:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- summary .

# edit Jsons/WhenItFails/categories.en.json

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- summary .

git diff -- Jsons/WhenItFails/categories.en.json
```

## Category shape

A typical category contains:

```json
{
  "name": "STORAGE",
  "displayName": "Storage",
  "description": "Errors related to disks, volumes, storage devices, capacity, health, and storage operations.",
  "aliases": [ "DISK", "VOLUME" ],
  "parentCategories": [],
  "defaultTags": [ "STORAGE" ],
  "defaultMappings": {
    "defaultSeverity": "Error"
  }
}
```

Use the active catalog conventions.

Do not copy an example blindly.

## Name

The `name` field is the stable category identifier.

Example:

```json
"name": "STORAGE"
```

A good category name is:

- stable,
- uppercase if that is the catalog convention,
- machine-friendly,
- short,
- specific,
- not tied to a temporary implementation.

Good:

```text
STORAGE
CACHE
LOCALIZATION
MESSAGING
OBSERVABILITY
```

Weak:

```text
THING
MISC
TEMP
SPECIAL_CASES
NEW_ERRORS
```

## Display name

The `displayName` field is the human-readable label.

Example:

```json
"displayName": "Storage"
```

A good display name is:

- short,
- readable,
- suitable for terminal output,
- not overloaded with technical detail.

The display name may be nicer than the stable category name.

## Description

The `description` field explains what belongs in the category.

Good:

```json
"description": "Errors related to disks, volumes, storage devices, capacity, health, and storage operations."
```

Weak:

```json
"description": "Storage stuff."
```

The description should help future authors decide whether an error belongs in this category.

## Aliases

The `aliases` array lists alternate terms.

Example:

```json
"aliases": [ "DISK", "VOLUME" ]
```

Aliases are useful for:

- common synonyms,
- old terminology,
- related domain names,
- search or future tooling.

Do not add aliases that broaden the category too much.

Avoid aliases that belong to another category.

## Parent categories

The `parentCategories` array can describe category hierarchy.

Example:

```json
"parentCategories": [ "FILE_SYSTEM" ]
```

Use hierarchy sparingly.

A deep taxonomy can become harder to maintain than a flat one.

If parent categories are used, make sure the parent exists.

## Default tags

The `defaultTags` array describes tags naturally associated with the category.

Example:

```json
"defaultTags": [ "STORAGE" ]
```

Default tags should be:

- meaningful,
- stable,
- consistent with catalog tag style,
- useful for future tooling.

They are not a substitute for reviewing the tags on each error definition.

## Default mappings

The `defaultMappings` object stores category-level defaults.

Example:

```json
"defaultMappings": {
  "defaultSeverity": "Error"
}
```

Mappings are string key-value pairs.

They may guide consuming applications or future tools.

Do not put unclear flags into mappings.

Prefer predictable names.

## Choosing default severity

A category may have a default severity mapping.

Example:

```json
"defaultMappings": {
  "defaultSeverity": "Warning"
}
```

This does not mean every error in the category must use that severity.

It is a default hint.

Specific error definitions should still choose their own `defaultSeverity`.

## Add category references deliberately

After adding a category, update only the errors and profiles that should actually reference it.

Do not bulk-add the new category everywhere.

A category is useful only when references are meaningful.

## Updating errors

An error may reference the category through:

```text
primaryCategory
```

and/or:

```text
categories
```

Example:

```json
"primaryCategory": "STORAGE",
"categories": [ "STORAGE", "FILE_SYSTEM" ]
```

Use the new category as `primaryCategory` only when it is the main problem domain.

Use it in `categories` when it is relevant but not necessarily the main category.

## Updating profiles

Profiles may reference the category through:

```text
includeCategories
```

Example:

```json
"includeCategories": [ "STORAGE", "FILE_SYSTEM" ]
```

Add the category to a profile only if that profile should include errors from the new category.

Production-facing profiles deserve extra review.

## Validate references

After adding a category and references, run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

Expected exit code:

```text
0
```

Validation should catch unknown references or inconsistent catalog state.

## Inspect summary

Run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .
```

Check whether the category appears in primary-category counts if errors use it.

If the category does not appear, that may be fine.

A category can exist before errors use it, but unused categories should be intentional.

## Search affected errors

If you added errors that use the new category, list them:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --category STORAGE
```

Also search:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --search storage
```

Use the actual category name.

## Review profile effects

If profiles were changed, test them:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile DISK_TOOL
```

or:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile WEB
```

Use profiles that were actually affected.

## Category naming checklist

Before committing the category name, ask:

- Is it stable?
- Is it specific?
- Is it broad enough to reuse?
- Does it overlap heavily with an existing category?
- Is it a category rather than a tag?
- Is it a category rather than an owner?
- Would a new contributor understand it?
- Would it look sensible in summary output?

## Description checklist

A category description should answer:

```text
Which errors belong here?
```

and often:

```text
Which similar errors do not belong here?
```

Good descriptions reduce future catalog drift.

## Alias checklist

Before adding aliases, ask:

- Is the alias a real synonym?
- Is it likely to help authors?
- Could it conflict with another category?
- Is it too broad?
- Is it outdated but still useful?

Do not use aliases as a dumping ground for every related word.

## Parent category checklist

Before adding a parent category, ask:

- Does the parent exist?
- Is the relationship obvious?
- Does hierarchy improve understanding?
- Will it help future tooling?
- Is the hierarchy becoming too deep?

Flat categories are often easier to maintain.

## Default mapping checklist

Before adding default mappings, ask:

- Is the key predictable?
- Is the value intentionally a string?
- Is this truly a category-level default?
- Could it conflict with error-level severity?
- Is the behavior documented elsewhere if needed?

Avoid policy surprises.

## Common mistakes

Common category mistakes include:

- creating a category that duplicates an existing one,
- creating a category for one temporary error,
- using vague names like `MISC`,
- using display names in references instead of stable names,
- forgetting to validate after adding references,
- adding a category to profiles without review,
- using aliases as unrelated keywords,
- changing existing category names casually,
- using category as owner,
- using category as severity.

## Stable category names

Changing a category name later may break:

- error definitions,
- profiles,
- documentation,
- tests,
- scripts,
- runtime consumers.

Prefer adding aliases or migration support rather than renaming a released category casually.

## Deprecating a category

If a category becomes obsolete, do not delete it casually.

First check:

- existing error references,
- profile references,
- documentation links,
- tests,
- scripts,
- runtime consumers.

A future deprecation process may be better than immediate removal.

## Review Git diff

Run:

```bash
git diff -- Jsons/WhenItFails/categories.en.json
```

If profiles or errors were updated:

```bash
git diff -- Jsons/WhenItFails
```

Then:

```bash
git diff --check
```

Confirm:

- intended category was added,
- JSON formatting is clean,
- no unrelated categories changed,
- no unrelated profile changes appeared,
- no backup files are staged.

## Review checklist

Before commit, confirm:

```text
category name is stable
display name is readable
description is useful
aliases are intentional
parents exist if used
default tags are meaningful
default mappings are predictable
validation passes
summary looks reasonable
affected errors list correctly
affected profiles were reviewed
Git diff is intentional
```

## Commit message

Good:

```text
Add storage category
```

```text
Add localization category
```

```text
Add observability category
```

Weak:

```text
update categories
```

```text
json changes
```

```text
misc category stuff
```

## Related documentation

- [Catalog Files](../Catalog%20Files/en.md)
- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Adding a New Error Definition](../Adding%20a%20New%20Error%20Definition/en.md)
- [Adding a New Profile](../Adding%20a%20New%20Profile/en.md)
- [Validation](../Validation/en.md)
- [Workspace Summary](../Workspace%20Summary/en.md)
- [Browsing and Filtering Errors](../Browsing%20and%20Filtering%20Errors/en.md)
- [Glossary](../Glossary/en.md)

## Central principle

> Add a category when it gives the catalog a stable shared language for a real class of problems.
