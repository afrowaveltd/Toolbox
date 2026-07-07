# Contributing to Setter

This guide explains how to contribute changes to WhenItFails Setter and its catalog documentation.

It is intended for contributors who modify:

- Setter commands,
- validation logic,
- catalog files,
- documentation,
- tests,
- examples,
- release or CI scripts.

## Contribution principle

A good contribution is:

```text
small
clear
tested
validated
documented
reviewable
```

The best contribution leaves the next maintainer with fewer questions than before.

## Before making a change

Start from a clean or understood working tree:

```bash
git status --short
```

Run the baseline checks:

```bash
dotnet build

dotnet test

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

If the baseline is already failing, fix or document that first.

Do not mix baseline failures with unrelated new work.

## Understand the area being changed

Before editing code or catalogs, read the focused documentation:

```text
commands
→ Commands, Command Quick Reference
```

```text
exit codes
→ Exit Codes and Automation
```

```text
catalog content
→ Catalog Files, Catalog Author Checklist
```

```text
profiles
→ Profiles
```

```text
safe writes
→ Safe Writes, Backups and Recovery
```

```text
tests
→ Testing and CI
```

```text
platform behavior
→ Windows and PowerShell, Linux and Bash
```

A change is easier to make safely when the current documented behavior is understood first.

## Keep changes focused

Prefer one focused change per commit.

Good examples:

```text
Add profile validation tests
```

```text
Document catalog file relationships
```

```text
Add set-message command coverage
```

```text
Fix workspace path resolution for direct package paths
```

Risky examples:

```text
Update everything
```

```text
Refactor and docs and catalog cleanup
```

```text
Many fixes
```

Large changes are sometimes necessary, but they need a clear plan.

## Contribution types

Common contribution types are:

- documentation-only,
- catalog-only,
- test-only,
- command behavior,
- validation behavior,
- safe-write behavior,
- workspace initialization,
- output rendering,
- CI or release infrastructure.

Each type has a different review focus.

## Documentation-only changes

Documentation-only changes should still be checked.

Run:

```bash
dotnet build

dotnet test

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

git diff --check
```

Also check:

- links point to existing files,
- command examples are current,
- Bash examples use backslashes,
- PowerShell examples use backticks,
- Markdown code fences are closed,
- README indexes are updated.

Documentation can break user workflows even when code is untouched.

## Catalog-only changes

For catalog-only changes, run:

```bash
dotnet build

dotnet test

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .

git diff -- Jsons/WhenItFails

git diff --check
```

If an error definition changed, inspect it:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001
```

Use the actual affected ID.

## Code changes

For code changes, run:

```bash
dotnet restore

dotnet build --no-restore

dotnet test --no-build

dotnet run \
  --no-build \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

git diff --check
```

Also run a disposable smoke test when the change affects command execution, workspace files, validation, or saving.

## Test changes

When changing tests, ask:

- Is the test checking behavior rather than formatting noise?
- Does the test use a disposable workspace?
- Does the test clean up after itself?
- Does the test assert useful issue codes?
- Does the test cover failure behavior?
- Does the test remain stable across platforms?

Avoid brittle tests that depend on rich terminal table layout unless layout is exactly what is being tested.

## Command behavior changes

When changing a command, verify:

- command name still works,
- aliases still work,
- missing arguments produce the documented issue and exit code,
- success returns exit code `0`,
- validation failure returns the expected exit code,
- help and documentation are updated,
- plain output behavior is unchanged or documented,
- tests cover the changed behavior.

Command behavior is user-facing API.

Treat it carefully.

## Exit-code changes

Exit codes are automation contracts.

Before changing an exit code, ask:

- Is the old behavior documented?
- Could scripts rely on it?
- Is the new behavior more correct?
- Are tests updated?
- Is the change mentioned in documentation?
- Is a compatibility note needed?

Do not change exit codes casually.

## Validation changes

When changing validation, check:

- valid default workspace still passes,
- invalid cases fail with useful issue codes,
- issue messages are actionable,
- issue paths point to useful locations,
- cross-file relationships are checked deliberately,
- tests include both success and failure cases,
- documentation explains new rules.

Validation should help users fix problems, not merely reject files.

## Safe-write changes

When changing save behavior, check:

- successful writes still create expected backups,
- failed validation does not create unnecessary backups,
- temporary files are handled safely,
- access-denied errors are reported clearly,
- I/O errors are reported clearly,
- cancellation behavior is safe,
- tests cover successful and rejected writes,
- backup filenames remain predictable or documentation is updated.

Safe-write behavior protects user data.

Be conservative.

## Output rendering changes

When changing rich output, check:

- information is still complete,
- tables remain readable,
- narrow terminals are acceptable,
- plain output still works where supported,
- scripts should not depend on rich output,
- documentation screenshots or examples remain accurate if present.

When changing plain output, be more careful.

Plain output may be used by scripts even though it is still presentation-oriented.

## Workspace initialization changes

When changing `init`, verify:

- project-root behavior still works,
- existing files are not overwritten unexpectedly,
- partial workspaces are handled intentionally,
- direct package path behavior is documented,
- disposable workspace smoke tests pass,
- Windows and Linux path behavior is sane.

Initialization is often the first user experience.

It should be predictable.

## Profile changes

When changing profile logic or default profiles, check:

- `summary` shows expected profiles,
- `errors --profile <name>` works,
- unknown profiles fail correctly,
- production mappings remain safe,
- development mappings are not copied into production,
- current Setter limitations are documented,
- runtime behavior and Setter browsing behavior are not confused.

Profiles are policy-like.

Review them as policy, not only as data.

## Documentation style

Documentation should be:

- practical,
- direct,
- implementation-grounded,
- honest about current limitations,
- clear about future possibilities,
- careful with platform differences,
- copy-paste friendly.

Prefer:

```text
what the command does
what it accepts
what it returns
what can go wrong
how to test it
```

Avoid vague claims such as:

```text
This always works.
```

or:

```text
This is fully automatic.
```

unless the source and tests really support that.

## Future features in documentation

Future ideas are allowed, but mark them clearly.

Good:

```text
Possible future improvements include...
```

```text
This is a future possibility, not current behavior.
```

Avoid writing future behavior as if it already exists.

This is especially important for:

- restore commands,
- JSON output,
- profile editing commands,
- generated schemas,
- export/import,
- runtime-equivalent filtering.

## Code style

Prefer code that is:

- explicit,
- readable,
- testable,
- boring in the best way,
- consistent with existing project style.

Avoid clever code in command-line tooling.

A future maintainer should be able to debug it at 2 AM without solving a puzzle box.

## Naming

Use names that are:

- clear,
- stable,
- consistent with existing catalog vocabulary,
- easy to search,
- not overly abbreviated.

For public concepts, prefer names that match documentation.

If the command says:

```text
documentation key
```

the code and tests should not randomly call it:

```text
doc slug thing
```

## Error and issue wording

When adding issue messages, make them:

- specific,
- actionable,
- neutral,
- safe,
- free of secrets,
- consistent with existing terminology.

Good:

```text
The validate command requires a project root or Jsons/WhenItFails directory path.
```

Weak:

```text
Bad input.
```

Good issue wording saves support time.

## Tests first where practical

When changing behavior, prefer adding or updating tests before or alongside implementation.

Good flow:

```text
add failing test
→ implement change
→ make test pass
→ update docs
→ run full checks
```

For documentation-only changes, this may not apply directly, but command examples should still be checked mentally or manually.

## Disposable workspaces

Use disposable workspaces for tests and experiments.

Linux:

```bash
test_root="$(mktemp -d /tmp/when-it-fails-contribution-XXXXXXXXXX)"
trap 'rm -rf "$test_root"' EXIT
```

PowerShell:

```powershell
$testRoot = Join-Path `
    $env:TEMP `
    ("when-it-fails-contribution-" + [Guid]::NewGuid().ToString("N"))
```

Do not damage the real catalog to test failure cases.

## Negative tests

For negative tests:

- mutate one thing,
- verify the mutation happened,
- run validation,
- assert the expected exit code,
- assert the useful issue code,
- clean up.

Do not merely check that “something failed.”

Check that the right thing failed.

## Platform behavior

When changing anything related to paths, processes, files, or shell examples, consider both:

```text
Windows
Linux
```

Important differences:

```text
PowerShell uses $LASTEXITCODE
Bash uses $?
```

```text
PowerShell line continuation uses backtick
Bash line continuation uses backslash
```

```text
Windows paths are often case-insensitive
Linux paths are usually case-sensitive
```

```text
Windows file locks are often more visible
Linux ownership and permissions often matter more
```

## Cross-platform command examples

If a document includes both Bash and PowerShell examples, keep them equivalent.

Bash:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

PowerShell:

```powershell
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .
```

Do not paste Bash syntax into a PowerShell-only section.

Do not paste PowerShell syntax into a Bash-only section.

## Git hygiene

Before committing:

```bash
git status --short
git diff --stat
git diff --check
```

Check for accidental files:

- backups,
- temporary files,
- IDE folders,
- build outputs,
- user-specific config,
- logs,
- test scratch files.

Keep commits clean.

## Backup files

Setter backups may look like:

```text
errors.en.20260627-095820-480.bak.json
```

Do not commit them unless project policy explicitly says to.

Most local backups are recovery artifacts, not source files.

## Temporary files

Temporary files may look like:

```text
.errors.en.json.<guid>.tmp
```

They should not be committed.

If a temporary file remains after a failed operation, inspect it before deleting if recovery may be needed.

## Secrets

Never commit real:

- passwords,
- tokens,
- API keys,
- connection strings,
- private hostnames,
- customer identifiers,
- personal data,
- production stack traces,
- raw incident logs.

Use placeholders in examples:

```text
api.example.test
```

```text
<connection-string>
```

```text
<profile-name>
```

## Pull request description

A useful PR description includes:

```text
what changed
why it changed
how it was tested
compatibility notes
documentation notes
```

Example:

```markdown
## Summary

Adds a catalog author checklist for WhenItFails Setter documentation.

## Testing

- dotnet build
- dotnet test
- dotnet run --project Toolroom/WhenItFails/Setter -- validate .
- git diff --check

## Compatibility

Documentation only. No command or catalog schema changes.
```

## Review focus by change type

Documentation PR:

- links,
- command examples,
- platform syntax,
- current behavior,
- future behavior clearly marked.

Catalog PR:

- validation,
- stable IDs,
- numeric ranges,
- profile policy,
- safe wording,
- no secrets.

Code PR:

- tests,
- exit codes,
- command behavior,
- error handling,
- compatibility,
- docs.

Test PR:

- meaningful assertions,
- stable setup,
- cleanup,
- cross-platform assumptions.

## Reviewer checklist

A reviewer should ask:

- Is the change understandable?
- Is the change focused?
- Are tests sufficient?
- Does validation pass?
- Are docs updated?
- Are exit codes preserved?
- Are stable catalog identifiers preserved?
- Are platform differences handled?
- Is the diff free of unrelated churn?
- Are secrets absent?

Review should protect future maintainers, not only current functionality.

## When to update documentation

Update documentation when changing:

- command names,
- command arguments,
- command options,
- exit codes,
- validation rules,
- catalog file shape,
- default catalogs,
- profile behavior,
- backup behavior,
- safe-write behavior,
- platform-specific instructions,
- CI requirements.

If users can observe the change, documentation probably needs an update.

## When to update tests

Update or add tests when changing:

- command parsing,
- workspace resolution,
- validation rules,
- edit operations,
- severity handling,
- lookup behavior,
- safe writes,
- backup creation,
- issue codes,
- exit-code behavior,
- summary models,
- profile filtering.

If behavior changed and no test changed, pause and ask why.

## When to update catalog docs

Update catalog docs when changing:

- error fields,
- category fields,
- code-group fields,
- owner fields,
- profile fields,
- default mappings,
- stable naming conventions,
- validation requirements,
- profile behavior.

Catalog docs are the contract for authors.

## When to add a new doc page

Add a new focused page when:

- a topic becomes too large inside another page,
- a workflow needs step-by-step guidance,
- a role needs a dedicated checklist,
- platform behavior differs,
- users need a quick reference,
- the same explanation appears in several places.

Avoid creating tiny pages that only repeat another guide.

## Keeping README indexes updated

When adding a new documentation page, update both relevant README files.

Add a link with URL-encoded spaces:

```markdown
- [Contributing to Setter](Docs/Contributing%20to%20Setter/en.md)
```

Then check that the path matches the actual file:

```text
Docs/Contributing to Setter/en.md
```

## Commit message guidance

Use imperative or concise noun-phrase style.

Good:

```text
Add Setter contributing guide
```

```text
Validate profile category references
```

```text
Document catalog backup recovery
```

Weak:

```text
stuff
```

```text
updates
```

```text
fix
```

```text
work in progress
```

A good commit message helps history stay useful.

## Final local check

Before saying the change is done:

```bash
dotnet build

dotnet test

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

git diff --check

git status --short
```

PowerShell equivalent:

```powershell
dotnet build

dotnet test

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

git diff --check

git status --short
```

## If checks fail

Do not paper over failures.

Use this order:

1. Read the first clear error.
2. Identify whether it is code, catalog, documentation, environment, or test setup.
3. Fix the smallest cause.
4. Rerun the relevant focused check.
5. Rerun the full check.
6. Update docs or tests if behavior changed.

## If behavior and docs disagree

Source code and tests describe current behavior.

Documentation describes intended user understanding.

When they disagree, resolve the disagreement.

Do not leave both versions in the repository.

## If a change is intentionally breaking

Document:

- what changed,
- why it changed,
- what breaks,
- how to migrate,
- which version contains the change,
- whether old behavior is still available.

Breaking changes should be rare and explicit.

## Related documentation

- [Documentation Map](../Documentation%20Map/en.md)
- [Glossary](../Glossary/en.md)
- [Command Quick Reference](../Command%20Quick%20Reference/en.md)
- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Release Checklist](../Release%20Checklist/en.md)
- [Testing and CI](../Testing%20and%20CI/en.md)
- [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
- [Windows and PowerShell](../Windows%20and%20PowerShell/en.md)
- [Linux and Bash](../Linux%20and%20Bash/en.md)
- [Troubleshooting](../Troubleshooting/en.md)

## Central principle

> Contributing to Setter means changing behavior, catalogs, or documentation in a way that the next person can verify, understand, and safely maintain.
