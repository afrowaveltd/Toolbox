# FAQ

This page answers common questions about WhenItFails Setter.

It is intended for users, catalog authors, contributors, and maintainers who need quick guidance without reading every detailed guide first.

## What is Setter?

Setter is a command-line companion for WhenItFails JSON catalogs.

It helps with:

- initializing catalog workspaces,
- validating catalog files,
- summarizing catalog contents,
- browsing errors,
- inspecting one error,
- editing selected error text fields,
- saving changes with backups.

It is intentionally focused.

## What is a WhenItFails catalog?

A WhenItFails catalog is a set of JSON files that describe structured errors and related metadata.

A typical package lives here:
Typical files include:
The catalog gives applications a stable vocabulary for failures.

## Is Setter a runtime library?

No.

Setter is a command-line tool for catalog authors and maintainers.

Runtime applications may consume WhenItFails catalogs, but Setter itself is not the runtime failure-handling engine.

## Is Setter a full editor?

No.

Setter currently provides focused edit commands for selected fields in `errors.en.json`.

For broader catalog changes, edit JSON manually and then validate.

## How do I initialize a workspace?

Run:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- init .
This expects a project root and creates or ensures:
Do not pass `Jsons/WhenItFails` itself to `init` unless you intentionally want a nested package.

## How do I validate the catalog?

Run:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
Expected successful exit code:
Validation failure returns:
Missing command input usually returns:
## Can I pass the package directory instead of project root?

For most commands, yes.

These are usually both accepted:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate Jsons/WhenItFails
The main exception is:
which expects the project root.

## How do I see a summary?

Run:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .
Alias:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- inspect .
The summary is read-only.

## How do I list errors?

Run:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors .
You can filter:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --owner AFW
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --group NETWORK
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --category NETWORK
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --severity Error
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --search network
## How do I inspect one error?

Run:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001
Alias:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- detail . AFW_NET_0001
Lookup can use:
Numeric lookup searches numeric code.

Text lookup searches stable ID and symbolic name.

## Can I inspect by title?

No.

Titles are human-facing text and may change.

Use stable identifiers instead:
## How do I change an error title?

Run:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-title . AFW_NET_0001 "Network unavailable"
This updates:
and creates a backup when the write succeeds.

## How do I change an error message?

Run:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-message . AFW_NET_0001 "The network is unavailable."
Keep messages safe, neutral, and reusable.

## How do I change a developer hint?

Run:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-developer-hint . AFW_NET_0001 "Check connectivity, DNS, firewall, proxy, VPN, and host availability."
Developer hints should be actionable and safe.

Do not include secrets.

## How do I change severity?

Run:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-severity . AFW_NET_0001 Warning
Supported values:
Severity input is case-insensitive and is stored using canonical casing.

## How do I change a documentation key?

Run:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-documentation-key . AFW_NET_0001 when-it-fails/errors/network/network-unavailable
A documentation key is a stable reference to extended guidance.

It is not necessarily a URL.

## Does Setter create backups?

Yes, successful focused edits create timestamped backups next to the target file.

Example:
Backups are local recovery files.

They are not a replacement for Git.

## How do I restore a backup?

There is no `restore-backup` command yet.

Restore manually:
cp Jsons/WhenItFails/errors.en.20260627-095820-480.bak.json Jsons/WhenItFails/errors.en.json
Then validate:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
Use the actual backup file name.

## Does Setter delete old backups?

No.

Backup cleanup is manual.

Before deleting backups, make sure:

- active catalogs validate,
- changes are committed,
- no recovery is needed,
- no backup files are accidentally staged.

## Can I run multiple Setter write commands at once?

Avoid it.

Recommended rule:
Safe writes protect individual file replacement, but they are not a multi-process transaction system.

## Does validation prove everything is good?

No.

Validation checks catalog structure and known relationships.

It does not prove every product-level policy, documentation target, production safety decision, or wording quality.

Validation is necessary, not sufficient.

## Does Setter verify documentation links?

Not fully.

Setter stores and displays `documentationKey`, but current behavior does not fully prove that every key resolves to a real page.

Review documentation separately.

## Does Setter support JSON output?

Not currently.

There is no stable `--json` output mode yet.

Use exit codes for automation decisions.

Use `--plain` where available for simpler human-readable output.

## Is plain output stable for scripts?

Not as a formal machine-readable API.

Plain output is simpler than rich output, but it is not currently a versioned JSON/TSV/CSV contract.

For robust automation, use exit codes.

## What are the exit codes?

General model:
See the exit-code guide for command-specific details.

## What is an owner?

An owner describes responsibility.

Examples:
Owner answers:
It does not answer what kind of problem occurred.

## What is a code group?

A code group defines a numeric range and symbolic prefix for a family of related errors.

Example:
It helps keep numeric codes organized.

## What is a category?

A category describes the problem domain.

Examples:
A category answers:
## What is a profile?

A profile describes a usage context.

Examples:
A profile answers:
## What is the difference between category and code group?

A code group controls numbering.

A category describes meaning.

They may have the same name, but they are different concepts.

Example:
## What is the difference between owner and category?

Owner:
Category:
Example:
## What is the difference between profile and category?

Category:
Profile:
Example:
## Can I add a new error through Setter?

Not yet.

Add it manually to:
Then run:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
and inspect it:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . NEW_ERROR_ID
## Can I add a new profile through Setter?

Not yet.

Add it manually to:
Then validate and inspect summary.

## Can I add a new category, code group, or owner through Setter?

Not yet.

Edit the relevant JSON manually:
Then validate.

## Should I create a new category or tag?

Create a category when the concept is central, stable, and useful for classification.

Use a tag when the concept is flexible, secondary, or used as a profile/runtime hint.

Example:
## Should I create a new code group or category?

Create a code group when you need a stable numeric range and prefix.

Create a category when you need a problem-domain classification.

Do not create a code group just because one category exists.

## Should I create a new owner?

Create an owner when there is a real responsibility boundary.

Do not create an owner for every feature, category, or temporary idea.

## Should I create a new profile?

Create a profile when a usage context is stable and reusable.

Do not create a profile just because one temporary filter was convenient.

## What is a good error message?

A good message is:

- clear,
- neutral,
- safe,
- reusable,
- a complete sentence,
- not blaming the user,
- not claiming an unproven cause.

Good:
Weak:
## What is a good developer hint?

A good developer hint is actionable and safe.

Good:
Weak:
Never put secrets in hints.

## Can user-facing messages expose technical details?

Be careful.

User-facing messages should not expose:

- tokens,
- credentials,
- stack traces,
- private paths,
- internal hostnames,
- customer identifiers,
- raw SQL.

Use developer hints and documentation for deeper troubleshooting.

## What should I do before renaming something?

Search references first.

Bash:
grep -R "OLD_VALUE" .
Then consider compatibility.

Stable names should not be renamed casually.

Prefer aliases, migration notes, or additive replacements where possible.

## Can I reuse an old numeric code?

Avoid it.

Old numeric codes may exist in logs, support tickets, dashboards, or external systems.

Reusing a code for a different meaning can corrupt history.

## Should validation automatically fix catalogs?

No.

Validation should report problems.

It should not silently rewrite catalogs.

Any migration or formatting command should be explicit and documented.

## What should I commit?

A good commit should be focused.

Good:
Weak:
## What should I run before commit?

Run:
dotnet build

dotnet test

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

git diff --check

git status --short
Also review the actual diff:
git diff
## Why does the documentation repeat some ideas?

Because catalog work has expensive mistakes.

Stable IDs, numeric codes, profile safety, and migration rules appear in multiple guides because they matter in multiple workflows.

A little repetition is cheaper than one broken public error code.

## Where should I start reading?

For a new user:

1. Getting started
2. Overview
3. Documentation Map
4. Command Quick Reference
5. Workspace Paths and Initialization
6. Validation
7. Browsing and Filtering Errors
8. Inspecting Error Details

For a catalog author:

1. Catalog Files
2. Naming and Numbering Conventions
3. Catalog Author Checklist
4. Adding a New Error Definition
5. Authoring Error Text
6. Profiles

For a maintainer:

1. Architecture Overview
2. Adding a New Command
3. Testing and CI
4. Maintainer Notes
5. Schema Evolution
6. Deprecation and Migration
7. Known Limitations
8. Roadmap and Future Work

## Related documentation

- [Documentation Map](../Documentation%20Map/en.md)
- [Command Quick Reference](../Command%20Quick%20Reference/en.md)
- [Workspace Paths and Initialization](../Workspace%20Paths%20and%20Initialization/en.md)
- [Validation](../Validation/en.md)
- [Catalog Files](../Catalog%20Files/en.md)
- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Known Limitations](../Known%20Limitations/en.md)
- [Roadmap and Future Work](../Roadmap%20and%20Future%20Work/en.md)

## Central principle

> If a catalog value may appear in code, logs, docs, tests, or support, treat it as a contract before changing it.