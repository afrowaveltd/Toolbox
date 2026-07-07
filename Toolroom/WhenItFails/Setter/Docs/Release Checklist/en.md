# Release checklist

This checklist is for preparing a WhenItFails Setter change for commit, merge, package, or release.

It focuses on practical verification:

```text
source builds
tests pass
catalog validates
documentation links work
platform commands are sane
Git diff is intentional
```

Use it after normal development and before publishing or merging changes.

## Scope

This checklist applies to changes in and around:

```text
Toolroom/WhenItFails/Setter
Jsons/WhenItFails
WhenItFails
WhenItFails.Tests
Toolroom/WhenItFails/Setter.Tests
```

It is useful for:

- documentation-only changes,
- catalog changes,
- Setter command changes,
- validation changes,
- safe-write changes,
- CI changes,
- release preparation.

## Release principle

A change is not ready just because it compiles.

A Setter change should be considered ready only when:

```text
it builds
it tests
it validates
it runs
it is documented
it has an intentional diff
```

## Quick release sequence

Linux / Bash:

```bash
dotnet restore

dotnet build   --no-restore

dotnet test   --no-build

dotnet run   --no-build   --project Toolroom/WhenItFails/Setter   -- validate .

dotnet run   --no-build   --project Toolroom/WhenItFails/Setter   -- summary .

git diff --check

git status --short
```

Windows / PowerShell:

```powershell
dotnet restore

dotnet build `
  --no-restore

dotnet test `
  --no-build

dotnet run `
  --no-build `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

dotnet run `
  --no-build `
  --project Toolroom/WhenItFails/Setter `
  -- summary .

git diff --check

git status --short
```

## Before starting release checks

Confirm current branch:

```bash
git branch --show-current
```

PowerShell:

```powershell
git branch --show-current
```

Confirm workspace state:

```bash
git status --short
```

PowerShell:

```powershell
git status --short
```

If the working tree contains unrelated changes, decide whether to:

- commit them separately,
- stash them,
- discard them,
- keep them intentionally in the release.

Do not release accidental local edits.

## Restore packages

Run:

```bash
dotnet restore
```

PowerShell:

```powershell
dotnet restore
```

Expected result:

```text
restore succeeds
```

If restore fails, check:

- SDK availability,
- package feeds,
- corporate proxy,
- network access,
- NuGet configuration,
- credentials if private feeds are used.

## Build

Run:

```bash
dotnet build --no-restore
```

PowerShell:

```powershell
dotnet build --no-restore
```

Expected result:

```text
build succeeds
```

Do not ignore warnings casually.

A new warning may indicate a real release issue.

## Tests

Run:

```bash
dotnet test --no-build
```

PowerShell:

```powershell
dotnet test --no-build
```

Expected result:

```text
all tests pass
```

If a test fails, do not patch the test expectation until the behavior is understood.

First ask:

- Did the production behavior change intentionally?
- Did validation rules change?
- Did command output change?
- Did catalog seed data change?
- Did path or platform behavior change?
- Is the test too strict about presentation?

## Catalog validation

Run:

```bash
dotnet run   --no-build   --project Toolroom/WhenItFails/Setter   -- validate .
```

PowerShell:

```powershell
dotnet run `
  --no-build `
  --project Toolroom/WhenItFails/Setter `
  -- validate .
```

Expected exit code:

```text
0
```

Validation must pass before release.

A release with invalid default catalogs is not ready.

## Workspace summary

Run:

```bash
dotnet run   --no-build   --project Toolroom/WhenItFails/Setter   -- summary .
```

PowerShell:

```powershell
dotnet run `
  --no-build `
  --project Toolroom/WhenItFails/Setter `
  -- summary .
```

Review:

- catalog counts,
- owners,
- code groups,
- profiles,
- primary-category distribution,
- displayed workspace path.

The summary should look plausible.

A passing validation result does not replace human review.

## Smoke test

Run a disposable smoke test.

Linux / Bash:

```bash
test_root="$(mktemp -d /tmp/when-it-fails-release-XXXXXXXXXX)"

cleanup()
{
  rm -rf "$test_root"
}

trap cleanup EXIT

dotnet run   --no-build   --project Toolroom/WhenItFails/Setter   -- init "$test_root"

dotnet run   --no-build   --project Toolroom/WhenItFails/Setter   -- validate "$test_root"

dotnet run   --no-build   --project Toolroom/WhenItFails/Setter   -- details "$test_root" AFW_NET_0001 --plain

dotnet run   --no-build   --project Toolroom/WhenItFails/Setter   -- set-title   "$test_root"   AFW_NET_0001   "Network is not available"

dotnet run   --no-build   --project Toolroom/WhenItFails/Setter   -- validate "$test_root"
```

Windows / PowerShell:

```powershell
$testRoot = Join-Path `
    $env:TEMP `
    ("when-it-fails-release-" + [Guid]::NewGuid().ToString("N"))

try
{
    New-Item `
        -Path $testRoot `
        -ItemType Directory `
        -Force |
    Out-Null

    dotnet run `
      --no-build `
      --project Toolroom/WhenItFails/Setter `
      -- init $testRoot

    if ($LASTEXITCODE -ne 0)
    {
        throw "Initialization failed with exit code $LASTEXITCODE."
    }

    dotnet run `
      --no-build `
      --project Toolroom/WhenItFails/Setter `
      -- validate $testRoot

    if ($LASTEXITCODE -ne 0)
    {
        throw "Validation failed with exit code $LASTEXITCODE."
    }

    dotnet run `
      --no-build `
      --project Toolroom/WhenItFails/Setter `
      -- details $testRoot AFW_NET_0001 --plain

    if ($LASTEXITCODE -ne 0)
    {
        throw "Details failed with exit code $LASTEXITCODE."
    }

    dotnet run `
      --no-build `
      --project Toolroom/WhenItFails/Setter `
      -- set-title `
      $testRoot `
      AFW_NET_0001 `
      "Network is not available"

    if ($LASTEXITCODE -ne 0)
    {
        throw "Edit failed with exit code $LASTEXITCODE."
    }

    dotnet run `
      --no-build `
      --project Toolroom/WhenItFails/Setter `
      -- validate $testRoot

    if ($LASTEXITCODE -ne 0)
    {
        throw "Post-edit validation failed with exit code $LASTEXITCODE."
    }
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

## Command sanity checks

Run these basic commands:

```bash
dotnet run   --no-build   --project Toolroom/WhenItFails/Setter   -- help
```

```bash
dotnet run   --no-build   --project Toolroom/WhenItFails/Setter   -- errors . --plain
```

```bash
dotnet run   --no-build   --project Toolroom/WhenItFails/Setter   -- details . AFW_NET_0001 --plain
```

```bash
dotnet run   --no-build   --project Toolroom/WhenItFails/Setter   -- errors . --profile WEB
```

For PowerShell, use the same commands with backtick line continuation.

## Negative command sanity checks

Check at least one expected failure.

Unknown command:

```bash
dotnet run   --no-build   --project Toolroom/WhenItFails/Setter   -- definitely-not-a-command

echo "$?"
```

Expected exit code:

```text
1
```

Unsupported severity:

```bash
dotnet run   --no-build   --project Toolroom/WhenItFails/Setter   -- set-severity . AFW_NET_0001 Banana

echo "$?"
```

Expected exit code:

```text
2
```

PowerShell:

```powershell
dotnet run `
  --no-build `
  --project Toolroom/WhenItFails/Setter `
  -- set-severity . AFW_NET_0001 Banana

$LASTEXITCODE
```

Expected:

```text
2
```

## Documentation link check

Review README documentation links.

At minimum, confirm links exist for:

```text
Documentation Map
Glossary
Commands
Command Quick Reference
Exit Codes and Automation
Windows and PowerShell
Linux and Bash
Workspace Paths and Initialization
Catalog Files
Catalog Author Checklist
Profiles
Validation
Workspace Summary
Browsing and Filtering Errors
Inspecting Error Details
Editing Error Fields
Authoring Error Text
Setting Title
Plain Output
Safe Writes
Backups and Recovery
Testing and CI
Troubleshooting
```

Check paths carefully.

Spaces in Markdown links should normally be encoded as:

```text
%20
```

Example:

```markdown
[Catalog Files](Docs/Catalog%20Files/en.md)
```

## Documentation consistency check

For documentation changes, check:

- file path matches README link,
- title matches link text,
- related-documentation links point to existing files,
- command examples use current command names,
- Bash examples use `\`,
- PowerShell examples use backticks,
- Windows examples use `$LASTEXITCODE`,
- Bash examples capture `$?` immediately,
- no accidental local paths remain,
- no obsolete command names remain.

## Markdown formatting check

Run:

```bash
git diff --check
```

PowerShell:

```powershell
git diff --check
```

This catches trailing whitespace and some whitespace problems.

Also manually inspect rendered Markdown when possible.

Common Markdown mistakes:

- unclosed code fence,
- wrong language tag,
- broken relative link,
- heading level jump,
- accidental nested code fence,
- raw Windows path interpreted oddly,
- missing blank line before a list.

## Git diff review

Run:

```bash
git diff --stat
```

Then inspect relevant files:

```bash
git diff -- Toolroom/WhenItFails/Setter
git diff -- Jsons/WhenItFails
git diff -- WhenItFails
git diff -- WhenItFails.Tests
```

PowerShell commands are the same.

Confirm:

- only intended files changed,
- no timestamped backups are included,
- no temporary files are included,
- no IDE files are included,
- generated files are expected,
- documentation changes are intentional,
- catalog changes are intentional.

## Backup and temporary files

Search for backup and temporary files.

Bash:

```bash
find .   -type f   \(     -name '*.bak.json'     -o -name '.*.tmp'     -o -name '*.tmp'   \)   -print
```

PowerShell:

```powershell
Get-ChildItem `
    -Path . `
    -Recurse `
    -File |
Where-Object {
    $_.Name -like "*.bak.json" -or
    $_.Name -like "*.tmp" -or
    $_.Name -match "^\..*\.tmp$"
} |
Select-Object FullName
```

Do not commit local Setter backups unless project policy explicitly says to do so.

## Line ending check

If the diff is unexpectedly huge, inspect line endings.

Bash:

```bash
git diff --stat
git diff -- Jsons/WhenItFails/errors.en.json
```

PowerShell:

```powershell
git diff --stat
git diff -- Jsons/WhenItFails/errors.en.json
```

If every line changed, line endings or formatting may have been rewritten.

Do not commit whole-file churn unless it was deliberate.

## Path casing check

On Linux, path casing errors are visible immediately.

On Windows, check carefully that documentation and project paths use canonical casing:

```text
Jsons/WhenItFails
Toolroom/WhenItFails/Setter
Docs/Windows and PowerShell/en.md
Docs/Linux and Bash/en.md
```

Wrong casing may work on Windows and fail on Linux CI.

## Public API check

If source code changed, review whether public API changed.

Ask:

- Was a public type renamed?
- Was a public method removed?
- Was a namespace changed?
- Was a command removed?
- Was an exit code changed?
- Was a JSON field renamed?
- Was catalog schema changed?
- Was validation made stricter?

Public changes need documentation and tests.

## Command compatibility check

If a command changed, verify:

- help text is still accurate,
- command name still works,
- aliases still work,
- missing arguments return expected exit code,
- validation failure returns expected exit code,
- success returns expected exit code,
- documentation examples still work,
- tests cover the changed behavior.

## Exit-code compatibility check

Exit codes are automation contracts.

Before changing them, ask:

- Will scripts break?
- Is the previous behavior documented?
- Is the new behavior more correct?
- Do tests cover it?
- Is the change mentioned in documentation?

Do not change exit codes casually.

## Catalog compatibility check

If default catalogs changed, ask:

- Were stable IDs changed?
- Were numeric codes changed?
- Were names changed?
- Were owners changed?
- Were categories changed?
- Were profile policies changed?
- Could consumers rely on old values?
- Is migration needed?
- Is documentation updated?

Catalog compatibility matters even when code does not change.

## Security review

Before release, check for accidental sensitive content:

```bash
git diff
```

Look for:

- passwords,
- tokens,
- connection strings,
- customer data,
- personal data,
- internal hostnames,
- private file paths,
- stack traces,
- raw production logs,
- secrets in documentation examples.

Do not publish real secrets in examples.

Use safe placeholders.

## Production-safety review

For production-facing profiles, messages, or mappings, confirm:

- stack traces are not exposed by default,
- exception details are not exposed by default,
- sensitive metadata is not exposed by default,
- user-facing text is safe,
- developer hints do not leak secrets,
- documentation keys are safe.

Development convenience should not leak into production defaults.

## Versioning check

If the change affects packaging or release identity, check:

- package version,
- assembly version if used,
- changelog if present,
- release notes if present,
- documentation mentioning version behavior,
- schema version if catalog shape changed.

Do not bump schema version just because catalog content changed.

Schema version is for document shape and compatibility.

## Changelog check

If the project uses a changelog, add an entry describing:

- new commands,
- changed commands,
- validation changes,
- catalog changes,
- documentation additions,
- breaking changes,
- migration notes.

A good changelog entry is short but actionable.

## README check

Confirm the README still answers:

- what Setter is,
- how to run it,
- where documentation lives,
- how to validate,
- how to get help,
- what the current command set is.

If a new documentation page was added, add it to both relevant README files.

## Release notes outline

A simple release note can include:

```markdown
## Added

- Added documentation for catalog files.
- Added profile guide.
- Added catalog author checklist.

## Changed

- Clarified workspace validation workflow.

## Fixed

- Corrected command examples.

## Compatibility

- No command syntax changes.
- No catalog schema changes.
```

Use only sections that apply.

## Documentation-only release check

For documentation-only changes, still run:

```bash
dotnet build
dotnet test
dotnet run   --project Toolroom/WhenItFails/Setter   -- validate .
git diff --check
```

Documentation can contain wrong commands, broken paths, or stale assumptions.

A documentation-only change can still mislead users badly.

## Catalog-only release check

For catalog-only changes, run:

```bash
dotnet build
dotnet test
dotnet run   --project Toolroom/WhenItFails/Setter   -- validate .
dotnet run   --project Toolroom/WhenItFails/Setter   -- summary .
git diff -- Jsons/WhenItFails
git diff --check
```

Also inspect affected entries:

```bash
dotnet run   --project Toolroom/WhenItFails/Setter   -- details . AFW_NET_0001
```

Use the actual affected ID.

## Code-change release check

For code changes, run the full sequence:

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build
dotnet run --no-build --project Toolroom/WhenItFails/Setter -- validate .
dotnet run --no-build --project Toolroom/WhenItFails/Setter -- summary .
git diff --check
```

Also run a disposable smoke test.

## Cross-platform check

When possible, test on both:

```text
Windows
Linux
```

Especially when changing:

- path handling,
- process exit behavior,
- file writes,
- backups,
- temporary files,
- command examples,
- documentation for shells,
- CI scripts.

If only one platform was tested, state that honestly in the PR or release notes.

## Final pre-commit checklist

Before committing:

```text
restore succeeded
build succeeded
tests passed
workspace validation passed
summary reviewed
smoke test passed where relevant
documentation links reviewed
Git diff reviewed
no backup files included
no temporary files included
no secrets included
commit message is clear
```

## Commit message examples

Documentation:

```text
Add Setter release checklist
```

Catalog:

```text
Add storage error definitions
```

Code:

```text
Validate profile category references
```

Tests:

```text
Add Setter profile validation tests
```

Mixed changes:

```text
Add profile validation and documentation
```

Prefer a clear specific message over:

```text
Update files
```

## Tagging checklist

If creating a release tag:

1. Ensure working tree is clean.
2. Ensure tests passed.
3. Ensure validation passed.
4. Ensure release notes are ready.
5. Ensure version is correct.
6. Create annotated tag if project policy uses it.
7. Push tag deliberately.

Example:

```bash
git status --short

git tag -a v0.1.0 -m "Release v0.1.0"

git push origin v0.1.0
```

Use the actual versioning policy of the repository.

## After release

After release, consider verifying:

- package can be restored or installed,
- command can run from packaged form,
- documentation links work in the repository UI,
- release notes render correctly,
- CI status is green,
- tag points to the intended commit.

## Rollback readiness

Before releasing risky changes, know how to recover.

Useful commands:

```bash
git log --oneline -n 10
```

```bash
git show --stat HEAD
```

```bash
git revert <commit>
```

For catalog mistakes, also know where Setter backups are created:

```text
Jsons/WhenItFails/*.bak.json
```

Rollback is easier when commits are small and focused.

## Related documentation

- [Command Quick Reference](../Command%20Quick%20Reference/en.md)
- [Testing and CI](../Testing%20and%20CI/en.md)
- [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
- [Windows and PowerShell](../Windows%20and%20PowerShell/en.md)
- [Linux and Bash](../Linux%20and%20Bash/en.md)
- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Catalog Files](../Catalog%20Files/en.md)
- [Profiles](../Profiles/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Troubleshooting](../Troubleshooting/en.md)

## Central principle

> A release is ready when the machine checks pass and the human diff review still tells the same story you intended to ship.
