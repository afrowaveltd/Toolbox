# Translation Target snapshots, source revisions and obsolete localization keys

Status: architecture proposal (revised 2026-10-19). No scheduled scanner, shared lookup, SQL storage or key lifecycle has yet been implemented.

## Per-project culture and identity policy

Each configured Translation Target has its own source/default culture (such as en, cs, sw) and independent key strategy:

- SourceText / dictionaryless: source-locale dictionary entry uses key = value, with the source text as the key. Changing that text creates a **new identity**. An old phrase is a separate entry, not automatically equivalent to the new wording.
- Symbolic: the key is a stable application identifier, while its source-language value can change. Scans must compare **both key and source value**, not only key presence. Changing a source value makes translations tied to its previous revision stale until reviewed/retranslated.

Key mode, source culture, metadata backend and retention policy are per target, not global. The same rules apply whether the storage adapter uses JSON files, SQL rows or REST.

## Durable old-vs-current comparison

Afutrans keeps a per-target **last successfully reconciled authoritative source snapshot or equivalent checkpoint**. For small JSON sources, keeping the entire previous normalized dictionary is simple. For large SQL/API sources, a revision token, change feed or stored key + content hashes may be more efficient while preserving equivalent comparisons.

A scan reads the coherent current source state, compares it to the prior accepted state, then classifies:

| Difference | Consequence |
| --- | --- |
| New key | Submit eligible missing-language translation work once. |
| Same symbolic key, changed source value | Mark previous target translations stale for that source revision; preserve history and request suitable translation/review. |
| SourceText key replaced/reworded | Detect a new key; preserve the old dictionaryless phrase and its translations for cross-project reuse. |
| Source key missing from a project's currently active source catalog/manifest | Record the project-level absence without expiring a previously discovered dictionaryless entry in the shared reusable pool. |
| Relevant placeholder/context/grammar metadata changed | Revalidate affected translations/variants; decide whether focused regeneration is necessary. |
| Target translation changed | Refresh consumers/audit; do not recursively trigger automatic source translation. |

Checkpoint advancement occurs only **after changed work is durably admitted/acknowledged by the normal orchestration path**. Completion of later machine/human translations is not needed to finish a scan. A failure or crash before acknowledged admission leaves the previous checkpoint for retry; idempotent work keys prevent duplicates after a successful admission followed by retry. A parallel source update must be reconciled using revision checks instead of blindly overwriting a newer checkpoint.

Push notifications and polling scans share one normalized change and job-admission path. The scanner is a discovery worker, not a second translation engine. Per-task scheduler settings and disabled polling remain valid.

## Dictionaryless retention — revised decision (2026-10-19)

**Supersedes the earlier proposal to archive dictionaryless keys after inactivity.** A discovered dictionaryless / SourceText phrase **stays in its source and translated catalogs even when the originating application no longer references it**. Those entries can be reused by another project and may be located through the planned `FindInAnyDictionary` signature search. There is no `LastSeen`-based retirement, automatic archive or purge for this keying mode.

An application's active set of references and the shared store of reusable phrases are different concepts; a refactoring of a web page need not destroy useful translation history. Only an explicitly authorized removal for another justified purpose (e.g., a mistaken or sensitive entry) may physically delete content, subject to permissions and audit.

Keep the distinct Symbolic key mode's source revision detection and optional, independently configured project-level retirement policy if a host needs it. A symbolic source-value modification still invalidates translations tied to its previous source revision; it is not treated as key reuse without checks.

For the canonical placeholder identity/signature matching, protected target-placeholder renaming, context constraints and planned tests, see [Shared translation reuse and placeholder signatures](Shared-Translation-Reuse-and-Placeholder-Signatures/en.md).

## Planned tests

- en and cs source cultures with independent SourceText and Symbolic modes, including cs key = value.
- Symbolic key value change generates stale-target state and new work without inventing a new key.
- SourceText text modification generates a new key; original dictionaryless phrase remains available without inactivity cleanup.
- Two catalogs unchanged while runtime stopped using an old phrase: old dictionaryless entry remains available for other projects.
- Low-frequency annual messages, feature flags, offline clients and previously registered dictionaryless phrases remain intact.
- Missing event delivery, scan failure/crash, source revision conflict, and push/poll duplicate all preserve data and do not lose/duplicate admitted work.
- Cross-project signature lookup finds equivalent templates and safely remaps placeholder names without losing approved human edits/history.
- SQL revision and JSON full-dictionary comparison produce equivalent logical changes.

Related design: Configuration-and-Change-Detection/en.md (sibling directory) and Afutrans-Integration/en.md. The current verified localizer remains unchanged by this document.
