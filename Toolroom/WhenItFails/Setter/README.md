# WhenItFails Setter

A command-line tool for managing Afrowave.Toolbox.WhenItFails JSON workspaces.

Setter can:

* create missing workspace files,
* validate the complete workspace,
* inspect catalog summaries,
* list and filter errors,
* show one error in detail,
* safely update selected error fields,
* create timestamped backups before writes,
* produce rich interactive output or plain script-friendly output.

## Quick start

### Show help

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- help
```

Running the application without arguments also shows help:

```bash
dotnet run --project Toolroom/WhenItFails/Setter
```

### Initialize a workspace

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- init ./MyProject
```

The command creates only missing WhenItFails JSON files.

Existing files are preserved.

### Validate a workspace

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Validation checks the complete workspace, including cross-catalog relationships.

### Show a workspace summary

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- summary .
```

The alias `inspect` performs the same operation:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- inspect .
```

### List errors

Rich interactive table:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- errors .
```

Filtered by category:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- errors . --category NETWORK
```

Plain output for scripts:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- errors . --plain --category NETWORK
```

### Show one error

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- details . AFW_NET_0001
```

An error may be selected by:

```text
stable ID
numeric code
symbolic name
```

The singular alias is also available:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- detail . NETWORKUNAVAILABLE
```

### Change an error title

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  set-title . AFW_NET_0001 "Network is not available"
```

### Change an error message

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  set-message . AFW_NET_0001 "The network is currently unavailable."
```

### Change a developer hint

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  set-developer-hint . AFW_NET_0001 \
  "Check connectivity, DNS, proxy, VPN, and remote endpoint availability."
```

### Change default severity

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  set-severity . AFW_NET_0001 Warning
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

### Change a documentation key

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  set-documentation-key . AFW_NET_0001 \
  "when-it-fails/errors/network/network-unavailable"
```

## Commands

| Command                                               | Description                                   |
| ----------------------------------------------------- | --------------------------------------------- |
| `help`                                                | Show the help screen.                         |
| `init <path>`                                         | Create missing WhenItFails JSON files.        |
| `validate <path>`                                     | Validate the complete WhenItFails workspace.  |
| `summary <path>`                                      | Show a read-only workspace summary.           |
| `inspect <path>`                                      | Alias for `summary`.                          |
| `errors <path> [filters]`                             | List error definitions with optional filters. |
| `details <path> <id\|code\|name>`                     | Show one error definition in detail.          |
| `detail <path> <id\|code\|name>`                      | Alias for `details`.                          |
| `set-title <path> <id\|code\|name> <title>`           | Safely change an error title.                 |
| `set-message <path> <id\|code\|name> <message>`       | Safely change an error message.               |
| `set-developer-hint <path> <id\|code\|name> <hint>`   | Safely change a developer hint.               |
| `set-severity <path> <id\|code\|name> <severity>`     | Safely change default severity.               |
| `set-documentation-key <path> <id\|code\|name> <key>` | Safely change a documentation key.            |
| `demo`                                                | Show a sample validation result.              |

## Error-list filters

The `errors` command supports:

```text
--owner
--group
--category
--severity
--profile
--search
--plain
```

Examples:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  errors . --owner AFW
```

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  errors . --group NETWORK
```

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  errors . --severity Error
```

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  errors . --profile WEB
```

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  errors . --search timeout
```

Filters may be combined.

## Rich and plain output

Setter uses rich Spectre.Console output for interactive terminal use.

Commands that support `--plain` produce simpler output suitable for:

* shell scripts,
* redirection,
* text processing,
* CI logs,
* external tools.

Example:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- \
  errors . --plain > errors.tsv
```

## Safe writes

Write commands use a conservative workflow:

```text
locate target error
→ create updated in-memory document
→ validate
→ create timestamped backup
→ save updated file
```

Existing files are never rewritten without an explicit write command.

A failed validation must not replace the original catalog.

## Workspace location

The supplied path represents the project or workspace root.

Setter resolves the WhenItFails catalogs beneath it using the expected JSON workspace structure:

```text
Jsons/
└── WhenItFails/
    ├── errors.en.json
    ├── categories.en.json
    ├── code-groups.en.json
    ├── owners.en.json
    └── profiles.json
```

## Documentation

- [Getting started](Docs/Getting-Started/en.md)
- [Overview](Docs/Overview/en.md)
- [Commands](Docs/Commands/en.md)
- [Exit Codes and Automation](Docs/Exit%20Codes%20and%20Automation/en.md)
- [Windows and PowerShell](Docs/Windows%20and%20PowerShell/en.md)
- [Workspace Paths and Initialization](Docs/Workspace%20Paths%20and%20Initialization/en.md)
- [Validation](Docs/Validation/en.md)
- [Workspace Summary](Docs/Workspace%20Summary/en.md)
- [Browsing and Filtering Errors](Docs/Browsing%20and%20Filtering%20Errors/en.md)
- [Inspecting Error Details](Docs/Inspecting%20Error%20Details/en.md)
- [Editing error fields](Docs/Editing%20Error%20Fields/en.md)
- [Authoring Error Text](Docs/Authoring%20Error%20Text/en.md)
- [Setting Title](Docs/Setting%20Title/en.md)
- [Plain Output](Docs/Plain%20Output/en.md)
- [Safe Writes](Docs/Safe%20Writes/en.md)
- [Backups and Recovery](Docs/Backups%20and%20Recovery/en.md)
- [Testing and CI](Docs/Testing%20and%20CI/en.md)
- [Troubleshooting](Docs/Troubleshooting/en.md)

## Design

* **Small and focused** — each command has its own class.
* **Thin dispatcher** — `Program.cs` only routes commands.
* **Safe by default** — writes validate first and create timestamped backups.
* **Dual output** — rich terminal views and plain script-friendly output.
* **Project ownership** — Setter edits project catalogs only through explicit commands.
* **Afrowave conventions** — naming, namespaces, documentation, and structured responses follow Toolbox conventions.

## Project structure

```text
Toolroom/WhenItFails/Setter/
├── Commands/          # One class per CLI command
├── Models/            # Option and result models
├── Views/             # Rich and plain console rendering
├── Docs/              # Topic-based documentation
├── Readme/            # Readme source
├── Program.cs         # Thin command dispatcher
└── README.md
```

Service classes such as:

```text
WhenItFailsWorkspaceInitializer
WhenItFailsWorkspaceValidator
WhenItFailsWorkspaceSummarizer
WhenItFailsWorkspaceEditor
```

live at the project root.

## Current scope

Setter currently edits selected presentation and diagnostic fields:

```text
Title
Message
DeveloperHint
DefaultSeverity
DocumentationKey
```

Broader catalog authoring, profile import/export, migrations, and guided identity allocation may be added later as explicit tools.

> Localized versions of this documentation may later be generated by Afrowave translation tooling.
