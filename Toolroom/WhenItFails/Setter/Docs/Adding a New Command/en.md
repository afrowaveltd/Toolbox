# Adding a New Command

This guide explains how to add a new command to WhenItFails Setter.

It is intended for maintainers and contributors who are extending the command-line tool.

A new command is public behavior. Treat it as a small API.

## Command principle

A Setter command should do one clear thing.

Good command design is:
Avoid commands that become hidden workflows with many unrelated side effects.

## Before adding a command

Before writing code, answer these questions:

- What problem does the command solve?
- Is it read-only or does it write files?
- Which workspace path forms does it accept?
- Does it require validation before running?
- What arguments are required?
- What options are supported?
- What is the success exit code?
- What are the failure exit codes?
- What issue codes can it produce?
- Does it need rich output?
- Does it need plain output?
- Which documentation pages must change?
- Which tests prove the behavior?

If these answers are unclear, the command design is not ready.

## Prefer focused commands

Prefer focused commands such as:
over broad commands such as:
Focused commands are easier to document, test, automate, review, and keep backward-compatible.

A broad command can be useful later, but it should be designed deliberately.

## Read-only versus write command

The first architectural decision is whether the command is read-only.

Read-only examples:
Write examples:
Write commands need extra care because they can change user files.

## Recommended read-only flow

A read-only workspace command should usually follow this flow:
Missing required command input should normally return:
Workspace validation failure should normally return:
## Recommended write flow

A write command should usually follow this flow:
A write command should not save if:

- required arguments are missing,
- the target cannot be found,
- the new value is invalid,
- validation fails,
- safe write fails.

## Command name checklist

A command name should be:

- lowercase,
- stable,
- easy to type,
- easy to search,
- specific enough to understand,
- consistent with existing command names.

Good:
Weak:
## Alias checklist

Only add an alias when it clearly helps.

Existing aliases:
Aliases are public behavior.

Every alias should be documented, tested, and kept compatible.

Do not add aliases for every personal preference.

## Argument design

Command arguments should be positional only when their meaning is obvious.

Example:
Example:
Avoid ambiguous positional shapes such as:
without clear documentation.

## Option design

Options should start with:
Examples:
Options should be named for the user concept, not the implementation detail.

Good:
Weak:
unless that is truly what users need.

## Switches and values

A switch has no value:
An option with value needs the next argument:
Document whether an option is:

- required,
- optional,
- repeatable,
- case-sensitive,
- exact match,
- substring match.

Current Setter options are intentionally simple.

Keep that spirit unless a stronger parser is introduced.

## Missing arguments

A command should produce a specific issue when required arguments are missing.

Good issue code examples:
A missing-argument issue should include:

- clear message,
- expected syntax,
- useful path or context when applicable.

Expected exit code:
## Invalid values

Invalid command values should produce specific issue codes.

Examples:
For existing edit commands, invalid edit values generally return:
Follow existing behavior unless changing it deliberately.

## Lookup failures

When a command looks up an error definition and cannot find it, use a specific issue.

Current issue:
Do not invent inconsistent lookup behavior without a reason.

## Exit code model

Use the general Setter exit-code model:
Exit codes are automation contracts.

Do not change them casually.

## Validation first

Commands that display derived workspace information should validate before rendering.

Recommended:
Not recommended:
A summary of invalid data can mislead users.

## Write commands and validation

Write commands should validate enough to avoid corrupting the catalog.

A typical pattern is:
Do not save first and ask validation questions later.

## Safe writes

If a command writes JSON, it should use the safe-write workflow.

Expected behavior:
Do not use direct unsafe file replacement for catalog files.

## Backups

A successful edit to an existing catalog should create a backup.

Backup shape:
Example:
If a new command writes a different catalog file, document its backup behavior.

## Output design

Decide whether the command needs:
Most user-facing commands should report what happened.

Automation-friendly commands should also provide stable exit codes.

## Rich output

Rich output is for humans.

Use it for:

- tables,
- headings,
- summaries,
- readable validation results.

Do not make scripts depend on rich output.

## Plain output

Plain output is simpler and line-oriented.

It is useful for:

- copying,
- redirecting,
- simple scripts,
- logs.

Plain output is not currently JSON.

If a command supports `--plain`, document exactly what it prints.

## JSON output

If adding a future JSON output mode, treat it as a stronger contract.

Define:

- supported commands,
- exact schema,
- success output,
- failure output,
- versioning,
- tests,
- documentation.

Do not improvise JSON output casually.

## Help output

When adding a command, update help output.

Help should include:

- command name,
- short description,
- syntax,
- important aliases if any.

Help is often the first documentation a user sees.

## Documentation updates

A new command usually needs updates to:

- Commands,
- Command Quick Reference,
- Documentation Map,
- Architecture Overview if architectural behavior changes,
- Maintainer Notes if it changes maintenance rules,
- Testing and CI if it affects checks,
- Troubleshooting if it introduces common failure modes.

A command is not finished until users can discover how to use it.

## README updates

If the command requires a new documentation page, update both relevant README files.

Example:
- [Adding a New Command](Docs/Adding%20a%20New%20Command/en.md)
Actual path:
Use URL-encoded spaces in Markdown links.

## Test plan for a read-only command

A read-only command should have tests for:

- success with valid workspace,
- missing required path,
- validation failure,
- expected rendering model or output,
- alias if one exists,
- important options,
- unknown or invalid option behavior if supported.

If the command searches or filters, test:

- match found,
- no match found,
- case behavior,
- combined filters where applicable.

## Test plan for a write command

A write command should have tests for:

- successful write,
- expected field changed,
- unrelated fields preserved,
- backup created,
- missing arguments,
- target not found,
- invalid value rejected,
- unsupported value rejected,
- no backup for rejected input when applicable,
- validation still passes after successful write.

Use disposable workspaces.

Do not mutate the real repository catalogs.

## Smoke test

After adding a command, run a manual smoke test.

Example for read-only command:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- <command> .
Example for write command in a disposable workspace:
test_root="$(mktemp -d /tmp/when-it-fails-command-XXXXXXXXXX)"

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- init "$test_root"

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- <new-command> "$test_root" ...

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate "$test_root"

rm -rf "$test_root"
PowerShell equivalent should be added to documentation when relevant.

## Command implementation checklist

Before implementation is considered done:

- command is reachable from dispatch,
- help mentions it,
- missing arguments are handled,
- invalid values are handled,
- validation failure is handled,
- success path returns `0`,
- failure paths return expected codes,
- issue codes are specific,
- rich output is readable,
- plain output exists if needed,
- tests cover important paths,
- docs are updated.

## Example: adding a read-only command

Imagine a future command:
Purpose:
Possible syntax:
Expected flow:
Expected failures:
Documentation updates:

- Commands,
- Command Quick Reference,
- Profiles,
- Documentation Map.

Tests:

- lists profiles,
- missing path,
- invalid workspace,
- plain output if supported.

## Example: adding a write command

Imagine a future command:
Possible syntax:
Expected flow:
Expected issue codes could include:
Documentation updates:

- Commands,
- Command Quick Reference,
- Profiles,
- Safe Writes,
- Backups and Recovery if backup behavior differs.

Tests:

- successful description update,
- backup created for profiles.json,
- unknown profile rejected,
- empty description rejected,
- validation still passes.

## Naming issue codes

Issue code names should explain the problem.

Good pattern:
Examples:
Avoid generic issue codes.

## Adding command docs

A command documentation page should usually include:

- purpose,
- syntax,
- accepted paths,
- examples,
- options,
- exit codes,
- validation behavior,
- output behavior,
- automation notes,
- troubleshooting notes,
- related documentation.

Keep examples copy-paste friendly.

Include both Bash and PowerShell if the command is important for both platforms.

## Updating quick reference

The quick reference should include:

- syntax,
- one or two common examples,
- expected exit codes,
- important issue codes.

Do not turn the quick reference into a duplicate full manual.

It should remain compact.

## Updating troubleshooting

Add troubleshooting notes when the command can fail in ways users may not understand.

Examples:

- path confusion,
- profile not found,
- unsupported severity,
- backup not created,
- permission denied,
- invalid workspace.

Troubleshooting should explain what to check next.

## Backward compatibility

When adding a new command, compatibility risk is usually low.

However, check for:

- command name conflicts,
- alias conflicts,
- option name conflicts,
- changes to existing command parsing,
- changed help behavior,
- changed exit codes,
- changed validation timing.

A new command should not break existing commands.

## Do not add hidden behavior

Avoid commands that silently:

- rewrite unrelated files,
- normalize whole catalogs unexpectedly,
- delete backups,
- delete temporary files,
- rename stable IDs,
- change profiles,
- change severity defaults,
- hide validation failures.

If a command does one of these, it must be explicit, documented, and tested.

## Platform checklist

If the command handles paths or files, test on:
Check:

- spaces in paths,
- path separators,
- case sensitivity,
- permissions,
- file locks,
- temporary directories,
- line endings where relevant.

## Security checklist

If the command prints or writes user-provided values, ensure it does not accidentally expose:

- secrets,
- tokens,
- connection strings,
- stack traces,
- private paths,
- sensitive metadata.

Documentation examples should use safe placeholder values.

## Review checklist

A reviewer should check:

- command purpose is clear,
- command name is appropriate,
- argument order is intuitive,
- exit codes match conventions,
- issue codes are useful,
- validation behavior is safe,
- write behavior uses safe writes,
- tests cover success and failure,
- docs are updated,
- existing commands are not broken.

## Final check

Before committing a new command:
dotnet build

dotnet test

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

git diff --check

git status --short
PowerShell:
dotnet build

dotnet test

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

git diff --check

git status --short
## Commit message

Good:
Weak:
## Related documentation

- [Architecture Overview](../Architecture%20Overview/en.md)
- [Maintainer Notes](../Maintainer%20Notes/en.md)
- [Contributing to Setter](../Contributing%20to%20Setter/en.md)
- [Command Quick Reference](../Command%20Quick%20Reference/en.md)
- [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
- [Testing and CI](../Testing%20and%20CI/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Troubleshooting](../Troubleshooting/en.md)

## Central principle

> A new command is done when it is reachable, tested, documented, safe on failure, and boring enough that automation can trust it.