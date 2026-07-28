# Roadmap and Future Work

This page records genuine future candidates for WhenItFails Setter.

It is not a promise, schedule, or release commitment. Implemented behavior belongs in the command reference and README, not in the roadmap.

## Main principle

Setter should grow carefully. A useful feature should have a clear command contract, validation behavior, output, exit codes, tests, documentation, and safe write semantics where applicable.

Setter should remain a focused catalog-authoring tool rather than becoming a hidden runtime framework, full IDE, remote registry, package publisher, or general-purpose JSON editor.

## Completed foundation

The following capabilities already exist and are not roadmap items:

- workspace initialization, validation, reference inspection, and summary,
- error browsing, detail inspection, profile filtering, and profile explanation,
- error code suggestion, creation, removal, reference inspection, and focused editing,
- error tags, categories, subcategories, metadata, ownership, code-group, severity, and documentation-key editing,
- profile creation, removal, text editing, selectors, exclusions, mappings, and metadata,
- category, code-group, owner, and profile browsing,
- timestamped backup listing and validated restore,
- documentation-link and documentation-key checks,
- rich output, selected plain output, selected versioned JSON output, and stable exit codes.

See [Commands](../Commands/en.md) for the current public command surface.

## Near-term documentation candidates

Useful documentation work that does not change runtime behavior includes:

- runtime consumer guidance,
- catalog design principles,
- a security review checklist,
- worked examples and recipes,
- release-note templates,
- clearer policy guidance for public identifiers and deprecation.

## Catalog formatting and sorting

Possible explicit commands could normalize JSON formatting or reorder catalog entries.

Any implementation should:

- avoid hidden formatting or sorting during validation,
- define ordering rules precisely,
- support preview or dry-run where useful,
- create backups before writes,
- avoid unrelated diff churn.

## Backup retention

Automatic backup cleanup remains a possible feature, but it is destructive and requires a deliberate retention contract.

A safe design should provide:

- dry-run output,
- exact file selection,
- protection for active catalogs,
- explicit retention criteria,
- validation before deletion,
- clear confirmation and exit-code behavior.

## Schema migration

Setter does not automatically migrate workspace schemas.

A future explicit migration command would require:

- source and target schema versions,
- preview or dry-run support,
- backups,
- validation before and after migration,
- a migration report,
- fixtures and tests for every supported transition.

Validation must never silently migrate files.

## Localization workflow

A future localization workflow may coordinate language-neutral and translatable fields across catalog files.

Open design questions include:

- fallback language behavior,
- stale translation detection,
- translation completeness,
- synchronization of language-neutral identifiers,
- localized documentation relationships.

Setter should integrate with the future TalkToMe tooling rather than accidentally becoming a separate translation platform.

## Generated schemas and editor integration

Possible future work includes generated JSON schemas and editor integrations such as diagnostics, tasks, hover documentation, profile previews, and safe quick fixes.

These features require:

- schema versioning,
- alignment tests against runtime models,
- stable machine-readable output,
- documented editor setup,
- no hidden writes.

## Import and export

Profile or catalog package import/export may be useful for sharing reviewed policies or producing release artifacts.

A safe design should define:

- package and schema versions,
- conflict handling,
- validation,
- preview or dry-run behavior,
- metadata preservation,
- deterministic output.

Export must not replace Git as the source of truth.

## Dependency discovery

Setter currently reports catalog-level references but does not scan every source repository, generated artifact, external application, or published package for identifier usage.

A future dependency-discovery integration could help with renames and removals, but it must distinguish authoritative references from textual matches and avoid claiming completeness it cannot guarantee.

## Validation modes

Specialized validation modes may be useful for authoring, CI, release, or compatibility review.

Any mode system must preserve one predictable default and define stable warning and failure behavior. Release validation must not silently differ from local validation.

## Security review assistance

A future read-only security review command could flag suspicious patterns such as secrets, private paths, internal hostnames, raw SQL, stack traces, or unsafe user-facing text.

Such a command would be advisory. Human review remains necessary, and Setter should not claim to be a complete security scanner.

## Non-goals

Setter should not become:

- a full IDE or GUI platform,
- a hidden runtime framework,
- a remote catalog registry,
- a package publisher,
- a replacement for Git,
- a general-purpose JSON editor,
- a complete security scanner,
- a standalone localization platform.

## Feature acceptance checklist

Before accepting a new roadmap item, confirm that it:

- solves a repeated and concrete problem,
- has an explicit public contract,
- preserves safe-write guarantees,
- has focused tests,
- documents automation behavior,
- updates README and topic documentation,
- does not duplicate an existing command.

## Priority guide

Prefer future work in this order:

1. documentation and read-only diagnostics,
2. explicit previewable transformations,
3. schema and editor integration,
4. import/export workflows,
5. destructive or cross-file operations only after their rollback contract is proven.

## Related documentation

- [Known Limitations](../Known%20Limitations/en.md)
- [Commands](../Commands/en.md)
- [Architecture Overview](../Architecture%20Overview/en.md)
- [Schema Evolution](../Schema%20Evolution/en.md)
- [Deprecation and Migration](../Deprecation%20and%20Migration/en.md)
- [Testing and CI](../Testing%20and%20CI/en.md)

## Central principle

> Roadmap items describe work that is genuinely still ahead; completed features belong in current documentation.