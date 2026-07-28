# Architecture Overview

This page is a high-level map of `Toolroom/WhenItFails/Setter` for contributors and maintainers.

Setter is a .NET 10 command-line authoring tool for the WhenItFails JSON catalog workspace. Its architecture favors explicit orchestration, reusable services, structured failures, conservative persistence, and output contracts that remain understandable to both humans and automation.

> Commands orchestrate; services implement reusable behavior; views render.

## Architectural goals

Setter should make the safe path obvious:

1. resolve the requested workspace;
2. load and normalize catalog documents;
3. validate before trusting derived data or persisting changes;
4. perform one focused operation;
5. return structured success or failure information;
6. render the selected rich, plain, or JSON output;
7. preserve the active catalog through the safe-write and backup contract.

The project deliberately prefers small explicit components over hidden repair, broad mutation, or clever command behavior.

## Entry point and dispatch

The application entry point accepts raw command-line arguments and dispatches them to command implementations.

Its responsibilities include:

- normalizing the command name;
- selecting the command or alias;
- showing help for help and no-argument cases;
- returning the command exit code;
- converting an unexpected top-level failure into exit code `3`.

The entry point should not contain catalog business logic. New behavior belongs in a command or reusable service, with the entry point limited to composition and dispatch.

## Command layer

Command classes own one user-facing workflow.

A command normally:

1. validates its command-line arguments;
2. interprets supported switches;
3. invokes workspace, catalog, validation, editing, or recovery services;
4. chooses the relevant view or JSON contract;
5. returns the documented process exit code.

Commands may coordinate several services, but should not duplicate reusable loading, validation, persistence, lookup, or formatting rules.

Read-only commands stop before persistence. Write commands continue through validation and the safe-write boundary.

## Service layer

Services implement reusable behavior independently of terminal decoration.

Current service responsibilities include:

- resolving a project root or direct `Jsons/WhenItFails` workspace;
- initializing a workspace;
- loading and normalizing catalogs;
- validating individual documents and cross-catalog relationships;
- summarizing and inspecting workspace content;
- locating errors and related references;
- creating, editing, and removing catalog entries;
- evaluating profiles and mappings;
- checking documentation keys and local Markdown links;
- writing catalog files safely;
- listing and restoring backups.

A service should have one coherent responsibility. When orchestration, validation, persistence, and rendering begin accumulating in the same class, the boundary should be split rather than expanded into a god object.

## Workspace and catalog models

The logical workspace lives under:

```text
Jsons/WhenItFails
```

Its catalog documents include errors, categories, code groups, owners, and profiles. These files form one logical package even though persistence currently operates on one target file at a time.

Catalog models represent serialized data and normalized in-memory state. They should remain easy to compare with the JSON contract.

Models do not render themselves.

Behavior such as lookup, validation, profile evaluation, editing, and persistence belongs in services rather than hidden model side effects.

## Loading and normalization

Loading converts JSON files into catalog document models while preserving useful distinctions between missing files, malformed JSON, access failures, and unsupported data.

Normalization creates predictable in-memory shapes and canonical values where the current contract allows it. It is not silent schema migration and must not disguise invalid catalog meaning.

A normalized workspace may still be invalid. Validation remains a separate gate.

## Validation and structured failures

Validation protects all derived output and write operations.

Read-only workflows generally follow:

```text
resolve → load → normalize → validate → query → render
```

Write workflows generally follow:

```text
resolve → load → normalize → validate input → modify in memory
→ validate resulting state → persist → reload or inspect → render
```

Expected failures are represented through structured responses, issues, and stable issue codes. Process exit codes classify the broad result:

- `0` — success;
- `1` — command usage or argument failure;
- `2` — expected workspace, validation, lookup, edit, save, or recovery failure;
- `3` — unexpected top-level failure.

Exceptions are not the normal representation for missing command arguments or ordinary catalog rejection.

## Query and authoring operations

Read-only operations include workspace validation, summaries, reference catalogs, error details, filtering, profile explanation, documentation checks, and backup discovery.

Authoring operations include error creation and removal, focused field editing, tag and alias changes, ownership and category changes, profile authoring, mappings, metadata, and explicit restoration.

Operations should stay focused. A narrowly named command is easier to test, document, automate, and review than a broad command that mutates unrelated concepts.

## Persistence and recovery

Setter persistence uses a single-file safe write.

The intended sequence is:

```text
validate new state
→ serialize a complete temporary file
→ create a timestamped backup of the current target
→ replace the target
```

Rejected input or failed pre-write validation must leave the target unchanged and create no backup.

A successful write should expose enough information to inspect the target and generated backup. Tests should reload the persisted document rather than trusting only the in-memory result.

This is not a multi-file transaction and not a multi-process locking system. Commands that conceptually affect several relationships must still respect the current one-target persistence boundary and validate the complete workspace afterward.

Recovery is explicit:

```text
list-backups → select by content → restore-backup
→ validate complete workspace → inspect diff → run tests
```

Backup age alone does not prove that a backup is the correct recovery point.

## Output boundaries

Setter has three distinct output surfaces:

- rich terminal output for interactive use;
- `--plain` human-readable output without rich decoration;
- `--json` machine-readable output.

Views do not decide command semantics.

Views receive already prepared results and render them. They should not load catalogs, validate workspaces, write files, or choose exit codes.

Automation should use `--json` together with process exit codes. Rich and plain output remain presentation surfaces and must not be parsed as stable JSON schemas.

## Dependency direction

The intended conceptual direction is:

```text
entry point
→ commands
→ services
→ catalog models and persistence abstractions
```

Rendering is called by commands after service results exist:

```text
command result
→ rich view | plain renderer | JSON renderer
```

Important boundaries:

- Models do not render themselves.
- Views do not decide command semantics.
- Validators do not write files.
- Writers do not choose user-facing exit codes.
- Commands do not duplicate reusable validation and persistence rules.
- Services do not depend on Spectre.Console layout decisions.

These boundaries keep service tests independent from terminal formatting and prevent presentation changes from altering catalog behavior.

## Testing boundaries

Tests should cover behavior at the smallest useful boundary:

- service tests for loading, validation, lookup, editing, persistence, profiles, mappings, and recovery;
- command tests for arguments, exit codes, aliases, output selection, and failure mapping;
- view tests for intentional rich or plain rendering contracts;
- JSON tests by parsing and asserting the machine-readable structure;
- documentation-contract tests for important published behavior.

Writable tests use isolated temporary workspaces and must not mutate the repository catalog.

The minimum Setter-wide verification gate is:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Run repository-wide tests when a change affects shared libraries, runtime contracts, package wiring, or other projects.

## Adding or changing architecture

For a new command or capability, decide before implementation:

- which layer owns the behavior;
- whether the operation is read-only or writes one catalog target;
- which service responsibilities are reusable;
- what validation occurs before derived output or persistence;
- which issue and exit-code contracts apply;
- which output modes are supported;
- how persistence and backup behavior are verified;
- which tests and documentation must change.

A new output mode, public model, issue code, exit-code behavior, JSON property, stable catalog identifier, or persistence rule is a compatibility-sensitive contract.

## Common architecture traps

Avoid:

- rendering inside validators or catalog models;
- validation rules hidden inside views;
- direct target truncation before serialization completes;
- saving before the resulting state validates;
- parsing rich output in automation;
- treating `--plain` as a JSON schema;
- changing issue or exit codes without tests and documentation;
- broad commands that mix unrelated mutations;
- services that combine orchestration, rendering, validation, and persistence;
- presenting future behavior as already implemented;
- presenting implemented behavior as future work.

## Architecture review checklist

Before completing a non-trivial change, confirm:

- [ ] the responsibility is in the correct layer;
- [ ] command orchestration remains thin and understandable;
- [ ] reusable logic is service-owned;
- [ ] validation happens before trust and persistence;
- [ ] expected failures remain structured;
- [ ] exit codes and output contracts remain intentional;
- [ ] a write preserves safe-write and backup invariants;
- [ ] tests use appropriate service, command, view, or documentation boundaries;
- [ ] Windows and Linux behavior was considered;
- [ ] documentation and `IMPLEMENTATION_STATUS.md` are updated;
- [ ] the focused Setter suite is green.

## Related documentation

- [Adding a New Command](../Adding%20a%20New%20Command/en.md)
- [Contributing to Setter](../Contributing%20to%20Setter/en.md)
- [Testing and CI](../Testing%20and%20CI/en.md)
- [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Known Limitations](../Known%20Limitations/en.md)

## Central principle

> Setter architecture is healthy when command orchestration, reusable behavior, catalog state, persistence, and rendering remain separately testable and agree on one explicit contract.
