# Getting started

This guide shows the shortest reliable path to running WhenItFails Setter against an existing repository workspace.

## Prerequisites

- .NET SDK compatible with the repository target framework
- a local checkout of Afrowave.Toolbox
- a WhenItFails workspace under `Jsons/WhenItFails`

## Run Setter

From the repository root:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- help
```

On Bash:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- help
```

## Validate the workspace first

Before reading or editing catalog data, validate it:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

A successful validation returns exit code `0`.

## Inspect the workspace

Show a summary:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- summary .
```

List errors:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- errors .
```

Inspect one error:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- details . AFW_NET_0001
```

## Make one focused change

Setter commands are intentionally narrow. For example:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- set-title . AFW_NET_0001 "Network is unavailable"
```

After any change:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
git diff --check
```

## Useful next pages

- [Overview](../Overview/en.md)
- [Commands](../Commands/en.md)
- [Command Quick Reference](../Command%20Quick%20Reference/en.md)
- [Workspace Paths and Initialization](../Workspace%20Paths%20and%20Initialization/en.md)
- [Validation](../Validation/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Troubleshooting](../Troubleshooting/en.md)
