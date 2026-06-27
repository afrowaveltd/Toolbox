# WhenItFails Setter — overview

WhenItFails Setter is a .NET 10 command-line authoring and inspection tool for project-local Afrowave.Toolbox.WhenItFails JSON workspaces.

It works with the standard workspace:

```text
Jsons/
└── WhenItFails/
    ├── errors.en.json
    ├── categories.en.json
    ├── code-groups.en.json
    ├── owners.en.json
    └── profiles.json
```

Setter helps developers inspect, validate, and safely edit catalogs without requiring them to manipulate every JSON document manually.

## Purpose

Setter is the primary command-line tool for authoring and maintaining WhenItFails workspaces.

It currently supports:

* workspace initialization,
* complete workspace validation,
* catalog summaries,
* filtered error browsing,
* single-error inspection,
* rich and plain output,
* safe updates of selected error fields,
* timestamped backups before successful writes.

Setter is designed for project authors, maintainers, CI pipelines, support tooling, and future catalog-management workflows.

## Current capabilities

### Initialize

Setter can create missing workspace directories and catalog files from bundled templates.

```bash
when-it-fails-setter init ./MyProject
```

Existing project files are preserved.

Initialization is additive, not destructive.

### Validate

Setter can validate the complete workspace.

```bash
when-it-fails-setter validate .
```

Validation includes:

* JSON loading,
* document structure,
* required fields,
* duplicate identities,
* owner references,
* code-group references,
* numeric ranges,
* categories,
* profiles,
* cross-catalog relationships.

### Summarize

Setter can display a read-only workspace overview.

```bash
when-it-fails-setter summary .
```

Alias:

```bash
when-it-fails-setter inspect .
```

The summary helps confirm:

* how many definitions exist,
* which owners are present,
* which code groups are allocated,
* which profiles are available,
* which categories are most used.

### Browse errors

Setter can list error definitions.

```bash
when-it-fails-setter errors .
```

Supported filters include:

```text
--owner
--group
--code-group
--category
--severity
--profile
--search
```

Example:

```bash
when-it-fails-setter errors . \
  --category NETWORK \
  --severity Error
```

### Inspect one error

Setter can display one complete error definition.

```bash
when-it-fails-setter details . AFW_NET_0001
```

An error may be selected by:

* stable ID,
* numeric code,
* symbolic name.

Alias:

```bash
when-it-fails-setter detail . AFW_NET_0001
```

### Edit selected fields

Setter currently supports safe updates of:

```text
Title
Message
DeveloperHint
DefaultSeverity
DocumentationKey
```

Commands:

```text
set-title
set-message
set-developer-hint
set-severity
set-documentation-key
```

Example:

```bash
when-it-fails-setter set-message . \
  AFW_NET_0001 \
  "The application could not reach the remote service."
```

Stable identity fields are not currently edited through simple setter commands.

## Current command set

| Command                 | Purpose                         |
| ----------------------- | ------------------------------- |
| `help`                  | Show command help.              |
| `init`                  | Create missing workspace files. |
| `validate`              | Validate the workspace.         |
| `summary`               | Show workspace overview.        |
| `inspect`               | Alias for `summary`.            |
| `errors`                | List and filter errors.         |
| `details`               | Show one error definition.      |
| `detail`                | Alias for `details`.            |
| `set-title`             | Change an error title.          |
| `set-message`           | Change an error message.        |
| `set-developer-hint`    | Change a developer hint.        |
| `set-severity`          | Change default severity.        |
| `set-documentation-key` | Change documentation key.       |
| `demo`                  | Show sample validation output.  |

For exact syntax and exit codes, see:

```text
Commands
```

## Design principles

### Small commands

Each command has a narrow responsibility.

Examples:

```text
validate
→ validate only

details
→ inspect one definition only

set-title
→ edit one field only
```

This keeps behavior understandable, testable, and reviewable.

### Thin dispatcher

`Program.cs` routes command names to command classes.

It does not contain workspace logic, JSON handling, validation rules, or presentation details.

Conceptually:

```text
Program.cs
→ command class
→ workspace service
→ core WhenItFails components
→ view
```

### Safe by default

Write commands follow a conservative workflow:

```text
load
→ normalize
→ locate
→ edit in memory
→ validate
→ write temporary file
→ create backup
→ replace target
```

Setter never silently overwrites existing workspace files during initialization.

### Read before write

Setter provides several read-only tools:

```text
validate
summary
inspect
errors
details
detail
```

These make it possible to inspect the workspace before making any change.

Recommended workflow:

```text
validate
→ inspect
→ edit
→ validate
→ review Git diff
```

### Project ownership

Project-local catalogs belong to the project.

Setter does not treat bundled templates as permanently authoritative over existing project files.

It does not silently:

* reset catalogs,
* merge template updates,
* renumber errors,
* rewrite identities,
* repair invalid data,
* replace project choices.

### Rich and plain presentation

Interactive output uses Spectre.Console.

Selected commands also support:

```text
--plain
```

Plain output removes rich terminal formatting for lightweight scripting and redirection.

It is not currently a formal serialization format.

### Structured failures

Setter uses the shared Afrowave response and validation infrastructure.

Expected problems are represented through:

* validation issues,
* stable issue codes,
* messages,
* command exit codes.

Unexpected top-level exceptions are rendered separately and return a distinct exit code.

## Architecture

Setter is intentionally split into several layers.

```text
command dispatch
→ command parsing
→ workspace services
→ WhenItFails core loading and validation
→ console views
```

## Command layer

Location:

```text
Commands/
```

Current command classes include:

```text
InitCommand
ValidateCommand
SummaryCommand
ErrorsCommand
DetailsCommand
SetTitleCommand
SetMessageCommand
SetDeveloperHintCommand
SetSeverityCommand
SetDocumentationKeyCommand
DemoCommand
```

Each class handles:

* command-specific argument checking,
* calling the relevant workspace service,
* selecting the correct view,
* returning a process exit code.

## Workspace services

Core Setter services live at the project root.

Important classes include:

```text
WhenItFailsWorkspaceInitializer
WhenItFailsWorkspaceValidator
WhenItFailsWorkspaceSummarizer
WhenItFailsWorkspaceEditor
WhenItFailsWorkspacePathResolver
```

Supporting result models include:

```text
WhenItFailsWorkspaceSummary
WhenItFailsWorkspaceValidationOutcome
```

These services keep command classes small.

## Models

Location:

```text
Models/
```

Models represent command options and view inputs.

For example:

```text
ErrorListOptions
```

stores filters and output choices for the `errors` command.

## Views

Location:

```text
Views/
```

Views handle terminal presentation.

Current responsibilities include:

* help rendering,
* bootstrap results,
* workspace summaries,
* error lists,
* error details,
* rich tables,
* plain output,
* shared table sizing and escaping.

Important view classes include:

```text
HelpView
BootstrapResultView
SummaryView
ErrorsView
DetailsView
ConsoleTableViewHelper
```

Validation-result rendering is shared through the SeeMe console project.

## Core-library reuse

Setter does not duplicate the central WhenItFails catalog model.

It reuses core components for:

* definitions,
* configuration,
* JSON loading,
* normalization,
* validation,
* safe JSON writing,
* response types.

This keeps Setter behavior aligned with the runtime package.

## Project structure

```text
Toolroom/WhenItFails/Setter/
├── Commands/
│   ├── DemoCommand.cs
│   ├── DetailsCommand.cs
│   ├── ErrorsCommand.cs
│   ├── InitCommand.cs
│   ├── SetDeveloperHintCommand.cs
│   ├── SetDocumentationKeyCommand.cs
│   ├── SetMessageCommand.cs
│   ├── SetSeverityCommand.cs
│   ├── SetTitleCommand.cs
│   ├── SummaryCommand.cs
│   └── ValidateCommand.cs
├── Models/
│   └── ErrorListOptions.cs
├── Views/
│   ├── BootstrapResultView.cs
│   ├── ConsoleTableViewHelper.cs
│   ├── DetailsView.cs
│   ├── ErrorsView.cs
│   ├── HelpView.cs
│   └── SummaryView.cs
├── Docs/
├── Readme/
├── WhenItFailsWorkspaceEditor.cs
├── WhenItFailsWorkspaceInitializer.cs
├── WhenItFailsWorkspacePathResolver.cs
├── WhenItFailsWorkspaceSummarizer.cs
├── WhenItFailsWorkspaceSummary.cs
├── WhenItFailsWorkspaceValidationOutcome.cs
├── WhenItFailsWorkspaceValidator.cs
├── Program.cs
├── Afrowave.Toolbox.Toolroom.WhenItFails.Setter.csproj
└── README.md
```

The exact file list may grow as new commands and services are added.

The architectural separation should remain:

```text
commands
models
views
workspace services
thin dispatcher
```

## Runtime target

Setter targets:

```text
.NET 10
```

Project configuration includes:

```text
nullable reference types enabled
implicit usings enabled
executable output
```

Setter is part of the .NET 10 generation of Afrowave.Toolbox.

## Dependencies

### Afrowave.Toolbox.WhenItFails

Provides:

* catalog definitions,
* JSON configuration,
* catalog loaders,
* normalization,
* validation,
* safe document writing.

### Afrowave.Toolbox.SeeMe.WhenItFails.Console

Provides shared console rendering for validation results.

This avoids creating Setter-specific validation presentation that diverges from other WhenItFails tools.

### Afrowave.Toolbox.Essentials

Provides the shared response and issue infrastructure used indirectly and through the WhenItFails package.

### Spectre.Console

Provides:

* tables,
* panels,
* rules,
* colors,
* markup,
* exception rendering,
* terminal-aware layout.

Setter currently references:

```text
Spectre.Console 0.57.1
```

Package versions may change over time; the project file remains the authoritative source.

## Read-only and write operations

### Read-only commands

```text
help
validate
summary
inspect
errors
details
detail
demo
```

These do not intentionally modify project catalogs.

### Write operations

```text
init
set-title
set-message
set-developer-hint
set-severity
set-documentation-key
```

`init` creates only missing files.

Field setters update `errors.en.json` using safe-write behavior.

## Current editing boundary

Setter currently edits presentation and diagnostic fields.

It does not yet provide guided commands for:

* creating a new error,
* deleting an error,
* changing stable IDs,
* allocating numeric codes,
* changing owners,
* changing code groups,
* editing categories,
* editing tags,
* editing profiles,
* importing profile packages,
* exporting profile packages,
* migrating catalog schemas.

These are reasonable future directions, but they require explicit design and stronger authoring rules.

## Why identity editing is restricted

Fields such as:

```text
Id
Code
Name
Owner
CodePrefix
CodeGroup
```

act as stable contracts.

Changing them may affect:

* application code,
* logs,
* monitoring,
* support references,
* integrations,
* profiles,
* historical records,
* documentation.

A simple `set-code` command would be easy to implement and easy to regret.

Setter therefore starts with lower-risk fields and keeps identity operations for richer future workflows.

## Validation relationship

Setter validation and runtime initialization serve different purposes.

Setter:

```text
authoring and maintenance
→ show all relevant problems
→ help repair workspace before deployment
```

Runtime:

```text
application startup
→ activate a valid context
→ follow strict or flexible recovery policy
```

Setter does not activate the runtime context.

It prepares and checks the files that the runtime may later load.

## Output philosophy

Rich output is optimized for humans.

Plain output is optimized for lightweight text workflows.

Structured application integrations should use:

* core WhenItFails APIs,
* JSON catalogs,
* future explicit export formats.

Console text should not become an accidental long-term API contract without deliberate versioning.

## Testing

Setter has its own test project:

```text
Toolroom/WhenItFails/Setter.Tests
```

Tests cover areas such as:

* workspace initialization,
* validation,
* path resolution,
* error filtering,
* lookup,
* editing,
* safe writing,
* command-related behavior.

The complete solution test run should remain green after Setter changes.

Recommended checks:

```bash
dotnet build
dotnet test
```

## Recommended user workflow

```text
initialize once
→ validate
→ inspect summary
→ browse errors
→ inspect target
→ perform focused edit
→ validate again
→ review Git diff
→ test consuming application
→ commit
```

## Documentation map

* **Getting started** — first complete workflow.
* **Overview** — purpose and architecture.
* **Commands** — syntax, options, and exit codes.
* **Editing error fields** — how and why to edit supported fields.
* **Setting Title** — concrete `set-title` example.
* **Plain Output** — scripting-oriented output behavior.
* **Safe Writes** — file safety, backups, and replacement workflow.

## Future direction

Possible future Setter capabilities include:

* guided creation of new errors,
* safe code allocation,
* owner and code-group editing,
* category management,
* profile creation and packaging,
* import and export,
* workspace migration,
* template comparison,
* interactive mode,
* richer automation output,
* dedicated JSON or TSV export,
* backup cleanup and restore commands.

Future features should preserve the existing principles:

```text
explicit action
safe default
visible validation
project ownership
reviewable change
```

## Central principle

> Setter is a careful authoring assistant for project-owned catalogs, not an automatic repair engine and not a hidden database.
