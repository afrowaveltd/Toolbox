# Inline images and link identities

ExchangeHouse Markdown now recognizes simple inline images and links, including destinations with balanced parentheses, angle-delimited destinations containing spaces, and optional quoted or parenthesized titles. The parser does not resolve or fetch URLs.

Each image/link node exposes the visible label/alt text, unmodified destination, optional title, full raw token span, and exact destination source offsets. This is intended to protect source paths from accidental translation. An image may be nested as a label child of a link. Syntax inside code fences remains protected.

This is a conservative subset, not full CommonMark. Reference-style links/images, multiline titles, autolinks, complete nested syntax and editable writing remain unimplemented. Malformed references remain readable source text. The later managed-document resolver and DualLinks policy are separate concerns.

Parser tests cover spans, images, titles, escaped/angle destinations, nesting, malformed input and lossless document round trips. CI must be verified before claiming completion.
