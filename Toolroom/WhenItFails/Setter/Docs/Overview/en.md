# WhenItFails Setter Overview

WhenItFails Setter is a `.NET 10` command-line tool for creating, inspecting, validating, editing, reviewing, and recovering project-local WhenItFails catalog workspaces.

The standard workspace lives under:

```text
Jsons/WhenItFails
```

Typical catalog files include:

```text
errors.en.json
categories.en.json
code-groups.en.json
owners.en.json
profiles.json
```

Setter is intended for catalog authors, maintainers, CI pipelines, support tooling, and other automation that needs explicit, reviewable control over these files.

## What Setter provides

Setter groups its current capabilities into several practical areas.

### Workspace lifecycle

Use Setter to initialize and validate a workspace:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- init .
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
dotnet run --project Toolroom/WhenItFails/Setter -- summary .
```

The workspace is validated as a package. A single JSON file may be syntactically valid while still containing invalid cross-catalog references.

### Catalog discovery

Setter can inspect the reference catalogs and browse the current error definitions.

Important commands include:

- `reference` for owners, categories, code groups, and related reference data;
- `errors` for filtered error browsing;
- `details` for one complete error definition;
- `error-references` for finding catalog references to an error;
- `next-code` for identifying an available numeric code;
- `suggest-doc-key` for preparing a documentation key.

These commands help authors understand the workspace before changing it.

### Error authoring

Setter supports explicit error creation, removal, and focused editing.

Examples include:

- `add-error`;
- `remove-error`;
- `set-title`;
- `set-message`;
- `set-developer-hint`;
- `set-severity`;
- `set-documentation-key`;
- tag, alias, owner, category, and code-group operations.

Prefer focused commands over broad manual rewrites. They provide validation, structured failures, predictable serialization, and safe persistence behavior.

### Profiles, mappings, and metadata

Setter supports profile creation and maintenance, selectors, mappings, and metadata.

Use `explain-profile` to inspect the effective profile selection and the reasons errors are included or excluded.

Profile and mapping changes are policy changes, not merely formatting changes. Review their effective behavior before committing them.

### Documentation integrity

Setter includes dedicated checks for catalog documentation:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
```

Use `check-doc-keys` to validate documentation-key quality and `check-doc-links` to validate local Markdown links.

### Backups and recovery

Successful writes to existing catalog files create timestamped `.bak.json` files.

Use:

- `list-backups` to discover available backups;
- `restore-backup` to restore one explicitly selected backup.

A backup is a recovery candidate, not proof that the workspace state is correct. Inspect the selected backup, restore it deliberately, validate the complete workspace, review the Git diff, and run the focused tests.

## Output modes

Setter separates human presentation from machine integration.

### Rich output

Rich terminal output is intended for interactive use. It may contain Spectre.Console tables, panels, borders, and other presentation details.

Do not parse rich output as a stable data schema.

### Plain output

`--plain` provides simpler human-readable output without rich terminal decoration. It is useful for logs, redirection, and copy-and-paste workflows.

Plain output remains presentation-oriented.

### JSON output

`--json` provides machine-readable output for supported commands.

Machine consumers should use JSON output and process exit codes.

Automation should parse the JSON structure rather than terminal decoration or incidental whitespace.

## Exit-code model

Setter uses a stable broad process classification:

```text
0  success
1  invalid command use or arguments
2  expected workspace, validation, lookup, or operation failure
3  unexpected top-level application failure
```

The process exit code describes the broad result. Structured issue codes and JSON fields describe the specific cause.

## Safe-write model

A normal single-file write follows this shape:

```text
parse and validate input
→ load current workspace data
→ apply the focused change in memory
→ validate the resulting data
→ serialize a complete temporary file
→ create a timestamped backup when the target exists
→ replace the target
→ report the result
```

Rejected input or failed pre-write validation must leave the target file unchanged and must not create a backup.

Setter safe writes are single-file operations. They are not a multi-file transaction and not a multi-process locking system.

## Recommended author workflow

For a normal catalog change:

1. inspect `git status --short`;
2. validate the workspace;
3. inspect references and current definitions;
4. make one focused change;
5. inspect the affected error, profile, mapping, or metadata;
6. run documentation checks when relevant;
7. review backups and the complete Git diff;
8. run the focused Setter suite;
9. update `IMPLEMENTATION_STATUS.md`;
10. commit one logical green step.

The focused verification gate is:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Do not continue while this suite is red.

## Documentation map

Use the specialized guides for details:

- [Getting Started](../Getting-Started/en.md)
- [Commands](../Commands/en.md)
- [Command Quick Reference](../Command%20Quick%20Reference/en.md)
- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Reviewing Catalog Changes](../Reviewing%20Catalog%20Changes/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
- [Testing and CI](../Testing%20and%20CI/en.md)
- [Architecture Overview](../Architecture%20Overview/en.md)
- [Adding a New Command](../Adding%20a%20New%20Command/en.md)
- [Contributing to Setter](../Contributing%20to%20Setter/en.md)
- [Maintainer Notes](../Maintainer%20Notes/en.md)
- [Known Limitations](../Known%20Limitations/en.md)
- [Roadmap and Future Work](../Roadmap%20and%20Future%20Work/en.md)

## Current boundaries

Setter intentionally does not claim to provide automatic schema migration, multi-file atomic transactions, multi-process locking, automatic backup-retention cleanup, remote synchronization, package publishing, or a GUI/TUI.

These are explicit boundaries, not hidden promises.

## Central principle

> Setter should make the safe path obvious: inspect first, change one thing, validate the result, review the persisted files, and stop whenever verification is red.
