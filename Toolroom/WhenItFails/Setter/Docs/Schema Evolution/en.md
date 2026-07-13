# Schema Evolution

This guide explains how to evolve the WhenItFails catalog schema safely.

It is intended for maintainers who change catalog document shapes, model classes, validation rules, or compatibility behavior.

Schema changes are more serious than content changes.

A content change modifies catalog data.

A schema change modifies what catalog data is allowed to look like.

## Main principle

Schema evolution should be:

```text
deliberate
compatible where possible
validated
documented
tested
migration-aware
```

Do not change catalog shape casually.

Even a small field rename can break tools, tests, documentation, runtime consumers, or project-local catalogs.

## What counts as schema evolution

Schema evolution includes changes such as:

- adding a new field,
- removing a field,
- renaming a field,
- changing a field type,
- making an optional field required,
- changing allowed values,
- changing default behavior,
- changing validation rules,
- changing catalog file names,
- changing root collection names,
- changing cross-file reference rules,
- changing mapping semantics.

Examples:

```text
adding `isDeprecated`
```

```text
renaming `defaultSeverity` to `severity`
```

```text
changing `defaultMappings` values from strings to booleans
```

```text
making `documentationKey` required
```

```text
changing `profiles.json` to `profiles.en.json`
```

Each of these affects compatibility.

## What is not schema evolution

Not every catalog change is a schema change.

Usually not schema evolution:

- adding a new error definition,
- adding a new category,
- adding a new profile,
- changing a title,
- changing a message,
- changing severity of one error,
- adding a tag using existing tag behavior,
- adding a documentation page.

These are content changes unless they require model or validation changes.

## Schema version

Catalog files commonly contain:

```text
schemaVersion
```

Example:

```json
"schemaVersion": "1.0"
```

The schema version describes the document shape and compatibility expectations.

Do not bump schema version for ordinary content changes.

Bump or change schema version only when the catalog shape or interpretation changes enough that consumers need to know.

## Versioning mindset

Think in terms of compatibility.

Safe additive change:

```text
new optional field
old readers can ignore it
new readers can use it
```

Behavioral change:

```text
new validation rule
old catalogs may now fail
```

Breaking change:

```text
renamed field
removed field
changed type
changed root collection name
```

Breaking schema changes require migration planning.

## Prefer additive fields

When possible, add optional fields instead of renaming or removing existing ones.

Safer:

```json
{
  "documentationKey": "when-it-fails/errors/network/network-timeout",
  "documentationUrl": "https://example.test/docs/network-timeout"
}
```

Riskier:

```json
{
  "documentationUrl": "https://example.test/docs/network-timeout"
}
```

with `documentationKey` removed.

An additive field allows old consumers to keep working.

## Optional first, required later

When introducing an important new field, consider this staged path:

```text
add optional field
→ document recommended usage
→ add warnings or soft validation if supported
→ migrate default catalogs
→ update consumers
→ later make required if truly necessary
```

Do not make a new required field without considering existing catalogs.

## Avoid field renames

A field rename is effectively removal plus addition.

Risky:

```text
defaultSeverity
→ severity
```

This can break:

- JSON readers,
- tests,
- documentation,
- manual examples,
- scripts,
- project-local catalogs.

Prefer keeping the old field unless the cost of keeping it is higher than migration cost.

## If a rename is necessary

If a field rename is necessary:

1. Add support for both old and new fields if possible.
2. Prefer the new field when both are present.
3. Emit a warning or validation issue for old field if supported.
4. Update default catalogs.
5. Update documentation.
6. Update tests.
7. Provide migration notes.
8. Remove old support only after a deliberate compatibility window.

Do not rename in one silent step.

## Field type changes

Changing a field type is risky.

Example:

```json
"defaultMappings": {
  "cli.includeHints": "true"
}
```

to:

```json
"defaultMappings": {
  "cli.includeHints": true
}
```

This changes string values to booleans.

Before doing this, consider:

- current model classes,
- JSON serialization,
- validation,
- documentation examples,
- consumers parsing strings,
- existing project-local catalogs.

A type change usually needs migration support.

## Root collection changes

Root collection names are important.

Examples:

```text
errors
categories
codeGroups
owners
profiles
```

Changing a root collection name is usually breaking.

Avoid:

```text
codeGroups
→ groups
```

unless there is a migration plan.

## File name changes

Current package files include:

```text
errors.en.json
categories.en.json
code-groups.en.json
owners.en.json
profiles.json
```

Changing file names can break:

- workspace resolver assumptions,
- validation,
- docs,
- tests,
- scripts,
- users.

If file names must change, consider supporting both old and new names for a compatibility window.

## Cross-file reference changes

Cross-file references are part of the schema contract.

Examples:

```text
error.owner
→ owners[].name
```

```text
error.codeGroup
→ codeGroups[].name
```

```text
error.primaryCategory
→ categories[].name
```

Changing reference semantics can break many catalogs.

Document and test these changes carefully.

## Allowed value changes

Allowed values include things such as severity names:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Adding a new allowed value may be additive for new consumers but confusing for old consumers.

Removing or renaming an allowed value is breaking.

Before changing allowed values, update:

- validation,
- edit commands,
- documentation,
- tests,
- runtime consumers.

## Validation changes

Validation changes can be schema evolution even without changing JSON fields.

Example:

```text
previously category aliases could be empty
now aliases are required
```

This may break existing catalogs.

Before adding stricter validation, ask:

- Which existing catalogs will fail?
- Is the rule necessary?
- Can it begin as a warning?
- Is migration guidance available?
- Are tests updated?
- Is the documentation updated?

## Soft validation

If the validation system supports severity levels in the future, consider staged validation:

```text
warning
→ error later
```

This allows catalog authors to adapt.

Current behavior may not support warning-level validation as a formal compatibility mechanism.

If it does not, document stricter rules clearly.

## Migration support

Schema evolution may need migration support.

Migration support can include:

- manual migration guide,
- command-line migration command,
- compatibility reader,
- validation explanation,
- sample before/after JSON,
- release notes.

Do not assume users can infer migration steps from code changes.

## Migration command caution

A future migration command should be explicit.

Good:

```text
migrate-schema <path> --from 1.0 --to 1.1
```

Risky:

```text
validate
```

silently rewriting catalogs.

Validation should not unexpectedly mutate files.

## Backward compatibility readers

A compatibility reader may accept old schema versions.

If used, document:

- which versions are accepted,
- how old fields map to new fields,
- whether output writes new format,
- whether old fields are preserved,
- when support ends.

Compatibility behavior must be tested.

## Writing new format

If a tool reads an old schema and writes a new schema, that is a migration.

It should not be surprising.

The command should explain what changed and create backups where applicable.

## Schema version bump checklist

Before changing `schemaVersion`, ask:

- What changed in document shape?
- Is the change additive or breaking?
- Can old readers safely ignore it?
- Can new readers read old files?
- Are migrations needed?
- Are docs updated?
- Are tests updated?
- Are sample catalogs updated?
- Are release notes updated?

Do not bump schema version without a reason.

## Proposed versioning style

A simple schema version policy can be:

```text
1.0
→ initial stable shape
```

```text
1.1
→ backward-compatible additive changes
```

```text
2.0
→ breaking shape changes
```

Use the actual project policy if one is defined.

Keep the policy simple enough that contributors can follow it.

## Adding a new optional field

Recommended flow:

```text
define purpose
→ add model field
→ keep it optional
→ update loader/serializer
→ update validation if needed
→ update docs
→ update examples if helpful
→ add tests
→ validate default catalogs
```

Example field:

```text
isDeprecated
```

If optional, old catalog files can remain valid.

## Adding a new required field

Recommended flow:

```text
avoid if possible
```

If truly necessary:

```text
define purpose
→ add optional support first
→ update default catalogs
→ provide migration guidance
→ update validation after migration window
→ update tests and docs
```

A new required field can break every existing project-local catalog.

## Removing a field

Removing a field is breaking.

Before removal:

- search all references,
- update models,
- update serialization,
- update validation,
- update docs,
- update tests,
- provide migration notes,
- consider compatibility support,
- consider deprecation period.

Do not remove a field just because it is not used by current code.

External consumers may use it.

## Changing mappings

`defaultMappings` is intentionally flexible.

Changing mapping conventions can still break consumers.

Before changing mapping keys or values:

- identify consumers,
- provide new keys additively,
- keep old keys temporarily,
- document precedence,
- add tests,
- update examples.

Mapping keys are small configuration APIs.

Treat them carefully.

## Example: adding deprecation metadata

Potential future fields:

```json
{
  "isDeprecated": true,
  "deprecatedSince": "1.2",
  "replacementId": "AFW_NET_0002",
  "deprecationReason": "Replaced by a more specific timeout error."
}
```

Safe approach:

```text
add fields as optional
→ validate types only when present
→ document usage
→ update detail view if needed
→ add tests
```

Do not make all existing errors include deprecation fields.

## Example: adding localization support

Potential future change:

```text
errors.en.json
errors.cs.json
```

Questions to answer:

- Do localized files contain full definitions or text only?
- What fields are language-neutral?
- How are missing translations handled?
- Does validation compare language files?
- How does Setter choose language?
- Are docs and examples updated?

Localization is schema and workflow evolution, not just file duplication.

## Example: JSON output mode

JSON output from commands is not catalog schema, but it is still a public schema.

If added, define:

- output version,
- success shape,
- failure shape,
- issue shape,
- command-specific payload shape,
- compatibility rules,
- tests.

Do not mix command output schema with catalog schema without documenting both.

## Documentation requirements

A schema change may require updates to:

- Catalog Files,
- Glossary,
- Naming and Numbering Conventions,
- Deprecation and Migration,
- Catalog Author Checklist,
- Adding a New Error Definition,
- Adding a New Category,
- Adding a New Code Group,
- Adding a New Owner,
- Adding a New Profile,
- Validation,
- Release Checklist,
- Testing and CI.

Update only relevant pages, but do not leave stale examples behind.

## Test requirements

Schema changes should usually include tests for:

- old valid catalog still valid if compatibility is promised,
- new valid catalog is accepted,
- invalid new shape is rejected,
- serializer writes expected shape,
- validation issues are useful,
- command behavior remains compatible,
- migration behavior if supported.

For breaking changes, tests should prove the new behavior and migration expectations.

## Default catalog update

If schema changes affect default catalogs, update:

```text
Jsons/WhenItFails/*.json
```

Then run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

Also inspect summary:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .
```

## Search references before changing schema

Bash:

```bash
grep -R "fieldName" .
```

PowerShell:

```powershell
Get-ChildItem `
  -Path . `
  -Recurse `
  -File |
Select-String `
  -Pattern "fieldName"
```

Search:

- source code,
- tests,
- documentation,
- JSON catalogs,
- examples,
- scripts.

## Git diff review

Schema changes can create large diffs.

Review carefully:

```bash
git diff --stat
git diff -- Jsons/WhenItFails
git diff -- Toolroom/WhenItFails
git diff -- WhenItFails
git diff --check
```

Confirm:

- shape changes are intentional,
- docs match code,
- tests match docs,
- no backup files are included,
- no unrelated formatting churn occurred.

## Release notes

Schema changes should be mentioned in release notes.

For additive changes:

```markdown
## Added

- Added optional `isDeprecated` metadata to error definitions.
```

For breaking changes:

```markdown
## Breaking changes

- Renamed `oldField` to `newField`.
- Existing catalogs must be migrated before validation succeeds.
```

For migration:

```markdown
## Migration

- Run the schema migration guide in `Docs/Schema Evolution/en.md`.
```

## Review checklist

Before merging schema evolution, confirm:

```text
schema change is necessary
compatibility impact is understood
schemaVersion decision is made
models are updated
validation is updated
serialization is updated
default catalogs validate
tests cover old/new behavior as needed
documentation is updated
migration notes exist if needed
release notes are planned
Git diff is intentional
```

## Common mistakes

Common schema-evolution mistakes include:

- changing JSON shape without bumping or considering schema version,
- bumping schema version for ordinary content changes,
- renaming fields without migration,
- changing mapping values from strings to booleans silently,
- making optional fields required too early,
- updating models but not documentation,
- updating docs but not tests,
- validating too strictly without migration guidance,
- silently rewriting catalogs during validation,
- breaking project-local catalogs accidentally.

## Related documentation

- [Catalog Files](../Catalog%20Files/en.md)
- [Naming and Numbering Conventions](../Naming%20and%20Numbering%20Conventions/en.md)
- [Deprecation and Migration](../Deprecation%20and%20Migration/en.md)
- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Validation](../Validation/en.md)
- [Testing and CI](../Testing%20and%20CI/en.md)
- [Release Checklist](../Release%20Checklist/en.md)
- [Maintainer Notes](../Maintainer%20Notes/en.md)

## Central principle

> Evolve the schema like a public contract: add carefully, break rarely, migrate clearly, and validate everything.
