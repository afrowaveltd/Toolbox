# Maintainer notes

This page collects practical notes for maintainers of WhenItFails Setter.

It is not a user tutorial.

It is a maintenance guide for people who need to understand how the tool should be kept reliable over time.

## Maintainer principle

Setter is small on purpose.

Its job is to make catalog work safer, not to become a large hidden framework.

Prefer changes that keep the tool:

```text
predictable
testable
boring
clear
safe
```

A boring tool that protects important catalogs is better than a clever tool that surprises users.

## Main responsibilities

Setter currently helps with:

- initializing a WhenItFails workspace,
- validating catalog files,
- summarizing workspace contents,
- listing and filtering errors,
- inspecting one error definition,
- editing selected error fields,
- creating backups during safe writes,
- supporting command-line automation.

Everything else should be added deliberately.

## Current command set

Current commands include:

```text
help
demo
init
validate
summary
inspect
errors
details
detail
set-title
set-message
set-developer-hint
set-severity
set-documentation-key
```

Aliases:

```text
inspect
→ summary
```

```text
detail
→ details
```

When adding a command, update:

- help output,
- Commands documentation,
- Command Quick Reference,
- Documentation Map if needed,
- tests,
- troubleshooting notes if relevant.

## Command design rules

A Setter command should have:

- a clear verb,
- predictable arguments,
- useful issue codes,
- stable exit behavior,
- good documentation,
- tests for success and failure paths.

Avoid commands that do too many things.

Prefer:

```text
set-title
set-message
set-severity
```

over:

```text
edit
```

unless a future interactive editor is explicitly designed.

## Exit-code rules

Keep the current broad model:

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
→ workspace validation, lookup, editing, saving, or operation failure
```

```text
3
→ unexpected top-level application failure
```

Exit codes are automation contracts.

Changing them is a breaking behavior change.

## Issue-code rules

Issue codes should be:

- stable,
- specific,
- searchable,
- PascalCase or otherwise consistent with current code,
- more precise than the exit code,
- useful in tests.

Good:

```text
MissingValidatePath
ErrorDefinitionNotFound
UnsupportedSeverity
TitleIsEmpty
```

Weak:

```text
Bad
Failed
Oops
Invalid
```

An issue code should help a user or test understand what went wrong.

## Error message rules

Command and validation messages should be:

- clear,
- neutral,
- actionable,
- safe to display,
- free of secrets,
- specific enough to fix the problem.

Good:

```text
The validate command requires a project root or Jsons/WhenItFails directory path.
```

Weak:

```text
Bad path.
```

## Workspace safety

Setter writes to project-local JSON files.

That makes safety more important than convenience.

When maintaining write behavior, preserve these expectations:

- do not write when validation failed before the edit,
- do not silently overwrite without backup,
- do not create backup files for failed argument validation,
- do not hide I/O failures,
- do not assume one platform’s filesystem behavior applies everywhere,
- do not make concurrent writes look supported unless they really are.

## Safe-write expectations

A normal successful write should:

```text
write temporary file
→ create backup if target exists
→ replace target
→ return success
```

The backup should be in the same directory as the target.

Current backup shape:

```text
<file-without-extension>.<UTC timestamp>.bak<extension>
```

Example:

```text
errors.en.20260627-095820-480.bak.json
```

If this shape changes, update:

- Safe Writes,
- Backups and Recovery,
- Release Checklist,
- tests,
- troubleshooting documentation.

## Temporary files

Temporary files should be treated as implementation details.

A stale temporary file after an interrupted write should not be committed.

The recovery documentation explains how to inspect one safely.

Do not add cleanup behavior that deletes potentially useful recovery artifacts before the user can inspect them unless the behavior is deliberate and documented.

## Concurrency

Current safe-write behavior is not a multi-process transaction system.

Maintainer assumption:

```text
one active writer per workspace
```

Do not imply stronger guarantees in documentation or command output.

If true concurrent writer support is added later, it needs tests and documentation.

## Catalog package assumptions

The default workspace is:

```text
Jsons/WhenItFails
```

Current files:

```text
errors.en.json
categories.en.json
code-groups.en.json
owners.en.json
profiles.json
```

Many commands accept either:

```text
project root
```

or:

```text
direct Jsons/WhenItFails package path
```

The `init` command expects a project root.

Be careful when changing path resolution.

## Path resolution maintenance

Path resolution must behave predictably on:

- Linux,
- Windows,
- CI,
- paths with spaces,
- direct package paths,
- project-root paths.

When changing path logic, test both:

```text
.
```

and:

```text
./Jsons/WhenItFails
```

On Windows, also test:

```text
.\Jsons\WhenItFails
```

Do not rely on case-insensitive path behavior.

Canonical documentation casing is:

```text
Jsons/WhenItFails
```

## Validation before loading derived views

Commands that display derived catalog data should validate first.

Examples:

```text
summary
errors
details
```

Recommended flow:

```text
resolve workspace
→ validate workspace
→ stop on validation failure
→ load normalized data
→ render output
```

This prevents presenting a misleading summary from an invalid workspace.

## Editing flow

Editing commands should follow this shape:

```text
parse arguments
→ resolve workspace
→ load current document
→ locate target
→ apply focused change
→ validate resulting workspace or document
→ save safely
→ report result
```

Do not save partial changes after detecting an invalid edit.

## Supported edit fields

Current focused edit commands modify fields in:

```text
errors.en.json
```

Supported fields:

```text
title
message
developerHint
defaultSeverity
documentationKey
```

Future edit commands should follow the same focused-command style unless there is a strong reason not to.

## Severity handling

Supported severity values:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Input may be case-insensitive, but stored values should be canonical.

Unsupported values should produce a useful issue code.

Example:

```text
UnsupportedSeverity
```

Do not silently accept new severity values without updating validation, docs, and tests.

## Profile behavior caution

Current Setter profile filtering is intentionally simpler than possible runtime profile behavior.

Current `errors --profile` filtering uses:

```text
includeOwners
includeCodeGroups
includeCategories
```

It does not currently apply:

```text
includeSubcategories
includeTags
excludeTags
defaultMappings
```

Do not accidentally document current Setter browsing as complete runtime profile resolution.

If Setter later delegates to a runtime-equivalent resolver, update:

- Profiles,
- Browsing and Filtering Errors,
- Command Quick Reference,
- tests,
- summary expectations if affected.

## Plain output maintenance

Plain output is easier to read and process than rich output, but it is still presentation-oriented.

If changing plain output, consider:

- existing scripts,
- documentation examples,
- tests,
- field labels,
- line order,
- delimiter behavior.

A future JSON output mode would be better for durable automation.

Do not pretend plain output is JSON.

## Rich output maintenance

Rich output is for humans.

It should prioritize:

- readability,
- useful tables,
- clear headings,
- stable terminology,
- good terminal behavior.

Avoid writing tests that break because a table width changed unless table layout is the exact behavior under test.

## Documentation maintenance

Whenever behavior changes, update focused documentation.

Useful rule:

```text
If a user can observe it, document it.
```

Update docs when changing:

- commands,
- arguments,
- options,
- exit codes,
- issue codes,
- output fields,
- validation rules,
- catalog file shapes,
- safe-write behavior,
- backup naming,
- platform-specific behavior.

## README index maintenance

When adding a doc page, update both relevant README files.

Use URL-encoded spaces:

```markdown
- [Maintainer Notes](Docs/Maintainer%20Notes/en.md)
```

Actual file path:

```text
Docs/Maintainer Notes/en.md
```

Keep README ordering stable and useful.

## Documentation tone

Documentation should be:

- practical,
- direct,
- honest,
- implementation-grounded,
- careful about current versus future behavior.

Do not oversell.

Do not hide limitations.

A clear limitation today is better than a false promise tomorrow.

## Future features

Future feature lists are useful, but must be marked clearly.

Good:

```text
Possible future improvements include...
```

Bad:

```text
Setter supports restore commands.
```

when no restore command exists.

Future ideas should not create false user expectations.

## Test maintenance

Tests should protect behavior, not incidental formatting.

Prefer tests for:

- command success paths,
- missing arguments,
- unknown lookup values,
- validation failures,
- issue codes,
- safe-write backup creation,
- rejected edits,
- severity normalization,
- workspace initialization,
- path resolution.

Be cautious with tests that assert large rich-output snapshots.

## Test workspace rule

Tests should use disposable workspaces.

They should not mutate the repository’s real catalog files.

A good test:

```text
creates temporary project root
initializes workspace
performs operation
asserts result
cleans up
```

Cleanup should be best-effort and should not hide the real test failure.

## Negative tests

Negative tests should verify:

- the intended mutation or bad input exists,
- the command fails,
- the exit code is expected,
- the issue code is expected,
- no unintended backup or write occurred when relevant.

Do not only assert that something failed.

Assert why it failed.

## CI expectations

CI should normally run:

```text
restore
build
test
validate
diff check
```

For shell scripts, use strict mode where appropriate.

Bash:

```bash
set -euo pipefail
```

PowerShell must check native exit codes with:

```powershell
$LASTEXITCODE
```

## Platform maintenance

Setter should remain practical on both:

```text
Windows
Linux
```

Be careful with:

- path separators,
- case sensitivity,
- line endings,
- file locks,
- permissions,
- temporary directory conventions,
- command examples,
- shell quoting,
- process exit-code handling.

Do not test only the shell you personally use if the change is platform-sensitive.

## Dependency caution

Avoid adding dependencies unless they clearly pay for themselves.

Setter should remain easy to build, test, and reason about.

Before adding a dependency, ask:

- Is it needed?
- Is it stable?
- Is it maintained?
- Does it affect startup time?
- Does it complicate packaging?
- Does it complicate security review?
- Can simple code solve the problem safely?

## Public behavior

Public behavior includes:

- command names,
- arguments,
- options,
- exit codes,
- issue codes,
- file layout,
- catalog field names,
- safe-write behavior,
- backup naming,
- plain output labels.

Treat public behavior as a contract.

Even if the project is young, users and scripts can quickly depend on these details.

## Compatibility levels

Think of changes in three levels:

```text
safe additive
→ new doc, new test, new optional command
```

```text
behavioral
→ changed validation, changed output, changed defaults
```

```text
breaking
→ renamed commands, changed exit codes, changed schema, changed stable IDs
```

Breaking changes require special care.

## Catalog compatibility

Stable catalog values should not be changed casually.

Be careful with:

- error IDs,
- numeric codes,
- error names,
- owner names,
- code-group names,
- code prefixes,
- category names,
- profile names.

If a stable value must change, document why and consider migration.

## Security maintenance

Never add examples containing real:

- credentials,
- tokens,
- connection strings,
- private hostnames,
- customer data,
- personal data,
- production stack traces,
- raw incident logs.

Use placeholders.

Documentation examples should be safe to publish.

## Production safety

Production-oriented defaults should avoid exposing:

- exception details,
- stack traces,
- sensitive metadata,
- internal paths,
- credentials,
- raw SQL,
- debug-only diagnostics.

Development profiles can be more verbose.

Production profiles should be conservative.

## Review checklist for maintainers

Before merging or releasing a Setter change, check:

- build passes,
- tests pass,
- catalog validates,
- command examples still work,
- docs are updated,
- README index is updated,
- exit codes are stable,
- issue codes are useful,
- Git diff is intentional,
- no backup files are included,
- no temporary files are included,
- no secrets are included.

## Common maintenance traps

Watch for:

- changing behavior without docs,
- adding docs for behavior that does not exist,
- breaking exit codes,
- making profile filtering look stronger than it is,
- committing local backups,
- relying on Windows path casing,
- relying on Bash syntax in PowerShell docs,
- overfitting tests to rich output layout,
- using manual JSON edits without validation,
- renaming stable catalog identifiers casually.

## Useful local commands

Build:

```bash
dotnet build
```

Test:

```bash
dotnet test
```

Validate:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

Summary:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .
```

List errors:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors .
```

Inspect one error:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001
```

Diff check:

```bash
git diff --check
```

## Useful PowerShell commands

Build:

```powershell
dotnet build
```

Test:

```powershell
dotnet test
```

Validate:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .
```

Summary:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- summary .
```

Check exit code:

```powershell
$LASTEXITCODE
```

Diff check:

```powershell
git diff --check
```

## When in doubt

When unsure whether a change is safe:

1. Make the change smaller.
2. Add a focused test.
3. Update the focused doc.
4. Run validation.
5. Review the diff.
6. Ask whether a user or script could depend on the old behavior.

Small reversible steps beat large mysterious rewrites.

## Related documentation

- [Contributing to Setter](../Contributing%20to%20Setter/en.md)
- [Release Checklist](../Release%20Checklist/en.md)
- [Testing and CI](../Testing%20and%20CI/en.md)
- [Command Quick Reference](../Command%20Quick%20Reference/en.md)
- [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
- [Catalog Files](../Catalog%20Files/en.md)
- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Profiles](../Profiles/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Troubleshooting](../Troubleshooting/en.md)

## Central principle

> Maintain Setter like a safety tool: make it clear, predictable, conservative, and easy to verify.
