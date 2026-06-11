# WhenItFails Setter — Overview

The **WhenItFails Setter** is a small CLI tool for the Afrowave.Toolbox.WhenItFails error catalog system. It operates on a project-local JSON workspace (`Jsons/WhenItFails/`) and provides commands to initialize, validate, inspect, list, and edit error definitions.

## Purpose

The Setter is the primary command-line interface for working with WhenItFails JSON workspaces. It is designed for developers who need to:

- Bootstrap a new WhenItFails workspace in a project.
- Validate existing workspace files for correctness.
- Browse error definitions with rich or plain text output.
- Edit error metadata (e.g., titles) with safe write semantics.

## Design Principles

- **Small and focused**: Each command does one thing well.
- **Safe by default**: Write operations create backups and validate before saving.
- **Dual output modes**: Rich Spectre.Console tables for interactive use; plain tab-separated text for scripting and piping.
- **Afrowave conventions**: Follows Afrowave.Toolbox naming, namespace, and documentation standards.

## Project Structure

```
Toolroom/WhenItFails/Setter/
├── Commands/          # One class per CLI command
│   ├── InitCommand.cs
│   ├── ValidateCommand.cs
│   ├── SummaryCommand.cs
│   ├── ErrorsCommand.cs
│   ├── DetailsCommand.cs
│   ├── SetTitleCommand.cs
│   └── DemoCommand.cs
├── Models/            # Option and result models
│   └── ErrorListOptions.cs
├── Views/             # Console rendering (Spectre.Console and plain text)
│   ├── HelpView.cs
│   ├── BootstrapResultView.cs
│   ├── SummaryView.cs
│   ├── ErrorsView.cs
│   ├── DetailsView.cs
│   └── ConsoleTableViewHelper.cs
├── Services/          # (root-level service classes)
│   ├── WhenItFailsWorkspaceValidator.cs
│   ├── WhenItFailsWorkspaceSummarizer.cs
│   ├── WhenItFailsWorkspaceEditor.cs
│   ├── WhenItFailsWorkspaceInitializer.cs
│   ├── WhenItFailsWorkspacePathResolver.cs
│   ├── WhenItFailsWorkspaceSummary.cs
│   └── WhenItFailsWorkspaceValidationOutcome.cs
├── Docs/              # Topic-based documentation
├── Readme/            # Readme source
├── Program.cs         # Thin command dispatcher
└── README.md
```

## Dependencies

- **Spectre.Console** — Rich console rendering (tables, colors, panels).
- **Afrowave.Toolbox.WhenItFails** — Core WhenItFails library (definitions, loading, validation, normalization).
- **Afrowave.Toolbox.SeeMe.WhenItFails.Console** — Shared console validation result display.
- **Afrowave.Toolbox.Essentials** — Result types and issue infrastructure.

> Localized versions of this documentation may be generated later by Afrowave translation tooling.
