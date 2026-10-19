# Layered Localization: Flat Dictionaries, Hints and Grammar Variants

**Status (2026-10-11):** agreed architectural direction and illustrative API sketches, **not implemented**. Exact public type names, union/container syntax, serialization schemas and conflict-handling policies still require discussion. This document records a starting point for incremental development.

## Non-negotiable behavior: dictionary-less first

1. A host can start with **no prebuilt dictionaries**, discover an unseen UI localization key from its source text, register `KEY = VALUE` in a configured source-language dictionary, and return a usable source/fallback text immediately.
2. **Absent hints, sidecar metadata, CLDR files, grammar variants, AI, machine providers, database access or network connectivity must never block or disable basic lookup.** Do not couple a basic `IStringLocalizer`/localizer consumer to Afutrans, EF Core or CLDR.
3. When a reliable translation/grammar choice is unavailable, prefer a usable base string (potentially imperfect grammar) over a failed lookup. **This is not permission to persist a translation with missing/duplicated/corrupted placeholders**: validation of any accepted translated candidate remains strict; reject an invalid candidate and return a safe already-known base/source string.
4. Afutrans conventionally treats English UI phrase keys as the authoritative source (e.g. `en.json`), because it is shared across code and consumers. UnderstandMe remains configurable for projects with a different source culture.
5. UI dictionary registration must never accidentally ingest private dynamic chat, unapproved documents or other transient translation content. See [Localization](../Localization/en.md).

## Storage separation and interoperability

The source and every target locale **always retain a simple flat key-to-string dictionary**. This ensures compatibility with ordinary JSON localizers and small/offline consumers.

```json
{
  "You have {count} children.": "Máte {count} dětí."
}
```

Optional metadata is stored in **separate files or SQL tables**, not nested into `cs.json`:

| Concept | Example sidecar | Responsibility |
| --- | --- | --- |
| Translator-facing source hints | `en.placeholders.json` | Meaning/example of `{count}`, generation hints; can be generated on missing source keys |
| Language-specific grammar variants | `cs.grammar.json`, `en.grammar.json` | Plural/gender/condition selections and already translated phrase variants |
| Language-specific rule overrides | separate grammar/rules record | Verified human correction or localized override independent of imported CLDR |
| Provenance, approvals and history | separate metadata/revision store | Automated suggestion vs reviewed/manual values, durable revision audit |

The exact formats are open; no `*.grammar.json` schema is finalized. Do not duplicate runtime user values in durable translator hints. Existing index-ordered example hints such as `["5", "2"]` remain valid during a compatible migration; do not silently change occurrence-index semantics.

Optional sidecars and per-locale metadata must be versioned against stable source keys and source revisions; mismatched/orphan metadata should not break ordinary localization.

## Inline hints and backwards-compatible API

Current call shape conceptually accepts source key plus ordered `params string[] args`:

```csharp
t["We found {count} files.", "5"];
```

We agreed to **preserve the original signature exactly**, then provide a separate overload/indexer with a typed hint (or a composite hint container) **before** the `params string[] args` tail:

```csharp
// Conceptual only; names are provisional:
public string this[string key, params string[] args] => Resolve(key, null, args);
public string this[string key, LocalizationHints hints, params string[] args] => Resolve(key, hints, args);

// Old:
t["You have {count} children.", "2"];

// New:
t["You have {count} children.", Hints.Plural("count"), "2"];
```

Do not replace the old signature with `hint = null` before `params`: C# does not skip an optional positional argument when given old-style positional strings. Keeping the original signature also preserves the binary contract. Both overloads delegate to **one shared resolver**.

A single `PluralHint`, `GenderHint`, `ConditionHint`, `CaseHint` etc. should carry its **own typed parameters**. One call can combine multiple hint rules (e.g. plural + grammatical gender). A **closed union of standard hint types** is a strong .NET 11/C# option; a composable container accepts a set of hints. For .NET 10, a typed record/interface hierarchy is viable. The exact public API must remain extensible for optional third-party hint handlers; exposing only an immutable closed union would constrain plugins. Do not lock into `enum + object[]` if it loses type safety, but it can serve as an internal discriminator where useful.

The **resolution engine evaluates combined selectors together**; it must not naively finalize a sentence with plural and then independently rewrite that finished sentence for gender.

Differentiate **translator-facing hints** (meaning, examples, permissible value domain, e.g. count is a nonnegative integer) from **per-request runtime context** (actual count, current gender, other values). Durable sidecars may record stable metadata on authorized key registration; never persist users' incidental runtime values as global metadata.

`string[] args` is the existing compatibility baseline. Later typed numeric/context arguments may be useful to distinguish `1` and `1.0` or preserve named bindings; add them without breaking the existing overload or placeholder order.

## Optional CLDR-driven generation and offline selection

Afutrans now vendors **official Unicode CLDR 48.2.3** reference JSON in [Afutrans.Core/Jsons/Cldr](https://github.com/afrowaveltd/Afutrans/tree/main/Afutrans.Core/Jsons/Cldr), accompanied by the source manifest and Unicode license. **No parser, interpreter, importer or selection engine is implemented yet**.

Intended incremental process:

1. A discovered source phrase plus an inline hint can create a stable, optional metadata record without delaying first localization.
2. A background translator/importer consults target-language CLDR categories (cardinal/ordinal/range where relevant), the hint's allowed value domain and representative example values.
3. The translation workflow generates candidates for *all relevant target-language categories*, validates placeholder identity/count and requests human review when needed, and stores validated variants in separate per-locale grammar data.
4. A lightweight local runtime resolver selects an available variant using the locale and runtime arguments, or falls back to the flat base translation/source key. Ordinary runtime lookup must make **no mandatory network request**.
5. The Afutrans editor permits **per-locale editing of variants and language-specific conditions**, independently of the original English phrase and globally imported CLDR reference rules. User overrides have their own provenance and must survive CLDR upgrades.
6. Duplicate *verified identical strings* may share storage. **Never infer universal rules such as `!= 1` merely because a few example translations (e.g. 2 and 5) look identical**; retain CLDR category logic or explicit reviewed overrides.

Plural categories are locale-dependent: typical English cardinal usage `one/other`; Czech also `few` and `many`, with a distinct `1.0` vs `1` behavior because CLDR operands include visible decimal digits. A count-domain hint can limit generation to meaningful values, but a default rule is not a full natural-language grammar engine. African noun classes, grammatical case and person agreement may need entirely different optional selectors and human-authored variants.

A source language may itself need grammar variants for native UI rendering; **variants are not exclusively for translated target languages**.

## First practical implementation slice (future, not started)

1. Introduce/verify simple source-key dictionary-less registration and flat JSON read/write with protected `{placeholder}` substitution; verify `IStringLocalizer` bridging and no optional-services installation.
2. Preserve original API signature; add an overload for one typed hint/composite container. Start with translator-facing hints and the minimum `PluralHint`; validate old-style positional arguments.
3. Implement a small optional cardinal CLDR reader/evaluator, with tests for `cs/en` categories including decimal visible-place edge cases and entirely missing CLDR.
4. Add sidecar variant storage, strict candidate placeholder validation, fallback resolution and human editor overrides.
5. Integrate asynchronous Afutrans generation and optional database storage *later*, with reusable UnderstandMe contracts and no dependency reversal.

Test requirements from the beginning: **completely empty dictionary start; no hints; no CLDR; no target grammar file; missing one variant; malformed optional sidecar; old overload; multi-hint combination; source-text fallback; strict placeholders; offline runtime; preserved manual edits; no changes to existing flat dictionary schema.**

## Deferred design details

- Concrete public hint/union API, representation of a combined hint set and plugin extensibility.
- Named and positional placeholder binding coexistence and typed numeric preservation.
- Sidecar JSON schema/normalization; selector expressions and order; revision/human override precedence.
- Whether optional metadata is generated immediately on key registration or only through a separate durable translation work item.
- Interaction between CLDR parent locales, explicit fallback cultures and source-language grammar variants.

**Canonical decision:** functionality and dictionary-less usability first; richer grammar and automation add independently opt-in layers without changing the base dictionary format or old public API.

## Metadata is a logical contract, not a required sidecar file (2026-10-19)

All examples named `en.placeholders.json`, `cs.grammar.json` and similar are **examples for the file/JSON adapter**. They do not define the only storage model. A host may store the same per-key hints, example/allowed placeholder values, grammar selectors, reviewed variants and provenance in SQL, a remotely served catalog or another DataAccess adapter, possibly alongside translations in a single physical store.

The localizer and the translation workflow **must consume a common logical metadata interface/normalized view**, so both observe compatible source and target identity, language, revision and placeholder context regardless of persistence. This also lets a translator update authoritative translations and metadata consistently without requiring two JSON files or duplicating language rules in every backend.

Physical atomicity differs across providers: file adapters may stage/revalidate/reconcile two files; SQL providers may use transactions; remote providers may use conditional revisions. The public semantic contract requires no silent lost updates or activation of internally inconsistent translations, while the adapter chooses how to meet it.

The file-based sidecar format stays optional. Dictionaryless start, lookup and source fallback must work when no metadata source is present. Exact new API names/signatures remain open until an actual second storage implementation is designed/tested.

