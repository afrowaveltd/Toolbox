# WhenItFails Setter — Commands

All commands follow the pattern:

```
dotnet run --project <Setter.csproj> -- <command> [arguments...]
```

Or, if published as a standalone tool:

```
when-it-fails-setter <command> [arguments...]
```

## Command Reference

### `help`

Shows the help screen with all available commands.

```
when-it-fails-setter help
when-it-fails-setter --help
when-it-fails-setter -h
```

### `init <path>`

Creates missing WhenItFails JSON files under the specified project root.

```
when-it-fails-setter init ./MyProject
```

The command creates a `Jsons/WhenItFails/` directory with default catalog files (errors, categories, code groups, owners, profiles). Existing files are skipped.

**Exit codes:**
- `0` — Workspace created or already exists.
- `1` — Missing path argument.
- `3` — Initialization failed.

### `validate <path>`

Validates all WhenItFails JSON files at the specified path.

```
when-it-fails-setter validate ./MyProject
when-it-fails-setter validate ./MyProject/Jsons/WhenItFails
```

Accepts either a project root or a direct `Jsons/WhenItFails` directory path. Performs per-document validation and cross-document validation.

**Exit codes:**
- `0` — All files are valid.
- `1` — Missing path argument.
- `2` — Validation errors found.

### `summary <path>` / `inspect <path>`

Shows a read-only summary of the workspace: catalog overview, owner table, code group table, profile table, and top categories by error count.

```
when-it-fails-setter summary .
when-it-fails-setter inspect ./MyProject
```

`inspect` is an alias for `summary`.

**Exit codes:**
- `0` — Summary displayed.
- `1` — Missing path argument.
- `2` — Validation errors found (summary not shown).

### `errors <path> [filters...]`

Lists error definitions with optional filters.

```
when-it-fails-setter errors .
when-it-fails-setter errors . --category NETWORK
when-it-fails-setter errors . --owner AFW --severity Error
when-it-fails-setter errors . --profile API
when-it-fails-setter errors . --search timeout
when-it-fails-setter errors . --plain --category NETWORK
```

**Filters:**

| Option | Description |
|---|---|
| `--owner <value>` | Filter by owner name (exact match, case-insensitive). |
| `--group <value>` | Filter by code group name. Alias: `--code-group`. |
| `--category <value>` | Filter by primary category name. |
| `--severity <value>` | Filter by severity (e.g., `Error`, `Warning`, `Information`). |
| `--profile <value>` | Filter by profile name or display name. |
| `--search <text>` | Full-text search across id, name, title, message, hint, documentation key, code, owner, group, category, tags, and subcategories. |
| `--plain` | Output as plain tab-separated text instead of rich tables. |

**Exit codes:**
- `0` — Errors listed.
- `1` — Missing path or unknown profile.
- `2` — Validation errors found.

### `details <path> <id|code|name>` / `detail <path> <id|code|name>`

Shows all fields of a single error definition.

```
when-it-fails-setter details . AFW_NET_0001
when-it-fails-setter details . 600001
when-it-fails-setter details . NETWORKUNAVAILABLE
when-it-fails-setter details . AFW_NET_0001 --plain
```

Lookup works by id, numeric code, or name (case-insensitive). `detail` is an alias for `details`.

**Exit codes:**
- `0` — Detail displayed.
- `1` — Missing arguments or error definition not found.
- `2` — Validation errors found.

### `set-title <path> <id|code|name> <new title>`

Changes the title of an error definition with safe write semantics.

```
when-it-fails-setter set-title . AFW_NET_0001 "Network unavailable"
```

The title argument can contain spaces — everything after the lookup value is treated as the new title.

**Exit codes:**
- `0` — Title updated successfully.
- `1` — Missing arguments.
- `2` — Edit failed (load error, validation error, or save error).

### `demo`

Shows a sample validation result with example errors, warnings, and information messages.

```
when-it-fails-setter demo
```

Useful for testing console output formatting.

> Localized versions of this documentation may be generated later by Afrowave translation tooling.
