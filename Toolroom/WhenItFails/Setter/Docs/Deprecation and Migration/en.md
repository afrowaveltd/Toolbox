# Deprecation and Migration

This guide explains how to deprecate or migrate WhenItFails catalog concepts safely.

It is intended for maintainers and catalog authors who need to change long-lived catalog entries without breaking users, scripts, tests, logs, documentation, or runtime consumers.

## Main principle

A catalog value can be easy to edit and still expensive to change.

Stable catalog values may be referenced by:

- code,
- tests,
- logs,
- documentation,
- support procedures,
- scripts,
- dashboards,
- external consumers,
- users.

Deprecation and migration should be deliberate.

## What may need deprecation

Catalog concepts that may need deprecation include:

- error definitions,
- numeric codes,
- error IDs,
- error names,
- categories,
- code groups,
- owners,
- profiles,
- tags,
- documentation keys,
- mapping keys.

The more stable and public the value is, the more careful the migration should be.

## Prefer additive changes

When possible, prefer an additive change over a breaking rename or deletion.

Safer:

```text
add a new error definition
keep old one valid
mark old one as deprecated through documentation or metadata if supported
```

Riskier:

```text
rename old error ID
reuse old numeric code
delete old error immediately
```

Additive changes give consumers time to move.

## Avoid casual renames

Do not rename stable identifiers just to make them prettier.

Stable identifiers include:

```text
error id
numeric code
error name
owner name
code group name
code prefix
category name
profile name
documentation key
mapping key
```

If a rename is only cosmetic, it is usually not worth the compatibility risk.

## When deprecation is justified

Deprecation may be justified when:

- the concept is misleading,
- the concept was modeled incorrectly,
- two concepts duplicate each other,
- a name contains a serious typo,
- a category became too broad or too narrow,
- a profile exposes unsafe behavior,
- a mapping key was poorly named,
- a numeric range conflicts with catalog policy,
- a replacement exists.

Deprecation should solve a real problem.

## When migration is required

Migration is required when consumers must move from an old value to a new value.

Examples:

```text
old category name
→ new category name
```

```text
old profile name
→ new profile name
```

```text
old documentation key
→ new documentation key
```

```text
old error definition
→ replacement error definition
```

A migration should explain both what changed and how to update references.

## Do not reuse stable values casually

Avoid reusing:

```text
numeric codes
error IDs
error names
profile names
category names
code prefixes
```

for different meanings.

A reused code can make old logs impossible to interpret correctly.

If a value was released, treat it as historical memory.

## Error definition deprecation

An error definition may need deprecation when:

- it duplicates another error,
- it describes a concept too vaguely,
- it exposes unsafe wording,
- it was assigned to the wrong family,
- it is replaced by a more precise definition.

Do not immediately delete a released error definition unless there is a strong reason.

## Error definition migration pattern

Recommended pattern:

```text
add replacement error definition
→ update documentation
→ update runtime code to emit replacement
→ keep old definition valid for compatibility
→ mark old definition as deprecated if supported
→ remove only after a deliberate compatibility window
```

This keeps old logs and support references understandable.

## Numeric code migration

Numeric codes are especially sensitive.

Before changing a numeric code, ask:

- Has it appeared in logs?
- Is it referenced in docs?
- Is it used by support?
- Is it used by tests?
- Is it consumed by external systems?
- Can the old code remain valid?
- Should a new error be added instead?

Usually, adding a new error with a new code is safer than changing an existing code.

## Error ID migration

Error IDs are stable human-readable references.

Before changing an ID, ask:

- Is the old ID in documentation?
- Is it used in tests?
- Is it used in scripts?
- Does runtime code reference it?
- Would aliases or documentation solve the issue?
- Is the new ID worth the break?

Do not rename IDs only for visual neatness.

## Error name migration

The `name` field may be used by code or lookup.

Before changing it, check:

- source references,
- tests,
- docs,
- examples,
- support material.

If the old name is misleading, consider whether the catalog can support aliases or a documented replacement.

## Category deprecation

A category may need deprecation when:

- it is too vague,
- it duplicates another category,
- it is consistently misused,
- it represents an implementation detail rather than a problem domain,
- a better category structure exists.

Avoid deleting a category while errors or profiles still reference it.

## Category migration pattern

Recommended pattern:

```text
add new category
→ update affected error definitions
→ update affected profiles
→ update documentation
→ validate
→ keep old category if compatibility requires it
→ remove later only when references are gone
```

Check references with:

```bash
grep -R "OLD_CATEGORY" Jsons/WhenItFails
```

PowerShell:

```powershell
Select-String `
  -Path Jsons/WhenItFails/*.json `
  -Pattern "OLD_CATEGORY"
```

## Code group deprecation

A code group may need deprecation when:

- its range was poorly chosen,
- its prefix is misleading,
- it overlaps another group,
- its concept is better modeled by another group,
- it was too narrow or too broad.

Code group migration is risky because it can affect both numeric codes and symbolic IDs.

## Code group migration pattern

Recommended pattern:

```text
add new code group
→ add new error definitions in the new range
→ update runtime emitters to use new definitions
→ keep old definitions valid
→ update profiles if needed
→ document replacement
```

Avoid changing numeric codes in place.

Old logs should remain interpretable.

## Owner deprecation

An owner may need deprecation when:

- responsibility moved,
- the owner was too narrow,
- the owner was temporary,
- the owner is merged into another responsibility boundary,
- the owner split into more specific owners.

Owner changes can affect support workflows and profile filters.

## Owner migration pattern

Recommended pattern:

```text
add new owner if needed
→ update new or active definitions
→ keep old definitions valid
→ update affected profiles
→ document ownership transition
→ avoid immediate deletion
```

Do not move all historical errors without considering log and support references.

## Profile deprecation

A profile may need deprecation when:

- it exposes unsafe defaults,
- it duplicates another profile,
- it is unused,
- its name is misleading,
- the application context no longer exists.

Profiles can be used by applications, tests, and documentation.

Treat profile names as public behavior.

## Profile migration pattern

Recommended pattern:

```text
add replacement profile
→ update applications or docs to use replacement
→ keep old profile temporarily
→ document that old profile is deprecated
→ remove only after references are gone
```

For production-facing profile changes, review safety carefully.

## Tag deprecation

A tag may need deprecation when:

- it is vague,
- it duplicates another tag,
- it is misspelled,
- it is not used consistently,
- it creates unsafe assumptions.

Before removing or renaming a tag, check:

```bash
grep -R "OLD_TAG" Jsons/WhenItFails
```

PowerShell:

```powershell
Select-String `
  -Path Jsons/WhenItFails/*.json `
  -Pattern "OLD_TAG"
```

## Documentation key migration

Documentation keys may be used by tools or generated links.

Before changing a documentation key, ask:

- Is it used by runtime code?
- Is it referenced in docs?
- Is it used by a website?
- Is it stored in logs?
- Can old links redirect?
- Can the old key remain as an alias?

If possible, preserve old keys or redirects.

## Mapping key migration

Mapping keys may be consumed by applications.

Before changing a mapping key, ask:

- Which application reads it?
- Is it documented?
- Is it in tests?
- Is the old key still accepted?
- Is a compatibility fallback needed?

A mapping key is effectively a small configuration API.

Treat it accordingly.

## Deprecation metadata

If the catalog gains formal deprecation metadata later, use it consistently.

Potential future fields could include:

```text
isDeprecated
deprecatedSince
replacementId
replacementName
deprecationReason
removeAfter
```

Current catalog behavior may not support these fields as formal schema.

Do not add unsupported fields unless validation and models allow them.

## Documentation-only deprecation

If formal metadata is not available, document deprecation clearly in:

- the relevant guide,
- release notes,
- migration notes,
- comments only if the JSON loader and project convention allow comments.

Prefer validated catalog fields over informal notes.

## Migration notes

A migration note should include:

```text
old value
new value
reason
impact
how to update
compatibility period
tests to run
```

Example:

```markdown
## Migration

`OLD_PROFILE` is replaced by `PUBLIC_API`.

Use `PUBLIC_API` for public HTTP API responses. The old profile remains available for one release window but should not be used for new code.
```

## Compatibility window

A compatibility window is a period where old and new values both remain valid.

Use a compatibility window when:

- runtime consumers need time to update,
- documentation links are already public,
- old logs must remain meaningful,
- external scripts may reference old values,
- the project is published as a package.

The length depends on project policy.

## Removal checklist

Before removing a deprecated value, check:

- no catalog references remain,
- no source references remain,
- no tests reference it unless testing migration,
- no documentation examples use it,
- no profiles include it,
- no release notes say it is still available,
- migration path has been documented,
- compatibility window has passed if applicable.

Search first.

Bash:

```bash
grep -R "DEPRECATED_VALUE" .
```

PowerShell:

```powershell
Select-String `
  -Path . `
  -Pattern "DEPRECATED_VALUE" `
  -Recurse
```

## Validation after migration

After any migration, run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

Then inspect summary:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .
```

Then inspect affected errors or profiles.

Example:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001
```

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile PUBLIC_API
```

## Git diff review

Migration diffs can be noisy.

Review carefully:

```bash
git diff --stat
git diff -- Jsons/WhenItFails
git diff --check
```

Confirm:

- only intended values changed,
- old and new values are not mixed accidentally,
- no unrelated formatting churn occurred,
- no backup files are staged,
- documentation was updated.

## Tests for migration

Migration-related tests should cover:

- old value still works if compatibility is promised,
- new value works,
- deprecated references are reported if validation supports that,
- removed values fail with useful issue codes,
- profiles still select intended errors,
- documentation examples use new values.

Do not rely only on manual review for behavior changes.

## Release notes

Deprecations and migrations should be visible in release notes.

Example:

```markdown
## Deprecated

- Deprecated `OLD_PROFILE`; use `PUBLIC_API` instead.

## Migration

- Update command examples and runtime configuration to use `PUBLIC_API`.
- `OLD_PROFILE` remains available during the compatibility window.
```

For breaking removals:

```markdown
## Breaking changes

- Removed `OLD_PROFILE` after the documented compatibility window.
```

## Common migration mistakes

Common mistakes include:

- renaming a stable value without migration notes,
- deleting a value still referenced by profiles,
- changing numeric codes in place,
- reusing old codes for new meanings,
- updating JSON but not documentation,
- updating documentation but not tests,
- forgetting runtime consumers,
- forgetting release notes,
- assuming search found everything,
- committing unrelated formatting churn.

## Safe migration workflow

Recommended workflow:

```text
identify old value
→ search all references
→ add replacement
→ update active references
→ validate
→ test
→ document migration
→ review diff
→ commit
```

If removal is needed later:

```text
confirm no references
→ remove old value
→ validate
→ test
→ document removal
→ review diff
→ commit
```

Keep addition/migration/removal as separate commits when possible.

## Bash reference search

Search JSON catalogs:

```bash
grep -R "OLD_VALUE" Jsons/WhenItFails
```

Search docs:

```bash
grep -R "OLD_VALUE" Toolroom/WhenItFails/Setter/Docs
```

Search source and tests:

```bash
grep -R "OLD_VALUE" WhenItFails Toolroom/WhenItFails
```

Search everything:

```bash
grep -R "OLD_VALUE" .
```

## PowerShell reference search

Search JSON catalogs:

```powershell
Select-String `
  -Path Jsons/WhenItFails/*.json `
  -Pattern "OLD_VALUE"
```

Search docs:

```powershell
Get-ChildItem `
  -Path Toolroom/WhenItFails/Setter/Docs `
  -Recurse `
  -File |
Select-String `
  -Pattern "OLD_VALUE"
```

Search everything:

```powershell
Get-ChildItem `
  -Path . `
  -Recurse `
  -File |
Select-String `
  -Pattern "OLD_VALUE"
```

## Review checklist

Before committing a deprecation or migration, confirm:

```text
old value was searched
replacement value is stable
migration reason is documented
active references were updated
compatibility behavior is clear
validation passes
tests pass
affected profiles were checked
affected docs were updated
release notes are planned if needed
Git diff is intentional
```

## Commit message examples

Good:

```text
Deprecate old public API profile
```

```text
Migrate storage errors to storage category
```

```text
Add replacement network timeout error
```

```text
Remove deprecated legacy profile
```

Weak:

```text
rename stuff
```

```text
cleanup
```

```text
fix json
```

## Related documentation

- [Naming and Numbering Conventions](../Naming%20and%20Numbering%20Conventions/en.md)
- [Catalog Files](../Catalog%20Files/en.md)
- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Adding a New Error Definition](../Adding%20a%20New%20Error%20Definition/en.md)
- [Adding a New Category](../Adding%20a%20New%20Category/en.md)
- [Adding a New Code Group](../Adding%20a%20New%20Code%20Group/en.md)
- [Adding a New Owner](../Adding%20a%20New%20Owner/en.md)
- [Adding a New Profile](../Adding%20a%20New%20Profile/en.md)
- [Release Checklist](../Release%20Checklist/en.md)
- [Testing and CI](../Testing%20and%20CI/en.md)

## Central principle

> Deprecation is how a catalog changes its mind without erasing its memory.
