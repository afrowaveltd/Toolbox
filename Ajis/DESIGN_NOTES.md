# AJIS / A2 Design Notes

> Temporary design capture for the future AJIS 2.0 / A2 work.
>
> This file is intentionally stored in the Toolbox repository only so the current design discussion is not lost. It is **not** the final A2 specification, implementation plan, or compatibility contract. When the dedicated A2 repository is created, these notes should be reviewed, normalized into the real specification and conformance plan, and this temporary folder can be removed.

Last updated: 2026-09-29

## 1. General direction

A2 is intended to become the next-generation AJIS design rather than a direct in-place rewrite of AJIS 1.x.

Key principles discussed so far:

- The specification defines semantics; individual language implementations do not.
- .NET is expected to be the first/reference implementation.
- C is an important early implementation because it forces the design to work in highly constrained environments.
- Rust is also a first-class native implementation target.
- Other intended ecosystems include TypeScript/JavaScript, Python, and Java.
- Common APIs should remain very short and memorable.
- The same logical operation should work for small and huge data sets; data size should change implementation strategy, not the public API.
- Streaming is a foundational behavior, not an optional optimization.
- The size of a data set must not by itself dictate required RAM.
- A2 should not try to protect users from intentionally requesting large operations. If the caller asks to process every item, A2 should process every item, preferably as a bounded-memory stream.

## 2. Resource profiles and capabilities

Profiles should be a language-independent A2 concept, while each implementation exposes them in an idiomatic way.

Potential profiles:

- Minimal
- LowMemory
- Standard
- HighThroughput
- Full / Adaptive

Profiles describe resource strategy, for example:

- buffer sizes
- memory budgets
- preference for disk spill
- caching
- parallelism
- DOM/materialization defaults

Profiles must be kept conceptually separate from capabilities.

Possible capabilities:

- Text
- Mapping
- Streaming
- Binary
- TP
- Compression
- Security
- A2FS
- Query
- Web / ASP.NET integration

A profile must not change A2 semantics. The same valid input must produce the same logical result in LowMemory and HighThroughput modes. Profiles change only execution strategy and included implementation features where applicable.

### .NET packaging idea

.NET can expose convenience NuGet packages or profile packages, for example:

- `Afrowave.A2`
- `Afrowave.A2.LowMem`
- `Afrowave.A2.Text` / `Afrowave.A2.NoBinary`
- `Afrowave.A2.Full`
- `Afrowave.A2.AspNetCore`

The common facade should stay the same regardless of selected package/profile.

Example:

```csharp
var value = A2.Parse<MyType>(source);
```

The selected package may provide build-time/default configuration, but explicit application or operation settings override profile defaults.

### C build profiles and generator

C should support profile-oriented builds such as:

```text
make lowmem
make standard
make minimal
make full
```

However, the preferred user-facing configuration experience should be:

```text
make generator
```

This launches a small console configuration utility that asks mostly yes/no questions and generates a suitable build configuration.

Example questions:

- Is the target constrained or legacy?
- Prefer low memory over throughput?
- Allow disk spill?
- Need textual A2?
- Need binary?
- Need TP?
- Need compression?
- Need password protection?
- Need certificate/key encryption?
- Need signatures?
- Need A2FS?
- Need query/filter/sort?
- Need DOM?
- Need async I/O?
- Is scratch storage available?

The generator should resolve dependencies and reject impossible combinations.

The interactive wizard is only a front-end over a deterministic configuration resolver. CI must be able to generate exactly the same result non-interactively.

Possible generated artifacts:

- `.a2config`
- `generated/a2_config.h`
- `generated/a2_sources.mk`
- `generated/a2_build.mk`

Disabled capabilities should preferably be excluded from the resulting binary, not merely disabled at runtime.

Target presets may include, for example:

- Generic
- Linux
- Windows
- macOS
- DOS / FreeDOS
- Embedded
- Legacy POSIX / NAS
- Custom

A DOS/FreeDOS profile is useful as a torture target for bounded-memory design.

## 3. TP package family

The package family currently discussed is:

- `.tp`  — plain transport package
- `.tpg` — compressed package
- `.tpp` — password-protected package
- `.tpe` — certificate/key encrypted package
- `.tps` — signed package

The extension is a human-facing convenience. Actual package properties must be discoverable from the binary header/magic and flags.

Capabilities may be combined. A package may, for example, be compressed, password protected, and signed.

### TPP versus TPE

The intended user-facing distinction is simple:

**TPP**
- protected by a text password
- intended for simple deployment and easy secret distribution
- password may come from configuration, environment variables, secret storage, etc.

**TPE**
- protected against a certificate, asymmetric key, key provider, key store, hardware-backed key, or similar key-management mechanism

Internally, TPP must not directly use password text as a raw encryption key. A password-derived key should be created using an appropriate KDF and random salt, then used with authenticated encryption.

The important product distinction is therefore:

- TPP = text-password credential
- TPE = key/certificate/provider credential

This distinction is about key management and user experience, not about intentionally making TPP cryptographically weak.

## 4. Encryption layout and selective protection

Credential type and encryption layout are independent dimensions.

Potential protection layouts:

### 4.1 Opaque

The complete protected payload is encrypted.

Without the key, only the minimum outer transport metadata required to identify and process the package remains visible.

Use cases include backups, sensitive exports, and opaque package transfer.

### 4.2 Values

The logical structure/schema and keys remain visible, while all values are encrypted.

This allows tools to inspect:

- type/schema structure
- field names
- collection structure
- counts where stored separately and intentionally exposed
- package metadata
- structural validity

without exposing values.

Operations depending on encrypted values, such as normal value comparison, filtering, grouping, ordering, and aggregation, require decryption.

### 4.3 Selective

Only explicitly selected values are encrypted.

Example .NET model:

```csharp
public sealed class Address
{
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    [Encrypted]
    public string Street { get; set; } = string.Empty;

    [Encrypted]
    public string HouseNumber { get; set; } = string.Empty;
}
```

Another example:

```csharp
public sealed class User
{
    public long Id { get; set; }

    [Encrypted]
    public string FirstName { get; set; } = string.Empty;

    [Encrypted]
    public string LastName { get; set; } = string.Empty;

    public Address Address { get; set; } = new();
}
```

This allows queries such as:

```text
Country == "CZ"
City == "Praha"
ORDER BY City
GROUP BY Country
```

without decrypting names, street addresses, phone numbers, or other protected values.

The .NET `[Encrypted]` attribute is only a language-specific projection of an A2 schema property such as:

```text
protection = encrypted
```

Other languages should map this idiomatically.

Selective protection should work at useful granularities such as:

- property/value
- nested object
- attachment
- potentially column in A2FS

The security model should not silently imply that visible field names are harmless; schema itself can reveal sensitive information. Therefore Opaque, Values, and Selective must be explicit security choices.

### Encrypted-value integrity

For value-level encryption, each encrypted value should be authenticated and bound to appropriate context so ciphertext cannot be silently moved between unrelated fields or records.

The authenticated context may include concepts such as:

- document identity
- schema/type identity
- property identity/path
- record identity

The exact cryptographic transcript remains to be specified.

Encrypted values should be treated as opaque by the ordinary query engine. Searchable/blind encrypted indexes, if ever added, must be explicit because they can leak relationships between values.

## 5. A2 tooling architecture

A2 needs first-class tooling because binary TP formats cannot be conveniently inspected with a normal text editor.

The proposed architecture is:

```text
A2.Tooling.Core
      |
      +-- a2tool
      |     - simple CLI
      |     - interactive TUI
      |     - query REPL
      |
      +-- A2 Studio
            - Avalonia GUI
```

The most important rule is:

> No core data-management operation may exist only in the GUI.

The GUI is a richer client over the same tooling/core APIs, not a more powerful implementation.

### 5.1 a2tool CLI

The CLI should use a simple hierarchical syntax, conceptually:

```text
a2tool <target> <verb> <subject> [options]
```

Examples:

```text
a2tool myfile.tp view data
a2tool myfile.tp view structure
a2tool myfile.tp view package
a2tool myfile.tp view attachments
a2tool myfile.tps view signature
a2tool myfile.tpe view security
a2tool people.a2fs view indexes
```

`view structure` should show schema/logical structure without requiring the complete data payload to be loaded.

Other command families may include:

- view
- edit
- validate
- convert
- pack
- extract
- query
- index
- security
- info

The CLI should also support machine-readable output where useful, such as JSON or A2 output, along with stable exit codes.

### 5.2 Spectre.Console interactive mode

Running `a2tool <file>` without a full command may open an interactive Spectre.Console TUI.

The TUI can expose menus for:

- Browse data
- View structure
- Query
- Indexes
- Attachments
- Package information
- Security/signatures
- Validation

A built-in command line / REPL should support interactive querying, for example:

```text
a2> where Country == "CZ"
a2> and Age >= 50
a2> sort LastName asc
a2> select Id, FirstName, LastName, Age
a2> take 20
a2> explain
```

The same operations must remain accessible through non-interactive CLI commands.

The text-based tool should be viable on systems where a GUI is not available, including constrained or remote systems accessed over SSH/telnet.

### 5.3 A2 Studio

A graphical Avalonia client is desirable for comfortable data inspection and editing.

Expected capabilities include:

- virtualized data grid
- schema/structure browser
- query editor
- package/security inspector
- index manager
- attachment browser
- structured editor
- preview of binary attachments

Possible attachment previews:

- image
- text
- A2/JSON
- PDF metadata/page preview
- audio metadata/playback
- video metadata/thumbnail/playback where practical
- unknown binary as hex/metadata

Preview providers should be extensible and must not automatically execute active content.

A2 Studio must use the same Tooling.Core capabilities as `a2tool`.

### Signed package behavior

A signed package represents a specific signed state and should therefore be treated as read-only with respect to the existing signature.

For a signed package, tools may offer:

- view data
- inspect structure
- verify signatures
- extract data
- create an unsigned editable copy
- create a working copy and sign the newly saved result with a new signature

Tools should not silently mutate a signed package and leave the user believing the original signature remains meaningful.

## 6. Native paging in A2

Paging should be a first-class A2 collection-access feature, not something invented by A2 Studio.

The fundamental operation is:

```text
offset = where to start
count  = how many logical items to return
```

A convenience page API can be built on top of this.

Example:

```csharp
var page = A2.GetPage(3, document);
```

A configuration object should contain at least:

- `PageSize`
- page-number base, either zero-based or one-based

The default page-number base should be **1**, but users must be able to configure **0** if that better matches their conventions.

Example conceptual configuration:

```csharp
public enum PageNumberBase
{
    Zero = 0,
    One = 1
}
```

The external page number should always be normalized internally into zero-based offset/count. Storage, parser, and query layers should operate on offset/count rather than caring about the user's page-number convention.

Page numbering is a consumer/API preference and should not become a semantic property stored in the AJIS document.

### Paging must remain streaming

`GetPage` must not mean:

```text
parse everything
-> materialize everything
-> Skip(offset)
-> Take(count)
```

It means that only the requested items are materialized/emitted.

For a plain sequential text document without an index, reaching a large offset may still require parsing/skipping earlier items, but skipped items must not be materialized into an object graph.

Indexed formats may seek directly or approximately to the requested location.

## 7. Stream-first collection processing

Streaming is the default architectural assumption.

Conceptually:

```text
read one record
-> parse/materialize or expose transient record
-> send to consumer
-> release/reuse working memory
-> read next record
```

The requested record count may be enormous. A2 should not refuse a request simply because it is large.

Examples of intended behavior:

- 100 items
- 1,000,000 items
- 1,000,000,000 items

should all be processable through the same streaming API, subject to time, storage, platform limits, and caller intent.

The important invariant is that working memory remains bounded by parsing/writing state, buffers, and the currently processed data rather than growing linearly with the number of records.

Explicit materialization methods such as `ToList()` or `ToArray()` may exist. If a caller explicitly materializes a huge result and exhausts RAM, that is a caller decision rather than something A2 should silently prevent.

C should be able to expose especially strict low-memory APIs where a record view is valid only until the next read and the same buffer is reused.

## 8. Network ingest, local spool, and ScratchStore

When receiving a huge stream over a network, A2 should not necessarily slow the connection to the speed of final parsing/indexing/storage immediately.

A disk-backed ingest spool can decouple incoming network speed from downstream processing.

Conceptual pipeline:

```text
NETWORK
   |
   v
INGEST SPOOL / SCRATCH
   |
   +---- concurrently ----> parser/transformer ----> A2FS / final store
```

The network side can perform a mostly sequential write to local scratch storage, while processing workers consume already completed/available spool segments.

This is especially attractive on modern SSD-backed systems.

### Segmented spool

Prefer segmented scratch files over a single huge temporary file.

Example:

```text
scratch/
  ingest-000001.tmp
  ingest-000002.tmp
  ingest-000003.tmp
  ingest.state
```

A processed segment can be deleted after its contents are safely committed to final storage.

Therefore required scratch space should normally reflect the lag between ingress and downstream processing rather than always requiring enough space for the entire input.

### Shared ScratchStore

The same ScratchStore infrastructure can serve:

- network ingest spool
- external sort runs
- GroupBy
- Distinct
- index building
- canonicalization
- compaction
- transactional rewrite
- other bounded-memory intermediate operations

Possible shared components:

- MemoryBudget
- ScratchStore
- BufferPool
- Partitioner
- ExternalMerge
- TempFileManager

### Backpressure

If:

- RAM buffers are full
- scratch throughput is insufficient, or
- scratch space reaches configured safety limits

then normal backpressure must eventually slow ingress.

The spool extends the buffering window; it does not override physical limits.

### Crash/restart behavior

A spool session may maintain enough state to identify:

- received segments
- committed segments
- pending segments

This can allow safe restart/recovery and, where the transport protocol supports it, resumption without retransmitting the complete data set.

### Protected packages and scratch storage

When handling TPP/TPE data, scratch storage should avoid creating unnecessary plaintext copies.

Where practical, store the incoming protected representation in scratch and decrypt only while processing.

## 9. A2FS implications

A2FS is intended for structured data sets that may be much larger than RAM.

Current design direction includes:

- logical row/object model
- physical column/segment-oriented storage
- fixed-width columns with direct addressing
- variable-width values referenced by fixed-width offset/length entries
- optional flags for null/external/compressed/encrypted/deleted/etc.
- secondary indexes
- disk-backed/external sorting
- projection without touching unused columns
- append/update strategies for variable-length data
- free-space tracking and compaction
- crash-safe journaling/WAL as required

Selective encryption fits A2FS particularly well.

Example:

```text
Id          PUBLIC / indexed
Country     PUBLIC / indexed
City        PUBLIC / indexed
FirstName   ENCRYPTED
LastName    ENCRYPTED
Street      ENCRYPTED
```

A query can first operate only on public/indexed columns, then decrypt only the requested protected fields for matching records.

The ordinary query engine should not pretend that encrypted values are searchable without an appropriate key or an explicitly designed searchable index.

## 10. Huge-data validation and stress tests

A2 should deliberately be tested on data sets larger than available RAM.

Important scenarios include:

- open and inspect a very large A2FS data set with bounded memory
- page through records
- projection of selected columns
- full sequential scan
- indexed lookup
- filtering
- external sorting
- index creation
- variable-field updates
- compaction
- crash/recovery
- network stream -> spool -> parser -> A2FS
- network stream significantly faster than final store
- restart/resume while scratch segments exist
- protected package ingestion without unnecessary plaintext scratch

A particularly valuable stress test is:

```text
generator
  -> huge network stream
  -> local disk spool
  -> A2 parser
  -> transform
  -> A2FS
  -> indexes
```

Expected property:

```text
working memory remains approximately bounded as record count grows
```

Testing on constrained systems is important because large-memory developer machines can hide poor architecture. A low-end Windows machine, an old NAS, or a DOS/FreeDOS C target can reveal assumptions that should not exist in the core design.

## 11. Core invariants captured from the discussion

The following statements summarize the strongest architectural direction agreed during the discussion:

1. A2 is stream-first.
2. Data size alone must not dictate RAM size.
3. Paging is native collection access, not a GUI feature.
4. Paging is based internally on offset/count.
5. Page size and zero/one-based page numbering are caller configuration.
6. A2 does not arbitrarily reject huge user-requested operations; it streams them where possible.
7. Disk is a valid extension of working space through a shared ScratchStore.
8. Network ingress may be spooled to disk and processed concurrently.
9. Profiles change execution/resource strategy, not logical semantics.
10. Capabilities and resource profiles are distinct concepts.
11. TPP means text-password protection; TPE means key/certificate/provider protection.
12. Credential type is independent of what portion of data is encrypted.
13. Encryption layouts include Opaque, Values, and Selective.
14. Selective encryption is part of the A2 schema/data model and may be represented in .NET with `[Encrypted]`.
15. Query/index operations should continue to work on deliberately public fields without requiring decryption of protected fields.
16. The text CLI/TUI and graphical Studio must expose the same core operations.
17. GUI-only functionality is not acceptable for core A2 data-management operations.
18. Signed packages are read-only with respect to the original signature; edits produce a new state/signature.
19. Tooling must itself obey the same bounded-memory and paging principles as the A2 engine.
20. The same public operation should scale from tiny input to data sets far larger than RAM, with strategy changing underneath rather than forcing a different application model.
21. An optional schema available at the beginning of a document may describe anonymous/self-describing data and allow parsers/storage engines to prepare execution before consuming the payload.
22. Optional collection counts in the early schema/header are hints and planning metadata: when known they enable capacity planning and progress reporting; when unknown they are simply omitted.
23. Lossless logical AJIS-to-JSON round trips may use a reserved `#ajisData` metadata envelope so AJIS-only semantics survive systems that can transport only JSON.
24. AJIS should aim to have no unrepresentable application data: uncommon/native/custom values need an extensible tagged representation or binary/attachment escape hatch rather than becoming unsupported.
25. Tooling may enrich an existing document later (for example by adding missing collection counts or inferred schema metadata) without requiring the original producer to know everything up front.
26. Once derived metadata exists, A2 tooling/storage should maintain it transactionally during supported mutations instead of forcing repeated full rescans.
27. Metadata tooling should expose automatic generation, non-mutating validation, repair, and safe copy-based full repair; copy-based repair must use generation/version checks or journaling so concurrent mutations cannot be lost.
28. Early metadata may include an Engine strategy hint/request with values Auto, Ram, or Disk so the reader can choose its storage/execution strategy before consuming the payload.
29. Early metadata may include a rounded working-memory hint measured or estimated by the producer so Auto engine selection can compare expected memory demand with current host availability before payload processing begins.
30. AJIS should reserve unquoted #directive tokens as parser/control-plane syntax while quoted keys such as "#meta" remain ordinary user data.
31. Conditional directives such as #if/#else/#endif may allow one document to carry platform- or environment-specific branches without turning AJIS into an arbitrary-code execution language.


## 12. Optional embedded schema header

A2/AJIS should support an optional schema declaration at the beginning of a document, tentatively represented by a reserved field/directive such as `#schema`.

The schema is not required for ordinary self-describing AJIS data, but when present it can act as an early contract and planning hint before the parser starts consuming the full payload.

Although the schema remains human-readable and editable, its primary producer is expected to be the A2/AJIS serializer. Applications should normally describe their language/runtime model and serialization policy, while the serializer emits the corresponding schema metadata consistently. Hand-authored or edited schemas remain supported where useful.

Conceptual example:

```ajis
#schema: {
    type: "Person",
    fields: {
        Id: Int64,
        Name: String,
        Age: Int32,
        Address: {
            Country: String,
            City: String,
            Street: String
        }
    }
}

[
    ...
]
```

The exact syntax is not yet frozen. The important semantic property is that the schema is available at the beginning of the stream.

### 12.1 Early allocation and storage planning

Because the parser can inspect `#schema` before reading the payload, it may prepare its execution strategy in advance.

Examples:

- allocate or rent appropriately sized working buffers
- choose RAM versus ScratchStore strategy
- prepare A2FS columns before the first record arrives
- create fixed-width column storage with known element widths
- create variable-width value indexes for strings/binary fields
- initialize selective-encryption handling for protected fields
- prepare indexes or metadata structures requested by the destination
- select optimized parsers/converters for known field types
- validate incoming records against the declared contract while streaming

The schema must be treated as a planning aid, not as permission to preallocate memory proportional to the declared number of records. A2's bounded-memory/stream-first invariants still apply.

### 12.2 Optional collection count metadata

When the serializer already knows the number of logical items in a collection, the early schema/header may include an optional `count`.

Conceptual example:

```ajis
#schema: {
    type: "Person",
    count: 10000000,
    fields: {
        Id: Int64,
        Name: String,
        Age: Int32
    }
}
```

`count` is explicitly optional.

If the source is an open-ended stream, generator, network feed, or any other source whose final item count is not known when serialization begins, the serializer simply omits `count`. The absence of `count` does not reduce document validity or streaming capability.

When `count` is present, a reader may use it as useful early metadata for:

- storage/capacity planning
- pre-sizing fixed-width A2FS regions where appropriate
- sizing indexes or metadata structures
- estimating scratch-space requirements
- exposing total logical item count without scanning the payload
- progress reporting during read/import/copy operations
- estimating completion time where the application chooses to do so

For example, tooling may expose:

```text
Reading records: 4,281,337 / 10,000,000  (42.8%)
```

without first scanning the document to discover its total size.

The intended model is:

```text
count known   -> emit count -> reader may optimize and report progress
count unknown -> omit count -> normal streaming continues
```

The metadata is helpful, not mandatory.

A reader must not require `count` in order to parse a collection, and the presence of `count` must not cause the complete collection to be preallocated in RAM. Resource-profile and bounded-memory rules remain authoritative.

The final specification should decide whether a declared count is merely advisory or whether strict validation may optionally verify that the number of received logical items matches the declaration. A sensible direction is to allow the parser to use it as planning metadata immediately and validate the final observed count when validation mode requests it.

### 12.3 Anonymous/self-describing objects

An embedded schema allows A2 to transport data for which the receiver has no precompiled CLR/C/Rust/Java model.

A receiver can:

1. read `#schema`
2. construct a runtime type/schema descriptor
3. prepare suitable storage
4. stream and validate values according to that descriptor

This enables genuinely anonymous/self-describing data exchange while retaining strong type information.

A .NET consumer may expose a dynamic/runtime record abstraction rather than requiring a generated CLR class. Other language implementations should provide equivalent idiomatic runtime-schema access.

### 12.4 Schema and compiled models

When the consumer already has a target model, the embedded schema may be used to verify compatibility before the payload is consumed.

Possible outcomes include:

- exact match
- compatible match
- compatible with conversions
- missing/extra optional fields
- incompatible schema

The exact compatibility/versioning rules must be defined later in the normative specification.

### 12.5 Schema and streaming

The presence of `#schema` must never force full-document buffering.

The intended pipeline is:

```text
read schema
-> build runtime plan
-> begin streaming values
-> validate/map/store one logical unit at a time
```

For network transport this is especially useful because the receiver can prepare the destination before most of the payload has arrived.

Example:

```text
network
  -> #schema
  -> prepare A2FS columns/indexes/encryption plan
  -> record 1
  -> record 2
  -> ...
```

### 12.6 Schema and security

Schema visibility is part of the protection model.

- Opaque protection may hide the schema with the payload.
- Values protection may intentionally expose the schema while encrypting values.
- Selective protection may expose schema metadata indicating which fields are protected.

Because field names and types can themselves reveal sensitive information, exposing `#schema` must remain an explicit security choice rather than an accidental side effect.

### 12.7 Schema identity and reuse

A future optimization may allow schemas to carry a stable identifier/version or to reference a known schema by ID.

This could reduce repeated schema transmission in long-lived streams or repeated TP messages, while still permitting the full schema to be embedded when portability/self-description is more important.

The exact schema-ID, hashing, canonicalization, and versioning rules are intentionally left open for the future specification.



## 13. JSON interoperability and lossless AJIS round-trip

JSON is treated as a compatible subset/input surface of AJIS/A2, but AJIS may contain semantics that plain JSON cannot represent directly.

A2 should therefore provide explicit conversion functions such as:

```text
ToJson(...)
FromJson(...)
```

Different conversion profiles may exist, but one profile must support a **lossless logical round-trip** through JSON.

### 13.1 Reserved `#ajisData` metadata envelope

For the lossless profile, AJIS-only metadata can be represented inside valid JSON using a reserved top-level metadata member tentatively named `#ajisData`.

Conceptual example:

```json
{
  "#ajisData": {
    "version": 2,
    "schema": {
      "type": "Person",
      "count": 10000000,
      "fields": {
        "Id": "Int64",
        "Name": "String",
        "Age": "Int32"
      }
    }
  },
  "data": [
    {
      "Id": 1,
      "Name": "Peter",
      "Age": 42
    }
  ]
}
```

The exact JSON envelope shape is not yet frozen. The key principle is that the output remains ordinary valid JSON while preserving enough reserved AJIS metadata for `FromJson()` to reconstruct the original logical AJIS document.

Potential `#ajisData` contents may include:

- embedded schema
- collection counts
- tuple/union type identity where JSON arrays/objects alone are ambiguous
- AJIS-specific scalar/type information
- directives or annotations required for semantic reconstruction
- selective-encryption metadata where appropriate and safe to expose
- package/document version information
- other AJIS-only semantics that would otherwise be lost

The metadata envelope should be extensible and versioned.

### 13.2 JSON compatibility profiles

A useful direction is to keep multiple export intents conceptually separate:

- **Minimal** — produce the simplest conventional JSON representation; AJIS-only distinctions may be intentionally lost.
- **Functional** — preserve practical semantics needed by common consumers while remaining convenient JSON.
- **DeterministicRoundtrip / Lossless** — include `#ajisData` metadata sufficient to reconstruct the logical AJIS document.

This retains the principle that a user who only needs ordinary JSON does not pay for all AJIS metadata, while a user forced to route data through JSON can preserve AJIS semantics.

### 13.3 Schema preservation through JSON-only systems

The optional AJIS `#schema` header maps naturally into the JSON metadata envelope.

Conceptually:

```text
AJIS:
#schema: { ... }
<data>

        ToJson(lossless)
              |
              v

JSON:
{
  "#ajisData": {
    "schema": { ... }
  },
  "data": ...
}

        FromJson(...)
              |
              v

AJIS:
#schema: { ... }
<data>
```

This is particularly useful when an intermediate API, message bus, persistence layer, or third-party service accepts JSON but does not understand AJIS.

The intermediary only needs to preserve the reserved metadata field. It does not need to understand it.

### 13.4 Reserved-name handling

Because `#ajisData` carries protocol metadata, the final specification must define an unambiguous collision rule for user data that contains the same property name.

A likely direction is to reserve the `#ajisData` name (or a broader `#...` protocol namespace) for AJIS metadata in round-trip JSON mode and provide an escaping/mapping rule for conflicting user keys.

The rule must be deterministic and itself losslessly reversible.

### 13.5 Meaning of "lossless"

The initial goal of lossless JSON conversion is **logical/semantic round-trip**, not necessarily byte-for-byte reproduction of the original textual AJIS source.

For example, a lossless round-trip should preserve distinctions such as schema/type information, tuples, unions, protected-field metadata, and AJIS-specific scalar semantics where required.

Preserving original whitespace, formatting style, or comment placement would require a separate source-preservation mode and should not be silently implied by semantic `ToJson()/FromJson()` round-trip guarantees.

### 13.6 Streaming compatibility

The JSON interoperability layer must remain stream-first.

When possible, `ToJson()` and `FromJson()` should stream the data payload while reading/writing the comparatively small metadata envelope separately.

A known schema/count can therefore be emitted before the streamed JSON payload, just as it can be emitted before native AJIS data.



## 14. Universal data representation and metadata enrichment

A2/AJIS should be designed with the goal that application data does not become "unsupported" merely because it is not one of a small fixed set of primitive types.

Human-readable native representations should be used for common data. For uncommon, platform-specific, or future data types, the format needs an explicit extensibility mechanism.

### 14.1 Native types plus an extension escape hatch

The native value model should cover the broadly useful cross-language cases directly, including at least:

- null
- boolean
- signed/unsigned integers of useful widths
- arbitrary/large integers where supported
- floating-point values
- decimal/fixed-precision values
- strings
- binary data
- date/time/duration-like values
- arrays/lists
- objects/maps
- tuples
- unions/tagged alternatives
- references where object identity must be preserved
- attachments/external binary payloads

For values outside the native model, AJIS should support a tagged/custom value form carrying:

- stable type identifier
- optional version
- payload representation
- optional codec/format identifier

Conceptual examples:

```ajis
CustomValue: @type("vendor/example", {
    ...
})
```

or for opaque binary payloads:

```ajis
CustomValue: @binary(
    type: "vendor/example",
    attachment: "payload-17"
)
```

The exact syntax is not yet fixed. The requirement is that unfamiliar data remains transportable and preservable even when the current reader cannot interpret its application-level meaning.

Readers that do not understand a custom type should be able to preserve/forward it as an opaque tagged value rather than corrupting or discarding it.

### 14.2 Human-readable where practical, binary where appropriate

"AJIS can represent it" does not mean every value must be expanded into human-readable text.

The preferred hierarchy is:

1. readable native textual representation when practical
2. structured tagged representation when additional type identity is required
3. attachment/binary representation for large or inherently binary values

This keeps ordinary documents pleasant to inspect and edit while still allowing complete representation of real application data.

### 14.3 Object identity and references

Simple JSON-style trees are insufficient for some application graphs.

A2 should eventually define a reference/identity mechanism so that:

- repeated references to the same object can remain the same logical object
- cyclic graphs can be represented without infinite recursion
- large shared subgraphs do not need to be duplicated

The exact reference syntax and semantics remain open, but this capability is important to the "no unrepresentable data" goal.

### 14.4 Tool-driven metadata enrichment

Metadata such as schema details, collection counts, indexes, inferred types, or statistics may be unknown when a document is initially created.

A2 tooling should be able to scan an existing document later and enrich it with useful metadata.

Examples:

```text
a2tool file.ajis enrich schema
a2tool file.ajis enrich count
a2tool file.ajis enrich all
```

Names are illustrative; final CLI syntax is not frozen.

For a 100,000-record collection whose `count` was omitted during streaming creation, the tool can later stream through the collection, count logical items, and write the exact count into the schema/header.

The enrichment scan must remain bounded-memory. Counting 100,000 or 100,000,000 records does not require materializing them.

### 14.5 Updating early metadata in large text documents

Because `#schema` is intentionally at the beginning of a plain text AJIS document, adding or enlarging metadata there cannot always be performed as an in-place byte edit.

For plain AJIS text, a safe enrichment implementation may therefore:

1. stream-scan the source
2. compute the missing metadata
3. write a new temporary document with the enriched header/schema
4. stream-copy the original payload
5. validate and fsync as appropriate
6. atomically replace the original file if requested

This is still bounded-memory even for very large documents.

Container/storage formats such as TP or A2FS may support more efficient metadata updates through their own headers/index areas without rewriting the entire payload.

A future format optimization may reserve metadata space or provide an indexed metadata section, but plain human-readable AJIS should not depend on fixed-size header padding.

### 14.6 Inference is assistance, not truth

When tooling infers schema from existing data, inferred information must be distinguishable from explicitly declared schema where that distinction matters.

For example, a scan might observe:

```text
Age: Int32 in all 100,000 observed records
```

but that does not necessarily prove that the producer's intended contract forbids another numeric representation in the future.

Therefore enrichment tools may offer modes such as:

- exact observed structure
- conservative inferred schema
- strict declared schema generation

The final specification/tooling design should define these semantics clearly.

### 14.8 Maintained derived metadata

Once optional derived metadata such as `count` exists, A2 tooling and mutable storage engines should keep it synchronized with supported data mutations.

Examples:

```text
Add one item       -> count + 1
Remove one item    -> count - 1
Replace one item   -> count unchanged
Add N items        -> count + N
Remove N items     -> count - N
```

Nested collections maintain their own independent counts.

The same principle may later apply to other safely maintainable metadata, for example:

- minimum/maximum values
- null counts
- fixed-width allocation metadata
- index cardinality
- attachment counts
- checksum/signature state
- schema/version metadata
- storage statistics

Not every metadata value can be updated incrementally. A mutation may therefore mark some derived metadata as **dirty/stale** and trigger recalculation at commit/save time or on explicit request.

The logical mutation and its metadata update should be treated as one transaction where the storage format supports transactional mutation. A successful `Add` must not leave the collection with an old count.

For plain human-readable AJIS text, the in-memory/tooling model may update the count immediately while the physical file is safely rewritten on save/commit. A2FS/TP-style storage can maintain dedicated metadata areas more efficiently.

For append-only streaming creation where the final count is initially unknown, the serializer may:

1. omit `count` while streaming, or
2. maintain an internal running count and emit/finalize it if the chosen output/container format permits safe finalization.

A2 should never require a full rescan merely to update metadata that can be derived exactly from the mutation already being performed.


### 14.9 Metadata generation and repair API

The .NET-facing configuration should expose a simple switch controlling whether serializers generate useful metadata automatically while writing.

Working-name example:

```csharp
public bool AutomaticallyGenerateMeta { get; set; } = true;
```

The final public name should be reviewed for API consistency; shorter alternatives such as `AutoGenerateMeta` or `GenerateMetadataAutomatically` may be preferable. The semantic default is **true**.

When enabled, serializers should emit metadata they already know cheaply and exactly, for example:

- schema/type information
- known collection counts
- protection/encryption annotations
- attachment metadata
- other deterministic metadata that can be produced without an expensive second pass

The setting must not force a pre-scan of open-ended streams merely to discover optional metadata.

#### CheckMeta()

`CheckMeta()` is a non-mutating validation operation.

It should inspect existing metadata and report conditions such as:

- missing metadata that can be inferred
- incorrect/stale collection counts
- schema/data mismatches
- stale statistics
- invalid metadata references
- metadata marked dirty
- security/signature metadata implications

The operation should return a structured result/report rather than only text, so CLI, Studio, tests, and applications can consume the same information.

Conceptual result:

```text
Meta status:
  schema         VALID
  count          STALE (declared 100000, observed 100014)
  statistics     MISSING
  indexes        VALID
```

`CheckMeta()` must not modify the document.

#### FixMeta()

`FixMeta()` is the mutating repair operation for a document that can be safely updated through the active storage/editor transaction model.

It may:

- add missing metadata
- correct stale counts
- regenerate schema metadata
- refresh derived statistics
- clear dirty metadata after successful recalculation

Where the active format supports transactional mutation, metadata changes and data changes must commit atomically.

For plain text AJIS, the tooling layer may perform a safe rewrite internally even though the public operation is simply called `FixMeta()`.

#### FixMetaOffline()

A separate full-document repair mode is useful for very large documents or cases where metadata must be reconstructed from a complete scan.

Working-name example:

```text
FixMetaOffline()
```

The operation works against a copy/snapshot of the original document:

```text
original
   |
   +--> stable snapshot / generation G
            |
            v
        rebuild TEMP
            |
            v
        validate TEMP
            |
            v
        atomic replace / generation G+1
```

The original document remains authoritative until the rebuilt copy has been fully validated and committed.

If the rebuild fails, the temporary copy is discarded and the original remains intact.

#### Concurrent changes during offline repair

A long-running offline repair must not silently overwrite changes made after the repair started.

A2 therefore needs document generation/version tracking or an equivalent optimistic-concurrency token.

At minimum:

1. capture source generation/version `G`
2. rebuild metadata against a stable snapshot of `G`
3. before replacement, verify that the authoritative document is still at `G`
4. if unchanged, perform an atomic replacement and advance generation
5. if changed, do not overwrite the newer document

A more advanced online-compatible implementation may allow writes to continue while the rebuild runs by maintaining a mutation journal/WAL.

Conceptual flow:

```text
capture snapshot at generation G
        |
        +--> background rebuild into TEMP
        |
new writes continue on original
        |
        +--> append mutations to journal
        |
rebuild catches up
        |
brief commit lock
        |
replay remaining journal tail
        |
refresh affected metadata
        |
validate
        |
atomic swap
        |
generation G+1
```

This preserves responsiveness while avoiding lost updates.

The final API may expose both policies explicitly, for example:

- exclusive offline repair: block mutations for the duration
- snapshot/background repair: allow mutations and reconcile them before commit

The exact names remain open, but the correctness rule is not negotiable:

> Metadata repair must never replace a document with a rebuilt copy that omits mutations committed after the repair snapshot was taken.

Readers may remain available throughout a copy-based repair where the storage format permits it.



### 14.10 Engine strategy metadata

Early document metadata may include an execution/storage strategy selector named `Engine` (working name) with values:

```text
Auto
Ram
Disk
```

The default is:

```text
Engine = Auto
```

Conceptual schema/header example:

```ajis
#schema: {
    engine: Disk,
    type: "Person",
    count: 10000000,
    fields: {
        Id: Int64,
        Name: String,
        Age: Int32
    }
}
```

Because metadata is read before the payload, the parser/storage planner can choose the implementation strategy immediately.

Semantics:

- `Auto` — let the implementation choose RAM, disk-backed scratch/storage, or a hybrid strategy according to profile, memory pressure, source/destination capabilities, and workload.
- `Ram` — prefer/require RAM-backed processing for this document where permitted by host policy.
- `Disk` — prefer/require disk-backed processing from the beginning rather than waiting for memory pressure or a later spill decision.

This is especially useful for known-large streams. A producer that knows a document will be large can emit `Engine: Disk`, allowing the receiver to prepare ScratchStore/A2FS/temp structures before reading the first data item.

Example pipeline:

```text
read metadata
  -> Engine = Disk
  -> open ScratchStore / disk-backed structures
  -> prepare schema/storage plan
  -> begin streaming payload
```

This avoids unnecessary RAM growth followed by a later migration to disk.

#### Host policy remains authoritative

Document metadata must not be allowed to override hard resource/security policy of the consuming application.

Recommended precedence:

```text
explicit operation override
        >
application/runtime policy
        >
document Engine metadata
        >
default Auto
```

Examples:

- a server configured as disk-only may ignore a document's `Ram` request
- a read-only/constrained environment with no usable scratch storage may reject `Disk` with a stable capability/resource error
- an application may intentionally force `Auto` regardless of document metadata

This keeps `Engine` useful for planning while preventing untrusted input from dictating unsafe host resource behavior.

#### Engine is execution metadata, not data semantics

Changing:

```text
Engine: Ram
```

to:

```text
Engine: Disk
```

must not change the logical document value.

Therefore `Engine` belongs to execution/planning metadata, not the logical schema contract itself, even if it is physically stored in the same early metadata/header region.

A future specification may separate logical schema metadata and execution hints into distinct namespaces while keeping both available before the payload.



### 14.11 Memory requirement / working-memory hint

Early metadata may include a numeric memory-planning hint describing the approximate working memory required or observed for processing the document.

The purpose is to improve `Engine = Auto` decisions before the payload is consumed.

Conceptual example:

```ajis
#schema: {
    engine: Auto,
    memoryHint: 512MiB,
    type: "Person",
    count: 10000000,
    fields: {
        Id: Int64,
        Name: String,
        Age: Int32
    }
}
```

The exact name and representation are not yet frozen. Internally the value should use an unambiguous byte-based unit; human-readable tooling may display rounded KiB/MiB/GiB values.

#### Producer-generated value

When the serializer can observe or estimate its own peak working memory for the operation, it may emit a rounded value.

For example:

```text
observed peak working memory: 487 MiB
stored memory hint:          512 MiB
```

Rounding upward to a practical boundary is preferable to reporting false precision.

The value is optional. Open-ended streams, cross-language implementations, or serializers that cannot measure the value reliably may omit it.

#### Hint, not a semantic requirement

The memory figure is execution/planning metadata, not part of the logical data model.

It must not mean that every implementation literally requires that amount of RAM. Different runtimes and profiles may process the same document with very different working sets.

Therefore the safer semantic interpretation is:

```text
producer-observed/recommended working-memory hint
```

rather than a strict universal minimum.

A C LowMemory implementation may use far less RAM than a .NET serializer that produced the document. Conversely, another implementation may require more.

#### Auto engine decision

When `Engine = Auto`, the reader may compare the memory hint against both:

- configured A2 memory budget
- current actually available memory / memory pressure

The decision should not rely only on total physical RAM.

Conceptual policy:

```text
effectiveRamBudget =
    min(
        configured A2 budget,
        safely usable currently available memory
    )

if memoryHint <= effectiveRamBudget
    -> RAM strategy may be selected
else
    -> Disk/hybrid strategy
```

This matters on machines where nominal RAM is mostly consumed by the operating system and other applications.

Example:

```text
Physical RAM:       4 GiB
A2 configured cap:  20% = ~819 MiB
Currently available: 620 MiB
Safe A2 share now:   350 MiB
Document hint:       512 MiB

Decision: Disk
```

Even though 512 MiB is below the nominal 20% cap, current memory pressure makes a disk-backed strategy safer.

#### Engine interaction

The intended relationship is:

```text
Engine = Ram
    -> explicitly prefer RAM subject to host policy

Engine = Disk
    -> use disk-backed strategy immediately

Engine = Auto
    -> evaluate memory hint + current host conditions + profile
```

The host/application remains authoritative and may override document hints.

#### Updating the hint

Metadata tools may refresh the memory hint when they have enough information.

Possible sources include:

- measured peak working memory during serialization
- measured peak during `CheckMeta` / `FixMeta`
- model/schema-based estimates
- storage-engine statistics

Because the value is implementation-dependent, tooling should preserve provenance where useful, for example conceptually:

```text
memoryHint:
  bytes: 536870912
  source: observed
  profile: Standard
```

The exact representation remains open.

A stale or missing memory hint never makes a document invalid. It only reduces the quality of automatic planning.


### 14.7 Progressive knowledge

A2 should support a document lifecycle where information becomes richer over time:

```text
initial stream
  -> valid AJIS with minimal metadata
  -> later count/schema enrichment
  -> optional indexes/statistics
  -> optional packaging/signing/encryption
```

A document is not invalid merely because optional optimization metadata is absent.


## 15. Directive namespace and conditional documents

AJIS should distinguish ordinary data from parser/control directives syntactically.

In strict/canonical AJIS, ordinary object keys are quoted and string values are quoted. The parser may accept relaxed input forms such as unquoted ordinary keys, but canonical serialization should emit the strict form selected by the final specification.

### 15.1 Unquoted # directives

An unquoted token beginning with # is reserved for AJIS directives.

Conceptually:

    #meta: {
        "engine": "Auto"
    }

is a parser directive, while:

    {
        "#meta": {
            "engine": "Auto"
        }
    }

is ordinary user data whose key happens to be the string #meta.

This gives AJIS a collision-free control namespace:

    #meta    -> AJIS directive
    "#meta" -> ordinary data key

Potential reserved directives include #meta, #schema, #if, #else, and #endif. The exact vocabulary and extension/versioning rules remain open.

### 15.2 Meta as a control-plane container

A useful direction is to keep execution/planning metadata under #meta and logical type information under #schema.

Conceptually:

    #meta: {
        "engine": "Auto",
        "memoryHint": 536870912
    }

    #schema: {
        "type": "Person",
        "count": 10000000
    }

This makes the separation explicit:

    #meta   -> how the document may be processed
    #schema -> what the logical data is
    payload -> the data itself

### 15.3 Conditional directives

AJIS may support declarative conditional sections so one document can contain branches for different target systems or environments.

Conceptual example:

    #if: "platform == 'windows'"
        ... Windows branch ...
    #else
        ... other platforms ...
    #endif

The paired form originally discussed, where #endif may repeat the condition, may also be considered during grammar design.

The parser evaluates the condition before materializing the branch. Non-selected branches should be structurally skipped in streaming fashion.

### 15.4 Declarative condition language only

Conditional directives should remain side-effect-free and portable rather than becoming an embedded arbitrary programming language.

Potential context values include platform/OS family, architecture, runtime, A2 implementation/version, active capabilities, document profile, and application-defined named variables explicitly supplied by the host.

Potential operations include equality/inequality, boolean AND/OR/NOT, membership in a finite set, version comparison, and capability presence.

Document conditions must not implicitly gain file access, network access, process execution, reflection, arbitrary function invocation, or unrestricted host-environment access. The host supplies the evaluation context and remains authoritative.

### 15.5 Unknown conditions

The final specification must define deterministic behavior for unknown variables or unsupported condition features. Candidate behaviors include false-by-default with a validation warning, or fail-closed with a stable condition error. Implementations must not silently diverge.

### 15.6 Conditional metadata and schema

Conditional directives may apply to data, metadata, or schema annotations. For example a low-memory profile may select Engine=Disk while another profile selects Auto. Grammar and nesting rules must remain unambiguous, and early metadata must still be available soon enough for execution planning.

### 15.7 Streaming behavior

Conditional parsing remains stream-first:

    read #if
      -> evaluate condition
      -> selected branch: parse normally
      -> non-selected branch: structural skip
      -> continue after #endif

This allows very large inactive branches without materializing them.

### 15.8 JSON lossless conversion

JSON has no native AJIS directives, so lossless ToJson()/FromJson() should preserve directive semantics inside the reserved #ajisData metadata envelope.

### 15.9 Tooling

a2tool and A2 Studio should expose directive-aware inspection and validation. Useful operations may include viewing directives, resolving a document against an explicit context, validating conditions, and explaining why a branch was selected or skipped.


### 15.10 Example use case: platform-specific software delivery

Conditional directives are expected to be relatively uncommon in ordinary data documents, but they can remove the need for separate configuration/decision formats in cases where environment-dependent data is genuinely useful.

One example is software download routing.

A download service or client can supply an explicit A2 evaluation context such as:

```text
platform = windows
architecture = x64
```

and an AJIS document may contain:

```ajis
{
    #if: "platform == 'windows'"
    "download": "https://example.invalid/download/windows"
    #else
    #if: "platform == 'linux'"
    "download": "https://example.invalid/download/linux"
    #else
    #if: "platform == 'macos'"
    "download": "https://example.invalid/download/macos"
    #else
    "download": "https://example.invalid/download/generic"
    #endif
    #endif
    #endif
}
```

The exact conditional syntax may later become more compact (for example `#elseif`), but the semantic model is straightforward: one transportable document can resolve differently for Windows, Linux, macOS, or a generic fallback.

AJIS itself should not secretly probe the machine. The application/browser/client determines or receives the environment information and explicitly supplies it to the parser/resolver. This keeps resolution deterministic and testable.

The same pattern can be useful for:

- platform-specific paths
- architecture-specific binaries
- feature/capability-specific configuration
- runtime-version compatibility choices
- regional endpoints supplied through an explicit context
- deployment-profile defaults

These are convenience/control-plane cases rather than the primary purpose of AJIS, but supporting them avoids needing a separate ad-hoc format when the requirement appears.

### 15.11 Lossless JSON remains the universal bridge

Conditional directives do not break JSON interoperability.

In lossless conversion, the ordinary JSON data representation can still be emitted as valid JSON while AJIS-only directive structure is stored inside the reserved `#ajisData` envelope.

Conceptually:

```text
AJIS source with #if/#else/#endif
        |
        | ToJson(Lossless)
        v
valid JSON + "#ajisData"
        |
        | arbitrary JSON-only transport/storage
        v
valid JSON + "#ajisData"
        |
        | FromJson()
        v
reconstructed AJIS directive tree
```

Therefore a system that understands only JSON does not need to evaluate or even understand AJIS directives. It only needs to preserve the reserved `#ajisData` metadata if lossless reconstruction is required.

This supports the wider design goal:

> Rich AJIS semantics may exceed JSON, but JSON remains a viable lossless transport envelope when AJIS metadata is preserved.



## 16. Binary data in JSON interoperability

AJIS/A2 may contain true binary values and attachments. Plain JSON has no native binary scalar type, so lossless JSON conversion needs an explicit representation.

Two complementary transport forms are useful.

### 16.1 Single-file JSON: Base64 text representation

When the result must remain one ordinary JSON document, binary data should be encoded as Base64 text.

Base64 is preferred because it is widely supported, deterministic, streamable, and interoperable across languages and platforms.

The size overhead is approximately 4/3 of the original byte count (about 33%, before any JSON/string overhead).

Conceptually:

```json
{
  "#ajisData": {
    "binary": {
      "$.avatar": {
        "encoding": "base64",
        "mime": "image/png"
      }
    }
  },
  "avatar": "iVBORw0KGgoAAA..."
}
```

The exact metadata shape is not frozen. The important rule is that `#ajisData` carries enough type information for `FromJson()` to distinguish a Base64-encoded binary value from an ordinary user string.

A reader must never guess that an arbitrary string is binary merely because it looks like Base64.

Optional metadata may include:

- encoding
- MIME/media type
- logical binary type
- original length
- hash/checksum
- attachment identity
- compression/encryption information where applicable

### 16.2 Streaming Base64

Large binary values must not require loading the complete binary payload or the complete encoded string into RAM.

The converter should support streaming transformation:

```text
binary input chunks
    -> Base64 encoder
    -> JSON string output
```

and the reverse:

```text
JSON/Base64 chunks
    -> Base64 decoder
    -> binary destination
```

Working memory should remain bounded by encoder/decoder buffers.

### 16.3 Compression interaction

When a binary value is compressible and the chosen interoperability profile allows it, compression may occur before Base64 encoding:

```text
binary
  -> optional compression
  -> Base64
  -> JSON
```

However, already-compressed formats such as JPEG, PNG, MP4, ZIP, or many PDF files may gain little from another compression pass.

The conversion metadata must identify any transform required for exact reconstruction.

### 16.4 Bundle mode: JSON plus binary attachments

For large attachments, a second lossless interoperability mode may avoid Base64 expansion by keeping JSON metadata separate from raw binary payloads.

Conceptually:

```text
bundle/
  data.json
  attachments/
    avatar.bin
    document.pdf
    payload-17.bin
```

or a single ZIP-compatible/container archive containing the same logical layout.

The JSON document stores references and metadata; binary files remain binary.

Example concept:

```json
{
  "#ajisData": {
    "attachments": {
      "avatar-1": {
        "path": "attachments/avatar.bin",
        "mime": "image/png",
        "length": 284719,
        "hash": "..."
      }
    }
  },
  "avatar": {
    "#attachment": "avatar-1"
  }
}
```

The exact syntax is not frozen.

This mode is more efficient for very large binary content, while single-file Base64 remains the universal fallback when only one JSON document can be transported.

### 16.5 Suggested conversion intents

A useful API direction is to make the tradeoff explicit:

```text
ToJson(..., BinaryMode.InlineBase64)
ToJson(..., BinaryMode.Bundle)
```

Possible semantics:

- `InlineBase64` — one valid JSON document; universally portable; larger output.
- `Bundle` — JSON manifest/document plus external binary attachments; smaller and faster for large binary data, but requires multi-file/container transport.

A higher-level `Auto` mode may choose based on attachment size and target capabilities, but an explicit deterministic mode should always be available.

### 16.6 Relationship to TP

TP remains the native A2 transport/container solution for data plus attachments.

JSON bundle mode exists primarily for interoperability with ecosystems that require JSON for the structured part but can carry companion files or an archive.

If a system can carry TP directly, there is normally no reason to convert large binary attachments to Base64 JSON first.

### 16.7 Lossless round-trip requirement

Both forms must preserve enough metadata for:

```text
AJIS/A2
  -> JSON representation
  -> intermediate transport/storage
  -> FromJson()
  -> original logical binary values/attachments
```

The goal remains semantic/lossless reconstruction, not byte-identical reproduction of textual formatting.


## 17. Future migration

When the dedicated A2 repository is created:

1. Review every item in this file against the final A2 goals.
2. Separate normative semantics from implementation ideas.
3. Move normative material into the A2 Specification.
4. Turn testable requirements into Conformance vectors/tests.
5. Move implementation-specific .NET/C/Rust details into their respective implementation plans.
6. Create and maintain the real A2 `IMPLEMENTATION_STATUS.md`.
7. Remove this temporary `Ajis` folder from Toolbox once it is no longer needed.
