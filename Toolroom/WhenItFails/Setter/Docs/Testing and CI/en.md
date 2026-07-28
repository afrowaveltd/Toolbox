# Testing and CI

This guide defines the current verification workflow for WhenItFails Setter.

Setter changes are not complete when the code or documentation looks correct. They are complete when the focused test suite is green, the relevant command behavior has been inspected, and the repository diff is clean.

## Core rule

Work in small vertical slices:

```text
one implementation or documentation change
→ one corresponding test change
→ one focused test run
→ inspect the diff
→ commit
```

Do not continue while the focused Setter test run is red.

## Primary test project

Setter has a dedicated .NET 10 test project:

```text
Toolroom/WhenItFails/Setter.Tests
```

Run it from the repository root:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

This is the primary verification command after every Setter-sized change.

Run the wider repository suite when a change affects shared projects, public contracts, catalog models, Essentials, or runtime WhenItFails behavior:

```powershell
dotnet test
```

A focused green run is required before moving on. A full repository run provides broader integration confidence but does not replace focused tests while developing a Setter change.

## What the test suite protects

The Setter suite covers the complete authoring surface rather than only a few text-edit commands. Important areas include:

- workspace initialization, path resolution, validation, summary, and reference output;
- error listing, filters, details, creation, removal, and reference inspection;
- focused error-field changes, tags, aliases, ownership, categories, and code groups;
- profiles, selectors, mappings, metadata, and explanation behavior;
- safe writes, backups, backup discovery, and restoration;
- rich output, `--plain`, `--json`, and exit codes;
- documentation-key and Markdown-link checks;
- command help, usage text, README content, and topic documentation contracts;
- failure behavior, issue codes, and protection against partial or rejected writes.

The test count is useful as a snapshot, but the contracts being protected matter more than the number itself.

## Test categories

### Service and model tests

These exercise validators, loaders, editors, resolvers, selectors, renderers, and other focused components directly.

Use them when behavior can be isolated without the command-line dispatcher.

### Command tests

Command tests verify the public CLI surface:

- argument parsing;
- aliases and option casing;
- command dispatch;
- stdout and stderr behavior;
- rich, plain, and JSON output;
- exit codes;
- expected failure presentation.

A command test should describe behavior visible to a real caller, not private implementation details.

### Persistence tests

Every successful write test should verify three independent contracts:

1. the response and issue contract;
2. the persisted catalog state after reloading the file;
3. backup side effects.

Checking only the returned object is insufficient. A returned value can look correct even if persistence fails afterward.

For a rejected write, verify the inverse:

- the operation fails with the expected issue code;
- the original catalog remains unchanged;
- no inappropriate backup or temporary replacement becomes active.

### Documentation contract tests

High-value documentation is executable project state.

Documentation tests should protect statements that would otherwise become misleading, such as:

- implemented commands being described as missing;
- stale command syntax;
- omitted machine-readable output modes;
- outdated backup or documentation-check behavior;
- broken continuation instructions.

Do not test every sentence. Test the small set of claims that must remain synchronized with the implementation.

## Temporary workspace isolation

Write tests must never modify the repository workspace directly.

Use one temporary workspace per write test.

The normal pattern is:

```text
create a unique temporary project root
→ initialize the bundled workspace
→ perform one operation
→ reload affected files
→ assert response, persistence, and backup behavior
→ remove the temporary workspace
```

A GUID-based directory avoids collisions between parallel runs. Do not share one mutable workspace between tests that write catalog files.

Cleanup should be deterministic, but a cleanup-only failure must not hide the real assertion failure. During debugging, it is acceptable to preserve or print the temporary path locally; remove temporary diagnostics before committing unless they have lasting value.

## Output-mode coverage

Setter has three output families with different contracts.

### Rich output

Rich output is intended for people. Tests may verify important headings, values, tables, and failure messages, but scripts must not depend on terminal decoration or exact layout unless that layout is explicitly part of a documented contract.

### Plain output

`--plain` removes rich terminal formatting and supports simpler inspection. It is not automatically a versioned CSV, TSV, or JSON API.

Plain-output tests should verify meaningful content and absence of rich decorations without overfitting to incidental spacing.

### JSON output

`--json` is machine-readable public behavior.

JSON tests should verify:

- valid JSON;
- schema or payload version where defined;
- success and failure shapes;
- stable property meaning;
- null and empty collection behavior;
- option casing such as documented `--JSON` support;
- exit codes that agree with the JSON result.

Avoid snapshotting irrelevant whitespace or property formatting when semantic assertions are clearer.

## Exit-code coverage

Command tests must verify exit codes as part of the public automation contract.

The general model is:

```text
0 = success
1 = missing or invalid command input
2 = validation, lookup, editing, backup, save, or operation failure
3 = unexpected top-level application failure
```

A valid query with no matching rows may still return `0`. Test the documented command contract rather than assuming that empty output is always an error.

## Documentation verification

After documentation or catalog-documentation changes, run:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
```

`check-doc-keys` protects documentation-key presence, uniqueness, and canonical formatting according to current rules.

`check-doc-links` checks local Markdown links. It does not prove remote URLs are reachable or that prose is accurate; documentation tests and review cover those concerns.

## Catalog verification

After any catalog write or catalog-model change, run:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Then inspect the affected command output, for example:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- details . <error-id>
dotnet run --project Toolroom/WhenItFails/Setter -- error-references . <error-id>
dotnet run --project Toolroom/WhenItFails/Setter -- explain-profile . <profile>
```

Choose the smallest inspection command that proves the changed relationship is visible to users.

## Recommended local sequence

For a normal Setter change:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
git diff --check
git status --short
git diff
```

For a catalog or documentation change, add the relevant Setter checks:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
```

For shared-library or runtime changes, finish with:

```powershell
dotnet test
```

## Diagnosing failures

When a focused test fails:

1. read the first meaningful assertion failure, not only the final build summary;
2. identify whether the implementation, documentation, or test expectation is stale;
3. inspect the exact file and line reported;
4. reproduce with the narrowest test or test class available;
5. fix the contract, not merely the assertion text;
6. rerun the full Setter test project before continuing.

A test may correctly reveal missing documentation even when the command already appears inside a code block. Explicit prose can still be required when the command name is part of the protected public guidance.

## Writing resilient tests

Prefer assertions that protect meaning:

- stable command names and options;
- issue codes;
- exit codes;
- persisted values;
- backup creation or absence;
- required documentation claims;
- forbidden stale claims.

Avoid brittle assertions on:

- generated timestamps;
- temporary paths;
- terminal widths;
- incidental whitespace;
- unrelated ordering;
- complete rich-rendered frames when a smaller semantic assertion is sufficient.

Use `null!` only in intentional negative tests that must bypass a non-nullable model contract to exercise defensive validation. Do not weaken production nullability merely to silence a test warning.

## CI expectations

CI should fail when:

- the project does not compile;
- any Setter test fails;
- a documented public contract no longer matches implementation;
- catalog validation fails where validation is part of the workflow;
- generated or temporary artifacts are accidentally committed;
- formatting or whitespace checks fail.

CI should not silently rewrite catalogs, update snapshots without review, or convert validation into an implicit migration step.

## Change-completion checklist

Before considering a Setter change complete, confirm:

- the change is narrow and intentional;
- a corresponding test was added or updated;
- the focused Setter suite is green;
- relevant validation or documentation checks are green;
- persisted state and backup behavior were verified for writes;
- rich, plain, JSON, and exit-code contracts were considered where applicable;
- `git diff --check` succeeds;
- the diff contains no backup, temporary, or unrelated files;
- `IMPLEMENTATION_STATUS.md` records the new continuation point.

## Related documentation

- [Getting Started](../Getting-Started/en.md)
- [Commands](../Commands/en.md)
- [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Reviewing Catalog Changes](../Reviewing%20Catalog%20Changes/en.md)
- [Adding a New Command](../Adding%20a%20New%20Command/en.md)
- [Known Limitations](../Known%20Limitations/en.md)

## Central principle

> A green focused test run is the gate between one deliberate change and the next.
