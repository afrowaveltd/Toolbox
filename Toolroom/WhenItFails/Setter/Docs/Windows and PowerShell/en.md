# Windows and PowerShell

WhenItFails Setter runs on .NET 10 and can be used from:

* PowerShell,
* Windows Terminal,
* Command Prompt,
* Visual Studio Code terminal,
* CI runners on Windows.

This guide uses PowerShell unless stated otherwise.

## Requirements

Check the installed .NET SDK:

```rust

```

```rust

```

```powershell
dotnet --version
dotnet --list-sdks
```

Setter targets:

Build the project:

```powershell
dotnet build
```

Run the Setter help:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- help
```

PowerShell uses the backtick character:

for line continuation.

The backtick must be the final character on the line.

Trailing spaces after it may break continuation.

## One-line commands

When copying commands manually, one-line form is often safer:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- help
```

Example validation:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

## Current directory

Show the current directory:

```powershell
Get-Location
```

Resolve the current path:

```powershell
Resolve-Path .
```

Setter resolves relative workspace paths from the current working directory.

Before using:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

confirm that `.` is the intended project root.

## Accepted workspace paths

Most Setter commands accept either:

or:

Examples:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .
```

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .\\Jsons\\WhenItFails
```

PowerShell also accepts forward slashes in most .NET paths:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate ./Jsons/WhenItFails
```

Use one path style consistently within scripts.

## Initialization path

The `init` command expects a project root.

Correct:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- init C:\\Temp\\WhenItFailsTest
```

Result:

Do not pass the package directory itself:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- init C:\\Temp\\WhenItFailsTest\\Jsons\\WhenItFails
```

That would treat the supplied path as a project root and target:

## Paths containing spaces

Quote paths containing spaces:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate "C:\\Work Projects\\My Application"
```

The same applies to text values:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- set-title . AFW\_NET\_0001 "Network is not available"
```

## PowerShell exit codes

PowerShell stores the exit code of the last native executable in:

```powershell
$LASTEXITCODE
```

Example:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

$setterExitCode = $LASTEXITCODE

Write-Host "Setter exit code: $setterExitCode"
```

Do not use:

```powershell
$?
```

as a direct substitute for a native process exit code.

`$?` indicates whether the previous operation succeeded from PowerShell’s perspective, while `$LASTEXITCODE` preserves the numeric exit code returned by the executable.

For Setter automation, prefer:

```powershell
$LASTEXITCODE
```

## Exit-code check

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

$setterExitCode = $LASTEXITCODE

if ($setterExitCode -ne 0)
{
    Write-Error "Setter failed with exit code $setterExitCode."
    exit $setterExitCode
}
```

## Expected exit-code model

## Stop script on Setter failure

PowerShell does not automatically stop a script merely because a native executable returned a non-zero exit code.

Use an explicit check:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}
```

A reusable helper:

```powershell
function Invoke-Setter
{
    param
    (
        \[Parameter(Mandatory = $true)]
        \[string\[]] $Arguments
    )

    \& dotnet run `
        --project Toolroom/WhenItFails/Setter `
        -- @Arguments

    $setterExitCode = $LASTEXITCODE

    if ($setterExitCode -ne 0)
    {
        throw "Setter failed with exit code $setterExitCode."
    }
}
```

Example:

```powershell
Invoke-Setter -Arguments @(
    "validate",
    "."
)
```

## Native argument passing

The marker:

```powershell
--
```

separates `dotnet run` arguments from application arguments.

Example:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- details . AFW\_NET\_0001
```

Everything after `--` is passed to Setter.

## Listing errors

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- errors .
```

Filtered:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- errors . --group NETWORK --severity Error
```

Plain output:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- errors . --plain
```

## Inspecting one error

By stable ID:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- details . AFW\_NET\_0001
```

By numeric code:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- details . 600001
```

By symbolic name:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- detail . NETWORKUNAVAILABLE
```

Plain output:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- details . AFW\_NET\_0001 --plain
```

## Editing fields

Set title:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- set-title . AFW\_NET\_0001 "Network is not available"
```

Set message:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- set-message . AFW\_NET\_0001 "The application could not reach the remote service."
```

Set developer hint:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- set-developer-hint . AFW\_NET\_0001 "Check DNS, proxy, VPN, firewall, and service availability."
```

Set severity:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- set-severity . AFW\_NET\_0001 Warning
```

Set documentation key:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- set-documentation-key . AFW\_NET\_0001 "when-it-fails/errors/network/network-unavailable"
```

## PowerShell quoting

Double quotes expand variables:

```powershell
$serviceName = "api.example.test"

$message = "The service $serviceName could not be reached."
```

Single quotes preserve literal text:

```powershell
$message = 'The service $serviceName could not be reached.'
```

For static Setter text, either may be used.

Use single quotes when the text contains `$` that must remain literal.

Example:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- set-message . AFW\_NET\_0001 'The value $endpoint could not be resolved.'
```

## Apostrophes inside text

Use double quotes around text containing an apostrophe:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- set-message . AFW\_NET\_0001 "The user's network configuration could not be loaded."
```

## Double quotes inside text

Escape a double quote with a backtick:

```powershell
$message = "The profile `"WEB`" could not be loaded."
```

Then:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- set-message . AFW\_NET\_0001 $message
```

Using a variable is often clearer than deeply nested quoting.

## Here-strings

For longer text, PowerShell supports here-strings.

Double-quoted here-string:

```powershell
$message = @"
The remote service could not be reached.
Check the selected environment and endpoint configuration.
"@
```

Single-quoted here-string:

```powershell
$message = @'
The remote service could not be reached.
The value $endpoint remains literal here.
'@
```

Setter fields are currently best kept concise.

A here-string may contain line breaks, which will be stored in JSON as part of the value.

Use multiline values only when intentionally supported by the consuming UI.

## Temporary directory

Create a disposable workspace under the user temp directory:

```powershell
$testRoot = Join-Path `
    $env:TEMP `
    "when-it-fails-test"

Remove-Item `
    -Path $testRoot `
    -Recurse `
    -Force `
    -ErrorAction SilentlyContinue

New-Item `
    -Path $testRoot `
    -ItemType Directory `
    -Force |
Out-Null
```

Initialize:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- init $testRoot
```

Validate:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate $testRoot
```

Clean up:

```powershell
Remove-Item `
  -Path $testRoot `
  -Recurse `
  -Force
```

## Unique temporary directory

For parallel or repeated tests:

```powershell
$testRoot = Join-Path `
    $env:TEMP `
    ("when-it-fails-test-" + \[Guid]::NewGuid().ToString("N"))
```

Create it:

```powershell
New-Item `
    -Path $testRoot `
    -ItemType Directory `
    -Force |
Out-Null
```

## Listing catalog files

```powershell
Get-ChildItem `
    -Path .\\Jsons\\WhenItFails `
    -File |
Sort-Object Name |
Select-Object Name
```

Expected catalog files include:

## Finding workspaces

From the repository root:

```powershell
Get-ChildItem `
    -Path . `
    -Directory `
    -Recurse `
    -Filter WhenItFails |
Where-Object {
    $\_.Parent.Name -ieq "Jsons"
} |
Select-Object FullName
```

## Finding backups

```powershell
Get-ChildItem `
    -Path .\\Jsons\\WhenItFails `
    -File `
    -Filter \*.bak.json |
Sort-Object Name |
Select-Object Name, Length, LastWriteTimeUtc
```

Only error-catalog backups:

```powershell
Get-ChildItem `
    -Path .\\Jsons\\WhenItFails `
    -File `
    -Filter errors.en.\*.bak.json |
Sort-Object Name |
Select-Object Name, Length, LastWriteTimeUtc
```

## Newest backup

```powershell
$newestBackup =
    Get-ChildItem `
        -Path .\\Jsons\\WhenItFails `
        -File `
        -Filter errors.en.\*.bak.json |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1
```

Display:

```powershell
$newestBackup.FullName
```

Do not restore it automatically without inspecting its content.

## Comparing files

PowerShell has `Compare-Object`, but it is line-oriented and less readable than a normal unified diff.

Simple comparison:

```powershell
$backupContent =
    Get-Content `
        -Path $newestBackup.FullName

$activeContent =
    Get-Content `
        -Path .\\Jsons\\WhenItFails\\errors.en.json

Compare-Object `
    -ReferenceObject $backupContent `
    -DifferenceObject $activeContent
```

When Git is available, a better comparison is:

```powershell
git diff --no-index -- `
  $newestBackup.FullName `
  .\\Jsons\\WhenItFails\\errors.en.json
```

Git returns exit code `1` when differences are found.

For `git diff --no-index`, that normally means the files differ, not that the command itself failed.

## Restoring a backup

Preserve the active file first:

```powershell
$preservedFile = Join-Path `
    $env:TEMP `
    "errors.en.before-recovery.json"

Copy-Item `
    -Path .\\Jsons\\WhenItFails\\errors.en.json `
    -Destination $preservedFile `
    -Force
```

Restore the reviewed backup:

```powershell
Copy-Item `
    -Path $newestBackup.FullName `
    -Destination .\\Jsons\\WhenItFails\\errors.en.json `
    -Force
```

Validate immediately:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .
```

## Inspecting JSON

PowerShell can parse JSON directly:

```powershell
$catalog =
    Get-Content `
        -Path .\\Jsons\\WhenItFails\\errors.en.json `
        -Raw |
    ConvertFrom-Json
```

Find one error:

```powershell
$errorDefinition =
    $catalog.errors |
    Where-Object {
        $\_.id -ieq "AFW\_NET\_0001"
    }
```

Display selected fields:

```powershell
$errorDefinition |
Select-Object `
    id,
    code,
    name,
    title,
    message,
    defaultSeverity,
    developerHint,
    documentationKey
```

## Strict JSON check

PowerShell JSON parsing:

```powershell
Get-Content `
    -Path .\\Jsons\\WhenItFails\\errors.en.json `
    -Raw |
ConvertFrom-Json |
Out-Null
```

This is useful for ordinary JSON syntax checking.

Setter may accept comments and trailing commas through its configured JSON loader, while `ConvertFrom-Json` behavior may vary by PowerShell version and syntax used.

Complete Setter validation remains authoritative for the workspace.

## Searching text

Equivalent of a simple recursive `grep`:

```powershell
Get-ChildItem `
    -Path .\\Jsons\\WhenItFails `
    -File |
Select-String `
    -Pattern "timeout"
```

Search exact severity text:

```powershell
Select-String `
    -Path .\\Jsons\\WhenItFails\\errors.en.json `
    -Pattern '"defaultSeverity": "Error"'
```

## Replacing test text safely

For a disposable negative test:

```powershell
$catalogPath =
    Join-Path `
        $testRoot `
        "Jsons\\WhenItFails\\errors.en.json"

$content =
    Get-Content `
        -Path $catalogPath `
        -Raw

$modifiedContent =
    $content -replace `
        '"defaultSeverity": "Error"', `
        '"defaultSeverity": "Fatal"'

Set-Content `
    -Path $catalogPath `
    -Value $modifiedContent `
    -Encoding utf8
```

Verify the mutation:

```powershell
Select-String `
    -Path $catalogPath `
    -Pattern '"defaultSeverity": "Fatal"'
```

Never trust a negative validation test until the mutation is confirmed.

## Important replacement behavior

PowerShell `-replace` uses regular expressions.

Characters with regex meaning must be escaped when necessary.

For exact literal replacement, this is safer:

```powershell
$oldValue = '"defaultSeverity": "Error"'
$newValue = '"defaultSeverity": "Fatal"'

$modifiedContent =
    $content.Replace(
        $oldValue,
        $newValue)
```

`.Replace()` performs literal string replacement.

## Negative validation test

Create a disposable workspace:

```powershell
$testRoot = Join-Path `
    $env:TEMP `
    ("when-it-fails-invalid-" + \[Guid]::NewGuid().ToString("N"))

New-Item `
    -Path $testRoot `
    -ItemType Directory `
    -Force |
Out-Null
```

Initialize:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- init $testRoot
```

Mutate:

```powershell
$catalogPath =
    Join-Path `
        $testRoot `
        "Jsons\\WhenItFails\\errors.en.json"

$content =
    Get-Content `
        -Path $catalogPath `
        -Raw

$modifiedContent =
    $content.Replace(
        '"defaultSeverity": "Error"',
        '"defaultSeverity": "Fatal"')

Set-Content `
    -Path $catalogPath `
    -Value $modifiedContent `
    -Encoding utf8
```

Verify:

```powershell
Select-String `
    -Path $catalogPath `
    -Pattern '"defaultSeverity": "Fatal"'
```

Validate:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate $testRoot

$validationExitCode = $LASTEXITCODE
```

Assert:

```powershell
if ($validationExitCode -ne 2)
{
    throw `
        "Expected validation exit code 2, got $validationExitCode."
}
```

Clean up:

```powershell
Remove-Item `
    -Path $testRoot `
    -Recurse `
    -Force
```

## Copying a workspace

Copy the package directory:

```powershell
Copy-Item `
    -Path .\\Jsons\\WhenItFails `
    -Destination $testRoot `
    -Recurse
```

This creates:

rather than:

When a project-root layout is required, create the parent structure first:

```powershell
$targetJsonsPath =
    Join-Path `
        $testRoot `
        "Jsons"

New-Item `
    -Path $targetJsonsPath `
    -ItemType Directory `
    -Force |
Out-Null

Copy-Item `
    -Path .\\Jsons\\WhenItFails `
    -Destination $targetJsonsPath `
    -Recurse
```

Result:

## File encoding

PowerShell version matters when writing text files.

Modern PowerShell:

uses UTF-8 without BOM for many text-writing operations when `-Encoding utf8` is selected.

Windows PowerShell 5.1 may write UTF-8 with BOM.

Setter and .NET can normally read UTF-8 with or without BOM, but large repository-wide encoding changes should be avoided.

Check PowerShell version:

```powershell
$PSVersionTable.PSVersion
```

## Inspecting BOM

Read the first bytes:

```powershell
$bytes =
    \[System.IO.File]::ReadAllBytes(
        ".\\Jsons\\WhenItFails\\errors.en.json")

$bytes |
Select-Object -First 4
```

UTF-8 BOM bytes are:

Hexadecimal:

## Preserve existing encoding

Setter itself serializes the catalog through .NET JSON APIs.

Manual PowerShell edits may rewrite encoding or line endings.

Prefer Setter edit commands for supported fields.

Use direct PowerShell file rewriting only in:

* disposable tests,
* deliberate manual maintenance,
* recovery procedures,
* unsupported field editing.

Always review the Git diff afterward.

## Windows line endings

Windows commonly uses:

Linux commonly uses:

JSON parsing normally accepts both.

However, Git may show large diffs if an editor rewrites every line ending.

Inspect:

```powershell
git diff --stat
git diff -- .\\Jsons\\WhenItFails\\errors.en.json
```

Do not commit unexplained whole-file rewrites.

## Git line-ending configuration

Inspect:

```powershell
git config --get core.autocrlf
```

Common Windows setting:

Repository `.gitattributes` should be treated as the project authority when present.

Do not change line-ending policy merely to fix one local diff without understanding repository conventions.

## Case sensitivity on Windows

Windows filesystems are usually case-insensitive by default.

These may resolve to the same file locally:

Linux usually treats them as different paths.

Always use canonical repository casing:

A path that works on Windows may fail in Linux CI due to casing.

## Casing-only rename in Git

On a case-insensitive filesystem, use an intermediate name:

```powershell
git mv `
  .\\Docs\\validation `
  .\\Docs\\validation.tmp

git mv `
  .\\Docs\\validation.tmp `
  .\\Docs\\Validation
```

This ensures Git records the casing change.

## File locks

Windows applications often hold stronger file locks than typical Linux tools.

A save may fail when:

* an editor holds the file exclusively,
* antivirus scans the temporary file,
* synchronization software is active,
* another Setter process is writing,
* an indexing service briefly locks the file.

Close editors or tools that may hold an exclusive lock and retry only after confirming the source file is unchanged.

## Antivirus and endpoint security

Corporate Windows machines may inspect:

* temporary files,
* newly generated executables,
* JSON files,
* `dotnet` child processes.

This can occasionally delay or block operations.

Do not disable endpoint security.

Instead:

* inspect the reported error,
* verify allowed project locations,
* use approved development directories,
* contact system administration when policy blocks required work.

## Corporate proxy

Restore may fail behind a corporate proxy.

Check:

```powershell
dotnet restore
```

Inspect environment variables:

```powershell
Get-ChildItem Env: |
Where-Object {
    $\_.Name -match 'PROXY'
}
```

Use only organization-approved proxy configuration.

Workspace validation itself does not require internet access after dependencies are restored and the project is built.

## Read-only files

Inspect file attributes:

```powershell
Get-Item `
    .\\Jsons\\WhenItFails\\errors.en.json |
Select-Object `
    FullName,
    IsReadOnly,
    Attributes
```

Remove the read-only attribute only when appropriate:

```powershell
$file =
    Get-Item `
        .\\Jsons\\WhenItFails\\errors.en.json

$file.IsReadOnly = $false
```

Also confirm directory permissions.

## Access checks

PowerShell does not have a direct equivalent of Unix `test -w`.

A practical safe check is to inspect ACLs:

```powershell
Get-Acl `
    .\\Jsons\\WhenItFails |
Format-List
```

Avoid creating test files in an important workspace merely to probe permissions unless necessary.

Use a disposable directory for permission experiments.

## Long paths

Modern .NET supports long paths when the operating system and policy permit them.

Problems may still occur with:

* older tools,
* Git configuration,
* corporate policies,
* deeply nested temporary directories.

Prefer reasonably short project roots.

Example:

rather than deeply nested profile or synchronized folders.

## OneDrive and synchronized folders

Repositories under OneDrive or similar synchronization tools may experience:

* transient locks,
* delayed file availability,
* conflict copies,
* backup duplication,
* temporary-file interference.

For active development, a normal local workspace is often more predictable.

## Windows Terminal

Windows Terminal is recommended because it provides:

* modern Unicode support,
* better ANSI handling,
* resize behavior,
* PowerShell profiles,
* multiple tabs,
* improved rendering.

The rich Spectre.Console output should display well in a modern terminal.

## Rich output

Commands such as:

may use rich terminal rendering.

When redirecting output, use `--plain` where supported:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- errors . --plain |
Out-File `
  -FilePath .\\errors.txt `
  -Encoding utf8
```

## Capturing plain output

```powershell
$output =
    dotnet run `
      --project Toolroom/WhenItFails/Setter `
      -- details . AFW\_NET\_0001 --plain

$setterExitCode = $LASTEXITCODE

if ($setterExitCode -ne 0)
{
    throw "Detail lookup failed with exit code $setterExitCode."
}

$output
```

## Extracting one field

```powershell
$output =
    dotnet run `
      --project Toolroom/WhenItFails/Setter `
      -- details . AFW\_NET\_0001 --plain

$severityLine =
    $output |
    Where-Object {
        $\_ -like "Severity:\*"
    }

$severity =
    $severityLine.Substring(
        "Severity: ".Length)

$severity
```

Plain output remains presentation-oriented.

A future JSON output mode would be preferable for durable automation.

## Build and test

```powershell
dotnet restore

dotnet build `
  --no-restore

dotnet test `
  --no-build
```

Validate:

```powershell
dotnet run `
  --no-build `
  --project Toolroom/WhenItFails/Setter `
  -- validate .
```

## Important `--no-build` behavior

The Setter project must already be built before:

```powershell
dotnet run --no-build
```

Correct sequence:

## Recommended Windows pre-commit sequence

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

## Reusable native-command helper

```powershell
function Invoke-NativeCommand
{
    param
    (
        \[Parameter(Mandatory = $true)]
        \[scriptblock] $Command,

        \[Parameter(Mandatory = $true)]
        \[string] $Description
    )

    \& $Command

    $nativeExitCode = $LASTEXITCODE

    if ($nativeExitCode -ne 0)
    {
        throw `
            "$Description failed with exit code $nativeExitCode."
    }
}
```

Example:

```powershell
Invoke-NativeCommand `
    -Description "WhenItFails validation" `
    -Command {
        dotnet run `
          --project Toolroom/WhenItFails/Setter `
          -- validate .
    }
```

## Complete disposable Windows smoke test

```powershell
$testRoot = Join-Path `
    $env:TEMP `
    ("when-it-fails-smoke-" + \[Guid]::NewGuid().ToString("N"))

try
{
    New-Item `
        -Path $testRoot `
        -ItemType Directory `
        -Force |
    Out-Null

    dotnet run `
      --project Toolroom/WhenItFails/Setter `
      -- init $testRoot

    if ($LASTEXITCODE -ne 0)
    {
        throw "Initialization failed with exit code $LASTEXITCODE."
    }

    dotnet run `
      --project Toolroom/WhenItFails/Setter `
      -- validate $testRoot

    if ($LASTEXITCODE -ne 0)
    {
        throw "Validation failed with exit code $LASTEXITCODE."
    }

    dotnet run `
      --project Toolroom/WhenItFails/Setter `
      -- details $testRoot AFW\_NET\_0001 --plain

    if ($LASTEXITCODE -ne 0)
    {
        throw "Detail lookup failed with exit code $LASTEXITCODE."
    }

    dotnet run `
      --project Toolroom/WhenItFails/Setter `
      -- set-title `
      $testRoot `
      AFW\_NET\_0001 `
      "Network is not available"

    if ($LASTEXITCODE -ne 0)
    {
        throw "Title update failed with exit code $LASTEXITCODE."
    }

    dotnet run `
      --project Toolroom/WhenItFails/Setter `
      -- validate $testRoot

    if ($LASTEXITCODE -ne 0)
    {
        throw "Post-edit validation failed with exit code $LASTEXITCODE."
    }

    Get-ChildItem `
        -Path (Join-Path $testRoot "Jsons\\WhenItFails") `
        -Filter "errors.en.\*.bak.json" `
        -File
}
finally
{
    if (Test-Path $testRoot)
    {
        Remove-Item `
            -Path $testRoot `
            -Recurse `
            -Force
    }
}
```

## Command Prompt

Setter can also run from `cmd.exe`.

Example:

```cmd
dotnet run --project Toolroom\\WhenItFails\\Setter -- validate .
```

Exit code:

```cmd
echo %ERRORLEVEL%
```

Conditional failure:

```cmd
dotnet run --project Toolroom\\WhenItFails\\Setter -- validate .

if errorlevel 1 exit /b %ERRORLEVEL%
```

PowerShell is generally better suited for more complex automation.

## Batch line continuation

Command Prompt uses caret:

Example:

```cmd
dotnet run ^
  --project Toolroom\\WhenItFails\\Setter ^
  -- validate .
```

As with PowerShell backticks, trailing characters after the continuation marker may break the command.

## Visual Studio Code terminal

In VS Code, confirm which shell is active.

Typical options:

* PowerShell,
* Command Prompt,
* Git Bash,
* WSL.

The command syntax depends on the active shell.

A command copied from Bash may not work in PowerShell without translation.

## Git Bash

Git Bash supports many Linux-style examples:

```sh
pwd
find
grep
rm -rf
```

However, Windows path conversion may affect commands.

Example:

```sh
dotnet run \\
  --project Toolroom/WhenItFails/Setter \\
  -- validate .
```

When using Git Bash, continue following Bash exit-code rules with:

```sh
echo $?
```

## WSL

WSL is a separate Linux environment.

A repository accessed through:

may have different performance and permission behavior than a repository stored inside the WSL filesystem.

Do not mix simultaneous Windows and WSL writers on the same workspace.

## Cross-platform rule

A reliable repository should work from both:

Therefore:

* preserve exact path casing,
* avoid machine-specific absolute paths,
* avoid unreviewed line-ending rewrites,
* keep scripts shell-specific where necessary,
* test Linux CI even when developing on Windows,
* test Windows behavior for Windows users.

## Windows troubleshooting checklist

When Setter fails on Windows, check:

1. Current directory.
2. Exact project path.
3. .NET 10 SDK availability.
4. `$LASTEXITCODE`.
5. Quoting of paths and text.
6. File read-only attribute.
7. Directory ACLs.
8. Editor or antivirus file locks.
9. OneDrive or synchronization interference.
10. Git line-ending changes.
11. Exact repository casing.
12. Corporate proxy during restore.
13. Available disk space.
14. Whether another Setter process is running.

## Related documentation

* [Getting Started](../Getting-Started/en.md)
* [Commands](../Commands/en.md)
* [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
* [Workspace Paths and Initialization](../Workspace%20Paths%20and%20Initialization/en.md)
* [Testing and CI](../Testing%20and%20CI/en.md)
* [Backups and Recovery](../Backups%20and%20Recovery/en.md)
* [Troubleshooting](../Troubleshooting/en.md)

## Central principle

> On Windows, use the same Setter workflow, but let PowerShell handle paths, temporary workspaces, quoting, and `$LASTEXITCODE` explicitly.

