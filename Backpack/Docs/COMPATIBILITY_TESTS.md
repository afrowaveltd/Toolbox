# Compatibility / conformance tests to implement

**Not yet implemented.** Tests should be executable fixtures shared across .NET/C/other implementations whenever possible.

## Syntax & plain values

- Ordinary JSON object/array/primitive works with no optional features.
- Quoted `"#directive"` remains an ordinary field; unquoted `#directive` is special only according to the new grammar.
- Lexer handles escaped quotes, commas/colons in strings, nested arrays/objects and multi-line comments correctly.
- All supported non-decimal bases and grouping patterns match AJIS 1 norms.

## Tolerant import vs strict mode

- Strict canonical AJIS rejects ambiguous malformed source rather than guessing.
- Lex/Lax import can recover supported unquoted JS keys without overriding recognized AJIS constructs.
- Unknown directives/fragments survive a lossless repair when safely frameable.
- Contrast *lexical recovery* versus *semantic Preserve*, test their Cartesian combinations.

## References

- `*name` declarations appear before root; using an undefined forward pointer fails strict streaming with a precise diagnostic.
- Repeated references preserve type, large payloads are streamed/spooled not duplicated in RAM.
- Test alias, cycles, duplicate declarations, undefined symbols, nested references and versions when semantics are specified.

## Binary / native / TP

- Length is available before payload and lets reader skip unknown blob with constant memory.
- Native object offset counted from data-area start, never disk LBA.
- Move file between directories/devices without invalidating relative offsets.
- Gzip transfer round trip preserves uncompressed binary AJIS bytes.
- Reader cannot depend on trailing annotations to decode earlier payload.
- Crash between payload append and metadata commit recovers last valid snapshot.

## Conditional directives

- First selected branch takes effect; others retained on lossless write.
- Default handler receives a framed element when no registered handler processes it.
- Untrusted document cannot execute arbitrary code or grant itself IO permission.

## Concurrency and history

- Distinct workspace writes do not overwrite each other.
- Shared object concurrent edits detect revisions and retain both proposed branches.
- AddOnly history survives compaction and approved snapshot creation.
- Unknown metadata remains attached to correct logical element across repack and update.

## Scaffold / round trip

- C# generated model round-trips source names, binary refs and unknown properties.
- Preserve-mode edits of a known field do not drop unrelated unknown directives.
- Missing, nullable and union-typed fields are correctly marked as inferred versus certain.
