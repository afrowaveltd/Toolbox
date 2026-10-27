# Backpack architecture — design principles, not final wire grammar

## Purpose

Small JSON-like documents must stay as simple as `{"name":"Alex"}`. Additional features are *opt-in abilities*, not mandatory syntax or runtime dependencies. Grow capability by composing **fewer, general mechanisms**, not inventing a new language.

## Layers / responsibilities

1. **Framer/tokenizer** recognizes structural boundaries and content lengths; it must not decode gzip, decrypt payloads, execute code or know domain business logic.
2. **Parser** emits structural elements, source spans and associations. Non-core semantics go to subsequent layers.
3. **Resolvers** handle symbolic pointers and contextual data placement.
4. **Directive handlers / conditions / delegates** interpret rules without altering the core grammar.
5. **Serializer** produces valid ordered output; caller chooses capabilities using familiar `.Use...` composition.
6. **Storage / AjisFS** provides persistent objects, updates, transactionality, and direct access in the uncompressed native representation.
7. **Tools** (FixAjis, scaffolder, CLI/TUI, agent profiles) consume the same abstractions; should not redefine format semantics.

## Proposed minimal syntax

- Ordinary JSON-style quoted keys remain plain keys, including `"#scope"` or `"$version"`.
- **One directive prefix, `#`**. AJIS 1 already experimented with `#`; the briefly proposed `$` form was dropped during the 2026-10-27 discussion. A directive logically attaches to the *next* element, including the root/document object. No special document-vs-object directive syntax.
- Pointer declarations use `*symbol = value` in a **prolog before the first document object**. Pointer uses `*symbol` inside document values. A pointer refers to an AJIS value of any type, including binary data and nested objects.
- A normal quoted key `"#version"` is not a directive. In tolerant JS-style parsing, recognized AJIS syntax has precedence over compatibility heuristics; unknown ambiguous items must be retained for repair.
- Directive payloads may be arbitrary AJIS data, but the exact declaration syntax, attachment-to-property-vs-value rules and unknown-directive representation are OPEN.
- Simple conditional constructs `#if / #elseIf / #else` may select a following branch (illustrative), with **no loops, try/catch, functions or arbitrary embedded scripts**.

```text
*picture = <binary-attachment>     // illustrative, not finalized binary literal
#agent: { workspaces: {} }         // attaches to next/root object
{
  "userName": "Mukwano",
  "picture": *picture,
  "#version": "an ordinary quoted key"
}
```

## Streaming is non-negotiable

- Must read forwards without requiring seek or full-document materialization.
- Information needed before an element (pointer declarations, mandatory schema/codec metadata) must be **before** that element. Binary payloads have lengths / framing *before* payload bytes.
- Unknown framed elements can be skipped or preserved. Without trustworthy boundaries, fail diagnostically rather than inventing sizes.
- Prolog pointer definitions must not force a 150 GB binary into RAM: resolver can spool to disk, stream or retain an offset if source permits.
- Comments and descriptive directives can be separate physical metadata; **semantically required** metadata cannot exist exclusively at the end of a nonseekable stream.

## Binary document / AjisFS / distribution

- Native uncompressed binary AJIS object data area begins immediately after the structural header; proposed offsets are **relative to data-area start, not disk LBA**.
- Stable logical IDs may identify objects; internal offsets locate current physical bytes. Copy/repack operations may require index/references regeneration.
- Comments and mutable directives should be physically separated from object payload where possible, with logical references to their target occurrence and order for faithful textual reconstruction.
- Appended object data plus metadata tail and indexes require a **crash-safe physical commit strategy**; details remain OPEN. Moving a metadata trailer after every append is not automatically free.
- AjisFS works on **uncompressed** binary data; compression belongs primarily to shipping / transport (`TP`, compressed variants). Normal edits should not be performed inside a whole-file gzip stream.
- `AddOnly` (immutable committed versions), update/revision records, branch detection, tombstones, snapshots and compaction are proposed storage capabilities. Preserve old versions when promised, without pretending logical immutability forbids all authorized secure erasure.

## Preserve non-understood constructs

- Source text + raw bytes + element attachment/order must be retained on a **lossless editing path**.
- "Strict / Smart / Preserve" discuss **semantic treatment** of unmatched handlers; `Lex`/`Lax` is a *separate* lexical recovery concern. Do not merge these option categories.
- Parsing or preserving encrypted payloads is different from decrypting them. For unsigned/unencrypted/basic source, simple readers need not know optional extension semantics.
- A future version's syntax may exceed an old parser's framing capabilities; **forward preservation has a bounded guarantee**, never magic.

## Conditions / environmental bindings

- Registered delegates/predicates choose branches, and registered handlers process data.
- A registered default handler receives unmatched *framed* elements, or the chosen policy preserves them.
- Conditions are declarative and cannot execute arbitrary instructions embedded in files.
- Pointer-resolution destinations can use options (desktop temp, browser-local storage, server storage, RAM-only fallback). Valid use of a binary image does not require persisting it.
- Policy/capabilities define allowed IO; app/browser runtime permissions remain in force. Any default action on binary executables should be inert / inspect, never auto-run.

## Agent workspace (optional #agent directive)

- Document carries structured shared workspace and per-agent personal `public`/`private` areas with arbitrary AJIS data, including binary artifacts.
- `private` means personal/not-to-edit, **not encrypted or secret**. All plaintext workspace data may be readable.
- Agents may read one another's notes and hand off artifacts; publishing to project-root models remains separately reviewable.
- Concurrent writes to separate objects should avoid coarse locks; shared object updates need version-check/transactions. Single physical file metadata/index allocation still requires synchronization.
- Instructions inside untrusted documents never grant system permissions or override a host's authorization.
- Agent-facing schema/semantics could be exposed by reader/tools (possibly MCP later); the **document itself does not require an MCP server**.

## Extensibility and C# surface (illustrative)

```csharp
services.UseAjis()
    .UseAjisBinary()
    .UsePointers()
    .UseDirectives()
    .UseConditions()
    .UseVersioning()
    .UseSignature()
    .UseEncryption()
    .UseGzip()
    .UseAjisFS();
```

Serializer chooses optional capabilities. The structural reader must still frame all AJIS-standard elements whether or not a semantic handler is registered; it cannot promise to interpret all future extensions or decrypt without a codec/key. .NET DI/fluent syntax is optional. A basic `Ajis.Serialize(value)` and `Ajis.Deserialize<T>(text)` must remain convenient.

One potential `AjisDirective` abstraction + target reference (root or specific element), not separate DocumentDirective/ObjectDirective class families. The `#` notation remains the same for both. Implementations may expose type-safe `AjisPointer<T>` and object-model APIs; these remain design proposals.

## Model-owned conditional delegates and cross-language semantics

A **model-local** callback/predicate is a planned first-class registration site, alongside global/application registration and optional DI. Conceptually (API not implemented):

```csharp
// Illustrative only: a model can register its own conditions without DI.
model.When("hasPhoto", context => context.Has("photo"));
model.Otherwise(context => Preserve(context.Element));
```

```js
// Illustrative only: idiomatic JS API, not C# conventions copied verbatim.
const profile = createAjisModel({
  conditions: {
    hasPhoto: ({ value }) => Boolean(value.photo)
  },
  fallback: ({ element }) => element
});
```

**Portable data vs host functions:** AJIS documents store declarative condition names, arguments and alternatives; C# delegates/JS functions belong to the local model/runtime registration, **not serialized executable function bodies**. The same AJIS document can reference `hasPhoto`; its model-specific delegate is provided separately by each implementation. Missing registrations must have explicit fallback/preservation behavior.

Local, operation, application and default delegate precedence, lifetimes and async support are OPEN. The common contract is **shared semantics and conformance cases**, not identical .NET, vanilla JS or TypeScript API shapes.

### Web ecosystem direction

Vanilla JS and TypeScript must be viable first-class consumers so Backpack can serve the largest web-development communities. Web usage should not mandate React, .NET-style DI, SignalR or JSLinq. However, complementary [JSLinq](https://github.com/afrowaveltd/JSLinq) query utilities and the existing JavaScript SignalR work can be offered via separate integration modules: make common tasks feel cohesive instead of forcing users to assemble unrelated libraries manually.

The .NET 11 implementation is an initial reference implementation, **not the definition of the language-independent AJIS format**.

## Design test

For every new proposed mechanism ask: **Can it be represented by composing existing generic rules without new grammar or cross-layer coupling?**
