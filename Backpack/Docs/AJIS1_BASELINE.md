# AJIS 1.x baseline — repository review

**Review date:** 2026-10-27. **Source:** [afrowaveltd/Ajis.Dotnet](https://github.com/afrowaveltd/Ajis.Dotnet) `master`, plus [afrowaveltd/AJIS_ATP](https://github.com/afrowaveltd/AJIS_ATP) `main`.
This is a source-based **orientation**, not a full execution audit. Historical docs frequently distinguish planned contracts from implemented features, and some descriptions conflict.

## Verified in repository source

| Area | AJIS 1 evidence | Backpack implication |
|---|---|---|
| Basic text model | `src/Afrowave.AJIS.Core/` and the separate `Serialization` and `Streaming` projects | Preserve the easy JSON-like entry point and small parser contract. |
| Numeric extensions | [Format norms](https://github.com/afrowaveltd/Ajis.Dotnet/blob/master/Docs/03_Format_Norms.md) cover decimal/binary/octal/hex and separators; lexer fixtures exist | Reuse conformance fixtures and decide which literal forms remain canonical. |
| Comments | AJIS/Lex lexer handling and tests described in [streaming docs](https://github.com/afrowaveltd/Ajis.Dotnet/blob/master/Docs/09_Streaming.md) | Comments remain preservable metadata when requesting lossless round trips. |
| Tolerant mode | `src/Afrowave.AJIS.Streaming/Walk/StreamWalkMode.cs` explicitly defines `Lax`; `AjisTextMode.cs` has `Json`, `Ajis`, `Lex`, `Lax` | Do **not** confuse lexical recovery (Lex/Lax) with semantic Strict/Smart/Preserve. |
| One-pass event processing | `Streaming/Walk/StreamWalkContracts.cs` defines event + offset and visitor interfaces; [StreamWalk checklist](https://github.com/afrowaveltd/Ajis.Dotnet/blob/master/Docs/20_StreamWalk_Contract_Checklist.md) records both implemented and pending tests | Reuse visitor/event concepts; independently verify stream parity and memory behavior. |
| Attachment model | `src/Afrowave.AJIS.Core/BinaryAttachment.cs`: attachment ID, name/MIME/checksum, compression info and **`byte[] Data`** | New streaming/offset-backed binary representation must not mandate `byte[]`. |
| Transport | [ATP text+binary](https://github.com/afrowaveltd/Ajis.Dotnet/blob/master/Docs/12_ATP_Text_Binary_Container.md) describes header, AJIS text, binary segments and lengths | Reconcile with future native binary object layout; distinguish implemented variants from proposals. |
| Configuration | `src/Afrowave.AJIS.Core/Configuration/AjisSettings.cs` has `TextMode`, resource profiles, `AllowDirectives`, limits and naming | Existing `Naming` **defaults PascalCase**; Backpack intent is serializer **camelCase**, configurable. |
| Directive prototype | `src/Afrowave.AJIS.Core/Directives/AjisDirectiveParser.cs` parses legacy `#namespace command key=value` text; `AjisDirectiveApplier.cs` recognizes AJIS mode | **Important:** previous conversational shorthand that “AJIS 1 has no directives” is not literally true of this source tree. Existing prototype is *not* the Backpack design. |
| C implementation | [AJIS_ATP](https://github.com/afrowaveltd/AJIS_ATP) contains C11 lexer/parser, AST, serializer, IO, ATP code, docs and tests | Consult low-memory C decisions before fixing new wire assumptions. |

## Historical documentation versus verified runtime

- [AJIS 1 FINAL completion](https://github.com/afrowaveltd/Ajis.Dotnet/blob/master/Docs/FINAL_v1_COMPLETION.md) reports successful ATP round trips, streaming, JSON migration, benchmarks and tests, but these are **historical project statements**; no fresh tests were run for this review.
- [AJIS Parser Modes](https://github.com/afrowaveltd/Ajis.Dotnet/blob/master/Docs/06_Parsers_and_Modes.md) defines SmallFiles/BigFiles/ExtraFast/Auto as strategy profiles. These are **not** the same as strictness or preservation policies.
- [AJIS Serialization](https://github.com/afrowaveltd/Ajis.Dotnet/blob/master/Docs/08_Serialization.md) discusses canonical output, functional/deterministic round-trip exports and directives. Some of these are normative *design text*, not necessarily current runtime behavior.
- [M11 Binary Format Design](https://github.com/afrowaveltd/Ajis.Dotnet/blob/master/Docs/M11_Binary_Format_Design.md) contains an earlier binary-format design including claimed performance figures; **do not import those figures or layout as verified implementation or final wire spec**.
- [AJIS 1 Roadmap](https://github.com/afrowaveltd/Ajis.Dotnet/blob/master/Docs/00_Complete_Roadmap_v1_to_v2_Plus.md) contains an older vision for AJIS 2. Backpack supersedes only where explicitly decided.

## Preserve / review

1. Preserve real behavior in JSON-compatible basic documents and round-trip tests.
2. Preserve error location reporting, streaming readers and low-memory intent.
3. Review how historical `#` directives interact with a **single `#` prefix attached to next element**. The existing syntax cannot simply be assumed equivalent.
4. Review and reconcile naming policies and the distinction between `Lex` and `Lax` in source and tests.
5. Check existing numeric grouping, escape, signed/binary packaging, and stream fixture behavior before changing it.
6. Review current legal and licensing metadata before copying code between repositories.

## Baseline is not migration

Backpack begins with documentation and an empty `.NET 11` project; AJIS 1 continues to exist independently. A compatibility matrix and actual tests must decide which readers can round-trip which newer features. Unknown-construct preservation is a *goal*, never an unconditional guarantee for syntactically unframeable future data.
