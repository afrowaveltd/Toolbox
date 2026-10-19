# Translation Target snapshots, source revisions and obsolete localization keys

Status: architecture proposal (2026-10-19). No scheduled scanner, usage tracking, SQL storage or key archival has yet been implemented.

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
| SourceText key replaced/reworded | Detect a new key; old key is only a retirement candidate, not automatically a confirmed rename. |
| Source key missing from authoritative source catalog/active-key manifest | Potential retirement candidate; do not immediately hard-delete translations. |
| Relevant placeholder/context/grammar metadata changed | Revalidate affected translations/variants; decide whether focused regeneration is necessary. |
| Target translation changed | Refresh consumers/audit; do not recursively trigger automatic source translation. |

Checkpoint advancement occurs only **after changed work is durably admitted/acknowledged by the normal orchestration path**. Completion of later machine/human translations is not needed to finish a scan. A failure or crash before acknowledged admission leaves the previous checkpoint for retry; idempotent work keys prevent duplicates after a successful admission followed by retry. A parallel source update must be reconciled using revision checks instead of blindly overwriting a newer checkpoint.

Push notifications and polling scans share one normalized change and job-admission path. The scanner is a discovery worker, not a second translation engine. Per-task scheduler settings and disabled polling remain valid.

## Stored key versus actual use

**Crucial:** A dictionaryless localizer registers discovered source keys, but normally does not remove old entries. Comparing two stored catalogs therefore **cannot reveal that an unchanged key has stopped being referenced by the application**. No lookup for a while is also weak evidence: rare screens, annual messages, feature flags, older deployed versions, or offline clients may still depend on it.

Support optional independent evidence, where feasible:
- An application/build manifest enumerating keys referenced by a particular released version; track which versions remain deployed. Static extraction may miss dynamically constructed keys.
- A batched, anonymous *per key* last-seen/usage aggregator in consenting hosts, with no per-user tracing and no disk/network I/O on every lookup.
- Explicit removal/rename declarations from a source repository or app, or an administrator-approved retirement decision.
- Coverage and retention rules so unavailable deployment telemetry cannot be mistaken for proof of obsolescence.

## Configurable safe key lifecycle

Proposed conceptual states, not frozen public enum values:

1. **Active:** normal resolution and translation.
2. **SuspectedUnused:** qualified by configurable inactivity/manifests/declared removals. Key still resolves; any later lookup can restore Active.
3. **Archived:** soft-retired, excluded from active export/new translation work when policy permits; keep the full prior translation, hints, edit history and ability to restore.
4. **Purged:** separately authorized/retention-governed physical removal, **disabled by default**.

An illustrative 90-day no-usage threshold may nominate a key for review; it must **not itself delete anything**. Absence of telemetry or the existence of offline/older clients should block automatic destructive assumptions. Policy is per target and may require a release grace window, active manifest evidence or manual approval.

If a key is reused, restore archived translations only when they match its source identity/revision and relevant metadata; otherwise use source fallback and retranslate. For SourceText, two different phrasings must not be automatically treated as a rename on the basis of fuzzy text similarity; suggestions for manually assisted mapping are fine.

## Planned tests

- en and cs source cultures with independent SourceText and Symbolic modes, including cs key = value.
- Symbolic key value change generates stale-target state and new work without inventing a new key.
- SourceText text modification generates a new key; original remains until the configured retirement policy.
- Two catalogs unchanged while runtime stopped using an old phrase: scanner alone cannot claim that phrase is unused.
- Low-frequency annual messages, feature flags, offline clients and old active versions are not incorrectly purged.
- Missing event delivery, scan failure/crash, source revision conflict, and push/poll duplicate all preserve data and do not lose/duplicate admitted work.
- Soft archive and later reactivation preserve approved human edits/history.
- SQL revision and JSON full-dictionary comparison produce equivalent logical changes.

Related design: Configuration-and-Change-Detection/en.md (sibling directory) and Afutrans-Integration/en.md. The current verified localizer remains unchanged by this document.
