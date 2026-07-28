# Adding a New Command

This guide describes the current workflow for adding a command to WhenItFails Setter.

> Treat every command as a public contract.

A command is complete only when dispatch, argument handling, service behavior, output surfaces, exit and issue codes, tests, documentation, and continuation status agree.

## Start with the contract

Before writing code, define:

- the command name and any aliases;
- whether it is read-only or writes a catalog;
- required positional arguments;
- supported options and switches;
- accepted workspace path forms;
- success behavior;
- expected domain and validation failures;
- process exit codes;
- stable issue codes;
- rich, `--plain`, and `--json` output requirements;
- persistence and backup behavior for writes;
- documentation and tests that must change.

Do not begin implementation while these decisions are still ambiguous.

## Keep the command focused

A Setter command should do one clear thing.

Prefer:

```text
show one definition
add one error
set one field
explain one profile
restore one selected backup
```

Avoid commands that silently combine unrelated editing, cleanup, migration, validation, and rendering work.

One logical behavior is easier to test, review, automate, document, and preserve for backward compatibility.

## Command dispatch

The command must be reachable through the application command dispatch.

Add the canonical lowercase name and only intentional aliases. Command and alias matching should follow existing normalization rules rather than introducing a private parsing convention.

Verify:

- canonical command dispatch;
- alias dispatch where applicable;
- unknown-command behavior remains unchanged;
- help output lists the command;
- existing commands still resolve correctly.

Aliases are public behavior. Document and test every alias that is added.

## Command class responsibility

The command class owns the CLI workflow:

1. parse and validate command-line arguments;
2. create a specific input failure when syntax is incomplete;
3. invoke the appropriate service layer operation;
4. select rich, plain, or JSON rendering;
5. map the result to the documented exit code.

The command class should not become the implementation home for catalog loading, cross-catalog validation, persistence, backup discovery, or profile evaluation.

Use `CommandInputError` or the established equivalent for missing or malformed command input. Do not use an unhandled exception for an ordinary missing argument.

## Service layer

Reusable domain behavior belongs in the service layer.

A service should:

- accept explicit inputs;
- return structured results or issues;
- avoid terminal rendering;
- remain testable without launching the CLI;
- preserve cancellation behavior;
- keep expected failures separate from unexpected exceptions.

The same service result should be usable by rich, plain, and JSON output paths.

Do not duplicate the same lookup, validation, or write algorithm inside several command classes.

## Workspace and validation behavior

Decide whether the command requires a workspace and whether validation must happen before the operation.

Read-only commands that derive trusted catalog information should normally validate the workspace before presenting that information.

Write commands should reject the operation before persistence when:

- required input is missing;
- the target does not exist;
- the requested value is invalid;
- the resulting catalog or workspace would be invalid;
- the selected backup or catalog target is ambiguous.

Do not save first and validate afterward as the primary safety mechanism.

## Read-only command flow

A typical read-only command follows:

```text
parse arguments
→ resolve workspace
→ load and validate
→ call query service
→ render selected output
→ return exit code
```

Tests should cover:

- valid success;
- missing arguments;
- invalid workspace;
- lookup not found where relevant;
- filters or options;
- rich, plain, and JSON contracts where supported;
- canonical name and aliases.

A valid query that returns zero matches may still be a successful command. Define that behavior explicitly.

## Write command flow

A typical write command follows:

```text
parse arguments
→ resolve workspace
→ load current data
→ validate requested operation
→ modify in memory
→ validate resulting state
→ safe-write one target file
→ render result
→ return exit code
```

For successful writes, tests should verify:

1. the structured response;
2. the persisted value after reloading;
3. unrelated values remain unchanged;
4. the expected timestamped backup exists;
5. the workspace validates after the write.

For rejected writes, verify:

1. the intended issue code;
2. the target file remains unchanged;
3. no backup is created;
4. no partial success is reported.

Setter persistence is a single-file operation. A new command must not imply a multi-file transaction unless such a transaction is explicitly implemented and tested.

## Argument and option design

Use positional arguments only when their meaning is obvious from the syntax.

Use explicit option names for optional behavior:

```text
--plain
--json
--profile
--owner
```

Document whether each option is:

- optional or required;
- a switch or value-bearing option;
- repeatable;
- case-sensitive;
- an exact or partial match.

Follow existing option conventions. Do not add a second private mini-parser without a strong reason and tests.

## Exit codes and issue codes

Exit codes classify the process result broadly:

```text
0  success
1  invalid command usage or arguments
2  expected workspace, validation, lookup, authoring, persistence, or recovery failure
3  unexpected top-level failure
```

An issue code explains the specific reason.

For example, several failures may return exit code `2`, while their issue codes distinguish invalid workspace data, an unknown target, a rejected value, or a save failure.

Tests should assert both the exit code and useful issue code when both are part of the contract.

Do not change an existing exit code casually. Scripts may depend on it.

## Rich output

Rich output is the interactive human-facing surface.

Use it for readable tables, panels, summaries, and diagnostics. Keep domain logic out of Spectre.Console views.

A rich renderer should receive a result or view model and render it. It should not load catalogs, decide persistence, or invent different command semantics.

## Plain output

`--plain` is simplified human-readable output.

It should avoid terminal borders, color control sequences, and layout-dependent decoration. It remains presentation-oriented and is not a replacement for a stable machine schema.

Test meaningful text rather than incidental spacing unless formatting itself is the contract.

## JSON output

`--json` is the machine-readable surface.

When a command supports JSON, define and test:

- success schema;
- failure schema;
- issue representation;
- field names and casing;
- null and empty collection behavior;
- exit-code relationship;
- schema or contract version where applicable.

Parse JSON in tests and assert its structure. Do not compare it only as a whitespace-sensitive string.

Machine consumers should use `--json` and process exit codes. They must not parse rich terminal rendering as a stable API.

## Help and discoverability

Update the command help with:

- canonical name;
- short purpose;
- syntax;
- important options;
- aliases where applicable.

A command hidden from help and documentation is not finished.

## Required documentation updates

At minimum, review and update:

- `README.md` when the high-level command surface changes;
- `Docs/Commands/en.md`;
- `Docs/Command Quick Reference/en.md`;
- the focused topic page for substantial behavior;
- `Docs/Exit Codes and Automation/en.md` when automation behavior changes;
- `Docs/Safe Writes/en.md` or `Docs/Backups and Recovery/en.md` for persistence changes;
- `Docs/Architecture Overview/en.md` when layer boundaries change;
- `IMPLEMENTATION_STATUS.md`.

Important documentation behavior should receive a documentation-contract test under:

```text
Toolroom/WhenItFails/Setter.Tests/Docs
```

## Test structure

Add tests immediately with the command change.

Use the narrowest useful layers:

- service tests for domain behavior;
- command tests for argument, output, and exit-code behavior;
- persistence tests for actual reloaded values and backups;
- renderer tests for rich, plain, and JSON surfaces;
- documentation tests for public usage claims.

Use isolated temporary workspaces for all mutating tests. Never modify the repository's real catalog workspace in a test.

## Cross-platform checks

Commands that use paths, processes, files, or shell examples must consider Windows and Linux.

Check:

- spaces in paths;
- path separators and casing assumptions;
- permissions and file locking;
- temporary-directory behavior;
- Bash and PowerShell quoting;
- `$?` and `$LASTEXITCODE` examples;
- line-ending assumptions.

Do not weaken a correct contract merely to hide a platform-specific defect.

## Security review

A command that renders or writes user-provided data must not expose or persist unintended:

- credentials or tokens;
- connection strings;
- private paths;
- stack traces in normal user-facing output;
- sensitive metadata;
- raw internal exceptions as structured success data.

Use safe placeholder values in tests and documentation.

## Verification gate

Run the focused Setter suite:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Run broader repository tests when the command changes shared libraries, public runtime contracts, package wiring, or cross-project integrations:

```powershell
dotnet test
```

For catalog or documentation behavior, also run the relevant checks:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
git diff --check
```

## Final command checklist

Before the command is complete, confirm:

- [ ] the command has one clear purpose;
- [ ] canonical dispatch and aliases are tested;
- [ ] missing and invalid arguments are handled;
- [ ] service behavior is independent of rendering;
- [ ] success and expected failures return documented exit codes;
- [ ] useful issue codes identify specific failures;
- [ ] rich output is readable;
- [ ] `--plain` is tested where supported;
- [ ] `--json` has a parsed structural test where supported;
- [ ] successful writes reload correctly and create expected backups;
- [ ] rejected writes preserve the target and create no backup;
- [ ] help and command references are updated;
- [ ] `README.md` and focused documentation remain aligned;
- [ ] `IMPLEMENTATION_STATUS.md` records the completed step;
- [ ] the complete Git diff was reviewed;
- [ ] the focused Setter suite is green.

## Stop rule

> Do not start the next command while the focused Setter suite is red.

Complete one command-sized green step, document it, commit it, and only then continue.

## Related documentation

- [Architecture Overview](../Architecture%20Overview/en.md)
- [Contributing to Setter](../Contributing%20to%20Setter/en.md)
- [Testing and CI](../Testing%20and%20CI/en.md)
- [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
- [Commands](../Commands/en.md)
- [Command Quick Reference](../Command%20Quick%20Reference/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)

## Central principle

> A new command is complete only when users, services, automation, tests, documentation, and the continuation status all describe the same behavior.
