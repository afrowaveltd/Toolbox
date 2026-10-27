# Requirements and gap inventory (AJIS 1 → Backpack)

Each row is an *architectural comparison*, not an assertion that the corresponding Backpack code exists.

| Capability | AJIS 1 reference / status | Backpack idea | Verification / next action |
|---|---|---|---|
| Ordinary JSON-shaped object | Existing text parser/serializer | Keep minimal and familiar | Add JSON round-trip + external STJ equivalence fixtures. |
| Streaming | StreamWalk visitor/events, streaming docs | Default low-memory behavior | Measure memory as input grows; test nonseekable streams. |
| Relaxed syntax | `AjisTextMode` Lex/Lax, StreamWalk Lax | Preserve AJIS precedence in recovery | Test `"key"`, unquoted `key`, reserved `#name` and quoted `"#name"`. |
| Comments | Lexer supports comments; some conversion modes intentionally discard | Optional **lossless** preservation and re-creation | Add source-position / trivia retention tests. |
| Directives | AJIS 1 has `#namespace command` parser/applier prototype; various historical docs | **Single `#` directive attached to next element** | Decide grammar and scope; migration fixture for legacy form. |
| Named pointers | No verified first-class `*symbol` pointer implementation in reviewed AJIS 1 files | Prolog-declared typed named references | Define ownership, rebinding/version semantics and binary handles. |
| Binary attachments | `BinaryAttachment` contains `byte[] Data`, metadata, checksums; ATP has a text+binary model | Framed/offset-backed binary objects and streaming | Avoid RAM-only attachment design; define lengths upfront. |
| Default naming | `AjisSettings.Naming = PascalCase` in inspected code | **camelCase by default for new serialization**, configurable | Preserve original names for lossless edits. |
| File/package format | Existing ATP docs; other old plans mention `.tp/.tpg/.tpp/.tpe/.tps` | Working uncompressed AJIS/AjisFS, compressed distribution | **Unresolved** `.gtp` (conversation) vs `.tpg` (older notes), header compatibility. |
| Conditions | Older A2 notes mention `#if/#else/#endif` concept | `#if/#elseIf/#else`; no loops/try-catch | Formalize condition contract and branch ordering. |
| Handler / default delegate | Core extension infrastructure partial / requires inspection | Model-local predicates and handlers **as well as** optional DI/app registrations; default fallback | Define priority and result union, plus equivalent idiomatic JS callbacks. |
| JavaScript / TypeScript | No verified first-class JS AJIS implementation in reviewed AJIS 1 .NET sources | Full JS and TS interoperability, browser-first convenience API | Shared syntax fixtures; browser streams, Node adapters; no forced DI/React/SignalR dependency; optional JSLinq/SignalR integration. |
| Strict/Smart/Preserve | Lexical modes already present, semantic layer not verified as proposed | Handler fallback policy distinct from lexical mode | Define error versus preservation behavior. |
| AddOnly revisions | Old A2FS notes discuss updates/journals | Append-only logical revision history, provenance, conflict branches | Design atomic commits and reader snapshots. |
| Concurrent access | No verified AJIS 1 transactional object concurrency contract | Independent agent workspaces / shared version checks | Test physical index/header allocation and crash recovery. |
| Offset-based AJIS FS | Older binary/ATP concepts, A2FS in Toolbox prior notes | Uncompressed offsets relative to binary data start | Formalize object/index/trailer layout. |
| Update / FixAjis | CLI/repair concepts in historical A2 notes | Disk-backed tolerant repair + canonical reassembly | Unknown elements survive; no guessed boundaries. |
| Encryption/signatures/gzip | Some attachment-level compression fields; old design docs | Composable optional serializer capabilities | Separate framing from codec/crypto; test authenticated integrity. |
| Agent whiteboard | New discussion concept | `#agent` root directive, shared and personal workspaces | Define portable schema without granting host permissions. |
| Portable environment bindings | Not established in AJIS 1 baseline | Desktop/browser/server/temp/RAM-only policy resolution | Preserve functional data if persistence unavailable. |
| Model scaffolding | EF/database-first ideas in old roadmap | Generate C# models from unknown AJIS structures | Analyze schema uncertainty, preserve unknown extensions. |
| Hardware Revival | New use case | Research, evidence, compiled artifacts and per-OS packages | Keep schema separate from AJIS Core; clarify redistribution. |

## Existing documents to bring forward

- Toolbox [`Ajis/DESIGN_NOTES.md`](../../Ajis/DESIGN_NOTES.md): profiles vs capabilities, `ScratchStore`, huge datasets, transport and trust, native paging, A2FS, source generation and testing. 164 KB of prior concept notes — **do not delete**.
- AJIS 1 [format norms](https://github.com/afrowaveltd/Ajis.Dotnet/blob/master/Docs/03_Format_Norms.md), [core API](https://github.com/afrowaveltd/Ajis.Dotnet/blob/master/Docs/07_Core_API.md), [streaming](https://github.com/afrowaveltd/Ajis.Dotnet/blob/master/Docs/09_Streaming.md), [ATP](https://github.com/afrowaveltd/Ajis.Dotnet/blob/master/Docs/12_ATP_Text_Binary_Container.md), [mapping](https://github.com/afrowaveltd/Ajis.Dotnet/blob/master/Docs/13_AJIS_Settings_And_Mapping.md).
- C implementation [`AJIS_ATP/docs`](https://github.com/afrowaveltd/AJIS_ATP/tree/main/docs) for low-memory format invariants.

## Not current scope

- Porting AJIS 1 code into Backpack without review.
- Shipping a compiled driver, decryptor, executable policy engine, production MCP server or production object store.
- Publishing compatibility/performance claims before tests.
