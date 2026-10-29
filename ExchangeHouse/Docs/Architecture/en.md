# ExchangeHouse architecture and dependency boundaries

**Status: first Markdown block-scanning foundation; extended formats/converters are planned.**

## Ownership

- `ExchangeHouse.Markdown`: parse/inspect/write Markdown, document structure and source preservation. No UI framework.
- `Blackboard.SpectreConsole.Markdown` (planned): render parsed Markdown in an `IAnsiConsole` with injectable heading, emphasis, code and link themes. It references ExchangeHouse.Markdown, never the reverse.
- `UnderstandMe/Afutrans`: own translation workflow, preservation of URLs/emails/code, language-aware links, dual English/original references, structural validation and publication decisions. Reusable format-neutral logic may later move here only when reuse warrants it.
- Converters: independent packages as needed, depending only on source and destination format components. No generic `IConverter` or shared AST unless actual usage demonstrates a stable contract.

## Requirements for format parsers

- Safe handling of untrusted data: no execution of HTML/script, no unsolicited network/file access, no arbitrary link opening.
- Preserve exact source when no changes are requested; track offsets and original line endings for diagnostic and rewrite workflows.
- Distinguish syntax and translatable content instead of simply replacing text via regular expressions.
- Be explicit about unsupported syntax and experimental features. CommonMark compatibility is a long-term goal, not a first release guarantee.
- Low-allocation and streaming capabilities may be introduced based on measured needs; do not force all data to be resident in memory forever.

## Converter strategy

Conversions are directed edges, not an automatic claim that a pair of formats is perfectly reversible. For example, Markdown to plain text loses link destinations and emphasis unless explicitly retained as annotations, while JSON to XML must decide how to represent arrays, scalar types and nulls. AJIS attachments, directives and aliases may have no faithful JSON/XML equivalent. Therefore future converter policies should include explicit loss behavior and diagnostics.

Keep the format-specific classes modular and avoid coupling conversion code to presentation libraries. See [Converters](../Converters/en.md).
