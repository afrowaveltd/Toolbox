# Conservative Markdown inline scanning

**Implemented:** first inline-token milestone, 2026-10-29. This is not full CommonMark; the public surface may evolve before a stable NuGet release.

## Model and integration

`MarkdownInlineParser.Parse(text, sourceOffset)` returns a read-only list of `MarkdownInlineNode` records. Each node has `Kind`, absolute `Start`, `Length`, exact `RawText`, human-readable inner `Text`, optional `Destination` for links, and nested `Children` for links/formatting. The original Markdown is never rewritten by the parser.

`MarkdownParser.Parse` attaches these inline nodes to a heading, list item or plain paragraph line as `MarkdownBlock.Inlines`, using offsets into the full source. **Code fence lines and content have no inline nodes**, preventing accidental parsing of Markdown-like syntax inside fenced code.

Supported conservative forms:

- `**strong**`, `__strong__`, `*emphasis*`, `_emphasis_`.
- Simple inline links `[label](target)`, including balanced parentheses in destinations and escaped closing brackets in labels. Visible link labels may themselves contain emphasis or code spans. The destination is left unchanged and never fetched, translated or rewritten.
- Backtick code spans with matching delimiter run lengths, preserving the content exactly.
- Backslash escapes for recognition of special syntax. Invalid/unclosed or unsupported syntax is emitted literally as text.

## Known limits

This **does not** implement full CommonMark delimiter-run/flanking rules, emphasis marker runs of three, nested links, images, reference-style links, link titles, angle-bracket destinations, automatic URL/email linkification, multiline inline tokens, entity decoding, HTML or escaped-character normalization. Such content is not deliberately transformed. The parser is conservative; it may recognize only a subset of valid syntax. It has a nesting-depth limit.

No translation segmenter, link-localization resolver, full AST editor, editable Markdown writer or conversion to plain text/HTML exists yet. The initial source-preserving `MarkdownWriter` still reconstructs the unchanged text.

## Safety and future work

Treat `Destination` as untrusted data; consumers must check schemes and destinations before opening or rendering interactive links. For translation, labels may be translated while destinations/emails/code are protected. Language-aware dual links and original-versus-translated structural validation will be separate stages after the syntax model stabilizes.

Tests cover absolute offsets, headings/lists integration, escaped syntax, nested emphasis, code protection and nontrivial relative link destinations.
