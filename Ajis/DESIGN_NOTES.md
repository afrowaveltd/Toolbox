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


## 12. Optional embedded schema header

A2/AJIS should support an optional schema declaration at the beginning of a document, tentatively represented by a reserved field/directive such as `#schema`.

The schema is not required for ordinary self-describing AJIS data, but when present it can act as an early contract and planning hint before the parser starts consuming the full payload.

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

### 12.2 Anonymous/self-describing objects

An embedded schema allows A2 to transport data for which the receiver has no precompiled CLR/C/Rust/Java model.

A receiver can:

1. read `#schema`
2. construct a runtime type/schema descriptor
3. prepare suitable storage
4. stream and validate values according to that descriptor

This enables genuinely anonymous/self-describing data exchange while retaining strong type information.

A .NET consumer may expose a dynamic/runtime record abstraction rather than requiring a generated CLR class. Other language implementations should provide equivalent idiomatic runtime-schema access.

### 12.3 Schema and compiled models

When the consumer already has a target model, the embedded schema may be used to verify compatibility before the payload is consumed.

Possible outcomes include:

- exact match
- compatible match
- compatible with conversions
- missing/extra optional fields
- incompatible schema

The exact compatibility/versioning rules must be defined later in the normative specification.

### 12.4 Schema and streaming

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

### 12.5 Schema and security

Schema visibility is part of the protection model.

- Opaque protection may hide the schema with the payload.
- Values protection may intentionally expose the schema while encrypting values.
- Selective protection may expose schema metadata indicating which fields are protected.

Because field names and types can themselves reveal sensitive information, exposing `#schema` must remain an explicit security choice rather than an accidental side effect.

### 12.6 Schema identity and reuse

A future optimization may allow schemas to carry a stable identifier/version or to reference a known schema by ID.

This could reduce repeated schema transmission in long-lived streams or repeated TP messages, while still permitting the full schema to be embedded when portability/self-description is more important.

The exact schema-ID, hashing, canonicalization, and versioning rules are intentionally left open for the future specification.


## 13. Future migration

When the dedicated A2 repository is created:

1. Review every item in this file against the final A2 goals.
2. Separate normative semantics from implementation ideas.
3. Move normative material into the A2 Specification.
4. Turn testable requirements into Conformance vectors/tests.
5. Move implementation-specific .NET/C/Rust details into their respective implementation plans.
6. Create and maintain the real A2 `IMPLEMENTATION_STATUS.md`.
7. Remove this temporary `Ajis` folder from Toolbox once it is no longer needed.
