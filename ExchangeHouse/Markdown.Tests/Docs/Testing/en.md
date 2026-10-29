# Initial Markdown parser and writer test coverage

- Exact round trip for LF, CRLF, CR, Unicode and missing final newline.
- Original raw slices and source offsets stay contiguous.
- Heading recognition respects indentation and maximum heading level.
- Simple ordered/unordered list markers are recognized.
- Code fence parsing keeps inline Markdown-like characters as code until matching delimiter closes.
- Null input is explicitly rejected.

Further cases (nested blocks, reference-style links, escaped punctuation, inline tokens, safe conversion) will be added as capabilities are implemented. CI status is recorded separately.
