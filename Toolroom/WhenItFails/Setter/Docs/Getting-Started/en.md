# Getting started

This guide shows the shortest safe path from an empty or existing repository workspace to a reviewed WhenItFails catalog change.

## Prerequisites

- .NET SDK compatible with the repository target framework,
- a local checkout of Afrowave.Toolbox,
- Git available for reviewing changes.

Run all examples from the repository root.

## 1. Show help

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- help
```

Setter also accepts `--help`, `-h`, or no command.

## 2. Create or locate the workspace

For a new project root:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- init .
```

`init` creates missing files under:

```text
Jsons/WhenItFails
```

It preserves existing catalog files.

Most other commands accept either the project root or the `Jsons/WhenItFails` directory itself.

## 3. Validate before editing

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

A valid workspace returns exit code `0`. Do not begin authoring against a workspace that already fails validation.

## 4. Inspect the available reference data

Show the workspace summary:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- summary .
```

Browse the reference catalogs before choosing identifiers:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- reference .
dotnet run --project Toolroom/WhenItFails/Setter -- list-owners .
dotnet run --project Toolroom/WhenItFails/Setter -- list-code-groups .
dotnet run --project Toolroom/WhenItFails/Setter -- list-categories .
dotnet run --project Toolroom/WhenItFails/Setter -- list-profiles .
```

Use the matching `show-*` command when you need details about one owner, code group, category, or profile.

## 5. Inspect existing errors

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- errors .
dotnet run --project Toolroom/WhenItFails/Setter -- details . AFW_NET_0001
```

Errors can be selected by stable ID, numeric code, or symbolic name.

## 6. Prepare a new error safely

Ask Setter for the next available code in the intended code group:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- next-code . NETWORK
```

Ask for the first available canonical documentation key:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- suggest-doc-key . NETWORK "Connection interrupted"
```

These commands are read-only suggestions. Review their output before using it.

## 7. Add or update one error

Add a new definition with explicit identifiers and catalog references:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- add-error . `
  600002 `
  AFW_NET_0002 `
  CONNECTIONINTERRUPTED `
  "Connection interrupted" `
  "The connection was interrupted." `
  Error `
  AFW `
  NETWORK `
  NETWORK `
  when-it-fails/errors/network/connection-interrupted
```

For an existing error, use one focused command such as:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- set-message . `
  AFW_NET_0001 `
  "The application could not reach the remote service."
```

Write commands validate their inputs, create a timestamped backup, write through a temporary file, and atomically replace the target catalog file.

## 8. Inspect the result

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- details . AFW_NET_0002
dotnet run --project Toolroom/WhenItFails/Setter -- error-references . AFW_NET_0002
```

For profile behavior:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- explain-profile . WEB AFW_NET_0002
```

## 9. Validate documentation metadata

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
```

Documentation-key checks validate presence, uniqueness, and canonical format. Link checks validate local Markdown links in the selected documentation tree.

## 10. Run tests and review the diff

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
git diff --check
git diff
```

A reliable authoring loop is:

```text
validate
→ inspect reference data
→ make one focused change
→ inspect the changed object
→ check documentation metadata
→ validate again
→ run tests
→ review the Git diff
→ commit
```

## Output for automation

Use exit codes for success and failure decisions. Use `--json` when a command exposes machine-readable output, and `--plain` for simple text processing. Do not parse rich terminal output as a stable API.

## Recovery

List available backups:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- list-backups .
```

Restore only after inspecting the target and backup:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- restore-backup . <backup-file>
```

Setter backs up the active catalog before restoring and validates the restored workspace.

## Useful next pages

- [Overview](../Overview/en.md)
- [Commands](../Commands/en.md)
- [Command Quick Reference](../Command%20Quick%20Reference/en.md)
- [Workspace Paths and Initialization](../Workspace%20Paths%20and%20Initialization/en.md)
- [Validation](../Validation/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Troubleshooting](../Troubleshooting/en.md)

## Central principle

> Validate first, make one explicit change, inspect it, test it, and commit it.
