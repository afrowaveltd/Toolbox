# Exit codes and automation

WhenItFails Setter uses process exit codes to communicate whether a command succeeded, received invalid input, detected invalid workspace data, or failed unexpectedly.

For scripts and CI, the exit code is the primary success signal.

## General exit-code model

Setter currently uses:

```text
0
→ command completed successfully
```

```text
1
→ command input was missing, unknown, or otherwise invalid
```

```text
2
→ workspace loading, validation, lookup, editing, or saving failed
```

```text
3
→ unexpected unhandled exception reached the top-level application handler
```

The exact meaning of `1` and `2` depends on the command.

## Top-level command handling

The application dispatches commands from the first argument.

Command names are normalized using:

```text
trim
→ lowercase invariant
```

Therefore these are treated equivalently:

```text
validate
VALIDATE
Validate
```

Canonical lowercase spelling is recommended.

## Help behavior

These commands show help and return:

```text
0
```

```bash
when-it-fails-setter help
```

```bash
when-it-fails-setter --help
```

```bash
when-it-fails-setter -h
```

Running without arguments also shows help and returns:

```text
0
```

Example:

```bash
when-it-fails-setter

echo "Exit code: $?"
```

Expected:

```text
Exit code: 0
```

Showing help is considered a successful operation.

## Unknown command

Example:

```bash
when-it-fails-setter frobnicate
```

Setter:

```text
prints an unknown-command message
→ shows help
→ returns 1
```

Expected:

```text
Exit code: 1
```

This distinguishes an unknown command from an unexpected runtime failure.

## Unexpected exception

The top-level application catches unhandled exceptions.

It renders the exception through Spectre.Console and returns:

```text
3
```

This is reserved for failures that were not converted into normal structured command results.

Examples may include:

* unexpected programming error,
* unhandled filesystem exception,
* invalid internal state,
* dependency failure outside normal response handling.

Scripts should treat `3` as an application failure requiring investigation.

## Capturing an exit code

Capture `$?` immediately after the command:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

exit_code=$?

echo "Exit code: $exit_code"
```

Every later shell command replaces `$?`.

Incorrect:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

echo "Validation finished"

echo "$?"
```

The final value belongs to `echo`, not to Setter.

## Recommended shell pattern

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

setter_exit_code=$?

if [ "$setter_exit_code" -ne 0 ]
then
  echo "Setter failed with exit code $setter_exit_code" >&2
  exit "$setter_exit_code"
fi
```

## `set -e`

A simple automation script may use:

```bash
set -e
```

Then a non-zero Setter result stops the script automatically.

Example:

```bash
#!/usr/bin/env bash

set -e

dotnet build

dotnet test

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

## Recommended strict shell mode

For reliable scripts:

```bash
set -euo pipefail
```

Meaning:

```text
-e
→ stop after a failing command
```

```text
-u
→ fail when an undefined variable is used
```

```text
-o pipefail
→ fail when any command in a pipeline fails
```

## Pipelines

Without `pipefail`, a pipeline may hide Setter failure.

Example:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate . |
tee validation.log
```

The pipeline may return the exit code of `tee`, even when Setter failed.

Use:

```bash
set -o pipefail
```

Then:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate . \
  2>&1 |
tee validation.log
```

Now the pipeline preserves the failure.

## `PIPESTATUS`

Bash also exposes individual pipeline exit codes.

Example:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate . \
  2>&1 |
tee validation.log

setter_exit_code="${PIPESTATUS[0]}"

echo "Setter exit code: $setter_exit_code"
```

This is Bash-specific.

## Validate command

```text
validate <path>
```

Exit codes:

```text
0
→ workspace is valid
```

```text
1
→ path argument is missing
```

```text
2
→ workspace contains loading or validation errors
```

Example valid workspace:

```bash
when-it-fails-setter validate .

echo "Exit code: $?"
```

Expected:

```text
Exit code: 0
```

Example missing path:

```bash
when-it-fails-setter validate

echo "Exit code: $?"
```

Expected:

```text
Exit code: 1
```

Example invalid severity:

```json
"defaultSeverity": "Fatal"
```

Validation returns:

```text
2
```

## Summary and inspect

```text
summary <path>
inspect <path>
```

Exit codes:

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

The alias `inspect` uses the same command implementation and exit behavior.

## Errors command

```text
errors <path> [filters]
```

Exit codes:

```text
0
→ listing completed
```

```text
1
→ path missing or selected profile does not exist
```

```text
2
→ workspace validation failed
```

A valid filter returning zero rows still returns:

```text
0
```

Example:

```bash
when-it-fails-setter errors . \
  --owner DOES_NOT_EXIST

echo "Exit code: $?"
```

A zero-row result is not considered a command failure.

## Unknown profile

Example:

```bash
when-it-fails-setter errors . \
  --profile DOES_NOT_EXIST
```

Expected issue:

```text
UnknownProfileFilter
```

Expected exit code:

```text
1
```

The profile argument itself is invalid because the requested profile definition does not exist.

## Details and detail

```text
details <path> <id|code|name>
detail <path> <id|code|name>
```

Exit codes:

```text
0
→ error displayed
```

```text
1
→ arguments missing or error definition not found
```

```text
2
→ workspace validation failed
```

Example not found:

```bash
when-it-fails-setter details . \
  AFW_UNKNOWN_9999

echo "Exit code: $?"
```

Expected:

```text
Exit code: 1
```

## Initialization

```text
init <project-root>
```

Typical exit-code interpretation:

```text
0
→ workspace initialization completed
```

```text
1
→ required project-root argument missing
```

```text
2
→ bootstrap or filesystem operation failed
```

Initialization should be followed by validation:

```bash
when-it-fails-setter init .
when-it-fails-setter validate .
```

A successful `init` does not prove that every existing catalog is valid.

## Edit commands

Current edit commands:

```text
set-title
set-message
set-developer-hint
set-severity
set-documentation-key
```

General exit-code model:

```text
0
→ field updated and saved successfully
```

```text
1
→ required command arguments missing
```

```text
2
→ loading, lookup, validation, backup, or save failed
```

## Edit lookup failure

Example:

```bash
when-it-fails-setter set-title . \
  AFW_UNKNOWN_9999 \
  "Unknown title"
```

Expected issue:

```text
ErrorDefinitionNotFound
```

Expected exit code:

```text
2
```

For edit commands, lookup failure is treated as an editing failure rather than ordinary invalid syntax.

## Unsupported severity

Example:

```bash
when-it-fails-setter set-severity . \
  AFW_NET_0001 \
  Banana
```

Expected issue:

```text
UnsupportedSeverity
```

Expected exit code:

```text
2
```

No backup should be created because no valid write occurs.

## Empty edit value

Example:

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "   "
```

Expected issue:

```text
TitleIsEmpty
```

Expected exit code:

```text
2
```

Missing the argument entirely is different and normally returns:

```text
1
```

## Demo command

```bash
when-it-fails-setter demo
```

The demo command is intended for manually showing sample validation output.

It is not a workspace validation substitute and should not be used as a release gate.

## Exit code versus issue code

These are different layers.

Exit code:

```text
2
```

means the command failed in a broad category.

Issue code:

```text
UnknownDefaultSeverity
```

describes the specific reason.

Automation should use:

```text
exit code
→ success or failure control flow
```

and, where stable structured access exists:

```text
issue code
→ diagnostic classification
```

Avoid parsing complete human-readable messages as a permanent contract.

## Human output is not the automation contract

Rich output may change because of:

* terminal width,
* Spectre.Console version,
* wording improvements,
* additional details,
* localization,
* formatting changes.

Stable automation should prefer:

```text
process exit code
```

over:

```text
exact rendered sentence
```

## Plain output

Commands supporting `--plain` include:

```text
errors
details
detail
```

Plain output is easier to process, but it is still presentation-oriented.

It does not replace the exit code.

Example:

```bash
output="$(
  when-it-fails-setter details . \
    AFW_NET_0001 \
    --plain
)"

exit_code=$?

if [ "$exit_code" -ne 0 ]
then
  echo "Detail lookup failed." >&2
  exit "$exit_code"
fi

printf '%s\n' "$output"
```

## Command substitution caveat

When using command substitution:

```bash
output="$(command)"
```

the following `$?` normally contains the exit code of the command substitution.

Capture it immediately.

Example:

```bash
output="$(
  when-it-fails-setter errors . \
    --plain
)"

setter_exit_code=$?
```

## Redirecting output

Redirect standard output:

```bash
when-it-fails-setter errors . \
  --plain \
  > errors.txt
```

Redirect errors as well:

```bash
when-it-fails-setter validate . \
  > validation.log \
  2>&1
```

Redirection does not inherently change the process exit code.

## Conditional execution

Run a command only after successful validation:

```bash
when-it-fails-setter validate . &&
when-it-fails-setter summary .
```

Run fallback logic after failure:

```bash
when-it-fails-setter validate . ||
echo "Workspace validation failed." >&2
```

Be careful: the final command in a compound expression may affect the shell’s resulting exit code.

## Preserving failure in fallback logic

This pattern prints a message but preserves failure:

```bash
when-it-fails-setter validate . || {
  exit_code=$?

  echo \
    "Workspace validation failed with exit code $exit_code." \
    >&2

  exit "$exit_code"
}
```

## CI example

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
```

Any non-zero exit code stops the job.

## CI with log capture

```bash
#!/usr/bin/env bash

set -euo pipefail

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate . \
  2>&1 |
tee when-it-fails-validation.log
```

Because `pipefail` is enabled, a Setter failure remains a job failure.

## Distinguishing expected negative tests

A negative test should explicitly require a non-zero result.

Example:

```bash
set +e

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate /tmp/invalid-workspace

exit_code=$?

set -e

if [ "$exit_code" -ne 2 ]
then
  echo \
    "Expected validation exit code 2, got $exit_code." \
    >&2

  exit 1
fi
```

This is useful because `set -e` would otherwise stop the script before the assertion.

## Negative test helper

```bash
expect_exit_code()
{
  expected_exit_code="$1"
  shift

  set +e
  "$@"
  actual_exit_code=$?
  set -e

  if [ "$actual_exit_code" -ne "$expected_exit_code" ]
  then
    echo \
      "Expected exit code $expected_exit_code, got $actual_exit_code." \
      >&2

    return 1
  fi
}
```

Example:

```bash
expect_exit_code \
  2 \
  dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate /tmp/invalid-workspace
```

## Checking success explicitly

Instead of relying on `set -e`:

```bash
if dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
then
  echo "Workspace is valid."
else
  exit_code=$?

  echo \
    "Validation failed with exit code $exit_code." \
    >&2

  exit "$exit_code"
fi
```

## Exit-code summary by command

| Command                     | Success | Input problem | Operational or data failure |
| --------------------------- | ------: | ------------: | --------------------------: |
| `help`                      |     `0` |             — | `3` if unexpected exception |
| no arguments                |     `0` |             — | `3` if unexpected exception |
| unknown command             |       — |           `1` | `3` if unexpected exception |
| `init`                      |     `0` |           `1` |                         `2` |
| `validate`                  |     `0` |           `1` |                         `2` |
| `summary` / `inspect`       |     `0` |           `1` |                         `2` |
| `errors`                    |     `0` |           `1` |                         `2` |
| `details` / `detail`        |     `0` |           `1` |                         `2` |
| edit commands               |     `0` |           `1` |                         `2` |
| top-level unhandled failure |       — |             — |                         `3` |

## Important interpretation rule

Do not interpret every non-zero exit code as the same problem.

```text
1
→ command invocation or requested selection problem
```

```text
2
→ workspace or operation could not be completed
```

```text
3
→ unexpected application failure
```

This distinction helps automation decide whether to:

* correct command input,
* reject catalog changes,
* retry an I/O operation,
* escalate an application defect.

## Retry guidance

Do not automatically retry failures caused by:

* invalid JSON,
* unsupported severity,
* unknown owner,
* duplicate ID,
* missing command argument,
* unknown error definition.

Retry may make sense for transient failures such as:

* temporarily unavailable network mount,
* short-lived file lock,
* interrupted external storage.

The exit code alone does not prove that a failure is transient.

Inspect the issue code and message.

## Logging recommendation

A CI log should contain:

```text
command
working directory
exit code
Setter output
```

Example:

```bash
echo "Working directory: $(pwd)"

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

exit_code=$?

echo "Setter exit code: $exit_code"

exit "$exit_code"
```

## Future compatibility

Exit codes are part of the command-line behavior and should remain stable where practical.

Future commands should follow the same broad model:

```text
0
→ success
1
→ invalid invocation or selection
2
→ expected operational or validation failure
3
→ unexpected unhandled failure
```

Any deliberate change should be documented and tested.

## Testing exit codes

Recommended smoke tests:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- help

echo "Help: $?"
```

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- unknown-command

echo "Unknown command: $?"
```

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

echo "Valid workspace: $?"
```

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_UNKNOWN_9999

echo "Missing definition: $?"
```

Expected:

```text
Help: 0
Unknown command: 1
Valid workspace: 0
Missing definition: 1
```

## Checklist for automation

Before using Setter in automation, confirm:

* the correct working directory is used,
* command paths use exact casing,
* every required argument is supplied,
* option values are quoted when needed,
* `$?` is captured immediately,
* pipelines use `pipefail`,
* zero-row results are not confused with command failure,
* negative tests assert the expected non-zero code,
* rich output is not parsed as a stable API,
* plain output is treated as presentation-oriented,
* exit code `3` is escalated as unexpected,
* validation runs before packaging or deployment.

## Related documentation

* [Commands](../Commands/en.md)
* [Plain Output](../Plain%20Output/en.md)
* [Validation](../Validation/en.md)
* [Testing and CI](../Testing%20and%20CI/en.md)
* [Troubleshooting](../Troubleshooting/en.md)
* [Safe Writes](../Safe%20Writes/en.md)

## Central principle

> Use the exit code to control automation and the issue details to understand why the command failed.
