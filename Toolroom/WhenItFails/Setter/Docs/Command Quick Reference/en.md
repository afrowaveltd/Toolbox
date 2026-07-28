# Command quick reference

This page is the compact command index for WhenItFails Setter.

Use it when you know the workflow and need the current command name or command family. For complete arguments, output contracts, examples, and command-specific exit codes, see [Commands](../Commands/en.md).

## Invocation

```bash
when-it-fails-setter <command> [arguments] [options]
```

From the Toolbox repository:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- <command> [arguments] [options]
```

## Workspace paths

Most workspace commands accept either the project root or the `Jsons/WhenItFails` package directory.

`init` is the important exception: it expects a project root and creates the default package directory beneath it.

## General and workspace commands

```text
help
demo
init
validate
reference
summary
inspect
```

`inspect` is an alias for `summary`.

## Error inspection and identity helpers

```text
errors
details
detail
next-code
suggest-doc-key
error-references
```

`detail` is an alias for `details`.

Error lookup commands accept a stable ID, numeric code, or symbolic name where documented.

## Error lifecycle

```text
add-error
remove-error
```

Before removing a public or referenced error, run `error-references` and review compatibility impact.

## Error field and collection editing

```text
set-name
set-title
set-message
set-developer-hint
set-severity
set-documentation-key
set-owner
set-code-group
set-primary-category
error-add-tag
error-remove-tag
error-add-category
error-remove-category
error-add-subcategory
error-remove-subcategory
error-set-metadata
error-remove-metadata
```

Write commands validate the proposed result, create a timestamped backup, and only then replace the affected catalog file.

## Profile browsing and explanation

```text
list-profiles
show-profile
explain-profile
```

## Profile lifecycle and text

```text
add-profile
remove-profile
set-profile-display-name
set-profile-description
```

## Profile include selectors

```text
profile-add-owner
profile-remove-owner
profile-add-category
profile-remove-category
profile-add-code-group
profile-remove-code-group
profile-add-subcategory
profile-remove-subcategory
profile-add-tag
profile-remove-tag
profile-add-error
profile-remove-error
```

## Profile exclusion selectors

```text
profile-add-excluded-tag
profile-remove-excluded-tag
profile-add-excluded-error
profile-remove-excluded-error
```

Exclusions are vetoes: an excluded error is removed even when an include selector also matches it.

## Profile mappings and metadata

```text
profile-set-default-mapping
profile-remove-default-mapping
profile-set-metadata
profile-remove-metadata
```

Default mappings and metadata describe profile behavior but do not select errors.

## Reference catalogs

```text
list-categories
show-category
list-code-groups
show-code-group
list-owners
show-owner
```

## Backups

```text
list-backups
restore-backup
```

Restore validates the selected backup and protects the current active file before replacement.

## Documentation checks

```text
check-doc-links
check-doc-keys
suggest-doc-key
```

`check-doc-links` validates local Markdown links.

`check-doc-keys` validates documentation-key presence, format, and uniqueness.

`suggest-doc-key` is read-only and can expose plain or JSON output where documented.

## Common output modes

```text
rich terminal output
--plain
--json
```

Support is command-specific. Rich output is for humans. Treat JSON output as the machine-readable contract only for commands that explicitly document it.

## Exit-code summary

```text
0  command succeeded
1  command syntax or input was invalid
2  workspace validation, lookup, editing, restore, or save failed
3  unexpected top-level failure
```

A command may document a more specific meaning while preserving this general classification.

## Recommended workflow

```text
pull the latest repository state
→ validate the workspace
→ inspect the target
→ run one focused write command
→ inspect the result
→ validate again
→ run tests
→ review git diff
→ commit
```

## Related documentation

- [Complete command reference](../Commands/en.md)
- [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Troubleshooting](../Troubleshooting/en.md)

> Setter should make inspection easy, editing explicit, and failure visible through both output and exit code.
