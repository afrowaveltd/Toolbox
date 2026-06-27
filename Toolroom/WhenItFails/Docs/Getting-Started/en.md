# Getting started

This guide walks through the first practical use of WhenItFails Setter.

The recommended first workflow is:

```text
show help
→ initialize workspace
→ validate workspace
→ inspect summary
→ list errors
→ inspect one error
→ make one safe change
→ validate again
```

## Prerequisites

Setter is part of the Afrowave.Toolbox solution.

You need:

* .NET 10 SDK,
* the Toolbox repository,
* a terminal opened at the repository root.

Check the SDK:

```bash
dotnet --version
```

Build the Setter project:

```bash
dotnet build Toolroom/WhenItFails/Setter
```

Run its tests:

```bash
dotnet test Toolroom/WhenItFails/Setter.Tests
```

## Show help

Run:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- help
```

You may also use:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- --help
```

or:

```bash
dotnet run --project Toolroom/WhenItFails/Setter
```

The help screen lists all currently supported commands.

## Command structure

The general form is:

```text
dotnet run --project Toolroom/WhenItFails/Setter -- <command> <arguments>
```

The double dash separates `dotnet run` arguments from Setter arguments.

Example:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Here:

```text
validate
```

is the Setter command, and:

```text
.
```

is the workspace path.

## Choose a workspace root

Setter expects a project or workspace root.

Inside that root, the default WhenItFails workspace is:

```text
Jsons/
└── WhenItFails/
    ├── errors.en.json
    ├── categories.en.json
    ├── code-groups.en.json
    ├── owners.en.json
    └── profiles.json
```

For the current directory, use:

```text
.
```

For another project:

```text
./MyProject
```

or an absolute path:

```text
/home/user/projects/MyProject
```

## Initialize the workspace

Run:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- init .
```

The `init` command:

* creates the required directories when missing,
* creates missing catalog files,
* uses bundled templates,
* preserves existing files,
* never overwrites project-owned catalogs.

Typical first-run result:

```text
Jsons/WhenItFails created
errors.en.json created
categories.en.json created
code-groups.en.json created
owners.en.json created
profiles.json created
```

Later runs typically report that existing files were skipped.

## Initialization safety

Setter initialization follows one central rule:

> Missing files may be created, but existing files must not be replaced.

This means `init` is safe to run repeatedly.

It is not a reset command.

It does not restore bundled defaults over modified project files.

## Validate the workspace

After initialization, run:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Validation checks:

* JSON loading,
* required catalog headers,
* required error fields,
* duplicate IDs,
* duplicate names,
* duplicate numeric codes,
* owner references,
* code-group references,
* code prefixes,
* numeric ranges,
* categories,
* profiles,
* cross-catalog consistency.

A valid workspace should finish without validation errors.

Warnings may still indicate data worth reviewing.

## Validation before editing

Validation should be run before the first edit.

This establishes whether the workspace was already healthy.

Recommended sequence:

```text
validate before change
→ make one change
→ validate after change
```

Without the first validation, it may be unclear whether a later error was introduced by the new edit or already existed.

## Show a workspace summary

Run:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- summary .
```

The alias is:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- inspect .
```

The summary provides a quick overview of the workspace, such as:

```text
Errors
Categories
Code groups
Owners
Profiles
```

Use it to confirm that the expected catalogs were loaded and that their approximate sizes make sense.

## List all errors

Run:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- errors .
```

Setter displays the error catalog as a rich terminal table.

Typical columns include error identity, numeric code, ownership, classification, and severity.

## Filter the error list

### By owner

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  errors . --owner AFW
```

### By code group

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  errors . --group NETWORK
```

### By category

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  errors . --category NETWORK
```

### By severity

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  errors . --severity Warning
```

### By profile

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  errors . --profile WEB
```

### By search text

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  errors . --search network
```

Filters may be combined.

Example:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  errors . --owner AFW --category NETWORK --severity Warning
```

## Plain output

For scripts or redirected output, add:

```text
--plain
```

Example:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  errors . --plain
```

Redirect to a file:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  errors . --plain > errors.tsv
```

Plain output avoids rich terminal formatting.

## Inspect one error

Use:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  details . AFW_NET_0001
```

The target may be:

* stable ID,
* numeric code,
* symbolic name.

Examples:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  details . AFW_NET_0001
```

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  details . 600001
```

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  details . NETWORKUNAVAILABLE
```

The alias is:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  detail . AFW_NET_0001
```

## Inspect before editing

Before changing an error, inspect it first.

Recommended sequence:

```text
details
→ verify ID, code, name, owner and category
→ apply change
→ details again
→ validate
```

This prevents editing the wrong definition when names or codes are similar.

## First safe edit

A good first edit is changing a title.

Example:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  set-title . AFW_NET_0001 "Network is not available"
```

Setter safely updates the selected definition.

The command:

* locates the error,
* creates an updated in-memory document,
* validates the change,
* creates a timestamped backup,
* writes the updated catalog.

## Verify the change

Inspect the same error again:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  details . AFW_NET_0001
```

Then validate the complete workspace:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

A successful field edit must not break cross-catalog consistency.

## Change a message

Run:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  set-message . AFW_NET_0001 \
  "The network is currently unavailable."
```

The message is intended for human-facing output.

Keep it:

* understandable,
* concise,
* free of internal implementation details,
* suitable for the expected audience.

## Change a developer hint

Run:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  set-developer-hint . AFW_NET_0001 \
  "Check connectivity, DNS, proxy, VPN, and endpoint availability."
```

A developer hint may contain technical diagnostic guidance.

It should not be presented to ordinary end users unless the consuming application explicitly chooses to expose it.

## Change severity

Run:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  set-severity . AFW_NET_0001 Warning
```

Supported severities are:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Severity changes can affect logging, monitoring, alerting, and presentation.

Treat them as behavioral changes, not merely wording changes.

## Change documentation key

Run:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  set-documentation-key . AFW_NET_0001 \
  "when-it-fails/errors/network/network-unavailable"
```

A documentation key should be stable and machine-friendly.

It may later be used to connect an error definition with:

* online documentation,
* local help,
* support articles,
* administration UI,
* troubleshooting pages.

## Backups

Before a successful write, Setter creates a timestamped backup of the modified catalog.

The backup protects against accidental changes and makes manual recovery possible.

Do not treat backups as a replacement for version control.

Recommended practice:

```text
version control
+
Setter backup
+
validation
```

Each protects against a different class of mistake.

## Review changes with Git

After editing, inspect the change:

```bash
git status
```

Then:

```bash
git diff
```

Review:

* the intended field changed,
* no unrelated definitions changed,
* formatting remains acceptable,
* backup files are handled according to project policy.

## Commit the catalog change

After review and validation:

```bash
git add Jsons/WhenItFails
git commit -m "Update network error text"
```

Use a commit message that describes the catalog change, not merely the command used.

## Recommended daily workflow

A safe authoring session looks like this:

```text
git pull
→ validate
→ summary
→ locate error
→ details
→ apply one focused change
→ details
→ validate
→ git diff
→ test application
→ commit
```

## Multiple changes

When changing several fields, prefer small logical groups.

Example:

```text
change title
change message
change developer hint
validate
review diff
commit
```

Avoid mixing unrelated catalog changes into one large unreviewable edit.

## Exit codes

Setter uses process exit codes so shell scripts and CI can detect failure.

The exact command-specific behavior may vary, but the general expectation is:

```text
0
→ command succeeded

non-zero
→ command failed, input was invalid, or an unexpected error occurred
```

Do not rely only on visible terminal text in automation.

Check the exit code.

Example:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- validate .

echo $?
```

## Using Setter in CI

A basic validation step may run:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  validate .
```

A non-zero result should fail the build.

This prevents invalid catalogs from reaching deployment.

## Common first-run problems

### Wrong workspace path

Symptom:

```text
required catalog files not found
```

Check whether the supplied path is the project root rather than the `Jsons/WhenItFails` directory itself.

Usually correct:

```bash
validate .
```

Usually not intended:

```bash
validate ./Jsons/WhenItFails
```

### Missing write permission

`init` and write commands need permission to create directories, files, or backups.

Read-only commands such as `summary`, `errors`, and `details` still require read access.

### Existing invalid catalog

`init` preserves existing files.

It does not replace an invalid file with a template.

Run:

```bash
validate .
```

and repair the reported issue explicitly.

### Error not found

Confirm the target using:

```bash
errors . --search network
```

Then inspect the exact ID, code, or symbolic name.

### Invalid severity

Use one of:

```text
Trace
Debug
Information
Warning
Error
Critical
```

## What Setter does not do automatically

Setter does not silently:

* renumber errors,
* change stable IDs,
* repair cross-catalog references,
* replace project files with defaults,
* merge package template changes,
* rewrite the whole workspace,
* activate the runtime context,
* deploy catalogs.

These operations must remain explicit.

## Suggested next reading

After completing this guide, continue with:

* [Commands](../Commands/en.md)
* [Safe Writes](../Safe%20Writes/en.md)
* [Plain Output](../Plain%20Output/en.md)
* [Setting Title](../Setting%20Title/en.md)

## Central principle

> Inspect first, change one thing, validate again, and review the resulting diff.
