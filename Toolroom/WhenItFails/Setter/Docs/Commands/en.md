# WhenItFails Setter — command reference

This document describes all currently supported WhenItFails Setter commands.

## Invocation

From the Toolbox repository, use:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- <command> [arguments]
```

The double dash separates `dotnet run` arguments from Setter arguments.

Example:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

If Setter is later published as a standalone executable, the equivalent form may be:

```bash
when-it-fails-setter <command> [arguments]
```

The examples below use the shorter standalone form where it improves readability.

## Common path argument

Most commands accept:

```text
<path>
```

The path may point to:

* a project root containing `Jsons/WhenItFails`,
* the `Jsons/WhenItFails` directory itself.

Examples:

```bash
when-it-fails-setter validate ./MyProject
```

```bash
when-it-fails-setter validate ./MyProject/Jsons/WhenItFails
```

```bash
when-it-fails-setter validate .
```

## Common lookup argument

Commands that operate on one error accept:

```text
<id|code|name>
```

The error may be selected by:

* stable ID,
* numeric code,
* symbolic name.

Examples:

```text
AFW_NET_0001
600001
NETWORKUNAVAILABLE
```

Text lookup is case-insensitive.

## Exit codes

Setter commands use process exit codes.

General convention:

```text
0
→ command succeeded

1
→ command syntax or input was invalid

2
→ workspace validation, lookup, editing, or save failed

3
→ unexpected command-level or initialization failure
```

The exact meaning is documented for each command.

# Commands

## `help`

Shows the help screen.

```bash
when-it-fails-setter help
```

Aliases:

```bash
when-it-fails-setter --help
when-it-fails-setter -h
```

Running Setter without arguments also displays help.

```bash
when-it-fails-setter
```

Exit code:

```text
0
```

## `init <path>`

Creates missing WhenItFails workspace files.

```bash
when-it-fails-setter init ./MyProject
```

The default workspace structure is:

```text
Jsons/
└── WhenItFails/
    ├── errors.en.json
    ├── categories.en.json
    ├── code-groups.en.json
    ├── owners.en.json
    └── profiles.json
```

Behavior:

* creates missing directories,
* creates missing files from bundled templates,
* preserves existing files,
* never overwrites project-owned catalogs.

Repeated execution is safe.

`init` is not a reset command.

Exit codes:

```text
0
→ workspace created or already complete

1
→ path argument missing or invalid

3
→ workspace initialization failed
```

## `validate <path>`

Validates the complete workspace.

```bash
when-it-fails-setter validate .
```

Direct catalog-directory example:

```bash
when-it-fails-setter validate ./Jsons/WhenItFails
```

Validation includes:

* JSON loading,
* document validation,
* required fields,
* duplicate identities,
* owner references,
* code-group references,
* numeric ranges,
* categories,
* profiles,
* cross-catalog relationships.

Exit codes:

```text
0
→ workspace is valid

1
→ path argument missing or invalid

2
→ loading or validation errors found
```

Warnings do not necessarily make the workspace invalid, but they should still be reviewed.

## `summary <path>`

Shows a read-only workspace summary.

```bash
when-it-fails-setter summary .
```

The summary may include:

* catalog overview,
* error count,
* category count,
* code-group count,
* owner count,
* profile count,
* owner table,
* code-group table,
* profile table,
* category usage.

The workspace must validate before the summary is shown.

Exit codes:

```text
0
→ summary displayed

1
→ path argument missing or invalid

2
→ workspace loading or validation failed
```

## `inspect <path>`

Alias for:

```text
summary
```

Example:

```bash
when-it-fails-setter inspect ./MyProject
```

Behavior and exit codes are identical to `summary`.

## `errors <path> [filters]`

Lists error definitions.

```bash
when-it-fails-setter errors .
```

By default, Setter uses a rich terminal table.

Plain script-friendly output is available through:

```text
--plain
```

Example:

```bash
when-it-fails-setter errors . --plain
```

### Filters

| Option                 | Description                           |
| ---------------------- | ------------------------------------- |
| `--owner <value>`      | Filter by owner name.                 |
| `--group <value>`      | Filter by code-group name.            |
| `--code-group <value>` | Alias for `--group`.                  |
| `--category <value>`   | Filter by primary category.           |
| `--severity <value>`   | Filter by default severity.           |
| `--profile <value>`    | Resolve and filter through a profile. |
| `--search <text>`      | Search across error fields.           |
| `--plain`              | Produce plain tab-separated output.   |

### Owner filter

```bash
when-it-fails-setter errors . --owner AFW
```

### Code-group filter

```bash
when-it-fails-setter errors . --group NETWORK
```

Equivalent alias:

```bash
when-it-fails-setter errors . --code-group NETWORK
```

### Category filter

```bash
when-it-fails-setter errors . --category NETWORK
```

### Severity filter

```bash
when-it-fails-setter errors . --severity Warning
```

### Profile filter

```bash
when-it-fails-setter errors . --profile API
```

The profile may be selected by profile name or display name.

### Full-text search

```bash
when-it-fails-setter errors . --search timeout
```

Search may inspect fields such as:

* ID,
* numeric code,
* name,
* title,
* message,
* developer hint,
* documentation key,
* owner,
* code group,
* primary category,
* tags,
* subcategories.

### Combined filters

```bash
when-it-fails-setter errors . \
  --owner AFW \
  --category NETWORK \
  --severity Warning
```

Filters narrow the result together.

### Plain output

```bash
when-it-fails-setter errors . \
  --plain \
  --category NETWORK
```

Redirect example:

```bash
when-it-fails-setter errors . --plain > errors.tsv
```

Exit codes:

```text
0
→ error list displayed

1
→ path, arguments, or profile invalid

2
→ workspace loading or validation failed
```

An empty filtered result is not necessarily a command failure.

## `details <path> <id|code|name> [--plain]`

Shows one complete error definition.

By ID:

```bash
when-it-fails-setter details . AFW_NET_0001
```

By numeric code:

```bash
when-it-fails-setter details . 600001
```

By symbolic name:

```bash
when-it-fails-setter details . NETWORKUNAVAILABLE
```

Plain output:

```bash
when-it-fails-setter details . AFW_NET_0001 --plain
```

The command displays fields such as:

* ID,
* code,
* name,
* owner,
* code prefix,
* code group,
* primary category,
* categories,
* subcategories,
* title,
* message,
* severity,
* developer hint,
* documentation key,
* tags,
* metadata.

Exit codes:

```text
0
→ error displayed

1
→ arguments missing or error not found

2
→ workspace loading or validation failed
```

## `detail <path> <id|code|name>`

Alias for:

```text
details
```

Example:

```bash
when-it-fails-setter detail . AFW_NET_0001
```

Behavior and exit codes are identical to `details`.

# Write commands

All write commands use safe-write behavior.

The general workflow is:

```text
load workspace
→ locate target error
→ create updated in-memory document
→ validate updated document
→ create timestamped backup
→ save updated catalog
```

A failed edit must not replace the original catalog.

## `set-title <path> <id|code|name> <title>`

Changes the human-facing title.

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network is not available"
```

Everything after the lookup argument is joined into the new title.

This means quotes are recommended but not always technically required.

Preferred:

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network is not available"
```

The title should be:

* concise,
* human-readable,
* naturally capitalized,
* suitable for display.

Exit codes:

```text
0
→ title updated

1
→ required arguments missing

2
→ lookup, validation, backup, or save failed
```

## `set-message <path> <id|code|name> <message>`

Changes the human-facing error message.

```bash
when-it-fails-setter set-message . \
  AFW_NET_0001 \
  "The network is currently unavailable."
```

Everything after the lookup argument is joined into the new message.

The message should explain the failure in user-facing language.

Avoid placing sensitive internal details directly into this field.

Exit codes:

```text
0
→ message updated

1
→ required arguments missing

2
→ lookup, validation, backup, or save failed
```

## `set-developer-hint <path> <id|code|name> <developer-hint>`

Changes the developer-oriented diagnostic hint.

```bash
when-it-fails-setter set-developer-hint . \
  AFW_NET_0001 \
  "Check connectivity, DNS, proxy, VPN, and endpoint availability."
```

The developer hint may include:

* diagnostic guidance,
* likely causes,
* suggested checks,
* technical recovery advice.

It should not contain secrets, passwords, tokens, or private production data.

Everything after the lookup argument is joined into the new hint.

Exit codes:

```text
0
→ developer hint updated

1
→ required arguments missing

2
→ lookup, validation, backup, or save failed
```

## `set-severity <path> <id|code|name> <severity>`

Changes the default severity.

```bash
when-it-fails-setter set-severity . \
  AFW_NET_0001 \
  Warning
```

Supported values:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Severity matching should use one of the documented values.

Severity changes may affect:

* logging,
* monitoring,
* alerting,
* filtering,
* user-interface presentation,
* profile behavior.

Treat severity changes as behavioral catalog changes.

Exit codes:

```text
0
→ severity updated

1
→ required arguments missing or severity invalid

2
→ lookup, validation, backup, or save failed
```

## `set-documentation-key <path> <id|code|name> <documentation-key>`

Changes the documentation key.

```bash
when-it-fails-setter set-documentation-key . \
  AFW_NET_0001 \
  "when-it-fails/errors/network/network-unavailable"
```

A documentation key may connect the error with:

* online documentation,
* local help,
* troubleshooting pages,
* support articles,
* administration interfaces.

Recommended properties:

```text
stable
machine-friendly
human-reviewable
independent of deployment URL
```

The documentation key is not necessarily a full URL.

Everything after the lookup argument is joined into the new value.

Exit codes:

```text
0
→ documentation key updated

1
→ required arguments missing

2
→ lookup, validation, backup, or save failed
```

## `demo`

Shows a sample validation result.

```bash
when-it-fails-setter demo
```

The command is useful for:

* checking console rendering,
* previewing validation output,
* testing terminal colors and layout,
* developing presentation components.

It does not modify a workspace.

Exit code:

```text
0
```

# Unknown commands

An unsupported command produces an error and displays the help screen.

Example:

```bash
when-it-fails-setter dance
```

Exit code:

```text
1
```

# Unexpected exceptions

Unexpected command exceptions are rendered through Spectre.Console.

The application returns:

```text
3
```

This differs from ordinary validation or editing failures, which should normally return `2`.

# Scripting examples

## Validate in a shell script

```bash
if ! dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
then
  echo "WhenItFails workspace validation failed."
  exit 1
fi
```

## Export filtered errors

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --plain --category NETWORK \
  > network-errors.tsv
```

## Inspect one error

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001 --plain
```

# Recommended editing workflow

```text
validate
→ details
→ run one write command
→ details
→ validate
→ git diff
→ commit
```

Example:

```bash
when-it-fails-setter validate .

when-it-fails-setter details . AFW_NET_0001

when-it-fails-setter set-message . \
  AFW_NET_0001 \
  "The network is currently unavailable."

when-it-fails-setter details . AFW_NET_0001

when-it-fails-setter validate .

git diff
```

# Central principle

> Setter commands should make inspection easy, editing explicit, and failure visible through both output and exit code.
