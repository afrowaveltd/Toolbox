# Shared dictionary matching, casing, permissions and cache

Status: agreed design direction (2026-10-19), **documentation only**. No new matching/indexing API, formatter, SQL shared store or cache implementation has been tested or released.

## One shared store across many applications

Multiple applications may intentionally select the **same backing dictionary provider** (including one SQL database) and participate in one shared translation pool. `FindInAnyDictionary` is also useful when sources are physically separate. The application/localizer must keep its own source culture, source-key policy, domain/tenant permission, placeholder hints, target languages and manual overrides even if the physical database is shared.

Logical resource identity and access checks must distinguish a project's authoritative translation from a community/shared candidate. Cross-project reading must be explicitly authorized by effective host/tenant/target policies and must never leak private app strings, chat, credentials or unpublished proprietary records. Sharing is about reusable UI localization resources, not automatic exposure of all tables in a connected database.

## Lookup precedence: exact, then compatible structural and case variants

1. The requesting application's **exact key + source/target locale** authoritative translation and reviewed overrides win.
2. An authorized shared **exact source key + compatible placeholder/metadata/grammar signature** may supply an already validated translation.
3. A **case-insensitive candidate** (same text differing only in permitted letter case), compatible context/grammar and no unresolved collision, may be reused with a selected target-case presentation policy.
4. An authorized cross-dictionary **placeholder-name-insensitive canonical signature** may match, subject to the same case and metadata checks, with verified canonical-slot re-binding.
5. Otherwise use normal translation workflow or source fallback. Never choose an ambiguous hit arbitrarily.

The specific ordering of case and signature indexing can be optimized, but semantics are authoritative: **exact project overrides first**; case-insensitive matching is an optional second-stage equivalence test, not an irreversible global key merge.

### Case-insensitive matching is configurable, not compulsory

Proposed (names not frozen) modes: `Exact`, `IgnoreCase`, `Smart` (prefer exact; consider case-folded equivalents after validation). For shared UI dictionaries, `Smart` is the preferred design default; a domain or individual key can demand `Exact` to preserve distinctions.

Examples: `Hello World` and `hello world` may share one semantic translation candidate. **Do not collapse** English `US` (United States) and `us` (pronoun), `Polish` and `polish`, or similarly ambiguous case-only pairs. If more than one source candidate has the same folded index key but incompatible context/meaning, mark the index family ambiguous and require exact/context resolution, manual approval or fresh translation.

Define explicit **versioned source normalization** for cross-backend matching, e.g. Unicode NFC plus a carefully specified Unicode case-fold / culture-sensitive policy as appropriate to the source locale. Never rely solely on a SQL database's default collation, OS casing tables or client-platform-specific behavior to silently determine cross-project identity; use a stable normalized-index representation and test locale edge cases (e.g. Turkish I/dotted-i, German sharp-s, combining marks). Retain original source spelling and exact keys for fidelity and migrations.

**Result presentation is a separate concern.** A capitalized request may desire a capitalized target sentence, but title-casing or first-letter uppercasing is not universally safe across languages. Do not blindly use `ToTitleCase` or assume every source capitalized word must be capitalized in translation. The host can choose: preserve approved target translation (default safest), configurable locale-aware sentence-case transformation when validated, or an explicit target-case variant/override. Casing transformations must not alter placeholder values, abbreviations, names or branded identifiers.

## Placeholder signatures, grammar and allowed domains

Cross-project lookup must **consider all existing localization hints**, not only a textual signature: placeholder positions/repeated identity, original/default illustrative value, allowed enum values or ranges, grammatical gender, number, case, person, format/context and target-language variants, along with source and target culture. Example: `Hello {user}` and `Hello {customer}` have structural signature `Hello {#0}`, and a validated target template can rebind slot `#0` to either local variable name.

Illustrative/default values used to guide translations are **not** actual runtime substitutions and must not leak application/user data. Compare semantic constraints with a normalized metadata fingerprint where supplied; the user's proposed **conservative rule** is that differing allowed-value sets are not an automatic shared match. For sets with the same members but different order, the declared hint semantics decide whether order matters. A numeric count hint can be shared **within the compatible language, grammatical context and formatting domain**; plural categories, case and grammatical inflections are locale-dependent, so the same numeric value does not imply the same string in Czech, English or Arabic. Metadata absence must not break dictionaryless startup, but reuse policy may require review instead of assuming grammatical equivalence.

Sometimes structural matching is still unsafe despite compatible shallow hints: different domain meanings, terminology, tone, product/legal language, a special-case translation, manually authored override, tenant confidentiality or licensing limits. Preserve an **optional entry-level no-share override** in the logical metadata contract (e.g. `SharingPolicy=ProjectOnly`); also support independently enforced tenant/target authorization. A permissive entry metadata flag never overrides broader host privacy/access restrictions. Sharing opt-in/out defaults are host/operator policies; no cross-tenant publicity is assumed.

## Lookup performance, indexes, sorting and caching

- The current .NET `Dictionary<TKey,TValue>` is **already hash-table-backed**, offering expected O(1) lookup for exact in-memory keys. A new custom hash table is not necessary just to handle tens of thousands of phrases.
- Keep file-based JSON dictionaries **deterministically sorted by key on write** for human review, Git diffs and stable interchange. Sorting persistent records is different from lookup indexing; once loaded, use hash indexes (and optionally a second signature/case-fold index) without sorting on every read.
- A shared SQL store can index combinations such as `(sourceCulture, targetCulture, canonicalSignatureVersion, canonicalSignature, metadataFingerprint, scope)` and relevant exact-key indices. Exact composite schema and partial indices must follow real queries, measured selectivity and permissions; do not implement a fixed global table layout in the localization core.
- About **60,000 phrases can plausibly fit in RAM** for one useful cached catalog on ordinary servers, but actual memory depends on text length, number of languages, .NET string/object overhead, cache duplication, variant counts and hint metadata. Measure memory and cold-load/index rebuild under realistic test datasets rather than promising a fixed footprint. Sharing 60,000 items across several languages/tenants may scale quite differently.
- For remote providers, keep a local bounded **read-through cache** (and optionally preloaded snapshots); cache by full relevant identity: source/target locales, project/scope authorization, exact key or versioned canonical signature, compatible context/grammar/hints and revision. Invalidate by committed change notification or TTL/revision reconciliation; allow offline reads of previously validated entries. Avoid synchronous network requests on every `t[...]`.
- Avoid caching denied or tenant-private results globally. Store revision and provenance so a cached shared candidate cannot override a newer local human-approved translation. Use negative caching only with a short lifetime and revision awareness so newly added translations become visible.

## Verification scenarios for implementation

- Compare one shared SQL dictionary with separate JSON/in-memory dictionaries: equivalent authorized candidates, keys and metadata.
- `Hello World`/`hello world` find a compatible candidate without forcing identical output capitalization; case-sensitive collision `US`/`us` and language-specific casing are handled safely.
- `Hello {user}`/`Hello {customer}` match structurally and rebind a validated translation; incompatible placeholder multiplicity, grammar, available-value sets and locale reject automatic reuse.
- Entry-level `ProjectOnly` prevents cross-project reuse even when structure is identical; a permissive flag cannot bypass tenant access control.
- Source-locale numeric 1, decimal 1.0 and multiple plural/case variants select appropriate target-language outcomes when available; no assumptions of one global number rule.
- Stable sorted JSON output is reproducible; hash-index search and 60k-entry benchmark measure latency/memory; an online cache refresh never overwrites later manual edits.
- A disconnected remote provider does not block local rendering; missed events can be reconciled against versioned authoritative storage.

**Scope:** this design adds no new APIs or tests. The last observed verified code baseline was 15/15 UnderstandMe and 5,929/5,929 Toolbox tests.
