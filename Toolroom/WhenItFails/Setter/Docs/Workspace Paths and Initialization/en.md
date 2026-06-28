# Workspace paths and initialization

WhenItFails Setter works with project-local JSON catalogs stored in:

```text
Jsons/
└── WhenItFails/
    ├── errors.en.json
    ├── categories.en.json
    ├── code-groups.en.json
    ├── owners.en.json
    └── profiles.json
```

The tool accepts two path forms for most commands:

```text
project root
```

or:

```text
direct Jsons/WhenItFails directory
```

Initialization is slightly different:

```text
init expects a project root
```

This distinction is intentional.

## Project-root path

A project-root path points to the directory under which the `Jsons` directory should exist.

Example:

```text
/home/user/projects/MyApplication
```

Setter resolves this to:

```text
/home/user/projects/MyApplication/Jsons/WhenItFails
```

Example command:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate /home/user/projects/MyApplication
```

## Direct package-directory path

Most read, validation, summary, and edit commands also accept the package directory directly.

Example:

```text
/home/user/projects/MyApplication/Jsons/WhenItFails
```

Example:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate \
  /home/user/projects/MyApplication/Jsons/WhenItFails
```

Both forms resolve to the same catalog files.

## Commands accepting both path forms

The shared path resolver is used by commands such as:

```text
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

For these commands, either of the following is valid:

```bash
when-it-fails-setter validate .
```

```bash
when-it-fails-setter validate \
  ./Jsons/WhenItFails
```

## How Setter recognizes a package directory

A supplied path is treated as a direct package directory when either:

1. it contains at least one known catalog file, or
2. its directory name is `WhenItFails` and its parent is named `Jsons`.

Known catalog filenames are:

```text
errors.en.json
categories.en.json
code-groups.en.json
owners.en.json
profiles.json
```

This means a partially created workspace can still be recognized as the package directory when at least one expected catalog exists.

It also means an empty directory named:

```text
Jsons/WhenItFails
```

can be recognized by its path structure.

## Case handling during path recognition

The structural directory-name check is case-insensitive.

These names may therefore be recognized by the resolver:

```text
Jsons/WhenItFails
jsons/whenitfails
JSONS/WHENITFAILS
```

However, Linux filesystems usually treat these as different physical paths.

Recognition does not rename directories or correct their casing.

The recommended canonical path remains:

```text
Jsons/WhenItFails
```

Using consistent casing avoids problems with:

* Git,
* deployment,
* shell scripts,
* documentation links,
* cross-platform builds,
* manual file inspection.

## Default resolution behavior

When a supplied path does not look like a package directory, Setter treats it as a project root.

Conceptually:

```text
input path
→ does it look like Jsons/WhenItFails?
   → yes: use it directly
   → no: append Jsons/WhenItFails
```

Example:

```text
input:
./MyProject
```

resolves to:

```text
./MyProject/Jsons/WhenItFails
```

## Relative paths

Relative paths are converted to full paths internally.

Example:

```bash
when-it-fails-setter validate .
```

The `.` is resolved against the current working directory.

Always verify where the shell currently is:

```bash
pwd
```

A common mistake is running the correct command from the wrong directory.

## Absolute paths

Absolute paths are useful during troubleshooting.

Example:

```bash
when-it-fails-setter validate \
  /home/user/projects/Toolbox
```

Or directly:

```bash
when-it-fails-setter validate \
  /home/user/projects/Toolbox/Jsons/WhenItFails
```

Absolute paths reduce ambiguity when several repositories contain their own WhenItFails workspace.

## Display path

Setter creates a shorter display path for console output.

When the supplied path is the package directory itself, the displayed workspace may be:

```text
WhenItFails
```

When the supplied path is the project root, the displayed path is relative to that root, typically:

```text
Jsons/WhenItFails
```

The display path is intended for readability.

Internal loading still uses full resolved paths.

## Initialization command

Syntax:

```text
init <project-root>
```

Example:

```bash
when-it-fails-setter init .
```

From the Toolbox repository:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- init .
```

The initializer converts the supplied project-root path to a full path and configures:

```text
RootDirectory
→ <project-root>/Jsons
```

and:

```text
PackageDirectoryName
→ WhenItFails
```

The resulting workspace path is:

```text
<project-root>/Jsons/WhenItFails
```

## Important initialization difference

Unlike the other workspace commands, `init` does not use the shared path resolver.

It always treats the supplied argument as a project root.

Therefore this is correct:

```bash
when-it-fails-setter init ./MyProject
```

and creates or checks:

```text
./MyProject/Jsons/WhenItFails
```

But this command:

```bash
when-it-fails-setter init \
  ./MyProject/Jsons/WhenItFails
```

would treat that directory as the project root and target:

```text
./MyProject/Jsons/WhenItFails/Jsons/WhenItFails
```

Do not pass the package directory directly to `init`.

## What initialization creates

Initialization ensures that the workspace directory exists and creates missing catalog files from bundled templates.

The expected files are:

```text
errors.en.json
categories.en.json
code-groups.en.json
owners.en.json
profiles.json
```

The templates are provided through:

```text
DefaultJsonsTemplateProvider
```

and written through:

```text
JsonsBootstrapper
```

## Existing files are preserved

Initialization follows a non-destructive rule:

```text
missing file
→ create from bundled template
```

```text
existing file
→ leave unchanged
```

It does not overwrite:

* customized catalogs,
* invalid catalogs,
* partially edited catalogs,
* older project-local versions.

This is essential because project catalogs belong to the project, not to the package.

## Initialization is not repair

`init` does not:

* validate existing files,
* repair malformed JSON,
* normalize catalogs,
* replace invalid definitions,
* reset customized values,
* restore bundled defaults over project files.

Recommended sequence:

```bash
when-it-fails-setter init .
when-it-fails-setter validate .
```

Initialization creates what is missing.

Validation determines whether the complete result is correct.

## Re-running initialization

Initialization is designed to be safe to run repeatedly.

Example:

```bash
when-it-fails-setter init .
when-it-fails-setter init .
```

The second run should preserve all existing files.

It may still report that the workspace is already complete.

Re-running initialization is useful after:

* accidentally deleting one catalog,
* adding a newly required template in a future version,
* cloning a project with incomplete generated files,
* preparing a clean test workspace.

## Partial workspace

Suppose only these files exist:

```text
errors.en.json
categories.en.json
```

Running:

```bash
when-it-fails-setter init .
```

should create the missing:

```text
code-groups.en.json
owners.en.json
profiles.json
```

while preserving the two existing files.

Validation should then be run immediately.

## Invalid existing file

Suppose `errors.en.json` exists but contains malformed JSON.

Running:

```bash
when-it-fails-setter init .
```

does not replace it.

The file exists, so bootstrap preserves it.

Then:

```bash
when-it-fails-setter validate .
```

reports the loading or syntax failure.

This separation prevents silent data loss.

## Missing project directory

The initializer resolves the supplied path to a full project-root path.

Depending on the underlying bootstrap behavior and filesystem permissions, missing parent directories may be created as part of ensuring the workspace.

Before initializing an important location, confirm the path:

```bash
realpath -m ./MyProject
```

Then inspect afterward:

```bash
find ./MyProject/Jsons/WhenItFails \
  -maxdepth 1 \
  -type f \
  -printf '%f\n' \
  | sort
```

## Permission requirements

Initialization needs permission to create:

```text
Jsons
Jsons/WhenItFails
catalog files
```

Check the project root:

```bash
test -w ./MyProject \
  && echo writable \
  || echo not-writable
```

Also inspect ownership:

```bash
ls -ld ./MyProject
```

Possible failures include:

* access denied,
* read-only filesystem,
* unavailable mount,
* disk full,
* exhausted inodes,
* invalid path.

## Safe test workspace

A disposable initialization test:

```bash
rm -rf /tmp/when-it-fails-init-test

mkdir -p /tmp/when-it-fails-init-test

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- init /tmp/when-it-fails-init-test
```

Inspect:

```bash
find /tmp/when-it-fails-init-test \
  -maxdepth 3 \
  -type f \
  -printf '%P\n' \
  | sort
```

Validate:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate /tmp/when-it-fails-init-test
```

Clean up:

```bash
rm -rf /tmp/when-it-fails-init-test
```

## Preserve-existing-file test

Create a workspace:

```bash
rm -rf /tmp/when-it-fails-init-test

mkdir -p /tmp/when-it-fails-init-test

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- init /tmp/when-it-fails-init-test
```

Record a checksum:

```bash
sha256sum \
  /tmp/when-it-fails-init-test/Jsons/WhenItFails/errors.en.json
```

Run initialization again:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- init /tmp/when-it-fails-init-test
```

Check the checksum again:

```bash
sha256sum \
  /tmp/when-it-fails-init-test/Jsons/WhenItFails/errors.en.json
```

The checksum should remain unchanged.

## Missing-file recreation test

After initialization:

```bash
rm \
  /tmp/when-it-fails-init-test/Jsons/WhenItFails/profiles.json
```

Run initialization again:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- init /tmp/when-it-fails-init-test
```

Confirm:

```bash
test -f \
  /tmp/when-it-fails-init-test/Jsons/WhenItFails/profiles.json \
  && echo recreated
```

Then validate:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate /tmp/when-it-fails-init-test
```

## Common path mistake

Incorrect:

```bash
when-it-fails-setter init \
  ./MyProject/Jsons/WhenItFails
```

Likely target:

```text
./MyProject/Jsons/WhenItFails/Jsons/WhenItFails
```

Correct:

```bash
when-it-fails-setter init ./MyProject
```

For validation, both are valid:

```bash
when-it-fails-setter validate ./MyProject
```

```bash
when-it-fails-setter validate \
  ./MyProject/Jsons/WhenItFails
```

## Several workspaces in one repository

A repository may contain several applications:

```text
Repository/
├── AppOne/
│   └── Jsons/WhenItFails/
├── AppTwo/
│   └── Jsons/WhenItFails/
└── SharedTool/
    └── Jsons/WhenItFails/
```

Initialize each using its own project root:

```bash
when-it-fails-setter init ./AppOne
when-it-fails-setter init ./AppTwo
when-it-fails-setter init ./SharedTool
```

Validate each explicitly:

```bash
when-it-fails-setter validate ./AppOne
when-it-fails-setter validate ./AppTwo
when-it-fails-setter validate ./SharedTool
```

Do not assume the repository root is always the intended workspace root.

## Symbolic links

Paths are converted to full paths, but full-path normalization does not necessarily resolve every symbolic link to its final physical target.

When symbolic links are involved, inspect both:

```bash
realpath ./linked-project
```

and:

```bash
ls -ld ./linked-project
```

Be certain which physical workspace is being modified.

## Git workflow

After initialization:

```bash
git status --short
```

Review newly created files:

```bash
git diff --no-index \
  /dev/null \
  Jsons/WhenItFails/errors.en.json
```

Then validate:

```bash
when-it-fails-setter validate .
```

Commit only after reviewing the templates and adjusting project-specific values.

## Recommended first-run workflow

```text
choose project root
→ initialize missing files
→ inspect created workspace
→ validate
→ review catalogs
→ customize definitions
→ validate again
→ commit
```

Example:

```bash
when-it-fails-setter init .

find Jsons/WhenItFails \
  -maxdepth 1 \
  -type f \
  -printf '%f\n' \
  | sort

when-it-fails-setter validate .

git status --short
```

## Recommended daily workflow

Initialization is not usually needed for every edit.

Normal authoring:

```text
validate
→ inspect
→ edit
→ validate
→ review diff
```

Use `init` when:

* creating a new workspace,
* restoring a missing file,
* adopting newly introduced template files.

## Related documentation

* [Getting started](../Getting-Started/en.md)
* [Command reference](../Commands/en.md)
* [Validation](../Validation/en.md)
* [Troubleshooting](../Troubleshooting/en.md)
* [Overview](../Overview/en.md)

## Central principle

> Initialization creates only what is missing; path resolution determines where every later command looks.
