# Naming and Numbering Conventions

This guide explains the naming and numbering conventions used by WhenItFails catalogs.

It is intended for catalog authors and maintainers who create or review:

- owners,
- code groups,
- categories,
- profiles,
- error definitions,
- numeric error codes,
- documentation keys,
- tags,
- mappings.

## Main principle

Catalog names and numbers are not decoration.

They are long-term references used by:

- source code,
- tests,
- documentation,
- support,
- logs,
- scripts,
- users,
- future tools.

A good convention is:

```text
stable
readable
searchable
predictable
hard to misuse
```

## Stability matters

Avoid changing stable identifiers after release.

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

Changing stable identifiers may break scripts, tests, logs, support references, or external consumers.

Prefer adding aliases, documentation, or migration support over casual renames.

## Display names are different

Display names are for humans.

Examples:

```json
"name": "FILE_SYSTEM",
"displayName": "File system"
```

The stable name is:

```text
FILE_SYSTEM
```

The display name is:

```text
File system
```

Use stable names in references.

Use display names in human-facing output.

## General casing conventions

Current catalog style commonly uses:

```text
UPPERCASE_WITH_UNDERSCORES
```

for stable catalog names such as:

```text
NETWORK
FILE_SYSTEM
EXTERNAL_SERVICE
USER_VISIBLE
```

Current documentation keys commonly use:

```text
lowercase-slug-style
```

inside path-like keys:

```text
when-it-fails/errors/network/network-unavailable
```

Display names use normal readable casing:

```text
Network
File system
External service
```

## Error IDs

An error ID is the stable human-readable identifier of an error definition.

Example:

```text
AFW_NET_0001
```

Recommended structure:

```text
<owner>_<code-prefix>_<sequence>
```

Example:

```text
AFW_NET_0001
```

This reads as:

```text
AFW
→ owner
```

```text
NET
→ code prefix
```

```text
0001
→ sequence within that owner/prefix family
```

Use the active catalog convention.

Do not mix formats without a deliberate migration plan.

## Error ID checklist

A good error ID is:

- unique,
- stable,
- readable,
- consistent with active catalog style,
- connected to owner and code group,
- not tied to wording that may change,
- not reused after deletion unless policy allows it.

Good:

```text
AFW_NET_0001
APP_VAL_0001
PLUGIN_EXT_0001
```

Weak:

```text
ERROR1
TEMP
NETWORK_ERROR_FINAL
NEW_ERROR
FIXME
```

## Error ID sequence numbers

Use fixed-width sequence numbers when that is the active convention.

Example:

```text
0001
0002
0003
```

Fixed width keeps sorting readable.

Do not renumber existing IDs to fill gaps.

Gaps are harmless.

Broken references are not.

## Numeric error codes

Numeric error codes are stable machine-readable values.

Example:

```json
"code": 600001
```

A numeric code should be:

- unique,
- stable,
- inside the intended code-group range,
- inside the intended owner range where applicable,
- easy to reference in logs and support.

Do not change a numeric code just because the message wording changed.

## Numeric code ranges

Code groups define numeric ranges.

Example:

```text
NETWORK
→ 600000 to 699999
```

A network error should use a code inside that range.

Example:

```text
600001
```

Before choosing a code, inspect the existing range and used codes.

Bash:

```bash
grep -n '"code"' Jsons/WhenItFails/errors.en.json
```

PowerShell:

```powershell
Select-String `
  -Path Jsons/WhenItFails/errors.en.json `
  -Pattern '"code"'
```

## Code group names

Code group names are stable identifiers.

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

A good code group name is:

- stable,
- broad enough for multiple errors,
- specific enough to guide numbering,
- uppercase if following current convention,
- not a temporary feature name.

## Code prefixes

Code prefixes are short symbolic prefixes used by code groups.

Examples:

```text
GEN
CFG
VAL
IO
SEC
NET
DB
EXT
FMT
```

A good prefix is:

- unique,
- short,
- readable,
- stable,
- obviously related to the code group.

Avoid:

```text
X
AAA
TMP
NEW
```

unless there is a very deliberate reason.

## Owner names

Owner names describe responsibility.

Examples:

```text
AFW
APP
PLUGIN
USER
```

A good owner name is:

- stable,
- responsibility-based,
- short,
- uppercase if following current convention,
- not a problem domain,
- not a profile name.

Good:

```text
APP
PLUGIN
USER
```

Weak:

```text
NETWORK
DATABASE
TEMP
TODAY
```

Network and database are usually categories or code groups, not owners.

## Category names

Category names describe problem domains.

Examples:

```text
NETWORK
VALIDATION
SECURITY
FILE_SYSTEM
```

A good category name is:

- stable,
- reusable,
- understandable,
- not too broad,
- not too narrow,
- not a team or owner name.

Avoid junk-drawer names:

```text
MISC
OTHER
STUFF
SPECIAL
```

If a category feels like “miscellaneous,” the catalog probably needs better modeling.

## Profile names

Profile names describe usage contexts.

Examples:

```text
WEB
API
CLI
DESKTOP
SERVICE
DEVELOPMENT
PRODUCTION
```

A good profile name is:

- stable,
- context-based,
- easy to type,
- easy to understand,
- not a category,
- not an owner,
- not a one-off filter.

Good:

```text
PUBLIC_API
ADMIN_PORTAL
DISK_TOOL
SUPPORT
WORKER
```

Weak:

```text
MY_FILTER
TEST_PROFILE
TEMP
FINAL2
```

## Error names

The `name` field is the machine-friendly symbolic name of an error.

Example:

```text
NETWORKUNAVAILABLE
```

A good error name is:

- unique,
- stable,
- developer-readable,
- not a sentence,
- not tied to a runtime exception class unless that is the actual stable concept.

Good:

```text
NETWORKUNAVAILABLE
CONFIGURATIONVALUEISMISSING
INVALIDCATALOGREFERENCE
```

Weak:

```text
The network was not available when HttpClient failed
```

## Documentation keys

Documentation keys point to extended documentation.

Example:

```text
when-it-fails/errors/network/network-unavailable
```

A good documentation key is:

- stable,
- lowercase where convention expects it,
- path-like,
- predictable,
- not a local filesystem path,
- not a temporary URL,
- not secret.

Avoid:

```text
C:\Users\Me\Desktop\help.md
```

```text
http://localhost:5000/test
```

```text
final-page-v3
```

## Documentation key pattern

Recommended pattern:

```text
when-it-fails/errors/<domain>/<specific-error>
```

Example:

```text
when-it-fails/errors/network/network-timeout
```

For broader topics, use predictable paths:

```text
when-it-fails/categories/storage
```

```text
when-it-fails/profiles/production
```

Only use paths that make sense for actual documentation organization.

## Tags

Tags are flexible labels.

Examples:

```text
USER_VISIBLE
INTERNAL_ONLY
DEBUG_ONLY
NETWORK
SECURITY
```

A good tag is:

- stable,
- meaningful,
- searchable,
- useful for filtering or profiles,
- uppercase if following current convention.

Weak tags:

```text
TEMP
THING
MISC
TODO
```

Do not use tags as comments.

Use documentation or developer hints for explanations.

## Mapping keys

Mapping keys are string keys inside `defaultMappings`.

Examples:

```text
web.problemDetails
web.includeTraceId
cli.includeHints
production.includeStackTrace
catalogRole
editable
```

A good mapping key is:

- predictable,
- namespaced where helpful,
- readable,
- stable,
- connected to a real consumer or policy.

Recommended pattern:

```text
context.settingName
```

Examples:

```text
web.includeTraceId
service.includeRetryInformation
disk.includeDestructiveWarning
```

Avoid:

```text
flag1
x
thing
show
mode
```

## Mapping values

Current mapping values are strings.

Example:

```json
"web.includeTraceId": "true"
```

This is a string, not a JSON boolean.

Stay consistent with existing catalog style unless the model deliberately changes.

## Aliases

Aliases should represent real alternate names or terminology.

Examples:

```json
"aliases": [ "HTTP", "CONNECTIVITY" ]
```

Good aliases are:

- useful,
- recognizable,
- not misleading,
- not broader than the actual concept,
- not duplicates of unrelated categories.

Do not stuff every related keyword into aliases.

## Sequence strategy

When assigning new sequence values, prefer the next available number in the family.

Example:

```text
AFW_NET_0001
AFW_NET_0002
AFW_NET_0003
```

If a number is skipped, do not renumber old entries just to close the gap.

History and stable references matter more than visual neatness.

## Sorting strategy

Keep catalogs easy to review.

Common practical sorting approaches:

```text
by numeric code
```

or:

```text
by stable name
```

or:

```text
by logical group
```

Whatever approach is used, keep it consistent inside the file.

Avoid moving unrelated entries when adding one item.

Unrelated sorting churn makes review harder.

## Avoid synonyms as separate stable names

Do not create separate stable names for the same concept.

Example problem:

```text
NETWORK_TIMEOUT
TIMEOUT_NETWORK
REMOTE_TIMEOUT
```

If they mean the same thing, choose one stable concept.

Use aliases or tags for related terminology if needed.

## Avoid implementation leakage

Stable catalog names should represent durable concepts, not accidental implementation details.

Avoid names based on:

- private class names,
- temporary method names,
- one library’s exception type,
- one provider’s internal wording,
- one short-lived project codename.

Use implementation details in developer hints or documentation when helpful.

## Avoid emotional severity naming

Do not encode emotional judgment into names.

Weak:

```text
TERRIBLE_NETWORK_FAILURE
BAD_USER_INPUT
HORRIBLE_CONFIG
```

Good:

```text
NETWORKTIMEOUT
INVALIDINPUTVALUE
CONFIGURATIONVALUEISMISSING
```

The catalog should remain neutral and professional.

## Do not rename casually

Before renaming a stable name, ask:

- Is it referenced in code?
- Is it referenced in tests?
- Is it referenced in docs?
- Is it referenced in support material?
- Is it logged externally?
- Is it part of a released package?
- Can an alias solve the problem?
- Is migration needed?

Renames are compatibility events.

## When a rename is justified

A rename may be justified when:

- the old name is actively misleading,
- the old name contains a typo that creates real confusion,
- the concept was modeled incorrectly,
- a migration plan exists,
- tests and docs are updated,
- affected consumers are considered.

A rename should usually be its own focused commit.

## Deprecation naming

If a concept is obsolete, consider deprecation before deletion.

A deprecated entry should still have a stable name.

Do not replace it with unclear names such as:

```text
OLD
OLD2
DO_NOT_USE
```

Use documentation or metadata to explain deprecation when a formal process exists.

## Examples of good naming families

Network family:

```text
codeGroup: NETWORK
codePrefix: NET
category: NETWORK
error id: AFW_NET_0001
error name: NETWORKUNAVAILABLE
documentation key: when-it-fails/errors/network/network-unavailable
tags: NETWORK, USER_VISIBLE
```

Validation family:

```text
codeGroup: VALIDATION
codePrefix: VAL
category: VALIDATION
error id: AFW_VAL_0001
error name: INVALIDINPUTVALUE
documentation key: when-it-fails/errors/validation/invalid-input-value
tags: VALIDATION, USER_VISIBLE
```

## Review checklist

Before committing new names or numbers, confirm:

```text
stable names are unique
numeric codes are unique
numeric codes fit ranges
code prefixes match code groups
references use stable names, not display names
documentation keys are predictable
tags are meaningful
mapping keys are predictable
display names are readable
no unrelated renames occurred
validation passes
Git diff is intentional
```

## Useful checks

List numeric codes:

```bash
grep -n '"code"' Jsons/WhenItFails/errors.en.json
```

List code group ranges:

```bash
grep -n '"name"\|"codePrefix"\|"codeFrom"\|"codeTo"' Jsons/WhenItFails/code-groups.en.json
```

List owners:

```bash
grep -n '"name"\|"codeFrom"\|"codeTo"' Jsons/WhenItFails/owners.en.json
```

Validate:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

Review diff:

```bash
git diff -- Jsons/WhenItFails
git diff --check
```

## PowerShell checks

List numeric codes:

```powershell
Select-String `
  -Path Jsons/WhenItFails/errors.en.json `
  -Pattern '"code"'
```

List code group ranges:

```powershell
Select-String `
  -Path Jsons/WhenItFails/code-groups.en.json `
  -Pattern '"name"|"codePrefix"|"codeFrom"|"codeTo"'
```

List owners:

```powershell
Select-String `
  -Path Jsons/WhenItFails/owners.en.json `
  -Pattern '"name"|"codeFrom"|"codeTo"'
```

Validate:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .
```

Review diff:

```powershell
git diff -- Jsons/WhenItFails
git diff --check
```

## Related documentation

- [Catalog Files](../Catalog%20Files/en.md)
- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Adding a New Error Definition](../Adding%20a%20New%20Error%20Definition/en.md)
- [Adding a New Category](../Adding%20a%20New%20Category/en.md)
- [Adding a New Code Group](../Adding%20a%20New%20Code%20Group/en.md)
- [Adding a New Owner](../Adding%20a%20New%20Owner/en.md)
- [Adding a New Profile](../Adding%20a%20New%20Profile/en.md)
- [Glossary](../Glossary/en.md)

## Central principle

> Names and numbers are the catalog’s memory. Make them stable enough that future code, logs, docs, and people can trust them.
