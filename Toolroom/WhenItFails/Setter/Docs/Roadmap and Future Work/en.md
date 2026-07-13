# Roadmap and Future Work

This page collects possible future improvements for WhenItFails Setter.

It is not a promise, schedule, or release commitment.

It is a planning map.

Use it to understand which ideas are natural next steps and which boundaries should remain deliberate.

## Main principle

Setter should grow carefully.

A useful future feature should be:
Setter should not become a large hidden framework by accident.

## Roadmap status

Items in this document are ideas or candidates unless explicitly implemented elsewhere.

Do not treat this page as a feature contract.

Before implementing any item, define:

- exact command name,
- arguments,
- options,
- output,
- exit codes,
- issue codes,
- validation behavior,
- write behavior,
- tests,
- documentation updates.

## Current foundation

Setter already provides a useful base:

- workspace initialization,
- workspace validation,
- workspace summary,
- error browsing,
- error detail inspection,
- focused error text editing,
- severity editing,
- documentation-key editing,
- safe writes with backups,
- rich human output,
- selected plain output,
- exit codes for automation.

Future work should build on this base rather than bypass it.

## Near-term documentation improvements

Good near-term documentation candidates:

- Examples Cookbook,
- FAQ,
- Reviewing Catalog Changes,
- Documentation Link Checks,
- Runtime Consumer Guide,
- Catalog Design Principles,
- Release Notes Template,
- Security Review Checklist.

These are low-risk because they improve clarity without changing behavior.

## Near-term command candidates

Good near-term command candidates:
These are read-only and can reuse existing validation and rendering patterns.

Read-only commands are safer first steps than broad write commands.

## list-profiles

Candidate command:
Purpose:
Possible output:

- profile name,
- display name,
- description,
- included owners,
- included code groups,
- included categories.

Expected flow:
## show-profile

Candidate command:
Purpose:
Useful fields:

- name,
- display name,
- description,
- include owners,
- include code groups,
- include categories,
- include tags,
- exclude tags,
- default mappings.

This would make profile review easier without requiring direct JSON inspection.

## list-categories

Candidate command:
Purpose:
Useful fields:

- category name,
- display name,
- description,
- aliases,
- parent categories,
- default tags.

This would support catalog authors before adding new error definitions.

## list-code-groups

Candidate command:
Purpose:
Useful fields:

- name,
- display name,
- code prefix,
- codeFrom,
- codeTo,
- description,
- default categories,
- default tags.

This would reduce mistakes when choosing numeric codes.

## list-owners

Candidate command:
Purpose:
Useful fields:

- name,
- display name,
- description,
- codeFrom,
- codeTo,
- isBuiltIn,
- aliases,
- mappings.

This would help authors choose the correct responsibility boundary.

## JSON output mode

A future JSON output mode could improve automation.

Candidate option:
Possible supported commands:
JSON output should be versioned and tested.

It should define:

- success shape,
- failure shape,
- issue shape,
- command payload shape,
- ordering,
- null/empty behavior,
- schema version.

Do not add JSON output casually.

It becomes a machine-readable public contract.

## Stable issue output

If JSON output is added, validation and operation issues should have a stable shape.

Possible shape:
{
  "code": "ErrorDefinitionNotFound",
  "message": "Error definition was not found.",
  "severity": "Error",
  "path": "Jsons/WhenItFails/errors.en.json"
}
This would make CI and editor integrations easier.

## add-error command

A future `add-error` command would be valuable but should be designed carefully.

Candidate command:
Important design questions:

- Should it auto-select numeric code?
- Should it auto-select ID sequence?
- Should it require explicit code?
- Should it require explicit documentation key?
- Should it create backup?
- Should it validate before and after?
- Should it refuse duplicate concepts?
- Should it support interactive mode later?

This command should not be rushed.

## add-error minimal design

A safer first version may require explicit stable identifiers:
This avoids hidden numbering decisions.

Auto-numbering can come later.

## next-code helper

Before full `add-error`, a read-only helper may be useful.

Candidate command:
Purpose:
This should be read-only.

It should clearly say “suggested,” not silently reserve anything.

## restore-backup command

A future restore command could reduce manual recovery mistakes.

Candidate command:
Design requirements:

- list available backups,
- confirm target file,
- backup current active file before restore,
- validate after restore,
- clear output,
- safe exit codes.

Restoring should be as careful as writing.

## list-backups command

Candidate command:
Purpose:
Useful fields:

- target file,
- backup file,
- timestamp,
- size,
- age.

This could be added before `restore-backup`.

## backup cleanup command

Backup cleanup is risky.

Candidate command:
Requirements:

- dry-run by default or strongly recommended,
- never delete active catalog files,
- show exact files,
- require explicit confirmation or option,
- validate workspace first,
- document retention policy.

Do not add destructive cleanup casually.

## documentation link check

A future documentation checker could validate internal documentation links.

Candidate command:
or:
Potential checks:

- README links resolve,
- relative links resolve,
- doc folders contain `en.md`,
- documentation keys are represented somewhere,
- no broken internal references,
- no empty docs.

This would make the growing documentation set easier to maintain.

## documentation-key check

A narrower checker could focus only on error documentation keys.

Candidate command:
Potential checks:

- documentationKey is present,
- documentationKey follows naming convention,
- documentationKey maps to an expected documentation location if policy exists,
- duplicate keys are reported.

This should not assume external web URLs unless the project defines that behavior.

## catalog format command

A future format command could normalize JSON formatting.

Candidate command:
Possible behavior:
Design caution:

- formatting creates large diffs,
- ordering policy must be defined,
- comments are not supported in strict JSON,
- backup behavior must be clear.

## catalog sort command

A future sort command could reorder catalog entries.

Candidate command:
Possible sort keys:
This should be explicit.

Never sort as a hidden side effect of validation.

## schema migration command

A future schema migration command could help with breaking schema changes.

Candidate command:
Requirements:

- explicit source and target versions,
- dry-run option,
- backups,
- validation before and after,
- migration report,
- tests for old and new shape,
- documentation.

Validation should not silently migrate.

## localization workflow

Future localization support may include:
Possible commands:
Design questions:

- Which fields are language-neutral?
- Which fields are translatable?
- How are fallback languages handled?
- How are stale translations detected?
- How are documentation keys localized?

Localization is both schema and workflow evolution.

## profile resolver improvements

Future profile support may apply more of the full profile model.

Possible improvements:

- include subcategories,
- include tags,
- exclude tags,
- mapping-aware output,
- profile explanation output,
- “why included” diagnostics.

Candidate command:
This could help users understand why a given error appears or does not appear in a profile.

## explain command

A future explain command could clarify catalog relationships.

Candidate examples:
This would be useful for teaching and debugging.

## validation profiles

Future validation may support modes.

Possible examples:
Design caution:

- default validation must remain predictable,
- CI behavior must be documented,
- warning/error levels must be stable,
- release validation should not surprise local authors.

## editor integration

Future editor integrations could use Setter as a backend.

Possibilities:

- VS Code tasks,
- language-server-like diagnostics,
- JSON schema files,
- quick fixes,
- hover docs,
- profile previews.

This likely requires stable JSON output first.

## generated JSON schemas

Future generated schema files could help editors validate catalog JSON.

Possible outputs:
Requirements:

- schema versioning,
- accurate required/optional fields,
- enums where appropriate,
- documentation for editor setup,
- tests to keep schemas aligned with models.

## package templates

Future project templates could create starter catalog packages.

Candidate command:
Potential templates:
Templates should remain small and explain what they create.

## import/export profiles

Future profile portability may support:
Requirements:

- metadata,
- versioning,
- conflict handling,
- validation,
- preview/dry-run,
- documentation,
- tests.

This is useful for sharing profile policies across projects.

## catalog package export

A broader export command could package the whole catalog.

Candidate command:
Possible contents:

- JSON catalogs,
- docs,
- schema version metadata,
- README,
- validation report.

This should not replace Git, but it may help release artifacts.

## runtime consumer guide

Future docs should explain how applications should consume catalogs.

Topics:

- resolving errors by ID/code/name,
- profile use,
- severity use,
- user-facing safety,
- developer hints,
- documentation keys,
- mappings,
- fallback behavior,
- logging.

Setter supports catalog authors, but runtime consumers need their own guide.

## security review checklist

A future security-focused doc or command could help review production-facing errors.

Manual checklist topics:

- no secrets,
- no stack traces,
- no raw SQL,
- no private paths,
- no internal hostnames,
- safe production profiles,
- safe user-facing messages.

A command may help find suspicious patterns, but human review remains necessary.

## Non-goals

Setter should probably not become:

- a full IDE,
- a hidden runtime framework,
- a package publisher,
- a remote registry,
- a general JSON editor,
- a replacement for Git,
- a security scanner,
- a localization platform by accident.

It can integrate with these areas later, but its core should stay focused.

## Feature acceptance checklist

Before accepting a new feature into Setter, confirm:
## Roadmap priority guide

Prefer features in this order:
This keeps risk manageable.

## Suggested implementation order

A practical future order could be:

1. `list-profiles`
2. `show-profile`
3. `list-categories`
4. `list-code-groups`
5. `list-owners`
6. JSON output for read-only commands
7. backup listing
8. restore-backup
9. documentation link checks
10. next-code helper
11. carefully designed add-error
12. schema migration tooling
13. localization workflow

This order is not mandatory.

It is just a safe path.

## Related documentation

- [Known Limitations](../Known%20Limitations/en.md)
- [Architecture Overview](../Architecture%20Overview/en.md)
- [Adding a New Command](../Adding%20a%20New%20Command/en.md)
- [Schema Evolution](../Schema%20Evolution/en.md)
- [Deprecation and Migration](../Deprecation%20and%20Migration/en.md)
- [Maintainer Notes](../Maintainer%20Notes/en.md)
- [Release Checklist](../Release%20Checklist/en.md)
- [Testing and CI](../Testing%20and%20CI/en.md)

## Central principle

> Setter should grow like a good toolbox: one useful, tested, well-labeled tool at a time.