# WhenItFails Setter — command reference

This document lists the commands currently registered by WhenItFails Setter.

## Invocation

From the Toolbox repository:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- <command> [arguments]
```

Published executable form:

```bash
when-it-fails-setter <command> [arguments]
```

Most commands accept a project root containing `Jsons/WhenItFails` or the package directory itself. `init` expects a project root.

Commands accepting `<id|code|name>` resolve an error by stable ID, numeric code, or symbolic name.

## Exit codes

```text
0  command succeeded
1  command syntax or input was invalid
2  workspace validation, lookup, editing, or save failed
3  unexpected command-level failure
```

## General and workspace commands

| Command | Purpose |
| --- | --- |
| `help`, `--help`, `-h` | Show help. Running Setter without arguments also shows help. |
| `demo` | Show a sample validation result without changing a workspace. |
| `init <path>` | Create missing workspace files from bundled templates. Existing files are preserved. |
| `validate <path>` | Validate the complete workspace and cross-catalog relationships. |
| `reference <path>` | Show reference-catalog information. |
| `summary <path>` | Show a read-only workspace summary. |
| `inspect <path>` | Alias for `summary`. |

## Error inspection

| Command | Purpose |
| --- | --- |
| `errors <path> [filters]` | List and filter error definitions. |
| `details <path> <id|code|name> [--plain]` | Show one complete error definition. |
| `detail <path> <id|code|name>` | Alias for `details`. |
| `error-references <path> <id|code|name>` | Show profile and catalog references to an error. |
| `next-code <path> <owner> <code-group>` | Suggest the next available numeric code without writing. |

The `errors` command supports:

```text
--owner <value>
--group <value>
--code-group <value>
--category <value>
--severity <value>
--profile <value>
--search <text>
--plain
```

Profile resolution runs first. Other filters then narrow the resolved result.

## Error lifecycle

| Command | Purpose |
| --- | --- |
| `add-error <path> ...` | Add and validate a new error definition. |
| `remove-error <path> <id|code|name>` | Remove an error definition after compatibility checks. |

Use `next-code` before `add-error` when a numeric-code suggestion is needed. Use `error-references` before removal.

## Error identity and text

| Command | Purpose |
| --- | --- |
| `set-owner` | Change the canonical owner. |
| `set-code-group` | Change the code group. |
| `set-primary-category` | Change the primary category. |
| `set-name` | Change the symbolic name. |
| `set-title` | Change the title. |
| `set-message` | Change the user-facing message. |
| `set-developer-hint` | Change the developer hint. |
| `set-severity` | Change the default severity. |
| `set-documentation-key` | Change the documentation key. |

Supported severity values:

```text
Trace
Debug
Information
Warning
Error
Critical
```

## Error collections and metadata

| Command | Purpose |
| --- | --- |
| `error-add-tag` / `error-remove-tag` | Add or remove an error tag. |
| `error-add-category` / `error-remove-category` | Add or remove an additional category. |
| `error-add-subcategory` / `error-remove-subcategory` | Add or remove a subcategory. |
| `error-set-metadata` / `error-remove-metadata` | Add, replace, or remove metadata. |

Write commands resolve canonical values, reject invalid or no-op changes, validate the result, create a timestamped backup, and replace only the affected catalog file.

## Profile inspection

| Command | Purpose |
| --- | --- |
| `list-profiles <path> [--plain]` | List profiles. |
| `show-profile <path> <profile-name> [--plain]` | Show one profile and all selectors, mappings, and metadata. |
| `explain-profile <path> <profile-name> [--plain|--json]` | Explain why errors are included or excluded. |

Profiles may be selected by canonical name or display name where supported.

## Profile lifecycle and text

| Command | Purpose |
| --- | --- |
| `add-profile <path> <name> <display-name> [description]` | Add a profile. |
| `remove-profile <path> <name>` | Remove a profile. |
| `set-profile-display-name` | Change a profile display name. |
| `set-profile-description` | Change or clear a profile description. |

## Profile include selectors

| Command | Purpose |
| --- | --- |
| `profile-add-owner` / `profile-remove-owner` | Edit included owners. |
| `profile-add-category` / `profile-remove-category` | Edit included categories. |
| `profile-add-code-group` / `profile-remove-code-group` | Edit included code groups. |
| `profile-add-subcategory` / `profile-remove-subcategory` | Edit included subcategories. |
| `profile-add-tag` / `profile-remove-tag` | Edit included tags. |
| `profile-add-error` / `profile-remove-error` | Edit explicitly included errors. |

## Profile exclusions

| Command | Purpose |
| --- | --- |
| `profile-add-excluded-tag` / `profile-remove-excluded-tag` | Edit excluded tags. |
| `profile-add-excluded-error` / `profile-remove-excluded-error` | Edit explicitly excluded errors. |

Exclusions are vetoes. An excluded error is removed even when it matches an include selector.

## Profile mappings and metadata

| Command | Purpose |
| --- | --- |
| `profile-set-default-mapping` / `profile-remove-default-mapping` | Edit default string mappings. |
| `profile-set-metadata` / `profile-remove-metadata` | Edit profile metadata. |

Mappings and metadata do not select errors.

## Reference catalogs

| Command | Purpose |
| --- | --- |
| `list-categories <path> [--plain]` | List categories. |
| `show-category <path> <category-name> [--plain]` | Show a category selected by name, display name, or alias. |
| `list-code-groups <path> [--plain]` | List code groups. |
| `show-code-group <path> <group-name|prefix> [--plain]` | Show a code group. |
| `list-owners <path> [--plain]` | List owners. |
| `show-owner <path> <owner-name|alias> [--plain]` | Show an owner. |

## Backups

| Command | Purpose |
| --- | --- |
| `list-backups <path> [--plain|--json]` | List timestamped catalog backups. |
| `restore-backup <path> <backup-file> [--plain|--json]` | Restore a selected backup through the validated safe-write workflow. |

Restore protects the current active file before replacement and validates the restored workspace.

## Documentation checks

| Command | Purpose |
| --- | --- |
| `check-doc-links <path> [--plain|--json]` | Check local Markdown links and expected documentation files. |
| `check-doc-keys <path> [--plain|--json]` | Check documentation-key presence, canonical format, and uniqueness. |
| `suggest-doc-key <path> <category-name|alias> <title> [--plain|--json]` | Suggest the first available canonical documentation key without writing. |

The command is read-only. It does not change `errors.en.json` and does not create a backup.

JSON output for `suggest-doc-key` uses the standard command envelope and exposes:

```text
category
title
documentationKey
failureCode
failureMessage
```

Exit codes for `suggest-doc-key`:

```text
0  suggestion produced
1  command arguments were invalid
2  workspace loading, category lookup, or key generation failed
```

## Output modes

Rich Spectre.Console output is intended for humans. Commands supporting `--plain` provide simpler script-friendly output. Selected commands provide a versioned `--json` contract covered by dedicated tests.

Do not parse rich terminal output in automation.

## Profile resolution summary

The shared `ErrorProfileResolver` evaluates:

```text
includeOwners
includeCodeGroups
includeCategories
includeSubcategories
includeTags
includeErrors
excludeTags
excludeErrors
```

Non-empty include collections combine with OR. Exclusion rules are vetoes. `defaultMappings` and `metadata` do not select errors.

## Recommended workflow

```text
validate
→ inspect target
→ run one write command
→ inspect target again
→ validate
→ git diff
→ commit
```

## Unknown commands

An unsupported command returns exit code `1` and displays the help screen.

## Central principle

> Setter should make inspection easy, editing explicit, and failure visible through both output and exit code.
