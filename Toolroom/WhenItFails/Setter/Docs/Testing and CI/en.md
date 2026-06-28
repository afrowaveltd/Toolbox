# Testing and CI

WhenItFails Setter should be tested at three levels:

```text
unit and integration tests
→ command-line smoke tests
→ workspace validation
```

Each level checks something different.

## Test project

Setter has a dedicated test project:

```text
Toolroom/WhenItFails/Setter.Tests
```

Run it directly:

```bash
dotnet test \
  Toolroom/WhenItFails/Setter.Tests
```

Or run all repository tests:

```bash
dotnet test
```

## Build before testing

Recommended local sequence:

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```

This separates:

* dependency restore,
* compilation,
* test execution.

It also makes failures easier to diagnose.

## What the Setter tests cover

Current Setter tests verify editing behavior such as:

* changing an error title,
* changing an error message,
* changing a developer hint,
* changing default severity,
* changing a documentation key,
* creating a backup after a successful write,
* rejecting unsupported severity,
* rejecting empty values,
* preserving the original catalog after rejected edits,
* returning not found for an unknown error,
* avoiding backup creation when no valid write occurs.

## Temporary workspaces

Tests should not modify the repository workspace directly.

The existing test pattern creates a unique temporary project root under the operating system temporary directory.

Conceptually:

```text
temporary project root
→ initialize workspace
→ perform edit
→ reload saved catalog
→ assert result
→ remove temporary workspace
```

A unique GUID-based directory prevents collisions between test runs.

Example structure:

```text
/tmp/
└── afrowave-whenitfails-setter-tests/
    └── <unique-guid>/
        └── Jsons/
            └── WhenItFails/
```

## Why tests use initialization

A temporary workspace is created through:

```text
WhenItFailsWorkspaceInitializer
```

This ensures tests begin with the same bundled templates used by real initialization.

It avoids duplicating large JSON fixtures inside the test project.

The typical sequence is:

```text
create temporary directory
→ initialize bundled catalogs
→ verify initialization success
→ execute test operation
```

## Successful edit test

A successful edit test should verify all three outcomes:

```text
response succeeded
→ saved catalog contains the new value
→ backup was created
```

Example concept:

```csharp
Response<ErrorDefinition> response =
    await editor.SetErrorTitleAsync(
        projectRootPath,
        "AFW_NET_0001",
        "Network is not available");

Assert.True(response.IsSuccess);
Assert.Equal(
    "Network is not available",
    response.Data?.Title);
```

Then reload the file:

```csharp
ErrorDefinition savedError =
    await LoadErrorDefinitionAsync("AFW_NET_0001");

Assert.Equal(
    "Network is not available",
    savedError.Title);
```

Finally verify backup creation:

```text
errors.en.*.bak.json
```

## Why reloading matters

Checking only the returned object is not enough.

A response may contain the intended value even if persistence failed later.

Reloading the catalog verifies:

```text
the file was actually written
```

This distinction is important for any safe-write test.

## Backup assertion

Successful edits should create at least one file matching:

```text
errors.en.*.bak.json
```

Example test logic:

```csharp
string[] backupFiles =
    Directory.GetFiles(
        whenItFailsJsonsPath,
        "errors.en.*.bak.json",
        SearchOption.TopDirectoryOnly);

Assert.NotEmpty(backupFiles);
```

The test should not depend on an exact timestamp.

The timestamp is intentionally generated at runtime.

## Rejected edit test

A rejected edit should verify:

```text
response failed
→ expected issue code exists
→ original value remains unchanged
→ no backup was created
```

Example unsupported severity:

```csharp
Response<ErrorDefinition> response =
    await editor.SetErrorSeverityAsync(
        projectRootPath,
        "AFW_NET_0001",
        "Banana");
```

Expected issue:

```text
UnsupportedSeverity
```

Expected persistence result:

```text
original severity remains Error
```

Expected filesystem result:

```text
no errors.en.*.bak.json file
```

## Empty-value tests

The current edit methods reject whitespace-only values.

Expected issue codes include:

```text
TitleIsEmpty
MessageIsEmpty
DeveloperHintIsEmpty
DocumentationKeyIsEmpty
```

The rejected operation should not:

* modify the source file,
* create a backup,
* return a successful response.

## Unknown error test

An edit targeting a nonexistent error should return:

```text
ErrorDefinitionNotFound
```

Example lookup:

```text
AFW_UNKNOWN_9999
```

The test should also confirm:

```text
no backup created
```

because no valid edit reached the writer.

## Severity normalization test

Severity input is case-insensitive.

Example input:

```text
warning
```

Expected stored value:

```text
Warning
```

This verifies that Setter stores the canonical severity representation.

Supported canonical values are:

```text
Trace
Debug
Information
Warning
Error
Critical
```

## Original values in tests

Tests should record expected original values explicitly.

Example:

```csharp
private const string OriginalTitle =
    "Network unavailable";

private const string OriginalSeverity =
    "Error";
```

This makes failed assertions easier to understand and prevents the test from passing merely because it compared a value to itself.

## Test cleanup

Temporary workspaces should implement deterministic cleanup.

Typical pattern:

```csharp
using TemporaryWorkspace temporaryWorkspace =
    await TemporaryWorkspace.CreateAsync();
```

The workspace is removed when the test ends.

Cleanup should not hide the actual test result.

Therefore cleanup code may safely ignore cleanup-only failures:

```csharp
try
{
    Directory.Delete(
        ProjectRootPath,
        recursive: true);
}
catch
{
    // Test cleanup should not hide the real test result.
}
```

A cleanup failure should not replace the meaningful assertion failure.

## Inspecting failed temporary workspaces

Automatic cleanup is useful for normal runs but can make debugging harder.

During investigation, temporarily disable cleanup or print the workspace path.

Example diagnostic addition:

```csharp
Console.WriteLine(
    temporaryWorkspace.ProjectRootPath);
```

Do not commit debugging changes unless they are intentionally useful long-term.

## Parallel test considerations

Tests create unique workspace directories, which makes them suitable for parallel execution.

However, each individual test should use its own workspace.

Do not share one mutable workspace across tests that edit:

```text
errors.en.json
```

Shared mutable fixtures could cause:

* backup collisions,
* overwritten edits,
* nondeterministic failures,
* incorrect original-value assertions.

## Test isolation rule

Use:

```text
one temporary workspace per test
```

not:

```text
one workspace per test class
```

for tests that perform writes.

## Command-line smoke tests

Unit and integration tests exercise services directly.

Command-line smoke tests verify:

* command dispatch,
* argument parsing,
* exit codes,
* console execution,
* project wiring.

Basic smoke test:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- help
```

Expected exit code:

```text
0
```

## Validation smoke test

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

Capture the result:

```bash
validation_exit_code=$?

echo "Validation exit code: $validation_exit_code"
```

Expected for a valid repository workspace:

```text
0
```

## Initialization smoke test

Use a disposable directory:

```bash
rm -rf /tmp/when-it-fails-ci-test

mkdir -p /tmp/when-it-fails-ci-test

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- init /tmp/when-it-fails-ci-test
```

Then validate:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate /tmp/when-it-fails-ci-test
```

Clean up:

```bash
rm -rf /tmp/when-it-fails-ci-test
```

## Negative validation smoke test

A negative test confirms that invalid catalogs produce a failing exit code.

Create a disposable copy:

```bash
rm -rf /tmp/when-it-fails-invalid-test

cp -a \
  Jsons/WhenItFails \
  /tmp/when-it-fails-invalid-test
```

Modify one severity:

```bash
sed -i \
  '0,/"defaultSeverity": "Error"/s//"defaultSeverity": "Fatal"/' \
  /tmp/when-it-fails-invalid-test/errors.en.json
```

Verify the mutation:

```bash
grep -n \
  '"defaultSeverity": "Fatal"' \
  /tmp/when-it-fails-invalid-test/errors.en.json
```

Run validation:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate /tmp/when-it-fails-invalid-test

validation_exit_code=$?

echo "Validation exit code: $validation_exit_code"
```

Expected:

```text
Validation exit code: 2
```

Then clean up:

```bash
rm -rf /tmp/when-it-fails-invalid-test
```

## Why mutation verification matters

A negative test is valid only when the test input was actually changed.

This command:

```bash
grep -n \
  '"defaultSeverity": "Fatal"' \
  /tmp/when-it-fails-invalid-test/errors.en.json
```

must produce a match before the validation result can be trusted.

Otherwise a zero exit code may simply mean the workspace remained valid.

## Safe-write smoke test

Create a temporary workspace:

```bash
rm -rf /tmp/when-it-fails-write-test

mkdir -p /tmp/when-it-fails-write-test

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- init /tmp/when-it-fails-write-test
```

Apply an edit:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-title \
  /tmp/when-it-fails-write-test \
  AFW_NET_0001 \
  "Network is not available"
```

Confirm the value:

```bash
grep -n \
  '"title": "Network is not available"' \
  /tmp/when-it-fails-write-test/Jsons/WhenItFails/errors.en.json
```

Confirm backup creation:

```bash
find \
  /tmp/when-it-fails-write-test/Jsons/WhenItFails \
  -maxdepth 1 \
  -type f \
  -name 'errors.en.*.bak.json'
```

Validate:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate /tmp/when-it-fails-write-test
```

Clean up:

```bash
rm -rf /tmp/when-it-fails-write-test
```

## CI responsibilities

A minimal CI job should verify:

```text
restore
→ build
→ tests
→ catalog validation
```

Example:

```bash
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

## Important `--no-build` detail

`dotnet run --no-build` expects the required project to have already been built.

Therefore this order is correct:

```text
dotnet build
→ dotnet run --no-build
```

Running `dotnet run --no-build` before building may fail because the output assembly does not exist.

## Minimal shell CI script

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

`set -euo pipefail` means:

```text
-e
→ stop after a failing command
```

```text
-u
→ fail on undefined variables
```

```text
-o pipefail
→ fail when any command in a pipeline fails
```

## Capturing validation output

For CI logs:

```bash
dotnet run \
  --no-build \
  --project Toolroom/WhenItFails/Setter \
  -- validate . \
  2>&1 | tee when-it-fails-validation.log
```

Because pipelines can hide failures without `pipefail`, use:

```bash
set -o pipefail
```

before the command.

Then the pipeline returns the Setter failure correctly.

## Separate validation job

Catalog validation may be placed in a dedicated CI job.

Advantages:

* failure reason is immediately visible,
* catalog authors can identify the failing stage,
* validation can be rerun independently,
* release workflows can depend on it explicitly.

Suggested job name:

```text
WhenItFails catalog validation
```

## CI working directory

Setter resolves relative paths from the current working directory.

Before validation, CI may print:

```bash
pwd
```

and confirm:

```bash
test -d Jsons/WhenItFails
```

This prevents accidentally validating:

```text
<wrong-directory>/Jsons/WhenItFails
```

## CI path casing

Linux CI runners usually use case-sensitive filesystems.

These are different:

```text
Jsons/WhenItFails
jsons/WhenItFails
Jsons/Whenitfails
```

Use exact repository casing in:

* scripts,
* workflow files,
* documentation,
* project paths.

## CI SDK version

Setter targets:

```text
.NET 10
```

CI must install a compatible .NET SDK.

Useful diagnostic:

```bash
dotnet --info
```

A repository `global.json`, when present, should be honored by CI.

## CI package cache

Package caching may speed up restore, but correctness should not depend on it.

When investigating strange restore behavior:

```bash
dotnet nuget locals all --clear
dotnet restore
```

Do not clear caches in every normal CI run unless necessary.

## Tests versus workspace validation

These operations are complementary.

```text
dotnet test
```

checks implementation behavior.

```text
Setter validate
```

checks the actual repository workspace.

A passing test suite does not prove that project-local JSON catalogs are valid.

A valid workspace does not prove that Setter code behaves correctly.

Both should run.

## Documentation-only changes

Documentation changes should still run at least:

```bash
dotnet build
dotnet test
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

This catches accidental:

* file deletion,
* project-file damage,
* malformed paths,
* unrelated uncommitted changes,
* catalog edits included in the same commit.

## Targeted test runs

Run only Setter tests:

```bash
dotnet test \
  Toolroom/WhenItFails/Setter.Tests
```

Run one test class:

```bash
dotnet test \
  Toolroom/WhenItFails/Setter.Tests \
  --filter \
  FullyQualifiedName~WhenItFailsWorkspaceEditorTests
```

Run one specific test:

```bash
dotnet test \
  Toolroom/WhenItFails/Setter.Tests \
  --filter \
  FullyQualifiedName~SetErrorSeverityAsync_ShouldReturnInvalid_WhenSeverityIsUnsupported
```

## Verbose test diagnostics

Normal verbosity:

```bash
dotnet test \
  Toolroom/WhenItFails/Setter.Tests \
  --verbosity normal
```

More detailed output:

```bash
dotnet test \
  Toolroom/WhenItFails/Setter.Tests \
  --verbosity detailed
```

Use detailed verbosity only while diagnosing failures because CI logs can become large.

## Test result files

Generate a TRX result:

```bash
dotnet test \
  Toolroom/WhenItFails/Setter.Tests \
  --logger \
  "trx;LogFileName=setter-tests.trx"
```

The result is normally written under:

```text
TestResults/
```

CI may publish this file as a test artifact.

## Repeated test execution

To reveal timing or isolation problems:

```bash
for iteration in $(seq 1 10)
do
  echo "Run $iteration"

  dotnet test \
    Toolroom/WhenItFails/Setter.Tests \
    --no-restore \
    || exit 1
done
```

Repeated runs are useful for detecting:

* shared temporary state,
* backup filename collisions,
* cleanup issues,
* race conditions.

## Backup timestamp collisions

Backup names include milliseconds.

Tests should not assume that two rapid edits always produce distinct backups under all possible timing conditions unless the writer explicitly guarantees it.

Prefer tests that verify:

```text
at least one backup exists
```

for a single edit.

For multiple sequential edits, inspect writer guarantees before asserting an exact count.

## Filesystem portability

Setter tests should remain valid on:

* Linux,
* Windows,
* macOS where supported by .NET and dependencies.

Avoid tests that depend unnecessarily on:

* `/tmp` literal paths,
* Windows drive letters,
* specific path separators,
* case-insensitive filesystems,
* shell-only behavior.

C# tests should use:

```csharp
Path.GetTempPath()
Path.Combine(...)
```

instead of hardcoded platform paths.

Shell smoke tests may remain platform-specific and should be documented as such.

## Avoid testing console formatting too rigidly

Rich Spectre.Console output may vary with:

* terminal width,
* ANSI support,
* rendering environment,
* library version.

Tests should prefer stable values such as:

* exit codes,
* issue codes,
* saved JSON values,
* created files,
* response success state.

Avoid asserting entire rendered tables unless formatting itself is the feature under test.

## Stable test contracts

Good test contracts:

```text
response.IsSuccess
issue.Code
saved property value
backup existence
exit code
```

Fragile contracts:

```text
complete colored console output
exact whitespace layout
exact timestamp
temporary GUID
absolute temporary path
```

## Test naming

Current tests use descriptive names such as:

```text
SetErrorTitleAsync_ShouldChangeTitleAndCreateBackup
```

and:

```text
SetErrorSeverityAsync_ShouldReturnInvalid_WhenSeverityIsUnsupported
```

This pattern clearly describes:

```text
method
→ expected behavior
→ condition
```

Use the same style for future tests.

## Future test areas

Useful future coverage includes:

* direct `Jsons/WhenItFails` path editing,
* no-op edit behavior,
* malformed JSON handling,
* access-denied writes,
* failed backup creation,
* temporary-file cleanup,
* cancellation,
* concurrent writers,
* command exit codes,
* plain-output structure,
* workspace path resolution,
* initialization preservation,
* cross-catalog validation failures.

These are candidates, not claims about current coverage.

## Recommended pre-commit sequence

```bash
dotnet restore

dotnet build \
  --no-restore

dotnet test \
  --no-build

dotnet run \
  --no-build \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

git status --short
git diff --check
```

`git diff --check` detects problems such as:

* trailing whitespace,
* whitespace errors,
* conflict markers in changed lines.

## Recommended release sequence

```text
clean checkout
→ restore
→ build
→ test
→ validate catalogs
→ inspect package contents
→ publish
```

Do not rely only on a developer workstation state.

A clean environment catches:

* missing committed files,
* hidden generated dependencies,
* casing mistakes,
* undeclared package requirements.

## Testing checklist

Before merging Setter changes, confirm:

* solution builds,
* Setter test project passes,
* complete repository tests pass,
* project-local workspace validates,
* successful edits create backups,
* rejected edits preserve source files,
* rejected edits do not create backups,
* issue codes remain stable where intended,
* temporary workspaces are isolated,
* cleanup does not hide failures,
* command-line smoke test succeeds,
* CI uses a compatible .NET SDK,
* relative paths resolve from the expected directory.

## Related documentation

* [Getting started](../Getting-Started/en.md)
* [Commands](../Commands/en.md)
* [Workspace Paths and Initialization](../Workspace%20Paths%20and%20Initialization/en.md)
* [Validation](../Validation/en.md)
* [Safe Writes](../Safe%20Writes/en.md)
* [Troubleshooting](../Troubleshooting/en.md)

## Central principle

> Tests prove Setter behavior; validation proves the workspace data. A reliable build needs both.
