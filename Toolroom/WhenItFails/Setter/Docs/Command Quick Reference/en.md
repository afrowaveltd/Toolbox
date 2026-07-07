# Command quick reference

This page is a compact command reference for WhenItFails Setter.

Use it when you already know the basic workflow and only need the command shape, common examples, and expected exit-code behavior.

## Command groups

Setter commands can be grouped as:

```text
help and demo
→ show information
```

```text
workspace commands
→ initialize, validate, summarize
```

```text
browsing commands
→ list and inspect errors
```

```text
editing commands
→ update selected fields in errors.en.json
```

## General syntax

When installed as a tool or executable:

```bash
when-it-fails-setter <command> [arguments] [options]
```

From the Toolbox repository:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- <command> [arguments] [options]
```

PowerShell equivalent:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- <command> [arguments] [options]
```

## Workspace path

Most commands accept either:

```text
project root
```

or:

```text
Jsons/WhenItFails
```

Examples:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate ./Jsons/WhenItFails
```

Important exception:

```text
init
```

expects a project root.

## Exit-code summary

```text
0
→ success
```

```text
1
→ missing or invalid command input
```

```text
2
→ workspace validation, lookup, editing, save, or operation failure
```

```text
3
→ unexpected top-level application failure
```

## Help

Show help:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- help
```

Aliases:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- --help
```

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- -h
```

No arguments also show help.

Expected exit code:

```text
0
```

## Demo

Show sample validation output:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- demo
```

Expected exit code:

```text
0
```

The demo command is not a workspace validation substitute.

## Initialize workspace

Create or ensure a WhenItFails JSON workspace under a project root:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- init .
```

Disposable test workspace:

```bash
test_root="$(mktemp -d /tmp/when-it-fails-init-XXXXXXXXXX)"

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- init "$test_root"
```

Result:

```text
<project-root>/Jsons/WhenItFails
```

Expected exit codes:

```text
0
→ initialization completed
```

```text
1
→ project-root argument missing
```

```text
2
→ initialization failed
```

## Validate workspace

Validate all WhenItFails catalog files:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

Validate package directory directly:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate ./Jsons/WhenItFails
```

Expected exit codes:

```text
0
→ workspace is valid
```

```text
1
→ path missing
```

```text
2
→ validation failed
```

## Summary

Show a read-only workspace overview:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .
```

Alias:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- inspect .
```

Shows:

```text
catalog counts
owners
code groups
profiles
primary-category distribution
```

Expected exit codes:

```text
0
→ summary displayed
```

```text
1
→ path missing
```

```text
2
→ workspace validation failed
```

## List errors

List all error definitions:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors .
```

Plain output:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --plain
```

Expected exit codes:

```text
0
→ listing completed
```

```text
1
→ path missing or selected profile unknown
```

```text
2
→ workspace validation failed
```

A valid filter with zero results still returns:

```text
0
```

## Error filters

Available filters:

```text
--owner <value>
--group <value>
--code-group <value>
--category <value>
--severity <value>
--profile <value>
--search <text>
--plain
```

`--group` and `--code-group` are aliases.

## Filter by owner

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --owner AFW
```

Matching is exact and case-insensitive.

## Filter by code group

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --group NETWORK
```

Equivalent:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --code-group NETWORK
```

## Filter by category

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --category NETWORK
```

The dedicated category filter checks the primary category.

Use `--search` for broader category, subcategory, and tag discovery.

## Filter by severity

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --severity Error
```

Case-insensitive:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --severity warning
```

Supported canonical values:

```text
Trace
Debug
Information
Warning
Error
Critical
```

## Filter by profile

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile WEB
```

Profiles may be selected by name or display name.

Quote display names containing spaces:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile "Web application"
```

Unknown profile:

```text
UnknownProfileFilter
```

Expected exit code:

```text
1
```

## Search

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --search timeout
```

Search is:

```text
case-insensitive
substring-based
not a regular expression
```

Search checks:

```text
Id
Name
Title
Message
DeveloperHint
DocumentationKey
Code
Owner
CodeGroup
PrimaryCategory
Categories
Subcategories
Tags
```

## Combine filters

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . \
  --group NETWORK \
  --severity Error \
  --search connection
```

Filters combine with logical AND.

## Inspect one error

By ID:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001
```

By numeric code:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . 600001
```

By symbolic name:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . NETWORKUNAVAILABLE
```

Alias:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- detail . AFW_NET_0001
```

Plain output:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001 --plain
```

Expected exit codes:

```text
0
→ error displayed
```

```text
1
→ arguments missing or error not found
```

```text
2
→ workspace validation failed
```

## Fields shown by details

```text
Code
Id
Name
Title
Message
Severity
Owner
Code prefix
Code group
Primary category
Categories
Subcategories
Tags
Developer hint
Documentation key
```

## Edit title

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-title . \
  AFW_NET_0001 \
  "Network is not available"
```

Expected exit codes:

```text
0
→ title updated
```

```text
1
→ required arguments missing
```

```text
2
→ loading, lookup, validation, backup, or save failed
```

Common issue codes:

```text
TitleIsEmpty
ErrorDefinitionNotFound
```

## Edit message

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-message . \
  AFW_NET_0001 \
  "The application could not reach the remote service."
```

Common issue codes:

```text
MessageIsEmpty
ErrorDefinitionNotFound
```

## Edit developer hint

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-developer-hint . \
  AFW_NET_0001 \
  "Check DNS, proxy, VPN, firewall, and service availability."
```

Common issue codes:

```text
DeveloperHintIsEmpty
ErrorDefinitionNotFound
```

## Edit severity

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-severity . \
  AFW_NET_0001 \
  Warning
```

Input is case-insensitive and stored canonically.

Example:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-severity . \
  AFW_NET_0001 \
  warning
```

Stored value:

```text
Warning
```

Common issue codes:

```text
UnsupportedSeverity
ErrorDefinitionNotFound
```

## Edit documentation key

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-documentation-key . \
  AFW_NET_0001 \
  "when-it-fails/errors/network/network-unavailable"
```

Common issue codes:

```text
DocumentationKeyIsEmpty
ErrorDefinitionNotFound
```

## Edit workflow

Recommended sequence:

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
  -- details . AFW_NET_0001

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

git diff -- Jsons/WhenItFails/errors.en.json
```

## Backup behavior after edits

A successful edit creates a timestamped backup of the existing error catalog.

Pattern:

```text
errors.en.<UTC timestamp>.bak.json
```

Example:

```text
errors.en.20260627-095820-480.bak.json
```

Backups are stored in:

```text
Jsons/WhenItFails
```

Find backups:

```bash
find Jsons/WhenItFails \
  -maxdepth 1 \
  -type f \
  -name 'errors.en.*.bak.json' \
  -printf '%f\n' \
  | sort
```

## Restore from backup manually

Preserve current file:

```bash
cp \
  Jsons/WhenItFails/errors.en.json \
  /tmp/errors.en.before-recovery.json
```

Restore reviewed backup:

```bash
cp \
  Jsons/WhenItFails/errors.en.20260627-095820-480.bak.json \
  Jsons/WhenItFails/errors.en.json
```

Validate:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

Review:

```bash
git diff -- Jsons/WhenItFails/errors.en.json
```

## Common Bash patterns

Capture exit code:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

exit_code=$?
```

Stop on failure:

```bash
if [ "$exit_code" -ne 0 ]
then
  exit "$exit_code"
fi
```

Enable strict mode:

```bash
set -euo pipefail
```

Use a temporary workspace:

```bash
test_root="$(mktemp -d /tmp/when-it-fails-test-XXXXXXXXXX)"

trap 'rm -rf "$test_root"' EXIT
```

## Common PowerShell patterns

Capture exit code:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

$exitCode = $LASTEXITCODE
```

Stop on failure:

```powershell
if ($exitCode -ne 0)
{
    exit $exitCode
}
```

Use a temporary workspace:

```powershell
$testRoot = Join-Path `
    $env:TEMP `
    ("when-it-fails-test-" + [Guid]::NewGuid().ToString("N"))
```

## Minimal Linux CI sequence

```bash
#!/usr/bin/env bash

set -euo pipefail

dotnet restore

dotnet build \
  --no-restore

dotnet test \
  --no-build

dotnet run \
  --no-build \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

git diff --check
```

## Minimal Windows PowerShell CI sequence

```powershell
dotnet restore

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

dotnet build --no-restore

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

dotnet test --no-build

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

dotnet run `
  --no-build `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

git diff --check

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}
```

## Disposable smoke test

Bash:

```bash
test_root="$(mktemp -d /tmp/when-it-fails-smoke-XXXXXXXXXX)"

trap 'rm -rf "$test_root"' EXIT

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- init "$test_root"

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate "$test_root"

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details "$test_root" AFW_NET_0001 --plain
```

PowerShell:

```powershell
$testRoot = Join-Path `
    $env:TEMP `
    ("when-it-fails-smoke-" + [Guid]::NewGuid().ToString("N"))

New-Item `
    -Path $testRoot `
    -ItemType Directory `
    -Force |
Out-Null

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- init $testRoot

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate $testRoot

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- details $testRoot AFW_NET_0001 --plain

Remove-Item `
  -Path $testRoot `
  -Recurse `
  -Force
```

## Quick troubleshooting

Unknown command:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- definitely-not-a-command
```

Expected exit code:

```text
1
```

Missing validation path:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate
```

Expected issue:

```text
MissingValidatePath
```

Expected exit code:

```text
1
```

Unknown error detail:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_UNKNOWN_9999
```

Expected issue:

```text
ErrorDefinitionNotFound
```

Expected exit code:

```text
1
```

Unsupported severity edit:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-severity . AFW_NET_0001 Banana
```

Expected issue:

```text
UnsupportedSeverity
```

Expected exit code:

```text
2
```

## Command index

```text
help
--help
-h
demo
init
validate
summary
inspect
errors
details
detail
set-title
set-message
set-developer-hint
set-severity
set-documentation-key
```

## Related documentation

- [Getting Started](../Getting-Started/en.md)
- [Commands](../Commands/en.md)
- [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
- [Windows and PowerShell](../Windows%20and%20PowerShell/en.md)
- [Linux and Bash](../Linux%20and%20Bash/en.md)
- [Validation](../Validation/en.md)
- [Browsing and Filtering Errors](../Browsing%20and%20Filtering%20Errors/en.md)
- [Inspecting Error Details](../Inspecting%20Error%20Details/en.md)
- [Editing Error Fields](../Editing%20Error%20Fields/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)

## Central principle

> Use this page as the compact map; use the dedicated guides when you need the details behind a command.
