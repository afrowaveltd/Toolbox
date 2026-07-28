# Exit Codes and Automation

This guide describes the current process-exit and machine-output contract of WhenItFails Setter.

Automation should evaluate two independent signals:

1. the process exit code;
2. structured command output, preferably `--json`.

> Machine consumers should use JSON and the process exit code together.

## Exit-code model

Exit code `0` means that the command completed successfully.

Exit code `1` means that the command could not start correctly because its invocation was invalid. Typical causes include a missing required argument, an unknown command, or another command-line usage problem.

Exit code `2` means that Setter understood the command but could not complete the requested operation. Typical causes include an invalid workspace, lookup failure, validation failure, rejected edit, backup failure, restore failure, or persistence failure.

Exit code `3` means that an unexpected exception reached the top-level application boundary. Treat this as an application or environment failure requiring investigation.

The exact issue code and JSON payload provide the detailed reason. The exit code intentionally remains a small process-level classification.

## Exit code and issue code are different

A process exit code answers:

```text
Did the command succeed, fail because of invocation, fail during the requested operation, or crash unexpectedly?
```

An issue code answers:

```text
Why did the operation fail?
```

For example, two commands may both return exit code `2` while reporting different issue codes such as a validation failure, an unknown error reference, or an I/O failure.

Do not replace issue inspection with exit-code inspection. Do not ignore the exit code merely because a JSON payload was produced.

## Output modes

Setter supports three output surfaces:

- rich terminal output for interactive use;
- `--plain` for simplified human-readable text;
- `--json` for machine-readable output.

Do not parse rich terminal output in scripts. Its borders, tables, spacing, wrapping, and terminal decoration are presentation details rather than a machine contract.

`--plain` is useful for logs and simple human review, but it is still text intended for people.

Use `--json` whenever automation needs fields, issue codes, counts, paths, or command results.

## Recommended automation rule

A robust script should:

1. run Setter with `--json` where supported;
2. capture stdout and stderr intentionally;
3. capture the exit code immediately;
4. reject every non-zero exit code unless that exact failure is an expected branch;
5. parse JSON only after confirming that the expected output was produced;
6. preserve failure output for diagnostics.

Never decide success by searching rendered text for words such as `Success`, `Valid`, or `Completed`.

## Bash example

```bash
#!/usr/bin/env bash

set -euo pipefail

output_file="$(mktemp)"
error_file="$(mktemp)"
trap 'rm -f "$output_file" "$error_file"' EXIT

set +e
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate . --json \
  >"$output_file" \
  2>"$error_file"
setter_exit_code=$?
set -e

if [ "$setter_exit_code" -ne 0 ]; then
  cat "$error_file" >&2
  cat "$output_file" >&2
  exit "$setter_exit_code"
fi

cat "$output_file"
```

The important rule is to capture the exit code immediately. Any later command replaces `$?`.

`set -euo pipefail` is recommended for automation, but scripts that need to inspect an expected non-zero result must temporarily handle that command explicitly, as shown above.

## Pipelines

Without `pipefail`, this pipeline may report the exit code of `tee` rather than Setter:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate . --json |
tee validation.json
```

Use:

```bash
set -o pipefail
```

or capture Setter output before piping it into another command.

## PowerShell example

```powershell
$outputPath = Join-Path $env:TEMP "setter-validation.json"
$errorPath = Join-Path $env:TEMP "setter-validation.err.txt"

& dotnet run `
    --project Toolroom/WhenItFails/Setter `
    -- validate . --json `
    1> $outputPath `
    2> $errorPath

$setterExitCode = $LASTEXITCODE

if ($setterExitCode -ne 0)
{
    Get-Content $errorPath | Write-Error
    Get-Content $outputPath | Write-Error
    exit $setterExitCode
}

Get-Content $outputPath
```

Use `$LASTEXITCODE` for native processes such as `dotnet`. Capture it before running another native command.

PowerShell `$?` indicates whether the last operation succeeded, but `$LASTEXITCODE` is the explicit native process code required for Setter automation.

## Success does not always mean data changed

Some read-only commands may successfully return an empty result set. That is still exit code `0` when the command and filters are valid.

Automation must distinguish:

- command success;
- returned item count;
- whether a write occurred;
- whether the resulting workspace satisfies the caller's policy.

Do not reinterpret a valid empty result as an application crash.

## Write-command automation

For commands that change catalogs, exit code `0` is necessary but not sufficient evidence for a release workflow.

After a successful write:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate . --json
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys . --json
git diff --check
git status --short
```

Run `check-doc-links` when Markdown or local documentation links are involved.

Automation should also inspect the resulting Git diff and ensure that timestamped `.bak.json` files were not staged accidentally.

## Restore automation

`restore-backup` is a write operation. A successful process result means the selected backup was restored according to the command contract; it does not prove that the restored workspace is semantically correct for the current branch.

After restoration:

1. validate the complete workspace;
2. inspect affected entries, profiles, mappings, or references;
3. review the Git diff;
4. run the focused Setter test suite;
5. stop if any verification remains red.

Do not build automation that restores the newest backup blindly.

## Expected failures

Some scripts intentionally test failure behavior. In that case, compare the exit code and the structured issue result explicitly.

Example logic:

```text
run command expected to fail
→ capture exit code immediately
→ assert expected non-zero class
→ parse JSON issue code
→ assert target file is unchanged
→ assert no unexpected backup was created
```

Do not use a blanket `|| true` or `-ErrorAction SilentlyContinue` without later checking the real result. Those patterns can turn an unexpected failure into a false green build.

## Unexpected failures

Exit code `3` should be rare. Preserve:

- command arguments with secrets removed;
- stdout and stderr;
- operating-system and .NET information;
- relevant file permissions;
- workspace path;
- the exact Setter commit;
- reproduction steps.

Do not automatically retry an unexpected failure against a writable catalog until the active file, temporary files, and backups have been inspected.

## CI gate

The focused Setter suite is the minimum gate for Setter-only changes:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Run the repository-wide suite when shared libraries, public runtime contracts, project wiring, or other consumers may be affected:

```powershell
dotnet test
```

A CI job must fail when the relevant Setter command or test command returns a non-zero exit code.

## Automation checklist

Before relying on a Setter command in automation, confirm:

- [ ] the command supports the intended output mode;
- [ ] the script uses `--json` for machine parsing;
- [ ] the exit code is captured immediately;
- [ ] stdout and stderr are handled deliberately;
- [ ] rich output is not parsed;
- [ ] expected failures assert both exit and issue codes;
- [ ] pipelines preserve the Setter exit code;
- [ ] writes are followed by validation and diff review;
- [ ] backups are not staged accidentally;
- [ ] restore operations receive complete post-restore verification;
- [ ] the focused Setter tests are green.

## Stop rule

> A script must not convert an unexplained non-zero Setter exit code into a successful pipeline result.

Fix or explicitly classify the current failure before continuing.

## Related documentation

- [Testing and CI](../Testing%20and%20CI/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Reviewing Catalog Changes](../Reviewing%20Catalog%20Changes/en.md)
- [Known Limitations](../Known%20Limitations/en.md)
