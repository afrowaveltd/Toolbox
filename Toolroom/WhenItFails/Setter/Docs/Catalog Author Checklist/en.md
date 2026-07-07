# Catalog author checklist

This checklist is for people who edit WhenItFails catalog content.

Use it before changing catalog files, while editing them, and before committing the result.

The checklist is intentionally practical and repetitive.

Catalog work is safest when every change is small, validated, inspected, and reviewed.

## Scope

This checklist applies to catalog files under:

```text
Jsons/WhenItFails
```

Typical files:

```text
errors.en.json
categories.en.json
code-groups.en.json
owners.en.json
profiles.json
```

## Basic rule

For every meaningful catalog change:

```text
inspect
→ edit
→ validate
→ inspect again
→ review diff
→ commit
```

Do not skip validation just because the change looks small.

Small JSON changes can still break cross-file relationships.

## Before you start

Confirm the repository root:

```bash
pwd
```

Confirm the workspace path:

```bash
realpath Jsons/WhenItFails
```

Confirm Git state:

```bash
git status --short
```

Run validation before changing anything:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

Run summary:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .
```

Starting from a known valid workspace makes later failures easier to understand.

## Confirm you are editing the right workspace

Common mistake:

```text
editing a copied workspace
while validating another workspace
```

Check:

```bash
realpath Jsons/WhenItFails
```

and compare it with the path passed to Setter.

If you use a temporary workspace, always store it in a named variable:

```bash
test_root="$(mktemp -d /tmp/when-it-fails-edit-XXXXXXXXXX)"
```

Then use the variable consistently:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate "$test_root"
```

## Choose the smallest safe change

Prefer one semantic change at a time.

Good:

```text
change one title
validate
commit
```

Good:

```text
add one category
validate
commit
```

Risky:

```text
rename categories
change profiles
change severities
rewrite messages
add owners
reformat all JSON
commit everything together
```

Large edits are sometimes necessary, but they should be deliberate and carefully reviewed.

## Use Setter commands where available

Prefer Setter edit commands for supported error fields:

```text
set-title
set-message
set-developer-hint
set-severity
set-documentation-key
```

Example:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-title . \
  AFW_NET_0001 \
  "Network is not available"
```

Setter commands provide:

- lookup by ID, code, or name,
- validation,
- safe writes,
- timestamped backups,
- consistent JSON serialization.

## Use manual editing only where needed

Manual JSON editing is currently needed for:

- adding a new error,
- adding a category,
- adding a code group,
- adding an owner,
- adding or changing profiles,
- changing fields not yet supported by Setter edit commands.

After manual edits, run validation immediately.

## Error definition checklist

When adding or reviewing an error definition, check:

- `id` is stable and follows project convention.
- `code` is unique.
- `code` is inside the intended code-group range.
- `name` is stable and machine-friendly.
- `owner` references a known owner.
- `codePrefix` matches the selected code group.
- `codeGroup` references a known code group.
- `primaryCategory` references a known category.
- `categories` contains known categories.
- `subcategories` are intentional and consistently named.
- `title` is short and human-readable.
- `message` is a complete reusable sentence.
- `defaultSeverity` is supported and correctly cased.
- `developerHint` is actionable and safe.
- `documentationKey` is stable and predictable.
- `tags` are meaningful and stable.
- `metadata` is used only for advanced structured data.

## Error ID checklist

Good error IDs are:

- stable,
- readable,
- unique,
- consistent with active catalog style,
- not tied to temporary wording,
- not renumbered casually.

Example:

```text
AFW_NET_0001
```

Avoid:

```text
ERROR1
TEMP_ERROR
NETWORK_ERROR_FINAL
PLEASE_DELETE
```

## Numeric code checklist

Check that the code:

- is an integer,
- is unique,
- belongs to the intended code group,
- belongs to the intended owner range where applicable,
- does not reuse a retired code without deliberate reason,
- is not changed after release unless compatibility is considered.

Example:

```json
"code": 600001
```

## Code group checklist

When choosing or adding a code group, check:

- `name` is stable.
- `displayName` is readable.
- `codePrefix` is short and unique.
- `codeFrom` and `codeTo` define a clear range.
- range does not overlap unintentionally.
- `defaultCategories` reference existing categories.
- `defaultTags` are meaningful.
- description explains the group.

## Owner checklist

When choosing or adding an owner, check:

- `name` is stable.
- `displayName` is readable.
- code range is intentional.
- `isBuiltIn` is correct.
- aliases are useful and not misleading.
- default mappings are policy-like and understandable.

Do not use owners as categories.

Owners describe responsibility.

Categories describe the problem domain.

## Category checklist

When choosing or adding a category, check:

- `name` is stable.
- `displayName` is readable.
- description is specific.
- aliases are useful.
- parent categories are intentional.
- default tags are meaningful.
- default mappings do not conflict with error-level decisions.

Do not create a new category if an existing category clearly fits.

Do create a new category when repeated errors need a stable shared classification.

## Profile checklist

When adding or changing a profile, check:

- `name` is stable.
- `displayName` is human-readable.
- description states the target context.
- included owners are intentional.
- included code groups are intentional.
- included categories are intentional.
- included and excluded tags are intentional.
- default mappings are safe for the target context.
- production profiles do not expose sensitive details.
- development profiles are clearly not production profiles.

Then test:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile <PROFILE_NAME>
```

## Text authoring checklist

For `title`, ask:

- Is it short?
- Is it specific?
- Does it avoid implementation details?
- Does it avoid a final period?
- Does it differ from the message?

For `message`, ask:

- Is it a complete sentence?
- Does it state what is known?
- Does it avoid blaming the user?
- Does it avoid unsupported assumptions?
- Is it reusable?
- Is it safe to display?

For `developerHint`, ask:

- Is it actionable?
- Does it suggest plausible checks?
- Does it avoid claiming an unproven cause?
- Is it useful to developers or operators?
- Is it free of secrets?

## Severity checklist

Supported values:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Ask:

```text
diagnostic only?
→ Trace or Debug
```

```text
notable but not harmful?
→ Information
```

```text
degraded but continued?
→ Warning
```

```text
operation failed?
→ Error
```

```text
system or data safety threatened?
→ Critical
```

Do not choose severity based on frustration.

Choose it based on operational impact.

## Documentation key checklist

A good documentation key is:

- stable,
- predictable,
- lowercase where project convention expects it,
- not a temporary URL,
- not a local filesystem path,
- not secret,
- connected to the same error concept.

Example:

```text
when-it-fails/errors/network/network-unavailable
```

Avoid:

```text
C:\Users\Me\Desktop\final-error-help.md
```

```text
http://localhost:5000/test
```

```text
page-final-v3
```

## Tag checklist

Tags should be:

- stable,
- meaningful,
- uppercase if that is the catalog convention,
- useful for searching, filtering, profiles, or future tooling.

Good:

```text
USER_VISIBLE
NETWORK
DEBUG_ONLY
INTERNAL_ONLY
```

Weak:

```text
TEMP
MISC
THING
TODO
```

Do not create tags that nobody can interpret later.

## Security checklist

Before committing catalog text, confirm it does not contain:

- passwords,
- tokens,
- connection strings,
- private filesystem paths,
- customer identifiers,
- internal hostnames unless intentional,
- raw SQL with sensitive data,
- personal data,
- stack traces in production-oriented messages,
- secret URLs.

Catalog files are likely to be stored in source control and may be distributed.

## Localization readiness checklist

Even if English is the only current language, write text that can be translated later.

Prefer:

- complete sentences,
- clear subject and object,
- simple punctuation,
- no concatenated sentence fragments,
- no culture-specific jokes in operational errors,
- placeholders only when the tooling supports them.

Avoid:

```text
"File " + name + " failed"
```

Prefer a full sentence template when placeholders become supported.

## Manual JSON editing checklist

Before saving manual JSON edits, check:

- brackets and braces are balanced,
- commas are valid,
- strings are quoted,
- property names are correct,
- arrays remain arrays,
- numeric codes remain numbers,
- boolean-like mapping values are intentionally strings where the model expects strings,
- no accidental editor auto-format changed unrelated sections.

Then run validation.

## Before changing stable identifiers

Stable identifiers include:

```text
error id
error name
numeric code
owner name
code group name
code prefix
category name
profile name
```

Before changing one, ask:

- Is this already referenced in code?
- Is it referenced in docs?
- Is it used by tests?
- Is it used in scripts?
- Is it visible to users or support?
- Is this change worth the compatibility cost?
- Should this be a new entry instead of a rename?

Renames need more care than wording changes.

## Before changing ranges

Range fields include:

```text
codeFrom
codeTo
```

Before changing ranges, ask:

- Which existing errors are inside the old range?
- Which existing errors move out of range?
- Does another range overlap?
- Are profiles or docs affected?
- Is this a breaking catalog change?
- Is migration needed?

Always validate after range changes.

## Before changing profiles

Before changing profile policy, ask:

- Which applications use this profile?
- Is it a development or production profile?
- Does it expose more information than before?
- Does it hide information that support needs?
- Does `errors --profile` still return plausible results?
- Do runtime consumers interpret mappings the same way?
- Should this be a new profile instead of changing an existing one?

Production-facing changes deserve extra review.

## Validation loop

Run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

Expected:

```text
exit code 0
```

If validation fails, do not continue editing blindly.

Fix the first clear issue, then validate again.

## Inspection loop

After editing an error field:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001
```

After editing profiles:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile WEB
```

After editing categories or code groups:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .
```

## Git diff checklist

Run:

```bash
git diff --stat
```

Then:

```bash
git diff -- Jsons/WhenItFails
```

Then:

```bash
git diff --check
```

Confirm:

- only intended files changed,
- no unrelated reformatting occurred,
- no backup files are being committed accidentally,
- no temporary files are present,
- changed wording is intentional,
- changed identifiers are intentional,
- changed profile policy is intentional.

## Backup file checklist

Setter backups look like:

```text
errors.en.<timestamp>.bak.json
```

Before committing, check:

```bash
git status --short
```

If backup files appear, decide deliberately whether project policy allows them in Git.

Most projects should not commit local timestamped backups.

## Temporary file checklist

Temporary files may look like:

```text
.errors.en.json.<guid>.tmp
```

If a temporary file remains:

- confirm no Setter process is running,
- inspect it if recovery may be needed,
- do not commit it,
- delete it only after it is understood.

## Commit checklist

Before committing:

```bash
dotnet build
dotnet test
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
git diff --check
git status --short
```

Commit message should describe the catalog change.

Examples:

```text
Update network error wording
```

```text
Add storage diagnostics profile
```

```text
Add external service timeout error
```

```text
Refine production profile mappings
```

## Good commit size

A good catalog commit usually contains one clear idea.

Good:

```text
Add storage code group
```

Good:

```text
Update validation error messages
```

Good:

```text
Add production-safe profile mappings
```

Risky:

```text
Update everything
```

```text
Many fixes
```

```text
Catalog cleanup
```

Large cleanup commits should have a clear plan and careful review.

## Review checklist for pull requests

A reviewer should check:

- validation passes,
- tests pass,
- changed files are expected,
- stable identifiers were not changed casually,
- new references point to existing catalog entries,
- severity choices are reasonable,
- production mappings remain safe,
- messages avoid secrets,
- documentation keys are stable,
- backups and temporary files are not included,
- diffs are readable.

## Negative test checklist

When creating a negative validation test:

- use a disposable workspace,
- mutate exactly one thing,
- confirm the mutation happened,
- expect a specific non-zero exit code,
- clean up the workspace.

Bash pattern:

```bash
test_root="$(mktemp -d /tmp/when-it-fails-negative-XXXXXXXXXX)"
trap 'rm -rf "$test_root"' EXIT
```

Do not damage the real workspace for negative tests.

## Disposable workspace checklist

Use disposable workspaces for experiments.

Create:

```bash
test_root="$(mktemp -d /tmp/when-it-fails-edit-XXXXXXXXXX)"
```

Initialize:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- init "$test_root"
```

Validate:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate "$test_root"
```

Clean up:

```bash
rm -rf "$test_root"
```

## Recovery checklist

If a catalog edit goes wrong:

1. Stop active writers.
2. Confirm the workspace path.
3. Preserve the current active file.
4. List backups.
5. Compare candidate backups.
6. Test the candidate in a temporary workspace.
7. Restore deliberately.
8. Validate.
9. Inspect affected definitions.
10. Review Git diff.

Do not restore the newest backup blindly.

## “Do not” checklist

Do not:

- edit the wrong workspace,
- skip validation,
- commit backup files accidentally,
- commit temporary files,
- change stable IDs casually,
- use display names where stable names are required,
- copy development mappings into production,
- expose secrets in messages,
- use severity as an emotional label,
- create vague categories,
- create profiles that duplicate existing profiles without reason,
- reformat whole catalog files unintentionally,
- run normal edit commands with `sudo`.

## Quick full checklist

Before edit:

```text
workspace path confirmed
git status checked
validation passes
summary looks plausible
```

During edit:

```text
one semantic change
stable names preserved
references checked
wording reviewed
security reviewed
```

After edit:

```text
validation passes
details or summary inspected
profile behavior checked if relevant
git diff reviewed
backup/temp files checked
tests pass if needed
commit message clear
```

## Related documentation

- [Catalog Files](../Catalog%20Files/en.md)
- [Authoring Error Text](../Authoring%20Error%20Text/en.md)
- [Profiles](../Profiles/en.md)
- [Validation](../Validation/en.md)
- [Workspace Summary](../Workspace%20Summary/en.md)
- [Inspecting Error Details](../Inspecting%20Error%20Details/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Testing and CI](../Testing%20and%20CI/en.md)
- [Troubleshooting](../Troubleshooting/en.md)

## Central principle

> A catalog edit is finished only after it validates, reads correctly, and produces an intentional diff.
