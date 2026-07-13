# Reviewing Catalog Changes

This guide explains how to review changes to WhenItFails catalog files.

It is intended for maintainers, pull-request reviewers, and catalog authors who want to catch mistakes before they become stable public references.

Catalog changes can look small in Git, but they may affect code, logs, documentation, support, automation, and runtime behavior.

## Main principle

Review catalog changes as contracts, not as ordinary text edits.

A good review checks:

```text
meaning
stability
safety
references
validation
documentation
diff cleanliness
```

## Files commonly reviewed

Catalog changes usually affect files under:

```text
Jsons/WhenItFails
```

Common files:

```text
errors.en.json
categories.en.json
code-groups.en.json
owners.en.json
profiles.json
```

Documentation changes usually affect:

```text
Toolroom/WhenItFails/Setter/Docs
```

Some changes may also affect:

- source code,
- tests,
- README files,
- release notes,
- examples,
- scripts.

## Start with validation

Before reviewing details, run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

Expected successful exit code:

```text
0
```

If validation fails, review should usually stop until the catalog is fixed.

## Run tests

Run:

```bash
dotnet build

dotnet test
```

Catalog changes can affect tests, examples, and command behavior.

Do not assume JSON-only changes are isolated.

## Review whitespace and formatting

Run:

```bash
git diff --check
```

This catches common whitespace problems such as trailing spaces.

Then review the changed files:

```bash
git diff -- Jsons/WhenItFails
```

If docs changed:

```bash
git diff -- Toolroom/WhenItFails/Setter/Docs
```

## Review scope

Ask:

- Is this change focused?
- Does the commit do one logical thing?
- Are unrelated formatting changes mixed in?
- Are generated backup files accidentally included?
- Are temporary files accidentally included?
- Are docs and catalogs changed together for a reason?

A focused diff is easier to trust.

## Watch for accidental backup files

Setter may create backup files such as:

```text
errors.en.20260627-095820-480.bak.json
```

These should usually not be committed.

Check:

```bash
git status --short
```

If backup files appear, review carefully before staging.

Usually they should remain local recovery files.

## Reviewing a new error definition

When a new error is added, check:

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

Then inspect it:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . NEW_ERROR_ID
```

Use the actual new ID.

## New error checklist

Confirm:

```text
ID is unique
numeric code is unique
numeric code fits the code-group range
owner exists
code group exists
code prefix matches the code group
primary category exists
categories exist
severity is canonical
title is short and clear
message is safe and neutral
developer hint is useful and safe
documentation key is stable
tags are intentional
metadata is necessary
```

## Reviewing a changed error title

A title should be:

- short,
- specific,
- neutral,
- readable,
- not a full diagnostic paragraph,
- usually without a final period.

Good:

```text
Network unavailable
```

Weak:

```text
The network request failed because DNS, firewall, proxy, or VPN settings may be wrong.
```

That belongs in message, hint, or documentation.

## Reviewing a changed error message

A message should be:

- a complete sentence,
- safe to display,
- reusable,
- neutral,
- not blaming the user,
- not claiming an unproven cause.

Good:

```text
The network is unavailable.
```

Weak:

```text
Your internet is broken.
```

Weak:

```text
The firewall blocked the request.
```

unless the firewall cause is actually known.

## Reviewing a developer hint

A developer hint should be:

- actionable,
- technical,
- safe,
- concise,
- not a secret store,
- not a stack trace.

Good:

```text
Check connectivity, DNS, firewall, proxy, VPN, and host availability.
```

Weak:

```text
Fix it.
```

Dangerous:

```text
Try the production password from the deployment notes.
```

## Reviewing severity changes

Supported values:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Check that severity reflects operational impact, not emotion.

Ask:

- Did the operation fail?
- Can the system continue?
- Is data safety affected?
- Is security affected?
- Is availability affected?
- Is this user-facing or diagnostic only?

Severity changes may affect logging, alerting, dashboards, and support triage.

## Reviewing documentation keys

A documentation key should be:

- stable,
- predictable,
- lowercase where convention expects it,
- not a local filesystem path,
- not a temporary URL,
- not secret.

Good:

```text
when-it-fails/errors/network/network-unavailable
```

Weak:

```text
C:\Users\Me\Desktop\notes.md
```

Weak:

```text
http://localhost:5000/test
```

## Reviewing user-visible errors

If an error is tagged or intended as user-visible, check that it does not expose:

- secrets,
- tokens,
- credentials,
- stack traces,
- private paths,
- internal hostnames,
- raw SQL,
- customer identifiers,
- sensitive metadata.

User-visible should be clear, not reckless.

## Reviewing production-facing profiles

Profiles such as:

```text
PRODUCTION
WEB
API
```

may affect what users or external systems see.

Check:

- included owners,
- included code groups,
- included categories,
- include tags,
- exclude tags,
- default mappings,
- exception-detail policy,
- stack-trace policy,
- sensitive metadata policy.

Remember that current Setter profile browsing uses simplified filtering.

Runtime code must still enforce production safety.

## Reviewing a new category

Check:

```text
name
displayName
description
aliases
parentCategories
defaultTags
defaultMappings
```

Ask:

- Is the category stable?
- Is it broad enough to reuse?
- Is it specific enough to mean something?
- Does an existing category already fit?
- Is this really a tag instead?
- Is this really a code group instead?

Avoid junk-drawer categories:

```text
MISC
OTHER
STUFF
SPECIAL
```

## Reviewing a new code group

Check:

```text
name
displayName
codePrefix
codeFrom
codeTo
description
defaultCategories
defaultTags
defaultMappings
```

Ask:

- Is the prefix unique?
- Does the numeric range overlap unintentionally?
- Is the range large enough?
- Is this really a code group, not just a category?
- Are related errors using matching `codePrefix`?
- Are related errors inside the range?

## Reviewing a new owner

Check:

```text
name
displayName
description
codeFrom
codeTo
isBuiltIn
aliases
defaultMappings
```

Ask:

- Is this a real responsibility boundary?
- Is the range intentional?
- Does it overlap unintentionally?
- Is `isBuiltIn` correct?
- Is this really an owner, not a category or profile?
- Are affected profiles updated intentionally?

## Reviewing a new profile

Check:

```text
name
displayName
description
includeOwners
includeCodeGroups
includeCategories
includeSubcategories
includeTags
excludeTags
defaultMappings
```

Ask:

- Is this a stable reusable context?
- Does an existing profile already fit?
- Are included values known?
- Are excluded tags safe?
- Are production mappings conservative?
- Does the profile accidentally expose internal details?

Test:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile PROFILE_NAME
```

## Reviewing renames

Stable values should not be renamed casually.

Before approving a rename, check references.

Bash:

```bash
grep -R "OLD_VALUE" .
```

PowerShell:

```powershell
Get-ChildItem `
  -Path . `
  -Recurse `
  -File |
Select-String `
  -Pattern "OLD_VALUE"
```

Ask:

- Is the rename necessary?
- Is there a migration note?
- Can an alias solve it?
- Are tests updated?
- Are docs updated?
- Are old logs still understandable?

## Reviewing deletions

Before approving deletion, ask:

- Is the value released?
- Is it referenced in code?
- Is it referenced in tests?
- Is it referenced in docs?
- Is it referenced by profiles?
- Is it used in logs or support?
- Is deprecation safer?
- Is there a migration path?

Deletion is often more serious than addition.

## Reviewing numeric code changes

Changing a numeric code is high risk.

Ask:

- Has the code appeared in logs?
- Is it referenced by support?
- Is it used in tests?
- Is it documented?
- Is the old code being reused?
- Would adding a new error be safer?

Usually, do not change released numeric codes.

## Reviewing profile filter expectations

If a change depends on profile behavior, remember:

```text
Setter browsing currently uses simplified profile filtering.
```

It focuses on:

```text
includeOwners
includeCodeGroups
includeCategories
```

It does not fully enforce all runtime-style profile semantics.

Review runtime code separately if production behavior matters.

## Reviewing docs changes

Documentation changes should be checked for:

- correct paths,
- correct README links,
- URL-encoded spaces,
- accurate command examples,
- current command names,
- current exit codes,
- working relative links,
- no stale behavior claims,
- no future features described as current behavior.

Docs should be clear about current behavior versus future ideas.

## README link checklist

Documentation page path:

```text
Toolroom/WhenItFails/Setter/Docs/Page Name/en.md
```

README link:

```markdown
[Page Name](Docs/Page%20Name/en.md)
```

Spaces in links should be URL-encoded.

## Reviewing command examples

Check that examples include:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- <command> .
```

For PowerShell examples, check backticks:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- <command> .
```

Do not mix Bash line continuation with PowerShell line continuation.

## Reviewing exit-code claims

If documentation mentions exit codes, check against current behavior.

General model:

```text
0
→ success
```

```text
1
→ missing or invalid command input
```

```text
2
→ validation, lookup, editing, save, or operation failure
```

```text
3
→ unexpected top-level application failure
```

Command-specific behavior should match actual implementation.

## Reviewing generated or copied content

When adding large docs or copied examples, check:

- no duplicate wrong paths,
- no stale command names,
- no invented options,
- no broken Markdown fences,
- no accidental Czech text inside English docs unless intended,
- no internal notes accidentally included.

Long docs should still be technically precise.

## Manual smoke tests

Useful smoke tests:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- help
```

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .
```

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --search network
```

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001
```

Use catalog-specific IDs when examples differ.

## Review before staging

Check changed files:

```bash
git status --short
```

Review diff:

```bash
git diff
```

Stage only intended files.

Example:

```bash
git add Jsons/WhenItFails/errors.en.json
git add Toolroom/WhenItFails/Setter/Docs/Reviewing Catalog Changes/en.md
```

Avoid:

```bash
git add .
```

when backup or temporary files may exist.

## Review after staging

Check staged diff:

```bash
git diff --cached
```

This is the exact content that will be committed.

Review it before commit.

## Commit message checklist

A good commit message is specific.

Good:

```text
Add storage category
```

```text
Add network timeout error definition
```

```text
Update network unavailable message
```

```text
Add Setter catalog review guide
```

Weak:

```text
changes
```

```text
json fix
```

```text
misc
```

## Pull request checklist

A pull request should explain:

- what changed,
- why it changed,
- whether stable identifiers changed,
- whether migration is needed,
- which validation/tests were run,
- which docs were updated,
- whether production-facing behavior changed.

Short and precise is better than vague and long.

## Red flags

Pause review if you see:

- numeric code reuse,
- renamed stable IDs without migration,
- deleted public errors,
- production profile becoming more permissive,
- user-facing messages exposing internals,
- unrelated formatting churn,
- backup files staged,
- docs claiming future behavior as current,
- validation not run,
- tests not run after command behavior changes.

## Final reviewer checklist

Before approving, confirm:

```text
validation passes
tests pass
diff is focused
stable identifiers are preserved or migration is documented
new references are valid
user-facing text is safe
production profiles remain safe
docs are updated
README links are correct
no backup/temp files are staged
commit message is specific
```

## Related documentation

- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Naming and Numbering Conventions](../Naming%20and%20Numbering%20Conventions/en.md)
- [Deprecation and Migration](../Deprecation%20and%20Migration/en.md)
- [Known Limitations](../Known%20Limitations/en.md)
- [Release Checklist](../Release%20Checklist/en.md)
- [Testing and CI](../Testing%20and%20CI/en.md)
- [Authoring Error Text](../Authoring%20Error%20Text/en.md)
- [Profiles](../Profiles/en.md)

## Central principle

> A catalog review is not just checking JSON syntax; it is checking whether future people can safely trust the names, numbers, text, and meanings being committed.
