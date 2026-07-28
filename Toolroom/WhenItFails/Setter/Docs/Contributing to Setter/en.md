# Contributing to Setter

This guide defines the current contribution workflow for `Toolroom/WhenItFails/Setter`.

Setter is developed incrementally. Changes should remain small, tested, documented, and easy for the next maintainer to continue.

> GitHub `master` is the source of truth.

## Core workflow

Use this sequence for every contribution:

1. inspect the current repository state;
2. read `IMPLEMENTATION_STATUS.md`;
3. choose one narrowly defined change;
4. add or update the corresponding test immediately;
5. implement or document the change;
6. run the focused Setter suite;
7. run relevant catalog and documentation checks;
8. review the complete Git diff;
9. update `IMPLEMENTATION_STATUS.md`;
10. commit the finished logical step;
11. stop until the focused suite is green.

## One logical change per commit

One logical change per commit is the default rule.

Good scopes include:

- add one command behavior and its tests;
- correct one validation rule and its tests;
- refresh one documentation topic and its documentation-contract test;
- fix one output contract across rich, plain, and JSON surfaces;
- correct one safe-write or restore invariant;
- add one focused catalog capability.

Do not combine unrelated refactoring, formatting, documentation cleanup, catalog editing, and command behavior in one commit.

A narrow commit is easier to test, review, revert, and continue from.

## Start from an understood state

From the repository root:

```powershell
git status --short
git diff --check
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Do not hide pre-existing failures inside a new contribution.

Read:

```text
Toolroom/WhenItFails/Setter/IMPLEMENTATION_STATUS.md
```

The status file records the verified test state, completed documentation, known boundaries, working rules, and next recommended step.

## Tests belong to the same change

Add or update the corresponding test immediately.

Do not postpone tests until several unrelated changes have accumulated.

For behavior changes, prefer:

```text
write or update the focused test
→ implement the behavior
→ run the focused test
→ run the complete Setter suite
```

For documentation changes, add or update a documentation-contract test in:

```text
Toolroom/WhenItFails/Setter.Tests/Docs
```

A documentation-only change is still a real repository change and must remain test-protected.

## Focused verification gate

The minimum gate for Setter-only work is:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Run the repository-wide suite when changing shared libraries, public runtime contracts, project wiring, package behavior, or cross-project integrations:

```powershell
dotnet test
```

Do not continue while the focused Setter suite is red.

When a test fails, inspect the exact expected and actual values before changing implementation, documentation, or assertions.

## Catalog verification

When a contribution affects catalog data or catalog-writing behavior, run:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

When documentation keys or Markdown files are involved, also run:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
```

Use `check-doc-keys` to verify documentation-key quality and `check-doc-links` to verify local Markdown links.

Inspect affected definitions, references, profiles, mappings, backups, and restore behavior with the relevant Setter commands rather than relying only on raw JSON.

## Documentation structure

Setter documentation has two required layers.

The root project overview is:

```text
Toolroom/WhenItFails/Setter/README.md
```

Detailed localized documentation uses:

```text
Toolroom/WhenItFails/Setter/Docs/<topic>/en.md
```

Only `en.md` is authored manually at this stage. Other languages will be generated later.

Keep `README.md` and `Docs/<topic>/en.md` aligned with current behavior. Do not describe implemented capabilities as future work, and do not describe planned capabilities as already available.

## Implementation status is mandatory

Every contribution that changes current behavior, documentation coverage, known boundaries, verification status, or the next recommended step must update:

```text
Toolroom/WhenItFails/Setter/IMPLEMENTATION_STATUS.md
```

The update should record:

- the latest user-verified focused test result;
- the newly completed change;
- synchronized documentation topics;
- changed intentional boundaries, if any;
- the next concrete continuation point.

Do not store brittle historical values in tests unless the exact value is the contract being tested.

## Command changes

Treat command behavior as a public interface.

Verify:

- canonical command and aliases;
- argument validation;
- success behavior;
- expected domain or workspace failures;
- rich output;
- `--plain` output where supported;
- `--json` output where supported;
- exit codes;
- help and command-reference documentation.

Machine consumers must use JSON output and exit codes. They must not parse rich terminal rendering as a stable schema.

## Validation changes

Validation should identify the correct problem and help the author fix it.

Cover both valid and invalid cases. Assert useful issue codes, paths, and messages where they are part of the contract.

Avoid tests that merely prove that “something failed.” Prove that the intended rule failed for the intended reason.

## Safe-write and restore changes

For a successful write, tests should verify:

1. the response contract;
2. the persisted value after reloading;
3. expected timestamped backup creation;
4. successful post-write validation.

For rejected input or failed pre-write validation, verify:

1. failure response or issue code;
2. unchanged target file;
3. no backup creation.

For restore changes, verify explicit backup selection, replacement scope, complete-workspace validation, affected-contract inspection, and failure behavior.

Setter safe writes are single-file operations. They are not multi-file transactions or a multi-process locking system.

## Temporary workspaces

Tests that write catalogs must use isolated temporary workspaces.

Use one mutable workspace per test. Do not share a writable catalog fixture across parallel tests.

A test should not modify the repository’s real `Jsons/WhenItFails` workspace.

## Output tests

Test behavior rather than incidental decoration.

Rich layout tests may intentionally assert Spectre.Console rendering, but service and automation tests should prefer stable semantic content.

For JSON output, parse and assert the JSON structure. Do not compare JSON as an arbitrary whitespace-sensitive string unless formatting itself is the contract.

For plain output, assert meaningful human-readable content without depending on terminal borders or color control sequences.

## Cross-platform behavior

Setter supports Windows and Linux development workflows.

Consider:

- path separators and path casing;
- filesystem permissions;
- Windows file locking;
- shell quoting;
- Bash `$?` and PowerShell `$LASTEXITCODE`;
- line continuation differences;
- temporary-directory behavior.

Do not weaken a correct cross-platform contract merely to hide an environment-specific failure. Identify the actual platform assumption and fix or document it.

## Review the diff

Before committing:

```powershell
git status --short
git diff --check
git diff
git diff --cached
```

Confirm that:

- only intended files changed;
- generated `.bak.json` files are not staged;
- temporary diagnostics are removed;
- source and tests agree;
- documentation and implementation agree;
- `IMPLEMENTATION_STATUS.md` is current.

## Commit messages

Use concise, specific commit messages describing the logical result.

Good examples:

```text
Add profile mapping validation
Refresh Setter recovery guide
Fix plain validation failure output
```

Avoid messages such as:

```text
Changes
Various fixes
Update stuff
```

## Current implemented capabilities

Contributors should assume the current Setter already includes, among other features:

- error creation and removal;
- focused error editing;
- reference inspection;
- profile creation and editing;
- profile explanation;
- JSON automation output;
- documentation-key and Markdown-link checks;
- backup listing;
- explicit backup restoration.

Check the current command reference and implementation before labeling a capability as future work.

## Final contribution checklist

Before the contribution is considered complete:

- [ ] the change has one logical purpose;
- [ ] its test was added or updated immediately;
- [ ] the focused Setter suite is green;
- [ ] broader tests ran when shared contracts changed;
- [ ] catalog validation ran when relevant;
- [ ] documentation checks ran when relevant;
- [ ] output and exit-code contracts were reviewed;
- [ ] persistence and backup behavior were reviewed for writes;
- [ ] `README.md` and `Docs/<topic>/en.md` remain aligned;
- [ ] `IMPLEMENTATION_STATUS.md` was updated;
- [ ] `git diff --check` is clean;
- [ ] the actual diff was read;
- [ ] no unrelated files or backups are staged.

## Stop rule

> Do not continue while the focused Setter suite is red.

Complete one small green step, record it, and only then begin the next one.

## Related documentation

- [Testing and CI](../Testing%20and%20CI/en.md)
- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Reviewing Catalog Changes](../Reviewing%20Catalog%20Changes/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
- [Known Limitations](../Known%20Limitations/en.md)

## Central principle

> A Setter contribution is complete only when implementation, tests, documentation, verification evidence, and the continuation status agree.
