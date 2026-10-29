# Markdown block scanner and lossless writer

**Status:** first implementation 2026-10-29, tested by ExchangeHouse.Markdown.Tests; GitHub CI verification is tracked separately in `IMPLEMENTATION_STATUS.md`.

`MarkdownParser.Parse(string)` returns an immutable source snapshot and read-only line token collection. Each token exposes its kind, text for inspection, heading level (where applicable), exact raw text (including original line terminator), source offset and length. Parsing never alters the input or follows links.

The scanner recognizes Markdown headings with up to three leading spaces, unordered markers (`-`, `+`, `*`), basic ordered markers, fenced code delimiters (backtick or tilde) and code lines. Content inside fences is not reinterpreted as headings or lists. This is not yet a CommonMark AST: multiline grouping, closing ATX suffixes, indentation subtleties, HTML blocks, inline syntax and advanced lists are not implemented.

`MarkdownWriter.Write(document)` returns the unedited original content by concatenating raw token spans. It preserves CRLF, LF, CR and missing final newline exactly. This **does not yet support editing nodes and regenerating Markdown**; transformation contracts will be designed before implementing converters or translation segments.

Protected values (URLs, emails, placeholders and code) and dual-language link policies belong to a later parser/analysis/resolver increment. See [UnderstandMe translation architecture](../../../UnderstandMe/Docs/Markdown-Translation-and-Documentation-Updates/en.md).

## Safety and next increments

Keep the core UI-independent and side-effect free. Expand first with tests for inline structure, code and reference identity, then add a structured writer. The Spectre.Console renderer remains a separately packaged Blackboard component; it must depend on ExchangeHouse, never the reverse.
