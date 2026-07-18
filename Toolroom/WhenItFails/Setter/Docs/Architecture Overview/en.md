It should not silently guess, repair, migrate, or rewrite large parts of the workspace unless that behavior is explicitly designed, tested, and documented.

## Main layers

At a high level, Setter can be understood in these layers:

# Architecture overview

This page describes the high-level architecture of WhenItFails Setter.

It is intended for contributors and maintainers who need to understand how the command-line tool is organized and how the main parts fit together.

This is not a full source-code reference.

It is a map.

## Main idea

Setter is a command-line companion for WhenItFails JSON catalogs.

Its job is to make catalog work safer by providing focused operations such as:

- initialize workspace,
- validate catalogs,
- summarize workspace,
- list errors,
- inspect one error,
- edit selected error fields,
- save with backups.

Setter should stay small, explicit, and predictable.

## Architectural principle

Setter should behave like a careful workshop tool:

Not every command uses every layer.

Read-only commands usually stop before safe writing.

Editing commands use the full path.

## Program entry point

The entry point is responsible for:

- accepting raw command-line arguments,
- deciding which command was requested,
- dispatching to the correct command implementation,
- showing help for help/no-argument cases,
- returning process exit codes,
- catching unexpected top-level exceptions.

The command name is normalized before dispatch.

Top-level unexpected failures should return:

## Command dispatch

Command dispatch maps command names such as:

to command implementations.

Aliases are also handled here or through equivalent dispatch logic.

Current aliases:

When adding a command, update both dispatch and documentation.

## Command implementations

A command implementation should own the user-facing workflow for one command.

Examples:

A command should:

- parse its arguments,
- report missing or invalid input clearly,
- call shared workspace services,
- render success or failure output,
- return the intended exit code.

A command should not hide failure details.

## Command argument parsing

Argument parsing is intentionally simple.

Commands currently use direct command-line argument arrays and command-specific parsing.

This keeps behavior easy to inspect.

When adding options, prefer:

- explicit option names,
- predictable values,
- case-insensitive switches where existing behavior does that,
- useful behavior when values are missing.

Do not add complicated parsing rules without tests.

## Workspace resolution

Most commands accept either:

or:

The resolver determines the actual package directory.

The `init` command is different:

because it creates or ensures:

Path behavior must remain predictable across Windows and Linux.

## Workspace package

The logical workspace package is:

Current catalog files:

The package is validated as a whole.

One file may be syntactically valid but still invalid because it references unknown values from another file.

## Loading

Loading reads JSON catalog files from disk.

The loader is responsible for turning JSON into catalog document models.

Loading should preserve the distinction between:

These failures are different and should produce useful diagnostics.

## Normalization

Normalization prepares loaded data for consistent use.

It may ensure predictable collections, defaults, or model shapes.

Normalization is not a substitute for validation.

Do not rely on normalization to silently fix broken catalog meaning.

A normalized invalid workspace is still invalid.

## Validation

Validation checks whether the catalog package is structurally and logically acceptable.

Validation should happen before commands present derived data.

Recommended read-only command flow:

Validation protects users from trusting summaries or details produced from broken data.

## Summary model

The summary workflow produces a workspace overview.

It is useful because it gives users a fast sanity check:

- how many errors exist,
- which owners exist,
- which code groups exist,
- which profiles exist,
- primary-category distribution.

The summary should remain read-only.

It should not mutate catalogs.

## Error listing

The `errors` command lists error definitions and supports filters.

Current filter concepts include:

The command should stay useful for discovery.

It should not become a full query language unless that is deliberately designed.

## Error lookup

The detail workflow locates one error definition by:

Numeric lookup checks numeric error code.

Text lookup checks stable ID and symbolic name.

The detail view should show enough information for a catalog author to safely edit or review the definition.

## Editing operations

Editing operations are focused.

Current edit commands update:

in:

Focused commands are preferred because they are easier to test, document, and reason about.

## Editing workflow shape

A safe edit should generally follow:

Avoid saving if the edit is invalid.

Avoid saving if the target error cannot be found.

## Safe writing

Safe writing should protect the active catalog from careless replacement.

Current safe-write concept:

Backups are local recovery artifacts.

Safe writing is not a complete multi-file transaction system.

## Rendering

Rendering is responsible for user-facing output.

There are two broad styles:

Rich output should be pleasant for humans.

Plain output should be easier to copy, redirect, or inspect.

Neither should be confused with a stable JSON API unless a JSON mode is explicitly added later.

## Views

View classes should focus on presentation.

They should not own core catalog logic.

Good separation:

This makes tests and future output formats easier.

## Exit code boundary

The process exit code is part of the command contract.

The command implementation should return the intended code.

General model:

Do not bury exit-code decisions deep in presentation code.

## Issue codes

Issue codes describe specific problems.

They are more precise than exit codes.

Examples:

Tests should prefer checking issue codes where practical.

Issue codes should stay stable.

## Models

Catalog models represent JSON document data.

They should remain close enough to the JSON shape that catalog authors and maintainers can reason about the file format.

Avoid adding hidden behavior to simple catalog models.

Prefer explicit services for behavior.

## Services

Shared services should handle reusable operations such as:

- workspace resolution,
- initialization,
- validation,
- loading,
- summarization,
- editing,
- writing.

A service should have a clear responsibility.

If a service becomes a â€śgod object,â€ť split it.

## Tests

Tests should cover behavior at useful boundaries:

- command argument handling,
- workspace initialization,
- workspace validation,
- editing success,
- editing rejection,
- backup creation,
- severity normalization,
- lookup behavior,
- profile filtering,
- exit-code behavior.

Tests should use disposable workspaces.

They should not mutate the real repository workspace.

## Documentation as architecture support

For Setter, documentation is part of maintainability.

Whenever architecture changes, update the relevant documentation:

- Commands,
- Command Quick Reference,
- Exit Codes and Automation,
- Catalog Files,
- Profiles,
- Safe Writes,
- Backups and Recovery,
- Testing and CI,
- Maintainer Notes.

A command that exists only in code but not in docs is not really finished.

## Adding a new command

Before adding a command, decide:

- command name,
- purpose,
- arguments,
- options,
- success exit code,
- failure exit codes,
- issue codes,
- whether it reads or writes,
- whether it needs validation first,
- whether it needs plain output,
- what tests prove it works,
- which docs must change.

Recommended implementation path:

## Adding a read-only command

A read-only command should normally:

On invalid workspace:

On missing required command input:

## Adding a write command

A write command should normally:

Rejected command input should usually return:

Rejected operation or invalid resulting data should usually return:

Follow existing command behavior for consistency.

## Adding a validation rule

When adding a validation rule, also add:

- a success test,
- a failure test,
- a useful issue code,
- a useful issue message,
- documentation if users can hit the rule,
- migration notes if existing catalogs may fail.

Validation should be strict enough to protect users and clear enough to help them fix problems.

## Adding a catalog field

Adding a new catalog field may require updates to:

- model classes,
- loaders,
- normalizers,
- validators,
- summary or detail views,
- documentation,
- tests,
- example catalogs,
- profiles or mappings,
- backward compatibility behavior.

Do not add a field only in JSON without considering model and validation behavior.

## Adding a new output mode

If adding a future output mode such as JSON, define:

- which commands support it,
- exact output shape,
- whether it is stable,
- error output behavior,
- relationship to exit codes,
- tests,
- documentation.

JSON output would be a machine-readable contract.

Treat it more strictly than rich output.

## Maintaining documentation paths

Documentation pages are stored under:

README links should usually look like:
[Page Name](Docs/Page%20Name/en.md)
Spaces in links should be URL-encoded.

Actual folder names may contain spaces.

Keep the link and file path aligned.

## Common architecture traps

Avoid:

- mixing rendering logic into validators,
- mixing validation logic into views,
- saving before validation,
- treating plain output as stable JSON,
- adding commands without tests,
- changing exit codes without documentation,
- silently accepting invalid catalog data,
- hiding file I/O failures,
- overloading one command with many unrelated behaviors,
- making profile browsing look runtime-complete when it is not.

## Dependency direction

Recommended dependency direction:

Avoid circular conceptual dependencies.

Views should not call commands.

Models should not render themselves.

Validators should not write files.

Writers should not decide command semantics.

## Error-handling style

Expected failures should be represented as structured operation results or validation issues.

Unexpected failures may reach the top-level catch and return:

Do not use exceptions for ordinary missing command arguments.

Do not swallow exceptions that indicate real I/O or serialization failure.

## Performance expectations

Catalogs are currently small.

Prefer clarity over micro-optimization.

However, avoid obviously wasteful behavior such as:

- repeatedly parsing the same file in one command without need,
- validating many times inside a tight loop,
- using expensive reflection where simple code is clearer.

If catalogs become large later, optimize with tests and measurements.

## Backward compatibility

Backward compatibility matters for:

- command names,
- exit codes,
- issue codes,
- JSON field names,
- stable catalog IDs,
- numeric error codes,
- profile names,
- README links.

When changing one of these, consider whether users or scripts may already depend on it.

## Architecture review checklist

For a non-trivial code change, ask:

- Is the responsibility in the right layer?
- Does validation happen before derived output?
- Does write behavior remain safe?
- Are exit codes preserved?
- Are issue codes useful?
- Are tests focused?
- Are docs updated?
- Does the change work on Windows and Linux?
- Is the diff understandable?

## Related documentation

- [Maintainer Notes](../Maintainer%20Notes/en.md)
- [Contributing to Setter](../Contributing%20to%20Setter/en.md)
- [Testing and CI](../Testing%20and%20CI/en.md)
- [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
- [Command Quick Reference](../Command%20Quick%20Reference/en.md)
- [Catalog Files](../Catalog%20Files/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Profiles](../Profiles/en.md)

## Central principle

> Setter architecture should make the safe path obvious: validate before trust, edit one thing at a time, write carefully, and return a result automation can understand.