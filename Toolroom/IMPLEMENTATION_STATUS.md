# Toolroom — Implementation status

## 2026-10-31 — existing tools mapped

- [Setter](WhenItFails/Setter/README.md) is an implemented .NET 10 executable with its own test suite; it edits and validates WhenItFails catalog workspaces.
- [Public API comparer](WhenItFails/PublicApiComparer/README.md) and [candidate release tools](WhenItFails/Release/README.md) are script-based workflows.
- Created a family README and English topic `Docs/Utilities/en.md` during root documentation reconciliation.
- This documentation change did not modify catalog behavior or publish a Toolroom package. The child projects' docs/implementation status provide detailed continuation instructions.

## 2026-10-31 — documentation cross-link validator

- Added a standalone Python 3 link-target validation script under [Documentation](Documentation/README.md), with an independent CI job. This is documentation tooling, not a new runtime library; CI pending.
