# Troubleshooting

This guide describes common WhenItFails Setter problems, their likely causes, and safe ways to diagnose them.

The recommended troubleshooting order is:

```text
read the first issue
→ check the reported path
→ confirm the command input
→ inspect the catalog
→ fix one logical problem
→ validate again
```

Avoid making several unrelated changes at once.

## Start with help

When command syntax is uncertain, run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- help
```

Setter also shows help when run without arguments:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter
```

## Confirm the exit code

After a command:

```bash
echo $?
```

Typical meaning:

```text
0
→ command succeeded
```

```text
1
→ command arguments or input were invalid
```

```text
2
→ loading, validation, lookup, editing, or saving failed
```

```text
3
→ unexpected application-level failure
```

Always capture the exit code immediately after the command.

This is important:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

validation_exit_code=$?

echo "Exit code: $validation_exit_code"
```

Do not run another command before saving `$?`, because every shell command replaces it.

## Command returns exit code 0 unexpectedly

A negative test may appear to succeed when the test input was not actually changed.

Example:

```bash
sed -i \
  's/"DefaultSeverity": "Error"/"DefaultSeverity": "Fatal"/' \
  errors.en.json
```

The actual JSON property is:

```json
"defaultSeverity": "Error"
```

Linux text tools are case-sensitive.

The incorrect `sed` expression changes nothing, so validation correctly succeeds.

Always verify the mutation:

```bash
grep -n \
  '"defaultSeverity": "Fatal"' \
  errors.en.json
```

If `grep` prints nothing, the test file was not modified.

A reliable negative test:

```bash
sed -i \
  '0,/"defaultSeverity": "Error"/s//"defaultSeverity": "Fatal"/' \
  errors.en.json
```

Then verify:

```bash
grep -n \
  '"defaultSeverity": "Fatal"' \
  errors.en.json
```

## Wrong workspace path

Setter accepts either:

```text
project root
```

or:

```text
Jsons/WhenItFails directory
```

Valid examples:

```bash
when-it-fails-setter validate .
```

```bash
when-it-fails-setter validate ./MyProject
```

```bash
when-it-fails-setter validate ./MyProject/Jsons/WhenItFails
```

A common mistake is passing a path that contains neither a project workspace nor the catalog directory.

Check:

```bash
find . \
  -maxdepth 3 \
  -path '*/Jsons/WhenItFails' \
  -type d
```

## Required catalog file not found

The workspace requires:

```text
errors.en.json
categories.en.json
code-groups.en.json
owners.en.json
profiles.json
```

List the directory:

```bash
find Jsons/WhenItFails \
  -maxdepth 1 \
  -type f \
  -printf '%f\n' \
  | sort
```

To create missing files:

```bash
when-it-fails-setter init .
```

Important:

```text
init creates only missing files
```

It does not replace invalid existing files.

## `init` did not repair an invalid file

This is expected.

Setter initialization follows:

```text
missing file
→ create from template
```

```text
existing file
→ preserve
```

Even an invalid existing catalog remains untouched.

Run:

```bash
when-it-fails-setter validate .
```

Then repair the reported file explicitly.

## Malformed JSON

Common causes:

* missing comma,
* extra comma in an unsupported position,
* missing quote,
* missing closing brace,
* missing closing bracket,
* accidental pasted text,
* duplicate structural fragments.

Inspect the file with:

```bash
python -m json.tool \
  Jsons/WhenItFails/errors.en.json
```

This strict parser may reject comments or trailing commas even when the WhenItFails loader accepts them, but it is useful for ordinary JSON syntax checking.

Another option:

```bash
jq empty \
  Jsons/WhenItFails/errors.en.json
```

Again, `jq` expects strict JSON.

## Valid JSON but invalid catalog

Syntactically valid JSON can still violate catalog rules.

Examples:

* duplicate error ID,
* duplicate numeric code,
* unsupported severity,
* unknown owner,
* nonexistent category,
* code outside its group range,
* profile referencing a missing error.

Run:

```bash
when-it-fails-setter validate .
```

Read:

```text
issue code
path
message
```

The issue code is usually the most stable diagnostic clue.

## Unsupported severity

Supported values are:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Invalid example:

```json
"defaultSeverity": "Fatal"
```

Expected validation issue:

```text
UnknownDefaultSeverity
```

Use:

```json
"defaultSeverity": "Critical"
```

when the failure truly represents a critical condition.

Do not replace values mechanically without reviewing their meaning.

## Error definition not found

Setter can locate an error by:

```text
Id
Code
Name
```

Examples:

```text
AFW_NET_0001
600001
NETWORKUNAVAILABLE
```

Search first:

```bash
when-it-fails-setter errors . \
  --search network
```

Then inspect the result:

```bash
when-it-fails-setter details . AFW_NET_0001
```

Common causes:

* typo,
* wrong numeric code,
* using a title instead of a name,
* searching the wrong workspace,
* error was renamed or removed.

## Unknown profile

Example:

```bash
when-it-fails-setter errors . \
  --profile WEB
```

If `WEB` does not exist, Setter returns:

```text
UnknownProfileFilter
```

Inspect available profiles:

```bash
when-it-fails-setter summary .
```

Profile matching uses:

* profile name,
* display name.

Matching is case-insensitive.

## Filter returns no results

This is not necessarily an error.

Example:

```bash
when-it-fails-setter errors . \
  --category DOES_NOT_EXIST
```

may return:

```text
Errors: 0 shown from 37
```

with exit code:

```text
0
```

An empty valid result means the filter matched nothing.

It is different from an unknown profile, which is treated as invalid input.

## Full-text search does not find expected text

The `--search` filter checks fields such as:

* ID,
* name,
* title,
* message,
* developer hint,
* documentation key,
* numeric code,
* owner,
* code group,
* primary category,
* categories,
* subcategories,
* tags.

Search is case-insensitive.

Possible reasons for no match:

* searching a value stored in another catalog,
* searching formatting not stored in the normalized value,
* typo,
* wrong workspace,
* expectation that metadata keys are searched.

Use:

```bash
grep -Rni \
  'searched text' \
  Jsons/WhenItFails
```

to confirm where the text actually exists.

## `set-title` or another edit reports missing arguments

Example failure code:

```text
MissingSetTitleArguments
```

Expected syntax:

```text
set-title <path> <id|code|name> <title>
```

Correct:

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network is not available"
```

Incorrect:

```bash
when-it-fails-setter set-title . AFW_NET_0001
```

The new value is required.

## Shell changed the text argument

Unquoted shell input can be interpreted unexpectedly.

Characters such as:

```text
*
?
$
>
<
&
|
(
)
```

may have shell meaning.

Prefer:

```bash
when-it-fails-setter set-message . \
  AFW_NET_0001 \
  "The request failed while connecting to the remote service."
```

rather than:

```bash
when-it-fails-setter set-message . \
  AFW_NET_0001 \
  The request failed > remote service
```

The second command redirects output instead of storing the intended message.

## Empty value rejected

Setter rejects empty or whitespace-only values for current edit commands.

Possible issue codes:

```text
TitleIsEmpty
MessageIsEmpty
DeveloperHintIsEmpty
DocumentationKeyIsEmpty
SeverityIsEmpty
```

The current commands assign non-empty values.

They do not provide general clear/remove behavior for optional fields.

Do not use whitespace as an attempt to clear a field.

## `Fatal` severity rejected

This is expected.

The supported highest severity is:

```text
Critical
```

Use:

```bash
when-it-fails-setter set-severity . \
  AFW_NET_0001 \
  Critical
```

not:

```bash
when-it-fails-setter set-severity . \
  AFW_NET_0001 \
  Fatal
```

## Edit created a larger Git diff than expected

The editor:

```text
loads
→ normalizes
→ serializes as indented JSON
```

Therefore a successful edit may also normalize:

* whitespace,
* property formatting,
* collection formatting,
* canonical symbolic values.

Inspect:

```bash
git diff -- \
  Jsons/WhenItFails/errors.en.json
```

Distinguish:

```text
semantic changes
```

from:

```text
formatting or normalization changes
```

Do not commit unexplained large rewrites.

## Edit created a backup even though the value did not change

The current editor does not detect every no-op update before saving.

Example:

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network unavailable"
```

may still save when the current title is already identical.

This may create:

```text
errors.en.<timestamp>.bak.json
```

Inspect before editing:

```bash
when-it-fails-setter details . AFW_NET_0001
```

A future optimization may skip no-op writes.

## Backup file not found

A backup is created only when:

* the target file already exists,
* validation succeeds,
* serialization succeeds,
* backup creation succeeds,
* the write reaches the backup step.

No backup is expected after:

* missing arguments,
* error not found,
* empty value,
* unsupported severity,
* edited catalog validation failure,
* loading failure.

Find backups:

```bash
find Jsons/WhenItFails \
  -maxdepth 1 \
  -type f \
  -name '*.bak.json' \
  -printf '%f\n' \
  | sort
```

## Backup naming confusion

Current backup pattern:

```text
<name>.<UTC timestamp>.bak<extension>
```

Example:

```text
errors.en.20260627-095820-480.bak.json
```

Not:

```text
errors.en.json.20260627_095820.bak
```

Timestamp format:

```text
yyyyMMdd-HHmmss-fff
```

## Temporary file remains after failure

Temporary files use:

```text
.<target filename>.<GUID>.tmp
```

Example:

```text
.errors.en.json.79e0917a3a6d48e1ad61b28a50f91976.tmp
```

A temporary file may remain after:

* cancellation,
* serialization failure,
* I/O failure,
* interrupted process,
* failed final move.

Find them:

```bash
find Jsons/WhenItFails \
  -maxdepth 1 \
  -type f \
  -name '.*.tmp' \
  -printf '%f\n'
```

Before deleting one:

1. Confirm no Setter process is running.
2. Inspect the active catalog.
3. Inspect the backup.
4. Confirm the temporary file is stale.
5. Remove it manually.

## Access denied

Possible issue code:

```text
AccessDenied
```

Check file and directory permissions:

```bash
ls -ld Jsons/WhenItFails
ls -l Jsons/WhenItFails
```

Check ownership:

```bash
stat Jsons/WhenItFails/errors.en.json
```

Test write permission carefully:

```bash
test -w Jsons/WhenItFails/errors.en.json \
  && echo writable \
  || echo not-writable
```

Also check directory write permission because Setter creates:

* temporary files,
* backups.

```bash
test -w Jsons/WhenItFails \
  && echo directory-writable \
  || echo directory-not-writable
```

## File is writable but save still fails

The target file may be writable while the directory is not.

Setter needs directory permissions to:

```text
create temporary file
create backup
move temporary file over target
```

Therefore both matter:

```bash
test -w Jsons/WhenItFails/errors.en.json
test -w Jsons/WhenItFails
```

Possible additional causes:

* filesystem mounted read-only,
* file lock,
* exhausted disk space,
* exhausted inodes,
* network mount interruption,
* security policy.

## Check disk space

```bash
df -h .
```

Check inode availability:

```bash
df -i .
```

A filesystem may have free bytes but no free inodes.

## Input/output error

Possible issue code:

```text
InputOutputError
```

Possible causes:

* disk full,
* disconnected mount,
* filesystem problem,
* hardware failure,
* file lock,
* failed backup creation,
* failed move,
* network share interruption.

Inspect:

```bash
dmesg --level=err,warn | tail -n 50
```

On systems using systemd:

```bash
journalctl -p warning -n 50
```

Use elevated access only when needed.

## Concurrent edits overwrite each other

Setter does not currently coordinate multiple writers.

Possible race:

```text
process A loads catalog
process B loads catalog
process A saves
process B saves older copy
```

Recommended rule:

```text
one writer per workspace
```

Before editing, check for running Setter processes:

```bash
pgrep -af \
  'WhenItFails.*Setter'
```

Also avoid editing the same file manually while a Setter write is running.

## Validation succeeds but application uses different data

Possible reasons:

* application points to another workspace,
* runtime uses a previous context,
* flexible initialization activated bundled fallback,
* application was not restarted,
* path configuration differs,
* deployment did not include updated catalogs.

Setter validation confirms the supplied workspace.

It does not confirm which runtime context a separate application currently uses.

Check the runtime:

```text
GetCurrentContext()
GetStatus()
```

and inspect its source path and state.

## Setter validates one workspace but you edited another

This is easy when several projects contain:

```text
Jsons/WhenItFails
```

Print the current directory:

```bash
pwd
```

Resolve the target:

```bash
realpath Jsons/WhenItFails
```

For a supplied path:

```bash
realpath ./MyProject/Jsons/WhenItFails
```

Use absolute paths temporarily when diagnosing ambiguity.

## Rich output looks broken

Possible causes:

* terminal width too small,
* terminal lacks expected capabilities,
* output redirected while rich mode is active,
* ANSI handling disabled,
* unusual font or encoding.

Use:

```text
--plain
```

where supported:

```bash
when-it-fails-setter errors . --plain
```

```bash
when-it-fails-setter details . AFW_NET_0001 --plain
```

## Plain output is not strict TSV

`errors --plain` includes metadata before the tabular section.

Example:

```text
WhenItFails Error Definitions
Workspace: ...
Errors: ...

Code	Id	Name	Owner	Group	Category	Severity	Title
```

To extract the TSV section:

```bash
when-it-fails-setter errors . --plain |
awk '
  /^Code\tId\tName\tOwner\tGroup\tCategory\tSeverity\tTitle$/ {
    output = 1
  }

  output {
    print
  }
'
```

## ANSI output appears in failure logs

Successful plain list and detail views use ordinary console output.

However, validation failures may still use the shared validation renderer.

Therefore:

```text
--plain
```

does not currently guarantee that every possible failure path is a strict ANSI-free machine format.

Use exit codes as the primary automation signal.

## `summary`, `errors`, or `details` refuses to display data

These commands validate the workspace first.

When validation fails, the normal view is not shown.

Run:

```bash
when-it-fails-setter validate .
```

Repair the workspace, then retry the read command.

This prevents invalid data from being presented as authoritative.

## Cross-validation errors do not appear

Cross-validation runs only when all five catalogs independently:

* loaded successfully,
* normalized successfully,
* passed document validation.

If one catalog is invalid, cross-validation is skipped.

Fix the individual catalog errors first.

Then rerun validation to reveal any remaining relationship problems.

## One repair reveals new errors

This can be expected.

Example:

```text
owner catalog invalid
→ cross-validation skipped
```

After repairing owners:

```text
cross-validation runs
→ unknown owner references now reported
```

The later errors were not created by the repair.

They became checkable only after prerequisite catalogs were valid.

## JSON key casing

JSON property matching may be case-insensitive during deserialization, but text tools are not.

These differ to `grep`, `sed`, Linux paths, and Git:

```text
defaultSeverity
DefaultSeverity
DEFAULTSEVERITY
```

When writing test scripts, match the exact file text.

Verify every automated mutation before trusting its result.

## Filename casing

Linux filesystems usually distinguish:

```text
errors.en.json
Errors.en.json
ERRORS.EN.JSON
```

Setter expects the configured path.

A file with incorrect casing may appear missing even though a similarly named file exists.

List exact names:

```bash
find Jsons/WhenItFails \
  -maxdepth 1 \
  -printf '%f\n' \
  | sort
```

## Works on Windows but fails on Linux

Common causes:

* filename casing,
* directory casing,
* path separator assumptions,
* missing execute permission,
* different file ownership,
* hidden reliance on case-insensitive lookup,
* line-ending-sensitive shell scripts,
* unavailable Windows-only paths,
* different current directory.

Check Git for casing-only changes:

```bash
git status
git ls-files | sort
```

Use exact project casing in code and documentation.

## Works locally but fails in CI

Check:

* CI working directory,
* checked-out files,
* case-sensitive paths,
* .gitignore rules,
* generated files not committed,
* SDK version,
* permissions,
* environment variables,
* command exit code,
* whether the workspace path exists.

Useful diagnostics:

```bash
pwd
dotnet --info
find . -path '*/Jsons/WhenItFails' -type d
git status --short
```

## Build succeeds but Setter command does not run

Verify the project path:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- help
```

If needed, use the exact project file:

```bash
dotnet run \
  --project \
  Toolroom/WhenItFails/Setter/Afrowave.Toolbox.Toolroom.WhenItFails.Setter.csproj \
  -- help
```

Check:

```bash
dotnet build \
  Toolroom/WhenItFails/Setter
```

## Wrong .NET SDK

Setter targets:

```text
.NET 10
```

Check:

```bash
dotnet --version
dotnet --list-sdks
```

If the repository contains `global.json`, also inspect:

```bash
cat global.json
```

The selected SDK must satisfy the project requirements.

## Package restore problem

Run:

```bash
dotnet restore
```

Then:

```bash
dotnet build --no-restore
```

Check package status:

```bash
dotnet list package
```

For outdated package investigation:

```bash
dotnet list package --outdated
```

Do not update packages merely to fix an unrelated workspace validation problem.

## Tests fail after documentation-only change

Documentation changes should not normally affect build behavior.

First inspect:

```bash
git status
git diff
```

Possible causes:

* another uncommitted code change,
* accidental file deletion,
* malformed project file,
* generated file included unexpectedly,
* test depending on repository content,
* changed line endings or encoding.

Run a targeted test project if needed:

```bash
dotnet test \
  Toolroom/WhenItFails/Setter.Tests
```

Then run the complete solution tests.

## Markdown link does not work

Paths containing spaces must be URL-encoded in links.

Correct:

```markdown
[Safe Writes](Docs/Safe%20Writes/en.md)
```

Incorrect:

```markdown
[Safe Writes](Docs/Safe Writes/en.md)
```

Check links for:

```text
%20
```

in folder names containing spaces.

Linux path casing must also match exactly.

## Markdown content looks corrupted after pasting

Some visual Markdown editors may reinterpret pasted blocks.

A reliable workflow is:

```text
remove old file
→ create clean file
→ paste plain Markdown
→ save
→ inspect Git diff
```

Example:

```bash
rm \
  "Toolroom/WhenItFails/Setter/Docs/Troubleshooting/en.md"

nano \
  "Toolroom/WhenItFails/Setter/Docs/Troubleshooting/en.md"
```

Then inspect:

```bash
git diff -- \
  "Toolroom/WhenItFails/Setter/Docs/Troubleshooting/en.md"
```

## Unexpected UTF-8 or BOM behavior

Inspect encoding:

```bash
file \
  Toolroom/WhenItFails/Setter/README.md
```

Inspect first bytes:

```bash
xxd -l 4 \
  Toolroom/WhenItFails/Setter/README.md
```

A UTF-8 BOM appears as:

```text
ef bb bf
```

Prefer consistent UTF-8 handling across documentation and JSON files.

Do not change encoding across many files without a deliberate repository-wide decision.

## Git does not show a casing-only rename

On a case-insensitive filesystem, Git may need an intermediate name.

Example:

```bash
git mv \
  "Docs/validation" \
  "Docs/validation.tmp"

git mv \
  "Docs/validation.tmp" \
  "Docs/Validation"
```

On Linux this is usually less troublesome because casing changes are visible to the filesystem.

## Restore from backup

Setter currently has no dedicated restore command.

Recommended process:

```text
stop writers
→ identify backup
→ compare backup and active file
→ preserve current active file
→ restore selected backup
→ validate
→ review Git diff
```

Example:

```bash
cp \
  Jsons/WhenItFails/errors.en.20260627-095820-480.bak.json \
  Jsons/WhenItFails/errors.en.json
```

Then:

```bash
when-it-fails-setter validate .
```

## Do not restore blindly

The newest backup is not automatically the correct backup.

Inspect:

```bash
diff -u \
  Jsons/WhenItFails/errors.en.20260627-095820-480.bak.json \
  Jsons/WhenItFails/errors.en.json
```

Choose based on content, not only timestamp.

## Clean up test workspace

Temporary tests should use a separate directory.

Example:

```bash
rm -rf /tmp/when-it-fails-test

cp -a \
  Jsons/WhenItFails \
  /tmp/when-it-fails-test
```

After testing:

```bash
rm -rf /tmp/when-it-fails-test
```

Always verify the path before using recursive deletion.

A safer check:

```bash
test \
  "/tmp/when-it-fails-test" = \
  "$(realpath -m /tmp/when-it-fails-test)" \
  && rm -rf /tmp/when-it-fails-test
```

## Minimal diagnostic bundle

When reporting a Setter problem, include:

```text
command used
exit code
Setter output
workspace path form
dotnet --info
relevant issue codes
small redacted catalog fragment
```

Useful commands:

```bash
dotnet --info

pwd

git status --short

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

echo "Exit code: $?"
```

Do not include secrets, credentials, tokens, or private customer data.

## Recommended troubleshooting checklist

1. Confirm the exact command.
2. Capture the immediate exit code.
3. Confirm the workspace path.
4. Confirm required files exist.
5. Verify the test mutation actually happened.
6. Run complete validation.
7. Read the first stable issue code.
8. Inspect the reported file and path.
9. Fix one logical cause.
10. Validate again.
11. Review the Git diff.
12. Check permissions and free space for write failures.
13. Inspect backups and temporary files after interrupted writes.
14. Avoid concurrent writers.
15. Confirm runtime and Setter use the same workspace.

## Related documentation

* [Getting started](../Getting-Started/en.md)
* [Command reference](../Commands/en.md)
* [Validation](../Validation/en.md)
* [Editing error fields](../Editing%20Error%20Fields/en.md)
* [Plain output](../Plain%20Output/en.md)
* [Safe writes](../Safe%20Writes/en.md)

## Central principle

> Confirm what actually happened before fixing what was expected to happen.
