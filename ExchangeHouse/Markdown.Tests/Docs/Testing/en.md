# Initial Markdown parser and writer test coverage

- Exact round trip for LF, CRLF, CR, Unicode and missing final newline.
- Original raw slices and source offsets stay contiguous.
- Heading recognition respects indentation and maximum heading level.
- Simple ordered/unordered list markers are recognized.
- Code fence parsing keeps inline Markdown-like characters as code until matching delimiter closes.
- Null input is explicitly rejected.

Further cases (nested blocks, reference-style links, escaped punctuation, inline tokens, safe conversion) will be added as capabilities are implemented. CI status is recorded separately.

## Added inline regression coverage (2026-10-29)

Inline recognition, nested label formatting, absolute source positions, raw link destinations, code spans, escaping, malformed syntax and absence of inline parsing inside fenced blocks. Link titles, reference-style links, multiline inline constructs and complex delimiter runs remain unimplemented.
