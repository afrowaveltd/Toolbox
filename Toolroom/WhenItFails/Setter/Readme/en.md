# WhenItFails Setter

A small CLI tool for managing Afrowave.Toolbox.WhenItFails JSON workspaces.

## Quick Start

```bash
# Show help
dotnet run --project Toolroom/WhenItFails/Setter -- help

# Initialize a workspace
dotnet run --project Toolroom/WhenItFails/Setter -- init ./MyProject

# Validate workspace files
dotnet run --project Toolroom/WhenItFails/Setter -- validate .

# Show workspace summary
dotnet run --project Toolroom/WhenItFails/Setter -- summary .

# List errors (rich table)
dotnet run --project Toolroom/WhenItFails/Setter -- errors . --category NETWORK

# List errors (plain text for scripting)
dotnet run --project Toolroom/WhenItFails/Setter -- errors . --plain --category NETWORK

# Show one error in detail
dotnet run --project Toolroom/WhenItFails/Setter -- details . AFW_NET_0001

# Change an error title (safe write with backup)
dotnet run --project Toolroom/WhenItFails/Setter -- set-title . AFW_NET_0001 "Network unavailable"
```

## Commands

| Command | Description |
|---|---|
| `help` | Show help screen. |
| `init <path>` | Create missing WhenItFails JSON files. |
| `validate <path>` | Validate WhenItFails JSON files. |
| `summary <path>` / `inspect <path>` | Show read-only workspace summary. |
| `errors <path> [filters]` | List error definitions with optional filters (`--owner`, `--group`, `--category`, `--severity`, `--profile`, `--search`, `--plain`). |
| `details <path> <id\|code\|name>` / `detail` | Show one error definition in detail. Supports `--plain`. |
| `set-title <path> <id\|code\|name> <title>` | Change error title with safe write and backup. |
| `demo` | Show a sample validation result. |

## Documentation

- [Overview](Docs/Overview/en.md)
- [Commands](Docs/Commands/en.md)
- [Setting Title](Docs/Setting%20Title/en.md)
- [Plain Output](Docs/Plain%20Output/en.md)
- [Safe Writes](Docs/Safe%20Writes/en.md)

## Design

- **Small and focused** — Each command is a separate class. `Program.cs` is a thin dispatcher.
- **Safe by default** — Write operations validate before saving and create timestamped backups.
- **Dual output** — Rich Spectre.Console tables for interactive use; plain TSV for scripting.
- **Afrowave conventions** — Follows Afrowave.Toolbox naming, namespace, and documentation standards.

## Project Structure

```
Toolroom/WhenItFails/Setter/
├── Commands/          # One class per CLI command
├── Models/            # Option and result models
├── Views/             # Console rendering (rich + plain)
├── Docs/              # Topic-based documentation
├── Readme/            # Readme source (mirrors README.md)
├── Program.cs         # Thin command dispatcher (~70 lines)
└── README.md
```

Service classes (`WhenItFailsWorkspaceValidator`, `WhenItFailsWorkspaceSummarizer`, `WhenItFailsWorkspaceEditor`, etc.) live at the project root.

> Localized versions of this documentation may be generated later by Afrowave translation tooling.
