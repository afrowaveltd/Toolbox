# Reviewing Catalog Changes

This guide describes the current review workflow for changes to WhenItFails catalogs, Setter documentation, and the commands that maintain them.

Catalog entries are public contracts. A small JSON diff can affect runtime lookup, logs, dashboards, support procedures, documentation, profiles, mappings, and released consumers.

> Review the contract, not only the JSON diff.

## Review order

Use this order:

1. confirm the change has one clear purpose;
2. inspect the exact diff and untracked files;
3. validate the workspace;
4. inspect affected catalog relationships;
5. run documentation checks when documentation or keys are involved;
6. run the focused Setter tests;
7. review persisted values, backups, and output contracts;
8. approve only when the change is green and understandable.

## One logical change per commit

One logical change per commit is the default rule.

Do not mix:

- unrelated catalog edits;
- broad JSON reformatting;
- generated backup files;
- temporary diagnostics;
- documentation cleanup unrelated to the behavior change;
- multiple independent command changes.

A narrow diff is easier to explain, test, review, revert, and recover.

## Inspect the working tree first

From the repository root:

```powershell
git status --short
git diff --check
git diff
```

Review staged changes separately before committing:

```powershell
git diff --cached
```

`git diff --check` must be clean. `git status --short` must not hide unexpected files.

## Watch for local backup files

Setter safe-write commands create timestamped local recovery files. Their names contain:

```text
.bak.json
```

Backup files should normally remain local and should not be committed.

Before staging, check for them explicitly:

```powershell
git status --short
git diff --name-only
```

A backup is recovery material, not a replacement for Git history or a release artifact.

## Validate before detailed review

Run:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
```

Validation checks the catalog schema and known relationships. A failing validation result blocks approval.

Validation is necessary but does not replace semantic review. It cannot decide whether wording is ideal, severity is the best product decision, a new concept duplicates an existing one, or a released identifier may be safely removed.

## Review a new error

Before approving a new error, confirm that:

- its stable ID, symbolic name, and numeric code are unique;
- the numeric code belongs to the selected code group;
- owner, category, code group, and profile references exist;
- title and message are clear, neutral, reusable, and safe;
- developer guidance is actionable and contains no secrets;
- severity reflects operational impact rather than emotion;
- the documentation key is canonical and intentional;
- tags and aliases add real meaning;
- the change has a corresponding test or documented reason why an existing test already covers it.

Inspect the result:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- details . NEW_ERROR_ID
```

Use `next-code` and `suggest-doc-key` during authoring, but review the chosen values as contracts rather than accepting suggestions blindly.

## Review edits to an existing error

Use stable lookup values and inspect the complete definition:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- details . AFW_NET_0001
```

Check both the field that changed and fields whose meaning may now conflict with it.

Examples:

- a title change may require message or documentation updates;
- a category change may affect profiles and mappings;
- an owner or code-group change may conflict with numbering policy;
- a documentation-key change may break external references;
- a severity change may alter alerting, logging, and support expectations.

## Review renames and removals

Renames and removals are compatibility-sensitive.

Before approving them, use `error-references`:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- error-references . AFW_NET_0001
```

Also search source code, tests, documentation, scripts, and released integration points.

Ask:

- can an alias preserve compatibility;
- is deprecation safer than deletion;
- is a migration note required;
- will old logs and support records remain understandable;
- is a new error safer than changing the meaning of an existing numeric code.

Do not reuse a released numeric code for a different meaning.

## Review categories, code groups, and owners

For a new or changed category, ask whether it is a stable problem-domain concept rather than a temporary tag.

For a code group, verify:

- prefix uniqueness;
- numeric range boundaries;
- absence of unintended overlap;
- enough capacity for future additions;
- consistency between the group, error prefix, and numeric code.

For an owner, verify that it represents a real responsibility boundary rather than another name for a category or feature.

Use the list and show commands to inspect the current reference catalogs before approving additions.

## Review profiles and mappings

Profile and mapping changes may alter which errors a consumer receives and how they are represented.

Inspect the profile:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- show-profile . WEB
```

Use `explain-profile` to review its effective selection and the reasons errors are included or excluded:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- explain-profile . WEB
```

Review include and exclude selectors, default mappings, explicit mapping entries, and metadata together.

The explanation output is an authoring and diagnostic aid. The consuming application remains responsible for enforcing its runtime safety policy.

## Review user-facing text

User-facing messages must not expose:

- credentials, tokens, or secrets;
- stack traces;
- raw SQL;
- private filesystem paths;
- internal hostnames;
- customer identifiers;
- sensitive metadata;
- an unproven technical cause presented as fact.

A title should be short and specific. A message should be a complete, neutral sentence. A developer hint may be technical, but must remain safe and actionable.

## Review documentation keys and Markdown

When documentation keys or Markdown files change, run:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
```

`check-doc-keys` reviews presence, uniqueness, and canonical form according to current catalog policy.

`check-doc-links` reviews local Markdown links. It does not prove that arbitrary remote URLs are available or that every document is complete and correct.

Review documentation for:

- current command names and examples;
- correct relative paths and encoded spaces;
- accurate current-versus-future claims;
- alignment between root README and `Docs/<topic>/en.md`;
- a corresponding documentation-contract test for important behavior claims.

## Review output contracts

Commands may expose rich terminal output, `--plain`, and `--json`.

Review each supported surface intentionally:

- rich output is for interactive use;
- plain output is simplified human-readable text;
- `--json` is the machine-readable contract;
- exit codes are part of automation behavior.

Do not parse rich or plain output as though it were a stable JSON schema.

When a command changes, check success, validation failure, lookup failure, and unexpected failure behavior where applicable.

## Review safe writes

For write operations, tests should verify more than the returned object.

Check:

1. the response or issue contract;
2. the persisted catalog value after reloading;
3. expected backup creation on success;
4. no source mutation and no backup on rejected input;
5. post-write validation;
6. clear failure behavior if saving cannot complete.

A successful in-memory response is not enough if persistence failed.

## Run the focused Setter suite

The primary review gate is:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Every implementation or documentation change should add or update its corresponding test immediately.

Do not postpone tests until several unrelated changes have accumulated.

## Run broader verification when required

Run the repository-wide suite when the change touches shared libraries, public runtime contracts, project wiring, package behavior, or other projects:

```powershell
dotnet test
```

A focused Setter run is the minimum gate for Setter-only work, not evidence that unrelated consumers remain compatible after a shared API change.

## Diagnose failures before changing assertions

When a test fails:

1. read the exact expected and actual values;
2. identify whether implementation, documentation, test expectation, fixture, or environment is stale;
3. fix the source of truth;
4. rerun the focused test or class;
5. rerun the complete Setter suite.

Do not weaken a correct assertion merely to obtain green output.

Do not retain brittle historical values, such as a test count or commit SHA, unless that exact value is itself the contract being tested.

## Final review checklist

Before approval, confirm:

- [ ] the change has one clear purpose;
- [ ] `git status --short` contains no unexpected files;
- [ ] `git diff --check` is clean;
- [ ] backup files are not staged;
- [ ] the workspace validates;
- [ ] affected references, profiles, and mappings were inspected;
- [ ] documentation checks pass when relevant;
- [ ] persisted state and backup behavior are tested for writes;
- [ ] output and exit-code contracts remain intentional;
- [ ] `dotnet test Toolroom/WhenItFails/Setter.Tests` is green;
- [ ] `IMPLEMENTATION_STATUS.md` is updated;
- [ ] the actual diff was read before commit.

## Stop rule

> Do not approve a red change.

If validation, documentation checks, or relevant tests fail, fix the current change before starting another one.

## Related documentation

- [Getting Started](../Getting-Started/en.md)
- [Testing and CI](../Testing%20and%20CI/en.md)
- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
- [Known Limitations](../Known%20Limitations/en.md)

## Central principle

> A catalog review is complete only when the meaning, compatibility, persistence, documentation, and verification evidence agree.
