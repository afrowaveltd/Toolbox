# Catalog author checklist

This checklist describes the current safe authoring workflow for WhenItFails catalogs maintained with Setter.

Catalog entries are stable contracts used by runtime lookup, logs, support, documentation, profiles, mappings, automation, and released consumers.

> One logical catalog change at a time.

## Before editing

From the repository root:

```powershell
git status --short
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
dotnet run --project Toolroom/WhenItFails/Setter -- summary .
dotnet run --project Toolroom/WhenItFails/Setter -- reference .
```

Use `reference` to inspect the current owners, categories, code groups, profiles, and other workspace metadata before inventing a new value.

Confirm that:

- the workspace is already valid;
- the working tree contains no unrelated changes;
- the intended change has one clear purpose;
- an existing error, category, owner, code group, profile, tag, alias, or mapping does not already express the same concept.

## Choose stable identifiers

Stable identifiers include error IDs, symbolic names, numeric codes, owner names, category names, code-group names and prefixes, profile names, aliases, and documentation keys.

Before adding or changing one, ask:

- is it unique;
- is it understandable without temporary project context;
- is it already referenced by source code, tests, documentation, scripts, logs, dashboards, or support procedures;
- can compatibility be preserved with an alias or deprecation instead of a rename or removal;
- would adding a new entry be safer than changing the meaning of a released one.

Do not reuse a released numeric code for a different meaning.

## Prepare a new error

Use `next-code` to obtain a candidate code within the intended group:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- next-code . NETWORK
```

Use `suggest-doc-key` to obtain a canonical documentation-key candidate:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- suggest-doc-key . NETWORK "Network unavailable"
```

Suggestions are authoring aids, not automatic approval. Review the selected values against the catalog contract.

Before creation, confirm:

- stable ID, symbolic name, and numeric code are unique;
- the code belongs to the selected code-group range;
- owner, category, code group, and profile references exist;
- title is short and specific;
- message is a complete, neutral, reusable sentence;
- developer guidance is actionable and safe;
- severity reflects operational impact;
- documentation key is canonical and stable;
- tags, aliases, and metadata add real meaning.

Create the error with `add-error` using explicit values:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- add-error . AFW_NET_0042 network operational "Network is unavailable" network-is-unavailable --owner platform --profile default
```

After creation, inspect the persisted definition with `details`:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- details . AFW_NET_0042
```

## Edit an existing error

Prefer focused Setter commands for supported changes. Current commands cover user-facing text, developer guidance, severity, documentation key, name, subcategory, owner, code group, primary category, tags, and aliases.

Examples:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- set-title . AFW_NET_0001 "Network unavailable"
dotnet run --project Toolroom/WhenItFails/Setter -- set-message . AFW_NET_0001 "The network is unavailable."
dotnet run --project Toolroom/WhenItFails/Setter -- add-error-tag . AFW_NET_0001 USER_VISIBLE
```

After every focused edit:

1. inspect the result with `details`;
2. verify that related fields still agree;
3. validate the workspace;
4. review the actual diff;
5. run the corresponding tests.

A title change may require message or documentation updates. A category, owner, code-group, tag, or alias change may affect profiles, mappings, filtering, compatibility, and support expectations.

## Review renames and removals

Renames and removals are compatibility-sensitive.

Before changing or removing a stable error, use `error-references`:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- error-references . AFW_NET_0001
```

Also search source code, tests, documentation, scripts, release notes, and known integration points.

Prefer:

- aliases for compatible alternate lookup;
- deprecation when consumers may still depend on an entry;
- migration notes for intentional breaking changes;
- a new error instead of changing the historical meaning of an existing code.

## Author categories, code groups, and owners

Before adding a category, decide whether the concept is a stable problem domain rather than a temporary tag.

For a code group, verify:

- prefix uniqueness;
- numeric range boundaries;
- absence of unintended overlap;
- enough capacity for future entries;
- consistency between group, prefix, and error codes.

For an owner, verify that it represents a real responsibility boundary rather than another name for a category, feature, or profile.

Use the list and show commands to compare the proposed value with the existing reference catalogs.

## Author profiles and mappings

Profiles and mappings can change which errors a consumer receives and how failures are represented.

Inspect the current profile:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- show-profile . WEB
```

Use `explain-profile` to understand the effective selection and why errors are included or excluded:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- explain-profile . WEB
```

Review together:

- include and exclude selectors;
- owners, categories, code groups, tags, and subcategories;
- default mappings;
- explicit mapping entries;
- metadata;
- production safety expectations.

Profile explanation is an authoring and diagnostic aid. Runtime consumers remain responsible for enforcing their safety policy.

## Author safe text

User-facing titles and messages must not expose:

- credentials, tokens, or secrets;
- stack traces;
- raw SQL;
- private filesystem paths;
- internal hostnames;
- customer identifiers;
- sensitive metadata;
- an unproven technical cause presented as fact.

A title should be short and specific. A message should be a complete sentence describing what is known. A developer hint may be technical, but must remain safe and actionable.

Write English text so it can be localized later: use complete sentences, clear grammar, simple punctuation, and no concatenated fragments or culture-dependent jokes in operational messages.

## Validate documentation

When documentation keys or Markdown files change, run both checks:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
```

`check-doc-keys` checks presence, uniqueness, and canonical form according to current catalog policy.

`check-doc-links` checks local Markdown links. It does not prove that remote URLs are available or that every document is semantically complete.

Keep root README links and `Docs/<topic>/en.md` content aligned with actual behavior.

## Review safe writes and backups

Setter write commands create timestamped local recovery files containing:

```text
.bak.json
```

After a write, verify:

- the command response succeeded;
- the intended value is present after reloading or inspecting the catalog;
- validation still succeeds;
- a backup was created when expected;
- rejected input did not modify the source or create a backup.

Backups normally remain local and should not be committed.

Check the working tree:

```powershell
git status --short
git diff --check
git diff
```

Read the complete diff. Do not stage unrelated formatting, temporary diagnostics, or generated backup files.

## Run tests immediately

Every implementation or documentation change must add or update its corresponding test immediately.

Primary Setter verification:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Run the repository-wide suite when shared libraries, runtime contracts, project wiring, or other consumers are affected:

```powershell
dotnet test
```

Do not weaken a correct assertion merely to obtain green output. Fix the stale implementation, documentation, fixture, or test expectation that is actually wrong.

## Before committing

Confirm:

- [ ] the change has one logical purpose;
- [ ] stable identifiers and references were reviewed;
- [ ] new codes and documentation keys were deliberately selected;
- [ ] affected errors were inspected with `details`;
- [ ] renames or removals were checked with `error-references`;
- [ ] profile changes were inspected with `explain-profile`;
- [ ] validation passes;
- [ ] documentation checks pass when relevant;
- [ ] persistence and backup behavior are correct;
- [ ] `git status --short` contains no unexpected files;
- [ ] `git diff --check` is clean;
- [ ] the focused Setter test suite is green;
- [ ] `IMPLEMENTATION_STATUS.md` is updated;
- [ ] the actual diff was read before commit.

## Stop rule

> Do not start the next catalog change while this one is red.

Finish, verify, document, and commit the current change before opening another authoring task.

## Related documentation

- [Getting Started](../Getting-Started/en.md)
- [Commands](../Commands/en.md)
- [Reviewing Catalog Changes](../Reviewing%20Catalog%20Changes/en.md)
- [Testing and CI](../Testing%20and%20CI/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Documentation Keys](../Checking%20Documentation%20Keys/en.md)

## Central principle

> Author one clear contract change, inspect its consequences, preserve recovery evidence, and keep it red only long enough to understand and fix it.
