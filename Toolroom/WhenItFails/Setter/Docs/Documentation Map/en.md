# Documentation map

This page is the navigation map for WhenItFails Setter documentation.

Use it to choose the right guide for the task you are doing.

## Start here

If you are new to Setter, read these first:

1. [Getting Started](../Getting-Started/en.md)
2. [Overview](../Overview/en.md)
3. [Commands](../Commands/en.md)
4. [Command Quick Reference](../Command%20Quick%20Reference/en.md)

Recommended first workflow:

```text
read overview
→ initialize or locate workspace
→ validate workspace
→ view summary
→ list errors
→ inspect one error
```

## Platform guides

Use the guide matching your shell.

### Windows

Read:

- [Windows and PowerShell](../Windows%20and%20PowerShell/en.md)

Use it for:

- PowerShell commands,
- `$LASTEXITCODE`,
- Windows paths,
- temporary directories,
- file locks,
- antivirus or endpoint security behavior,
- Windows line endings,
- Command Prompt notes.

### Linux

Read:

- [Linux and Bash](../Linux%20and%20Bash/en.md)

Use it for:

- Bash commands,
- `$?`,
- `set -euo pipefail`,
- `/tmp` workspaces,
- permissions,
- ownership,
- `jq`,
- `grep`,
- `find`,
- Linux CI scripts.

## Daily user path

For ordinary use, follow this path:

1. [Workspace Paths and Initialization](../Workspace%20Paths%20and%20Initialization/en.md)
2. [Validation](../Validation/en.md)
3. [Workspace Summary](../Workspace%20Summary/en.md)
4. [Browsing and Filtering Errors](../Browsing%20and%20Filtering%20Errors/en.md)
5. [Inspecting Error Details](../Inspecting%20Error%20Details/en.md)

This path answers:

```text
Where is the workspace?
Is it valid?
What does it contain?
Which errors exist?
What does one specific error mean?
```

## Editing path

For catalog editing, follow this path:

1. [Inspecting Error Details](../Inspecting%20Error%20Details/en.md)
2. [Authoring Error Text](../Authoring%20Error%20Text/en.md)
3. [Editing Error Fields](../Editing%20Error%20Fields/en.md)
4. [Setting Title](../Setting%20Title/en.md)
5. [Safe Writes](../Safe%20Writes/en.md)
6. [Backups and Recovery](../Backups%20and%20Recovery/en.md)
7. [Validation](../Validation/en.md)

Recommended editing workflow:

```text
inspect before
→ edit one field
→ inspect after
→ validate
→ review Git diff
→ commit
```

## Automation path

For scripts and CI, read:

1. [Command Quick Reference](../Command%20Quick%20Reference/en.md)
2. [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
3. [Plain Output](../Plain%20Output/en.md)
4. [Testing and CI](../Testing%20and%20CI/en.md)

For platform-specific scripting, also read:

- [Windows and PowerShell](../Windows%20and%20PowerShell/en.md)
- [Linux and Bash](../Linux%20and%20Bash/en.md)

Automation should rely primarily on:

```text
process exit code
```

and only secondarily on:

```text
human-readable output
```

## Recovery path

When something went wrong, read:

1. [Troubleshooting](../Troubleshooting/en.md)
2. [Backups and Recovery](../Backups%20and%20Recovery/en.md)
3. [Safe Writes](../Safe%20Writes/en.md)
4. [Validation](../Validation/en.md)

Recommended recovery workflow:

```text
stop writers
→ confirm workspace path
→ preserve current file
→ inspect backups
→ test candidate in temporary workspace
→ restore deliberately
→ validate
→ review diff
```

## Testing path

For maintainers and CI work, read:

1. [Testing and CI](../Testing%20and%20CI/en.md)
2. [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
3. [Windows and PowerShell](../Windows%20and%20PowerShell/en.md)
4. [Linux and Bash](../Linux%20and%20Bash/en.md)

Minimum local validation sequence:

```bash
dotnet build
dotnet test
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
git diff --check
```

PowerShell equivalent:

```powershell
dotnet build
dotnet test
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .
git diff --check
```

## Documentation by task

### I want to install or run Setter

Read:

- [Getting Started](../Getting-Started/en.md)
- [Commands](../Commands/en.md)
- [Command Quick Reference](../Command%20Quick%20Reference/en.md)

### I want to know what Setter is for

Read:

- [Overview](../Overview/en.md)

### I want to create a workspace

Read:

- [Workspace Paths and Initialization](../Workspace%20Paths%20and%20Initialization/en.md)

Then run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- init .
```

### I want to validate a workspace

Read:

- [Validation](../Validation/en.md)

Then run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

### I want to see what is inside a workspace

Read:

- [Workspace Summary](../Workspace%20Summary/en.md)

Then run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .
```

### I want to list errors

Read:

- [Browsing and Filtering Errors](../Browsing%20and%20Filtering%20Errors/en.md)

Then run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors .
```

### I want to find errors by text

Read:

- [Browsing and Filtering Errors](../Browsing%20and%20Filtering%20Errors/en.md)

Then run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --search network
```

### I want to inspect one error

Read:

- [Inspecting Error Details](../Inspecting%20Error%20Details/en.md)

Then run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001
```

### I want to change a title or message

Read:

- [Authoring Error Text](../Authoring%20Error%20Text/en.md)
- [Editing Error Fields](../Editing%20Error%20Fields/en.md)
- [Setting Title](../Setting%20Title/en.md)

Then run one focused command:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-title . \
  AFW_NET_0001 \
  "Network is not available"
```

### I want to change severity

Read:

- [Authoring Error Text](../Authoring%20Error%20Text/en.md)
- [Editing Error Fields](../Editing%20Error%20Fields/en.md)

Then run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-severity . \
  AFW_NET_0001 \
  Warning
```

### I want plain output for scripts

Read:

- [Plain Output](../Plain%20Output/en.md)
- [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)

Then run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001 --plain
```

### I want to recover a previous version

Read:

- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Safe Writes](../Safe%20Writes/en.md)

Start with:

```bash
find Jsons/WhenItFails \
  -maxdepth 1 \
  -type f \
  -name '*.bak.json' \
  -printf '%f\n' \
  | sort
```

### I want to understand why saving is safe

Read:

- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)

### I want to debug a failing command

Read:

- [Troubleshooting](../Troubleshooting/en.md)
- [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)

Then capture the exit code immediately.

Bash:

```bash
echo "$?"
```

PowerShell:

```powershell
$LASTEXITCODE
```

### I want to run in CI

Read:

- [Testing and CI](../Testing%20and%20CI/en.md)
- [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)

Use:

```bash
dotnet run \
  --no-build \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

## Documentation by command

| Command | Main documentation |
| --- | --- |
| `help` | [Commands](../Commands/en.md), [Command Quick Reference](../Command%20Quick%20Reference/en.md) |
| `demo` | [Commands](../Commands/en.md) |
| `init` | [Workspace Paths and Initialization](../Workspace%20Paths%20and%20Initialization/en.md) |
| `validate` | [Validation](../Validation/en.md) |
| `summary` | [Workspace Summary](../Workspace%20Summary/en.md) |
| `inspect` | [Workspace Summary](../Workspace%20Summary/en.md) |
| `errors` | [Browsing and Filtering Errors](../Browsing%20and%20Filtering%20Errors/en.md) |
| `details` | [Inspecting Error Details](../Inspecting%20Error%20Details/en.md) |
| `detail` | [Inspecting Error Details](../Inspecting%20Error%20Details/en.md) |
| `set-title` | [Setting Title](../Setting%20Title/en.md), [Editing Error Fields](../Editing%20Error%20Fields/en.md) |
| `set-message` | [Editing Error Fields](../Editing%20Error%20Fields/en.md), [Authoring Error Text](../Authoring%20Error%20Text/en.md) |
| `set-developer-hint` | [Editing Error Fields](../Editing%20Error%20Fields/en.md), [Authoring Error Text](../Authoring%20Error%20Text/en.md) |
| `set-severity` | [Editing Error Fields](../Editing%20Error%20Fields/en.md), [Authoring Error Text](../Authoring%20Error%20Text/en.md) |
| `set-documentation-key` | [Editing Error Fields](../Editing%20Error%20Fields/en.md), [Authoring Error Text](../Authoring%20Error%20Text/en.md) |

## Documentation by file or concept

| Concept | Documentation |
| --- | --- |
| Workspace layout | [Workspace Paths and Initialization](../Workspace%20Paths%20and%20Initialization/en.md) |
| Catalog validation | [Validation](../Validation/en.md) |
| Error listing | [Browsing and Filtering Errors](../Browsing%20and%20Filtering%20Errors/en.md) |
| Error detail | [Inspecting Error Details](../Inspecting%20Error%20Details/en.md) |
| Error wording | [Authoring Error Text](../Authoring%20Error%20Text/en.md) |
| Field editing | [Editing Error Fields](../Editing%20Error%20Fields/en.md) |
| Title editing | [Setting Title](../Setting%20Title/en.md) |
| Plain output | [Plain Output](../Plain%20Output/en.md) |
| Safe write flow | [Safe Writes](../Safe%20Writes/en.md) |
| Backups | [Backups and Recovery](../Backups%20and%20Recovery/en.md) |
| Exit codes | [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md) |
| Linux usage | [Linux and Bash](../Linux%20and%20Bash/en.md) |
| Windows usage | [Windows and PowerShell](../Windows%20and%20PowerShell/en.md) |
| CI | [Testing and CI](../Testing%20and%20CI/en.md) |
| Failures | [Troubleshooting](../Troubleshooting/en.md) |

## Recommended README order

A project README can list the documentation in this order:

```markdown
## Documentation

- [Getting started](Docs/Getting-Started/en.md)
- [Overview](Docs/Overview/en.md)
- [Documentation Map](Docs/Documentation%20Map/en.md)
- [Commands](Docs/Commands/en.md)
- [Command Quick Reference](Docs/Command%20Quick%20Reference/en.md)
- [Exit Codes and Automation](Docs/Exit%20Codes%20and%20Automation/en.md)
- [Windows and PowerShell](Docs/Windows%20and%20PowerShell/en.md)
- [Linux and Bash](Docs/Linux%20and%20Bash/en.md)
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
```

## Minimal “I only have one minute” sequence

Bash:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --search network

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001
```

PowerShell:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- summary .

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- errors . --search network

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- details . AFW_NET_0001
```

## Minimal safe edit sequence

Bash:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-title . \
  AFW_NET_0001 \
  "Network is not available"

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

git diff -- \
  Jsons/WhenItFails/errors.en.json
```

PowerShell:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- details . AFW_NET_0001

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- set-title . `
  AFW_NET_0001 `
  "Network is not available"

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

git diff -- `
  Jsons/WhenItFails/errors.en.json
```

## Suggested reading order for contributors

Contributors who modify Setter itself should read:

1. [Overview](../Overview/en.md)
2. [Commands](../Commands/en.md)
3. [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
4. [Testing and CI](../Testing%20and%20CI/en.md)
5. [Safe Writes](../Safe%20Writes/en.md)
6. [Backups and Recovery](../Backups%20and%20Recovery/en.md)
7. [Troubleshooting](../Troubleshooting/en.md)

Then platform-specific guides as needed.

## Suggested reading order for catalog authors

Catalog authors should read:

1. [Workspace Paths and Initialization](../Workspace%20Paths%20and%20Initialization/en.md)
2. [Validation](../Validation/en.md)
3. [Workspace Summary](../Workspace%20Summary/en.md)
4. [Browsing and Filtering Errors](../Browsing%20and%20Filtering%20Errors/en.md)
5. [Inspecting Error Details](../Inspecting%20Error%20Details/en.md)
6. [Authoring Error Text](../Authoring%20Error%20Text/en.md)
7. [Editing Error Fields](../Editing%20Error%20Fields/en.md)
8. [Backups and Recovery](../Backups%20and%20Recovery/en.md)

## Suggested reading order for CI maintainers

CI maintainers should read:

1. [Command Quick Reference](../Command%20Quick%20Reference/en.md)
2. [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
3. [Testing and CI](../Testing%20and%20CI/en.md)
4. [Linux and Bash](../Linux%20and%20Bash/en.md)
5. [Windows and PowerShell](../Windows%20and%20PowerShell/en.md)

## If something seems contradictory

When a focused guide and this documentation map differ, prefer the focused guide.

This page is only a navigation map.

Command behavior should be confirmed against:

- the dedicated documentation,
- current tests,
- current source code,
- actual command execution.

## Central principle

> Start with the smallest guide that answers the current question, then jump to the deeper guide only when needed.
