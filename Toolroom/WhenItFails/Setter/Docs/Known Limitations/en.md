# Known Limitations

This page documents the current boundaries of WhenItFails Setter.

A limitation is not necessarily a defect. Several boundaries are intentional because Setter favors explicit, reviewable, and safe catalog changes over hidden automation.

## Current capabilities are not limitations

Setter already supports:

- creating and validating workspaces,
- browsing reference catalogs and error definitions,
- adding, editing, and removing error definitions,
- creating and editing profiles,
- profile mappings and metadata,
- timestamped backups and validated restore,
- local Markdown link checks,
- documentation-key checks and suggestions,
- rich output, plain output, and selected versioned JSON contracts.

The items below describe what remains outside the current contract.

## No automatic schema migration

Setter validates the schema version and catalog relationships, but it does not silently migrate older workspaces.

Any future migration feature must be an explicit command with documented backup, validation, rollback, and compatibility behavior.

## No multi-file transaction

Each write command protects the file it changes through validation, a temporary file, backup creation, and replacement.

Setter does not currently provide one atomic transaction spanning several catalog files. Avoid concurrent writers and review multi-file manual changes carefully.

## No multi-process locking contract

Safe writes reduce the risk of corrupting one file, but they are not a distributed or multi-process coordination system.

Run one Setter write operation against a workspace at a time.

## Backups have no automatic retention policy

Setter can list and restore timestamped backups, but it does not automatically delete old backups.

Review backups before cleanup and keep Git history or another external recovery mechanism. Local backup files are not a replacement for version control.

## Temporary files may remain after interruption

An interrupted write can leave a temporary file next to the catalog. Inspect unknown files before deleting them, then validate the workspace.

## Validation is not a complete policy review

Validation checks supported structure, values, uniqueness rules, and known cross-catalog relationships. It does not decide whether:

- wording is ideal,
- severity is perfect for every product,
- two errors are semantically duplicates,
- a profile represents every application policy correctly,
- a developer hint exposes sensitive operational details.

Human review remains necessary.

## Documentation checks are local

`check-doc-links` verifies local Markdown links. `check-doc-keys` verifies documentation-key presence, uniqueness, and canonical format.

Setter does not currently prove that every documentation key resolves to a published external page, nor does it perform a complete prose, style, spelling, or semantic documentation review.

## No full localization workflow

Setter works with the current catalog files but does not yet provide an end-to-end workflow for creating languages, comparing translations, detecting missing translations, or synchronizing language-neutral fields.

Localization belongs to the planned TalkToMe tooling and future schema workflow.

## No interactive TUI or GUI editor

Setter is a command-line tool. It does not currently provide a full-screen terminal editor, desktop GUI, or browser-based catalog editor.

Such tools should reuse Setter and WhenItFails validation rules rather than introducing a second catalog contract.

## No remote catalog synchronization

Setter operates on local workspace files. It does not synchronize catalogs with registries, package feeds, web services, or central catalog servers.

Use Git and the normal build, review, and release process to share changes.

## No complete dependency discovery

`error-references` reports references represented in the loaded catalogs and profiles. Setter does not scan every source file, documentation site, deployed application, external database, or downstream package for uses of an ID, numeric code, name, profile, or mapping key.

Before renaming or removing public values, also search the repository and review downstream compatibility.

## No package publishing automation

Setter maintains catalogs; it does not build, sign, tag, or publish NuGet packages or release artifacts.

Release operations remain part of the repository release workflow.

## No command plug-in system

Commands are compiled into Setter. There is no third-party plug-in discovery model.

A future plug-in contract would need explicit rules for trust, versioning, validation, output schemas, documentation, and write safety.

## No broad partial-validation mode

Workspace validation treats the catalogs as one related package. Setter does not expose a general option that declares one catalog valid while ignoring required cross-catalog relationships.

Focused planning and documentation checks exist, but they do not replace complete workspace validation.

## No automatic formatting or global sorting command

Setter preserves focused diffs and does not currently expose a command that reformats or globally sorts every catalog.

Large formatting-only changes should remain explicit because they can obscure meaningful review.

## Runtime policy belongs to consumers

Setter manages catalog data. Consuming applications still decide how profiles, mappings, tags, severity, documentation keys, and developer hints affect runtime behavior.

Setter cannot define every policy for every future application.

## Related documentation

- [Commands](../Commands/en.md)
- [Command Quick Reference](../Command%20Quick%20Reference/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Validation](../Validation/en.md)
- [Schema Evolution](../Schema%20Evolution/en.md)
- [Deprecation and Migration](../Deprecation%20and%20Migration/en.md)
- [Roadmap and Future Work](../Roadmap%20and%20Future%20Work/en.md)

## Central principle

> Setter should document real boundaries without describing implemented features as missing.
