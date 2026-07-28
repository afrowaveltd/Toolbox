# WhenItFails Setter

A command-line tool for creating, validating, inspecting, and safely editing Afrowave.Toolbox.WhenItFails JSON workspaces.

Setter keeps catalog maintenance explicit and reviewable. Read-only commands never change workspace files. Write commands validate the proposed result, create timestamped backups, and only then replace the affected catalog file.

## Quick start

### Show help

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- help
```

Running the application without arguments also shows help.

### Initialize a workspace

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- init ./MyProject
```

The command creates only missing WhenItFails JSON files. Existing files are preserved.

### Validate a workspace

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Validation checks the complete workspace, including cross-catalog relationships.

### Show a workspace summary

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- summary .
```

The alias `inspect` performs the same operation.

### Browse errors

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- errors .
```

Filters may be combined:

```text
--owner
--group
--category
--severity
--profile
--search
--plain
```

Example:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  errors . --category NETWORK --severity Error
```

### Inspect one error

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  details . AFW_NET_0001
```

An error may be selected by stable ID, numeric code, or symbolic name. The singular alias `detail` is also available.

### Add an error definition

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  add-error . AFW NET NETWORKUNAVAILABLE \
  "Network is unavailable" \
  "The network is currently unavailable."
```

Use `next-code` first when a safe numeric-code suggestion is needed. See [Adding errors](Docs/Adding%20Errors/en.md) for the complete contract and required arguments.

### Remove an error definition

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  remove-error . AFW_NET_0001
```

Removal is compatibility-sensitive. Use `error-references` first to inspect profile and catalog references.

### Preview a documentation key

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  suggest-doc-key . NETWORK "Connection interrupted"
```

The command is read-only. It resolves the category name or alias and returns the first available canonical key without changing the workspace or creating a backup.

Use `--plain` for only the key or `--json` for the stable structured success/failure contract.

## Command groups

### Workspace and validation

| Command | Description |
| --- | --- |
| `help` | Show the help screen. |
| `demo` | Show a sample validation result. |
| `init <path>` | Create missing workspace files. |
| `validate <path>` | Validate the complete workspace. |
| `reference <path>` | Show reference-catalog information. |
| `summary <path>` | Show a read-only workspace summary. |
| `inspect <path>` | Alias for `summary`. |

### Error inspection and authoring

| Command | Description |
| --- | --- |
| `errors <path> [filters]` | List and filter error definitions. |
| `details <path> <id\|code\|name>` | Show one error definition. |
| `detail <path> <id\|code\|name>` | Alias for `details`. |
| `next-code <path> <owner> <code-group>` | Suggest the next available numeric code. |
| `add-error <path> ...` | Add and validate a new error definition. |
| `remove-error <path> <id\|code\|name>` | Remove an error after reference checks. |
| `error-references <path> <id\|code\|name>` | Show references to an error definition. |
| `error-add-tag` / `error-remove-tag` | Edit error tags. |
| `error-add-category` / `error-remove-category` | Edit additional categories. |
| `error-add-subcategory` / `error-remove-subcategory` | Edit subcategories. |
| `error-set-metadata` / `error-remove-metadata` | Edit metadata entries. |
| `set-primary-category` | Change the primary category. |
| `set-owner` | Change the owner. |
| `set-code-group` | Change the code group. |
| `set-name` | Change the symbolic name. |
| `set-title` | Change the title. |
| `set-message` | Change the user-facing message. |
| `set-developer-hint` | Change the developer hint. |
| `set-severity` | Change the default severity. |
| `set-documentation-key` | Change the documentation key. |

### Profiles

| Command | Description |
| --- | --- |
| `list-profiles` | List profiles. |
| `show-profile` | Show one profile. |
| `explain-profile` | Explain why errors match a profile. |
| `add-profile` / `remove-profile` | Add or remove a profile. |
| `set-profile-display-name` | Change a profile display name. |
| `set-profile-description` | Change a profile description. |
| `profile-add-owner` / `profile-remove-owner` | Edit included owners. |
| `profile-add-category` / `profile-remove-category` | Edit included categories. |
| `profile-add-code-group` / `profile-remove-code-group` | Edit included code groups. |
| `profile-add-subcategory` / `profile-remove-subcategory` | Edit included subcategories. |
| `profile-add-tag` / `profile-remove-tag` | Edit included tags. |
| `profile-add-excluded-tag` / `profile-remove-excluded-tag` | Edit excluded tags. |
| `profile-add-error` / `profile-remove-error` | Edit explicitly included errors. |
| `profile-add-excluded-error` / `profile-remove-excluded-error` | Edit explicitly excluded errors. |
| `profile-set-default-mapping` / `profile-remove-default-mapping` | Edit default mappings. |
| `profile-set-metadata` / `profile-remove-metadata` | Edit profile metadata. |

### Reference catalogs

| Command | Description |
| --- | --- |
| `list-categories` / `show-category` | Browse categories. |
| `list-code-groups` / `show-code-group` | Browse code groups. |
| `list-owners` / `show-owner` | Browse owners. |

### Backups and documentation checks

| Command | Description |
| --- | --- |
| `list-backups` | List timestamped catalog backups. |
| `restore-backup` | Safely restore a selected backup. |
| `check-doc-links` | Check local Markdown documentation links. |
| `check-doc-keys` | Check documentation-key completeness and uniqueness. |
| `suggest-doc-key <path> <category-name\|alias> <title> [--plain\|--json]` | Suggest an available canonical key without writing. |

## Rich, plain, and JSON output

Setter uses Spectre.Console output for interactive terminal use. Commands that support `--plain` produce simpler output for scripts, redirection, text processing, and CI logs.

Selected commands also expose a versioned `--json` contract. JSON output is a public machine-readable interface and is covered by dedicated tests. Do not parse rich terminal output in automation.

## Safe writes

Write commands follow this conservative workflow:

```text
locate the target
→ create the proposed in-memory change
→ validate the resulting workspace
→ create a timestamped backup
→ replace the affected file
```

A failed validation must not replace the original catalog. Setter does not silently rewrite unrelated files.

## Backups and recovery

Backups are stored next to the affected catalog file. Use:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- list-backups .
```

and restore a selected backup with:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  restore-backup . <backup-file>
```

Restore operations are validated and protect the current active file before replacement.

## Workspace location

Most commands accept either the project root or the WhenItFails package directory. The expected default workspace is:

```text
Jsons/
└── WhenItFails/
    ├── errors.en.json
    ├── categories.en.json
    ├── code-groups.en.json
    ├── owners.en.json
    └── profiles.json
```

`init` expects a project root and creates the default structure beneath it.

## Current scope

Setter currently supports the complete day-to-day authoring workflow for error definitions and profiles:

- workspace creation and validation,
- catalog and reference inspection,
- error creation, removal, and focused field editing,
- error tags, categories, subcategories, and metadata,
- profile creation, removal, selection rules, mappings, and metadata,
- safe backups and validated restore,
- documentation-link and documentation-key checks,
- rich human output plus selected plain and JSON automation contracts.

The remaining boundaries are deliberate. Setter is not a full IDE, remote catalog registry, package publisher, localization platform, or general-purpose JSON editor.

## Documentation

- [Documentation Map](Docs/Documentation%20Map/en.md)
- [FAQ](Docs/FAQ/en.md)
- [Glossary](Docs/Glossary/en.md)
- [Getting started](Docs/Getting-Started/en.md)
- [Overview](Docs/Overview/en.md)
- [Commands](Docs/Commands/en.md)
- [Exit Codes and Automation](Docs/Exit%20Codes%20and%20Automation/en.md)
- [Command Quick Reference](Docs/Command%20Quick%20Reference/en.md)
- [Windows and PowerShell](Docs/Windows%20and%20PowerShell/en.md)
- [Linux and Bash](Docs/Linux%20and%20Bash/en.md)
- [Workspace Paths and Initialization](Docs/Workspace%20Paths%20and%20Initialization/en.md)
- [Catalog Files](Docs/Catalog%20Files/en.md)
- [Naming and Numbering Conventions](Docs/Naming%20and%20Numbering%20Conventions/en.md)
- [Deprecation and Migration](Docs/Deprecation%20and%20Migration/en.md)
- [Schema Evolution](Docs/Schema%20Evolution/en.md)
- [Known Limitations](Docs/Known%20Limitations/en.md)
- [Roadmap and Future Work](Docs/Roadmap%20and%20Future%20Work/en.md)
- [Adding a New Category](Docs/Adding%20a%20New%20Category/en.md)
- [Adding a New Code Group](Docs/Adding%20a%20New%20Code%20Group/en.md)
- [Adding a New Owner](Docs/Adding%20a%20New%20Owner/en.md)
- [Catalog Author Checklist](Docs/Catalog%20Author%20Checklist/en.md)
- [Reviewing Catalog Changes](Docs/Reviewing%20Catalog%20Changes/en.md)
- [Validation](Docs/Validation/en.md)
- [Workspace Summary](Docs/Workspace%20Summary/en.md)
- [Profiles](Docs/Profiles/en.md)
- [Adding a New Profile](Docs/Adding%20a%20New%20Profile/en.md)
- [Browsing and Filtering Errors](Docs/Browsing%20and%20Filtering%20Errors/en.md)
- [Inspecting Error Details](Docs/Inspecting%20Error%20Details/en.md)
- [Editing error fields](Docs/Editing%20Error%20Fields/en.md)
- [Adding a New Error Definition](Docs/Adding%20a%20New%20Error%20Definition/en.md)
- [Adding errors](Docs/Adding%20Errors/en.md)
- [Authoring Error Text](Docs/Authoring%20Error%20Text/en.md)
- [Setting Title](Docs/Setting%20Title/en.md)
- [Plain Output](Docs/Plain%20Output/en.md)
- [Safe Writes](Docs/Safe%20Writes/en.md)
- [Backups and Recovery](Docs/Backups%20and%20Recovery/en.md)
- [Testing and CI](Docs/Testing%20and%20CI/en.md)
- [Contributing to Setter](Docs/Contributing%20to%20Setter/en.md)
- [Architecture Overview](Docs/Architecture%20Overview/en.md)
- [Adding a New Command](Docs/Adding%20a%20New%20Command/en.md)
- [Maintainer Notes](Docs/Maintainer%20Notes/en.md)
- [Release Checklist](Docs/Release%20Checklist/en.md)
- [Troubleshooting](Docs/Troubleshooting/en.md)

## Design

- **Small commands** — each command has its own class.
- **Thin dispatcher** — `Program.cs` routes commands without owning catalog logic.
- **Safe by default** — writes validate first and create timestamped backups.
- **Explicit automation contracts** — plain and JSON output are opt-in command behavior.
- **Project ownership** — Setter changes project catalogs only through explicit commands.
- **Shared catalog rules** — Setter reuses WhenItFails runtime models and validation.

> Localized versions of this documentation may later be generated by Afrowave translation tooling.
