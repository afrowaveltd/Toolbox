# Adding a New Error Definition

This guide explains how to add a new error definition to the WhenItFails catalog.

It is intended for catalog authors and maintainers who edit:
A new error definition is a long-term catalog contract.

Treat it carefully.

## Main principle

A good error definition is:
It should help users, developers, operators, tests, and support understand the same failure in the same way.

## Before adding an error

Before editing JSON, decide:

- What failed?
- Who owns the error?
- Which code group does it belong to?
- Which numeric code should it use?
- Which primary category describes it?
- Is it user-visible?
- Is it internal-only?
- What should the title say?
- What should the message say?
- What should the developer hint say?
- What severity is appropriate?
- Is extended documentation needed?
- Which profiles should include it?

If these answers are unclear, the error definition is not ready.

## Recommended workflow

Use this workflow:
Commands:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --group NETWORK

# edit Jsons/WhenItFails/errors.en.json

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0002

git diff -- Jsons/WhenItFails/errors.en.json
PowerShell:
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- errors . --group NETWORK

# edit Jsons/WhenItFails/errors.en.json

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- details . AFW_NET_0002

git diff -- Jsons/WhenItFails/errors.en.json
## Error definition shape

A typical error definition contains:
{
  "id": "AFW_NET_0002",
  "code": 600002,
  "name": "NETWORKTIMEOUT",
  "owner": "AFW",
  "codePrefix": "NET",
  "codeGroup": "NETWORK",
  "primaryCategory": "NETWORK",
  "categories": [ "NETWORK" ],
  "subcategories": [ "TIMEOUT" ],
  "title": "Network timeout",
  "message": "The operation timed out while waiting for the remote endpoint.",
  "defaultSeverity": "Error",
  "developerHint": "Check endpoint availability, DNS, proxy settings, firewall rules, timeout values, and network latency.",
  "documentationKey": "when-it-fails/errors/network/network-timeout",
  "tags": [ "NETWORK", "USER_VISIBLE" ],
  "metadata": {}
}
Use the active catalog conventions.

Do not copy this example blindly.

## Choose the owner

The `owner` field identifies responsibility.

Example:
"owner": "AFW"
Known owners are defined in:
Common default owners include:
Use:
for built-in Afrowave definitions.

Use:
for project-local application definitions.

Use:
for extension or plugin definitions.

Use:
for local user-defined or experimental definitions.

## Owner checklist

Before choosing an owner, ask:

- Who should maintain this definition?
- Does the owner exist in `owners.en.json`?
- Is this built-in or project-local?
- Does the numeric code fit the owner range where applicable?
- Will support know who is responsible?

Do not use owner as a category.

## Choose the code group

The `codeGroup` field identifies the numeric and symbolic error family.

Example:
"codeGroup": "NETWORK"
Known code groups are defined in:
Common default code groups include:
## Code group checklist

Before choosing a code group, ask:

- Which technical family does this error belong to?
- Does the code group exist?
- What prefix does it use?
- What numeric range does it own?
- Is there already a more appropriate group?

Do not create a new code group for one error unless the concept is likely to grow.

## Choose the numeric code

The `code` field is the stable numeric error code.

Example:
"code": 600002
The code should be:

- unique,
- inside the intended code-group range,
- stable after release,
- not reused casually,
- not changed just because wording changes.

Example range:
Therefore:
is a plausible network code.

## Numeric code checklist

Before choosing a code, check existing codes.

Bash:
grep -n '"code"' Jsons/WhenItFails/errors.en.json
PowerShell:
Select-String `
  -Path Jsons/WhenItFails/errors.en.json `
  -Pattern '"code"'
Then confirm the chosen code is not already used.

## Choose the code prefix

The `codePrefix` field should match the selected code group.

Example:
"codePrefix": "NET"
For the `NETWORK` code group, the prefix is typically:
Do not invent a different prefix for an existing group.

## Choose the ID

The `id` field is the stable human-readable identifier.

Example:
"id": "AFW_NET_0002"
Use the active catalog convention.

A good ID is:

- unique,
- stable,
- readable,
- connected to owner and code group,
- not tied to temporary wording.

Avoid:
## ID numbering

If the catalog uses sequential IDs, choose the next available number in that family.

Example:
Do not renumber existing IDs to make a list prettier.

Stable IDs matter more than visual order.

## Choose the name

The `name` field is the machine-friendly symbolic name.

Example:
"name": "NETWORKTIMEOUT"
A good name is:

- unique,
- stable,
- uppercase if that is the catalog convention,
- readable to developers,
- not too long,
- not a full sentence.

Avoid:
Prefer:
or a similarly stable symbolic name.

## Choose the primary category

The `primaryCategory` field describes the main problem domain.

Example:
"primaryCategory": "NETWORK"
The value should exist in:
Use the most specific existing category that fits.

Do not create a new category when an existing one clearly applies.

## Additional categories

The `categories` array can include additional known categories.

Example:
"categories": [ "NETWORK", "EXTERNAL_SERVICE" ]
Use additional categories only when they add meaningful classification.

Do not duplicate random tags in categories.

## Subcategories

The `subcategories` array gives finer classification.

Example:
"subcategories": [ "TIMEOUT" ]
Subcategories should be consistent.

Avoid spelling drift:
Choose one convention and reuse it.

## Title

The `title` field is the short human-readable label.

Example:
"title": "Network timeout"
A good title is:

- short,
- specific,
- neutral,
- readable,
- not a full diagnostic paragraph,
- not a stack trace,
- usually without a final period.

Good:
Weak:
Weak:
## Message

The `message` field explains what happened.

Example:
"message": "The operation timed out while waiting for the remote endpoint."
A good message is:

- a complete sentence,
- safe to display,
- clear without internal context,
- reusable,
- neutral,
- not blaming the user,
- not claiming an unproven cause.

Good:
Weak:
Weak:
unless the firewall cause is actually known.

## Developer hint

The `developerHint` field suggests what to investigate.

Example:
"developerHint": "Check endpoint availability, DNS, proxy settings, firewall rules, timeout values, and network latency."
A good developer hint is:

- actionable,
- technically useful,
- safe,
- not too long,
- not a replacement for documentation,
- not exposing secrets.

Good:
Weak:
Weak:
## Default severity

The `defaultSeverity` field defines the normal severity.

Supported values:
Example:
"defaultSeverity": "Error"
Use canonical casing.

Severity should reflect operational impact.

It should not reflect frustration.

## Severity decision guide

Use:
for very detailed diagnostic signals.

Use:
for developer-only diagnostic information.

Use:
for notable but non-failing events.

Use:
for degraded or risky behavior where the operation may continue.

Use:
when an operation failed.

Use:
when system health, data safety, security, or service availability is seriously threatened.

## Documentation key

The `documentationKey` field points to extended guidance.

Example:
"documentationKey": "when-it-fails/errors/network/network-timeout"
A good documentation key is:

- stable,
- lowercase where convention expects it,
- predictable,
- not a local path,
- not a temporary URL,
- not secret.

It does not have to be a URL.

## Tags

The `tags` array gives flexible labels.

Example:
"tags": [ "NETWORK", "USER_VISIBLE" ]
Good tags are:

- meaningful,
- stable,
- consistent,
- useful for profiles or search,
- not random notes.

Common useful tags may include:
Do not mark an error `USER_VISIBLE` unless the title and message are safe for user-facing presentation.

## Metadata

The `metadata` object is for advanced structured data.

Example:
"metadata": {}
Use metadata only when the information does not fit normal fields.

Do not put ordinary explanations in metadata.

Use:
for normal human-facing information.

## User-visible safety

Before marking an error as user-visible, check:

- title is safe,
- message is safe,
- no internal hostname is exposed,
- no local file path is exposed,
- no stack trace is exposed,
- no secret is exposed,
- no unsupported cause is claimed,
- wording is neutral.

User-visible does not mean vague.

It means safe.

## Production safety

For production-facing errors, avoid exposing:

- exception type names unless intended,
- stack traces,
- internal service names,
- private paths,
- raw SQL,
- credentials,
- tokens,
- customer identifiers,
- sensitive metadata.

Use developer hints and documentation keys for deeper troubleshooting.

## When to add a new error

Add a new error when:

- the failure has a distinct meaning,
- users or developers need a stable reference,
- support needs a stable code,
- the failure needs different severity,
- the failure needs different remediation guidance,
- the failure belongs to a different category or profile.

Do not add a new error only because the message wording is slightly different.

## When not to add a new error

Do not add a new error when:

- an existing error already describes the same failure,
- the difference is only a runtime value,
- the failure is too vague,
- the failure is temporary debug noise,
- the meaning is not understood yet,
- the error would duplicate another code.

Improve the existing definition instead.

## Relationship to runtime placeholders

If runtime code later supports placeholders, keep the catalog text compatible with localization.

Prefer complete sentence templates.

Avoid concatenated fragments.

Good future-friendly style:
Potential future template style:
Avoid:
in catalog-like text.

## Relationship to documentation

Use `documentationKey` when an error needs deeper explanation.

Documentation may cover:

- causes,
- remediation,
- operator checks,
- developer checks,
- examples,
- related errors,
- escalation path.

The catalog should remain concise.

The documentation can be deeper.

## Add one error at a time

When practical, add one error definition per commit.

This makes review easier.

If adding many related errors, keep them in one clearly scoped family.

Good:
Good:
Risky:
## Validate after adding

Run:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
Expected exit code:
If validation fails, fix the catalog before continuing.

## Inspect after adding

Run:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0002
Use the actual new ID.

Review:

- title,
- message,
- severity,
- owner,
- code group,
- category,
- tags,
- documentation key.

## List by group or category

Check whether the new error appears in expected lists.

Examples:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --group NETWORK
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --category NETWORK
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --search timeout
## Check profile visibility

If the error should appear in a profile, test it.

Example:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile WEB --search timeout
Remember that current Setter profile browsing uses a simplified profile filter.

It does not currently apply every runtime-style profile rule such as include/exclude tags.

## Review Git diff

Run:
git diff -- Jsons/WhenItFails/errors.en.json
Check:

- only intended entry was added,
- no unrelated formatting churn occurred,
- no stable IDs were changed,
- no numeric codes were renumbered,
- no accidental backup files are staged.

Then run:
git diff --check
## Common mistakes

Common mistakes when adding an error:

- duplicate numeric code,
- duplicate ID,
- unknown owner,
- unknown category,
- unknown code group,
- code outside code-group range,
- code prefix does not match code group,
- title is too vague,
- message claims an unproven cause,
- developer hint contains secrets,
- production-safe error exposes internals,
- tags are inconsistent,
- documentation key is temporary,
- workspace was not validated.

## Review checklist

Before commit, confirm:
## Commit message

Good:
Weak:
## Related documentation

- [Catalog Files](../Catalog%20Files/en.md)
- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Authoring Error Text](../Authoring%20Error%20Text/en.md)
- [Profiles](../Profiles/en.md)
- [Validation](../Validation/en.md)
- [Inspecting Error Details](../Inspecting%20Error%20Details/en.md)
- [Browsing and Filtering Errors](../Browsing%20and%20Filtering%20Errors/en.md)
- [Glossary](../Glossary/en.md)

## Central principle

> Add a new error only when it gives the catalog a clearer, more stable way to describe a distinct failure.