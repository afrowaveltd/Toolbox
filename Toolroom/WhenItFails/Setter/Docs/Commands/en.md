# WhenItFails Setter — command reference

This document lists the currently supported Setter commands.

## Invocation

From the Toolbox repository:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- <command> [arguments]
```

Published executable form:

```bash
when-it-fails-setter <command> [arguments]
```

Most examples below use the shorter executable form.

## Path argument

`<path>` may point to:

```text
a project root containing Jsons/WhenItFails
or
the Jsons/WhenItFails directory itself
```

## Error lookup

Commands accepting `<id|code|name>` can locate an error by:

```text
stable ID
numeric code
symbolic name
```

Text lookup is case-insensitive.

## Exit codes

```text
0  command succeeded
1  command syntax or input was invalid
2  workspace validation, lookup, editing, or save failed
3  unexpected command-level failure
```

# General commands

## `help`

```bash
when-it-fails-setter help
when-it-fails-setter --help
when-it-fails-setter -h
```

Running Setter without arguments also displays help.

## `demo`

Displays a sample validation result without modifying a workspace.

```bash
when-it-fails-setter demo
```

## `init <path>`

Creates missing workspace files from bundled templates and preserves existing files.

```bash
when-it-fails-setter init .
```

Created catalog set:

```text
errors.en.json
categories.en.json
code-groups.en.json
owners.en.json
profiles.json
```

## `validate <path>`

Loads and validates the complete workspace, including cross-catalog references.

```bash
when-it-fails-setter validate .
```

## `summary <path>`

Displays a read-only workspace summary.

```bash
when-it-fails-setter summary .
```

## `inspect <path>`

Alias for `summary`.

```bash
when-it-fails-setter inspect .
```

# Error inspection

## `errors <path> [filters]`

Lists error definitions.

```bash
when-it-fails-setter errors .
```

Supported options:

| Option | Meaning |
| --- | --- |
| `--owner <value>` | Filter by owner. |
| `--group <value>` | Filter by code group. |
| `--code-group <value>` | Alias for `--group`. |
| `--category <value>` | Filter by category. |
| `--severity <value>` | Filter by severity. |
| `--profile <value>` | Resolve errors through a profile. |
| `--search <text>` | Search error fields. |
| `--plain` | Produce tab-separated output. |

Example:

```bash
when-it-fails-setter errors . \
  --profile WEB \
  --severity Warning \
  --search network \
  --plain
```

Profile resolution runs first. Ordinary command filters then narrow the resolved result.

## `details <path> <id|code|name> [--plain]`

Displays one complete error definition.

```bash
when-it-fails-setter details . AFW_NET_0001
when-it-fails-setter details . 600001 --plain
```

## `detail <path> <id|code|name>`

Alias for `details`.

## `suggest-doc-key <path> <category-name|alias> <title> [--plain|--json]`

Resolves the category and suggests the first available canonical documentation key without modifying the workspace.

```bash
when-it-fails-setter suggest-doc-key . \
  NETWORK \
  "Connection interrupted"
```

Plain output returns only the suggested key:

```bash
when-it-fails-setter suggest-doc-key . \
  NETWORK \
  "Connection interrupted" \
  --plain
```

JSON output uses the standard command envelope and returns the same stable result fields for success and failure:

```text
category
title
documentationKey
failureCode
failureMessage
```

The command is read-only. It does not change `errors.en.json` and does not create a backup.

Exit codes:

```text
0  suggestion produced
1  command arguments were invalid
2  workspace loading, category lookup, or key generation failed
```

# Catalog browsing

## `list-profiles <path> [--plain]`

```bash
when-it-fails-setter list-profiles .
```

## `show-profile <path> <profile-name> [--plain]`

Displays all profile selectors, mappings, and metadata.

```bash
when-it-fails-setter show-profile . WEB --plain
```

Profiles may be selected by stable name or display name.

## `list-categories <path> [--plain]`

```bash
when-it-fails-setter list-categories .
```

## `show-category <path> <category-name> [--plain]`

Finds a category by name, display name, or alias.

```bash
when-it-fails-setter show-category . NETWORK
```

## `list-code-groups <path> [--plain]`

```bash
when-it-fails-setter list-code-groups .
```

## `show-code-group <path> <group-name|prefix> [--plain]`

```bash
when-it-fails-setter show-code-group . NETWORK
```

## `list-owners <path> [--plain]`

```bash
when-it-fails-setter list-owners .
```

## `show-owner <path> <owner-name|alias> [--plain]`

```bash
when-it-fails-setter show-owner . AFW
```

# Error write commands

All write commands use validation, temporary-file writing, timestamped backup creation, and atomic replacement of the target catalog.

Invalid or no-op edits do not replace the catalog.

## `set-title <path> <id|code|name> <title>`

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network is unavailable"
```

## `set-message <path> <id|code|name> <message>`

```bash
when-it-fails-setter set-message . \
  AFW_NET_0001 \
  "The network is currently unavailable."
```

## `set-developer-hint <path> <id|code|name> <developer-hint>`

```bash
when-it-fails-setter set-developer-hint . \
  AFW_NET_0001 \
  "Check connectivity, DNS, proxy, VPN, and endpoint availability."
```

## `set-severity <path> <id|code|name> <severity>`

Supported severities:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Example:

```bash
when-it-fails-setter set-severity . AFW_NET_0001 Warning
```

## `set-documentation-key <path> <id|code|name> <documentation-key>`

```bash
when-it-fails-setter set-documentation-key . \
  AFW_NET_0001 \
  when-it-fails/errors/network/network-unavailable
```

# Profile lifecycle and text

## `add-profile <path> <name> <display-name> [description]`

```bash
when-it-fails-setter add-profile . \
  DITA \
  "DiTa" \
  "Disk diagnostics profile"
```

## `remove-profile <path> <name>`

```bash
when-it-fails-setter remove-profile . DITA
```

## `set-profile-display-name <path> <profile-name> <display-name>`

```bash
when-it-fails-setter set-profile-display-name . \
  DITA \
  "DiTa storage tools"
```

## `set-profile-description <path> <profile-name> <description>`

```bash
when-it-fails-setter set-profile-description . \
  DITA \
  "Profile for disk diagnostics and storage tools."
```

Pass an empty quoted string to clear the description.

# Profile include selectors

## Owners

```bash
when-it-fails-setter profile-add-owner . DITA AFW
when-it-fails-setter profile-remove-owner . DITA AFW
```

The owner may be selected by canonical name or alias.

## Categories

```bash
when-it-fails-setter profile-add-category . DITA FILE_SYSTEM
when-it-fails-setter profile-remove-category . DITA FILE_SYSTEM
```

The category may be selected by canonical name or alias.

## Code groups

```bash
when-it-fails-setter profile-add-code-group . DITA STORAGE
when-it-fails-setter profile-remove-code-group . DITA STORAGE
```

The group may be selected by canonical name or prefix.

## Subcategories

```bash
when-it-fails-setter profile-add-subcategory . DITA TIMEOUT
when-it-fails-setter profile-remove-subcategory . DITA TIMEOUT
```

The value must exist in at least one error definition.

## Included tags

```bash
when-it-fails-setter profile-add-tag . DITA USER_VISIBLE
when-it-fails-setter profile-remove-tag . DITA USER_VISIBLE
```

The tag must exist in at least one error definition.

## Explicitly included errors

```bash
when-it-fails-setter profile-add-error . DITA AFW_NET_0001
when-it-fails-setter profile-remove-error . DITA AFW_NET_0001
```

The lookup may be an error ID, numeric code, or symbolic name. Setter stores the canonical error ID.

# Profile exclusion selectors

## Excluded tags

```bash
when-it-fails-setter profile-add-excluded-tag . DITA DEBUG_ONLY
when-it-fails-setter profile-remove-excluded-tag . DITA DEBUG_ONLY
```

## Explicitly excluded errors

```bash
when-it-fails-setter profile-add-excluded-error . DITA AFW_NET_0001
when-it-fails-setter profile-remove-excluded-error . DITA AFW_NET_0001
```

Exclusions are vetoes. An excluded error is removed even when it also matches an include selector.

# Profile default mappings

## `profile-set-default-mapping <path> <profile-name> <mapping-key> <mapping-value>`

Adds or updates one string mapping.

```bash
when-it-fails-setter profile-set-default-mapping . \
  DITA \
  web.problemDetails \
  true
```

Mapping keys are normalized with `TextKeyNormalizer.NormalizeKey`.

```text
web.problemDetails
→ WEB_PROBLEMDETAILS
```

Quote values containing spaces.

## `profile-remove-default-mapping <path> <profile-name> <mapping-key>`

```bash
when-it-fails-setter profile-remove-default-mapping . \
  DITA \
  web.problemDetails
```

# Profile metadata

## `profile-set-metadata <path> <profile-name> <metadata-key> <metadata-value>`

Adds or updates one metadata string.

```bash
when-it-fails-setter profile-set-metadata . \
  DITA \
  documentation.owner \
  "DiTa Team"
```

Metadata keys use the same key normalization as default mappings.

## `profile-remove-metadata <path> <profile-name> <metadata-key>`

```bash
when-it-fails-setter profile-remove-metadata . \
  DITA \
  documentation.owner
```

# Profile resolution summary

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

Non-empty include collections combine with OR.

Exclusion rules are vetoes.

`defaultMappings` and `metadata` do not select errors.

# Recommended workflow

```text
validate
→ inspect target
→ run one write command
→ inspect target again
→ validate
→ git diff
→ commit
```

Example:

```bash
when-it-fails-setter validate .
when-it-fails-setter show-profile . DITA --plain
when-it-fails-setter profile-add-tag . DITA STORAGE
when-it-fails-setter errors . --profile DITA --plain
when-it-fails-setter validate .
git diff
```

# Unknown commands

An unsupported command returns exit code `1` and displays the help screen.

# Central principle

> Setter should make inspection easy, editing explicit, and failure visible through both output and exit code.
