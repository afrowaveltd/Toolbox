# Shared dictionary search and placeholder-insensitive source signatures

Status: **agreed design direction (2026-10-19); not yet implemented**. This clarifies cross-project dictionary reuse in UnderstandMe / Afutrans. The method name \`FindInAnyDictionary\` is illustrative, not yet a frozen public API.

## Dictionaryless entries stay available

**Revised decision:** In dictionaryless / SourceText key mode, a phrase once discovered and stored **is not retired, archived or purged automatically due to inactivity, missing usage telemetry, app release changes or source rewording**. Source-phrase history is valuable reusable translation material for other authorized projects. Do not implement the previously proposed \`LastSeenUtc\`-based key pruning for this mode.

The source-language entry is \`key=value\` in **that project's** source culture (possibly \`cs\`, not always \`en\`). A reworded phrase is a new key; old source and target-language translations remain available. A project's active references can change without deleting shared dictionaries. Explicit administrative removal for error, privacy, rights or other justified cases is a different, authorized operation and must not be confused with automatic inactivity cleanup.

The distinct **Symbolic** key strategy remains configurable: source text may differ from key, and a value change at an unchanged symbolic key is a genuine source revision requiring stale-target translation review. A project-specific source record may be explicitly deleted by its owner without automatically erasing other projects' entries or the authorized reusable pool.

## Search once, reuse across dictionaries

Afutrans / another translation host may optionally implement a cross-dictionary lookup operation, conceptually \`FindInAnyDictionary\`, through an **indexed service over authorized source dictionaries and approved translations**. It can look beyond the requesting application's local dictionary; lookup must not imply moving, modifying or globally publishing the source application's entire dictionary.

Resolution preference:

1. Valid authoritative translation for the current project/key/culture, including explicit manual overrides.
2. Exact source-key match from an authorized shared catalog, if source/target culture and relevant context are compatible.
3. **Structural source-signature match** ignoring only placeholder identifiers, if fixed text, placeholder topology and supported metadata constraints are compatible.
4. If none exists, normal translation workflow or original/source-language fallback.

Do not make cross-project/network lookup mandatory for synchronous UI localization: online search can run as part of optional Afutrans generation, refresh or translation memory; application runtime can use a validated local cache. Never reuse private chat/document content as public UI dictionary data without separate authorization.

## Normalized source signature

For a parsed application UI template, replace each **distinct valid placeholder identity** with a stable canonical ID assigned on first appearance, retaining all other characters *verbatim* and tracking repeated references. The canonical source culture and meaningful format/context metadata belong to the search/filter, separate from the parsed text signature. Illustrative examples:

| Original template | Canonical structural signature |
| --- | --- |
| \`Hello {user}\` | \`Hello {#0}\` |
| \`Hello {customer}\` | \`Hello {#0}\` |
| \`From {sender} to {recipient}\` | \`From {#0} to {#1}\` |
| \`From {user} to {user}\` | \`From {#0} to {#0}\` |
| \`Your {count} items\` | \`Your {#0} items\` |

The first two **match**, the different repeated-reference shapes do **not**. Literal/escaped braces, syntax punctuation, quoted text, placeholder format specifiers and nested/invalid braces need a common robust parser; do not use a naïve regular-expression replacement that mistakes \`{{user}}\` for a parameter.

Deliberately **do not normalize** case, punctuation, whitespace, fixed words or word order outside placeholders. For example \`Hello {user}\` and \`Hi {customer}\` are not the same signature. Names and runtime values of placeholder variables are ignored for structural indexing, **not** the facts that placeholders exist, where they occur, and whether an occurrence repeats an earlier binding.

A shared translated template is stored with protected **canonical placeholder identities**, not simply its original variable names. When returning the translation to an application, remap \`#0\` to the request's \`customer\` (or another local name), including when the target language reorders placeholders. Validate exact expected identity/multiplicity before accepting; never perform unverified simple find-and-replace on arbitrary translation prose.

### Structural equality versus translation safety

A matching structure identifies a **reuse candidate**. Automatic reuse is appropriate only when the source culture, desired target culture, grammar variant, context/domain requirements and any available hint/type constraints are compatible. Example: \`{count}\` versus \`{price}\` may have the same structural signature but require different grammatical number/case or formatting; treating every match as semantically interchangeable would corrupt some languages. Missing optional metadata should not block dictionaryless lookup; the host chooses its reuse strictness (approved translation / best-effort candidate / human review). Always enforce placeholder integrity.

Index by a versioned signature + source/target locale and other required search dimensions rather than scanning all JSON/SQL dictionaries on each user request. Different storage providers supply normalized records through the same contracts. Shared translations can be reused without changing each application's original keys or metadata, preserving independent edit history, permissions and provenance.

## Planned tests

- \`Hello {user}\` matches \`Hello {customer}\` and remaps \`Ahoj {user}\` to \`Ahoj {customer}\`.
- A two-placeholder target translation that changes their order still maps to the correct requesting placeholders.
- Repeat identity patterns differ: \`{user} and {user}\` versus \`{sender} and {recipient}\`; reject mismatched occurrence counts.
- \`{{user}}\` stays literal; invalid/malformed placeholder tokens and non-equivalent fixed text do not create false matches.
- Source/target language pair, incompatible grammar hints and domain context prevent unsafe reuse; authorized manual overrides take precedence.
- An old dictionaryless phrase remains accessible even if unused by all currently observed app versions; no inactivity retirement is initiated.
- \`cs\` and \`en\` source-culture projects are indexed separately; symbolic source-value revisions still invalidate stale translations.
- Source files, SQL and in-memory adapters yield equivalent normalized candidate results, with no mandatory remote dependency at UI-render time.
- Concurrent project updates and push/poll discoveries do not duplicate approved translations or overwrite manual edits.

**Boundary:** neither \`FindInAnyDictionary\` nor signature indexing, reuse/remapping, shared translation memory or related tests exist in the current code baseline; design only.
