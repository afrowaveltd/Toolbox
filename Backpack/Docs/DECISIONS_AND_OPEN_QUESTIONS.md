# Backpack decision ledger

**Status key:** **Direction** = explicitly discussed preference; **Proposal** = interesting but not chosen; **Open** = unanswered; **Legacy** = older spec/code, requires review. All directions remain subject to conformance tests.

## Directions, 2026-10-27

1. **Backpack** is the proposed name of AJIS 2 .NET project **inside Toolbox**; AJIS 1 remains separate.
2. **Simple first:** ordinary JSON-style documents and tiny serialize/deserialize APIs must remain trivial; optional capabilities must not burden them.
3. **Layer isolation:** parser/framer only frames data and hands off; handlers interpret or transform. Unknown well-framed data survive on lossless paths.
4. **Single directive prefix `#`:** one directive attaches to the following element, possibly the root. Earlier `$` proposal was **withdrawn** in favor of reusing the AJIS 1 `#` symbol.
5. **Pointers:** typed named `*symbol` references to any AJIS value; declarations appear in the **prolog before document body** for streaming.
6. **Stream-first:** headers, byte lengths and other required parse info precede corresponding payloads.
7. **Storage:** uncompressed native AjisFS and object-offset access; transport gzip for smaller distribution; avoid routine compressed editing.
8. **Naming:** serializer defaults to camelCase (configurable), parser preserves source keys.
9. **Repair:** FixAjis may stage on disk, normalize and reassemble; never silently discard unknown safely framed data.
10. **Agents:** optional personal/shared whiteboard, public/private as social ownership not secrecy.
11. **Logic:** only declarative conditions (if/elseIf/else), **no general language** (loops, try-catch or executable scripts).
12. **Modules:** fluent `Use...` opt-ins principally affect serialization and interpretation; framing stays universal for AJIS's stable grammar. Fluent/DI chaining is an **optional extension**, not the only route. Basic AJIS 1-style operations must remain convenient without DI.
13. **Backpack target:** .NET 11 / C# 15 direction to explore discriminated unions, not implemented yet.
14. **Proof before promotion:** keep experimental proposals visibly separate from settled grammar.

## Open / conflicting points

- **Precisely what is an "element"** for `#` attachment: key-value occurrence, value, root, or structural node? How many directives can precede one target?
- **Legacy `#AJIS mode...` syntax:** how should old directive prototypes be interpreted in new strict vs tolerant readers?
- **Unknown unquoted `#name` in Lex/Lax:** directive vs JS identifier when unregistered, preserving raw spelling and intent.
- **Pointer semantics:** snapshot/reference identity vs mutable version head; aliasing; cycles; duplicate declaration; forward reference explicitly rejected for streaming unless staged repair.
- **Exact condition syntax:** input metadata visibility, predicate registry, fallback ordering, deterministic evaluation and no execution in source.
- **Bin framing / metadata:** split logical vs physical metadata; header counts, index location, trailer updates without shifting offsets, atomic journal/recovery; nonseekable-reader contract.
- **AJIS 1 → Backpack property naming migration:** AJIS 1 `AjisSettings.Naming` currently defaults to PascalCase; Backpack proposes camelCase for **new output only**. Lexical parsing of `"UserName"` vs `"userName"` accepts both but must retain their exact spelling; those are **different key names** in the data model. Binding either to C# `UserName` is a separate configurable deserializer policy (and must define collision behavior when both exist). Existing JSON/API/contract consumers can depend on exact casing, so changing the serializer default may be a **breaking output contract** even if every AJIS parser accepts both files. Do **not** attempt to infer NuGet update-vs-fresh-install status to pick naming at runtime. Prefer a documented explicit legacy policy (`PascalCase`) and preservation of original names on lossless edit/round trips; assess tests and actual users before settling default. The AJIS 2 major version gives space to change the default consciously.
- **Naming of compressed transport:** older `.tpg` vs discussion `.gtp`; extension is not authoritative.
- **Strict/Smart/Preserve naming:** semantic handling may be independent of lexical Ajis/Json/Lex/Lax strictness; avoid conflict with "Strict AJIS" grammar.
- **History:** `AddOnly`, revision provenance, parallel branching, tombstones, audit, authorized erasure and rekeying.
- **Binary identity / dedup:** content hash, stable logical ID, time-stable offset? How to support repack safely.
- **Object storage transacts:** shared writes without one giant lock, concurrent metadata/index serialization and crash safety.
- **Tooling / compiler:** how to generate .NET models from untyped inputs, union and nullable inference, preserving unknown directives.
- **Transport security:** sign/encrypt/compress order, streaming envelopes, key management and encrypted scratch behavior.
- **Version compatibility:** guarantee old-to-new textual parsing where provable, but do not claim all old parsers can frame arbitrary new syntax. Prefer preserving existing usage where inexpensive; migration and adapter decisions must reflect impact, not an absolute compatibility slogan.

## Migration policy: compatibility proportional to real impact (2026-10-27)

**Direction, explicit user clarification:** AJIS 1 was deliberately released without promotion, and its known use is limited; a few package downloads do not prove zero users. Backpack is free to introduce justified breaking changes in a new major version, but **not to break compatibility gratuitously**.

1. **Existing AJIS 1 documents:** Backpack should accept and interpret them, insofar as they conform to the actual earlier format. Preserve original quoted field names and their casing during parsing/lossless edits. Basic AJIS text must not require optional DI or advanced modules. This is **data-format backward compatibility**, separately tested.
2. **Existing AJIS 1 calling patterns:** keep simple `Ajis.Serialize(...)` / `Ajis.Deserialize<T>(...)`-style APIs practical (exact name/namespace still to decide). DI/fluent `.UseAjis().UsePointers()...` is an *additional convenience/composition* path, not mandatory infrastructure. Only remove old API behavior when there's a concrete architectural need.
3. **Small consumer migration:** changed DI registration or naming configuration requiring a few lines in one composition root is acceptable. Provide clear **AJIS 1 → Backpack migration instructions** and an explicit `PascalCase` option for consumers that depend on prior output.
4. **Large consumer migration:** if a difference forces widespread model, application, serialized data, storage or public API changes, investigate a compatibility adapter/facade, converter or migration tool before shipping the break. Compatibility effort should scale with real disruption, not with theoretical user count alone.
5. **Default new output:** proposed `camelCase` is acceptable for new Backpack serialization. It need not automatically detect upgrades via NuGet; a consumer with casing-dependent downstream contracts explicitly selects PascalCase. It is **not accurate** to declare this categorically non-breaking for third-party exact-key consumers, even though standard AJIS reading tolerates both naming styles.
6. **Source-to-source vs data compatibility:** keep separate test suites: (a) parse old files, (b) parse new files, (c) use legacy-style .NET API, (d) compare emitted property names as consumed by external exact-key integrations. A failure in one category is not automatically failure in another.

**Design filter:** first ask *whether there is any practical reason to change the existing behavior*. Preserve it if easy. If not, estimate the migration cost; small localized source edits are acceptable, widespread consumer rewrites justify a bridge.

## How to record future ideas

For each new feature capture: problem, minimal rule, example, interactions with streaming and other existing rules, AJIS 1 precedent, test fixture, status, and whether some earlier mechanism can be **removed**. Prefer replacing overlapping features over accumulating knobs.
