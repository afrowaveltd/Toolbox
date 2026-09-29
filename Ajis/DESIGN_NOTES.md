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
32. A2 transport adapters may stream a continuous GZip-compressed AJIS byte stream through SignalR or other transports without materializing the complete document.
33. A2 may support bounded indexed joins and server/client execution profiles without becoming a general-purpose relational database engine.
34. A2 may use EF Core metadata for bidirectional database export/import, preserving entity identity and relationships while offering both relational snapshots and human-friendly graph projections.
35. A2 should support logical references/pointers so shared entities, many-to-many relationships, and cyclic graphs can be represented without duplication while preserving bounded-memory processing.
36. A pointer is a named logical address to one complete A2 value/object; small pointer tables may be materialized early in RAM, while large tables may resolve through disk-backed indexes without changing semantics.
37. A2 should support compact and documentation-oriented writer modes; Verbal output may generate explanatory comments without changing document semantics.
38. Samples, documentation, CLI help, and the web playground should preferably share one executable sample catalog.
39. A dual-backend reference application should compare equivalent SQL/EF Core and A2/A2FS behavior on a realistic sample database before performance claims are made.
40. A2 Identity should be the first major reference library, implementing ASP.NET Core Identity storage contracts over A2FS and publishing measured scalability/resource limits instead of assuming database-like unlimited scale.
41. A2 Identity should optimize for ordinary deployments where tens of thousands of users and single-file portability/backup are more valuable than hyperscale complexity.
42. A2 Identity should include a built-in backup scheduler combining rapid mirror replication with retained, verified point-in-time snapshots so disk failure and logical corruption/deletion are both covered.


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



### 16.8 Default compression: GZip

For JSON bundles and other interoperability-oriented compressed representations, GZip should be the default compression algorithm unless a more specific format requirement exists.

Reasons:

- extremely broad cross-platform and cross-language support
- mature and well-tested implementations
- streaming compression/decompression
- simple integration with .NET through `System.IO.Compression`
- easy implementation in C, Rust, Java, JavaScript runtimes, Python, and common tooling
- no dependency on a proprietary codec
- already aligned with the intended `.tpg` package meaning

The default should remain an implementation/interoperability choice rather than a permanent restriction of the format. The container/metadata model should identify the compression algorithm explicitly so future implementations may support additional codecs without changing document semantics.

Conceptually:

```text
compression:
  algorithm: gzip
```

For a JSON bundle, a practical transport form may therefore be:

```text
document.ajson.gz
```

or a bundle/container whose manifest identifies GZip-compressed members.

GZip is compression, not confidentiality protection. Encryption/authentication remain separate TP/A2 security concerns.



## 17. GZip streaming over network transports

A2/AJIS should support transport-level streaming compression so a producer can serialize AJIS, compress it incrementally with GZip, and send the compressed bytes over a streaming transport such as SignalR without first materializing the complete document.

ASP.NET Core SignalR supports server-to-client and client-to-server streaming through `IAsyncEnumerable<T>` and `ChannelReader<T>`, so a .NET implementation can expose compressed byte chunks as the stream items.

Conceptual pipeline:

```text
object/data source
    -> AJIS serializer
    -> GZip stream encoder
    -> bounded byte chunks
    -> SignalR stream
    -> bounded byte chunks
    -> GZip stream decoder
    -> AJIS parser
    -> destination
```

The important property is that neither side needs the complete uncompressed or compressed document in memory.

### 17.1 Compress before transport chunking

Compression should operate on the continuous AJIS byte stream before transport framing/chunking.

Preferred order:

```text
AJIS bytes
  -> GZip encoder
  -> compressed byte stream
  -> transport chunks/messages
```

rather than compressing every SignalR message independently.

Keeping one logical GZip stream allows the compressor to exploit redundancy across chunk boundaries and avoids per-message GZip framing overhead.

Transport chunk size is an implementation/runtime concern and does not change AJIS semantics.

### 17.2 SignalR shape

A .NET transport adapter may conceptually expose:

```csharp
IAsyncEnumerable<ReadOnlyMemory<byte>> StreamAjisAsync(...);
```

or an equivalent `ChannelReader<byte[]>` / pipeline abstraction.

The exact public API is not frozen.

A bounded producer/consumer buffer should provide natural backpressure when the network or receiver cannot keep up.

Cancellation must flow from SignalR into the serializer/compressor so disconnecting a client stops producing data and releases resources.

### 17.3 Transport header / negotiation

The receiver needs to know the transport encoding before it can interpret the compressed AJIS bytes.

This information may be supplied outside the compressed payload by the transport invocation/session metadata, for example conceptually:

```text
contentType  = application/a2
compression  = gzip
version      = 2
```

Alternatively, formats that carry their own outer header may identify GZip through magic/flags.

After the GZip decoder starts, the normal early AJIS directives such as `#meta` and `#schema` become the first logical content seen by the AJIS parser.

Therefore transport compression does not remove the benefit of early AJIS metadata:

```text
transport says gzip
    -> initialize GZip decoder
    -> read #meta/#schema
    -> choose RAM/Disk/Auto strategy
    -> continue streaming payload
```

### 17.4 Compression policy

GZip transport compression should be optional and explicit.

A useful policy direction:

```text
Compression = None | GZip | Auto
```

- `None` — send AJIS bytes directly.
- `GZip` — stream through GZip.
- `Auto` — choose according to payload/schema hints, transport capabilities, and application policy.

The host may skip compression for payloads that are already mostly compressed binary data.

### 17.5 Binary attachments

If an AJIS/TP stream contains attachments such as JPEG, PNG, MP4, ZIP, or other already-compressed formats, recompressing those bytes may provide little benefit.

A2 may therefore eventually support segmented/member-level compression policies where structured/textual AJIS content is compressed while already-compressed attachment payloads are passed through efficiently.

The simple baseline remains valid: one GZip stream over the whole AJIS byte stream.

### 17.6 SignalR transport versus WebSocket compression

Application-level AJIS GZip streaming is distinct from WebSocket protocol compression.

SignalR may run over WebSockets or other transports, and WebSocket compression is negotiated at the WebSocket layer. A2 should not depend on that transport-specific feature for its own compression contract.

Using an explicit A2 GZip stream provides consistent behavior across supported streaming transports.

If lower transport layers also compress, implementations should avoid accidental double compression where it provides no benefit.

### 17.7 Security note

Compression is orthogonal to encryption and authentication.

When compression is combined with secrets and attacker-influenced data over an encrypted interactive channel, implementations should account for known compression side-channel risks at the transport/application level.

This does not make GZip unsuitable as the normal A2 compression codec; it means security-sensitive protocols should choose compression boundaries deliberately.

### 17.8 Reuse by SemTam / TamTam

The same transport-compression abstraction can later be reused by SemTam/TamTam adapters.

A2 should provide the byte-stream serialization/compression primitives; SignalR, raw sockets, HTTP streams, files, and future transports should be adapters over the same stream-first core rather than separate serialization implementations.



## 18. Server/client execution profiles and bounded joins

A2 may be useful as an embedded server-side data engine for applications that need structured persistence, indexing, filtering, and a small number of relational-style joins without operating a full external database server.

The goal is not to reproduce SQL/database-server breadth. The useful target is a deliberately bounded subset that preserves A2's stream-first and bounded-memory properties.

### 18.1 Server and Client execution profiles

A2 should distinguish execution profile from storage engine.

Storage engine answers:

```text
Engine = Auto | Ram | Disk
```

Execution profile answers:

```text
Profile = Auto | Server | Client
```

These are orthogonal.

Examples:

```text
Server + Disk
Server + Auto
Client + Ram
Client + Auto
```

#### Server profile

The Server profile should optimize for:

- fast request completion
- bounded per-request memory
- quick release/reuse of buffers
- avoiding large temporary object graphs
- aggressive use of persistent indexes
- disk-backed scratch/intermediate state when useful
- high concurrency
- cancellation/backpressure
- predictable resource ceilings
- reusable pools and caches whose size is globally controlled
- avoiding one expensive query starving unrelated requests

A server may intentionally spend more disk space to save RAM and CPU time on repeated queries.

Useful server-side persisted helpers may include:

- secondary indexes
- join indexes
- sort/order indexes
- query-plan metadata
- precomputed statistics
- materialized lookup tables
- cached projections where explicitly enabled

#### Client profile

The Client profile should optimize for efficient interactive work on the current machine:

- adapt to current RAM and memory pressure
- use more RAM/cache when available
- prefetch where it improves responsiveness
- spill automatically when memory becomes constrained
- prioritize smooth paging/browsing/editing
- choose strategies according to local CPU, RAM, and storage characteristics

A client profile may keep useful data hot longer than a server profile because there are fewer competing independent requests.

#### Auto profile

`Auto` may derive an execution strategy from environment and host policy, but applications should be able to select Server or Client explicitly.

### 18.2 Indexed joins without a general relational engine

A2 may support a deliberately small join model.

A common case:

```text
Users.ajis
  Id
  Name
  OrganizationId
  AddressId

Addresses.ajis
  Id
  City
  Country

Organizations.ajis
  Id
  Name
```

If the join key has an index, the engine does not need to load both data sets into RAM.

Conceptual lookup join:

```text
stream Users
   |
   +-- OrganizationId -> Organizations.Id index lookup
   |
   +-- AddressId      -> Addresses.Id index lookup
   |
   v
project result
```

Working memory remains approximately proportional to the current record, parser/query state, and bounded lookup buffers rather than total file size.

### 18.3 Initial join scope

To avoid uncontrolled database-engine scope, an initial A2 join feature should prefer simple equality/index joins.

Useful initial forms:

- inner join
- left join
- semi join / existence filter
- anti join / non-existence filter

Example:

```csharp
users
    .Where(u => u.OrganizationId == selectedOrganizationId)
    .Join(
        addresses,
        u => u.AddressId,
        a => a.Id,
        (u, a) => new { u.Id, u.Name, a.City });
```

The exact API is not frozen.

The important implementation property is that indexed joins can be executed as streaming lookup joins rather than hash-materializing an entire large side in memory.

### 18.4 Organization / identity use case

A2FS/AJIS could serve as an application identity/user store in deployments where requirements are modest and well-defined.

For example, one Users data set may contain people belonging to three organizations.

An index on:

```text
OrganizationId
```

allows:

```text
OrganizationId == 2
```

to identify only the relevant people without scanning/materializing all users.

A join or lookup through `OrganizationId` may then attach organization information, while `AddressId` may resolve an address stored in a separate data set.

This supports clean separation of data sets without requiring a large general-purpose DBMS merely to perform one or two well-known relationships.

### 18.5 Join indexes and disk-for-speed tradeoffs

A2 explicitly does not need to minimize disk usage at all costs.

When a repeated join is important, A2 may create a persistent join-oriented index.

Conceptually:

```text
Users.OrganizationId -> Organization row/record ID
Users.AddressId      -> Address row/record ID
```

or a materialized lookup structure optimized for a known relationship.

This duplicates some information on disk but can substantially reduce CPU, random scanning, and request latency.

The preferred tradeoff for Server mode is often:

```text
more disk
in exchange for
less RAM + faster request completion
```

### 18.6 Query planner expectations

A2 does not need a full SQL optimizer.

For the intended join scope, a small deterministic planner can choose among a few strategies:

```text
indexed lookup join
merge join when both sides are ordered/indexed by the same key
bounded in-memory hash join for demonstrably small inputs
disk-backed/external strategy when needed
```

The planner should expose its decision through tooling:

```text
a2> explain
Drive: Users.OrganizationId index
Join: Addresses.Id lookup index
Projection: Id, Name, City
Engine: Disk
Estimated working memory: 18 MiB
```

### 18.7 Server resource lifetime

Server mode should strongly prefer request-scoped or pooled resources with deterministic release.

Conceptually:

```text
request
  -> acquire bounded buffers/readers
  -> execute indexed/streaming query
  -> emit result stream
  -> return buffers / close readers
  -> release scratch segments
```

The profile should avoid retaining large temporary state after a request merely because memory is currently available.

Long-lived shared indexes/cache structures are a separate controlled resource class.

### 18.8 Identity-store boundary

Using A2 as an identity/user data store is plausible, but doing so safely requires storage guarantees in addition to query capability.

Relevant requirements include:

- unique-key enforcement
- atomic mutation
- crash recovery / WAL
- durable commit semantics
- concurrency control
- consistent indexes
- backup/restore
- access-control integration
- encryption/protection where required

These requirements should be implemented explicitly rather than assuming that indexing and joins alone make A2 a database server.

The target remains an embedded/specialized structured data engine, not an attempt to duplicate every feature of a mature general-purpose RDBMS.


## 19. EF Core / database import-export bridge

A2 should investigate a bidirectional database bridge built around EF Core metadata.

Core idea:

    database / DbContext
        <-> EF Core model metadata
        <-> A2 database snapshot / graph

EF Core already exposes entity types, properties, keys, foreign keys, reference/collection navigations, and many-to-many skip navigations. A2 can use that model instead of reverse-engineering relationships from row values.

Working API concepts:

    await A2.Database.ExportAsync(dbContext, destination, options);
    await A2.Database.ImportAsync(source, dbContext, options);

Final naming is not frozen.

### 19.1 Export goals

A database export may preserve:

- entity sets / logical tables
- scalar property values
- primary and alternate keys
- foreign keys
- one-to-one relationships
- one-to-many relationships
- many-to-many relationships
- join-entity payload where present
- owned/complex value structures where supported
- indexes and uniqueness metadata where available/useful
- concurrency/version properties
- provider/model identity metadata
- optional provider-specific annotations required for higher-fidelity restore

The export should remain stream-first and should not require loading the complete database into an EF change tracker or object graph.

Read-only no-tracking queries, batching/keyset iteration, and bounded navigation loading should be preferred.

### 19.2 Three representation modes

A2 should separate physical/lossless database preservation from human-friendly document projection.

#### Relational / Snapshot mode

Entities remain stored once in their logical sets and relationships are represented explicitly by keys/relationship metadata.

Conceptually:

    {
        "Users": [
            { "Id": "u1", "Name": "Peter" }
        ],
        "Roles": [
            { "Id": "r1", "Name": "Admin" }
        ],
        "UserRoles": [
            { "UserId": "u1", "RoleId": "r1" }
        ]
    }

This is the preferred lossless database round-trip representation because entity identity and join relationships remain unambiguous.

#### Graph / Document mode

The same logical database may be projected into an object-oriented document view.

Conceptually:

    {
        "Users": [
            {
                "Id": "u1",
                "Name": "Peter",
                "Roles": [
                    { "Id": "r1", "Name": "Admin" }
                ]
            }
        ]
    }

This is convenient for humans, APIs, export files, and Mongo/document-style consumers.

However, repeated/shared entities must retain identity. A role referenced by 50,000 users must not become 50,000 unrelated roles during a reverse import.

Therefore Graph mode needs one of:

- explicit A2 references
- stable entity identity metadata
- schema-defined key-based identity resolution

The final mechanism should reuse A2's general object-reference/identity feature rather than inventing a database-only reference syntax.

#### Hybrid mode

A Hybrid representation may preserve canonical entity sets once while also carrying convenient relationship projections or indexes/views.

This spends additional disk space in exchange for easier browsing and faster common access.

### 19.3 Example: ASP.NET Core Identity

A useful conformance/demo target is an ASP.NET Core Identity-style model containing entities such as:

    Users
    Roles
    UserRoles
    UserClaims
    RoleClaims
    UserLogins
    UserTokens

A document-oriented export may expose a user approximately as:

    {
        "Id": "...",
        "UserName": "...",
        "Roles": [
            { "Id": "...", "Name": "Administrator" },
            { "Id": "...", "Name": "Billing" }
        ],
        "Claims": [ ... ],
        "Logins": [ ... ]
    }

while the lossless snapshot retains enough entity/key/relationship information to reconstruct the original many-to-many/link entities.

This makes Identity a strong real-world test because it contains ordinary entities, uniqueness requirements, several relationship types, and security-sensitive fields.

### 19.4 Import / reverse direction

Import must be a first-class operation rather than an afterthought.

The importer should reconstruct entities and relationships in dependency order.

Conceptual phases:

    read #schema / database model metadata
        -> validate target model compatibility
        -> establish key policy
        -> insert/update principal entities
        -> insert/update dependent entities
        -> create join relationships
        -> restore indexes/constraints where applicable
        -> validate counts/relationships
        -> commit

Import policies may include:

- InsertOnly
- Upsert
- Replace
- Merge
- ValidateOnly

Key handling may include:

- PreserveKeys
- RegenerateKeys
- MapKeys

If keys are regenerated, the importer must keep an old-key -> new-key map so foreign keys and many-to-many relationships are rewritten consistently. Large maps may spill to disk rather than consume unbounded RAM.

### 19.5 Existing target model versus database creation

Two distinct use cases should be separated.

#### Import into an existing DbContext/model

This is the simpler and safer first target.

A2 validates the exported model/schema against the target EF Core model and writes entities through the target context/provider.

#### Create a new database from A2

A later capability may create a database/schema directly from the stored A2 model.

This requires more provider-specific handling for:

- SQL types
- identity/sequence behavior
- computed columns
- collations
- defaults
- provider-specific indexes
- constraints
- migration semantics

Therefore logical EF-model round-trip should come before promising exact physical database reproduction across arbitrary providers.

### 19.6 Relationship discovery

The bridge should use EF Core metadata rather than relying only on CLR property shape.

That matters because foreign keys define relationships, navigations provide object-oriented access, collection navigations represent the many side, and many-to-many relationships may use skip navigations with a join entity hidden from ordinary CLR navigation code.

Join entities may also contain payload columns that must not be lost.

### 19.7 Streaming and large databases

Exporting a large database must not mean:

    Include(every navigation)
    -> ToList()
    -> serialize giant object graph

The intended model is:

    read model metadata once
        -> stream entity batches
        -> write A2 entity sets / graph fragments
        -> write/update relationship metadata/indexes
        -> release batch

The same principle applies during import.

For very large relationship maps or key-remapping tables, A2 may use ScratchStore/A2FS-backed temporary indexes.

### 19.8 Database snapshot metadata

A database-oriented A2 document may include metadata such as:

- source framework/provider
- model/schema version
- entity counts
- relationship counts
- key definitions
- export timestamp
- model fingerprint
- database/application schema identifier

This metadata is optional where appropriate, generated automatically when available, and can be validated through the existing CheckMeta/FixMeta concepts.

A model fingerprint can allow a fast compatibility check before a large import begins.

### 19.9 Security / selective export

Database exports may contain fields that should not automatically be placed into a portable backup/export.

Identity-style databases are a particularly important example because password hashes, security stamps, login-provider data, authenticator keys, tokens, and personal data may have different export requirements.

The bridge should therefore integrate with A2's selective protection model and explicit export policies.

Possible actions per property/entity:

- Include
- Exclude
- Encrypt
- Transform
- Redact

The exact defaults should be chosen deliberately for each adapter rather than assuming that database export always means copying every sensitive value in plaintext.

A true backup mode may intentionally preserve everything, but should make that intent explicit.

### 19.10 Database bridge is not an ORM replacement

The EF Core bridge uses EF Core as model/relationship knowledge and database-provider access.

A2 does not need to replace EF Core's change tracking, provider ecosystem, migrations, or general LINQ translation.

The goal is a portable A2 snapshot/document representation with reliable round-trip semantics and bounded-memory processing.

## 20. Logical pointers / references

A2 should support logical references (working name: pointers) so repeated/shared entities can be represented once and referenced many times.

The term pointer is useful conceptually, but the persisted value must not be a raw machine memory address. It is a stable document-level reference identifier that the parser can resolve to an object/value.

Conceptual idea:

    #pointers: {
        roleAdmin: p1,
        roleUser:  p2
    }

    {
        "Roles": {
            p1: { "Id": 1, "Name": "Admin" },
            p2: { "Id": 2, "Name": "User" }
        },

        "Users": [
            {
                "Id": 10,
                "Name": "Peter",
                "Roles": [ *p1, *p2 ]
            },
            {
                "Id": 11,
                "Name": "Anna",
                "Roles": [ *p2 ]
            }
        ]
    }

The exact syntax is not frozen. The important semantic distinction is:

    object/value definition -> assigned logical reference ID
    *p1                     -> reference to that previously/known object/value

### 20.1 Why references matter

Logical references solve several problems at once:

- shared entities are stored once instead of duplicated
- many-to-many relationships map naturally
- cyclic object graphs become representable
- database exports can preserve entity identity
- graph/document views can stay compact
- repeated large subobjects need not be serialized repeatedly
- in-memory readers may resolve references to the same logical object instance where appropriate

Example: one Role entity can be referenced by 50,000 users without serializing 50,000 copies of the role object.

### 20.2 Parser resolution model

A reader may maintain a compact reference table in memory:

    p1 -> Role(Admin)
    p2 -> Role(User)
    p3 -> Address(...)

For a small reference table this can be held directly in RAM.

For very large pointer tables, A2 must remain bounded-memory and may store the lookup table in disk-backed ScratchStore/A2FS indexes.

The existence of references must never imply that every referenced object must remain permanently materialized in RAM.

Possible strategies include:

- direct in-memory reference table for small graphs
- lazy object materialization
- record-ID / offset lookup into A2FS
- disk-backed pointer index
- weak/cache-based materialization for repeated access

### 20.3 Forward and backward references

The simplest initial implementation may require definitions before references:

    define p1
    later use *p1

However, the format may eventually support forward references if the parser can register unresolved references and bind them later.

For streaming and low-memory implementations, backward-only references are simpler and should be considered as a baseline capability.

If forward references are supported, unresolved-reference limits and disk spill must prevent unbounded memory growth.

### 20.4 Pointer table versus inline identity annotation

Two complementary forms may be useful.

Central table:

    #pointers: {
        p1: ...
        p2: ...
    }

Inline identity:

    {
        #id: p1,
        "Id": 1,
        "Name": "Admin"
    }

and reference:

    *p1

The final grammar should choose whether both forms are allowed or whether one is canonical.

A central #pointers section is attractive for early planning and graph inspection. Inline IDs are attractive for streaming because an object can define its identity where it appears.

A hybrid design may use inline definitions while #pointers contains an optional early index/directory of known references.

### 20.5 References are logical identity, not copying

Resolving *p1 should mean 'the same logical entity/value', not 'deserialize a fresh copy of the content'.

This distinction is especially important for:

- database entity identity
- mutable in-memory graphs
- cycles
- deduplication
- equality semantics

Language bindings may expose this differently. For example, .NET may resolve repeated references to the same object instance in graph-materialization mode, while a low-memory C reader may expose the same stable reference ID/record locator without materializing a persistent object instance.

### 20.6 Interaction with database export/import

The database bridge can use A2 references to preserve shared entities naturally.

Example:

    Roles:
      p1 -> Admin
      p2 -> User

    Users:
      Peter.Roles -> [*p1, *p2]
      Anna.Roles  -> [*p2]

During import, the relationship layer resolves p1/p2 to the correct Role primary keys and recreates UserRoles relationships.

This avoids duplicating shared entities in Graph mode while retaining a human-readable object-oriented view.

### 20.7 Interaction with #schema and metadata

#schema may describe reference-capable fields, for example conceptually:

    "Roles": {
        "type": "List<RoleRef>",
        "referenceTarget": "Role"
    }

#meta may contain planning information such as pointer count or preferred pointer-index engine.

Optional metadata examples:

    pointerCount: 2
    pointerEngine: Auto

These values remain optional planning aids.

### 20.8 JSON lossless conversion

JSON has no native reference syntax, so lossless ToJson()/FromJson() should preserve references through #ajisData.

Conceptually, ordinary JSON may carry stable IDs and reference placeholders while #ajisData defines which paths/values represent A2 references.

This allows:

    AJIS shared graph
        -> JSON + #ajisData
        -> JSON-only transport
        -> FromJson()
        -> same logical graph identity

### 20.9 Cycles

References make cyclic graphs representable.

Example:

    p1 -> Person { manager: *p2 }
    p2 -> Person { manager: *p1 }

A2 parsers must detect and handle cycles without recursive infinite materialization.

Graph materializers may create placeholders/identity slots first, then populate members.

Streaming/event readers may expose references symbolically rather than constructing a full cyclic object graph.

### 20.10 Integrity

Reference identifiers must be unique within their declared scope.

A reference to an unknown identifier is a stable validation error unless the selected mode explicitly permits forward references.

Duplicate pointer definitions, incompatible target types, and broken references should be detectable through CheckMeta()/validation tooling.

Reference IDs are document identities, not security capabilities; possession of an ID must not imply authorization to access external resources.

### 20.11 Pointer as a named logical address

The intended A2 pointer concept is more specific than a cache entry and less physical than a C/C++ memory pointer.

A pointer is a named logical address to an A2 value/object stored in the document's reference space.

Conceptually:

    pointer name -> one complete logical A2 value/object

For small shared lookup domains such as Roles, Statuses, Countries, Permissions, Categories, or other dictionary-like entities, the pointer table may contain the complete values near the beginning of the document.

Example:

    {
        #meta: {
            ...
        },

        #pointers: {
            roleAdmin: {
                "Id": 1,
                "Name": "Administrator"
            },
            roleUser: {
                "Id": 2,
                "Name": "User"
            }
        },

        "Users": [
            {
                "Id": 100,
                "Name": "Peter",
                "Roles": [ *roleAdmin, *roleUser ]
            },
            {
                "Id": 101,
                "Name": "Anna",
                "Roles": [ *roleUser ]
            }
        ]
    }

The exact grammar remains open, but the intended semantics are clear:

    roleAdmin   -> pointer definition / named logical address
    *roleAdmin  -> dereference/reference to that logical value
    "roleAdmin" -> ordinary string

### 20.12 Pointer table as an early header structure

#pointers is an early AJIS directive/header section, alongside #meta and #schema, and should be available before ordinary payload data where possible.

A conceptual document shape is:

    {
        #meta: { ... },
        #schema: { ... },
        #pointers: { ... },
        ... ordinary data ...
    }

The final canonical ordering of #meta, #schema, and #pointers remains to be specified, but all are intended to be discoverable early enough for the parser to plan execution and resolve common references efficiently.

For a small pointer table the parser may materialize all pointer values immediately in RAM because the cost is bounded and deliberate.

Example:

    Roles = 6 objects
    Permissions = 24 objects

Keeping these values resident can be substantially cheaper than repeatedly reading or reconstructing them from the payload.

For a very large pointer table the implementation may transparently switch to an indexed disk-backed resolver. Pointer semantics do not change.

### 20.13 Pointer updates and bulk changes

Because references point to one logical value rather than embedding copies, changing a pointer target changes the value observed through every reference to it.

Example:

    roleUser.Name = "Standard User"

does not require finding and rewriting every User record that contains *roleUser.

This enables efficient bulk changes for shared values such as:

- role names
- status definitions
- organization metadata
- category labels
- shared configuration fragments
- common address/location objects where identity is intentional

The update must preserve the distinction between changing the target object and rebinding a pointer name to a different target.

### 20.14 Relationship to database foreign keys

A database foreign key normally stores the identity needed to find another row.

An A2 pointer is conceptually similar, but the A2 document may place the complete referenced object in its pointer/reference area and expose a direct logical reference to it.

Thus for small lookup tables:

    SQL:
      User.RoleId -> lookup row in Roles table

    A2:
      User.Role -> *roleAdmin

The A2 reader may already have roleAdmin materialized, making the lookup effectively immediate.

For large lookup sets, the pointer may resolve through an index/RecordId/offset instead, preserving the same public semantics.

### 20.15 Pointers are not automatically caches

Pointers define identity and reference semantics. Caching is an implementation optimization layered underneath.

A parser may cache pointer targets because they are frequently used, but:

- a pointer remains valid even if its target is not resident in RAM
- eviction from a cache must not change pointer identity
- pointer lifetime is defined by document/reference scope, not cache lifetime
- disk-backed/lazy resolution is allowed

This distinction keeps the data model deterministic while allowing aggressive performance optimization.

## 21. Human-readable writing modes, samples, playground, and reference benchmark

A2/AJIS should deliberately support both compact transport-oriented output and highly readable working/documentation output.

### 21.1 Writer presentation modes

A useful initial model is:

    WriteMode = Compact | Standard | Verbal

Possible semantics:

- Compact: minimize textual overhead for transport/storage while remaining valid AJIS.
- Standard: normal readable canonical AJIS with sensible indentation.
- Verbal: documentation-oriented AJIS with generated explanatory comments and section markers.

The exact enum names are not frozen.

Canonical semantic content must not change between modes. Only formatting, comments, and other non-semantic presentation details change.

### 21.2 Generated structural comments

Because AJIS supports comments, generated documents can use them to improve orientation.

Example Verbal output:

    /**
     * --- META ---
     * Processing hints for the AJIS reader.
     * These values do not change the logical payload.
     */
    #meta: {
        "engine": "Auto"
    },

    /**
     * --- POINTERS ---
     * Named shared values referenced later by *name.
     */
    #pointers: {
        roleAdmin: { "Id": 1, "Name": "Administrator" },
        roleUser:  { "Id": 2, "Name": "User" }
    },

    /* ---- PAYLOAD DATA ---- */

    "Users": [
        ...
    ]

Generated comments are presentation metadata only. They must not affect parsing semantics and may be omitted by Compact/transport writers.

### 21.3 Comment levels

Verbal mode may itself support different explanation levels, for example:

    Comments = None | Sections | Explanatory | Tutorial

Possible intent:

- Sections: only visual separators such as META / SCHEMA / POINTERS / PAYLOAD.
- Explanatory: short descriptions of what each section or unusual construct means.
- Tutorial: include examples and hints suitable for learning the format.

This keeps normal readable files concise while allowing sample/tutorial generation to be intentionally verbose.

### 21.4 GetSample(...) as executable documentation

A2 should provide a sample-generation API so developers can learn by generating valid examples instead of reading only long prose documentation.

Working API concept:

    var sample = A2.GetSample(parameters);

or:

    var sample = A2.Samples.Create(options);

Final naming is not frozen.

Parameters may select features such as:

- schema
- meta
- pointers
- conditions
- tuples
- unions
- binary values / attachments
- encryption annotations
- paging metadata
- database graph example
- server/client profile example
- Compact / Standard / Verbal output

Example concept:

    A2.GetSample(new() {
        Pointers = true,
        Schema = true,
        Conditions = true,
        WriteMode = Verbal
    });

The generator should use the real production serializer/format model so examples cannot silently drift away from actual parser behavior.

### 21.5 A2 website / interactive playground

A2 should have a first-party web playground developed alongside the specification and implementations.

The playground should use the real A2 implementation where practical rather than a separately reimplemented demo grammar.

Useful panes/features:

- source AJIS editor
- resolved document view
- JSON lossless conversion view
- parsed tree/schema view
- metadata/pointer view
- validation result
- query result
- execution plan
- Compact / Standard / Verbal formatting switch
- sample selector
- editable parameters/context
- ToJson / FromJson round-trip demonstration
- download/export of generated examples

A beginner should be able to open the site, select a feature, change a value, and immediately see how AJIS behaves.

This interactive documentation is especially important as A2 grows beyond simple JSON-like serialization.

### 21.6 Documentation and playground share samples

Samples shown in written documentation, tests, CLI help, and the web playground should come from one shared sample catalog where feasible.

Conceptually:

    A2.SampleCatalog
        -> Docs
        -> GetSample()
        -> a2tool sample
        -> web playground
        -> conformance/demo tests

This reduces documentation drift and turns examples into testable artifacts.

### 21.7 WideWorldImporters as a large reference database

A strong end-to-end demonstration/benchmark candidate is Microsoft's WideWorldImporters sample database.

It provides a non-trivial relational model with realistic tables, relationships, transactional data, and enough complexity to exercise:

- DB -> A2 export
- A2 -> DB import
- indexes
- joins
- filtering
- projections
- server profile
- bounded-memory operation
- EF Core metadata mapping
- query-plan comparison

AdventureWorks can remain a secondary/alternate sample, but WideWorldImporters is a particularly suitable primary test target because it was designed as a modern SQL Server sample and includes OLTP and analytics-oriented material.

### 21.8 Dual-backend reference application

A2 should eventually include a reference application exposing the same logical operations through two interchangeable backends:

    Backend A: SQL Server / EF Core
    Backend B: A2 / A2FS

Both backends operate on equivalent data exported from the same source database.

Example operations:

- find customer/user by key
- list records for one organization/category
- indexed filter
- simple join
- ordered paging
- projection
- insert/update/delete
- bulk read/export

The application should compare results for correctness first, then optionally measure performance/resource use.

Important measurements include:

- latency
- throughput
- peak working memory
- allocated memory
- disk reads/writes
- index size
- cold/warm behavior
- concurrent request behavior

The goal is not to claim that A2 universally beats SQL Server. The goal is to identify the workload boundary where an embedded A2 engine is simpler or more resource-efficient, and where a mature DBMS remains the better tool.

### 21.9 Semantic equivalence testing

For operations supported by both backends, the reference app/test suite should assert semantic equivalence:

    SQL result == A2 result

before comparing speed.

This makes the SQL implementation an independent behavioral oracle for relational-style features such as simple joins, filtering, ordering, and paging.

### 21.10 Scaling the benchmark

WideWorldImporters can be exported once at normal size and also expanded/generated into much larger A2 data sets for stress testing.

Useful scales:

- original sample size
- 10x
- 100x
- larger-than-RAM
- ~150 GB A2FS stress target

The same query corpus should run against both backends where practical.

Server profile tests should particularly verify that A2 does not consume memory proportional to total data size.

## 22. A2 Identity as the first reference library

A2 should not try to reproduce every database feature. Its target is a deliberately smaller storage/query model that is highly efficient for workloads where A2's strengths matter: indexed lookup, simple relationships, bounded joins, paging, streaming, and predictable resource use.

The first major reference library should be **A2 Identity**.

The purpose of A2 Identity is twofold:

1. provide a useful production-oriented ASP.NET Core Identity storage provider backed by A2/A2FS
2. act as the first serious end-to-end proving ground for A2 storage, indexing, relationships, binary values, concurrency, recovery, and server execution profiles

### 22.1 Compatibility target

A2 Identity should aim to be usable through the normal ASP.NET Core Identity manager/store model rather than inventing a separate authentication API.

Current ASP.NET Core Identity explicitly supports custom persistence stores. Its high-level managers are separated from storage through interfaces such as IUserStore<TUser>, IRoleStore<TRole>, IUserRoleStore<TUser>, and the other capability-specific user store interfaces.

A2 Identity should therefore implement the relevant Identity store contracts so existing application code can continue to use UserManager<TUser>, RoleManager<TRole>, SignInManager<TUser>, role checks, claims, logins, tokens, passkeys, lockout, two-factor features, and other supported Identity capabilities according to the interfaces implemented by the store.

The exact .NET 11 surface must be verified against the final released framework before freezing A2 Identity 1.0 APIs.

### 22.2 Storage model

A2 Identity should use A2FS rather than a giant in-memory AJIS object graph.

Conceptual data sets:

    Users
    Roles
    UserRoles
    UserClaims
    RoleClaims
    UserLogins
    UserTokens
    UserPasskeys / related capability data where required

Small lookup domains such as Roles may be represented through A2 pointer/reference semantics and kept resident when practical.

Large collections such as Users remain A2FS-backed and indexed.

Likely important indexes include:

- User.Id
- User.NormalizedUserName
- User.NormalizedEmail where required
- Role.Id
- Role.NormalizedName
- UserRoles.UserId
- UserRoles.RoleId
- claims/login lookup keys required by Identity operations

Uniqueness rules required by Identity must be enforced explicitly by the A2 storage layer.

### 22.3 User images as a native extension

A2 Identity may add first-class user images/avatars as an A2-specific extension.

Unlike a conventional relational schema where an avatar is often stored externally or as a BLOB column, A2 can keep the binary value or attachment in the same logical user store.

Conceptually:

    User
      Id
      UserName
      ...
      Avatar -> binary / attachment

The avatar should remain optional and must not be loaded when ordinary Identity operations need only credentials, normalized names, roles, or claims.

A2FS projection/column separation should ensure that:

    FindByNameAsync(userName)

does not read megabytes of avatar data.

This is an important A2FS conformance requirement: large binary fields must not penalize queries that do not project them.

### 22.4 Identity workload is deliberately narrow

A2 Identity does not need SQL arithmetic, arbitrary aggregation, complex multi-table joins, stored procedures, or a general SQL dialect.

The important operations are closer to:

- unique indexed lookup by user ID/name/email
- indexed lookup by role name
- add/remove role membership
- retrieve roles for one user
- retrieve users for one role
- claims/login/token/passkey lookup and mutation
- create/update/delete user
- paging/search for administration UI
- one or two bounded relationship traversals
- concurrency/version checks

This workload is an excellent match for the intended A2 query/storage scope.

### 22.5 Primary scalability question

The benchmark should answer a concrete question:

> How many Identity users can A2FS manage while keeping common Identity operations fast enough and server memory bounded?

This should be measured rather than guessed.

Candidate test scales:

    1,000 users
    10,000 users
    100,000 users
    1,000,000 users
    10,000,000 users
    larger if results remain useful

Each scale should include realistic role/claim/login/token distributions and optionally avatars of multiple sizes.

### 22.6 Critical benchmark operations

At every scale, measure at least:

- FindById
- FindByNormalizedUserName
- FindByNormalizedEmail
- Create user
- Update user
- Delete user
- IsInRole
- GetRoles
- GetUsersInRole
- AddToRole / RemoveFromRole
- claims lookup/update
- login/token/passkey-related operations supported by the target Identity version
- ordered/paged administrative user listing
- concurrent login-like reads
- concurrent mixed read/write workload
- avatar lookup separately from normal user lookup

Measure:

- p50/p95/p99 latency
- throughput
- peak process working memory
- A2-owned buffer/memory usage where observable
- allocations
- disk I/O
- index size
- total A2FS size
- cold-start behavior
- warm-cache behavior
- startup/open time
- recovery time after simulated interruption

### 22.7 Comparison target

The reference application should expose the same logical ASP.NET Core Identity operations through two storage providers:

    Microsoft/EF Core Identity store
    A2 Identity store

The first assertion is correctness/behavioral compatibility.

Only after equivalent Identity behavior is demonstrated should resource/performance comparisons be made.

The useful result is not 'A2 wins'. The useful result is a measured operating envelope such as:

    A2 Identity remains comfortable up to X users on profile Y / hardware Z
    A2 Identity remains functional but latency changes above X
    for workload Q a conventional database becomes the better choice

### 22.8 Hardware profiles

Benchmarks should include several deliberately different machines/profiles.

Useful examples:

- constrained client/workstation: 4 GiB RAM
- ordinary small server
- modern workstation/server with abundant RAM
- slow SATA/HDD or old NAS storage
- SSD/NVMe storage

This helps determine whether the A2 Server profile and disk-first design actually keep memory independent of total user count.

### 22.9 Server profile expectations

A2 Identity should normally use Profile=Server.

Desired behavior:

- persistent indexes on disk
- bounded per-request buffers
- no whole-user-store materialization
- rapid release/reuse of temporary request resources
- avatars/binary payloads fetched only when requested
- role pointer values may remain resident because the domain is tiny
- controlled shared caches
- WAL/crash-safe mutations
- deterministic concurrency handling

### 22.10 Demo application

A2 Identity should ship with or be accompanied by a small reference web application.

The same UI/application behavior should be runnable with either backend through configuration:

    IdentityBackend = EfCore

or:

    IdentityBackend = A2

The demo should exercise registration, login, roles, claims, administration, user search/paging, avatar upload/display, and representative concurrent requests.

This makes A2 Identity both a practical package and a continuously executable demonstration of the A2 architecture.

### 22.11 Success criteria

A2 Identity should be considered successful when:

- normal ASP.NET Core Identity application code needs minimal/no changes beyond DI/store configuration
- supported Identity store contracts have conformance tests
- storage remains crash-safe and transactionally consistent
- memory usage is bounded as user count grows
- user images do not penalize ordinary identity lookups
- indexed role/user operations remain predictable
- the practical user-count envelope has been measured and documented on known hardware

The project should explicitly publish benchmark limits rather than implying unlimited scale.

### 22.12 Target deployment scale and operational simplicity

A2 Identity is primarily aimed at ordinary web sites, internal applications, small-to-medium services, and self-hosted deployments rather than hyperscale cloud identity infrastructure.

A realistic first operating target is not hundreds of millions of users. A much more valuable target is to make tens of thousands of users boring, predictable, and easy to operate.

A representative practical milestone is:

    50,000 users

with normal Identity operations remaining fast, memory usage bounded, indexes healthy, and deployment/backup simple.

If A2 Identity scales comfortably beyond that, the measured envelope should be documented, but the design should not become needlessly complex merely to chase hyperscale workloads outside the intended audience.

### 22.13 Single-file operational model

One of the strongest A2 Identity advantages should be operational simplicity.

The preferred deployment model should allow the complete logical identity store to be represented as a single portable A2/A2FS container where practical.

Conceptually:

    identity.a2fs

may contain:

- users
- roles
- user-role relationships
- claims
- logins
- tokens/passkeys where supported
- indexes
- metadata
- avatars/binary user data
- recovery/journal state as appropriate to the selected storage layout

The operator experience should be as close as safely possible to:

    stop application
    copy one file
    start application

for an offline backup, migration, or deployment transfer.

This is a major product goal rather than merely a file-layout convenience.

### 22.14 Copyability versus consistency

The simple one-file story must still respect transactional consistency.

For a guaranteed offline copy, the application/store should be stopped or placed into an explicit quiescent/snapshot state before copying.

For online backup, A2 should provide a snapshot/export operation that produces a transactionally consistent copy while the live store continues running where supported.

A raw file copy taken during active mutation must not be advertised as safe unless the storage format explicitly guarantees that behavior.

Potential APIs/tooling may include:

    A2Identity.BackupAsync(destination)
    a2tool identity.a2fs snapshot backup.a2fs

Final naming is not frozen.

### 22.15 Portability as a first-class feature

The same A2 Identity file should be portable across machines and deployments without requiring a separate database server installation.

Typical migration scenario:

    stop old instance
      -> copy identity.a2fs
      -> copy application/configuration
      -> start new instance

Provider-specific server objects, migration history tables, connection-string changes, and external DB provisioning should not be required merely to move the identity store.

Security-sensitive key material that is intentionally external to the data store remains separate where appropriate; portability must not weaken key-management boundaries.

### 22.16 Benchmark emphasis for the intended audience

The most meaningful benchmark points for A2 Identity should therefore include:

    1,000 users
    5,000 users
    10,000 users
    25,000 users
    50,000 users
    100,000 users

before moving into million-user stress tests.

For the intended audience, success at 50,000 users with low operational complexity may be more valuable than extreme-scale headline numbers.

The benchmark should answer not only 'how fast is it?' but also:

- how much RAM does the server need?
- how large is the store/index footprint?
- how long does startup/open take?
- how quickly can a consistent backup be made?
- how quickly can the store be restored/moved?
- how much maintenance does the store require?
- can a small server host the application and identity store comfortably together?

These operational measurements are part of A2 Identity's value proposition.

### 22.17 Free/open A2 Identity distribution

A2 Identity is intended to be freely usable by anyone as part of the A2 ecosystem.

The exact repository/package license should be selected explicitly when the dedicated A2/A2 Identity repositories are created, but the product goal is that the A2 Identity library, A2FS storage provider, tooling, and built-in backup facilities do not require a commercial database license or a paid identity-storage module.

This is an operational/deployment advantage rather than a claim that ASP.NET Core Identity itself is proprietary; ASP.NET Core Identity is open-source and supports custom persistence providers.

### 22.18 Built-in backup scheduler

A2 Identity should include a first-class backup scheduler as part of the normal product, not as a separate optional utility.

The scheduler should support at least two related but distinct protection modes:

1. Mirror replication
2. Versioned point-in-time backups

These solve different failure modes and should normally be used together.

#### Mirror replication

A mirror keeps a second copy of the current logical store on another target such as:

- another physical disk
- another volume
- NAS/network path
- removable backup target
- remote A2 transport endpoint

Conceptually:

    primary identity store
        -> committed generation N
        -> mirror generation N

The mirror is intended for rapid recovery from primary-disk/device failure.

For a single-file deployment, the mirror target may also be a single TP/A2 container.

#### Versioned backups

A mirror is not sufficient protection against logical mistakes or corruption because deletion/corruption can also be mirrored.

The scheduler therefore needs retained generations/snapshots, for example:

    identity-2026-09-29T120000Z.tp
    identity-2026-09-29T130000Z.tp
    identity-2026-09-29T140000Z.tp

or an equivalent generation-based repository.

Retention policy examples:

- keep last N backups
- hourly for 24 hours
- daily for 30 days
- weekly for 12 weeks
- monthly for 12 months

Final defaults remain open.

### 22.19 TP as the portable backup artifact

A2 Identity backups should be able to use the TP family as the portable backup artifact.

Examples:

    identity.tp   -> plain snapshot
    identity.tpg  -> GZip-compressed snapshot
    identity.tpp  -> password-protected snapshot
    identity.tpe  -> key/certificate-protected snapshot
    identity.tps  -> signed snapshot

Protection/compression/signing capabilities may be combined according to the TP container rules.

A backup TP may contain the complete logical A2FS Identity store, metadata, indexes or rebuild metadata, and any required recovery information.

The preferred restore experience is:

    stop/quiesce app
      -> select backup TP
      -> validate
      -> restore/swap store
      -> start/resume app

### 22.20 Consistent snapshots

Every scheduled backup must represent a transactionally consistent generation.

The backup scheduler should integrate with A2FS generation/WAL/snapshot facilities rather than copying an actively mutating file blindly.

Conceptual flow:

    request snapshot
      -> establish generation G
      -> continue live writes to newer WAL/generation
      -> copy/export stable G
      -> validate backup
      -> publish backup atomically

This allows online backups without stopping the web application where the storage engine supports snapshots.

For very small/simple deployments, an explicit short quiesce window is also acceptable.

### 22.21 Efficient mirroring for large stores

Repeatedly copying a multi-gigabyte TP/A2FS file in full after every small Identity mutation would be wasteful.

The mirror mechanism should therefore be able to replicate only committed changes where the storage engine supports it.

Possible mechanisms include:

- WAL/journal segment replication
- changed-page/chunk replication
- append-only generation segments
- block/hash based delta copy

The mirror target then advances from generation G to G+1 without retransmitting unchanged data.

A full verified copy remains available as a fallback/reseed mechanism.

### 22.22 Backup verification

A backup is not complete merely because bytes were copied.

The scheduler should verify at least:

- container/header validity
- expected generation ID
- checksums/hashes
- required metadata/index structures
- ability to open the snapshot read-only

Stronger verification modes may perform CheckMeta() and selected Identity consistency checks.

The scheduler should record the last successful verified backup generation and expose it through diagnostics/tooling.

### 22.23 Scheduling and health

A2 Identity should expose scheduler configuration through normal application configuration and tooling.

Conceptual settings:

    Enabled
    MirrorTarget
    SnapshotTarget
    Interval
    RetentionPolicy
    Compression
    Protection
    VerificationLevel

The service should expose health information such as:

- last successful mirror generation
- last verified backup time
- current backup generation
- mirror lag
- failed backup count
- last failure reason
- available target space

This is especially important for small deployments where there may be no dedicated DBA monitoring the system.

### 22.24 Recovery workflow

Recovery should be intentionally simple and scriptable.

Conceptually:

    a2tool identity status
    a2tool identity backups list
    a2tool identity restore <backup>

A2 Studio may provide the same operations graphically.

The restore operation should validate the selected backup before replacing the live store, preserve the old live store until the new one is committed, and use atomic swap/rename semantics where supported.

The key product goal is that a small-site operator can understand and recover the Identity store without specialist database-administration knowledge.

## 23. Future migration

When the dedicated A2 repository is created:

1. Review every item in this file against the final A2 goals.
2. Separate normative semantics from implementation ideas.
3. Move normative material into the A2 Specification.
4. Turn testable requirements into Conformance vectors/tests.
5. Move implementation-specific .NET/C/Rust details into their respective implementation plans.
6. Create and maintain the real A2 `IMPLEMENTATION_STATUS.md`.
7. Remove this temporary `Ajis` folder from Toolbox once it is no longer needed.
